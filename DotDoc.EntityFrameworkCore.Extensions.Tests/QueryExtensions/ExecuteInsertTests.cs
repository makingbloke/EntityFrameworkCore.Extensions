// Copyright ©2021-2025 Mike King.
// This file is licensed to you under the MIT license.
// See the License.txt file in the solution root for more information.

using DotDoc.EntityFrameworkCore.Extensions.Constants;
using DotDoc.EntityFrameworkCore.Extensions.Extensions;
using DotDoc.EntityFrameworkCore.Extensions.Tests.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DotDoc.EntityFrameworkCore.Extensions.Tests.QueryExtensions;

/// <summary>
/// Tests for ExecuteInsert extensions.
/// </summary>
[TestClass]
public class ExecuteInsertTests
{
    #region public methods

    /// <summary>
    /// Test ExecuteInsert with FormattableString parameter returning a long Id.
    /// </summary>
    /// <param name="databaseType">Database type.</param>
    [TestMethod]
    [DataRow(DatabaseType.Sqlite, DisplayName = "SQLite ExecuteInsert FormattableString Long Id.")]
    [DataRow(DatabaseType.SqlServer, DisplayName = "SQL Server ExecuteInsert FormattableString Long Id.")]
    public void Test_ExecuteInsert_FormattableString_Long(string databaseType)
    {
        using Context context = DatabaseUtils.CreateDatabase(databaseType);

        string value = DatabaseUtils.GetMethodName();
        FormattableString sql = $"INSERT INTO TestTable1 (TestField) VALUES ({value})";

        long id = context.Database.ExecuteInsert(sql);

        int count = context.TestTable1.Count(e => e.Id == id);
        Assert.AreEqual(1, count, "Invalid count");
    }

    /// <summary>
    /// Test ExecuteInsert with FormattableString parameter returning a generic Id.
    /// </summary>
    /// <param name="databaseType">Database type.</param>
    [TestMethod]
    [DataRow(DatabaseType.Sqlite, DisplayName = "SQLite ExecuteInsert FormattableString Generic Id.")]
    [DataRow(DatabaseType.SqlServer, DisplayName = "SQL Server ExecuteInsert FormattableString Generic Id.")]
    public void Test_ExecuteInsert_FormattableString_Generic(string databaseType)
    {
        using Context context = DatabaseUtils.CreateDatabase(databaseType);

        string value = DatabaseUtils.GetMethodName();
        FormattableString sql = $"INSERT INTO TestTable1 (TestField) VALUES ({value})";

        long id = context.Database.ExecuteInsert<long>(sql);

        int count = context.TestTable1.Count(e => e.Id == id);
        Assert.AreEqual(1, count, "Invalid count");
    }

    /// <summary>
    /// Test ExecuteInsert with FormattableString parameter returning a long Id.
    /// </summary>
    /// <param name="databaseType">Database type.</param>
    /// <returns>A task that represents the asynchronous test operation.</returns>
    [TestMethod]
    [DataRow(DatabaseType.Sqlite, DisplayName = "SQLite ExecuteInsert FormattableString Long Id.")]
    [DataRow(DatabaseType.SqlServer, DisplayName = "SQL Server ExecuteInsert FormattableString Long Id.")]
    public async Task Test_ExecuteInsert_FormattableString_LongAsync(string databaseType)
    {
        using Context context = DatabaseUtils.CreateDatabase(databaseType);

        string value = DatabaseUtils.GetMethodName();
        FormattableString sql = $"INSERT INTO TestTable1 (TestField) VALUES ({value})";

        long id = await context.Database.ExecuteInsertAsync(sql).ConfigureAwait(false);

        int count = await context.TestTable1.CountAsync(e => e.Id == id).ConfigureAwait(false);
        Assert.AreEqual(1, count, "Invalid count");
    }

    /// <summary>
    /// Test ExecuteInsert with FormattableString parameter returning a generic Id.
    /// </summary>
    /// <param name="databaseType">Database type.</param>
    /// <returns>A task that represents the asynchronous test operation.</returns>
    [TestMethod]
    [DataRow(DatabaseType.Sqlite, DisplayName = "SQLite ExecuteInsert FormattableString Generic Id.")]
    [DataRow(DatabaseType.SqlServer, DisplayName = "SQL Server ExecuteInsert FormattableString Generic Id.")]
    public async Task Test_ExecuteInsert_FormattableString_GenericAsync(string databaseType)
    {
        using Context context = DatabaseUtils.CreateDatabase(databaseType);

        string value = DatabaseUtils.GetMethodName();
        FormattableString sql = $"INSERT INTO TestTable1 (TestField) VALUES ({value})";

        long id = await context.Database.ExecuteInsertAsync<long>(sql).ConfigureAwait(false);

        int count = await context.TestTable1.CountAsync(e => e.Id == id).ConfigureAwait(false);
        Assert.AreEqual(1, count, "Invalid count");
    }

    /// <summary>
    /// Test ExecuteInsert with params parameters returning a long Id.
    /// </summary>
    /// <param name="databaseType">Database type.</param>
    [TestMethod]
    [DataRow(DatabaseType.Sqlite, DisplayName = "SQLite ExecuteInsert params Long Id.")]
    [DataRow(DatabaseType.SqlServer, DisplayName = "SQL Server ExecuteInsert params Long Id.")]
    public void Test_ExecuteInsert_Params_Long(string databaseType)
    {
        using Context context = DatabaseUtils.CreateDatabase(databaseType);

        string value = DatabaseUtils.GetMethodName();
        string sql = "INSERT INTO TestTable1 (TestField) VALUES ({0})";

        long id = context.Database.ExecuteInsert(sql, value);

        int count = context.TestTable1.Count(e => e.Id == id);
        Assert.AreEqual(1, count, "Invalid count");
    }

    /// <summary>
    /// Test ExecuteInsert with params parameters returning a generic Id.
    /// </summary>
    /// <param name="databaseType">Database type.</param>
    [TestMethod]
    [DataRow(DatabaseType.Sqlite, DisplayName = "SQLite ExecuteInsert params Generic Id.")]
    [DataRow(DatabaseType.SqlServer, DisplayName = "SQL Server ExecuteInsert params Generic Id.")]
    public void Test_ExecuteInsert_Params_Generic(string databaseType)
    {
        using Context context = DatabaseUtils.CreateDatabase(databaseType);

        string value = DatabaseUtils.GetMethodName();
        string sql = "INSERT INTO TestTable1 (TestField) VALUES ({0})";

        long id = context.Database.ExecuteInsert<long>(sql, value);

        int count = context.TestTable1.Count(e => e.Id == id);
        Assert.AreEqual(1, count, "Invalid count");
    }

    /// <summary>
    /// Test ExecuteInsert with params parameters returning a long Id.
    /// </summary>
    /// <param name="databaseType">Database type.</param>
    /// <returns>A task that represents the asynchronous test operation.</returns>
    [TestMethod]
    [DataRow(DatabaseType.Sqlite, DisplayName = "SQLite ExecuteInsert params Long Id.")]
    [DataRow(DatabaseType.SqlServer, DisplayName = "SQL Server ExecuteInsert params Long Id.")]
    public async Task Test_ExecuteInsert_Params_LongAsync(string databaseType)
    {
        using Context context = DatabaseUtils.CreateDatabase(databaseType);

        string value = DatabaseUtils.GetMethodName();
        string sql = "INSERT INTO TestTable1 (TestField) VALUES ({0})";

        long id = await context.Database.ExecuteInsertAsync(sql, parameters: value).ConfigureAwait(false);

        int count = await context.TestTable1.CountAsync(e => e.Id == id).ConfigureAwait(false);
        Assert.AreEqual(1, count, "Invalid count");
    }

    /// <summary>
    /// Test ExecuteInsert with params parameters returning a generic Id.
    /// </summary>
    /// <param name="databaseType">Database type.</param>
    /// <returns>A task that represents the asynchronous test operation.</returns>
    [TestMethod]
    [DataRow(DatabaseType.Sqlite, DisplayName = "SQLite ExecuteInsert params Generic Id.")]
    [DataRow(DatabaseType.SqlServer, DisplayName = "SQL Server ExecuteInsert params Generic Id.")]
    public async Task Test_ExecuteInsert_Params_GenericAsync(string databaseType)
    {
        using Context context = DatabaseUtils.CreateDatabase(databaseType);

        string value = DatabaseUtils.GetMethodName();
        string sql = "INSERT INTO TestTable1 (TestField) VALUES ({0})";

        long id = await context.Database.ExecuteInsertAsync<long>(sql, parameters: value).ConfigureAwait(false);

        int count = await context.TestTable1.CountAsync(e => e.Id == id).ConfigureAwait(false);
        Assert.AreEqual(1, count, "Invalid count");
    }

    #endregion public methods
}
