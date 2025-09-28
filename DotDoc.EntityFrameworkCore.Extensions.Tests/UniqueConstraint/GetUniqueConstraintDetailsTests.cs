// Copyright ©2021-2025 Mike King.
// This file is licensed to you under the MIT license.
// See the License.txt file in the solution root for more information.

using DotDoc.EntityFrameworkCore.Extensions.DatabaseType;
using DotDoc.EntityFrameworkCore.Extensions.Execute;
using DotDoc.EntityFrameworkCore.Extensions.Tests.Data;
using DotDoc.EntityFrameworkCore.Extensions.Tests.TestUtilities;
using DotDoc.EntityFrameworkCore.Extensions.UniqueConstraint;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace DotDoc.EntityFrameworkCore.Extensions.Tests.UniqueConstraint;

/// <summary>
/// Tests for GetUniqueConstraintDetails extensions.
/// </summary>
[TestClass]
public class GetUniqueConstraintDetailsTests
{
    #region public methods

    /// <summary>
    /// Test UseUniqueConstraintInterceptor Guard Clause.
    /// </summary>
    [TestMethod(DisplayName = "UseUniqueConstraintInterceptor Guard Clause")]
    public void Test_UseUniqueConstraintInterceptor_GuardClause()
    {
        // ARRANGE
        DbContextOptionsBuilder? optionsBuilder = null;

        // ACT / ASSERT
        ArgumentNullException e = Assert.ThrowsExactly<ArgumentNullException>(() => _ = optionsBuilder!.UseUniqueConstraintInterceptor(), "Unexpected exception");
        Assert.AreEqual(nameof(optionsBuilder), e.ParamName, "Invalid parameter name");
    }

    /// <summary>
    /// Test GetUniqueConstraintDetailsAsync Guard Clauses.
    /// </summary>
    /// <param name="databaseFacade">The <see cref="DatabaseFacade"/>.</param>
    /// <param name="e">The exception to extract the unique constraint details from.</param>
    /// <param name="exceptionType">The type of exception raised.</param>
    /// <param name="paramName">Name of parameter being checked.</param>
    /// <returns>A <see cref="Task"/> that represents the asynchronous operation.</returns>
    [TestMethod(DisplayName = "GetUniqueConstraintDetailsAsync Guard Clauses")]
    [DynamicData(nameof(Get_GetUniqueConstraintDetailsAsync_GuardClause_TestData), DynamicDataDisplayName = nameof(TestUtils.CreateDynamicDisplayName), DynamicDataDisplayNameDeclaringType = typeof(TestUtils))]
    public async Task Test_GetUniqueConstraintDetailsAsync_GuardClauses_Async(DatabaseFacade? databaseFacade, Exception? e, Type exceptionType, string paramName)
    {
        // ARRANGE

        // ACT / ASSERT
        Exception e1 = await Assert.ThrowsAsync<Exception>(() => databaseFacade!.GetUniqueConstraintDetailsAsync(e!, CancellationToken.None), "Unexpected exception").ConfigureAwait(false);
        Assert.AreEqual(exceptionType, e1.GetType(), "Invalid exception type");
        Assert.AreEqual(paramName, ((ArgumentException)e1).ParamName, "Invalid parameter name");
    }

    /// <summary>
    /// Test GetUniqueConstraintDetailsAsync with EF Core.
    /// </summary>
    /// <param name="databaseType">Database type.</param>
    /// <returns>A <see cref="Task"/> that represents the asynchronous operation.</returns>
    /// <remarks>
    /// Use EF to perform the insert so the exception raised is wrapped inside a DbUpdateException.
    /// The Unique Constraint Exception Processor will see the table details are held in EF Core and
    /// convert the database table name and field names into the ones used by EF Core.
    /// </remarks>
    [TestMethod(DisplayName = "GetUniqueConstraintDetailsAsync with EF Core")]
    [DataRow(DatabaseTypes.Sqlite, DisplayName = DatabaseTypes.Sqlite)]
    [DataRow(DatabaseTypes.SqlServer, DisplayName = DatabaseTypes.SqlServer)]
    public async Task Test_GetUniqueConstraintDetailsAsync_EfCore_Async(string databaseType)
    {
        // ARRANGE
        using Context context = await DatabaseUtils.CreateDatabaseAsync(databaseType).ConfigureAwait(false);

        string? schema = context.DefaultSchema;
        string value = "TestValue";

        TestTable2 testTable2 = new() { TestField = value };
        context.Add(testTable2);
        await context.SaveChangesAsync(CancellationToken.None).ConfigureAwait(false);

        testTable2 = new() { TestField = value };
        context.Add(testTable2);

        // ACT / ASSERT
        DbUpdateException e = await Assert.ThrowsExactlyAsync<DbUpdateException>(() => context.SaveChangesAsync(CancellationToken.None), "Unexpected exception").ConfigureAwait(false);
        UniqueConstraintDetails? details = await context.Database.GetUniqueConstraintDetailsAsync(e, CancellationToken.None).ConfigureAwait(false);

        // Check the details contain the EF Core table name and field name.
        Assert.IsNotNull(details, "Details are null");
        Assert.AreEqual(schema, details.Schema, "Invalid schema name");
        Assert.AreEqual("TestTable2", details.TableName, "Invalid table name");
        Assert.HasCount(1, details.FieldNames, "Invalid field names count");
        Assert.AreEqual("TestField", details.FieldNames[0], "Invalid field name");
    }

    /// <summary>
    /// Test GetUniqueConstraintDetailsAsync with EF Core and SQL.
    /// </summary>
    /// <param name="databaseType">Database type.</param>
    /// <returns>A <see cref="Task"/> that represents the asynchronous operation.</returns>
    /// <remarks>
    /// Use SQL rather than EF to perform the insert so the exception raised comes from the database,
    /// not wrapped in a DbUpdateException. The Unique Constraint Exception Processor will see the table details
    /// are held in EF Core and convert the database table name and field names into the ones used by EF Core.
    /// </remarks>
    [TestMethod(DisplayName = "GetUniqueConstraintDetailsAsync with EF Core and SQL")]
    [DataRow(DatabaseTypes.Sqlite, DisplayName = DatabaseTypes.Sqlite)]
    [DataRow(DatabaseTypes.SqlServer, DisplayName = DatabaseTypes.SqlServer)]
    public async Task Test_GetUniqueConstraintDetailsAsync_EfCoreAndSql_Async(string databaseType)
    {
        // ARRANGE
        using Context context = await DatabaseUtils.CreateDatabaseAsync(databaseType).ConfigureAwait(false);

        string? schema = context.DefaultSchema;
        string value = "TestValue";

        FormattableString sql = $"INSERT INTO TestTable2RealName (TestFieldRealName) VALUES ({value})";
        await context.Database.ExecuteInsertAsync<long>(sql, CancellationToken.None).ConfigureAwait(false);

        // ACT / ASSERT
        Exception e = await Assert.ThrowsAsync<Exception>(() => context.Database.ExecuteInsertAsync<long>(sql, CancellationToken.None), "Unexpected exception").ConfigureAwait(false);
        UniqueConstraintDetails? details = await context.Database.GetUniqueConstraintDetailsAsync(e, CancellationToken.None).ConfigureAwait(false);

        // Check the details contain the EF Core table name and field name.
        Assert.IsNotNull(details, "Details are null");
        Assert.AreEqual(schema, details.Schema, "Invalid schema name");
        Assert.AreEqual("TestTable2", details.TableName, "Invalid table name");
        Assert.HasCount(1, details.FieldNames, "Invalid field names count");
        Assert.AreEqual("TestField", details.FieldNames[0], "Invalid field name");
    }

    /// <summary>
    /// Test GetUniqueConstraintDetailsAsync with SQL.
    /// </summary>
    /// <param name="databaseType">Database type.</param>
    /// <returns>A <see cref="Task"/> that represents the asynchronous operation.</returns>
    /// <remarks>
    /// Use SQL to create the test table and perform the insert so the exception raised comes from the
    /// database, not wrapped in a DbUpdateException. The Unique Constraint Exception Processor will see
    /// the table details are not in EF Core and should return the database table and field names.
    /// </remarks>
    [TestMethod(DisplayName = "GetUniqueConstraintDetailsAsync with SQL")]
    [DataRow(DatabaseTypes.Sqlite, DisplayName = DatabaseTypes.Sqlite)]
    [DataRow(DatabaseTypes.SqlServer, DisplayName = DatabaseTypes.SqlServer)]
    public async Task Test_GetUniqueConstraintDetailsAsync_SqlTable_Async(string databaseType)
    {
        // ARRANGE
        using Context context = await DatabaseUtils.CreateDatabaseAsync(databaseType).ConfigureAwait(false);

        string? schema = context.DefaultSchema;
        string value = "TestValue";

        switch (databaseType)
        {
            case DatabaseTypes.Sqlite:
                await context.Database.ExecuteNonQueryAsync(
@"CREATE TABLE [TestTable3] (
    [Id] INTEGER NOT NULL CONSTRAINT [PK_TestTable3] PRIMARY KEY AUTOINCREMENT,
    [TestField] TEXT NOT NULL
);", CancellationToken.None)
                    .ConfigureAwait(false);

                await context.Database.ExecuteNonQueryAsync(
@"CREATE UNIQUE INDEX [IX_TestTable3_TestField] ON [TestTable3] ([TestField]);", CancellationToken.None)
                    .ConfigureAwait(false);
                break;

            case DatabaseTypes.SqlServer:
                await context.Database.ExecuteNonQueryAsync(
@"CREATE TABLE [TestTable3] (
    [Id] bigint NOT NULL IDENTITY,
    [TestField] nvarchar(256) NOT NULL,
    CONSTRAINT [PK_TestTable3] PRIMARY KEY ([Id])
);", CancellationToken.None)
                    .ConfigureAwait(false);

                await context.Database.ExecuteNonQueryAsync(
@"CREATE UNIQUE INDEX [IX_TestTable3_TestField] ON [TestTable3] ([TestField]);", CancellationToken.None)
                    .ConfigureAwait(false);
                break;

            default:
                throw new ArgumentException("Unsupported database type", nameof(databaseType));
        }

        FormattableString sql = $"INSERT INTO TestTable3 (TestField) VALUES ({value})";
        await context.Database.ExecuteInsertAsync<long>(sql, CancellationToken.None).ConfigureAwait(false);

        // ACT / ASSERT
        Exception e = await Assert.ThrowsAsync<Exception>(() => context.Database.ExecuteInsertAsync<long>(sql, CancellationToken.None), "Unexpected exception").ConfigureAwait(false);
        UniqueConstraintDetails? details = await context.Database.GetUniqueConstraintDetailsAsync(e, CancellationToken.None).ConfigureAwait(false);

        // Check the details contain the database table name and field name.
        Assert.IsNotNull(details, "Details are null");
        Assert.AreEqual(schema, details.Schema, "Invalid schema name");
        Assert.AreEqual("TestTable3", details.TableName, "Invalid EF table name");
        Assert.HasCount(1, details.FieldNames, "Invalid field names count");
        Assert.AreEqual("TestField", details.FieldNames[0], "Invalid EF field name");
    }

    #endregion public methods

    #region private methods

    /// <summary>
    /// Get test data for GetUniqueConstraintDetails methods.
    /// </summary>
    /// <returns><see cref="IEnumerable{T}"/>.</returns>
    private static IEnumerable<object?[]> Get_GetUniqueConstraintDetailsAsync_GuardClause_TestData()
    {
        using Context context = DatabaseUtils.CreateDatabaseAsync(DatabaseTypes.Sqlite).Result;

        // DatabaseFacade databaseFacade
        // Exception e
        // Type exceptionType
        // string paramName
        yield return [
            null,
            new InvalidOperationException("Test Exception"),
            typeof(ArgumentNullException),
            "database"];

        yield return [
            context.Database,
            null,
            typeof(ArgumentNullException),
            "e"];
    }

    #endregion private methods
}