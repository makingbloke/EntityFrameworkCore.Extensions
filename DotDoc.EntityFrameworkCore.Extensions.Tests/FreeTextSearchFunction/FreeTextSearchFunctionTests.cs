// Copyright ©2021-2026 Mike King.
// This file is licensed to you under the MIT license.
// See the License.txt file in the solution root for more information.

using DotDoc.EntityFrameworkCore.Extensions.DatabaseType;
using DotDoc.EntityFrameworkCore.Extensions.FreeTextSearchFunction;
using DotDoc.EntityFrameworkCore.Extensions.Tests.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DotDoc.EntityFrameworkCore.Extensions.Tests.FreeTextSearchFunction;

/// <summary>
/// Tests for the FreeTextSearchFunction extension.
/// </summary>
[TestClass]
public class FreeTextSearchFunctionTests
{
    #region private fields

    /// <summary>
    /// The MSTest <see cref="TestContext"/> for the current test.
    /// </summary>
    private readonly TestContext _testContext;

    #endregion private fields

    #region public constructors

    /// <summary>
    /// Initializes a new instance of the <see cref="FreeTextSearchFunctionTests"/> class.
    /// </summary>
    /// <param name="testContext">The MSTest test context.</param>
    public FreeTextSearchFunctionTests(TestContext testContext)
    {
        this._testContext = testContext;
    }

    #endregion public constructors

    #region UseFreeTextSearchExtensions tests

    /// <summary>
    /// Test UseFreeTextSearchExtensions with a Null DbContextOptionsBuilder OptionsBuilder parameter.
    /// </summary>
    [TestMethod(DisplayName = "UseFreeTextSearchExtensions with a Null DbContextOptionsBuilder OptionsBuilder parameter")]
    public void FreeTextSearchFunctionTests_001()
    {
        // ARRANGE
        DbContextOptionsBuilder optionsBuilder = null!;

        // ACT / ASSERT
        Assert.ThrowsExactly<ArgumentNullException>(() => optionsBuilder.UseFreeTextSearchExtensions(), "Unexpected exception");
    }

    #endregion UseFreeTextSearchExtensions tests

    #region SetStemmingTable tests

    /// <summary>
    /// Test SetStemmingTable with a Null EntityTypeBuilder EntityBuilder parameter.
    /// </summary>
    [TestMethod(DisplayName = "SetStemmingTable with a Null EntityTypeBuilder EntityBuilder parameter")]
    public void FreeTextSearchFunctionTests_002()
    {
        // ARRANGE
        EntityTypeBuilder<FreeText>? entityBuilder = null;
        string tableName = "dummy";

        // ACT / ASSERT
        ArgumentNullException e = Assert.ThrowsExactly<ArgumentNullException>(() => entityBuilder!.SetStemmingTable(tableName), "Unexpected exception");
        Assert.AreEqual(nameof(entityBuilder), e.ParamName, "Invalid parameter name");
    }

    /// <summary>
    /// Test SetStemmingTable with a Null or Empty String TableName parameter.
    /// </summary>
    /// <param name="tableName">The stemming table name.</param>
    /// <param name="exceptionType">The type of exception raised.</param>
    /// <returns>A <see cref="Task"/> that represents the asynchronous operation.</returns>
    [TestMethod(DisplayName = "SetStemmingTable with a Null or Empty String TableName parameter")]
    [DataRow(null, typeof(ArgumentNullException), DisplayName = "Null")]
    [DataRow("", typeof(ArgumentException), DisplayName = "Empty")]
    public async Task FreeTextSearchFunctionTests_003_Async(string tableName, Type exceptionType)
    {
        // ARRANGE / ACT / ASSERT
        Exception e = await Assert.ThrowsAsync<Exception>(
            async () =>
            {
                using Context context = await DatabaseUtils.OpenDatabaseAsync(
                    this._testContext,
                    databaseType: DatabaseTypes.Sqlite,
                    customModelCreationActions: modelBuilder =>
                    {
                        modelBuilder.SharedTypeEntity<FreeText>(Context.TestFreeTextTableName)
                            .SetStemmingTable(tableName);
                    })
                    .ConfigureAwait(false);
            },
            "Unexpected exception")
            .ConfigureAwait(false);

        Assert.IsInstanceOfType(e, exceptionType, "Invalid exception type");
    }

    #endregion SetStemmingTable tests

    #region GetStemmingTable tests

    /// <summary>
    /// Test GetStemmingTable with a Null IEntityType EntityType parameter.
    /// </summary>
    [TestMethod(DisplayName = "GetStemmingTable with a Null IEntityType EntityType parameter")]
    public void FreeTextSearchFunctionTests_004()
    {
        // ARRANGE
        IEntityType? entityType = null;

        // ACT / ASSERT
        Assert.ThrowsExactly<ArgumentNullException>(() => entityType!.GetStemmingTable(), "Unexpected exception");
    }

    /// <summary>
    /// Test SetStemmingTable and GetStemmingTable.
    /// </summary>
    /// <returns>A <see cref="Task"/> that represents the asynchronous operation.</returns>
    [TestMethod(DisplayName = "SetStemmingTable and GetStemmingTable")]
    public async Task FreeTextSearchFunctionTests_005_Async()
    {
        // ARRANGE / ACT (SetStemming is called in CreateDatabase).
        using Context context = await DatabaseUtils.OpenDatabaseAsync(this._testContext, DatabaseTypes.Sqlite).ConfigureAwait(false);

        IEntityType entityType = context.Model.FindEntityType(Context.TestFreeTextTableName)!;
        string? stemmingTableName = entityType.GetStemmingTable();

        // ASSERT
        Assert.AreEqual(Context.TestFreeTextStemmingTableName, stemmingTableName, "Invalid stemming table name");
    }

    #endregion GetStemmingTable tests

    #region FreeTextSearch tests

    /// <summary>
    /// Test FreeTextSearch where a stemming table name has not been specified.
    /// </summary>
    /// <returns>A <see cref="Task"/> that represents the asynchronous operation.</returns>
    [TestMethod(DisplayName = "FreeTextSearch function where a stemming table name has not been specified")]
    public async Task FreeTextSearchFunctionTests_006_Async()
    {
        // ARRANGE
        // Create a new freetext table with no stemming table specified.
        string tableName = $"{Context.TestFreeTextTableName}2";

        using Context context = await DatabaseUtils.OpenDatabaseAsync(
            this._testContext,
            databaseType: DatabaseTypes.Sqlite,
            customModelCreationActions: modelBuilder =>
            {
                modelBuilder.SharedTypeEntity<FreeText>(tableName)
                    .Property<long>(nameof(FreeText.Id))
                    .HasColumnName("ROWID");
            })
            .ConfigureAwait(false);

        string searchValue = "Apple";

        // ACT / ASSERT
        await Assert.ThrowsExactlyAsync<InvalidOperationException>(
            async () =>
            {
                await context.Set<FreeText>(tableName)
                    .Where(e => EF.Functions.FreeTextSearch(e.FreeTextField!, searchValue, true))
                    .ToListAsync(this._testContext.CancellationToken)
                    .ConfigureAwait(false);
            },
            "Unexpected exception");
    }

    /// <summary>
    /// Test FreeTextSearch where the stemming table does not exist.
    /// </summary>
    /// <returns>A <see cref="Task"/> that represents the asynchronous operation.</returns>
    [TestMethod(DisplayName = "FreeTextSearch function where the stemming table does not exist")]
    public async Task FreeTextSearchFunctionTests_007_Async()
    {
        // ARRANGE
        // Assign a non existent stemming table name to the free text table.
        string stemmingTableName = "NonExistantTableName";

        using Context context = await DatabaseUtils.OpenDatabaseAsync(
            this._testContext,
            databaseType: DatabaseTypes.Sqlite,
            customModelCreationActions: modelBuilder =>
            {
                modelBuilder.SharedTypeEntity<FreeText>(Context.TestFreeTextTableName)
                    .SetStemmingTable(stemmingTableName)
                    .Property<long>(nameof(FreeText.Id))
                    .HasColumnName("ROWID");
            })
            .ConfigureAwait(false);

        string searchValue = "Apple";

        // ACT / ASSERT
        await Assert.ThrowsExactlyAsync<InvalidOperationException>(
            async () =>
            {
                await context.TestFreeText
                    .Where(e => EF.Functions.FreeTextSearch(e.FreeTextField!, searchValue, true))
                    .ToListAsync(this._testContext.CancellationToken)
                    .ConfigureAwait(false);
            },
            "Unexpected exception");
    }

    /// <summary>
    /// Test FreeTextSearch.
    /// </summary>
    /// <param name="databaseType">Database type.</param>
    /// <returns>A <see cref="Task"/> that represents the asynchronous operation.</returns>
    [TestMethod(DisplayName = "FreeTextSearch")]
    [DataRow(DatabaseTypes.Sqlite, DisplayName = DatabaseTypes.Sqlite)]
    [DataRow(DatabaseTypes.SqlServer, DisplayName = DatabaseTypes.SqlServer)]
    public async Task FreeTextSearchFunctionTests_008_Async(string databaseType)
    {
        // ARRANGE
        using Context context = await DatabaseUtils.OpenDatabaseAsync(this._testContext, databaseType).ConfigureAwait(false);

        int count = 1;
        string value = "Apple";
        string searchValue = "Apple";

        await DatabaseUtils.CreateTestFreeTextTableEntryAsync(context, value, this._testContext.CancellationToken).ConfigureAwait(false);

        // ACT
        List<FreeText> rows = await context.TestFreeText
            .Where(e => EF.Functions.FreeTextSearch(e.FreeTextField!, searchValue))
            .ToListAsync(this._testContext.CancellationToken)
            .ConfigureAwait(false);

        // ASSERT
        Assert.HasCount(count, rows, "Invalid count");
        Assert.AreEqual(value, rows[0].FreeTextField, "Unexpected field value");
    }

    /// <summary>
    /// Test FreeTextSearch with bool useStemming parameter.
    /// </summary>
    /// <param name="databaseType">Database type.</param>
    /// <param name="useStemming">A value indicating whether stemming will be used when searching.</param>
    /// <returns>A <see cref="Task"/> that represents the asynchronous operation.</returns>
    [TestMethod(DisplayName = "FreeTextSearch with bool useStemming parameter")]
    [DataRow(DatabaseTypes.Sqlite, true, DisplayName = $"{DatabaseTypes.Sqlite} stemming on")]
    [DataRow(DatabaseTypes.Sqlite, false, DisplayName = $"{DatabaseTypes.Sqlite} stemming off")]
    [DataRow(DatabaseTypes.SqlServer, true, DisplayName = $"{DatabaseTypes.SqlServer} stemming on")]
    [DataRow(DatabaseTypes.SqlServer, false, DisplayName = $"{DatabaseTypes.SqlServer} stemming off")]
    public async Task FreeTextSearchFunctionTests_009_Async(string databaseType, bool useStemming)
    {
        // ARRANGE
        using Context context = await DatabaseUtils.OpenDatabaseAsync(this._testContext, databaseType).ConfigureAwait(false);

        int count = useStemming ? 1 : 0;
        string value = "Apple";
        string searchValue = "Apples";

        await DatabaseUtils.CreateTestFreeTextTableEntryAsync(context, value, this._testContext.CancellationToken).ConfigureAwait(false);

        // ACT
        List<FreeText> rows = await context.TestFreeText
            .Where(e => EF.Functions.FreeTextSearch(e.FreeTextField!, searchValue, useStemming))
            .ToListAsync(this._testContext.CancellationToken)
            .ConfigureAwait(false);

        // ASSERT
        Assert.HasCount(count, rows, "Invalid count");

        if (rows.Count > 0)
        {
            Assert.AreEqual(value, rows[0].FreeTextField, "Unexpected field value");
        }
    }

    /// <summary>
    /// Test FreeTextSearch with int LanguageTerm parameter.
    /// </summary>
    /// <param name="databaseType">Database type.</param>
    /// <returns>A <see cref="Task"/> that represents the asynchronous operation.</returns>
    [TestMethod(DisplayName = "FreeTextSearch with int LanguageTerm parameter")]
    [DataRow(DatabaseTypes.Sqlite, DisplayName = DatabaseTypes.Sqlite)]
    [DataRow(DatabaseTypes.SqlServer, DisplayName = DatabaseTypes.SqlServer)]
    public async Task FreeTextSearchFunctionTests_010_Async(string databaseType)
    {
        // ARRANGE
        using Context context = await DatabaseUtils.OpenDatabaseAsync(this._testContext, databaseType).ConfigureAwait(false);

        int count = 1;
        string value = "Apple";
        string searchValue = "Apple";

        await DatabaseUtils.CreateTestFreeTextTableEntryAsync(context, value, this._testContext.CancellationToken).ConfigureAwait(false);

        // ACT
        List<FreeText> rows = await context.TestFreeText
            .Where(e => EF.Functions.FreeTextSearch(e.FreeTextField!, searchValue, Context.EnglishLanguageTerm))
            .ToListAsync(this._testContext.CancellationToken)
            .ConfigureAwait(false);

        // ASSERT
        Assert.HasCount(count, rows, "Invalid count");
        Assert.AreEqual(value, rows[0].FreeTextField, "Unexpected field value");
    }

    /// <summary>
    /// Test FreeTextSearch with bool useStemming and int LanguageTerm parameters.
    /// </summary>
    /// <param name="databaseType">Database type.</param>
    /// <param name="useStemming">A value indicating whether stemming will be used when searching.</param>
    /// <returns>A <see cref="Task"/> that represents the asynchronous operation.</returns>
    [TestMethod(DisplayName = "FreeTextSearch with bool useStemming and int LanguageTerm parameters")]
    [DataRow(DatabaseTypes.Sqlite, true, DisplayName = $"{DatabaseTypes.Sqlite} stemming on")]
    [DataRow(DatabaseTypes.Sqlite, false, DisplayName = $"{DatabaseTypes.Sqlite} stemming off")]
    [DataRow(DatabaseTypes.SqlServer, true, DisplayName = $"{DatabaseTypes.SqlServer} stemming on")]
    [DataRow(DatabaseTypes.SqlServer, false, DisplayName = $"{DatabaseTypes.SqlServer} stemming off")]
    public async Task FreeTextSearchFunctionTests_011_Async(string databaseType, bool useStemming)
    {
        // ARRANGE
        using Context context = await DatabaseUtils.OpenDatabaseAsync(this._testContext, databaseType).ConfigureAwait(false);

        int count = useStemming ? 1 : 0;
        string value = "Apple";
        string searchValue = "Apples";

        await DatabaseUtils.CreateTestFreeTextTableEntryAsync(context, value, this._testContext.CancellationToken).ConfigureAwait(false);

        // ACT
        List<FreeText> rows = await context.TestFreeText
            .Where(e => EF.Functions.FreeTextSearch(e.FreeTextField!, searchValue, useStemming, Context.EnglishLanguageTerm))
            .ToListAsync(this._testContext.CancellationToken)
            .ConfigureAwait(false);

        // ASSERT
        Assert.HasCount(count, rows, "Invalid count");

        if (rows.Count > 0)
        {
            Assert.AreEqual(value, rows[0].FreeTextField, "Unexpected field value");
        }
    }

    /// <summary>
    /// Test FreeTextSearch called outside of a query.
    /// </summary>
    /// <param name="overload">The FreeTextSearch overload to call (1-4).</param>
    /// <remarks>The function can only be translated to SQL, so calling it directly (or after the query has switched to client evaluation) must throw.</remarks>
    [TestMethod(DisplayName = "FreeTextSearch called outside of a query")]
    [DataRow(1, DisplayName = "FreeTextSearch(propertyReference, freeText)")]
    [DataRow(2, DisplayName = "FreeTextSearch(propertyReference, freeText, useStemming)")]
    [DataRow(3, DisplayName = "FreeTextSearch(propertyReference, freeText, languageTerm)")]
    [DataRow(4, DisplayName = "FreeTextSearch(propertyReference, freeText, useStemming, languageTerm)")]
    public void FreeTextSearchFunctionTests_012(int overload)
    {
        // ARRANGE
        DbFunctions dbFunctions = EF.Functions;
        string propertyReference = "Apple";
        string freeText = "Apple";

        Action freeTextSearch = overload switch
        {
            1 => () => dbFunctions.FreeTextSearch(propertyReference, freeText),
            2 => () => dbFunctions.FreeTextSearch(propertyReference, freeText, true),
            3 => () => dbFunctions.FreeTextSearch(propertyReference, freeText, Context.EnglishLanguageTerm),
            4 => () => dbFunctions.FreeTextSearch(propertyReference, freeText, true, Context.EnglishLanguageTerm),
            _ => throw new ArgumentOutOfRangeException(nameof(overload))
        };

        // ACT / ASSERT
        Assert.ThrowsExactly<InvalidOperationException>(freeTextSearch, "Unexpected exception");
    }

    /// <summary>
    /// Test FreeTextSearch generates the correct SQL Server function.
    /// </summary>
    /// <param name="expectedFunction">The expected SQL Server function (FREETEXT when stemming, otherwise CONTAINS).</param>
    /// <param name="useStemming">The useStemming parameter value, or <see langword="null"/> to use an overload without it.</param>
    /// <param name="languageTerm">The languageTerm parameter value, or <see langword="null"/> to use an overload without it.</param>
    /// <remarks>The SQL is generated without being run, so this test does not need a SQL Server.</remarks>
    [TestMethod(DisplayName = "FreeTextSearch generates the correct SQL Server function")]
    [DataRow("CONTAINS", null, null, DisplayName = "FreeTextSearch(propertyReference, freeText)")]
    [DataRow("FREETEXT", true, null, DisplayName = "FreeTextSearch(propertyReference, freeText, true)")]
    [DataRow("CONTAINS", false, null, DisplayName = "FreeTextSearch(propertyReference, freeText, false)")]
    [DataRow("CONTAINS", null, 1033, DisplayName = "FreeTextSearch(propertyReference, freeText, 1033)")]
    [DataRow("FREETEXT", true, 1033, DisplayName = "FreeTextSearch(propertyReference, freeText, true, 1033)")]
    [DataRow("CONTAINS", false, 1033, DisplayName = "FreeTextSearch(propertyReference, freeText, false, 1033)")]
    public void FreeTextSearchFunctionTests_013(string expectedFunction, bool? useStemming, int? languageTerm)
    {
        // ARRANGE
        using Context context = new(DatabaseTypes.SqlServer, "Server=localhost;Initial Catalog=Dummy");

        string searchValue = "Apple";

        IQueryable<FreeText> query = (useStemming, languageTerm) switch
        {
            (null, null) => context.TestFreeText.Where(e => EF.Functions.FreeTextSearch(e.FreeTextField!, searchValue)),
            ({ } stemming, null) => context.TestFreeText.Where(e => EF.Functions.FreeTextSearch(e.FreeTextField!, searchValue, stemming)),
            (null, { } language) => context.TestFreeText.Where(e => EF.Functions.FreeTextSearch(e.FreeTextField!, searchValue, language)),
            ({ } stemming, { } language) => context.TestFreeText.Where(e => EF.Functions.FreeTextSearch(e.FreeTextField!, searchValue, stemming, language))
        };

        // ACT
        string sql = query.ToQueryString();

        // ASSERT
        Assert.Contains($"{expectedFunction}(", sql, "Invalid SQL Server function.");

        if (languageTerm is null)
        {
            Assert.DoesNotContain("LANGUAGE", sql, "Unexpected LANGUAGE term.");
        }
        else
        {
            Assert.Contains($"LANGUAGE {languageTerm}", sql, "Missing LANGUAGE term.");
        }
    }

    /// <summary>
    /// Test each FreeTextSearch call in a SQLite query uses its own useStemming value.
    /// </summary>
    /// <param name="firstUseStemming">The useStemming value of the first FreeTextSearch call.</param>
    /// <param name="secondUseStemming">The useStemming value of the second FreeTextSearch call.</param>
    /// <param name="expectedCount">The expected number of rows.</param>
    /// <returns>A <see cref="Task"/> that represents the asynchronous operation.</returns>
    /// <remarks>
    /// SQLite uses a separate table for stemming. Both calls search for "Apples", which only matches "Apple" with stemming,
    /// so the row is only found when both calls use stemming.
    /// </remarks>
    [TestMethod(DisplayName = "Each FreeTextSearch call on SQLite uses its own useStemming value")]
    [DataRow(true, true, 1, DisplayName = "Stemming on then on")]
    [DataRow(true, false, 0, DisplayName = "Stemming on then off")]
    [DataRow(false, true, 0, DisplayName = "Stemming off then on")]
    [DataRow(false, false, 0, DisplayName = "Stemming off then off")]
    public async Task FreeTextSearchFunctionTests_014_Async(bool firstUseStemming, bool secondUseStemming, int expectedCount)
    {
        // ARRANGE
        using Context context = await DatabaseUtils.OpenDatabaseAsync(this._testContext, DatabaseTypes.Sqlite).ConfigureAwait(false);

        string value = "Apple";

        await DatabaseUtils.CreateTestFreeTextTableEntryAsync(context, value, this._testContext.CancellationToken).ConfigureAwait(false);

        // ACT
        List<FreeText> rows = await context.TestFreeText
            .Where(e => EF.Functions.FreeTextSearch(e.FreeTextField!, "Apples", firstUseStemming))
            .Where(e => EF.Functions.FreeTextSearch(e.FreeTextField!, "Apples", secondUseStemming))
            .ToListAsync(this._testContext.CancellationToken)
            .ConfigureAwait(false);

        // ASSERT
        Assert.HasCount(expectedCount, rows, "Invalid count");
    }

    /// <summary>
    /// Test FreeTextSearch and EF Core's own method translations work together.
    /// </summary>
    /// <param name="databaseType">Database type.</param>
    /// <param name="connectionString">A connection string for the database type (no connection is opened).</param>
    /// <param name="expectedContainsSql">The SQL EF Core generates for <see cref="string.Contains(string)"/> on the database type.</param>
    /// <remarks>
    /// The test <see cref="Context"/> calls <see cref="FreeTextSearchExtensions.UseFreeTextSearchExtensions"/>. Its translator must be added to
    /// EF Core's translators rather than replacing them, otherwise methods such as Substring and Math.Abs can't be translated.
    /// The SQL is generated without being run, so this test does not need a database.
    /// </remarks>
    [TestMethod(DisplayName = "FreeTextSearch and EF Core's own method translations work together")]
    [DataRow(DatabaseTypes.Sqlite, "Data Source=:memory:", "INSTR(", DisplayName = DatabaseTypes.Sqlite)]
    [DataRow(DatabaseTypes.SqlServer, "Server=localhost;Initial Catalog=Dummy", "LIKE", DisplayName = DatabaseTypes.SqlServer)]
    public void FreeTextSearchFunctionTests_015(string databaseType, string connectionString, string expectedContainsSql)
    {
        // ARRANGE
        using Context context = new(databaseType, connectionString);

        // ACT
        string containsSql = context.TestTable1.Where(e => e.TestField.Contains("Test")).ToQueryString();
        string substringSql = context.TestTable1.Where(e => e.TestField.Substring(0, 2) == "Te").ToQueryString();
        string absSql = context.TestTable1.Where(e => Math.Abs(e.Id) > 1).ToQueryString();
        string freeTextSearchSql = context.TestFreeText.Where(e => EF.Functions.FreeTextSearch(e.FreeTextField!, "Apple")).ToQueryString();

        // ASSERT
        Assert.Contains(expectedContainsSql, containsSql.ToUpperInvariant(), "Contains not translated.");
        Assert.Contains("SUBSTR", substringSql.ToUpperInvariant(), "Substring not translated.");
        Assert.Contains("ABS(", absSql.ToUpperInvariant(), "Math.Abs not translated.");
        Assert.Contains(databaseType == DatabaseTypes.Sqlite ? "MATCH(" : "CONTAINS(", freeTextSearchSql.ToUpperInvariant(), "FreeTextSearch not translated.");
    }

    /// <summary>
    /// Test FreeTextSearch with stemming on SQLite, in a query that also uses another shared-type entity type without a stemming table.
    /// </summary>
    /// <returns>A <see cref="Task"/> that represents the asynchronous operation.</returns>
    /// <remarks>
    /// Only the free text table is swapped for its stemming table; the property bag entity type is left alone.
    /// "Apples" only matches "Apple" in the stemming table, so one row shows the free text table was swapped.
    /// </remarks>
    [TestMethod(DisplayName = "FreeTextSearch with stemming leaves other shared-type entity types alone")]
    public async Task FreeTextSearchFunctionTests_016_Async()
    {
        // ARRANGE
        const string propertyBagTableName = "PropertyBag";
        CancellationToken cancellationToken = this._testContext.CancellationToken;

        using Context context = await DatabaseUtils.OpenDatabaseAsync(
            this._testContext,
            databaseType: DatabaseTypes.Sqlite,
            customModelCreationActions: modelBuilder =>
            {
                modelBuilder.SharedTypeEntity<Dictionary<string, object>>(propertyBagTableName, builder =>
                {
                    builder.Property<long>("Id");
                    builder.Property<string>("Name");
                    builder.HasKey("Id");
                });
            })
            .ConfigureAwait(false);

        await DatabaseUtils.CreateTestFreeTextTableEntryAsync(context, "Apple", cancellationToken).ConfigureAwait(false);
        await context.Database.ExecuteSqlRawAsync($"INSERT INTO {propertyBagTableName} (Id, Name) VALUES (1, 'Bag')", cancellationToken).ConfigureAwait(false);

        // ACT
        List<FreeText> rows = await (
            from freeText in context.TestFreeText
            from propertyBag in context.Set<Dictionary<string, object>>(propertyBagTableName)
            where EF.Functions.FreeTextSearch(freeText.FreeTextField!, "Apples", true) && EF.Property<string>(propertyBag, "Name") == "Bag"
            select freeText)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        // ASSERT
        Assert.HasCount(1, rows, "Invalid count");
    }

    /// <summary>
    /// Test a SQLite query with a stemming and a non-stemming FreeTextSearch on different rows of the same table.
    /// </summary>
    /// <returns>A <see cref="Task"/> that represents the asynchronous operation.</returns>
    /// <remarks>
    /// "Apples" only matches "Apple" with stemming, and "Banana" matches without it, so the pair is only found if each call uses its own
    /// useStemming value. The rows come from the main table, with the stemming search matched back to them by key.
    /// </remarks>
    [TestMethod(DisplayName = "FreeTextSearch with and without stemming on different rows of the same table")]
    public async Task FreeTextSearchFunctionTests_017_Async()
    {
        // ARRANGE
        CancellationToken cancellationToken = this._testContext.CancellationToken;
        using Context context = await DatabaseUtils.OpenDatabaseAsync(this._testContext, DatabaseTypes.Sqlite).ConfigureAwait(false);

        await DatabaseUtils.CreateTestFreeTextTableEntryAsync(context, "Apple", cancellationToken).ConfigureAwait(false);
        context.ChangeTracker.Clear();
        await DatabaseUtils.CreateTestFreeTextTableEntryAsync(context, "Banana", cancellationToken).ConfigureAwait(false);
        context.ChangeTracker.Clear();

        IQueryable<string?[]> query =
            from apple in context.TestFreeText
            from banana in context.TestFreeText
            where EF.Functions.FreeTextSearch(apple.FreeTextField!, "Apples", true) && EF.Functions.FreeTextSearch(banana.FreeTextField!, "Banana", false)
            select new[] { apple.FreeTextField, banana.FreeTextField };

        // ACT
        string sql = query.ToQueryString();
        List<string?[]> rows = await query.ToListAsync(cancellationToken).ConfigureAwait(false);

        // ASSERT
        Assert.Contains(Context.TestFreeTextStemmingTableName, sql, "Stemming table not searched");
        Assert.HasCount(1, rows, "Invalid count");
        Assert.AreEqual("Apple", rows[0][0], "Invalid stemming search row");
        Assert.AreEqual("Banana", rows[0][1], "Invalid non-stemming search row");
    }

    /// <summary>
    /// Test FreeTextSearch with stemming on SQLite where the table or its stemming table has no property mapped to ROWID.
    /// </summary>
    /// <param name="tableName">The table whose Id property is mapped to an ordinary column rather than ROWID.</param>
    /// <remarks>
    /// The stemming search is matched back to the row by ROWID, which EF Core can only refer to through a mapped property.
    /// The SQL is generated without being run, so this test does not need a database.
    /// </remarks>
    [TestMethod(DisplayName = "FreeTextSearch with stemming where a table has no property mapped to ROWID")]
    [DataRow("TestFreeText", DisplayName = "Table")]
    [DataRow("TestFreeText_Stemming", DisplayName = "Stemming table")]
    public void FreeTextSearchFunctionTests_018(string tableName)
    {
        // ARRANGE
        using Context context = new(
            DatabaseTypes.Sqlite,
            "Data Source=:memory:",
            customModelCreationActions: modelBuilder => modelBuilder.SharedTypeEntity<FreeText>(tableName).Property<long>(nameof(FreeText.Id)).HasColumnName(nameof(FreeText.Id)));

        IQueryable<FreeText> query = context.TestFreeText.Where(e => EF.Functions.FreeTextSearch(e.FreeTextField!, "Apples", true));

        // ACT / ASSERT
        InvalidOperationException exception = Assert.ThrowsExactly<InvalidOperationException>(() => query.ToQueryString(), "Unexpected exception");

        Assert.Contains("ROWID", exception.Message, "Unexpected exception message");
        Assert.Contains(tableName, exception.Message, "Unexpected exception message");
    }

    #endregion FreeTextSearch tests
}