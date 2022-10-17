// Copyright ©2021-2022 Mike King.
// This file is licensed to you under the MIT license.
// See the License.txt file in the solution root for more information.

using DotDoc.EntityFrameworkCore.Extensions.Constants;
using DotDoc.EntityFrameworkCore.Extensions.Model;
using DotDoc.EntityFrameworkCore.Extensions.Tests.Data;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DotDoc.EntityFrameworkCore.Extensions.Tests.QueryExtensions;

/// <summary>
/// Tests for ExecutePagedQuery extensions.
/// </summary>
[TestClass]
public class ExecutePagedQueryTests
{
    /// <summary>
    /// Test ExecutePagedQueryInterpolated.
    /// </summary>
    /// <param name="databaseType">Database type.</param>
    /// <param name="useAsync">If <see langword="true"/> then tests the async method.</param>
    /// <returns><see cref="Task"/>.</returns>
    [TestMethod]
    [DataRow(DatabaseType.Sqlite, false, DisplayName = "SQLite ExecutePagedQueryInterpolated")]
    [DataRow(DatabaseType.Sqlite, true, DisplayName = "SQLite ExecutePagedQueryInterpolatedAsync")]
    [DataRow(DatabaseType.SqlServer, false, DisplayName = "SQL Server ExecutePagedQueryInterpolated")]
    [DataRow(DatabaseType.SqlServer, true, DisplayName = "SQL Server ExecutePagedQueryInterpolatedAsync")]
    public async Task TestExecutePagedQueryInterpolatedAsync(DatabaseType databaseType, bool useAsync)
    {
        string value = DatabaseUtils.GetMethodName();
        const int recordCount = 20;
        const long page = 2;
        const long pageSize = 5;

        using Context context = DatabaseUtils.CreateDatabase(databaseType);
        DatabaseUtils.CreateMultipleTestTableEntries(context, value, recordCount);

        QueryPage queryPage = useAsync
            ? await context.Database.ExecutePagedQueryInterpolatedAsync($"SELECT * FROM TestTable1 WHERE ID <= {recordCount}", page, pageSize).ConfigureAwait(false)
            : context.Database.ExecutePagedQueryInterpolated($"SELECT * FROM TestTable1 WHERE ID <= {recordCount}", page, pageSize);

        Assert.AreEqual(page, queryPage.Page, "Invalid page");
        Assert.AreEqual(pageSize, queryPage.PageSize, "Invalid page size");
        Assert.AreEqual(recordCount, queryPage.RecordCount, "Invalid record count");
        Assert.AreEqual((recordCount + pageSize - 1) / pageSize, queryPage.PageCount, "Invalid row page count");
        Assert.AreEqual(pageSize, queryPage.DataTable.Rows.Count, "Invalid data table row count");

        for (int i = 0; i < pageSize; i++)
        {
            Assert.AreEqual((page * pageSize) + i + 1, queryPage.DataTable.Rows[i]["ID"], $"invalid Id[{i}]");
        }
    }

    /// <summary>
    /// Test ExecutePagedQueryRawAsync.
    /// </summary>
    /// <param name="databaseType">Database type.</param>
    /// <param name="useAsync">If <see langword="true"/> then tests the async method.</param>
    /// <returns><see cref="Task"/>.</returns>
    [TestMethod]
    [DataRow(DatabaseType.Sqlite, false, DisplayName = "SQLite ExecutePagedQueryRaw")]
    [DataRow(DatabaseType.Sqlite, true, DisplayName = "SQLite ExecutePagedQueryRawAsync")]
    [DataRow(DatabaseType.SqlServer, false, DisplayName = "SQL Server ExecutePagedQueryRaw")]
    [DataRow(DatabaseType.SqlServer, true, DisplayName = "SQL Server ExecutePagedQueryRawAsync")]
    public async Task TestExecutePagedQueryRawAsync(DatabaseType databaseType, bool useAsync)
    {
        string value = DatabaseUtils.GetMethodName();
        const int recordCount = 20;
        const long page = 2;
        const long pageSize = 5;

        using Context context = DatabaseUtils.CreateDatabase(databaseType);
        DatabaseUtils.CreateMultipleTestTableEntries(context, value, recordCount);

        QueryPage queryPage = useAsync
            ? await context.Database.ExecutePagedQueryRawAsync("SELECT * FROM TestTable1 WHERE ID <= {0}", page, pageSize, parameters: recordCount).ConfigureAwait(false)
            : context.Database.ExecutePagedQueryRaw("SELECT * FROM TestTable1 WHERE ID <= {0}", page, pageSize, recordCount);

        Assert.AreEqual(page, queryPage.Page, "Invalid page");
        Assert.AreEqual(pageSize, queryPage.PageSize, "Invalid page size");
        Assert.AreEqual(recordCount, queryPage.RecordCount, "Invalid record count");
        Assert.AreEqual((recordCount + pageSize - 1) / pageSize, queryPage.PageCount, "Invalid row page count");
        Assert.AreEqual(pageSize, queryPage.DataTable.Rows.Count, "Invalid data table row count");

        for (int i = 0; i < pageSize; i++)
        {
            Assert.AreEqual((page * pageSize) + i + 1, queryPage.DataTable.Rows[i]["ID"], $"invalid Id[{i}]");
        }
    }

    /// <summary>
    /// Test the code that splits SQL queries and handles Order By clauses.
    /// </summary>
    /// <param name="databaseType">Database type.</param>
    /// <param name="useAsync">If <see langword="true"/> then tests the async method.</param>
    /// <returns><see cref="Task"/>.</returns>
    [TestMethod]
    [DataRow(DatabaseType.Sqlite, false, DisplayName = "SQLite ExecutePagedQueryInterpolated order by.")]
    [DataRow(DatabaseType.Sqlite, true, DisplayName = "SQLite ExecutePagedQueryInterpolatedAsync order by.")]
    [DataRow(DatabaseType.SqlServer, false, DisplayName = "SQL Server ExecutePagedQueryInterpolated order by.")]
    [DataRow(DatabaseType.SqlServer, true, DisplayName = "SQL Server ExecutePagedQueryInterpolatedAsync order by.")]
    public async Task TestExecutePagedQueryInterpolatedOrderByAsync(DatabaseType databaseType, bool useAsync)
    {
        string value = DatabaseUtils.GetMethodName();
        const int recordCount = 20;
        const long page = 2;
        const long pageSize = 5;

        using Context context = DatabaseUtils.CreateDatabase(databaseType);
        DatabaseUtils.CreateMultipleTestTableEntries(context, value, recordCount);

        QueryPage queryPage = useAsync
            ? await context.Database.ExecutePagedQueryInterpolatedAsync($"SELECT * FROM TestTable1 WHERE ID <= {recordCount} ORDER BY ID", page, pageSize).ConfigureAwait(false)
            : context.Database.ExecutePagedQueryInterpolated($"SELECT * FROM TestTable1 WHERE ID <= {recordCount} ORDER BY ID", page, pageSize);

        Assert.AreEqual(page, queryPage.Page, "Invalid page");
    }

    /// <summary>
    /// Test if the requested page number is greater than the total number of pages.
    /// The page number should be reduced to the last one.
    /// </summary>
    /// <param name="databaseType">Database type.</param>
    /// <param name="useAsync">If <see langword="true"/> then tests the async method.</param>
    /// <returns><see cref="Task"/>.</returns>
    [TestMethod]
    [DataRow(DatabaseType.Sqlite, false, DisplayName = "SQLite ExecutePagedQueryInterpolated page number overflow.")]
    [DataRow(DatabaseType.Sqlite, true, DisplayName = "SQLite ExecutePagedQueryInterpolatedAsync page number overflow.")]
    [DataRow(DatabaseType.SqlServer, false, DisplayName = "SQL Server ExecutePagedQueryInterpolated page number overflow.")]
    [DataRow(DatabaseType.SqlServer, true, DisplayName = "SQL Server ExecutePagedQueryInterpolatedAsync page number overflow.")]
    public async Task TestExecutePagedQueryInterpolatedPageNumberOverflowAsync(DatabaseType databaseType, bool useAsync)
    {
        string value = DatabaseUtils.GetMethodName();
        const int recordCount = 20;
        const long page = 999;
        const long pageSize = 5;

        using Context context = DatabaseUtils.CreateDatabase(databaseType);
        DatabaseUtils.CreateMultipleTestTableEntries(context, value, recordCount);

        QueryPage queryPage = useAsync
            ? await context.Database.ExecutePagedQueryInterpolatedAsync($"SELECT * FROM TestTable1 WHERE ID <= {recordCount}", page, pageSize).ConfigureAwait(false)
            : context.Database.ExecutePagedQueryInterpolated($"SELECT * FROM TestTable1 WHERE ID <= {recordCount}", page, pageSize);

        Assert.AreEqual(((recordCount + pageSize - 1) / pageSize) - 1, queryPage.Page, "Invalid page");
    }
}
