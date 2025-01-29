// Copyright ©2021-2025 Mike King.
// This file is licensed to you under the MIT license.
// See the License.txt file in the solution root for more information.

using DotDoc.EntityFrameworkCore.Extensions.Constants;
using DotDoc.EntityFrameworkCore.Extensions.Extensions;
using DotDoc.EntityFrameworkCore.Extensions.Tests.Data;
using DotDoc.EntityFrameworkCore.Extensions.Tests.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DotDoc.EntityFrameworkCore.Extensions.Tests.QueryExtensions;

/// <summary>
/// Tests for ExecuteInsert extensions.
/// </summary>
[TestClass]
public class ExecuteInsertTests
{
    #region public methods

    /// <summary>
    /// Test ExecuteInsert with FormattableString parameter returning a long Id Guard Clauses.
    /// </summary>
    /// <param name="databaseFacade">The <see cref="DatabaseFacade"/> for the context.</param>
    /// <param name="sql">The <see cref="FormattableString"/> representing a SQL query with parameters.</param>
    /// <param name="exceptionType">The type of exception raised.</param>
    /// <param name="paramName">Name of parameter being checked.</param>
    [TestMethod]
    [DynamicData(nameof(Get_ExecuteInsert_FormattableString_GuardClause_TestData), DynamicDataSourceType.Method)]
    public void Test_ExecuteInsert_FormattableString_Long_GuardClauses(DatabaseFacade? databaseFacade, FormattableString? sql, Type exceptionType, string paramName)
    {
        // ARRANGE

        // ACT / ASSERT
        Exception e = Assert.That.ThrowsAnyException(() => databaseFacade!.ExecuteInsert(sql!), "Unexpected exception");
        Assert.AreEqual(exceptionType, e.GetType(), "Invalid exception type");
        Assert.AreEqual(paramName, ((ArgumentException)e).ParamName, "Invalid parameter name");
    }

    /// <summary>
    /// Test ExecuteInsert with FormattableString parameter returning a generic Id Guard Clauses.
    /// </summary>
    /// <param name="databaseFacade">The <see cref="DatabaseFacade"/> for the context.</param>
    /// <param name="sql">The <see cref="FormattableString"/> representing a SQL query with parameters.</param>
    /// <param name="exceptionType">The type of exception raised.</param>
    /// <param name="paramName">Name of parameter being checked.</param>
    [TestMethod]
    [DynamicData(nameof(Get_ExecuteInsert_FormattableString_GuardClause_TestData), DynamicDataSourceType.Method)]
    public void Test_ExecuteInsert_FormattableString_Generic_GuardClauses(DatabaseFacade? databaseFacade, FormattableString? sql, Type exceptionType, string paramName)
    {
        // ARRANGE

        // ACT / ASSERT
        Exception e = Assert.That.ThrowsAnyException(() => databaseFacade!.ExecuteInsert<long>(sql!), "Unexpected exception");
        Assert.AreEqual(exceptionType, e.GetType(), "Invalid exception type");
        Assert.AreEqual(paramName, ((ArgumentException)e).ParamName, "Invalid parameter name");
    }

    /// <summary>
    /// Test ExecuteInsert with string parameter returning a long Id Guard Clauses.
    /// </summary>
    /// <param name="databaseFacade">The <see cref="DatabaseFacade"/> for the context.</param>
    /// <param name="sql">The <see cref="string"/> representing a SQL query with parameters.</param>
    /// <param name="exceptionType">The type of exception raised.</param>
    /// <param name="paramName">Name of parameter being checked.</param>
    [TestMethod]
    [DynamicData(nameof(Get_ExecuteInsert_String_GuardClause_TestData), DynamicDataSourceType.Method)]
    public void Test_ExecuteInsert_String_Long_GuardClauses(DatabaseFacade? databaseFacade, string? sql, Type exceptionType, string paramName)
    {
        // ARRANGE

        // ACT / ASSERT
        Exception e = Assert.That.ThrowsAnyException(() => databaseFacade!.ExecuteInsert(sql!), "Unexpected exception");
        Assert.AreEqual(exceptionType, e.GetType(), "Invalid exception type");
        Assert.AreEqual(paramName, ((ArgumentException)e).ParamName, "Invalid parameter name");
    }

    /// <summary>
    /// Test ExecuteInsert with string parameter returning a generic Id Guard Clauses.
    /// </summary>
    /// <param name="databaseFacade">The <see cref="DatabaseFacade"/> for the context.</param>
    /// <param name="sql">The <see cref="string"/> representing a SQL query with parameters.</param>
    /// <param name="exceptionType">The type of exception raised.</param>
    /// <param name="paramName">Name of parameter being checked.</param>
    [TestMethod]
    [DynamicData(nameof(Get_ExecuteInsert_String_GuardClause_TestData), DynamicDataSourceType.Method)]
    public void Test_ExecuteInsert_String_Generic_GuardClauses(DatabaseFacade? databaseFacade, string? sql, Type exceptionType, string paramName)
    {
        // ARRANGE

        // ACT / ASSERT
        Exception e = Assert.That.ThrowsAnyException(() => databaseFacade!.ExecuteInsert<long>(sql!), "Unexpected exception");
        Assert.AreEqual(exceptionType, e.GetType(), "Invalid exception type");
        Assert.AreEqual(paramName, ((ArgumentException)e).ParamName, "Invalid parameter name");
    }

    /// <summary>
    /// Test ExecuteInsert with FormattableString parameter returning a long Id Guard Clauses.
    /// </summary>
    /// <param name="databaseFacade">The <see cref="DatabaseFacade"/> for the context.</param>
    /// <param name="sql">The <see cref="FormattableString"/> representing a SQL query with parameters.</param>
    /// <param name="exceptionType">The type of exception raised.</param>
    /// <param name="paramName">Name of parameter being checked.</param>
    /// <returns>A task that represents the asynchronous test operation.</returns>
    [TestMethod]
    [DynamicData(nameof(Get_ExecuteInsert_FormattableString_GuardClause_TestData), DynamicDataSourceType.Method)]
    public async Task Test_ExecuteInsert_FormattableString_Long_GuardClausesAsync(DatabaseFacade? databaseFacade, FormattableString? sql, Type exceptionType, string paramName)
    {
        // ARRANGE

        // ACT / ASSERT
        Exception e = await Assert.That.ThrowsAnyExceptionAsync(() => databaseFacade!.ExecuteInsertAsync(sql!), "Unexpected exception").ConfigureAwait(false);
        Assert.AreEqual(exceptionType, e.GetType(), "Invalid exception type");
        Assert.AreEqual(paramName, ((ArgumentException)e).ParamName, "Invalid parameter name");
    }

    /// <summary>
    /// Test ExecuteInsert with FormattableString parameter returning a generic Id Guard Clauses.
    /// </summary>
    /// <param name="databaseFacade">The <see cref="DatabaseFacade"/> for the context.</param>
    /// <param name="sql">The <see cref="FormattableString"/> representing a SQL query with parameters.</param>
    /// <param name="exceptionType">The type of exception raised.</param>
    /// <param name="paramName">Name of parameter being checked.</param>
    /// <returns>A task that represents the asynchronous test operation.</returns>
    [TestMethod]
    [DynamicData(nameof(Get_ExecuteInsert_FormattableString_GuardClause_TestData), DynamicDataSourceType.Method)]
    public async Task Test_ExecuteInsert_FormattableString_Generic_GuardClausesAsync(DatabaseFacade? databaseFacade, FormattableString? sql, Type exceptionType, string paramName)
    {
        // ARRANGE

        // ACT / ASSERT
        Exception e = await Assert.That.ThrowsAnyExceptionAsync(() => databaseFacade!.ExecuteInsertAsync<long>(sql!), "Unexpected exception").ConfigureAwait(false);
        Assert.AreEqual(exceptionType, e.GetType(), "Invalid exception type");
        Assert.AreEqual(paramName, ((ArgumentException)e).ParamName, "Invalid parameter name");
    }

    /// <summary>
    /// Test ExecuteInsert with string parameter returning a long Id Guard Clauses.
    /// </summary>
    /// <param name="databaseFacade">The <see cref="DatabaseFacade"/> for the context.</param>
    /// <param name="sql">The <see cref="string"/> representing a SQL query with parameters.</param>
    /// <param name="exceptionType">The type of exception raised.</param>
    /// <param name="paramName">Name of parameter being checked.</param>
    /// <returns>A task that represents the asynchronous test operation.</returns>
    [TestMethod]
    [DynamicData(nameof(Get_ExecuteInsert_String_GuardClause_TestData), DynamicDataSourceType.Method)]
    public async Task Test_ExecuteInsert_String_Long_GuardClausesAsync(DatabaseFacade? databaseFacade, string? sql, Type exceptionType, string paramName)
    {
        // ARRANGE

        // ACT / ASSERT
        Exception e = await Assert.That.ThrowsAnyExceptionAsync(() => databaseFacade!.ExecuteInsertAsync(sql!), "Unexpected exception").ConfigureAwait(false);
        Assert.AreEqual(exceptionType, e.GetType(), "Invalid exception type");
        Assert.AreEqual(paramName, ((ArgumentException)e).ParamName, "Invalid parameter name");
    }

    /// <summary>
    /// Test ExecuteInsert with string parameter returning a generic Id Guard Clauses.
    /// </summary>
    /// <param name="databaseFacade">The <see cref="DatabaseFacade"/> for the context.</param>
    /// <param name="sql">The <see cref="string"/> representing a SQL query with parameters.</param>
    /// <param name="exceptionType">The type of exception raised.</param>
    /// <param name="paramName">Name of parameter being checked.</param>
    /// <returns>A task that represents the asynchronous test operation.</returns>
    [TestMethod]
    [DynamicData(nameof(Get_ExecuteInsert_String_GuardClause_TestData), DynamicDataSourceType.Method)]
    public async Task Test_ExecuteInsert_String_Generic_GuardClausesAsync(DatabaseFacade? databaseFacade, string? sql, Type exceptionType, string paramName)
    {
        // ARRANGE

        // ACT / ASSERT
        Exception e = await Assert.That.ThrowsAnyExceptionAsync(() => databaseFacade!.ExecuteInsertAsync<long>(sql!), "Unexpected exception").ConfigureAwait(false);
        Assert.AreEqual(exceptionType, e.GetType(), "Invalid exception type");
        Assert.AreEqual(paramName, ((ArgumentException)e).ParamName, "Invalid parameter name");
    }

    /// <summary>
    /// Test ExecuteInsert with FormattableString parameter returning a long Id.
    /// </summary>
    /// <param name="databaseType">Database type.</param>
    [TestMethod]
    [DataRow(DatabaseType.Sqlite, DisplayName = "SQLite ExecuteInsert FormattableString Long Id.")]
    [DataRow(DatabaseType.SqlServer, DisplayName = "SQL Server ExecuteInsert FormattableString Long Id.")]
    public void Test_ExecuteInsert_FormattableString_Long(string databaseType)
    {
        // ARRANGE
        using Context context = DatabaseUtils.CreateDatabase(databaseType);

        string value = DatabaseUtils.GetMethodName();
        FormattableString sql = $"INSERT INTO TestTable1 (TestField) VALUES ({value})";

        // ACT
        long id = context.Database.ExecuteInsert(sql);

        // ASSERT
        int count = context.TestTable1.Count(e => e.Id == id);
        Assert.AreEqual(1, count, "Invalid count");
    }

    /// <summary>
    /// Test ExecuteInsert with FormattableString parameter returning a generic Id.
    /// </summary>
    /// <param name="databaseType">Database type.</param>
    [TestMethod]
    [DataRow(DatabaseType.Sqlite, DisplayName = "SQLite ExecuteInsert FormattableString Generic Id.")]
    [DataRow(DatabaseType.SqlServer, DisplayName = "SQL Server ExecuteInsert FormattableString Generic Id.")]
    public void Test_ExecuteInsert_FormattableString_Generic(string databaseType)
    {
        // ARRANGE
        using Context context = DatabaseUtils.CreateDatabase(databaseType);

        string value = DatabaseUtils.GetMethodName();
        FormattableString sql = $"INSERT INTO TestTable1 (TestField) VALUES ({value})";

        // ACT
        long id = context.Database.ExecuteInsert<long>(sql);

        // ASSERT
        int count = context.TestTable1.Count(e => e.Id == id);
        Assert.AreEqual(1, count, "Invalid count");
    }

    /// <summary>
    /// Test ExecuteInsert with FormattableString parameter returning a long Id.
    /// </summary>
    /// <param name="databaseType">Database type.</param>
    /// <returns>A task that represents the asynchronous test operation.</returns>
    [TestMethod]
    [DataRow(DatabaseType.Sqlite, DisplayName = "SQLite ExecuteInsert FormattableString Long Id.")]
    [DataRow(DatabaseType.SqlServer, DisplayName = "SQL Server ExecuteInsert FormattableString Long Id.")]
    public async Task Test_ExecuteInsert_FormattableString_LongAsync(string databaseType)
    {
        // ARRANGE
        using Context context = DatabaseUtils.CreateDatabase(databaseType);

        string value = DatabaseUtils.GetMethodName();
        FormattableString sql = $"INSERT INTO TestTable1 (TestField) VALUES ({value})";

        // ACT
        long id = await context.Database.ExecuteInsertAsync(sql).ConfigureAwait(false);

        // ASSERT
        int count = await context.TestTable1.CountAsync(e => e.Id == id).ConfigureAwait(false);
        Assert.AreEqual(1, count, "Invalid count");
    }

    /// <summary>
    /// Test ExecuteInsert with FormattableString parameter returning a generic Id.
    /// </summary>
    /// <param name="databaseType">Database type.</param>
    /// <returns>A task that represents the asynchronous test operation.</returns>
    [TestMethod]
    [DataRow(DatabaseType.Sqlite, DisplayName = "SQLite ExecuteInsert FormattableString Generic Id.")]
    [DataRow(DatabaseType.SqlServer, DisplayName = "SQL Server ExecuteInsert FormattableString Generic Id.")]
    public async Task Test_ExecuteInsert_FormattableString_GenericAsync(string databaseType)
    {
        // ARRANGE
        using Context context = DatabaseUtils.CreateDatabase(databaseType);

        string value = DatabaseUtils.GetMethodName();
        FormattableString sql = $"INSERT INTO TestTable1 (TestField) VALUES ({value})";

        // ACT
        long id = await context.Database.ExecuteInsertAsync<long>(sql).ConfigureAwait(false);

        // ASSERT
        int count = await context.TestTable1.CountAsync(e => e.Id == id).ConfigureAwait(false);
        Assert.AreEqual(1, count, "Invalid count");
    }

    /// <summary>
    /// Test ExecuteInsert with params parameters returning a long Id.
    /// </summary>
    /// <param name="databaseType">Database type.</param>
    [TestMethod]
    [DataRow(DatabaseType.Sqlite, DisplayName = "SQLite ExecuteInsert params Long Id.")]
    [DataRow(DatabaseType.SqlServer, DisplayName = "SQL Server ExecuteInsert params Long Id.")]
    public void Test_ExecuteInsert_Params_Long(string databaseType)
    {
        // ARRANGE
        using Context context = DatabaseUtils.CreateDatabase(databaseType);

        string value = DatabaseUtils.GetMethodName();
        string sql = "INSERT INTO TestTable1 (TestField) VALUES ({0})";

        // ACT
        long id = context.Database.ExecuteInsert(sql, value);

        // ASSERT
        int count = context.TestTable1.Count(e => e.Id == id);
        Assert.AreEqual(1, count, "Invalid count");
    }

    /// <summary>
    /// Test ExecuteInsert with params parameters returning a generic Id.
    /// </summary>
    /// <param name="databaseType">Database type.</param>
    [TestMethod]
    [DataRow(DatabaseType.Sqlite, DisplayName = "SQLite ExecuteInsert params Generic Id.")]
    [DataRow(DatabaseType.SqlServer, DisplayName = "SQL Server ExecuteInsert params Generic Id.")]
    public void Test_ExecuteInsert_Params_Generic(string databaseType)
    {
        // ARRANGE
        using Context context = DatabaseUtils.CreateDatabase(databaseType);

        string value = DatabaseUtils.GetMethodName();
        string sql = "INSERT INTO TestTable1 (TestField) VALUES ({0})";

        // ACT
        long id = context.Database.ExecuteInsert<long>(sql, value);

        // ASSERT
        int count = context.TestTable1.Count(e => e.Id == id);
        Assert.AreEqual(1, count, "Invalid count");
    }

    /// <summary>
    /// Test ExecuteInsert with params parameters returning a long Id.
    /// </summary>
    /// <param name="databaseType">Database type.</param>
    /// <returns>A task that represents the asynchronous test operation.</returns>
    [TestMethod]
    [DataRow(DatabaseType.Sqlite, DisplayName = "SQLite ExecuteInsert params Long Id.")]
    [DataRow(DatabaseType.SqlServer, DisplayName = "SQL Server ExecuteInsert params Long Id.")]
    public async Task Test_ExecuteInsert_Params_LongAsync(string databaseType)
    {
        // ARRANGE
        using Context context = DatabaseUtils.CreateDatabase(databaseType);

        string value = DatabaseUtils.GetMethodName();
        string sql = "INSERT INTO TestTable1 (TestField) VALUES ({0})";

        // ACT
        long id = await context.Database.ExecuteInsertAsync(sql, parameters: value).ConfigureAwait(false);

        // ASSERT
        int count = await context.TestTable1.CountAsync(e => e.Id == id).ConfigureAwait(false);
        Assert.AreEqual(1, count, "Invalid count");
    }

    /// <summary>
    /// Test ExecuteInsert with params parameters returning a generic Id.
    /// </summary>
    /// <param name="databaseType">Database type.</param>
    /// <returns>A task that represents the asynchronous test operation.</returns>
    [TestMethod]
    [DataRow(DatabaseType.Sqlite, DisplayName = "SQLite ExecuteInsert params Generic Id.")]
    [DataRow(DatabaseType.SqlServer, DisplayName = "SQL Server ExecuteInsert params Generic Id.")]
    public async Task Test_ExecuteInsert_Params_GenericAsync(string databaseType)
    {
        // ARRANGE
        using Context context = DatabaseUtils.CreateDatabase(databaseType);

        string value = DatabaseUtils.GetMethodName();
        string sql = "INSERT INTO TestTable1 (TestField) VALUES ({0})";

        // ACT
        long id = await context.Database.ExecuteInsertAsync<long>(sql, parameters: value).ConfigureAwait(false);

        // ASSERT
        int count = await context.TestTable1.CountAsync(e => e.Id == id).ConfigureAwait(false);
        Assert.AreEqual(1, count, "Invalid count");
    }

    #endregion public methods

    #region private methods

    /// <summary>
    /// Get test data for the ExecuteInsert method with FormattableString parameter.
    /// </summary>
    /// <returns><see cref="IEnumerable{T}"/>.</returns>
    private static IEnumerable<object?[]> Get_ExecuteInsert_FormattableString_GuardClause_TestData()
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
    /// Get test data for the ExecuteInsert method with String parameter.
    /// </summary>
    /// <returns><see cref="IEnumerable{T}"/>.</returns>
    private static IEnumerable<object?[]> Get_ExecuteInsert_String_GuardClause_TestData()
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
