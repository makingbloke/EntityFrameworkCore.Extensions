// Copyright ©2021-2026 Mike King.
// This file is licensed to you under the MIT license.
// See the License.txt file in the solution root for more information.

using DotDoc.EntityFrameworkCore.Extensions.DatabaseType;
using DotDoc.EntityFrameworkCore.Extensions.ExecuteUpdate;
using DotDoc.EntityFrameworkCore.Extensions.Tests.Data;
using DotDoc.EntityFrameworkCore.Extensions.Tests.Model;
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
    [TestMethod]
    public async Task ExecuteDeleteGetRowsAsync_NullSource_ThrowsArgumentNullException()
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
    [TestMethod]
    [DataRow(DatabaseTypes.Sqlite, 0, DisplayName = $"Where {DatabaseTypes.Sqlite} 0 Records")]
    [DataRow(DatabaseTypes.Sqlite, 1, DisplayName = $"Where {DatabaseTypes.Sqlite} 1 Record")]
    [DataRow(DatabaseTypes.Sqlite, 10, DisplayName = $"Where {DatabaseTypes.Sqlite} 10 Records")]
    [DataRow(DatabaseTypes.SqlServer, 0, DisplayName = $"Where {DatabaseTypes.SqlServer} 0 Records")]
    [DataRow(DatabaseTypes.SqlServer, 1, DisplayName = $"Where {DatabaseTypes.SqlServer} 1 Record")]
    [DataRow(DatabaseTypes.SqlServer, 10, DisplayName = $"Where {DatabaseTypes.SqlServer} 10 Records")]
    public async Task ExecuteDeleteGetRowsAsync_Where_DeletesAndReturnsMatchingRows(string databaseType, int count)
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
    [TestMethod]
    [DataRow(DatabaseTypes.Sqlite, DisplayName = $"CallInProgress {DatabaseTypes.Sqlite}")]
    [DataRow(DatabaseTypes.SqlServer, DisplayName = $"CallInProgress {DatabaseTypes.SqlServer}")]
    public async Task ExecuteDeleteGetRowsAsync_CallInProgress_ThrowsInvalidOperationException(string databaseType)
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
    [TestMethod]
    [DataRow(DatabaseTypes.Sqlite, DisplayName = $"NoUseExecuteUpdateExtensions {DatabaseTypes.Sqlite}")]
    [DataRow(DatabaseTypes.SqlServer, DisplayName = $"NoUseExecuteUpdateExtensions {DatabaseTypes.SqlServer}")]
    public async Task ExecuteDeleteGetRowsAsync_NoUseExecuteUpdateExtensions_ThrowsInvalidOperationException(string databaseType)
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
    [TestMethod]
    [DataRow(DatabaseTypes.Sqlite, DisplayName = $"NoReturningClause {DatabaseTypes.Sqlite}")]
    [DataRow(DatabaseTypes.SqlServer, DisplayName = $"NoReturningClause {DatabaseTypes.SqlServer}")]
    public async Task ExecuteDeleteGetRowsAsync_NoReturningClause_ThrowsNotSupportedException(string databaseType)
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
    [TestMethod]
    [DataRow(DatabaseTypes.Sqlite, DisplayName = $"BracesInSql {DatabaseTypes.Sqlite}")]
    [DataRow(DatabaseTypes.SqlServer, DisplayName = $"BracesInSql {DatabaseTypes.SqlServer}")]
    public async Task ExecuteDeleteGetRowsAsync_BracesInSql_ReturnsDeletedRows(string databaseType)
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

    /// <summary>
    /// Test ExecuteDeleteGetRowsAsync on an entity with a renamed column, a value converter and a query filter.
    /// </summary>
    /// <param name="databaseType">Database type.</param>
    /// <returns>A <see cref="Task"/> that represents the asynchronous operation.</returns>
    /// <remarks>
    /// The deleted rows must be read with the entity's mapping: <see cref="MappedEntity.Name"/> is in the <c>name_col</c> column and
    /// <see cref="MappedEntity.Day"/> is stored as a string. The query filter must stop a soft deleted row being deleted.
    /// </remarks>
    [TestMethod]
    [DataRow(DatabaseTypes.Sqlite, DisplayName = $"RenamedColumnConverterAndFilter {DatabaseTypes.Sqlite}")]
    [DataRow(DatabaseTypes.SqlServer, DisplayName = $"RenamedColumnConverterAndFilter {DatabaseTypes.SqlServer}")]
    public async Task ExecuteDeleteGetRowsAsync_RenamedColumnConverterAndFilter_ReturnsDeletedRows(string databaseType)
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
            .ExecuteDeleteGetRowsAsync(cancellationToken)
            .ConfigureAwait(false);

        // ASSERT
        ExecuteUpdateTestUtils.AssertStateCleared(context);

        Assert.HasCount(1, rows, "Invalid count");
        Assert.AreEqual("Monday", rows[0].Name, "Unexpected name");
        Assert.AreEqual(DayOfWeek.Monday, rows[0].Day, "Unexpected day");
        Assert.IsFalse(rows[0].IsDeleted, "Unexpected soft deleted row");

        List<string> remaining = await context.Set<MappedEntity>()
            .IgnoreQueryFilters()
            .OrderBy(e => e.Name)
            .Select(e => e.Name)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        Assert.HasCount(2, remaining, "Unexpected number of rows left");
        Assert.AreEqual("Deleted", remaining[0], "The soft deleted row has been deleted");
        Assert.AreEqual("Friday", remaining[1], "Unexpected row left");
    }

    /// <summary>
    /// Test ExecuteDeleteGetRowsAsync on the base type of a table-per-hierarchy inheritance.
    /// </summary>
    /// <param name="databaseType">Database type.</param>
    /// <returns>A <see cref="Task"/> that represents the asynchronous operation.</returns>
    /// <remarks>Each deleted row must be returned as its own type, so a <see cref="Dog"/> keeps its <see cref="Dog.Breed"/>.</remarks>
    [TestMethod]
    [DataRow(DatabaseTypes.Sqlite, DisplayName = $"TphBaseType {DatabaseTypes.Sqlite}")]
    [DataRow(DatabaseTypes.SqlServer, DisplayName = $"TphBaseType {DatabaseTypes.SqlServer}")]
    public async Task ExecuteDeleteGetRowsAsync_TphBaseType_ReturnsRowsAsDerivedTypes(string databaseType)
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
            .ExecuteDeleteGetRowsAsync(cancellationToken)
            .ConfigureAwait(false);

        // ASSERT
        ExecuteUpdateTestUtils.AssertStateCleared(context);

        Assert.HasCount(2, rows, "Invalid count");

        Animal cat = rows.Single(r => r.Name == "Cat");
        Assert.AreEqual(typeof(Animal), cat.GetType(), "Unexpected type of the base type row");

        Dog? dog = rows.Single(r => r.Name == "Rex") as Dog;
        Assert.IsNotNull(dog, "The derived type row was not returned as a Dog");
        Assert.AreEqual("Collie", dog.Breed, "Unexpected breed");
    }

    /// <summary>
    /// Test ExecuteDeleteGetRowsAsync on a derived type of a table-per-hierarchy inheritance.
    /// </summary>
    /// <param name="databaseType">Database type.</param>
    /// <returns>A <see cref="Task"/> that represents the asynchronous operation.</returns>
    /// <remarks>Only the rows of the derived type must be deleted, and returned with the derived type's properties.</remarks>
    [TestMethod]
    [DataRow(DatabaseTypes.Sqlite, DisplayName = $"TphDerivedType {DatabaseTypes.Sqlite}")]
    [DataRow(DatabaseTypes.SqlServer, DisplayName = $"TphDerivedType {DatabaseTypes.SqlServer}")]
    public async Task ExecuteDeleteGetRowsAsync_TphDerivedType_ReturnsDeletedRows(string databaseType)
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
        IList<Dog> rows = await context.Set<Dog>()
            .ExecuteDeleteGetRowsAsync(cancellationToken)
            .ConfigureAwait(false);

        // ASSERT
        ExecuteUpdateTestUtils.AssertStateCleared(context);

        Assert.HasCount(1, rows, "Invalid count");
        Assert.AreEqual("Rex", rows[0].Name, "Unexpected name");
        Assert.AreEqual("Collie", rows[0].Breed, "Unexpected breed");

        List<string> remaining = await context.Set<Animal>()
            .Select(e => e.Name)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        Assert.HasCount(1, remaining, "Unexpected number of rows left");
        Assert.AreEqual("Cat", remaining[0], "Unexpected row left");
    }

    /// <summary>
    /// Test ExecuteDeleteGetRowsAsync on a shared type entity (a property bag).
    /// </summary>
    /// <param name="databaseType">Database type.</param>
    /// <returns>A <see cref="Task"/> that represents the asynchronous operation.</returns>
    /// <remarks>A shared type entity is only known by its name, which must be found from the query.</remarks>
    [TestMethod]
    [DataRow(DatabaseTypes.Sqlite, DisplayName = $"SharedTypeEntity {DatabaseTypes.Sqlite}")]
    [DataRow(DatabaseTypes.SqlServer, DisplayName = $"SharedTypeEntity {DatabaseTypes.SqlServer}")]
    public async Task ExecuteDeleteGetRowsAsync_SharedTypeEntity_ReturnsDeletedRows(string databaseType)
    {
        // ARRANGE
        const string propertyBagTableName = "PropertyBag";
        CancellationToken cancellationToken = this._testContext.CancellationToken;

        using Context context = await DatabaseUtils.OpenDatabaseAsync(
            this._testContext,
            databaseType,
            customConfigurationActions: optionsBuilder => optionsBuilder.UseExecuteUpdateExtensions(),
            customModelCreationActions: modelBuilder =>
            {
                modelBuilder.SharedTypeEntity<Dictionary<string, object>>(propertyBagTableName, builder =>
                {
                    builder.Property<long>("Id").ValueGeneratedNever();
                    builder.Property<string>("Name").HasMaxLength(256);
                    builder.HasKey("Id");
                });
            })
            .ConfigureAwait(false);

        DbSet<Dictionary<string, object>> propertyBags = context.Set<Dictionary<string, object>>(propertyBagTableName);

        propertyBags.AddRange(
            new Dictionary<string, object> { ["Id"] = 1L, ["Name"] = "Bag" },
            new Dictionary<string, object> { ["Id"] = 2L, ["Name"] = "Other" });

        await context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        context.ChangeTracker.Clear();

        // ACT
        IList<Dictionary<string, object>> rows = await propertyBags
            .Where(e => EF.Property<string>(e, "Name") == "Bag")
            .ExecuteDeleteGetRowsAsync(cancellationToken)
            .ConfigureAwait(false);

        // ASSERT
        ExecuteUpdateTestUtils.AssertStateCleared(context);

        Assert.HasCount(1, rows, "Invalid count");
        Assert.AreEqual("Bag", rows[0]["Name"], "Unexpected name");

        int remaining = await propertyBags.CountAsync(cancellationToken).ConfigureAwait(false);
        Assert.AreEqual(1, remaining, "Unexpected number of rows left");
    }

    /// <summary>
    /// Test ExecuteDeleteGetRowsAsync on an entity with an owned type.
    /// </summary>
    /// <param name="databaseType">Database type.</param>
    /// <returns>A <see cref="Task"/> that represents the asynchronous operation.</returns>
    /// <remarks>EF Core can't read an owned type from the deleted rows, so it must throw before anything is deleted.</remarks>
    [TestMethod]
    [DataRow(DatabaseTypes.Sqlite, DisplayName = $"OwnedType {DatabaseTypes.Sqlite}")]
    [DataRow(DatabaseTypes.SqlServer, DisplayName = $"OwnedType {DatabaseTypes.SqlServer}")]
    public async Task ExecuteDeleteGetRowsAsync_OwnedType_ThrowsNotSupportedException(string databaseType)
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
        NotSupportedException exception = await Assert.ThrowsExactlyAsync<NotSupportedException>(() => context.Set<Customer>().ExecuteDeleteGetRowsAsync(cancellationToken), "Unexpected exception").ConfigureAwait(false);

        Assert.AreEqual("ExecuteDeleteGetRowsAsync does not support entity types with owned types (Customer.Address).", exception.Message, "Unexpected exception message");
        ExecuteUpdateTestUtils.AssertStateCleared(context);

        int rowCount = await context.Set<Customer>().CountAsync(cancellationToken).ConfigureAwait(false);
        Assert.AreEqual(1, rowCount, "Unexpected count");
    }

    /// <summary>
    /// Test ExecuteDeleteGetRowsAsync on an entity type in an inheritance hierarchy mapped with TPT or TPC.
    /// </summary>
    /// <param name="databaseType">Database type.</param>
    /// <param name="tablePerConcreteType"><see langword="true"/> for TPC, <see langword="false"/> for TPT.</param>
    /// <returns>A <see cref="Task"/> that represents the asynchronous operation.</returns>
    /// <remarks>EF Core can't read the deleted rows of a hierarchy without a discriminator, so it must throw before anything is deleted.</remarks>
    [TestMethod]
    [DataRow(DatabaseTypes.Sqlite, false, DisplayName = $"TPT {DatabaseTypes.Sqlite}")]
    [DataRow(DatabaseTypes.Sqlite, true, DisplayName = $"TPC {DatabaseTypes.Sqlite}")]
    [DataRow(DatabaseTypes.SqlServer, false, DisplayName = $"TPT {DatabaseTypes.SqlServer}")]
    [DataRow(DatabaseTypes.SqlServer, true, DisplayName = $"TPC {DatabaseTypes.SqlServer}")]
    public async Task ExecuteDeleteGetRowsAsync_TptOrTpcInheritance_ThrowsNotSupportedException(string databaseType, bool tablePerConcreteType)
    {
        // ARRANGE
        CancellationToken cancellationToken = this._testContext.CancellationToken;

        using Context context = await DatabaseUtils.OpenDatabaseAsync(
            this._testContext,
            databaseType,
            customConfigurationActions: optionsBuilder => optionsBuilder.UseExecuteUpdateExtensions(),
            customModelCreationActions: modelBuilder => ExecuteUpdateTestUtils.AddAnimals(modelBuilder, tablePerConcreteType))
            .ConfigureAwait(false);

        context.Add(new Dog { Name = "Rex", Breed = "Collie" });
        await context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        context.ChangeTracker.Clear();

        // ACT / ASSERT
        NotSupportedException exception = await Assert.ThrowsExactlyAsync<NotSupportedException>(() => context.Set<Dog>().ExecuteDeleteGetRowsAsync(cancellationToken), "Unexpected exception").ConfigureAwait(false);

        Assert.AreEqual("ExecuteDeleteGetRowsAsync does not support entity types in an inheritance hierarchy mapped with TPT or TPC (Animal).", exception.Message, "Unexpected exception message");
        ExecuteUpdateTestUtils.AssertStateCleared(context);

        int rowCount = await context.Set<Dog>().CountAsync(cancellationToken).ConfigureAwait(false);
        Assert.AreEqual(1, rowCount, "Unexpected count");
    }

    /// <summary>
    /// Test ExecuteDeleteGetRowsAsync on an entity with a complex type.
    /// </summary>
    /// <param name="databaseType">Database type.</param>
    /// <returns>A <see cref="Task"/> that represents the asynchronous operation.</returns>
    /// <remarks>A complex type is stored in the entity's own columns, so it's read from the deleted rows.</remarks>
    [TestMethod]
    [DataRow(DatabaseTypes.Sqlite, DisplayName = $"ComplexType {DatabaseTypes.Sqlite}")]
    [DataRow(DatabaseTypes.SqlServer, DisplayName = $"ComplexType {DatabaseTypes.SqlServer}")]
    public async Task ExecuteDeleteGetRowsAsync_ComplexType_ReturnsDeletedRows(string databaseType)
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
            .ExecuteDeleteGetRowsAsync(cancellationToken)
            .ConfigureAwait(false);

        // ASSERT
        ExecuteUpdateTestUtils.AssertStateCleared(context);

        Assert.HasCount(1, rows, "Invalid count");
        Assert.AreEqual("Fred", rows[0].Name, "Unexpected name");
        Assert.AreEqual("Paris", rows[0].Address.City, "Unexpected city");

        int rowCount = await context.Set<Customer>().CountAsync(cancellationToken).ConfigureAwait(false);
        Assert.AreEqual(0, rowCount, "Unexpected count");
    }

    #endregion ExecuteDeleteGetRowsAsync tests
}
