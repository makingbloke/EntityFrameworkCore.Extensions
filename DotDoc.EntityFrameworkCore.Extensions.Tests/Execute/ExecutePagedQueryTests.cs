// Copyright ©2021-2025 Mike King.
// This file is licensed to you under the MIT license.
// See the License.txt file in the solution root for more information.

using DotDoc.EntityFrameworkCore.Extensions.DatabaseType;
using DotDoc.EntityFrameworkCore.Extensions.Execute;
using DotDoc.EntityFrameworkCore.Extensions.Tests.Data;
using DotDoc.EntityFrameworkCore.Extensions.Tests.Utilities;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.VisualStudio.TestTools.UnitTesting;
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
    /// Test ExecutePagedQueryAsync with FormattableString parameter returning a PageResultTable Guard Clauses.
    /// </summary>
    /// <param name="databaseFacade">The <see cref="DatabaseFacade"/> for the context.</param>
    /// <param name="sql">The <see cref="FormattableString"/> representing a SQL query with parameters.</param>
    /// <param name="page">Page number to return (starting at 0).</param>
    /// <param name="pageSize">Number of records per page.</param>
    /// <param name="exceptionType">The type of exception raised.</param>
    /// <param name="paramName">Name of parameter being checked.</param>
    /// <returns>A <see cref="Task"/> that represents the asynchronous operation.</returns>
    [TestMethod("ExecutePagedQueryAsync with FormattableString parameter returning a PageResultTable Guard Clauses")]
    [DynamicData(nameof(Get_ExecutePagedQuery_FormattableString_GuardClause_TestData), DynamicDataSourceType.Method, DynamicDataDisplayName = nameof(TestUtils.CreateDynamicDisplayName), DynamicDataDisplayNameDeclaringType = typeof(TestUtils))]
    public async Task Test_ExecutePagedQuery_FormattableString_PageResultTable_GuardClausesAsync(DatabaseFacade? databaseFacade, FormattableString? sql, long page, long pageSize, Type exceptionType, string paramName)
    {
        // ARRANGE

        // ACT / ASSERT
        Exception e = await Assert.ThrowsAsync<Exception>(() => databaseFacade!.ExecutePagedQueryAsync(sql!, page, pageSize), "Unexpected exception").ConfigureAwait(false);
        Assert.AreEqual(exceptionType, e.GetType(), "Invalid exception type");
        Assert.AreEqual(paramName, ((ArgumentException)e).ParamName, "Invalid parameter name");
    }

    /// <summary>
    /// Test ExecutePagedQueryAsync with FormattableString parameter returning an PageResultEntity Guard Clauses.
    /// </summary>
    /// <param name="databaseFacade">The <see cref="DatabaseFacade"/> for the context.</param>
    /// <param name="sql">The <see cref="FormattableString"/> representing a SQL query with parameters.</param>
    /// <param name="page">Page number to return (starting at 0).</param>
    /// <param name="pageSize">Number of records per page.</param>
    /// <param name="exceptionType">The type of exception raised.</param>
    /// <param name="paramName">Name of parameter being checked.</param>
    /// <returns>A <see cref="Task"/> that represents the asynchronous operation.</returns>
    [TestMethod("ExecutePagedQueryAsync with FormattableString parameter returning an PageResultEntity Guard Clauses")]
    [DynamicData(nameof(Get_ExecutePagedQuery_FormattableString_GuardClause_TestData), DynamicDataSourceType.Method, DynamicDataDisplayName = nameof(TestUtils.CreateDynamicDisplayName), DynamicDataDisplayNameDeclaringType = typeof(TestUtils))]
    public async Task Test_ExecutePagedQuery_FormattableString_PageResultEntity_GuardClausesAsync(DatabaseFacade? databaseFacade, FormattableString? sql, long page, long pageSize, Type exceptionType, string paramName)
    {
        // ARRANGE

        // ACT / ASSERT
        Exception e = await Assert.ThrowsAsync<Exception>(() => databaseFacade!.ExecutePagedQueryAsync<TestTable1>(sql!, page, pageSize), "Unexpected exception").ConfigureAwait(false);
        Assert.AreEqual(exceptionType, e.GetType(), "Invalid exception type");
        Assert.AreEqual(paramName, ((ArgumentException)e).ParamName, "Invalid parameter name");
    }

    /// <summary>
    /// Test ExecutePagedQueryAsync with string parameter returning a PageResultTable Guard Clauses.
    /// </summary>
    /// <param name="databaseFacade">The <see cref="DatabaseFacade"/> for the context.</param>
    /// <param name="sql">The <see cref="string"/> representing a SQL query with parameters.</param>
    /// <param name="page">Page number to return (starting at 0).</param>
    /// <param name="pageSize">Number of records per page.</param>
    /// <param name="exceptionType">The type of exception raised.</param>
    /// <param name="paramName">Name of parameter being checked.</param>
    /// <returns>A <see cref="Task"/> that represents the asynchronous operation.</returns>
    [TestMethod("ExecutePagedQueryAsync with string parameter returning a PageResultTable Guard Clauses")]
    [DynamicData(nameof(Get_ExecutePagedQuery_String_GuardClause_TestData), DynamicDataSourceType.Method, DynamicDataDisplayName = nameof(TestUtils.CreateDynamicDisplayName), DynamicDataDisplayNameDeclaringType = typeof(TestUtils))]
    public async Task Test_ExecutePagedQuery_String_PageResultTable_GuardClausesAsync(DatabaseFacade? databaseFacade, string? sql, long page, long pageSize, Type exceptionType, string paramName)
    {
        // ARRANGE

        // ACT / ASSERT
        Exception e = await Assert.ThrowsAsync<Exception>(() => databaseFacade!.ExecutePagedQueryAsync(sql!, page, pageSize), "Unexpected exception").ConfigureAwait(false);
        Assert.AreEqual(exceptionType, e.GetType(), "Invalid exception type");
        Assert.AreEqual(paramName, ((ArgumentException)e).ParamName, "Invalid parameter name");
    }

    /// <summary>
    /// Test ExecutePagedQueryAsync with string parameter returning an PageResultEntity Guard Clauses.
    /// </summary>
    /// <param name="databaseFacade">The <see cref="DatabaseFacade"/> for the context.</param>
    /// <param name="sql">The <see cref="string"/> representing a SQL query with parameters.</param>
    /// <param name="page">Page number to return (starting at 0).</param>
    /// <param name="pageSize">Number of records per page.</param>
    /// <param name="exceptionType">The type of exception raised.</param>
    /// <param name="paramName">Name of parameter being checked.</param>
    /// <returns>A <see cref="Task"/> that represents the asynchronous operation.</returns>
    [TestMethod("ExecutePagedQueryAsync with string parameter returning an PageResultEntity Guard Clauses")]
    [DynamicData(nameof(Get_ExecutePagedQuery_String_GuardClause_TestData), DynamicDataSourceType.Method, DynamicDataDisplayName = nameof(TestUtils.CreateDynamicDisplayName), DynamicDataDisplayNameDeclaringType = typeof(TestUtils))]
    public async Task Test_ExecutePagedQuery_String_PageResultEntity_GuardClausesAsync(DatabaseFacade? databaseFacade, string? sql, long page, long pageSize, Type exceptionType, string paramName)
    {
        // ARRANGE

        // ACT / ASSERT
        Exception e = await Assert.ThrowsAsync<Exception>(() => databaseFacade!.ExecutePagedQueryAsync<TestTable1>(sql!, page, pageSize), "Unexpected exception").ConfigureAwait(false);
        Assert.AreEqual(exceptionType, e.GetType(), "Invalid exception type");
        Assert.AreEqual(paramName, ((ArgumentException)e).ParamName, "Invalid parameter name");
    }

    /// <summary>
    /// Test ExecutePagedQueryAsync with FormattableString parameter returning a DataTable.
    /// </summary>
    /// <param name="databaseType">Database type.</param>
    /// <returns>A <see cref="Task"/> that represents the asynchronous operation.</returns>
    [TestMethod("ExecutePagedQueryAsync with FormattableString parameter returning a DataTable")]
    [DataRow(DatabaseTypes.Sqlite, DisplayName = DatabaseTypes.Sqlite)]
    [DataRow(DatabaseTypes.SqlServer, DisplayName = DatabaseTypes.SqlServer)]
    public async Task Test_ExecutePagedQuery_FormattableString_DataTableAsync(string databaseType)
    {
        // ARRANGE
        using Context context = await DatabaseUtils.CreateDatabaseAsync(databaseType).ConfigureAwait(false);

        const int recordCount = 20;
        const long page = 2;
        const long pageSize = 5;
        string value = TestUtils.GetMethodName();
        await DatabaseUtils.CreateTestTableEntriesAsync(context, value, recordCount).ConfigureAwait(false);
        FormattableString sql = $"SELECT * FROM TestTable1 WHERE ID <= {recordCount}";

        // ACT
        PageResultTable pageResult = await context.Database.ExecutePagedQueryAsync(sql, page, pageSize).ConfigureAwait(false);

        // ASSERT
        Assert.AreEqual(page, pageResult.Page, "Invalid page");
        Assert.AreEqual(pageSize, pageResult.PageSize, "Invalid page size");
        Assert.AreEqual(recordCount, pageResult.RecordCount, "Invalid record count");
        Assert.AreEqual((recordCount + pageSize - 1) / pageSize, pageResult.PageCount, "Invalid page count");
        Assert.AreEqual(pageSize, pageResult.Result.Rows.Count, "Invalid data table row count");

        for (int i = 0; i < pageSize; i++)
        {
            Assert.AreEqual((page * pageSize) + i + 1, pageResult.Result.Rows[i]["ID"], $"invalid Id[{i}]");
        }
    }

    /// <summary>
    /// Test ExecutePagedQueryAsync with FormattableString parameter returning a list of entities.
    /// </summary>
    /// <param name="databaseType">Database type.</param>
    /// <returns>A <see cref="Task"/> that represents the asynchronous operation.</returns>
    [TestMethod("ExecutePagedQueryAsync with FormattableString parameter returning a list of entities")]
    [DataRow(DatabaseTypes.Sqlite, DisplayName = DatabaseTypes.Sqlite)]
    [DataRow(DatabaseTypes.SqlServer, DisplayName = DatabaseTypes.SqlServer)]
    public async Task Test_ExecutePagedQuery_FormattableString_EntityAsync(string databaseType)
    {
        // ARRANGE
        using Context context = await DatabaseUtils.CreateDatabaseAsync(databaseType).ConfigureAwait(false);

        const int recordCount = 20;
        const long page = 2;
        const long pageSize = 5;
        string value = TestUtils.GetMethodName();
        await DatabaseUtils.CreateTestTableEntriesAsync(context, value, recordCount).ConfigureAwait(false);
        FormattableString sql = $"SELECT * FROM TestTable1 WHERE ID <= {recordCount}";

        // ACT
        PageResultEntity<TestTable1> pageResult = await context.Database.ExecutePagedQueryAsync<TestTable1>(sql, page, pageSize).ConfigureAwait(false);

        // ASSERT
        Assert.AreEqual(page, pageResult.Page, "Invalid page");
        Assert.AreEqual(pageSize, pageResult.PageSize, "Invalid page size");
        Assert.AreEqual(recordCount, pageResult.RecordCount, "Invalid record count");
        Assert.AreEqual((recordCount + pageSize - 1) / pageSize, pageResult.PageCount, "Invalid page count");
        Assert.AreEqual(pageSize, pageResult.Result.Count, "Invalid list row count");

        for (int i = 0; i < pageSize; i++)
        {
            Assert.AreEqual((page * pageSize) + i + 1, pageResult.Result[i].Id, $"invalid Id[{i}]");
        }
    }

    /// <summary>
    /// Test ExecutePagedQueryAsync with params parameters returning a DataTable.
    /// </summary>
    /// <param name="databaseType">Database type.</param>
    /// <returns>A <see cref="Task"/> that represents the asynchronous operation.</returns>
    [TestMethod("ExecutePagedQueryAsync with params parameters returning a DataTable")]
    [DataRow(DatabaseTypes.Sqlite, DisplayName = DatabaseTypes.Sqlite)]
    [DataRow(DatabaseTypes.SqlServer, DisplayName = DatabaseTypes.SqlServer)]
    public async Task Test_ExecutePagedQuery_Params_DataTableAsync(string databaseType)
    {
        // ARRANGE
        using Context context = await DatabaseUtils.CreateDatabaseAsync(databaseType).ConfigureAwait(false);

        const int recordCount = 20;
        const long page = 2;
        const long pageSize = 5;
        string value = TestUtils.GetMethodName();
        await DatabaseUtils.CreateTestTableEntriesAsync(context, value, recordCount).ConfigureAwait(false);
        string sql = "SELECT * FROM TestTable1 WHERE ID <= {0}";

        // ACT
        PageResultTable pageResult = await context.Database.ExecutePagedQueryAsync(sql, page, pageSize, parameters: recordCount).ConfigureAwait(false);

        // ASSERT
        Assert.AreEqual(page, pageResult.Page, "Invalid page");
        Assert.AreEqual(pageSize, pageResult.PageSize, "Invalid page size");
        Assert.AreEqual(recordCount, pageResult.RecordCount, "Invalid record count");
        Assert.AreEqual((recordCount + pageSize - 1) / pageSize, pageResult.PageCount, "Invalid page count");
        Assert.AreEqual(pageSize, pageResult.Result.Rows.Count, "Invalid data table row count");

        for (int i = 0; i < pageSize; i++)
        {
            Assert.AreEqual((page * pageSize) + i + 1, pageResult.Result.Rows[i]["ID"], $"invalid Id[{i}]");
        }
    }

    /// <summary>
    /// Test ExecutePagedQueryAsync with params parameters returning a list of entities.
    /// </summary>
    /// <param name="databaseType">Database type.</param>
    /// <returns>A <see cref="Task"/> that represents the asynchronous operation.</returns>
    [TestMethod("ExecutePagedQueryAsync with params parameters returning a list of entities")]
    [DataRow(DatabaseTypes.Sqlite, DisplayName = DatabaseTypes.Sqlite)]
    [DataRow(DatabaseTypes.SqlServer, DisplayName = DatabaseTypes.SqlServer)]
    public async Task Test_ExecutePagedQuery_Params_EntityAsync(string databaseType)
    {
        // ARRANGE
        using Context context = await DatabaseUtils.CreateDatabaseAsync(databaseType).ConfigureAwait(false);

        const int recordCount = 20;
        const long page = 2;
        const long pageSize = 5;
        string value = TestUtils.GetMethodName();
        await DatabaseUtils.CreateTestTableEntriesAsync(context, value, recordCount).ConfigureAwait(false);
        string sql = "SELECT * FROM TestTable1 WHERE ID <= {0}";

        // ACT
        PageResultEntity<TestTable1> pageResult = await context.Database.ExecutePagedQueryAsync<TestTable1>(sql, page, pageSize, parameters: recordCount).ConfigureAwait(false);

        // ASSERT
        Assert.AreEqual(page, pageResult.Page, "Invalid page");
        Assert.AreEqual(pageSize, pageResult.PageSize, "Invalid page size");
        Assert.AreEqual(recordCount, pageResult.RecordCount, "Invalid record count");
        Assert.AreEqual((recordCount + pageSize - 1) / pageSize, pageResult.PageCount, "Invalid page count");
        Assert.AreEqual(pageSize, pageResult.Result.Count, "Invalid list row count");

        for (int i = 0; i < pageSize; i++)
        {
            Assert.AreEqual((page * pageSize) + i + 1, pageResult.Result[i].Id, $"invalid Id[{i}]");
        }
    }

    /// <summary>
    /// Test splitting SQL queries and handles Order By clauses returning a DataTable.
    /// </summary>
    /// <param name="databaseType">Database type.</param>
    /// <returns>A <see cref="Task"/> that represents the asynchronous operation.</returns>
    [TestMethod("ExecutePagedQueryAsync order by DataTable")]
    [DataRow(DatabaseTypes.Sqlite, DisplayName = DatabaseTypes.Sqlite)]
    [DataRow(DatabaseTypes.SqlServer, DisplayName = DatabaseTypes.SqlServer)]
    public async Task Test_ExecutePagedQuery_OrderBy_DataTableAsync(string databaseType)
    {
        // ARRANGE
        string value = TestUtils.GetMethodName();
        const int recordCount = 20;
        const long page = 2;
        const long pageSize = 5;

        using Context context = await DatabaseUtils.CreateDatabaseAsync(databaseType).ConfigureAwait(false);
        await DatabaseUtils.CreateTestTableEntriesAsync(context, value, recordCount).ConfigureAwait(false);

        // ACT
        PageResultTable pageResult = await context.Database.ExecutePagedQueryAsync($"SELECT * FROM TestTable1 WHERE ID <= {recordCount} ORDER BY ID", page, pageSize).ConfigureAwait(false);

        // ASSERT
        Assert.AreEqual(page, pageResult.Page, "Invalid page");
    }

    /// <summary>
    /// Test splitting SQL queries and handles Order By clauses returning a list of entities.
    /// </summary>
    /// <param name="databaseType">Database type.</param>
    /// <returns>A <see cref="Task"/> that represents the asynchronous operation.</returns>
    [TestMethod("ExecutePagedQueryAsync order by Entity")]
    [DataRow(DatabaseTypes.Sqlite, DisplayName = DatabaseTypes.Sqlite)]
    [DataRow(DatabaseTypes.SqlServer, DisplayName = DatabaseTypes.SqlServer)]
    public async Task Test_ExecutePagedQuery_OrderBy_EntityAsync(string databaseType)
    {
        // ARRANGE
        string value = TestUtils.GetMethodName();
        const int recordCount = 20;
        const long page = 2;
        const long pageSize = 5;

        using Context context = await DatabaseUtils.CreateDatabaseAsync(databaseType).ConfigureAwait(false);
        await DatabaseUtils.CreateTestTableEntriesAsync(context, value, recordCount).ConfigureAwait(false);

        // ACT
        PageResultEntity<TestTable1> pageResult = await context.Database.ExecutePagedQueryAsync<TestTable1>($"SELECT * FROM TestTable1 WHERE ID <= {recordCount} ORDER BY ID", page, pageSize).ConfigureAwait(false);

        // ASSERT
        Assert.AreEqual(page, pageResult.Page, "Invalid page");
    }

    /// <summary>
    /// Test if the requested page number is greater than the total number of pages returning a DataTable.
    /// The page number should be reduced to the last one.
    /// </summary>
    /// <param name="databaseType">Database type.</param>
    /// <returns>A <see cref="Task"/> that represents the asynchronous operation.</returns>
    [TestMethod("ExecutePagedQueryAsync page number overflow DataTable")]
    [DataRow(DatabaseTypes.Sqlite, DisplayName = DatabaseTypes.Sqlite)]
    [DataRow(DatabaseTypes.SqlServer, DisplayName = DatabaseTypes.SqlServer)]
    public async Task Test_ExecutePagedQuery_PageNumberOverflow_DataTableAsync(string databaseType)
    {
        // ARRANGE
        string value = TestUtils.GetMethodName();
        const int recordCount = 20;
        const long page = 999;
        const long pageSize = 5;

        using Context context = await DatabaseUtils.CreateDatabaseAsync(databaseType).ConfigureAwait(false);
        await DatabaseUtils.CreateTestTableEntriesAsync(context, value, recordCount).ConfigureAwait(false);

        // ACT
        PageResultTable pageResult = await context.Database.ExecutePagedQueryAsync($"SELECT * FROM TestTable1 WHERE ID <= {recordCount}", page, pageSize).ConfigureAwait(false);

        // ASSERT
        Assert.AreEqual(((recordCount + pageSize - 1) / pageSize) - 1, pageResult.Page, "Invalid page");
    }

    /// <summary>
    /// Test if the requested page number is greater than the total number of pages returning a list of entities.
    /// The page number should be reduced to the last one.
    /// </summary>
    /// <param name="databaseType">Database type.</param>
    /// <returns>A <see cref="Task"/> that represents the asynchronous operation.</returns>
    [TestMethod("ExecutePagedQueryAsync page number overflow Entity")]
    [DataRow(DatabaseTypes.Sqlite, DisplayName = DatabaseTypes.Sqlite)]
    [DataRow(DatabaseTypes.SqlServer, DisplayName = DatabaseTypes.SqlServer)]
    public async Task Test_ExecutePagedQuery_PageNumberOverflow_EntityAsync(string databaseType)
    {
        // ARRANGE
        string value = TestUtils.GetMethodName();
        const int recordCount = 20;
        const long page = 999;
        const long pageSize = 5;

        using Context context = await DatabaseUtils.CreateDatabaseAsync(databaseType).ConfigureAwait(false);
        await DatabaseUtils.CreateTestTableEntriesAsync(context, value, recordCount).ConfigureAwait(false);

        // ACT
        PageResultEntity<TestTable1> pageResult = await context.Database.ExecutePagedQueryAsync<TestTable1>($"SELECT * FROM TestTable1 WHERE ID <= {recordCount}", page, pageSize).ConfigureAwait(false);

        // ASSERT
        Assert.AreEqual(((recordCount + pageSize - 1) / pageSize) - 1, pageResult.Page, "Invalid page");
    }

    #endregion public methods

    #region private methods

    /// <summary>
    /// Get test data for the ExecutePagedQuery method with FormattableString parameter.
    /// </summary>
    /// <returns><see cref="IEnumerable{T}"/>.</returns>
    private static IEnumerable<object?[]> Get_ExecutePagedQuery_FormattableString_GuardClause_TestData()
    {
        using Context context = DatabaseUtils.CreateDatabaseAsync(DatabaseTypes.Sqlite).Result;

        // 0. DatabaseFacade databaseFacade
        // 1. FormattableString sql
        // 2. long page
        // 3. long pageSize
        // 4. Type exceptionType
        // 5. string paramName
        yield return [
            null,
            (FormattableString)$"dummy",
            0,
            1,
            typeof(ArgumentNullException),
            "databaseFacade"];

        yield return [
            context.Database,
            null,
            0,
            1,
            typeof(ArgumentNullException),
            "sql"];

        yield return [
            context.Database,
            (FormattableString)$"dummy",
            -1,
            1,
            typeof(ArgumentOutOfRangeException),
            "page"];

        yield return [
            context.Database,
            (FormattableString)$"dummy",
            0,
            0,
            typeof(ArgumentOutOfRangeException),
            "pageSize"];

        yield return [
            context.Database,
            (FormattableString)$"dummy",
            0,
            -1,
            typeof(ArgumentOutOfRangeException),
            "pageSize"];
    }

    /// <summary>
    /// Get test data for the ExecutePagedQuery method with String parameter.
    /// </summary>
    /// <returns><see cref="IEnumerable{T}"/>.</returns>
    private static IEnumerable<object?[]> Get_ExecutePagedQuery_String_GuardClause_TestData()
    {
        using Context context = DatabaseUtils.CreateDatabaseAsync(DatabaseTypes.Sqlite).Result;

        // 0. DatabaseFacade databaseFacade
        // 1. FormattableString sql
        // 2. long page
        // 3. long pageSize
        // 4. Type exceptionType
        // 5. string paramName
        yield return [
            null,
            "dummy",
            0,
            1,
            typeof(ArgumentNullException),
            "databaseFacade"];

        yield return [
            context.Database,
            null,
            0,
            1,
            typeof(ArgumentNullException),
            "sql"];

        yield return [
            context.Database,
            string.Empty,
            0,
            1,
            typeof(ArgumentException),
            "sql"];

        yield return [
            context.Database,
            "dummy",
            -1,
            1,
            typeof(ArgumentOutOfRangeException),
            "page"];

        yield return [
            context.Database,
            "dummy",
            0,
            0,
            typeof(ArgumentOutOfRangeException),
            "pageSize"];

        yield return [
            context.Database,
            "dummy",
            0,
            -1,
            typeof(ArgumentOutOfRangeException),
            "pageSize"];
    }

    #endregion private methods
}