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
/// Tests for the SQLite functions registered by UseSqliteNoCaseReplacement, which replace SQLite's built-in lower, upper, instr,
/// like, replace, trim, ltrim and rtrim functions, and add startswith and endswith.
/// </summary>
/// <remarks>
/// Each replacement must keep the interface of the SQLite function it replaces (argument order, NULL and empty arguments, results and errors),
/// so the SQL EF Core generates for SQLite works unchanged. Only the Unicode processing follows SQL Server, which
/// <see cref="SqliteSqlServerEquivalenceTests"/> checks.
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
    /// Test UseSqliteNoCaseReplacement replaces SQLite's built-in functions, and they are unchanged without it.
    /// </summary>
    /// <param name="sql">The SQL to run.</param>
    /// <param name="expectedBuiltIn">The expected result from SQLite's built-in function.</param>
    /// <param name="expectedReplacement">The expected result from the replacement function.</param>
    /// <returns>A <see cref="Task"/> that represents the asynchronous operation.</returns>
    [TestMethod(DisplayName = "UseSqliteNoCaseReplacement replaces the built-in functions")]
    [DataRow("SELECT lower('É')", "É", "é", DisplayName = "lower")]
    [DataRow("SELECT upper('é')", "é", "É", DisplayName = "upper")]
    [DataRow("SELECT instr('ÉCOLE', 'école')", "0", "1", DisplayName = "instr")]
    [DataRow("SELECT 'École' LIKE 'éc%'", "0", "1", DisplayName = "LIKE")]
    [DataRow("SELECT 'École' LIKE 'éc!%' ESCAPE '!'", "0", "0", DisplayName = "LIKE with an escape character")]
    [DataRow("SELECT 'École%' LIKE 'éCOLE!%' ESCAPE '!'", "0", "1", DisplayName = "LIKE with an escaped wildcard")]
    [DataRow("SELECT replace('ÉaA', 'é', 'x')", "ÉaA", "xaA", DisplayName = "replace")]
    [DataRow("SELECT trim('ÉéabcéÉ', 'é')", "ÉéabcéÉ", "abc", DisplayName = "trim")]
    [DataRow("SELECT ltrim('ÉéabcéÉ', 'é')", "ÉéabcéÉ", "abcéÉ", DisplayName = "ltrim")]
    [DataRow("SELECT rtrim('ÉéabcéÉ', 'é')", "ÉéabcéÉ", "Ééabc", DisplayName = "rtrim")]
    public async Task SqliteNoCaseFunctionsTests_001_Async(string sql, string expectedBuiltIn, string expectedReplacement)
    {
        // ARRANGE

        // ACT
        string? builtInResult = await this.ExecuteSqlAsync<string?>(sql, useNoCaseReplacement: false).ConfigureAwait(false);
        string? replacementResult = await this.ExecuteSqlAsync<string?>(sql, useNoCaseReplacement: true).ConfigureAwait(false);

        // ASSERT
        Assert.AreEqual(expectedBuiltIn, builtInResult, "Invalid built-in result");
        Assert.AreEqual(expectedReplacement, replacementResult, "Invalid replacement result");
    }

    /// <summary>
    /// Test the startswith and endswith functions are not registered without UseSqliteNoCaseReplacement (SQLite has no built-in versions).
    /// </summary>
    /// <param name="functionName">The function to call.</param>
    /// <returns>A <see cref="Task"/> that represents the asynchronous operation.</returns>
    [TestMethod(DisplayName = "startswith and endswith are not registered without UseSqliteNoCaseReplacement")]
    [DataRow("startswith", DisplayName = "startswith")]
    [DataRow("endswith", DisplayName = "endswith")]
    public async Task SqliteNoCaseFunctionsTests_014_Async(string functionName)
    {
        // ARRANGE

        // ACT / ASSERT
        SqliteException exception = await Assert.ThrowsExactlyAsync<SqliteException>(
            () => this.ExecuteSqlAsync<bool?>($"SELECT {functionName}('a', 'a')", useNoCaseReplacement: false),
            "Unexpected exception")
            .ConfigureAwait(false);

        Assert.Contains("no such function", exception.Message, "Invalid exception message");
    }

    /// <summary>
    /// Test the replacement functions have the same interface as SQLite's built-in functions: the same argument order, handling of NULL
    /// and empty arguments, result types and results, for text where case and Unicode processing make no difference.
    /// </summary>
    /// <param name="sql">The SQL to run.</param>
    /// <returns>A <see cref="Task"/> that represents the asynchronous operation.</returns>
    [TestMethod(DisplayName = "The replacement functions have the built-in interface")]
    [DataRow("SELECT lower(NULL)", DisplayName = "lower NULL")]
    [DataRow("SELECT typeof(lower('abc'))", DisplayName = "lower result type")]
    [DataRow("SELECT lower(12)", DisplayName = "lower number")]
    [DataRow("SELECT upper(NULL)", DisplayName = "upper NULL")]
    [DataRow("SELECT typeof(upper('abc'))", DisplayName = "upper result type")]
    [DataRow("SELECT instr('abc', 'c')", DisplayName = "instr ASCII")]
    [DataRow("SELECT instr('éèê-x', 'x')", DisplayName = "instr after non-ASCII characters")]
    [DataRow("SELECT instr('😀😀x', 'x')", DisplayName = "instr after characters outside the BMP")]
    [DataRow("SELECT instr('abc', '')", DisplayName = "instr empty search")]
    [DataRow("SELECT instr('', '')", DisplayName = "instr empty search in empty value")]
    [DataRow("SELECT instr('abc', 'z')", DisplayName = "instr no match")]
    [DataRow("SELECT instr(NULL, 'a')", DisplayName = "instr NULL value")]
    [DataRow("SELECT instr('a', NULL)", DisplayName = "instr NULL search")]
    [DataRow("SELECT instr(12345, 34)", DisplayName = "instr numbers")]
    [DataRow("SELECT typeof(instr('abc', 'b'))", DisplayName = "instr result type")]
    [DataRow("SELECT 'abc' LIKE 'a%'", DisplayName = "LIKE match")]
    [DataRow("SELECT 'abc' LIKE 'a_d'", DisplayName = "LIKE no match")]
    [DataRow("SELECT '' LIKE ''", DisplayName = "LIKE empty pattern and value")]
    [DataRow("SELECT NULL LIKE 'a'", DisplayName = "LIKE NULL value")]
    [DataRow("SELECT 'a' LIKE NULL", DisplayName = "LIKE NULL pattern")]
    [DataRow("SELECT 'abc' NOT LIKE 'a%'", DisplayName = "NOT LIKE")]
    [DataRow("SELECT typeof('abc' LIKE 'a%')", DisplayName = "LIKE result type")]
    [DataRow(@"SELECT 'a%' LIKE 'a\%' ESCAPE '\'", DisplayName = "LIKE ESCAPE match")]
    [DataRow(@"SELECT 'a' LIKE 'a\' ESCAPE '\'", DisplayName = "LIKE ESCAPE pattern ending with the escape character")]
    [DataRow("SELECT 'a' LIKE 'a' ESCAPE NULL", DisplayName = "LIKE ESCAPE NULL escape")]
    [DataRow(@"SELECT NULL LIKE 'a' ESCAPE '\'", DisplayName = "LIKE ESCAPE NULL value")]
    [DataRow("SELECT like('a%', 'abc')", DisplayName = "like function, pattern first")]
    [DataRow("SELECT like('abc', 'a%')", DisplayName = "like function, value first")]
    [DataRow(@"SELECT like('a\%', 'a%', '\')", DisplayName = "like function with an escape character")]
    [DataRow("SELECT replace('abc', 'b', 'x')", DisplayName = "replace")]
    [DataRow("SELECT replace('abc', '', 'x')", DisplayName = "replace empty search")]
    [DataRow("SELECT replace('abc', '', NULL)", DisplayName = "replace empty search, NULL replacement")]
    [DataRow("SELECT replace('abc', 'b', NULL)", DisplayName = "replace NULL replacement")]
    [DataRow("SELECT replace('abc', 'z', NULL)", DisplayName = "replace NULL replacement, no match")]
    [DataRow("SELECT replace(NULL, 'a', 'x')", DisplayName = "replace NULL value")]
    [DataRow("SELECT replace('abc', NULL, 'x')", DisplayName = "replace NULL search")]
    [DataRow("SELECT replace(12321, 2, 9)", DisplayName = "replace numbers")]
    [DataRow("SELECT trim('xabcx', 'x')", DisplayName = "trim")]
    [DataRow("SELECT trim('abc', '')", DisplayName = "trim empty characters")]
    [DataRow("SELECT trim('', 'x')", DisplayName = "trim empty value")]
    [DataRow("SELECT trim(NULL, 'a')", DisplayName = "trim NULL value")]
    [DataRow("SELECT trim('a', NULL)", DisplayName = "trim NULL characters")]
    [DataRow("SELECT ltrim('xabcx', 'x')", DisplayName = "ltrim")]
    [DataRow("SELECT ltrim(NULL, 'a')", DisplayName = "ltrim NULL value")]
    [DataRow("SELECT ltrim('a', NULL)", DisplayName = "ltrim NULL characters")]
    [DataRow("SELECT rtrim('xabcx', 'x')", DisplayName = "rtrim")]
    [DataRow("SELECT rtrim(NULL, 'a')", DisplayName = "rtrim NULL value")]
    [DataRow("SELECT rtrim('a', NULL)", DisplayName = "rtrim NULL characters")]
    public async Task SqliteNoCaseFunctionsTests_005_Async(string sql)
    {
        // ARRANGE
        string? expected = await this.ExecuteSqlAsync<string?>(sql, useNoCaseReplacement: false).ConfigureAwait(false);

        // ACT
        string? result = await this.ExecuteSqlAsync<string?>(sql, useNoCaseReplacement: true).ConfigureAwait(false);

        // ASSERT
        Assert.AreEqual(expected, result, "Result doesn't match the built-in function");
    }

    #endregion UseSqliteNoCaseReplacement tests

    #region lower and upper tests

    /// <summary>
    /// Test the lower and upper functions convert non-ASCII letters (SQLite's built-in functions only convert ASCII letters).
    /// </summary>
    /// <param name="functionName">The function to call.</param>
    /// <param name="value">The value to convert.</param>
    /// <param name="expected">The expected result.</param>
    /// <returns>A <see cref="Task"/> that represents the asynchronous operation.</returns>
    [TestMethod(DisplayName = "lower and upper")]
    [DataRow("lower", "ÉCOLE", "école", DisplayName = "lower non-ASCII")]
    [DataRow("lower", "ABC", "abc", DisplayName = "lower ASCII")]
    [DataRow("lower", "ÆØÅ", "æøå", DisplayName = "lower Nordic")]
    [DataRow("lower", null, null, DisplayName = "lower NULL")]
    [DataRow("upper", "école", "ÉCOLE", DisplayName = "upper non-ASCII")]
    [DataRow("upper", "abc", "ABC", DisplayName = "upper ASCII")]
    [DataRow("upper", "æøå", "ÆØÅ", DisplayName = "upper Nordic")]
    [DataRow("upper", "straße", "STRAßE", DisplayName = "upper keeps the length (ß is unchanged)")]
    [DataRow("upper", null, null, DisplayName = "upper NULL")]
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

    #endregion lower and upper tests

    #region instr tests

    /// <summary>
    /// Test the instr function.
    /// </summary>
    /// <param name="value">The string to search.</param>
    /// <param name="search">The string to find.</param>
    /// <param name="expected">The expected position.</param>
    /// <returns>A <see cref="Task"/> that represents the asynchronous operation.</returns>
    [TestMethod(DisplayName = "instr")]
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
        int? result = await this.ExecuteFunctionAsync<int?>("instr", value, search).ConfigureAwait(false);

        // ASSERT
        Assert.AreEqual(expected, result, "Invalid position");
    }

    #endregion instr tests

    #region like tests

    /// <summary>
    /// Test the LIKE operator with no escape character.
    /// </summary>
    /// <param name="pattern">The LIKE pattern.</param>
    /// <param name="value">The value to match.</param>
    /// <param name="expected">The expected result.</param>
    /// <returns>A <see cref="Task"/> that represents the asynchronous operation.</returns>
    [TestMethod(DisplayName = "LIKE")]
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
        bool? result = await this.ExecuteSqlAsync<bool?>("SELECT {1} LIKE {0}", useNoCaseReplacement: true, pattern, value).ConfigureAwait(false);

        // ASSERT
        Assert.AreEqual(expected, result, "Invalid result");
    }

    /// <summary>
    /// Test the LIKE operator with an escape character.
    /// </summary>
    /// <param name="pattern">The LIKE pattern.</param>
    /// <param name="value">The value to match.</param>
    /// <param name="escape">The escape character.</param>
    /// <param name="expected">The expected result.</param>
    /// <returns>A <see cref="Task"/> that represents the asynchronous operation.</returns>
    [TestMethod(DisplayName = "LIKE with an escape character")]
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
        bool? result = await this.ExecuteSqlAsync<bool?>("SELECT {1} LIKE {0} ESCAPE {2}", useNoCaseReplacement: true, pattern, value, escape).ConfigureAwait(false);

        // ASSERT
        Assert.AreEqual(expected, result, "Invalid result");
    }

    /// <summary>
    /// Test the LIKE operator with an escape that isn't a single character.
    /// </summary>
    /// <param name="pattern">The LIKE pattern.</param>
    /// <param name="escape">The escape character.</param>
    /// <returns>A <see cref="Task"/> that represents the asynchronous operation.</returns>
    [TestMethod(DisplayName = "LIKE with an escape that isn't a single character")]
    [DataRow("a", "ab", DisplayName = "Two characters")]
    [DataRow("a", "", DisplayName = "Empty")]
    [DataRow(null, "ab", DisplayName = "Two characters with a NULL pattern")]
    public async Task SqliteNoCaseFunctionsTests_008_Async(string? pattern, string escape)
    {
        // ARRANGE

        // ACT / ASSERT
        SqliteException exception = await Assert.ThrowsExactlyAsync<SqliteException>(
            () => this.ExecuteSqlAsync<bool?>("SELECT 'a' LIKE {0} ESCAPE {1}", useNoCaseReplacement: true, pattern, escape),
            "Unexpected exception")
            .ConfigureAwait(false);

        Assert.Contains("ESCAPE expression must be a single character", exception.Message, "Invalid exception message");
    }

    /// <summary>
    /// Test the LIKE operator with a pattern with many wildcards that doesn't match.
    /// </summary>
    /// <returns>A <see cref="Task"/> that represents the asynchronous operation.</returns>
    /// <remarks>A matcher that tried every way of splitting the value between the wildcards would take exponential time.</remarks>
    [TestMethod(DisplayName = "LIKE with many wildcards")]
    [Timeout(10000, CooperativeCancellation = true)]
    public async Task SqliteNoCaseFunctionsTests_009_Async()
    {
        // ARRANGE
        string pattern = string.Concat(Enumerable.Repeat("%a", 20)) + "%b";
        string value = new('a', 200);

        // ACT
        bool? result = await this.ExecuteSqlAsync<bool?>("SELECT {1} LIKE {0}", useNoCaseReplacement: true, pattern, value).ConfigureAwait(false);

        // ASSERT
        Assert.IsFalse(result, "Invalid result");
    }

    /// <summary>
    /// Test the LIKE operator with the longest pattern SQLite allows (50,000 UTF-8 bytes) matches, rather than overflowing the stack.
    /// </summary>
    /// <returns>A <see cref="Task"/> that represents the asynchronous operation.</returns>
    [TestMethod(DisplayName = "LIKE with the longest allowed pattern")]
    [Timeout(10000, CooperativeCancellation = true)]
    public async Task SqliteNoCaseFunctionsTests_010_Async()
    {
        // ARRANGE
        string pattern = new('_', 50_000);
        string value = new('a', 50_000);

        // ACT
        bool? result = await this.ExecuteSqlAsync<bool?>("SELECT {1} LIKE {0}", useNoCaseReplacement: true, pattern, value).ConfigureAwait(false);

        // ASSERT
        Assert.IsTrue(result, "Invalid result");
    }

    /// <summary>
    /// Test the LIKE operator with a pattern longer than SQLite allows (50,000 UTF-8 bytes) raises the same error as SQLite,
    /// which checks the length before anything else.
    /// </summary>
    /// <param name="sql">The SQL to run, with <c>{0}</c> where the pattern goes.</param>
    /// <param name="patternCharacter">The character the pattern is made of.</param>
    /// <param name="patternLength">The number of characters in the pattern.</param>
    /// <returns>A <see cref="Task"/> that represents the asynchronous operation.</returns>
    [TestMethod(DisplayName = "LIKE with a pattern that is too long")]
    [DataRow("SELECT 'a' LIKE {0}", '_', 50_001, DisplayName = "50,001 ASCII characters")]
    [DataRow("SELECT 'a' LIKE {0}", 'é', 25_001, DisplayName = "25,001 two-byte characters (50,002 bytes)")]
    [DataRow("SELECT NULL LIKE {0}", '_', 50_001, DisplayName = "NULL value")]
    [DataRow("SELECT 'a' LIKE {0} ESCAPE NULL", '_', 50_001, DisplayName = "NULL escape")]
    [DataRow("SELECT 'a' LIKE {0} ESCAPE 'ab'", '_', 50_001, DisplayName = "Invalid escape")]
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
    /// Test the LIKE operator accepts a pattern of exactly 50,000 UTF-8 bytes made of two-byte characters (the limit is in bytes, not characters).
    /// </summary>
    /// <returns>A <see cref="Task"/> that represents the asynchronous operation.</returns>
    [TestMethod(DisplayName = "LIKE with the longest allowed pattern of two-byte characters")]
    public async Task SqliteNoCaseFunctionsTests_018_Async()
    {
        // ARRANGE
        string pattern = new('é', 25_000);

        // ACT
        bool? result = await this.ExecuteSqlAsync<bool?>("SELECT 'a' LIKE {0}", useNoCaseReplacement: true, pattern).ConfigureAwait(false);

        // ASSERT
        Assert.IsFalse(result, "Invalid result");
    }

    /// <summary>
    /// Test the like function, which takes the pattern first (like(pattern, value) is the same as value LIKE pattern).
    /// </summary>
    /// <param name="sql">The SQL to run.</param>
    /// <param name="expected">The expected result.</param>
    /// <returns>A <see cref="Task"/> that represents the asynchronous operation.</returns>
    [TestMethod(DisplayName = "like function")]
    [DataRow("SELECT like('éc%', 'École')", true, DisplayName = "Pattern first")]
    [DataRow("SELECT like('École', 'éc%')", false, DisplayName = "Value first doesn't match")]
    [DataRow("SELECT like('éc!%', 'Éc%', '!')", true, DisplayName = "Pattern first, with an escape character")]
    public async Task SqliteNoCaseFunctionsTests_015_Async(string sql, bool expected)
    {
        // ARRANGE

        // ACT
        bool? result = await this.ExecuteSqlAsync<bool?>(sql, useNoCaseReplacement: true).ConfigureAwait(false);

        // ASSERT
        Assert.AreEqual(expected, result, "Invalid result");
    }

    #endregion like tests

    #region replace tests

    /// <summary>
    /// Test the replace function.
    /// </summary>
    /// <param name="value">The string to search.</param>
    /// <param name="search">The string to replace.</param>
    /// <param name="replacement">The replacement string.</param>
    /// <param name="expected">The expected result.</param>
    /// <returns>A <see cref="Task"/> that represents the asynchronous operation.</returns>
    [TestMethod(DisplayName = "replace")]
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
        string? result = await this.ExecuteFunctionAsync<string?>("replace", value, search, replacement).ConfigureAwait(false);

        // ASSERT
        Assert.AreEqual(expected, result, "Invalid result");
    }

    #endregion replace tests

    #region startswith and endswith tests

    /// <summary>
    /// Test the startswith and endswith functions.
    /// </summary>
    /// <param name="functionName">The function to call.</param>
    /// <param name="value">The string to check.</param>
    /// <param name="text">The prefix or suffix.</param>
    /// <param name="expected">The expected result.</param>
    /// <returns>A <see cref="Task"/> that represents the asynchronous operation.</returns>
    [TestMethod(DisplayName = "startswith and endswith")]
    [DataRow("startswith", "École", "éc", true, DisplayName = "startswith non-ASCII case is ignored")]
    [DataRow("startswith", "école", "ec", false, DisplayName = "startswith accents are not ignored")]
    [DataRow("startswith", "abc", "bc", false, DisplayName = "startswith no match")]
    [DataRow("startswith", "ab", "abc", false, DisplayName = "startswith prefix longer than value")]
    [DataRow("startswith", "abc", "", true, DisplayName = "startswith empty prefix")]
    [DataRow("startswith", "50% off", "50%", true, DisplayName = "startswith % is a plain character")]
    [DataRow("startswith", "abc", "%", false, DisplayName = "startswith % is not a wildcard")]
    [DataRow("startswith", null, "a", null, DisplayName = "startswith NULL value")]
    [DataRow("startswith", "a", null, null, DisplayName = "startswith NULL prefix")]
    [DataRow("endswith", "Body ÉCOLE", "école", true, DisplayName = "endswith non-ASCII case is ignored")]
    [DataRow("endswith", "école", "ECOLE", false, DisplayName = "endswith accents are not ignored")]
    [DataRow("endswith", "abc", "ab", false, DisplayName = "endswith no match")]
    [DataRow("endswith", "abc", "", true, DisplayName = "endswith empty suffix")]
    [DataRow("endswith", "a_c", "_C", true, DisplayName = "endswith _ is a plain character")]
    [DataRow("endswith", "abc", "_", false, DisplayName = "endswith _ is not a wildcard")]
    [DataRow("endswith", null, "a", null, DisplayName = "endswith NULL value")]
    [DataRow("endswith", "a", null, null, DisplayName = "endswith NULL suffix")]
    public async Task SqliteNoCaseFunctionsTests_012_Async(string functionName, string? value, string? text, bool? expected)
    {
        // ARRANGE

        // ACT
        bool? result = await this.ExecuteFunctionAsync<bool?>(functionName, value, text).ConfigureAwait(false);

        // ASSERT
        Assert.AreEqual(expected, result, "Invalid result");
    }

    #endregion startswith and endswith tests

    #region trim, ltrim and rtrim tests

    /// <summary>
    /// Test the trim, ltrim and rtrim functions with a set of characters.
    /// </summary>
    /// <param name="functionName">The function to call.</param>
    /// <param name="value">The string to trim.</param>
    /// <param name="characters">The characters to remove.</param>
    /// <param name="expected">The expected result.</param>
    /// <returns>A <see cref="Task"/> that represents the asynchronous operation.</returns>
    [TestMethod(DisplayName = "trim, ltrim and rtrim")]
    [DataRow("trim", "ÉéabcéÉ", "é", "abc", DisplayName = "trim non-ASCII case is ignored")]
    [DataRow("trim", "xXabcXx", "x", "abc", DisplayName = "trim ASCII case is ignored")]
    [DataRow("trim", "éabcé", "e", "éabcé", DisplayName = "trim accents are not ignored")]
    [DataRow("trim", "ÉxabcxÉ", "xé", "abc", DisplayName = "trim several characters")]
    [DataRow("trim", "xaxbx", "x", "axb", DisplayName = "trim leaves characters in the middle")]
    [DataRow("trim", "aAa", "A", "", DisplayName = "trim everything")]
    [DataRow("trim", "😀a😀", "😀", "a", DisplayName = "trim characters outside the BMP")]
    [DataRow("trim", "abc", "", "abc", DisplayName = "trim empty characters")]
    [DataRow("trim", "­abc­", "x", "­abc­", DisplayName = "trim leaves an ignorable character that isn't in the set")]
    [DataRow("trim", "I ❤️", "x", "I ❤️", DisplayName = "trim leaves a variation selector that isn't in the set")]
    [DataRow("trim", null, "a", null, DisplayName = "trim NULL value")]
    [DataRow("trim", "a", null, null, DisplayName = "trim NULL characters")]
    [DataRow("ltrim", "XxabcXx", "x", "abcXx", DisplayName = "ltrim")]
    [DataRow("ltrim", null, "a", null, DisplayName = "ltrim NULL value")]
    [DataRow("ltrim", "a", null, null, DisplayName = "ltrim NULL characters")]
    [DataRow("rtrim", "XxabcXx", "x", "Xxabc", DisplayName = "rtrim")]
    [DataRow("rtrim", null, "a", null, DisplayName = "rtrim NULL value")]
    [DataRow("rtrim", "a", null, null, DisplayName = "rtrim NULL characters")]
    public async Task SqliteNoCaseFunctionsTests_013_Async(string functionName, string? value, string? characters, string? expected)
    {
        // ARRANGE

        // ACT
        string? result = await this.ExecuteFunctionAsync<string?>(functionName, value, characters).ConfigureAwait(false);

        // ASSERT
        Assert.AreEqual(expected, result, "Invalid result");
    }

    /// <summary>
    /// Test the one-argument trim, ltrim and rtrim functions are still SQLite's built-in ones, which remove only spaces (U+0020), as SQL Server does.
    /// </summary>
    /// <param name="functionName">The function to call.</param>
    /// <param name="value">The string to trim.</param>
    /// <param name="expected">The expected result.</param>
    /// <returns>A <see cref="Task"/> that represents the asynchronous operation.</returns>
    [TestMethod(DisplayName = "One-argument trim, ltrim and rtrim")]
    [DataRow("trim", "  abc  ", "abc", DisplayName = "trim spaces")]
    [DataRow("trim", "　abc　", "　abc　", DisplayName = "trim leaves ideographic spaces")]
    [DataRow("trim", "\tabc\t", "\tabc\t", DisplayName = "trim leaves tabs")]
    [DataRow("ltrim", "  abc  ", "abc  ", DisplayName = "ltrim spaces")]
    [DataRow("rtrim", "  abc  ", "  abc", DisplayName = "rtrim spaces")]
    public async Task SqliteNoCaseFunctionsTests_016_Async(string functionName, string value, string expected)
    {
        // ARRANGE

        // ACT
        string? result = await this.ExecuteFunctionAsync<string?>(functionName, value).ConfigureAwait(false);

        // ASSERT
        Assert.AreEqual(expected, result, "Invalid result");
    }

    #endregion trim, ltrim and rtrim tests

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
