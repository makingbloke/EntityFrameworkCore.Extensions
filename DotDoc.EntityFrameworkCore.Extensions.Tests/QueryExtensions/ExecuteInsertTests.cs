// Copyright ©2021-2022 Mike King.
// This file is licensed to you under the MIT license.
// See the License.txt file in the solution root for more information.

using DotDoc.EntityFrameworkCore.Extensions.Constants;
using DotDoc.EntityFrameworkCore.Extensions.Extensions;
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
    /// <param name="useAsync">If <see langword="true"/> then tests the async method.</param>
    /// <returns><see cref="Task"/>.</returns>
    [TestMethod]
    [DataRow(DatabaseType.Sqlite, false, DisplayName = "SQLite ExecuteInsertInterpolated.")]
    [DataRow(DatabaseType.Sqlite, true, DisplayName = "SQLite ExecuteInsertInterpolatedAsync.")]
    [DataRow(DatabaseType.SqlServer, false, DisplayName = "SQL Server ExecuteInsertInterpolated.")]
    [DataRow(DatabaseType.SqlServer, true, DisplayName = "SQL Server ExecuteInsertInterpolatedAsync.")]
    public async Task TestExecuteInsertInterpolatedAsync(DatabaseType databaseType, bool useAsync)
    {
        const string value = nameof(this.TestExecuteInsertInterpolatedAsync);
        FormattableString sql = $"INSERT INTO TestTable (TestField) VALUES ({value})";

        using Context context = DatabaseUtils.CreateDatabase(databaseType);

        long id = useAsync
            ? await context.Database.ExecuteInsertInterpolatedAsync(sql).ConfigureAwait(false)
            : context.Database.ExecuteInsertInterpolated(sql);

        Assert.AreEqual(1, id, "Invalid Id");

        TestTable testTable = context.TestTable.FirstOrDefault(e => e.Id == id);
        Assert.AreEqual(value, testTable?.TestField, "Invalid value");
    }

    /// <summary>
    /// Test ExecuteInsertRaw.
    /// </summary>
    /// <param name="databaseType">Database type.</param>
    /// <param name="useAsync">If <see langword="true"/> then tests the async method.</param>
    /// <returns><see cref="Task"/>.</returns>
    [TestMethod]
    [DataRow(DatabaseType.Sqlite, false, DisplayName = "SQLite ExecuteInsertRaw.")]
    [DataRow(DatabaseType.Sqlite, true, DisplayName = "SQLite ExecuteInsertRawAsync.")]
    [DataRow(DatabaseType.SqlServer, false, DisplayName = "SQL Server ExecuteInsertRaw.")]
    [DataRow(DatabaseType.SqlServer, true, DisplayName = "SQL Server ExecuteInsertRawAsync.")]
    public async Task TestExecuteInsertRawAsync(DatabaseType databaseType, bool useAsync)
    {
        const string value = nameof(this.TestExecuteInsertRawAsync);
        string sql = "INSERT INTO TestTable (TestField) VALUES ({0})";

        using Context context = DatabaseUtils.CreateDatabase(databaseType);

        long id = useAsync
            ? await context.Database.ExecuteInsertRawAsync(sql, parameters: value).ConfigureAwait(false)
            : context.Database.ExecuteInsertRaw(sql, value);

        Assert.AreEqual(1, id, "Invalid Id");

        TestTable testTable = context.TestTable.FirstOrDefault(e => e.Id == id);
        Assert.AreEqual(value, testTable?.TestField, "Invalid value");
    }
}
