// Copyright ©2021-2026 Mike King.
// This file is licensed to you under the MIT license.
// See the License.txt file in the solution root for more information.

using DotDoc.EntityFrameworkCore.Extensions.DatabaseType;
using DotDoc.EntityFrameworkCore.Extensions.Tests.Data;
using DotDoc.EntityFrameworkCore.Extensions.UniqueConstraint;
using Microsoft.EntityFrameworkCore;

namespace DotDoc.EntityFrameworkCore.Extensions.Tests.UniqueConstraint;

/// <summary>
/// Tests for UniqueConstraintInterceptor extensions.
/// </summary>
[TestClass]
public class UniqueConstraintInterceptorTests
{
    #region public methods

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

        string? schema = context.DefaultSchema;
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

        string? schema = context.DefaultSchema;
        string value = "TestValue";

        TestTable2 testTable2 = new() { TestField = value };
        await context.AddAsync(testTable2, CancellationToken.None).ConfigureAwait(false);
        await context.SaveChangesAsync(CancellationToken.None).ConfigureAwait(false);

        testTable2 = new() { TestField = value };
        await context.AddAsync(testTable2, CancellationToken.None).ConfigureAwait(false);

        // ACT / ASSERT
        UniqueConstraintException e = await Assert.ThrowsExactlyAsync<UniqueConstraintException>(async () => await context.SaveChangesAsync(CancellationToken.None).ConfigureAwait(false), "Unexpected exception").ConfigureAwait(false);

        // Check the details contain the EF Core table name and field name.
        Assert.IsNotNull(e.Details, "Details are null");
        Assert.AreEqual(schema, e.Details.Schema, "Invalid schema name");
        Assert.AreEqual("TestTable2", e.Details.TableName, "Invalid table name");
        Assert.HasCount(1, e.Details.FieldNames, "Invalid field names count");
        Assert.AreEqual("TestField", e.Details.FieldNames[0], "Invalid field name");
    }

    #endregion public methods
}
