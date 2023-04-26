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
    /// Test ExecuteQueryInterpolated.
    /// </summary>
    /// <param name="databaseType">Database type.</param>
    /// <param name="useAsync">If <see langword="true"/> then tests the async method.</param>
    /// <returns>A task that represents the asynchronous test operation.</returns>
    [TestMethod]
    [DataRow(DatabaseType.Sqlite, false, DisplayName = "SQLite ExecuteQueryInterpolated.")]
    [DataRow(DatabaseType.Sqlite, true, DisplayName = "SQLite ExecuteQueryInterpolatedAsync.")]
    [DataRow(DatabaseType.SqlServer, false, DisplayName = "SQL Server ExecuteQueryInterpolated.")]
    [DataRow(DatabaseType.SqlServer, true, DisplayName = "SQL Server ExecuteQueryInterpolatedAsync.")]
    public async Task TestExecuteQueryInterpolatedAsync(DatabaseType databaseType, bool useAsync)
    {
        using Context context = DatabaseUtils.CreateDatabase(databaseType);

        const int recordCount = 20;
        string value = DatabaseUtils.GetMethodName();
        DatabaseUtils.CreateMultipleTestTableEntries(context, value, recordCount);
        FormattableString sql = $"SELECT * FROM TestTable1 WHERE ID <= {recordCount}";

        DataTable dataTable = useAsync
            ? await context.Database.ExecuteQueryInterpolatedAsync(sql).ConfigureAwait(false)
            : context.Database.ExecuteQueryInterpolated(sql);

        Assert.AreEqual(recordCount, dataTable.Rows.Count, "Invalid record count");
    }

    /// <summary>
    /// Test ExecuteQueryRaw with params parameters.
    /// </summary>
    /// <param name="databaseType">Database type.</param>
    /// <param name="useAsync">If <see langword="true"/> then tests the async method.</param>
    /// <returns>A task that represents the asynchronous test operation.</returns>
    [TestMethod]
    [DataRow(DatabaseType.Sqlite, false, DisplayName = "SQLite ExecuteQueryRaw params.")]
    [DataRow(DatabaseType.Sqlite, true, DisplayName = "SQLite ExecuteQueryRawAsync params.")]
    [DataRow(DatabaseType.SqlServer, false, DisplayName = "SQL Server ExecuteQueryRaw params.")]
    [DataRow(DatabaseType.SqlServer, true, DisplayName = "SQL Server ExecuteQueryRawAsync params.")]
    public async Task TestExecuteQueryRawWithParamsAsync(DatabaseType databaseType, bool useAsync)
    {
        using Context context = DatabaseUtils.CreateDatabase(databaseType);

        const int recordCount = 20;
        string value = DatabaseUtils.GetMethodName();
        DatabaseUtils.CreateMultipleTestTableEntries(context, value, recordCount);
        string sql = "SELECT * FROM TestTable1 WHERE ID <= {0}";

        DataTable dataTable = useAsync
            ? await context.Database.ExecuteQueryRawAsync(sql, parameters: recordCount).ConfigureAwait(false)
            : context.Database.ExecuteQueryRaw(sql, recordCount);

        Assert.AreEqual(recordCount, dataTable.Rows.Count, "Invalid record count");
    }

    /// <summary>
    /// Test ExecuteQueryRaw with IEnumerable parameter.
    /// </summary>
    /// <param name="databaseType">Database type.</param>
    /// <param name="useAsync">If <see langword="true"/> then tests the async method.</param>
    /// <returns>A task that represents the asynchronous test operation.</returns>
    [TestMethod]
    [DataRow(DatabaseType.Sqlite, false, DisplayName = "SQLite ExecuteQueryRaw IEnumerable.")]
    [DataRow(DatabaseType.Sqlite, true, DisplayName = "SQLite ExecuteQueryRawAsync IEnumerable.")]
    [DataRow(DatabaseType.SqlServer, false, DisplayName = "SQL Server ExecuteQueryRaw IEnumerable.")]
    [DataRow(DatabaseType.SqlServer, true, DisplayName = "SQL Server ExecuteQueryRawAsync IEnumerable.")]
    public async Task TestExecuteQueryRawWithIEnumerableAsync(DatabaseType databaseType, bool useAsync)
    {
        using Context context = DatabaseUtils.CreateDatabase(databaseType);

        const int recordCount = 20;
        string value = DatabaseUtils.GetMethodName();
        DatabaseUtils.CreateMultipleTestTableEntries(context, value, recordCount);
        List<object> parameters = new () { recordCount };
        string sql = "SELECT * FROM TestTable1 WHERE ID <= {0}";

        DataTable dataTable = useAsync
            ? await context.Database.ExecuteQueryRawAsync(sql, parameters).ConfigureAwait(false)
            : context.Database.ExecuteQueryRaw(sql, parameters);

        Assert.AreEqual(recordCount, dataTable.Rows.Count, "Invalid record count");
    }
}
