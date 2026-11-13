// Copyright ©2021-2026 Mike King.
// This file is licensed to you under the MIT license.
// See the License.txt file in the solution root for more information.

using DotDoc.EntityFrameworkCore.Extensions.DatabaseType;
using DotDoc.EntityFrameworkCore.Extensions.Tests.Data;
using DotDoc.EntityFrameworkCore.Extensions.UniqueConstraint;
using Microsoft.EntityFrameworkCore;

namespace DotDoc.EntityFrameworkCore.Extensions.Tests.UniqueConstraint;

/// <summary>
/// Tests for <see cref="UniqueConstraintInterceptor"/> and the <see cref="UniqueConstraintExtensions.UseUniqueConstraintInterceptor"/> extension.
/// </summary>
[TestClass]
public class UniqueConstraintInterceptorTests
{
    #region private fields

    /// <summary>
    /// The MSTest <see cref="TestContext"/> for the current test.
    /// </summary>
    private readonly TestContext _testContext;

    #endregion private fields

    #region public constructors

    /// <summary>
    /// Initializes a new instance of the <see cref="UniqueConstraintInterceptorTests"/> class.
    /// </summary>
    /// <param name="testContext">The MSTest test context.</param>
    public UniqueConstraintInterceptorTests(TestContext testContext)
    {
        this._testContext = testContext;
    }

    #endregion public constructors

    #region UseUniqueConstraintInterceptor tests

    /// <summary>
    /// Test UseUniqueConstraintInterceptor with a Null DbContextOptionsBuilder OptionsBuilder parameter.
    /// </summary>
    [TestMethod(DisplayName = "UseUniqueConstraintInterceptor with a Null DbContextOptionsBuilder OptionsBuilder parameter")]
    public void UniqueConstraintInterceptorTests_001()
    {
        // ARRANGE
        DbContextOptionsBuilder optionsBuilder = null!;

        // ACT / ASSERT
        Assert.ThrowsExactly<ArgumentNullException>(() => optionsBuilder.UseUniqueConstraintInterceptor(), "Unexpected exception");
    }

    #endregion UseUniqueConstraintInterceptor tests

    #region UniqueConstraintInterceptor tests

    /// <summary>
    /// Test the Synchronous UniqueConstraintInterceptor.
    /// </summary>
    /// <param name="databaseType">Database type.</param>
    /// <remarks>
    /// Test our interceptor captures Unique Constraint Exceptions inside EF Core.
    /// The Unique Constraint Exception Processor will see the table details are held in EF Core and
    /// convert the database table name and field names into the ones used by EF Core.
    /// Unique Constraint exceptions can be raised by synchronous or asynchronous operations so we
    /// need to be able to handle both.
    /// </remarks>
    [TestMethod(DisplayName = "Synchronous UniqueConstraintInterceptor")]
    [DataRow(DatabaseTypes.Sqlite, DisplayName = DatabaseTypes.Sqlite)]
    [DataRow(DatabaseTypes.SqlServer, DisplayName = DatabaseTypes.SqlServer)]
    public void UniqueConstraintInterceptorTests_002(string databaseType)
    {
        // ARRANGE
        using Context context = DatabaseUtils.OpenDatabaseAsync(
            databaseType,
            customConfigurationActions: optionsBuilder => optionsBuilder.UseUniqueConstraintInterceptor())
            .Result;

        string? schema = null;
        string value = "TestValue";

        TestTable2 testTable2 = new() { TestField = value };
        context.Add(testTable2);
        context.SaveChanges();

        testTable2 = new() { TestField = value };
        context.Add(testTable2);

        // ACT / ASSERT
        UniqueConstraintException e = Assert.ThrowsExactly<UniqueConstraintException>(() => context.SaveChanges(), "Unexpected exception");

        // Check the details contain the EF Core table name and field name.
        Assert.IsNotNull(e.Details, "Details are null");
        Assert.AreEqual(schema, e.Details.Schema, "Invalid schema name");
        Assert.AreEqual("TestTable2", e.Details.TableName, "Invalid table name");
        Assert.HasCount(1, e.Details.FieldNames, "Invalid field names count");
        Assert.AreEqual("TestField", e.Details.FieldNames[0], "Invalid field name");
    }

    /// <summary>
    /// Test the Asynchronous UniqueConstraintInterceptor.
    /// </summary>
    /// <param name="databaseType">Database type.</param>
    /// <returns>A <see cref="Task"/> that represents the asynchronous operation.</returns>
    /// <remarks>
    /// Test our interceptor captures Unique Constraint Exceptions inside EF Core.
    /// The Unique Constraint Exception Processor will see the table details are held in EF Core and
    /// convert the database table name and field names into the ones used by EF Core.
    /// Unique Constraint exceptions can be raised by synchronous or asynchronous operations so we
    /// need to be able to handle both.
    /// </remarks>
    [TestMethod(DisplayName = "Asynchronous UniqueConstraintInterceptor")]
    [DataRow(DatabaseTypes.Sqlite, DisplayName = DatabaseTypes.Sqlite)]
    [DataRow(DatabaseTypes.SqlServer, DisplayName = DatabaseTypes.SqlServer)]
    public async Task UniqueConstraintInterceptorTests_003_Async(string databaseType)
    {
        // ARRANGE
        using Context context = await DatabaseUtils.OpenDatabaseAsync(
            databaseType,
            customConfigurationActions: optionsBuilder => optionsBuilder.UseUniqueConstraintInterceptor())
            .ConfigureAwait(false);

        string? schema = null;
        string value = "TestValue";

        TestTable2 testTable2 = new() { TestField = value };
        await context.AddAsync(testTable2, this._testContext.CancellationToken).ConfigureAwait(false);
        await context.SaveChangesAsync(this._testContext.CancellationToken).ConfigureAwait(false);

        testTable2 = new() { TestField = value };
        await context.AddAsync(testTable2, this._testContext.CancellationToken).ConfigureAwait(false);

        // ACT / ASSERT
        UniqueConstraintException e = await Assert.ThrowsExactlyAsync<UniqueConstraintException>(async () => await context.SaveChangesAsync(this._testContext.CancellationToken).ConfigureAwait(false), "Unexpected exception").ConfigureAwait(false);

        // Check the details contain the EF Core table name and field name.
        Assert.IsNotNull(e.Details, "Details are null");
        Assert.AreEqual(schema, e.Details.Schema, "Invalid schema name");
        Assert.AreEqual("TestTable2", e.Details.TableName, "Invalid table name");
        Assert.HasCount(1, e.Details.FieldNames, "Invalid field names count");
        Assert.AreEqual("TestField", e.Details.FieldNames[0], "Invalid field name");
    }

    /// <summary>
    /// Test the Synchronous UniqueConstraintInterceptor with an error that is not a unique constraint violation.
    /// </summary>
    /// <param name="databaseType">Database type.</param>
    /// <remarks>Inserting a NULL into a NOT NULL field fails, and the original <see cref="DbUpdateException"/> must be passed on unchanged.</remarks>
    [TestMethod(DisplayName = "Synchronous UniqueConstraintInterceptor with an error that is not a unique constraint violation")]
    [DataRow(DatabaseTypes.Sqlite, DisplayName = DatabaseTypes.Sqlite)]
    [DataRow(DatabaseTypes.SqlServer, DisplayName = DatabaseTypes.SqlServer)]
    public void UniqueConstraintInterceptorTests_004(string databaseType)
    {
        // ARRANGE
        using Context context = DatabaseUtils.OpenDatabaseAsync(
            databaseType,
            customConfigurationActions: optionsBuilder => optionsBuilder.UseUniqueConstraintInterceptor())
            .Result;

        TestTable1 testTable1 = new() { TestField = null! };
        context.Add(testTable1);

        // ACT / ASSERT
        Assert.ThrowsExactly<DbUpdateException>(() => context.SaveChanges(), "Unexpected exception");
    }

    /// <summary>
    /// Test the Asynchronous UniqueConstraintInterceptor with an error that is not a unique constraint violation.
    /// </summary>
    /// <param name="databaseType">Database type.</param>
    /// <returns>A <see cref="Task"/> that represents the asynchronous operation.</returns>
    /// <remarks>Inserting a NULL into a NOT NULL field fails, and the original <see cref="DbUpdateException"/> must be passed on unchanged.</remarks>
    [TestMethod(DisplayName = "Asynchronous UniqueConstraintInterceptor with an error that is not a unique constraint violation")]
    [DataRow(DatabaseTypes.Sqlite, DisplayName = DatabaseTypes.Sqlite)]
    [DataRow(DatabaseTypes.SqlServer, DisplayName = DatabaseTypes.SqlServer)]
    public async Task UniqueConstraintInterceptorTests_005_Async(string databaseType)
    {
        // ARRANGE
        using Context context = await DatabaseUtils.OpenDatabaseAsync(
            databaseType,
            customConfigurationActions: optionsBuilder => optionsBuilder.UseUniqueConstraintInterceptor())
            .ConfigureAwait(false);

        TestTable1 testTable1 = new() { TestField = null! };
        await context.AddAsync(testTable1, this._testContext.CancellationToken).ConfigureAwait(false);

        // ACT / ASSERT
        await Assert.ThrowsExactlyAsync<DbUpdateException>(() => context.SaveChangesAsync(this._testContext.CancellationToken), "Unexpected exception").ConfigureAwait(false);
    }

    #endregion UniqueConstraintInterceptor tests
}
