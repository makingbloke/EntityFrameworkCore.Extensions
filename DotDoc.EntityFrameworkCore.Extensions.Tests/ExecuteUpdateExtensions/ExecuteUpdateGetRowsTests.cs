﻿// Copyright ©2021-2025 Mike King.
// This file is licensed to you under the MIT license.
// See the License.txt file in the solution root for more information.

using DotDoc.EntityFrameworkCore.Extensions.Classes;
using DotDoc.EntityFrameworkCore.Extensions.Constants;
using DotDoc.EntityFrameworkCore.Extensions.Extensions;
using DotDoc.EntityFrameworkCore.Extensions.Tests.Data;
using DotDoc.EntityFrameworkCore.Extensions.Tests.Utilities;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DotDoc.EntityFrameworkCore.Extensions.Tests.ExecuteUpdateExtensions;

/// <summary>
/// Tests for ExecuteUpdateGetRows extensions.
/// </summary>
[TestClass]
public class ExecuteUpdateGetRowsTests
{
    #region public methods

    /// <summary>
    /// Test UseExecuteUpdateExtensions Guard Clause.
    /// </summary>
    [TestMethod("UseExecuteUpdateExtensions Guard Clause")]
    public void Test_UseExecuteUpdateExtensions_GuardClause()
    {
        // ARRANGE
        DbContextOptionsBuilder? optionsBuilder = null;
        string paramName = "optionsBuilder";

        // ACT / ASSERT
        ArgumentNullException e = Assert.ThrowsExactly<ArgumentNullException>(() => _ = optionsBuilder!.UseExecuteUpdateExtensions(), "Missing exception");
        Assert.AreEqual(paramName, e.ParamName, "Invalid parameter name");
    }

    /// <summary>
    /// Test ExecuteUpdateGetRows Guard Clauses.
    /// </summary>
    /// <param name="query">The LINQ query.</param>
    /// <param name="setPropertyAction">A method containing set property statements specifying properties to update.</param>
    /// <param name="exceptionType">The type of exception raised.</param>
    /// <param name="paramName">Name of parameter being checked.</param>
    [TestMethod("ExecuteUpdateGetRows Guard Clauses")]
    [DynamicData(nameof(Get_ExecuteUpdateGetRows_GuardClause_TestData), DynamicDataSourceType.Method, DynamicDataDisplayName = nameof(TestUtils.CreateDynamicDisplayName), DynamicDataDisplayNameDeclaringType = typeof(TestUtils))]
    public void Test_ExecuteUpdateGetRows_GuardClauses(IQueryable<TestTable1> query, Action<SetPropertyBuilder<TestTable1>> setPropertyAction, Type exceptionType, string paramName)
    {
        // ARRANGE

        // ACT / ASSERT
        Exception e = Assert.Throws<Exception>(() => query.ExecuteUpdateGetRows(setPropertyAction!), "Missing exception");
        Assert.AreEqual(exceptionType, e.GetType(), "Invalid exception type");
        Assert.AreEqual(paramName, ((ArgumentException)e).ParamName, "Invalid parameter name");
    }

    /// <summary>
    /// Test ExecuteUpdateGetRowsAsync Guard Clauses.
    /// </summary>
    /// <param name="query">The LINQ query.</param>
    /// <param name="setPropertyAction">A method containing set property statements specifying properties to update.</param>
    /// <param name="exceptionType">The type of exception raised.</param>
    /// <param name="paramName">Name of parameter being checked.</param>
    /// <returns>A task that represents the asynchronous test operation.</returns>
    [TestMethod("ExecuteUpdateGetRowsAsync Guard Clauses")]
    [DynamicData(nameof(Get_ExecuteUpdateGetRows_GuardClause_TestData), DynamicDataSourceType.Method, DynamicDataDisplayName = nameof(TestUtils.CreateDynamicDisplayName), DynamicDataDisplayNameDeclaringType = typeof(TestUtils))]
    public async Task Test_ExecuteUpdateGetRows_GuardClausesAsync(IQueryable<TestTable1> query, Action<SetPropertyBuilder<TestTable1>> setPropertyAction, Type exceptionType, string paramName)
    {
        // ARRANGE

        // ACT / ASSERT
        Exception e = await Assert.ThrowsAsync<Exception>(() => query.ExecuteUpdateGetRowsAsync(setPropertyAction!), "Missing exception").ConfigureAwait(false);
        Assert.AreEqual(exceptionType, e.GetType(), "Invalid exception type");
        Assert.AreEqual(paramName, ((ArgumentException)e).ParamName, "Invalid parameter name");
    }

    /// <summary>
    /// Test ExecuteUpdateGetRows.
    /// </summary>
    /// <param name="databaseType">Database type.</param>
    /// <param name="count">Number of records to update.</param>
    [TestMethod("ExecuteUpdateGetRows")]
    [DataRow(DatabaseType.Sqlite, 0, DisplayName = $"{DatabaseType.Sqlite} Update 0 Records.")]
    [DataRow(DatabaseType.Sqlite, 1, DisplayName = $"{DatabaseType.Sqlite} Update 1 Record.")]
    [DataRow(DatabaseType.Sqlite, 10, DisplayName = $"{DatabaseType.Sqlite} Update 10 Records.")]
    [DataRow(DatabaseType.SqlServer, 0, DisplayName = $"{DatabaseType.SqlServer} Update 0 Records.")]
    [DataRow(DatabaseType.SqlServer, 1, DisplayName = $"{DatabaseType.SqlServer} Update 1 Record.")]
    [DataRow(DatabaseType.SqlServer, 10, DisplayName = $"{DatabaseType.SqlServer} Update 10 Records.")]
    public void Test_ExecuteUpdateGetRows(string databaseType, int count)
    {
        // ARRANGE
        using Context context = DatabaseUtils.CreateDatabase(
            databaseType,
            customConfigurationActions: (optionsBuilder) => optionsBuilder.UseExecuteUpdateExtensions());

        string value = TestUtils.GetMethodName();
        string originalValue = $"Original {value}";
        string updatedValue = $"Updated {value}";

        DatabaseUtils.CreateTestTableEntries(context, originalValue, (count + 1) * 10);

        long startId = 2;
        long endId = startId + count - 1;

        IQueryable<TestTable1> query = context.TestTable1.Where(e => e.Id >= startId && e.Id <= endId);

        // ACT
        IList<TestTable1> rows = query.ExecuteUpdateGetRows(builder =>
        {
            builder.SetProperty(e => e.TestField, updatedValue);
        });

        // ASSERT
        Assert.AreEqual(count, rows.Count, "Invalid count");

        foreach (TestTable1 row in rows)
        {
            Assert.AreEqual(updatedValue, row.TestField, "Unexpected field value");
        }
    }

    /// <summary>
    /// Test ExecuteUpdateGetRowsAsync.
    /// </summary>
    /// <param name="databaseType">Database type.</param>
    /// <param name="count">Number of records to update.</param>
    /// <returns>A task that represents the asynchronous test operation.</returns>
    [TestMethod("ExecuteUpdateGetRowsAsync")]
    [DataRow(DatabaseType.Sqlite, 0, DisplayName = $"{DatabaseType.Sqlite} Update 0 Records.")]
    [DataRow(DatabaseType.Sqlite, 1, DisplayName = $"{DatabaseType.Sqlite} Update 1 Record.")]
    [DataRow(DatabaseType.Sqlite, 10, DisplayName = $"{DatabaseType.Sqlite} Update 10 Records.")]
    [DataRow(DatabaseType.SqlServer, 0, DisplayName = $"{DatabaseType.SqlServer} Update 0 Records.")]
    [DataRow(DatabaseType.SqlServer, 1, DisplayName = $"{DatabaseType.SqlServer} Update 1 Record.")]
    [DataRow(DatabaseType.SqlServer, 10, DisplayName = $"{DatabaseType.SqlServer} Update 10 Records.")]
    public async Task Test_ExecuteUpdateGetRowsAsync(string databaseType, int count)
    {
        // ARRANGE
        using Context context = DatabaseUtils.CreateDatabase(
            databaseType,
            customConfigurationActions: (optionsBuilder) => optionsBuilder.UseExecuteUpdateExtensions());

        string value = TestUtils.GetMethodName();
        string originalValue = $"Original {value}";
        string updatedValue = $"Updated {value}";

        DatabaseUtils.CreateTestTableEntries(context, originalValue, (count + 1) * 10);

        long startId = 2;
        long endId = startId + count - 1;

        IQueryable<TestTable1> query = context.TestTable1.Where(e => e.Id >= startId && e.Id <= endId);

        // ACT
        IList<TestTable1> rows = await query.ExecuteUpdateGetRowsAsync(builder =>
        {
            builder.SetProperty(e => e.TestField, updatedValue);
        }).ConfigureAwait(false);

        // ASSERT
        Assert.AreEqual(count, rows.Count, "Invalid count");

        foreach (TestTable1 row in rows)
        {
            Assert.AreEqual(updatedValue, row.TestField, "Unexpected field value");
        }
    }

    #endregion public methods

    #region private methods

    /// <summary>
    /// Get test data for ExecuteUpdateGetRows methods.
    /// </summary>
    /// <returns><see cref="IEnumerable{T}"/>.</returns>
    private static IEnumerable<object?[]> Get_ExecuteUpdateGetRows_GuardClause_TestData()
    {
        // 0. IQueryable<TestTable1> query
        // 1. Action<SetPropertyBuilder<TestTable1>> setPropertyAction
        // 2. Type exceptionType
        // 3. string paramName
        yield return [
            null,
            new Action<SetPropertyBuilder<TestTable1>>(builder => { }),
            typeof(ArgumentNullException),
            "query"];

        yield return [
            Array.Empty<TestTable1>().AsQueryable(),
            null,
            typeof(ArgumentNullException),
            "setPropertyAction"];
    }

    #endregion private methods
}
