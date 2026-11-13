// Copyright ©2021-2026 Mike King.
// This file is licensed to you under the MIT license.
// See the License.txt file in the solution root for more information.

using DotDoc.EntityFrameworkCore.Extensions.DatabaseType;
using DotDoc.EntityFrameworkCore.Extensions.Execute;
using DotDoc.EntityFrameworkCore.Extensions.Tests.Data;
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
    #region private fields

    /// <summary>
    /// The MSTest <see cref="TestContext"/> for the current test.
    /// </summary>
    private readonly TestContext _testContext;

    #endregion private fields

    #region public constructors

    /// <summary>
    /// Initializes a new instance of the <see cref="GetUniqueConstraintDetailsTests"/> class.
    /// </summary>
    /// <param name="testContext">The MSTest test context.</param>
    public GetUniqueConstraintDetailsTests(TestContext testContext)
    {
        this._testContext = testContext;
    }

    #endregion public constructors

    #region GetUniqueConstraintDetailsAsync tests

    /// <summary>
    /// Test GetUniqueConstraintDetailsAsync with Null DatabaseFacade Database parameter.
    /// </summary>
    /// <returns>A <see cref="Task"/> that represents the asynchronous operation.</returns>
    [TestMethod(DisplayName = "GetUniqueConstraintDetailsAsync with Null DatabaseFacade Database parameter")]
    public async Task GetUniqueConstraintDetailsAsyncTests_001_Async()
    {
        // ARRANGE
        DatabaseFacade database = null!;
        Exception e = new InvalidOperationException("Test Exception");

        // ACT / ASSERT
        await Assert.ThrowsExactlyAsync<ArgumentNullException>(() => database.GetUniqueConstraintDetailsAsync(e, this._testContext.CancellationToken), "Unexpected exception").ConfigureAwait(false);
    }

    /// <summary>
    /// Test GetUniqueConstraintDetailsAsync with Null Exception E parameter.
    /// </summary>
    /// <returns>A <see cref="Task"/> that represents the asynchronous operation.</returns>
    [TestMethod(DisplayName = "GetUniqueConstraintDetailsAsync with Null Exception E parameter")]
    public async Task GetUniqueConstraintDetailsAsyncTests_002_Async()
    {
        // ARRANGE
        using Context context = await DatabaseUtils.OpenDatabaseAsync(DatabaseTypes.Sqlite).ConfigureAwait(false);
        Exception e = null!;

        // ACT / ASSERT
        await Assert.ThrowsExactlyAsync<ArgumentNullException>(() => context.Database.GetUniqueConstraintDetailsAsync(e, this._testContext.CancellationToken), "Unexpected exception").ConfigureAwait(false);
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
    public async Task GetUniqueConstraintDetailsAsyncTests_003_Async(string databaseType)
    {
        // ARRANGE
        using Context context = await DatabaseUtils.OpenDatabaseAsync(databaseType).ConfigureAwait(false);

        string? schema = null;
        string tableName = nameof(TestTable2);
        string fieldName = nameof(TestTable2.TestField);

        string value = "TestValue";

        TestTable2 testTable2 = new() { TestField = value };
        context.Add(testTable2);
        await context.SaveChangesAsync(this._testContext.CancellationToken).ConfigureAwait(false);

        testTable2 = new() { TestField = value };
        context.Add(testTable2);

        // ACT / ASSERT
        DbUpdateException e = await Assert.ThrowsExactlyAsync<DbUpdateException>(() => context.SaveChangesAsync(this._testContext.CancellationToken), "Unexpected exception").ConfigureAwait(false);
        UniqueConstraintDetails? details = await context.Database.GetUniqueConstraintDetailsAsync(e, this._testContext.CancellationToken).ConfigureAwait(false);

        // Check the details contain the EF Core table name and field name.
        Assert.IsNotNull(details, "Details are null");
        Assert.AreEqual(schema, details.Schema, "Invalid schema name");
        Assert.AreEqual(tableName, details.TableName, "Invalid table name");
        Assert.HasCount(1, details.FieldNames, "Invalid field names count");
        Assert.AreEqual(fieldName, details.FieldNames[0], "Invalid field name");
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
    public async Task GetUniqueConstraintDetailsAsyncTests_004_Async(string databaseType)
    {
        // ARRANGE
        using Context context = await DatabaseUtils.OpenDatabaseAsync(databaseType).ConfigureAwait(false);

        string? schema = null;
        string tableName = nameof(TestTable2);
        string fieldName = nameof(TestTable2.TestField);

        string value = "TestValue";

        FormattableString sql = $"INSERT INTO TestTable2RealName (TestFieldRealName) VALUES ({value})";
        await context.Database.ExecuteInsertAsync<long>(sql, this._testContext.CancellationToken).ConfigureAwait(false);

        // ACT / ASSERT
        Exception e = await Assert.ThrowsAsync<Exception>(() => context.Database.ExecuteInsertAsync<long>(sql, this._testContext.CancellationToken), "Unexpected exception").ConfigureAwait(false);
        UniqueConstraintDetails? details = await context.Database.GetUniqueConstraintDetailsAsync(e, this._testContext.CancellationToken).ConfigureAwait(false);

        // Check the details contain the EF Core table name and field name.
        Assert.IsNotNull(details, "Details are null");
        Assert.AreEqual(schema, details.Schema, "Invalid schema name");
        Assert.AreEqual(tableName, details.TableName, "Invalid table name");
        Assert.HasCount(1, details.FieldNames, "Invalid field names count");
        Assert.AreEqual(fieldName, details.FieldNames[0], "Invalid field name");
    }

    /// <summary>
    /// Test GetUniqueConstraintDetailsAsync with SQL only.
    /// </summary>
    /// <param name="databaseType">Database type.</param>
    /// <returns>A <see cref="Task"/> that represents the asynchronous operation.</returns>
    /// <remarks>
    /// Use SQL to create the test table and perform the insert so the exception raised comes from the
    /// database, not wrapped in a DbUpdateException. The Unique Constraint Exception Processor will see
    /// the table details are not in EF Core and should return the database table and field names.
    /// </remarks>
    [TestMethod(DisplayName = "GetUniqueConstraintDetailsAsync with SQL only")]
    [DataRow(DatabaseTypes.Sqlite, DisplayName = DatabaseTypes.Sqlite)]
    [DataRow(DatabaseTypes.SqlServer, DisplayName = DatabaseTypes.SqlServer)]
    public async Task GetUniqueConstraintDetailsAsyncTests_005_Async(string databaseType)
    {
        // ARRANGE
        using Context context = await DatabaseUtils.OpenDatabaseAsync(databaseType).ConfigureAwait(false);

        string? schema = null;
        string tableName = "TestTable3";
        string fieldName = "TestField";

        string value = "TestValue";

        switch (databaseType)
        {
            case DatabaseTypes.Sqlite:
                await context.Database.ExecuteNonQueryAsync(
@"CREATE TABLE [TestTable3] (
    [Id] INTEGER NOT NULL CONSTRAINT [PK_TestTable3] PRIMARY KEY AUTOINCREMENT,
    [TestField] TEXT NOT NULL
);", this._testContext.CancellationToken)
                    .ConfigureAwait(false);

                await context.Database.ExecuteNonQueryAsync(
@"CREATE UNIQUE INDEX [IX_TestTable3_TestField] ON [TestTable3] ([TestField]);", this._testContext.CancellationToken)
                    .ConfigureAwait(false);
                break;

            case DatabaseTypes.SqlServer:
                await context.Database.ExecuteNonQueryAsync(
@"CREATE TABLE [TestTable3] (
    [Id] bigint NOT NULL IDENTITY,
    [TestField] nvarchar(256) NOT NULL,
    CONSTRAINT [PK_TestTable3] PRIMARY KEY ([Id])
);", this._testContext.CancellationToken)
                    .ConfigureAwait(false);

                await context.Database.ExecuteNonQueryAsync(
@"CREATE UNIQUE INDEX [IX_TestTable3_TestField] ON [TestTable3] ([TestField]);", this._testContext.CancellationToken)
                    .ConfigureAwait(false);
                break;

            default:
                throw new UnsupportedDatabaseTypeException();
        }

        FormattableString sql = $"INSERT INTO TestTable3 (TestField) VALUES ({value})";
        await context.Database.ExecuteInsertAsync<long>(sql, this._testContext.CancellationToken).ConfigureAwait(false);

        // ACT / ASSERT
        Exception e = await Assert.ThrowsAsync<Exception>(() => context.Database.ExecuteInsertAsync<long>(sql, this._testContext.CancellationToken), "Unexpected exception").ConfigureAwait(false);
        UniqueConstraintDetails? details = await context.Database.GetUniqueConstraintDetailsAsync(e, this._testContext.CancellationToken).ConfigureAwait(false);

        // Check the details contain the database table name and field name.
        Assert.IsNotNull(details, "Details are null");
        Assert.AreEqual(schema, details.Schema, "Invalid schema name");
        Assert.AreEqual(tableName, details.TableName, "Invalid EF table name");
        Assert.HasCount(1, details.FieldNames, "Invalid field names count");
        Assert.AreEqual(fieldName, details.FieldNames[0], "Invalid EF field name");
    }

    /// <summary>
    /// Test GetUniqueConstraintDetailsAsync with an exception that did not come from the database.
    /// </summary>
    /// <param name="databaseType">Database type.</param>
    /// <returns>A <see cref="Task"/> that represents the asynchronous operation.</returns>
    /// <remarks>The exception is not looked at by the database, so this test does not need a SQL Server.</remarks>
    [TestMethod(DisplayName = "GetUniqueConstraintDetailsAsync with an exception that did not come from the database")]
    [DataRow(DatabaseTypes.Sqlite, DisplayName = DatabaseTypes.Sqlite)]
    [DataRow(DatabaseTypes.SqlServer, DisplayName = DatabaseTypes.SqlServer)]
    public async Task GetUniqueConstraintDetailsAsyncTests_006_Async(string databaseType)
    {
        // ARRANGE
        using Context context = new(databaseType, "Dummy");
        Exception e = new InvalidOperationException("Cannot insert duplicate key row in object 'dbo.TestTable2RealName' with unique index 'IX_TestTable2RealName_TestFieldRealName'");

        // ACT
        UniqueConstraintDetails? details = await context.Database.GetUniqueConstraintDetailsAsync(e, this._testContext.CancellationToken).ConfigureAwait(false);

        // ASSERT
        Assert.IsNull(details, "Details are not null");
    }

    /// <summary>
    /// Test GetUniqueConstraintDetailsAsync with a database exception that is not a unique constraint violation (using SQL).
    /// </summary>
    /// <param name="databaseType">Database type.</param>
    /// <returns>A <see cref="Task"/> that represents the asynchronous operation.</returns>
    /// <remarks>
    /// Inserting a NULL into a NOT NULL field raises a constraint error, but not a unique constraint error
    /// (on SQLite both are error 19, so the message must be checked too).
    /// </remarks>
    [TestMethod(DisplayName = "GetUniqueConstraintDetailsAsync with a database exception that is not a unique constraint violation (SQL)")]
    [DataRow(DatabaseTypes.Sqlite, DisplayName = DatabaseTypes.Sqlite)]
    [DataRow(DatabaseTypes.SqlServer, DisplayName = DatabaseTypes.SqlServer)]
    public async Task GetUniqueConstraintDetailsAsyncTests_007_Async(string databaseType)
    {
        // ARRANGE
        using Context context = await DatabaseUtils.OpenDatabaseAsync(databaseType).ConfigureAwait(false);

        string sql = "INSERT INTO TestTable1 (TestField) VALUES (NULL)";

        // ACT
        Exception e = await Assert.ThrowsAsync<Exception>(() => context.Database.ExecuteNonQueryAsync(sql, this._testContext.CancellationToken), "Unexpected exception").ConfigureAwait(false);
        UniqueConstraintDetails? details = await context.Database.GetUniqueConstraintDetailsAsync(e, this._testContext.CancellationToken).ConfigureAwait(false);

        // ASSERT
        Assert.IsNull(details, "Details are not null");
    }

    /// <summary>
    /// Test GetUniqueConstraintDetailsAsync with a database exception that is not a unique constraint violation (using EF Core).
    /// </summary>
    /// <param name="databaseType">Database type.</param>
    /// <returns>A <see cref="Task"/> that represents the asynchronous operation.</returns>
    /// <remarks>EF Core wraps the database exception inside a DbUpdateException.</remarks>
    [TestMethod(DisplayName = "GetUniqueConstraintDetailsAsync with a database exception that is not a unique constraint violation (EF Core)")]
    [DataRow(DatabaseTypes.Sqlite, DisplayName = DatabaseTypes.Sqlite)]
    [DataRow(DatabaseTypes.SqlServer, DisplayName = DatabaseTypes.SqlServer)]
    public async Task GetUniqueConstraintDetailsAsyncTests_008_Async(string databaseType)
    {
        // ARRANGE
        using Context context = await DatabaseUtils.OpenDatabaseAsync(databaseType).ConfigureAwait(false);

        TestTable1 testTable1 = new() { TestField = null! };
        context.Add(testTable1);

        // ACT
        DbUpdateException e = await Assert.ThrowsExactlyAsync<DbUpdateException>(() => context.SaveChangesAsync(this._testContext.CancellationToken), "Unexpected exception").ConfigureAwait(false);
        UniqueConstraintDetails? details = await context.Database.GetUniqueConstraintDetailsAsync(e, this._testContext.CancellationToken).ConfigureAwait(false);

        // ASSERT
        Assert.IsNull(details, "Details are not null");
    }

    /// <summary>
    /// Test GetUniqueConstraintDetailsAsync with a unique index on more than one field (using SQL only).
    /// </summary>
    /// <param name="databaseType">Database type.</param>
    /// <returns>A <see cref="Task"/> that represents the asynchronous operation.</returns>
    /// <remarks>
    /// The index fields are not in the same order as the table fields, so this checks the field names are returned in index order.
    /// The table is not in EF Core so the database table and field names are returned.
    /// </remarks>
    [TestMethod(DisplayName = "GetUniqueConstraintDetailsAsync with a unique index on more than one field")]
    [DataRow(DatabaseTypes.Sqlite, DisplayName = DatabaseTypes.Sqlite)]
    [DataRow(DatabaseTypes.SqlServer, DisplayName = DatabaseTypes.SqlServer)]
    public async Task GetUniqueConstraintDetailsAsyncTests_009_Async(string databaseType)
    {
        // ARRANGE
        using Context context = await DatabaseUtils.OpenDatabaseAsync(databaseType).ConfigureAwait(false);

        string? schema = null;
        string tableName = "TestTable3";

        switch (databaseType)
        {
            case DatabaseTypes.Sqlite:
                await context.Database.ExecuteNonQueryAsync(
@"CREATE TABLE [TestTable3] (
    [Id] INTEGER NOT NULL CONSTRAINT [PK_TestTable3] PRIMARY KEY AUTOINCREMENT,
    [FieldA] TEXT NOT NULL,
    [FieldB] TEXT NOT NULL
);", this._testContext.CancellationToken)
                    .ConfigureAwait(false);
                break;

            case DatabaseTypes.SqlServer:
                await context.Database.ExecuteNonQueryAsync(
@"CREATE TABLE [TestTable3] (
    [Id] bigint NOT NULL IDENTITY,
    [FieldA] nvarchar(256) NOT NULL,
    [FieldB] nvarchar(256) NOT NULL,
    CONSTRAINT [PK_TestTable3] PRIMARY KEY ([Id])
);", this._testContext.CancellationToken)
                    .ConfigureAwait(false);
                break;

            default:
                throw new UnsupportedDatabaseTypeException();
        }

        await context.Database.ExecuteNonQueryAsync(
@"CREATE UNIQUE INDEX [IX_TestTable3_FieldB_FieldA] ON [TestTable3] ([FieldB], [FieldA]);", this._testContext.CancellationToken)
            .ConfigureAwait(false);

        string valueA = "ValueA";
        string valueB = "ValueB";

        FormattableString sql = $"INSERT INTO TestTable3 (FieldA, FieldB) VALUES ({valueA}, {valueB})";
        await context.Database.ExecuteInsertAsync<long>(sql, this._testContext.CancellationToken).ConfigureAwait(false);

        // ACT
        Exception e = await Assert.ThrowsAsync<Exception>(() => context.Database.ExecuteInsertAsync<long>(sql, this._testContext.CancellationToken), "Unexpected exception").ConfigureAwait(false);
        UniqueConstraintDetails? details = await context.Database.GetUniqueConstraintDetailsAsync(e, this._testContext.CancellationToken).ConfigureAwait(false);

        // ASSERT
        Assert.IsNotNull(details, "Details are null");
        Assert.AreEqual(schema, details.Schema, "Invalid schema name");
        Assert.AreEqual(tableName, details.TableName, "Invalid table name");
        Assert.AreEqual("FieldB,FieldA", string.Join(",", details.FieldNames), "Invalid field names");
    }

    /// <summary>
    /// Test GetUniqueConstraintDetailsAsync on SQL Server with a table in a schema other than dbo (using EF Core).
    /// </summary>
    /// <returns>A <see cref="Task"/> that represents the asynchronous operation.</returns>
    /// <remarks>The table details are held in EF Core, so the EF Core table name and field names are returned.</remarks>
    [TestMethod(DisplayName = "GetUniqueConstraintDetailsAsync on SQL Server with a table in a schema other than dbo (EF Core)")]
    public async Task GetUniqueConstraintDetailsAsyncTests_010_Async()
    {
        // ARRANGE
        string schema = "test";

        using Context context = await DatabaseUtils.OpenDatabaseAsync(
            DatabaseTypes.SqlServer,
            customModelCreationActions: modelBuilder => modelBuilder.Entity<TestTable2>().ToTable("TestTable2RealName", schema))
            .ConfigureAwait(false);

        string value = "TestValue";

        context.Add(new TestTable2 { TestField = value });
        await context.SaveChangesAsync(this._testContext.CancellationToken).ConfigureAwait(false);

        context.Add(new TestTable2 { TestField = value });

        // ACT
        DbUpdateException e = await Assert.ThrowsExactlyAsync<DbUpdateException>(() => context.SaveChangesAsync(this._testContext.CancellationToken), "Unexpected exception").ConfigureAwait(false);
        UniqueConstraintDetails? details = await context.Database.GetUniqueConstraintDetailsAsync(e, this._testContext.CancellationToken).ConfigureAwait(false);

        // ASSERT
        Assert.IsNotNull(details, "Details are null");
        Assert.AreEqual(schema, details.Schema, "Invalid schema name");
        Assert.AreEqual(nameof(TestTable2), details.TableName, "Invalid table name");
        Assert.HasCount(1, details.FieldNames, "Invalid field names count");
        Assert.AreEqual(nameof(TestTable2.TestField), details.FieldNames[0], "Invalid field name");
    }

    /// <summary>
    /// Test GetUniqueConstraintDetailsAsync on SQL Server with a table in a schema other than dbo (using SQL only).
    /// </summary>
    /// <returns>A <see cref="Task"/> that represents the asynchronous operation.</returns>
    /// <remarks>The table is not in EF Core, so the details are read from the database.</remarks>
    [TestMethod(DisplayName = "GetUniqueConstraintDetailsAsync on SQL Server with a table in a schema other than dbo (SQL only)")]
    public async Task GetUniqueConstraintDetailsAsyncTests_011_Async()
    {
        // ARRANGE
        using Context context = await DatabaseUtils.OpenDatabaseAsync(DatabaseTypes.SqlServer).ConfigureAwait(false);

        string schema = "test";
        string tableName = "TestTable3";
        string fieldName = "TestField";

        // CREATE SCHEMA must be the only statement in its batch.
        await context.Database.ExecuteNonQueryAsync("CREATE SCHEMA [test];", this._testContext.CancellationToken).ConfigureAwait(false);

        await context.Database.ExecuteNonQueryAsync(
@"CREATE TABLE [test].[TestTable3] (
    [Id] bigint NOT NULL IDENTITY,
    [TestField] nvarchar(256) NOT NULL,
    CONSTRAINT [PK_TestTable3] PRIMARY KEY ([Id])
);", this._testContext.CancellationToken)
            .ConfigureAwait(false);

        await context.Database.ExecuteNonQueryAsync(
@"CREATE UNIQUE INDEX [IX_TestTable3_TestField] ON [test].[TestTable3] ([TestField]);", this._testContext.CancellationToken)
            .ConfigureAwait(false);

        string value = "TestValue";

        FormattableString sql = $"INSERT INTO test.TestTable3 (TestField) VALUES ({value})";
        await context.Database.ExecuteInsertAsync<long>(sql, this._testContext.CancellationToken).ConfigureAwait(false);

        // ACT
        Exception e = await Assert.ThrowsAsync<Exception>(() => context.Database.ExecuteInsertAsync<long>(sql, this._testContext.CancellationToken), "Unexpected exception").ConfigureAwait(false);
        UniqueConstraintDetails? details = await context.Database.GetUniqueConstraintDetailsAsync(e, this._testContext.CancellationToken).ConfigureAwait(false);

        // ASSERT
        Assert.IsNotNull(details, "Details are null");
        Assert.AreEqual(schema, details.Schema, "Invalid schema name");
        Assert.AreEqual(tableName, details.TableName, "Invalid table name");
        Assert.HasCount(1, details.FieldNames, "Invalid field names count");
        Assert.AreEqual(fieldName, details.FieldNames[0], "Invalid field name");
    }

    #endregion GetUniqueConstraintDetailsAsync tests
}