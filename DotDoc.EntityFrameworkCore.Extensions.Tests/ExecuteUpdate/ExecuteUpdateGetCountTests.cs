// Copyright ©2021-2026 Mike King.
// This file is licensed to you under the MIT license.
// See the License.txt file in the solution root for more information.

using DotDoc.EntityFrameworkCore.Extensions.DatabaseType;
using DotDoc.EntityFrameworkCore.Extensions.ExecuteUpdate;
using DotDoc.EntityFrameworkCore.Extensions.Tests.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;

namespace DotDoc.EntityFrameworkCore.Extensions.Tests.ExecuteUpdate;

/// <summary>
/// Tests for ExecuteUpdateGetCount extensions.
/// </summary>
[TestClass]
public class ExecuteUpdateGetCountTests
{
    #region public methods

    /// <summary>
    /// Test ExecuteUpdateGetCountAsync with Null IQueryable source parameter.
    /// </summary>
    /// <returns>A <see cref="Task"/> that represents the asynchronous operation.</returns>
    [TestMethod(DisplayName = "ExecuteUpdateGetCountAsync with Null IQueryable source parameter")]
    public async Task ExecuteUpdateGetCountTests_001_Async()
    {
        // ARRANGE
        IQueryable<TestTable1> source = null!;
        Action<UpdateSettersBuilder<TestTable1>> setPropertyCalls = new(builder => { });

        // ACT / ASSERT
        await Assert.ThrowsExactlyAsync<ArgumentNullException>(() => source.ExecuteUpdateGetCountAsync(setPropertyCalls, CancellationToken.None), "Unexpected exception").ConfigureAwait(false);
    }

    /// <summary>
    /// Test ExecuteUpdateGetCountAsync with Null UpdateSettersBuilder setPropertyCalls parameter.
    /// </summary>
    /// <returns>A <see cref="Task"/> that represents the asynchronous operation.</returns>
    [TestMethod(DisplayName = "ExecuteUpdateGetCountAsync with Null UpdateSettersBuilder setPropertyCalls parameter")]
    public async Task ExecuteUpdateGetCountTests_002_Async()
    {
        // ARRANGE
        IQueryable<TestTable1> source = Array.Empty<TestTable1>().AsQueryable();
        Action<UpdateSettersBuilder<TestTable1>> setPropertyCalls = null!;

        // ACT / ASSERT
        await Assert.ThrowsExactlyAsync<ArgumentNullException>(() => source.ExecuteUpdateGetCountAsync(setPropertyCalls, CancellationToken.None), "Unexpected exception").ConfigureAwait(false);
    }

    /// <summary>
    /// Test ExecuteUpdateGetCountAsync.
    /// </summary>
    /// <param name="databaseType">Database type.</param>
    /// <param name="count">Number of records to update.</param>
    /// <returns>A <see cref="Task"/> that represents the asynchronous operation.</returns>
    [TestMethod(DisplayName = "ExecuteUpdateGetCountAsync")]
    [DataRow(DatabaseTypes.Sqlite, 0, DisplayName = $"{DatabaseTypes.Sqlite} Update 0 Records")]
    [DataRow(DatabaseTypes.Sqlite, 1, DisplayName = $"{DatabaseTypes.Sqlite} Update 1 Record")]
    [DataRow(DatabaseTypes.Sqlite, 10, DisplayName = $"{DatabaseTypes.Sqlite} Update 10 Records")]
    [DataRow(DatabaseTypes.SqlServer, 0, DisplayName = $"{DatabaseTypes.SqlServer} Update 0 Records")]
    [DataRow(DatabaseTypes.SqlServer, 1, DisplayName = $"{DatabaseTypes.SqlServer} Update 1 Record")]
    [DataRow(DatabaseTypes.SqlServer, 10, DisplayName = $"{DatabaseTypes.SqlServer} Update 10 Records")]
    public async Task ExecuteUpdateGetCountTests_002_Async(string databaseType, int count)
    {
        // ARRANGE
        using Context context = await DatabaseUtils.OpenDatabaseAsync(
            databaseType,
            customConfigurationActions: optionsBuilder => optionsBuilder.UseExecuteUpdateExtensions())
            .ConfigureAwait(false);

        string originalValue = "Original TestValue";
        string updatedValue = "Updated TestValue";

        await DatabaseUtils.CreateTestTableEntriesAsync(context, originalValue, (count + 1) * 10).ConfigureAwait(false);

        long startId = 2;
        long endId = startId + count;

        IQueryable<TestTable1> source = context.TestTable1.Where(e => e.Id >= startId && e.Id < endId);

        // ACT
        int updateRowCount = await source.ExecuteUpdateGetCountAsync(
            builder =>
            {
                builder.SetProperty(e => e.TestField, updatedValue);
            },
            CancellationToken.None)
            .ConfigureAwait(false);

        // ASSERT
        Assert.AreEqual(count, updateRowCount, "Invalid count");

        IList<TestTable1> rows = await source
            .AsNoTracking()
            .ToListAsync(CancellationToken.None)
            .ConfigureAwait(false);

        foreach (TestTable1 row in rows)
        {
            Assert.AreEqual(updatedValue, row.TestField, "Unexpected field value");
        }
    }

    #endregion public methods
}
