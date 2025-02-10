// Copyright ©2021-2025 Mike King.
// This file is licensed to you under the MIT license.
// See the License.txt file in the solution root for more information.

using DotDoc.EntityFrameworkCore.Extensions.Constants;
using DotDoc.EntityFrameworkCore.Extensions.Extensions;
using DotDoc.EntityFrameworkCore.Extensions.Model;
using DotDoc.EntityFrameworkCore.Extensions.Tests.Data;
using DotDoc.EntityFrameworkCore.Extensions.Tests.Extensions;
using DotDoc.EntityFrameworkCore.Extensions.Tests.Utilities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DotDoc.EntityFrameworkCore.Extensions.Tests.UniqueConstraintExtensions;

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
    [TestMethod("UseUniqueConstraintInterceptor Guard Clause")]
    public void Test_UseUniqueConstraintInterceptor_GuardClause()
    {
        // ARRANGE
        DbContextOptionsBuilder? optionsBuilder = null;
        string paramName = "optionsBuilder";

        // ACT / ASSERT
        ArgumentNullException e = Assert.ThrowsException<ArgumentNullException>(() => optionsBuilder!.UseUniqueConstraintInterceptor(), "Missing exception");
        Assert.AreEqual(paramName, e.ParamName, "Invalid parameter name");
    }

    /// <summary>
    /// Test GetUniqueConstraintDetails Guard Clauses.
    /// </summary>
    /// <param name="databaseFacade">The <see cref="DatabaseFacade"/>.</param>
    /// <param name="e">The exception to extract the unique constraint details from.</param>
    /// <param name="exceptionType">The type of exception raised.</param>
    /// <param name="paramName">Name of parameter being checked.</param>
    [TestMethod("GetUniqueConstraintDetails Guard Clauses")]
    [DynamicData(nameof(Get_GetUniqueConstraintDetails_GuardClause_TestData), DynamicDataSourceType.Method, DynamicDataDisplayName = nameof(TestUtils.CreateDynamicDisplayName), DynamicDataDisplayNameDeclaringType = typeof(TestUtils))]
    public void Test_GetUniqueConstraintDetails_GuardClauses(DatabaseFacade? databaseFacade, Exception? e, Type exceptionType, string paramName)
    {
        // ARRANGE

        // ACT / ASSERT
        Exception e1 = Assert.That.ThrowsAnyException(() => databaseFacade!.GetUniqueConstraintDetails(e!), "Missing exception");
        Assert.AreEqual(exceptionType, e1.GetType(), "Invalid exception type");
        Assert.AreEqual(paramName, ((ArgumentException)e1).ParamName, "Invalid parameter name");
    }

    /// <summary>
    /// Test GetUniqueConstraintDetailsAsync Guard Clauses.
    /// </summary>
    /// <param name="databaseFacade">The <see cref="DatabaseFacade"/>.</param>
    /// <param name="e">The exception to extract the unique constraint details from.</param>
    /// <param name="exceptionType">The type of exception raised.</param>
    /// <param name="paramName">Name of parameter being checked.</param>
    /// <returns>A task that represents the asynchronous test operation.</returns>
    [TestMethod("GetUniqueConstraintDetailsAsync Guard Clauses")]
    [DynamicData(nameof(Get_GetUniqueConstraintDetails_GuardClause_TestData), DynamicDataSourceType.Method, DynamicDataDisplayName = nameof(TestUtils.CreateDynamicDisplayName), DynamicDataDisplayNameDeclaringType = typeof(TestUtils))]
    public async Task Test_GetUniqueConstraintDetails_GuardClausesAsync(DatabaseFacade? databaseFacade, Exception? e, Type exceptionType, string paramName)
    {
        // ARRANGE

        // ACT / ASSERT
        Exception e1 = await Assert.That.ThrowsAnyExceptionAsync(() => databaseFacade!.GetUniqueConstraintDetailsAsync(e!), "Missing exception").ConfigureAwait(false);
        Assert.AreEqual(exceptionType, e1.GetType(), "Invalid exception type");
        Assert.AreEqual(paramName, ((ArgumentException)e1).ParamName, "Invalid parameter name");
    }

    /// <summary>
    /// Test GetUniqueConstraintDetails with EF Core.
    /// </summary>
    /// <param name="databaseType">Database type.</param>
    /// <remarks>
    /// Use EF to perform the insert so the exception raised is wrapped inside a DbUpdateException.
    /// The Unique Constraint Exception Processor will see the table details are held in EF Core and
    /// convert the database table name and field names into the ones used by EF Core.
    /// </remarks>
    [TestMethod("GetUniqueConstraintDetails with EF Core")]
    [DataRow(DatabaseType.Sqlite, DisplayName = DatabaseType.Sqlite)]
    [DataRow(DatabaseType.SqlServer, DisplayName = DatabaseType.SqlServer)]
    public void Test_GetUniqueConstraintDetails_EfCore(string databaseType)
    {
        // ARRANGE
        using Context context = DatabaseUtils.CreateDatabase(databaseType);

        string? schema = DatabaseUtils.GetDefaultSchema(databaseType);
        string value = TestUtils.GetMethodName();

        TestTable2 testTable2 = new() { TestField = value };
        context.Add(testTable2);
        context.SaveChanges();

        testTable2 = new() { TestField = value };
        context.Add(testTable2);

        // ACT / ASSERT
        DbUpdateException e = Assert.ThrowsException<DbUpdateException>(() => context.SaveChanges(), "Unexpected exception");
        UniqueConstraintDetails? details = context.Database.GetUniqueConstraintDetails(e);

        // Check the details contain the EF Core table name and field name.
        Assert.IsNotNull(details, "Details are null");
        Assert.AreEqual(schema, details.Schema, "Invalid schema name");
        Assert.AreEqual("TestTable2", details.TableName, "Invalid table name");
        Assert.AreEqual(1, details.FieldNames.Count, "Invalid field names count");
        Assert.AreEqual("TestField", details.FieldNames[0], "Invalid field name");
    }

    /// <summary>
    /// Test GetUniqueConstraintDetailsAsync with EF Core.
    /// </summary>
    /// <param name="databaseType">Database type.</param>
    /// <returns>A task that represents the asynchronous test operation.</returns>
    /// <remarks>
    /// Use EF to perform the insert so the exception raised is wrapped inside a DbUpdateException.
    /// The Unique Constraint Exception Processor will see the table details are held in EF Core and
    /// convert the database table name and field names into the ones used by EF Core.
    /// </remarks>
    [TestMethod("GetUniqueConstraintDetailsAsync with EF Core")]
    [DataRow(DatabaseType.Sqlite, DisplayName = DatabaseType.Sqlite)]
    [DataRow(DatabaseType.SqlServer, DisplayName = DatabaseType.SqlServer)]
    public async Task Test_GetUniqueConstraintDetails_EfCoreAsync(string databaseType)
    {
        // ARRANGE
        using Context context = DatabaseUtils.CreateDatabase(databaseType);

        string? schema = DatabaseUtils.GetDefaultSchema(databaseType);
        string value = TestUtils.GetMethodName();

        TestTable2 testTable2 = new() { TestField = value };
        context.Add(testTable2);
        await context.SaveChangesAsync().ConfigureAwait(false);

        testTable2 = new() { TestField = value };
        context.Add(testTable2);

        // ACT / ASSERT
        DbUpdateException e = await Assert.ThrowsExceptionAsync<DbUpdateException>(() => context.SaveChangesAsync(), "Unexpected exception").ConfigureAwait(false);
        UniqueConstraintDetails? details = await context.Database.GetUniqueConstraintDetailsAsync(e).ConfigureAwait(false);

        // Check the details contain the EF Core table name and field name.
        Assert.IsNotNull(details, "Details are null");
        Assert.AreEqual(schema, details.Schema, "Invalid schema name");
        Assert.AreEqual("TestTable2", details.TableName, "Invalid table name");
        Assert.AreEqual(1, details.FieldNames.Count, "Invalid field names count");
        Assert.AreEqual("TestField", details.FieldNames[0], "Invalid field name");
    }

    /// <summary>
    /// Test GetUniqueConstraintDetails with EF Core and SQL.
    /// </summary>
    /// <param name="databaseType">Database type.</param>
    /// <remarks>
    /// Use SQL rather than EF to perform the insert so the exception raised comes from the database,
    /// not wrapped in a DbUpdateException. The Unique Constraint Exception Processor will see the table details
    /// are held in EF Core and convert the database table name and field names into the ones used by EF Core.
    /// </remarks>
    [TestMethod("GetUniqueConstraintDetails with EF Core and SQL")]
    [DataRow(DatabaseType.Sqlite, DisplayName = DatabaseType.Sqlite)]
    [DataRow(DatabaseType.SqlServer, DisplayName = DatabaseType.SqlServer)]
    public void Test_GetUniqueConstraintDetails_EfCoreAndSql(string databaseType)
    {
        // ARRANGE
        using Context context = DatabaseUtils.CreateDatabase(databaseType);

        string? schema = DatabaseUtils.GetDefaultSchema(databaseType);
        string value = TestUtils.GetMethodName();

        FormattableString sql = $"INSERT INTO TestTable2RealName (TestFieldRealName) VALUES ({value})";
        context.Database.ExecuteInsert(sql);

        // ACT / ASSERT
        Exception e = Assert.That.ThrowsAnyException(() => context.Database.ExecuteInsert(sql), "Missing exception");
        UniqueConstraintDetails? details = context.Database.GetUniqueConstraintDetails(e);

        // Check the details contain the EF Core table name and field name.
        Assert.IsNotNull(details, "Details are null");
        Assert.AreEqual(schema, details.Schema, "Invalid schema name");
        Assert.AreEqual("TestTable2", details.TableName, "Invalid table name");
        Assert.AreEqual(1, details.FieldNames.Count, "Invalid field names count");
        Assert.AreEqual("TestField", details.FieldNames[0], "Invalid field name");
    }

    /// <summary>
    /// Test GetUniqueConstraintDetailsAsync with EF Core and SQL.
    /// </summary>
    /// <param name="databaseType">Database type.</param>
    /// <returns>A task that represents the asynchronous test operation.</returns>
    /// <remarks>
    /// Use SQL rather than EF to perform the insert so the exception raised comes from the database,
    /// not wrapped in a DbUpdateException. The Unique Constraint Exception Processor will see the table details
    /// are held in EF Core and convert the database table name and field names into the ones used by EF Core.
    /// </remarks>
    [TestMethod("GetUniqueConstraintDetailsAsync with EF Core and SQL")]
    [DataRow(DatabaseType.Sqlite, DisplayName = DatabaseType.Sqlite)]
    [DataRow(DatabaseType.SqlServer, DisplayName = DatabaseType.SqlServer)]
    public async Task Test_GetUniqueConstraintDetails_EfCoreAndSqlAsync(string databaseType)
    {
        // ARRANGE
        using Context context = DatabaseUtils.CreateDatabase(databaseType);

        string? schema = DatabaseUtils.GetDefaultSchema(databaseType);
        string value = TestUtils.GetMethodName();

        FormattableString sql = $"INSERT INTO TestTable2RealName (TestFieldRealName) VALUES ({value})";
        await context.Database.ExecuteInsertAsync(sql).ConfigureAwait(false);

        // ACT / ASSERT
        Exception e = await Assert.That.ThrowsAnyExceptionAsync(() => context.Database.ExecuteInsertAsync(sql), "Missing exception").ConfigureAwait(false);
        UniqueConstraintDetails? details = await context.Database.GetUniqueConstraintDetailsAsync(e).ConfigureAwait(false);

        // Check the details contain the EF Core table name and field name.
        Assert.IsNotNull(details, "Details are null");
        Assert.AreEqual(schema, details.Schema, "Invalid schema name");
        Assert.AreEqual("TestTable2", details.TableName, "Invalid table name");
        Assert.AreEqual(1, details.FieldNames.Count, "Invalid field names count");
        Assert.AreEqual("TestField", details.FieldNames[0], "Invalid field name");
    }

    /// <summary>
    /// Test GetUniqueConstraintDetails with SQL.
    /// </summary>
    /// <param name="databaseType">Database type.</param>
    /// <remarks>
    /// Use SQL to create the test table and perform the insert so the exception raised comes from the
    /// database, not wrapped in a DbUpdateException. The Unique Constraint Exception Processor will see
    /// the table details are not in EF Core and should return the database table and field names.
    /// </remarks>
    [TestMethod("GetUniqueConstraintDetails with SQL")]
    [DataRow(DatabaseType.Sqlite, DisplayName = DatabaseType.Sqlite)]
    [DataRow(DatabaseType.SqlServer, DisplayName = DatabaseType.SqlServer)]
    public void Test_GetUniqueConstraintDetails_SqlTable(string databaseType)
    {
        // ARRANGE
        using Context context = DatabaseUtils.CreateDatabase(databaseType);

        string? schema = DatabaseUtils.GetDefaultSchema(databaseType);
        string value = TestUtils.GetMethodName();

        switch (databaseType)
        {
            case DatabaseType.Sqlite:
                context.Database.ExecuteNonQuery(
@"CREATE TABLE [TestTable3] (
    [Id] INTEGER NOT NULL CONSTRAINT [PK_TestTable3] PRIMARY KEY AUTOINCREMENT,
    [TestField] TEXT NOT NULL
);");

                context.Database.ExecuteNonQuery(
@"CREATE UNIQUE INDEX [IX_TestTable3_TestField] ON [TestTable3] ([TestField]);");
                break;

            case DatabaseType.SqlServer:
                context.Database.ExecuteNonQuery(
@"CREATE TABLE [TestTable3] (
    [Id] bigint NOT NULL IDENTITY,
    [TestField] nvarchar(256) NOT NULL,
    CONSTRAINT [PK_TestTable3] PRIMARY KEY ([Id])
);");

                context.Database.ExecuteNonQuery(
@"CREATE UNIQUE INDEX [IX_TestTable3_TestField] ON [TestTable3] ([TestField]);");
                break;

            default:
                throw new ArgumentException("Unsupported database type", nameof(databaseType));
        }

        FormattableString sql = $"INSERT INTO TestTable3 (TestField) VALUES ({value})";
        context.Database.ExecuteInsert(sql);

        // ACT / ASSERT
        Exception e = Assert.That.ThrowsAnyException(() => context.Database.ExecuteInsert(sql), "Missing exception");
        UniqueConstraintDetails? details = context.Database.GetUniqueConstraintDetails(e);

        // Check the details contain the database table name and field name.
        Assert.IsNotNull(details, "Details are null");
        Assert.AreEqual(schema, details.Schema, "Invalid schema name");
        Assert.AreEqual("TestTable3", details.TableName, "Invalid EF table name");
        Assert.AreEqual(1, details.FieldNames.Count, "Invalid field names count");
        Assert.AreEqual("TestField", details.FieldNames[0], "Invalid EF field name");
    }

    /// <summary>
    /// Test GetUniqueConstraintDetailsAsync with SQL.
    /// </summary>
    /// <param name="databaseType">Database type.</param>
    /// <returns>A task that represents the asynchronous test operation.</returns>
    /// <remarks>
    /// Use SQL to create the test table and perform the insert so the exception raised comes from the
    /// database, not wrapped in a DbUpdateException. The Unique Constraint Exception Processor will see
    /// the table details are not in EF Core and should return the database table and field names.
    /// </remarks>
    [TestMethod("GetUniqueConstraintDetailsAsync with SQL")]
    [DataRow(DatabaseType.Sqlite, DisplayName = DatabaseType.Sqlite)]
    [DataRow(DatabaseType.SqlServer, DisplayName = DatabaseType.SqlServer)]
    public async Task Test_GetUniqueConstraintDetails_SqlTableAsync(string databaseType)
    {
        // ARRANGE
        using Context context = DatabaseUtils.CreateDatabase(databaseType);

        string? schema = DatabaseUtils.GetDefaultSchema(databaseType);
        string value = TestUtils.GetMethodName();

        switch (databaseType)
        {
            case DatabaseType.Sqlite:
                await context.Database.ExecuteNonQueryAsync(
@"CREATE TABLE [TestTable3] (
    [Id] INTEGER NOT NULL CONSTRAINT [PK_TestTable3] PRIMARY KEY AUTOINCREMENT,
    [TestField] TEXT NOT NULL
);")
                    .ConfigureAwait(false);

                await context.Database.ExecuteNonQueryAsync(
@"CREATE UNIQUE INDEX [IX_TestTable3_TestField] ON [TestTable3] ([TestField]);")
                    .ConfigureAwait(false);
                break;

            case DatabaseType.SqlServer:
                await context.Database.ExecuteNonQueryAsync(
@"CREATE TABLE [TestTable3] (
    [Id] bigint NOT NULL IDENTITY,
    [TestField] nvarchar(256) NOT NULL,
    CONSTRAINT [PK_TestTable3] PRIMARY KEY ([Id])
);")
                    .ConfigureAwait(false);

                await context.Database.ExecuteNonQueryAsync(
@"CREATE UNIQUE INDEX [IX_TestTable3_TestField] ON [TestTable3] ([TestField]);")
                    .ConfigureAwait(false);
                break;

            default:
                throw new ArgumentException("Unsupported database type", nameof(databaseType));
        }

        FormattableString sql = $"INSERT INTO TestTable3 (TestField) VALUES ({value})";
        await context.Database.ExecuteInsertAsync(sql).ConfigureAwait(false);

        // ACT / ASSERT
        Exception e = await Assert.That.ThrowsAnyExceptionAsync(() => context.Database.ExecuteInsertAsync(sql), "Missing exception").ConfigureAwait(false);
        UniqueConstraintDetails? details = await context.Database.GetUniqueConstraintDetailsAsync(e).ConfigureAwait(false);

        // Check the details contain the database table name and field name.
        Assert.IsNotNull(details, "Details are null");
        Assert.AreEqual(schema, details.Schema, "Invalid schema name");
        Assert.AreEqual("TestTable3", details.TableName, "Invalid EF table name");
        Assert.AreEqual(1, details.FieldNames.Count, "Invalid field names count");
        Assert.AreEqual("TestField", details.FieldNames[0], "Invalid EF field name");
    }

    #endregion public methods

    #region private methods

    /// <summary>
    /// Get test data for GetUniqueConstraintDetails methods.
    /// </summary>
    /// <returns><see cref="IEnumerable{T}"/>.</returns>
    private static IEnumerable<object?[]> Get_GetUniqueConstraintDetails_GuardClause_TestData()
    {
        using Context context = DatabaseUtils.CreateDatabase(DatabaseType.Sqlite);

        // DatabaseFacade databaseFacade
        // Exception e
        // Type exceptionType
        // string paramName
        yield return [
            null,
            new InvalidOperationException("Test Exception"),
            typeof(ArgumentNullException),
            "databaseFacade"];

        yield return [
            context.Database,
            null,
            typeof(ArgumentNullException),
            "e"];
    }

    #endregion private methods
}