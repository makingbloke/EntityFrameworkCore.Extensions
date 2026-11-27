// Copyright ©2021-2026 Mike King.
// This file is licensed to you under the MIT license.
// See the License.txt file in the solution root for more information.

using DotDoc.EntityFrameworkCore.Extensions.CaseInsensitivity;
using DotDoc.EntityFrameworkCore.Extensions.DatabaseType;
using DotDoc.EntityFrameworkCore.Extensions.Execute;
using DotDoc.EntityFrameworkCore.Extensions.Tests.Data;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;

namespace DotDoc.EntityFrameworkCore.Extensions.Tests.CaseInsensitivity;

/// <summary>
/// Tests for the SQLite functions registered by UseSqliteNoCaseReplacement: the replacement lower and upper functions, and the nocase_* functions
/// nocase_endswith, nocase_instr, nocase_like, nocase_ltrim, nocase_replace, nocase_rtrim, nocase_startswith and nocase_trim.
/// </summary>
/// <remarks>
/// Each function must keep the interface of the SQLite built-in function it replaces or is based on (argument order, NULL and empty arguments,
/// results and errors), so it can be used in place of the built-in in the SQL EF Core generates for SQLite. Only the Unicode processing
/// follows SQL Server, which <see cref="SqliteSqlServerEquivalenceTests"/> checks.
/// The functions are called through SQL on a real SQLite connection, so the tests also check that they are registered,
/// that SQL NULL arguments and results are passed correctly, and that an exception in a function becomes a SQLite error.
/// </remarks>
[TestClass]
public class SqliteNoCaseFunctionsTests
{
    #region private fields

    /// <summary>
    /// The MSTest <see cref="TestContext"/> for the current test.
    /// </summary>
    private readonly TestContext _testContext;

    #endregion private fields

    #region public constructors

    /// <summary>
    /// Initializes a new instance of the <see cref="SqliteNoCaseFunctionsTests"/> class.
    /// </summary>
    /// <param name="testContext">The MSTest test context.</param>
    public SqliteNoCaseFunctionsTests(TestContext testContext)
    {
        this._testContext = testContext;
    }

    #endregion public constructors

    #region UseSqliteNoCaseReplacement tests

    /// <summary>
    /// Test UseSqliteNoCaseReplacement adds the nocase_* functions and leaves SQLite's built-in functions unchanged.
    /// </summary>
    /// <param name="builtInSql">The SQL calling SQLite's built-in function.</param>
    /// <param name="noCaseSql">The same SQL calling the nocase_* function.</param>
    /// <param name="expectedBuiltIn">The expected result from SQLite's built-in function.</param>
    /// <param name="expectedNoCase">The expected result from the nocase_* function.</param>
    /// <returns>A <see cref="Task"/> that represents the asynchronous operation.</returns>
    [TestMethod]
    [DataRow("SELECT instr('ÉCOLE', 'école')", $"SELECT {SqliteUnicodeFunctionNames.NoCaseInstrFunction}('ÉCOLE', 'école')", "0", "1", DisplayName = "NonAsciiCase nocase_instr")]
    [DataRow("SELECT 'École' LIKE 'éc%'", $"SELECT {SqliteUnicodeFunctionNames.NoCaseLikeFunction}('éc%', 'École')", "0", "1", DisplayName = "NonAsciiCase nocase_like")]
    [DataRow("SELECT 'École' LIKE 'éc!%' ESCAPE '!'", $"SELECT {SqliteUnicodeFunctionNames.NoCaseLikeFunction}('éc!%', 'École', '!')", "0", "0", DisplayName = "NonAsciiCase nocase_like with an escape character")]
    [DataRow("SELECT 'École%' LIKE 'éCOLE!%' ESCAPE '!'", $"SELECT {SqliteUnicodeFunctionNames.NoCaseLikeFunction}('éCOLE!%', 'École%', '!')", "0", "1", DisplayName = "NonAsciiCase nocase_like with an escaped wildcard")]
    [DataRow("SELECT ltrim('ÉéabcéÉ', 'é')", $"SELECT {SqliteUnicodeFunctionNames.NoCaseLTrimFunction}('ÉéabcéÉ', 'é')", "ÉéabcéÉ", "abcéÉ", DisplayName = "NonAsciiCase nocase_ltrim")]
    [DataRow("SELECT replace('ÉaA', 'é', 'x')", $"SELECT {SqliteUnicodeFunctionNames.NoCaseReplaceFunction}('ÉaA', 'é', 'x')", "ÉaA", "xaA", DisplayName = "NonAsciiCase nocase_replace")]
    [DataRow("SELECT rtrim('ÉéabcéÉ', 'é')", $"SELECT {SqliteUnicodeFunctionNames.NoCaseRTrimFunction}('ÉéabcéÉ', 'é')", "ÉéabcéÉ", "Ééabc", DisplayName = "NonAsciiCase nocase_rtrim")]
    [DataRow("SELECT trim('ÉéabcéÉ', 'é')", $"SELECT {SqliteUnicodeFunctionNames.NoCaseTrimFunction}('ÉéabcéÉ', 'é')", "ÉéabcéÉ", "abc", DisplayName = "NonAsciiCase nocase_trim")]
    public async Task UseSqliteNoCaseReplacement_NonAsciiCase_OnlyNoCaseFunctionsIgnoreIt(string builtInSql, string noCaseSql, string expectedBuiltIn, string expectedNoCase)
    {
        // ARRANGE

        // ACT
        string? builtInResult = await this.ExecuteSqlAsync<string?>(builtInSql, useNoCaseReplacement: true).ConfigureAwait(false);
        string? noCaseResult = await this.ExecuteSqlAsync<string?>(noCaseSql, useNoCaseReplacement: true).ConfigureAwait(false);

        // ASSERT
        Assert.AreEqual(expectedBuiltIn, builtInResult, "Invalid built-in result");
        Assert.AreEqual(expectedNoCase, noCaseResult, "Invalid nocase_* result");
    }

    /// <summary>
    /// Test the nocase_* functions have the same interface as SQLite's built-in functions: the same argument order, handling of NULL
    /// and empty arguments, result types and results, for text where case and Unicode processing make no difference.
    /// </summary>
    /// <param name="sql">
    /// The SQL to run, with <c>{0}</c> before each function name: replaced by nothing to call SQLite's built-in function
    /// and by <c>nocase_</c> to call the nocase_* function.
    /// </param>
    /// <returns>A <see cref="Task"/> that represents the asynchronous operation.</returns>
    [TestMethod]
    [DataRow("SELECT {0}instr('abc', 'c')", DisplayName = "NoCaseFunctions nocase_instr ASCII")]
    [DataRow("SELECT {0}instr('éèê-x', 'x')", DisplayName = "NoCaseFunctions nocase_instr after non-ASCII characters")]
    [DataRow("SELECT {0}instr('😀😀x', 'x')", DisplayName = "NoCaseFunctions nocase_instr after characters outside the BMP")]
    [DataRow("SELECT {0}instr('abc', '')", DisplayName = "NoCaseFunctions nocase_instr empty search")]
    [DataRow("SELECT {0}instr('', '')", DisplayName = "NoCaseFunctions nocase_instr empty search in empty value")]
    [DataRow("SELECT {0}instr('abc', 'z')", DisplayName = "NoCaseFunctions nocase_instr no match")]
    [DataRow("SELECT {0}instr(NULL, 'a')", DisplayName = "NoCaseFunctions nocase_instr NULL value")]
    [DataRow("SELECT {0}instr('a', NULL)", DisplayName = "NoCaseFunctions nocase_instr NULL search")]
    [DataRow("SELECT {0}instr(12345, 34)", DisplayName = "NoCaseFunctions nocase_instr numbers")]
    [DataRow("SELECT typeof({0}instr('abc', 'b'))", DisplayName = "NoCaseFunctions nocase_instr result type")]
    [DataRow("SELECT {0}like('a%', 'abc')", DisplayName = "NoCaseFunctions nocase_like match")]
    [DataRow("SELECT {0}like('a_d', 'abc')", DisplayName = "NoCaseFunctions nocase_like no match")]
    [DataRow("SELECT {0}like('abc', 'a%')", DisplayName = "NoCaseFunctions nocase_like value first doesn't match")]
    [DataRow("SELECT {0}like('', '')", DisplayName = "NoCaseFunctions nocase_like empty pattern and value")]
    [DataRow("SELECT {0}like('a', NULL)", DisplayName = "NoCaseFunctions nocase_like NULL value")]
    [DataRow("SELECT {0}like(NULL, 'a')", DisplayName = "NoCaseFunctions nocase_like NULL pattern")]
    [DataRow("SELECT NOT {0}like('a%', 'abc')", DisplayName = "NoCaseFunctions NOT nocase_like")]
    [DataRow("SELECT typeof({0}like('a%', 'abc'))", DisplayName = "NoCaseFunctions nocase_like result type")]
    [DataRow(@"SELECT {0}like('a\%', 'a%', '\')", DisplayName = "NoCaseFunctions nocase_like with an escape character, match")]
    [DataRow(@"SELECT {0}like('a\', 'a', '\')", DisplayName = "NoCaseFunctions nocase_like with an escape character, pattern ending with the escape character")]
    [DataRow("SELECT {0}like('a', 'a', NULL)", DisplayName = "NoCaseFunctions nocase_like with an escape character, NULL escape")]
    [DataRow(@"SELECT {0}like('a', NULL, '\')", DisplayName = "NoCaseFunctions nocase_like with an escape character, NULL value")]
    [DataRow("SELECT {0}ltrim('xabcx', 'x')", DisplayName = "NoCaseFunctions nocase_ltrim")]
    [DataRow("SELECT {0}ltrim(NULL, 'a')", DisplayName = "NoCaseFunctions nocase_ltrim NULL value")]
    [DataRow("SELECT {0}ltrim('a', NULL)", DisplayName = "NoCaseFunctions nocase_ltrim NULL characters")]
    [DataRow("SELECT {0}replace('abc', 'b', 'x')", DisplayName = "NoCaseFunctions nocase_replace")]
    [DataRow("SELECT {0}replace('abc', '', 'x')", DisplayName = "NoCaseFunctions nocase_replace empty search")]
    [DataRow("SELECT {0}replace('abc', '', NULL)", DisplayName = "NoCaseFunctions nocase_replace empty search, NULL replacement")]
    [DataRow("SELECT {0}replace('abc', 'b', NULL)", DisplayName = "NoCaseFunctions nocase_replace NULL replacement")]
    [DataRow("SELECT {0}replace('abc', 'z', NULL)", DisplayName = "NoCaseFunctions nocase_replace NULL replacement, no match")]
    [DataRow("SELECT {0}replace(NULL, 'a', 'x')", DisplayName = "NoCaseFunctions nocase_replace NULL value")]
    [DataRow("SELECT {0}replace('abc', NULL, 'x')", DisplayName = "NoCaseFunctions nocase_replace NULL search")]
    [DataRow("SELECT {0}replace(12321, 2, 9)", DisplayName = "NoCaseFunctions nocase_replace numbers")]
    [DataRow("SELECT {0}rtrim('xabcx', 'x')", DisplayName = "NoCaseFunctions nocase_rtrim")]
    [DataRow("SELECT {0}rtrim(NULL, 'a')", DisplayName = "NoCaseFunctions nocase_rtrim NULL value")]
    [DataRow("SELECT {0}rtrim('a', NULL)", DisplayName = "NoCaseFunctions nocase_rtrim NULL characters")]
    [DataRow("SELECT {0}trim('xabcx', 'x')", DisplayName = "NoCaseFunctions nocase_trim")]
    [DataRow("SELECT {0}trim('abc', '')", DisplayName = "NoCaseFunctions nocase_trim empty characters")]
    [DataRow("SELECT {0}trim('', 'x')", DisplayName = "NoCaseFunctions nocase_trim empty value")]
    [DataRow("SELECT {0}trim(NULL, 'a')", DisplayName = "NoCaseFunctions nocase_trim NULL value")]
    [DataRow("SELECT {0}trim('a', NULL)", DisplayName = "NoCaseFunctions nocase_trim NULL characters")]
    public async Task UseSqliteNoCaseReplacement_NoCaseFunctions_HaveBuiltInInterface(string sql)
    {
        // ARRANGE
        string builtInSql = string.Format(CultureInfo.InvariantCulture, sql, string.Empty);
        string noCaseSql = string.Format(CultureInfo.InvariantCulture, sql, SqliteUnicodeFunctionNames.NoCasePrefix);
        string? expected = await this.ExecuteSqlAsync<string?>(builtInSql, useNoCaseReplacement: true).ConfigureAwait(false);

        // ACT
        string? result = await this.ExecuteSqlAsync<string?>(noCaseSql, useNoCaseReplacement: true).ConfigureAwait(false);

        // ASSERT
        Assert.AreEqual(expected, result, "Result doesn't match the built-in function");
    }

    #endregion UseSqliteNoCaseReplacement tests

    #region lower and upper tests

    /// <summary>
    /// Test the replacement lower and upper functions convert non-ASCII letters (SQLite's built-in lower and upper only convert ASCII letters).
    /// </summary>
    /// <param name="functionName">The function to call.</param>
    /// <param name="value">The value to convert.</param>
    /// <param name="expected">The expected result.</param>
    /// <returns>A <see cref="Task"/> that represents the asynchronous operation.</returns>
    [TestMethod]
    [DataRow(SqliteUnicodeFunctionNames.LowerFunction, "ÉCOLE", "école", DisplayName = "Convert lower non-ASCII")]
    [DataRow(SqliteUnicodeFunctionNames.LowerFunction, "ABC", "abc", DisplayName = "Convert lower ASCII")]
    [DataRow(SqliteUnicodeFunctionNames.LowerFunction, "ÆØÅ", "æøå", DisplayName = "Convert lower Nordic")]
    [DataRow(SqliteUnicodeFunctionNames.LowerFunction, null, null, DisplayName = "Convert lower NULL")]
    [DataRow(SqliteUnicodeFunctionNames.UpperFunction, "école", "ÉCOLE", DisplayName = "Convert upper non-ASCII")]
    [DataRow(SqliteUnicodeFunctionNames.UpperFunction, "abc", "ABC", DisplayName = "Convert upper ASCII")]
    [DataRow(SqliteUnicodeFunctionNames.UpperFunction, "æøå", "ÆØÅ", DisplayName = "Convert upper Nordic")]
    [DataRow(SqliteUnicodeFunctionNames.UpperFunction, "straße", "STRAßE", DisplayName = "Convert upper keeps the length (ß is unchanged)")]
    [DataRow(SqliteUnicodeFunctionNames.UpperFunction, null, null, DisplayName = "Convert upper NULL")]
    public async Task LowerAndUpper_Convert_ConvertsUnicodeLetters(string functionName, string? value, string? expected)
    {
        // ARRANGE

        // ACT
        string? result = await this.ExecuteFunctionAsync<string?>(functionName, value).ConfigureAwait(false);

        // ASSERT
        Assert.AreEqual(expected, result, "Invalid result");
    }

    /// <summary>
    /// Test ToLower and ToUpper in a LINQ query use the replacement lower and upper functions.
    /// </summary>
    /// <returns>A <see cref="Task"/> that represents the asynchronous operation.</returns>
    /// <remarks>
    /// The calls are in a Where, which EF Core can't evaluate on the client, so the test can't pass by .NET doing the conversion.
    /// The TestField column has no collation, so the comparisons are exact.
    /// </remarks>
    [TestMethod]
    [SuppressMessage("Globalization", "CA1304:Specify CultureInfo", Justification = "EF Core translates ToLower() / ToUpper() to lower / upper, but not the overloads that take a culture.")]
    [SuppressMessage("Globalization", "CA1311:Specify a culture or use an invariant version", Justification = "EF Core translates ToLower() / ToUpper() to lower / upper, but not the overloads that take a culture.")]
    [SuppressMessage("Performance", "CA1862:Use the 'StringComparison' method overloads to perform case-insensitive string comparisons", Justification = "The test checks EF Core's translation of ToLower() / ToUpper(); EF Core doesn't translate string.Equals with a StringComparison.")]
    public async Task LowerAndUpper_LinqToLowerAndToUpper_UseReplacements()
    {
        // ARRANGE
        using Context context = await DatabaseUtils.OpenDatabaseAsync(
            this._testContext,
            DatabaseTypes.Sqlite,
            customConfigurationActions: optionsBuilder => optionsBuilder.UseSqliteNoCaseReplacement())
            .ConfigureAwait(false);

        await DatabaseUtils.CreateTestTableEntriesAsync(context, "Éclair", 1, this._testContext.CancellationToken).ConfigureAwait(false);

        // ACT
        int lowerCount = await context.TestTable1
            .CountAsync(e => e.TestField.ToLower() == "éclair", this._testContext.CancellationToken)
            .ConfigureAwait(false);

        int upperCount = await context.TestTable1
            .CountAsync(e => e.TestField.ToUpper() == "ÉCLAIR", this._testContext.CancellationToken)
            .ConfigureAwait(false);

        // ASSERT
        Assert.AreEqual(1, lowerCount, "Invalid lower case count");
        Assert.AreEqual(1, upperCount, "Invalid upper case count");
    }

    /// <summary>
    /// Test the replacement lower and upper functions have the same interface as SQLite's built-in functions: the same handling of NULL,
    /// the same result type, and numbers converted to text.
    /// </summary>
    /// <param name="sql">The SQL to run.</param>
    /// <param name="expected">The expected result (the result of SQLite's built-in function).</param>
    /// <returns>A <see cref="Task"/> that represents the asynchronous operation.</returns>
    /// <remarks>
    /// The built-in functions can't be called on the same connection for comparison, as the replacement takes their place,
    /// so the expected results are those of the built-in functions.
    /// </remarks>
    [TestMethod]
    [DataRow($"SELECT {SqliteUnicodeFunctionNames.LowerFunction}(NULL)", null, DisplayName = "Replacements lower NULL")]
    [DataRow($"SELECT typeof({SqliteUnicodeFunctionNames.LowerFunction}('abc'))", "text", DisplayName = "Replacements lower result type")]
    [DataRow($"SELECT {SqliteUnicodeFunctionNames.LowerFunction}(12)", "12", DisplayName = "Replacements lower number")]
    [DataRow($"SELECT {SqliteUnicodeFunctionNames.UpperFunction}(NULL)", null, DisplayName = "Replacements upper NULL")]
    [DataRow($"SELECT typeof({SqliteUnicodeFunctionNames.UpperFunction}('abc'))", "text", DisplayName = "Replacements upper result type")]
    public async Task LowerAndUpper_Replacements_HaveBuiltInInterface(string sql, string? expected)
    {
        // ARRANGE

        // ACT
        string? result = await this.ExecuteSqlAsync<string?>(sql, useNoCaseReplacement: true).ConfigureAwait(false);

        // ASSERT
        Assert.AreEqual(expected, result, "Result doesn't match the built-in function");
    }

    #endregion lower and upper tests

    #region nocase_endswith and nocase_startswith tests

    /// <summary>
    /// Test the nocase_endswith and nocase_startswith functions.
    /// </summary>
    /// <param name="functionName">The function to call.</param>
    /// <param name="value">The string to check.</param>
    /// <param name="text">The prefix or suffix.</param>
    /// <param name="expected">The expected result.</param>
    /// <returns>A <see cref="Task"/> that represents the asynchronous operation.</returns>
    [TestMethod]
    [DataRow(SqliteUnicodeFunctionNames.NoCaseEndsWithFunction, "Body ÉCOLE", "école", true, DisplayName = "Search nocase_endswith non-ASCII case is ignored")]
    [DataRow(SqliteUnicodeFunctionNames.NoCaseEndsWithFunction, "école", "ECOLE", false, DisplayName = "Search nocase_endswith accents are not ignored")]
    [DataRow(SqliteUnicodeFunctionNames.NoCaseEndsWithFunction, "abc", "ab", false, DisplayName = "Search nocase_endswith no match")]
    [DataRow(SqliteUnicodeFunctionNames.NoCaseEndsWithFunction, "abc", "", true, DisplayName = "Search nocase_endswith empty suffix")]
    [DataRow(SqliteUnicodeFunctionNames.NoCaseEndsWithFunction, "a_c", "_C", true, DisplayName = "Search nocase_endswith _ is a plain character")]
    [DataRow(SqliteUnicodeFunctionNames.NoCaseEndsWithFunction, "abc", "_", false, DisplayName = "Search nocase_endswith _ is not a wildcard")]
    [DataRow(SqliteUnicodeFunctionNames.NoCaseEndsWithFunction, null, "a", null, DisplayName = "Search nocase_endswith NULL value")]
    [DataRow(SqliteUnicodeFunctionNames.NoCaseEndsWithFunction, "a", null, null, DisplayName = "Search nocase_endswith NULL suffix")]
    [DataRow(SqliteUnicodeFunctionNames.NoCaseStartsWithFunction, "École", "éc", true, DisplayName = "Search nocase_startswith non-ASCII case is ignored")]
    [DataRow(SqliteUnicodeFunctionNames.NoCaseStartsWithFunction, "école", "ec", false, DisplayName = "Search nocase_startswith accents are not ignored")]
    [DataRow(SqliteUnicodeFunctionNames.NoCaseStartsWithFunction, "abc", "bc", false, DisplayName = "Search nocase_startswith no match")]
    [DataRow(SqliteUnicodeFunctionNames.NoCaseStartsWithFunction, "ab", "abc", false, DisplayName = "Search nocase_startswith prefix longer than value")]
    [DataRow(SqliteUnicodeFunctionNames.NoCaseStartsWithFunction, "abc", "", true, DisplayName = "Search nocase_startswith empty prefix")]
    [DataRow(SqliteUnicodeFunctionNames.NoCaseStartsWithFunction, "50% off", "50%", true, DisplayName = "Search nocase_startswith % is a plain character")]
    [DataRow(SqliteUnicodeFunctionNames.NoCaseStartsWithFunction, "abc", "%", false, DisplayName = "Search nocase_startswith % is not a wildcard")]
    [DataRow(SqliteUnicodeFunctionNames.NoCaseStartsWithFunction, null, "a", null, DisplayName = "Search nocase_startswith NULL value")]
    [DataRow(SqliteUnicodeFunctionNames.NoCaseStartsWithFunction, "a", null, null, DisplayName = "Search nocase_startswith NULL prefix")]
    public async Task NoCaseEndsWithAndStartsWith_Search_MatchesIgnoringCase(string functionName, string? value, string? text, bool? expected)
    {
        // ARRANGE

        // ACT
        bool? result = await this.ExecuteFunctionAsync<bool?>(functionName, value, text).ConfigureAwait(false);

        // ASSERT
        Assert.AreEqual(expected, result, "Invalid result");
    }

    #endregion nocase_endswith and nocase_startswith tests

    #region nocase_instr tests

    /// <summary>
    /// Test the nocase_instr function.
    /// </summary>
    /// <param name="value">The string to search.</param>
    /// <param name="search">The string to find.</param>
    /// <param name="expected">The expected position.</param>
    /// <returns>A <see cref="Task"/> that represents the asynchronous operation.</returns>
    [TestMethod]
    [DataRow("Body ÉCOLE", "école", 6, DisplayName = "Search Non-ASCII case is ignored")]
    [DataRow("abc", "B", 2, DisplayName = "Search ASCII case is ignored")]
    [DataRow("ＡＢＣ", "b", 2, DisplayName = "Search Width is ignored")]
    [DataRow("école", "ecole", 0, DisplayName = "Search Accents are not ignored")]
    [DataRow("abc", "d", 0, DisplayName = "Search No match")]
    [DataRow("abcabc", "C", 3, DisplayName = "Search First match")]
    [DataRow("abc", "", 1, DisplayName = "Search Empty search matches at 1")]
    [DataRow("", "", 1, DisplayName = "Search Empty search in empty value matches at 1")]
    [DataRow("😀abc", "A", 2, DisplayName = "Search Position counts a character outside the BMP once")]
    [DataRow("é😀é😀ÉX", "x", 6, DisplayName = "Search Position after several non-ASCII characters")]
    [DataRow(null, "a", null, DisplayName = "Search NULL value")]
    [DataRow("a", null, null, DisplayName = "Search NULL search")]
    public async Task NoCaseInstr_Search_ReturnsPositionIgnoringCase(string? value, string? search, int? expected)
    {
        // ARRANGE

        // ACT
        int? result = await this.ExecuteFunctionAsync<int?>(SqliteUnicodeFunctionNames.NoCaseInstrFunction, value, search).ConfigureAwait(false);

        // ASSERT
        Assert.AreEqual(expected, result, "Invalid position");
    }

    #endregion nocase_instr tests

    #region nocase_like tests

    /// <summary>
    /// Test the nocase_like function with no escape character.
    /// </summary>
    /// <param name="pattern">The LIKE pattern.</param>
    /// <param name="value">The value to match.</param>
    /// <param name="expected">The expected result.</param>
    /// <returns>A <see cref="Task"/> that represents the asynchronous operation.</returns>
    [TestMethod]
    [DataRow("éc%", "École", true, DisplayName = "Pattern Non-ASCII case is ignored")]
    [DataRow("%COLE", "école", true, DisplayName = "Pattern Leading wildcard")]
    [DataRow("ec%", "école", false, DisplayName = "Pattern Accents are not ignored")]
    [DataRow("_cole", "École", true, DisplayName = "Pattern _ matches a non-ASCII character")]
    [DataRow("_bc", "😀bc", true, DisplayName = "Pattern _ matches a character outside the BMP")]
    [DataRow("__bc", "😀bc", false, DisplayName = "Pattern _ doesn't match half a character outside the BMP")]
    [DataRow("a%b%c", "AXXBYYC", true, DisplayName = "Pattern Several wildcards")]
    [DataRow("a%b%c", "AXXBYY", false, DisplayName = "Pattern Several wildcards, no match")]
    [DataRow("a%%c", "abc", true, DisplayName = "Pattern Consecutive wildcards")]
    [DataRow("abc", "ABC", true, DisplayName = "Pattern Whole value")]
    [DataRow("abc", "abcd", false, DisplayName = "Pattern must match the whole value")]
    [DataRow("a_c", "ac", false, DisplayName = "Pattern _ must match one character")]
    [DataRow("%", "", true, DisplayName = "Pattern % matches an empty value")]
    [DataRow("", "", true, DisplayName = "Pattern Empty pattern matches an empty value")]
    [DataRow("", "a", false, DisplayName = "Pattern Empty pattern doesn't match a value")]
    [DataRow("a[b]c", "A[B]C", true, DisplayName = "Pattern [ is not a wildcard")]
    [DataRow(null, "a", null, DisplayName = "Pattern NULL pattern")]
    [DataRow("a", null, null, DisplayName = "Pattern NULL value")]
    public async Task NoCaseLike_Pattern_MatchesIgnoringCase(string? pattern, string? value, bool? expected)
    {
        // ARRANGE

        // ACT
        bool? result = await this.ExecuteSqlAsync<bool?>($"SELECT {SqliteUnicodeFunctionNames.NoCaseLikeFunction}({{0}}, {{1}})", useNoCaseReplacement: true, pattern, value).ConfigureAwait(false);

        // ASSERT
        Assert.AreEqual(expected, result, "Invalid result");
    }

    /// <summary>
    /// Test the nocase_like function with an escape character.
    /// </summary>
    /// <param name="pattern">The LIKE pattern.</param>
    /// <param name="value">The value to match.</param>
    /// <param name="escape">The escape character.</param>
    /// <param name="expected">The expected result.</param>
    /// <returns>A <see cref="Task"/> that represents the asynchronous operation.</returns>
    [TestMethod]
    [DataRow(@"10\%", "10%", @"\", true, DisplayName = "EscapeCharacter Escaped % matches %")]
    [DataRow(@"10\%", "105", @"\", false, DisplayName = "EscapeCharacter Escaped % is not a wildcard")]
    [DataRow(@"a\_c", "A_C", @"\", true, DisplayName = "EscapeCharacter Escaped _ matches _")]
    [DataRow(@"a\_c", "abc", @"\", false, DisplayName = "EscapeCharacter Escaped _ is not a wildcard")]
    [DataRow(@"a\\c", @"A\C", @"\", true, DisplayName = "EscapeCharacter Escaped escape character")]
    [DataRow(@"\é%", "École", @"\", true, DisplayName = "EscapeCharacter Escaped ordinary character ignores case")]
    [DataRow(@"a\", "a", @"\", false, DisplayName = "EscapeCharacter Pattern ending with the escape character never matches")]
    [DataRow("a%%", "a%", "%", true, DisplayName = "EscapeCharacter % as the escape character")]
    [DataRow("a%%", "ab", "%", false, DisplayName = "EscapeCharacter % as the escape character is not a wildcard")]
    [DataRow("😀%_", "%x", "😀", true, DisplayName = "EscapeCharacter Escape character outside the BMP")]
    [DataRow("%a!_b%", "xxA_Bxx", "!", true, DisplayName = "EscapeCharacter Escaped _ after a % that has to absorb characters")]
    [DataRow("%a!_b%", "xxaxbxx", "!", false, DisplayName = "EscapeCharacter Escaped _ after a % is not a wildcard")]
    [DataRow("%!%", "50%", "!", true, DisplayName = "EscapeCharacter Escaped % at the end, after a %")]
    [DataRow("%!%", "50", "!", false, DisplayName = "EscapeCharacter Escaped % at the end, after a %, no match")]
    [DataRow("a!%b%c_", "A%BxyCz", "!", true, DisplayName = "EscapeCharacter Escaped and unescaped wildcards")]
    [DataRow("%a!%", "a%a%", "!", true, DisplayName = "EscapeCharacter Escaped literal at the end, tried at every position")]
    [DataRow("a%", "a%", "!", true, DisplayName = "EscapeCharacter Escape character not in the pattern")]
    [DataRow(null, "a", @"\", null, DisplayName = "EscapeCharacter NULL pattern")]
    [DataRow("a", null, @"\", null, DisplayName = "EscapeCharacter NULL value")]
    [DataRow("a", "a", null, null, DisplayName = "EscapeCharacter NULL escape")]
    public async Task NoCaseLike_EscapeCharacter_MatchesIgnoringCase(string? pattern, string? value, string? escape, bool? expected)
    {
        // ARRANGE

        // ACT
        bool? result = await this.ExecuteSqlAsync<bool?>($"SELECT {SqliteUnicodeFunctionNames.NoCaseLikeFunction}({{0}}, {{1}}, {{2}})", useNoCaseReplacement: true, pattern, value, escape).ConfigureAwait(false);

        // ASSERT
        Assert.AreEqual(expected, result, "Invalid result");
    }

    /// <summary>
    /// Test the nocase_like function with an escape that isn't a single character.
    /// </summary>
    /// <param name="pattern">The LIKE pattern.</param>
    /// <param name="escape">The escape character.</param>
    /// <returns>A <see cref="Task"/> that represents the asynchronous operation.</returns>
    [TestMethod]
    [DataRow("a", "ab", DisplayName = "InvalidEscape Two characters")]
    [DataRow("a", "", DisplayName = "InvalidEscape Empty")]
    [DataRow(null, "ab", DisplayName = "InvalidEscape Two characters with a NULL pattern")]
    public async Task NoCaseLike_InvalidEscape_ThrowsSqliteException(string? pattern, string escape)
    {
        // ARRANGE

        // ACT / ASSERT
        SqliteException exception = await Assert.ThrowsExactlyAsync<SqliteException>(
            () => this.ExecuteSqlAsync<bool?>($"SELECT {SqliteUnicodeFunctionNames.NoCaseLikeFunction}({{0}}, 'a', {{1}})", useNoCaseReplacement: true, pattern, escape),
            "Unexpected exception")
            .ConfigureAwait(false);

        Assert.Contains("ESCAPE expression must be a single character", exception.Message, "Invalid exception message");
    }

    /// <summary>
    /// Test the nocase_like function with a pattern with many wildcards that doesn't match.
    /// </summary>
    /// <returns>A <see cref="Task"/> that represents the asynchronous operation.</returns>
    /// <remarks>A matcher that tried every way of splitting the value between the wildcards would take exponential time.</remarks>
    [TestMethod]
    [Timeout(10000, CooperativeCancellation = true)]
    public async Task NoCaseLike_ManyWildcardsNoMatch_ReturnsFalse()
    {
        // ARRANGE
        string pattern = string.Concat(Enumerable.Repeat("%a", 20)) + "%b";
        string value = new('a', 200);

        // ACT
        bool? result = await this.ExecuteSqlAsync<bool?>($"SELECT {SqliteUnicodeFunctionNames.NoCaseLikeFunction}({{0}}, {{1}})", useNoCaseReplacement: true, pattern, value).ConfigureAwait(false);

        // ASSERT
        Assert.IsFalse(result, "Invalid result");
    }

    /// <summary>
    /// Test the nocase_like function with the longest pattern SQLite allows (50,000 UTF-8 bytes) matches, rather than overflowing the stack.
    /// </summary>
    /// <returns>A <see cref="Task"/> that represents the asynchronous operation.</returns>
    [TestMethod]
    [Timeout(10000, CooperativeCancellation = true)]
    public async Task NoCaseLike_LongestAllowedPattern_Matches()
    {
        // ARRANGE
        string pattern = new('_', 50_000);
        string value = new('a', 50_000);

        // ACT
        bool? result = await this.ExecuteSqlAsync<bool?>($"SELECT {SqliteUnicodeFunctionNames.NoCaseLikeFunction}({{0}}, {{1}})", useNoCaseReplacement: true, pattern, value).ConfigureAwait(false);

        // ASSERT
        Assert.IsTrue(result, "Invalid result");
    }

    /// <summary>
    /// Test the nocase_like function with a pattern longer than SQLite allows (50,000 UTF-8 bytes) raises the same error as SQLite,
    /// which checks the length before anything else.
    /// </summary>
    /// <param name="sql">The SQL to run, with <c>{0}</c> where the pattern goes.</param>
    /// <param name="patternCharacter">The character the pattern is made of.</param>
    /// <param name="patternLength">The number of characters in the pattern.</param>
    /// <returns>A <see cref="Task"/> that represents the asynchronous operation.</returns>
    [TestMethod]
    [DataRow($"SELECT {SqliteUnicodeFunctionNames.NoCaseLikeFunction}({{0}}, 'a')", '_', 50_001, DisplayName = "PatternTooLong 50,001 ASCII characters")]
    [DataRow($"SELECT {SqliteUnicodeFunctionNames.NoCaseLikeFunction}({{0}}, 'a')", 'é', 25_001, DisplayName = "PatternTooLong 25,001 two-byte characters (50,002 bytes)")]
    [DataRow($"SELECT {SqliteUnicodeFunctionNames.NoCaseLikeFunction}({{0}}, 'a')", '€', 16_667, DisplayName = "PatternTooLong 16,667 three-byte characters (50,001 bytes)")]
    [DataRow($"SELECT {SqliteUnicodeFunctionNames.NoCaseLikeFunction}({{0}}, NULL)", '_', 50_001, DisplayName = "PatternTooLong NULL value")]
    [DataRow($"SELECT {SqliteUnicodeFunctionNames.NoCaseLikeFunction}({{0}}, 'a', NULL)", '_', 50_001, DisplayName = "PatternTooLong NULL escape")]
    [DataRow($"SELECT {SqliteUnicodeFunctionNames.NoCaseLikeFunction}({{0}}, 'a', 'ab')", '_', 50_001, DisplayName = "PatternTooLong Invalid escape")]
    public async Task NoCaseLike_PatternTooLong_ThrowsSqliteException(string sql, char patternCharacter, int patternLength)
    {
        // ARRANGE
        string pattern = new(patternCharacter, patternLength);

        // ACT / ASSERT
        SqliteException exception = await Assert.ThrowsExactlyAsync<SqliteException>(
            () => this.ExecuteSqlAsync<bool?>(sql, useNoCaseReplacement: true, pattern),
            "Unexpected exception")
            .ConfigureAwait(false);

        Assert.Contains("LIKE or GLOB pattern too complex", exception.Message, "Invalid exception message");
    }

    /// <summary>
    /// Test the nocase_like function accepts a pattern of exactly 50,000 UTF-8 bytes made of two-byte characters (the limit is in bytes, not characters).
    /// </summary>
    /// <returns>A <see cref="Task"/> that represents the asynchronous operation.</returns>
    [TestMethod]
    public async Task NoCaseLike_LongestAllowedTwoBytePattern_IsAccepted()
    {
        // ARRANGE
        string pattern = new('é', 25_000);

        // ACT
        bool? result = await this.ExecuteSqlAsync<bool?>($"SELECT {SqliteUnicodeFunctionNames.NoCaseLikeFunction}({{0}}, 'a')", useNoCaseReplacement: true, pattern).ConfigureAwait(false);

        // ASSERT
        Assert.IsFalse(result, "Invalid result");
    }

    /// <summary>
    /// Test the nocase_like function takes the pattern first, as SQLite's like function does (like(pattern, value) is the same as value LIKE pattern).
    /// </summary>
    /// <param name="sql">The SQL to run.</param>
    /// <param name="expected">The expected result.</param>
    /// <returns>A <see cref="Task"/> that represents the asynchronous operation.</returns>
    [TestMethod]
    [DataRow($"SELECT {SqliteUnicodeFunctionNames.NoCaseLikeFunction}('éc%', 'École')", true, DisplayName = "ArgumentOrder Pattern first")]
    [DataRow($"SELECT {SqliteUnicodeFunctionNames.NoCaseLikeFunction}('École', 'éc%')", false, DisplayName = "ArgumentOrder Value first doesn't match")]
    [DataRow($"SELECT {SqliteUnicodeFunctionNames.NoCaseLikeFunction}('éc!%', 'Éc%', '!')", true, DisplayName = "ArgumentOrder Pattern first, with an escape character")]
    public async Task NoCaseLike_ArgumentOrder_TakesPatternFirst(string sql, bool expected)
    {
        // ARRANGE

        // ACT
        bool? result = await this.ExecuteSqlAsync<bool?>(sql, useNoCaseReplacement: true).ConfigureAwait(false);

        // ASSERT
        Assert.AreEqual(expected, result, "Invalid result");
    }

    /// <summary>
    /// Test the nocase_like function with an escape character doesn't allocate for every row, or every time a % backtracks.
    /// </summary>
    /// <param name="expected">The expected result: the value ends with the text the pattern needs, or it doesn't.</param>
    /// <remarks>
    /// The function is called directly, 100 times as it would be for 100 rows. The literal text after the % has an escape character, and is tried
    /// at almost every position of the 2,000 character value. Removing the escape characters each time the literal text was read allocated about
    /// 26 MB. The first call isn't measured, as it fills the array pool.
    /// </remarks>
    [TestMethod]
    [DataRow(false, DisplayName = "ManyRowsWithEscapeCharacter No match")]
    [DataRow(true, DisplayName = "ManyRowsWithEscapeCharacter Match at the end")]
    public void NoCaseLike_ManyRowsWithEscapeCharacter_DoesNotAllocatePerRow(bool expected)
    {
        // ARRANGE
        string pattern = "%a!_b%";
        string value = new string('a', 2_000) + (expected ? "_b" : string.Empty);
        bool? result = SqliteUnicodeMethods.Like(pattern, value, "!");

        // ACT
        long allocatedBefore = GC.GetAllocatedBytesForCurrentThread();

        for (int row = 0; row < 100; row++)
        {
            result = SqliteUnicodeMethods.Like(pattern, value, "!");
        }

        long allocated = GC.GetAllocatedBytesForCurrentThread() - allocatedBefore;

        // ASSERT
        Assert.AreEqual(expected, result, "Invalid result");
        Assert.IsLessThan(10_000L, allocated, $"Too much allocated ({allocated:N0} bytes)");
    }

    #endregion nocase_like tests

    #region nocase_ltrim, nocase_rtrim and nocase_trim tests

    /// <summary>
    /// Test the nocase_ltrim, nocase_rtrim and nocase_trim functions.
    /// </summary>
    /// <param name="functionName">The function to call.</param>
    /// <param name="value">The string to trim.</param>
    /// <param name="characters">The characters to remove.</param>
    /// <param name="expected">The expected result.</param>
    /// <returns>A <see cref="Task"/> that represents the asynchronous operation.</returns>
    [TestMethod]
    [DataRow(SqliteUnicodeFunctionNames.NoCaseLTrimFunction, "XxabcXx", "x", "abcXx", DisplayName = "Characters nocase_ltrim")]
    [DataRow(SqliteUnicodeFunctionNames.NoCaseLTrimFunction, null, "a", null, DisplayName = "Characters nocase_ltrim NULL value")]
    [DataRow(SqliteUnicodeFunctionNames.NoCaseLTrimFunction, "a", null, null, DisplayName = "Characters nocase_ltrim NULL characters")]
    [DataRow(SqliteUnicodeFunctionNames.NoCaseRTrimFunction, "XxabcXx", "x", "Xxabc", DisplayName = "Characters nocase_rtrim")]
    [DataRow(SqliteUnicodeFunctionNames.NoCaseRTrimFunction, null, "a", null, DisplayName = "Characters nocase_rtrim NULL value")]
    [DataRow(SqliteUnicodeFunctionNames.NoCaseRTrimFunction, "a", null, null, DisplayName = "Characters nocase_rtrim NULL characters")]
    [DataRow(SqliteUnicodeFunctionNames.NoCaseTrimFunction, "ÉéabcéÉ", "é", "abc", DisplayName = "Characters nocase_trim non-ASCII case is ignored")]
    [DataRow(SqliteUnicodeFunctionNames.NoCaseTrimFunction, "xXabcXx", "x", "abc", DisplayName = "Characters nocase_trim ASCII case is ignored")]
    [DataRow(SqliteUnicodeFunctionNames.NoCaseTrimFunction, "éabcé", "e", "éabcé", DisplayName = "Characters nocase_trim accents are not ignored")]
    [DataRow(SqliteUnicodeFunctionNames.NoCaseTrimFunction, "ÉxabcxÉ", "xé", "abc", DisplayName = "Characters nocase_trim several characters")]
    [DataRow(SqliteUnicodeFunctionNames.NoCaseTrimFunction, "xaxbx", "x", "axb", DisplayName = "Characters nocase_trim leaves characters in the middle")]
    [DataRow(SqliteUnicodeFunctionNames.NoCaseTrimFunction, "aAa", "A", "", DisplayName = "Characters nocase_trim everything")]
    [DataRow(SqliteUnicodeFunctionNames.NoCaseTrimFunction, "😀a😀", "😀", "a", DisplayName = "Characters nocase_trim characters outside the BMP")]
    [DataRow(SqliteUnicodeFunctionNames.NoCaseTrimFunction, "abc", "", "abc", DisplayName = "Characters nocase_trim empty characters")]
    [DataRow(SqliteUnicodeFunctionNames.NoCaseTrimFunction, "­abc­", "x", "­abc­", DisplayName = "Characters nocase_trim leaves an ignorable character that isn't in the set")]
    [DataRow(SqliteUnicodeFunctionNames.NoCaseTrimFunction, "I ❤️", "x", "I ❤️", DisplayName = "Characters nocase_trim leaves a variation selector that isn't in the set")]
    [DataRow(SqliteUnicodeFunctionNames.NoCaseTrimFunction, null, "a", null, DisplayName = "Characters nocase_trim NULL value")]
    [DataRow(SqliteUnicodeFunctionNames.NoCaseTrimFunction, "a", null, null, DisplayName = "Characters nocase_trim NULL characters")]
    public async Task NoCaseTrim_Characters_TrimsIgnoringCase(string functionName, string? value, string? characters, string? expected)
    {
        // ARRANGE

        // ACT
        string? result = await this.ExecuteFunctionAsync<string?>(functionName, value, characters).ConfigureAwait(false);

        // ASSERT
        Assert.AreEqual(expected, result, "Invalid result");
    }

    /// <summary>
    /// Test there are no one-argument nocase_ltrim, nocase_rtrim and nocase_trim functions (SQLite's built-in one-argument versions only remove spaces,
    /// so case makes no difference).
    /// </summary>
    /// <param name="functionName">The function to call.</param>
    /// <returns>A <see cref="Task"/> that represents the asynchronous operation.</returns>
    [TestMethod]
    [DataRow(SqliteUnicodeFunctionNames.NoCaseLTrimFunction, DisplayName = "OneArgument nocase_ltrim")]
    [DataRow(SqliteUnicodeFunctionNames.NoCaseRTrimFunction, DisplayName = "OneArgument nocase_rtrim")]
    [DataRow(SqliteUnicodeFunctionNames.NoCaseTrimFunction, DisplayName = "OneArgument nocase_trim")]
    public async Task NoCaseTrim_OneArgument_ThrowsSqliteException(string functionName)
    {
        // ARRANGE

        // ACT / ASSERT
        SqliteException exception = await Assert.ThrowsExactlyAsync<SqliteException>(
            () => this.ExecuteFunctionAsync<string?>(functionName, "  abc  "),
            "Unexpected exception")
            .ConfigureAwait(false);

        Assert.Contains("wrong number of arguments", exception.Message, "Invalid exception message");
    }

    #endregion nocase_ltrim, nocase_rtrim and nocase_trim tests

    #region nocase_replace tests

    /// <summary>
    /// Test the nocase_replace function.
    /// </summary>
    /// <param name="value">The string to search.</param>
    /// <param name="search">The string to replace.</param>
    /// <param name="replacement">The replacement string.</param>
    /// <param name="expected">The expected result.</param>
    /// <returns>A <see cref="Task"/> that represents the asynchronous operation.</returns>
    [TestMethod]
    [DataRow("École école ÉCOLE", "école", "x", "x x x", DisplayName = "Search Non-ASCII case is ignored")]
    [DataRow("aAa", "a", "bb", "bbbbbb", DisplayName = "Search ASCII case is ignored, replacement longer than match")]
    [DataRow("école", "ecole", "x", "école", DisplayName = "Search Accents are not ignored")]
    [DataRow("abc", "d", "x", "abc", DisplayName = "Search No match")]
    [DataRow("abc", "B", "", "ac", DisplayName = "Search Empty replacement")]
    [DataRow("café bar", "é", "x", "cafx bar", DisplayName = "Search Match a different length from the search")]
    [DataRow("😀a😀A", "a", "b", "😀b😀b", DisplayName = "Search Characters outside the BMP")]
    [DataRow("abc", "", "x", "abc", DisplayName = "Search Empty search returns the value")]
    [DataRow("abc", "", null, "abc", DisplayName = "Search Empty search returns the value even with a NULL replacement")]
    [DataRow("abc", "b", null, null, DisplayName = "Search NULL replacement")]
    [DataRow(null, "a", "x", null, DisplayName = "Search NULL value")]
    [DataRow("abc", null, "x", null, DisplayName = "Search NULL search")]
    public async Task NoCaseReplace_Search_ReplacesIgnoringCase(string? value, string? search, string? replacement, string? expected)
    {
        // ARRANGE

        // ACT
        string? result = await this.ExecuteFunctionAsync<string?>(SqliteUnicodeFunctionNames.NoCaseReplaceFunction, value, search, replacement).ConfigureAwait(false);

        // ASSERT
        Assert.AreEqual(expected, result, "Invalid result");
    }

    #endregion nocase_replace tests

    #region private methods

    /// <summary>
    /// Calls a SQLite function on a connection with UseSqliteNoCaseReplacement.
    /// </summary>
    /// <typeparam name="T">The type of the function's result.</typeparam>
    /// <param name="functionName">The function to call.</param>
    /// <param name="arguments">The function's arguments. A <see langword="null"/> argument is passed as a SQL NULL literal, the others as parameters.</param>
    /// <returns>The function's result.</returns>
    private Task<T> ExecuteFunctionAsync<T>(string functionName, params string?[] arguments)
    {
        string sql = $"SELECT {functionName}({string.Join(", ", arguments.Select((_, index) => $"{{{index}}}"))})";
        return this.ExecuteSqlAsync<T>(sql, useNoCaseReplacement: true, arguments);
    }

    /// <summary>
    /// Runs a SQL query with a single scalar result on SQLite.
    /// </summary>
    /// <typeparam name="T">The type of the result.</typeparam>
    /// <param name="sql">The SQL, with <c>{0}</c>, <c>{1}</c>... where the arguments go.</param>
    /// <param name="useNoCaseReplacement">Whether to call UseSqliteNoCaseReplacement.</param>
    /// <param name="arguments">The arguments. A <see langword="null"/> argument is passed as a SQL NULL literal, the others as parameters.</param>
    /// <returns>The result.</returns>
    private async Task<T> ExecuteSqlAsync<T>(string sql, bool useNoCaseReplacement, params string?[] arguments)
    {
        using Context context = await DatabaseUtils.OpenDatabaseAsync(
            this._testContext,
            DatabaseTypes.Sqlite,
            createNew: false,
            customConfigurationActions: optionsBuilder =>
            {
                if (useNoCaseReplacement)
                {
                    optionsBuilder.UseSqliteNoCaseReplacement();
                }
            })
            .ConfigureAwait(false);

        List<string> sqlArguments = [];
        List<object?> parameters = [];

        foreach (string? argument in arguments)
        {
            if (argument is null)
            {
                sqlArguments.Add("NULL");
            }
            else
            {
                sqlArguments.Add($"{{{parameters.Count}}}");
                parameters.Add(argument);
            }
        }

        string formattedSql = string.Format(CultureInfo.InvariantCulture, sql, [.. sqlArguments]);

        T result = await context.Database.ExecuteScalarAsync<T>(formattedSql, this._testContext.CancellationToken, parameters).ConfigureAwait(false);
        return result;
    }

    #endregion private methods
}
