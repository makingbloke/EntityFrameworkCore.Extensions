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
    /// <summary>
    /// Test ExecuteQuery with FormattableString parameter.
    /// </summary>
    /// <param name="databaseType">Database type.</param>
    [TestMethod]
    [DataRow(DatabaseType.Sqlite, DisplayName = "SQLite ExecuteQuery FormattableString.")]
    [DataRow(DatabaseType.SqlServer, DisplayName = "SQL Server ExecuteQuery FormattableString.")]
    public void Test_ExecuteQuery_FormattableString(DatabaseType databaseType)
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
    /// Test ExecuteQuery with FormattableString parameter.
    /// </summary>
    /// <param name="databaseType">Database type.</param>
    /// <returns>A task that represents the asynchronous test operation.</returns>
    [TestMethod]
    [DataRow(DatabaseType.Sqlite, DisplayName = "SQLite ExecuteQuery FormattableString.")]
    [DataRow(DatabaseType.SqlServer, DisplayName = "SQL Server ExecuteQuery FormattableString.")]
    public async Task Test_ExecuteQuery_FormattableStringAsync(DatabaseType databaseType)
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
    /// Test ExecuteQuery with params parameters.
    /// </summary>
    /// <param name="databaseType">Database type.</param>
    [TestMethod]
    [DataRow(DatabaseType.Sqlite, DisplayName = "SQLite ExecuteQuery params.")]
    [DataRow(DatabaseType.SqlServer, DisplayName = "SQL Server ExecuteQuery params.")]
    public void Test_ExecuteQuery_Params(DatabaseType databaseType)
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
    /// Test ExecuteQuery with params parameters.
    /// </summary>
    /// <param name="databaseType">Database type.</param>
    /// <returns>A task that represents the asynchronous test operation.</returns>
    [TestMethod]
    [DataRow(DatabaseType.Sqlite, DisplayName = "SQLite ExecuteQuery params.")]
    [DataRow(DatabaseType.SqlServer, DisplayName = "SQL Server ExecuteQuery params.")]
    public async Task Test_ExecuteQuery_ParamsAsync(DatabaseType databaseType)
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
    /// Test ExecuteQuery with IEnumerable parameter.
    /// </summary>
    /// <param name="databaseType">Database type.</param>
    [TestMethod]
    [DataRow(DatabaseType.Sqlite, DisplayName = "SQLite ExecuteQuery IEnumerable.")]
    [DataRow(DatabaseType.SqlServer, DisplayName = "SQL Server ExecuteQuery IEnumerable.")]
    public void Test_ExecuteQuery_IEnumerable(DatabaseType databaseType)
    {
        using Context context = DatabaseUtils.CreateDatabase(databaseType);

        const int recordCount = 20;
        string value = DatabaseUtils.GetMethodName();
        DatabaseUtils.CreateMultipleTestTableEntries(context, value, recordCount);
        List<object> parameters = [recordCount];
        string sql = "SELECT * FROM TestTable1 WHERE ID <= {0}";

        DataTable dataTable = context.Database.ExecuteQuery(sql, parameters);

        Assert.AreEqual(recordCount, dataTable.Rows.Count, "Invalid record count");
    }

    /// <summary>
    /// Test ExecuteQuery with IEnumerable parameter.
    /// </summary>
    /// <param name="databaseType">Database type.</param>
    /// <returns>A task that represents the asynchronous test operation.</returns>
    [TestMethod]
    [DataRow(DatabaseType.Sqlite, DisplayName = "SQLite ExecuteQuery IEnumerable.")]
    [DataRow(DatabaseType.SqlServer, DisplayName = "SQL Server ExecuteQuery IEnumerable.")]
    public async Task Test_ExecuteQuery_IEnumerableAsync(DatabaseType databaseType)
    {
        using Context context = DatabaseUtils.CreateDatabase(databaseType);

        const int recordCount = 20;
        string value = DatabaseUtils.GetMethodName();
        DatabaseUtils.CreateMultipleTestTableEntries(context, value, recordCount);
        List<object> parameters = [recordCount];
        string sql = "SELECT * FROM TestTable1 WHERE ID <= {0}";

        DataTable dataTable = await context.Database.ExecuteQueryAsync(sql, parameters).ConfigureAwait(false);

        Assert.AreEqual(recordCount, dataTable.Rows.Count, "Invalid record count");
    }
}
