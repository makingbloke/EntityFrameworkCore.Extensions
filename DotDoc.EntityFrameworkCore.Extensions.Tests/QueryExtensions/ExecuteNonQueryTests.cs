// Copyright ©2021-2023 Mike King.
// This file is licensed to you under the MIT license.
// See the License.txt file in the solution root for more information.

using DotDoc.EntityFrameworkCore.Extensions.Constants;
using DotDoc.EntityFrameworkCore.Extensions.Tests.Data;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DotDoc.EntityFrameworkCore.Extensions.Tests.QueryExtensions;

/// <summary>
/// Tests for ExecuteNonQuery extensions.
/// </summary>
[TestClass]
public class ExecuteNonQueryTests
{
    /// <summary>
    /// Test ExecuteNonQueryInterpolated.
    /// </summary>
    /// <param name="databaseType">Database type.</param>
    /// <param name="useAsync">If <see langword="true"/> then tests the async method.</param>
    /// <returns><see cref="Task"/>.</returns>
    [TestMethod]
    [DataRow(DatabaseType.Sqlite, false, DisplayName = "SQLite ExecuteNonQueryInterpolated")]
    [DataRow(DatabaseType.Sqlite, true, DisplayName = "SQLite ExecuteNonQueryInterpolatedAsync")]
    [DataRow(DatabaseType.SqlServer, false, DisplayName = "SQL Server ExecuteNonQueryInterpolated")]
    [DataRow(DatabaseType.SqlServer, true, DisplayName = "SQL Server ExecuteNonQueryInterpolatedAsync")]
    public async Task TestExecuteNonQueryInterpolatedAsync(DatabaseType databaseType, bool useAsync)
    {
        string value = DatabaseUtils.GetMethodName();

        using Context context = DatabaseUtils.CreateDatabase(databaseType);
        long id = DatabaseUtils.CreateSingleTestTableEntry(context, value);

        long count = useAsync
            ? await context.Database.ExecuteNonQueryInterpolatedAsync($"DELETE FROM TestTable1 WHERE ID = {id}").ConfigureAwait(false)
            : context.Database.ExecuteNonQueryInterpolated($"DELETE FROM TestTable1 WHERE ID = {id}");

        Assert.AreEqual(1, count, "Invalid count");
    }

    /// <summary>
    /// Test ExecuteNonQueryRaw.
    /// </summary>
    /// <param name="databaseType">Database type.</param>
    /// <param name="useAsync">If <see langword="true"/> then tests the async method.</param>
    /// <returns><see cref="Task"/>.</returns>
    [TestMethod]
    [DataRow(DatabaseType.Sqlite, false, DisplayName = "SQLite ExecuteNonQueryRaw")]
    [DataRow(DatabaseType.Sqlite, true, DisplayName = "SQLite ExecuteNonQueryRawAsync")]
    [DataRow(DatabaseType.SqlServer, false, DisplayName = "SQL Server ExecuteNonQueryRaw")]
    [DataRow(DatabaseType.SqlServer, true, DisplayName = "SQL Server ExecuteNonQueryRawAsync")]
    public async Task TestExecuteNonQueryRawAsync(DatabaseType databaseType, bool useAsync)
    {
        string value = DatabaseUtils.GetMethodName();

        using Context context = DatabaseUtils.CreateDatabase(databaseType);
        long id = DatabaseUtils.CreateSingleTestTableEntry(context, value);

        long count = useAsync
            ? await context.Database.ExecuteNonQueryRawAsync("DELETE FROM TestTable1 WHERE ID = {0}", parameters: id).ConfigureAwait(false)
            : context.Database.ExecuteNonQueryRaw("DELETE FROM TestTable1 WHERE ID = {0}", id);

        Assert.AreEqual(1, count, "Invalid count");
    }
}
