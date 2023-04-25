// Copyright ©2021-2023 Mike King.
// This file is licensed to you under the MIT license.
// See the License.txt file in the solution root for more information.

using DotDoc.EntityFrameworkCore.Extensions.Constants;
using DotDoc.EntityFrameworkCore.Extensions.Tests.Constants;
using DotDoc.EntityFrameworkCore.Extensions.Tests.Data;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DotDoc.EntityFrameworkCore.Extensions.Tests.QueryExtensions;

/// <summary>
/// Tests for ExecuteInsert extensions.
/// </summary>
[TestClass]
public class ExecuteInsertTests
{
    /// <summary>
    /// Test ExecuteInsertInterpolated.
    /// </summary>
    /// <param name="databaseType">Database type.</param>
    /// <param name="methodType">Type of method to test. See <see cref="TestMethodType"/>.</param>
    /// <returns><see cref="Task"/>.</returns>
    [TestMethod]
    [DataRow(DatabaseType.Sqlite, TestMethodType.Sync, DisplayName = "SQLite ExecuteInsertInterpolated.")]
    [DataRow(DatabaseType.Sqlite, TestMethodType.Async, DisplayName = "SQLite ExecuteInsertInterpolatedAsync.")]
    [DataRow(DatabaseType.SqlServer, TestMethodType.Sync, DisplayName = "SQL Server ExecuteInsertInterpolated.")]
    [DataRow(DatabaseType.SqlServer, TestMethodType.Async, DisplayName = "SQL Server ExecuteInsertInterpolatedAsync.")]
    public async Task TestExecuteInsertInterpolatedAsync(DatabaseType databaseType, TestMethodType methodType)
    {
        string value = DatabaseUtils.GetMethodName();

        using Context context = DatabaseUtils.CreateDatabase(databaseType);

        FormattableString sql = $"INSERT INTO TestTable1 (TestField) VALUES ({value})";

        long id = methodType switch
        {
            TestMethodType.Sync => context.Database.ExecuteInsertInterpolated(sql),
            TestMethodType.Async => await context.Database.ExecuteInsertInterpolatedAsync(sql).ConfigureAwait(false),
            _ => throw new ArgumentException($"Unsupported value: {methodType}", nameof(methodType))
        };

        Assert.AreEqual(1, id, "Invalid Id");

        TestTable1 testTable1 = context.TestTable1.FirstOrDefault(e => e.Id == id);
        Assert.AreEqual(value, testTable1?.TestField, "Invalid value");
    }

    /// <summary>
    /// Test ExecuteInsertRaw.
    /// </summary>
    /// <param name="databaseType">Database type.</param>
    /// <param name="methodType">Type of method to test. See <see cref="TestMethodType"/>.</param>
    /// <returns><see cref="Task"/>.</returns>
    [TestMethod]
    [DataRow(DatabaseType.Sqlite, TestMethodType.Params | TestMethodType.Sync, DisplayName = "SQLite ExecuteInsertRaw params.")]
    [DataRow(DatabaseType.Sqlite, TestMethodType.Params | TestMethodType.Async, DisplayName = "SQLite ExecuteInsertRawAsync params.")]
    [DataRow(DatabaseType.Sqlite, TestMethodType.IEnumerable | TestMethodType.Sync, DisplayName = "SQLite ExecuteInsertRaw IEnumerable.")]
    [DataRow(DatabaseType.Sqlite, TestMethodType.IEnumerable | TestMethodType.Async, DisplayName = "SQLite ExecuteInsertRawAsync IEnumerable.")]
    [DataRow(DatabaseType.SqlServer, TestMethodType.Params | TestMethodType.Sync, DisplayName = "SQL Server ExecuteInsertRaw params.")]
    [DataRow(DatabaseType.SqlServer, TestMethodType.Params | TestMethodType.Async, DisplayName = "SQL Server ExecuteInsertRawAsync params.")]
    [DataRow(DatabaseType.SqlServer, TestMethodType.IEnumerable | TestMethodType.Sync, DisplayName = "SQL Server ExecuteInsertRaw IEnumerable.")]
    [DataRow(DatabaseType.SqlServer, TestMethodType.IEnumerable | TestMethodType.Async, DisplayName = "SQL Server ExecuteInsertRawAsync IEnumerable.")]
    public async Task TestExecuteInsertRawAsync(DatabaseType databaseType, TestMethodType methodType)
    {
        string value = DatabaseUtils.GetMethodName();

        using Context context = DatabaseUtils.CreateDatabase(databaseType);

        string sql = "INSERT INTO TestTable1 (TestField) VALUES ({0})";

        long id = methodType switch
        {
            TestMethodType.Params | TestMethodType.Sync => context.Database.ExecuteInsertRaw(sql, value),
            TestMethodType.Params | TestMethodType.Async => await context.Database.ExecuteInsertRawAsync(sql, default, value).ConfigureAwait(false),
            TestMethodType.IEnumerable | TestMethodType.Sync => context.Database.ExecuteInsertRaw(sql, new List<object> { value }),
            TestMethodType.IEnumerable | TestMethodType.Async => await context.Database.ExecuteInsertRawAsync(sql, new List<object> { value }).ConfigureAwait(false),
            _ => throw new ArgumentException($"Unsupported value: {methodType}", nameof(methodType))
        };

        Assert.AreEqual(1, id, "Invalid Id");

        TestTable1 testTable1 = context.TestTable1.FirstOrDefault(e => e.Id == id);
        Assert.AreEqual(value, testTable1?.TestField, "Invalid value");
    }
}
