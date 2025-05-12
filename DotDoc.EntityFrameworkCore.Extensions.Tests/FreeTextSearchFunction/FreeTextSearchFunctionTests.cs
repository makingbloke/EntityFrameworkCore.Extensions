// Copyright ©2021-2025 Mike King.
// This file is licensed to you under the MIT license.
// See the License.txt file in the solution root for more information.

using DotDoc.EntityFrameworkCore.Extensions.DatabaseType;
using DotDoc.EntityFrameworkCore.Extensions.FreeTextSearchFunction;
using DotDoc.EntityFrameworkCore.Extensions.Tests.Data;
using DotDoc.EntityFrameworkCore.Extensions.Tests.Utilities;
using Microsoft.EntityFrameworkCore;
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

    // Test clauses in stemming table setup.

    /// <summary>
    /// Test FreeTextSearch function.
    /// </summary>
    /// <param name="databaseType">Database type.</param>
    /// <param name="useStemming">A value indicating whether stemming will be used when searching.</param>
    /// <param name="languageTerm">A Language ID from the sys.syslanguages table (SQL Server only).</param>
    /// <returns>A task that represents the asynchronous test operation.</returns>
    [TestMethod("FreeTextSearch")]
    [DataRow(DatabaseTypes.Sqlite, null, null, DisplayName = DatabaseTypes.Sqlite)]
    [DataRow(DatabaseTypes.Sqlite, false, null, DisplayName = DatabaseTypes.Sqlite)]
    [DataRow(DatabaseTypes.Sqlite, true, null, DisplayName = DatabaseTypes.Sqlite)]
    ////[DataRow(DatabaseTypes.SqlServer, DisplayName = DatabaseTypes.SqlServer)]
    public async Task Test_FreeTextSearchFunctionAsync(string databaseType, bool? useStemming, int? languageTerm)
    {
        // ARRANGE
        string tableName = "TestFreeText";
        string stemmingTableName = "TestFreeText_Stemming";

        using Context context = DatabaseUtils.CreateDatabase(
            databaseType: databaseType,
            customConfigurationActions: (optionsBuilder) =>
            {
                optionsBuilder.UseFreeTextExtensions();
            },
            customModelCreationActions: (modelBuilder) =>
            {
                modelBuilder.SharedTypeEntity<FreeText>(tableName)
                    .SetStemmingTable(stemmingTableName)
                    .Property<long>(nameof(FreeText.Id))
                    .HasColumnName("ROWID");

                if (databaseType == DatabaseTypes.Sqlite)
                {
                    modelBuilder.SharedTypeEntity<FreeText>(stemmingTableName)
                        .Property<long>(nameof(FreeText.Id))
                        .HasColumnName("ROWID");
                }
            });

        await DatabaseUtils.InitialiseFreeTextTablesAsync(context, tableName, stemmingTableName).ConfigureAwait(false);

        int freeTextCount = 1;

        FreeText freeText = new()
        {
            TextContent = "Apple"
        };

        await context.Set<FreeText>(tableName).AddAsync(freeText).ConfigureAwait(false);

        if (databaseType == DatabaseTypes.Sqlite)
        {
            await context.Set<FreeText>(stemmingTableName).AddAsync(freeText).ConfigureAwait(false);
        }

        await context.SaveChangesAsync().ConfigureAwait(false);

        // ACT
        IQueryable<FreeText> query = context.Set<FreeText>(tableName);

        if (useStemming.HasValue && languageTerm.HasValue)
        {
            query = query.Where(e => EF.Functions.FreeTextSearch(e.TextContent!, freeText.TextContent, useStemming!.Value, languageTerm!.Value));
        }
        else if (useStemming.HasValue)
        {
            query = query.Where(e => EF.Functions.FreeTextSearch(e.TextContent!, freeText.TextContent, useStemming.Value));
        }
        else if (languageTerm.HasValue)
        {
            query = query.Where(e => EF.Functions.FreeTextSearch(e.TextContent!, freeText.TextContent, languageTerm.Value));
        }
        else
        {
            query = query.Where(e => EF.Functions.FreeTextSearch(e.TextContent!, freeText.TextContent));
        }

        List<FreeText> rows = await query
            .ToListAsync()
            .ConfigureAwait(false);

        // ASSERT
        Assert.AreEqual(freeTextCount, rows.Count, "Invalid count");
        Assert.AreEqual(freeText.TextContent, rows[0].TextContent, "Unexpected field value");
    }

    #endregion public methods
}