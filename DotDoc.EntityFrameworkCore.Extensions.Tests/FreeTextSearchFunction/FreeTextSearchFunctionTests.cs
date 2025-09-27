// Copyright ©2021-2025 Mike King.
// This file is licensed to you under the MIT license.
// See the License.txt file in the solution root for more information.

using DotDoc.EntityFrameworkCore.Extensions.CaseInsensitivity;
using DotDoc.EntityFrameworkCore.Extensions.DatabaseType;
using DotDoc.EntityFrameworkCore.Extensions.FreeTextSearchFunction;
using DotDoc.EntityFrameworkCore.Extensions.Tests.Data;
using DotDoc.EntityFrameworkCore.Extensions.Tests.TestUtilities;
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
    #region public methods

    /// <summary>
    /// Test UseFreeTextSearchExtensions with a Null DbContextOptionsBuilder OptionsBuilder parameter.
    /// </summary>
    [TestMethod(DisplayName = "UseFreeTextSearchExtensions with a Null DbContextOptionsBuilder OptionsBuilder parameter")]
    public void FreeTextSearchFunctionTests_001()
    {
        // ARRANGE
        DbContextOptionsBuilder optionsBuilder = null!;

        // ACT / ASSERT
        Assert.ThrowsExactly<ArgumentNullException>(() => _ = optionsBuilder.UseFreeTextSearchExtensions(), "Unexpected exception");
    }

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
        ArgumentNullException e = Assert.ThrowsExactly<ArgumentNullException>(() => _ = entityBuilder!.SetStemmingTable(tableName), "Unexpected exception");
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
                using Context context = await DatabaseUtils.CreateDatabaseAsync(
                    databaseType: DatabaseTypes.Sqlite,
                    customModelCreationActions: (modelBuilder) =>
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

    /// <summary>
    /// Test GetStemmingTable with a Null IEntityType EntityType parameter.
    /// </summary>
    [TestMethod(DisplayName = "GetStemmingTable with a Null IEntityType EntityType parameter")]
    public void FreeTextSearchFunctionTests_004()
    {
        // ARRANGE
        IEntityType? entityType = null;

        // ACT / ASSERT
        Assert.ThrowsExactly<ArgumentNullException>(() => _ = entityType!.GetStemmingTable(), "Unexpected exception");
    }

    /// <summary>
    /// Test SetStemmingTable and GetStemmingTable.
    /// </summary>
    /// <returns>A <see cref="Task"/> that represents the asynchronous operation.</returns>
    [TestMethod(DisplayName = "SetStemmingTable and GetStemmingTable")]
    public async Task FreeTextSearchFunctionTests_005_Async()
    {
        // ARRANGE / ACT (SetStemming is called in CreateDatabase).
        using Context context = await DatabaseUtils.CreateDatabaseAsync(DatabaseTypes.Sqlite).ConfigureAwait(false);

        IEntityType entityType = context.Model.FindEntityType(Context.TestFreeTextTableName)!;
        string? stemmingTableName = entityType.GetStemmingTable();

        // ASSERT
        Assert.AreEqual(Context.TestFreeTextStemmingTableName, stemmingTableName, "Invalid stemming table name");
    }

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

        using Context context = await DatabaseUtils.CreateDatabaseAsync(
            databaseType: DatabaseTypes.Sqlite,
            customModelCreationActions: (modelBuilder) =>
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
                    .ToListAsync(CancellationToken.None)
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

        using Context context = await DatabaseUtils.CreateDatabaseAsync(
            databaseType: DatabaseTypes.Sqlite,
            customModelCreationActions: (modelBuilder) =>
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
                    .ToListAsync(CancellationToken.None)
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
    public async Task FreeTextSearchFunctionTests_007_Async(string databaseType)
    {
        // ARRANGE
        using Context context = await DatabaseUtils.CreateDatabaseAsync(databaseType).ConfigureAwait(false);

        int count = 1;
        string value = "Apple";
        string searchValue = "Apple";

        await DatabaseUtils.CreateTestFreeTextTableEntryAsync(context, value).ConfigureAwait(false);

        // ACT
        List<FreeText> rows = await context.TestFreeText
            .Where(e => EF.Functions.FreeTextSearch(e.FreeTextField!, searchValue))
            .ToListAsync(CancellationToken.None)
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
    public async Task FreeTextSearchFunctionTests_008_Async(string databaseType, bool useStemming)
    {
        // ARRANGE
        using Context context = await DatabaseUtils.CreateDatabaseAsync(databaseType).ConfigureAwait(false);

        int count = useStemming ? 1 : 0;
        string value = "Apple";
        string searchValue = "Apples";

        await DatabaseUtils.CreateTestFreeTextTableEntryAsync(context, value).ConfigureAwait(false);

        // ACT
        List<FreeText> rows = await context.TestFreeText
            .Where(e => EF.Functions.FreeTextSearch(e.FreeTextField!, searchValue, useStemming))
            .ToListAsync(CancellationToken.None)
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
    public async Task FreeTextSearchFunctionTests_009_Async(string databaseType)
    {
        // ARRANGE
        using Context context = await DatabaseUtils.CreateDatabaseAsync(databaseType).ConfigureAwait(false);

        int count = 1;
        string value = "Apple";
        string searchValue = "Apple";

        await DatabaseUtils.CreateTestFreeTextTableEntryAsync(context, value).ConfigureAwait(false);

        // ACT
        List<FreeText> rows = await context.TestFreeText
            .Where(e => EF.Functions.FreeTextSearch(e.FreeTextField!, searchValue, Context.EnglishLanguageTerm))
            .ToListAsync(CancellationToken.None)
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
    public async Task FreeTextSearchFunctionTests_010_Async(string databaseType, bool useStemming)
    {
        // ARRANGE
        using Context context = await DatabaseUtils.CreateDatabaseAsync(databaseType).ConfigureAwait(false);

        int count = useStemming ? 1 : 0;
        string value = "Apple";
        string searchValue = "Apples";

        await DatabaseUtils.CreateTestFreeTextTableEntryAsync(context, value).ConfigureAwait(false);

        // ACT
        List<FreeText> rows = await context.TestFreeText
            .Where(e => EF.Functions.FreeTextSearch(e.FreeTextField!, searchValue, useStemming, Context.EnglishLanguageTerm))
            .ToListAsync(CancellationToken.None)
            .ConfigureAwait(false);

        // ASSERT
        Assert.HasCount(count, rows, "Invalid count");

        if (rows.Count > 0)
        {
            Assert.AreEqual(value, rows[0].FreeTextField, "Unexpected field value");
        }
    }

    #endregion public methods
}