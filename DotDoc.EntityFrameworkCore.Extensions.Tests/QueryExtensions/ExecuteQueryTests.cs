// Copyright ©2021-2025 Mike King.
// This file is licensed to you under the MIT license.
// See the License.txt file in the solution root for more information.

using DotDoc.EntityFrameworkCore.Extensions.DatabaseType;
using DotDoc.EntityFrameworkCore.Extensions.Query;
using DotDoc.EntityFrameworkCore.Extensions.Tests.Data;
using DotDoc.EntityFrameworkCore.Extensions.Tests.Extensions;
using DotDoc.EntityFrameworkCore.Extensions.Tests.Utilities;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Data;

namespace DotDoc.EntityFrameworkCore.Extensions.Tests.QueryExtensions;

/// <summary>
/// Tests for ExecuteQuery extensions.
/// </summary>
[TestClass]
public class ExecuteQueryTests
{
    #region public methods

    /// <summary>
    /// Test ExecuteQuery with FormattableString parameter returning a DataTable Guard Clauses.
    /// </summary>
    /// <param name="databaseFacade">The <see cref="DatabaseFacade"/> for the context.</param>
    /// <param name="sql">The <see cref="FormattableString"/> representing a SQL query with parameters.</param>
    /// <param name="exceptionType">The type of exception raised.</param>
    /// <param name="paramName">Name of parameter being checked.</param>
    [TestMethod("ExecuteQuery with FormattableString parameter returning a DataTable Guard Clauses")]
    [DynamicData(nameof(Get_ExecuteQuery_FormattableString_GuardClause_TestData), DynamicDataSourceType.Method, DynamicDataDisplayName = nameof(TestUtils.CreateDynamicDisplayName), DynamicDataDisplayNameDeclaringType = typeof(TestUtils))]
    public void Test_ExecuteQuery_FormattableString_DataTable_GuardClauses(DatabaseFacade? databaseFacade, FormattableString? sql, Type exceptionType, string paramName)
    {
        // ARRANGE

        // ACT / ASSERT
        Exception e = Assert.That.ThrowsAnyException(() => databaseFacade!.ExecuteQuery(sql!), "Unexpected exception");
        Assert.AreEqual(exceptionType, e.GetType(), "Invalid exception type");
        Assert.AreEqual(paramName, ((ArgumentException)e).ParamName, "Invalid parameter name");
    }

    /// <summary>
    /// Test ExecuteQuery with FormattableString parameter returning an Entity Guard Clauses.
    /// </summary>
    /// <param name="databaseFacade">The <see cref="DatabaseFacade"/> for the context.</param>
    /// <param name="sql">The <see cref="FormattableString"/> representing a SQL query with parameters.</param>
    /// <param name="exceptionType">The type of exception raised.</param>
    /// <param name="paramName">Name of parameter being checked.</param>
    [TestMethod("ExecuteQuery with FormattableString parameter returning an Entity Guard Clauses")]
    [DynamicData(nameof(Get_ExecuteQuery_FormattableString_GuardClause_TestData), DynamicDataSourceType.Method, DynamicDataDisplayName = nameof(TestUtils.CreateDynamicDisplayName), DynamicDataDisplayNameDeclaringType = typeof(TestUtils))]
    public void Test_ExecuteQuery_FormattableString_Entity_GuardClauses(DatabaseFacade? databaseFacade, FormattableString? sql, Type exceptionType, string paramName)
    {
        // ARRANGE

        // ACT / ASSERT
        Exception e = Assert.That.ThrowsAnyException(() => databaseFacade!.ExecuteQuery<TestTable1>(sql!), "Unexpected exception");
        Assert.AreEqual(exceptionType, e.GetType(), "Invalid exception type");
        Assert.AreEqual(paramName, ((ArgumentException)e).ParamName, "Invalid parameter name");
    }

    /// <summary>
    /// Test ExecuteQuery with string parameter returning a DataTable Guard Clauses.
    /// </summary>
    /// <param name="databaseFacade">The <see cref="DatabaseFacade"/> for the context.</param>
    /// <param name="sql">The <see cref="string"/> representing a SQL query with parameters.</param>
    /// <param name="exceptionType">The type of exception raised.</param>
    /// <param name="paramName">Name of parameter being checked.</param>
    [TestMethod("ExecuteQuery with string parameter returning a DataTable Guard Clauses")]
    [DynamicData(nameof(Get_ExecuteQuery_String_GuardClause_TestData), DynamicDataSourceType.Method, DynamicDataDisplayName = nameof(TestUtils.CreateDynamicDisplayName), DynamicDataDisplayNameDeclaringType = typeof(TestUtils))]
    public void Test_ExecuteQuery_String_DataTable_GuardClauses(DatabaseFacade? databaseFacade, string? sql, Type exceptionType, string paramName)
    {
        // ARRANGE

        // ACT / ASSERT
        Exception e = Assert.That.ThrowsAnyException(() => databaseFacade!.ExecuteQuery(sql!), "Unexpected exception");
        Assert.AreEqual(exceptionType, e.GetType(), "Invalid exception type");
        Assert.AreEqual(paramName, ((ArgumentException)e).ParamName, "Invalid parameter name");
    }

    /// <summary>
    /// Test ExecuteQuery with string parameter returning an Entity Guard Clauses.
    /// </summary>
    /// <param name="databaseFacade">The <see cref="DatabaseFacade"/> for the context.</param>
    /// <param name="sql">The <see cref="string"/> representing a SQL query with parameters.</param>
    /// <param name="exceptionType">The type of exception raised.</param>
    /// <param name="paramName">Name of parameter being checked.</param>
    [TestMethod("ExecuteQuery with string parameter returning an Entity Guard Clauses")]
    [DynamicData(nameof(Get_ExecuteQuery_String_GuardClause_TestData), DynamicDataSourceType.Method, DynamicDataDisplayName = nameof(TestUtils.CreateDynamicDisplayName), DynamicDataDisplayNameDeclaringType = typeof(TestUtils))]
    public void Test_ExecuteQuery_String_Entity_GuardClauses(DatabaseFacade? databaseFacade, string? sql, Type exceptionType, string paramName)
    {
        // ARRANGE

        // ACT / ASSERT
        Exception e = Assert.That.ThrowsAnyException(() => databaseFacade!.ExecuteQuery<TestTable1>(sql!), "Unexpected exception");
        Assert.AreEqual(exceptionType, e.GetType(), "Invalid exception type");
        Assert.AreEqual(paramName, ((ArgumentException)e).ParamName, "Invalid parameter name");
    }

    /// <summary>
    /// Test ExecuteQueryAsync with FormattableString parameter returning a DataTable Guard Clauses.
    /// </summary>
    /// <param name="databaseFacade">The <see cref="DatabaseFacade"/> for the context.</param>
    /// <param name="sql">The <see cref="FormattableString"/> representing a SQL query with parameters.</param>
    /// <param name="exceptionType">The type of exception raised.</param>
    /// <param name="paramName">Name of parameter being checked.</param>
    /// <returns>A task that represents the asynchronous test operation.</returns>
    [TestMethod("ExecuteQueryAsync with FormattableString parameter returning a DataTable Guard Clauses")]
    [DynamicData(nameof(Get_ExecuteQuery_FormattableString_GuardClause_TestData), DynamicDataSourceType.Method, DynamicDataDisplayName = nameof(TestUtils.CreateDynamicDisplayName), DynamicDataDisplayNameDeclaringType = typeof(TestUtils))]
    public async Task Test_ExecuteQuery_FormattableString_DataTable_GuardClausesAsync(DatabaseFacade? databaseFacade, FormattableString? sql, Type exceptionType, string paramName)
    {
        // ARRANGE

        // ACT / ASSERT
        Exception e = await Assert.That.ThrowsAnyExceptionAsync(() => databaseFacade!.ExecuteQueryAsync(sql!), "Unexpected exception").ConfigureAwait(false);
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
    /// <returns>A task that represents the asynchronous test operation.</returns>
    [TestMethod("ExecuteQueryAsync with FormattableString parameter returning an Entity Guard Clauses")]
    [DynamicData(nameof(Get_ExecuteQuery_FormattableString_GuardClause_TestData), DynamicDataSourceType.Method, DynamicDataDisplayName = nameof(TestUtils.CreateDynamicDisplayName), DynamicDataDisplayNameDeclaringType = typeof(TestUtils))]
    public async Task Test_ExecuteQuery_FormattableString_Entity_GuardClausesAsync(DatabaseFacade? databaseFacade, FormattableString? sql, Type exceptionType, string paramName)
    {
        // ARRANGE

        // ACT / ASSERT
        Exception e = await Assert.That.ThrowsAnyExceptionAsync(() => databaseFacade!.ExecuteQueryAsync<TestTable1>(sql!), "Unexpected exception").ConfigureAwait(false);
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
    /// <returns>A task that represents the asynchronous test operation.</returns>
    [TestMethod("ExecuteQueryAsync with string parameter returning a DataTable Guard Clauses")]
    [DynamicData(nameof(Get_ExecuteQuery_String_GuardClause_TestData), DynamicDataSourceType.Method, DynamicDataDisplayName = nameof(TestUtils.CreateDynamicDisplayName), DynamicDataDisplayNameDeclaringType = typeof(TestUtils))]
    public async Task Test_ExecuteQuery_String_DataTable_GuardClausesAsync(DatabaseFacade? databaseFacade, string? sql, Type exceptionType, string paramName)
    {
        // ARRANGE

        // ACT / ASSERT
        Exception e = await Assert.That.ThrowsAnyExceptionAsync(() => databaseFacade!.ExecuteQueryAsync(sql!), "Unexpected exception").ConfigureAwait(false);
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
    /// <returns>A task that represents the asynchronous test operation.</returns>
    [TestMethod("ExecuteQueryAsync with string parameter returning an Entity Guard Clauses")]
    [DynamicData(nameof(Get_ExecuteQuery_String_GuardClause_TestData), DynamicDataSourceType.Method, DynamicDataDisplayName = nameof(TestUtils.CreateDynamicDisplayName), DynamicDataDisplayNameDeclaringType = typeof(TestUtils))]
    public async Task Test_ExecuteQuery_String_Entity_GuardClausesAsync(DatabaseFacade? databaseFacade, string? sql, Type exceptionType, string paramName)
    {
        // ARRANGE

        // ACT / ASSERT
        Exception e = await Assert.That.ThrowsAnyExceptionAsync(() => databaseFacade!.ExecuteQueryAsync<TestTable1>(sql!), "Unexpected exception").ConfigureAwait(false);
        Assert.AreEqual(exceptionType, e.GetType(), "Invalid exception type");
        Assert.AreEqual(paramName, ((ArgumentException)e).ParamName, "Invalid parameter name");
    }

    /// <summary>
    /// Test ExecuteQuery with FormattableString parameter returning a DataTable.
    /// </summary>
    /// <param name="databaseType">Database type.</param>
    [TestMethod("ExecuteQuery with FormattableString parameter returning a DataTable")]
    [DataRow(DatabaseTypes.Sqlite, DisplayName = DatabaseTypes.Sqlite)]
    [DataRow(DatabaseTypes.SqlServer, DisplayName = DatabaseTypes.SqlServer)]
    public void Test_ExecuteQuery_FormattableString_DataTable(string databaseType)
    {
        using Context context = DatabaseUtils.CreateDatabase(databaseType);

        const int recordCount = 20;
        string value = TestUtils.GetMethodName();
        DatabaseUtils.CreateTestTableEntries(context, value, recordCount);
        FormattableString sql = $"SELECT * FROM TestTable1 WHERE ID <= {recordCount}";

        DataTable dataTable = context.Database.ExecuteQuery(sql);

        Assert.AreEqual(recordCount, dataTable.Rows.Count, "Invalid record count");
    }

    /// <summary>
    /// Test ExecuteQueryAsync with FormattableString parameter returning a DataTable.
    /// </summary>
    /// <param name="databaseType">Database type.</param>
    /// <returns>A task that represents the asynchronous test operation.</returns>
    [TestMethod("ExecuteQueryAsync with FormattableString parameter returning a DataTable")]
    [DataRow(DatabaseTypes.Sqlite, DisplayName = DatabaseTypes.Sqlite)]
    [DataRow(DatabaseTypes.SqlServer, DisplayName = DatabaseTypes.SqlServer)]
    public async Task Test_ExecuteQuery_FormattableString_DataTableAsync(string databaseType)
    {
        using Context context = DatabaseUtils.CreateDatabase(databaseType);

        const int recordCount = 20;
        string value = TestUtils.GetMethodName();
        DatabaseUtils.CreateTestTableEntries(context, value, recordCount);
        FormattableString sql = $"SELECT * FROM TestTable1 WHERE ID <= {recordCount}";

        DataTable dataTable = await context.Database.ExecuteQueryAsync(sql).ConfigureAwait(false);

        Assert.AreEqual(recordCount, dataTable.Rows.Count, "Invalid record count");
    }

    /// <summary>
    /// Test ExecuteQuery with FormattableString parameter returning a list of entities.
    /// </summary>
    /// <param name="databaseType">Database type.</param>
    [TestMethod("ExecuteQuery with FormattableString parameter returning a list of entities")]
    [DataRow(DatabaseTypes.Sqlite, DisplayName = DatabaseTypes.Sqlite)]
    [DataRow(DatabaseTypes.SqlServer, DisplayName = DatabaseTypes.SqlServer)]
    public void Test_ExecuteQuery_FormattableString_Entity(string databaseType)
    {
        using Context context = DatabaseUtils.CreateDatabase(databaseType);

        const int recordCount = 20;
        string value = TestUtils.GetMethodName();
        DatabaseUtils.CreateTestTableEntries(context, value, recordCount);
        FormattableString sql = $"SELECT * FROM TestTable1 WHERE ID <= {recordCount}";

        IList<TestTable1> results = context.Database.ExecuteQuery<TestTable1>(sql);

        Assert.AreEqual(recordCount, results.Count, "Invalid record count");
    }

    /// <summary>
    /// Test ExecuteQueryAsync with FormattableString parameter returning a list of entities.
    /// </summary>
    /// <param name="databaseType">Database type.</param>
    /// <returns>A task that represents the asynchronous test operation.</returns>
    [TestMethod("ExecuteQueryAsync with FormattableString parameter returning a list of entities")]
    [DataRow(DatabaseTypes.Sqlite, DisplayName = DatabaseTypes.Sqlite)]
    [DataRow(DatabaseTypes.SqlServer, DisplayName = DatabaseTypes.SqlServer)]
    public async Task Test_ExecuteQuery_FormattableString_EntityAsync(string databaseType)
    {
        using Context context = DatabaseUtils.CreateDatabase(databaseType);

        const int recordCount = 20;
        string value = TestUtils.GetMethodName();
        DatabaseUtils.CreateTestTableEntries(context, value, recordCount);
        FormattableString sql = $"SELECT * FROM TestTable1 WHERE ID <= {recordCount}";

        IList<TestTable1> results = await context.Database.ExecuteQueryAsync<TestTable1>(sql).ConfigureAwait(false);

        Assert.AreEqual(recordCount, results.Count, "Invalid record count");
    }

    /// <summary>
    /// Test ExecuteQuery with params parameters returning a DataTable.
    /// </summary>
    /// <param name="databaseType">Database type.</param>
    [TestMethod("ExecuteQuery with params parameters returning a DataTable")]
    [DataRow(DatabaseTypes.Sqlite, DisplayName = DatabaseTypes.Sqlite)]
    [DataRow(DatabaseTypes.SqlServer, DisplayName = DatabaseTypes.SqlServer)]
    public void Test_ExecuteQuery_Params_DataTable(string databaseType)
    {
        using Context context = DatabaseUtils.CreateDatabase(databaseType);

        const int recordCount = 20;
        string value = TestUtils.GetMethodName();
        DatabaseUtils.CreateTestTableEntries(context, value, recordCount);
        string sql = "SELECT * FROM TestTable1 WHERE ID <= {0}";

        DataTable dataTable = context.Database.ExecuteQuery(sql, recordCount);

        Assert.AreEqual(recordCount, dataTable.Rows.Count, "Invalid record count");
    }

    /// <summary>
    /// Test ExecuteQueryAsync with params parameters returning a DataTable.
    /// </summary>
    /// <param name="databaseType">Database type.</param>
    /// <returns>A task that represents the asynchronous test operation.</returns>
    [TestMethod("ExecuteQueryAsync with params parameters returning a DataTable")]
    [DataRow(DatabaseTypes.Sqlite, DisplayName = DatabaseTypes.Sqlite)]
    [DataRow(DatabaseTypes.SqlServer, DisplayName = DatabaseTypes.SqlServer)]
    public async Task Test_ExecuteQuery_Params_DataTableAsync(string databaseType)
    {
        using Context context = DatabaseUtils.CreateDatabase(databaseType);

        const int recordCount = 20;
        string value = TestUtils.GetMethodName();
        DatabaseUtils.CreateTestTableEntries(context, value, recordCount);
        string sql = "SELECT * FROM TestTable1 WHERE ID <= {0}";

        DataTable dataTable = await context.Database.ExecuteQueryAsync(sql, parameters: recordCount).ConfigureAwait(false);

        Assert.AreEqual(recordCount, dataTable.Rows.Count, "Invalid record count");
    }

    /// <summary>
    /// Test ExecuteQuery with params parameters returning a list of entities.
    /// </summary>
    /// <param name="databaseType">Database type.</param>
    [TestMethod("ExecuteQuery with params parameters returning a list of entities")]
    [DataRow(DatabaseTypes.Sqlite, DisplayName = DatabaseTypes.Sqlite)]
    [DataRow(DatabaseTypes.SqlServer, DisplayName = DatabaseTypes.SqlServer)]
    public void Test_ExecuteQuery_Params_Entity(string databaseType)
    {
        using Context context = DatabaseUtils.CreateDatabase(databaseType);

        const int recordCount = 20;
        string value = TestUtils.GetMethodName();
        DatabaseUtils.CreateTestTableEntries(context, value, recordCount);
        string sql = "SELECT * FROM TestTable1 WHERE ID <= {0}";

        IList<TestTable1> results = context.Database.ExecuteQuery<TestTable1>(sql, parameters: recordCount);

        Assert.AreEqual(recordCount, results.Count, "Invalid record count");
    }

    /// <summary>
    /// Test ExecuteQueryAsync with params parameters returning a list of entities.
    /// </summary>
    /// <param name="databaseType">Database type.</param>
    /// <returns>A task that represents the asynchronous test operation.</returns>
    [TestMethod("ExecuteQueryAsync with params parameters returning a list of entities")]
    [DataRow(DatabaseTypes.Sqlite, DisplayName = DatabaseTypes.Sqlite)]
    [DataRow(DatabaseTypes.SqlServer, DisplayName = DatabaseTypes.SqlServer)]
    public async Task Test_ExecuteQuery_Params_EntityAsync(string databaseType)
    {
        using Context context = DatabaseUtils.CreateDatabase(databaseType);

        const int recordCount = 20;
        string value = TestUtils.GetMethodName();
        DatabaseUtils.CreateTestTableEntries(context, value, recordCount);
        string sql = "SELECT * FROM TestTable1 WHERE ID <= {0}";

        IList<TestTable1> results = await context.Database.ExecuteQueryAsync<TestTable1>(sql, parameters: recordCount).ConfigureAwait(false);

        Assert.AreEqual(recordCount, results.Count, "Invalid record count");
    }

    #endregion public methods

    #region private methods

    /// <summary>
    /// Get test data for the ExecuteQuery method with FormattableString parameter.
    /// </summary>
    /// <returns><see cref="IEnumerable{T}"/>.</returns>
    private static IEnumerable<object?[]> Get_ExecuteQuery_FormattableString_GuardClause_TestData()
    {
        using Context context = DatabaseUtils.CreateDatabase(DatabaseTypes.Sqlite);

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
    private static IEnumerable<object?[]> Get_ExecuteQuery_String_GuardClause_TestData()
    {
        using Context context = DatabaseUtils.CreateDatabase(DatabaseTypes.Sqlite);

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
