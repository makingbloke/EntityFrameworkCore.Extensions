// Copyright ©2021-2026 Mike King.
// This file is licensed to you under the MIT license.
// See the License.txt file in the solution root for more information.

using DotDoc.EntityFrameworkCore.Extensions.CustomQueryGenerators;
using DotDoc.EntityFrameworkCore.Extensions.DatabaseType;
using DotDoc.EntityFrameworkCore.Extensions.ExecuteUpdate;
using DotDoc.EntityFrameworkCore.Extensions.TableHints;
using DotDoc.EntityFrameworkCore.Extensions.Tests.CustomQueryGenerators;
using DotDoc.EntityFrameworkCore.Extensions.Tests.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Query;
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

    #region UseTableHintExtensions tests

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
                    this._testContext,
                    DatabaseTypes.Sqlite,
                    customConfigurationActions: optionsBuilder => optionsBuilder.UseTableHintExtensions())
                    .ConfigureAwait(false);
            },
            "Unexpected exception")
            .ConfigureAwait(false);
    }

    #endregion UseTableHintExtensions tests

    #region WithTableHints tests

    /// <summary>
    /// Test WithTableHints with an empty Table Hint IEnumerable for an entity type.
    /// </summary>
    /// <returns>A <see cref="Task"/> that represents the asynchronous operation.</returns>
    [TestMethod(DisplayName = "WithTableHints with an empty Table Hint IEnumerable for an entity type")]
    public async Task TableHintsTests_003_Async()
    {
        // ARRANGE
        using Context context = await DatabaseUtils.OpenDatabaseAsync(
            this._testContext,
            DatabaseTypes.SqlServer,
            customConfigurationActions: optionsBuilder => optionsBuilder.UseTableHintExtensions())
            .ConfigureAwait(false);

        // ACT / ASSERT
        Assert.ThrowsExactly<ArgumentException>(() => context.TestTable1.WithTableHints(tableHints => tableHints.For<TestTable1>()), "Unexpected exception");
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
            this._testContext,
            DatabaseTypes.SqlServer,
            customConfigurationActions: optionsBuilder => optionsBuilder.UseTableHintExtensions())
            .ConfigureAwait(false);

        // ACT / ASSERT
        Assert.ThrowsExactly<ArgumentException>(() => context.TestTable1.WithTableHints(tableHints => tableHints.For<TestTable1>(tableHint)), "Unexpected exception");
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
            this._testContext,
            DatabaseTypes.SqlServer,
            customConfigurationActions: optionsBuilder => optionsBuilder.UseTableHintExtensions())
            .ConfigureAwait(false);

        string tableHintClause = $"WITH ({tableHint})";

        // ACT
        IQueryable<TestTable1> query = context.TestTable1.WithTableHints(tableHints => tableHints.For<TestTable1>(tableHint));
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
            this._testContext,
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
        IQueryable<TestTable1> query = context.TestTable1.WithTableHints(builder => builder.For<TestTable1>(tableHints));
        string sql = query.ToQueryString();

        // ASSERT
        Assert.Contains(tableHintClause, sql, "Table hint missing from SQL.");
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
    public async Task TableHintsTests_007_Async()
    {
        // ARRANGE
        using Context context = await DatabaseUtils.OpenDatabaseAsync(
            this._testContext,
            DatabaseTypes.SqlServer,
            customConfigurationActions: optionsBuilder => optionsBuilder.UseTableHintExtensions())
            .ConfigureAwait(false);

        string tableHintClause = $"WITH ({SqlServerTableHints.NoLock})";
        string? value = null;

        IQueryable<TestTable1> query = context.TestTable1
            .WithTableHints(tableHints => tableHints.For<TestTable1>(SqlServerTableHints.NoLock))
            .Where(e => e.TestField == value);

        // ACT
        string sqlWithNull = query.ToQueryString();

        value = "Test";
        string sqlWithValue = query.ToQueryString();

        // ASSERT
        Assert.AreNotEqual(sqlWithNull, sqlWithValue, "SQL was not generated again, so the test is not valid.");

        Assert.Contains(tableHintClause, sqlWithNull, "Table hint missing from SQL (null parameter).");
        Assert.Contains(tableHintClause, sqlWithValue, "Table hint missing from SQL (non-null parameter).");

        Assert.IsEmpty(GetCustomTags(sqlWithNull).TableHints, "Table hints tag included in SQL (null parameter).");
        Assert.IsEmpty(GetCustomTags(sqlWithValue).TableHints, "Table hints tag included in SQL (non-null parameter).");
    }

    /// <summary>
    /// Test WithTableHints with a Null IQueryable Source parameter.
    /// </summary>
    [TestMethod(DisplayName = "WithTableHints with a Null IQueryable Source parameter")]
    public void TableHintsTests_008()
    {
        // ARRANGE
        IQueryable<TestTable1> source = null!;

        // ACT / ASSERT
        Assert.ThrowsExactly<ArgumentNullException>(() => source.WithTableHints(tableHints => tableHints.For<TestTable1>(SqlServerTableHints.NoLock)), "Unexpected exception");
    }

    /// <summary>
    /// Test WithTableHints leaves the query unchanged on SQLite (table hints are SQL Server only).
    /// </summary>
    /// <returns>A <see cref="Task"/> that represents the asynchronous operation.</returns>
    [TestMethod(DisplayName = "WithTableHints leaves the query unchanged on SQLite")]
    public async Task TableHintsTests_009_Async()
    {
        // ARRANGE
        using Context context = await DatabaseUtils.OpenDatabaseAsync(this._testContext, DatabaseTypes.Sqlite).ConfigureAwait(false);

        string expectedSql = context.TestTable1.ToQueryString();

        // ACT
        string sqlWithHint = context.TestTable1.WithTableHints(tableHints => tableHints.For<TestTable1>(SqlServerTableHints.NoLock)).ToQueryString();
        string sqlWithNoHints = context.TestTable1.WithTableHints(_ => { }).ToQueryString();

        // ASSERT
        Assert.AreEqual(expectedSql, sqlWithHint, "Invalid SQL (with a hint).");
        Assert.AreEqual(expectedSql, sqlWithNoHints, "Invalid SQL (with no hints).");
    }

    /// <summary>
    /// Test queries using WithTableHints run on SQL Server.
    /// </summary>
    /// <returns>A <see cref="Task"/> that represents the asynchronous operation.</returns>
    [TestMethod(DisplayName = "WithTableHints queries run on SQL Server")]
    public async Task TableHintsTests_010_Async()
    {
        // ARRANGE
        using Context context = await DatabaseUtils.OpenDatabaseAsync(
            this._testContext,
            DatabaseTypes.SqlServer,
            customConfigurationActions: optionsBuilder => optionsBuilder.UseTableHintExtensions())
            .ConfigureAwait(false);

        await DatabaseUtils.CreateTestTableEntriesAsync(context, "TestValue", 3, this._testContext.CancellationToken).ConfigureAwait(false);

        // ACT
        List<TestTable1> allRows = await context.TestTable1
            .WithTableHints(tableHints => tableHints.For<TestTable1>(SqlServerTableHints.NoLock))
            .AsNoTracking()
            .ToListAsync(this._testContext.CancellationToken)
            .ConfigureAwait(false);

        List<TestTable1> filteredRows = await context.TestTable1
            .WithTableHints(tableHints => tableHints.For<TestTable1>(SqlServerTableHints.Index("PK_TestTable1"), SqlServerTableHints.NoLock))
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
    public async Task TableHintsTests_011_Async(string methodName)
    {
        // ARRANGE
        List<string> log = [];

        using Context context = await DatabaseUtils.OpenDatabaseAsync(
            this._testContext,
            DatabaseTypes.SqlServer,
            customConfigurationActions: optionsBuilder =>
            {
                optionsBuilder.UseTableHintExtensions();
                optionsBuilder.UseExecuteUpdateExtensions();
                optionsBuilder.LogTo(log.Add, [RelationalEventId.CommandExecuted], LogLevel.Information);
            })
            .ConfigureAwait(false);

        await DatabaseUtils.CreateTestTableEntriesAsync(context, "Original TestValue", 3, this._testContext.CancellationToken).ConfigureAwait(false);

        string tableHintClause = $"WITH ({SqlServerTableHints.RowLock})";
        string value = "Updated TestValue";

        IQueryable<TestTable1> source = context.TestTable1.WithTableHints(tableHints => tableHints.For<TestTable1>(SqlServerTableHints.RowLock));

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

    /// <summary>
    /// Test WithTableHints with EF Core's own ExecuteUpdateAsync / ExecuteDeleteAsync when the session has SET NOCOUNT ON.
    /// </summary>
    /// <param name="methodName">The name of the EF Core method to test.</param>
    /// <returns>A <see cref="Task"/> that represents the asynchronous operation.</returns>
    /// <remarks>
    /// The custom query generator leaves EF Core's own UPDATE / DELETE to EF Core, adding only the table hints. EF Core starts them with
    /// SET NOCOUNT OFF, so the count is still returned. The connection is opened first so the SET NOCOUNT ON stays in effect.
    /// </remarks>
    [TestMethod(DisplayName = "WithTableHints with EF Core's ExecuteUpdate / ExecuteDelete and SET NOCOUNT ON")]
    [DataRow(nameof(EntityFrameworkQueryableExtensions.ExecuteDeleteAsync), DisplayName = nameof(EntityFrameworkQueryableExtensions.ExecuteDeleteAsync))]
    [DataRow(nameof(EntityFrameworkQueryableExtensions.ExecuteUpdateAsync), DisplayName = nameof(EntityFrameworkQueryableExtensions.ExecuteUpdateAsync))]
    public async Task TableHintsTests_012_Async(string methodName)
    {
        // ARRANGE
        List<string> log = [];

        using Context context = await DatabaseUtils.OpenDatabaseAsync(
            this._testContext,
            DatabaseTypes.SqlServer,
            customConfigurationActions: optionsBuilder =>
            {
                optionsBuilder.UseTableHintExtensions();
                optionsBuilder.LogTo(log.Add, [RelationalEventId.CommandExecuted], LogLevel.Information);
            })
            .ConfigureAwait(false);

        await DatabaseUtils.CreateTestTableEntriesAsync(context, "Original TestValue", 10, this._testContext.CancellationToken).ConfigureAwait(false);

        await context.Database.OpenConnectionAsync(this._testContext.CancellationToken).ConfigureAwait(false);
        await context.Database.ExecuteSqlRawAsync("SET NOCOUNT ON", this._testContext.CancellationToken).ConfigureAwait(false);

        string tableHintClause = $"WITH ({SqlServerTableHints.RowLock})";

        IQueryable<TestTable1> source = context.TestTable1
            .WithTableHints(tableHints => tableHints.For<TestTable1>(SqlServerTableHints.RowLock))
            .Where(e => e.Id <= 3);

        log.Clear();

        // ACT
        int count = methodName switch
        {
            nameof(EntityFrameworkQueryableExtensions.ExecuteDeleteAsync) => await source
                .ExecuteDeleteAsync(this._testContext.CancellationToken)
                .ConfigureAwait(false),

            nameof(EntityFrameworkQueryableExtensions.ExecuteUpdateAsync) => await source
                .ExecuteUpdateAsync(builder => builder.SetProperty(e => e.TestField, "Updated TestValue"), this._testContext.CancellationToken)
                .ConfigureAwait(false),

            _ => throw new ArgumentException($"Unknown method {methodName}", nameof(methodName))
        };

        // ASSERT
        string sql = string.Join(Environment.NewLine, log);

        Assert.AreEqual(3, count, "Invalid row count");
        Assert.Contains(tableHintClause, sql, "Table hint missing from SQL.");
        Assert.Contains("SET NOCOUNT OFF", sql, "SET NOCOUNT OFF missing from SQL.");
    }

    /// <summary>
    /// Test WithTableHints on SQL Server without UseTableHintExtensions.
    /// </summary>
    /// <remarks>
    /// Without the custom query generator the hints would be ignored, so it must throw.
    /// No database connection is needed, as nothing is run.
    /// </remarks>
    [TestMethod(DisplayName = "WithTableHints on SQL Server without UseTableHintExtensions")]
    public void TableHintsTests_013()
    {
        // ARRANGE
        using Context context = new(DatabaseTypes.SqlServer, "Server=localhost;Initial Catalog=Dummy");

        // ACT / ASSERT
        InvalidOperationException exception = Assert.ThrowsExactly<InvalidOperationException>(() => context.TestTable1.WithTableHints(tableHints => tableHints.For<TestTable1>(SqlServerTableHints.NoLock)), "Unexpected exception");
        Assert.Contains(nameof(TableHintExtensions.UseTableHintExtensions), exception.Message, "Unexpected exception message");
        Assert.DoesNotContain("another extension library", exception.Message, "Unexpected exception message");
    }

    /// <summary>
    /// Test WithTableHints on SQL Server with only UseExecuteUpdateExtensions.
    /// </summary>
    /// <remarks>
    /// UseExecuteUpdateExtensions installs the same custom query generator, so the hints work (the upsert methods rely on this).
    /// The SQL is generated with ToQueryString, so no database connection is needed.
    /// </remarks>
    [TestMethod(DisplayName = "WithTableHints on SQL Server with only UseExecuteUpdateExtensions")]
    public void TableHintsTests_014()
    {
        // ARRANGE
        using Context context = new(
            DatabaseTypes.SqlServer,
            "Server=localhost;Initial Catalog=Dummy",
            optionsBuilder => optionsBuilder.UseExecuteUpdateExtensions());

        string tableHintClause = $"WITH ({SqlServerTableHints.NoLock})";

        // ACT
        string sql = context.TestTable1
            .WithTableHints(tableHints => tableHints.For<TestTable1>(SqlServerTableHints.NoLock))
            .ToQueryString();

        // ASSERT
        Assert.Contains(tableHintClause, sql, "Table hint missing from SQL.");
    }

    /// <summary>
    /// Test WithTableHints on SQL Server when another extension library has replaced the query generator.
    /// </summary>
    /// <remarks>
    /// UseTableHintExtensions has been called, but the query generator it installs has been replaced, so the hints would be ignored.
    /// It must throw and say the generator has been replaced. No database connection is needed, as nothing is run.
    /// </remarks>
    [TestMethod(DisplayName = "WithTableHints on SQL Server when another extension library has replaced the query generator")]
    public void TableHintsTests_015()
    {
        // ARRANGE
        using Context context = new(
            DatabaseTypes.SqlServer,
            "Server=localhost;Initial Catalog=Dummy",
            optionsBuilder =>
            {
                optionsBuilder.UseTableHintExtensions();
                optionsBuilder.ReplaceService<IQuerySqlGeneratorFactory, ConflictingSqlServerQuerySqlGeneratorFactory>();
            });

        // ACT / ASSERT
        InvalidOperationException exception = Assert.ThrowsExactly<InvalidOperationException>(() => context.TestTable1.WithTableHints(tableHints => tableHints.For<TestTable1>(SqlServerTableHints.NoLock)), "Unexpected exception");
        Assert.Contains("another extension library", exception.Message, "Unexpected exception message");
    }

    /// <summary>
    /// Test WithTableHints with a Null Action BuildAction parameter.
    /// </summary>
    [TestMethod(DisplayName = "WithTableHints with a Null Action BuildAction parameter")]
    public void TableHintsTests_025()
    {
        // ARRANGE
        using Context context = new(
            DatabaseTypes.SqlServer,
            "Server=localhost;Initial Catalog=Dummy",
            optionsBuilder => optionsBuilder.UseTableHintExtensions());

        Action<TableHintsBuilder> buildAction = null!;

        // ACT / ASSERT
        Assert.ThrowsExactly<ArgumentNullException>(() => context.TestTable1.WithTableHints(buildAction), "Unexpected exception");
    }

    /// <summary>
    /// Test WithTableHints with a BuildAction that adds no table hints.
    /// </summary>
    [TestMethod(DisplayName = "WithTableHints with a BuildAction that adds no table hints")]
    public void TableHintsTests_026()
    {
        // ARRANGE
        using Context context = new(
            DatabaseTypes.SqlServer,
            "Server=localhost;Initial Catalog=Dummy",
            optionsBuilder => optionsBuilder.UseTableHintExtensions());

        // ACT / ASSERT
        Assert.ThrowsExactly<ArgumentException>(() => context.TestTable1.WithTableHints(_ => { }), "Unexpected exception");
    }

    /// <summary>
    /// Test TableHintsBuilder.For with a Null IEnumerable TableHints parameter.
    /// </summary>
    [TestMethod(DisplayName = "TableHintsBuilder.For with a Null IEnumerable TableHints parameter")]
    public void TableHintsTests_027()
    {
        // ARRANGE
        using Context context = new(
            DatabaseTypes.SqlServer,
            "Server=localhost;Initial Catalog=Dummy",
            optionsBuilder => optionsBuilder.UseTableHintExtensions());

        IEnumerable<ITableHint> tableHints = null!;

        // ACT / ASSERT
        Assert.ThrowsExactly<ArgumentNullException>(() => context.TestTable1.WithTableHints(builder => builder.For<TestTable1>(tableHints)), "Unexpected exception");
    }

    /// <summary>
    /// Test TableHintsBuilder.For with a type that isn't an entity type in the model, or is a shared type entity type.
    /// </summary>
    [TestMethod(DisplayName = "TableHintsBuilder.For with a type that isn't an entity type in the model, or is a shared type entity type")]
    public void TableHintsTests_028()
    {
        // ARRANGE
        using Context context = new(
            DatabaseTypes.SqlServer,
            "Server=localhost;Initial Catalog=Dummy",
            optionsBuilder => optionsBuilder.UseTableHintExtensions());

        // ACT / ASSERT
        Assert.ThrowsExactly<ArgumentException>(() => context.TestTable1.WithTableHints(tableHints => tableHints.For<TestFieldValue>(SqlServerTableHints.NoLock)), "Unexpected exception (not in the model)");
        Assert.ThrowsExactly<ArgumentException>(() => context.TestTable1.WithTableHints(tableHints => tableHints.For<FreeText>(SqlServerTableHints.NoLock)), "Unexpected exception (shared type entity type)");
    }

    /// <summary>
    /// Test TableHintsBuilder.For with an entity type that isn't mapped to a table or view.
    /// </summary>
    [TestMethod(DisplayName = "TableHintsBuilder.For with an entity type that isn't mapped to a table or view")]
    public void TableHintsTests_029()
    {
        // ARRANGE
        using Context context = new(
            DatabaseTypes.SqlServer,
            "Server=localhost;Initial Catalog=Dummy",
            optionsBuilder => optionsBuilder.UseTableHintExtensions(),
            modelBuilder => modelBuilder.Entity<TestFieldValue>().HasNoKey().ToSqlQuery("SELECT TestField FROM TestTable1"));

        // ACT / ASSERT
        Assert.ThrowsExactly<ArgumentException>(() => context.TestTable1.WithTableHints(tableHints => tableHints.For<TestFieldValue>(SqlServerTableHints.NoLock)), "Unexpected exception");
    }

    /// <summary>
    /// Test TableHintsBuilder.For called twice for the same entity type.
    /// </summary>
    [TestMethod(DisplayName = "TableHintsBuilder.For called twice for the same entity type")]
    public void TableHintsTests_030()
    {
        // ARRANGE
        using Context context = new(
            DatabaseTypes.SqlServer,
            "Server=localhost;Initial Catalog=Dummy",
            optionsBuilder => optionsBuilder.UseTableHintExtensions());

        // ACT / ASSERT
        Assert.ThrowsExactly<ArgumentException>(
            () => context.TestTable1.WithTableHints(tableHints => tableHints
                .For<TestTable1>(SqlServerTableHints.NoLock)
                .For<TestTable1>(SqlServerTableHints.RowLock)),
            "Unexpected exception");
    }

    /// <summary>
    /// Test WithTableHints only adds the table hints to the table of the entity type they are for, in a query with a join.
    /// </summary>
    /// <returns>A <see cref="Task"/> that represents the asynchronous operation.</returns>
    /// <remarks>
    /// The INDEX hint names an index that only exists on TestTable1. If it were added to TestTable2 as well, SQL Server would reject the query.
    /// TestTable2's table name is different from its class name, so the table is found from the model.
    /// </remarks>
    [TestMethod(DisplayName = "WithTableHints only adds the table hints to their own table in a join")]
    public async Task TableHintsTests_031_Async()
    {
        // ARRANGE
        using Context context = await DatabaseUtils.OpenDatabaseAsync(
            this._testContext,
            DatabaseTypes.SqlServer,
            customConfigurationActions: optionsBuilder => optionsBuilder.UseTableHintExtensions())
            .ConfigureAwait(false);

        await DatabaseUtils.CreateTestTableEntriesAsync(context, "TestValue", 3, this._testContext.CancellationToken).ConfigureAwait(false);

        IQueryable<string> query = context.TestTable1
            .Join(context.TestTable2, t1 => t1.TestField, t2 => t2.TestField, (t1, t2) => t1.TestField)
            .WithTableHints(tableHints => tableHints.For<TestTable1>(SqlServerTableHints.Index("PK_TestTable1")));

        // ACT
        string sql = query.ToQueryString();
        List<string> rows = await query.ToListAsync(this._testContext.CancellationToken).ConfigureAwait(false);

        // ASSERT
        Assert.Contains("[TestTable1] AS [t] WITH (INDEX([PK_TestTable1]))", sql, "Table hint missing from TestTable1.");
        Assert.Contains("[TestTable2RealName] AS [t0] ON", sql, "Table hint added to TestTable2.");
        Assert.IsEmpty(rows, "Invalid row count.");
    }

    /// <summary>
    /// Test WithTableHints with different table hints for two entity types in one builder.
    /// </summary>
    /// <returns>A <see cref="Task"/> that represents the asynchronous operation.</returns>
    [TestMethod(DisplayName = "WithTableHints with different table hints for two entity types")]
    public async Task TableHintsTests_032_Async()
    {
        // ARRANGE
        using Context context = await DatabaseUtils.OpenDatabaseAsync(
            this._testContext,
            DatabaseTypes.SqlServer,
            customConfigurationActions: optionsBuilder => optionsBuilder.UseTableHintExtensions())
            .ConfigureAwait(false);

        IQueryable<string> query = context.TestTable1
            .Join(context.TestTable2, t1 => t1.TestField, t2 => t2.TestField, (t1, t2) => t1.TestField)
            .WithTableHints(tableHints => tableHints
                .For<TestTable1>(SqlServerTableHints.Index("PK_TestTable1"))
                .For<TestTable2>(SqlServerTableHints.Index("PK_TestTable2RealName"), SqlServerTableHints.NoLock));

        // ACT
        string sql = query.ToQueryString();
        List<string> rows = await query.ToListAsync(this._testContext.CancellationToken).ConfigureAwait(false);

        // ASSERT
        Assert.Contains("[TestTable1] AS [t] WITH (INDEX([PK_TestTable1]))", sql, "Table hint missing from TestTable1.");
        Assert.Contains("[TestTable2RealName] AS [t0] WITH (INDEX([PK_TestTable2RealName]), NOLOCK)", sql, "Table hints missing from TestTable2.");
        Assert.IsEmpty(rows, "Invalid row count.");
    }

    /// <summary>
    /// Test WithTableHints called on both queries of a join, and on a query used in a subquery.
    /// </summary>
    /// <returns>A <see cref="Task"/> that represents the asynchronous operation.</returns>
    /// <remarks>EF Core collects the tags from the whole query, so each table's tag can come from any part of it.</remarks>
    [TestMethod(DisplayName = "WithTableHints called on both queries of a join, and on a query used in a subquery")]
    public async Task TableHintsTests_033_Async()
    {
        // ARRANGE
        using Context context = await DatabaseUtils.OpenDatabaseAsync(
            this._testContext,
            DatabaseTypes.SqlServer,
            customConfigurationActions: optionsBuilder => optionsBuilder.UseTableHintExtensions())
            .ConfigureAwait(false);

        await DatabaseUtils.CreateTestTableEntriesAsync(context, "TestValue", 3, this._testContext.CancellationToken).ConfigureAwait(false);

        IQueryable<TestTable2> hintedTestTable2 = context.TestTable2.WithTableHints(tableHints => tableHints.For<TestTable2>(SqlServerTableHints.NoLock));

        IQueryable<string> joinQuery = context.TestTable1
            .WithTableHints(tableHints => tableHints.For<TestTable1>(SqlServerTableHints.Index("PK_TestTable1")))
            .Join(hintedTestTable2, t1 => t1.TestField, t2 => t2.TestField, (t1, t2) => t1.TestField);

        IQueryable<TestTable1> subquery = context.TestTable1
            .Where(t1 => !hintedTestTable2.Any(t2 => t2.TestField == t1.TestField));

        // ACT
        string joinSql = joinQuery.ToQueryString();
        List<string> joinRows = await joinQuery.ToListAsync(this._testContext.CancellationToken).ConfigureAwait(false);

        string subquerySql = subquery.ToQueryString();
        List<TestTable1> subqueryRows = await subquery.ToListAsync(this._testContext.CancellationToken).ConfigureAwait(false);

        // ASSERT
        Assert.Contains("[TestTable1] AS [t] WITH (INDEX([PK_TestTable1]))", joinSql, "Table hint missing from TestTable1 (join).");
        Assert.Contains("[TestTable2RealName] AS [t0] WITH (NOLOCK)", joinSql, "Table hint missing from TestTable2 (join).");
        Assert.IsEmpty(joinRows, "Invalid row count (join).");

        Assert.Contains("[TestTable1] AS [t]\r\n", subquerySql.ReplaceLineEndings("\r\n"), "Table hint added to TestTable1 (subquery).");
        Assert.Contains("[TestTable2RealName] AS [t0] WITH (NOLOCK)", subquerySql, "Table hint missing from TestTable2 (subquery).");
        Assert.HasCount(3, subqueryRows, "Invalid row count (subquery).");
    }

    /// <summary>
    /// Test WithTableHints with an included navigation's entity type.
    /// </summary>
    /// <returns>A <see cref="Task"/> that represents the asynchronous operation.</returns>
    [TestMethod(DisplayName = "WithTableHints with an included navigation's entity type")]
    public async Task TableHintsTests_034_Async()
    {
        // ARRANGE
        using Context context = await DatabaseUtils.OpenDatabaseAsync(
            this._testContext,
            DatabaseTypes.SqlServer,
            customConfigurationActions: optionsBuilder => optionsBuilder.UseTableHintExtensions(),
            customModelCreationActions: modelBuilder =>
            {
                modelBuilder.Entity<NavigationParent>();
                modelBuilder.Entity<NavigationChild>();
            })
            .ConfigureAwait(false);

        NavigationParent parent = new() { Name = "Parent" };
        parent.Children.Add(new() { Name = "Child 1" });
        parent.Children.Add(new() { Name = "Child 2" });
        context.Add(parent);
        await context.SaveChangesAsync(this._testContext.CancellationToken).ConfigureAwait(false);
        context.ChangeTracker.Clear();

        IQueryable<NavigationParent> query = context.Set<NavigationParent>()
            .Include(p => p.Children)
            .WithTableHints(tableHints => tableHints
                .For<NavigationParent>(SqlServerTableHints.Index("PK_NavigationParent"))
                .For<NavigationChild>(SqlServerTableHints.NoLock));

        // ACT
        string sql = query.ToQueryString();
        List<NavigationParent> rows = await query.ToListAsync(this._testContext.CancellationToken).ConfigureAwait(false);

        // ASSERT
        Assert.Contains("[NavigationParent] AS [n] WITH (INDEX([PK_NavigationParent]))", sql, "Table hint missing from NavigationParent.");
        Assert.Contains("[NavigationChild] AS [n0] WITH (NOLOCK)", sql, "Table hint missing from NavigationChild.");
        Assert.HasCount(1, rows, "Invalid row count.");
        Assert.HasCount(2, rows[0].Children, "Invalid child count.");
    }

    /// <summary>
    /// Test WithTableHints called twice with different table hints for the same table.
    /// </summary>
    /// <remarks>The tags are only read when the SQL is generated, so it throws then. No database connection is needed, as nothing is run.</remarks>
    [TestMethod(DisplayName = "WithTableHints called twice with different table hints for the same table")]
    public void TableHintsTests_035()
    {
        // ARRANGE
        using Context context = new(
            DatabaseTypes.SqlServer,
            "Server=localhost;Initial Catalog=Dummy",
            optionsBuilder => optionsBuilder.UseTableHintExtensions());

        IQueryable<TestTable1> query = context.TestTable1
            .WithTableHints(tableHints => tableHints.For<TestTable1>(SqlServerTableHints.NoLock))
            .WithTableHints(tableHints => tableHints.For<TestTable1>(SqlServerTableHints.RowLock));

        // ACT / ASSERT
        Assert.ThrowsExactly<InvalidOperationException>(() => query.ToQueryString(), "Unexpected exception");
    }

    /// <summary>
    /// Test WithTableHints with an entity type mapped to a view.
    /// </summary>
    /// <remarks>The SQL is generated with ToQueryString, so no database connection (or view) is needed.</remarks>
    [TestMethod(DisplayName = "WithTableHints with an entity type mapped to a view")]
    public void TableHintsTests_036()
    {
        // ARRANGE
        using Context context = new(
            DatabaseTypes.SqlServer,
            "Server=localhost;Initial Catalog=Dummy",
            optionsBuilder => optionsBuilder.UseTableHintExtensions(),
            modelBuilder => modelBuilder.Entity<TestFieldValue>().HasNoKey().ToView("TestFieldView", "views"));

        // ACT
        string sql = context.Set<TestFieldValue>()
            .WithTableHints(tableHints => tableHints.For<TestFieldValue>(SqlServerTableHints.NoExpand))
            .ToQueryString();

        // ASSERT
        Assert.Contains("[views].[TestFieldView] AS [t] WITH (NOEXPAND)", sql, "Table hint missing from the view.");
    }

    /// <summary>
    /// Test a tag written by the caller that looks like a table hints tag stays a comment and adds no table hints.
    /// </summary>
    /// <remarks>
    /// The library's tag prefix has a random part, so text from outside the application passed to TagWith can't be taken for table hints
    /// and written into the SQL. The SQL is generated with ToQueryString, so no database connection is needed.
    /// </remarks>
    [TestMethod(DisplayName = "A tag that looks like a table hints tag stays a comment")]
    public void TableHintsTests_037()
    {
        // ARRANGE
        using Context context = new(
            DatabaseTypes.SqlServer,
            "Server=localhost;Initial Catalog=Dummy",
            optionsBuilder => optionsBuilder.UseTableHintExtensions());

        string forgedTag = "$$TABLEHINTS$$[TestTable1] NOLOCK) WHERE 1 = 0 --";

        // ACT
        string sql = context.TestTable1
            .TagWith(forgedTag)
            .ToQueryString();

        // ASSERT
        Assert.Contains($"-- {forgedTag}", sql, "The tag is not a comment.");
        Assert.DoesNotContain("WITH (NOLOCK)", sql, "The tag was taken for table hints.");
    }

    #endregion WithTableHints tests

    #region Index tests

    /// <summary>
    /// Test SqlServerTableHint.Index with a Null or Empty IEnumerable IndexValues parameter.
    /// </summary>
    /// <param name="indexValues">The index values (names).</param>
    /// <param name="exceptionType">Type of exception raised.</param>
    [TestMethod(DisplayName = "SqlServerTableHint.Index with a Null or Empty IEnumerable IndexValues parameter")]
    [DataRow(null!, typeof(ArgumentNullException), DisplayName = "Null")]
    [DataRow(new string[0], typeof(ArgumentException), DisplayName = "Empty")]
    public void TableHintsTests_016(IEnumerable<string> indexValues, Type exceptionType)
    {
        // ARRANGE

        // ACT / ASSERT
        Exception e = Assert.Throws<Exception>(() => SqlServerTableHints.Index(indexValues), "Unexpected exception");
        Assert.AreEqual(exceptionType, e.GetType(), "Unexpected exception type");
    }

    /// <summary>
    /// Test SqlServerTableHint.Index with a Null IndexValues parameter value.
    /// </summary>
    [TestMethod(DisplayName = "SqlServerTableHint.Index with a Null IndexValues parameter value")]
    public void TableHintsTests_017()
    {
        // ARRANGE
        string indexValue = null!;

        // ACT / ASSERT
        Assert.ThrowsExactly<ArgumentException>(() => SqlServerTableHints.Index(indexValue), "Unexpected exception");
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
    public void TableHintsTests_018(string[] indexValues, string expectedHint)
    {
        // ARRANGE

        // ACT
        SqlServerTableHints tableHint = SqlServerTableHints.Index(indexValues);

        // ASSERT
        Assert.AreEqual(expectedHint, tableHint.ToString(), "Invalid hint");
    }

    #endregion Index tests

    #region ForceSeek tests

    /// <summary>
    /// Test SqlServerTableHint.ForceSeek with a Null or Empty IndexValue parameter value.
    /// </summary>
    /// <param name="indexValue">The index value (name).</param>
    /// <param name="exceptionType">Type of exception raised.</param>
    [TestMethod(DisplayName = "SqlServerTableHint.ForceSeek with a Null or Empty IndexValue parameter value")]
    [DataRow(null!, typeof(ArgumentNullException), DisplayName = "Null")]
    [DataRow("", typeof(ArgumentException), DisplayName = "Empty")]
    public void TableHintsTests_019(string indexValue, Type exceptionType)
    {
        // ARRANGE
        IEnumerable<string> indexColumnNames = ["DummyIndexColumnName"];

        // ACT / ASSERT
        Exception e = Assert.Throws<Exception>(() => SqlServerTableHints.ForceSeek(indexValue, indexColumnNames), "Unexpected exception");
        Assert.AreEqual(exceptionType, e.GetType(), "Unexpected exception type");
    }

    /// <summary>
    /// Test SqlServerTableHint.ForceSeek with a Null or Empty IEnumerable IndexColumnNames parameter.
    /// </summary>
    /// <param name="indexColumnNames">The index values (names).</param>
    /// <param name="exceptionType">Type of exception raised.</param>
    [TestMethod(DisplayName = "SqlServerTableHint.ForceSeek with a Null or Empty IEnumerable IndexColumnNames parameter")]
    [DataRow(null!, typeof(ArgumentNullException), DisplayName = "Null")]
    [DataRow(new string[0], typeof(ArgumentException), DisplayName = "Empty")]
    public void TableHintsTests_020(IEnumerable<string> indexColumnNames, Type exceptionType)
    {
        // ARRANGE
        string indexValue = "DummyIndexValue";

        // ACT / ASSERT
        Exception e = Assert.Throws<Exception>(() => SqlServerTableHints.ForceSeek(indexValue, indexColumnNames), "Unexpected exception");
        Assert.AreEqual(exceptionType, e.GetType(), "Unexpected exception type");
    }

    /// <summary>
    /// Test SqlServerTableHint.ForceSeek with a Null IndexColumnNames parameter value.
    /// </summary>
    [TestMethod(DisplayName = "SqlServerTableHint.ForceSeek with a Null IndexColumnNames parameter value")]
    public void TableHintsTests_021()
    {
        // ARRANGE
        string indexValue = "DummyIndexValue";
        string indexColumnName = null!;

        // ACT / ASSERT
        Assert.ThrowsExactly<ArgumentException>(() => SqlServerTableHints.ForceSeek(indexValue, indexColumnName), "Unexpected exception");
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
    public void TableHintsTests_022(string indexValue, string[] indexColumnNames, string expectedHint)
    {
        // ARRANGE

        // ACT
        SqlServerTableHints tableHint = SqlServerTableHints.ForceSeek(indexValue, indexColumnNames);

        // ASSERT
        Assert.AreEqual(expectedHint, tableHint.ToString(), "Invalid hint");
    }

    #endregion ForceSeek tests

    #region SpatialWindowMaxCells tests

    /// <summary>
    /// Test SqlServerTableHint.SpatialWindowMaxCells with an Int Value outside the allowed range.
    /// </summary>
    /// <param name="value">Specifies the maximum number of cells to use for tessellating a geometry or geography object (1-8192).</param>
    [TestMethod(DisplayName = "SqlServerTableHint.SpatialWindowMaxCells with an Int Value outside the allowed range")]
    [DataRow(0, DisplayName = "0")]
    [DataRow(8193, DisplayName = "8193")]
    public void TableHintsTests_023(int value)
    {
        // ARRANGE

        // ACT / ASSERT
        Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => SqlServerTableHints.SpatialWindowMaxCells(value), "Unexpected exception");
    }

    /// <summary>
    /// Test SqlServerTableHint.SpatialWindowMaxCells generates the correct hint.
    /// </summary>
    /// <param name="value">Specifies the maximum number of cells to use for tessellating a geometry or geography object (1-8192).</param>
    /// <param name="expectedHint">The expected hint.</param>
    [TestMethod(DisplayName = "SqlServerTableHint.SpatialWindowMaxCells generates the correct hint")]
    [DataRow(1, "SPATIAL_WINDOW_MAX_CELLS = 1", DisplayName = "1")]
    [DataRow(8192, "SPATIAL_WINDOW_MAX_CELLS = 8192", DisplayName = "8192")]
    public void TableHintsTests_024(int value, string expectedHint)
    {
        // ARRANGE

        // ACT
        SqlServerTableHints tableHint = SqlServerTableHints.SpatialWindowMaxCells(value);

        // ASSERT
        Assert.AreEqual(expectedHint, tableHint.ToString(), "Invalid hint");
    }

    #endregion SpatialWindowMaxCells tests

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
        yield return SqlServerTableHints.UpdLock;
        yield return SqlServerTableHints.XLock;
        yield return SqlServerTableHints.Index("DummyIndexValue");
        yield return SqlServerTableHints.ForceSeek();
        yield return SqlServerTableHints.ForceSeek("DummyIndexValue", "DummyIndexColumnName");
        yield return SqlServerTableHints.SpatialWindowMaxCells(100);
    }

    /// <summary>
    /// Read the custom tags from the comment lines of some SQL, in the same way the custom query generators read a query's tags.
    /// </summary>
    /// <param name="sql">The SQL.</param>
    /// <returns>The custom values found in the SQL's comment lines.</returns>
    private static CustomTags GetCustomTags(string sql)
    {
        HashSet<string> comments = sql
            .Split('\n')
            .Select(line => line.Trim())
            .Where(line => line.StartsWith("-- ", StringComparison.Ordinal))
            .Select(line => line[3..])
            .ToHashSet();

        return CustomTagExtensions.GetCustomTags(comments);
    }

    #endregion private methods
}