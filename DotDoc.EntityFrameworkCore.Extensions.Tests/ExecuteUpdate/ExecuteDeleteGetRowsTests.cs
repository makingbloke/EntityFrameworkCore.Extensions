// Copyright ©2021-2026 Mike King.
// This file is licensed to you under the MIT license.
// See the License.txt file in the solution root for more information.

using DotDoc.EntityFrameworkCore.Extensions.DatabaseType;
using DotDoc.EntityFrameworkCore.Extensions.ExecuteUpdate;
using DotDoc.EntityFrameworkCore.Extensions.Tests.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DotDoc.EntityFrameworkCore.Extensions.Tests.ExecuteUpdate;

/// <summary>
/// Tests for ExecuteDeleteGetRows extensions.
/// </summary>
[TestClass]
public class ExecuteDeleteGetRowsTests
{
    #region private fields

    /// <summary>
    /// The MSTest <see cref="TestContext"/> for the current test.
    /// </summary>
    private readonly TestContext _testContext;

    #endregion private fields

    #region public constructors

    /// <summary>
    /// Initializes a new instance of the <see cref="ExecuteDeleteGetRowsTests"/> class.
    /// </summary>
    /// <param name="testContext">The MSTest test context.</param>
    public ExecuteDeleteGetRowsTests(TestContext testContext)
    {
        this._testContext = testContext;
    }

    #endregion public constructors

    #region ExecuteDeleteGetRowsAsync tests

    /// <summary>
    /// Test ExecuteDeleteGetRowsAsync with Null IQueryable source parameter.
    /// </summary>
    /// <returns>A <see cref="Task"/> that represents the asynchronous operation.</returns>
    [TestMethod(DisplayName = "ExecuteDeleteGetRowsAsync with Null IQueryable source parameter")]
    public async Task ExecuteDeleteGetRowsTests_001_Async()
    {
        // ARRANGE
        IQueryable<TestTable1> source = null!;

        // ACT / ASSERT
        await Assert.ThrowsExactlyAsync<ArgumentNullException>(() => source.ExecuteDeleteGetRowsAsync(this._testContext.CancellationToken), "Unexpected exception").ConfigureAwait(false);
    }

    /// <summary>
    /// Test ExecuteDeleteGetRowsAsync.
    /// </summary>
    /// <param name="databaseType">Database type.</param>
    /// <param name="useReturningClause">If true delete uses returning clause in sql else uses a select after delete is completed.</param>
    /// <param name="count">Number of records to delete.</param>
    /// <returns>A <see cref="Task"/> that represents the asynchronous operation.</returns>
    [TestMethod(DisplayName = "ExecuteDeleteGetRowsAsync")]
    [DataRow(DatabaseTypes.Sqlite, true, 0, DisplayName = $"{DatabaseTypes.Sqlite} Use Returning Clause Delete 0 Records")]
    [DataRow(DatabaseTypes.Sqlite, false, 0, DisplayName = $"{DatabaseTypes.Sqlite} Use Select Statement Delete 0 Records")]
    [DataRow(DatabaseTypes.Sqlite, true, 1, DisplayName = $"{DatabaseTypes.Sqlite} Use Returning Clause Delete 1 Record")]
    [DataRow(DatabaseTypes.Sqlite, false, 1, DisplayName = $"{DatabaseTypes.Sqlite} Use Select Statement Delete 1 Record")]
    [DataRow(DatabaseTypes.Sqlite, true, 10, DisplayName = $"{DatabaseTypes.Sqlite} Use Returning Clause Delete 10 Records")]
    [DataRow(DatabaseTypes.Sqlite, false, 10, DisplayName = $"{DatabaseTypes.Sqlite} Use Select Statement Delete 10 Records")]
    [DataRow(DatabaseTypes.SqlServer, true, 0, DisplayName = $"{DatabaseTypes.SqlServer} Use Returning Clause Delete 0 Records")]
    [DataRow(DatabaseTypes.SqlServer, false, 0, DisplayName = $"{DatabaseTypes.SqlServer} Use Select Statement Delete 0 Records")]
    [DataRow(DatabaseTypes.SqlServer, true, 1, DisplayName = $"{DatabaseTypes.SqlServer} Use Returning Clause Delete 1 Record")]
    [DataRow(DatabaseTypes.SqlServer, false, 1, DisplayName = $"{DatabaseTypes.SqlServer} Use Select Statement Delete 1 Record")]
    [DataRow(DatabaseTypes.SqlServer, true, 10, DisplayName = $"{DatabaseTypes.SqlServer} Use Returning Clause Delete 10 Records")]
    [DataRow(DatabaseTypes.SqlServer, false, 10, DisplayName = $"{DatabaseTypes.SqlServer} Use Select Statement Delete 10 Records")]
    public async Task ExecuteDeleteGetRowsTests_002_Async(string databaseType, bool useReturningClause, int count)
    {
        // ARRANGE
        using Context context = await DatabaseUtils.OpenDatabaseAsync(
            databaseType: databaseType,
            customConfigurationActions: (optionsBuilder) =>
            {
                // Stop EF core caching the service provider so a new model gets
                // created every time and uses the desired useReturningClause value.
                optionsBuilder.EnableServiceProviderCaching(false);

                optionsBuilder.UseExecuteUpdateExtensions();
            },
            customModelCreationActions: modelBuilder =>
            {
                Action<TableBuilder<TestTable1>> buildAction = databaseType switch
                {
                    DatabaseTypes.Sqlite => tableBuilder => tableBuilder.UseSqlReturningClause(useReturningClause),
                    DatabaseTypes.SqlServer => tableBuilder => tableBuilder.UseSqlOutputClause(useReturningClause),
                    _ => throw new UnsupportedDatabaseTypeException()
                };

                modelBuilder.Entity<TestTable1>()
                    .ToTable(buildAction);
            })
            .ConfigureAwait(false);

        string value = "TestValue";
        await DatabaseUtils.CreateTestTableEntriesAsync(context, value, (count + 1) * 10).ConfigureAwait(false);

        long startId = 2;
        long endId = startId + count;

        IQueryable<TestTable1> source = context.TestTable1.Where(e => e.Id >= startId && e.Id < endId);

        // ACT
        IList<TestTable1> rows = await source.ExecuteDeleteGetRowsAsync(this._testContext.CancellationToken).ConfigureAwait(false);

        // ASSERT
        ExecuteUpdateTestUtils.AssertStateCleared(context);

        Assert.HasCount(count, rows, "Invalid count");

        long id = startId;
        foreach (TestTable1 row in rows.OrderBy(r => r.Id))
        {
            Assert.AreEqual(id, row.Id, "Unexpected Id value");
            Assert.AreEqual(value, row.TestField, "Unexpected field value");

            id++;
        }
    }

    /// <summary>
    /// Test ExecuteDeleteGetRowsAsync when another ExecuteUpdate call is in progress on the DbContext.
    /// </summary>
    /// <param name="databaseType">Database type.</param>
    /// <returns>A <see cref="Task"/> that represents the asynchronous operation.</returns>
    /// <remarks>
    /// The context's <see cref="ExecuteUpdateState"/> only holds a query type while a call is running, so one already being set means
    /// another call is in progress. The call must throw without deleting anything, and must leave the other call's state alone.
    /// </remarks>
    [TestMethod(DisplayName = "ExecuteDeleteGetRowsAsync when another ExecuteUpdate call is in progress on the DbContext")]
    [DataRow(DatabaseTypes.Sqlite, DisplayName = DatabaseTypes.Sqlite)]
    [DataRow(DatabaseTypes.SqlServer, DisplayName = DatabaseTypes.SqlServer)]
    public async Task ExecuteDeleteGetRowsTests_003_Async(string databaseType)
    {
        // ARRANGE
        using Context context = await DatabaseUtils.OpenDatabaseAsync(
            databaseType,
            customConfigurationActions: optionsBuilder => optionsBuilder.UseExecuteUpdateExtensions())
            .ConfigureAwait(false);

        await DatabaseUtils.CreateTestTableEntriesAsync(context, "TestValue", 1).ConfigureAwait(false);

        ExecuteUpdateState state = context.GetService<ExecuteUpdateState>();
        state.SetQueryType(QueryType.UpdateGetRows);

        // ACT / ASSERT
        await Assert.ThrowsExactlyAsync<InvalidOperationException>(() => context.TestTable1.ExecuteDeleteGetRowsAsync(this._testContext.CancellationToken), "Unexpected exception").ConfigureAwait(false);

        Assert.AreEqual(QueryType.UpdateGetRows, state.QueryType, "The state of the call in progress has been changed.");

        await state.ResetStateAsync(this._testContext.CancellationToken).ConfigureAwait(false);

        int rowCount = await context.TestTable1.CountAsync(this._testContext.CancellationToken).ConfigureAwait(false);
        Assert.AreEqual(1, rowCount, "Unexpected count");
    }

    /// <summary>
    /// Test ExecuteDeleteGetRowsAsync without UseExecuteUpdateExtensions.
    /// </summary>
    /// <param name="databaseType">Database type.</param>
    /// <returns>A <see cref="Task"/> that represents the asynchronous operation.</returns>
    /// <remarks>Without the extensions the delete would run, then fail because no SQL was captured, so it must throw before anything is deleted.</remarks>
    [TestMethod(DisplayName = "ExecuteDeleteGetRowsAsync without UseExecuteUpdateExtensions")]
    [DataRow(DatabaseTypes.Sqlite, DisplayName = DatabaseTypes.Sqlite)]
    [DataRow(DatabaseTypes.SqlServer, DisplayName = DatabaseTypes.SqlServer)]
    public async Task ExecuteDeleteGetRowsTests_004_Async(string databaseType)
    {
        // ARRANGE
        using Context context = await DatabaseUtils.OpenDatabaseAsync(databaseType).ConfigureAwait(false);

        await DatabaseUtils.CreateTestTableEntriesAsync(context, "TestValue", 5).ConfigureAwait(false);

        // ACT / ASSERT
        InvalidOperationException exception = await Assert.ThrowsExactlyAsync<InvalidOperationException>(() => context.TestTable1.ExecuteDeleteGetRowsAsync(this._testContext.CancellationToken), "Unexpected exception").ConfigureAwait(false);

        Assert.Contains(nameof(ExecuteUpdateExtensions.UseExecuteUpdateExtensions), exception.Message, "Unexpected exception message");

        int rowCount = await context.TestTable1.CountAsync(this._testContext.CancellationToken).ConfigureAwait(false);
        Assert.AreEqual(5, rowCount, "Unexpected count");
    }

    #endregion ExecuteDeleteGetRowsAsync tests
}
