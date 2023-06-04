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
    /// Test ExecuteInsert with FormattableString parameter.
    /// </summary>
    /// <param name="databaseType">Database type.</param>
    /// <param name="useAsync">If <see langword="true"/> then tests the async method.</param>
    /// <returns>A task that represents the asynchronous test operation.</returns>
    [TestMethod]
    [DataRow(DatabaseType.Sqlite, false, DisplayName = "SQLite ExecuteInsert FormattableString.")]
    [DataRow(DatabaseType.Sqlite, true, DisplayName = "SQLite ExecuteInsertAsync FormattableString.")]
    [DataRow(DatabaseType.SqlServer, false, DisplayName = "SQL Server ExecuteInsert FormattableString.")]
    [DataRow(DatabaseType.SqlServer, true, DisplayName = "SQL Server ExecuteInsertAsync FormattableString.")]
    public async Task TestExecuteInsertWithFormattableStringAsync(DatabaseType databaseType, bool useAsync)
    {
        using Context context = DatabaseUtils.CreateDatabase(databaseType);

        string value = DatabaseUtils.GetMethodName();
        FormattableString sql = $"INSERT INTO TestTable1 (TestField) VALUES ({value})";

        long id = useAsync
            ? await context.Database.ExecuteInsertAsync(sql).ConfigureAwait(false)
            : context.Database.ExecuteInsert(sql);

        int count = context.TestTable1.Count(e => e.Id == id);
        Assert.AreEqual(1, count, "Invalid count");
    }

    /// <summary>
    /// Test ExecuteInsert with params parameters.
    /// </summary>
    /// <param name="databaseType">Database type.</param>
    /// <param name="useAsync">If <see langword="true"/> then tests the async method.</param>
    /// <returns>A task that represents the asynchronous test operation.</returns>
    [TestMethod]
    [DataRow(DatabaseType.Sqlite, false, DisplayName = "SQLite ExecuteInsert params.")]
    [DataRow(DatabaseType.Sqlite, true, DisplayName = "SQLite ExecuteInsertAsync params.")]
    [DataRow(DatabaseType.SqlServer, false, DisplayName = "SQL Server ExecuteInsert params.")]
    [DataRow(DatabaseType.SqlServer, true, DisplayName = "SQL Server ExecuteInsertAsync params.")]
    public async Task TestExecuteInsertWithParamsAsync(DatabaseType databaseType, bool useAsync)
    {
        using Context context = DatabaseUtils.CreateDatabase(databaseType);

        string value = DatabaseUtils.GetMethodName();
        string sql = "INSERT INTO TestTable1 (TestField) VALUES ({0})";

        long id = useAsync
            ? await context.Database.ExecuteInsertAsync(sql, parameters: value).ConfigureAwait(false)
            : context.Database.ExecuteInsert(sql, value);

        int count = context.TestTable1.Count(e => e.Id == id);
        Assert.AreEqual(1, count, "Invalid count");
    }

    /// <summary>
    /// Test ExecuteInsert with IEnumerable parameter.
    /// </summary>
    /// <param name="databaseType">Database type.</param>
    /// <param name="useAsync">If <see langword="true"/> then tests the async method.</param>
    /// <returns>A task that represents the asynchronous test operation.</returns>
    [TestMethod]
    [DataRow(DatabaseType.Sqlite, false, DisplayName = "SQLite ExecuteInsert IEnumerable.")]
    [DataRow(DatabaseType.Sqlite, true, DisplayName = "SQLite ExecuteInsertAsync IEnumerable.")]
    [DataRow(DatabaseType.SqlServer, false, DisplayName = "SQL Server ExecuteInsert IEnumerable.")]
    [DataRow(DatabaseType.SqlServer, true, DisplayName = "SQL Server ExecuteInsertAsync IEnumerable.")]
    public async Task TestExecuteInsertWithIEnumerableAsync(DatabaseType databaseType, bool useAsync)
    {
        using Context context = DatabaseUtils.CreateDatabase(databaseType);

        string value = DatabaseUtils.GetMethodName();
        List<object> parameters = new () { value };
        string sql = "INSERT INTO TestTable1 (TestField) VALUES ({0})";

        long id = useAsync
            ? await context.Database.ExecuteInsertAsync(sql, parameters).ConfigureAwait(false)
            : context.Database.ExecuteInsert(sql, parameters);

        int count = context.TestTable1.Count(e => e.Id == id);
        Assert.AreEqual(1, count, "Invalid count");
    }

}
