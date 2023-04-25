// Copyright ©2021-2023 Mike King.
// This file is licensed to you under the MIT license.
// See the License.txt file in the solution root for more information.

using DotDoc.EntityFrameworkCore.Extensions.Constants;
using DotDoc.EntityFrameworkCore.Extensions.Tests.Constants;
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
    /// <param name="methodType">Type of method to test. See <see cref="TestMethodType"/>.</param>
    /// <returns><see cref="Task"/>.</returns>
    [TestMethod]
    [DataRow(DatabaseType.Sqlite, TestMethodType.Sync, DisplayName = "SQLite ExecuteNonQueryInterpolated.")]
    [DataRow(DatabaseType.Sqlite, TestMethodType.Async, DisplayName = "SQLite ExecuteNonQueryInterpolatedAsync.")]
    [DataRow(DatabaseType.SqlServer, TestMethodType.Sync, DisplayName = "SQL Server ExecuteNonQueryInterpolated.")]
    [DataRow(DatabaseType.SqlServer, TestMethodType.Async, DisplayName = "SQL Server ExecuteNonQueryInterpolatedAsync.")]
    public async Task TestExecuteNonQueryInterpolatedAsync(DatabaseType databaseType, TestMethodType methodType)
    {
        string value = DatabaseUtils.GetMethodName();

        using Context context = DatabaseUtils.CreateDatabase(databaseType);

        long id = DatabaseUtils.CreateSingleTestTableEntry(context, value);
        FormattableString sql = $"DELETE FROM TestTable1 WHERE ID = {id}";

        long count = methodType switch
        {
            TestMethodType.Sync => await context.Database.ExecuteNonQueryInterpolatedAsync(sql).ConfigureAwait(false),
            TestMethodType.Async => context.Database.ExecuteNonQueryInterpolated(sql),
            _ => throw new ArgumentException($"Unsupported value: {methodType}", nameof(methodType))
        };

        Assert.AreEqual(1, count, "Invalid count");
    }

    /// <summary>
    /// Test ExecuteNonQueryRaw.
    /// </summary>
    /// <param name="databaseType">Database type.</param>
    /// <param name="methodType">Type of method to test. See <see cref="TestMethodType"/>.</param>
    /// <returns><see cref="Task"/>.</returns>
    [TestMethod]
    [DataRow(DatabaseType.Sqlite, TestMethodType.Params | TestMethodType.Sync, DisplayName = "SQLite ExecuteNonQueryRaw params.")]
    [DataRow(DatabaseType.Sqlite, TestMethodType.Params | TestMethodType.Async, DisplayName = "SQLite ExecuteNonQueryRawAsync params.")]
    [DataRow(DatabaseType.Sqlite, TestMethodType.IEnumerable | TestMethodType.Sync, DisplayName = "SQLite ExecuteNonQueryRaw IEnumerable.")]
    [DataRow(DatabaseType.Sqlite, TestMethodType.IEnumerable | TestMethodType.Async, DisplayName = "SQLite ExecuteNonQueryRawAsync IEnumerable.")]
    [DataRow(DatabaseType.SqlServer, TestMethodType.Params | TestMethodType.Sync, DisplayName = "SQL Server ExecuteNonQueryRaw params.")]
    [DataRow(DatabaseType.SqlServer, TestMethodType.Params | TestMethodType.Async, DisplayName = "SQL Server ExecuteNonQueryRawAsync params.")]
    [DataRow(DatabaseType.SqlServer, TestMethodType.IEnumerable | TestMethodType.Sync, DisplayName = "SQL Server ExecuteNonQueryRaw IEnumerable.")]
    [DataRow(DatabaseType.SqlServer, TestMethodType.IEnumerable | TestMethodType.Async, DisplayName = "SQL Server ExecuteNonQueryRawAsync IEnumerable.")]
    public async Task TestExecuteNonQueryRawAsync(DatabaseType databaseType, TestMethodType methodType)
    {
        string value = DatabaseUtils.GetMethodName();

        using Context context = DatabaseUtils.CreateDatabase(databaseType);

        long id = DatabaseUtils.CreateSingleTestTableEntry(context, value);
        string sql = "DELETE FROM TestTable1 WHERE ID = {0}";

        long count = methodType switch
        {
            TestMethodType.Params | TestMethodType.Sync => context.Database.ExecuteNonQueryRaw(sql, id),
            TestMethodType.Params | TestMethodType.Async => await context.Database.ExecuteNonQueryRawAsync(sql, default, id).ConfigureAwait(false),
            TestMethodType.IEnumerable | TestMethodType.Sync => context.Database.ExecuteNonQueryRaw(sql, new List<object> { id }),
            TestMethodType.IEnumerable | TestMethodType.Async => await context.Database.ExecuteNonQueryRawAsync(sql, new List<object> { id }).ConfigureAwait(false),
            _ => throw new ArgumentException($"Unsupported value: {methodType}", nameof(methodType))
        };

        Assert.AreEqual(1, count, "Invalid count");
    }
}
