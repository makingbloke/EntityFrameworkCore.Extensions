// Copyright ©2021-2025 Mike King.
// This file is licensed to you under the MIT license.
// See the License.txt file in the solution root for more information.

using DotDoc.EntityFrameworkCore.Extensions.DatabaseType;
using DotDoc.EntityFrameworkCore.Extensions.Tests.Data;
using DotDoc.EntityFrameworkCore.Extensions.Tests.TestUtilities;
using DotDoc.EntityFrameworkCore.Extensions.UniqueConstraint;

namespace DotDoc.EntityFrameworkCore.Extensions.Tests.UniqueConstraint;

/// <summary>
/// Tests for UniqueConstraintInterceptor extensions.
/// </summary>
[TestClass]
public class UniqueConstraintInterceptorTests
{
    #region public methods

    /// <summary>
    /// Test UniqueConstraintInterceptor.
    /// </summary>
    /// <param name="databaseType">Database type.</param>
    /// <remarks>
    /// Test our interceptor captures Unique Constraint Exceptions inside EF Core.
    /// The Unique Constraint Exception Processor will see the table details are held in EF Core and
    /// convert the database table name and field names into the ones used by EF Core.
    /// </remarks>
    [TestMethod(DisplayName = "UniqueConstraintInterceptor sync")]
    [DataRow(DatabaseTypes.Sqlite, DisplayName = DatabaseTypes.Sqlite)]
    [DataRow(DatabaseTypes.SqlServer, DisplayName = DatabaseTypes.SqlServer)]
    public void Test_UniqueConstraintInterceptor(string databaseType)
    {
        // ARRANGE
        using Context context = DatabaseUtils.CreateDatabaseAsync(
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
        UniqueConstraintException e = Assert.ThrowsExactly<UniqueConstraintException>(() => _ = context.SaveChanges(), "Unexpected exception");

        // Check the details contain the EF Core table name and field name.
        Assert.IsNotNull(e.Details, "Details are null");
        Assert.AreEqual(schema, e.Details.Schema, "Invalid schema name");
        Assert.AreEqual("TestTable2", e.Details.TableName, "Invalid table name");
        Assert.HasCount(1, e.Details.FieldNames, "Invalid field names count");
        Assert.AreEqual("TestField", e.Details.FieldNames[0], "Invalid field name");
    }

    /// <summary>
    /// Test UniqueConstraintInterceptor.
    /// </summary>
    /// <param name="databaseType">Database type.</param>
    /// <returns>A <see cref="Task"/> that represents the asynchronous operation.</returns>
    /// <remarks>
    /// Test our interceptor captures Unique Constraint Exceptions inside EF Core.
    /// The Unique Constraint Exception Processor will see the table details are held in EF Core and
    /// convert the database table name and field names into the ones used by EF Core.
    /// </remarks>
    [TestMethod(DisplayName = "UniqueConstraintInterceptorAsync async")]
    [DataRow(DatabaseTypes.Sqlite, DisplayName = DatabaseTypes.Sqlite)]
    [DataRow(DatabaseTypes.SqlServer, DisplayName = DatabaseTypes.SqlServer)]
    public async Task Test_UniqueConstraintInterceptor_Async(string databaseType)
    {
        // ARRANGE
        using Context context = await DatabaseUtils.CreateDatabaseAsync(
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
