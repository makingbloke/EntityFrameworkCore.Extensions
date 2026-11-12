// Copyright ©2021-2026 Mike King.
// This file is licensed to you under the MIT license.
// See the License.txt file in the solution root for more information.

using DotDoc.EntityFrameworkCore.Extensions.DatabaseType;
using DotDoc.EntityFrameworkCore.Extensions.ExecuteUpdate;
using DotDoc.EntityFrameworkCore.Extensions.TableHints;
using DotDoc.EntityFrameworkCore.Extensions.Tests.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Logging;

namespace DotDoc.EntityFrameworkCore.Extensions.Tests.TableHints;

/// <summary>
/// Tests for the WithTableHints extension.
/// </summary>
[TestClass]
public class TableHintsTests
{
    #region private fields

    /// <summary>
    /// The MSTest <see cref="TestContext"/> for the current test.
    /// </summary>
    private readonly TestContext _testContext;

    #endregion private fields

    #region public constructors

    /// <summary>
    /// Initializes a new instance of the <see cref="TableHintsTests"/> class.
    /// </summary>
    /// <param name="testContext">The MSTest test context.</param>
    public TableHintsTests(TestContext testContext)
    {
        this._testContext = testContext;
    }

    #endregion public constructors

    #region public methods

    /// <summary>
    /// Test UseTableHintExtensions with a Null DbContextOptionsBuilder OptionsBuilder parameter.
    /// </summary>
    [TestMethod(DisplayName = "UseTableHintExtensions with a Null DbContextOptionsBuilder OptionsBuilder parameter")]
    public void TableHintsTests_001()
    {
        // ARRANGE
        DbContextOptionsBuilder optionsBuilder = null!;

        // ACT / ASSERT
        Assert.ThrowsExactly<ArgumentNullException>(() => optionsBuilder.UseTableHintExtensions(), "Unexpected exception");
    }

    /// <summary>
    /// Test UseTableHintExtensions with an unsupported database type (only SQL Server is currently supported).
    /// </summary>
    /// <returns>A <see cref="Task"/> that represents the asynchronous operation.</returns>
    [TestMethod(DisplayName = "UseTableHintExtensions with an unsupported database type")]
    public async Task TableHintsTests_002_Async()
    {
        // ARRANGE / ACT / ASSERT
        await Assert.ThrowsExactlyAsync<UnsupportedDatabaseTypeException>(
            async () =>
            {
                using Context context = await DatabaseUtils.OpenDatabaseAsync(
                    DatabaseTypes.Sqlite,
                    customConfigurationActions: optionsBuilder => optionsBuilder.UseTableHintExtensions())
                    .ConfigureAwait(false);
            },
            "Unexpected exception")
            .ConfigureAwait(false);
    }

    /// <summary>
    /// Test WithTableHints with an empty Table Hint IEnumerable.
    /// </summary>
    /// <returns>A <see cref="Task"/> that represents the asynchronous operation.</returns>
    [TestMethod(DisplayName = "WithTableHints with an empty Table Hint IEnumerable")]
    public async Task TableHintsTests_003_Async()
    {
        // ARRANGE
        using Context context = await DatabaseUtils.OpenDatabaseAsync(
            DatabaseTypes.SqlServer,
            customConfigurationActions: optionsBuilder => optionsBuilder.UseTableHintExtensions())
            .ConfigureAwait(false);

        // ACT / ASSERT
        Assert.ThrowsExactly<ArgumentNullException>(() => context.TestTable1.WithTableHints(), "Unexpected exception");
    }

    /// <summary>
    /// Test WithTableHints with a Null Table Hint.
    /// </summary>
    /// <returns>A <see cref="Task"/> that represents the asynchronous operation.</returns>
    [TestMethod(DisplayName = "WithTableHints with a Null Table Hint")]
    public async Task TableHintsTests_004_Async()
    {
        // ARRANGE
        SqlServerTableHints tableHint = null!;

        using Context context = await DatabaseUtils.OpenDatabaseAsync(
            DatabaseTypes.SqlServer,
            customConfigurationActions: optionsBuilder => optionsBuilder.UseTableHintExtensions())
            .ConfigureAwait(false);

        // ACT / ASSERT
        Assert.ThrowsExactly<ArgumentException>(() => context.TestTable1.WithTableHints(tableHint), "Unexpected exception");
    }

    /// <summary>
    /// Test WithTableHints with all valid Table Hints.
    /// </summary>
    /// <param name="tableHint">The Table Hint to insert.</param>
    /// <returns>A <see cref="Task"/> that represents the asynchronous operation.</returns>
    [TestMethod(DisplayName = "WithTableHints with all valid Table Hints")]
    [DynamicData(nameof(TableHintsSource))]
    public async Task TableHintsTests_005_Async(SqlServerTableHints tableHint)
    {
        // ARRANGE
        using Context context = await DatabaseUtils.OpenDatabaseAsync(
            DatabaseTypes.SqlServer,
            customConfigurationActions: optionsBuilder => optionsBuilder.UseTableHintExtensions())
            .ConfigureAwait(false);

        string tableHintClause = $"WITH ({tableHint})";

        // ACT
        IQueryable<TestTable1> query = context.TestTable1.WithTableHints(tableHint);
        string sql = query.ToQueryString();

        // ASSERT
        Assert.Contains(tableHintClause, sql, "Table hint missing from SQL.");
    }

    /// <summary>
    /// Test WithTableHints with multiple Table Hints.
    /// </summary>
    /// <returns>A <see cref="Task"/> that represents the asynchronous operation.</returns>
    [TestMethod(DisplayName = "WithTableHints with multiple Table Hints")]
    public async Task TableHintsTests_006_Async()
    {
        // ARRANGE
        using Context context = await DatabaseUtils.OpenDatabaseAsync(
            DatabaseTypes.SqlServer,
            customConfigurationActions: optionsBuilder => optionsBuilder.UseTableHintExtensions())
            .ConfigureAwait(false);

        List<SqlServerTableHints> tableHints =
            [
                SqlServerTableHints.NoExpand,
                SqlServerTableHints.ForceScan,
                SqlServerTableHints.HoldLock,
                SqlServerTableHints.NoLock
            ];

        string tableHintClause = $"WITH ({string.Join(", ", tableHints)})";

        // ACT
        IQueryable<TestTable1> query = context.TestTable1.WithTableHints(tableHints);
        string sql = query.ToQueryString();

        // ASSERT
        Assert.Contains(tableHintClause, sql, "Table hint missing from SQL.");
    }

    /// <summary>
    /// Test SqlServerTableHint.Index with a Null or Empty IEnumerable IndexValues parameter.
    /// </summary>
    /// <param name="indexValues">The index values (names).</param>
    [TestMethod(DisplayName = "SqlServerTableHint.Index with a Null or Empty IEnumerable IndexValues parameter")]
    [DataRow(null!, DisplayName = "Null")]
    [DataRow([], DisplayName = "Empty")]
    public void TableHintsTests_007(IEnumerable<string> indexValues)
    {
        // ARRANGE

        // ACT / ASSERT
        Assert.ThrowsExactly<ArgumentNullException>(() => SqlServerTableHints.Index(indexValues), "Unexpected exception");
    }

    /// <summary>
    /// Test SqlServerTableHint.Index with a Null IndexValues parameter value.
    /// </summary>
    [TestMethod(DisplayName = "SqlServerTableHint.Index with a Null IndexValues parameter value")]
    public void TableHintsTests_008()
    {
        // ARRANGE
        string indexValue = null!;

        // ACT / ASSERT
        Assert.ThrowsExactly<ArgumentException>(() => SqlServerTableHints.Index(indexValue), "Unexpected exception");
    }

    /// <summary>
    /// Test SqlServerTableHint.ForceSeek with a Null or Empty IndexValue parameter value.
    /// </summary>
    /// <param name="indexValue">The index value (name).</param>
    /// <param name="exceptionType">Type of exception raised.</param>
    [TestMethod(DisplayName = "SqlServerTableHint.ForceSeek with a Null or Empty IndexValue parameter value")]
    [DataRow(null!, typeof(ArgumentNullException), DisplayName = "Null")]
    [DataRow("", typeof(ArgumentException), DisplayName = "Empty")]
    public void TableHintsTests_009(string indexValue, Type exceptionType)
    {
        // ARRANGE
        IEnumerable<string> indexColumnNames = ["DummyIndexColumnName"];

        // ACT / ASSERT
        Exception e = Assert.Throws<Exception>(() => SqlServerTableHints.ForceSeek(indexValue, indexColumnNames), "Unexpected exception");
        Assert.IsInstanceOfType(e, exceptionType, "Unexpected exception type");
    }

    /// <summary>
    /// Test SqlServerTableHint.ForceSeek with a Null or Empty IEnumerable IndexColumnNames parameter.
    /// </summary>
    /// <param name="indexColumnNames">The index values (names).</param>
    [TestMethod(DisplayName = "SqlServerTableHint.ForceSeek with a Null or Empty IEnumerable IndexColumnNames parameter")]
    [DataRow(null!, DisplayName = "Null")]
    [DataRow([], DisplayName = "Empty")]
    public void TableHintsTests_010(IEnumerable<string> indexColumnNames)
    {
        // ARRANGE
        string indexValue = "DummyIndexValue";

        // ACT / ASSERT
        Assert.ThrowsExactly<ArgumentNullException>(() => SqlServerTableHints.ForceSeek(indexValue, indexColumnNames), "Unexpected exception");
    }

    /// <summary>
    /// Test SqlServerTableHint.ForceSeek with a Null IndexColumnNames parameter value.
    /// </summary>
    [TestMethod(DisplayName = "SqlServerTableHint.ForceSeek with a Null IndexColumnNames parameter value")]
    public void TableHintsTests_011()
    {
        // ARRANGE
        string indexValue = "DummyIndexValue";
        string indexColumnName = null!;

        // ACT / ASSERT
        Assert.ThrowsExactly<ArgumentException>(() => SqlServerTableHints.ForceSeek(indexValue, indexColumnName), "Unexpected exception");
    }

    /// <summary>
    /// Test SqlServerTableHint.SpacialWindowMaxCells with an Int Value outside the allowed range.
    /// </summary>
    /// <param name="value">Specifies the maximum number of cells to use for tessellating a geometry or geography object (1-8192).</param>
    [TestMethod(DisplayName = "SqlServerTableHint.SpacialWindowMaxCells with an Int Value outside the allowed range")]
    [DataRow(0, DisplayName = "0")]
    [DataRow(8193, DisplayName = "8193")]
    public void TableHintsTests_012(int value)
    {
        // ARRANGE

        // ACT / ASSERT
        Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => SqlServerTableHints.SpacialWindowMaxCells(value), "Unexpected exception");
    }

    /// <summary>
    /// Test WithTableHints keeps the Table Hints when EF generates the SQL for a cached query again.
    /// </summary>
    /// <returns>A <see cref="Task"/> that represents the asynchronous operation.</returns>
    /// <remarks>
    /// EF generates the SQL for a cached query again when a nullable parameter changes between null and non-null,
    /// so this checks the table hints tag is not removed from the cached query when the first SQL is generated.
    /// </remarks>
    [TestMethod(DisplayName = "WithTableHints keeps the Table Hints when the SQL is generated again")]
    public async Task TableHintsTests_013_Async()
    {
        // ARRANGE
        using Context context = await DatabaseUtils.OpenDatabaseAsync(
            DatabaseTypes.SqlServer,
            customConfigurationActions: optionsBuilder => optionsBuilder.UseTableHintExtensions())
            .ConfigureAwait(false);

        string tableHintClause = $"WITH ({SqlServerTableHints.NoLock})";
        string? value = null;

        IQueryable<TestTable1> query = context.TestTable1
            .WithTableHints(SqlServerTableHints.NoLock)
            .Where(e => e.TestField == value);

        // ACT
        string sqlWithNull = query.ToQueryString();

        value = "Test";
        string sqlWithValue = query.ToQueryString();

        // ASSERT
        Assert.AreNotEqual(sqlWithNull, sqlWithValue, "SQL was not generated again, so the test is not valid.");

        Assert.Contains(tableHintClause, sqlWithNull, "Table hint missing from SQL (null parameter).");
        Assert.Contains(tableHintClause, sqlWithValue, "Table hint missing from SQL (non-null parameter).");

        Assert.DoesNotContain(TableHintExtensions.TableHintsTagPrefix, sqlWithNull, "Table hints tag included in SQL (null parameter).");
        Assert.DoesNotContain(TableHintExtensions.TableHintsTagPrefix, sqlWithValue, "Table hints tag included in SQL (non-null parameter).");
    }

    /// <summary>
    /// Test SqlServerTableHint.Index generates the correct hint.
    /// </summary>
    /// <param name="indexValues">The index values (names).</param>
    /// <param name="expectedHint">The expected hint.</param>
    [TestMethod(DisplayName = "SqlServerTableHint.Index generates the correct hint")]
    [DataRow(new[] { "IX_1" }, "INDEX([IX_1])", DisplayName = "One index")]
    [DataRow(new[] { "IX_1", "IX_2" }, "INDEX([IX_1], [IX_2])", DisplayName = "Two indexes")]
    [DataRow(new[] { "IX_1", "ix_1" }, "INDEX([IX_1])", DisplayName = "Duplicate indexes are removed")]
    [DataRow(new[] { "IX]1" }, "INDEX([IX]]1])", DisplayName = "Index name containing ]")]
    public void TableHintsTests_014(string[] indexValues, string expectedHint)
    {
        // ARRANGE

        // ACT
        SqlServerTableHints tableHint = SqlServerTableHints.Index(indexValues);

        // ASSERT
        Assert.AreEqual(expectedHint, tableHint.ToString(), "Invalid hint");
    }

    /// <summary>
    /// Test SqlServerTableHint.ForceSeek generates the correct hint.
    /// </summary>
    /// <param name="indexValue">The index value (name).</param>
    /// <param name="indexColumnNames">The index column names.</param>
    /// <param name="expectedHint">The expected hint.</param>
    [TestMethod(DisplayName = "SqlServerTableHint.ForceSeek generates the correct hint")]
    [DataRow("IX_1", new[] { "Col1" }, "FORCESEEK([IX_1] ([Col1]))", DisplayName = "One column")]
    [DataRow("IX_1", new[] { "Col1", "Col2" }, "FORCESEEK([IX_1] ([Col1], [Col2]))", DisplayName = "Two columns")]
    [DataRow("IX]1", new[] { "Col]1" }, "FORCESEEK([IX]]1] ([Col]]1]))", DisplayName = "Index and column names containing ]")]
    public void TableHintsTests_015(string indexValue, string[] indexColumnNames, string expectedHint)
    {
        // ARRANGE

        // ACT
        SqlServerTableHints tableHint = SqlServerTableHints.ForceSeek(indexValue, indexColumnNames);

        // ASSERT
        Assert.AreEqual(expectedHint, tableHint.ToString(), "Invalid hint");
    }

    /// <summary>
    /// Test SqlServerTableHint.SpacialWindowMaxCells generates the correct hint.
    /// </summary>
    /// <param name="value">Specifies the maximum number of cells to use for tessellating a geometry or geography object (1-8192).</param>
    /// <param name="expectedHint">The expected hint.</param>
    [TestMethod(DisplayName = "SqlServerTableHint.SpacialWindowMaxCells generates the correct hint")]
    [DataRow(1, "SPATIAL_WINDOW_MAX_CELLS = 1", DisplayName = "1")]
    [DataRow(8192, "SPATIAL_WINDOW_MAX_CELLS = 8192", DisplayName = "8192")]
    public void TableHintsTests_016(int value, string expectedHint)
    {
        // ARRANGE

        // ACT
        SqlServerTableHints tableHint = SqlServerTableHints.SpacialWindowMaxCells(value);

        // ASSERT
        Assert.AreEqual(expectedHint, tableHint.ToString(), "Invalid hint");
    }

    /// <summary>
    /// Test WithTableHints with a Null IQueryable Source parameter.
    /// </summary>
    [TestMethod(DisplayName = "WithTableHints with a Null IQueryable Source parameter")]
    public void TableHintsTests_017()
    {
        // ARRANGE
        IQueryable<TestTable1> source = null!;

        // ACT / ASSERT
        Assert.ThrowsExactly<ArgumentNullException>(() => source.WithTableHints(SqlServerTableHints.NoLock), "Unexpected exception");
    }

    /// <summary>
    /// Test WithTableHints leaves the query unchanged on SQLite (table hints are SQL Server only).
    /// </summary>
    /// <returns>A <see cref="Task"/> that represents the asynchronous operation.</returns>
    [TestMethod(DisplayName = "WithTableHints leaves the query unchanged on SQLite")]
    public async Task TableHintsTests_018_Async()
    {
        // ARRANGE
        using Context context = await DatabaseUtils.OpenDatabaseAsync(DatabaseTypes.Sqlite).ConfigureAwait(false);

        string expectedSql = context.TestTable1.ToQueryString();

        // ACT
        string sqlWithHint = context.TestTable1.WithTableHints(SqlServerTableHints.NoLock).ToQueryString();
        string sqlWithNoHints = context.TestTable1.WithTableHints().ToQueryString();

        // ASSERT
        Assert.AreEqual(expectedSql, sqlWithHint, "Invalid SQL (with a hint).");
        Assert.AreEqual(expectedSql, sqlWithNoHints, "Invalid SQL (with no hints).");
    }

    /// <summary>
    /// Test queries using WithTableHints run on SQL Server.
    /// </summary>
    /// <returns>A <see cref="Task"/> that represents the asynchronous operation.</returns>
    [TestMethod(DisplayName = "WithTableHints queries run on SQL Server")]
    public async Task TableHintsTests_019_Async()
    {
        // ARRANGE
        using Context context = await DatabaseUtils.OpenDatabaseAsync(
            DatabaseTypes.SqlServer,
            customConfigurationActions: optionsBuilder => optionsBuilder.UseTableHintExtensions())
            .ConfigureAwait(false);

        await DatabaseUtils.CreateTestTableEntriesAsync(context, "TestValue", 3).ConfigureAwait(false);

        // ACT
        List<TestTable1> allRows = await context.TestTable1
            .WithTableHints(SqlServerTableHints.NoLock)
            .AsNoTracking()
            .ToListAsync(this._testContext.CancellationToken)
            .ConfigureAwait(false);

        List<TestTable1> filteredRows = await context.TestTable1
            .WithTableHints(SqlServerTableHints.Index("PK_TestTable1"), SqlServerTableHints.NoLock)
            .Where(e => e.Id > 1)
            .AsNoTracking()
            .ToListAsync(this._testContext.CancellationToken)
            .ConfigureAwait(false);

        // ASSERT
        Assert.HasCount(3, allRows, "Invalid count (all rows).");
        Assert.HasCount(2, filteredRows, "Invalid count (filtered rows).");
    }

    /// <summary>
    /// Test WithTableHints with the ExecuteUpdate extensions that return rows.
    /// </summary>
    /// <param name="methodName">The name of the ExecuteUpdate extension method to test.</param>
    /// <returns>A <see cref="Task"/> that represents the asynchronous operation.</returns>
    /// <remarks>
    /// ROWLOCK is used because SQL Server does not allow NOLOCK on the table an INSERT, UPDATE or DELETE writes to.
    /// The SQL is checked by logging the commands that are run.
    /// </remarks>
    [TestMethod(DisplayName = "WithTableHints with the ExecuteUpdate extensions")]
    [DataRow(nameof(ExecuteUpdateExtensions.ExecuteDeleteGetRowsAsync), DisplayName = nameof(ExecuteUpdateExtensions.ExecuteDeleteGetRowsAsync))]
    [DataRow(nameof(ExecuteUpdateExtensions.ExecuteInsertGetRowAsync), DisplayName = nameof(ExecuteUpdateExtensions.ExecuteInsertGetRowAsync))]
    [DataRow(nameof(ExecuteUpdateExtensions.ExecuteUpdateGetRowsAsync), DisplayName = nameof(ExecuteUpdateExtensions.ExecuteUpdateGetRowsAsync))]
    public async Task TableHintsTests_020_Async(string methodName)
    {
        // ARRANGE
        List<string> log = [];

        using Context context = await DatabaseUtils.OpenDatabaseAsync(
            DatabaseTypes.SqlServer,
            customConfigurationActions: optionsBuilder =>
            {
                optionsBuilder.UseTableHintExtensions();
                optionsBuilder.UseExecuteUpdateExtensions();
                optionsBuilder.LogTo(log.Add, [RelationalEventId.CommandExecuted], LogLevel.Information);
            })
            .ConfigureAwait(false);

        await DatabaseUtils.CreateTestTableEntriesAsync(context, "Original TestValue", 3).ConfigureAwait(false);

        string tableHintClause = $"WITH ({SqlServerTableHints.RowLock})";
        string value = "Updated TestValue";

        IQueryable<TestTable1> source = context.TestTable1.WithTableHints(SqlServerTableHints.RowLock);

        log.Clear();

        // ACT
        int count = methodName switch
        {
            nameof(ExecuteUpdateExtensions.ExecuteDeleteGetRowsAsync) => (await source
                .Where(e => e.Id == 1)
                .ExecuteDeleteGetRowsAsync(this._testContext.CancellationToken)
                .ConfigureAwait(false)).Count,

            nameof(ExecuteUpdateExtensions.ExecuteInsertGetRowAsync) => await source
                .ExecuteInsertGetRowAsync(builder => builder.SetProperty(e => e.TestField, value), this._testContext.CancellationToken)
                .ConfigureAwait(false) is not null ? 1 : 0,

            nameof(ExecuteUpdateExtensions.ExecuteUpdateGetRowsAsync) => (await source
                .Where(e => e.Id == 1)
                .ExecuteUpdateGetRowsAsync(builder => builder.SetProperty(e => e.TestField, value), this._testContext.CancellationToken)
                .ConfigureAwait(false)).Count,

            _ => throw new ArgumentException($"Unknown method {methodName}", nameof(methodName))
        };

        // ASSERT
        Assert.AreEqual(1, count, "Invalid row count");
        Assert.Contains(tableHintClause, string.Join(Environment.NewLine, log), "Table hint missing from SQL.");
    }

    #endregion public methods

    #region private methods

    /// <summary>
    /// Return all valid Table Hints.
    /// </summary>
    /// <returns><see cref="IEnumerable{SqlServerTableHint}"/>.</returns>
    private static IEnumerable<SqlServerTableHints> TableHintsSource()
    {
        yield return SqlServerTableHints.NoExpand;
        yield return SqlServerTableHints.ForceScan;
        yield return SqlServerTableHints.HoldLock;
        yield return SqlServerTableHints.NoLock;
        yield return SqlServerTableHints.NoWait;
        yield return SqlServerTableHints.PagLock;
        yield return SqlServerTableHints.ReadCommitted;
        yield return SqlServerTableHints.ReadCommittedLock;
        yield return SqlServerTableHints.ReadPast;
        yield return SqlServerTableHints.ReadUncommitted;
        yield return SqlServerTableHints.RepeatableRead;
        yield return SqlServerTableHints.RowLock;
        yield return SqlServerTableHints.Serializable;
        yield return SqlServerTableHints.Snapshot;
        yield return SqlServerTableHints.TabLock;
        yield return SqlServerTableHints.TabLockX;
        yield return SqlServerTableHints.Updlock;
        yield return SqlServerTableHints.XLock;
        yield return SqlServerTableHints.Index("DummyIndexValue");
        yield return SqlServerTableHints.ForceSeek();
        yield return SqlServerTableHints.ForceSeek("DummyIndexValue", "DummyIndexColumnName");
        yield return SqlServerTableHints.SpacialWindowMaxCells(100);
    }

    #endregion private methods
}