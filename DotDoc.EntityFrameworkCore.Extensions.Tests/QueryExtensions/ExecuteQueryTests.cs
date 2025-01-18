// Copyright ©2021-2025 Mike King.
// This file is licensed to you under the MIT license.
// See the License.txt file in the solution root for more information.

using DotDoc.EntityFrameworkCore.Extensions.Constants;
using DotDoc.EntityFrameworkCore.Extensions.Extensions;
using DotDoc.EntityFrameworkCore.Extensions.Tests.Data;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Data;

namespace DotDoc.EntityFrameworkCore.Extensions.Tests.QueryExtensions;

/// <summary>
/// Tests for ExecuteQuery extensions.
/// </summary>
[TestClass]
public class ExecuteQueryTests
{
    #region public methods

    /// <summary>
    /// Test ExecuteQuery with FormattableString parameter returning a DataTable.
    /// </summary>
    /// <param name="databaseType">Database type.</param>
    [TestMethod]
    [DataRow(DatabaseType.Sqlite, DisplayName = "SQLite ExecuteQuery FormattableString DataTable.")]
    [DataRow(DatabaseType.SqlServer, DisplayName = "SQL Server ExecuteQuery FormattableString DataTable.")]
    public void Test_ExecuteQuery_FormattableString_DataTable(string databaseType)
    {
        using Context context = DatabaseUtils.CreateDatabase(databaseType);

        const int recordCount = 20;
        string value = DatabaseUtils.GetMethodName();
        DatabaseUtils.CreateMultipleTestTableEntries(context, value, recordCount);
        FormattableString sql = $"SELECT * FROM TestTable1 WHERE ID <= {recordCount}";

        DataTable dataTable = context.Database.ExecuteQuery(sql);

        Assert.AreEqual(recordCount, dataTable.Rows.Count, "Invalid record count");
    }

    /// <summary>
    /// Test ExecuteQuery with FormattableString parameter returning a DataTable.
    /// </summary>
    /// <param name="databaseType">Database type.</param>
    /// <returns>A task that represents the asynchronous test operation.</returns>
    [TestMethod]
    [DataRow(DatabaseType.Sqlite, DisplayName = "SQLite ExecuteQuery FormattableString DataTable.")]
    [DataRow(DatabaseType.SqlServer, DisplayName = "SQL Server ExecuteQuery FormattableString DataTable.")]
    public async Task Test_ExecuteQuery_FormattableString_DataTableAsync(string databaseType)
    {
        using Context context = DatabaseUtils.CreateDatabase(databaseType);

        const int recordCount = 20;
        string value = DatabaseUtils.GetMethodName();
        DatabaseUtils.CreateMultipleTestTableEntries(context, value, recordCount);
        FormattableString sql = $"SELECT * FROM TestTable1 WHERE ID <= {recordCount}";

        DataTable dataTable = await context.Database.ExecuteQueryAsync(sql).ConfigureAwait(false);

        Assert.AreEqual(recordCount, dataTable.Rows.Count, "Invalid record count");
    }

    /// <summary>
    /// Test ExecuteQuery with FormattableString parameter returning a list of entities.
    /// </summary>
    /// <param name="databaseType">Database type.</param>
    [TestMethod]
    [DataRow(DatabaseType.Sqlite, DisplayName = "SQLite ExecuteQuery FormattableString Entity.")]
    [DataRow(DatabaseType.SqlServer, DisplayName = "SQL Server ExecuteQuery FormattableString Entity.")]
    public void Test_ExecuteQuery_FormattableString_Entity(string databaseType)
    {
        using Context context = DatabaseUtils.CreateDatabase(databaseType);

        const int recordCount = 20;
        string value = DatabaseUtils.GetMethodName();
        DatabaseUtils.CreateMultipleTestTableEntries(context, value, recordCount);
        FormattableString sql = $"SELECT * FROM TestTable1 WHERE ID <= {recordCount}";

        IList<TestTable1> results = context.Database.ExecuteQuery<TestTable1>(sql);

        Assert.AreEqual(recordCount, results.Count, "Invalid record count");
    }

    /// <summary>
    /// Test ExecuteQuery with FormattableString parameter returning a list of entities.
    /// </summary>
    /// <param name="databaseType">Database type.</param>
    /// <returns>A task that represents the asynchronous test operation.</returns>
    [TestMethod]
    [DataRow(DatabaseType.Sqlite, DisplayName = "SQLite ExecuteQuery FormattableString Entity.")]
    [DataRow(DatabaseType.SqlServer, DisplayName = "SQL Server ExecuteQuery FormattableString Entity.")]
    public async Task Test_ExecuteQuery_FormattableString_EntityAsync(string databaseType)
    {
        using Context context = DatabaseUtils.CreateDatabase(databaseType);

        const int recordCount = 20;
        string value = DatabaseUtils.GetMethodName();
        DatabaseUtils.CreateMultipleTestTableEntries(context, value, recordCount);
        FormattableString sql = $"SELECT * FROM TestTable1 WHERE ID <= {recordCount}";

        IList<TestTable1> results = await context.Database.ExecuteQueryAsync<TestTable1>(sql).ConfigureAwait(false);

        Assert.AreEqual(recordCount, results.Count, "Invalid record count");
    }

    /// <summary>
    /// Test ExecuteQuery with params parameters returning a DataTable.
    /// </summary>
    /// <param name="databaseType">Database type.</param>
    [TestMethod]
    [DataRow(DatabaseType.Sqlite, DisplayName = "SQLite ExecuteQuery params DataTable.")]
    [DataRow(DatabaseType.SqlServer, DisplayName = "SQL Server ExecuteQuery params DataTable.")]
    public void Test_ExecuteQuery_Params_DataTable(string databaseType)
    {
        using Context context = DatabaseUtils.CreateDatabase(databaseType);

        const int recordCount = 20;
        string value = DatabaseUtils.GetMethodName();
        DatabaseUtils.CreateMultipleTestTableEntries(context, value, recordCount);
        string sql = "SELECT * FROM TestTable1 WHERE ID <= {0}";

        DataTable dataTable = context.Database.ExecuteQuery(sql, recordCount);

        Assert.AreEqual(recordCount, dataTable.Rows.Count, "Invalid record count");
    }

    /// <summary>
    /// Test ExecuteQuery with params parameters returning a DataTable.
    /// </summary>
    /// <param name="databaseType">Database type.</param>
    /// <returns>A task that represents the asynchronous test operation.</returns>
    [TestMethod]
    [DataRow(DatabaseType.Sqlite, DisplayName = "SQLite ExecuteQuery params DataTable.")]
    [DataRow(DatabaseType.SqlServer, DisplayName = "SQL Server ExecuteQuery params DataTable.")]
    public async Task Test_ExecuteQuery_Params_DataTableAsync(string databaseType)
    {
        using Context context = DatabaseUtils.CreateDatabase(databaseType);

        const int recordCount = 20;
        string value = DatabaseUtils.GetMethodName();
        DatabaseUtils.CreateMultipleTestTableEntries(context, value, recordCount);
        string sql = "SELECT * FROM TestTable1 WHERE ID <= {0}";

        DataTable dataTable = await context.Database.ExecuteQueryAsync(sql, parameters: recordCount).ConfigureAwait(false);

        Assert.AreEqual(recordCount, dataTable.Rows.Count, "Invalid record count");
    }

    /// <summary>
    /// Test ExecuteQuery with params parameters returning a list of entities.
    /// </summary>
    /// <param name="databaseType">Database type.</param>
    [TestMethod]
    [DataRow(DatabaseType.Sqlite, DisplayName = "SQLite ExecuteQuery params Entity.")]
    [DataRow(DatabaseType.SqlServer, DisplayName = "SQL Server ExecuteQuery params Entity.")]
    public void Test_ExecuteQuery_Params_Entity(string databaseType)
    {
        using Context context = DatabaseUtils.CreateDatabase(databaseType);

        const int recordCount = 20;
        string value = DatabaseUtils.GetMethodName();
        DatabaseUtils.CreateMultipleTestTableEntries(context, value, recordCount);
        string sql = "SELECT * FROM TestTable1 WHERE ID <= {0}";

        IList<TestTable1> results = context.Database.ExecuteQuery<TestTable1>(sql, parameters: recordCount);

        Assert.AreEqual(recordCount, results.Count, "Invalid record count");
    }

    /// <summary>
    /// Test ExecuteQuery with params parameters returning a list of entities.
    /// </summary>
    /// <param name="databaseType">Database type.</param>
    /// <returns>A task that represents the asynchronous test operation.</returns>
    [TestMethod]
    [DataRow(DatabaseType.Sqlite, DisplayName = "SQLite ExecuteQuery params Entity.")]
    [DataRow(DatabaseType.SqlServer, DisplayName = "SQL Server ExecuteQuery params Entity.")]
    public async Task Test_ExecuteQuery_Params_EntityAsync(string databaseType)
    {
        using Context context = DatabaseUtils.CreateDatabase(databaseType);

        const int recordCount = 20;
        string value = DatabaseUtils.GetMethodName();
        DatabaseUtils.CreateMultipleTestTableEntries(context, value, recordCount);
        string sql = "SELECT * FROM TestTable1 WHERE ID <= {0}";

        IList<TestTable1> results = await context.Database.ExecuteQueryAsync<TestTable1>(sql, parameters: recordCount).ConfigureAwait(false);

        Assert.AreEqual(recordCount, results.Count, "Invalid record count");
    }

    #endregion public methods
}
