// Copyright ©2021-2022 Mike King.
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
    /// <summary>
    /// Test ExecuteScalarInterpolated.
    /// </summary>
    /// <param name="databaseType">Database type.</param>
    /// <param name="useAsync">If <see langword="true"/> then tests the async method.</param>
    /// <returns><see cref="Task"/>.</returns>
    [TestMethod]
    [DataRow(DatabaseType.Sqlite, false, DisplayName = "SQLite TestExecuteScalarInterpolated")]
    [DataRow(DatabaseType.Sqlite, true, DisplayName = "SQLite TestExecuteScalarInterpolatedAsync")]
    [DataRow(DatabaseType.SqlServer, false, DisplayName = "SQL Server TestExecuteScalarInterpolated")]
    [DataRow(DatabaseType.SqlServer, true, DisplayName = "SQL Server TestExecuteScalarInterpolatedAsync")]
    public async Task TestExecuteScalarInterpolatedAsync(DatabaseType databaseType, bool useAsync)
    {
        string value = DatabaseUtils.GetMethodName();

        using Context context = DatabaseUtils.CreateDatabase(databaseType);
        long id = DatabaseUtils.CreateSingleTestTableEntry(context, value);

        string actualValue = useAsync
            ? await context.Database.ExecuteScalarInterpolatedAsync<string>($"SELECT TestField_1 FROM TestTable_1 WHERE ID = {id}").ConfigureAwait(false)
            : context.Database.ExecuteScalarInterpolated<string>($"SELECT TestField_1 FROM TestTable_1 WHERE ID = {id}");

        Assert.AreEqual(value, actualValue, "Invalid value");
    }

    /// <summary>
    /// Test ExecuteScalarRaw.
    /// </summary>
    /// <param name="databaseType">Database type.</param>
    /// <param name="useAsync">If <see langword="true"/> then tests the async method.</param>
    /// <returns><see cref="Task"/>.</returns>
    [TestMethod]
    [DataRow(DatabaseType.Sqlite, false, DisplayName = "SQLite TestExecuteScalarRaw")]
    [DataRow(DatabaseType.Sqlite, true, DisplayName = "SQLite TestExecuteScalarRawAsync")]
    [DataRow(DatabaseType.SqlServer, false, DisplayName = "SQL Server TestExecuteScalarRaw")]
    [DataRow(DatabaseType.SqlServer, true, DisplayName = "SQL Server TestExecuteScalarRawAsync")]
    public async Task TestExecuteScalarRawAsync(DatabaseType databaseType, bool useAsync)
    {
        string value = DatabaseUtils.GetMethodName();

        using Context context = DatabaseUtils.CreateDatabase(databaseType);
        long id = DatabaseUtils.CreateSingleTestTableEntry(context, value);

        string actualValue = useAsync
            ? await context.Database.ExecuteScalarRawAsync<string>("SELECT TestField_1 FROM TestTable_1 WHERE ID = {0}", parameters: id).ConfigureAwait(false)
            : context.Database.ExecuteScalarRaw<string>("SELECT TestField_1 FROM TestTable_1 WHERE ID = {0}", id);

        Assert.AreEqual(value, actualValue, "Invalid value");
    }
}
