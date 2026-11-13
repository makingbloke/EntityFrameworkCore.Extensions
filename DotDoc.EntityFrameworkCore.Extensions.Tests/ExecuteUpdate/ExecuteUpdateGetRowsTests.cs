// Copyright ©2021-2026 Mike King.
// This file is licensed to you under the MIT license.
// See the License.txt file in the solution root for more information.

using DotDoc.EntityFrameworkCore.Extensions.DatabaseType;
using DotDoc.EntityFrameworkCore.Extensions.ExecuteUpdate;
using DotDoc.EntityFrameworkCore.Extensions.Tests.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Query;
using System.Data.Common;

namespace DotDoc.EntityFrameworkCore.Extensions.Tests.ExecuteUpdate;

/// <summary>
/// Tests for ExecuteUpdateGetRows extensions.
/// </summary>
[TestClass]
public class ExecuteUpdateGetRowsTests
{
    #region private fields

    /// <summary>
    /// The MSTest <see cref="TestContext"/> for the current test.
    /// </summary>
    private readonly TestContext _testContext;

    #endregion private fields

    #region public constructors

    /// <summary>
    /// Initializes a new instance of the <see cref="ExecuteUpdateGetRowsTests"/> class.
    /// </summary>
    /// <param name="testContext">The MSTest test context.</param>
    public ExecuteUpdateGetRowsTests(TestContext testContext)
    {
        this._testContext = testContext;
    }

    #endregion public constructors

    #region ExecuteUpdateGetRowsAsync tests

    /// <summary>
    /// Test ExecuteUpdateGetRowsAsync with Null IQueryable source parameter.
    /// </summary>
    /// <returns>A <see cref="Task"/> that represents the asynchronous operation.</returns>
    [TestMethod(DisplayName = "ExecuteUpdateGetRowsAsync with Null IQueryable source parameter")]
    public async Task ExecuteUpdateGetRowsTests_001_Async()
    {
        // ARRANGE
        IQueryable<TestTable1> source = null!;
        Action<UpdateSettersBuilder<TestTable1>> setPropertyCalls = new(builder => { });

        // ACT / ASSERT
        await Assert.ThrowsExactlyAsync<ArgumentNullException>(() => source.ExecuteUpdateGetRowsAsync(setPropertyCalls, this._testContext.CancellationToken), "Unexpected exception").ConfigureAwait(false);
    }

    /// <summary>
    /// Test ExecuteUpdateGetRowsAsync with Null UpdateSettersBuilder setPropertyCalls parameter.
    /// </summary>
    /// <returns>A <see cref="Task"/> that represents the asynchronous operation.</returns>
    [TestMethod(DisplayName = "ExecuteUpdateGetRowsAsync with Null UpdateSettersBuilder setPropertyCalls parameter")]
    public async Task ExecuteUpdateGetRowsTests_002_Async()
    {
        // ARRANGE
        IQueryable<TestTable1> source = Array.Empty<TestTable1>().AsQueryable();
        Action<UpdateSettersBuilder<TestTable1>> setPropertyCalls = null!;

        // ACT / ASSERT
        await Assert.ThrowsExactlyAsync<ArgumentNullException>(() => source.ExecuteUpdateGetRowsAsync(setPropertyCalls, this._testContext.CancellationToken), "Unexpected exception").ConfigureAwait(false);
    }

    /// <summary>
    /// Test ExecuteUpdateGetRowsAsync.
    /// </summary>
    /// <param name="databaseType">Database type.</param>
    /// <param name="useReturningClause">If true update uses returning clause in sql else uses a select after update is completed.</param>
    /// <param name="count">Number of records to update.</param>
    /// <returns>A <see cref="Task"/> that represents the asynchronous operation.</returns>
    [TestMethod(DisplayName = "ExecuteUpdateGetRowsAsync")]
    [DataRow(DatabaseTypes.Sqlite, true, 0, DisplayName = $"{DatabaseTypes.Sqlite} Use Returning Clause Update 0 Records")]
    [DataRow(DatabaseTypes.Sqlite, false, 0, DisplayName = $"{DatabaseTypes.Sqlite} Use Select Statement Update 0 Records")]
    [DataRow(DatabaseTypes.Sqlite, true, 1, DisplayName = $"{DatabaseTypes.Sqlite} Use Returning Clause Update 1 Record")]
    [DataRow(DatabaseTypes.Sqlite, false, 1, DisplayName = $"{DatabaseTypes.Sqlite} Use Select Statement Update 1 Record")]
    [DataRow(DatabaseTypes.Sqlite, true, 10, DisplayName = $"{DatabaseTypes.Sqlite} Use Returning Clause Update 10 Records")]
    [DataRow(DatabaseTypes.Sqlite, false, 10, DisplayName = $"{DatabaseTypes.Sqlite} Use Select Statement Update 10 Records")]
    [DataRow(DatabaseTypes.SqlServer, true, 0, DisplayName = $"{DatabaseTypes.SqlServer} Use Returning Clause Update 0 Records")]
    [DataRow(DatabaseTypes.SqlServer, false, 0, DisplayName = $"{DatabaseTypes.SqlServer} Use Select Statement Update 0 Records")]
    [DataRow(DatabaseTypes.SqlServer, true, 1, DisplayName = $"{DatabaseTypes.SqlServer} Use Returning Clause Update 1 Record")]
    [DataRow(DatabaseTypes.SqlServer, false, 1, DisplayName = $"{DatabaseTypes.SqlServer} Use Select Statement Update 1 Record")]
    [DataRow(DatabaseTypes.SqlServer, true, 10, DisplayName = $"{DatabaseTypes.SqlServer} Use Returning Clause Update 10 Records")]
    [DataRow(DatabaseTypes.SqlServer, false, 10, DisplayName = $"{DatabaseTypes.SqlServer} Use Select Statement Update 10 Records")]
    public async Task ExecuteUpdateGetRowsTests_003_Async(string databaseType, bool useReturningClause, int count)
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

        string originalValue = "Original TestValue";
        string updatedValue = "Updated TestValue";

        await DatabaseUtils.CreateTestTableEntriesAsync(context, originalValue, (count + 1) * 10).ConfigureAwait(false);

        long startId = 2;
        long endId = startId + count;

        IQueryable<TestTable1> source = context.TestTable1.Where(e => e.Id >= startId && e.Id < endId);

        // ACT
        IList<TestTable1> rows = await source.ExecuteUpdateGetRowsAsync(
            builder =>
            {
                builder.SetProperty(e => e.TestField, updatedValue);
            },
            this._testContext.CancellationToken)
            .ConfigureAwait(false);

        // ASSERT
        ExecuteUpdateTestUtils.AssertStateCleared(context);

        Assert.HasCount(count, rows, "Invalid count");

        long id = startId;
        foreach (TestTable1 row in rows.OrderBy(r => r.Id))
        {
            Assert.AreEqual(id, row.Id, "Unexpected Id value");
            Assert.AreEqual(updatedValue, row.TestField, "Unexpected field value");

            id++;
        }
    }

    /// <summary>
    /// Test ExecuteUpdateGetRowsAsync when another ExecuteUpdate call is in progress on the DbContext.
    /// </summary>
    /// <param name="databaseType">Database type.</param>
    /// <returns>A <see cref="Task"/> that represents the asynchronous operation.</returns>
    /// <remarks>
    /// The context's <see cref="ExecuteUpdateState"/> only holds a query type while a call is running, so one already being set means
    /// another call is in progress. The call must throw without updating anything, and must leave the other call's state alone.
    /// </remarks>
    [TestMethod(DisplayName = "ExecuteUpdateGetRowsAsync when another ExecuteUpdate call is in progress on the DbContext")]
    [DataRow(DatabaseTypes.Sqlite, DisplayName = DatabaseTypes.Sqlite)]
    [DataRow(DatabaseTypes.SqlServer, DisplayName = DatabaseTypes.SqlServer)]
    public async Task ExecuteUpdateGetRowsTests_004_Async(string databaseType)
    {
        // ARRANGE
        using Context context = await DatabaseUtils.OpenDatabaseAsync(
            databaseType,
            customConfigurationActions: optionsBuilder => optionsBuilder.UseExecuteUpdateExtensions())
            .ConfigureAwait(false);

        string originalValue = "Original TestValue";

        await DatabaseUtils.CreateTestTableEntriesAsync(context, originalValue, 5).ConfigureAwait(false);

        Action<UpdateSettersBuilder<TestTable1>> setPropertyCalls = new(builder => builder.SetProperty(e => e.TestField, "Updated TestValue"));

        ExecuteUpdateState state = context.GetService<ExecuteUpdateState>();
        state.SetQueryType(QueryType.InsertGetRow);

        // ACT / ASSERT
        await Assert.ThrowsExactlyAsync<InvalidOperationException>(() => context.TestTable1.ExecuteUpdateGetRowsAsync(setPropertyCalls, this._testContext.CancellationToken), "Unexpected exception").ConfigureAwait(false);

        Assert.AreEqual(QueryType.InsertGetRow, state.QueryType, "The state of the call in progress has been changed.");

        await state.ResetStateAsync(this._testContext.CancellationToken).ConfigureAwait(false);

        List<TestTable1> rows = await context.TestTable1
            .AsNoTracking()
            .ToListAsync(this._testContext.CancellationToken)
            .ConfigureAwait(false);

        Assert.IsTrue(rows.All(r => r.TestField == originalValue), "Rows have been updated");
    }

    /// <summary>
    /// Test ExecuteUpdateGetRowsAsync passes on the database exception when the update fails.
    /// </summary>
    /// <param name="databaseType">Database type.</param>
    /// <returns>A <see cref="Task"/> that represents the asynchronous operation.</returns>
    /// <remarks>
    /// Setting every row to the same value breaks the unique index on <see cref="TestTable2.TestField"/>. The context's
    /// <see cref="ExecuteUpdateState"/> must still be cleared, so that a later call on the same context works.
    /// </remarks>
    [TestMethod(DisplayName = "ExecuteUpdateGetRowsAsync passes on the database exception when the update fails")]
    [DataRow(DatabaseTypes.Sqlite, DisplayName = DatabaseTypes.Sqlite)]
    [DataRow(DatabaseTypes.SqlServer, DisplayName = DatabaseTypes.SqlServer)]
    public async Task ExecuteUpdateGetRowsTests_005_Async(string databaseType)
    {
        // ARRANGE
        using Context context = await DatabaseUtils.OpenDatabaseAsync(
            databaseType,
            customConfigurationActions: optionsBuilder => optionsBuilder.UseExecuteUpdateExtensions())
            .ConfigureAwait(false);

        TestTable2 first = new() { TestField = "First TestValue" };
        context.Add(first);
        context.Add(new TestTable2 { TestField = "Second TestValue" });
        await context.SaveChangesAsync(this._testContext.CancellationToken).ConfigureAwait(false);

        string updatedValue = "Updated TestValue";

        // ACT / ASSERT
        await Assert.ThrowsAsync<DbException>(() => context.TestTable2.ExecuteUpdateGetRowsAsync(builder => builder.SetProperty(e => e.TestField, updatedValue), this._testContext.CancellationToken), "Unexpected exception").ConfigureAwait(false);

        ExecuteUpdateTestUtils.AssertStateCleared(context);

        long firstId = first.Id;
        IList<TestTable2> rows = await context.TestTable2
            .Where(e => e.Id == firstId)
            .ExecuteUpdateGetRowsAsync(builder => builder.SetProperty(e => e.TestField, updatedValue), this._testContext.CancellationToken)
            .ConfigureAwait(false);

        Assert.HasCount(1, rows, "Unexpected count after the failed update");
        Assert.AreEqual(updatedValue, rows[0].TestField, "Unexpected field value after the failed update");
    }

    /// <summary>
    /// Test ExecuteUpdateGetRowsAsync without UseExecuteUpdateExtensions.
    /// </summary>
    /// <param name="databaseType">Database type.</param>
    /// <returns>A <see cref="Task"/> that represents the asynchronous operation.</returns>
    /// <remarks>Without the extensions the update would run, then fail because no SQL was captured, so it must throw before anything is updated.</remarks>
    [TestMethod(DisplayName = "ExecuteUpdateGetRowsAsync without UseExecuteUpdateExtensions")]
    [DataRow(DatabaseTypes.Sqlite, DisplayName = DatabaseTypes.Sqlite)]
    [DataRow(DatabaseTypes.SqlServer, DisplayName = DatabaseTypes.SqlServer)]
    public async Task ExecuteUpdateGetRowsTests_006_Async(string databaseType)
    {
        // ARRANGE
        using Context context = await DatabaseUtils.OpenDatabaseAsync(databaseType).ConfigureAwait(false);

        string originalValue = "Original TestValue";

        await DatabaseUtils.CreateTestTableEntriesAsync(context, originalValue, 5).ConfigureAwait(false);

        Action<UpdateSettersBuilder<TestTable1>> setPropertyCalls = new(builder => builder.SetProperty(e => e.TestField, "Updated TestValue"));

        // ACT / ASSERT
        InvalidOperationException exception = await Assert.ThrowsExactlyAsync<InvalidOperationException>(() => context.TestTable1.ExecuteUpdateGetRowsAsync(setPropertyCalls, this._testContext.CancellationToken), "Unexpected exception").ConfigureAwait(false);

        Assert.Contains(nameof(ExecuteUpdateExtensions.UseExecuteUpdateExtensions), exception.Message, "Unexpected exception message");

        List<TestTable1> rows = await context.TestTable1
            .AsNoTracking()
            .ToListAsync(this._testContext.CancellationToken)
            .ConfigureAwait(false);

        Assert.HasCount(5, rows, "Unexpected count");
        Assert.IsTrue(rows.All(r => r.TestField == originalValue), "Rows have been updated");
    }

    #endregion ExecuteUpdateGetRowsAsync tests
}
