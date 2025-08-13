// Copyright ©2021-2025 Mike King.
// This file is licensed to you under the MIT license.
// See the License.txt file in the solution root for more information.

using DotDoc.EntityFrameworkCore.Extensions.DatabaseType;
using DotDoc.EntityFrameworkCore.Extensions.Execute;
using DotDoc.EntityFrameworkCore.Extensions.Tests.Data;
using DotDoc.EntityFrameworkCore.Extensions.Tests.Utilities;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace DotDoc.EntityFrameworkCore.Extensions.Tests.Execute;

/// <summary>
/// Tests for ExecuteNonQuery extensions.
/// </summary>
[TestClass]
public class ExecuteNonQueryTests
{
    #region public methods

    /// <summary>
    /// Test ExecuteNonQueryAsync with FormattableString parameter Guard Clauses.
    /// </summary>
    /// <param name="databaseFacade">The <see cref="DatabaseFacade"/> for the context.</param>
    /// <param name="sql">The <see cref="FormattableString"/> representing a SQL query with parameters.</param>
    /// <param name="exceptionType">The type of exception raised.</param>
    /// <param name="paramName">Name of parameter being checked.</param>
    /// <returns>A <see cref="Task"/> that represents the asynchronous operation.</returns>
    [TestMethod("ExecuteNonQueryAsync with FormattableString parameter Guard Clauses")]
    [DynamicData(nameof(Get_ExecuteNonQueryAsync_FormattableString_GuardClause_TestData), DynamicDataSourceType.Method, DynamicDataDisplayName = nameof(TestUtils.CreateDynamicDisplayName), DynamicDataDisplayNameDeclaringType = typeof(TestUtils))]
    public async Task Test_ExecuteNonQueryAsync_FormattableString_GuardClauses_Async(DatabaseFacade? databaseFacade, FormattableString? sql, Type exceptionType, string paramName)
    {
        // ARRANGE

        // ACT / ASSERT
        Exception e = await Assert.ThrowsAsync<Exception>(() => databaseFacade!.ExecuteNonQueryAsync(sql!, CancellationToken.None), "Unexpected exception").ConfigureAwait(false);
        Assert.AreEqual(exceptionType, e.GetType(), "Invalid exception type");
        Assert.AreEqual(paramName, ((ArgumentException)e).ParamName, "Invalid parameter name");
    }

    /// <summary>
    /// Test ExecuteNonQueryAsync with string parameter Guard Clauses.
    /// </summary>
    /// <param name="databaseFacade">The <see cref="DatabaseFacade"/> for the context.</param>
    /// <param name="sql">The <see cref="string"/> representing a SQL query with parameters.</param>
    /// <param name="exceptionType">The type of exception raised.</param>
    /// <param name="paramName">Name of parameter being checked.</param>
    /// <returns>A <see cref="Task"/> that represents the asynchronous operation.</returns>
    [TestMethod("ExecuteNonQueryAsync with string parameter Guard Clauses")]
    [DynamicData(nameof(Get_ExecuteNonQueryAsync_String_GuardClause_TestData), DynamicDataSourceType.Method, DynamicDataDisplayName = nameof(TestUtils.CreateDynamicDisplayName), DynamicDataDisplayNameDeclaringType = typeof(TestUtils))]
    public async Task Test_ExecuteNonQueryAsync_String_GuardClauses_Async(DatabaseFacade? databaseFacade, string? sql, Type exceptionType, string paramName)
    {
        // ARRANGE

        // ACT / ASSERT
        Exception e = await Assert.ThrowsAsync<Exception>(() => databaseFacade!.ExecuteNonQueryAsync(sql!, CancellationToken.None), "Unexpected exception").ConfigureAwait(false);
        Assert.AreEqual(exceptionType, e.GetType(), "Invalid exception type");
        Assert.AreEqual(paramName, ((ArgumentException)e).ParamName, "Invalid parameter name");
    }

    /// <summary>
    /// Test ExecuteNonQueryAsync with FormattableString parameter.
    /// </summary>
    /// <param name="databaseType">Database type.</param>
    /// <returns>A <see cref="Task"/> that represents the asynchronous operation.</returns>
    [TestMethod("ExecuteNonQueryAsync with FormattableString parameter")]
    [DataRow(DatabaseTypes.Sqlite, DisplayName = DatabaseTypes.Sqlite)]
    [DataRow(DatabaseTypes.SqlServer, DisplayName = DatabaseTypes.SqlServer)]
    public async Task Test_ExecuteNonQueryAsync_FormattableString_Async(string databaseType)
    {
        // ARRANGE
        using Context context = await DatabaseUtils.CreateDatabaseAsync(databaseType).ConfigureAwait(false);

        string value = TestUtils.GetMethodName();
        await DatabaseUtils.CreateTestTableEntriesAsync(context, value, 1).ConfigureAwait(false);
        long minId = 0;

        FormattableString sql = $"DELETE FROM TestTable1 WHERE ID >  {minId}";

        // ACT
        long count = await context.Database.ExecuteNonQueryAsync(sql, CancellationToken.None).ConfigureAwait(false);

        // ASSERT
        Assert.AreEqual(1, count, "Invalid count");
    }

    /// <summary>
    /// Test ExecuteNonQueryAsync with params parameters.
    /// </summary>
    /// <param name="databaseType">Database type.</param>
    /// <returns>A <see cref="Task"/> that represents the asynchronous operation.</returns>
    [TestMethod("ExecuteNonQueryAsync with params parameters")]
    [DataRow(DatabaseTypes.Sqlite, DisplayName = DatabaseTypes.Sqlite)]
    [DataRow(DatabaseTypes.SqlServer, DisplayName = DatabaseTypes.SqlServer)]
    public async Task Test_ExecuteNonQueryAsync_Params_Async(string databaseType)
    {
        // ARRANGE
        using Context context = await DatabaseUtils.CreateDatabaseAsync(databaseType).ConfigureAwait(false);

        string value = TestUtils.GetMethodName();
        await DatabaseUtils.CreateTestTableEntriesAsync(context, value, 1).ConfigureAwait(false);
        long minId = 0;

        string sql = "DELETE FROM TestTable1 WHERE ID > {0}";

        // ACT
        long count = await context.Database.ExecuteNonQueryAsync(sql, CancellationToken.None, minId).ConfigureAwait(false);

        // ASSERT
        Assert.AreEqual(1, count, "Invalid count");
    }

    #endregion public methods

    #region private methods

    /// <summary>
    /// Get test data for the ExecuteNonQuery method with FormattableString parameter.
    /// </summary>
    /// <returns><see cref="IEnumerable{T}"/>.</returns>
    private static IEnumerable<object?[]> Get_ExecuteNonQueryAsync_FormattableString_GuardClause_TestData()
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
    /// Get test data for the ExecuteNonQuery method with String parameter.
    /// </summary>
    /// <returns><see cref="IEnumerable{T}"/>.</returns>
    private static IEnumerable<object?[]> Get_ExecuteNonQueryAsync_String_GuardClause_TestData()
    {
        // 0. DatabaseFacade databaseFacade
        // 1. string sql
        // 2. Type exceptionType
        // 3. string paramName
        using Context context = DatabaseUtils.CreateDatabaseAsync(DatabaseTypes.Sqlite).Result;

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
