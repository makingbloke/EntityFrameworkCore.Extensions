// Copyright ©2021-2026 Mike King.
// This file is licensed to you under the MIT license.
// See the License.txt file in the solution root for more information.

using DotDoc.EntityFrameworkCore.Extensions.CaseInsensitivity;
using DotDoc.EntityFrameworkCore.Extensions.DatabaseType;
using DotDoc.EntityFrameworkCore.Extensions.Execute;
using DotDoc.EntityFrameworkCore.Extensions.Tests.Data;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
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
    [TestMethod(DisplayName = "UseSqliteNoCaseReplacement adds the nocase_* functions and leaves the built-in functions unchanged")]
    [DataRow("SELECT instr('ÉCOLE', 'école')", $"SELECT {SqliteUnicodeFunctionNames.NoCaseInstrFunction}('ÉCOLE', 'école')", "0", "1", DisplayName = "nocase_instr")]
    [DataRow("SELECT 'École' LIKE 'éc%'", $"SELECT {SqliteUnicodeFunctionNames.NoCaseLikeFunction}('éc%', 'École')", "0", "1", DisplayName = "nocase_like")]
    [DataRow("SELECT 'École' LIKE 'éc!%' ESCAPE '!'", $"SELECT {SqliteUnicodeFunctionNames.NoCaseLikeFunction}('éc!%', 'École', '!')", "0", "0", DisplayName = "nocase_like with an escape character")]
    [DataRow("SELECT 'École%' LIKE 'éCOLE!%' ESCAPE '!'", $"SELECT {SqliteUnicodeFunctionNames.NoCaseLikeFunction}('éCOLE!%', 'École%', '!')", "0", "1", DisplayName = "nocase_like with an escaped wildcard")]
    [DataRow("SELECT ltrim('ÉéabcéÉ', 'é')", $"SELECT {SqliteUnicodeFunctionNames.NoCaseLTrimFunction}('ÉéabcéÉ', 'é')", "ÉéabcéÉ", "abcéÉ", DisplayName = "nocase_ltrim")]
    [DataRow("SELECT replace('ÉaA', 'é', 'x')", $"SELECT {SqliteUnicodeFunctionNames.NoCaseReplaceFunction}('ÉaA', 'é', 'x')", "ÉaA", "xaA", DisplayName = "nocase_replace")]
    [DataRow("SELECT rtrim('ÉéabcéÉ', 'é')", $"SELECT {SqliteUnicodeFunctionNames.NoCaseRTrimFunction}('ÉéabcéÉ', 'é')", "ÉéabcéÉ", "Ééabc", DisplayName = "nocase_rtrim")]
    [DataRow("SELECT trim('ÉéabcéÉ', 'é')", $"SELECT {SqliteUnicodeFunctionNames.NoCaseTrimFunction}('ÉéabcéÉ', 'é')", "ÉéabcéÉ", "abc", DisplayName = "nocase_trim")]
    public async Task SqliteNoCaseFunctionsTests_001_Async(string builtInSql, string noCaseSql, string expectedBuiltIn, string expectedNoCase)
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
    [TestMethod(DisplayName = "The nocase_* functions have the built-in interface")]
    [DataRow("SELECT {0}instr('abc', 'c')", DisplayName = "nocase_instr ASCII")]
    [DataRow("SELECT {0}instr('éèê-x', 'x')", DisplayName = "nocase_instr after non-ASCII characters")]
    [DataRow("SELECT {0}instr('😀😀x', 'x')", DisplayName = "nocase_instr after characters outside the BMP")]
    [DataRow("SELECT {0}instr('abc', '')", DisplayName = "nocase_instr empty search")]
    [DataRow("SELECT {0}instr('', '')", DisplayName = "nocase_instr empty search in empty value")]
    [DataRow("SELECT {0}instr('abc', 'z')", DisplayName = "nocase_instr no match")]
    [DataRow("SELECT {0}instr(NULL, 'a')", DisplayName = "nocase_instr NULL value")]
    [DataRow("SELECT {0}instr('a', NULL)", DisplayName = "nocase_instr NULL search")]
    [DataRow("SELECT {0}instr(12345, 34)", DisplayName = "nocase_instr numbers")]
    [DataRow("SELECT typeof({0}instr('abc', 'b'))", DisplayName = "nocase_instr result type")]
    [DataRow("SELECT {0}like('a%', 'abc')", DisplayName = "nocase_like match")]
    [DataRow("SELECT {0}like('a_d', 'abc')", DisplayName = "nocase_like no match")]
    [DataRow("SELECT {0}like('abc', 'a%')", DisplayName = "nocase_like value first doesn't match")]
    [DataRow("SELECT {0}like('', '')", DisplayName = "nocase_like empty pattern and value")]
    [DataRow("SELECT {0}like('a', NULL)", DisplayName = "nocase_like NULL value")]
    [DataRow("SELECT {0}like(NULL, 'a')", DisplayName = "nocase_like NULL pattern")]
    [DataRow("SELECT NOT {0}like('a%', 'abc')", DisplayName = "NOT nocase_like")]
    [DataRow("SELECT typeof({0}like('a%', 'abc'))", DisplayName = "nocase_like result type")]
    [DataRow(@"SELECT {0}like('a\%', 'a%', '\')", DisplayName = "nocase_like with an escape character, match")]
    [DataRow(@"SELECT {0}like('a\', 'a', '\')", DisplayName = "nocase_like with an escape character, pattern ending with the escape character")]
    [DataRow("SELECT {0}like('a', 'a', NULL)", DisplayName = "nocase_like with an escape character, NULL escape")]
    [DataRow(@"SELECT {0}like('a', NULL, '\')", DisplayName = "nocase_like with an escape character, NULL value")]
    [DataRow("SELECT {0}ltrim('xabcx', 'x')", DisplayName = "nocase_ltrim")]
    [DataRow("SELECT {0}ltrim(NULL, 'a')", DisplayName = "nocase_ltrim NULL value")]
    [DataRow("SELECT {0}ltrim('a', NULL)", DisplayName = "nocase_ltrim NULL characters")]
    [DataRow("SELECT {0}replace('abc', 'b', 'x')", DisplayName = "nocase_replace")]
    [DataRow("SELECT {0}replace('abc', '', 'x')", DisplayName = "nocase_replace empty search")]
    [DataRow("SELECT {0}replace('abc', '', NULL)", DisplayName = "nocase_replace empty search, NULL replacement")]
    [DataRow("SELECT {0}replace('abc', 'b', NULL)", DisplayName = "nocase_replace NULL replacement")]
    [DataRow("SELECT {0}replace('abc', 'z', NULL)", DisplayName = "nocase_replace NULL replacement, no match")]
    [DataRow("SELECT {0}replace(NULL, 'a', 'x')", DisplayName = "nocase_replace NULL value")]
    [DataRow("SELECT {0}replace('abc', NULL, 'x')", DisplayName = "nocase_replace NULL search")]
    [DataRow("SELECT {0}replace(12321, 2, 9)", DisplayName = "nocase_replace numbers")]
    [DataRow("SELECT {0}rtrim('xabcx', 'x')", DisplayName = "nocase_rtrim")]
    [DataRow("SELECT {0}rtrim(NULL, 'a')", DisplayName = "nocase_rtrim NULL value")]
    [DataRow("SELECT {0}rtrim('a', NULL)", DisplayName = "nocase_rtrim NULL characters")]
    [DataRow("SELECT {0}trim('xabcx', 'x')", DisplayName = "nocase_trim")]
    [DataRow("SELECT {0}trim('abc', '')", DisplayName = "nocase_trim empty characters")]
    [DataRow("SELECT {0}trim('', 'x')", DisplayName = "nocase_trim empty value")]
    [DataRow("SELECT {0}trim(NULL, 'a')", DisplayName = "nocase_trim NULL value")]
    [DataRow("SELECT {0}trim('a', NULL)", DisplayName = "nocase_trim NULL characters")]
    public async Task SqliteNoCaseFunctionsTests_005_Async(string sql)
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
    [TestMethod(DisplayName = "lower and upper")]
    [DataRow(SqliteUnicodeFunctionNames.LowerFunction, "ÉCOLE", "école", DisplayName = "lower non-ASCII")]
    [DataRow(SqliteUnicodeFunctionNames.LowerFunction, "ABC", "abc", DisplayName = "lower ASCII")]
    [DataRow(SqliteUnicodeFunctionNames.LowerFunction, "ÆØÅ", "æøå", DisplayName = "lower Nordic")]
    [DataRow(SqliteUnicodeFunctionNames.LowerFunction, null, null, DisplayName = "lower NULL")]
    [DataRow(SqliteUnicodeFunctionNames.UpperFunction, "école", "ÉCOLE", DisplayName = "upper non-ASCII")]
    [DataRow(SqliteUnicodeFunctionNames.UpperFunction, "abc", "ABC", DisplayName = "upper ASCII")]
    [DataRow(SqliteUnicodeFunctionNames.UpperFunction, "æøå", "ÆØÅ", DisplayName = "upper Nordic")]
    [DataRow(SqliteUnicodeFunctionNames.UpperFunction, "straße", "STRAßE", DisplayName = "upper keeps the length (ß is unchanged)")]
    [DataRow(SqliteUnicodeFunctionNames.UpperFunction, null, null, DisplayName = "upper NULL")]
    public async Task SqliteNoCaseFunctionsTests_002_Async(string functionName, string? value, string? expected)
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
    [TestMethod(DisplayName = "ToLower and ToUpper in a LINQ query")]
    public async Task SqliteNoCaseFunctionsTests_003_Async()
    {
        // ARRANGE
        using Context context = await DatabaseUtils.OpenDatabaseAsync(
            DatabaseTypes.Sqlite,
            customConfigurationActions: optionsBuilder => optionsBuilder.UseSqliteNoCaseReplacement())
            .ConfigureAwait(false);

        await DatabaseUtils.CreateTestTableEntriesAsync(context, "Éclair", 1).ConfigureAwait(false);

        // ACT
#pragma warning disable CA1304, CA1311, CA1862 // EF Core translates ToLower() / ToUpper() to lower / upper, but not the overloads that take a culture or string.Equals with a StringComparison.
        int lowerCount = await context.TestTable1
            .CountAsync(e => e.TestField.ToLower() == "éclair", this._testContext.CancellationToken)
            .ConfigureAwait(false);

        int upperCount = await context.TestTable1
            .CountAsync(e => e.TestField.ToUpper() == "ÉCLAIR", this._testContext.CancellationToken)
            .ConfigureAwait(false);
#pragma warning restore CA1304, CA1311, CA1862

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
    [TestMethod(DisplayName = "lower and upper have the built-in interface")]
    [DataRow($"SELECT {SqliteUnicodeFunctionNames.LowerFunction}(NULL)", null, DisplayName = "lower NULL")]
    [DataRow($"SELECT typeof({SqliteUnicodeFunctionNames.LowerFunction}('abc'))", "text", DisplayName = "lower result type")]
    [DataRow($"SELECT {SqliteUnicodeFunctionNames.LowerFunction}(12)", "12", DisplayName = "lower number")]
    [DataRow($"SELECT {SqliteUnicodeFunctionNames.UpperFunction}(NULL)", null, DisplayName = "upper NULL")]
    [DataRow($"SELECT typeof({SqliteUnicodeFunctionNames.UpperFunction}('abc'))", "text", DisplayName = "upper result type")]
    public async Task SqliteNoCaseFunctionsTests_019_Async(string sql, string? expected)
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
    [TestMethod(DisplayName = "nocase_endswith and nocase_startswith")]
    [DataRow(SqliteUnicodeFunctionNames.NoCaseEndsWithFunction, "Body ÉCOLE", "école", true, DisplayName = "nocase_endswith non-ASCII case is ignored")]
    [DataRow(SqliteUnicodeFunctionNames.NoCaseEndsWithFunction, "école", "ECOLE", false, DisplayName = "nocase_endswith accents are not ignored")]
    [DataRow(SqliteUnicodeFunctionNames.NoCaseEndsWithFunction, "abc", "ab", false, DisplayName = "nocase_endswith no match")]
    [DataRow(SqliteUnicodeFunctionNames.NoCaseEndsWithFunction, "abc", "", true, DisplayName = "nocase_endswith empty suffix")]
    [DataRow(SqliteUnicodeFunctionNames.NoCaseEndsWithFunction, "a_c", "_C", true, DisplayName = "nocase_endswith _ is a plain character")]
    [DataRow(SqliteUnicodeFunctionNames.NoCaseEndsWithFunction, "abc", "_", false, DisplayName = "nocase_endswith _ is not a wildcard")]
    [DataRow(SqliteUnicodeFunctionNames.NoCaseEndsWithFunction, null, "a", null, DisplayName = "nocase_endswith NULL value")]
    [DataRow(SqliteUnicodeFunctionNames.NoCaseEndsWithFunction, "a", null, null, DisplayName = "nocase_endswith NULL suffix")]
    [DataRow(SqliteUnicodeFunctionNames.NoCaseStartsWithFunction, "École", "éc", true, DisplayName = "nocase_startswith non-ASCII case is ignored")]
    [DataRow(SqliteUnicodeFunctionNames.NoCaseStartsWithFunction, "école", "ec", false, DisplayName = "nocase_startswith accents are not ignored")]
    [DataRow(SqliteUnicodeFunctionNames.NoCaseStartsWithFunction, "abc", "bc", false, DisplayName = "nocase_startswith no match")]
    [DataRow(SqliteUnicodeFunctionNames.NoCaseStartsWithFunction, "ab", "abc", false, DisplayName = "nocase_startswith prefix longer than value")]
    [DataRow(SqliteUnicodeFunctionNames.NoCaseStartsWithFunction, "abc", "", true, DisplayName = "nocase_startswith empty prefix")]
    [DataRow(SqliteUnicodeFunctionNames.NoCaseStartsWithFunction, "50% off", "50%", true, DisplayName = "nocase_startswith % is a plain character")]
    [DataRow(SqliteUnicodeFunctionNames.NoCaseStartsWithFunction, "abc", "%", false, DisplayName = "nocase_startswith % is not a wildcard")]
    [DataRow(SqliteUnicodeFunctionNames.NoCaseStartsWithFunction, null, "a", null, DisplayName = "nocase_startswith NULL value")]
    [DataRow(SqliteUnicodeFunctionNames.NoCaseStartsWithFunction, "a", null, null, DisplayName = "nocase_startswith NULL prefix")]
    public async Task SqliteNoCaseFunctionsTests_012_Async(string functionName, string? value, string? text, bool? expected)
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
    [TestMethod(DisplayName = "nocase_instr")]
    [DataRow("Body ÉCOLE", "école", 6, DisplayName = "Non-ASCII case is ignored")]
    [DataRow("abc", "B", 2, DisplayName = "ASCII case is ignored")]
    [DataRow("ＡＢＣ", "b", 2, DisplayName = "Width is ignored")]
    [DataRow("école", "ecole", 0, DisplayName = "Accents are not ignored")]
    [DataRow("abc", "d", 0, DisplayName = "No match")]
    [DataRow("abcabc", "C", 3, DisplayName = "First match")]
    [DataRow("abc", "", 1, DisplayName = "Empty search matches at 1")]
    [DataRow("", "", 1, DisplayName = "Empty search in empty value matches at 1")]
    [DataRow("😀abc", "A", 2, DisplayName = "Position counts a character outside the BMP once")]
    [DataRow("é😀é😀ÉX", "x", 6, DisplayName = "Position after several non-ASCII characters")]
    [DataRow(null, "a", null, DisplayName = "NULL value")]
    [DataRow("a", null, null, DisplayName = "NULL search")]
    public async Task SqliteNoCaseFunctionsTests_004_Async(string? value, string? search, int? expected)
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
    [TestMethod(DisplayName = "nocase_like")]
    [DataRow("éc%", "École", true, DisplayName = "Non-ASCII case is ignored")]
    [DataRow("%COLE", "école", true, DisplayName = "Leading wildcard")]
    [DataRow("ec%", "école", false, DisplayName = "Accents are not ignored")]
    [DataRow("_cole", "École", true, DisplayName = "_ matches a non-ASCII character")]
    [DataRow("_bc", "😀bc", true, DisplayName = "_ matches a character outside the BMP")]
    [DataRow("__bc", "😀bc", false, DisplayName = "_ doesn't match half a character outside the BMP")]
    [DataRow("a%b%c", "AXXBYYC", true, DisplayName = "Several wildcards")]
    [DataRow("a%b%c", "AXXBYY", false, DisplayName = "Several wildcards, no match")]
    [DataRow("a%%c", "abc", true, DisplayName = "Consecutive wildcards")]
    [DataRow("abc", "ABC", true, DisplayName = "Whole value")]
    [DataRow("abc", "abcd", false, DisplayName = "Pattern must match the whole value")]
    [DataRow("a_c", "ac", false, DisplayName = "_ must match one character")]
    [DataRow("%", "", true, DisplayName = "% matches an empty value")]
    [DataRow("", "", true, DisplayName = "Empty pattern matches an empty value")]
    [DataRow("", "a", false, DisplayName = "Empty pattern doesn't match a value")]
    [DataRow("a[b]c", "A[B]C", true, DisplayName = "[ is not a wildcard")]
    [DataRow(null, "a", null, DisplayName = "NULL pattern")]
    [DataRow("a", null, null, DisplayName = "NULL value")]
    public async Task SqliteNoCaseFunctionsTests_006_Async(string? pattern, string? value, bool? expected)
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
    [TestMethod(DisplayName = "nocase_like with an escape character")]
    [DataRow(@"10\%", "10%", @"\", true, DisplayName = "Escaped % matches %")]
    [DataRow(@"10\%", "105", @"\", false, DisplayName = "Escaped % is not a wildcard")]
    [DataRow(@"a\_c", "A_C", @"\", true, DisplayName = "Escaped _ matches _")]
    [DataRow(@"a\_c", "abc", @"\", false, DisplayName = "Escaped _ is not a wildcard")]
    [DataRow(@"a\\c", @"A\C", @"\", true, DisplayName = "Escaped escape character")]
    [DataRow(@"\é%", "École", @"\", true, DisplayName = "Escaped ordinary character ignores case")]
    [DataRow(@"a\", "a", @"\", false, DisplayName = "Pattern ending with the escape character never matches")]
    [DataRow("a%%", "a%", "%", true, DisplayName = "% as the escape character")]
    [DataRow("a%%", "ab", "%", false, DisplayName = "% as the escape character is not a wildcard")]
    [DataRow("😀%_", "%x", "😀", true, DisplayName = "Escape character outside the BMP")]
    [DataRow(null, "a", @"\", null, DisplayName = "NULL pattern")]
    [DataRow("a", null, @"\", null, DisplayName = "NULL value")]
    [DataRow("a", "a", null, null, DisplayName = "NULL escape")]
    public async Task SqliteNoCaseFunctionsTests_007_Async(string? pattern, string? value, string? escape, bool? expected)
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
    [TestMethod(DisplayName = "nocase_like with an escape that isn't a single character")]
    [DataRow("a", "ab", DisplayName = "Two characters")]
    [DataRow("a", "", DisplayName = "Empty")]
    [DataRow(null, "ab", DisplayName = "Two characters with a NULL pattern")]
    public async Task SqliteNoCaseFunctionsTests_008_Async(string? pattern, string escape)
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
    [TestMethod(DisplayName = "nocase_like with many wildcards")]
    [Timeout(10000, CooperativeCancellation = true)]
    public async Task SqliteNoCaseFunctionsTests_009_Async()
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
    [TestMethod(DisplayName = "nocase_like with the longest allowed pattern")]
    [Timeout(10000, CooperativeCancellation = true)]
    public async Task SqliteNoCaseFunctionsTests_010_Async()
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
    [TestMethod(DisplayName = "nocase_like with a pattern that is too long")]
    [DataRow($"SELECT {SqliteUnicodeFunctionNames.NoCaseLikeFunction}({{0}}, 'a')", '_', 50_001, DisplayName = "50,001 ASCII characters")]
    [DataRow($"SELECT {SqliteUnicodeFunctionNames.NoCaseLikeFunction}({{0}}, 'a')", 'é', 25_001, DisplayName = "25,001 two-byte characters (50,002 bytes)")]
    [DataRow($"SELECT {SqliteUnicodeFunctionNames.NoCaseLikeFunction}({{0}}, NULL)", '_', 50_001, DisplayName = "NULL value")]
    [DataRow($"SELECT {SqliteUnicodeFunctionNames.NoCaseLikeFunction}({{0}}, 'a', NULL)", '_', 50_001, DisplayName = "NULL escape")]
    [DataRow($"SELECT {SqliteUnicodeFunctionNames.NoCaseLikeFunction}({{0}}, 'a', 'ab')", '_', 50_001, DisplayName = "Invalid escape")]
    public async Task SqliteNoCaseFunctionsTests_017_Async(string sql, char patternCharacter, int patternLength)
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
    [TestMethod(DisplayName = "nocase_like with the longest allowed pattern of two-byte characters")]
    public async Task SqliteNoCaseFunctionsTests_018_Async()
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
    [TestMethod(DisplayName = "nocase_like argument order")]
    [DataRow($"SELECT {SqliteUnicodeFunctionNames.NoCaseLikeFunction}('éc%', 'École')", true, DisplayName = "Pattern first")]
    [DataRow($"SELECT {SqliteUnicodeFunctionNames.NoCaseLikeFunction}('École', 'éc%')", false, DisplayName = "Value first doesn't match")]
    [DataRow($"SELECT {SqliteUnicodeFunctionNames.NoCaseLikeFunction}('éc!%', 'Éc%', '!')", true, DisplayName = "Pattern first, with an escape character")]
    public async Task SqliteNoCaseFunctionsTests_015_Async(string sql, bool expected)
    {
        // ARRANGE

        // ACT
        bool? result = await this.ExecuteSqlAsync<bool?>(sql, useNoCaseReplacement: true).ConfigureAwait(false);

        // ASSERT
        Assert.AreEqual(expected, result, "Invalid result");
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
    [TestMethod(DisplayName = "nocase_ltrim, nocase_rtrim and nocase_trim")]
    [DataRow(SqliteUnicodeFunctionNames.NoCaseLTrimFunction, "XxabcXx", "x", "abcXx", DisplayName = "nocase_ltrim")]
    [DataRow(SqliteUnicodeFunctionNames.NoCaseLTrimFunction, null, "a", null, DisplayName = "nocase_ltrim NULL value")]
    [DataRow(SqliteUnicodeFunctionNames.NoCaseLTrimFunction, "a", null, null, DisplayName = "nocase_ltrim NULL characters")]
    [DataRow(SqliteUnicodeFunctionNames.NoCaseRTrimFunction, "XxabcXx", "x", "Xxabc", DisplayName = "nocase_rtrim")]
    [DataRow(SqliteUnicodeFunctionNames.NoCaseRTrimFunction, null, "a", null, DisplayName = "nocase_rtrim NULL value")]
    [DataRow(SqliteUnicodeFunctionNames.NoCaseRTrimFunction, "a", null, null, DisplayName = "nocase_rtrim NULL characters")]
    [DataRow(SqliteUnicodeFunctionNames.NoCaseTrimFunction, "ÉéabcéÉ", "é", "abc", DisplayName = "nocase_trim non-ASCII case is ignored")]
    [DataRow(SqliteUnicodeFunctionNames.NoCaseTrimFunction, "xXabcXx", "x", "abc", DisplayName = "nocase_trim ASCII case is ignored")]
    [DataRow(SqliteUnicodeFunctionNames.NoCaseTrimFunction, "éabcé", "e", "éabcé", DisplayName = "nocase_trim accents are not ignored")]
    [DataRow(SqliteUnicodeFunctionNames.NoCaseTrimFunction, "ÉxabcxÉ", "xé", "abc", DisplayName = "nocase_trim several characters")]
    [DataRow(SqliteUnicodeFunctionNames.NoCaseTrimFunction, "xaxbx", "x", "axb", DisplayName = "nocase_trim leaves characters in the middle")]
    [DataRow(SqliteUnicodeFunctionNames.NoCaseTrimFunction, "aAa", "A", "", DisplayName = "nocase_trim everything")]
    [DataRow(SqliteUnicodeFunctionNames.NoCaseTrimFunction, "😀a😀", "😀", "a", DisplayName = "nocase_trim characters outside the BMP")]
    [DataRow(SqliteUnicodeFunctionNames.NoCaseTrimFunction, "abc", "", "abc", DisplayName = "nocase_trim empty characters")]
    [DataRow(SqliteUnicodeFunctionNames.NoCaseTrimFunction, "­abc­", "x", "­abc­", DisplayName = "nocase_trim leaves an ignorable character that isn't in the set")]
    [DataRow(SqliteUnicodeFunctionNames.NoCaseTrimFunction, "I ❤️", "x", "I ❤️", DisplayName = "nocase_trim leaves a variation selector that isn't in the set")]
    [DataRow(SqliteUnicodeFunctionNames.NoCaseTrimFunction, null, "a", null, DisplayName = "nocase_trim NULL value")]
    [DataRow(SqliteUnicodeFunctionNames.NoCaseTrimFunction, "a", null, null, DisplayName = "nocase_trim NULL characters")]
    public async Task SqliteNoCaseFunctionsTests_013_Async(string functionName, string? value, string? characters, string? expected)
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
    [TestMethod(DisplayName = "No one-argument nocase_ltrim, nocase_rtrim and nocase_trim")]
    [DataRow(SqliteUnicodeFunctionNames.NoCaseLTrimFunction, DisplayName = "nocase_ltrim")]
    [DataRow(SqliteUnicodeFunctionNames.NoCaseRTrimFunction, DisplayName = "nocase_rtrim")]
    [DataRow(SqliteUnicodeFunctionNames.NoCaseTrimFunction, DisplayName = "nocase_trim")]
    public async Task SqliteNoCaseFunctionsTests_016_Async(string functionName)
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
    [TestMethod(DisplayName = "nocase_replace")]
    [DataRow("École école ÉCOLE", "école", "x", "x x x", DisplayName = "Non-ASCII case is ignored")]
    [DataRow("aAa", "a", "bb", "bbbbbb", DisplayName = "ASCII case is ignored, replacement longer than match")]
    [DataRow("école", "ecole", "x", "école", DisplayName = "Accents are not ignored")]
    [DataRow("abc", "d", "x", "abc", DisplayName = "No match")]
    [DataRow("abc", "B", "", "ac", DisplayName = "Empty replacement")]
    [DataRow("café bar", "é", "x", "cafx bar", DisplayName = "Match a different length from the search")]
    [DataRow("😀a😀A", "a", "b", "😀b😀b", DisplayName = "Characters outside the BMP")]
    [DataRow("abc", "", "x", "abc", DisplayName = "Empty search returns the value")]
    [DataRow("abc", "", null, "abc", DisplayName = "Empty search returns the value even with a NULL replacement")]
    [DataRow("abc", "b", null, null, DisplayName = "NULL replacement")]
    [DataRow(null, "a", "x", null, DisplayName = "NULL value")]
    [DataRow("abc", null, "x", null, DisplayName = "NULL search")]
    public async Task SqliteNoCaseFunctionsTests_011_Async(string? value, string? search, string? replacement, string? expected)
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
