// Copyright ©2021-2022 Mike King.
// This file is licensed to you under the MIT license.
// See the License.txt file in the solution root for more information.

using DotDoc.EntityFrameworkCore.Extensions.Constants;
using DotDoc.EntityFrameworkCore.Extensions.Extensions;
using DotDoc.EntityFrameworkCore.Extensions.Tests.Data;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Data;

namespace DotDoc.EntityFrameworkCore.Extensions.Tests;

/// <summary>
/// Tests for ExecuteQuery extensions.
/// </summary>
[TestClass]
public class ExecuteQueryTests
{
    /// <summary>
    /// Test ExecuteQueryInterpolated.
    /// </summary>
    /// <param name="databaseType">Database type.</param>
    [TestMethod]
    [DataRow(DatabaseType.Sqlite)]
    [DataRow(DatabaseType.SqlServer)]
    public void ExecuteQueryInterpolatedTest(DatabaseType databaseType)
    {
        const string value = nameof(this.ExecuteQueryInterpolatedTest);
        const int recordCount = 20;

        using Context context = DatabaseUtils.CreateDatabase(databaseType);
        DatabaseUtils.CreateMultipleTestTableEntries(context, value, recordCount);

        DataTable dataTable = context.Database.ExecuteQueryInterpolated($"SELECT * FROM TestTable WHERE ID <= {recordCount}");

        Assert.AreEqual(recordCount, dataTable.Rows.Count);
    }

    /// <summary>
    /// Test ExecuteQueryRaw.
    /// </summary>
    /// <param name="databaseType">Database type.</param>
    [TestMethod]
    [DataRow(DatabaseType.Sqlite)]
    [DataRow(DatabaseType.SqlServer)]
    public void ExecuteQueryRawTest(DatabaseType databaseType)
    {
        const string value = nameof(this.ExecuteQueryInterpolatedTest);
        const int recordCount = 20;

        using Context context = DatabaseUtils.CreateDatabase(databaseType);
        DatabaseUtils.CreateMultipleTestTableEntries(context, value, recordCount);

        DataTable dataTable = context.Database.ExecuteQueryRaw("SELECT * FROM TestTable WHERE ID <= {0}", recordCount);

        Assert.AreEqual(recordCount, dataTable.Rows.Count);
    }

    /// <summary>
    /// Test ExecuteQueryInterpolatedAsync.
    /// </summary>
    /// <param name="databaseType">Database type.</param>
    /// <returns><see cref="Task"/>.</returns>
    [TestMethod]
    [DataRow(DatabaseType.Sqlite)]
    [DataRow(DatabaseType.SqlServer)]
    public async Task ExecuteQueryInterpolatedTestAsync(DatabaseType databaseType)
    {
        const string value = nameof(this.ExecuteQueryInterpolatedTest);
        const int recordCount = 20;

        using Context context = DatabaseUtils.CreateDatabase(databaseType);
        DatabaseUtils.CreateMultipleTestTableEntries(context, value, recordCount);

        DataTable dataTable = await context.Database.ExecuteQueryInterpolatedAsync($"SELECT * FROM TestTable WHERE ID <= {recordCount}").ConfigureAwait(false);

        Assert.AreEqual(recordCount, dataTable.Rows.Count);
    }

    /// <summary>
    /// Test ExecuteQueryRawAsync.
    /// </summary>
    /// <param name="databaseType">Database type.</param>
    /// <returns><see cref="Task"/>.</returns>
    [TestMethod]
    [DataRow(DatabaseType.Sqlite)]
    [DataRow(DatabaseType.SqlServer)]
    public async Task ExecuteQueryRawTestAsync(DatabaseType databaseType)
    {
        const string value = nameof(this.ExecuteQueryInterpolatedTest);
        const int recordCount = 20;

        using Context context = DatabaseUtils.CreateDatabase(databaseType);
        DatabaseUtils.CreateMultipleTestTableEntries(context, value, recordCount);

        DataTable dataTable = await context.Database.ExecuteQueryRawAsync("SELECT * FROM TestTable WHERE ID <= {0}", parameters: recordCount).ConfigureAwait(false);

        Assert.AreEqual(recordCount, dataTable.Rows.Count);
    }
}
