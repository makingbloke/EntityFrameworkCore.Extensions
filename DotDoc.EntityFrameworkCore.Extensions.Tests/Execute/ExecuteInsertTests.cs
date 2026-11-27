// Copyright ©2021-2026 Mike King.
// This file is licensed to you under the MIT license.
// See the License.txt file in the solution root for more information.

using DotDoc.EntityFrameworkCore.Extensions.DatabaseType;
using DotDoc.EntityFrameworkCore.Extensions.Execute;
using DotDoc.EntityFrameworkCore.Extensions.Tests.Data;
using DotDoc.EntityFrameworkCore.Extensions.Tests.Model;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace DotDoc.EntityFrameworkCore.Extensions.Tests.Execute;

/// <summary>
/// Tests for ExecuteInsert extensions.
/// </summary>
[TestClass]
public class ExecuteInsertTests
{
    #region private fields

    /// <summary>
    /// The MSTest <see cref="TestContext"/> for the current test.
    /// </summary>
    private readonly TestContext _testContext;

    #endregion private fields

    #region public constructors

    /// <summary>
    /// Initializes a new instance of the <see cref="ExecuteInsertTests"/> class.
    /// </summary>
    /// <param name="testContext">The MSTest test context.</param>
    public ExecuteInsertTests(TestContext testContext)
    {
        this._testContext = testContext;
    }

    #endregion public constructors

    #region ExecuteInsertAsync tests

    /// <summary>
    /// Test ExecuteInsertAsync with Null DatabaseFacade Database and FormattableString Sql parameters.
    /// </summary>
    /// <returns>A <see cref="Task"/> that represents the asynchronous operation.</returns>
    [TestMethod]
    public async Task ExecuteInsertAsync_NullDatabaseWithFormattableSql_ThrowsArgumentNullException()
    {
        // ARRANGE
        DatabaseFacade database = null!;
        FormattableString sql = $"dummy";

        // ACT / ASSERT
        await Assert.ThrowsExactlyAsync<ArgumentNullException>(() => database.ExecuteInsertAsync<long>(sql, this._testContext.CancellationToken), "Unexpected exception").ConfigureAwait(false);
    }

    /// <summary>
    /// Test ExecuteInsertAsync with Null DatabaseFacade Database and String Sql parameters.
    /// </summary>
    /// <returns>A <see cref="Task"/> that represents the asynchronous operation.</returns>
    [TestMethod]
    public async Task ExecuteInsertAsync_NullDatabaseWithStringSql_ThrowsArgumentNullException()
    {
        // ARRANGE
        DatabaseFacade database = null!;
        string sql = "dummy";

        // ACT / ASSERT
        await Assert.ThrowsExactlyAsync<ArgumentNullException>(() => database.ExecuteInsertAsync<long>(sql, this._testContext.CancellationToken), "Unexpected exception").ConfigureAwait(false);
    }

    /// <summary>
    /// Test ExecuteInsertAsync with a Null FormattableString Sql parameter.
    /// </summary>
    /// <returns>A <see cref="Task"/> that represents the asynchronous operation.</returns>
    [TestMethod]
    public async Task ExecuteInsertAsync_NullFormattableSql_ThrowsArgumentNullException()
    {
        // ARRANGE
        using Context context = await DatabaseUtils.OpenDatabaseAsync(this._testContext, DatabaseTypes.Sqlite).ConfigureAwait(false);
        FormattableString sql = null!;

        // ACT / ASSERT
        await Assert.ThrowsExactlyAsync<ArgumentNullException>(() => context.Database.ExecuteInsertAsync<long>(sql, this._testContext.CancellationToken), "Unexpected exception").ConfigureAwait(false);
    }

    /// <summary>
    /// Test ExecuteInsertAsync with a Null, empty or invalid String Sql parameter.
    /// </summary>
    /// <param name="sql">The SQL string.</param>
    /// <param name="exceptionType">The type of exception raised.</param>
    /// <returns>A <see cref="Task"/> that represents the asynchronous operation.</returns>
    [TestMethod]
    [DataRow(null, typeof(ArgumentNullException), DisplayName = "InvalidStringSql Null")]
    [DataRow("", typeof(ArgumentException), DisplayName = "InvalidStringSql Empty")]
    [DataRow("   ", typeof(ArgumentException), DisplayName = "InvalidStringSql Whitespace only")]
    [DataRow("-- comment", typeof(ArgumentException), DisplayName = "InvalidStringSql Comment only")]
    [DataRow("INSERT INTO TestTable1 (TestField) VALUES ('abc)", typeof(ArgumentException), DisplayName = "InvalidStringSql Unclosed string")]
    public async Task ExecuteInsertAsync_InvalidStringSql_ThrowsArgumentException(string sql, Type exceptionType)
    {
        // ARRANGE
        using Context context = await DatabaseUtils.OpenDatabaseAsync(this._testContext, DatabaseTypes.Sqlite).ConfigureAwait(false);

        // ACT / ASSERT
        Exception e = await Assert.ThrowsAsync<Exception>(() => context.Database.ExecuteInsertAsync<long>(sql, this._testContext.CancellationToken), "Unexpected exception").ConfigureAwait(false);
        Assert.IsInstanceOfType(e, exceptionType, "Invalid exception type");
    }

    /// <summary>
    /// Test ExecuteInsertAsync with FormattableString Sql parameter.
    /// </summary>
    /// <param name="databaseType">Database type.</param>
    /// <returns>A <see cref="Task"/> that represents the asynchronous operation.</returns>
    [TestMethod]
    [DataRow(DatabaseTypes.Sqlite, DisplayName = $"FormattableSql {DatabaseTypes.Sqlite}")]
    [DataRow(DatabaseTypes.SqlServer, DisplayName = $"FormattableSql {DatabaseTypes.SqlServer}")]
    public async Task ExecuteInsertAsync_FormattableSql_ReturnsNewId(string databaseType)
    {
        // ARRANGE
        using Context context = await DatabaseUtils.OpenDatabaseAsync(this._testContext, databaseType).ConfigureAwait(false);

        string value = "TestValue";
        FormattableString sql = $"INSERT INTO TestTable1 (TestField) VALUES ({value})";

        // ACT
        long id = await context.Database.ExecuteInsertAsync<long>(sql, this._testContext.CancellationToken).ConfigureAwait(false);

        // ASSERT
        TestTable1 result = await context.TestTable1.SingleAsync(e => e.Id == id, this._testContext.CancellationToken).ConfigureAwait(false);

        Assert.AreEqual(id, result.Id, "Invalid Id");
        Assert.AreEqual(value, result.TestField, "Invalid Value");
    }

    /// <summary>
    /// Test ExecuteInsertAsync with a String Sql parameter.
    /// </summary>
    /// <param name="databaseType">Database type.</param>
    /// <returns>A <see cref="Task"/> that represents the asynchronous operation.</returns>
    [TestMethod]
    [DataRow(DatabaseTypes.Sqlite, DisplayName = $"StringSql {DatabaseTypes.Sqlite}")]
    [DataRow(DatabaseTypes.SqlServer, DisplayName = $"StringSql {DatabaseTypes.SqlServer}")]
    public async Task ExecuteInsertAsync_StringSql_ReturnsNewId(string databaseType)
    {
        // ARRANGE
        using Context context = await DatabaseUtils.OpenDatabaseAsync(this._testContext, databaseType).ConfigureAwait(false);

        string value = "TestValue";
        string sql = "INSERT INTO TestTable1 (TestField) VALUES ({0})";

        // ACT
        long id = await context.Database.ExecuteInsertAsync<long>(sql, this._testContext.CancellationToken, value).ConfigureAwait(false);

        // ASSERT
        TestTable1 result = await context.TestTable1.SingleAsync(e => e.Id == id, this._testContext.CancellationToken).ConfigureAwait(false);

        Assert.AreEqual(id, result.Id, "Invalid Id");
        Assert.AreEqual(value, result.TestField, "Invalid Value");
    }

    /// <summary>
    /// Test ExecuteInsertAsync with SQL ending in a semicolon and / or a comment, which must not hide the query that gets the new Id.
    /// </summary>
    /// <param name="databaseType">Database type.</param>
    /// <param name="ending">The text added to the end of the insert SQL.</param>
    /// <returns>A <see cref="Task"/> that represents the asynchronous operation.</returns>
    [TestMethod]
    [DataRow(DatabaseTypes.Sqlite, ";", DisplayName = $"TrailingSemicolonOrComment {DatabaseTypes.Sqlite} Semicolon")]
    [DataRow(DatabaseTypes.Sqlite, " -- new row", DisplayName = $"TrailingSemicolonOrComment {DatabaseTypes.Sqlite} Line comment")]
    [DataRow(DatabaseTypes.Sqlite, "; -- new row", DisplayName = $"TrailingSemicolonOrComment {DatabaseTypes.Sqlite} Semicolon and line comment")]
    [DataRow(DatabaseTypes.Sqlite, " /* new row */", DisplayName = $"TrailingSemicolonOrComment {DatabaseTypes.Sqlite} Block comment")]
    [DataRow(DatabaseTypes.Sqlite, " /* new row", DisplayName = $"TrailingSemicolonOrComment {DatabaseTypes.Sqlite} Unclosed block comment")]
    [DataRow(DatabaseTypes.SqlServer, ";", DisplayName = $"TrailingSemicolonOrComment {DatabaseTypes.SqlServer} Semicolon")]
    [DataRow(DatabaseTypes.SqlServer, " -- new row", DisplayName = $"TrailingSemicolonOrComment {DatabaseTypes.SqlServer} Line comment")]
    [DataRow(DatabaseTypes.SqlServer, "; -- new row", DisplayName = $"TrailingSemicolonOrComment {DatabaseTypes.SqlServer} Semicolon and line comment")]
    [DataRow(DatabaseTypes.SqlServer, " /* new row */", DisplayName = $"TrailingSemicolonOrComment {DatabaseTypes.SqlServer} Block comment")]
    public async Task ExecuteInsertAsync_TrailingSemicolonOrComment_ReturnsNewId(string databaseType, string ending)
    {
        // ARRANGE
        using Context context = await DatabaseUtils.OpenDatabaseAsync(this._testContext, databaseType).ConfigureAwait(false);

        string value = "TestValue";
        string sql = "INSERT INTO TestTable1 (TestField) VALUES ({0})" + ending;

        // ACT
        long id = await context.Database.ExecuteInsertAsync<long>(sql, this._testContext.CancellationToken, value).ConfigureAwait(false);

        // ASSERT
        TestTable1 result = await context.TestTable1.SingleAsync(e => e.Id == id, this._testContext.CancellationToken).ConfigureAwait(false);

        Assert.AreEqual(id, result.Id, "Invalid Id");
        Assert.AreEqual(value, result.TestField, "Invalid Value");
    }

    /// <summary>
    /// Test ExecuteInsertAsync with a Null IEnumerable Parameters parameter.
    /// </summary>
    /// <returns>A <see cref="Task"/> that represents the asynchronous operation.</returns>
    [TestMethod]
    public async Task ExecuteInsertAsync_NullParameters_ThrowsArgumentNullException()
    {
        // ARRANGE
        using Context context = await DatabaseUtils.OpenDatabaseAsync(this._testContext, DatabaseTypes.Sqlite).ConfigureAwait(false);

        string sql = "INSERT INTO TestTable1 (TestField) VALUES ('TestValue')";
        IEnumerable<object?> parameters = null!;

        // ACT / ASSERT
        await Assert.ThrowsExactlyAsync<ArgumentNullException>(() => context.Database.ExecuteInsertAsync<long>(sql, this._testContext.CancellationToken, parameters), "Unexpected exception").ConfigureAwait(false);
    }

    /// <summary>
    /// Test ExecuteInsertAsync with an unclosed block comment on SQL Server (SQL Server, unlike SQLite, does not allow it).
    /// </summary>
    /// <returns>A <see cref="Task"/> that represents the asynchronous operation.</returns>
    /// <remarks>The SQL is rejected before the database is used, so this test does not need a SQL Server.</remarks>
    [TestMethod]
    public async Task ExecuteInsertAsync_SqlServerUnclosedBlockComment_ThrowsArgumentException()
    {
        // ARRANGE
        using Context context = new(DatabaseTypes.SqlServer, "Server=localhost;Initial Catalog=Dummy");

        string sql = "INSERT INTO TestTable1 (TestField) VALUES ({0}) /* new row";

        // ACT / ASSERT
        await Assert.ThrowsExactlyAsync<ArgumentException>(() => context.Database.ExecuteInsertAsync<long>(sql, this._testContext.CancellationToken, "TestValue"), "Unexpected exception").ConfigureAwait(false);
    }

    /// <summary>
    /// Test ExecuteInsertAsync on SQLite with an insert that is ignored because of a duplicate key, after another insert on the same connection.
    /// </summary>
    /// <param name="sql">The insert SQL ({0} is the duplicate Id, {1} the value).</param>
    /// <returns>A <see cref="Task"/> that represents the asynchronous operation.</returns>
    /// <remarks>SQLite's last_insert_rowid() is still the earlier insert's rowid, which must not be returned.</remarks>
    [TestMethod]
    [DataRow("INSERT OR IGNORE INTO TestTable1 (Id, TestField) VALUES ({0}, {1})", DisplayName = "SqliteInsertIgnored INSERT OR IGNORE")]
    [DataRow("INSERT INTO TestTable1 (Id, TestField) VALUES ({0}, {1}) ON CONFLICT DO NOTHING", DisplayName = "SqliteInsertIgnored ON CONFLICT DO NOTHING")]
    public async Task ExecuteInsertAsync_SqliteInsertIgnored_ReturnsDefault(string sql)
    {
        // ARRANGE
        using Context context = await DatabaseUtils.OpenDatabaseAsync(this._testContext, DatabaseTypes.Sqlite).ConfigureAwait(false);

        long existingId = await context.Database.ExecuteInsertAsync<long>("INSERT INTO TestTable1 (TestField) VALUES ({0})", this._testContext.CancellationToken, "Existing").ConfigureAwait(false);
        int rowCount = await context.TestTable1.CountAsync(this._testContext.CancellationToken).ConfigureAwait(false);

        // ACT
        long id = await context.Database.ExecuteInsertAsync<long>(sql, this._testContext.CancellationToken, existingId, "Duplicate").ConfigureAwait(false);

        // ASSERT
        Assert.AreNotEqual(0L, existingId, "Invalid existing Id");
        Assert.AreEqual(0L, id, "Invalid Id");
        Assert.AreEqual(rowCount, await context.TestTable1.CountAsync(this._testContext.CancellationToken).ConfigureAwait(false), "A row was inserted");
    }

    /// <summary>
    /// Test ExecuteInsertAsync with an INSERT ... SELECT that inserts nothing, after another insert on the same connection.
    /// </summary>
    /// <param name="databaseType">Database type.</param>
    /// <returns>A <see cref="Task"/> that represents the asynchronous operation.</returns>
    [TestMethod]
    [DataRow(DatabaseTypes.Sqlite, DisplayName = $"InsertSelectInsertsNothing {DatabaseTypes.Sqlite}")]
    [DataRow(DatabaseTypes.SqlServer, DisplayName = $"InsertSelectInsertsNothing {DatabaseTypes.SqlServer}")]
    public async Task ExecuteInsertAsync_InsertSelectInsertsNothing_ReturnsDefault(string databaseType)
    {
        // ARRANGE
        using Context context = await DatabaseUtils.OpenDatabaseAsync(this._testContext, databaseType).ConfigureAwait(false);

        string value = "Existing";
        long existingId = await context.Database.ExecuteInsertAsync<long>("INSERT INTO TestTable1 (TestField) VALUES ({0})", this._testContext.CancellationToken, value).ConfigureAwait(false);
        int rowCount = await context.TestTable1.CountAsync(this._testContext.CancellationToken).ConfigureAwait(false);

        string sql = "INSERT INTO TestTable1 (TestField) SELECT {0} WHERE NOT EXISTS (SELECT 1 FROM TestTable1 WHERE TestField = {1})";

        // ACT
        long id = await context.Database.ExecuteInsertAsync<long>(sql, this._testContext.CancellationToken, value, value).ConfigureAwait(false);

        // ASSERT
        Assert.AreNotEqual(0L, existingId, "Invalid existing Id");
        Assert.AreEqual(0L, id, "Invalid Id");
        Assert.AreEqual(rowCount, await context.TestTable1.CountAsync(this._testContext.CancellationToken).ConfigureAwait(false), "A row was inserted");
    }

    #endregion ExecuteInsertAsync tests
}
