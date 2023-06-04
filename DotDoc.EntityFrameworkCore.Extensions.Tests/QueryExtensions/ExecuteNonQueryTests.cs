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
    /// Test ExecuteNonQuery with FormattableString parameter.
    /// </summary>
    /// <param name="databaseType">Database type.</param>
    /// <param name="useAsync">If <see langword="true"/> then tests the async method.</param>
    /// <returns>A task that represents the asynchronous test operation.</returns>
    [TestMethod]
    [DataRow(DatabaseType.Sqlite, false, DisplayName = "SQLite ExecuteNonQuery FormattableString.")]
    [DataRow(DatabaseType.Sqlite, true, DisplayName = "SQLite ExecuteNonQueryAsync FormattableString.")]
    [DataRow(DatabaseType.SqlServer, false, DisplayName = "SQL Server ExecuteNonQuery FormattableString.")]
    [DataRow(DatabaseType.SqlServer, true, DisplayName = "SQL Server ExecuteNonQueryAsync FormattableString.")]
    public async Task TestExecuteNonQueryWithFormattableStringAsync(DatabaseType databaseType, bool useAsync)
    {
        using Context context = DatabaseUtils.CreateDatabase(databaseType);

        string value = DatabaseUtils.GetMethodName();
        long id = DatabaseUtils.CreateSingleTestTableEntry(context, value);
        FormattableString sql = $"DELETE FROM TestTable1 WHERE ID = {id}";

        long count = useAsync
            ? await context.Database.ExecuteNonQueryAsync(sql).ConfigureAwait(false)
            : context.Database.ExecuteNonQuery(sql);

        Assert.AreEqual(1, count, "Invalid count");
    }

    /// <summary>
    /// Test ExecuteNonQuery with params parameters.
    /// </summary>
    /// <param name="databaseType">Database type.</param>
    /// <param name="useAsync">If <see langword="true"/> then tests the async method.</param>
    /// <returns>A task that represents the asynchronous test operation.</returns>
    [TestMethod]
    [DataRow(DatabaseType.Sqlite, false, DisplayName = "SQLite ExecuteNonQuery params.")]
    [DataRow(DatabaseType.Sqlite, true, DisplayName = "SQLite ExecuteNonQueryAsync params.")]
    [DataRow(DatabaseType.SqlServer, false, DisplayName = "SQL Server ExecuteNonQuery params.")]
    [DataRow(DatabaseType.SqlServer, true, DisplayName = "SQL Server ExecuteNonQueryAsync params.")]
    public async Task TestExecuteNonQueryWithParamsAsync(DatabaseType databaseType, bool useAsync)
    {
        using Context context = DatabaseUtils.CreateDatabase(databaseType);

        string value = DatabaseUtils.GetMethodName();
        long id = DatabaseUtils.CreateSingleTestTableEntry(context, value);
        string sql = "DELETE FROM TestTable1 WHERE ID = {0}";

        long count = useAsync
            ? await context.Database.ExecuteNonQueryAsync(sql, parameters: id).ConfigureAwait(false)
            : context.Database.ExecuteNonQuery(sql, id);

        Assert.AreEqual(1, count, "Invalid count");
    }

    /// <summary>
    /// Test ExecuteNonQuery with IEnumerable parameter.
    /// </summary>
    /// <param name="databaseType">Database type.</param>
    /// <param name="useAsync">If <see langword="true"/> then tests the async method.</param>
    /// <returns>A task that represents the asynchronous test operation.</returns>
    [TestMethod]
    [DataRow(DatabaseType.Sqlite, false, DisplayName = "SQLite ExecuteNonQuery IEnumerable.")]
    [DataRow(DatabaseType.Sqlite, true, DisplayName = "SQLite ExecuteNonQueryAsync IEnumerable.")]
    [DataRow(DatabaseType.SqlServer, false, DisplayName = "SQL Server ExecuteNonQuery IEnumerable.")]
    [DataRow(DatabaseType.SqlServer, true, DisplayName = "SQL Server ExecuteNonQueryAsync IEnumerable.")]
    public async Task TestExecuteNonQueryWithIEnumerableAsync(DatabaseType databaseType, bool useAsync)
    {
        using Context context = DatabaseUtils.CreateDatabase(databaseType);

        string value = DatabaseUtils.GetMethodName();
        List<object> parameters = new () { DatabaseUtils.CreateSingleTestTableEntry(context, value) };
        string sql = "DELETE FROM TestTable1 WHERE ID = {0}";

        long count = useAsync
            ? await context.Database.ExecuteNonQueryAsync(sql, parameters).ConfigureAwait(false)
            : context.Database.ExecuteNonQuery(sql, parameters);

        Assert.AreEqual(1, count, "Invalid count");
    }

}
