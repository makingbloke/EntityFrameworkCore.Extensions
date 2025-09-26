// Copyright ©2021-2025 Mike King.
// This file is licensed to you under the MIT license.
// See the License.txt file in the solution root for more information.

using DotDoc.EntityFrameworkCore.Extensions.DatabaseType;
using DotDoc.EntityFrameworkCore.Extensions.Tests.Data;
using DotDoc.EntityFrameworkCore.Extensions.Tests.TestUtilities;
using Microsoft.EntityFrameworkCore;

namespace DotDoc.EntityFrameworkCore.Extensions.Tests.ExecuteDelete;

/// <summary>
/// Tests for ExecuteDeleteGetCount extensions.
/// </summary>
[TestClass]
public class ExecuteDeleteGetCountTests
{
    #region public methods

    /// <summary>
    /// Test ExecuteDeleteGetCountAsync Guard Clauses.
    /// </summary>
    /// <returns>A <see cref="Task"/> that represents the asynchronous operation.</returns>
    [TestMethod(DisplayName = "ExecuteDeleteGetCountAsync Guard Clauses")]
    public async Task Test_ExecuteDeleteGetCountAsync_GuardClauses_Async()
    {
        // ARRANGE
        IQueryable<TestTable1>? query = null;

        // ACT / ASSERT
        await Assert.ThrowsExactlyAsync<ArgumentNullException>(() => query!.ExecuteDeleteGetCountAsync(CancellationToken.None), "Unexpected exception").ConfigureAwait(false);
    }

    /// <summary>
    /// Test ExecuteDeleteGetCountAsync.
    /// </summary>
    /// <param name="databaseType">Database type.</param>
    /// <param name="count">Number of records to Delete.</param>
    /// <returns>A <see cref="Task"/> that represents the asynchronous operation.</returns>
    [TestMethod(DisplayName = "ExecuteDeleteGetCountAsync")]
    [DataRow(DatabaseTypes.Sqlite, 0, DisplayName = $"{DatabaseTypes.Sqlite} Delete 0 Records.")]
    [DataRow(DatabaseTypes.Sqlite, 1, DisplayName = $"{DatabaseTypes.Sqlite} Delete 1 Record.")]
    [DataRow(DatabaseTypes.Sqlite, 10, DisplayName = $"{DatabaseTypes.Sqlite} Delete 10 Records.")]
    [DataRow(DatabaseTypes.SqlServer, 0, DisplayName = $"{DatabaseTypes.SqlServer} Delete 0 Records.")]
    [DataRow(DatabaseTypes.SqlServer, 1, DisplayName = $"{DatabaseTypes.SqlServer} Delete 1 Record.")]
    [DataRow(DatabaseTypes.SqlServer, 10, DisplayName = $"{DatabaseTypes.SqlServer} Delete 10 Records.")]
    public async Task Test_ExecuteDeleteGetCountAsync_Async(string databaseType, int count)
    {
        // ARRANGE
        using Context context = await DatabaseUtils.CreateDatabaseAsync(
            databaseType,
            customConfigurationActions: (optionsBuilder) => optionsBuilder.UseExecuteDeleteExtensions())
            .ConfigureAwait(false);

        string value = "TestValue";
        string originalValue = $"Original {value}";

        await DatabaseUtils.CreateTestTableEntriesAsync(context, originalValue, (count + 1) * 10).ConfigureAwait(false);

        long startId = 2;
        long endId = startId + count - 1;

        IQueryable<TestTable1> query = context.TestTable1.Where(e => e.Id >= startId && e.Id <= endId);

        // ACT
        int deleteRowCount = await query.ExecuteDeleteGetCountAsync(CancellationToken.None).ConfigureAwait(false);

        // ASSERT
        Assert.AreEqual(count, deleteRowCount, "Invalid count");
    }

    #endregion public methods
}
