// Copyright ©2021-2024 Mike King.
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
    [TestMethod]
    [DataRow(DatabaseType.Sqlite, DisplayName = "SQLite ExecuteNonQueryAsync FormattableString.")]
    [DataRow(DatabaseType.SqlServer, DisplayName = "SQL Server ExecuteNonQueryAsync FormattableString.")]
    public void Test_ExecuteNonQuery_FormattableString(DatabaseType databaseType)
    {
        using Context context = DatabaseUtils.CreateDatabase(databaseType);

        string value = DatabaseUtils.GetMethodName();
        long id = DatabaseUtils.CreateSingleTestTableEntry(context, value);
        FormattableString sql = $"DELETE FROM TestTable1 WHERE ID = {id}";

        long count = context.Database.ExecuteNonQuery(sql);

        Assert.AreEqual(1, count, "Invalid count");
    }

    /// <summary>
    /// Test ExecuteNonQuery with FormattableString parameter.
    /// </summary>
    /// <param name="databaseType">Database type.</param>
    /// <returns>A task that represents the asynchronous test operation.</returns>
    [TestMethod]
    [DataRow(DatabaseType.Sqlite, DisplayName = "SQLite ExecuteNonQueryAsync FormattableString.")]
    [DataRow(DatabaseType.SqlServer, DisplayName = "SQL Server ExecuteNonQueryAsync FormattableString.")]
    public async Task Test_ExecuteNonQuery_FormattableStringAsync(DatabaseType databaseType)
    {
        using Context context = DatabaseUtils.CreateDatabase(databaseType);

        string value = DatabaseUtils.GetMethodName();
        long id = DatabaseUtils.CreateSingleTestTableEntry(context, value);
        FormattableString sql = $"DELETE FROM TestTable1 WHERE ID = {id}";

        long count = await context.Database.ExecuteNonQueryAsync(sql).ConfigureAwait(false);

        Assert.AreEqual(1, count, "Invalid count");
    }

    /// <summary>
    /// Test ExecuteNonQuery with params parameters.
    /// </summary>
    /// <param name="databaseType">Database type.</param>
    [TestMethod]
    [DataRow(DatabaseType.Sqlite, DisplayName = "SQLite ExecuteNonQuery params.")]
    [DataRow(DatabaseType.SqlServer, DisplayName = "SQL Server ExecuteNonQuery params.")]
    public void Test_ExecuteNonQuery_Params(DatabaseType databaseType)
    {
        using Context context = DatabaseUtils.CreateDatabase(databaseType);

        string value = DatabaseUtils.GetMethodName();
        long id = DatabaseUtils.CreateSingleTestTableEntry(context, value);
        string sql = "DELETE FROM TestTable1 WHERE ID = {0}";

        long count = context.Database.ExecuteNonQuery(sql, id);

        Assert.AreEqual(1, count, "Invalid count");
    }

    /// <summary>
    /// Test ExecuteNonQuery with params parameters.
    /// </summary>
    /// <param name="databaseType">Database type.</param>
    /// <returns>A task that represents the asynchronous test operation.</returns>
    [TestMethod]
    [DataRow(DatabaseType.Sqlite, DisplayName = "SQLite ExecuteNonQueryAsync params.")]
    [DataRow(DatabaseType.SqlServer, DisplayName = "SQL Server ExecuteNonQueryAsync params.")]
    public async Task Test_ExecuteNonQuery_ParamsAsync(DatabaseType databaseType)
    {
        using Context context = DatabaseUtils.CreateDatabase(databaseType);

        string value = DatabaseUtils.GetMethodName();
        long id = DatabaseUtils.CreateSingleTestTableEntry(context, value);
        string sql = "DELETE FROM TestTable1 WHERE ID = {0}";

        long count = await context.Database.ExecuteNonQueryAsync(sql, parameters: id).ConfigureAwait(false);

        Assert.AreEqual(1, count, "Invalid count");
    }

    /// <summary>
    /// Test ExecuteNonQuery with IEnumerable parameter.
    /// </summary>
    /// <param name="databaseType">Database type.</param>
    [TestMethod]
    [DataRow(DatabaseType.Sqlite, DisplayName = "SQLite ExecuteNonQueryAsync IEnumerable.")]
    [DataRow(DatabaseType.SqlServer, DisplayName = "SQL Server ExecuteNonQueryAsync IEnumerable.")]
    public void Test_ExecuteNonQuery_IEnumerable(DatabaseType databaseType)
    {
        using Context context = DatabaseUtils.CreateDatabase(databaseType);

        string value = DatabaseUtils.GetMethodName();
        List<object> parameters = [DatabaseUtils.CreateSingleTestTableEntry(context, value)];
        string sql = "DELETE FROM TestTable1 WHERE ID = {0}";

        long count = context.Database.ExecuteNonQuery(sql, parameters);

        Assert.AreEqual(1, count, "Invalid count");
    }

    /// <summary>
    /// Test ExecuteNonQuery with IEnumerable parameter.
    /// </summary>
    /// <param name="databaseType">Database type.</param>
    /// <returns>A task that represents the asynchronous test operation.</returns>
    [TestMethod]
    [DataRow(DatabaseType.Sqlite, DisplayName = "SQLite ExecuteNonQueryAsync IEnumerable.")]
    [DataRow(DatabaseType.SqlServer, DisplayName = "SQL Server ExecuteNonQueryAsync IEnumerable.")]
    public async Task Test_ExecuteNonQuery_IEnumerableAsync(DatabaseType databaseType)
    {
        using Context context = DatabaseUtils.CreateDatabase(databaseType);

        string value = DatabaseUtils.GetMethodName();
        List<object> parameters = [DatabaseUtils.CreateSingleTestTableEntry(context, value)];
        string sql = "DELETE FROM TestTable1 WHERE ID = {0}";

        long count = await context.Database.ExecuteNonQueryAsync(sql, parameters).ConfigureAwait(false);

        Assert.AreEqual(1, count, "Invalid count");
    }
}
