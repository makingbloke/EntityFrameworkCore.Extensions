// Copyright ©2021-2025 Mike King.
// This file is licensed to you under the MIT license.
// See the License.txt file in the solution root for more information.

using DotDoc.EntityFrameworkCore.Extensions.Constants;
using DotDoc.EntityFrameworkCore.Extensions.Extensions;
using DotDoc.EntityFrameworkCore.Extensions.Model;
using DotDoc.EntityFrameworkCore.Extensions.Tests.Data;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Data;

namespace DotDoc.EntityFrameworkCore.Extensions.Tests.QueryExtensions;

/// <summary>
/// Tests for ExecutePagedQuery extensions.
/// </summary>
[TestClass]
public class ExecutePagedQueryTests
{
    #region public methods

    /// <summary>
    /// Test ExecutePagedQuery with FormattableString parameter returning a DataTable.
    /// </summary>
    /// <param name="databaseType">Database type.</param>
    [TestMethod]
    [DataRow(DatabaseType.Sqlite, DisplayName = "SQLite ExecutePagedQueryAsync FormattableString DataTable.")]
    [DataRow(DatabaseType.SqlServer, DisplayName = "SQL Server ExecutePagedQueryAsync FormattableString DataTable.")]
    public void Test_ExecutePagedQuery_FormattableString_DataTable(string databaseType)
    {
        using Context context = DatabaseUtils.CreateDatabase(databaseType);

        const int recordCount = 20;
        const long page = 2;
        const long pageSize = 5;
        string value = DatabaseUtils.GetMethodName();
        DatabaseUtils.CreateMultipleTestTableEntries(context, value, recordCount);
        FormattableString sql = $"SELECT * FROM TestTable1 WHERE ID <= {recordCount}";

        QueryPageTable queryPage = context.Database.ExecutePagedQuery(sql, page, pageSize);

        Assert.AreEqual(page, queryPage.Page, "Invalid page");
        Assert.AreEqual(pageSize, queryPage.PageSize, "Invalid page size");
        Assert.AreEqual(recordCount, queryPage.RecordCount, "Invalid record count");
        Assert.AreEqual((recordCount + pageSize - 1) / pageSize, queryPage.PageCount, "Invalid page count");
        Assert.AreEqual(pageSize, queryPage.Result.Rows.Count, "Invalid data table row count");

        for (int i = 0; i < pageSize; i++)
        {
            Assert.AreEqual((page * pageSize) + i + 1, queryPage.Result.Rows[i]["ID"], $"invalid Id[{i}]");
        }
    }

    /// <summary>
    /// Test ExecutePagedQuery with FormattableString parameter returning a DataTable.
    /// </summary>
    /// <param name="databaseType">Database type.</param>
    /// <returns>A task that represents the asynchronous test operation.</returns>
    [TestMethod]
    [DataRow(DatabaseType.Sqlite, DisplayName = "SQLite ExecutePagedQueryAsync FormattableString DataTable.")]
    [DataRow(DatabaseType.SqlServer, DisplayName = "SQL Server ExecutePagedQueryAsync FormattableString DataTable.")]
    public async Task Test_ExecutePagedQuery_FormattableString_DataTableAsync(string databaseType)
    {
        using Context context = DatabaseUtils.CreateDatabase(databaseType);

        const int recordCount = 20;
        const long page = 2;
        const long pageSize = 5;
        string value = DatabaseUtils.GetMethodName();
        DatabaseUtils.CreateMultipleTestTableEntries(context, value, recordCount);
        FormattableString sql = $"SELECT * FROM TestTable1 WHERE ID <= {recordCount}";

        QueryPageTable queryPage = await context.Database.ExecutePagedQueryAsync(sql, page, pageSize).ConfigureAwait(false);

        Assert.AreEqual(page, queryPage.Page, "Invalid page");
        Assert.AreEqual(pageSize, queryPage.PageSize, "Invalid page size");
        Assert.AreEqual(recordCount, queryPage.RecordCount, "Invalid record count");
        Assert.AreEqual((recordCount + pageSize - 1) / pageSize, queryPage.PageCount, "Invalid page count");
        Assert.AreEqual(pageSize, queryPage.Result.Rows.Count, "Invalid data table row count");

        for (int i = 0; i < pageSize; i++)
        {
            Assert.AreEqual((page * pageSize) + i + 1, queryPage.Result.Rows[i]["ID"], $"invalid Id[{i}]");
        }
    }

    /// <summary>
    /// Test ExecutePagedQuery with FormattableString parameter returning a list of entities.
    /// </summary>
    /// <param name="databaseType">Database type.</param>
    [TestMethod]
    [DataRow(DatabaseType.Sqlite, DisplayName = "SQLite ExecutePagedQueryAsync FormattableString Entity.")]
    [DataRow(DatabaseType.SqlServer, DisplayName = "SQL Server ExecutePagedQueryAsync FormattableString Entity.")]
    public void Test_ExecutePagedQuery_FormattableString_Entity(string databaseType)
    {
        using Context context = DatabaseUtils.CreateDatabase(databaseType);

        const int recordCount = 20;
        const long page = 2;
        const long pageSize = 5;
        string value = DatabaseUtils.GetMethodName();
        DatabaseUtils.CreateMultipleTestTableEntries(context, value, recordCount);
        FormattableString sql = $"SELECT * FROM TestTable1 WHERE ID <= {recordCount}";

        QueryPageEntity<TestTable1> queryPage = context.Database.ExecutePagedQuery<TestTable1>(sql, page, pageSize);

        Assert.AreEqual(page, queryPage.Page, "Invalid page");
        Assert.AreEqual(pageSize, queryPage.PageSize, "Invalid page size");
        Assert.AreEqual(recordCount, queryPage.RecordCount, "Invalid record count");
        Assert.AreEqual((recordCount + pageSize - 1) / pageSize, queryPage.PageCount, "Invalid page count");
        Assert.AreEqual(pageSize, queryPage.Result.Count, "Invalid list row count");

        for (int i = 0; i < pageSize; i++)
        {
            Assert.AreEqual((page * pageSize) + i + 1, queryPage.Result[i].Id, $"invalid Id[{i}]");
        }
    }

    /// <summary>
    /// Test ExecutePagedQuery with FormattableString parameter returning a list of entities.
    /// </summary>
    /// <param name="databaseType">Database type.</param>
    /// <returns>A task that represents the asynchronous test operation.</returns>
    [TestMethod]
    [DataRow(DatabaseType.Sqlite, DisplayName = "SQLite ExecutePagedQueryAsync FormattableString Entity.")]
    [DataRow(DatabaseType.SqlServer, DisplayName = "SQL Server ExecutePagedQueryAsync FormattableString Entity.")]
    public async Task Test_ExecutePagedQuery_FormattableString_EntityAsync(string databaseType)
    {
        using Context context = DatabaseUtils.CreateDatabase(databaseType);

        const int recordCount = 20;
        const long page = 2;
        const long pageSize = 5;
        string value = DatabaseUtils.GetMethodName();
        DatabaseUtils.CreateMultipleTestTableEntries(context, value, recordCount);
        FormattableString sql = $"SELECT * FROM TestTable1 WHERE ID <= {recordCount}";

        QueryPageEntity<TestTable1> queryPage = await context.Database.ExecutePagedQueryAsync<TestTable1>(sql, page, pageSize).ConfigureAwait(false);

        Assert.AreEqual(page, queryPage.Page, "Invalid page");
        Assert.AreEqual(pageSize, queryPage.PageSize, "Invalid page size");
        Assert.AreEqual(recordCount, queryPage.RecordCount, "Invalid record count");
        Assert.AreEqual((recordCount + pageSize - 1) / pageSize, queryPage.PageCount, "Invalid page count");
        Assert.AreEqual(pageSize, queryPage.Result.Count, "Invalid list row count");

        for (int i = 0; i < pageSize; i++)
        {
            Assert.AreEqual((page * pageSize) + i + 1, queryPage.Result[i].Id, $"invalid Id[{i}]");
        }
    }

    /// <summary>
    /// Test ExecutePagedQueryAsync with params parameters returning a DataTable.
    /// </summary>
    /// <param name="databaseType">Database type.</param>
    [TestMethod]
    [DataRow(DatabaseType.Sqlite, DisplayName = "SQLite ExecutePagedQuery params DataTable.")]
    [DataRow(DatabaseType.SqlServer, DisplayName = "SQL Server ExecutePagedQuery params DataTable.")]
    public void Test_ExecutePagedQuery_Params_DataTable(string databaseType)
    {
        using Context context = DatabaseUtils.CreateDatabase(databaseType);

        const int recordCount = 20;
        const long page = 2;
        const long pageSize = 5;
        string value = DatabaseUtils.GetMethodName();
        DatabaseUtils.CreateMultipleTestTableEntries(context, value, recordCount);
        string sql = "SELECT * FROM TestTable1 WHERE ID <= {0}";

        QueryPageTable queryPage = context.Database.ExecutePagedQuery(sql, page, pageSize, recordCount);

        Assert.AreEqual(page, queryPage.Page, "Invalid page");
        Assert.AreEqual(pageSize, queryPage.PageSize, "Invalid page size");
        Assert.AreEqual(recordCount, queryPage.RecordCount, "Invalid record count");
        Assert.AreEqual((recordCount + pageSize - 1) / pageSize, queryPage.PageCount, "Invalid page count");
        Assert.AreEqual(pageSize, queryPage.Result.Rows.Count, "Invalid data table row count");

        for (int i = 0; i < pageSize; i++)
        {
            Assert.AreEqual((page * pageSize) + i + 1, queryPage.Result.Rows[i]["ID"], $"invalid Id[{i}]");
        }
    }

    /// <summary>
    /// Test ExecutePagedQueryAsync with params parameters returning a DataTable.
    /// </summary>
    /// <param name="databaseType">Database type.</param>
    /// <returns>A task that represents the asynchronous test operation.</returns>
    [TestMethod]
    [DataRow(DatabaseType.Sqlite, DisplayName = "SQLite ExecutePagedQuery params DataTable.")]
    [DataRow(DatabaseType.SqlServer, DisplayName = "SQL Server ExecutePagedQuery params DataTable.")]
    public async Task Test_ExecutePagedQuery_Params_DataTableAsync(string databaseType)
    {
        using Context context = DatabaseUtils.CreateDatabase(databaseType);

        const int recordCount = 20;
        const long page = 2;
        const long pageSize = 5;
        string value = DatabaseUtils.GetMethodName();
        DatabaseUtils.CreateMultipleTestTableEntries(context, value, recordCount);
        string sql = "SELECT * FROM TestTable1 WHERE ID <= {0}";

        QueryPageTable queryPage = await context.Database.ExecutePagedQueryAsync(sql, page, pageSize, parameters: recordCount).ConfigureAwait(false);

        Assert.AreEqual(page, queryPage.Page, "Invalid page");
        Assert.AreEqual(pageSize, queryPage.PageSize, "Invalid page size");
        Assert.AreEqual(recordCount, queryPage.RecordCount, "Invalid record count");
        Assert.AreEqual((recordCount + pageSize - 1) / pageSize, queryPage.PageCount, "Invalid page count");
        Assert.AreEqual(pageSize, queryPage.Result.Rows.Count, "Invalid data table row count");

        for (int i = 0; i < pageSize; i++)
        {
            Assert.AreEqual((page * pageSize) + i + 1, queryPage.Result.Rows[i]["ID"], $"invalid Id[{i}]");
        }
    }

    /// <summary>
    /// Test ExecutePagedQueryAsync with params parameters returning a list of entities.
    /// </summary>
    /// <param name="databaseType">Database type.</param>
    [TestMethod]
    [DataRow(DatabaseType.Sqlite, DisplayName = "SQLite ExecutePagedQuery params Entity.")]
    [DataRow(DatabaseType.SqlServer, DisplayName = "SQL Server ExecutePagedQuery params Entity.")]
    public void Test_ExecutePagedQuery_Params_Entity(string databaseType)
    {
        using Context context = DatabaseUtils.CreateDatabase(databaseType);

        const int recordCount = 20;
        const long page = 2;
        const long pageSize = 5;
        string value = DatabaseUtils.GetMethodName();
        DatabaseUtils.CreateMultipleTestTableEntries(context, value, recordCount);
        string sql = "SELECT * FROM TestTable1 WHERE ID <= {0}";

        QueryPageEntity<TestTable1> queryPage = context.Database.ExecutePagedQuery<TestTable1>(sql, page, pageSize, recordCount);

        Assert.AreEqual(page, queryPage.Page, "Invalid page");
        Assert.AreEqual(pageSize, queryPage.PageSize, "Invalid page size");
        Assert.AreEqual(recordCount, queryPage.RecordCount, "Invalid record count");
        Assert.AreEqual((recordCount + pageSize - 1) / pageSize, queryPage.PageCount, "Invalid page count");
        Assert.AreEqual(pageSize, queryPage.Result.Count, "Invalid list row count");

        for (int i = 0; i < pageSize; i++)
        {
            Assert.AreEqual((page * pageSize) + i + 1, queryPage.Result[i].Id, $"invalid Id[{i}]");
        }
    }

    /// <summary>
    /// Test ExecutePagedQueryAsync with params parameters returning a list of entities.
    /// </summary>
    /// <param name="databaseType">Database type.</param>
    /// <returns>A task that represents the asynchronous test operation.</returns>
    [TestMethod]
    [DataRow(DatabaseType.Sqlite, DisplayName = "SQLite ExecutePagedQuery params Entity.")]
    [DataRow(DatabaseType.SqlServer, DisplayName = "SQL Server ExecutePagedQuery params Entity.")]
    public async Task Test_ExecutePagedQuery_Params_EntityAsync(string databaseType)
    {
        using Context context = DatabaseUtils.CreateDatabase(databaseType);

        const int recordCount = 20;
        const long page = 2;
        const long pageSize = 5;
        string value = DatabaseUtils.GetMethodName();
        DatabaseUtils.CreateMultipleTestTableEntries(context, value, recordCount);
        string sql = "SELECT * FROM TestTable1 WHERE ID <= {0}";

        QueryPageEntity<TestTable1> queryPage = await context.Database.ExecutePagedQueryAsync<TestTable1>(sql, page, pageSize, parameters: recordCount).ConfigureAwait(false);

        Assert.AreEqual(page, queryPage.Page, "Invalid page");
        Assert.AreEqual(pageSize, queryPage.PageSize, "Invalid page size");
        Assert.AreEqual(recordCount, queryPage.RecordCount, "Invalid record count");
        Assert.AreEqual((recordCount + pageSize - 1) / pageSize, queryPage.PageCount, "Invalid page count");
        Assert.AreEqual(pageSize, queryPage.Result.Count, "Invalid list row count");

        for (int i = 0; i < pageSize; i++)
        {
            Assert.AreEqual((page * pageSize) + i + 1, queryPage.Result[i].Id, $"invalid Id[{i}]");
        }
    }

    /// <summary>
    /// Test the code that splits SQL queries and handles Order By clauses returning a DataTable.
    /// </summary>
    /// <param name="databaseType">Database type.</param>
    [TestMethod]
    [DataRow(DatabaseType.Sqlite, DisplayName = "SQLite ExecutePagedQuery order by DataTable.")]
    [DataRow(DatabaseType.SqlServer, DisplayName = "SQL Server ExecutePagedQuery order by DataTable.")]
    public void Test_ExecutePagedQuery_OrderBy_DataTable(string databaseType)
    {
        string value = DatabaseUtils.GetMethodName();
        const int recordCount = 20;
        const long page = 2;
        const long pageSize = 5;

        using Context context = DatabaseUtils.CreateDatabase(databaseType);
        DatabaseUtils.CreateMultipleTestTableEntries(context, value, recordCount);

        QueryPageTable queryPage = context.Database.ExecutePagedQuery($"SELECT * FROM TestTable1 WHERE ID <= {recordCount} ORDER BY ID", page, pageSize);

        Assert.AreEqual(page, queryPage.Page, "Invalid page");
    }

    /// <summary>
    /// Test the code that splits SQL queries and handles Order By clauses returning a DataTable.
    /// </summary>
    /// <param name="databaseType">Database type.</param>
    /// <returns>A task that represents the asynchronous test operation.</returns>
    [TestMethod]
    [DataRow(DatabaseType.Sqlite, DisplayName = "SQLite ExecutePagedQuery order by DataTable.")]
    [DataRow(DatabaseType.SqlServer, DisplayName = "SQL Server ExecutePagedQuery order by DataTable.")]
    public async Task Test_ExecutePagedQuery_OrderBy_DataTableAsync(string databaseType)
    {
        string value = DatabaseUtils.GetMethodName();
        const int recordCount = 20;
        const long page = 2;
        const long pageSize = 5;

        using Context context = DatabaseUtils.CreateDatabase(databaseType);
        DatabaseUtils.CreateMultipleTestTableEntries(context, value, recordCount);

        QueryPageTable queryPage = await context.Database.ExecutePagedQueryAsync($"SELECT * FROM TestTable1 WHERE ID <= {recordCount} ORDER BY ID", page, pageSize).ConfigureAwait(false);

        Assert.AreEqual(page, queryPage.Page, "Invalid page");
    }

    /// <summary>
    /// Test the code that splits SQL queries and handles Order By clauses returning a list of entities.
    /// </summary>
    /// <param name="databaseType">Database type.</param>
    [TestMethod]
    [DataRow(DatabaseType.Sqlite, DisplayName = "SQLite ExecutePagedQuery order by Entity.")]
    [DataRow(DatabaseType.SqlServer, DisplayName = "SQL Server ExecutePagedQuery order by Entity.")]
    public void Test_ExecutePagedQuery_OrderBy_Entity(string databaseType)
    {
        string value = DatabaseUtils.GetMethodName();
        const int recordCount = 20;
        const long page = 2;
        const long pageSize = 5;

        using Context context = DatabaseUtils.CreateDatabase(databaseType);
        DatabaseUtils.CreateMultipleTestTableEntries(context, value, recordCount);

        QueryPageEntity<TestTable1> queryPage = context.Database.ExecutePagedQuery<TestTable1>($"SELECT * FROM TestTable1 WHERE ID <= {recordCount} ORDER BY ID", page, pageSize);

        Assert.AreEqual(page, queryPage.Page, "Invalid page");
    }

    /// <summary>
    /// Test the code that splits SQL queries and handles Order By clauses returning a list of entities.
    /// </summary>
    /// <param name="databaseType">Database type.</param>
    /// <returns>A task that represents the asynchronous test operation.</returns>
    [TestMethod]
    [DataRow(DatabaseType.Sqlite, DisplayName = "SQLite ExecutePagedQuery order by Entity.")]
    [DataRow(DatabaseType.SqlServer, DisplayName = "SQL Server ExecutePagedQuery order by Entity.")]
    public async Task Test_ExecutePagedQuery_OrderBy_EntityAsync(string databaseType)
    {
        string value = DatabaseUtils.GetMethodName();
        const int recordCount = 20;
        const long page = 2;
        const long pageSize = 5;

        using Context context = DatabaseUtils.CreateDatabase(databaseType);
        DatabaseUtils.CreateMultipleTestTableEntries(context, value, recordCount);

        QueryPageEntity<TestTable1> queryPage = await context.Database.ExecutePagedQueryAsync<TestTable1>($"SELECT * FROM TestTable1 WHERE ID <= {recordCount} ORDER BY ID", page, pageSize).ConfigureAwait(false);

        Assert.AreEqual(page, queryPage.Page, "Invalid page");
    }

    /// <summary>
    /// Test if the requested page number is greater than the total number of pages returning a DataTable.
    /// The page number should be reduced to the last one.
    /// </summary>
    /// <param name="databaseType">Database type.</param>
    [TestMethod]
    [DataRow(DatabaseType.Sqlite, DisplayName = "SQLite ExecutePagedQuery page number overflow DataTable.")]
    [DataRow(DatabaseType.SqlServer, DisplayName = "SQL Server ExecutePagedQuery page number overflow DataTable.")]
    public void Test_ExecutePagedQuery_PageNumber_DataTableOverflow(string databaseType)
    {
        string value = DatabaseUtils.GetMethodName();
        const int recordCount = 20;
        const long page = 999;
        const long pageSize = 5;

        using Context context = DatabaseUtils.CreateDatabase(databaseType);
        DatabaseUtils.CreateMultipleTestTableEntries(context, value, recordCount);

        QueryPageTable queryPage = context.Database.ExecutePagedQuery($"SELECT * FROM TestTable1 WHERE ID <= {recordCount}", page, pageSize);

        Assert.AreEqual(((recordCount + pageSize - 1) / pageSize) - 1, queryPage.Page, "Invalid page");
    }

    /// <summary>
    /// Test if the requested page number is greater than the total number of pages returning a DataTable.
    /// The page number should be reduced to the last one.
    /// </summary>
    /// <param name="databaseType">Database type.</param>
    /// <returns>A task that represents the asynchronous test operation.</returns>
    [TestMethod]
    [DataRow(DatabaseType.Sqlite, DisplayName = "SQLite ExecutePagedQuery page number overflow DataTable.")]
    [DataRow(DatabaseType.SqlServer, DisplayName = "SQL Server ExecutePagedQuery page number overflow DataTable.")]
    public async Task Test_ExecutePagedQuery_PageNumberOverflow_DataTableAsync(string databaseType)
    {
        string value = DatabaseUtils.GetMethodName();
        const int recordCount = 20;
        const long page = 999;
        const long pageSize = 5;

        using Context context = DatabaseUtils.CreateDatabase(databaseType);
        DatabaseUtils.CreateMultipleTestTableEntries(context, value, recordCount);

        QueryPageTable queryPage = await context.Database.ExecutePagedQueryAsync($"SELECT * FROM TestTable1 WHERE ID <= {recordCount}", page, pageSize).ConfigureAwait(false);

        Assert.AreEqual(((recordCount + pageSize - 1) / pageSize) - 1, queryPage.Page, "Invalid page");
    }

    /// <summary>
    /// Test if the requested page number is greater than the total number of pages returning a list of entities.
    /// The page number should be reduced to the last one.
    /// </summary>
    /// <param name="databaseType">Database type.</param>
    [TestMethod]
    [DataRow(DatabaseType.Sqlite, DisplayName = "SQLite ExecutePagedQuery page number overflow Entity.")]
    [DataRow(DatabaseType.SqlServer, DisplayName = "SQL Server ExecutePagedQuery page number overflow Entity.")]
    public void Test_ExecutePagedQuery_PageNumberOverflow_Entity(string databaseType)
    {
        string value = DatabaseUtils.GetMethodName();
        const int recordCount = 20;
        const long page = 999;
        const long pageSize = 5;

        using Context context = DatabaseUtils.CreateDatabase(databaseType);
        DatabaseUtils.CreateMultipleTestTableEntries(context, value, recordCount);

        QueryPageEntity<TestTable1> queryPage = context.Database.ExecutePagedQuery<TestTable1>($"SELECT * FROM TestTable1 WHERE ID <= {recordCount}", page, pageSize);

        Assert.AreEqual(((recordCount + pageSize - 1) / pageSize) - 1, queryPage.Page, "Invalid page");
    }

    /// <summary>
    /// Test if the requested page number is greater than the total number of pages returning a list of entities.
    /// The page number should be reduced to the last one.
    /// </summary>
    /// <param name="databaseType">Database type.</param>
    /// <returns>A task that represents the asynchronous test operation.</returns>
    [TestMethod]
    [DataRow(DatabaseType.Sqlite, DisplayName = "SQLite ExecutePagedQuery page number overflow Entity.")]
    [DataRow(DatabaseType.SqlServer, DisplayName = "SQL Server ExecutePagedQuery page number overflow Entity.")]
    public async Task Test_ExecutePagedQuery_PageNumberOverflow_EntityAsync(string databaseType)
    {
        string value = DatabaseUtils.GetMethodName();
        const int recordCount = 20;
        const long page = 999;
        const long pageSize = 5;

        using Context context = DatabaseUtils.CreateDatabase(databaseType);
        DatabaseUtils.CreateMultipleTestTableEntries(context, value, recordCount);

        QueryPageEntity<TestTable1> queryPage = await context.Database.ExecutePagedQueryAsync<TestTable1>($"SELECT * FROM TestTable1 WHERE ID <= {recordCount}", page, pageSize).ConfigureAwait(false);

        Assert.AreEqual(((recordCount + pageSize - 1) / pageSize) - 1, queryPage.Page, "Invalid page");
    }

    #endregion public methods
}