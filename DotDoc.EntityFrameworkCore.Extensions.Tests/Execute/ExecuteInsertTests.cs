// Copyright ©2021-2026 Mike King.
// This file is licensed to you under the MIT license.
// See the License.txt file in the solution root for more information.

using DotDoc.EntityFrameworkCore.Extensions.DatabaseType;
using DotDoc.EntityFrameworkCore.Extensions.Execute;
using DotDoc.EntityFrameworkCore.Extensions.Tests.Data;
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

    #region public methods

    /// <summary>
    /// Test ExecuteInsertAsync with Null DatabaseFacade Database and FormattableString Sql parameters.
    /// </summary>
    /// <returns>A <see cref="Task"/> that represents the asynchronous operation.</returns>
    [TestMethod(DisplayName = "ExecuteInsertAsync with Null DatabaseFacade Database and FormattableString Sql parameters")]
    public async Task ExecuteInsertTests_001_Async()
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
    [TestMethod(DisplayName = "ExecuteInsertAsync with Null DatabaseFacade Database and String Sql parameters")]
    public async Task ExecuteInsertTests_002_Async()
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
    [TestMethod(DisplayName = "ExecuteInsertAsync with a Null FormattableString Sql parameter")]
    public async Task ExecuteInsertTests_003_Async()
    {
        // ARRANGE
        using Context context = await DatabaseUtils.OpenDatabaseAsync(DatabaseTypes.Sqlite).ConfigureAwait(false);
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
    [TestMethod(DisplayName = "ExecuteInsertAsync with a Null, empty or invalid String Sql parameter")]
    [DataRow(null, typeof(ArgumentNullException), DisplayName = "Null")]
    [DataRow("", typeof(ArgumentException), DisplayName = "Empty")]
    [DataRow("   ", typeof(ArgumentException), DisplayName = "Whitespace only")]
    [DataRow("-- comment", typeof(ArgumentException), DisplayName = "Comment only")]
    [DataRow("INSERT INTO TestTable1 (TestField) VALUES ('abc)", typeof(ArgumentException), DisplayName = "Unclosed string")]
    public async Task ExecuteInsertTests_004_Async(string sql, Type exceptionType)
    {
        // ARRANGE
        using Context context = await DatabaseUtils.OpenDatabaseAsync(DatabaseTypes.Sqlite).ConfigureAwait(false);

        // ACT / ASSERT
        Exception e = await Assert.ThrowsAsync<Exception>(() => context.Database.ExecuteInsertAsync<long>(sql, this._testContext.CancellationToken), "Unexpected exception").ConfigureAwait(false);
        Assert.IsInstanceOfType(e, exceptionType, "Invalid exception type");
    }

    /// <summary>
    /// Test ExecuteInsertAsync with FormattableString Sql parameter.
    /// </summary>
    /// <param name="databaseType">Database type.</param>
    /// <returns>A <see cref="Task"/> that represents the asynchronous operation.</returns>
    [TestMethod(DisplayName = "ExecuteInsertAsync with FormattableString Sql parameter")]
    [DataRow(DatabaseTypes.Sqlite, DisplayName = DatabaseTypes.Sqlite)]
    [DataRow(DatabaseTypes.SqlServer, DisplayName = DatabaseTypes.SqlServer)]
    public async Task ExecuteInsertTests_005_Async(string databaseType)
    {
        // ARRANGE
        using Context context = await DatabaseUtils.OpenDatabaseAsync(databaseType).ConfigureAwait(false);

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
    [TestMethod(DisplayName = "ExecuteInsertAsync with a String Sql parameter")]
    [DataRow(DatabaseTypes.Sqlite, DisplayName = DatabaseTypes.Sqlite)]
    [DataRow(DatabaseTypes.SqlServer, DisplayName = DatabaseTypes.SqlServer)]
    public async Task ExecuteInsertTests_006_Async(string databaseType)
    {
        // ARRANGE
        using Context context = await DatabaseUtils.OpenDatabaseAsync(databaseType).ConfigureAwait(false);

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
    [TestMethod(DisplayName = "ExecuteInsertAsync with a trailing semicolon or comment")]
    [DataRow(DatabaseTypes.Sqlite, ";", DisplayName = $"{DatabaseTypes.Sqlite} Semicolon")]
    [DataRow(DatabaseTypes.Sqlite, " -- new row", DisplayName = $"{DatabaseTypes.Sqlite} Line comment")]
    [DataRow(DatabaseTypes.Sqlite, "; -- new row", DisplayName = $"{DatabaseTypes.Sqlite} Semicolon and line comment")]
    [DataRow(DatabaseTypes.Sqlite, " /* new row */", DisplayName = $"{DatabaseTypes.Sqlite} Block comment")]
    [DataRow(DatabaseTypes.Sqlite, " /* new row", DisplayName = $"{DatabaseTypes.Sqlite} Unclosed block comment")]
    [DataRow(DatabaseTypes.SqlServer, ";", DisplayName = $"{DatabaseTypes.SqlServer} Semicolon")]
    [DataRow(DatabaseTypes.SqlServer, " -- new row", DisplayName = $"{DatabaseTypes.SqlServer} Line comment")]
    [DataRow(DatabaseTypes.SqlServer, "; -- new row", DisplayName = $"{DatabaseTypes.SqlServer} Semicolon and line comment")]
    [DataRow(DatabaseTypes.SqlServer, " /* new row */", DisplayName = $"{DatabaseTypes.SqlServer} Block comment")]
    public async Task ExecuteInsertTests_007_Async(string databaseType, string ending)
    {
        // ARRANGE
        using Context context = await DatabaseUtils.OpenDatabaseAsync(databaseType).ConfigureAwait(false);

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
    [TestMethod(DisplayName = "ExecuteInsertAsync with a Null IEnumerable Parameters parameter")]
    public async Task ExecuteInsertTests_008_Async()
    {
        // ARRANGE
        using Context context = await DatabaseUtils.OpenDatabaseAsync(DatabaseTypes.Sqlite).ConfigureAwait(false);

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
    [TestMethod(DisplayName = "ExecuteInsertAsync with an unclosed block comment on SQL Server")]
    public async Task ExecuteInsertTests_009_Async()
    {
        // ARRANGE
        using Context context = new(DatabaseTypes.SqlServer, "Server=localhost;Initial Catalog=Dummy");

        string sql = "INSERT INTO TestTable1 (TestField) VALUES ({0}) /* new row";

        // ACT / ASSERT
        await Assert.ThrowsExactlyAsync<ArgumentException>(() => context.Database.ExecuteInsertAsync<long>(sql, this._testContext.CancellationToken, "TestValue"), "Unexpected exception").ConfigureAwait(false);
    }

    #endregion public methods
}
