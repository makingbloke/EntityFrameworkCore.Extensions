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
    /// <returns><see cref="Task"/>.</returns>
    [TestMethod]
    [DataRow(DatabaseType.Sqlite, false, DisplayName = "SQLite ExecuteQueryInterpolated")]
    [DataRow(DatabaseType.Sqlite, true, DisplayName = "SQLite ExecuteQueryInterpolatedAsync")]
    [DataRow(DatabaseType.SqlServer, false, DisplayName = "SQL Server ExecuteQueryInterpolated")]
    [DataRow(DatabaseType.SqlServer, true, DisplayName = "SQL Server ExecuteQueryInterpolatedAsync")]
    public async Task TestExecuteQueryInterpolatedAsync(DatabaseType databaseType, bool useAsync)
    {
        string value = DatabaseUtils.GetMethodName();
        const int recordCount = 20;

        using Context context = DatabaseUtils.CreateDatabase(databaseType);
        DatabaseUtils.CreateMultipleTestTableEntries(context, value, recordCount);

        DataTable dataTable = useAsync
            ? await context.Database.ExecuteQueryInterpolatedAsync($"SELECT * FROM TestTable1 WHERE ID <= {recordCount}").ConfigureAwait(false)
            : context.Database.ExecuteQueryInterpolated($"SELECT * FROM TestTable1 WHERE ID <= {recordCount}");

        Assert.AreEqual(recordCount, dataTable.Rows.Count, "Invalid record count");
    }

    /// <summary>
    /// Test ExecuteQueryRaw.
    /// </summary>
    /// <param name="databaseType">Database type.</param>
    /// <param name="useAsync">If <see langword="true"/> then tests the async method.</param>
    /// <returns><see cref="Task"/>.</returns>
    [TestMethod]
    [DataRow(DatabaseType.Sqlite, false, DisplayName = "SQLite ExecuteQueryRaw")]
    [DataRow(DatabaseType.Sqlite, true, DisplayName = "SQLite ExecuteQueryRawAsync")]
    [DataRow(DatabaseType.SqlServer, false, DisplayName = "SQL Server ExecuteQueryRaw")]
    [DataRow(DatabaseType.SqlServer, true, DisplayName = "SQL Server ExecuteQueryRawAsync")]
    public async Task TestExecuteQueryRawAsync(DatabaseType databaseType, bool useAsync)
    {
        string value = DatabaseUtils.GetMethodName();
        const int recordCount = 20;

        using Context context = DatabaseUtils.CreateDatabase(databaseType);
        DatabaseUtils.CreateMultipleTestTableEntries(context, value, recordCount);

        DataTable dataTable = useAsync
            ? await context.Database.ExecuteQueryRawAsync("SELECT * FROM TestTable1 WHERE ID <= {0}", parameters: recordCount).ConfigureAwait(false)
            : context.Database.ExecuteQueryRaw("SELECT * FROM TestTable1 WHERE ID <= {0}", recordCount);

        Assert.AreEqual(recordCount, dataTable.Rows.Count, "Invalid record count");
    }
}
