// Copyright ©2021-2026 Mike King.
// This file is licensed to you under the MIT license.
// See the License.txt file in the solution root for more information.

using DotDoc.EntityFrameworkCore.Extensions.DatabaseType;
using DotDoc.EntityFrameworkCore.Extensions.ExecuteUpdate;
using DotDoc.EntityFrameworkCore.Extensions.Tests.Data;
using DotDoc.EntityFrameworkCore.Extensions.Tests.Model;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
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
    [TestMethod]
    public async Task ExecuteUpdateGetRowsAsync_NullSource_ThrowsArgumentNullException()
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
    [TestMethod]
    public async Task ExecuteUpdateGetRowsAsync_NullSetPropertyCalls_ThrowsArgumentNullException()
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
    /// <param name="count">Number of records to update.</param>
    /// <returns>A <see cref="Task"/> that represents the asynchronous operation.</returns>
    [TestMethod]
    [DataRow(DatabaseTypes.Sqlite, 0, DisplayName = $"Where {DatabaseTypes.Sqlite} 0 Records")]
    [DataRow(DatabaseTypes.Sqlite, 1, DisplayName = $"Where {DatabaseTypes.Sqlite} 1 Record")]
    [DataRow(DatabaseTypes.Sqlite, 10, DisplayName = $"Where {DatabaseTypes.Sqlite} 10 Records")]
    [DataRow(DatabaseTypes.SqlServer, 0, DisplayName = $"Where {DatabaseTypes.SqlServer} 0 Records")]
    [DataRow(DatabaseTypes.SqlServer, 1, DisplayName = $"Where {DatabaseTypes.SqlServer} 1 Record")]
    [DataRow(DatabaseTypes.SqlServer, 10, DisplayName = $"Where {DatabaseTypes.SqlServer} 10 Records")]
    public async Task ExecuteUpdateGetRowsAsync_Where_UpdatesAndReturnsMatchingRows(string databaseType, int count)
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

        string originalValue = "Original TestValue";
        string updatedValue = "Updated TestValue";

        await DatabaseUtils.CreateTestTableEntriesAsync(context, originalValue, (count + 1) * 10, this._testContext.CancellationToken).ConfigureAwait(false);

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
    [TestMethod]
    [DataRow(DatabaseTypes.Sqlite, DisplayName = $"CallInProgress {DatabaseTypes.Sqlite}")]
    [DataRow(DatabaseTypes.SqlServer, DisplayName = $"CallInProgress {DatabaseTypes.SqlServer}")]
    public async Task ExecuteUpdateGetRowsAsync_CallInProgress_ThrowsInvalidOperationException(string databaseType)
    {
        // ARRANGE
        using Context context = await DatabaseUtils.OpenDatabaseAsync(
            this._testContext,
            databaseType,
            customConfigurationActions: optionsBuilder => optionsBuilder.UseExecuteUpdateExtensions())
            .ConfigureAwait(false);

        string originalValue = "Original TestValue";

        await DatabaseUtils.CreateTestTableEntriesAsync(context, originalValue, 5, this._testContext.CancellationToken).ConfigureAwait(false);

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
    [TestMethod]
    [DataRow(DatabaseTypes.Sqlite, DisplayName = $"UpdateFails {DatabaseTypes.Sqlite}")]
    [DataRow(DatabaseTypes.SqlServer, DisplayName = $"UpdateFails {DatabaseTypes.SqlServer}")]
    public async Task ExecuteUpdateGetRowsAsync_UpdateFails_ThrowsDbException(string databaseType)
    {
        // ARRANGE
        using Context context = await DatabaseUtils.OpenDatabaseAsync(
            this._testContext,
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
    [TestMethod]
    [DataRow(DatabaseTypes.Sqlite, DisplayName = $"NoUseExecuteUpdateExtensions {DatabaseTypes.Sqlite}")]
    [DataRow(DatabaseTypes.SqlServer, DisplayName = $"NoUseExecuteUpdateExtensions {DatabaseTypes.SqlServer}")]
    public async Task ExecuteUpdateGetRowsAsync_NoUseExecuteUpdateExtensions_ThrowsInvalidOperationException(string databaseType)
    {
        // ARRANGE
        using Context context = await DatabaseUtils.OpenDatabaseAsync(this._testContext, databaseType).ConfigureAwait(false);

        string originalValue = "Original TestValue";

        await DatabaseUtils.CreateTestTableEntriesAsync(context, originalValue, 5, this._testContext.CancellationToken).ConfigureAwait(false);

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

    /// <summary>
    /// Test ExecuteUpdateGetRowsAsync without the RETURNING (SQLite) / OUTPUT (SQL Server) clause.
    /// </summary>
    /// <param name="databaseType">Database type.</param>
    /// <returns>A <see cref="Task"/> that represents the asynchronous operation.</returns>
    /// <remarks>The updated rows can only be returned with the clause, so it must throw before anything is updated.</remarks>
    [TestMethod]
    [DataRow(DatabaseTypes.Sqlite, DisplayName = $"NoReturningClause {DatabaseTypes.Sqlite}")]
    [DataRow(DatabaseTypes.SqlServer, DisplayName = $"NoReturningClause {DatabaseTypes.SqlServer}")]
    public async Task ExecuteUpdateGetRowsAsync_NoReturningClause_ThrowsNotSupportedException(string databaseType)
    {
        // ARRANGE
        using Context context = await DatabaseUtils.OpenDatabaseAsync(
            this._testContext,
            databaseType,
            customConfigurationActions: optionsBuilder => optionsBuilder.UseExecuteUpdateExtensions(),
            customModelCreationActions: modelBuilder => ExecuteUpdateTestUtils.TurnOffReturningClause(modelBuilder, databaseType))
            .ConfigureAwait(false);

        string originalValue = "Original TestValue";

        await DatabaseUtils.CreateTestTableEntriesAsync(context, originalValue, 5, this._testContext.CancellationToken).ConfigureAwait(false);

        Action<UpdateSettersBuilder<TestTable1>> setPropertyCalls = new(builder => builder.SetProperty(e => e.TestField, "Updated TestValue"));

        // ACT / ASSERT
        NotSupportedException exception = await Assert.ThrowsExactlyAsync<NotSupportedException>(() => context.TestTable1.ExecuteUpdateGetRowsAsync(setPropertyCalls, this._testContext.CancellationToken), "Unexpected exception").ConfigureAwait(false);

        ExecuteUpdateTestUtils.AssertReturningClauseNotUsedException(exception, databaseType, nameof(ExecuteUpdateExtensions.ExecuteUpdateGetRowsAsync));
        ExecuteUpdateTestUtils.AssertStateCleared(context);

        List<TestTable1> rows = await context.TestTable1
            .AsNoTracking()
            .ToListAsync(this._testContext.CancellationToken)
            .ConfigureAwait(false);

        Assert.IsTrue(rows.All(r => r.TestField == originalValue), "Rows have been updated");
    }

    /// <summary>
    /// Test ExecuteUpdateGetRowsAsync with braces in the SQL.
    /// </summary>
    /// <param name="databaseType">Database type.</param>
    /// <returns>A <see cref="Task"/> that represents the asynchronous operation.</returns>
    /// <remarks>
    /// The captured SQL is run again with SqlQueryRaw, which treats braces as placeholders, so they must be escaped. A string constant in a
    /// lambda and a tag are written into the SQL as they are. Unescaped, "}{" and the tag throw a FormatException, and "{0}" is replaced with
    /// the name of the first parameter, so the wrong rows are updated.
    /// </remarks>
    [TestMethod]
    [DataRow(DatabaseTypes.Sqlite, DisplayName = $"BracesInSql {DatabaseTypes.Sqlite}")]
    [DataRow(DatabaseTypes.SqlServer, DisplayName = $"BracesInSql {DatabaseTypes.SqlServer}")]
    public async Task ExecuteUpdateGetRowsAsync_BracesInSql_ReturnsUpdatedRows(string databaseType)
    {
        // ARRANGE
        CancellationToken cancellationToken = this._testContext.CancellationToken;

        using Context context = await DatabaseUtils.OpenDatabaseAsync(
            this._testContext,
            databaseType,
            customConfigurationActions: optionsBuilder => optionsBuilder.UseExecuteUpdateExtensions())
            .ConfigureAwait(false);

        TestTable1 first = new() { TestField = "{0} First" };
        TestTable1 second = new() { TestField = "{0} Second" };
        TestTable1 braces = new() { TestField = "}{" };
        TestTable1 other = new() { TestField = "Other" };

        context.TestTable1.AddRange(first, second, braces, other);
        await context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        context.ChangeTracker.Clear();

        long minId = 0;
        string updatedValue = "Updated TestValue";

        // ACT
        IList<TestTable1> placeholderRows = await context.TestTable1
            .Where(e => e.TestField.StartsWith("{0}") && e.Id > minId)
            .ExecuteUpdateGetRowsAsync(builder => builder.SetProperty(e => e.TestField, updatedValue), cancellationToken)
            .ConfigureAwait(false);

        IList<TestTable1> braceRows = await context.TestTable1
            .TagWith("Tag with braces { } {0} }{")
            .Where(e => e.TestField == "}{")
            .ExecuteUpdateGetRowsAsync(builder => builder.SetProperty(e => e.TestField, updatedValue), cancellationToken)
            .ConfigureAwait(false);

        // ASSERT
        ExecuteUpdateTestUtils.AssertStateCleared(context);

        List<long> placeholderIds = [.. placeholderRows.Select(r => r.Id).Order()];
        Assert.HasCount(2, placeholderIds, "Unexpected number of rows updated with {0}");
        Assert.AreEqual(first.Id, placeholderIds[0], "Unexpected first row updated with {0}");
        Assert.AreEqual(second.Id, placeholderIds[1], "Unexpected second row updated with {0}");

        Assert.HasCount(1, braceRows, "Unexpected number of rows updated with }{");
        Assert.AreEqual(braces.Id, braceRows[0].Id, "Unexpected row updated with }{");

        Assert.IsTrue(placeholderRows.Concat(braceRows).All(r => r.TestField == updatedValue), "Unexpected updated value returned");

        string otherValue = await context.TestTable1
            .Where(e => e.Id == other.Id)
            .Select(e => e.TestField)
            .SingleAsync(cancellationToken)
            .ConfigureAwait(false);

        Assert.AreEqual("Other", otherValue, "A row outside the filters has been updated");
    }

    /// <summary>
    /// Test ExecuteUpdateGetRowsAsync on an entity with a renamed column, a value converter and a query filter.
    /// </summary>
    /// <param name="databaseType">Database type.</param>
    /// <returns>A <see cref="Task"/> that represents the asynchronous operation.</returns>
    /// <remarks>
    /// The updated rows must be read with the entity's mapping: <see cref="MappedEntity.Name"/> is in the <c>name_col</c> column and
    /// <see cref="MappedEntity.Day"/> is stored as a string. The query filter must stop a soft deleted row being updated, but not hide the
    /// rows the update soft deletes.
    /// </remarks>
    [TestMethod]
    [DataRow(DatabaseTypes.Sqlite, DisplayName = $"RenamedColumnConverterAndFilter {DatabaseTypes.Sqlite}")]
    [DataRow(DatabaseTypes.SqlServer, DisplayName = $"RenamedColumnConverterAndFilter {DatabaseTypes.SqlServer}")]
    public async Task ExecuteUpdateGetRowsAsync_RenamedColumnConverterAndFilter_ReturnsUpdatedRows(string databaseType)
    {
        // ARRANGE
        CancellationToken cancellationToken = this._testContext.CancellationToken;

        using Context context = await DatabaseUtils.OpenDatabaseAsync(
            this._testContext,
            databaseType,
            customConfigurationActions: optionsBuilder => optionsBuilder.UseExecuteUpdateExtensions(),
            customModelCreationActions: ExecuteUpdateTestUtils.AddMappedEntities)
            .ConfigureAwait(false);

        context.AddRange(
            new MappedEntity { Name = "Monday", Day = DayOfWeek.Monday },
            new MappedEntity { Name = "Deleted", Day = DayOfWeek.Monday, IsDeleted = true },
            new MappedEntity { Name = "Friday", Day = DayOfWeek.Friday });

        await context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        context.ChangeTracker.Clear();

        // ACT
        IList<MappedEntity> rows = await context.Set<MappedEntity>()
            .Where(e => e.Day == DayOfWeek.Monday)
            .ExecuteUpdateGetRowsAsync(
                builder => builder
                    .SetProperty(e => e.Day, DayOfWeek.Sunday)
                    .SetProperty(e => e.IsDeleted, true),
                cancellationToken)
            .ConfigureAwait(false);

        // ASSERT
        ExecuteUpdateTestUtils.AssertStateCleared(context);

        Assert.HasCount(1, rows, "Invalid count");
        Assert.AreEqual("Monday", rows[0].Name, "Unexpected name");
        Assert.AreEqual(DayOfWeek.Sunday, rows[0].Day, "Unexpected day");
        Assert.IsTrue(rows[0].IsDeleted, "The row has not been soft deleted");

        List<MappedEntity> all = await context.Set<MappedEntity>()
            .IgnoreQueryFilters()
            .AsNoTracking()
            .OrderBy(e => e.Name)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        Assert.HasCount(3, all, "Unexpected number of rows");
        Assert.AreEqual(DayOfWeek.Monday, all[0].Day, "The soft deleted row has been updated");
        Assert.AreEqual(DayOfWeek.Friday, all[1].Day, "A row outside the filter has been updated");
        Assert.AreEqual(DayOfWeek.Sunday, all[2].Day, "The row has not been updated");
    }

    /// <summary>
    /// Test ExecuteUpdateGetRowsAsync on the base type of a table-per-hierarchy inheritance.
    /// </summary>
    /// <param name="databaseType">Database type.</param>
    /// <returns>A <see cref="Task"/> that represents the asynchronous operation.</returns>
    /// <remarks>Each updated row must be returned as its own type, so a <see cref="Dog"/> keeps its <see cref="Dog.Breed"/>.</remarks>
    [TestMethod]
    [DataRow(DatabaseTypes.Sqlite, DisplayName = $"TphBaseType {DatabaseTypes.Sqlite}")]
    [DataRow(DatabaseTypes.SqlServer, DisplayName = $"TphBaseType {DatabaseTypes.SqlServer}")]
    public async Task ExecuteUpdateGetRowsAsync_TphBaseType_ReturnsRowsAsDerivedTypes(string databaseType)
    {
        // ARRANGE
        CancellationToken cancellationToken = this._testContext.CancellationToken;

        using Context context = await DatabaseUtils.OpenDatabaseAsync(
            this._testContext,
            databaseType,
            customConfigurationActions: optionsBuilder => optionsBuilder.UseExecuteUpdateExtensions(),
            customModelCreationActions: ExecuteUpdateTestUtils.AddMappedEntities)
            .ConfigureAwait(false);

        context.AddRange(new Animal { Name = "Cat" }, new Dog { Name = "Rex", Breed = "Collie" });

        await context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        context.ChangeTracker.Clear();

        // ACT
        IList<Animal> rows = await context.Set<Animal>()
            .ExecuteUpdateGetRowsAsync(builder => builder.SetProperty(e => e.Name, e => e.Name + " Updated"), cancellationToken)
            .ConfigureAwait(false);

        // ASSERT
        ExecuteUpdateTestUtils.AssertStateCleared(context);

        Assert.HasCount(2, rows, "Invalid count");

        Animal cat = rows.Single(r => r.Name == "Cat Updated");
        Assert.AreEqual(typeof(Animal), cat.GetType(), "Unexpected type of the base type row");

        Dog? dog = rows.Single(r => r.Name == "Rex Updated") as Dog;
        Assert.IsNotNull(dog, "The derived type row was not returned as a Dog");
        Assert.AreEqual("Collie", dog.Breed, "Unexpected breed");
    }

    /// <summary>
    /// Test ExecuteUpdateGetRowsAsync on an entity with an owned type.
    /// </summary>
    /// <param name="databaseType">Database type.</param>
    /// <returns>A <see cref="Task"/> that represents the asynchronous operation.</returns>
    /// <remarks>EF Core can't read an owned type from the updated rows, so it must throw before anything is updated.</remarks>
    [TestMethod]
    [DataRow(DatabaseTypes.Sqlite, DisplayName = $"OwnedType {DatabaseTypes.Sqlite}")]
    [DataRow(DatabaseTypes.SqlServer, DisplayName = $"OwnedType {DatabaseTypes.SqlServer}")]
    public async Task ExecuteUpdateGetRowsAsync_OwnedType_ThrowsNotSupportedException(string databaseType)
    {
        // ARRANGE
        CancellationToken cancellationToken = this._testContext.CancellationToken;

        using Context context = await DatabaseUtils.OpenDatabaseAsync(
            this._testContext,
            databaseType,
            customConfigurationActions: optionsBuilder => optionsBuilder.UseExecuteUpdateExtensions(),
            customModelCreationActions: modelBuilder => ExecuteUpdateTestUtils.AddCustomer(modelBuilder, owned: true))
            .ConfigureAwait(false);

        context.Add(new Customer { Name = "Fred", Address = new Address { City = "Paris" } });
        await context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        context.ChangeTracker.Clear();

        // ACT / ASSERT
        NotSupportedException exception = await Assert.ThrowsExactlyAsync<NotSupportedException>(
            () => context.Set<Customer>().ExecuteUpdateGetRowsAsync(builder => builder.SetProperty(e => e.Name, "Bob"), cancellationToken),
            "Unexpected exception")
            .ConfigureAwait(false);

        Assert.AreEqual("ExecuteUpdateGetRowsAsync does not support entity types with owned types (Customer.Address).", exception.Message, "Unexpected exception message");
        ExecuteUpdateTestUtils.AssertStateCleared(context);

        string name = await context.Set<Customer>().Select(e => e.Name).SingleAsync(cancellationToken).ConfigureAwait(false);
        Assert.AreEqual("Fred", name, "The row has been updated");
    }

    /// <summary>
    /// Test ExecuteUpdateGetRowsAsync on an entity with a complex type.
    /// </summary>
    /// <param name="databaseType">Database type.</param>
    /// <returns>A <see cref="Task"/> that represents the asynchronous operation.</returns>
    /// <remarks>A complex type is stored in the entity's own columns, so it's read from the updated rows.</remarks>
    [TestMethod]
    [DataRow(DatabaseTypes.Sqlite, DisplayName = $"ComplexType {DatabaseTypes.Sqlite}")]
    [DataRow(DatabaseTypes.SqlServer, DisplayName = $"ComplexType {DatabaseTypes.SqlServer}")]
    public async Task ExecuteUpdateGetRowsAsync_ComplexType_ReturnsUpdatedRows(string databaseType)
    {
        // ARRANGE
        CancellationToken cancellationToken = this._testContext.CancellationToken;

        using Context context = await DatabaseUtils.OpenDatabaseAsync(
            this._testContext,
            databaseType,
            customConfigurationActions: optionsBuilder => optionsBuilder.UseExecuteUpdateExtensions(),
            customModelCreationActions: modelBuilder => ExecuteUpdateTestUtils.AddCustomer(modelBuilder, owned: false))
            .ConfigureAwait(false);

        context.Add(new Customer { Name = "Fred", Address = new Address { City = "Paris" } });
        await context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        context.ChangeTracker.Clear();

        // ACT
        IList<Customer> rows = await context.Set<Customer>()
            .ExecuteUpdateGetRowsAsync(builder => builder.SetProperty(e => e.Address.City, "London"), cancellationToken)
            .ConfigureAwait(false);

        // ASSERT
        ExecuteUpdateTestUtils.AssertStateCleared(context);

        Assert.HasCount(1, rows, "Invalid count");
        Assert.AreEqual("Fred", rows[0].Name, "Unexpected name");
        Assert.AreEqual("London", rows[0].Address.City, "Unexpected city");
    }

    #endregion ExecuteUpdateGetRowsAsync tests
}
