// Copyright ©2021-2025 Mike King.
// This file is licensed to you under the MIT license.
// See the License.txt file in the solution root for more information.

using DotDoc.EntityFrameworkCore.Extensions.Constants;
using DotDoc.EntityFrameworkCore.Extensions.Extensions;
using DotDoc.EntityFrameworkCore.Extensions.Tests.Data;
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
        using Context context = DatabaseUtils.CreateDatabase(databaseType, useExecuteUpdateExtensions: true);

        string value = DatabaseUtils.GetMethodName();
        string originalValue = $"Original {value}";
        string updatedValue = $"Updated {value}";

        DatabaseUtils.CreateMultipleTestTableEntries(context, originalValue, (rowCount + 1) * 10);

        long startId = 2;
        long endId = startId + rowCount - 1;

        IQueryable<TestTable1> query = context.TestTable1.Where(e => e.Id >= startId && e.Id <= endId);

        IList<TestTable1> rows = query.ExecuteUpdateGetRows(builder =>
        {
            builder.SetProperty(e => e.TestField, updatedValue);
        });

        Assert.IsNotNull(rows, "Rows is null");
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
        using Context context = DatabaseUtils.CreateDatabase(databaseType, useExecuteUpdateExtensions: true);

        string value = DatabaseUtils.GetMethodName();
        string originalValue = $"Original {value}";
        string updatedValue = $"Updated {value}";

        DatabaseUtils.CreateMultipleTestTableEntries(context, originalValue, (rowCount + 1) * 10);

        long startId = 2;
        long endId = startId + rowCount - 1;

        IQueryable<TestTable1> query = context.TestTable1.Where(e => e.Id >= startId && e.Id <= endId);

        IList<TestTable1> rows = await query.ExecuteUpdateGetRowsAsync(builder =>
        {
            builder.SetProperty(e => e.TestField, updatedValue);
        }).ConfigureAwait(false);

        Assert.IsNotNull(rows, "Rows is null");
        Assert.AreEqual(rowCount, rows.Count, "Invalid count");

        foreach (TestTable1 row in rows)
        {
            Assert.AreEqual(updatedValue, row.TestField, "Unexpected field value");
        }
    }

    #endregion public methods
}
