// Copyright ©2021-2025 Mike King.
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
    #region public methods

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
        await Assert.ThrowsExactlyAsync<ArgumentNullException>(() => _ = database.ExecutePagedQueryAsync<TestTable1>(sql, page, pageSize, CancellationToken.None), "Unexpected exception").ConfigureAwait(false);
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
        await Assert.ThrowsExactlyAsync<ArgumentNullException>(() => _ = database.ExecutePagedQueryAsync<TestTable1>(sql, page, pageSize, CancellationToken.None), "Unexpected exception").ConfigureAwait(false);
    }

    /// <summary>
    /// Test ExecutePagedQueryAsync with a Null FormattableString Sql parameter.
    /// </summary>
    /// <returns>A <see cref="Task"/> that represents the asynchronous operation.</returns>
    [TestMethod(DisplayName = "ExecutePagedQueryAsync with a Null FormattableString Sql parameter")]
    public async Task ExecutePagedQueryTests_003_Async()
    {
        // ARRANGE
        using Context context = await DatabaseUtils.CreateDatabaseAsync(DatabaseTypes.Sqlite).ConfigureAwait(false);

        FormattableString sql = null!;
        long page = 0;
        long pageSize = 1;

        // ACT / ASSERT
        await Assert.ThrowsExactlyAsync<ArgumentNullException>(() => _ = context.Database.ExecutePagedQueryAsync<TestTable1>(sql, page, pageSize, CancellationToken.None), "Unexpected exception").ConfigureAwait(false);
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
        using Context context = await DatabaseUtils.CreateDatabaseAsync(DatabaseTypes.Sqlite).ConfigureAwait(false);

        long page = 0;
        long pageSize = 1;

        // ACT / ASSERT
        Exception e = await Assert.ThrowsAsync<Exception>(() => _ = context.Database.ExecutePagedQueryAsync<TestTable1>(sql, page, pageSize, CancellationToken.None), "Unexpected exception").ConfigureAwait(false);
        Assert.IsInstanceOfType(e, exceptionType, "Invalid exception type");
    }

    /// <summary>
    /// Test ExecutePagedQueryAsync with FormattableString Sql and negative long Page parameter.
    /// </summary>
    /// <returns>A <see cref="Task"/> that represents the asynchronous operation.</returns>
    [TestMethod(DisplayName = "ExecutePagedQueryAsync with FormattableString Sql and negative long Page parameters.")]
    public async Task ExecutePagedQueryTests_005_Async()
    {
        // ARRANGE
        using Context context = await DatabaseUtils.CreateDatabaseAsync(DatabaseTypes.Sqlite).ConfigureAwait(false);

        FormattableString sql = $"dummy";
        long page = -1;
        long pageSize = 1;

        // ACT / ASSERT
        await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() => _ = context.Database.ExecutePagedQueryAsync<TestTable1>(sql, page, pageSize, CancellationToken.None), "Unexpected exception").ConfigureAwait(false);
    }

    /// <summary>
    /// Test ExecutePagedQueryAsync with String Sql and negative long Page parameter.
    /// </summary>
    /// <returns>A <see cref="Task"/> that represents the asynchronous operation.</returns>
    [TestMethod(DisplayName = "ExecutePagedQueryAsync with String Sql and negative long Page parameters.")]
    public async Task ExecutePagedQueryTests_006_Async()
    {
        // ARRANGE
        using Context context = await DatabaseUtils.CreateDatabaseAsync(DatabaseTypes.Sqlite).ConfigureAwait(false);

        string sql = "dummy";
        long page = -1;
        long pageSize = 1;

        // ACT / ASSERT
        await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() => _ = context.Database.ExecutePagedQueryAsync<TestTable1>(sql, page, pageSize, CancellationToken.None), "Unexpected exception").ConfigureAwait(false);
    }

    /// <summary>
    /// Test ExecutePagedQueryAsync with FormattableString Sql and zero or negative long PageSize parameter.
    /// </summary>
    /// <param name="pageSize">The page size.</param>
    /// <returns>A <see cref="Task"/> that represents the asynchronous operation.</returns>
    [TestMethod(DisplayName = "ExecutePagedQueryAsync with FormattableString Sql and negative long Page parameters.")]
    [DataRow(0, DisplayName = "0")]
    [DataRow(-1, DisplayName = "-1")]
    public async Task ExecutePagedQueryTests_007_Async(long pageSize)
    {
        // ARRANGE
        using Context context = await DatabaseUtils.CreateDatabaseAsync(DatabaseTypes.Sqlite).ConfigureAwait(false);

        FormattableString sql = $"dummy";
        long page = -1;

        // ACT / ASSERT
        await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() => _ = context.Database.ExecutePagedQueryAsync<TestTable1>(sql, page, pageSize, CancellationToken.None), "Unexpected exception").ConfigureAwait(false);
    }

    /// <summary>
    /// Test ExecutePagedQueryAsync with String Sql and zero or negative long PageSize parameter.
    /// </summary>
    /// <param name="pageSize">The page size.</param>
    /// <returns>A <see cref="Task"/> that represents the asynchronous operation.</returns>
    [TestMethod(DisplayName = "ExecutePagedQueryAsync with String Sql and negative long Page parameters.")]
    [DataRow(0, DisplayName = "0")]
    [DataRow(-1, DisplayName = "-1")]
    public async Task ExecutePagedQueryTests_008_Async(long pageSize)
    {
        // ARRANGE
        using Context context = await DatabaseUtils.CreateDatabaseAsync(DatabaseTypes.Sqlite).ConfigureAwait(false);

        string sql = "dummy";
        long page = -1;

        // ACT / ASSERT
        await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() => _ = context.Database.ExecutePagedQueryAsync<TestTable1>(sql, page, pageSize, CancellationToken.None), "Unexpected exception").ConfigureAwait(false);
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
        using Context context = await DatabaseUtils.CreateDatabaseAsync(databaseType).ConfigureAwait(false);

        int recordCount = 20;
        FormattableString sql = $"SELECT * FROM TestTable1 WHERE ID <= {recordCount}";
        long page = 2;
        long pageSize = 5;

        string value = "TestValue";

        await DatabaseUtils.CreateTestTableEntriesAsync(context, value, recordCount).ConfigureAwait(false);

        // ACT
        PagedQueryResult<TestTable1> result = await context.Database.ExecutePagedQueryAsync<TestTable1>(sql, page, pageSize, CancellationToken.None).ConfigureAwait(false);

        // ASSERT
        Assert.AreEqual(page, result.Page, "Invalid page");
        Assert.AreEqual(pageSize, result.PageSize, "Invalid page size");
        Assert.AreEqual(recordCount, result.RecordCount, "Invalid record count");
        Assert.AreEqual((recordCount + pageSize - 1) / pageSize, result.PageCount, "Invalid page count");
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
        using Context context = await DatabaseUtils.CreateDatabaseAsync(databaseType).ConfigureAwait(false);

        int recordCount = 20;
        string sql = "SELECT * FROM TestTable1 WHERE ID <= {0}";
        long page = 2;
        long pageSize = 5;

        string value = "TestValue";

        await DatabaseUtils.CreateTestTableEntriesAsync(context, value, recordCount).ConfigureAwait(false);

        // ACT
        PagedQueryResult<TestTable1> result = await context.Database.ExecutePagedQueryAsync<TestTable1>(sql, page, pageSize, CancellationToken.None, recordCount).ConfigureAwait(false);

        // ASSERT
        Assert.AreEqual(page, result.Page, "Invalid page");
        Assert.AreEqual(pageSize, result.PageSize, "Invalid page size");
        Assert.AreEqual(recordCount, result.RecordCount, "Invalid record count");
        Assert.AreEqual((recordCount + pageSize - 1) / pageSize, result.PageCount, "Invalid page count");
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
        using Context context = await DatabaseUtils.CreateDatabaseAsync(databaseType).ConfigureAwait(false);

        int recordCount = 20;
        FormattableString sql = $"SELECT * FROM TestTable1 WHERE ID <= {recordCount} ORDER BY ID";
        long page = 2;
        long pageSize = 5;

        string value = "TestValue";

        await DatabaseUtils.CreateTestTableEntriesAsync(context, value, recordCount).ConfigureAwait(false);

        // ACT
        PagedQueryResult<TestTable1> pageResult = await context.Database.ExecutePagedQueryAsync<TestTable1>(sql, page, pageSize, CancellationToken.None).ConfigureAwait(false);

        // ASSERT
        Assert.AreEqual(page, pageResult.Page, "Invalid page");
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
        using Context context = await DatabaseUtils.CreateDatabaseAsync(databaseType).ConfigureAwait(false);

        int recordCount = 20;
        string sql = $"SELECT * FROM TestTable1 WHERE ID <= {recordCount}";
        long page = 999;
        long pageSize = 5;

        string value = "TestValue";

        await DatabaseUtils.CreateTestTableEntriesAsync(context, value, recordCount).ConfigureAwait(false);

        long expectedPage = ((recordCount + pageSize - 1) / pageSize) - 1;

        // ACT
        PagedQueryResult<TestTable1> pageResult = await context.Database.ExecutePagedQueryAsync<TestTable1>(sql, page, pageSize, CancellationToken.None).ConfigureAwait(false);

        // ASSERT
        Assert.AreEqual(expectedPage, pageResult.Page, "Invalid page");
    }

    #endregion public methods
}