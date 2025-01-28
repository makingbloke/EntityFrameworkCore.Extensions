// Copyright ©2021-2025 Mike King.
// This file is licensed to you under the MIT license.
// See the License.txt file in the solution root for more information.

using DotDoc.EntityFrameworkCore.Extensions.Classes;
using DotDoc.EntityFrameworkCore.Extensions.Constants;
using DotDoc.EntityFrameworkCore.Extensions.Extensions;
using DotDoc.EntityFrameworkCore.Extensions.Tests.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DotDoc.EntityFrameworkCore.Extensions.Tests.ExecuteUpdate;

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
    [TestMethod]
    public void Test_UseExecuteUpdateExtensions_GuardClause()
    {
        // ARRANGE
        DbContextOptionsBuilder? optionsBuilder = null;
        string paramName = "optionsBuilder";

        // ACT / ASSERT
        ArgumentNullException e = Assert.ThrowsException<ArgumentNullException>(() => optionsBuilder!.UseExecuteUpdateExtensions(), "Missing exception");
        Assert.AreEqual(paramName, e.ParamName, "Invalid parameter name");
    }

    /// <summary>
    /// Test ExecuteUpdateGetRows Guard Clauses.
    /// </summary>
    /// <param name="query">The LINQ query.</param>
    /// <param name="setPropertyAction">A method containing set property statements specifying properties to update.</param>
    /// <param name="paramName">Name of parameter being checked.</param>
    [TestMethod]
    [DynamicData(nameof(Get_ExecuteUpdateGetRows_TestData), DynamicDataSourceType.Method)]
    public void Test_ExecuteUpdateGetRows_GuardClauses(IQueryable<TestTable1> query, Action<SetPropertyBuilder<TestTable1>> setPropertyAction, string paramName)
    {
        // ARRANGE

        // ACT / ASSERT
        ArgumentNullException e = Assert.ThrowsException<ArgumentNullException>(() => query.ExecuteUpdateGetRows(setPropertyAction!), "Missing exception");
        Assert.AreEqual(paramName, e.ParamName, "Invalid parameter name");
    }

    /// <summary>
    /// Test ExecuteUpdateGetRows Guard Clauses.
    /// </summary>
    /// <param name="query">The LINQ query.</param>
    /// <param name="setPropertyAction">A method containing set property statements specifying properties to update.</param>
    /// <param name="paramName">Name of parameter being checked.</param>
    /// <returns>A task that represents the asynchronous test operation.</returns>
    [TestMethod]
    [DynamicData(nameof(Get_ExecuteUpdateGetRows_TestData), DynamicDataSourceType.Method)]
    public async Task Test_ExecuteUpdateGetRows_GuardClausesAsync(IQueryable<TestTable1> query, Action<SetPropertyBuilder<TestTable1>> setPropertyAction, string paramName)
    {
        // ARRANGE

        // ACT / ASSERT
        ArgumentNullException e = await Assert.ThrowsExceptionAsync<ArgumentNullException>(() => query.ExecuteUpdateGetRowsAsync(setPropertyAction!), "Missing exception").ConfigureAwait(false);
        Assert.AreEqual(paramName, e.ParamName, "Invalid parameter name");
    }

    /// <summary>
    /// Test ExecuteUpdateGetRows.
    /// </summary>
    /// <param name="databaseType">Database type.</param>
    /// <param name="rowCount">Number of rows to update.</param>
    [TestMethod]
    [DataRow(DatabaseType.Sqlite, 0, DisplayName = "SQLite ExecuteUpdateGetRows Update 0 Rows.")]
    [DataRow(DatabaseType.Sqlite, 1, DisplayName = "SQLite ExecuteUpdateGetRows Update 1 Row.")]
    [DataRow(DatabaseType.Sqlite, 10, DisplayName = "SQLite ExecuteUpdateGetRows Update 10 Rows.")]
    [DataRow(DatabaseType.SqlServer, 0, DisplayName = "SQL Server ExecuteUpdateGetRows Update 0 Rows.")]
    [DataRow(DatabaseType.SqlServer, 1, DisplayName = "SQL Server ExecuteUpdateGetRows Update 1 Row.")]
    [DataRow(DatabaseType.SqlServer, 10, DisplayName = "SQL Server ExecuteUpdateGetRows Update 10 Rows.")]
    public void Test_ExecuteUpdateGetRows(string databaseType, int rowCount)
    {
        // ARRANGE
        using Context context = DatabaseUtils.CreateDatabase(databaseType, useExecuteUpdateExtensions: true);

        string value = DatabaseUtils.GetMethodName();
        string originalValue = $"Original {value}";
        string updatedValue = $"Updated {value}";

        DatabaseUtils.CreateMultipleTestTableEntries(context, originalValue, (rowCount + 1) * 10);

        long startId = 2;
        long endId = startId + rowCount - 1;

        IQueryable<TestTable1> query = context.TestTable1.Where(e => e.Id >= startId && e.Id <= endId);

        // ACT
        IList<TestTable1> rows = query.ExecuteUpdateGetRows(builder =>
        {
            builder.SetProperty(e => e.TestField, updatedValue);
        });

        // ASSERT
        Assert.AreEqual(rowCount, rows.Count, "Invalid count");

        foreach (TestTable1 row in rows)
        {
            Assert.AreEqual(updatedValue, row.TestField, "Unexpected field value");
        }
    }

    /// <summary>
    /// Test ExecuteUpdateGetRows.
    /// </summary>
    /// <param name="databaseType">Database type.</param>
    /// <param name="rowCount">Number of rows to update.</param>
    /// <returns>A task that represents the asynchronous test operation.</returns>
    [TestMethod]
    [DataRow(DatabaseType.Sqlite, 0, DisplayName = "SQLite ExecuteUpdateGetRows Update 0 Rows.")]
    [DataRow(DatabaseType.Sqlite, 1, DisplayName = "SQLite ExecuteUpdateGetRows Update 1 Row.")]
    [DataRow(DatabaseType.Sqlite, 10, DisplayName = "SQLite ExecuteUpdateGetRows Update 10 Rows.")]
    [DataRow(DatabaseType.SqlServer, 0, DisplayName = "SQL Server ExecuteUpdateGetRows Update 0 Rows.")]
    [DataRow(DatabaseType.SqlServer, 1, DisplayName = "SQL Server ExecuteUpdateGetRows Update 1 Row.")]
    [DataRow(DatabaseType.SqlServer, 10, DisplayName = "SQL Server ExecuteUpdateGetRows Update 10 Rows.")]
    public async Task Test_ExecuteUpdateGetRowsAsync(string databaseType, int rowCount)
    {
        // ARRANGE
        using Context context = DatabaseUtils.CreateDatabase(databaseType, useExecuteUpdateExtensions: true);

        string value = DatabaseUtils.GetMethodName();
        string originalValue = $"Original {value}";
        string updatedValue = $"Updated {value}";

        DatabaseUtils.CreateMultipleTestTableEntries(context, originalValue, (rowCount + 1) * 10);

        long startId = 2;
        long endId = startId + rowCount - 1;

        IQueryable<TestTable1> query = context.TestTable1.Where(e => e.Id >= startId && e.Id <= endId);

        // ACT
        IList<TestTable1> rows = await query.ExecuteUpdateGetRowsAsync(builder =>
        {
            builder.SetProperty(e => e.TestField, updatedValue);
        }).ConfigureAwait(false);

        // ASSERT
        Assert.AreEqual(rowCount, rows.Count, "Invalid count");

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
    private static IEnumerable<object?[]> Get_ExecuteUpdateGetRows_TestData()
    {
        yield return [null, new Action<SetPropertyBuilder<TestTable1>>(builder => { }), "query"];
        yield return [Array.Empty<TestTable1>().AsQueryable(), null, "setPropertyAction"];
    }

    #endregion private methods
}
