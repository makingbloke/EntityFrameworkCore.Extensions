// Copyright ©2021-2023 Mike King.
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
    /// Test ExecutePagedQuery with FormattableString parameter.
    /// </summary>
    /// <param name="databaseType">Database type.</param>
    /// <param name="useAsync">If <see langword="true"/> then tests the async method.</param>
    /// <returns>A task that represents the asynchronous test operation.</returns>
    [TestMethod]
    [DataRow(DatabaseType.Sqlite, false, DisplayName = "SQLite ExecutePagedQuery FormattableString.")]
    [DataRow(DatabaseType.Sqlite, true, DisplayName = "SQLite ExecutePagedQueryAsync FormattableString.")]
    [DataRow(DatabaseType.SqlServer, false, DisplayName = "SQL Server ExecutePagedQuery FormattableString.")]
    [DataRow(DatabaseType.SqlServer, true, DisplayName = "SQL Server ExecutePagedQueryAsync FormattableString.")]
    public async Task TestExecutePagedQueryWithFormattableStringAsync(DatabaseType databaseType, bool useAsync)
    {
        using Context context = DatabaseUtils.CreateDatabase(databaseType);

        const int recordCount = 20;
        const long page = 2;
        const long pageSize = 5;
        string value = DatabaseUtils.GetMethodName();
        DatabaseUtils.CreateMultipleTestTableEntries(context, value, recordCount);
        FormattableString sql = $"SELECT * FROM TestTable1 WHERE ID <= {recordCount}";

        QueryPage queryPage = useAsync
            ? await context.Database.ExecutePagedQueryAsync(sql, page, pageSize).ConfigureAwait(false)
            : context.Database.ExecutePagedQuery(sql, page, pageSize);

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
    /// Test ExecutePagedQueryAsync with params parameters.
    /// </summary>
    /// <param name="databaseType">Database type.</param>
    /// <param name="useAsync">If <see langword="true"/> then tests the async method.</param>
    /// <returns>A task that represents the asynchronous test operation.</returns>
    [TestMethod]
    [DataRow(DatabaseType.Sqlite, false, DisplayName = "SQLite ExecutePagedQuery params.")]
    [DataRow(DatabaseType.Sqlite, true, DisplayName = "SQLite ExecutePagedQueryAsync params.")]
    [DataRow(DatabaseType.SqlServer, false, DisplayName = "SQL Server ExecutePagedQuery params.")]
    [DataRow(DatabaseType.SqlServer, true, DisplayName = "SQL Server ExecutePagedQueryAsync params.")]
    public async Task TestExecutePagedQueryWithParamsAsync(DatabaseType databaseType, bool useAsync)
    {
        using Context context = DatabaseUtils.CreateDatabase(databaseType);

        const int recordCount = 20;
        const long page = 2;
        const long pageSize = 5;
        string value = DatabaseUtils.GetMethodName();
        DatabaseUtils.CreateMultipleTestTableEntries(context, value, recordCount);
        string sql = "SELECT * FROM TestTable1 WHERE ID <= {0}";

        QueryPage queryPage = useAsync
            ? await context.Database.ExecutePagedQueryAsync(sql, page, pageSize, parameters: recordCount).ConfigureAwait(false)
            : context.Database.ExecutePagedQuery(sql, page, pageSize, recordCount);

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
    /// Test ExecutePagedQueryAsync with IEnumerable parameters.
    /// </summary>
    /// <param name="databaseType">Database type.</param>
    /// <param name="useAsync">If <see langword="true"/> then tests the async method.</param>
    /// <returns>A task that represents the asynchronous test operation.</returns>
    [TestMethod]
    [DataRow(DatabaseType.Sqlite, false, DisplayName = "SQLite ExecutePagedQuery IEnumerable.")]
    [DataRow(DatabaseType.Sqlite, true, DisplayName = "SQLite ExecutePagedQueryAsync IEnumerable.")]
    [DataRow(DatabaseType.SqlServer, false, DisplayName = "SQL Server ExecutePagedQuery IEnumerable.")]
    [DataRow(DatabaseType.SqlServer, true, DisplayName = "SQL Server ExecutePagedQueryAsync IEnumerable.")]
    public async Task TestExecutePagedQueryWithIEnumerableAsync(DatabaseType databaseType, bool useAsync)
    {
        using Context context = DatabaseUtils.CreateDatabase(databaseType);

        const int recordCount = 20;
        const long page = 2;
        const long pageSize = 5;
        string value = DatabaseUtils.GetMethodName();
        DatabaseUtils.CreateMultipleTestTableEntries(context, value, recordCount);
        List<object> parameters = new () { recordCount };
        string sql = "SELECT * FROM TestTable1 WHERE ID <= {0}";

        QueryPage queryPage = useAsync
            ? await context.Database.ExecutePagedQueryAsync(sql, page, pageSize, parameters).ConfigureAwait(false)
            : context.Database.ExecutePagedQuery(sql, page, pageSize, parameters);

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
    /// <returns>A task that represents the asynchronous test operation.</returns>
    [TestMethod]
    [DataRow(DatabaseType.Sqlite, false, DisplayName = "SQLite ExecutePagedQuery order by.")]
    [DataRow(DatabaseType.Sqlite, true, DisplayName = "SQLite ExecutePagedQueryAsync order by.")]
    [DataRow(DatabaseType.SqlServer, false, DisplayName = "SQL Server ExecutePagedQuery order by.")]
    [DataRow(DatabaseType.SqlServer, true, DisplayName = "SQL Server ExecutePagedQueryAsync order by.")]
    public async Task TestExecutePagedQueryOrderByAsync(DatabaseType databaseType, bool useAsync)
    {
        string value = DatabaseUtils.GetMethodName();
        const int recordCount = 20;
        const long page = 2;
        const long pageSize = 5;

        using Context context = DatabaseUtils.CreateDatabase(databaseType);
        DatabaseUtils.CreateMultipleTestTableEntries(context, value, recordCount);

        QueryPage queryPage = useAsync
            ? await context.Database.ExecutePagedQueryAsync($"SELECT * FROM TestTable1 WHERE ID <= {recordCount} ORDER BY ID", page, pageSize).ConfigureAwait(false)
            : context.Database.ExecutePagedQuery($"SELECT * FROM TestTable1 WHERE ID <= {recordCount} ORDER BY ID", page, pageSize);

        Assert.AreEqual(page, queryPage.Page, "Invalid page");
    }

    /// <summary>
    /// Test if the requested page number is greater than the total number of pages.
    /// The page number should be reduced to the last one.
    /// </summary>
    /// <param name="databaseType">Database type.</param>
    /// <param name="useAsync">If <see langword="true"/> then tests the async method.</param>
    /// <returns>A task that represents the asynchronous test operation.</returns>
    [TestMethod]
    [DataRow(DatabaseType.Sqlite, false, DisplayName = "SQLite ExecutePagedQuery page number overflow.")]
    [DataRow(DatabaseType.Sqlite, true, DisplayName = "SQLite ExecutePagedQueryAsync page number overflow.")]
    [DataRow(DatabaseType.SqlServer, false, DisplayName = "SQL Server ExecutePagedQuery page number overflow.")]
    [DataRow(DatabaseType.SqlServer, true, DisplayName = "SQL Server ExecutePagedQueryAsync page number overflow.")]
    public async Task TestExecutePagedQueryPageNumberOverflowAsync(DatabaseType databaseType, bool useAsync)
    {
        string value = DatabaseUtils.GetMethodName();
        const int recordCount = 20;
        const long page = 999;
        const long pageSize = 5;

        using Context context = DatabaseUtils.CreateDatabase(databaseType);
        DatabaseUtils.CreateMultipleTestTableEntries(context, value, recordCount);

        QueryPage queryPage = useAsync
            ? await context.Database.ExecutePagedQueryAsync($"SELECT * FROM TestTable1 WHERE ID <= {recordCount}", page, pageSize).ConfigureAwait(false)
            : context.Database.ExecutePagedQuery($"SELECT * FROM TestTable1 WHERE ID <= {recordCount}", page, pageSize);

        Assert.AreEqual(((recordCount + pageSize - 1) / pageSize) - 1, queryPage.Page, "Invalid page");
    }
}
