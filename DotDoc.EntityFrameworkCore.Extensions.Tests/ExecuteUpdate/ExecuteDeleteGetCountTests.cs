// Copyright ©2021-2026 Mike King.
// This file is licensed to you under the MIT license.
// See the License.txt file in the solution root for more information.

using DotDoc.EntityFrameworkCore.Extensions.DatabaseType;
using DotDoc.EntityFrameworkCore.Extensions.ExecuteUpdate;
using DotDoc.EntityFrameworkCore.Extensions.Tests.Data;
using Microsoft.EntityFrameworkCore;

namespace DotDoc.EntityFrameworkCore.Extensions.Tests.ExecuteUpdate;

/// <summary>
/// Tests for ExecuteDeleteGetCount extensions.
/// </summary>
[TestClass]
public class ExecuteDeleteGetCountTests
{
    #region private fields

    /// <summary>
    /// The MSTest <see cref="TestContext"/> for the current test.
    /// </summary>
    private readonly TestContext _testContext;

    #endregion private fields

    #region public constructors

    /// <summary>
    /// Initializes a new instance of the <see cref="ExecuteDeleteGetCountTests"/> class.
    /// </summary>
    /// <param name="testContext">The MSTest test context.</param>
    public ExecuteDeleteGetCountTests(TestContext testContext)
    {
        this._testContext = testContext;
    }

    #endregion public constructors

    #region public methods

    /// <summary>
    /// Test ExecuteDeleteGetCountAsync with Null IQueryable source parameter.
    /// </summary>
    /// <returns>A <see cref="Task"/> that represents the asynchronous operation.</returns>
    [TestMethod(DisplayName = "ExecuteDeleteGetCountAsync with Null IQueryable source parameter")]
    public async Task ExecuteDeleteGetCountTests_001_Async()
    {
        // ARRANGE
        IQueryable<TestTable1> source = null!;

        // ACT / ASSERT
        await Assert.ThrowsExactlyAsync<ArgumentNullException>(() => source.ExecuteDeleteGetCountAsync(this._testContext.CancellationToken), "Unexpected exception").ConfigureAwait(false);
    }

    /// <summary>
    /// Test ExecuteDeleteGetCountAsync.
    /// </summary>
    /// <param name="databaseType">Database type.</param>
    /// <param name="count">Number of records to delete.</param>
    /// <returns>A <see cref="Task"/> that represents the asynchronous operation.</returns>
    [TestMethod(DisplayName = "ExecuteDeleteGetCountAsync")]
    [DataRow(DatabaseTypes.Sqlite, 0, DisplayName = $"{DatabaseTypes.Sqlite} Delete 0 Records")]
    [DataRow(DatabaseTypes.Sqlite, 1, DisplayName = $"{DatabaseTypes.Sqlite} Delete 1 Record")]
    [DataRow(DatabaseTypes.Sqlite, 10, DisplayName = $"{DatabaseTypes.Sqlite} Delete 10 Records")]
    [DataRow(DatabaseTypes.SqlServer, 0, DisplayName = $"{DatabaseTypes.SqlServer} Delete 0 Records")]
    [DataRow(DatabaseTypes.SqlServer, 1, DisplayName = $"{DatabaseTypes.SqlServer} Delete 1 Record")]
    [DataRow(DatabaseTypes.SqlServer, 10, DisplayName = $"{DatabaseTypes.SqlServer} Delete 10 Records")]
    public async Task ExecuteDeleteGetCountTests_002_Async(string databaseType, int count)
    {
        // ARRANGE
        using Context context = await DatabaseUtils.OpenDatabaseAsync(
            databaseType,
            customConfigurationActions: optionsBuilder => optionsBuilder.UseExecuteUpdateExtensions())
            .ConfigureAwait(false);

        string value = "TestValue";
        await DatabaseUtils.CreateTestTableEntriesAsync(context, value, (count + 1) * 10).ConfigureAwait(false);

        long startId = 2;
        long endId = startId + count;

        IQueryable<TestTable1> source = context.TestTable1.Where(e => e.Id >= startId && e.Id < endId);

        // ACT
        int deleteRowCount = await source.ExecuteDeleteGetCountAsync(this._testContext.CancellationToken).ConfigureAwait(false);

        // ASSERT
        Assert.AreEqual(count, deleteRowCount, "Invalid count");
    }

    /// <summary>
    /// Test ExecuteDeleteGetCountAsync on SQL Server when the session has SET NOCOUNT ON.
    /// </summary>
    /// <returns>A <see cref="Task"/> that represents the asynchronous operation.</returns>
    /// <remarks>
    /// EF Core starts a SQL Server DELETE with SET NOCOUNT OFF, so the count is still returned. The custom query generator must keep it,
    /// otherwise the count is -1. The connection is opened first so the SET NOCOUNT ON stays in effect for the delete.
    /// </remarks>
    [TestMethod(DisplayName = "ExecuteDeleteGetCountAsync on SQL Server with SET NOCOUNT ON")]
    public async Task ExecuteDeleteGetCountTests_003_Async()
    {
        // ARRANGE
        using Context context = await DatabaseUtils.OpenDatabaseAsync(
            DatabaseTypes.SqlServer,
            customConfigurationActions: optionsBuilder => optionsBuilder.UseExecuteUpdateExtensions())
            .ConfigureAwait(false);

        await DatabaseUtils.CreateTestTableEntriesAsync(context, "TestValue", 10).ConfigureAwait(false);

        await context.Database.OpenConnectionAsync(this._testContext.CancellationToken).ConfigureAwait(false);
        await context.Database.ExecuteSqlRawAsync("SET NOCOUNT ON", this._testContext.CancellationToken).ConfigureAwait(false);

        IQueryable<TestTable1> source = context.TestTable1.Where(e => e.Id <= 3);

        // ACT
        int deleteRowCount = await source.ExecuteDeleteGetCountAsync(this._testContext.CancellationToken).ConfigureAwait(false);

        // ASSERT
        Assert.AreEqual(3, deleteRowCount, "Invalid count");
    }

    #endregion public methods
}
