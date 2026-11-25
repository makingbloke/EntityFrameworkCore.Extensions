// Copyright ©2021-2026 Mike King.
// This file is licensed to you under the MIT license.
// See the License.txt file in the solution root for more information.

using DotDoc.EntityFrameworkCore.Extensions.DatabaseType;
using DotDoc.EntityFrameworkCore.Extensions.ExecuteUpdate;
using DotDoc.EntityFrameworkCore.Extensions.Tests.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Query;
using System.Data.Common;

namespace DotDoc.EntityFrameworkCore.Extensions.Tests.ExecuteUpdate;

/// <summary>
/// Tests for ExecuteInsertGetRow extensions.
/// </summary>
[TestClass]
public class ExecuteInsertGetRowTests
{
    #region private fields

    /// <summary>
    /// The MSTest <see cref="TestContext"/> for the current test.
    /// </summary>
    private readonly TestContext _testContext;

    #endregion private fields

    #region public constructors

    /// <summary>
    /// Initializes a new instance of the <see cref="ExecuteInsertGetRowTests"/> class.
    /// </summary>
    /// <param name="testContext">The MSTest test context.</param>
    public ExecuteInsertGetRowTests(TestContext testContext)
    {
        this._testContext = testContext;
    }

    #endregion public constructors

    #region ExecuteInsertGetRowAsync tests

    /// <summary>
    /// Test ExecuteInsertGetRowAsync with Null IQueryable source parameter.
    /// </summary>
    /// <returns>A <see cref="Task"/> that represents the asynchronous operation.</returns>
    [TestMethod(DisplayName = "ExecuteInsertGetRowAsync with Null IQueryable source parameter")]
    public async Task ExecuteInsertGetRowTests_001_Async()
    {
        // ARRANGE
        IQueryable<TestTable1> source = null!;
        Action<UpdateSettersBuilder<TestTable1>> setPropertyCalls = new(builder => { });

        // ACT / ASSERT
        await Assert.ThrowsExactlyAsync<ArgumentNullException>(() => source.ExecuteInsertGetRowAsync(setPropertyCalls, this._testContext.CancellationToken), "Unexpected exception").ConfigureAwait(false);
    }

    /// <summary>
    /// Test ExecuteInsertGetRowAsync with Null UpdateSettersBuilder setPropertyCalls parameter.
    /// </summary>
    /// <returns>A <see cref="Task"/> that represents the asynchronous operation.</returns>
    [TestMethod(DisplayName = "ExecuteInsertGetRowAsync with Null UpdateSettersBuilder setPropertyCalls parameter")]
    public async Task ExecuteInsertGetRowTests_002_Async()
    {
        // ARRANGE
        IQueryable<TestTable1> source = Array.Empty<TestTable1>().AsQueryable();
        Action<UpdateSettersBuilder<TestTable1>> setPropertyCalls = null!;

        // ACT / ASSERT
        await Assert.ThrowsExactlyAsync<ArgumentNullException>(() => source.ExecuteInsertGetRowAsync(setPropertyCalls, this._testContext.CancellationToken), "Unexpected exception").ConfigureAwait(false);
    }

    /// <summary>
    /// Test ExecuteInsertGetRowAsync.
    /// </summary>
    /// <param name="databaseType">Database type.</param>
    /// <returns>A <see cref="Task"/> that represents the asynchronous operation.</returns>
    [TestMethod(DisplayName = "ExecuteInsertGetRowAsync")]
    [DataRow(DatabaseTypes.Sqlite, DisplayName = DatabaseTypes.Sqlite)]
    [DataRow(DatabaseTypes.SqlServer, DisplayName = DatabaseTypes.SqlServer)]
    public async Task ExecuteInsertGetRowTests_003_Async(string databaseType)
    {
        // ARRANGE
        using Context context = await DatabaseUtils.OpenDatabaseAsync(
            this._testContext,
            databaseType: databaseType,
            customConfigurationActions: (optionsBuilder) =>
            {
                optionsBuilder.UseExecuteUpdateExtensions();
            })
            .ConfigureAwait(false);

        long id = 1;
        string value = "TestValue";

        // ACT
        TestTable1 row = await context.TestTable1.ExecuteInsertGetRowAsync(
            builder =>
            {
                builder.SetProperty(e => e.TestField, value);
            },
            this._testContext.CancellationToken)
            .ConfigureAwait(false);

        // ASSERT
        ExecuteUpdateTestUtils.AssertStateCleared(context);

        Assert.IsNotNull(row, "No row returned");
        Assert.AreEqual(id, row.Id, "Unexpected Id value");
        Assert.AreEqual(value, row.TestField, "Unexpected field value");
    }

    /// <summary>
    /// Test ExecuteInsertGetRowAsync when another ExecuteUpdate call is in progress on the DbContext.
    /// </summary>
    /// <param name="databaseType">Database type.</param>
    /// <returns>A <see cref="Task"/> that represents the asynchronous operation.</returns>
    /// <remarks>
    /// The context's <see cref="ExecuteUpdateState"/> only holds a query type while a call is running, so one already being set means
    /// another call is in progress. The call must throw without inserting anything, and must leave the other call's state alone.
    /// </remarks>
    [TestMethod(DisplayName = "ExecuteInsertGetRowAsync when another ExecuteUpdate call is in progress on the DbContext")]
    [DataRow(DatabaseTypes.Sqlite, DisplayName = DatabaseTypes.Sqlite)]
    [DataRow(DatabaseTypes.SqlServer, DisplayName = DatabaseTypes.SqlServer)]
    public async Task ExecuteInsertGetRowTests_004_Async(string databaseType)
    {
        // ARRANGE
        using Context context = await DatabaseUtils.OpenDatabaseAsync(
            this._testContext,
            databaseType,
            customConfigurationActions: optionsBuilder => optionsBuilder.UseExecuteUpdateExtensions())
            .ConfigureAwait(false);

        Action<UpdateSettersBuilder<TestTable1>> setPropertyCalls = new(builder => builder.SetProperty(e => e.TestField, "TestValue"));

        ExecuteUpdateState state = context.GetService<ExecuteUpdateState>();
        state.SetQueryType(QueryType.DeleteGetRows);

        // ACT / ASSERT
        await Assert.ThrowsExactlyAsync<InvalidOperationException>(() => context.TestTable1.ExecuteInsertGetRowAsync(setPropertyCalls, this._testContext.CancellationToken), "Unexpected exception").ConfigureAwait(false);

        Assert.AreEqual(QueryType.DeleteGetRows, state.QueryType, "The state of the call in progress has been changed.");

        await state.ResetStateAsync(this._testContext.CancellationToken).ConfigureAwait(false);

        int rowCount = await context.TestTable1.CountAsync(this._testContext.CancellationToken).ConfigureAwait(false);
        Assert.AreEqual(0, rowCount, "Unexpected count");
    }

    /// <summary>
    /// Test ExecuteInsertGetRowAsync with a source that has a Where clause.
    /// </summary>
    /// <param name="databaseType">Database type.</param>
    /// <returns>A <see cref="Task"/> that represents the asynchronous operation.</returns>
    /// <remarks>An insert can't have a Where clause, so it must be rejected rather than run as an update.</remarks>
    [TestMethod(DisplayName = "ExecuteInsertGetRowAsync with a source that has a Where clause")]
    [DataRow(DatabaseTypes.Sqlite, DisplayName = DatabaseTypes.Sqlite)]
    [DataRow(DatabaseTypes.SqlServer, DisplayName = DatabaseTypes.SqlServer)]
    public async Task ExecuteInsertGetRowTests_005_Async(string databaseType)
    {
        // ARRANGE
        using Context context = await DatabaseUtils.OpenDatabaseAsync(
            this._testContext,
            databaseType: databaseType,
            customConfigurationActions: (optionsBuilder) =>
            {
                optionsBuilder.UseExecuteUpdateExtensions();
            })
            .ConfigureAwait(false);

        long id = 1;
        string originalValue = "Original TestValue";
        string insertedValue = "Inserted TestValue";

        await DatabaseUtils.CreateTestTableEntriesAsync(context, originalValue, 1, this._testContext.CancellationToken).ConfigureAwait(false);

        IQueryable<TestTable1> source = context.TestTable1.Where(e => e.Id == id);

        // ACT / ASSERT
        await Assert.ThrowsExactlyAsync<InvalidOperationException>(
            () => source.ExecuteInsertGetRowAsync(
                builder =>
                {
                    builder.SetProperty(e => e.TestField, insertedValue);
                },
                this._testContext.CancellationToken),
            "Unexpected exception")
            .ConfigureAwait(false);

        List<TestTable1> rows = await context.TestTable1
            .AsNoTracking()
            .ToListAsync(this._testContext.CancellationToken)
            .ConfigureAwait(false);

        Assert.HasCount(1, rows, "Unexpected count");
        Assert.AreEqual(originalValue, rows[0].TestField, "Unexpected field value");
    }

    /// <summary>
    /// Test ExecuteInsertGetRowAsync passes on the database exception when the insert fails.
    /// </summary>
    /// <param name="databaseType">Database type.</param>
    /// <returns>A <see cref="Task"/> that represents the asynchronous operation.</returns>
    /// <remarks>
    /// The second insert breaks the unique index on <see cref="TestTable2.TestField"/>. The context's <see cref="ExecuteUpdateState"/>
    /// must still be cleared, so that a later call on the same context works.
    /// </remarks>
    [TestMethod(DisplayName = "ExecuteInsertGetRowAsync passes on the database exception when the insert fails")]
    [DataRow(DatabaseTypes.Sqlite, DisplayName = DatabaseTypes.Sqlite)]
    [DataRow(DatabaseTypes.SqlServer, DisplayName = DatabaseTypes.SqlServer)]
    public async Task ExecuteInsertGetRowTests_006_Async(string databaseType)
    {
        // ARRANGE
        using Context context = await DatabaseUtils.OpenDatabaseAsync(
            this._testContext,
            databaseType: databaseType,
            customConfigurationActions: (optionsBuilder) =>
            {
                optionsBuilder.UseExecuteUpdateExtensions();
            })
            .ConfigureAwait(false);

        string value = "TestValue";
        Action<UpdateSettersBuilder<TestTable2>> setPropertyCalls = new(builder => builder.SetProperty(e => e.TestField, value));

        await context.TestTable2.ExecuteInsertGetRowAsync(setPropertyCalls, this._testContext.CancellationToken).ConfigureAwait(false);

        // ACT / ASSERT
        await Assert.ThrowsAsync<DbException>(() => context.TestTable2.ExecuteInsertGetRowAsync(setPropertyCalls, this._testContext.CancellationToken), "Unexpected exception").ConfigureAwait(false);

        ExecuteUpdateTestUtils.AssertStateCleared(context);

        List<TestTable2> rows = await context.TestTable2
            .AsNoTracking()
            .ToListAsync(this._testContext.CancellationToken)
            .ConfigureAwait(false);

        Assert.HasCount(1, rows, "Unexpected count");

        string otherValue = "Other TestValue";
        TestTable2 row = await context.TestTable2.ExecuteInsertGetRowAsync(builder => builder.SetProperty(e => e.TestField, otherValue), this._testContext.CancellationToken).ConfigureAwait(false);

        Assert.AreEqual(otherValue, row.TestField, "Unexpected field value after the failed insert");
    }

    /// <summary>
    /// Test ExecuteInsertGetRowAsync without UseExecuteUpdateExtensions.
    /// </summary>
    /// <param name="databaseType">Database type.</param>
    /// <returns>A <see cref="Task"/> that represents the asynchronous operation.</returns>
    /// <remarks>Without the extensions the insert would run as an update of every row, so it must throw before anything is changed.</remarks>
    [TestMethod(DisplayName = "ExecuteInsertGetRowAsync without UseExecuteUpdateExtensions")]
    [DataRow(DatabaseTypes.Sqlite, DisplayName = DatabaseTypes.Sqlite)]
    [DataRow(DatabaseTypes.SqlServer, DisplayName = DatabaseTypes.SqlServer)]
    public async Task ExecuteInsertGetRowTests_007_Async(string databaseType)
    {
        // ARRANGE
        using Context context = await DatabaseUtils.OpenDatabaseAsync(this._testContext, databaseType).ConfigureAwait(false);

        string originalValue = "Original TestValue";
        string insertedValue = "Inserted TestValue";

        await DatabaseUtils.CreateTestTableEntriesAsync(context, originalValue, 5, this._testContext.CancellationToken).ConfigureAwait(false);

        // ACT / ASSERT
        InvalidOperationException exception = await Assert.ThrowsExactlyAsync<InvalidOperationException>(
            () => context.TestTable1.ExecuteInsertGetRowAsync(
                builder =>
                {
                    builder.SetProperty(e => e.TestField, insertedValue);
                },
                this._testContext.CancellationToken),
            "Unexpected exception")
            .ConfigureAwait(false);

        Assert.Contains(nameof(ExecuteUpdateExtensions.UseExecuteUpdateExtensions), exception.Message, "Unexpected exception message");

        List<TestTable1> rows = await context.TestTable1
            .AsNoTracking()
            .ToListAsync(this._testContext.CancellationToken)
            .ConfigureAwait(false);

        Assert.HasCount(5, rows, "Unexpected count");
        Assert.IsTrue(rows.All(r => r.TestField == originalValue), "Existing rows have been changed");
    }

    /// <summary>
    /// Test ExecuteInsertGetRowAsync without the RETURNING (SQLite) / OUTPUT (SQL Server) clause.
    /// </summary>
    /// <param name="databaseType">Database type.</param>
    /// <returns>A <see cref="Task"/> that represents the asynchronous operation.</returns>
    /// <remarks>The inserted row can only be returned with the clause, so it must throw before anything is inserted.</remarks>
    [TestMethod(DisplayName = "ExecuteInsertGetRowAsync without the RETURNING / OUTPUT clause")]
    [DataRow(DatabaseTypes.Sqlite, DisplayName = DatabaseTypes.Sqlite)]
    [DataRow(DatabaseTypes.SqlServer, DisplayName = DatabaseTypes.SqlServer)]
    public async Task ExecuteInsertGetRowTests_008_Async(string databaseType)
    {
        // ARRANGE
        using Context context = await DatabaseUtils.OpenDatabaseAsync(
            this._testContext,
            databaseType,
            customConfigurationActions: optionsBuilder => optionsBuilder.UseExecuteUpdateExtensions(),
            customModelCreationActions: modelBuilder => ExecuteUpdateTestUtils.TurnOffReturningClause(modelBuilder, databaseType))
            .ConfigureAwait(false);

        Action<UpdateSettersBuilder<TestTable1>> setPropertyCalls = new(builder => builder.SetProperty(e => e.TestField, "TestValue"));

        // ACT / ASSERT
        NotSupportedException exception = await Assert.ThrowsExactlyAsync<NotSupportedException>(() => context.TestTable1.ExecuteInsertGetRowAsync(setPropertyCalls, this._testContext.CancellationToken), "Unexpected exception").ConfigureAwait(false);

        ExecuteUpdateTestUtils.AssertReturningClauseNotUsedException(exception, databaseType, nameof(ExecuteUpdateExtensions.ExecuteInsertGetRowAsync));
        ExecuteUpdateTestUtils.AssertStateCleared(context);

        int rowCount = await context.TestTable1.CountAsync(this._testContext.CancellationToken).ConfigureAwait(false);
        Assert.AreEqual(0, rowCount, "Unexpected count");
    }

    /// <summary>
    /// Test ExecuteInsertGetRowAsync with braces in the SQL.
    /// </summary>
    /// <param name="databaseType">Database type.</param>
    /// <returns>A <see cref="Task"/> that represents the asynchronous operation.</returns>
    /// <remarks>
    /// The captured SQL is run again with SqlQueryRaw, which treats braces as placeholders, so they must be escaped. An insert can't have
    /// a Where clause, so the braces are in a tag, which is written into the SQL as it is. Unescaped, the tag throws a FormatException.
    /// </remarks>
    [TestMethod(DisplayName = "ExecuteInsertGetRowAsync with braces in the SQL")]
    [DataRow(DatabaseTypes.Sqlite, DisplayName = DatabaseTypes.Sqlite)]
    [DataRow(DatabaseTypes.SqlServer, DisplayName = DatabaseTypes.SqlServer)]
    public async Task ExecuteInsertGetRowTests_009_Async(string databaseType)
    {
        // ARRANGE
        CancellationToken cancellationToken = this._testContext.CancellationToken;

        using Context context = await DatabaseUtils.OpenDatabaseAsync(
            this._testContext,
            databaseType,
            customConfigurationActions: optionsBuilder => optionsBuilder.UseExecuteUpdateExtensions())
            .ConfigureAwait(false);

        string value = "{0} }{";

        // ACT
        TestTable1 row = await context.TestTable1
            .TagWith("Tag with braces { } {0} }{")
            .ExecuteInsertGetRowAsync(builder => builder.SetProperty(e => e.TestField, value), cancellationToken)
            .ConfigureAwait(false);

        // ASSERT
        ExecuteUpdateTestUtils.AssertStateCleared(context);

        Assert.AreEqual(value, row.TestField, "Unexpected field value returned");

        List<string> rows = await context.TestTable1
            .Select(e => e.TestField)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        Assert.HasCount(1, rows, "Unexpected number of rows in the table");
        Assert.AreEqual(value, rows[0], "Unexpected row in the table");
    }

    /// <summary>
    /// Test ExecuteInsertGetRowAsync on an entity with a renamed column and a value converter.
    /// </summary>
    /// <param name="databaseType">Database type.</param>
    /// <returns>A <see cref="Task"/> that represents the asynchronous operation.</returns>
    /// <remarks>
    /// The inserted row must be read with the entity's mapping: <see cref="MappedEntity.Name"/> is in the <c>name_col</c> column and
    /// <see cref="MappedEntity.Day"/> is stored as a string. The entity's query filter is turned off, because an insert can't have one.
    /// </remarks>
    [TestMethod(DisplayName = "ExecuteInsertGetRowAsync on an entity with a renamed column and a value converter")]
    [DataRow(DatabaseTypes.Sqlite, DisplayName = DatabaseTypes.Sqlite)]
    [DataRow(DatabaseTypes.SqlServer, DisplayName = DatabaseTypes.SqlServer)]
    public async Task ExecuteInsertGetRowTests_010_Async(string databaseType)
    {
        // ARRANGE
        CancellationToken cancellationToken = this._testContext.CancellationToken;

        using Context context = await DatabaseUtils.OpenDatabaseAsync(
            this._testContext,
            databaseType,
            customConfigurationActions: optionsBuilder => optionsBuilder.UseExecuteUpdateExtensions(),
            customModelCreationActions: ExecuteUpdateTestUtils.AddMappedEntities)
            .ConfigureAwait(false);

        // ACT
        MappedEntity row = await context.Set<MappedEntity>()
            .IgnoreQueryFilters()
            .ExecuteInsertGetRowAsync(
                builder => builder
                    .SetProperty(e => e.Name, "Monday")
                    .SetProperty(e => e.Day, DayOfWeek.Monday)
                    .SetProperty(e => e.IsDeleted, false),
                cancellationToken)
            .ConfigureAwait(false);

        // ASSERT
        ExecuteUpdateTestUtils.AssertStateCleared(context);

        Assert.AreEqual("Monday", row.Name, "Unexpected name");
        Assert.AreEqual(DayOfWeek.Monday, row.Day, "Unexpected day");

        MappedEntity saved = await context.Set<MappedEntity>()
            .AsNoTracking()
            .SingleAsync(cancellationToken)
            .ConfigureAwait(false);

        Assert.AreEqual(row.Id, saved.Id, "Unexpected Id");
    }

    /// <summary>
    /// Test ExecuteInsertGetRowAsync on an entity with an owned type.
    /// </summary>
    /// <param name="databaseType">Database type.</param>
    /// <returns>A <see cref="Task"/> that represents the asynchronous operation.</returns>
    /// <remarks>EF Core can't read an owned type from the inserted row, so it must throw before anything is inserted.</remarks>
    [TestMethod(DisplayName = "ExecuteInsertGetRowAsync on an entity with an owned type")]
    [DataRow(DatabaseTypes.Sqlite, DisplayName = DatabaseTypes.Sqlite)]
    [DataRow(DatabaseTypes.SqlServer, DisplayName = DatabaseTypes.SqlServer)]
    public async Task ExecuteInsertGetRowTests_011_Async(string databaseType)
    {
        // ARRANGE
        CancellationToken cancellationToken = this._testContext.CancellationToken;

        using Context context = await DatabaseUtils.OpenDatabaseAsync(
            this._testContext,
            databaseType,
            customConfigurationActions: optionsBuilder => optionsBuilder.UseExecuteUpdateExtensions(),
            customModelCreationActions: modelBuilder => ExecuteUpdateTestUtils.AddCustomer(modelBuilder, owned: true))
            .ConfigureAwait(false);

        // ACT / ASSERT
        NotSupportedException exception = await Assert.ThrowsExactlyAsync<NotSupportedException>(
            () => context.Set<Customer>().ExecuteInsertGetRowAsync(builder => builder.SetProperty(e => e.Name, "Fred"), cancellationToken),
            "Unexpected exception")
            .ConfigureAwait(false);

        Assert.AreEqual("ExecuteInsertGetRowAsync does not support entity types with owned types (Customer.Address).", exception.Message, "Unexpected exception message");
        ExecuteUpdateTestUtils.AssertStateCleared(context);

        int rowCount = await context.Set<Customer>().CountAsync(cancellationToken).ConfigureAwait(false);
        Assert.AreEqual(0, rowCount, "Unexpected count");
    }

    /// <summary>
    /// Test ExecuteInsertGetRowAsync on an entity with a complex type.
    /// </summary>
    /// <param name="databaseType">Database type.</param>
    /// <returns>A <see cref="Task"/> that represents the asynchronous operation.</returns>
    /// <remarks>A complex type is stored in the entity's own columns, so it's read from the inserted row.</remarks>
    [TestMethod(DisplayName = "ExecuteInsertGetRowAsync on an entity with a complex type")]
    [DataRow(DatabaseTypes.Sqlite, DisplayName = DatabaseTypes.Sqlite)]
    [DataRow(DatabaseTypes.SqlServer, DisplayName = DatabaseTypes.SqlServer)]
    public async Task ExecuteInsertGetRowTests_012_Async(string databaseType)
    {
        // ARRANGE
        CancellationToken cancellationToken = this._testContext.CancellationToken;

        using Context context = await DatabaseUtils.OpenDatabaseAsync(
            this._testContext,
            databaseType,
            customConfigurationActions: optionsBuilder => optionsBuilder.UseExecuteUpdateExtensions(),
            customModelCreationActions: modelBuilder => ExecuteUpdateTestUtils.AddCustomer(modelBuilder, owned: false))
            .ConfigureAwait(false);

        // ACT
        Customer row = await context.Set<Customer>()
            .ExecuteInsertGetRowAsync(
                builder => builder
                    .SetProperty(e => e.Name, "Fred")
                    .SetProperty(e => e.Address.City, "Paris"),
                cancellationToken)
            .ConfigureAwait(false);

        // ASSERT
        ExecuteUpdateTestUtils.AssertStateCleared(context);

        Assert.AreEqual("Fred", row.Name, "Unexpected name");
        Assert.AreEqual("Paris", row.Address.City, "Unexpected city");

        int rowCount = await context.Set<Customer>().CountAsync(cancellationToken).ConfigureAwait(false);
        Assert.AreEqual(1, rowCount, "Unexpected count");
    }

    /// <summary>
    /// Test ExecuteInsertGetRowAsync on an entity type with a global query filter.
    /// </summary>
    /// <param name="databaseType">Database type.</param>
    /// <returns>A <see cref="Task"/> that represents the asynchronous operation.</returns>
    /// <remarks>EF Core adds a global query filter to the query as a Where, which an insert can't have, so the insert must ignore it.</remarks>
    [TestMethod(DisplayName = "ExecuteInsertGetRowAsync on an entity type with a global query filter")]
    [DataRow(DatabaseTypes.Sqlite, DisplayName = DatabaseTypes.Sqlite)]
    [DataRow(DatabaseTypes.SqlServer, DisplayName = DatabaseTypes.SqlServer)]
    public async Task ExecuteInsertGetRowTests_013_Async(string databaseType)
    {
        // ARRANGE
        CancellationToken cancellationToken = this._testContext.CancellationToken;

        using Context context = await DatabaseUtils.OpenDatabaseAsync(
            this._testContext,
            databaseType,
            customConfigurationActions: optionsBuilder => optionsBuilder.UseExecuteUpdateExtensions(),
            customModelCreationActions: modelBuilder => modelBuilder.Entity<TestTable1>().HasQueryFilter(e => e.TestField != "Hidden TestValue"))
            .ConfigureAwait(false);

        string insertedValue = "Inserted TestValue";

        // ACT
        TestTable1 row = await context.TestTable1
            .ExecuteInsertGetRowAsync(
                builder => builder.SetProperty(e => e.TestField, insertedValue),
                cancellationToken)
            .ConfigureAwait(false);

        // ASSERT
        ExecuteUpdateTestUtils.AssertStateCleared(context);

        Assert.AreEqual(insertedValue, row.TestField, "Unexpected field value");

        List<TestTable1> rows = await context.TestTable1
            .AsNoTracking()
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        Assert.HasCount(1, rows, "Unexpected count");
        Assert.AreEqual(row.Id, rows[0].Id, "The row returned isn't the row inserted");
    }

    #endregion ExecuteInsertGetRowAsync tests
}
