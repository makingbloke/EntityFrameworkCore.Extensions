// Copyright ©2021-2026 Mike King.
// This file is licensed to you under the MIT license.
// See the License.txt file in the solution root for more information.

using DotDoc.EntityFrameworkCore.Extensions.CaseInsensitivity;
using DotDoc.EntityFrameworkCore.Extensions.DatabaseType;
using DotDoc.EntityFrameworkCore.Extensions.Execute;
using DotDoc.EntityFrameworkCore.Extensions.Tests.Data;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace DotDoc.EntityFrameworkCore.Extensions.Tests.CaseInsensitivity;

/// <summary>
/// Tests for the SQLite functions registered by UseSqliteNoCaseReplacement: lower, upper and the nocase_* functions.
/// </summary>
/// <remarks>
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
    /// Test the nocase_* functions are not registered without UseSqliteNoCaseReplacement.
    /// </summary>
    /// <returns>A <see cref="Task"/> that represents the asynchronous operation.</returns>
    [TestMethod(DisplayName = "nocase_* functions are not registered without UseSqliteNoCaseReplacement")]
    public async Task SqliteNoCaseFunctionsTests_001_Async()
    {
        // ARRANGE
        using Context context = await DatabaseUtils.OpenDatabaseAsync(DatabaseTypes.Sqlite, createNew: false).ConfigureAwait(false);

        // ACT / ASSERT
        SqliteException exception = await Assert.ThrowsExactlyAsync<SqliteException>(
            () => context.Database.ExecuteScalarAsync<int?>("SELECT nocase_instr('a', 'a')", this._testContext.CancellationToken),
            "Unexpected exception")
            .ConfigureAwait(false);

        Assert.Contains("no such function", exception.Message, "Invalid exception message");
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
        int? result = await this.ExecuteFunctionAsync<int?>(SqliteNoCaseReplacementInterceptor.NoCaseInstrFunctionName, value, search).ConfigureAwait(false);

        // ASSERT
        Assert.AreEqual(expected, result, "Invalid position");
    }

    /// <summary>
    /// Test nocase_instr counts positions as SQLite's built-in instr does, for text where case doesn't matter.
    /// </summary>
    /// <param name="value">The string to search.</param>
    /// <param name="search">The string to find.</param>
    /// <returns>A <see cref="Task"/> that represents the asynchronous operation.</returns>
    [TestMethod(DisplayName = "nocase_instr matches instr")]
    [DataRow("abc", "c", DisplayName = "ASCII")]
    [DataRow("éèê-x", "x", DisplayName = "After non-ASCII characters")]
    [DataRow("😀😀x", "x", DisplayName = "After characters outside the BMP")]
    [DataRow("abc", "", DisplayName = "Empty search")]
    [DataRow("abc", "z", DisplayName = "No match")]
    public async Task SqliteNoCaseFunctionsTests_005_Async(string value, string search)
    {
        // ARRANGE
        int? expected = await this.ExecuteFunctionAsync<int?>("instr", value, search).ConfigureAwait(false);

        // ACT
        int? result = await this.ExecuteFunctionAsync<int?>(SqliteNoCaseReplacementInterceptor.NoCaseInstrFunctionName, value, search).ConfigureAwait(false);

        // ASSERT
        Assert.AreEqual(expected, result, "Position doesn't match instr");
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
        bool? result = await this.ExecuteFunctionAsync<bool?>(SqliteNoCaseReplacementInterceptor.NoCaseLikeFunctionName, pattern, value).ConfigureAwait(false);

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
        bool? result = await this.ExecuteFunctionAsync<bool?>(SqliteNoCaseReplacementInterceptor.NoCaseLikeFunctionName, pattern, value, escape).ConfigureAwait(false);

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
            () => this.ExecuteFunctionAsync<bool?>(SqliteNoCaseReplacementInterceptor.NoCaseLikeFunctionName, pattern, "a", escape),
            "Unexpected exception")
            .ConfigureAwait(false);

        Assert.Contains("ESCAPE expression must be a single character", exception.Message, "Invalid exception message");
    }

    /// <summary>
    /// Test the nocase_like function with a pattern with many wildcards that doesn't match (which would take exponential time without the failure cache).
    /// </summary>
    /// <returns>A <see cref="Task"/> that represents the asynchronous operation.</returns>
    [TestMethod(DisplayName = "nocase_like with many wildcards")]
    [Timeout(10000, CooperativeCancellation = true)]
    public async Task SqliteNoCaseFunctionsTests_009_Async()
    {
        // ARRANGE
        string pattern = string.Concat(Enumerable.Repeat("%a", 20)) + "%b";
        string value = new('a', 200);

        // ACT
        bool? result = await this.ExecuteFunctionAsync<bool?>(SqliteNoCaseReplacementInterceptor.NoCaseLikeFunctionName, pattern, value).ConfigureAwait(false);

        // ASSERT
        Assert.IsFalse(result, "Invalid result");
    }

    /// <summary>
    /// Test the nocase_like function with a pattern too long to match recursively returns a SQLite error, rather than overflowing the stack.
    /// </summary>
    /// <returns>A <see cref="Task"/> that represents the asynchronous operation.</returns>
    [TestMethod(DisplayName = "nocase_like with a very long pattern")]
    public async Task SqliteNoCaseFunctionsTests_010_Async()
    {
        // ARRANGE
        string pattern = new('_', 1_000_000);
        string value = new('a', 1_000_000);

        // ACT / ASSERT
        await Assert.ThrowsExactlyAsync<SqliteException>(
            () => this.ExecuteFunctionAsync<bool?>(SqliteNoCaseReplacementInterceptor.NoCaseLikeFunctionName, pattern, value),
            "Unexpected exception")
            .ConfigureAwait(false);
    }

    #endregion nocase_like tests

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
        string? result = await this.ExecuteFunctionAsync<string?>(SqliteNoCaseReplacementInterceptor.NoCaseReplaceFunctionName, value, search, replacement).ConfigureAwait(false);

        // ASSERT
        Assert.AreEqual(expected, result, "Invalid result");
    }

    #endregion nocase_replace tests

    #region nocase_startswith and nocase_endswith tests

    /// <summary>
    /// Test the nocase_startswith and nocase_endswith functions.
    /// </summary>
    /// <param name="functionName">The function to call.</param>
    /// <param name="value">The string to check.</param>
    /// <param name="text">The prefix or suffix.</param>
    /// <param name="expected">The expected result.</param>
    /// <returns>A <see cref="Task"/> that represents the asynchronous operation.</returns>
    [TestMethod(DisplayName = "nocase_startswith and nocase_endswith")]
    [DataRow(SqliteNoCaseReplacementInterceptor.NoCaseStartsWithFunctionName, "École", "éc", true, DisplayName = "startswith non-ASCII case is ignored")]
    [DataRow(SqliteNoCaseReplacementInterceptor.NoCaseStartsWithFunctionName, "école", "ec", false, DisplayName = "startswith accents are not ignored")]
    [DataRow(SqliteNoCaseReplacementInterceptor.NoCaseStartsWithFunctionName, "abc", "bc", false, DisplayName = "startswith no match")]
    [DataRow(SqliteNoCaseReplacementInterceptor.NoCaseStartsWithFunctionName, "ab", "abc", false, DisplayName = "startswith prefix longer than value")]
    [DataRow(SqliteNoCaseReplacementInterceptor.NoCaseStartsWithFunctionName, "abc", "", true, DisplayName = "startswith empty prefix")]
    [DataRow(SqliteNoCaseReplacementInterceptor.NoCaseStartsWithFunctionName, "50% off", "50%", true, DisplayName = "startswith % is a plain character")]
    [DataRow(SqliteNoCaseReplacementInterceptor.NoCaseStartsWithFunctionName, "abc", "%", false, DisplayName = "startswith % is not a wildcard")]
    [DataRow(SqliteNoCaseReplacementInterceptor.NoCaseStartsWithFunctionName, null, "a", null, DisplayName = "startswith NULL value")]
    [DataRow(SqliteNoCaseReplacementInterceptor.NoCaseStartsWithFunctionName, "a", null, null, DisplayName = "startswith NULL prefix")]
    [DataRow(SqliteNoCaseReplacementInterceptor.NoCaseEndsWithFunctionName, "Body ÉCOLE", "école", true, DisplayName = "endswith non-ASCII case is ignored")]
    [DataRow(SqliteNoCaseReplacementInterceptor.NoCaseEndsWithFunctionName, "école", "ECOLE", false, DisplayName = "endswith accents are not ignored")]
    [DataRow(SqliteNoCaseReplacementInterceptor.NoCaseEndsWithFunctionName, "abc", "ab", false, DisplayName = "endswith no match")]
    [DataRow(SqliteNoCaseReplacementInterceptor.NoCaseEndsWithFunctionName, "abc", "", true, DisplayName = "endswith empty suffix")]
    [DataRow(SqliteNoCaseReplacementInterceptor.NoCaseEndsWithFunctionName, "a_c", "_C", true, DisplayName = "endswith _ is a plain character")]
    [DataRow(SqliteNoCaseReplacementInterceptor.NoCaseEndsWithFunctionName, "abc", "_", false, DisplayName = "endswith _ is not a wildcard")]
    [DataRow(SqliteNoCaseReplacementInterceptor.NoCaseEndsWithFunctionName, null, "a", null, DisplayName = "endswith NULL value")]
    [DataRow(SqliteNoCaseReplacementInterceptor.NoCaseEndsWithFunctionName, "a", null, null, DisplayName = "endswith NULL suffix")]
    public async Task SqliteNoCaseFunctionsTests_012_Async(string functionName, string? value, string? text, bool? expected)
    {
        // ARRANGE

        // ACT
        bool? result = await this.ExecuteFunctionAsync<bool?>(functionName, value, text).ConfigureAwait(false);

        // ASSERT
        Assert.AreEqual(expected, result, "Invalid result");
    }

    #endregion nocase_startswith and nocase_endswith tests

    #region nocase_trim, nocase_ltrim and nocase_rtrim tests

    /// <summary>
    /// Test the nocase_trim, nocase_ltrim and nocase_rtrim functions.
    /// </summary>
    /// <param name="functionName">The function to call.</param>
    /// <param name="value">The string to trim.</param>
    /// <param name="characters">The characters to remove.</param>
    /// <param name="expected">The expected result.</param>
    /// <returns>A <see cref="Task"/> that represents the asynchronous operation.</returns>
    [TestMethod(DisplayName = "nocase_trim, nocase_ltrim and nocase_rtrim")]
    [DataRow(SqliteNoCaseReplacementInterceptor.NoCaseTrimFunctionName, "ÉéabcéÉ", "é", "abc", DisplayName = "trim non-ASCII case is ignored")]
    [DataRow(SqliteNoCaseReplacementInterceptor.NoCaseTrimFunctionName, "xXabcXx", "x", "abc", DisplayName = "trim ASCII case is ignored")]
    [DataRow(SqliteNoCaseReplacementInterceptor.NoCaseTrimFunctionName, "éabcé", "e", "éabcé", DisplayName = "trim accents are not ignored")]
    [DataRow(SqliteNoCaseReplacementInterceptor.NoCaseTrimFunctionName, "ÉxabcxÉ", "xé", "abc", DisplayName = "trim several characters")]
    [DataRow(SqliteNoCaseReplacementInterceptor.NoCaseTrimFunctionName, "xaxbx", "x", "axb", DisplayName = "trim leaves characters in the middle")]
    [DataRow(SqliteNoCaseReplacementInterceptor.NoCaseTrimFunctionName, "aAa", "A", "", DisplayName = "trim everything")]
    [DataRow(SqliteNoCaseReplacementInterceptor.NoCaseTrimFunctionName, "😀a😀", "😀", "a", DisplayName = "trim characters outside the BMP")]
    [DataRow(SqliteNoCaseReplacementInterceptor.NoCaseTrimFunctionName, "abc", "", "abc", DisplayName = "trim empty characters")]
    [DataRow(SqliteNoCaseReplacementInterceptor.NoCaseTrimFunctionName, null, "a", null, DisplayName = "trim NULL value")]
    [DataRow(SqliteNoCaseReplacementInterceptor.NoCaseTrimFunctionName, "a", null, null, DisplayName = "trim NULL characters")]
    [DataRow(SqliteNoCaseReplacementInterceptor.NoCaseLTrimFunctionName, "XxabcXx", "x", "abcXx", DisplayName = "ltrim")]
    [DataRow(SqliteNoCaseReplacementInterceptor.NoCaseLTrimFunctionName, null, "a", null, DisplayName = "ltrim NULL value")]
    [DataRow(SqliteNoCaseReplacementInterceptor.NoCaseLTrimFunctionName, "a", null, null, DisplayName = "ltrim NULL characters")]
    [DataRow(SqliteNoCaseReplacementInterceptor.NoCaseRTrimFunctionName, "XxabcXx", "x", "Xxabc", DisplayName = "rtrim")]
    [DataRow(SqliteNoCaseReplacementInterceptor.NoCaseRTrimFunctionName, null, "a", null, DisplayName = "rtrim NULL value")]
    [DataRow(SqliteNoCaseReplacementInterceptor.NoCaseRTrimFunctionName, "a", null, null, DisplayName = "rtrim NULL characters")]
    public async Task SqliteNoCaseFunctionsTests_013_Async(string functionName, string? value, string? characters, string? expected)
    {
        // ARRANGE

        // ACT
        string? result = await this.ExecuteFunctionAsync<string?>(functionName, value, characters).ConfigureAwait(false);

        // ASSERT
        Assert.AreEqual(expected, result, "Invalid result");
    }

    #endregion nocase_trim, nocase_ltrim and nocase_rtrim tests

    #region private methods

    /// <summary>
    /// Calls a SQLite function on a connection with UseSqliteNoCaseReplacement.
    /// </summary>
    /// <typeparam name="T">The type of the function's result.</typeparam>
    /// <param name="functionName">The function to call.</param>
    /// <param name="arguments">The function's arguments. A <see langword="null"/> argument is passed as a SQL NULL literal, the others as parameters.</param>
    /// <returns>The function's result.</returns>
    private async Task<T> ExecuteFunctionAsync<T>(string functionName, params string?[] arguments)
    {
        using Context context = await DatabaseUtils.OpenDatabaseAsync(
            DatabaseTypes.Sqlite,
            createNew: false,
            customConfigurationActions: optionsBuilder => optionsBuilder.UseSqliteNoCaseReplacement())
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

        string sql = $"SELECT {functionName}({string.Join(", ", sqlArguments)})";

        T result = await context.Database.ExecuteScalarAsync<T>(sql, this._testContext.CancellationToken, parameters).ConfigureAwait(false);
        return result;
    }

    #endregion private methods
}
