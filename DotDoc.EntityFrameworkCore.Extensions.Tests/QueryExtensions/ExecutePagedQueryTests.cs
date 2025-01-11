// Copyright ©2021-2025 Mike King.
// This file is licensed to you under the MIT license.
// See the License.txt file in the solution root for more information.

using DotDoc.EntityFrameworkCore.Extensions.Constants;
using DotDoc.EntityFrameworkCore.Extensions.Extensions;
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
    [TestMethod]
    [DataRow(DatabaseType.Sqlite, DisplayName = "SQLite ExecutePagedQueryAsync FormattableString.")]
    [DataRow(DatabaseType.SqlServer, DisplayName = "SQL Server ExecutePagedQueryAsync FormattableString.")]
    public void Test_ExecutePagedQuery_FormattableString(string databaseType)
    {
        using Context context = DatabaseUtils.CreateDatabase(databaseType);

        const int recordCount = 20;
        const long page = 2;
        const long pageSize = 5;
        string value = DatabaseUtils.GetMethodName();
        DatabaseUtils.CreateMultipleTestTableEntries(context, value, recordCount);
        FormattableString sql = $"SELECT * FROM TestTable1 WHERE ID <= {recordCount}";

        QueryPage queryPage = context.Database.ExecutePagedQuery(sql, page, pageSize);

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
    /// Test ExecutePagedQuery with FormattableString parameter.
    /// </summary>
    /// <param name="databaseType">Database type.</param>
    /// <returns>A task that represents the asynchronous test operation.</returns>
    [TestMethod]
    [DataRow(DatabaseType.Sqlite, DisplayName = "SQLite ExecutePagedQueryAsync FormattableString.")]
    [DataRow(DatabaseType.SqlServer, DisplayName = "SQL Server ExecutePagedQueryAsync FormattableString.")]
    public async Task Test_ExecutePagedQuery_FormattableStringAsync(string databaseType)
    {
        using Context context = DatabaseUtils.CreateDatabase(databaseType);

        const int recordCount = 20;
        const long page = 2;
        const long pageSize = 5;
        string value = DatabaseUtils.GetMethodName();
        DatabaseUtils.CreateMultipleTestTableEntries(context, value, recordCount);
        FormattableString sql = $"SELECT * FROM TestTable1 WHERE ID <= {recordCount}";

        QueryPage queryPage = await context.Database.ExecutePagedQueryAsync(sql, page, pageSize).ConfigureAwait(false);

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
    [TestMethod]
    [DataRow(DatabaseType.Sqlite, DisplayName = "SQLite ExecutePagedQuery params.")]
    [DataRow(DatabaseType.SqlServer, DisplayName = "SQL Server ExecutePagedQuery params.")]
    public void Test_ExecutePagedQuery_Params(string databaseType)
    {
        using Context context = DatabaseUtils.CreateDatabase(databaseType);

        const int recordCount = 20;
        const long page = 2;
        const long pageSize = 5;
        string value = DatabaseUtils.GetMethodName();
        DatabaseUtils.CreateMultipleTestTableEntries(context, value, recordCount);
        string sql = "SELECT * FROM TestTable1 WHERE ID <= {0}";

        QueryPage queryPage = context.Database.ExecutePagedQuery(sql, page, pageSize, recordCount);

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
    /// <returns>A task that represents the asynchronous test operation.</returns>
    [TestMethod]
    [DataRow(DatabaseType.Sqlite, DisplayName = "SQLite ExecutePagedQuery params.")]
    [DataRow(DatabaseType.SqlServer, DisplayName = "SQL Server ExecutePagedQuery params.")]
    public async Task Test_ExecutePagedQuery_ParamsAsync(string databaseType)
    {
        using Context context = DatabaseUtils.CreateDatabase(databaseType);

        const int recordCount = 20;
        const long page = 2;
        const long pageSize = 5;
        string value = DatabaseUtils.GetMethodName();
        DatabaseUtils.CreateMultipleTestTableEntries(context, value, recordCount);
        string sql = "SELECT * FROM TestTable1 WHERE ID <= {0}";

        QueryPage queryPage = await context.Database.ExecutePagedQueryAsync(sql, page, pageSize, parameters: recordCount).ConfigureAwait(false);

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
    [TestMethod]
    [DataRow(DatabaseType.Sqlite, DisplayName = "SQLite ExecutePagedQuery IEnumerable.")]
    [DataRow(DatabaseType.SqlServer, DisplayName = "SQL Server ExecutePagedQuery IEnumerable.")]
    public void Test_ExecutePagedQuery_IEnumerable(string databaseType)
    {
        using Context context = DatabaseUtils.CreateDatabase(databaseType);

        const int recordCount = 20;
        const long page = 2;
        const long pageSize = 5;
        string value = DatabaseUtils.GetMethodName();
        DatabaseUtils.CreateMultipleTestTableEntries(context, value, recordCount);
        List<object> parameters = [recordCount];
        string sql = "SELECT * FROM TestTable1 WHERE ID <= {0}";

        QueryPage queryPage = context.Database.ExecutePagedQuery(sql, page, pageSize, parameters);

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
    /// <returns>A task that represents the asynchronous test operation.</returns>
    [TestMethod]
    [DataRow(DatabaseType.Sqlite, DisplayName = "SQLite ExecutePagedQuery IEnumerable.")]
    [DataRow(DatabaseType.SqlServer, DisplayName = "SQL Server ExecutePagedQuery IEnumerable.")]
    public async Task Test_ExecutePagedQuery_IEnumerableAsync(string databaseType)
    {
        using Context context = DatabaseUtils.CreateDatabase(databaseType);

        const int recordCount = 20;
        const long page = 2;
        const long pageSize = 5;
        string value = DatabaseUtils.GetMethodName();
        DatabaseUtils.CreateMultipleTestTableEntries(context, value, recordCount);
        List<object> parameters = [recordCount];
        string sql = "SELECT * FROM TestTable1 WHERE ID <= {0}";

        QueryPage queryPage = await context.Database.ExecutePagedQueryAsync(sql, page, pageSize, parameters).ConfigureAwait(false);

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
    [TestMethod]
    [DataRow(DatabaseType.Sqlite, DisplayName = "SQLite ExecutePagedQuery order by.")]
    [DataRow(DatabaseType.SqlServer, DisplayName = "SQL Server ExecutePagedQuery order by.")]
    public void Test_ExecutePagedQuery_OrderBy(string databaseType)
    {
        string value = DatabaseUtils.GetMethodName();
        const int recordCount = 20;
        const long page = 2;
        const long pageSize = 5;

        using Context context = DatabaseUtils.CreateDatabase(databaseType);
        DatabaseUtils.CreateMultipleTestTableEntries(context, value, recordCount);

        QueryPage queryPage = context.Database.ExecutePagedQuery($"SELECT * FROM TestTable1 WHERE ID <= {recordCount} ORDER BY ID", page, pageSize);

        Assert.AreEqual(page, queryPage.Page, "Invalid page");
    }

    /// <summary>
    /// Test the code that splits SQL queries and handles Order By clauses.
    /// </summary>
    /// <param name="databaseType">Database type.</param>
    /// <returns>A task that represents the asynchronous test operation.</returns>
    [TestMethod]
    [DataRow(DatabaseType.Sqlite, DisplayName = "SQLite ExecutePagedQuery order by.")]
    [DataRow(DatabaseType.SqlServer, DisplayName = "SQL Server ExecutePagedQuery order by.")]
    public async Task Test_ExecutePagedQuery_OrderByAsync(string databaseType)
    {
        string value = DatabaseUtils.GetMethodName();
        const int recordCount = 20;
        const long page = 2;
        const long pageSize = 5;

        using Context context = DatabaseUtils.CreateDatabase(databaseType);
        DatabaseUtils.CreateMultipleTestTableEntries(context, value, recordCount);

        QueryPage queryPage = await context.Database.ExecutePagedQueryAsync($"SELECT * FROM TestTable1 WHERE ID <= {recordCount} ORDER BY ID", page, pageSize).ConfigureAwait(false);

        Assert.AreEqual(page, queryPage.Page, "Invalid page");
    }

    /// <summary>
    /// Test if the requested page number is greater than the total number of pages.
    /// The page number should be reduced to the last one.
    /// </summary>
    /// <param name="databaseType">Database type.</param>
    [TestMethod]
    [DataRow(DatabaseType.Sqlite, DisplayName = "SQLite ExecutePagedQuery page number overflow.")]
    [DataRow(DatabaseType.SqlServer, DisplayName = "SQL Server ExecutePagedQuery page number overflow.")]
    public void Test_ExecutePagedQuery_PageNumberOverflow(string databaseType)
    {
        string value = DatabaseUtils.GetMethodName();
        const int recordCount = 20;
        const long page = 999;
        const long pageSize = 5;

        using Context context = DatabaseUtils.CreateDatabase(databaseType);
        DatabaseUtils.CreateMultipleTestTableEntries(context, value, recordCount);

        QueryPage queryPage = context.Database.ExecutePagedQuery($"SELECT * FROM TestTable1 WHERE ID <= {recordCount}", page, pageSize);

        Assert.AreEqual(((recordCount + pageSize - 1) / pageSize) - 1, queryPage.Page, "Invalid page");
    }

    /// <summary>
    /// Test if the requested page number is greater than the total number of pages.
    /// The page number should be reduced to the last one.
    /// </summary>
    /// <param name="databaseType">Database type.</param>
    /// <returns>A task that represents the asynchronous test operation.</returns>
    [TestMethod]
    [DataRow(DatabaseType.Sqlite, DisplayName = "SQLite ExecutePagedQuery page number overflow.")]
    [DataRow(DatabaseType.SqlServer, DisplayName = "SQL Server ExecutePagedQuery page number overflow.")]
    public async Task Test_ExecutePagedQuery_PageNumberOverflowAsync(string databaseType)
    {
        string value = DatabaseUtils.GetMethodName();
        const int recordCount = 20;
        const long page = 999;
        const long pageSize = 5;

        using Context context = DatabaseUtils.CreateDatabase(databaseType);
        DatabaseUtils.CreateMultipleTestTableEntries(context, value, recordCount);

        QueryPage queryPage = await context.Database.ExecutePagedQueryAsync($"SELECT * FROM TestTable1 WHERE ID <= {recordCount}", page, pageSize).ConfigureAwait(false);

        Assert.AreEqual(((recordCount + pageSize - 1) / pageSize) - 1, queryPage.Page, "Invalid page");
    }
}
