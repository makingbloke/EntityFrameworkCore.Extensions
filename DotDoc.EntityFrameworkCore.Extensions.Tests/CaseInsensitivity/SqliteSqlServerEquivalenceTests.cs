// Copyright ©2021-2026 Mike King.
// This file is licensed to you under the MIT license.
// See the License.txt file in the solution root for more information.

using DotDoc.EntityFrameworkCore.Extensions.CaseInsensitivity;
using DotDoc.EntityFrameworkCore.Extensions.DatabaseType;
using DotDoc.EntityFrameworkCore.Extensions.Execute;
using DotDoc.EntityFrameworkCore.Extensions.Tests.Data;
using Microsoft.EntityFrameworkCore;
using System.Globalization;

namespace DotDoc.EntityFrameworkCore.Extensions.Tests.CaseInsensitivity;

/// <summary>
/// Tests that the nocase_* SQLite functions and the NOCASE collation registered by UseSqliteNoCaseReplacement process Unicode in the same way
/// as their SQL Server equivalents with the collation set by UseCaseInsensitiveCollation.
/// </summary>
/// <remarks>
/// Only the Unicode processing is compared (case, width, kana type, accents, ignorable characters and counting characters outside the BMP once).
/// The interface of each nocase_* function (argument order, NULL and empty arguments, results and errors) is that of the SQLite built-in function
/// it's based on, so it can differ from SQL Server's (nocase_instr with an empty search is 1, CHARINDEX is 0, for example).
/// That is tested against SQLite's built-in functions in <see cref="SqliteNoCaseFunctionsTests"/>.
/// Each test runs the SQLite SQL on SQLite and the equivalent SQL on SQL Server with the same arguments, and checks both give the expected result.
/// The SQL Server SQL names the collation explicitly, so it doesn't depend on the collation the test database was created with.
/// </remarks>
[TestClass]
public class SqliteSqlServerEquivalenceTests
{
    #region private constants

    /// <summary>
    /// The SQL Server collation set by UseCaseInsensitiveCollation.
    /// </summary>
    private const string SqlServerCollation = "Latin1_General_100_CI_AS_SC_UTF8";

    /// <summary>
    /// SQL Server SQL that escapes the LIKE wildcards (and the escape character <c>\</c>) in <c>{1}</c>, so it matches as plain text.
    /// </summary>
    private const string SqlServerEscapedLikeText = @"REPLACE(REPLACE(REPLACE(REPLACE({1}, N'\', N'\\'), N'%', N'\%'), N'_', N'\_'), N'[', N'\[')";

    #endregion private constants

    #region private fields

    /// <summary>
    /// The MSTest <see cref="TestContext"/> for the current test.
    /// </summary>
    private readonly TestContext _testContext;

    #endregion private fields

    #region public constructors

    /// <summary>
    /// Initializes a new instance of the <see cref="SqliteSqlServerEquivalenceTests"/> class.
    /// </summary>
    /// <param name="testContext">The MSTest test context.</param>
    public SqliteSqlServerEquivalenceTests(TestContext testContext)
    {
        this._testContext = testContext;
    }

    #endregion public constructors

    #region test lifecycle

    /// <summary>
    /// Creates the SQL Server test database, so the SQL-only tests have one to connect to.
    /// </summary>
    /// <param name="testContext">The MSTest test context.</param>
    /// <returns>A <see cref="Task"/> that represents the asynchronous operation.</returns>
    [ClassInitialize]
    public static async Task ClassInitializeAsync(TestContext testContext)
    {
        ArgumentNullException.ThrowIfNull(testContext);

        using Context context = await DatabaseUtils.OpenDatabaseAsync(
            testContext,
            DatabaseTypes.SqlServer,
            customModelCreationActions: modelBuilder => modelBuilder.UseCaseInsensitiveCollation(DatabaseTypes.SqlServer))
            .ConfigureAwait(false);
    }

    #endregion test lifecycle

    #region nocase collation tests

    /// <summary>
    /// Test comparisons with the NOCASE collation give the same results as SQL Server's collation.
    /// </summary>
    /// <param name="value1">The first value.</param>
    /// <param name="value2">The second value.</param>
    /// <param name="expected">-1 if <paramref name="value1"/> sorts first, 0 if they are equal, 1 if <paramref name="value2"/> sorts first.</param>
    /// <returns>A <see cref="Task"/> that represents the asynchronous operation.</returns>
    [TestMethod(DisplayName = "NOCASE comparisons match the SQL Server collation")]
    [DataRow("abc", "ABC", 0, DisplayName = "ASCII case is ignored")]
    [DataRow("ááá", "ÁÁÁ", 0, DisplayName = "Non-ASCII case is ignored")]
    [DataRow("æøå", "ÆØÅ", 0, DisplayName = "Nordic case is ignored")]
    [DataRow("ＡＢＣ", "abc", 0, DisplayName = "Width is ignored")]
    [DataRow("café", "café", 0, DisplayName = "Decomposed accent equals precomposed")]
    [DataRow("ááá", "AAA", 1, DisplayName = "Accents are not ignored")]
    [DataRow("abc", "abc  ", 0, DisplayName = "Trailing spaces are ignored")]
    [DataRow("abc  ", "abc", 0, DisplayName = "Trailing spaces are ignored either way round")]
    [DataRow("apple", "Éclair", -1, DisplayName = "Sort order ignores case and accents first")]
    [DataRow("Éclair", "Zebra", -1, DisplayName = "Non-ASCII letter sorts with its base letter")]
    [DataRow("b", "a", 1, DisplayName = "Greater")]
    public async Task SqliteSqlServerEquivalenceTests_008_Async(string value1, string value2, int? expected)
    {
        // ARRANGE
        const string sqliteSql = $"SELECT CASE WHEN {{0}} = {{1}} COLLATE {SqliteUnicodeFunctionNames.NoCaseCollation} THEN 0 WHEN {{0}} < {{1}} COLLATE {SqliteUnicodeFunctionNames.NoCaseCollation} THEN -1 WHEN {{0}} > {{1}} COLLATE {SqliteUnicodeFunctionNames.NoCaseCollation} THEN 1 END";
        const string sqlServerSql = $"SELECT CASE WHEN {{0}} COLLATE {SqlServerCollation} = {{1}} THEN 0 WHEN {{0}} COLLATE {SqlServerCollation} < {{1}} THEN -1 WHEN {{0}} COLLATE {SqlServerCollation} > {{1}} THEN 1 END";

        // ACT / ASSERT
        await this.AssertSameResultAsync(sqliteSql, sqlServerSql, expected, value1, value2).ConfigureAwait(false);
    }

    #endregion nocase collation tests

    #region lower and upper tests

    /// <summary>
    /// Test the replacement lower and upper give the same results as SQL Server's LOWER and UPPER.
    /// </summary>
    /// <param name="functionName">The function to call (the SQL Server function has the same name in upper case).</param>
    /// <param name="value">The value to convert.</param>
    /// <param name="expected">The expected result.</param>
    /// <returns>A <see cref="Task"/> that represents the asynchronous operation.</returns>
    [TestMethod(DisplayName = "lower and upper match LOWER and UPPER")]
    [DataRow(SqliteUnicodeFunctionNames.LowerFunction, "ÉCOLE", "école", DisplayName = "lower non-ASCII")]
    [DataRow(SqliteUnicodeFunctionNames.LowerFunction, "ABC", "abc", DisplayName = "lower ASCII")]
    [DataRow(SqliteUnicodeFunctionNames.LowerFunction, "ÆØÅ", "æøå", DisplayName = "lower Nordic")]
    [DataRow(SqliteUnicodeFunctionNames.LowerFunction, "ＡＢＣ", "ａｂｃ", DisplayName = "lower full width")]
    [DataRow(SqliteUnicodeFunctionNames.UpperFunction, "école", "ÉCOLE", DisplayName = "upper non-ASCII")]
    [DataRow(SqliteUnicodeFunctionNames.UpperFunction, "abc", "ABC", DisplayName = "upper ASCII")]
    [DataRow(SqliteUnicodeFunctionNames.UpperFunction, "æøå", "ÆØÅ", DisplayName = "upper Nordic")]
    [DataRow(SqliteUnicodeFunctionNames.UpperFunction, "ａｂｃ", "ＡＢＣ", DisplayName = "upper full width")]
    [DataRow(SqliteUnicodeFunctionNames.UpperFunction, "straße", "STRAßE", DisplayName = "upper keeps ß")]
    public async Task SqliteSqlServerEquivalenceTests_001_Async(string functionName, string value, string expected)
    {
        // ARRANGE
        string sqliteSql = $"SELECT {functionName}({{0}})";
        string sqlServerSql = $"SELECT {functionName.ToUpperInvariant()}({{0}} COLLATE {SqlServerCollation})";

        // ACT / ASSERT
        await this.AssertSameResultAsync(sqliteSql, sqlServerSql, expected, value).ConfigureAwait(false);
    }

    #endregion lower and upper tests

    #region nocase_endswith and nocase_startswith tests

    /// <summary>
    /// Test nocase_endswith and nocase_startswith give the same results as SQL Server's LIKE with the suffix or prefix escaped (as EF Core translates EndsWith and StartsWith).
    /// </summary>
    /// <param name="functionName">The function to call, without the nocase_ prefix.</param>
    /// <param name="value">The string to check.</param>
    /// <param name="text">The prefix or suffix.</param>
    /// <param name="expected">The expected result.</param>
    /// <returns>A <see cref="Task"/> that represents the asynchronous operation.</returns>
    [TestMethod(DisplayName = "nocase_endswith and nocase_startswith match LIKE")]
    [DataRow("endswith", "Body ÉCOLE", "école", true, DisplayName = "nocase_endswith non-ASCII case is ignored")]
    [DataRow("endswith", "école", "ECOLE", false, DisplayName = "nocase_endswith accents are not ignored")]
    [DataRow("endswith", "abc", "ab", false, DisplayName = "nocase_endswith no match")]
    [DataRow("endswith", "a_c", "_C", true, DisplayName = "nocase_endswith _ is a plain character")]
    [DataRow("endswith", "abc", "_", false, DisplayName = "nocase_endswith _ is not a wildcard")]
    [DataRow("endswith", "a😀", "😀", true, DisplayName = "nocase_endswith a character outside the BMP")]
    [DataRow("startswith", "École", "éc", true, DisplayName = "nocase_startswith non-ASCII case is ignored")]
    [DataRow("startswith", "école", "ec", false, DisplayName = "nocase_startswith accents are not ignored")]
    [DataRow("startswith", "ＡＢＣ", "ab", true, DisplayName = "nocase_startswith width is ignored")]
    [DataRow("startswith", "abc", "bc", false, DisplayName = "nocase_startswith no match")]
    [DataRow("startswith", "ab", "abc", false, DisplayName = "nocase_startswith prefix longer than value")]
    [DataRow("startswith", "50% off", "50%", true, DisplayName = "nocase_startswith % is a plain character")]
    [DataRow("startswith", "abc", "%", false, DisplayName = "nocase_startswith % is not a wildcard")]
    [DataRow("startswith", "abc", "[a]", false, DisplayName = "nocase_startswith [ is not a wildcard")]
    public async Task SqliteSqlServerEquivalenceTests_006_Async(string functionName, string value, string text, bool? expected)
    {
        // ARRANGE
        string sqliteSql = $"SELECT {SqliteUnicodeFunctionNames.NoCasePrefix}{functionName}({{0}}, {{1}})";
        string likePattern = functionName == "startswith"
            ? $"{SqlServerEscapedLikeText} + N'%'"
            : $"N'%' + {SqlServerEscapedLikeText}";

        string sqlServerSql = $@"SELECT CASE WHEN {{0}} COLLATE {SqlServerCollation} LIKE {likePattern} ESCAPE N'\' THEN CAST(1 AS bit) WHEN {{0}} COLLATE {SqlServerCollation} NOT LIKE {likePattern} ESCAPE N'\' THEN CAST(0 AS bit) END";

        // ACT / ASSERT
        await this.AssertSameResultAsync(sqliteSql, sqlServerSql, expected, value, text).ConfigureAwait(false);
    }

    #endregion nocase_endswith and nocase_startswith tests

    #region nocase_instr tests

    /// <summary>
    /// Test nocase_instr gives the same results as SQL Server's CHARINDEX.
    /// </summary>
    /// <param name="value">The string to search.</param>
    /// <param name="search">The string to find.</param>
    /// <param name="expected">The expected position.</param>
    /// <returns>A <see cref="Task"/> that represents the asynchronous operation.</returns>
    [TestMethod(DisplayName = "nocase_instr matches CHARINDEX")]
    [DataRow("Body ÉCOLE", "école", 6, DisplayName = "Non-ASCII case is ignored")]
    [DataRow("abc", "B", 2, DisplayName = "ASCII case is ignored")]
    [DataRow("ＡＢＣ", "b", 2, DisplayName = "Width is ignored")]
    [DataRow("カタカナ", "かな", 3, DisplayName = "Kana type is ignored")]
    [DataRow("école", "ecole", 0, DisplayName = "Accents are not ignored")]
    [DataRow("café", "é", 4, DisplayName = "Decomposed accent matches precomposed")]
    [DataRow("abc", "d", 0, DisplayName = "No match")]
    [DataRow("abcabc", "C", 3, DisplayName = "First match")]
    [DataRow("😀abc", "A", 2, DisplayName = "Position counts a character outside the BMP once")]
    [DataRow("é😀é😀ÉX", "x", 6, DisplayName = "Position after several non-ASCII characters")]
    public async Task SqliteSqlServerEquivalenceTests_002_Async(string value, string search, int? expected)
    {
        // ARRANGE
        const string sqliteSql = $"SELECT {SqliteUnicodeFunctionNames.NoCaseInstrFunction}({{0}}, {{1}})";
        const string sqlServerSql = $"SELECT CHARINDEX({{1}}, {{0}} COLLATE {SqlServerCollation})";

        // ACT / ASSERT
        await this.AssertSameResultAsync(sqliteSql, sqlServerSql, expected, value, search).ConfigureAwait(false);
    }

    #endregion nocase_instr tests

    #region nocase_like tests

    /// <summary>
    /// Test nocase_like gives the same results as SQL Server's LIKE.
    /// </summary>
    /// <param name="value">The value to match.</param>
    /// <param name="pattern">The LIKE pattern.</param>
    /// <param name="expected">The expected result.</param>
    /// <returns>A <see cref="Task"/> that represents the asynchronous operation.</returns>
    /// <remarks>Patterns with <c>[</c> aren't included, as it's a wildcard in SQL Server but not in SQLite.</remarks>
    [TestMethod(DisplayName = "nocase_like matches LIKE")]
    [DataRow("École", "éc%", true, DisplayName = "Non-ASCII case is ignored")]
    [DataRow("école", "%COLE", true, DisplayName = "Leading wildcard")]
    [DataRow("école", "ec%", false, DisplayName = "Accents are not ignored")]
    [DataRow("ＡＢＣ", "abc", true, DisplayName = "Width is ignored")]
    [DataRow("École", "_cole", true, DisplayName = "_ matches a non-ASCII character")]
    [DataRow("😀bc", "_bc", true, DisplayName = "_ matches a character outside the BMP")]
    [DataRow("😀bc", "__bc", false, DisplayName = "_ doesn't match half a character outside the BMP")]
    [DataRow("AXXBYYC", "a%b%c", true, DisplayName = "Several wildcards")]
    [DataRow("AXXBYY", "a%b%c", false, DisplayName = "Several wildcards, no match")]
    [DataRow("abc", "a%%c", true, DisplayName = "Consecutive wildcards")]
    [DataRow("ABC", "abc", true, DisplayName = "Whole value")]
    [DataRow("abcd", "abc", false, DisplayName = "Pattern must match the whole value")]
    [DataRow("ac", "a_c", false, DisplayName = "_ must match one character")]
    public async Task SqliteSqlServerEquivalenceTests_003_Async(string value, string pattern, bool? expected)
    {
        // ARRANGE
        const string sqliteSql = $"SELECT {SqliteUnicodeFunctionNames.NoCaseLikeFunction}({{1}}, {{0}})";
        const string sqlServerSql = $"SELECT CASE WHEN {{0}} COLLATE {SqlServerCollation} LIKE {{1}} THEN CAST(1 AS bit) WHEN {{0}} COLLATE {SqlServerCollation} NOT LIKE {{1}} THEN CAST(0 AS bit) END";

        // ACT / ASSERT
        await this.AssertSameResultAsync(sqliteSql, sqlServerSql, expected, value, pattern).ConfigureAwait(false);
    }

    /// <summary>
    /// Test nocase_like with an escape character gives the same results as SQL Server's LIKE.
    /// </summary>
    /// <param name="value">The value to match.</param>
    /// <param name="pattern">The LIKE pattern.</param>
    /// <param name="escape">The escape character.</param>
    /// <param name="expected">The expected result.</param>
    /// <returns>A <see cref="Task"/> that represents the asynchronous operation.</returns>
    [TestMethod(DisplayName = "nocase_like with an escape character matches LIKE")]
    [DataRow("10%", @"10\%", @"\", true, DisplayName = "Escaped % matches %")]
    [DataRow("105", @"10\%", @"\", false, DisplayName = "Escaped % is not a wildcard")]
    [DataRow("A_C", @"a\_c", @"\", true, DisplayName = "Escaped _ matches _")]
    [DataRow("abc", @"a\_c", @"\", false, DisplayName = "Escaped _ is not a wildcard")]
    [DataRow(@"A\C", @"a\\c", @"\", true, DisplayName = "Escaped escape character")]
    [DataRow("A[B]C", @"a\[b]c", @"\", true, DisplayName = "Escaped [")]
    [DataRow("École", @"\é%", @"\", true, DisplayName = "Escaped ordinary character ignores case")]
    public async Task SqliteSqlServerEquivalenceTests_004_Async(string value, string pattern, string escape, bool? expected)
    {
        // ARRANGE
        const string sqliteSql = $"SELECT {SqliteUnicodeFunctionNames.NoCaseLikeFunction}({{1}}, {{0}}, {{2}})";
        const string sqlServerSql = $"SELECT CASE WHEN {{0}} COLLATE {SqlServerCollation} LIKE {{1}} ESCAPE {{2}} THEN CAST(1 AS bit) WHEN {{0}} COLLATE {SqlServerCollation} NOT LIKE {{1}} ESCAPE {{2}} THEN CAST(0 AS bit) END";

        // ACT / ASSERT
        await this.AssertSameResultAsync(sqliteSql, sqlServerSql, expected, value, pattern, escape).ConfigureAwait(false);
    }

    #endregion nocase_like tests

    #region nocase_ltrim, nocase_rtrim and nocase_trim tests

    /// <summary>
    /// Test nocase_ltrim, nocase_rtrim and nocase_trim give the same results as SQL Server's LTRIM, RTRIM and TRIM with a set of characters.
    /// </summary>
    /// <param name="functionName">The function to call, without the nocase_ prefix (the SQL Server function has the same name in upper case).</param>
    /// <param name="value">The string to trim.</param>
    /// <param name="characters">The characters to remove.</param>
    /// <param name="expected">The expected result.</param>
    /// <returns>A <see cref="Task"/> that represents the asynchronous operation.</returns>
    [TestMethod(DisplayName = "nocase_ltrim, nocase_rtrim and nocase_trim match LTRIM, RTRIM and TRIM")]
    [DataRow("ltrim", "XxabcXx", "x", "abcXx", DisplayName = "nocase_ltrim")]
    [DataRow("rtrim", "XxabcXx", "x", "Xxabc", DisplayName = "nocase_rtrim")]
    [DataRow("trim", "ÉéabcéÉ", "é", "abc", DisplayName = "nocase_trim non-ASCII case is ignored")]
    [DataRow("trim", "xXabcXx", "x", "abc", DisplayName = "nocase_trim ASCII case is ignored")]
    [DataRow("trim", "ＡabcＡ", "a", "bc", DisplayName = "nocase_trim width is ignored")]
    [DataRow("trim", "éabcé", "e", "éabcé", DisplayName = "nocase_trim accents are not ignored")]
    [DataRow("trim", "ÉxabcxÉ", "xé", "abc", DisplayName = "nocase_trim several characters")]
    [DataRow("trim", "xaxbx", "x", "axb", DisplayName = "nocase_trim leaves characters in the middle")]
    [DataRow("trim", "aAa", "A", "", DisplayName = "nocase_trim everything")]
    [DataRow("trim", "😀a😀", "😀", "a", DisplayName = "nocase_trim characters outside the BMP")]
    public async Task SqliteSqlServerEquivalenceTests_007_Async(string functionName, string value, string characters, string expected)
    {
        // ARRANGE
        string sqliteSql = $"SELECT {SqliteUnicodeFunctionNames.NoCasePrefix}{functionName}({{0}}, {{1}})";
        string sqlServerSql = functionName == "trim"
            ? $"SELECT TRIM({{1}} FROM {{0}} COLLATE {SqlServerCollation})"
            : $"SELECT {functionName.ToUpperInvariant()}({{0}} COLLATE {SqlServerCollation}, {{1}})";

        // ACT / ASSERT
        await this.AssertSameResultAsync(sqliteSql, sqlServerSql, expected, value, characters).ConfigureAwait(false);
    }

    #endregion nocase_ltrim, nocase_rtrim and nocase_trim tests

    #region nocase_replace tests

    /// <summary>
    /// Test nocase_replace gives the same results as SQL Server's REPLACE.
    /// </summary>
    /// <param name="value">The string to search.</param>
    /// <param name="search">The string to replace.</param>
    /// <param name="replacement">The replacement string.</param>
    /// <param name="expected">The expected result.</param>
    /// <returns>A <see cref="Task"/> that represents the asynchronous operation.</returns>
    [TestMethod(DisplayName = "nocase_replace matches REPLACE")]
    [DataRow("École école ÉCOLE", "école", "x", "x x x", DisplayName = "Non-ASCII case is ignored")]
    [DataRow("aAa", "a", "bb", "bbbbbb", DisplayName = "ASCII case is ignored, replacement longer than match")]
    [DataRow("ＡＢＣ", "abc", "x", "x", DisplayName = "Width is ignored")]
    [DataRow("école", "ecole", "x", "école", DisplayName = "Accents are not ignored")]
    [DataRow("abc", "d", "x", "abc", DisplayName = "No match")]
    [DataRow("abc", "B", "", "ac", DisplayName = "Empty replacement")]
    [DataRow("café bar", "é", "x", "cafx bar", DisplayName = "Match a different length from the search")]
    [DataRow("😀a😀A", "a", "b", "😀b😀b", DisplayName = "Characters outside the BMP")]
    public async Task SqliteSqlServerEquivalenceTests_005_Async(string value, string search, string replacement, string expected)
    {
        // ARRANGE
        const string sqliteSql = $"SELECT {SqliteUnicodeFunctionNames.NoCaseReplaceFunction}({{0}}, {{1}}, {{2}})";
        const string sqlServerSql = $"SELECT REPLACE({{0}} COLLATE {SqlServerCollation}, {{1}}, {{2}})";

        // ACT / ASSERT
        await this.AssertSameResultAsync(sqliteSql, sqlServerSql, expected, value, search, replacement).ConfigureAwait(false);
    }

    #endregion nocase_replace tests

    #region linq tests

    /// <summary>
    /// Test string methods in a LINQ query give the same results on SQLite (with UseSqliteNoCaseReplacement) and SQL Server,
    /// both with UseCaseInsensitiveCollation.
    /// </summary>
    /// <param name="method">The method to call.</param>
    /// <param name="value">The value stored in the database.</param>
    /// <param name="argument">The argument to the method.</param>
    /// <param name="expected">The expected result (a count of matching rows for the methods used in a Where).</param>
    /// <returns>A <see cref="Task"/> that represents the asynchronous operation.</returns>
    [TestMethod(DisplayName = "LINQ string methods")]
    [DataRow("Contains", "Body ÉCOLE", "école", "1", DisplayName = "Contains non-ASCII case is ignored")]
    [DataRow("Contains", "école", "ecole", "0", DisplayName = "Contains accents are not ignored")]
    [DataRow("Contains", "ＡＢＣ", "b", "1", DisplayName = "Contains width is ignored")]
    [DataRow("Contains", "Body ÉCOLE", "", "1", DisplayName = "Contains empty search")]
    [DataRow("Contains", "Body ÉCOLE", null, "0", DisplayName = "Contains NULL search")]
    [DataRow("StartsWith", "École", "éc", "1", DisplayName = "StartsWith non-ASCII case is ignored")]
    [DataRow("StartsWith", "50% off", "50%", "1", DisplayName = "StartsWith % is a plain character")]
    [DataRow("StartsWith", "abc", "%", "0", DisplayName = "StartsWith % is not a wildcard")]
    [DataRow("StartsWith", "École", "", "1", DisplayName = "StartsWith empty search")]
    [DataRow("StartsWith", "École", null, "0", DisplayName = "StartsWith NULL search")]
    [DataRow("EndsWith", "Body ÉCOLE", "école", "1", DisplayName = "EndsWith non-ASCII case is ignored")]
    [DataRow("EndsWith", "a_c", "_C", "1", DisplayName = "EndsWith _ is a plain character")]
    [DataRow("EndsWith", "École", "", "1", DisplayName = "EndsWith empty search")]
    [DataRow("EndsWith", "École", null, "0", DisplayName = "EndsWith NULL search")]
    [DataRow("Equals", "ÁÁÁ", "ááá", "1", DisplayName = "== non-ASCII case is ignored")]
    [DataRow("Equals", "abc", "abc  ", "1", DisplayName = "== trailing spaces are ignored")]
    [DataRow("IndexOf", "Body ÉCOLE", "école", "5", DisplayName = "IndexOf non-ASCII case is ignored")]
    [DataRow("IndexOf", "😀abc", "A", "1", DisplayName = "IndexOf counts a character outside the BMP once")]
    [DataRow("IndexOf", "Body ÉCOLE", "", "0", DisplayName = "IndexOf empty search")]
    [DataRow("IndexOfStartIndex", "Body ÉCOLE", "école", "5", DisplayName = "IndexOf(string, int) non-ASCII case is ignored")]
    [DataRow("IndexOfStartIndex", "abc", "z", "-1", DisplayName = "IndexOf(string, int) no match")]
    [DataRow("Like", "Body ÉCOLE", "%école", "1", DisplayName = "EF.Functions.Like non-ASCII case is ignored")]
    [DataRow("Like", "école", "ec%", "0", DisplayName = "EF.Functions.Like accents are not ignored")]
    [DataRow("Replace", "École école", "ÉCOLE", "x x", DisplayName = "Replace non-ASCII case is ignored")]
    [DataRow("Replace", "abc", "", "abc", DisplayName = "Replace empty search")]
    [DataRow("TrimStart", "ÉéabcéÉ", null, "abcéÉ", DisplayName = "TrimStart(char) non-ASCII case is ignored")]
    [DataRow("TrimEnd", "ÉéabcéÉ", null, "Ééabc", DisplayName = "TrimEnd(char) non-ASCII case is ignored")]
    [DataRow("ToLower", "ÆØÅ", null, "æøå", DisplayName = "ToLower non-ASCII")]
    [DataRow("ToUpper", "straße", null, "STRAßE", DisplayName = "ToUpper keeps ß")]
    public async Task SqliteSqlServerEquivalenceTests_009_Async(string method, string value, string? argument, string expected)
    {
        // ARRANGE
        using Context sqliteContext = await DatabaseUtils.OpenDatabaseAsync(
            this._testContext,
            DatabaseTypes.Sqlite,
            customConfigurationActions: optionsBuilder => optionsBuilder.UseSqliteNoCaseReplacement(),
            customModelCreationActions: modelBuilder => modelBuilder.UseCaseInsensitiveCollation(DatabaseTypes.Sqlite))
            .ConfigureAwait(false);

        using Context sqlServerContext = await DatabaseUtils.OpenDatabaseAsync(
            this._testContext,
            DatabaseTypes.SqlServer,
            customModelCreationActions: modelBuilder => modelBuilder.UseCaseInsensitiveCollation(DatabaseTypes.SqlServer))
            .ConfigureAwait(false);

        await DatabaseUtils.CreateTestTableEntriesAsync(sqliteContext, value, 1, this._testContext.CancellationToken).ConfigureAwait(false);
        await DatabaseUtils.CreateTestTableEntriesAsync(sqlServerContext, value, 1, this._testContext.CancellationToken).ConfigureAwait(false);

        // ACT
        string? sqliteResult = await this.RunLinqMethodAsync(sqliteContext, method, argument).ConfigureAwait(false);
        string? sqlServerResult = await this.RunLinqMethodAsync(sqlServerContext, method, argument).ConfigureAwait(false);

        // ASSERT
        Assert.AreEqual(expected, sqliteResult, "Invalid SQLite result");
        Assert.AreEqual(expected, sqlServerResult, "Invalid SQL Server result");
    }

    #endregion linq tests

    #region private methods

    /// <summary>
    /// Runs SQL on SQLite (with UseSqliteNoCaseReplacement) and the equivalent SQL on SQL Server, and checks both give the expected result.
    /// </summary>
    /// <typeparam name="T">The type of the result.</typeparam>
    /// <param name="sqliteSql">The SQLite SQL, with <c>{0}</c>, <c>{1}</c>... where the arguments go.</param>
    /// <param name="sqlServerSql">The SQL Server SQL, with <c>{0}</c>, <c>{1}</c>... where the arguments go.</param>
    /// <param name="expected">The expected result.</param>
    /// <param name="arguments">The arguments, passed as parameters.</param>
    /// <returns>A <see cref="Task"/> that represents the asynchronous operation.</returns>
    private async Task AssertSameResultAsync<T>(string sqliteSql, string sqlServerSql, T expected, params string[] arguments)
    {
        T sqliteResult = await this.ExecuteSqlAsync<T>(DatabaseTypes.Sqlite, sqliteSql, arguments).ConfigureAwait(false);
        T sqlServerResult = await this.ExecuteSqlAsync<T>(DatabaseTypes.SqlServer, sqlServerSql, arguments).ConfigureAwait(false);

        Assert.AreEqual(expected, sqliteResult, "Invalid SQLite result");
        Assert.AreEqual(expected, sqlServerResult, "Invalid SQL Server result");
    }

    /// <summary>
    /// Runs a SQL query with a single scalar result, on SQLite with UseSqliteNoCaseReplacement or on SQL Server.
    /// </summary>
    /// <typeparam name="T">The type of the result.</typeparam>
    /// <param name="databaseType">The database type.</param>
    /// <param name="sql">The SQL, with <c>{0}</c>, <c>{1}</c>... where the arguments go.</param>
    /// <param name="arguments">The arguments, passed as parameters.</param>
    /// <returns>The result.</returns>
    private async Task<T> ExecuteSqlAsync<T>(string databaseType, string sql, params string[] arguments)
    {
        using Context context = await DatabaseUtils.OpenDatabaseAsync(
            this._testContext,
            databaseType,
            createNew: false,
            customConfigurationActions: optionsBuilder =>
            {
                if (databaseType == DatabaseTypes.Sqlite)
                {
                    optionsBuilder.UseSqliteNoCaseReplacement();
                }
            })
            .ConfigureAwait(false);

        T result = await context.Database.ExecuteScalarAsync<T>(sql, this._testContext.CancellationToken, arguments).ConfigureAwait(false);
        return result;
    }

    /// <summary>
    /// Runs a string method in a LINQ query on the single row in TestTable1.
    /// </summary>
    /// <param name="context">The database context.</param>
    /// <param name="method">The method to call.</param>
    /// <param name="argument">The argument to the method.</param>
    /// <returns>The result: a count of matching rows for the methods used in a Where, otherwise the method's result.</returns>
    private async Task<string?> RunLinqMethodAsync(Context context, string method, string? argument)
    {
        CancellationToken cancellationToken = this._testContext.CancellationToken;
        IQueryable<TestTable1> table = context.TestTable1;

#pragma warning disable CA1304, CA1311 // EF Core translates ToLower() / ToUpper() to lower / upper (LOWER / UPPER), but not the overloads that take a culture.
        string? result = method switch
        {
            "Contains" => (await table.CountAsync(e => e.TestField.Contains(argument!), cancellationToken).ConfigureAwait(false)).ToString(CultureInfo.InvariantCulture),
            "StartsWith" => (await table.CountAsync(e => e.TestField.StartsWith(argument!), cancellationToken).ConfigureAwait(false)).ToString(CultureInfo.InvariantCulture),
            "EndsWith" => (await table.CountAsync(e => e.TestField.EndsWith(argument!), cancellationToken).ConfigureAwait(false)).ToString(CultureInfo.InvariantCulture),
            "Equals" => (await table.CountAsync(e => e.TestField == argument, cancellationToken).ConfigureAwait(false)).ToString(CultureInfo.InvariantCulture),
            "IndexOf" => (await table.Select(e => e.TestField.IndexOf(argument!)).SingleAsync(cancellationToken).ConfigureAwait(false)).ToString(CultureInfo.InvariantCulture),
            "IndexOfStartIndex" => (await table.Select(e => e.TestField.IndexOf(argument!, 1)).SingleAsync(cancellationToken).ConfigureAwait(false)).ToString(CultureInfo.InvariantCulture),
            "Like" => (await table.CountAsync(e => EF.Functions.Like(e.TestField, argument!), cancellationToken).ConfigureAwait(false)).ToString(CultureInfo.InvariantCulture),
            "Replace" => await table.Select(e => e.TestField.Replace(argument!, "x")).SingleAsync(cancellationToken).ConfigureAwait(false),
            "TrimStart" => await table.Select(e => e.TestField.TrimStart('é')).SingleAsync(cancellationToken).ConfigureAwait(false),
            "TrimEnd" => await table.Select(e => e.TestField.TrimEnd('é')).SingleAsync(cancellationToken).ConfigureAwait(false),
            "ToLower" => await table.Select(e => e.TestField.ToLower()).SingleAsync(cancellationToken).ConfigureAwait(false),
            "ToUpper" => await table.Select(e => e.TestField.ToUpper()).SingleAsync(cancellationToken).ConfigureAwait(false),
            _ => throw new ArgumentOutOfRangeException(nameof(method), method, "Unknown method"),
        };
#pragma warning restore CA1304, CA1311

        return result;
    }

    #endregion private methods
}
