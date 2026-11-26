// Copyright ©2021-2026 Mike King.
// This file is licensed to you under the MIT license.
// See the License.txt file in the solution root for more information.

using DotDoc.EntityFrameworkCore.Extensions.DatabaseType;
using DotDoc.EntityFrameworkCore.Extensions.Execute;
using DotDoc.EntityFrameworkCore.Extensions.Tests.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using System.Data;

namespace DotDoc.EntityFrameworkCore.Extensions.Tests.Execute;

/// <summary>
/// Tests for ExecutePagedQuery extensions.
/// </summary>
[TestClass]
public class ExecutePagedQueryTests
{
    #region private fields

    /// <summary>
    /// The MSTest <see cref="TestContext"/> for the current test.
    /// </summary>
    private readonly TestContext _testContext;

    #endregion private fields

    #region public constructors

    /// <summary>
    /// Initializes a new instance of the <see cref="ExecutePagedQueryTests"/> class.
    /// </summary>
    /// <param name="testContext">The MSTest test context.</param>
    public ExecutePagedQueryTests(TestContext testContext)
    {
        this._testContext = testContext;
    }

    #endregion public constructors

    #region ExecutePagedQueryAsync tests

    /// <summary>
    /// Test ExecutePagedQueryAsync with Null DatabaseFacade Database and FormattableString Sql parameters.
    /// </summary>
    /// <returns>A <see cref="Task"/> that represents the asynchronous operation.</returns>
    [TestMethod(DisplayName = "ExecutePagedQueryAsync with Null DatabaseFacade Database and FormattableString Sql parameters")]
    public async Task ExecutePagedQueryTests_001_Async()
    {
        // ARRANGE
        DatabaseFacade database = null!;
        FormattableString sql = $"dummy";
        long page = 0;
        long pageSize = 1;

        // ACT / ASSERT
        await Assert.ThrowsExactlyAsync<ArgumentNullException>(() => database.ExecutePagedQueryAsync<TestTable1>(sql, page, pageSize, this._testContext.CancellationToken), "Unexpected exception").ConfigureAwait(false);
    }

    /// <summary>
    /// Test ExecutePagedQueryAsync with Null DatabaseFacade Database and String Sql parameters.
    /// </summary>
    /// <returns>A <see cref="Task"/> that represents the asynchronous operation.</returns>
    [TestMethod(DisplayName = "ExecutePagedQueryAsync with Null DatabaseFacade Database and String Sql parameters")]
    public async Task ExecutePagedQueryTests_002_Async()
    {
        // ARRANGE
        DatabaseFacade database = null!;
        string sql = "dummy";
        long page = 0;
        long pageSize = 1;

        // ACT / ASSERT
        await Assert.ThrowsExactlyAsync<ArgumentNullException>(() => database.ExecutePagedQueryAsync<TestTable1>(sql, page, pageSize, this._testContext.CancellationToken), "Unexpected exception").ConfigureAwait(false);
    }

    /// <summary>
    /// Test ExecutePagedQueryAsync with a Null FormattableString Sql parameter.
    /// </summary>
    /// <returns>A <see cref="Task"/> that represents the asynchronous operation.</returns>
    [TestMethod(DisplayName = "ExecutePagedQueryAsync with a Null FormattableString Sql parameter")]
    public async Task ExecutePagedQueryTests_003_Async()
    {
        // ARRANGE
        using Context context = await DatabaseUtils.OpenDatabaseAsync(this._testContext, DatabaseTypes.Sqlite).ConfigureAwait(false);

        FormattableString sql = null!;
        long page = 0;
        long pageSize = 1;

        // ACT / ASSERT
        await Assert.ThrowsExactlyAsync<ArgumentNullException>(() => context.Database.ExecutePagedQueryAsync<TestTable1>(sql, page, pageSize, this._testContext.CancellationToken), "Unexpected exception").ConfigureAwait(false);
    }

    /// <summary>
    /// Test ExecutePagedQueryAsync with a Null or empty String Sql parameter.
    /// </summary>
    /// <param name="sql">The SQL string.</param>
    /// <param name="exceptionType">The type of exception raised.</param>
    /// <returns>A <see cref="Task"/> that represents the asynchronous operation.</returns>
    [TestMethod(DisplayName = "ExecutePagedQueryAsync with a Null or empty String Sql parameter")]
    [DataRow(null, typeof(ArgumentNullException), DisplayName = "Null")]
    [DataRow("", typeof(ArgumentException), DisplayName = "Empty")]
    public async Task ExecutePagedQueryTests_004_Async(string sql, Type exceptionType)
    {
        // ARRANGE
        using Context context = await DatabaseUtils.OpenDatabaseAsync(this._testContext, DatabaseTypes.Sqlite).ConfigureAwait(false);

        long page = 0;
        long pageSize = 1;

        // ACT / ASSERT
        Exception e = await Assert.ThrowsAsync<Exception>(() => context.Database.ExecutePagedQueryAsync<TestTable1>(sql, page, pageSize, this._testContext.CancellationToken), "Unexpected exception").ConfigureAwait(false);
        Assert.IsInstanceOfType(e, exceptionType, "Invalid exception type");
    }

    /// <summary>
    /// Test ExecutePagedQueryAsync with FormattableString Sql and negative long Page parameter.
    /// </summary>
    /// <returns>A <see cref="Task"/> that represents the asynchronous operation.</returns>
    [TestMethod(DisplayName = "ExecutePagedQueryAsync with FormattableString Sql and negative long Page parameters")]
    public async Task ExecutePagedQueryTests_005_Async()
    {
        // ARRANGE
        using Context context = await DatabaseUtils.OpenDatabaseAsync(this._testContext, DatabaseTypes.Sqlite).ConfigureAwait(false);

        FormattableString sql = $"dummy";
        long page = -1;
        long pageSize = 1;

        // ACT / ASSERT
        await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() => context.Database.ExecutePagedQueryAsync<TestTable1>(sql, page, pageSize, this._testContext.CancellationToken), "Unexpected exception").ConfigureAwait(false);
    }

    /// <summary>
    /// Test ExecutePagedQueryAsync with String Sql and negative long Page parameter.
    /// </summary>
    /// <returns>A <see cref="Task"/> that represents the asynchronous operation.</returns>
    [TestMethod(DisplayName = "ExecutePagedQueryAsync with String Sql and negative long Page parameters")]
    public async Task ExecutePagedQueryTests_006_Async()
    {
        // ARRANGE
        using Context context = await DatabaseUtils.OpenDatabaseAsync(this._testContext, DatabaseTypes.Sqlite).ConfigureAwait(false);

        string sql = "dummy";
        long page = -1;
        long pageSize = 1;

        // ACT / ASSERT
        await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() => context.Database.ExecutePagedQueryAsync<TestTable1>(sql, page, pageSize, this._testContext.CancellationToken), "Unexpected exception").ConfigureAwait(false);
    }

    /// <summary>
    /// Test ExecutePagedQueryAsync with FormattableString Sql and zero or negative long PageSize parameter.
    /// </summary>
    /// <param name="pageSize">The page size.</param>
    /// <returns>A <see cref="Task"/> that represents the asynchronous operation.</returns>
    [TestMethod(DisplayName = "ExecutePagedQueryAsync with FormattableString Sql and zero or negative long PageSize parameters")]
    [DataRow(0, DisplayName = "0")]
    [DataRow(-1, DisplayName = "-1")]
    public async Task ExecutePagedQueryTests_007_Async(long pageSize)
    {
        // ARRANGE
        using Context context = await DatabaseUtils.OpenDatabaseAsync(this._testContext, DatabaseTypes.Sqlite).ConfigureAwait(false);

        FormattableString sql = $"dummy";
        long page = 0;

        // ACT / ASSERT
        await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() => context.Database.ExecutePagedQueryAsync<TestTable1>(sql, page, pageSize, this._testContext.CancellationToken), "Unexpected exception").ConfigureAwait(false);
    }

    /// <summary>
    /// Test ExecutePagedQueryAsync with String Sql and zero or negative long PageSize parameter.
    /// </summary>
    /// <param name="pageSize">The page size.</param>
    /// <returns>A <see cref="Task"/> that represents the asynchronous operation.</returns>
    [TestMethod(DisplayName = "ExecutePagedQueryAsync with String Sql and zero or negative long PageSize parameters")]
    [DataRow(0, DisplayName = "0")]
    [DataRow(-1, DisplayName = "-1")]
    public async Task ExecutePagedQueryTests_008_Async(long pageSize)
    {
        // ARRANGE
        using Context context = await DatabaseUtils.OpenDatabaseAsync(this._testContext, DatabaseTypes.Sqlite).ConfigureAwait(false);

        string sql = "dummy";
        long page = 0;

        // ACT / ASSERT
        await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() => context.Database.ExecutePagedQueryAsync<TestTable1>(sql, page, pageSize, this._testContext.CancellationToken), "Unexpected exception").ConfigureAwait(false);
    }

    /// <summary>
    /// Test ExecutePagedQueryAsync with FormattableString Sql parameter.
    /// </summary>
    /// <param name="databaseType">Database type.</param>
    /// <returns>A <see cref="Task"/> that represents the asynchronous operation.</returns>
    [TestMethod(DisplayName = "ExecutePagedQueryAsync with FormattableString Sql parameter")]
    [DataRow(DatabaseTypes.Sqlite, DisplayName = DatabaseTypes.Sqlite)]
    [DataRow(DatabaseTypes.SqlServer, DisplayName = DatabaseTypes.SqlServer)]
    public async Task ExecutePagedQueryTests_009_Async(string databaseType)
    {
        // ARRANGE
        using Context context = await DatabaseUtils.OpenDatabaseAsync(this._testContext, databaseType).ConfigureAwait(false);

        int recordCount = 20;
        FormattableString sql = $"SELECT * FROM TestTable1 WHERE ID <= {recordCount}";
        long page = 2;
        long pageSize = 5;

        string value = "TestValue";

        await DatabaseUtils.CreateTestTableEntriesAsync(context, value, recordCount, this._testContext.CancellationToken).ConfigureAwait(false);

        // ACT
        PagedQueryResult<TestTable1> result = await context.Database.ExecutePagedQueryAsync<TestTable1>(sql, page, pageSize, this._testContext.CancellationToken).ConfigureAwait(false);

        // ASSERT
        Assert.AreEqual(page, result.Page, "Invalid page");
        Assert.AreEqual(pageSize, result.PageSize, "Invalid page size");
        Assert.AreEqual(recordCount, result.RecordCount, "Invalid record count");
        Assert.AreEqual(4L, result.PageCount, "Invalid page count");
        Assert.HasCount((int)pageSize, result.Result, "Invalid list row count");

        for (int i = 0; i < pageSize; i++)
        {
            Assert.AreEqual((page * pageSize) + i + 1, result.Result[i].Id, $"invalid Id[{i}]");
        }
    }

    /// <summary>
    /// Test ExecutePagedQueryAsyncwith a String Sql parameter.
    /// </summary>
    /// <param name="databaseType">Database type.</param>
    /// <returns>A <see cref="Task"/> that represents the asynchronous operation.</returns>
    [TestMethod(DisplayName = "ExecutePagedQueryAsync with a String Sql parameter")]
    [DataRow(DatabaseTypes.Sqlite, DisplayName = DatabaseTypes.Sqlite)]
    [DataRow(DatabaseTypes.SqlServer, DisplayName = DatabaseTypes.SqlServer)]
    public async Task ExecutePagedQueryTests_010_Async(string databaseType)
    {
        // ARRANGE
        using Context context = await DatabaseUtils.OpenDatabaseAsync(this._testContext, databaseType).ConfigureAwait(false);

        int recordCount = 20;
        string sql = "SELECT * FROM TestTable1 WHERE ID <= {0}";
        long page = 2;
        long pageSize = 5;

        string value = "TestValue";

        await DatabaseUtils.CreateTestTableEntriesAsync(context, value, recordCount, this._testContext.CancellationToken).ConfigureAwait(false);

        // ACT
        PagedQueryResult<TestTable1> result = await context.Database.ExecutePagedQueryAsync<TestTable1>(sql, page, pageSize, this._testContext.CancellationToken, recordCount).ConfigureAwait(false);

        // ASSERT
        Assert.AreEqual(page, result.Page, "Invalid page");
        Assert.AreEqual(pageSize, result.PageSize, "Invalid page size");
        Assert.AreEqual(recordCount, result.RecordCount, "Invalid record count");
        Assert.AreEqual(4L, result.PageCount, "Invalid page count");
        Assert.HasCount((int)pageSize, result.Result, "Invalid list row count");

        for (int i = 0; i < pageSize; i++)
        {
            Assert.AreEqual((page * pageSize) + i + 1, result.Result[i].Id, $"invalid Id[{i}]");
        }
    }

    /// <summary>
    /// Test ExecutePagedQueryAsync handling of SQL queries with Order By clauses.
    /// </summary>
    /// <param name="databaseType">Database type.</param>
    /// <returns>A <see cref="Task"/> that represents the asynchronous operation.</returns>
    [TestMethod(DisplayName = "ExecutePagedQueryAsync handling of SQL queries with Order By clauses")]
    [DataRow(DatabaseTypes.Sqlite, DisplayName = DatabaseTypes.Sqlite)]
    [DataRow(DatabaseTypes.SqlServer, DisplayName = DatabaseTypes.SqlServer)]
    public async Task ExecutePagedQueryTests_011_Async(string databaseType)
    {
        // ARRANGE
        using Context context = await DatabaseUtils.OpenDatabaseAsync(this._testContext, databaseType).ConfigureAwait(false);

        int recordCount = 20;
        FormattableString sql = $"SELECT * FROM TestTable1 WHERE ID <= {recordCount} ORDER BY ID DESC";
        long page = 2;
        long pageSize = 5;

        string value = "TestValue";

        await DatabaseUtils.CreateTestTableEntriesAsync(context, value, recordCount, this._testContext.CancellationToken).ConfigureAwait(false);

        // ACT
        PagedQueryResult<TestTable1> pageResult = await context.Database.ExecutePagedQueryAsync<TestTable1>(sql, page, pageSize, this._testContext.CancellationToken).ConfigureAwait(false);

        // ASSERT
        Assert.AreEqual(page, pageResult.Page, "Invalid page");
        Assert.AreEqual(recordCount, pageResult.RecordCount, "Invalid record count");
        Assert.HasCount((int)pageSize, pageResult.Result, "Invalid list row count");

        for (int i = 0; i < pageSize; i++)
        {
            Assert.AreEqual(recordCount - (page * pageSize) - i, pageResult.Result[i].Id, $"invalid Id[{i}]");
        }
    }

    /// <summary>
    /// Test ExecutePagedQueryAsync handling of page number being greater than the total number of pages.
    /// The page number should be reduced to the last one.
    /// </summary>
    /// <param name="databaseType">Database type.</param>
    /// <returns>A <see cref="Task"/> that represents the asynchronous operation.</returns>
    [TestMethod(DisplayName = "ExecutePagedQueryAsync handling of page number being greater than the total number of pages")]
    [DataRow(DatabaseTypes.Sqlite, DisplayName = DatabaseTypes.Sqlite)]
    [DataRow(DatabaseTypes.SqlServer, DisplayName = DatabaseTypes.SqlServer)]
    public async Task ExecutePagedQueryTests_012_Async(string databaseType)
    {
        // ARRANGE
        using Context context = await DatabaseUtils.OpenDatabaseAsync(this._testContext, databaseType).ConfigureAwait(false);

        int recordCount = 20;
        string sql = $"SELECT * FROM TestTable1 WHERE ID <= {recordCount}";
        long page = 999;
        long pageSize = 5;

        string value = "TestValue";

        await DatabaseUtils.CreateTestTableEntriesAsync(context, value, recordCount, this._testContext.CancellationToken).ConfigureAwait(false);

        // 20 records at 5 per page is 4 pages, so the last page is 3.
        long expectedPage = 3;

        // ACT
        PagedQueryResult<TestTable1> pageResult = await context.Database.ExecutePagedQueryAsync<TestTable1>(sql, page, pageSize, this._testContext.CancellationToken).ConfigureAwait(false);

        // ASSERT
        Assert.AreEqual(expectedPage, pageResult.Page, "Invalid page");
    }

    /// <summary>
    /// Test ExecutePagedQueryAsync with different forms of SQL query (comments, strings, quoted identifiers, subqueries, CTEs etc.).
    /// </summary>
    /// <param name="databaseType">Database type.</param>
    /// <param name="sql">The SQL query.</param>
    /// <param name="expectedRecordCount">The expected record count.</param>
    /// <param name="expectedIds">The expected Ids on page 1 (with a page size of 3), or <see langword="null"/> if the order is not defined.</param>
    /// <returns>A <see cref="Task"/> that represents the asynchronous operation.</returns>
    /// <remarks>The test data is 5 x 'A' (Ids 1-5), 3 x 'B' (Ids 6-8) and 2 x 'C' (Ids 9-10).</remarks>
    [TestMethod(DisplayName = "ExecutePagedQueryAsync with different forms of SQL query")]
    [DataRow(DatabaseTypes.Sqlite, "SELECT Id, TestField\r\nFROM TestTable1\r\nORDER BY Id", 10L, "4,5,6", DisplayName = $"{DatabaseTypes.Sqlite} Multi-line SQL")]
    [DataRow(DatabaseTypes.SqlServer, "SELECT Id, TestField\r\nFROM TestTable1\r\nORDER BY Id", 10L, "4,5,6", DisplayName = $"{DatabaseTypes.SqlServer} Multi-line SQL")]
    [DataRow(DatabaseTypes.Sqlite, "SELECT * FROM TestTable1 ORDER BY TestField DESC, Id", 10L, "7,8,1", DisplayName = $"{DatabaseTypes.Sqlite} ORDER BY with multiple columns")]
    [DataRow(DatabaseTypes.SqlServer, "SELECT * FROM TestTable1 ORDER BY TestField DESC, Id", 10L, "7,8,1", DisplayName = $"{DatabaseTypes.SqlServer} ORDER BY with multiple columns")]
    [DataRow(DatabaseTypes.Sqlite, "SELECT Id, TestField, ROW_NUMBER() OVER (ORDER BY Id DESC) AS RowNumber FROM TestTable1 ORDER BY Id", 10L, "4,5,6", DisplayName = $"{DatabaseTypes.Sqlite} Window function with ORDER BY")]
    [DataRow(DatabaseTypes.SqlServer, "SELECT Id, TestField, ROW_NUMBER() OVER (ORDER BY Id DESC) AS RowNumber FROM TestTable1 ORDER BY Id", 10L, "4,5,6", DisplayName = $"{DatabaseTypes.SqlServer} Window function with ORDER BY")]
    [DataRow(DatabaseTypes.Sqlite, "SELECT * FROM (SELECT * FROM TestTable1 ORDER BY Id LIMIT 7) x WHERE x.TestField <> 'C' ORDER BY x.Id", 7L, "4,5,6", DisplayName = $"{DatabaseTypes.Sqlite} Subquery with ORDER BY")]
    [DataRow(DatabaseTypes.SqlServer, "SELECT * FROM (SELECT TOP 7 * FROM TestTable1 ORDER BY Id) x WHERE x.TestField <> 'C' ORDER BY x.Id", 7L, "4,5,6", DisplayName = $"{DatabaseTypes.SqlServer} Subquery with ORDER BY")]
    [DataRow(DatabaseTypes.Sqlite, "WITH Filtered AS (SELECT * FROM TestTable1 WHERE TestField <> 'C') SELECT * FROM Filtered ORDER BY Id", 8L, "4,5,6", DisplayName = $"{DatabaseTypes.Sqlite} Common table expression")]
    [DataRow(DatabaseTypes.SqlServer, "WITH Filtered AS (SELECT * FROM TestTable1 WHERE TestField <> 'C') SELECT * FROM Filtered ORDER BY Id", 8L, "4,5,6", DisplayName = $"{DatabaseTypes.SqlServer} Common table expression")]
    [DataRow(DatabaseTypes.Sqlite, "WITH NotC (Id, TestField) AS (SELECT Id, TestField FROM TestTable1 WHERE TestField <> 'C'), FromId2 AS (SELECT * FROM NotC WHERE Id > 1) SELECT * FROM FromId2 ORDER BY Id", 7L, "5,6,7", DisplayName = $"{DatabaseTypes.Sqlite} Common table expressions")]
    [DataRow(DatabaseTypes.SqlServer, "WITH NotC (Id, TestField) AS (SELECT Id, TestField FROM TestTable1 WHERE TestField <> 'C'), FromId2 AS (SELECT * FROM NotC WHERE Id > 1) SELECT * FROM FromId2 ORDER BY Id", 7L, "5,6,7", DisplayName = $"{DatabaseTypes.SqlServer} Common table expressions")]
    [DataRow(DatabaseTypes.Sqlite, "SELECT * FROM TestTable1 WHERE TestField <> 'it''s ORDER BY x' ORDER BY Id", 10L, "4,5,6", DisplayName = $"{DatabaseTypes.Sqlite} Keywords in a string")]
    [DataRow(DatabaseTypes.SqlServer, "SELECT * FROM TestTable1 WHERE TestField <> 'it''s ORDER BY x' ORDER BY Id", 10L, "4,5,6", DisplayName = $"{DatabaseTypes.SqlServer} Keywords in a string")]
    [DataRow(DatabaseTypes.Sqlite, "SELECT * /* ORDER BY TestField */ FROM TestTable1 -- ORDER BY TestField\r\nORDER BY Id", 10L, "4,5,6", DisplayName = $"{DatabaseTypes.Sqlite} Keywords in comments")]
    [DataRow(DatabaseTypes.SqlServer, "SELECT * /* ORDER BY TestField */ FROM TestTable1 -- ORDER BY TestField\r\nORDER BY Id", 10L, "4,5,6", DisplayName = $"{DatabaseTypes.SqlServer} Keywords in comments")]
    [DataRow(DatabaseTypes.Sqlite, @"SELECT Id, TestField, 1 AS [ORDER BY], 2 AS ""OFFSET"" FROM TestTable1 ORDER BY Id", 10L, "4,5,6", DisplayName = $"{DatabaseTypes.Sqlite} Keywords as quoted identifiers")]
    [DataRow(DatabaseTypes.SqlServer, @"SELECT Id, TestField, 1 AS [ORDER BY], 2 AS ""OFFSET"" FROM TestTable1 ORDER BY Id", 10L, "4,5,6", DisplayName = $"{DatabaseTypes.SqlServer} Keywords as quoted identifiers")]
    [DataRow(DatabaseTypes.Sqlite, "SELECT * FROM TestTable1 ORDER BY Id; -- all rows", 10L, "4,5,6", DisplayName = $"{DatabaseTypes.Sqlite} Trailing semicolon and comment")]
    [DataRow(DatabaseTypes.SqlServer, "SELECT * FROM TestTable1 ORDER BY Id; -- all rows", 10L, "4,5,6", DisplayName = $"{DatabaseTypes.SqlServer} Trailing semicolon and comment")]
    [DataRow(DatabaseTypes.Sqlite, "SELECT * FROM TestTable1 ORDER BY Id -- all rows", 10L, "4,5,6", DisplayName = $"{DatabaseTypes.Sqlite} Trailing line comment")]
    [DataRow(DatabaseTypes.SqlServer, "SELECT * FROM TestTable1 ORDER BY Id -- all rows", 10L, "4,5,6", DisplayName = $"{DatabaseTypes.SqlServer} Trailing line comment")]
    [DataRow(DatabaseTypes.Sqlite, "SELECT * FROM TestTable1 ORDER BY Id /* all rows */", 10L, "4,5,6", DisplayName = $"{DatabaseTypes.Sqlite} Trailing block comment")]
    [DataRow(DatabaseTypes.SqlServer, "SELECT * FROM TestTable1 ORDER BY Id /* all rows */", 10L, "4,5,6", DisplayName = $"{DatabaseTypes.SqlServer} Trailing block comment")]
    [DataRow(DatabaseTypes.Sqlite, "SELECT * FROM TestTable1 ORDER BY Id /* all rows", 10L, "4,5,6", DisplayName = $"{DatabaseTypes.Sqlite} Trailing unclosed block comment")]
    [DataRow(DatabaseTypes.Sqlite, "SELECT * FROM TestTable1 WHERE Id > 2 -- no ORDER BY", 8L, null, DisplayName = $"{DatabaseTypes.Sqlite} No ORDER BY and a trailing line comment")]
    [DataRow(DatabaseTypes.SqlServer, "SELECT * FROM TestTable1 WHERE Id > 2 -- no ORDER BY", 8L, null, DisplayName = $"{DatabaseTypes.SqlServer} No ORDER BY and a trailing line comment")]
    [DataRow(DatabaseTypes.SqlServer, "SELECT * FROM TestTable1 ORDER BY Id OPTION (RECOMPILE)", 10L, "4,5,6", DisplayName = $"{DatabaseTypes.SqlServer} OPTION clause")]
    [DataRow(DatabaseTypes.Sqlite, ";WITH Filtered AS (SELECT * FROM TestTable1 WHERE TestField <> 'C') SELECT * FROM Filtered ORDER BY Id", 8L, "4,5,6", DisplayName = $"{DatabaseTypes.Sqlite} ;WITH")]
    [DataRow(DatabaseTypes.SqlServer, ";WITH Filtered AS (SELECT * FROM TestTable1 WHERE TestField <> 'C') SELECT * FROM Filtered ORDER BY Id", 8L, "4,5,6", DisplayName = $"{DatabaseTypes.SqlServer} ;WITH")]
    [DataRow(DatabaseTypes.Sqlite, "-- leading comment\r\n; ;WITH Filtered AS (SELECT * FROM TestTable1 WHERE TestField <> 'C') SELECT * FROM Filtered", 8L, null, DisplayName = $"{DatabaseTypes.Sqlite} Comment and semicolons before WITH, no ORDER BY")]
    [DataRow(DatabaseTypes.SqlServer, "-- leading comment\r\n; ;WITH Filtered AS (SELECT * FROM TestTable1 WHERE TestField <> 'C') SELECT * FROM Filtered", 8L, null, DisplayName = $"{DatabaseTypes.SqlServer} Comment and semicolons before WITH, no ORDER BY")]
    [DataRow(DatabaseTypes.Sqlite, ";SELECT * FROM TestTable1 ORDER BY Id;", 10L, "4,5,6", DisplayName = $"{DatabaseTypes.Sqlite} Leading and trailing semicolons")]
    [DataRow(DatabaseTypes.SqlServer, ";SELECT * FROM TestTable1 ORDER BY Id;", 10L, "4,5,6", DisplayName = $"{DatabaseTypes.SqlServer} Leading and trailing semicolons")]
    public async Task ExecutePagedQueryTests_013_Async(string databaseType, string sql, long expectedRecordCount, string? expectedIds)
    {
        // ARRANGE
        using Context context = await DatabaseUtils.OpenDatabaseAsync(this._testContext, databaseType).ConfigureAwait(false);
        await this.CreateTestDataAsync(context).ConfigureAwait(false);

        long page = 1;
        long pageSize = 3;

        // ACT
        PagedQueryResult<TestTable1> result = await context.Database.ExecutePagedQueryAsync<TestTable1>(sql, page, pageSize, this._testContext.CancellationToken).ConfigureAwait(false);

        // ASSERT
        Assert.AreEqual(expectedRecordCount, result.RecordCount, "Invalid record count");
        Assert.AreEqual(page, result.Page, "Invalid page");
        Assert.HasCount((int)pageSize, result.Result, "Invalid list row count");

        if (expectedIds is not null)
        {
            Assert.AreEqual(expectedIds, string.Join(",", result.Result.Select(r => r.Id)), "Invalid Ids");
        }
    }

    /// <summary>
    /// Test ExecutePagedQueryAsync with queries that don't return whole rows (DISTINCT, GROUP BY and UNION) and are counted by wrapping the query.
    /// </summary>
    /// <param name="databaseType">Database type.</param>
    /// <param name="sql">The SQL query ({0} is 'A').</param>
    /// <param name="expectedRecordCount">The expected record count.</param>
    /// <param name="expectedValues">The expected TestField values on page 0 (with a page size of 2).</param>
    /// <returns>A <see cref="Task"/> that represents the asynchronous operation.</returns>
    /// <remarks>The test data is 5 x 'A' (Ids 1-5), 3 x 'B' (Ids 6-8) and 2 x 'C' (Ids 9-10).</remarks>
    [TestMethod(DisplayName = "ExecutePagedQueryAsync with DISTINCT, GROUP BY and UNION queries")]
    [DataRow(DatabaseTypes.Sqlite, "SELECT DISTINCT TestField FROM TestTable1 ORDER BY TestField", 3L, "A,B", DisplayName = $"{DatabaseTypes.Sqlite} DISTINCT")]
    [DataRow(DatabaseTypes.SqlServer, "SELECT DISTINCT TestField FROM TestTable1 ORDER BY TestField", 3L, "A,B", DisplayName = $"{DatabaseTypes.SqlServer} DISTINCT")]
    [DataRow(DatabaseTypes.Sqlite, "SELECT TestField FROM TestTable1 GROUP BY TestField ORDER BY TestField", 3L, "A,B", DisplayName = $"{DatabaseTypes.Sqlite} GROUP BY")]
    [DataRow(DatabaseTypes.SqlServer, "SELECT TestField FROM TestTable1 GROUP BY TestField ORDER BY TestField", 3L, "A,B", DisplayName = $"{DatabaseTypes.SqlServer} GROUP BY")]
    [DataRow(DatabaseTypes.Sqlite, "SELECT TestField FROM TestTable1 WHERE TestField = {0} UNION SELECT TestField FROM TestTable1 WHERE TestField = 'B' ORDER BY TestField", 2L, "A,B", DisplayName = $"{DatabaseTypes.Sqlite} UNION")]
    [DataRow(DatabaseTypes.SqlServer, "SELECT TestField FROM TestTable1 WHERE TestField = {0} UNION SELECT TestField FROM TestTable1 WHERE TestField = 'B' ORDER BY TestField", 2L, "A,B", DisplayName = $"{DatabaseTypes.SqlServer} UNION")]
    public async Task ExecutePagedQueryTests_014_Async(string databaseType, string sql, long expectedRecordCount, string expectedValues)
    {
        // ARRANGE
        using Context context = await DatabaseUtils.OpenDatabaseAsync(this._testContext, databaseType).ConfigureAwait(false);
        await this.CreateTestDataAsync(context).ConfigureAwait(false);

        long page = 0;
        long pageSize = 2;

        // ACT
        PagedQueryResult<TestFieldValue> result = await context.Database.ExecutePagedQueryAsync<TestFieldValue>(sql, page, pageSize, this._testContext.CancellationToken, "A").ConfigureAwait(false);

        // ASSERT
        Assert.AreEqual(expectedRecordCount, result.RecordCount, "Invalid record count");
        Assert.AreEqual(expectedValues, string.Join(",", result.Result.Select(r => r.TestField)), "Invalid values");
    }

    /// <summary>
    /// Test ExecutePagedQueryAsync with SQL that is invalid or can't be paged.
    /// </summary>
    /// <param name="databaseType">Database type.</param>
    /// <param name="sql">The SQL query.</param>
    /// <param name="exceptionType">The type of exception raised.</param>
    /// <returns>A <see cref="Task"/> that represents the asynchronous operation.</returns>
    [TestMethod(DisplayName = "ExecutePagedQueryAsync with SQL that is invalid or can't be paged")]
    [DataRow(DatabaseTypes.Sqlite, "SELECT * FROM TestTable1 ORDER BY Id LIMIT 5", typeof(NotSupportedException), DisplayName = $"{DatabaseTypes.Sqlite} Already paged")]
    [DataRow(DatabaseTypes.SqlServer, "SELECT * FROM TestTable1 ORDER BY Id OFFSET 5 ROWS", typeof(NotSupportedException), DisplayName = $"{DatabaseTypes.SqlServer} Already paged")]
    [DataRow(DatabaseTypes.SqlServer, "SELECT TOP 5 * FROM TestTable1 ORDER BY Id", typeof(NotSupportedException), DisplayName = $"{DatabaseTypes.SqlServer} TOP")]
    [DataRow(DatabaseTypes.SqlServer, "SELECT * FROM TestTable1 FOR JSON PATH", typeof(NotSupportedException), DisplayName = $"{DatabaseTypes.SqlServer} FOR JSON")]
    [DataRow(DatabaseTypes.Sqlite, "DELETE FROM TestTable1", typeof(NotSupportedException), DisplayName = $"{DatabaseTypes.Sqlite} Not a SELECT query")]
    [DataRow(DatabaseTypes.SqlServer, "DELETE FROM TestTable1", typeof(NotSupportedException), DisplayName = $"{DatabaseTypes.SqlServer} Not a SELECT query")]
    [DataRow(DatabaseTypes.Sqlite, "SELECT * FROM TestTable1; SELECT * FROM TestTable1", typeof(ArgumentException), DisplayName = $"{DatabaseTypes.Sqlite} Two statements")]
    [DataRow(DatabaseTypes.SqlServer, "SELECT * FROM TestTable1; SELECT * FROM TestTable1", typeof(ArgumentException), DisplayName = $"{DatabaseTypes.SqlServer} Two statements")]
    [DataRow(DatabaseTypes.SqlServer, "SELECT * FROM TestTable1 /* comment", typeof(ArgumentException), DisplayName = $"{DatabaseTypes.SqlServer} Unclosed comment")]
    [DataRow(DatabaseTypes.Sqlite, "-- comment only", typeof(ArgumentException), DisplayName = $"{DatabaseTypes.Sqlite} No query")]
    [DataRow(DatabaseTypes.SqlServer, "-- comment only", typeof(ArgumentException), DisplayName = $"{DatabaseTypes.SqlServer} No query")]
    public async Task ExecutePagedQueryTests_015_Async(string databaseType, string sql, Type exceptionType)
    {
        // ARRANGE
        using Context context = await DatabaseUtils.OpenDatabaseAsync(this._testContext, databaseType).ConfigureAwait(false);

        long page = 0;
        long pageSize = 5;

        // ACT / ASSERT
        Exception e = await Assert.ThrowsAsync<Exception>(() => context.Database.ExecutePagedQueryAsync<TestTable1>(sql, page, pageSize, this._testContext.CancellationToken), "Unexpected exception").ConfigureAwait(false);
        Assert.IsInstanceOfType(e, exceptionType, "Invalid exception type");
    }

    /// <summary>
    /// Test ExecutePagedQueryAsync with a Null IEnumerable Parameters parameter.
    /// </summary>
    /// <returns>A <see cref="Task"/> that represents the asynchronous operation.</returns>
    [TestMethod(DisplayName = "ExecutePagedQueryAsync with a Null IEnumerable Parameters parameter")]
    public async Task ExecutePagedQueryTests_016_Async()
    {
        // ARRANGE
        using Context context = await DatabaseUtils.OpenDatabaseAsync(this._testContext, DatabaseTypes.Sqlite).ConfigureAwait(false);

        string sql = "SELECT * FROM TestTable1";
        long page = 0;
        long pageSize = 1;
        IEnumerable<object?> parameters = null!;

        // ACT / ASSERT
        await Assert.ThrowsExactlyAsync<ArgumentNullException>(() => context.Database.ExecutePagedQueryAsync<TestTable1>(sql, page, pageSize, this._testContext.CancellationToken, parameters), "Unexpected exception").ConfigureAwait(false);
    }

    /// <summary>
    /// Test ExecutePagedQueryAsync with a query that returns no records.
    /// </summary>
    /// <param name="databaseType">Database type.</param>
    /// <returns>A <see cref="Task"/> that represents the asynchronous operation.</returns>
    /// <remarks>There are no pages, so any page number becomes page 0 and the result is empty.</remarks>
    [TestMethod(DisplayName = "ExecutePagedQueryAsync with a query that returns no records")]
    [DataRow(DatabaseTypes.Sqlite, DisplayName = DatabaseTypes.Sqlite)]
    [DataRow(DatabaseTypes.SqlServer, DisplayName = DatabaseTypes.SqlServer)]
    public async Task ExecutePagedQueryTests_017_Async(string databaseType)
    {
        // ARRANGE
        using Context context = await DatabaseUtils.OpenDatabaseAsync(this._testContext, databaseType).ConfigureAwait(false);

        string sql = "SELECT * FROM TestTable1 ORDER BY Id";
        long page = 2;
        long pageSize = 5;

        // ACT
        PagedQueryResult<TestTable1> result = await context.Database.ExecutePagedQueryAsync<TestTable1>(sql, page, pageSize, this._testContext.CancellationToken).ConfigureAwait(false);

        // ASSERT
        Assert.AreEqual(0, result.Page, "Invalid page");
        Assert.AreEqual(pageSize, result.PageSize, "Invalid page size");
        Assert.AreEqual(0, result.RecordCount, "Invalid record count");
        Assert.AreEqual(0, result.PageCount, "Invalid page count");
        Assert.IsEmpty(result.Result, "Invalid list row count");
    }

    /// <summary>
    /// Test ExecutePagedQueryAsync returns a partial last page.
    /// </summary>
    /// <param name="databaseType">Database type.</param>
    /// <returns>A <see cref="Task"/> that represents the asynchronous operation.</returns>
    [TestMethod(DisplayName = "ExecutePagedQueryAsync returns a partial last page")]
    [DataRow(DatabaseTypes.Sqlite, DisplayName = DatabaseTypes.Sqlite)]
    [DataRow(DatabaseTypes.SqlServer, DisplayName = DatabaseTypes.SqlServer)]
    public async Task ExecutePagedQueryTests_018_Async(string databaseType)
    {
        // ARRANGE
        using Context context = await DatabaseUtils.OpenDatabaseAsync(this._testContext, databaseType).ConfigureAwait(false);

        int recordCount = 22;
        string sql = "SELECT * FROM TestTable1 ORDER BY Id";
        long page = 4;
        long pageSize = 5;

        await DatabaseUtils.CreateTestTableEntriesAsync(context, "TestValue", recordCount, this._testContext.CancellationToken).ConfigureAwait(false);

        // ACT
        PagedQueryResult<TestTable1> result = await context.Database.ExecutePagedQueryAsync<TestTable1>(sql, page, pageSize, this._testContext.CancellationToken).ConfigureAwait(false);

        // ASSERT
        Assert.AreEqual(page, result.Page, "Invalid page");
        Assert.AreEqual(recordCount, result.RecordCount, "Invalid record count");
        Assert.AreEqual(5, result.PageCount, "Invalid page count");
        Assert.AreEqual("21,22", string.Join(",", result.Result.Select(r => r.Id)), "Invalid Ids");
    }

    /// <summary>
    /// Test ExecutePagedQueryAsync returns the rows of the last page when the page number is greater than the total number of pages.
    /// </summary>
    /// <param name="databaseType">Database type.</param>
    /// <returns>A <see cref="Task"/> that represents the asynchronous operation.</returns>
    [TestMethod(DisplayName = "ExecutePagedQueryAsync returns the last page rows when the page number is greater than the total number of pages")]
    [DataRow(DatabaseTypes.Sqlite, DisplayName = DatabaseTypes.Sqlite)]
    [DataRow(DatabaseTypes.SqlServer, DisplayName = DatabaseTypes.SqlServer)]
    public async Task ExecutePagedQueryTests_019_Async(string databaseType)
    {
        // ARRANGE
        using Context context = await DatabaseUtils.OpenDatabaseAsync(this._testContext, databaseType).ConfigureAwait(false);

        int recordCount = 22;
        string sql = "SELECT * FROM TestTable1 ORDER BY Id";
        long page = 999;
        long pageSize = 5;

        await DatabaseUtils.CreateTestTableEntriesAsync(context, "TestValue", recordCount, this._testContext.CancellationToken).ConfigureAwait(false);

        // ACT
        PagedQueryResult<TestTable1> result = await context.Database.ExecutePagedQueryAsync<TestTable1>(sql, page, pageSize, this._testContext.CancellationToken).ConfigureAwait(false);

        // ASSERT
        Assert.AreEqual(4, result.Page, "Invalid page");
        Assert.AreEqual(5, result.PageCount, "Invalid page count");
        Assert.AreEqual("21,22", string.Join(",", result.Result.Select(r => r.Id)), "Invalid Ids");
    }

    /// <summary>
    /// Test ExecutePagedQueryAsync with a WITH clause followed by an INSERT ... SELECT.
    /// It isn't a query, so it is rejected before anything is run, and no rows are inserted.
    /// </summary>
    /// <param name="databaseType">Database type.</param>
    /// <returns>A <see cref="Task"/> that represents the asynchronous operation.</returns>
    [TestMethod(DisplayName = "ExecutePagedQueryAsync with a WITH clause followed by an INSERT ... SELECT")]
    [DataRow(DatabaseTypes.Sqlite, DisplayName = DatabaseTypes.Sqlite)]
    [DataRow(DatabaseTypes.SqlServer, DisplayName = DatabaseTypes.SqlServer)]
    public async Task ExecutePagedQueryTests_020_Async(string databaseType)
    {
        // ARRANGE
        using Context context = await DatabaseUtils.OpenDatabaseAsync(this._testContext, databaseType).ConfigureAwait(false);

        string sql = "WITH NewRows AS (SELECT 'Inserted' AS TestField) INSERT INTO TestTable1 (TestField) SELECT TestField FROM NewRows";
        long page = 0;
        long pageSize = 5;

        // ACT / ASSERT
        await Assert.ThrowsExactlyAsync<NotSupportedException>(() => context.Database.ExecutePagedQueryAsync<TestTable1>(sql, page, pageSize, this._testContext.CancellationToken), "Unexpected exception").ConfigureAwait(false);

        int rowCount = await context.TestTable1.CountAsync(this._testContext.CancellationToken).ConfigureAwait(false);
        Assert.AreEqual(0, rowCount, "Rows were inserted");
    }

    /// <summary>
    /// Test ExecutePagedQueryAsync with UNION, UNION ALL, INTERSECT, EXCEPT and DISTINCT queries without an ORDER BY.
    /// On SQL Server the paged query needs an ORDER BY, and SQL Server rejects the dummy one after a set operation or on a SELECT DISTINCT.
    /// </summary>
    /// <param name="databaseType">Database type.</param>
    /// <param name="sql">The SQL query.</param>
    /// <param name="expectedRecordCount">The expected record count.</param>
    /// <returns>A <see cref="Task"/> that represents the asynchronous operation.</returns>
    /// <remarks>The test data is 5 x 'A' (Ids 1-5), 3 x 'B' (Ids 6-8) and 2 x 'C' (Ids 9-10). Without an ORDER BY the order isn't defined, so only the counts are checked.</remarks>
    [TestMethod(DisplayName = "ExecutePagedQueryAsync with UNION, UNION ALL, INTERSECT, EXCEPT and DISTINCT queries without an ORDER BY")]
    [DataRow(DatabaseTypes.Sqlite, "SELECT TestField FROM TestTable1 WHERE TestField = 'A' UNION SELECT TestField FROM TestTable1 WHERE TestField = 'B'", 2L, DisplayName = $"{DatabaseTypes.Sqlite} UNION")]
    [DataRow(DatabaseTypes.SqlServer, "SELECT TestField FROM TestTable1 WHERE TestField = 'A' UNION SELECT TestField FROM TestTable1 WHERE TestField = 'B'", 2L, DisplayName = $"{DatabaseTypes.SqlServer} UNION")]
    [DataRow(DatabaseTypes.Sqlite, "SELECT TestField FROM TestTable1 WHERE TestField = 'A' UNION ALL SELECT TestField FROM TestTable1 WHERE TestField = 'B'", 8L, DisplayName = $"{DatabaseTypes.Sqlite} UNION ALL")]
    [DataRow(DatabaseTypes.SqlServer, "SELECT TestField FROM TestTable1 WHERE TestField = 'A' UNION ALL SELECT TestField FROM TestTable1 WHERE TestField = 'B'", 8L, DisplayName = $"{DatabaseTypes.SqlServer} UNION ALL")]
    [DataRow(DatabaseTypes.Sqlite, "SELECT TestField FROM TestTable1 INTERSECT SELECT TestField FROM TestTable1 WHERE TestField <> 'C'", 2L, DisplayName = $"{DatabaseTypes.Sqlite} INTERSECT")]
    [DataRow(DatabaseTypes.SqlServer, "SELECT TestField FROM TestTable1 INTERSECT SELECT TestField FROM TestTable1 WHERE TestField <> 'C'", 2L, DisplayName = $"{DatabaseTypes.SqlServer} INTERSECT")]
    [DataRow(DatabaseTypes.Sqlite, "SELECT TestField FROM TestTable1 EXCEPT SELECT TestField FROM TestTable1 WHERE TestField = 'C'", 2L, DisplayName = $"{DatabaseTypes.Sqlite} EXCEPT")]
    [DataRow(DatabaseTypes.SqlServer, "SELECT TestField FROM TestTable1 EXCEPT SELECT TestField FROM TestTable1 WHERE TestField = 'C'", 2L, DisplayName = $"{DatabaseTypes.SqlServer} EXCEPT")]
    [DataRow(DatabaseTypes.Sqlite, "SELECT DISTINCT TestField FROM TestTable1", 3L, DisplayName = $"{DatabaseTypes.Sqlite} DISTINCT")]
    [DataRow(DatabaseTypes.SqlServer, "SELECT DISTINCT TestField FROM TestTable1", 3L, DisplayName = $"{DatabaseTypes.SqlServer} DISTINCT")]
    public async Task ExecutePagedQueryTests_021_Async(string databaseType, string sql, long expectedRecordCount)
    {
        // ARRANGE
        using Context context = await DatabaseUtils.OpenDatabaseAsync(this._testContext, databaseType).ConfigureAwait(false);
        await this.CreateTestDataAsync(context).ConfigureAwait(false);

        long page = 0;
        long pageSize = 2;

        // ACT
        PagedQueryResult<TestFieldValue> result = await context.Database.ExecutePagedQueryAsync<TestFieldValue>(sql, page, pageSize, this._testContext.CancellationToken).ConfigureAwait(false);

        // ASSERT
        Assert.AreEqual(expectedRecordCount, result.RecordCount, "Invalid record count");
        Assert.HasCount((int)pageSize, result.Result, "Invalid list row count");
    }

    /// <summary>
    /// Test ExecutePagedQueryAsync with a page size of <see cref="long.MaxValue"/> (everything on one page) returns all the rows on page 0, with a
    /// page count of 1. Rounding the page count up by adding the page size to the record count overflowed, giving a page count of 0 or -1.
    /// </summary>
    /// <param name="databaseType">Database type.</param>
    /// <param name="page">The page asked for (a page past the end becomes the last page, 0).</param>
    /// <returns>A <see cref="Task"/> that represents the asynchronous operation.</returns>
    /// <remarks>The test data is 5 x 'A' (Ids 1-5), 3 x 'B' (Ids 6-8) and 2 x 'C' (Ids 9-10).</remarks>
    [TestMethod(DisplayName = "ExecutePagedQueryAsync with a page size of long.MaxValue")]
    [DataRow(DatabaseTypes.Sqlite, 0L, DisplayName = $"{DatabaseTypes.Sqlite} Page 0")]
    [DataRow(DatabaseTypes.SqlServer, 0L, DisplayName = $"{DatabaseTypes.SqlServer} Page 0")]
    [DataRow(DatabaseTypes.Sqlite, 5L, DisplayName = $"{DatabaseTypes.Sqlite} Page past the end")]
    [DataRow(DatabaseTypes.SqlServer, 5L, DisplayName = $"{DatabaseTypes.SqlServer} Page past the end")]
    public async Task ExecutePagedQueryTests_022_Async(string databaseType, long page)
    {
        // ARRANGE
        using Context context = await DatabaseUtils.OpenDatabaseAsync(this._testContext, databaseType).ConfigureAwait(false);
        await this.CreateTestDataAsync(context).ConfigureAwait(false);

        string sql = "SELECT * FROM TestTable1 ORDER BY Id";
        long pageSize = long.MaxValue;

        // ACT
        PagedQueryResult<TestTable1> result = await context.Database.ExecutePagedQueryAsync<TestTable1>(sql, page, pageSize, this._testContext.CancellationToken).ConfigureAwait(false);

        // ASSERT
        Assert.AreEqual(0L, result.Page, "Invalid page");
        Assert.AreEqual(pageSize, result.PageSize, "Invalid page size");
        Assert.AreEqual(10L, result.RecordCount, "Invalid record count");
        Assert.AreEqual(1L, result.PageCount, "Invalid page count");
        Assert.AreEqual("1,2,3,4,5,6,7,8,9,10", string.Join(",", result.Result.Select(r => r.Id)), "Invalid Ids");
    }

    #endregion ExecutePagedQueryAsync tests

    #region private methods

    /// <summary>
    /// Create the test data used by the SQL query form tests: 5 x 'A' (Ids 1-5), 3 x 'B' (Ids 6-8) and 2 x 'C' (Ids 9-10).
    /// </summary>
    /// <param name="context">The database context.</param>
    /// <returns>A <see cref="Task"/> that represents the asynchronous operation.</returns>
    private async Task CreateTestDataAsync(Context context)
    {
        await DatabaseUtils.CreateTestTableEntriesAsync(context, "A", 5, this._testContext.CancellationToken).ConfigureAwait(false);
        await DatabaseUtils.CreateTestTableEntriesAsync(context, "B", 3, this._testContext.CancellationToken).ConfigureAwait(false);
        await DatabaseUtils.CreateTestTableEntriesAsync(context, "C", 2, this._testContext.CancellationToken).ConfigureAwait(false);
    }

    #endregion private methods
}