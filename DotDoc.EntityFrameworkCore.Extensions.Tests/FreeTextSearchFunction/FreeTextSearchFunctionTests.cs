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
    [DataRow(null, typeof(ArgumentNullException), DisplayName = "TableName null")]
    [DataRow("", typeof(ArgumentException), DisplayName = "TableName null")]
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
                        modelBuilder.SharedTypeEntity<FreeText>(Context.TestFreeTextTableName)
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
    /// Test SetStemmingTable and GetStemmingTable.
    /// </summary>
    [TestMethod("SetStemmingTable and GetStemmingTable")]
    public void Test_SetStemmingTable_GetStemmingTable()
    {
        // ARRANGE / ACT
        // SetStemming is called in CreateDatabase.
        using Context context = DatabaseUtils.CreateDatabase(DatabaseTypes.Sqlite);

        IEntityType entityType = context.Model.FindEntityType(Context.TestFreeTextTableName)!;
        string? stemmingTableName = entityType.GetStemmingTable();

        // ASSERT
        Assert.AreEqual(Context.TestFreeTextStemmingTableName, stemmingTableName, "Invalid stemming table name");
    }

    /// <summary>
    /// Test FreeTextSearch function with a missing stemming table name.
    /// </summary>
    /// <returns>A <see cref="Task"/> that represents the asynchronous operation.</returns>
    [TestMethod("FreeTextSearch missing stemming table name")]
    public async Task Test_FreeTextSearchFunction_MissingStemmingTableNameAsync()
    {
        string tableName = $"{Context.TestFreeTextTableName}2";

        // ARRANGE
        using Context context = DatabaseUtils.CreateDatabase(
            databaseType: DatabaseTypes.Sqlite,
            customModelCreationActions: (modelBuilder) =>
            {
                modelBuilder.SharedTypeEntity<FreeText>(tableName)
                    .Property<long>(nameof(FreeText.Id))
                    .HasColumnName("ROWID");
            });

        string searchValue = "Apple";
        string message = "Stemming table not specified.";

        // ACT / ASSERT
        InvalidOperationException e = await Assert.ThrowsExactlyAsync<InvalidOperationException>(
            async () =>
            {
                await context.Set<FreeText>(tableName)
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

        using Context context = DatabaseUtils.CreateDatabase(
            databaseType: DatabaseTypes.Sqlite,
            customModelCreationActions: (modelBuilder) =>
            {
                modelBuilder.SharedTypeEntity<FreeText>(Context.TestFreeTextTableName)
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
                await context.TestFreeText
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
        using Context context = DatabaseUtils.CreateDatabase(databaseType);

        int count = 1;
        string value = "Apple";
        string searchValue = "Apple";

        DatabaseUtils.CreateTestFreeTextTableEntry(context, value);

        // ACT
        List<FreeText> rows = await context.TestFreeText
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
        using Context context = DatabaseUtils.CreateDatabase(databaseType);

        int count = useStemming ? 1 : 0;
        string value = "Apple";
        string searchValue = "Apples";

        DatabaseUtils.CreateTestFreeTextTableEntry(context, value);

        // ACT
        List<FreeText> rows = await context.TestFreeText
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
        using Context context = DatabaseUtils.CreateDatabase(databaseType);

        int count = 1;
        string value = "Apple";
        string searchValue = "Apple";

        DatabaseUtils.CreateTestFreeTextTableEntry(context, value);

        // ACT
        List<FreeText> rows = await context.TestFreeText
            .Where(e => EF.Functions.FreeTextSearch(e.FreeTextField!, searchValue, Context.EnglishLanguageTerm))
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
        using Context context = DatabaseUtils.CreateDatabase(databaseType);

        int count = useStemming ? 1 : 0;
        string value = "Apple";
        string searchValue = "Apples";

        DatabaseUtils.CreateTestFreeTextTableEntry(context, value);

        // ACT
        List<FreeText> rows = await context.TestFreeText
            .Where(e => EF.Functions.FreeTextSearch(e.FreeTextField!, searchValue, useStemming, Context.EnglishLanguageTerm))
            .ToListAsync()
            .ConfigureAwait(false);

        // ASSERT
        Assert.AreEqual(count, rows.Count, "Invalid count");
        Assert.IsTrue(rows.Count == 0 || value == rows[0].FreeTextField, "Unexpected field value");
    }

    #endregion public methods
}