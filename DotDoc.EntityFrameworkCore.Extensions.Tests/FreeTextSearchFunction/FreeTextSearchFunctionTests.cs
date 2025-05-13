// Copyright ©2021-2025 Mike King.
// This file is licensed to you under the MIT license.
// See the License.txt file in the solution root for more information.

using DotDoc.EntityFrameworkCore.Extensions.DatabaseType;
using DotDoc.EntityFrameworkCore.Extensions.FreeTextSearchFunction;
using DotDoc.EntityFrameworkCore.Extensions.Tests.Data;
using DotDoc.EntityFrameworkCore.Extensions.Tests.Utilities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DotDoc.EntityFrameworkCore.Extensions.Tests.FreeTextSearchFunction;

/// <summary>
/// Tests for the FreeTextSearchFunction extension.
/// </summary>
[TestClass]
public class FreeTextSearchFunctionTests
{
    #region private fields

    /// <summary>
    /// FreeText table name.
    /// </summary>
    private const string TableName = "TestFreeText";

    /// <summary>
    /// SQLite FreeText Stemming table name.
    /// </summary>
    private const string StemmingTableName = "TestFreeText_Stemming";

    /// <summary>
    /// SQL Server Language Term for English (from sys.syslanguages).
    /// </summary>
    private const int EnglishLanguageTerm = 1033;

    #endregion private fields

    #region public methods

    /// <summary>
    /// Test UseFreeTextExtensions Guard Clause.
    /// </summary>
    [TestMethod("UseFreeTextExtensions Guard Clause")]
    public void Test_UseFreeTextExtensions_GuardClause()
    {
        // ARRANGE
        DbContextOptionsBuilder? optionsBuilder = null;

        // ACT / ASSERT
        ArgumentNullException e = Assert.ThrowsExactly<ArgumentNullException>(() => _ = optionsBuilder!.UseFreeTextExtensions(), "Missing exception");
        Assert.AreEqual(nameof(optionsBuilder), e.ParamName, "Invalid parameter name");
    }

    /// <summary>
    /// Test SetStemmingTable with EntityBuilder parameter Guard Clause.
    /// </summary>
    [TestMethod("SetStemmingTable with EntityBuilder parameter Guard Clause")]
    public void Test_SetStemmingTable_EntityBuilder_GuardClause()
    {
        // ARRANGE
        EntityTypeBuilder<FreeText>? entityBuilder = null;
        string tableName = "dummy";

        // ACT / ASSERT
        ArgumentNullException e = Assert.ThrowsExactly<ArgumentNullException>(() => _ = entityBuilder!.SetStemmingTable(tableName), "Missing exception");
        Assert.AreEqual(nameof(entityBuilder), e.ParamName, "Invalid parameter name");
    }

    /// <summary>
    /// Test SetStemmingTable with TableName parameter Guard Clause.
    /// </summary>
    /// <param name="tableName">The stemming table name.</param>
    /// <param name="exceptionType">The type of exception raised.</param>
    [TestMethod("SetStemmingTable with TableName parameter Guard Clause")]
    [DynamicData(nameof(Get_SetStemmingTable_TableName_GuardClause_TestData), DynamicDataSourceType.Method, DynamicDataDisplayName = nameof(TestUtils.CreateDynamicDisplayName), DynamicDataDisplayNameDeclaringType = typeof(TestUtils))]
    public void Test_SetStemmingTable_TableName_GuardClause(string tableName, Type exceptionType)
    {
        // ARRANGE / ACT / ASSERT
        Exception e = Assert.Throws<Exception>(
            () =>
            {
                using Context context = DatabaseUtils.CreateDatabase(
                     databaseType: DatabaseTypes.Sqlite,
                     customModelCreationActions: (modelBuilder) =>
                     {
                        modelBuilder.SharedTypeEntity<FreeText>(TableName)
                            .SetStemmingTable(tableName)
                            .Property<long>(nameof(FreeText.Id))
                            .HasColumnName("ROWID");
                     });
            },
            "Unexpected exception");

        Assert.AreEqual(exceptionType, e.GetType(), "Invalid exception type");
        Assert.AreEqual(nameof(tableName), ((ArgumentException)e).ParamName, "Invalid parameter name");
    }

    /// <summary>
    /// Test GetStemmingTable with EntityType parameter Guard Clause.
    /// </summary>
    [TestMethod("GetStemmingTable with EntityType parameter Guard Clause")]
    public void Test_GetStemmingTable_EntityType_GuardClause()
    {
        // ARRANGE
        IEntityType? entityType = null;

        // ACT / ASSERT
        ArgumentNullException e = Assert.ThrowsExactly<ArgumentNullException>(() => _ = entityType!.GetStemmingTable(), "Missing exception");
        Assert.AreEqual(nameof(entityType), e.ParamName, "Invalid parameter name");
    }

    /// <summary>
    /// Test SetStemmingTable.
    /// </summary>
    [TestMethod("SetStemmingTable")]
    public void Test_SetStemmingTable()
    {
        // ARRANGE / ACT / ASSERT
        using Context context = DatabaseUtils.CreateDatabase(
            databaseType: DatabaseTypes.Sqlite,
            customConfigurationActions: (optionsBuilder) =>
            {
                optionsBuilder.UseFreeTextExtensions();
            },
            customModelCreationActions: (modelBuilder) =>
            {
                modelBuilder.SharedTypeEntity<FreeText>(TableName)
                    .SetStemmingTable(StemmingTableName)
                    .Property<long>(nameof(FreeText.Id))
                    .HasColumnName("ROWID");
            });

        IEntityType entityType = context.Model.FindEntityType(TableName)!;
        string? stemmingTableName = entityType.GetStemmingTable();

        Assert.AreEqual(StemmingTableName, stemmingTableName, "Invalid stemming table name");
    }

    /// <summary>
    /// Test FreeTextSearch function with a missing stemming table name.
    /// </summary>
    /// <returns>A <see cref="Task"/> that represents the asynchronous operation.</returns>
    [TestMethod("FreeTextSearch missing stemming table name")]
    public async Task Test_FreeTextSearchFunction_MissingStemmingTableNameAsync()
    {
        // ARRANGE
        Context context = DatabaseUtils.CreateDatabase(
            databaseType: DatabaseTypes.Sqlite,
            customConfigurationActions: (optionsBuilder) =>
            {
                optionsBuilder.UseFreeTextExtensions();
            },
            customModelCreationActions: (modelBuilder) =>
            {
                modelBuilder.SharedTypeEntity<FreeText>(TableName)
                    .Property<long>(nameof(FreeText.Id))
                    .HasColumnName("ROWID");
            });

        string searchValue = "Apple";
        string message = "Stemming table not specified.";

        // ACT / ASSERT
        InvalidOperationException e = await Assert.ThrowsExactlyAsync<InvalidOperationException>(
            async () =>
            {
                await context.Set<FreeText>(TableName)
                    .Where(e => EF.Functions.FreeTextSearch(e.FreeTextField!, searchValue, true))
                    .ToListAsync()
                    .ConfigureAwait(false);
            },
            "Unexpected exception");

        Assert.AreEqual(message, e.Message, "Invalid exception message");
    }

    /// <summary>
    /// Test FreeTextSearch function with a non existant stemming table name.
    /// </summary>
    /// <returns>A <see cref="Task"/> that represents the asynchronous operation.</returns>
    [TestMethod("FreeTextSearch non existant stemming table name")]
    public async Task Test_FreeTextSearchFunction_NonExistantStemmingTableNameAsync()
    {
        // ARRANGE
        string stemmingTableName = "NonExistantTableName";

        Context context = DatabaseUtils.CreateDatabase(
            databaseType: DatabaseTypes.Sqlite,
            customConfigurationActions: (optionsBuilder) =>
            {
                optionsBuilder.UseFreeTextExtensions();
            },
            customModelCreationActions: (modelBuilder) =>
            {
                modelBuilder.SharedTypeEntity<FreeText>(TableName)
                    .SetStemmingTable(stemmingTableName)
                    .Property<long>(nameof(FreeText.Id))
                    .HasColumnName("ROWID");
            });

        string searchValue = "Apple";
        string message = $"Stemming table {stemmingTableName} not found.";

        // ACT / ASSERT
        InvalidOperationException e = await Assert.ThrowsExactlyAsync<InvalidOperationException>(
            async () =>
            {
                await context.Set<FreeText>(TableName)
                    .Where(e => EF.Functions.FreeTextSearch(e.FreeTextField!, searchValue, true))
                    .ToListAsync()
                    .ConfigureAwait(false);
            },
            "Unexpected exception");

        Assert.AreEqual(message, e.Message, "Invalid exception message");
    }

    /// <summary>
    /// Test FreeTextSearch function.
    /// </summary>
    /// <param name="databaseType">Database type.</param>
    /// <returns>A <see cref="Task"/> that represents the asynchronous operation.</returns>
    [TestMethod("FreeTextSearch")]
    [DataRow(DatabaseTypes.Sqlite, DisplayName = DatabaseTypes.Sqlite)]
    [DataRow(DatabaseTypes.SqlServer, DisplayName = DatabaseTypes.SqlServer)]
    public async Task Test_FreeTextSearchFunctionAsync(string databaseType)
    {
        // ARRANGE
        Context context = await CreateContextAsync(databaseType).ConfigureAwait(false);

        int count = 1;
        string value = "Apple";
        string searchValue = "Apple";

        await CreateFreeTextTableEntryAsync(context, value).ConfigureAwait(false);

        // ACT
        List<FreeText> rows = await context.Set<FreeText>(TableName)
            .Where(e => EF.Functions.FreeTextSearch(e.FreeTextField!, searchValue))
            .ToListAsync()
            .ConfigureAwait(false);

        // ASSERT
        Assert.AreEqual(count, rows.Count, "Invalid count");
        Assert.AreEqual(value, rows[0].FreeTextField, "Unexpected field value");
    }

    /// <summary>
    /// Test FreeTextSearch function with use stemming parameter.
    /// </summary>
    /// <param name="databaseType">Database type.</param>
    /// <param name="useStemming">A value indicating whether stemming will be used when searching.</param>
    /// <returns>A <see cref="Task"/> that represents the asynchronous operation.</returns>
    [TestMethod("FreeTextSearch with UseStemming parameter")]
    [DataRow(DatabaseTypes.Sqlite, true, DisplayName = $"{DatabaseTypes.Sqlite} stemming on")]
    [DataRow(DatabaseTypes.Sqlite, false, DisplayName = $"{DatabaseTypes.Sqlite} stemming off")]
    [DataRow(DatabaseTypes.SqlServer, true, DisplayName = $"{DatabaseTypes.SqlServer} stemming on")]
    [DataRow(DatabaseTypes.SqlServer, false, DisplayName = $"{DatabaseTypes.SqlServer} stemming off")]
    public async Task Test_FreeTextSearchFunction_UseStemmingAsync(string databaseType, bool useStemming)
    {
        // ARRANGE
        Context context = await CreateContextAsync(databaseType).ConfigureAwait(false);

        int count = useStemming ? 1 : 0;
        string value = "Apple";
        string searchValue = "Apples";

        await CreateFreeTextTableEntryAsync(context, value).ConfigureAwait(false);

        // ACT
        List<FreeText> rows = await context.Set<FreeText>(TableName)
            .Where(e => EF.Functions.FreeTextSearch(e.FreeTextField!, searchValue, useStemming))
            .ToListAsync()
            .ConfigureAwait(false);

        // ASSERT
        Assert.AreEqual(count, rows.Count, "Invalid count");
        Assert.IsTrue(rows.Count == 0 || value == rows[0].FreeTextField, "Unexpected field value");
    }

    /// <summary>
    /// Test FreeTextSearch function with language term parameter.
    /// </summary>
    /// <param name="databaseType">Database type.</param>
    /// <returns>A <see cref="Task"/> that represents the asynchronous operation.</returns>
    [TestMethod("FreeTextSearch with LanguageTerm parameter")]
    [DataRow(DatabaseTypes.Sqlite, DisplayName = DatabaseTypes.Sqlite)]
    [DataRow(DatabaseTypes.SqlServer, DisplayName = DatabaseTypes.SqlServer)]
    public async Task Test_FreeTextSearchFunction_LanguageTermAsync(string databaseType)
    {
        // ARRANGE
        Context context = await CreateContextAsync(databaseType).ConfigureAwait(false);

        int count = 1;
        string value = "Apple";
        string searchValue = "Apple";

        await CreateFreeTextTableEntryAsync(context, value).ConfigureAwait(false);

        // ACT
        List<FreeText> rows = await context.Set<FreeText>(TableName)
            .Where(e => EF.Functions.FreeTextSearch(e.FreeTextField!, searchValue, EnglishLanguageTerm))
            .ToListAsync()
            .ConfigureAwait(false);

        // ASSERT
        Assert.AreEqual(count, rows.Count, "Invalid count");
        Assert.AreEqual(value, rows[0].FreeTextField, "Unexpected field value");
    }

    /// <summary>
    /// Test FreeTextSearch function with use stemming and language term parameters.
    /// </summary>
    /// <param name="databaseType">Database type.</param>
    /// <param name="useStemming">A value indicating whether stemming will be used when searching.</param>
    /// <returns>A <see cref="Task"/> that represents the asynchronous operation.</returns>
    [TestMethod("FreeTextSearch with UseStemming and LanguageTerm parameters")]
    [DataRow(DatabaseTypes.Sqlite, true, DisplayName = $"{DatabaseTypes.Sqlite} stemming on")]
    [DataRow(DatabaseTypes.Sqlite, false, DisplayName = $"{DatabaseTypes.Sqlite} stemming off")]
    [DataRow(DatabaseTypes.SqlServer, true, DisplayName = $"{DatabaseTypes.SqlServer} stemming on")]
    [DataRow(DatabaseTypes.SqlServer, false, DisplayName = $"{DatabaseTypes.SqlServer} stemming off")]
    public async Task Test_FreeTextSearchFunction_UseStemming_LanguageTermAsync(string databaseType, bool useStemming)
    {
        // ARRANGE
        Context context = await CreateContextAsync(databaseType).ConfigureAwait(false);

        int count = useStemming ? 1 : 0;
        string value = "Apple";
        string searchValue = "Apples";

        await CreateFreeTextTableEntryAsync(context, value).ConfigureAwait(false);

        // ACT
        List<FreeText> rows = await context.Set<FreeText>(TableName)
            .Where(e => EF.Functions.FreeTextSearch(e.FreeTextField!, searchValue, useStemming, EnglishLanguageTerm))
            .ToListAsync()
            .ConfigureAwait(false);

        // ASSERT
        Assert.AreEqual(count, rows.Count, "Invalid count");
        Assert.IsTrue(rows.Count == 0 || value == rows[0].FreeTextField, "Unexpected field value");
    }

    #endregion public methods

    #region private methods

    /// <summary>
    /// Get test data for the SetStemmingTable with TableName parameter.
    /// </summary>
    /// <returns><see cref="IEnumerable{T}"/>.</returns>
    private static IEnumerable<object?[]> Get_SetStemmingTable_TableName_GuardClause_TestData()
    {
        // 0. table name
        // 1. Type exceptionType
        yield return [
            null,
            typeof(ArgumentNullException)];

        yield return [
            string.Empty,
            typeof(ArgumentException)];
    }

    /// <summary>
    /// Create a database context.
    /// </summary>
    /// <param name="databaseType">The database type.</param>
    /// <returns>An instance of <see cref="Context"/> with the free text table(s) configured.</returns>
    private static async Task<Context> CreateContextAsync(string databaseType)
    {
        Context context = DatabaseUtils.CreateDatabase(
            databaseType: databaseType,
            customConfigurationActions: (optionsBuilder) =>
            {
                optionsBuilder.UseFreeTextExtensions();
            },
            customModelCreationActions: (modelBuilder) =>
            {
                // In SQLite associate the stemming table with the non stemming table
                // and the Id column with the value of the ROWID column.
                if (databaseType == DatabaseTypes.Sqlite)
                {
                    modelBuilder.SharedTypeEntity<FreeText>(TableName)
                        .SetStemmingTable(StemmingTableName)
                        .Property<long>(nameof(FreeText.Id))
                        .HasColumnName("ROWID");

                    modelBuilder.SharedTypeEntity<FreeText>(StemmingTableName)
                        .Property<long>(nameof(FreeText.Id))
                        .HasColumnName("ROWID");
                }
            });

        await DatabaseUtils.InitialiseFreeTextTablesAsync(context, TableName, StemmingTableName).ConfigureAwait(false);

        return context;
    }

    /// <summary>
    /// Create an entry in the FreeText table(s).
    /// </summary>
    /// <param name="context">The database context.</param>
    /// <param name="value">The value to insert.</param>
    /// <returns>A <see cref="Task"/> that represents the asynchronous operation.</returns>
    private static async Task CreateFreeTextTableEntryAsync(Context context, string value)
    {
        FreeText freeText = new()
        {
            FreeTextField = value
        };

        await context.Set<FreeText>(TableName).AddAsync(freeText).ConfigureAwait(false);

        if (context.DatabaseType == DatabaseTypes.Sqlite)
        {
            await context.Set<FreeText>(StemmingTableName).AddAsync(freeText).ConfigureAwait(false);
        }

        await context.SaveChangesAsync().ConfigureAwait(false);
    }

    #endregion private methods
}