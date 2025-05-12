// Copyright ©2021-2025 Mike King.
// This file is licensed to you under the MIT license.
// See the License.txt file in the solution root for more information.

using DotDoc.EntityFrameworkCore.Extensions.DatabaseType;
using DotDoc.EntityFrameworkCore.Extensions.FreeTextSearchFunction;
using DotDoc.EntityFrameworkCore.Extensions.MatchFunction;
using DotDoc.EntityFrameworkCore.Extensions.Tests.Data;
using DotDoc.EntityFrameworkCore.Extensions.Tests.Utilities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DotDoc.EntityFrameworkCore.Extensions.Tests.MatchFunction;

/// <summary>
/// Tests for the MatchFunction extension.
/// </summary>
[TestClass]
public class MatchFunctionTests
{
    #region public methods

    /// <summary>
    /// Test UseMatchExtensions Guard Clause.
    /// </summary>
    [TestMethod("UseMatchExtensions Guard Clause")]
    public void Test_UseMatchExtensions_GuardClause()
    {
        // ARRANGE
        DbContextOptionsBuilder? optionsBuilder = null;

        // ACT / ASSERT
        ArgumentNullException e = Assert.ThrowsExactly<ArgumentNullException>(() => _ = optionsBuilder!.UseMatchExtensions(), "Missing exception");
        Assert.AreEqual(nameof(optionsBuilder), e.ParamName, "Invalid parameter name");
    }

    /// <summary>
    /// Test Match function.
    /// </summary>
    /// <returns>A task that represents the asynchronous test operation.</returns>
    [TestMethod("Match")]
    public async Task Test_MatchFunctionAsync()
    {
        // ARRANGE
        string tableName = "FreeText";

        using Context context = DatabaseUtils.CreateDatabase(
            databaseType: DatabaseTypes.Sqlite,
            customConfigurationActions: (optionsBuilder) =>
            {
                optionsBuilder.UseMatchExtensions();
            },
            customModelCreationActions: (modelBuilder) =>
            {
                modelBuilder.SharedTypeEntity<FreeText>(tableName)
                    .Property<long>(nameof(FreeText.Id))
                    .HasColumnName("ROWID");
            });

        DatabaseUtils.InitialiseFreeTextTables(context, tableName);

        int freeTextCount = 1;

        FreeText freeText = new()
        {
            Content = "Apple"
        };

        await context.Set<FreeText>(tableName).AddAsync(freeText).ConfigureAwait(false);
        await context.SaveChangesAsync().ConfigureAwait(false);

        // ACT
        List<FreeText> rows = await context.Set<FreeText>(tableName)
            .Where(e => EF.Functions.Match(freeText.Content, e.Content!))
            .ToListAsync()
            .ConfigureAwait(false);

        // ASSERT
        Assert.AreEqual(freeTextCount, rows.Count, "Invalid count");
        Assert.AreEqual(freeText.Content, rows[0].Content, "Unexpected field value");
    }

    #endregion public methods
}