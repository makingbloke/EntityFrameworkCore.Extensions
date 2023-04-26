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
    /// Test ExecuteScalarInterpolated.
    /// </summary>
    /// <param name="databaseType">Database type.</param>
    /// <param name="useAsync">If <see langword="true"/> then tests the async method.</param>
    /// <returns>A task that represents the asynchronous test operation.</returns>
    [TestMethod]
    [DataRow(DatabaseType.Sqlite, false, DisplayName = "SQLite ExecuteScalarInterpolated.")]
    [DataRow(DatabaseType.Sqlite, true, DisplayName = "SQLite ExecuteScalarInterpolatedAsync.")]
    [DataRow(DatabaseType.SqlServer, false, DisplayName = "SQL Server ExecuteScalarInterpolated.")]
    [DataRow(DatabaseType.SqlServer, true, DisplayName = "SQL Server ExecuteScalarInterpolatedAsync.")]
    public async Task TestExecuteScalarInterpolatedAsync(DatabaseType databaseType, bool useAsync)
    {
        using Context context = DatabaseUtils.CreateDatabase(databaseType);

        string value = DatabaseUtils.GetMethodName();
        long id = DatabaseUtils.CreateSingleTestTableEntry(context, value);
        FormattableString sql = $"SELECT COUNT(*) FROM TestTable1 WHERE ID = {id}";

        int count = useAsync
            ? await context.Database.ExecuteScalarInterpolatedAsync<int>(sql).ConfigureAwait(false)
            : context.Database.ExecuteScalarInterpolated<int>(sql);

        Assert.AreEqual(1, count, "Invalid count");
    }

    /// <summary>
    /// Test ExecuteScalarRaw with params parameter.
    /// </summary>
    /// <param name="databaseType">Database type.</param>
    /// <param name="useAsync">If <see langword="true"/> then tests the async method.</param>
    /// <returns>A task that represents the asynchronous test operation.</returns>
    [TestMethod]
    [DataRow(DatabaseType.Sqlite, false, DisplayName = "SQLite ExecuteScalarRaw params.")]
    [DataRow(DatabaseType.Sqlite, true, DisplayName = "SQLite ExecuteScalarRawAsync params.")]
    [DataRow(DatabaseType.SqlServer, false, DisplayName = "SQL Server ExecuteScalarRaw params.")]
    [DataRow(DatabaseType.SqlServer, true, DisplayName = "SQL Server ExecuteScalarRawAsync params.")]
    public async Task TestExecuteScalarRawWithParamsAsync(DatabaseType databaseType, bool useAsync)
    {
        using Context context = DatabaseUtils.CreateDatabase(databaseType);

        string value = DatabaseUtils.GetMethodName();
        long id = DatabaseUtils.CreateSingleTestTableEntry(context, value);
        string sql = "SELECT COUNT(*) FROM TestTable1 WHERE ID = {0}";

        int count = useAsync
            ? await context.Database.ExecuteScalarRawAsync<int>(sql, parameters: id).ConfigureAwait(false)
            : context.Database.ExecuteScalarRaw<int>(sql, id);

        Assert.AreEqual(1, count, "Invalid count");
    }

    /// <summary>
    /// Test ExecuteScalarRaw with IEnumerable parameter.
    /// </summary>
    /// <param name="databaseType">Database type.</param>
    /// <param name="useAsync">If <see langword="true"/> then tests the async method.</param>
    /// <returns>A task that represents the asynchronous test operation.</returns>
    [TestMethod]
    [DataRow(DatabaseType.Sqlite, false, DisplayName = "SQLite ExecuteScalarRaw IEnumerable.")]
    [DataRow(DatabaseType.Sqlite, true, DisplayName = "SQLite ExecuteScalarRawAsync IEnumerable.")]
    [DataRow(DatabaseType.SqlServer, false, DisplayName = "SQL Server ExecuteScalarRaw IEnumerable.")]
    [DataRow(DatabaseType.SqlServer, true, DisplayName = "SQL Server ExecuteScalarRawAsync IEnumerable.")]
    public async Task TestExecuteScalarRawWithIEnumerableAsync(DatabaseType databaseType, bool useAsync)
    {
        using Context context = DatabaseUtils.CreateDatabase(databaseType);

        string value = DatabaseUtils.GetMethodName();
        long id = DatabaseUtils.CreateSingleTestTableEntry(context, value);
        List<object> parameters = new () { id };
        string sql = "SELECT COUNT(*) FROM TestTable1 WHERE ID = {0}";

        int count = useAsync
            ? await context.Database.ExecuteScalarRawAsync<int>(sql, parameters).ConfigureAwait(false)
            : context.Database.ExecuteScalarRaw<int>(sql, parameters);

        Assert.AreEqual(1, count, "Invalid count");
    }

}
