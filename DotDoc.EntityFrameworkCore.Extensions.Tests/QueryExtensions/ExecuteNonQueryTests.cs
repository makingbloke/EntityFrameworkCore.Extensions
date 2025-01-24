// Copyright ©2021-2025 Mike King.
// This file is licensed to you under the MIT license.
// See the License.txt file in the solution root for more information.

using DotDoc.EntityFrameworkCore.Extensions.Constants;
using DotDoc.EntityFrameworkCore.Extensions.Extensions;
using DotDoc.EntityFrameworkCore.Extensions.Tests.Data;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DotDoc.EntityFrameworkCore.Extensions.Tests.QueryExtensions;

/// <summary>
/// Tests for ExecuteNonQuery extensions.
/// </summary>
[TestClass]
public class ExecuteNonQueryTests
{
    #region public methods

    /// <summary>
    /// Test ExecuteNonQuery with FormattableString parameter.
    /// </summary>
    /// <param name="databaseType">Database type.</param>
    [TestMethod]
    [DataRow(DatabaseType.Sqlite, DisplayName = "SQLite ExecuteNonQueryAsync FormattableString.")]
    [DataRow(DatabaseType.SqlServer, DisplayName = "SQL Server ExecuteNonQueryAsync FormattableString.")]
    public void Test_ExecuteNonQuery_FormattableString(string databaseType)
    {
        // ARRANGE
        using Context context = DatabaseUtils.CreateDatabase(databaseType);

        string value = DatabaseUtils.GetMethodName();
        long id = DatabaseUtils.CreateSingleTestTableEntry(context, value);
        FormattableString sql = $"DELETE FROM TestTable1 WHERE ID = {id}";

        // ACT
        long count = context.Database.ExecuteNonQuery(sql);

        // ASSERT
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
    public async Task Test_ExecuteNonQuery_FormattableStringAsync(string databaseType)
    {
        // ARRANGE
        using Context context = DatabaseUtils.CreateDatabase(databaseType);

        string value = DatabaseUtils.GetMethodName();
        long id = DatabaseUtils.CreateSingleTestTableEntry(context, value);
        FormattableString sql = $"DELETE FROM TestTable1 WHERE ID = {id}";

        // ACT
        long count = await context.Database.ExecuteNonQueryAsync(sql).ConfigureAwait(false);

        // ASSERT
        Assert.AreEqual(1, count, "Invalid count");
    }

    /// <summary>
    /// Test ExecuteNonQuery with params parameters.
    /// </summary>
    /// <param name="databaseType">Database type.</param>
    [TestMethod]
    [DataRow(DatabaseType.Sqlite, DisplayName = "SQLite ExecuteNonQuery params.")]
    [DataRow(DatabaseType.SqlServer, DisplayName = "SQL Server ExecuteNonQuery params.")]
    public void Test_ExecuteNonQuery_Params(string databaseType)
    {
        // ARRANGE
        using Context context = DatabaseUtils.CreateDatabase(databaseType);

        string value = DatabaseUtils.GetMethodName();
        long id = DatabaseUtils.CreateSingleTestTableEntry(context, value);
        string sql = "DELETE FROM TestTable1 WHERE ID = {0}";

        // ACT
        long count = context.Database.ExecuteNonQuery(sql, id);

        // ASSERT
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
    public async Task Test_ExecuteNonQuery_ParamsAsync(string databaseType)
    {
        // ARRANGE
        using Context context = DatabaseUtils.CreateDatabase(databaseType);

        string value = DatabaseUtils.GetMethodName();
        long id = DatabaseUtils.CreateSingleTestTableEntry(context, value);
        string sql = "DELETE FROM TestTable1 WHERE ID = {0}";

        // ACT
        long count = await context.Database.ExecuteNonQueryAsync(sql, parameters: id).ConfigureAwait(false);

        // ASSERT
        Assert.AreEqual(1, count, "Invalid count");
    }

    #endregion public methods
}
