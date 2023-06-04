// Copyright ©2021-2023 Mike King.
// This file is licensed to you under the MIT license.
// See the License.txt file in the solution root for more information.

using DotDoc.EntityFrameworkCore.Extensions.Constants;
using DotDoc.EntityFrameworkCore.Extensions.Tests.Data;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DotDoc.EntityFrameworkCore.Extensions.Tests.QueryExtensions;

/// <summary>
/// Tests for ExecuteScalar extensions.
/// </summary>
[TestClass]
public class ExecuteScalarTests
{
    /// <summary>
    /// Test ExecuteScalar with FormattableString parameter.
    /// </summary>
    /// <param name="databaseType">Database type.</param>
    /// <param name="useAsync">If <see langword="true"/> then tests the async method.</param>
    /// <returns>A task that represents the asynchronous test operation.</returns>
    [TestMethod]
    [DataRow(DatabaseType.Sqlite, false, DisplayName = "SQLite ExecuteScalar FormattableString.")]
    [DataRow(DatabaseType.Sqlite, true, DisplayName = "SQLite ExecuteScalarAsync FormattableString.")]
    [DataRow(DatabaseType.SqlServer, false, DisplayName = "SQL Server ExecuteScalar FormattableString.")]
    [DataRow(DatabaseType.SqlServer, true, DisplayName = "SQL Server ExecuteScalarAsync FormattableString.")]
    public async Task TestExecuteScalarWithFormattableStringAsync(DatabaseType databaseType, bool useAsync)
    {
        using Context context = DatabaseUtils.CreateDatabase(databaseType);

        string value = DatabaseUtils.GetMethodName();
        long id = DatabaseUtils.CreateSingleTestTableEntry(context, value);
        FormattableString sql = $"SELECT COUNT(*) FROM TestTable1 WHERE ID = {id}";

        int count = useAsync
            ? await context.Database.ExecuteScalarAsync<int>(sql).ConfigureAwait(false)
            : context.Database.ExecuteScalar<int>(sql);

        Assert.AreEqual(1, count, "Invalid count");
    }

    /// <summary>
    /// Test ExecuteScalar with params parameter.
    /// </summary>
    /// <param name="databaseType">Database type.</param>
    /// <param name="useAsync">If <see langword="true"/> then tests the async method.</param>
    /// <returns>A task that represents the asynchronous test operation.</returns>
    [TestMethod]
    [DataRow(DatabaseType.Sqlite, false, DisplayName = "SQLite ExecuteScalar params.")]
    [DataRow(DatabaseType.Sqlite, true, DisplayName = "SQLite ExecuteScalarAsync params.")]
    [DataRow(DatabaseType.SqlServer, false, DisplayName = "SQL Server ExecuteScalar params.")]
    [DataRow(DatabaseType.SqlServer, true, DisplayName = "SQL Server ExecuteScalarAsync params.")]
    public async Task TestExecuteScalarWithParamsAsync(DatabaseType databaseType, bool useAsync)
    {
        using Context context = DatabaseUtils.CreateDatabase(databaseType);

        string value = DatabaseUtils.GetMethodName();
        long id = DatabaseUtils.CreateSingleTestTableEntry(context, value);
        string sql = "SELECT COUNT(*) FROM TestTable1 WHERE ID = {0}";

        int count = useAsync
            ? await context.Database.ExecuteScalarAsync<int>(sql, parameters: id).ConfigureAwait(false)
            : context.Database.ExecuteScalar<int>(sql, id);

        Assert.AreEqual(1, count, "Invalid count");
    }

    /// <summary>
    /// Test ExecuteScalar with IEnumerable parameter.
    /// </summary>
    /// <param name="databaseType">Database type.</param>
    /// <param name="useAsync">If <see langword="true"/> then tests the async method.</param>
    /// <returns>A task that represents the asynchronous test operation.</returns>
    [TestMethod]
    [DataRow(DatabaseType.Sqlite, false, DisplayName = "SQLite ExecuteScalar IEnumerable.")]
    [DataRow(DatabaseType.Sqlite, true, DisplayName = "SQLite ExecuteScalarAsync IEnumerable.")]
    [DataRow(DatabaseType.SqlServer, false, DisplayName = "SQL Server ExecuteScalar IEnumerable.")]
    [DataRow(DatabaseType.SqlServer, true, DisplayName = "SQL Server ExecuteScalarAsync IEnumerable.")]
    public async Task TestExecuteScalarWithIEnumerableAsync(DatabaseType databaseType, bool useAsync)
    {
        using Context context = DatabaseUtils.CreateDatabase(databaseType);

        string value = DatabaseUtils.GetMethodName();
        long id = DatabaseUtils.CreateSingleTestTableEntry(context, value);
        List<object> parameters = new () { id };
        string sql = "SELECT COUNT(*) FROM TestTable1 WHERE ID = {0}";

        int count = useAsync
            ? await context.Database.ExecuteScalarAsync<int>(sql, parameters).ConfigureAwait(false)
            : context.Database.ExecuteScalar<int>(sql, parameters);

        Assert.AreEqual(1, count, "Invalid count");
    }

}
