// Copyright ©2021-2025 Mike King.
// This file is licensed to you under the MIT license.
// See the License.txt file in the solution root for more information.

using DotDoc.EntityFrameworkCore.Extensions.Constants;
using DotDoc.EntityFrameworkCore.Extensions.Extensions;
using DotDoc.EntityFrameworkCore.Extensions.Tests.Data;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DotDoc.EntityFrameworkCore.Extensions.Tests.QueryExtensions;

/// <summary>
/// Tests for ExecuteScalar extensions.
/// </summary>
[TestClass]
public class ExecuteScalarTests
{
    #region public methods

    /// <summary>
    /// Test ExecuteScalar with FormattableString parameter.
    /// </summary>
    /// <param name="databaseType">Database type.</param>
    [TestMethod]
    [DataRow(DatabaseType.Sqlite, DisplayName = "SQLite ExecuteScalar FormattableString.")]
    [DataRow(DatabaseType.SqlServer, DisplayName = "SQL Server ExecuteScalar FormattableString.")]
    public void Test_ExecuteScalar_FormattableString(string databaseType)
    {
        using Context context = DatabaseUtils.CreateDatabase(databaseType);

        string value = DatabaseUtils.GetMethodName();
        long id = DatabaseUtils.CreateSingleTestTableEntry(context, value);
        FormattableString sql = $"SELECT COUNT(*) FROM TestTable1 WHERE ID = {id}";

        int count = context.Database.ExecuteScalar<int>(sql);

        Assert.AreEqual(1, count, "Invalid count");
    }

    /// <summary>
    /// Test ExecuteScalar with FormattableString parameter.
    /// </summary>
    /// <param name="databaseType">Database type.</param>
    /// <returns>A task that represents the asynchronous test operation.</returns>
    [TestMethod]
    [DataRow(DatabaseType.Sqlite, DisplayName = "SQLite ExecuteScalar FormattableString.")]
    [DataRow(DatabaseType.SqlServer, DisplayName = "SQL Server ExecuteScalar FormattableString.")]
    public async Task Test_ExecuteScalar_FormattableStringAsync(string databaseType)
    {
        using Context context = DatabaseUtils.CreateDatabase(databaseType);

        string value = DatabaseUtils.GetMethodName();
        long id = DatabaseUtils.CreateSingleTestTableEntry(context, value);
        FormattableString sql = $"SELECT COUNT(*) FROM TestTable1 WHERE ID = {id}";

        int count = await context.Database.ExecuteScalarAsync<int>(sql).ConfigureAwait(false);

        Assert.AreEqual(1, count, "Invalid count");
    }

    /// <summary>
    /// Test ExecuteScalar with params parameter.
    /// </summary>
    /// <param name="databaseType">Database type.</param>
    [TestMethod]
    [DataRow(DatabaseType.Sqlite, DisplayName = "SQLite ExecuteScalar params.")]
    [DataRow(DatabaseType.SqlServer, DisplayName = "SQL Server ExecuteScalar params.")]
    public void Test_ExecuteScalar_Params(string databaseType)
    {
        using Context context = DatabaseUtils.CreateDatabase(databaseType);

        string value = DatabaseUtils.GetMethodName();
        long id = DatabaseUtils.CreateSingleTestTableEntry(context, value);
        string sql = "SELECT COUNT(*) FROM TestTable1 WHERE ID = {0}";

        int count = context.Database.ExecuteScalar<int>(sql, id);

        Assert.AreEqual(1, count, "Invalid count");
    }

    /// <summary>
    /// Test ExecuteScalar with params parameter.
    /// </summary>
    /// <param name="databaseType">Database type.</param>
    /// <returns>A task that represents the asynchronous test operation.</returns>
    [TestMethod]
    [DataRow(DatabaseType.Sqlite, DisplayName = "SQLite ExecuteScalar params.")]
    [DataRow(DatabaseType.SqlServer, DisplayName = "SQL Server ExecuteScalar params.")]
    public async Task Test_ExecuteScalar_ParamsAsync(string databaseType)
    {
        using Context context = DatabaseUtils.CreateDatabase(databaseType);

        string value = DatabaseUtils.GetMethodName();
        long id = DatabaseUtils.CreateSingleTestTableEntry(context, value);
        string sql = "SELECT COUNT(*) FROM TestTable1 WHERE ID = {0}";

        int count = await context.Database.ExecuteScalarAsync<int>(sql, parameters: id).ConfigureAwait(false);

        Assert.AreEqual(1, count, "Invalid count");
    }

    /// <summary>
    /// Test ExecuteScalar with IEnumerable parameter.
    /// </summary>
    /// <param name="databaseType">Database type.</param>
    [TestMethod]
    [DataRow(DatabaseType.Sqlite, DisplayName = "SQLite ExecuteScalar IEnumerable.")]
    [DataRow(DatabaseType.SqlServer, DisplayName = "SQL Server ExecuteScalar IEnumerable.")]
    public void Test_ExecuteScalar_IEnumerable(string databaseType)
    {
        using Context context = DatabaseUtils.CreateDatabase(databaseType);

        string value = DatabaseUtils.GetMethodName();
        long id = DatabaseUtils.CreateSingleTestTableEntry(context, value);
        List<object> parameters = [id];
        string sql = "SELECT COUNT(*) FROM TestTable1 WHERE ID = {0}";

        int count = context.Database.ExecuteScalar<int>(sql, parameters);

        Assert.AreEqual(1, count, "Invalid count");
    }

    /// <summary>
    /// Test ExecuteScalar with IEnumerable parameter.
    /// </summary>
    /// <param name="databaseType">Database type.</param>
    /// <returns>A task that represents the asynchronous test operation.</returns>
    [TestMethod]
    [DataRow(DatabaseType.Sqlite, DisplayName = "SQLite ExecuteScalar IEnumerable.")]
    [DataRow(DatabaseType.SqlServer, DisplayName = "SQL Server ExecuteScalar IEnumerable.")]
    public async Task Test_ExecuteScalar_IEnumerableAsync(string databaseType)
    {
        using Context context = DatabaseUtils.CreateDatabase(databaseType);

        string value = DatabaseUtils.GetMethodName();
        long id = DatabaseUtils.CreateSingleTestTableEntry(context, value);
        List<object> parameters = [id];
        string sql = "SELECT COUNT(*) FROM TestTable1 WHERE ID = {0}";

        int count = await context.Database.ExecuteScalarAsync<int>(sql, parameters).ConfigureAwait(false);

        Assert.AreEqual(1, count, "Invalid count");
    }

    #endregion public methods
}
