// Copyright ©2021-2026 Mike King.
// This file is licensed to you under the MIT license.
// See the License.txt file in the solution root for more information.

using DotDoc.EntityFrameworkCore.Extensions.DatabaseType;
using DotDoc.EntityFrameworkCore.Extensions.Execute;
using DotDoc.EntityFrameworkCore.Extensions.Tests.Data;
using DotDoc.EntityFrameworkCore.Extensions.Tests.Model;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Storage;

namespace DotDoc.EntityFrameworkCore.Extensions.Tests.Execute;

/// <summary>
/// Tests that the Execute extensions that run a scalar command (ExecuteScalarAsync, ExecuteInsertAsync and the count query of
/// ExecutePagedQueryAsync) run it through EF Core's command pipeline, as EF Core's own ExecuteSqlRawAsync does: so command
/// interceptors see it, with the context, and the context's execution strategy retries it after a transient error.
/// </summary>
[TestClass]
public class ExecuteCommandPipelineTests
{
    #region private fields

    /// <summary>
    /// The MSTest <see cref="TestContext"/> for the current test.
    /// </summary>
    private readonly TestContext _testContext;

    #endregion private fields

    #region public constructors

    /// <summary>
    /// Initializes a new instance of the <see cref="ExecuteCommandPipelineTests"/> class.
    /// </summary>
    /// <param name="testContext">The MSTest test context.</param>
    public ExecuteCommandPipelineTests(TestContext testContext)
    {
        this._testContext = testContext;
    }

    #endregion public constructors

    #region ExecuteScalarAsync tests

    /// <summary>
    /// Test ExecuteScalarAsync runs its command through EF Core's command pipeline, so a command interceptor sees it with the context.
    /// </summary>
    /// <param name="databaseType">Database type.</param>
    /// <returns>A <see cref="Task"/> that represents the asynchronous operation.</returns>
    [TestMethod]
    [DataRow(DatabaseTypes.Sqlite, DisplayName = $"CommandInterceptor {DatabaseTypes.Sqlite}")]
    [DataRow(DatabaseTypes.SqlServer, DisplayName = $"CommandInterceptor {DatabaseTypes.SqlServer}")]
    public async Task ExecuteScalarAsync_CommandInterceptor_SeesCommandWithContext(string databaseType)
    {
        // ARRANGE
        CommandRecordingInterceptor interceptor = new();

        using Context context = await DatabaseUtils.OpenDatabaseAsync(
            this._testContext,
            databaseType,
            customConfigurationActions: optionsBuilder => optionsBuilder.AddInterceptors(interceptor))
            .ConfigureAwait(false);

        await DatabaseUtils.CreateTestTableEntriesAsync(context, "TestValue", 1, this._testContext.CancellationToken).ConfigureAwait(false);

        FormattableString sql = $"SELECT COUNT(*) FROM TestTable1";
        interceptor.Commands.Clear();

        // ACT
        int count = await context.Database.ExecuteScalarAsync<int>(sql, this._testContext.CancellationToken).ConfigureAwait(false);

        // ASSERT
        Assert.AreEqual(1, count, "Invalid count");
        AssertCommandSeen(interceptor, "SELECT COUNT(*) FROM TestTable1", context);
    }

    /// <summary>
    /// Test ExecuteScalarAsync runs its command inside the context's execution strategy, so a transient error is retried.
    /// </summary>
    /// <param name="databaseType">Database type.</param>
    /// <returns>A <see cref="Task"/> that represents the asynchronous operation.</returns>
    [TestMethod]
    [DataRow(DatabaseTypes.Sqlite, DisplayName = $"TransientError {DatabaseTypes.Sqlite}")]
    [DataRow(DatabaseTypes.SqlServer, DisplayName = $"TransientError {DatabaseTypes.SqlServer}")]
    public async Task ExecuteScalarAsync_TransientError_IsRetriedByExecutionStrategy(string databaseType)
    {
        // ARRANGE
        CommandRecordingInterceptor interceptor = new();

        using Context context = await DatabaseUtils.OpenDatabaseAsync(
            this._testContext,
            databaseType,
            customConfigurationActions: optionsBuilder => optionsBuilder
                .AddInterceptors(interceptor)
                .ReplaceService<IExecutionStrategyFactory, RetryOnceExecutionStrategyFactory>())
            .ConfigureAwait(false);

        await DatabaseUtils.CreateTestTableEntriesAsync(context, "TestValue", 1, this._testContext.CancellationToken).ConfigureAwait(false);

        FormattableString sql = $"SELECT COUNT(*) FROM TestTable1";
        interceptor.Commands.Clear();
        interceptor.ScalarFailuresToThrow = 1;

        // ACT
        int count = await context.Database.ExecuteScalarAsync<int>(sql, this._testContext.CancellationToken).ConfigureAwait(false);

        // ASSERT
        Assert.AreEqual(1, count, "Invalid count");
        Assert.AreEqual(0, interceptor.ScalarFailuresToThrow, "The simulated transient error wasn't thrown");
        Assert.HasCount(2, interceptor.Commands.Where(c => c.CommandText.Contains("SELECT COUNT(*) FROM TestTable1", StringComparison.Ordinal)), "The command wasn't retried");
    }

    #endregion ExecuteScalarAsync tests

    #region ExecuteInsertAsync tests

    /// <summary>
    /// Test ExecuteInsertAsync runs its command through EF Core's command pipeline, so a command interceptor sees it with the context.
    /// </summary>
    /// <param name="databaseType">Database type.</param>
    /// <returns>A <see cref="Task"/> that represents the asynchronous operation.</returns>
    [TestMethod]
    [DataRow(DatabaseTypes.Sqlite, DisplayName = $"CommandInterceptor {DatabaseTypes.Sqlite}")]
    [DataRow(DatabaseTypes.SqlServer, DisplayName = $"CommandInterceptor {DatabaseTypes.SqlServer}")]
    public async Task ExecuteInsertAsync_CommandInterceptor_SeesCommandWithContext(string databaseType)
    {
        // ARRANGE
        CommandRecordingInterceptor interceptor = new();

        using Context context = await DatabaseUtils.OpenDatabaseAsync(
            this._testContext,
            databaseType,
            customConfigurationActions: optionsBuilder => optionsBuilder.AddInterceptors(interceptor))
            .ConfigureAwait(false);

        string sql = "INSERT INTO TestTable1 (TestField) VALUES ({0})";
        interceptor.Commands.Clear();

        // ACT
        long id = await context.Database.ExecuteInsertAsync<long>(sql, this._testContext.CancellationToken, "TestValue").ConfigureAwait(false);

        // ASSERT
        Assert.IsGreaterThan(0L, id, "Invalid Id");
        AssertCommandSeen(interceptor, "INSERT INTO TestTable1", context);
    }

    #endregion ExecuteInsertAsync tests

    #region ExecutePagedQueryAsync tests

    /// <summary>
    /// Test ExecutePagedQueryAsync runs its count query through EF Core's command pipeline, so a command interceptor sees it with the context
    /// (the page query always was, as it uses SqlQueryRaw).
    /// </summary>
    /// <param name="databaseType">Database type.</param>
    /// <returns>A <see cref="Task"/> that represents the asynchronous operation.</returns>
    [TestMethod]
    [DataRow(DatabaseTypes.Sqlite, DisplayName = $"CommandInterceptor {DatabaseTypes.Sqlite}")]
    [DataRow(DatabaseTypes.SqlServer, DisplayName = $"CommandInterceptor {DatabaseTypes.SqlServer}")]
    public async Task ExecutePagedQueryAsync_CommandInterceptor_SeesCountQueryWithContext(string databaseType)
    {
        // ARRANGE
        CommandRecordingInterceptor interceptor = new();

        using Context context = await DatabaseUtils.OpenDatabaseAsync(
            this._testContext,
            databaseType,
            customConfigurationActions: optionsBuilder => optionsBuilder.AddInterceptors(interceptor))
            .ConfigureAwait(false);

        await DatabaseUtils.CreateTestTableEntriesAsync(context, "TestValue", 10, this._testContext.CancellationToken).ConfigureAwait(false);

        FormattableString sql = $"SELECT * FROM TestTable1";
        interceptor.Commands.Clear();

        // ACT
        PagedQueryResult<TestTable1> result = await context.Database.ExecutePagedQueryAsync<TestTable1>(sql, 0, 5, this._testContext.CancellationToken).ConfigureAwait(false);

        // ASSERT
        Assert.AreEqual(10, result.RecordCount, "Invalid record count");
        AssertCommandSeen(interceptor, "SELECT COUNT(*) FROM", context);
    }

    #endregion ExecutePagedQueryAsync tests

    #region private methods

    /// <summary>
    /// Asserts that the interceptor saw exactly one command containing some SQL, marked as raw SQL and run for the context.
    /// </summary>
    /// <param name="interceptor">The interceptor.</param>
    /// <param name="sql">SQL the command must contain.</param>
    /// <param name="context">The context the command must have been run for.</param>
    private static void AssertCommandSeen(CommandRecordingInterceptor interceptor, string sql, DbContext context)
    {
        List<(string CommandText, CommandSource CommandSource, DbContext? Context)> commands = [.. interceptor.Commands.Where(c => c.CommandText.Contains(sql, StringComparison.Ordinal))];

        Assert.HasCount(1, commands, "The command wasn't seen by the interceptor");
        Assert.AreEqual(CommandSource.ExecuteSqlRaw, commands[0].CommandSource, "Invalid command source");
        Assert.AreSame(context, commands[0].Context, "The command wasn't run for the context");
    }

    #endregion private methods
}
