// Copyright ©2021-2023 Mike King.
// This file is licensed to you under the MIT license.
// See the License.txt file in the solution root for more information.

using DotDoc.EntityFrameworkCore.Extensions.Constants;
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
    /// <returns>A task that represents the asynchronous test operation.</returns>
    [TestMethod]
    [DataRow(DatabaseType.Sqlite, false, DisplayName = "SQLite ExecuteInsertInterpolated.")]
    [DataRow(DatabaseType.Sqlite, true, DisplayName = "SQLite ExecuteInsertInterpolatedAsync.")]
    [DataRow(DatabaseType.SqlServer, false, DisplayName = "SQL Server ExecuteInsertInterpolated.")]
    [DataRow(DatabaseType.SqlServer, true, DisplayName = "SQL Server ExecuteInsertInterpolatedAsync.")]
    public async Task TestExecuteInsertInterpolatedAsync(DatabaseType databaseType, bool useAsync)
    {
        using Context context = DatabaseUtils.CreateDatabase(databaseType);

        string value = DatabaseUtils.GetMethodName();
        FormattableString sql = $"INSERT INTO TestTable1 (TestField) VALUES ({value})";

        long id = useAsync
            ? await context.Database.ExecuteInsertInterpolatedAsync(sql).ConfigureAwait(false)
            : context.Database.ExecuteInsertInterpolated(sql);

        int count = context.TestTable1.Count(e => e.Id == id);
        Assert.AreEqual(1, count, "Invalid count");
    }

    /// <summary>
    /// Test ExecuteInsertRaw with params parameters.
    /// </summary>
    /// <param name="databaseType">Database type.</param>
    /// <param name="useAsync">If <see langword="true"/> then tests the async method.</param>
    /// <returns>A task that represents the asynchronous test operation.</returns>
    [TestMethod]
    [DataRow(DatabaseType.Sqlite, false, DisplayName = "SQLite ExecuteInsertRaw params.")]
    [DataRow(DatabaseType.Sqlite, true, DisplayName = "SQLite ExecuteInsertRawAsync params.")]
    [DataRow(DatabaseType.SqlServer, false, DisplayName = "SQL Server ExecuteInsertRaw params.")]
    [DataRow(DatabaseType.SqlServer, true, DisplayName = "SQL Server ExecuteInsertRawAsync params.")]
    public async Task TestExecuteInsertRawWithParamsAsync(DatabaseType databaseType, bool useAsync)
    {
        using Context context = DatabaseUtils.CreateDatabase(databaseType);

        string value = DatabaseUtils.GetMethodName();
        string sql = "INSERT INTO TestTable1 (TestField) VALUES ({0})";

        long id = useAsync
            ? await context.Database.ExecuteInsertRawAsync(sql, parameters: value).ConfigureAwait(false)
            : context.Database.ExecuteInsertRaw(sql, value);

        int count = context.TestTable1.Count(e => e.Id == id);
        Assert.AreEqual(1, count, "Invalid count");
    }

    /// <summary>
    /// Test ExecuteInsertRaw with IEnumerable parameter.
    /// </summary>
    /// <param name="databaseType">Database type.</param>
    /// <param name="useAsync">If <see langword="true"/> then tests the async method.</param>
    /// <returns>A task that represents the asynchronous test operation.</returns>
    [TestMethod]
    [DataRow(DatabaseType.Sqlite, false, DisplayName = "SQLite ExecuteInsertRaw IEnumerable.")]
    [DataRow(DatabaseType.Sqlite, true, DisplayName = "SQLite ExecuteInsertRawAsync IEnumerable.")]
    [DataRow(DatabaseType.SqlServer, false, DisplayName = "SQL Server ExecuteInsertRaw IEnumerable.")]
    [DataRow(DatabaseType.SqlServer, true, DisplayName = "SQL Server ExecuteInsertRawAsync IEnumerable.")]
    public async Task TestExecuteInsertRawWithIEnumerableAsync(DatabaseType databaseType, bool useAsync)
    {
        using Context context = DatabaseUtils.CreateDatabase(databaseType);

        string value = DatabaseUtils.GetMethodName();
        List<object> parameters = new () { value };
        string sql = "INSERT INTO TestTable1 (TestField) VALUES ({0})";

        long id = useAsync
            ? await context.Database.ExecuteInsertRawAsync(sql, parameters).ConfigureAwait(false)
            : context.Database.ExecuteInsertRaw(sql, parameters);

        int count = context.TestTable1.Count(e => e.Id == id);
        Assert.AreEqual(1, count, "Invalid count");
    }

}
