// Copyright ©2021-2026 Mike King.
// This file is licensed to you under the MIT license.
// See the License.txt file in the solution root for more information.

using DotDoc.EntityFrameworkCore.Extensions.DatabaseType;
using DotDoc.EntityFrameworkCore.Extensions.ExecuteUpdate;
using DotDoc.EntityFrameworkCore.Extensions.TableHints;
using DotDoc.EntityFrameworkCore.Extensions.Tests.CustomQueryGenerators;
using DotDoc.EntityFrameworkCore.Extensions.Tests.Data;
using DotDoc.EntityFrameworkCore.Extensions.Tests.Model;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;

namespace DotDoc.EntityFrameworkCore.Extensions.Tests.ExecuteUpdate;

/// <summary>
/// Tests for ExecuteInsert extensions.
/// </summary>
[TestClass]
public class ExecuteInsertTests
{
    #region private fields

    /// <summary>
    /// The MSTest <see cref="TestContext"/> for the current test.
    /// </summary>
    private readonly TestContext _testContext;

    #endregion private fields

    #region public constructors

    /// <summary>
    /// Initializes a new instance of the <see cref="ExecuteInsertTests"/> class.
    /// </summary>
    /// <param name="testContext">The MSTest test context.</param>
    public ExecuteInsertTests(TestContext testContext)
    {
        this._testContext = testContext;
    }

    #endregion public constructors

    #region ExecuteInsertAsync tests

    /// <summary>
    /// Test ExecuteInsertAsync with Null IQueryable source parameter.
    /// </summary>
    /// <returns>A <see cref="Task"/> that represents the asynchronous operation.</returns>
    [TestMethod]
    public async Task ExecuteInsertAsync_NullSource_ThrowsArgumentNullException()
    {
        // ARRANGE
        IQueryable<TestTable1> source = null!;
        Action<UpdateSettersBuilder<TestTable1>> setPropertyCalls = new(builder => { });

        // ACT / ASSERT
        await Assert.ThrowsExactlyAsync<ArgumentNullException>(() => source.ExecuteInsertAsync(setPropertyCalls, this._testContext.CancellationToken), "Unexpected exception").ConfigureAwait(false);
    }

    /// <summary>
    /// Test ExecuteInsertAsync with Null UpdateSettersBuilder setPropertyCalls parameter.
    /// </summary>
    /// <returns>A <see cref="Task"/> that represents the asynchronous operation.</returns>
    [TestMethod]
    public async Task ExecuteInsertAsync_NullSetPropertyCalls_ThrowsArgumentNullException()
    {
        // ARRANGE
        IQueryable<TestTable1> source = Array.Empty<TestTable1>().AsQueryable();
        Action<UpdateSettersBuilder<TestTable1>> setPropertyCalls = null!;

        // ACT / ASSERT
        await Assert.ThrowsExactlyAsync<ArgumentNullException>(() => source.ExecuteInsertAsync(setPropertyCalls, this._testContext.CancellationToken), "Unexpected exception").ConfigureAwait(false);
    }

    /// <summary>
    /// Test ExecuteInsertAsync.
    /// </summary>
    /// <param name="databaseType">Database type.</param>
    /// <returns>A <see cref="Task"/> that represents the asynchronous operation.</returns>
    [TestMethod]
    [DataRow(DatabaseTypes.Sqlite, DisplayName = $"Setters {DatabaseTypes.Sqlite}")]
    [DataRow(DatabaseTypes.SqlServer, DisplayName = $"Setters {DatabaseTypes.SqlServer}")]
    public async Task ExecuteInsertAsync_Setters_InsertsRow(string databaseType)
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
        await context.TestTable1.ExecuteInsertAsync(
            builder =>
            {
                builder.SetProperty(e => e.TestField, value);
            },
            this._testContext.CancellationToken)
            .ConfigureAwait(false);

        // ASSERT
        ExecuteUpdateTestUtils.AssertStateCleared(context);

        TestTable1? row = await context.TestTable1
            .Where(e => e.Id == id)
            .SingleOrDefaultAsync(this._testContext.CancellationToken)
            .ConfigureAwait(false);

        Assert.IsNotNull(row, "No row returned");
        Assert.AreEqual(id, row.Id, "Unexpected Id value");
        Assert.AreEqual(value, row.TestField, "Unexpected field value");
    }

    /// <summary>
    /// Test ExecuteInsertAsync without UseExecuteUpdateExtensions.
    /// </summary>
    /// <param name="databaseType">Database type.</param>
    /// <returns>A <see cref="Task"/> that represents the asynchronous operation.</returns>
    /// <remarks>Without the extensions the insert would run as an update of every row, so it must throw before anything is changed.</remarks>
    [TestMethod]
    [DataRow(DatabaseTypes.Sqlite, DisplayName = $"NoUseExecuteUpdateExtensions {DatabaseTypes.Sqlite}")]
    [DataRow(DatabaseTypes.SqlServer, DisplayName = $"NoUseExecuteUpdateExtensions {DatabaseTypes.SqlServer}")]
    public async Task ExecuteInsertAsync_NoUseExecuteUpdateExtensions_ThrowsInvalidOperationException(string databaseType)
    {
        // ARRANGE
        using Context context = await DatabaseUtils.OpenDatabaseAsync(this._testContext, databaseType).ConfigureAwait(false);

        string originalValue = "Original TestValue";
        string insertedValue = "Inserted TestValue";

        await DatabaseUtils.CreateTestTableEntriesAsync(context, originalValue, 5, this._testContext.CancellationToken).ConfigureAwait(false);

        Action<UpdateSettersBuilder<TestTable1>> setPropertyCalls = new(builder => builder.SetProperty(e => e.TestField, insertedValue));

        // ACT / ASSERT
        InvalidOperationException exception = await Assert.ThrowsExactlyAsync<InvalidOperationException>(() => context.TestTable1.ExecuteInsertAsync(setPropertyCalls, this._testContext.CancellationToken), "Unexpected exception").ConfigureAwait(false);

        Assert.Contains(nameof(ExecuteUpdateExtensions.UseExecuteUpdateExtensions), exception.Message, "Unexpected exception message");
        Assert.DoesNotContain("another extension library", exception.Message, "Unexpected exception message");

        List<TestTable1> rows = await context.TestTable1
            .AsNoTracking()
            .ToListAsync(this._testContext.CancellationToken)
            .ConfigureAwait(false);

        Assert.HasCount(5, rows, "Unexpected count");
        Assert.IsTrue(rows.All(r => r.TestField == originalValue), "Existing rows have been changed");
    }

    /// <summary>
    /// Test ExecuteInsertAsync with a source that has a Where clause.
    /// </summary>
    /// <param name="databaseType">Database type.</param>
    /// <returns>A <see cref="Task"/> that represents the asynchronous operation.</returns>
    /// <remarks>An insert can't have a Where clause, so it must be rejected rather than run as an update.</remarks>
    [TestMethod]
    [DataRow(DatabaseTypes.Sqlite, DisplayName = $"SourceWithWhere {DatabaseTypes.Sqlite}")]
    [DataRow(DatabaseTypes.SqlServer, DisplayName = $"SourceWithWhere {DatabaseTypes.SqlServer}")]
    public async Task ExecuteInsertAsync_SourceWithWhere_ThrowsInvalidOperationException(string databaseType)
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
        InvalidOperationException exception = await Assert.ThrowsExactlyAsync<InvalidOperationException>(
            () => source.ExecuteInsertAsync(
                builder =>
                {
                    builder.SetProperty(e => e.TestField, insertedValue);
                },
                this._testContext.CancellationToken),
            "Unexpected exception")
            .ConfigureAwait(false);

        Assert.Contains($"{nameof(ExecuteUpdateExtensions.ExecuteInsertAsync)} inserts a row", exception.Message, "Unexpected exception message");
        Assert.Contains("must be called on the table itself", exception.Message, "Unexpected exception message");
        Assert.DoesNotContain("bug in your EF Core provider", exception.Message, "Unexpected exception message");

        List<TestTable1> rows = await context.TestTable1
            .AsNoTracking()
            .ToListAsync(this._testContext.CancellationToken)
            .ConfigureAwait(false);

        Assert.HasCount(1, rows, "Unexpected count");
        Assert.AreEqual(originalValue, rows[0].TestField, "Unexpected field value");
    }

    /// <summary>
    /// Test ExecuteInsertAsync when another extension library has replaced the query generator.
    /// </summary>
    /// <param name="databaseType">Database type.</param>
    /// <returns>A <see cref="Task"/> that represents the asynchronous operation.</returns>
    /// <remarks>
    /// UseExecuteUpdateExtensions has been called, but the query generator it installs has been replaced, so without the check the insert
    /// would run as an update of every row. It must throw before anything is changed, and say the generator has been replaced.
    /// </remarks>
    [TestMethod]
    [DataRow(DatabaseTypes.Sqlite, DisplayName = $"QueryGeneratorReplaced {DatabaseTypes.Sqlite}")]
    [DataRow(DatabaseTypes.SqlServer, DisplayName = $"QueryGeneratorReplaced {DatabaseTypes.SqlServer}")]
    public async Task ExecuteInsertAsync_QueryGeneratorReplaced_ThrowsInvalidOperationException(string databaseType)
    {
        // ARRANGE
        using Context context = await DatabaseUtils.OpenDatabaseAsync(
            this._testContext,
            databaseType: databaseType,
            customConfigurationActions: (optionsBuilder) =>
            {
                optionsBuilder.UseExecuteUpdateExtensions();

                if (databaseType == DatabaseTypes.Sqlite)
                {
                    optionsBuilder.ReplaceService<IQuerySqlGeneratorFactory, ConflictingSqliteQuerySqlGeneratorFactory>();
                }
                else
                {
                    optionsBuilder.ReplaceService<IQuerySqlGeneratorFactory, ConflictingSqlServerQuerySqlGeneratorFactory>();
                }
            })
            .ConfigureAwait(false);

        string originalValue = "Original TestValue";
        string insertedValue = "Inserted TestValue";

        await DatabaseUtils.CreateTestTableEntriesAsync(context, originalValue, 5, this._testContext.CancellationToken).ConfigureAwait(false);

        Action<UpdateSettersBuilder<TestTable1>> setPropertyCalls = new(builder => builder.SetProperty(e => e.TestField, insertedValue));

        // ACT / ASSERT
        InvalidOperationException exception = await Assert.ThrowsExactlyAsync<InvalidOperationException>(() => context.TestTable1.ExecuteInsertAsync(setPropertyCalls, this._testContext.CancellationToken), "Unexpected exception").ConfigureAwait(false);

        Assert.Contains("another extension library", exception.Message, "Unexpected exception message");

        List<TestTable1> rows = await context.TestTable1
            .AsNoTracking()
            .ToListAsync(this._testContext.CancellationToken)
            .ConfigureAwait(false);

        Assert.HasCount(5, rows, "Unexpected count");
        Assert.IsTrue(rows.All(r => r.TestField == originalValue), "Existing rows have been changed");
    }

    /// <summary>
    /// Test ExecuteInsertAsync on SQL Server with only UseTableHintExtensions.
    /// </summary>
    /// <returns>A <see cref="Task"/> that represents the asynchronous operation.</returns>
    /// <remarks>
    /// UseTableHintExtensions installs the custom query generator but not the ExecuteUpdate state, so it must still throw and say UseExecuteUpdateExtensions must be called.
    /// No database connection is needed, as nothing is run.
    /// </remarks>
    [TestMethod]
    public async Task ExecuteInsertAsync_SqlServerOnlyUseTableHintExtensions_ThrowsInvalidOperationException()
    {
        // ARRANGE
        using Context context = new(
            DatabaseTypes.SqlServer,
            "Server=localhost;Initial Catalog=Dummy",
            optionsBuilder => optionsBuilder.UseTableHintExtensions());

        Action<UpdateSettersBuilder<TestTable1>> setPropertyCalls = new(builder => builder.SetProperty(e => e.TestField, "Inserted TestValue"));

        // ACT / ASSERT
        InvalidOperationException exception = await Assert.ThrowsExactlyAsync<InvalidOperationException>(() => context.TestTable1.ExecuteInsertAsync(setPropertyCalls, this._testContext.CancellationToken), "Unexpected exception").ConfigureAwait(false);

        Assert.Contains(nameof(ExecuteUpdateExtensions.UseExecuteUpdateExtensions), exception.Message, "Unexpected exception message");
        Assert.DoesNotContain("another extension library", exception.Message, "Unexpected exception message");
    }

    /// <summary>
    /// Test ExecuteInsertAsync with a source that has a Take.
    /// </summary>
    /// <param name="databaseType">Database type.</param>
    /// <returns>A <see cref="Task"/> that represents the asynchronous operation.</returns>
    /// <remarks>An insert of a single row can't have a Take, so it must be rejected on both databases (SQL Server used to run it as INSERT TOP (n)).</remarks>
    [TestMethod]
    [DataRow(DatabaseTypes.Sqlite, DisplayName = $"SourceWithTake {DatabaseTypes.Sqlite}")]
    [DataRow(DatabaseTypes.SqlServer, DisplayName = $"SourceWithTake {DatabaseTypes.SqlServer}")]
    public async Task ExecuteInsertAsync_SourceWithTake_ThrowsInvalidOperationException(string databaseType)
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
        string insertedValue = "Inserted TestValue";

        await DatabaseUtils.CreateTestTableEntriesAsync(context, originalValue, 1, this._testContext.CancellationToken).ConfigureAwait(false);

        IQueryable<TestTable1> source = context.TestTable1.Take(1);

        // ACT / ASSERT
        InvalidOperationException exception = await Assert.ThrowsExactlyAsync<InvalidOperationException>(
            () => source.ExecuteInsertAsync(
                builder =>
                {
                    builder.SetProperty(e => e.TestField, insertedValue);
                },
                this._testContext.CancellationToken),
            "Unexpected exception")
            .ConfigureAwait(false);

        Assert.Contains("must be called on the table itself", exception.Message, "Unexpected exception message");
        Assert.DoesNotContain("bug in your EF Core provider", exception.Message, "Unexpected exception message");

        List<TestTable1> rows = await context.TestTable1
            .AsNoTracking()
            .ToListAsync(this._testContext.CancellationToken)
            .ConfigureAwait(false);

        Assert.HasCount(1, rows, "Unexpected count");
        Assert.AreEqual(originalValue, rows[0].TestField, "Unexpected field value");
    }

    /// <summary>
    /// Test ExecuteInsertAsync on an entity type with a global query filter.
    /// </summary>
    /// <param name="databaseType">Database type.</param>
    /// <returns>A <see cref="Task"/> that represents the asynchronous operation.</returns>
    /// <remarks>EF Core adds a global query filter to the query as a Where, which an insert can't have, so the insert must ignore it.</remarks>
    [TestMethod]
    [DataRow(DatabaseTypes.Sqlite, DisplayName = $"GlobalQueryFilter {DatabaseTypes.Sqlite}")]
    [DataRow(DatabaseTypes.SqlServer, DisplayName = $"GlobalQueryFilter {DatabaseTypes.SqlServer}")]
    public async Task ExecuteInsertAsync_GlobalQueryFilter_InsertsRow(string databaseType)
    {
        // ARRANGE
        using Context context = await DatabaseUtils.OpenDatabaseAsync(
            this._testContext,
            databaseType: databaseType,
            customConfigurationActions: optionsBuilder => optionsBuilder.UseExecuteUpdateExtensions(),
            customModelCreationActions: modelBuilder => modelBuilder.Entity<TestTable1>().HasQueryFilter(e => e.TestField != "Hidden TestValue"))
            .ConfigureAwait(false);

        string insertedValue = "Inserted TestValue";

        // ACT
        await context.TestTable1.ExecuteInsertAsync(
            builder =>
            {
                builder.SetProperty(e => e.TestField, insertedValue);
            },
            this._testContext.CancellationToken)
            .ConfigureAwait(false);

        // ASSERT
        ExecuteUpdateTestUtils.AssertStateCleared(context);

        List<TestTable1> rows = await context.TestTable1
            .AsNoTracking()
            .ToListAsync(this._testContext.CancellationToken)
            .ConfigureAwait(false);

        Assert.HasCount(1, rows, "Unexpected count");
        Assert.AreEqual(insertedValue, rows[0].TestField, "Unexpected field value");
    }

    /// <summary>
    /// Test ExecuteInsertAsync on the root type of a table-per-hierarchy inheritance hierarchy.
    /// </summary>
    /// <param name="databaseType">Database type.</param>
    /// <returns>A <see cref="Task"/> that represents the asynchronous operation.</returns>
    /// <remarks>The discriminator (a shadow property) must be written, as SaveChanges does, so the row reads back as the root type.</remarks>
    [TestMethod]
    [DataRow(DatabaseTypes.Sqlite, DisplayName = $"TphRootType {DatabaseTypes.Sqlite}")]
    [DataRow(DatabaseTypes.SqlServer, DisplayName = $"TphRootType {DatabaseTypes.SqlServer}")]
    public async Task ExecuteInsertAsync_TphRootType_InsertsRow(string databaseType)
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
        await context.Set<Animal>()
            .ExecuteInsertAsync(builder => builder.SetProperty(e => e.Name, "Cat"), cancellationToken)
            .ConfigureAwait(false);

        // ASSERT
        ExecuteUpdateTestUtils.AssertStateCleared(context);

        List<Animal> rows = await context.Set<Animal>().AsNoTracking().ToListAsync(cancellationToken).ConfigureAwait(false);

        Assert.HasCount(1, rows, "Unexpected count");
        Assert.AreEqual(typeof(Animal), rows[0].GetType(), "Unexpected type");
        Assert.AreEqual("Cat", rows[0].Name, "Unexpected name");
    }

    /// <summary>
    /// Test ExecuteInsertAsync on a derived type of a table-per-hierarchy inheritance hierarchy.
    /// </summary>
    /// <param name="databaseType">Database type.</param>
    /// <returns>A <see cref="Task"/> that represents the asynchronous operation.</returns>
    /// <remarks>
    /// EF Core adds a filter on the discriminator to a query on a derived type, which the insert must allow, and the discriminator must be
    /// written so the row reads back as the derived type.
    /// </remarks>
    [TestMethod]
    [DataRow(DatabaseTypes.Sqlite, DisplayName = $"TphDerivedType {DatabaseTypes.Sqlite}")]
    [DataRow(DatabaseTypes.SqlServer, DisplayName = $"TphDerivedType {DatabaseTypes.SqlServer}")]
    public async Task ExecuteInsertAsync_TphDerivedType_InsertsRow(string databaseType)
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
        await context.Set<Dog>()
            .ExecuteInsertAsync(
                builder => builder
                    .SetProperty(e => e.Name, "Rex")
                    .SetProperty(e => e.Breed, "Collie"),
                cancellationToken)
            .ConfigureAwait(false);

        // ASSERT
        ExecuteUpdateTestUtils.AssertStateCleared(context);

        List<Animal> rows = await context.Set<Animal>().AsNoTracking().ToListAsync(cancellationToken).ConfigureAwait(false);

        Assert.HasCount(1, rows, "Unexpected count");
        Dog? dog = rows[0] as Dog;
        Assert.IsNotNull(dog, "The row was not read back as a Dog");
        Assert.AreEqual("Rex", dog.Name, "Unexpected name");
        Assert.AreEqual("Collie", dog.Breed, "Unexpected breed");
    }

    /// <summary>
    /// Test ExecuteInsertAsync on a table-per-hierarchy inheritance hierarchy when the setters set the discriminator.
    /// </summary>
    /// <param name="databaseType">Database type.</param>
    /// <returns>A <see cref="Task"/> that represents the asynchronous operation.</returns>
    /// <remarks>The caller's value must be used, and the discriminator not written a second time.</remarks>
    [TestMethod]
    [DataRow(DatabaseTypes.Sqlite, DisplayName = $"TphDiscriminatorSet {DatabaseTypes.Sqlite}")]
    [DataRow(DatabaseTypes.SqlServer, DisplayName = $"TphDiscriminatorSet {DatabaseTypes.SqlServer}")]
    public async Task ExecuteInsertAsync_TphDiscriminatorSet_InsertsRowWithSetValue(string databaseType)
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
        await context.Set<Animal>()
            .ExecuteInsertAsync(
                builder => builder
                    .SetProperty(e => e.Name, "Rex")
                    .SetProperty(e => EF.Property<string>(e, "Discriminator"), nameof(Dog)),
                cancellationToken)
            .ConfigureAwait(false);

        // ASSERT
        ExecuteUpdateTestUtils.AssertStateCleared(context);

        List<Animal> rows = await context.Set<Animal>().AsNoTracking().ToListAsync(cancellationToken).ConfigureAwait(false);

        Assert.HasCount(1, rows, "Unexpected count");
        Assert.AreEqual(typeof(Dog), rows[0].GetType(), "The row was not read back as a Dog");
    }

    /// <summary>
    /// Test ExecuteInsertAsync on a derived type of a table-per-hierarchy inheritance hierarchy with a Where.
    /// </summary>
    /// <param name="databaseType">Database type.</param>
    /// <returns>A <see cref="Task"/> that represents the asynchronous operation.</returns>
    /// <remarks>Only EF Core's own filter on the discriminator is allowed, so the caller's Where must still be rejected.</remarks>
    [TestMethod]
    [DataRow(DatabaseTypes.Sqlite, DisplayName = $"TphDerivedTypeWithWhere {DatabaseTypes.Sqlite}")]
    [DataRow(DatabaseTypes.SqlServer, DisplayName = $"TphDerivedTypeWithWhere {DatabaseTypes.SqlServer}")]
    public async Task ExecuteInsertAsync_TphDerivedTypeWithWhere_ThrowsInvalidOperationException(string databaseType)
    {
        // ARRANGE
        CancellationToken cancellationToken = this._testContext.CancellationToken;

        using Context context = await DatabaseUtils.OpenDatabaseAsync(
            this._testContext,
            databaseType,
            customConfigurationActions: optionsBuilder => optionsBuilder.UseExecuteUpdateExtensions(),
            customModelCreationActions: ExecuteUpdateTestUtils.AddMappedEntities)
            .ConfigureAwait(false);

        // ACT / ASSERT
        InvalidOperationException exception = await Assert.ThrowsExactlyAsync<InvalidOperationException>(
            () => context.Set<Dog>()
                .Where(e => e.Name == "Rex")
                .ExecuteInsertAsync(builder => builder.SetProperty(e => e.Name, "Rex"), cancellationToken),
            "Unexpected exception")
            .ConfigureAwait(false);

        Assert.Contains("must be called on the table itself", exception.Message, "Unexpected exception message");

        int rowCount = await context.Set<Animal>().CountAsync(cancellationToken).ConfigureAwait(false);
        Assert.AreEqual(0, rowCount, "Unexpected count");
    }

    /// <summary>
    /// Test ExecuteInsertAsync on an abstract entity type.
    /// </summary>
    /// <param name="databaseType">Database type.</param>
    /// <returns>A <see cref="Task"/> that represents the asynchronous operation.</returns>
    /// <remarks>EF Core can't create an instance of an abstract type, so a row of one couldn't be read back. It must throw before anything is inserted.</remarks>
    [TestMethod]
    [DataRow(DatabaseTypes.Sqlite, DisplayName = $"AbstractType {DatabaseTypes.Sqlite}")]
    [DataRow(DatabaseTypes.SqlServer, DisplayName = $"AbstractType {DatabaseTypes.SqlServer}")]
    public async Task ExecuteInsertAsync_AbstractType_ThrowsInvalidOperationException(string databaseType)
    {
        // ARRANGE
        CancellationToken cancellationToken = this._testContext.CancellationToken;

        using Context context = await DatabaseUtils.OpenDatabaseAsync(
            this._testContext,
            databaseType,
            customConfigurationActions: optionsBuilder => optionsBuilder.UseExecuteUpdateExtensions(),
            customModelCreationActions: ExecuteUpdateTestUtils.AddVehicles)
            .ConfigureAwait(false);

        // ACT / ASSERT
        InvalidOperationException exception = await Assert.ThrowsExactlyAsync<InvalidOperationException>(
            () => context.Set<Vehicle>().ExecuteInsertAsync(builder => builder.SetProperty(e => e.Name, "Van"), cancellationToken),
            "Unexpected exception")
            .ConfigureAwait(false);

        Assert.AreEqual("ExecuteInsertAsync can't insert a row of Vehicle, as it's an abstract type.", exception.Message, "Unexpected exception message");

        int rowCount = await context.Set<Vehicle>().CountAsync(cancellationToken).ConfigureAwait(false);
        Assert.AreEqual(0, rowCount, "Unexpected count");
    }

    #endregion ExecuteInsertAsync tests
}
