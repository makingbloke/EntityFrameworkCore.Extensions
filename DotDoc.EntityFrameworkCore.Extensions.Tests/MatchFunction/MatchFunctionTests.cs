// Copyright ©2021-2025 Mike King.
// This file is licensed to you under the MIT license.
// See the License.txt file in the solution root for more information.

using DotDoc.EntityFrameworkCore.Extensions.DatabaseType;
using DotDoc.EntityFrameworkCore.Extensions.MatchFunction;
using DotDoc.EntityFrameworkCore.Extensions.Tests.Data;
using DotDoc.EntityFrameworkCore.Extensions.Tests.Utilities;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DotDoc.EntityFrameworkCore.Extensions.Tests.MatchFunction;

/// <summary>
/// Tests for the MatchFunction extension.
/// </summary>
[TestClass]
public class MatchFunctionTests
{
    #region private fields

    /// <summary>
    /// FreeText table name.
    /// </summary>
    private const string TableName = "TestFreeText";

    #endregion private fields

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
    /// <returns>A <see cref="Task"/> that represents the asynchronous operation.</returns>
    [TestMethod("Match")]
    public async Task Test_MatchFunctionAsync()
    {
        // ARRANGE
        using Context context = await CreateContextAsync();

        int count = 1;
        string value = "Apple";
        string searchValue = "Apple";

        await CreateFreeTextTableEntryAsync(context, value).ConfigureAwait(false);

        // ACT
        List<FreeText> rows = await context.Set<FreeText>(TableName)
            .Where(e => EF.Functions.Match(searchValue, e.FreeTextField!))
            .ToListAsync()
            .ConfigureAwait(false);

        // ASSERT
        Assert.AreEqual(count, rows.Count, "Invalid count");
        Assert.AreEqual(value, rows[0].FreeTextField, "Unexpected field value");
    }

    #endregion public methods

    #region private methods

    /// <summary>
    /// Create a database context.
    /// </summary>
    /// <returns>An instance of <see cref="Context"/> with the free text table configured.</returns>
    private static async Task<Context> CreateContextAsync()
    {
        Context context = DatabaseUtils.CreateDatabase(
            databaseType: DatabaseTypes.Sqlite,
            customConfigurationActions: (optionsBuilder) =>
            {
                optionsBuilder.UseMatchExtensions();
            },
            customModelCreationActions: (modelBuilder) =>
            {
                modelBuilder.SharedTypeEntity<FreeText>(TableName)
                    .Property<long>(nameof(FreeText.Id))
                    .HasColumnName("ROWID");
            });

        await DatabaseUtils.InitialiseFreeTextTablesAsync(context, TableName).ConfigureAwait(false);

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

        await context.SaveChangesAsync().ConfigureAwait(false);
    }

    #endregion private methods
}