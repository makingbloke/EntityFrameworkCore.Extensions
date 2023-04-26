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
    /// <returns>A task that represents the asynchronous test operation.</returns>
    [TestMethod]
    [DataRow(DatabaseType.Sqlite, false, DisplayName = "SQLite ExecuteNonQueryInterpolated.")]
    [DataRow(DatabaseType.Sqlite, true, DisplayName = "SQLite ExecuteNonQueryInterpolatedAsync.")]
    [DataRow(DatabaseType.SqlServer, false, DisplayName = "SQL Server ExecuteNonQueryInterpolated.")]
    [DataRow(DatabaseType.SqlServer, true, DisplayName = "SQL Server ExecuteNonQueryInterpolatedAsync.")]
    public async Task TestExecuteNonQueryInterpolatedAsync(DatabaseType databaseType, bool useAsync)
    {
        using Context context = DatabaseUtils.CreateDatabase(databaseType);

        string value = DatabaseUtils.GetMethodName();
        long id = DatabaseUtils.CreateSingleTestTableEntry(context, value);
        FormattableString sql = $"DELETE FROM TestTable1 WHERE ID = {id}";

        long count = useAsync
            ? await context.Database.ExecuteNonQueryInterpolatedAsync(sql).ConfigureAwait(false)
            : context.Database.ExecuteNonQueryInterpolated(sql);

        Assert.AreEqual(1, count, "Invalid count");
    }

    /// <summary>
    /// Test ExecuteNonQueryRaw with params parameters.
    /// </summary>
    /// <param name="databaseType">Database type.</param>
    /// <param name="useAsync">If <see langword="true"/> then tests the async method.</param>
    /// <returns>A task that represents the asynchronous test operation.</returns>
    [TestMethod]
    [DataRow(DatabaseType.Sqlite, false, DisplayName = "SQLite ExecuteNonQueryRaw params.")]
    [DataRow(DatabaseType.Sqlite, true, DisplayName = "SQLite ExecuteNonQueryRawAsync params.")]
    [DataRow(DatabaseType.SqlServer, false, DisplayName = "SQL Server ExecuteNonQueryRaw params.")]
    [DataRow(DatabaseType.SqlServer, true, DisplayName = "SQL Server ExecuteNonQueryRawAsync params.")]
    public async Task TestExecuteNonQueryRawWithParamsAsync(DatabaseType databaseType, bool useAsync)
    {
        using Context context = DatabaseUtils.CreateDatabase(databaseType);

        string value = DatabaseUtils.GetMethodName();
        long id = DatabaseUtils.CreateSingleTestTableEntry(context, value);
        string sql = "DELETE FROM TestTable1 WHERE ID = {0}";

        long count = useAsync
            ? await context.Database.ExecuteNonQueryRawAsync(sql, parameters: id).ConfigureAwait(false)
            : context.Database.ExecuteNonQueryRaw(sql, id);

        Assert.AreEqual(1, count, "Invalid count");
    }

    /// <summary>
    /// Test ExecuteNonQueryRaw with IEnumerable parameter.
    /// </summary>
    /// <param name="databaseType">Database type.</param>
    /// <param name="useAsync">If <see langword="true"/> then tests the async method.</param>
    /// <returns>A task that represents the asynchronous test operation.</returns>
    [TestMethod]
    [DataRow(DatabaseType.Sqlite, false, DisplayName = "SQLite ExecuteNonQueryRaw IEnumerable.")]
    [DataRow(DatabaseType.Sqlite, true, DisplayName = "SQLite ExecuteNonQueryRawAsync IEnumerable.")]
    [DataRow(DatabaseType.SqlServer, false, DisplayName = "SQL Server ExecuteNonQueryRaw IEnumerable.")]
    [DataRow(DatabaseType.SqlServer, true, DisplayName = "SQL Server ExecuteNonQueryRawAsync IEnumerable.")]
    public async Task TestExecuteNonQueryRawWithIEnumerableAsync(DatabaseType databaseType, bool useAsync)
    {
        using Context context = DatabaseUtils.CreateDatabase(databaseType);

        string value = DatabaseUtils.GetMethodName();
        List<object> parameters = new () { DatabaseUtils.CreateSingleTestTableEntry(context, value) };
        string sql = "DELETE FROM TestTable1 WHERE ID = {0}";

        long count = useAsync
            ? await context.Database.ExecuteNonQueryRawAsync(sql, parameters).ConfigureAwait(false)
            : context.Database.ExecuteNonQueryRaw(sql, parameters);

        Assert.AreEqual(1, count, "Invalid count");
    }

}
