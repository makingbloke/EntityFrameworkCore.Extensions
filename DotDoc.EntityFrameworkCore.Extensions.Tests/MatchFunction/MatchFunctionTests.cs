// Copyright ©2021-2026 Mike King.
// This file is licensed to you under the MIT license.
// See the License.txt file in the solution root for more information.

using DotDoc.EntityFrameworkCore.Extensions.DatabaseType;
using DotDoc.EntityFrameworkCore.Extensions.FreeTextSearchFunction;
using DotDoc.EntityFrameworkCore.Extensions.MatchFunction;
using DotDoc.EntityFrameworkCore.Extensions.Tests.Data;
using Microsoft.EntityFrameworkCore;

namespace DotDoc.EntityFrameworkCore.Extensions.Tests.MatchFunction;

/// <summary>
/// Tests for the MatchFunction extension.
/// </summary>
[TestClass]
public class MatchFunctionTests
{
    #region private fields

    /// <summary>
    /// The MSTest <see cref="TestContext"/> for the current test.
    /// </summary>
    private readonly TestContext _testContext;

    #endregion private fields

    #region public constructors

    /// <summary>
    /// Initializes a new instance of the <see cref="MatchFunctionTests"/> class.
    /// </summary>
    /// <param name="testContext">The MSTest test context.</param>
    public MatchFunctionTests(TestContext testContext)
    {
        this._testContext = testContext;
    }

    #endregion public constructors

    #region UseMatchExtensions tests

    /// <summary>
    /// Test UseMatchExtensions with a Null DbContextOptionsBuilder OptionsBuilder parameter.
    /// </summary>
    [TestMethod(DisplayName = "UseMatchExtensions with a Null DbContextOptionsBuilder OptionsBuilder parameter")]
    public void MatchFunctionTests_001()
    {
        // ARRANGE
        DbContextOptionsBuilder optionsBuilder = null!;

        // ACT / ASSERT
        Assert.ThrowsExactly<ArgumentNullException>(() => optionsBuilder.UseMatchExtensions(), "Unexpected exception");
    }

    /// <summary>
    /// Test UseMatchExtensions with an unsupported database type (only SQLite is supported).
    /// </summary>
    /// <returns>A <see cref="Task"/> that represents the asynchronous operation.</returns>
    [TestMethod(DisplayName = "UseMatchExtensions with an unsupported database type")]
    public async Task MatchFunctionTests_002_Async()
    {
        // ARRANGE / ACT / ASSERT
        await Assert.ThrowsExactlyAsync<UnsupportedDatabaseTypeException>(
            async () =>
            {
                using Context context = await DatabaseUtils.OpenDatabaseAsync(
                    DatabaseTypes.SqlServer,
                    customConfigurationActions: optionsBuilder => optionsBuilder.UseMatchExtensions())
                    .ConfigureAwait(false);
            },
            "Unexpected exception")
            .ConfigureAwait(false);
    }

    #endregion UseMatchExtensions tests

    #region Match tests

    /// <summary>
    /// Test Match.
    /// </summary>
    /// <returns>A <see cref="Task"/> that represents the asynchronous operation.</returns>
    [TestMethod(DisplayName = "Match")]
    public async Task MatchFunctionTests_003_Async()
    {
        // ARRANGE
        using Context context = await DatabaseUtils.OpenDatabaseAsync(
            DatabaseTypes.Sqlite,
            customConfigurationActions: optionsBuilder => optionsBuilder.UseMatchExtensions())
            .ConfigureAwait(false);

        int count = 1;
        string value = "Apple";
        string searchValue = "Apple";

        await DatabaseUtils.CreateTestFreeTextTableEntryAsync(context, value).ConfigureAwait(false);

        // ACT
        List<FreeText> rows = await context.Set<FreeText>(Context.TestFreeTextTableName)
            .Where(e => EF.Functions.Match(searchValue, e.FreeTextField!))
            .ToListAsync(this._testContext.CancellationToken)
            .ConfigureAwait(false);

        // ASSERT
        Assert.HasCount(count, rows, "Invalid count");
        Assert.AreEqual(value, rows[0].FreeTextField, "Unexpected field value");
    }

    /// <summary>
    /// Test Match called outside of a query.
    /// </summary>
    /// <remarks>The function can only be translated to SQL, so calling it directly (or after the query has switched to client evaluation) must throw.</remarks>
    [TestMethod(DisplayName = "Match called outside of a query")]
    public void MatchFunctionTests_004()
    {
        // ARRANGE
        DbFunctions dbFunctions = EF.Functions;

        // ACT / ASSERT
        Assert.ThrowsExactly<InvalidOperationException>(() => dbFunctions.Match("Apple", "Apple"), "Unexpected exception");
    }

    /// <summary>
    /// Test Match with a value that is not found.
    /// </summary>
    /// <returns>A <see cref="Task"/> that represents the asynchronous operation.</returns>
    [TestMethod(DisplayName = "Match with a value that is not found")]
    public async Task MatchFunctionTests_005_Async()
    {
        // ARRANGE
        using Context context = await DatabaseUtils.OpenDatabaseAsync(
            DatabaseTypes.Sqlite,
            customConfigurationActions: optionsBuilder => optionsBuilder.UseMatchExtensions())
            .ConfigureAwait(false);

        string value = "Apple";
        string searchValue = "Banana";

        await DatabaseUtils.CreateTestFreeTextTableEntryAsync(context, value).ConfigureAwait(false);

        // ACT
        List<FreeText> rows = await context.Set<FreeText>(Context.TestFreeTextTableName)
            .Where(e => EF.Functions.Match(searchValue, e.FreeTextField!))
            .ToListAsync(this._testContext.CancellationToken)
            .ConfigureAwait(false);

        // ASSERT
        Assert.IsEmpty(rows, "Invalid count");
    }

    /// <summary>
    /// Test Match, FreeTextSearch and EF Core's own method translations work together.
    /// </summary>
    /// <param name="matchFirst">If true UseMatchExtensions is called before UseFreeTextSearchExtensions, else after it.</param>
    /// <remarks>
    /// Both extensions add a translator, which must be added to EF Core's translators rather than replacing them, so that both
    /// functions and EF Core's own translations (such as Contains, Substring and Math.Abs) work, whichever extension is configured first.
    /// The test <see cref="Context"/> calls UseFreeTextSearchExtensions before the custom configuration actions, so "Match first"
    /// calls UseFreeTextSearchExtensions a second time, which also checks that configuring it twice is harmless.
    /// The SQL is generated without being run, so this test does not need a database.
    /// </remarks>
    [TestMethod(DisplayName = "Match, FreeTextSearch and EF Core's own method translations work together")]
    [DataRow(false, DisplayName = "FreeTextSearch first")]
    [DataRow(true, DisplayName = "Match first")]
    public void MatchFunctionTests_006(bool matchFirst)
    {
        // ARRANGE
        using Context context = new(
            DatabaseTypes.Sqlite,
            "Data Source=:memory:",
            optionsBuilder =>
            {
                optionsBuilder.UseMatchExtensions();

                if (matchFirst)
                {
                    optionsBuilder.UseFreeTextSearchExtensions();
                }
            });

        // ACT
        string matchSql = context.TestFreeText.Where(e => EF.Functions.Match("Apple", e.FreeTextField!)).ToQueryString();
        string freeTextSearchSql = context.TestFreeText.Where(e => EF.Functions.FreeTextSearch(e.FreeTextField!, "Apple")).ToQueryString();
        string containsSql = context.TestTable1.Where(e => e.TestField.Contains("Test")).ToQueryString();
        string substringSql = context.TestTable1.Where(e => e.TestField.Substring(0, 2) == "Te").ToQueryString();
        string absSql = context.TestTable1.Where(e => Math.Abs(e.Id) > 1).ToQueryString();

        // ASSERT
        Assert.Contains("Match('Apple',", matchSql, "Match not translated.");
        Assert.Contains("MATCH('Apple',", freeTextSearchSql, "FreeTextSearch not translated.");
        Assert.Contains("instr(", containsSql, "Contains not translated.");
        Assert.Contains("substr(", substringSql, "Substring not translated.");
        Assert.Contains("abs(", absSql, "Math.Abs not translated.");
    }

    #endregion Match tests
}