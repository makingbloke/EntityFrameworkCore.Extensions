// Copyright ©2021-2025 Mike King.
// This file is licensed to you under the MIT license.
// See the License.txt file in the solution root for more information.

using DotDoc.EntityFrameworkCore.Extensions.DatabaseType;
using DotDoc.EntityFrameworkCore.Extensions.Execute;
using DotDoc.EntityFrameworkCore.Extensions.Tests.Data;
using DotDoc.EntityFrameworkCore.Extensions.Tests.Utilities;
using Microsoft.EntityFrameworkCore.Infrastructure;
using System.Data;

namespace DotDoc.EntityFrameworkCore.Extensions.Tests.Execute;

/// <summary>
/// Tests for ExecuteQuery extensions.
/// </summary>
[TestClass]
public class ExecuteQueryTests
{
    #region public methods

    /// <summary>
    /// Test ExecuteQueryAsync with FormattableString parameter returning a DataTable Guard Clauses.
    /// </summary>
    /// <param name="databaseFacade">The <see cref="DatabaseFacade"/> for the context.</param>
    /// <param name="sql">The <see cref="FormattableString"/> representing a SQL query with parameters.</param>
    /// <param name="exceptionType">The type of exception raised.</param>
    /// <param name="paramName">Name of parameter being checked.</param>
    /// <returns>A <see cref="Task"/> that represents the asynchronous operation.</returns>
    [TestMethod(DisplayName = "ExecuteQueryAsync with FormattableString parameter returning a DataTable Guard Clauses")]
    [DynamicData(nameof(Get_ExecuteQueryAsync_FormattableString_GuardClause_TestData), DynamicDataSourceType.Method, DynamicDataDisplayName = nameof(TestUtils.CreateDynamicDisplayName), DynamicDataDisplayNameDeclaringType = typeof(TestUtils))]
    public async Task Test_ExecuteQueryAsync_FormattableStringAsync_DataTable_GuardClauses_Async(DatabaseFacade? databaseFacade, FormattableString? sql, Type exceptionType, string paramName)
    {
        // ARRANGE

        // ACT / ASSERT
        Exception e = await Assert.ThrowsAsync<Exception>(() => databaseFacade!.ExecuteQueryAsync(sql!, CancellationToken.None), "Unexpected exception").ConfigureAwait(false);
        Assert.AreEqual(exceptionType, e.GetType(), "Invalid exception type");
        Assert.AreEqual(paramName, ((ArgumentException)e).ParamName, "Invalid parameter name");
    }

    /// <summary>
    /// Test ExecuteQueryAsync with FormattableString parameter returning an Entity Guard Clauses.
    /// </summary>
    /// <param name="databaseFacade">The <see cref="DatabaseFacade"/> for the context.</param>
    /// <param name="sql">The <see cref="FormattableString"/> representing a SQL query with parameters.</param>
    /// <param name="exceptionType">The type of exception raised.</param>
    /// <param name="paramName">Name of parameter being checked.</param>
    /// <returns>A <see cref="Task"/> that represents the asynchronous operation.</returns>
    [TestMethod(DisplayName = "ExecuteQueryAsync with FormattableString parameter returning an Entity Guard Clauses")]
    [DynamicData(nameof(Get_ExecuteQueryAsync_FormattableString_GuardClause_TestData), DynamicDataSourceType.Method, DynamicDataDisplayName = nameof(TestUtils.CreateDynamicDisplayName), DynamicDataDisplayNameDeclaringType = typeof(TestUtils))]
    public async Task Test_ExecuteQueryAsync_FormattableString_Entity_GuardClauses_Async(DatabaseFacade? databaseFacade, FormattableString? sql, Type exceptionType, string paramName)
    {
        // ARRANGE

        // ACT / ASSERT
        Exception e = await Assert.ThrowsAsync<Exception>(() => databaseFacade!.ExecuteQueryAsync<TestTable1>(sql!, CancellationToken.None), "Unexpected exception").ConfigureAwait(false);
        Assert.AreEqual(exceptionType, e.GetType(), "Invalid exception type");
        Assert.AreEqual(paramName, ((ArgumentException)e).ParamName, "Invalid parameter name");
    }

    /// <summary>
    /// Test ExecuteQueryAsync with string parameter returning a DataTable Guard Clauses.
    /// </summary>
    /// <param name="databaseFacade">The <see cref="DatabaseFacade"/> for the context.</param>
    /// <param name="sql">The <see cref="string"/> representing a SQL query with parameters.</param>
    /// <param name="exceptionType">The type of exception raised.</param>
    /// <param name="paramName">Name of parameter being checked.</param>
    /// <returns>A <see cref="Task"/> that represents the asynchronous operation.</returns>
    [TestMethod(DisplayName = "ExecuteQueryAsync with string parameter returning a DataTable Guard Clauses")]
    [DynamicData(nameof(Get_ExecuteQueryAsync_String_GuardClause_TestData), DynamicDataSourceType.Method, DynamicDataDisplayName = nameof(TestUtils.CreateDynamicDisplayName), DynamicDataDisplayNameDeclaringType = typeof(TestUtils))]
    public async Task Test_ExecuteQueryAsync_String_DataTable_GuardClauses_Async(DatabaseFacade? databaseFacade, string? sql, Type exceptionType, string paramName)
    {
        // ARRANGE

        // ACT / ASSERT
        Exception e = await Assert.ThrowsAsync<Exception>(() => databaseFacade!.ExecuteQueryAsync(sql!, CancellationToken.None), "Unexpected exception").ConfigureAwait(false);
        Assert.AreEqual(exceptionType, e.GetType(), "Invalid exception type");
        Assert.AreEqual(paramName, ((ArgumentException)e).ParamName, "Invalid parameter name");
    }

    /// <summary>
    /// Test ExecuteQueryAsync with string parameter returning an Entity Guard Clauses.
    /// </summary>
    /// <param name="databaseFacade">The <see cref="DatabaseFacade"/> for the context.</param>
    /// <param name="sql">The <see cref="string"/> representing a SQL query with parameters.</param>
    /// <param name="exceptionType">The type of exception raised.</param>
    /// <param name="paramName">Name of parameter being checked.</param>
    /// <returns>A <see cref="Task"/> that represents the asynchronous operation.</returns>
    [TestMethod(DisplayName = "ExecuteQueryAsync with string parameter returning an Entity Guard Clauses")]
    [DynamicData(nameof(Get_ExecuteQueryAsync_String_GuardClause_TestData), DynamicDataSourceType.Method, DynamicDataDisplayName = nameof(TestUtils.CreateDynamicDisplayName), DynamicDataDisplayNameDeclaringType = typeof(TestUtils))]
    public async Task Test_ExecuteQueryAsync_String_Entity_GuardClauses_Async(DatabaseFacade? databaseFacade, string? sql, Type exceptionType, string paramName)
    {
        // ARRANGE

        // ACT / ASSERT
        Exception e = await Assert.ThrowsAsync<Exception>(() => databaseFacade!.ExecuteQueryAsync<TestTable1>(sql!, CancellationToken.None), "Unexpected exception").ConfigureAwait(false);
        Assert.AreEqual(exceptionType, e.GetType(), "Invalid exception type");
        Assert.AreEqual(paramName, ((ArgumentException)e).ParamName, "Invalid parameter name");
    }

    /// <summary>
    /// Test ExecuteQueryAsync with FormattableString parameter returning a DataTable.
    /// </summary>
    /// <param name="databaseType">Database type.</param>
    /// <returns>A <see cref="Task"/> that represents the asynchronous operation.</returns>
    [TestMethod(DisplayName = "ExecuteQueryAsync with FormattableString parameter returning a DataTable")]
    [DataRow(DatabaseTypes.Sqlite, DisplayName = DatabaseTypes.Sqlite)]
    [DataRow(DatabaseTypes.SqlServer, DisplayName = DatabaseTypes.SqlServer)]
    public async Task Test_ExecuteQueryAsync_FormattableString_DataTable_Async(string databaseType)
    {
        // ARRANGE
        using Context context = await DatabaseUtils.CreateDatabaseAsync(databaseType).ConfigureAwait(false);

        const int recordCount = 20;
        string value = TestUtils.GetMethodName();
        await DatabaseUtils.CreateTestTableEntriesAsync(context, value, recordCount).ConfigureAwait(false);
        FormattableString sql = $"SELECT * FROM TestTable1 WHERE ID <= {recordCount}";

        // ACT
        DataTable dataTable = await context.Database.ExecuteQueryAsync(sql, CancellationToken.None).ConfigureAwait(false);

        // ASSERT
        Assert.AreEqual(recordCount, dataTable.Rows.Count, "Invalid record count");
    }

    /// <summary>
    /// Test ExecuteQueryAsync with FormattableString parameter returning a list of entities.
    /// </summary>
    /// <param name="databaseType">Database type.</param>
    /// <returns>A <see cref="Task"/> that represents the asynchronous operation.</returns>
    [TestMethod(DisplayName = "ExecuteQueryAsync with FormattableString parameter returning a list of entities")]
    [DataRow(DatabaseTypes.Sqlite, DisplayName = DatabaseTypes.Sqlite)]
    [DataRow(DatabaseTypes.SqlServer, DisplayName = DatabaseTypes.SqlServer)]
    public async Task Test_ExecuteQueryAsync_FormattableString_Entity_Async(string databaseType)
    {
        // ARRANGE
        using Context context = await DatabaseUtils.CreateDatabaseAsync(databaseType).ConfigureAwait(false);

        const int recordCount = 20;
        string value = TestUtils.GetMethodName();
        await DatabaseUtils.CreateTestTableEntriesAsync(context, value, recordCount).ConfigureAwait(false);
        FormattableString sql = $"SELECT * FROM TestTable1 WHERE ID <= {recordCount}";

        // ACT
        IList<TestTable1> results = await context.Database.ExecuteQueryAsync<TestTable1>(sql, CancellationToken.None).ConfigureAwait(false);

        // ASSERT
        Assert.HasCount(recordCount, results, "Invalid record count");
    }

    /// <summary>
    /// Test ExecuteQueryAsync with params parameters returning a DataTable.
    /// </summary>
    /// <param name="databaseType">Database type.</param>
    /// <returns>A <see cref="Task"/> that represents the asynchronous operation.</returns>
    [TestMethod(DisplayName = "ExecuteQueryAsync with params parameters returning a DataTable")]
    [DataRow(DatabaseTypes.Sqlite, DisplayName = DatabaseTypes.Sqlite)]
    [DataRow(DatabaseTypes.SqlServer, DisplayName = DatabaseTypes.SqlServer)]
    public async Task Test_ExecuteQueryAsync_Params_DataTable_Async(string databaseType)
    {
        // ARRANGE
        using Context context = await DatabaseUtils.CreateDatabaseAsync(databaseType).ConfigureAwait(false);

        const int recordCount = 20;
        string value = TestUtils.GetMethodName();
        await DatabaseUtils.CreateTestTableEntriesAsync(context, value, recordCount).ConfigureAwait(false);
        string sql = "SELECT * FROM TestTable1 WHERE ID <= {0}";

        // ACT
        DataTable dataTable = await context.Database.ExecuteQueryAsync(sql, CancellationToken.None, recordCount).ConfigureAwait(false);

        // ASSERT
        Assert.AreEqual(recordCount, dataTable.Rows.Count, "Invalid record count");
    }

    /// <summary>
    /// Test ExecuteQueryAsync with params parameters returning a list of entities.
    /// </summary>
    /// <param name="databaseType">Database type.</param>
    /// <returns>A <see cref="Task"/> that represents the asynchronous operation.</returns>
    [TestMethod(DisplayName = "ExecuteQueryAsync with params parameters returning a list of entities")]
    [DataRow(DatabaseTypes.Sqlite, DisplayName = DatabaseTypes.Sqlite)]
    [DataRow(DatabaseTypes.SqlServer, DisplayName = DatabaseTypes.SqlServer)]
    public async Task Test_ExecuteQueryAsync_Params_Entity_Async(string databaseType)
    {
        // ARRANGE
        using Context context = await DatabaseUtils.CreateDatabaseAsync(databaseType).ConfigureAwait(false);

        const int recordCount = 20;
        string value = TestUtils.GetMethodName();
        await DatabaseUtils.CreateTestTableEntriesAsync(context, value, recordCount).ConfigureAwait(false);
        string sql = "SELECT * FROM TestTable1 WHERE ID <= {0}";

        // ACT
        IList<TestTable1> results = await context.Database.ExecuteQueryAsync<TestTable1>(sql, CancellationToken.None, recordCount).ConfigureAwait(false);

        // ASSERT
        Assert.HasCount(recordCount, results, "Invalid record count");
    }

    #endregion public methods

    #region private methods

    /// <summary>
    /// Get test data for the ExecuteQuery method with FormattableString parameter.
    /// </summary>
    /// <returns><see cref="IEnumerable{T}"/>.</returns>
    private static IEnumerable<object?[]> Get_ExecuteQueryAsync_FormattableString_GuardClause_TestData()
    {
        using Context context = DatabaseUtils.CreateDatabaseAsync(DatabaseTypes.Sqlite).Result;

        // 0. DatabaseFacade databaseFacade
        // 1. FormattableString sql
        // 2. Type exceptionType
        // 3. string paramName
        yield return [
            null,
            (FormattableString)$"dummy",
            typeof(ArgumentNullException),
            "databaseFacade"];

        yield return [
            context.Database,
            null,
            typeof(ArgumentNullException),
            "sql"];
    }

    /// <summary>
    /// Get test data for the ExecuteQuery method with String parameter.
    /// </summary>
    /// <returns><see cref="IEnumerable{T}"/>.</returns>
    private static IEnumerable<object?[]> Get_ExecuteQueryAsync_String_GuardClause_TestData()
    {
        using Context context = DatabaseUtils.CreateDatabaseAsync(DatabaseTypes.Sqlite).Result;

        // 0. DatabaseFacade databaseFacade
        // 1. string sql
        // 2. Type exceptionType
        // 3. string paramName
        yield return [
            null,
            "dummy",
            typeof(ArgumentNullException),
            "databaseFacade"];

        yield return [
            context.Database,
            null,
            typeof(ArgumentNullException),
            "sql"];

        yield return [
            context.Database,
            string.Empty,
            typeof(ArgumentException),
            "sql"];
    }

    #endregion private methods
}
