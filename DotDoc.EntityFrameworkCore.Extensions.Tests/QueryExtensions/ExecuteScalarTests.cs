// Copyright ©2021-2025 Mike King.
// This file is licensed to you under the MIT license.
// See the License.txt file in the solution root for more information.

using DotDoc.EntityFrameworkCore.Extensions.Constants;
using DotDoc.EntityFrameworkCore.Extensions.Extensions;
using DotDoc.EntityFrameworkCore.Extensions.Tests.Data;
using DotDoc.EntityFrameworkCore.Extensions.Tests.Extensions;
using DotDoc.EntityFrameworkCore.Extensions.Tests.Utilities;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DotDoc.EntityFrameworkCore.Extensions.Tests.QueryExtensions;

/// <summary>
/// Tests for ExecuteScalar extensions.
/// </summary>
[TestClass]
public class ExecuteScalarTests
{
    #region public methods

    /// <summary>
    /// Test ExecuteScalar with FormattableString parameter Guard Clauses.
    /// </summary>
    /// <param name="databaseFacade">The <see cref="DatabaseFacade"/> for the context.</param>
    /// <param name="sql">The <see cref="FormattableString"/> representing a SQL query with parameters.</param>
    /// <param name="exceptionType">The type of exception raised.</param>
    /// <param name="paramName">Name of parameter being checked.</param>
    [TestMethod("ExecuteScalar with FormattableString parameter Guard Clauses")]
    [DynamicData(nameof(Get_ExecuteScalar_FormattableString_GuardClause_TestData), DynamicDataSourceType.Method, DynamicDataDisplayName = nameof(TestUtils.CreateDynamicDisplayName), DynamicDataDisplayNameDeclaringType = typeof(TestUtils))]
    public void Test_ExecuteScalar_FormattableString_GuardClauses(DatabaseFacade? databaseFacade, FormattableString? sql, Type exceptionType, string paramName)
    {
        // ARRANGE

        // ACT / ASSERT
        Exception e = Assert.That.ThrowsAnyException(() => databaseFacade!.ExecuteScalar<int>(sql!), "Unexpected exception");
        Assert.AreEqual(exceptionType, e.GetType(), "Invalid exception type");
        Assert.AreEqual(paramName, ((ArgumentException)e).ParamName, "Invalid parameter name");
    }

    /// <summary>
    /// Test ExecuteScalar with string parameter Guard Clauses.
    /// </summary>
    /// <param name="databaseFacade">The <see cref="DatabaseFacade"/> for the context.</param>
    /// <param name="sql">The <see cref="string"/> representing a SQL query with parameters.</param>
    /// <param name="exceptionType">The type of exception raised.</param>
    /// <param name="paramName">Name of parameter being checked.</param>
    [TestMethod("ExecuteScalar with string parameter Guard Clauses")]
    [DynamicData(nameof(Get_ExecuteScalar_String_GuardClause_TestData), DynamicDataSourceType.Method, DynamicDataDisplayName = nameof(TestUtils.CreateDynamicDisplayName), DynamicDataDisplayNameDeclaringType = typeof(TestUtils))]
    public void Test_ExecuteScalar_String_GuardClauses(DatabaseFacade? databaseFacade, string? sql, Type exceptionType, string paramName)
    {
        // ARRANGE

        // ACT / ASSERT
        Exception e = Assert.That.ThrowsAnyException(() => databaseFacade!.ExecuteScalar<int>(sql!), "Unexpected exception");
        Assert.AreEqual(exceptionType, e.GetType(), "Invalid exception type");
        Assert.AreEqual(paramName, ((ArgumentException)e).ParamName, "Invalid parameter name");
    }

    /// <summary>
    /// Test ExecuteScalarAsync with FormattableString parameter Guard Clauses.
    /// </summary>
    /// <param name="databaseFacade">The <see cref="DatabaseFacade"/> for the context.</param>
    /// <param name="sql">The <see cref="FormattableString"/> representing a SQL query with parameters.</param>
    /// <param name="exceptionType">The type of exception raised.</param>
    /// <param name="paramName">Name of parameter being checked.</param>
    /// <returns>A task that represents the asynchronous test operation.</returns>
    [TestMethod("ExecuteScalarAsync with FormattableString parameter Guard Clauses")]
    [DynamicData(nameof(Get_ExecuteScalar_FormattableString_GuardClause_TestData), DynamicDataSourceType.Method, DynamicDataDisplayName = nameof(TestUtils.CreateDynamicDisplayName), DynamicDataDisplayNameDeclaringType = typeof(TestUtils))]
    public async Task Test_ExecuteScalar_FormattableString_GuardClausesAsync(DatabaseFacade? databaseFacade, FormattableString? sql, Type exceptionType, string paramName)
    {
        // ARRANGE

        // ACT / ASSERT
        Exception e = await Assert.That.ThrowsAnyExceptionAsync(() => databaseFacade!.ExecuteScalarAsync<int>(sql!), "Unexpected exception").ConfigureAwait(false);
        Assert.AreEqual(exceptionType, e.GetType(), "Invalid exception type");
        Assert.AreEqual(paramName, ((ArgumentException)e).ParamName, "Invalid parameter name");
    }

    /// <summary>
    /// Test ExecuteScalarAsync with string parameter Guard Clauses.
    /// </summary>
    /// <param name="databaseFacade">The <see cref="DatabaseFacade"/> for the context.</param>
    /// <param name="sql">The <see cref="string"/> representing a SQL query with parameters.</param>
    /// <param name="exceptionType">The type of exception raised.</param>
    /// <param name="paramName">Name of parameter being checked.</param>
    /// <returns>A task that represents the asynchronous test operation.</returns>
    [TestMethod("ExecuteScalarAsync with string parameter Guard Clauses")]
    [DynamicData(nameof(Get_ExecuteScalar_String_GuardClause_TestData), DynamicDataSourceType.Method, DynamicDataDisplayName = nameof(TestUtils.CreateDynamicDisplayName), DynamicDataDisplayNameDeclaringType = typeof(TestUtils))]
    public async Task Test_ExecuteScalar_String_GuardClausesAsync(DatabaseFacade? databaseFacade, string? sql, Type exceptionType, string paramName)
    {
        // ARRANGE

        // ACT / ASSERT
        Exception e = await Assert.That.ThrowsAnyExceptionAsync(() => databaseFacade!.ExecuteScalarAsync<int>(sql!), "Unexpected exception").ConfigureAwait(false);
        Assert.AreEqual(exceptionType, e.GetType(), "Invalid exception type");
        Assert.AreEqual(paramName, ((ArgumentException)e).ParamName, "Invalid parameter name");
    }

    /// <summary>
    /// Test ExecuteScalar with FormattableString parameter.
    /// </summary>
    /// <param name="databaseType">Database type.</param>
    [TestMethod("ExecuteScalar with FormattableString parameter")]
    [DataRow(DatabaseType.Sqlite, DisplayName = DatabaseType.Sqlite)]
    [DataRow(DatabaseType.SqlServer, DisplayName = DatabaseType.SqlServer)]
    public void Test_ExecuteScalar_FormattableString(string databaseType)
    {
        // ARRANGE
        using Context context = DatabaseUtils.CreateDatabase(databaseType);

        string value = TestUtils.GetMethodName();
        DatabaseUtils.CreateTestTableEntries(context, value, 1);
        long minId = 0;

        FormattableString sql = $"SELECT COUNT(*) FROM TestTable1 WHERE ID > {minId}";

        // ACT
        int count = context.Database.ExecuteScalar<int>(sql);

        // ASSERT
        Assert.AreEqual(1, count, "Invalid count");
    }

    /// <summary>
    /// Test ExecuteScalarAsync with FormattableString parameter.
    /// </summary>
    /// <param name="databaseType">Database type.</param>
    /// <returns>A task that represents the asynchronous test operation.</returns>
    [TestMethod("ExecuteScalarAsync with FormattableString parameter")]
    [DataRow(DatabaseType.Sqlite, DisplayName = DatabaseType.Sqlite)]
    [DataRow(DatabaseType.SqlServer, DisplayName = DatabaseType.SqlServer)]
    public async Task Test_ExecuteScalar_FormattableStringAsync(string databaseType)
    {
        // ARRANGE
        using Context context = DatabaseUtils.CreateDatabase(databaseType);

        string value = TestUtils.GetMethodName();
        DatabaseUtils.CreateTestTableEntries(context, value, 1);
        long minId = 0;

        FormattableString sql = $"SELECT COUNT(*) FROM TestTable1 WHERE ID > {minId}";

        // ACT
        int count = await context.Database.ExecuteScalarAsync<int>(sql).ConfigureAwait(false);

        // ASSERT
        Assert.AreEqual(1, count, "Invalid count");
    }

    /// <summary>
    /// Test ExecuteScalar with params parameter.
    /// </summary>
    /// <param name="databaseType">Database type.</param>
    [TestMethod("ExecuteScalar with params parameter")]
    [DataRow(DatabaseType.Sqlite, DisplayName = DatabaseType.Sqlite)]
    [DataRow(DatabaseType.SqlServer, DisplayName = DatabaseType.SqlServer)]
    public void Test_ExecuteScalar_Params(string databaseType)
    {
        // ARRANGE
        using Context context = DatabaseUtils.CreateDatabase(databaseType);

        string value = TestUtils.GetMethodName();
        DatabaseUtils.CreateTestTableEntries(context, value, 1);
        long minId = 0;

        string sql = "SELECT COUNT(*) FROM TestTable1 WHERE ID > {0}";

        // ACT
        int count = context.Database.ExecuteScalar<int>(sql, minId);

        // ASSERT
        Assert.AreEqual(1, count, "Invalid count");
    }

    /// <summary>
    /// Test ExecuteScalarAsync with params parameter.
    /// </summary>
    /// <param name="databaseType">Database type.</param>
    /// <returns>A task that represents the asynchronous test operation.</returns>
    [TestMethod("ExecuteScalarAsync with params parameter")]
    [DataRow(DatabaseType.Sqlite, DisplayName = DatabaseType.Sqlite)]
    [DataRow(DatabaseType.SqlServer, DisplayName = DatabaseType.SqlServer)]
    public async Task Test_ExecuteScalar_ParamsAsync(string databaseType)
    {
        // ARRANGE
        using Context context = DatabaseUtils.CreateDatabase(databaseType);

        string value = TestUtils.GetMethodName();
        DatabaseUtils.CreateTestTableEntries(context, value, 1);
        long minId = 0;

        string sql = "SELECT COUNT(*) FROM TestTable1 WHERE ID > {0}";

        // ACT
        int count = await context.Database.ExecuteScalarAsync<int>(sql, parameters: minId).ConfigureAwait(false);

        // ASSERT
        Assert.AreEqual(1, count, "Invalid count");
    }

    #endregion public methods

    #region private methods

    /// <summary>
    /// Get test data for the ExecuteScalar method with FormattableString parameter.
    /// </summary>
    /// <returns><see cref="IEnumerable{T}"/>.</returns>
    private static IEnumerable<object?[]> Get_ExecuteScalar_FormattableString_GuardClause_TestData()
    {
        using Context context = DatabaseUtils.CreateDatabase(DatabaseType.Sqlite);

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
    /// Get test data for the ExecuteScalar method with String parameter.
    /// </summary>
    /// <returns><see cref="IEnumerable{T}"/>.</returns>
    private static IEnumerable<object?[]> Get_ExecuteScalar_String_GuardClause_TestData()
    {
        using Context context = DatabaseUtils.CreateDatabase(DatabaseType.Sqlite);

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
