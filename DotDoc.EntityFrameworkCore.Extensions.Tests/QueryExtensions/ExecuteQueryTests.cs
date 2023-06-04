// Copyright ©2021-2023 Mike King.
// This file is licensed to you under the MIT license.
// See the License.txt file in the solution root for more information.

using DotDoc.EntityFrameworkCore.Extensions.Constants;
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
    /// <param name="useAsync">If <see langword="true"/> then tests the async method.</param>
    /// <returns>A task that represents the asynchronous test operation.</returns>
    [TestMethod]
    [DataRow(DatabaseType.Sqlite, false, DisplayName = "SQLite ExecuteQuery FormattableString.")]
    [DataRow(DatabaseType.Sqlite, true, DisplayName = "SQLite ExecuteQueryAsync FormattableString.")]
    [DataRow(DatabaseType.SqlServer, false, DisplayName = "SQL Server ExecuteQuery FormattableString.")]
    [DataRow(DatabaseType.SqlServer, true, DisplayName = "SQL Server ExecuteQueryAsync FormattableString.")]
    public async Task TestExecuteQueryWithFormattableStringAsync(DatabaseType databaseType, bool useAsync)
    {
        using Context context = DatabaseUtils.CreateDatabase(databaseType);

        const int recordCount = 20;
        string value = DatabaseUtils.GetMethodName();
        DatabaseUtils.CreateMultipleTestTableEntries(context, value, recordCount);
        FormattableString sql = $"SELECT * FROM TestTable1 WHERE ID <= {recordCount}";

        DataTable dataTable = useAsync
            ? await context.Database.ExecuteQueryAsync(sql).ConfigureAwait(false)
            : context.Database.ExecuteQuery(sql);

        Assert.AreEqual(recordCount, dataTable.Rows.Count, "Invalid record count");
    }

    /// <summary>
    /// Test ExecuteQuery with params parameters.
    /// </summary>
    /// <param name="databaseType">Database type.</param>
    /// <param name="useAsync">If <see langword="true"/> then tests the async method.</param>
    /// <returns>A task that represents the asynchronous test operation.</returns>
    [TestMethod]
    [DataRow(DatabaseType.Sqlite, false, DisplayName = "SQLite ExecuteQuery params.")]
    [DataRow(DatabaseType.Sqlite, true, DisplayName = "SQLite ExecuteQueryAsync params.")]
    [DataRow(DatabaseType.SqlServer, false, DisplayName = "SQL Server ExecuteQuery params.")]
    [DataRow(DatabaseType.SqlServer, true, DisplayName = "SQL Server ExecuteQueryAsync params.")]
    public async Task TestExecuteQueryWithParamsAsync(DatabaseType databaseType, bool useAsync)
    {
        using Context context = DatabaseUtils.CreateDatabase(databaseType);

        const int recordCount = 20;
        string value = DatabaseUtils.GetMethodName();
        DatabaseUtils.CreateMultipleTestTableEntries(context, value, recordCount);
        string sql = "SELECT * FROM TestTable1 WHERE ID <= {0}";

        DataTable dataTable = useAsync
            ? await context.Database.ExecuteQueryAsync(sql, parameters: recordCount).ConfigureAwait(false)
            : context.Database.ExecuteQuery(sql, recordCount);

        Assert.AreEqual(recordCount, dataTable.Rows.Count, "Invalid record count");
    }

    /// <summary>
    /// Test ExecuteQuery with IEnumerable parameter.
    /// </summary>
    /// <param name="databaseType">Database type.</param>
    /// <param name="useAsync">If <see langword="true"/> then tests the async method.</param>
    /// <returns>A task that represents the asynchronous test operation.</returns>
    [TestMethod]
    [DataRow(DatabaseType.Sqlite, false, DisplayName = "SQLite ExecuteQuery IEnumerable.")]
    [DataRow(DatabaseType.Sqlite, true, DisplayName = "SQLite ExecuteQueryAsync IEnumerable.")]
    [DataRow(DatabaseType.SqlServer, false, DisplayName = "SQL Server ExecuteQuery IEnumerable.")]
    [DataRow(DatabaseType.SqlServer, true, DisplayName = "SQL Server ExecuteQueryAsync IEnumerable.")]
    public async Task TestExecuteQueryWithIEnumerableAsync(DatabaseType databaseType, bool useAsync)
    {
        using Context context = DatabaseUtils.CreateDatabase(databaseType);

        const int recordCount = 20;
        string value = DatabaseUtils.GetMethodName();
        DatabaseUtils.CreateMultipleTestTableEntries(context, value, recordCount);
        List<object> parameters = new () { recordCount };
        string sql = "SELECT * FROM TestTable1 WHERE ID <= {0}";

        DataTable dataTable = useAsync
            ? await context.Database.ExecuteQueryAsync(sql, parameters).ConfigureAwait(false)
            : context.Database.ExecuteQuery(sql, parameters);

        Assert.AreEqual(recordCount, dataTable.Rows.Count, "Invalid record count");
    }
}
