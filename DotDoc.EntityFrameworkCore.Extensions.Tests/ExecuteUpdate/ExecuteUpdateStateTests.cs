// Copyright ©2021-2026 Mike King.
// This file is licensed to you under the MIT license.
// See the License.txt file in the solution root for more information.

using DotDoc.EntityFrameworkCore.Extensions.DatabaseType;
using DotDoc.EntityFrameworkCore.Extensions.ExecuteUpdate;
using DotDoc.EntityFrameworkCore.Extensions.Tests.Data;
using DotDoc.EntityFrameworkCore.Extensions.Tests.Model;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;

namespace DotDoc.EntityFrameworkCore.Extensions.Tests.ExecuteUpdate;

/// <summary>
/// Tests for <see cref="ExecuteUpdateState"/>, and how it is registered and reset.
/// </summary>
[TestClass]
public class ExecuteUpdateStateTests
{
    #region private fields

    /// <summary>
    /// The MSTest <see cref="TestContext"/> for the current test.
    /// </summary>
    private readonly TestContext _testContext;

    #endregion private fields

    #region public constructors

    /// <summary>
    /// Initializes a new instance of the <see cref="ExecuteUpdateStateTests"/> class.
    /// </summary>
    /// <param name="testContext">The MSTest test context.</param>
    public ExecuteUpdateStateTests(TestContext testContext)
    {
        this._testContext = testContext;
    }

    #endregion public constructors

    #region constructor tests

    /// <summary>
    /// Test a new <see cref="ExecuteUpdateState"/> holds no call.
    /// </summary>
    [TestMethod]
    public void Constructor_Default_HoldsNoCall()
    {
        // ACT
        ExecuteUpdateState state = new();

        // ASSERT
        Assert.AreEqual(QueryType.Unknown, state.QueryType, "Unexpected query type");
        Assert.IsNull(state.Sql, "Unexpected SQL");
        Assert.IsNull(state.Parameters, "Unexpected parameters");
    }

    #endregion constructor tests

    #region ResetState tests

    /// <summary>
    /// Test ResetState clears the state.
    /// </summary>
    [TestMethod]
    public void ResetState_StateSet_ClearsState()
    {
        // ARRANGE
        ExecuteUpdateState state = new();
        state.SetQueryType(QueryType.UpdateGetRows);
        state.SetQuery("SELECT 1", []);

        // ACT
        state.ResetState();

        // ASSERT
        Assert.AreEqual(QueryType.Unknown, state.QueryType, "Unexpected query type");
        Assert.IsNull(state.Sql, "Unexpected SQL");
        Assert.IsNull(state.Parameters, "Unexpected parameters");
    }

    #endregion ResetState tests

    #region ResetStateAsync tests

    /// <summary>
    /// Test ResetStateAsync clears the state.
    /// </summary>
    /// <returns>A <see cref="Task"/> that represents the asynchronous operation.</returns>
    [TestMethod]
    public async Task ResetStateAsync_StateSet_ClearsState()
    {
        // ARRANGE
        ExecuteUpdateState state = new();
        state.SetQueryType(QueryType.DeleteGetRows);
        state.SetQuery("SELECT 1", []);

        // ACT
        await state.ResetStateAsync(this._testContext.CancellationToken).ConfigureAwait(false);

        // ASSERT
        Assert.AreEqual(QueryType.Unknown, state.QueryType, "Unexpected query type");
        Assert.IsNull(state.Sql, "Unexpected SQL");
        Assert.IsNull(state.Parameters, "Unexpected parameters");
    }

    #endregion ResetStateAsync tests

    #region service registration tests

    /// <summary>
    /// Test UseExecuteUpdateExtensions registers one <see cref="ExecuteUpdateState"/> per context.
    /// </summary>
    /// <param name="databaseType">Database type.</param>
    /// <returns>A <see cref="Task"/> that represents the asynchronous operation.</returns>
    [TestMethod]
    [DataRow(DatabaseTypes.Sqlite, DisplayName = $"UseExecuteUpdateExtensions {DatabaseTypes.Sqlite}")]
    [DataRow(DatabaseTypes.SqlServer, DisplayName = $"UseExecuteUpdateExtensions {DatabaseTypes.SqlServer}")]
    public async Task ServiceRegistration_UseExecuteUpdateExtensions_RegistersOneStatePerContext(string databaseType)
    {
        // ARRANGE
        using Context context1 = await DatabaseUtils.OpenDatabaseAsync(
            this._testContext,
            databaseType,
            customConfigurationActions: optionsBuilder => optionsBuilder.UseExecuteUpdateExtensions())
            .ConfigureAwait(false);

        using Context context2 = await DatabaseUtils.OpenDatabaseAsync(
            this._testContext,
            databaseType,
            createNew: false,
            customConfigurationActions: optionsBuilder => optionsBuilder.UseExecuteUpdateExtensions())
            .ConfigureAwait(false);

        // ACT
        ExecuteUpdateState state1 = context1.GetService<ExecuteUpdateState>();
        ExecuteUpdateState state1Again = context1.GetService<ExecuteUpdateState>();
        ExecuteUpdateState state2 = context2.GetService<ExecuteUpdateState>();

        // ASSERT
        Assert.AreSame(state1, state1Again, "A context should always get the same state");
        Assert.AreNotSame(state1, state2, "Each context should have its own state");
    }

    /// <summary>
    /// Test there is no <see cref="ExecuteUpdateState"/> without UseExecuteUpdateExtensions.
    /// </summary>
    /// <param name="databaseType">Database type.</param>
    /// <returns>A <see cref="Task"/> that represents the asynchronous operation.</returns>
    [TestMethod]
    [DataRow(DatabaseTypes.Sqlite, DisplayName = $"NoUseExecuteUpdateExtensions {DatabaseTypes.Sqlite}")]
    [DataRow(DatabaseTypes.SqlServer, DisplayName = $"NoUseExecuteUpdateExtensions {DatabaseTypes.SqlServer}")]
    public async Task ServiceRegistration_NoUseExecuteUpdateExtensions_HasNoState(string databaseType)
    {
        // ARRANGE
        using Context context = await DatabaseUtils.OpenDatabaseAsync(this._testContext, databaseType, createNew: false).ConfigureAwait(false);

        // ACT
        ExecuteUpdateState? state = context.GetInfrastructure().GetService<ExecuteUpdateState>();

        // ASSERT
        Assert.IsNull(state, "Unexpected state");
    }

    #endregion service registration tests

    #region context pooling tests

    /// <summary>
    /// Test a pooled context's <see cref="ExecuteUpdateState"/> is reset when the context is returned to the pool.
    /// </summary>
    /// <param name="databaseType">Database type.</param>
    /// <returns>A <see cref="Task"/> that represents the asynchronous operation.</returns>
    /// <remarks>
    /// A pooled context keeps its service scope, and so its state, between leases. The state is left set (as if a call had been
    /// interrupted) and must be cleared by the time the context is leased again.
    /// </remarks>
    [TestMethod]
    [DataRow(DatabaseTypes.Sqlite, DisplayName = $"ContextReturnedToPool {DatabaseTypes.Sqlite}")]
    [DataRow(DatabaseTypes.SqlServer, DisplayName = $"ContextReturnedToPool {DatabaseTypes.SqlServer}")]
    public async Task ContextPooling_ContextReturnedToPool_ResetsState(string databaseType)
    {
        // ARRANGE
        string originalValue = "Original TestValue";
        string updatedValue = "Updated TestValue";

        using (Context context = await DatabaseUtils.OpenDatabaseAsync(this._testContext, databaseType).ConfigureAwait(false))
        {
            await DatabaseUtils.CreateTestTableEntriesAsync(context, originalValue, 5, this._testContext.CancellationToken).ConfigureAwait(false);
        }

        PooledDbContextFactory<PooledContext> factory = new(this.CreatePooledContextOptions(databaseType), poolSize: 1);

        PooledContext firstLease;
        ExecuteUpdateState firstState;

        await using (PooledContext context = await factory.CreateDbContextAsync(this._testContext.CancellationToken).ConfigureAwait(false))
        {
            firstLease = context;
            firstState = context.GetService<ExecuteUpdateState>();
            firstState.SetQueryType(QueryType.UpdateGetRows);
            firstState.SetQuery("SELECT 1", []);
        }

        // ACT
        await using PooledContext secondLease = await factory.CreateDbContextAsync(this._testContext.CancellationToken).ConfigureAwait(false);
        ExecuteUpdateState secondState = secondLease.GetService<ExecuteUpdateState>();

        // ASSERT
        Assert.AreSame(firstLease, secondLease, "The context has not been reused from the pool");
        Assert.AreSame(firstState, secondState, "The pooled context should keep its state object");

        ExecuteUpdateTestUtils.AssertStateCleared(secondLease);

        long maxId = 2;
        IList<TestTable1> rows = await secondLease.TestTable1
            .Where(e => e.Id <= maxId)
            .ExecuteUpdateGetRowsAsync(builder => builder.SetProperty(e => e.TestField, updatedValue), this._testContext.CancellationToken)
            .ConfigureAwait(false);

        Assert.HasCount(2, rows, "Unexpected count");
        Assert.IsTrue(rows.All(r => r.TestField == updatedValue), "Unexpected field value");
    }

    #endregion context pooling tests

    #region private methods

    /// <summary>
    /// Create the options for a <see cref="PooledContext"/> that uses the Execute Update Extensions.
    /// </summary>
    /// <param name="databaseType">Database type.</param>
    /// <returns>The options.</returns>
    private DbContextOptions<PooledContext> CreatePooledContextOptions(string databaseType)
    {
        DbContextOptionsBuilder<PooledContext> optionsBuilder = new();

        switch (databaseType)
        {
            case DatabaseTypes.Sqlite:
                optionsBuilder.UseSqlite(DatabaseUtils.GetConnectionString(this._testContext, databaseType));
                break;

            case DatabaseTypes.SqlServer:
                optionsBuilder.UseSqlServer(DatabaseUtils.GetConnectionString(this._testContext, databaseType));
                break;

            default:
                throw new UnsupportedDatabaseTypeException();
        }

        optionsBuilder.UseExecuteUpdateExtensions();

        return optionsBuilder.Options;
    }

    #endregion private methods
}
