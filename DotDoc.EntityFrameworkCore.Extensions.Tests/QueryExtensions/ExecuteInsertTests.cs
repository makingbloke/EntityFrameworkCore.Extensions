﻿// Copyright ©2021-2024 Mike King.
// This file is licensed to you under the MIT license.
// See the License.txt file in the solution root for more information.

using DotDoc.EntityFrameworkCore.Extensions.Constants;
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
    /// <summary>
    /// Test ExecuteInsert with FormattableString parameter.
    /// </summary>
    /// <param name="databaseType">Database type.</param>
    [TestMethod]
    [DataRow(DatabaseType.Sqlite, DisplayName = "SQLite ExecuteInsert FormattableString.")]
    [DataRow(DatabaseType.SqlServer, DisplayName = "SQL Server ExecuteInsert FormattableString.")]
    public void Test_ExecuteInsert_FormattableString(DatabaseType databaseType)
    {
        using Context context = DatabaseUtils.CreateDatabase(databaseType);

        string value = DatabaseUtils.GetMethodName();
        FormattableString sql = $"INSERT INTO TestTable1 (TestField) VALUES ({value})";

        long id = context.Database.ExecuteInsert(sql);

        int count = context.TestTable1.Count(e => e.Id == id);
        Assert.AreEqual(1, count, "Invalid count");
    }

    /// <summary>
    /// Test ExecuteInsert with FormattableString parameter.
    /// </summary>
    /// <param name="databaseType">Database type.</param>
    /// <returns>A task that represents the asynchronous test operation.</returns>
    [TestMethod]
    [DataRow(DatabaseType.Sqlite, DisplayName = "SQLite ExecuteInsert FormattableString.")]
    [DataRow(DatabaseType.SqlServer, DisplayName = "SQL Server ExecuteInsert FormattableString.")]
    public async Task Test_ExecuteInsert_FormattableStringAsync(DatabaseType databaseType)
    {
        using Context context = DatabaseUtils.CreateDatabase(databaseType);

        string value = DatabaseUtils.GetMethodName();
        FormattableString sql = $"INSERT INTO TestTable1 (TestField) VALUES ({value})";

        long id = await context.Database.ExecuteInsertAsync(sql).ConfigureAwait(false);

        int count = await context.TestTable1.CountAsync(e => e.Id == id).ConfigureAwait(false);
        Assert.AreEqual(1, count, "Invalid count");
    }

    /// <summary>
    /// Test ExecuteInsert with params parameters.
    /// </summary>
    /// <param name="databaseType">Database type.</param>
    [TestMethod]
    [DataRow(DatabaseType.Sqlite, DisplayName = "SQLite ExecuteInsert params.")]
    [DataRow(DatabaseType.SqlServer, DisplayName = "SQL Server ExecuteInsert params.")]
    public void Test_ExecuteInsert_Params(DatabaseType databaseType)
    {
        using Context context = DatabaseUtils.CreateDatabase(databaseType);

        string value = DatabaseUtils.GetMethodName();
        string sql = "INSERT INTO TestTable1 (TestField) VALUES ({0})";

        long id = context.Database.ExecuteInsert(sql, value);

        int count = context.TestTable1.Count(e => e.Id == id);
        Assert.AreEqual(1, count, "Invalid count");
    }

    /// <summary>
    /// Test ExecuteInsert with params parameters.
    /// </summary>
    /// <param name="databaseType">Database type.</param>
    /// <returns>A task that represents the asynchronous test operation.</returns>
    [TestMethod]
    [DataRow(DatabaseType.Sqlite, DisplayName = "SQLite ExecuteInsert params.")]
    [DataRow(DatabaseType.SqlServer, DisplayName = "SQL Server ExecuteInsert params.")]
    public async Task Test_ExecuteInsert_ParamsAsync(DatabaseType databaseType)
    {
        using Context context = DatabaseUtils.CreateDatabase(databaseType);

        string value = DatabaseUtils.GetMethodName();
        string sql = "INSERT INTO TestTable1 (TestField) VALUES ({0})";

        long id = await context.Database.ExecuteInsertAsync(sql, parameters: value).ConfigureAwait(false);

        int count = await context.TestTable1.CountAsync(e => e.Id == id).ConfigureAwait(false);
        Assert.AreEqual(1, count, "Invalid count");
    }

    /// <summary>
    /// Test ExecuteInsert with IEnumerable parameter.
    /// </summary>
    /// <param name="databaseType">Database type.</param>
    [TestMethod]
    [DataRow(DatabaseType.Sqlite, DisplayName = "SQLite ExecuteInsertAsync IEnumerable.")]
    [DataRow(DatabaseType.SqlServer, DisplayName = "SQL Server ExecuteInsertAsync IEnumerable.")]
    public void Test_ExecuteInsert_IEnumerable(DatabaseType databaseType)
    {
        using Context context = DatabaseUtils.CreateDatabase(databaseType);

        string value = DatabaseUtils.GetMethodName();
        List<object> parameters = [value];
        string sql = "INSERT INTO TestTable1 (TestField) VALUES ({0})";

        long id = context.Database.ExecuteInsert(sql, parameters);

        int count = context.TestTable1.Count(e => e.Id == id);
        Assert.AreEqual(1, count, "Invalid count");
    }

    /// <summary>
    /// Test ExecuteInsert with IEnumerable parameter.
    /// </summary>
    /// <param name="databaseType">Database type.</param>
    /// <returns>A task that represents the asynchronous test operation.</returns>
    [TestMethod]
    [DataRow(DatabaseType.Sqlite, DisplayName = "SQLite ExecuteInsertAsync IEnumerable.")]
    [DataRow(DatabaseType.SqlServer, DisplayName = "SQL Server ExecuteInsertAsync IEnumerable.")]
    public async Task Test_ExecuteInsert_IEnumerableAsync(DatabaseType databaseType)
    {
        using Context context = DatabaseUtils.CreateDatabase(databaseType);

        string value = DatabaseUtils.GetMethodName();
        List<object> parameters = [value];
        string sql = "INSERT INTO TestTable1 (TestField) VALUES ({0})";

        long id = await context.Database.ExecuteInsertAsync(sql, parameters).ConfigureAwait(false);

        int count = await context.TestTable1.CountAsync(e => e.Id == id).ConfigureAwait(false);
        Assert.AreEqual(1, count, "Invalid count");
    }
}
