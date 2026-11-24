// Copyright ©2021-2026 Mike King.
// This file is licensed to you under the MIT license.
// See the License.txt file in the solution root for more information.

using DotDoc.EntityFrameworkCore.Extensions.DatabaseType;
using DotDoc.EntityFrameworkCore.Extensions.ExecuteUpdate;
using DotDoc.EntityFrameworkCore.Extensions.Tests.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.Logging;

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
    /// <param name="count">Number of records to delete.</param>
    /// <returns>A <see cref="Task"/> that represents the asynchronous operation.</returns>
    /// <remarks>The SQL is checked for the RETURNING (SQLite) / OUTPUT (SQL Server) clause by logging the commands that are run.</remarks>
    [TestMethod(DisplayName = "ExecuteDeleteGetRowsAsync")]
    [DataRow(DatabaseTypes.Sqlite, 0, DisplayName = $"{DatabaseTypes.Sqlite} Delete 0 Records")]
    [DataRow(DatabaseTypes.Sqlite, 1, DisplayName = $"{DatabaseTypes.Sqlite} Delete 1 Record")]
    [DataRow(DatabaseTypes.Sqlite, 10, DisplayName = $"{DatabaseTypes.Sqlite} Delete 10 Records")]
    [DataRow(DatabaseTypes.SqlServer, 0, DisplayName = $"{DatabaseTypes.SqlServer} Delete 0 Records")]
    [DataRow(DatabaseTypes.SqlServer, 1, DisplayName = $"{DatabaseTypes.SqlServer} Delete 1 Record")]
    [DataRow(DatabaseTypes.SqlServer, 10, DisplayName = $"{DatabaseTypes.SqlServer} Delete 10 Records")]
    public async Task ExecuteDeleteGetRowsTests_002_Async(string databaseType, int count)
    {
        // ARRANGE
        List<string> log = [];

        using Context context = await DatabaseUtils.OpenDatabaseAsync(
            this._testContext,
            databaseType: databaseType,
            customConfigurationActions: (optionsBuilder) =>
            {
                optionsBuilder.UseExecuteUpdateExtensions();
                optionsBuilder.LogTo(log.Add, [RelationalEventId.CommandExecuted], LogLevel.Information);
            })
            .ConfigureAwait(false);

        string value = "TestValue";
        await DatabaseUtils.CreateTestTableEntriesAsync(context, value, (count + 1) * 10, this._testContext.CancellationToken).ConfigureAwait(false);

        long startId = 2;
        long endId = startId + count;

        IQueryable<TestTable1> source = context.TestTable1.Where(e => e.Id >= startId && e.Id < endId);

        string returningClause = databaseType == DatabaseTypes.Sqlite ? "RETURNING *" : "OUTPUT DELETED.*";

        log.Clear();

        // ACT
        IList<TestTable1> rows = await source.ExecuteDeleteGetRowsAsync(this._testContext.CancellationToken).ConfigureAwait(false);

        // ASSERT
        ExecuteUpdateTestUtils.AssertStateCleared(context);

        Assert.Contains(returningClause, string.Join(Environment.NewLine, log), $"{returningClause} not found in the SQL");

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
            this._testContext,
            databaseType,
            customConfigurationActions: optionsBuilder => optionsBuilder.UseExecuteUpdateExtensions())
            .ConfigureAwait(false);

        await DatabaseUtils.CreateTestTableEntriesAsync(context, "TestValue", 1, this._testContext.CancellationToken).ConfigureAwait(false);

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
        using Context context = await DatabaseUtils.OpenDatabaseAsync(this._testContext, databaseType).ConfigureAwait(false);

        await DatabaseUtils.CreateTestTableEntriesAsync(context, "TestValue", 5, this._testContext.CancellationToken).ConfigureAwait(false);

        // ACT / ASSERT
        InvalidOperationException exception = await Assert.ThrowsExactlyAsync<InvalidOperationException>(() => context.TestTable1.ExecuteDeleteGetRowsAsync(this._testContext.CancellationToken), "Unexpected exception").ConfigureAwait(false);

        Assert.Contains(nameof(ExecuteUpdateExtensions.UseExecuteUpdateExtensions), exception.Message, "Unexpected exception message");

        int rowCount = await context.TestTable1.CountAsync(this._testContext.CancellationToken).ConfigureAwait(false);
        Assert.AreEqual(5, rowCount, "Unexpected count");
    }

    /// <summary>
    /// Test ExecuteDeleteGetRowsAsync without the RETURNING (SQLite) / OUTPUT (SQL Server) clause.
    /// </summary>
    /// <param name="databaseType">Database type.</param>
    /// <returns>A <see cref="Task"/> that represents the asynchronous operation.</returns>
    /// <remarks>The deleted rows can only be returned with the clause, so it must throw before anything is deleted.</remarks>
    [TestMethod(DisplayName = "ExecuteDeleteGetRowsAsync without the RETURNING / OUTPUT clause")]
    [DataRow(DatabaseTypes.Sqlite, DisplayName = DatabaseTypes.Sqlite)]
    [DataRow(DatabaseTypes.SqlServer, DisplayName = DatabaseTypes.SqlServer)]
    public async Task ExecuteDeleteGetRowsTests_005_Async(string databaseType)
    {
        // ARRANGE
        using Context context = await DatabaseUtils.OpenDatabaseAsync(
            this._testContext,
            databaseType,
            customConfigurationActions: optionsBuilder => optionsBuilder.UseExecuteUpdateExtensions(),
            customModelCreationActions: modelBuilder => ExecuteUpdateTestUtils.TurnOffReturningClause(modelBuilder, databaseType))
            .ConfigureAwait(false);

        await DatabaseUtils.CreateTestTableEntriesAsync(context, "TestValue", 5, this._testContext.CancellationToken).ConfigureAwait(false);

        // ACT / ASSERT
        NotSupportedException exception = await Assert.ThrowsExactlyAsync<NotSupportedException>(() => context.TestTable1.ExecuteDeleteGetRowsAsync(this._testContext.CancellationToken), "Unexpected exception").ConfigureAwait(false);

        ExecuteUpdateTestUtils.AssertReturningClauseNotUsedException(exception, databaseType, nameof(ExecuteUpdateExtensions.ExecuteDeleteGetRowsAsync));
        ExecuteUpdateTestUtils.AssertStateCleared(context);

        int rowCount = await context.TestTable1.CountAsync(this._testContext.CancellationToken).ConfigureAwait(false);
        Assert.AreEqual(5, rowCount, "Unexpected count");
    }

    /// <summary>
    /// Test ExecuteDeleteGetRowsAsync with braces in the SQL.
    /// </summary>
    /// <param name="databaseType">Database type.</param>
    /// <returns>A <see cref="Task"/> that represents the asynchronous operation.</returns>
    /// <remarks>
    /// The captured SQL is run again with SqlQueryRaw, which treats braces as placeholders, so they must be escaped. A string constant in a
    /// lambda and a tag are written into the SQL as they are. Unescaped, "}{" and the tag throw a FormatException, and "{0}" is replaced with
    /// the name of the first parameter, so the wrong rows are deleted.
    /// </remarks>
    [TestMethod(DisplayName = "ExecuteDeleteGetRowsAsync with braces in the SQL")]
    [DataRow(DatabaseTypes.Sqlite, DisplayName = DatabaseTypes.Sqlite)]
    [DataRow(DatabaseTypes.SqlServer, DisplayName = DatabaseTypes.SqlServer)]
    public async Task ExecuteDeleteGetRowsTests_006_Async(string databaseType)
    {
        // ARRANGE
        CancellationToken cancellationToken = this._testContext.CancellationToken;

        using Context context = await DatabaseUtils.OpenDatabaseAsync(
            this._testContext,
            databaseType,
            customConfigurationActions: optionsBuilder => optionsBuilder.UseExecuteUpdateExtensions())
            .ConfigureAwait(false);

        context.TestTable1.AddRange(
            new TestTable1 { TestField = "{0} First" },
            new TestTable1 { TestField = "{0} Second" },
            new TestTable1 { TestField = "}{" },
            new TestTable1 { TestField = "Other" });

        await context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        context.ChangeTracker.Clear();

        long minId = 0;

        // ACT
        IList<TestTable1> placeholderRows = await context.TestTable1
            .Where(e => e.TestField.StartsWith("{0}") && e.Id > minId)
            .ExecuteDeleteGetRowsAsync(cancellationToken)
            .ConfigureAwait(false);

        IList<TestTable1> braceRows = await context.TestTable1
            .TagWith("Tag with braces { } {0} }{")
            .Where(e => e.TestField == "}{")
            .ExecuteDeleteGetRowsAsync(cancellationToken)
            .ConfigureAwait(false);

        // ASSERT
        ExecuteUpdateTestUtils.AssertStateCleared(context);

        List<string> placeholderValues = [.. placeholderRows.Select(r => r.TestField).Order(StringComparer.Ordinal)];
        Assert.HasCount(2, placeholderValues, "Unexpected number of rows deleted with {0}");
        Assert.AreEqual("{0} First", placeholderValues[0], "Unexpected first row deleted with {0}");
        Assert.AreEqual("{0} Second", placeholderValues[1], "Unexpected second row deleted with {0}");

        Assert.HasCount(1, braceRows, "Unexpected number of rows deleted with }{");
        Assert.AreEqual("}{", braceRows[0].TestField, "Unexpected row deleted with }{");

        List<string> remaining = await context.TestTable1
            .Select(e => e.TestField)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        Assert.HasCount(1, remaining, "Unexpected number of rows left");
        Assert.AreEqual("Other", remaining[0], "Unexpected row left");
    }

    #endregion ExecuteDeleteGetRowsAsync tests
}
