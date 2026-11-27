// Copyright ©2021-2026 Mike King.
// This file is licensed to you under the MIT license.
// See the License.txt file in the solution root for more information.

using DotDoc.EntityFrameworkCore.Extensions.DatabaseType;
using DotDoc.EntityFrameworkCore.Extensions.ExecuteUpdate;
using DotDoc.EntityFrameworkCore.Extensions.Tests.Data;
using DotDoc.EntityFrameworkCore.Extensions.Tests.Model;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;

namespace DotDoc.EntityFrameworkCore.Extensions.Tests.ExecuteUpdate;

/// <summary>
/// Tests for ExecuteUpdateGetCount extensions.
/// </summary>
[TestClass]
public class ExecuteUpdateGetCountTests
{
    #region private fields

    /// <summary>
    /// The MSTest <see cref="TestContext"/> for the current test.
    /// </summary>
    private readonly TestContext _testContext;

    #endregion private fields

    #region public constructors

    /// <summary>
    /// Initializes a new instance of the <see cref="ExecuteUpdateGetCountTests"/> class.
    /// </summary>
    /// <param name="testContext">The MSTest test context.</param>
    public ExecuteUpdateGetCountTests(TestContext testContext)
    {
        this._testContext = testContext;
    }

    #endregion public constructors

    #region ExecuteUpdateGetCountAsync tests

    /// <summary>
    /// Test ExecuteUpdateGetCountAsync with Null IQueryable source parameter.
    /// </summary>
    /// <returns>A <see cref="Task"/> that represents the asynchronous operation.</returns>
    [TestMethod]
    public async Task ExecuteUpdateGetCountAsync_NullSource_ThrowsArgumentNullException()
    {
        // ARRANGE
        IQueryable<TestTable1> source = null!;
        Action<UpdateSettersBuilder<TestTable1>> setPropertyCalls = new(builder => { });

        // ACT / ASSERT
        await Assert.ThrowsExactlyAsync<ArgumentNullException>(() => source.ExecuteUpdateGetCountAsync(setPropertyCalls, this._testContext.CancellationToken), "Unexpected exception").ConfigureAwait(false);
    }

    /// <summary>
    /// Test ExecuteUpdateGetCountAsync with Null UpdateSettersBuilder setPropertyCalls parameter.
    /// </summary>
    /// <returns>A <see cref="Task"/> that represents the asynchronous operation.</returns>
    [TestMethod]
    public async Task ExecuteUpdateGetCountAsync_NullSetPropertyCalls_ThrowsArgumentNullException()
    {
        // ARRANGE
        IQueryable<TestTable1> source = Array.Empty<TestTable1>().AsQueryable();
        Action<UpdateSettersBuilder<TestTable1>> setPropertyCalls = null!;

        // ACT / ASSERT
        await Assert.ThrowsExactlyAsync<ArgumentNullException>(() => source.ExecuteUpdateGetCountAsync(setPropertyCalls, this._testContext.CancellationToken), "Unexpected exception").ConfigureAwait(false);
    }

    /// <summary>
    /// Test ExecuteUpdateGetCountAsync.
    /// </summary>
    /// <param name="databaseType">Database type.</param>
    /// <param name="count">Number of records to update.</param>
    /// <returns>A <see cref="Task"/> that represents the asynchronous operation.</returns>
    [TestMethod]
    [DataRow(DatabaseTypes.Sqlite, 0, DisplayName = $"Where {DatabaseTypes.Sqlite} 0 Records")]
    [DataRow(DatabaseTypes.Sqlite, 1, DisplayName = $"Where {DatabaseTypes.Sqlite} 1 Record")]
    [DataRow(DatabaseTypes.Sqlite, 10, DisplayName = $"Where {DatabaseTypes.Sqlite} 10 Records")]
    [DataRow(DatabaseTypes.SqlServer, 0, DisplayName = $"Where {DatabaseTypes.SqlServer} 0 Records")]
    [DataRow(DatabaseTypes.SqlServer, 1, DisplayName = $"Where {DatabaseTypes.SqlServer} 1 Record")]
    [DataRow(DatabaseTypes.SqlServer, 10, DisplayName = $"Where {DatabaseTypes.SqlServer} 10 Records")]
    public async Task ExecuteUpdateGetCountAsync_Where_UpdatesOnlyMatchingRowsAndReturnsCount(string databaseType, int count)
    {
        // ARRANGE
        using Context context = await DatabaseUtils.OpenDatabaseAsync(
            this._testContext,
            databaseType,
            customConfigurationActions: optionsBuilder => optionsBuilder.UseExecuteUpdateExtensions())
            .ConfigureAwait(false);

        string originalValue = "Original TestValue";
        string updatedValue = "Updated TestValue";

        await DatabaseUtils.CreateTestTableEntriesAsync(context, originalValue, (count + 1) * 10, this._testContext.CancellationToken).ConfigureAwait(false);

        long startId = 2;
        long endId = startId + count;

        IQueryable<TestTable1> source = context.TestTable1.Where(e => e.Id >= startId && e.Id < endId);

        // ACT
        int updateRowCount = await source.ExecuteUpdateGetCountAsync(
            builder =>
            {
                builder.SetProperty(e => e.TestField, updatedValue);
            },
            this._testContext.CancellationToken)
            .ConfigureAwait(false);

        // ASSERT
        Assert.AreEqual(count, updateRowCount, "Invalid count");

        IList<TestTable1> rows = await source
            .AsNoTracking()
            .ToListAsync(this._testContext.CancellationToken)
            .ConfigureAwait(false);

        foreach (TestTable1 row in rows)
        {
            Assert.AreEqual(updatedValue, row.TestField, "Unexpected field value");
        }

        List<TestTable1> otherRows = await context.TestTable1
            .Where(e => e.Id < startId || e.Id >= endId)
            .AsNoTracking()
            .ToListAsync(this._testContext.CancellationToken)
            .ConfigureAwait(false);

        Assert.HasCount(((count + 1) * 10) - count, otherRows, "Unexpected number of rows outside the query");
        Assert.IsTrue(otherRows.All(r => r.TestField == originalValue), "Rows outside the query have been updated");
    }

    /// <summary>
    /// Test ExecuteUpdateGetCountAsync on SQL Server when the session has SET NOCOUNT ON.
    /// </summary>
    /// <returns>A <see cref="Task"/> that represents the asynchronous operation.</returns>
    /// <remarks>
    /// EF Core starts a SQL Server UPDATE with SET NOCOUNT OFF, so the count is still returned. The custom query generator must keep it,
    /// otherwise the count is -1. The connection is opened first so the SET NOCOUNT ON stays in effect for the update.
    /// </remarks>
    [TestMethod]
    public async Task ExecuteUpdateGetCountAsync_SqlServerNoCountOn_StillReturnsCount()
    {
        // ARRANGE
        using Context context = await DatabaseUtils.OpenDatabaseAsync(
            this._testContext,
            DatabaseTypes.SqlServer,
            customConfigurationActions: optionsBuilder => optionsBuilder.UseExecuteUpdateExtensions())
            .ConfigureAwait(false);

        string updatedValue = "Updated TestValue";

        await DatabaseUtils.CreateTestTableEntriesAsync(context, "Original TestValue", 10, this._testContext.CancellationToken).ConfigureAwait(false);

        await context.Database.OpenConnectionAsync(this._testContext.CancellationToken).ConfigureAwait(false);
        await context.Database.ExecuteSqlRawAsync("SET NOCOUNT ON", this._testContext.CancellationToken).ConfigureAwait(false);

        IQueryable<TestTable1> source = context.TestTable1.Where(e => e.Id <= 3);

        // ACT
        int updateRowCount = await source.ExecuteUpdateGetCountAsync(
            builder =>
            {
                builder.SetProperty(e => e.TestField, updatedValue);
            },
            this._testContext.CancellationToken)
            .ConfigureAwait(false);

        // ASSERT
        Assert.AreEqual(3, updateRowCount, "Invalid count");
    }

    #endregion ExecuteUpdateGetCountAsync tests
}
