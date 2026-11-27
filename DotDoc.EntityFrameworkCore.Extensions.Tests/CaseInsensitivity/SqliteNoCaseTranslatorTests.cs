// Copyright ©2021-2026 Mike King.
// This file is licensed to you under the MIT license.
// See the License.txt file in the solution root for more information.

using DotDoc.EntityFrameworkCore.Extensions.CaseInsensitivity;
using DotDoc.EntityFrameworkCore.Extensions.DatabaseType;
using DotDoc.EntityFrameworkCore.Extensions.Tests.Data;
using DotDoc.EntityFrameworkCore.Extensions.Tests.Model;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;

namespace DotDoc.EntityFrameworkCore.Extensions.Tests.CaseInsensitivity;

/// <summary>
/// Tests for the SQLite translator registered by UseSqliteNoCaseReplacement, which translates string methods on a column with the NOCASE collation
/// to the nocase_* functions.
/// </summary>
/// <remarks>
/// Each test checks the SQL EF Core generates, as well as the result, so a test can't pass because EF Core evaluated the method on the client.
/// <see cref="SqliteSqlServerEquivalenceTests"/> checks the same methods give the same results on SQL Server.
/// </remarks>
[TestClass]
public class SqliteNoCaseTranslatorTests
{
    #region private fields

    /// <summary>
    /// The MSTest <see cref="TestContext"/> for the current test.
    /// </summary>
    private readonly TestContext _testContext;

    #endregion private fields

    #region public constructors

    /// <summary>
    /// Initializes a new instance of the <see cref="SqliteNoCaseTranslatorTests"/> class.
    /// </summary>
    /// <param name="testContext">The MSTest test context.</param>
    public SqliteNoCaseTranslatorTests(TestContext testContext)
    {
        this._testContext = testContext;
    }

    #endregion public constructors

    #region Translate tests

    /// <summary>
    /// Test string methods on a column with the NOCASE collation are translated to the nocase_* functions, and ignore case.
    /// </summary>
    /// <param name="method">The method to call (see <see cref="GetMethodQuery"/>).</param>
    /// <param name="value">The value stored in the database.</param>
    /// <param name="argument">The argument to the method.</param>
    /// <param name="expectedFunction">The nocase_* function expected in the SQL.</param>
    /// <param name="expected">The expected result (the value for a method used in a Where, or <see langword="null"/> if it doesn't match).</param>
    /// <returns>A <see cref="Task"/> that represents the asynchronous operation.</returns>
    [TestMethod]
    [DataRow("Contains", "Body ÉCOLE", "école", $"{SqliteUnicodeFunctionNames.NoCaseInstrFunction}(", "Body ÉCOLE", DisplayName = "NoCaseColumn Contains")]
    [DataRow("ContainsChar", "Body ÉCOLE", null, $"{SqliteUnicodeFunctionNames.NoCaseInstrFunction}(", "Body ÉCOLE", DisplayName = "NoCaseColumn Contains(char)")]
    [DataRow("IndexOf", "Body ÉCOLE", "école", $"{SqliteUnicodeFunctionNames.NoCaseInstrFunction}(", "5", DisplayName = "NoCaseColumn IndexOf")]
    [DataRow("IndexOfChar", "abcÉ", null, $"{SqliteUnicodeFunctionNames.NoCaseInstrFunction}(", "3", DisplayName = "NoCaseColumn IndexOf(char)")]
    [DataRow("IndexOfStartIndex", "Body ÉCOLE", "école", $"{SqliteUnicodeFunctionNames.NoCaseInstrFunction}(", "5", DisplayName = "NoCaseColumn IndexOf(string, int)")]
    [DataRow("IndexOfStartIndex", "abcABC", "a", $"{SqliteUnicodeFunctionNames.NoCaseInstrFunction}(", "3", DisplayName = "NoCaseColumn IndexOf(string, int) case is ignored after the start index")]
    [DataRow("IndexOfStartIndex", "Body ÉCOLE", "x", $"{SqliteUnicodeFunctionNames.NoCaseInstrFunction}(", "0", DisplayName = "NoCaseColumn IndexOf(string, int) no match is startIndex - 1")]
    [DataRow("IndexOfStartIndex", "😀😀é", "É", $"{SqliteUnicodeFunctionNames.NoCaseInstrFunction}(", "2", DisplayName = "NoCaseColumn IndexOf(string, int) counts a character outside the BMP once")]
    [DataRow("IndexOfCharStartIndex", "abcÉ", null, $"{SqliteUnicodeFunctionNames.NoCaseInstrFunction}(", "3", DisplayName = "NoCaseColumn IndexOf(char, int)")]
    [DataRow("StartsWith", "École", "éc", $"{SqliteUnicodeFunctionNames.NoCaseStartsWithFunction}(", "École", DisplayName = "NoCaseColumn StartsWith")]
    [DataRow("StartsWith", "50% off", "50%", $"{SqliteUnicodeFunctionNames.NoCaseStartsWithFunction}(", "50% off", DisplayName = "NoCaseColumn StartsWith % is a plain character")]
    [DataRow("StartsWith", "abc", "%", $"{SqliteUnicodeFunctionNames.NoCaseStartsWithFunction}(", null, DisplayName = "NoCaseColumn StartsWith % is not a wildcard")]
    [DataRow("StartsWithChar", "École", null, $"{SqliteUnicodeFunctionNames.NoCaseStartsWithFunction}(", "École", DisplayName = "NoCaseColumn StartsWith(char)")]
    [DataRow("EndsWith", "Body ÉCOLE", "école", $"{SqliteUnicodeFunctionNames.NoCaseEndsWithFunction}(", "Body ÉCOLE", DisplayName = "NoCaseColumn EndsWith")]
    [DataRow("EndsWithChar", "abcÉ", null, $"{SqliteUnicodeFunctionNames.NoCaseEndsWithFunction}(", "abcÉ", DisplayName = "NoCaseColumn EndsWith(char)")]
    [DataRow("Like", "Body ÉCOLE", "%école", $"{SqliteUnicodeFunctionNames.NoCaseLikeFunction}(", "Body ÉCOLE", DisplayName = "NoCaseColumn EF.Functions.Like")]
    [DataRow("LikeEscape", "50% ÉCOLE", "50!% é%", $"{SqliteUnicodeFunctionNames.NoCaseLikeFunction}(", "50% ÉCOLE", DisplayName = "NoCaseColumn EF.Functions.Like with an escape character")]
    [DataRow("Replace", "École école", "ÉCOLE", $"{SqliteUnicodeFunctionNames.NoCaseReplaceFunction}(", "x x", DisplayName = "NoCaseColumn Replace")]
    [DataRow("ReplaceChar", "École école", null, $"{SqliteUnicodeFunctionNames.NoCaseReplaceFunction}(", "xcole xcole", DisplayName = "NoCaseColumn Replace(char, char)")]
    [DataRow("Trim", "ÉabcÉ", null, $"{SqliteUnicodeFunctionNames.NoCaseTrimFunction}(", "abc", DisplayName = "NoCaseColumn Trim(char)")]
    [DataRow("TrimArray", "ÉxabcxÉ", null, $"{SqliteUnicodeFunctionNames.NoCaseTrimFunction}(", "abc", DisplayName = "NoCaseColumn Trim(char[])")]
    [DataRow("TrimStart", "ÉabcÉ", null, $"{SqliteUnicodeFunctionNames.NoCaseLTrimFunction}(", "abcÉ", DisplayName = "NoCaseColumn TrimStart(char)")]
    [DataRow("TrimStartArray", "ÉxabcxÉ", null, $"{SqliteUnicodeFunctionNames.NoCaseLTrimFunction}(", "abcxÉ", DisplayName = "NoCaseColumn TrimStart(char[])")]
    [DataRow("TrimEnd", "ÉabcÉ", null, $"{SqliteUnicodeFunctionNames.NoCaseRTrimFunction}(", "Éabc", DisplayName = "NoCaseColumn TrimEnd(char)")]
    [DataRow("TrimEndArray", "ÉxabcxÉ", null, $"{SqliteUnicodeFunctionNames.NoCaseRTrimFunction}(", "Éxabc", DisplayName = "NoCaseColumn TrimEnd(char[])")]
    public async Task Translate_NoCaseColumn_UsesNoCaseFunctions(string method, string value, string? argument, string expectedFunction, string? expected)
    {
        // ARRANGE
        using Context context = await this.OpenDatabaseAsync(useNoCaseReplacement: true, useCaseInsensitiveCollation: true, value).ConfigureAwait(false);
        IQueryable<object> query = GetMethodQuery(context.TestTable1, method, argument);

        // ACT
        string sql = query.ToQueryString();
        string? result = await this.GetResultAsync(query).ConfigureAwait(false);

        // ASSERT
        Assert.Contains(expectedFunction, sql, "nocase_* function not used");
        Assert.AreEqual(expected, result, "Invalid result");
    }

    /// <summary>
    /// Test string methods on a column without the NOCASE collation keep EF Core's own translation, which doesn't ignore the case of non-ASCII letters.
    /// </summary>
    /// <param name="method">The method to call (see <see cref="GetMethodQuery"/>).</param>
    /// <param name="value">The value stored in the database.</param>
    /// <param name="argument">The argument to the method.</param>
    /// <param name="expectedSql">The SQL expected from EF Core's own translation.</param>
    /// <param name="expected">The expected result (the value for a method used in a Where, or <see langword="null"/> if it doesn't match).</param>
    /// <returns>A <see cref="Task"/> that represents the asynchronous operation.</returns>
    [TestMethod]
    [DataRow("Contains", "Body ÉCOLE", "école", "instr(", null, DisplayName = "ColumnWithoutNoCase Contains")]
    [DataRow("IndexOf", "Body ÉCOLE", "école", "instr(", "-1", DisplayName = "ColumnWithoutNoCase IndexOf")]
    [DataRow("StartsWith", "École", "éc", " LIKE ", null, DisplayName = "ColumnWithoutNoCase StartsWith")]
    [DataRow("EndsWith", "Body ÉCOLE", "école", " LIKE ", null, DisplayName = "ColumnWithoutNoCase EndsWith")]
    [DataRow("Like", "Body ÉCOLE", "%école", " LIKE ", null, DisplayName = "ColumnWithoutNoCase EF.Functions.Like")]
    [DataRow("Replace", "École école", "ÉCOLE", "replace(", "École école", DisplayName = "ColumnWithoutNoCase Replace")]
    [DataRow("Trim", "ÉabcÉ", null, "trim(", "ÉabcÉ", DisplayName = "ColumnWithoutNoCase Trim(char)")]
    public async Task Translate_ColumnWithoutNoCase_KeepsEfCoreTranslation(string method, string value, string? argument, string expectedSql, string? expected)
    {
        // ARRANGE
        using Context context = await this.OpenDatabaseAsync(useNoCaseReplacement: true, useCaseInsensitiveCollation: false, value).ConfigureAwait(false);
        IQueryable<object> query = GetMethodQuery(context.TestTable1, method, argument);

        // ACT
        string sql = query.ToQueryString();
        string? result = await this.GetResultAsync(query).ConfigureAwait(false);

        // ASSERT
        Assert.DoesNotContain(SqliteUnicodeFunctionNames.NoCasePrefix, sql, "nocase_* function used");
        Assert.Contains(expectedSql, sql, "EF Core's translation not used");
        Assert.AreEqual(expected, result, "Invalid result");
    }

    /// <summary>
    /// Test ToLower and ToUpper keep EF Core's translation (lower and upper), which UseSqliteNoCaseReplacement replaces with Unicode versions,
    /// so they convert non-ASCII letters on a column without the NOCASE collation (case conversion doesn't depend on a collation).
    /// </summary>
    /// <param name="method">The method to call.</param>
    /// <param name="value">The value stored in the database.</param>
    /// <param name="expectedFunction">The function expected in the SQL.</param>
    /// <param name="expected">The expected result.</param>
    /// <returns>A <see cref="Task"/> that represents the asynchronous operation.</returns>
    [TestMethod]
    [DataRow("ToLower", "ÉCOLE", $"{SqliteUnicodeFunctionNames.LowerFunction}(", "école", DisplayName = "ToLowerAndToUpper ToLower")]
    [DataRow("ToUpper", "école", $"{SqliteUnicodeFunctionNames.UpperFunction}(", "ÉCOLE", DisplayName = "ToLowerAndToUpper ToUpper")]
    public async Task Translate_ToLowerAndToUpper_IgnoreCollation(string method, string value, string expectedFunction, string expected)
    {
        // ARRANGE
        using Context context = await this.OpenDatabaseAsync(useNoCaseReplacement: true, useCaseInsensitiveCollation: false, value).ConfigureAwait(false);
        IQueryable<object> query = GetMethodQuery(context.TestTable1, method, argument: null);

        // ACT
        string sql = query.ToQueryString();
        string? result = await this.GetResultAsync(query).ConfigureAwait(false);

        // ASSERT
        Assert.DoesNotContain(SqliteUnicodeFunctionNames.NoCasePrefix, sql, "nocase_* function used");
        Assert.Contains(expectedFunction, sql, "EF Core's translation not used");
        Assert.AreEqual(expected, result, "Invalid result");
    }

    /// <summary>
    /// Test EF.Functions.Collate(..., "NOCASE") on a column without the NOCASE collation uses the nocase_* functions.
    /// </summary>
    /// <returns>A <see cref="Task"/> that represents the asynchronous operation.</returns>
    [TestMethod]
    public async Task Translate_CollateNoCase_UsesNoCaseFunctions()
    {
        // ARRANGE
        using Context context = await this.OpenDatabaseAsync(useNoCaseReplacement: true, useCaseInsensitiveCollation: false, "Body ÉCOLE").ConfigureAwait(false);
        IQueryable<TestTable1> query = context.TestTable1.Where(e => EF.Functions.Collate(e.TestField, SqliteUnicodeFunctionNames.NoCaseCollation).Contains("école"));

        // ACT
        string sql = query.ToQueryString();
        int count = await query.CountAsync(this._testContext.CancellationToken).ConfigureAwait(false);

        // ASSERT
        Assert.Contains($"{SqliteUnicodeFunctionNames.NoCaseInstrFunction}(", sql, "nocase_instr not used");
        Assert.AreEqual(1, count, "Invalid count");
    }

    /// <summary>
    /// Test nothing is translated to the nocase_* functions without UseSqliteNoCaseReplacement, even on a column with the NOCASE collation.
    /// </summary>
    /// <param name="method">The method to call (see <see cref="GetMethodQuery"/>).</param>
    /// <param name="argument">The argument to the method.</param>
    /// <returns>A <see cref="Task"/> that represents the asynchronous operation.</returns>
    [TestMethod]
    [DataRow("Contains", "école", DisplayName = "NoUseSqliteNoCaseReplacement Contains")]
    [DataRow("IndexOf", "école", DisplayName = "NoUseSqliteNoCaseReplacement IndexOf")]
    [DataRow("StartsWith", "éc", DisplayName = "NoUseSqliteNoCaseReplacement StartsWith")]
    [DataRow("EndsWith", "école", DisplayName = "NoUseSqliteNoCaseReplacement EndsWith")]
    [DataRow("Like", "%école", DisplayName = "NoUseSqliteNoCaseReplacement EF.Functions.Like")]
    [DataRow("Replace", "ÉCOLE", DisplayName = "NoUseSqliteNoCaseReplacement Replace")]
    [DataRow("Trim", null, DisplayName = "NoUseSqliteNoCaseReplacement Trim(char)")]
    [DataRow("ToLower", null, DisplayName = "NoUseSqliteNoCaseReplacement ToLower")]
    [DataRow("ToUpper", null, DisplayName = "NoUseSqliteNoCaseReplacement ToUpper")]
    public async Task Translate_NoUseSqliteNoCaseReplacement_TranslatesNothing(string method, string? argument)
    {
        // ARRANGE
        using Context context = await this.OpenDatabaseAsync(useNoCaseReplacement: false, useCaseInsensitiveCollation: true, "Body ÉCOLE").ConfigureAwait(false);
        IQueryable<object> query = GetMethodQuery(context.TestTable1, method, argument);

        // ACT
        string sql = query.ToQueryString();

        // ASSERT
        Assert.DoesNotContain(SqliteUnicodeFunctionNames.NoCasePrefix, sql, "nocase_* function used");
    }

    /// <summary>
    /// Test a NOCASE column on the other side of a join uses the nocase_* functions.
    /// </summary>
    /// <returns>A <see cref="Task"/> that represents the asynchronous operation.</returns>
    [TestMethod]
    public async Task Translate_NoCaseColumnAfterJoin_UsesNoCaseFunctions()
    {
        // ARRANGE
        using Context context = await this.OpenDatabaseAsync(useNoCaseReplacement: true, useCaseInsensitiveCollation: true, "First").ConfigureAwait(false);
        context.TestTable2.Add(new TestTable2 { TestField = "Body ÉCOLE" });
        await context.SaveChangesAsync(this._testContext.CancellationToken).ConfigureAwait(false);

        IQueryable<TestTable1> query =
            from t1 in context.TestTable1
            join t2 in context.TestTable2 on t1.Id equals t2.Id
            where t2.TestField.Contains("école")
            select t1;

        // ACT
        string sql = query.ToQueryString();
        int count = await query.CountAsync(this._testContext.CancellationToken).ConfigureAwait(false);

        // ASSERT
        Assert.Contains($"{SqliteUnicodeFunctionNames.NoCaseInstrFunction}(", sql, "nocase_instr not used");
        Assert.AreEqual(1, count, "Invalid count");
    }

    /// <summary>
    /// Test an expression with no known collation (a concatenation of a constant and a number, neither of which has a collation) keeps
    /// EF Core's own translation.
    /// </summary>
    /// <returns>A <see cref="Task"/> that represents the asynchronous operation.</returns>
    [TestMethod]
    public async Task Translate_NoKnownCollation_KeepsEfCoreTranslation()
    {
        // ARRANGE
        using Context context = await this.OpenDatabaseAsync(useNoCaseReplacement: true, useCaseInsensitiveCollation: true, "Body ÉCOLE").ConfigureAwait(false);
        IQueryable<TestTable1> query = context.TestTable1.Where(e => ("Body ÉCOLE" + e.Id).Contains("école"));

        // ACT
        string sql = query.ToQueryString();
        int count = await query.CountAsync(this._testContext.CancellationToken).ConfigureAwait(false);

        // ASSERT
        Assert.DoesNotContain(SqliteUnicodeFunctionNames.NoCasePrefix, sql, "nocase_* function used");
        Assert.AreEqual(0, count, "Invalid count");
    }

    /// <summary>
    /// Test a column from raw SQL keeps EF Core's own translation, as its collation isn't known.
    /// </summary>
    /// <returns>A <see cref="Task"/> that represents the asynchronous operation.</returns>
    [TestMethod]
    public async Task Translate_RawSqlColumn_KeepsEfCoreTranslation()
    {
        // ARRANGE
        using Context context = await this.OpenDatabaseAsync(useNoCaseReplacement: true, useCaseInsensitiveCollation: true, "Body ÉCOLE").ConfigureAwait(false);
        IQueryable<string> query = context.Database
            .SqlQueryRaw<string>("SELECT TestField AS Value FROM TestTable1")
            .Where(value => value.Contains("école"));

        // ACT
        string sql = query.ToQueryString();
        int count = await query.CountAsync(this._testContext.CancellationToken).ConfigureAwait(false);

        // ASSERT
        Assert.DoesNotContain(SqliteUnicodeFunctionNames.NoCasePrefix, sql, "nocase_* function used");
        Assert.AreEqual(0, count, "Invalid count");
    }

    /// <summary>
    /// Test a NOCASE column from a set operation uses the nocase_* functions: EF Core still knows the column it came from, and SQLite gives a
    /// compound SELECT's column the collation of the leftmost SELECT.
    /// </summary>
    /// <returns>A <see cref="Task"/> that represents the asynchronous operation.</returns>
    [TestMethod]
    public async Task Translate_NoCaseColumnFromSetOperation_UsesNoCaseInstr()
    {
        // ARRANGE
        using Context context = await this.OpenDatabaseAsync(useNoCaseReplacement: true, useCaseInsensitiveCollation: true, "Body ÉCOLE").ConfigureAwait(false);
        IQueryable<string> query = context.TestTable1
            .Select(e => e.TestField)
            .Concat(context.TestTable2.Select(e => e.TestField))
            .Where(value => value.Contains("école"));

        // ACT
        string sql = query.ToQueryString();
        int count = await query.CountAsync(this._testContext.CancellationToken).ConfigureAwait(false);

        // ASSERT
        Assert.Contains($"{SqliteUnicodeFunctionNames.NoCaseInstrFunction}(", sql, "nocase_instr not used");
        Assert.AreEqual(1, count, "Invalid count");
    }

    /// <summary>
    /// Test a NOCASE column from a subquery uses the nocase_* functions: EF Core still knows the column it came from, and SQLite keeps the
    /// collation of a column selected from a subquery.
    /// </summary>
    /// <returns>A <see cref="Task"/> that represents the asynchronous operation.</returns>
    /// <remarks>The Where after Take can't be added to the same SELECT, so EF Core puts the Take in a subquery.</remarks>
    [TestMethod]
    public async Task Translate_NoCaseColumnFromSubquery_UsesNoCaseInstr()
    {
        // ARRANGE
        using Context context = await this.OpenDatabaseAsync(useNoCaseReplacement: true, useCaseInsensitiveCollation: true, "Body ÉCOLE").ConfigureAwait(false);
        IQueryable<TestTable1> query = context.TestTable1
            .OrderBy(e => e.Id)
            .Take(1)
            .Where(e => e.TestField.Contains("école"));

        // ACT
        string sql = query.ToQueryString();
        int count = await query.CountAsync(this._testContext.CancellationToken).ConfigureAwait(false);

        // ASSERT
        Assert.Contains($"{SqliteUnicodeFunctionNames.NoCaseInstrFunction}(", sql, "nocase_instr not used");
        Assert.AreEqual(1, count, "Invalid count");
    }

    /// <summary>
    /// Test a NOCASE column searched for inside a constant keeps EF Core's own translation: the string being searched decides the collation,
    /// and the constant has none.
    /// </summary>
    /// <returns>A <see cref="Task"/> that represents the asynchronous operation.</returns>
    [TestMethod]
    public async Task Translate_NoCaseColumnInsideConstant_KeepsEfCoreTranslation()
    {
        // ARRANGE
        using Context context = await this.OpenDatabaseAsync(useNoCaseReplacement: true, useCaseInsensitiveCollation: true, "body école").ConfigureAwait(false);
        IQueryable<TestTable1> query = context.TestTable1.Where(e => "BODY ÉCOLE!".Contains(e.TestField));

        // ACT
        string sql = query.ToQueryString();
        int count = await query.CountAsync(this._testContext.CancellationToken).ConfigureAwait(false);

        // ASSERT
        Assert.DoesNotContain(SqliteUnicodeFunctionNames.NoCasePrefix, sql, "nocase_* function used");
        Assert.AreEqual(0, count, "Invalid count");
    }

    /// <summary>
    /// Test a NOCASE column reached through a reference navigation (a join) uses the nocase_* functions.
    /// </summary>
    /// <returns>A <see cref="Task"/> that represents the asynchronous operation.</returns>
    [TestMethod]
    public async Task Translate_ReferenceNavigation_UsesNoCaseInstr()
    {
        // ARRANGE
        using Context context = await this.OpenNavigationDatabaseAsync().ConfigureAwait(false);
        IQueryable<NavigationChild> query = context.Set<NavigationChild>().Where(e => e.Parent.Name.Contains("école"));

        // ACT
        string sql = query.ToQueryString();
        int count = await query.CountAsync(this._testContext.CancellationToken).ConfigureAwait(false);

        // ASSERT
        Assert.Contains($"{SqliteUnicodeFunctionNames.NoCaseInstrFunction}(", sql, "nocase_instr not used");
        Assert.AreEqual(1, count, "Invalid count");
    }

    /// <summary>
    /// Test a NOCASE column reached through a collection navigation (EXISTS) uses the nocase_* functions.
    /// </summary>
    /// <returns>A <see cref="Task"/> that represents the asynchronous operation.</returns>
    [TestMethod]
    public async Task Translate_CollectionNavigation_UsesNoCaseInstr()
    {
        // ARRANGE
        using Context context = await this.OpenNavigationDatabaseAsync().ConfigureAwait(false);
        IQueryable<NavigationParent> query = context.Set<NavigationParent>().Where(e => e.Children.Any(c => c.Name.Contains("école")));

        // ACT
        string sql = query.ToQueryString();
        int count = await query.CountAsync(this._testContext.CancellationToken).ConfigureAwait(false);

        // ASSERT
        Assert.Contains($"{SqliteUnicodeFunctionNames.NoCaseInstrFunction}(", sql, "nocase_instr not used");
        Assert.AreEqual(1, count, "Invalid count");
    }

    /// <summary>
    /// Test IndexOf(string, int) on a NOCASE column gives the same result as EF Core's own translation on a column without NOCASE,
    /// including <c>startIndex - 1</c> when there's no match, for text where case makes no difference.
    /// </summary>
    /// <param name="value">The value stored in the database.</param>
    /// <param name="search">The string to find.</param>
    /// <param name="startIndex">The start index.</param>
    /// <returns>A <see cref="Task"/> that represents the asynchronous operation.</returns>
    [TestMethod]
    [DataRow("abcabc", "b", 0, DisplayName = "IndexOfWithStartIndex Start index 0")]
    [DataRow("abcabc", "b", 2, DisplayName = "IndexOfWithStartIndex Match after the start index")]
    [DataRow("abcabc", "b", 5, DisplayName = "IndexOfWithStartIndex Match only before the start index")]
    [DataRow("abc", "z", 1, DisplayName = "IndexOfWithStartIndex No match")]
    [DataRow("abc", "c", 5, DisplayName = "IndexOfWithStartIndex Start index past the end")]
    [DataRow("abc", "", 2, DisplayName = "IndexOfWithStartIndex Empty search")]
    [DataRow("abc", "a", -1, DisplayName = "IndexOfWithStartIndex Start index -1")]
    [DataRow("abc", "c", -2, DisplayName = "IndexOfWithStartIndex Negative start index counts from the end")]
    [DataRow("😀😀x😀x", "x", 3, DisplayName = "IndexOfWithStartIndex Characters outside the BMP")]
    public async Task Translate_IndexOfWithStartIndex_MatchesEfCoreTranslation(string value, string search, int startIndex)
    {
        // ARRANGE
        CancellationToken cancellationToken = this._testContext.CancellationToken;
        int expected;
        using (Context efContext = await this.OpenDatabaseAsync(useNoCaseReplacement: true, useCaseInsensitiveCollation: false, value).ConfigureAwait(false))
        {
            expected = await efContext.TestTable1.Select(e => e.TestField.IndexOf(search, startIndex)).SingleAsync(cancellationToken).ConfigureAwait(false);
        }

        using Context context = await this.OpenDatabaseAsync(useNoCaseReplacement: true, useCaseInsensitiveCollation: true, value).ConfigureAwait(false);
        IQueryable<int> query = context.TestTable1.Select(e => e.TestField.IndexOf(search, startIndex));

        // ACT
        string sql = query.ToQueryString();
        int result = await query.SingleAsync(cancellationToken).ConfigureAwait(false);

        // ASSERT
        Assert.Contains($"{SqliteUnicodeFunctionNames.NoCaseInstrFunction}(", sql, "nocase_instr not used");
        Assert.AreEqual(expected, result, "Result doesn't match EF Core's translation");
    }

    /// <summary>
    /// Test the result of a string method on a NOCASE column keeps the NOCASE collation, so comparing it ignores case,
    /// as it does on SQL Server (where a function's result has its argument's collation).
    /// </summary>
    /// <param name="method">
    /// The method: Replace (Replace("x", "y")), Trim, TrimStart or TrimEnd (with 'x'), TrimWhitespace, TrimStartWhitespace or TrimEndWhitespace
    /// (Trim(), TrimStart() or TrimEnd()), Substring (Substring(1)), SubstringLength (Substring(1, 5)), ToLower or ToUpper.
    /// </param>
    /// <param name="value">The value stored in the database.</param>
    /// <returns>A <see cref="Task"/> that represents the asynchronous operation.</returns>
    /// <remarks>The result is compared with "ÉcOlE", which only matches it ignoring case.</remarks>
    [TestMethod]
    [SuppressMessage("Globalization", "CA1304:Specify CultureInfo", Justification = "EF Core translates ToLower() / ToUpper(), but not the overloads that take a culture.")]
    [SuppressMessage("Globalization", "CA1311:Specify a culture or use an invariant version", Justification = "EF Core translates ToLower() / ToUpper(), but not the overloads that take a culture.")]
    [SuppressMessage("Performance", "CA1862:Use the 'StringComparison' method overloads to perform case-insensitive string comparisons", Justification = "The test checks the translation of ToLower() / ToUpper(); EF Core doesn't translate string.Equals with a StringComparison.")]
    [DataRow("Replace", "école", DisplayName = "CompareMethodResult Replace")]
    [DataRow("Trim", "xécolex", DisplayName = "CompareMethodResult Trim(char)")]
    [DataRow("TrimStart", "xécole", DisplayName = "CompareMethodResult TrimStart(char)")]
    [DataRow("TrimEnd", "écolex", DisplayName = "CompareMethodResult TrimEnd(char)")]
    [DataRow("TrimWhitespace", "  école  ", DisplayName = "CompareMethodResult Trim()")]
    [DataRow("TrimStartWhitespace", "  école", DisplayName = "CompareMethodResult TrimStart()")]
    [DataRow("TrimEndWhitespace", "école  ", DisplayName = "CompareMethodResult TrimEnd()")]
    [DataRow("Substring", "xécole", DisplayName = "CompareMethodResult Substring(int)")]
    [DataRow("SubstringLength", "xécolex", DisplayName = "CompareMethodResult Substring(int, int)")]
    [DataRow("ToLower", "ÉCOLE", DisplayName = "CompareMethodResult ToLower")]
    [DataRow("ToUpper", "école", DisplayName = "CompareMethodResult ToUpper")]
    public async Task Translate_CompareMethodResult_IgnoresCase(string method, string value)
    {
        // ARRANGE
        using Context context = await this.OpenDatabaseAsync(useNoCaseReplacement: true, useCaseInsensitiveCollation: true, value).ConfigureAwait(false);
        IQueryable<TestTable1> table = context.TestTable1;

        IQueryable<TestTable1> query = method switch
        {
            "Replace" => table.Where(e => e.TestField.Replace("x", "y") == "ÉcOlE"),
            "Trim" => table.Where(e => e.TestField.Trim('x') == "ÉcOlE"),
            "TrimStart" => table.Where(e => e.TestField.TrimStart('x') == "ÉcOlE"),
            "TrimEnd" => table.Where(e => e.TestField.TrimEnd('x') == "ÉcOlE"),
            "TrimWhitespace" => table.Where(e => e.TestField.Trim() == "ÉcOlE"),
            "TrimStartWhitespace" => table.Where(e => e.TestField.TrimStart() == "ÉcOlE"),
            "TrimEndWhitespace" => table.Where(e => e.TestField.TrimEnd() == "ÉcOlE"),
            "Substring" => table.Where(e => e.TestField.Substring(1) == "ÉcOlE"),
            "SubstringLength" => table.Where(e => e.TestField.Substring(1, 5) == "ÉcOlE"),
            "ToLower" => table.Where(e => e.TestField.ToLower() == "ÉcOlE"),
            "ToUpper" => table.Where(e => e.TestField.ToUpper() == "ÉcOlE"),
            _ => throw new ArgumentOutOfRangeException(nameof(method), method, "Unknown method"),
        };

        // ACT
        string sql = query.ToQueryString();
        int count = await query.CountAsync(this._testContext.CancellationToken).ConfigureAwait(false);

        // ASSERT
        Assert.Contains($"COLLATE \"{SqliteUnicodeFunctionNames.NoCaseCollation}\"", sql, "Result doesn't have the NOCASE collation");
        Assert.AreEqual(1, count, "Invalid count");
    }

    /// <summary>
    /// Test a string method called on the result of another string method on a NOCASE column also uses the nocase_* functions,
    /// as the result keeps the NOCASE collation.
    /// </summary>
    /// <param name="method">
    /// The method: Replace (Replace("x", "y")), Trim, TrimStart or TrimEnd (with 'x'), TrimWhitespace, TrimStartWhitespace or TrimEndWhitespace
    /// (Trim(), TrimStart() or TrimEnd()), Substring (Substring(1)), SubstringLength (Substring(1, 10)), ToLower or ToUpper, followed by Contains("ÉcOlE").
    /// </param>
    /// <param name="value">The value stored in the database.</param>
    /// <returns>A <see cref="Task"/> that represents the asynchronous operation.</returns>
    [TestMethod]
    [DataRow("Replace", "Body école", DisplayName = "MethodOnMethodResult Replace then Contains")]
    [DataRow("Trim", "xBody écolex", DisplayName = "MethodOnMethodResult Trim(char) then Contains")]
    [DataRow("TrimStart", "xBody école", DisplayName = "MethodOnMethodResult TrimStart(char) then Contains")]
    [DataRow("TrimEnd", "Body écolex", DisplayName = "MethodOnMethodResult TrimEnd(char) then Contains")]
    [DataRow("TrimWhitespace", "  Body école  ", DisplayName = "MethodOnMethodResult Trim() then Contains")]
    [DataRow("TrimStartWhitespace", "  Body école", DisplayName = "MethodOnMethodResult TrimStart() then Contains")]
    [DataRow("TrimEndWhitespace", "Body école  ", DisplayName = "MethodOnMethodResult TrimEnd() then Contains")]
    [DataRow("Substring", "xBody école", DisplayName = "MethodOnMethodResult Substring(int) then Contains")]
    [DataRow("SubstringLength", "xBody écolex", DisplayName = "MethodOnMethodResult Substring(int, int) then Contains")]
    [DataRow("ToLower", "BODY ÉCOLE", DisplayName = "MethodOnMethodResult ToLower then Contains")]
    [DataRow("ToUpper", "body école", DisplayName = "MethodOnMethodResult ToUpper then Contains")]
    [SuppressMessage("Globalization", "CA1304:Specify CultureInfo", Justification = "EF Core translates ToLower() / ToUpper(), but not the overloads that take a culture.")]
    [SuppressMessage("Globalization", "CA1311:Specify a culture or use an invariant version", Justification = "EF Core translates ToLower() / ToUpper(), but not the overloads that take a culture.")]
    [SuppressMessage("Performance", "CA1862:Use the 'StringComparison' method overloads to perform case-insensitive string comparisons", Justification = "The test checks the translation of ToLower() / ToUpper(); EF Core doesn't translate Contains with a StringComparison.")]
    public async Task Translate_MethodOnMethodResult_UsesNoCaseFunctions(string method, string value)
    {
        // ARRANGE
        using Context context = await this.OpenDatabaseAsync(useNoCaseReplacement: true, useCaseInsensitiveCollation: true, value).ConfigureAwait(false);
        IQueryable<TestTable1> table = context.TestTable1;

        IQueryable<TestTable1> query = method switch
        {
            "Replace" => table.Where(e => e.TestField.Replace("x", "y").Contains("ÉcOlE")),
            "Trim" => table.Where(e => e.TestField.Trim('x').Contains("ÉcOlE")),
            "TrimStart" => table.Where(e => e.TestField.TrimStart('x').Contains("ÉcOlE")),
            "TrimEnd" => table.Where(e => e.TestField.TrimEnd('x').Contains("ÉcOlE")),
            "TrimWhitespace" => table.Where(e => e.TestField.Trim().Contains("ÉcOlE")),
            "TrimStartWhitespace" => table.Where(e => e.TestField.TrimStart().Contains("ÉcOlE")),
            "TrimEndWhitespace" => table.Where(e => e.TestField.TrimEnd().Contains("ÉcOlE")),
            "Substring" => table.Where(e => e.TestField.Substring(1).Contains("ÉcOlE")),
            "SubstringLength" => table.Where(e => e.TestField.Substring(1, 10).Contains("ÉcOlE")),
            "ToLower" => table.Where(e => e.TestField.ToLower().Contains("ÉcOlE")),
            "ToUpper" => table.Where(e => e.TestField.ToUpper().Contains("ÉcOlE")),
            _ => throw new ArgumentOutOfRangeException(nameof(method), method, "Unknown method"),
        };

        // ACT
        string sql = query.ToQueryString();
        int count = await query.CountAsync(this._testContext.CancellationToken).ConfigureAwait(false);

        // ASSERT
        Assert.Contains($"{SqliteUnicodeFunctionNames.NoCaseInstrFunction}(", sql, "nocase_instr not used");
        Assert.AreEqual(1, count, "Invalid count");
    }

    /// <summary>
    /// Test the result of a concatenation, ?? or conditional expression on a NOCASE column keeps the NOCASE collation, so comparing it ignores case,
    /// as it does on SQL Server.
    /// </summary>
    /// <param name="expression">
    /// The expression: Concat (TestField + "X"), ConcatMethod (string.Concat(TestField, "X")), ConcatThree (TestField + "-" + TestField),
    /// Conditional (Id > 0 ? TestField : "z") or Coalesce ((Id > 1000 ? null : TestField) ?? "z").
    /// </param>
    /// <param name="compareTo">The value to compare the result with, which only matches it ignoring case.</param>
    /// <returns>A <see cref="Task"/> that represents the asynchronous operation.</returns>
    [TestMethod]
    [DataRow("Concat", "ÉcOlEx", DisplayName = "CompareCombination +")]
    [DataRow("ConcatMethod", "ÉcOlEx", DisplayName = "CompareCombination string.Concat")]
    [DataRow("ConcatThree", "ÉcOlE-ÉCOLE", DisplayName = "CompareCombination + with three operands")]
    [DataRow("Conditional", "ÉcOlE", DisplayName = "CompareCombination Conditional")]
    [DataRow("Coalesce", "ÉcOlE", DisplayName = "CompareCombination ??")]
    public async Task Translate_CompareCombination_IgnoresCase(string expression, string compareTo)
    {
        // ARRANGE
        using Context context = await this.OpenDatabaseAsync(useNoCaseReplacement: true, useCaseInsensitiveCollation: true, "école").ConfigureAwait(false);
        IQueryable<TestTable1> query = GetCombinationQuery(context.TestTable1, expression, compareTo, contains: false);

        // ACT
        string sql = query.ToQueryString();
        int count = await query.CountAsync(this._testContext.CancellationToken).ConfigureAwait(false);

        // ASSERT
        Assert.Contains($"COLLATE \"{SqliteUnicodeFunctionNames.NoCaseCollation}\"", sql, "Result doesn't have the NOCASE collation");
        Assert.AreEqual(1, count, "Invalid count");
    }

    /// <summary>
    /// Test Contains on the result of a concatenation, ?? or conditional expression on a NOCASE column uses nocase_instr.
    /// </summary>
    /// <param name="expression">The expression (see <see cref="Translate_CompareCombination_IgnoresCase"/>).</param>
    /// <returns>A <see cref="Task"/> that represents the asynchronous operation.</returns>
    [TestMethod]
    [DataRow("Concat", DisplayName = "ContainsOnCombination +")]
    [DataRow("ConcatMethod", DisplayName = "ContainsOnCombination string.Concat")]
    [DataRow("ConcatThree", DisplayName = "ContainsOnCombination + with three operands")]
    [DataRow("Conditional", DisplayName = "ContainsOnCombination Conditional")]
    [DataRow("Coalesce", DisplayName = "ContainsOnCombination ??")]
    public async Task Translate_ContainsOnCombination_UsesNoCaseInstr(string expression)
    {
        // ARRANGE
        using Context context = await this.OpenDatabaseAsync(useNoCaseReplacement: true, useCaseInsensitiveCollation: true, "école").ConfigureAwait(false);
        IQueryable<TestTable1> query = GetCombinationQuery(context.TestTable1, expression, "ÉcOlE", contains: true);

        // ACT
        string sql = query.ToQueryString();
        int count = await query.CountAsync(this._testContext.CancellationToken).ConfigureAwait(false);

        // ASSERT
        Assert.Contains($"{SqliteUnicodeFunctionNames.NoCaseInstrFunction}(", sql, "nocase_instr not used");
        Assert.AreEqual(1, count, "Invalid count");
    }

    /// <summary>
    /// Test a concatenation, ?? or conditional expression on a column without NOCASE is left as EF Core translates it, so comparing it is case sensitive.
    /// </summary>
    /// <param name="expression">The expression (see <see cref="Translate_CompareCombination_IgnoresCase"/>).</param>
    /// <param name="compareTo">The value to compare the result with, which only matches it ignoring case.</param>
    /// <returns>A <see cref="Task"/> that represents the asynchronous operation.</returns>
    [TestMethod]
    [DataRow("Concat", "ÉcOlEx", DisplayName = "CombinationWithoutNoCase +")]
    [DataRow("ConcatMethod", "ÉcOlEx", DisplayName = "CombinationWithoutNoCase string.Concat")]
    [DataRow("ConcatThree", "ÉcOlE-ÉCOLE", DisplayName = "CombinationWithoutNoCase + with three operands")]
    [DataRow("Conditional", "ÉcOlE", DisplayName = "CombinationWithoutNoCase Conditional")]
    [DataRow("Coalesce", "ÉcOlE", DisplayName = "CombinationWithoutNoCase ??")]
    public async Task Translate_CombinationWithoutNoCase_IsUnchanged(string expression, string compareTo)
    {
        // ARRANGE
        using Context context = await this.OpenDatabaseAsync(useNoCaseReplacement: true, useCaseInsensitiveCollation: false, "école").ConfigureAwait(false);
        IQueryable<TestTable1> query = GetCombinationQuery(context.TestTable1, expression, compareTo, contains: false);

        // ACT
        string sql = query.ToQueryString();
        int count = await query.CountAsync(this._testContext.CancellationToken).ConfigureAwait(false);

        // ASSERT
        Assert.DoesNotContain("COLLATE", sql, "Unexpected collation");
        Assert.AreEqual(0, count, "Invalid count");
    }

    /// <summary>
    /// Test a concatenation EF Core can't translate in the final projection still works (EF Core evaluates it on the client), as it does
    /// without UseSqliteNoCaseReplacement.
    /// </summary>
    /// <returns>A <see cref="Task"/> that represents the asynchronous operation.</returns>
    [TestMethod]
    public async Task Translate_ClientEvaluatedConcatenation_StillWorks()
    {
        // ARRANGE
        using Context context = await this.OpenDatabaseAsync(useNoCaseReplacement: true, useCaseInsensitiveCollation: true, "école").ConfigureAwait(false);
        IQueryable<string> query = context.TestTable1.Select(e => e.TestField + FormatId(e.Id));

        // ACT
        string sql = query.ToQueryString();
        string result = await query.SingleAsync(this._testContext.CancellationToken).ConfigureAwait(false);

        // ASSERT
        Assert.DoesNotContain("COLLATE", sql, "The concatenation was translated");
        Assert.AreEqual("école001", result, "Invalid result");
    }

    /// <summary>
    /// Test a NOCASE property of a complex type, or of a nested complex type, uses the nocase_* functions, whether the collation was set by
    /// UseCaseInsensitiveCollation or by UseCollation.
    /// </summary>
    /// <param name="nested"><see langword="true"/> to search the nested complex type's property, <see langword="false"/> the complex type's.</param>
    /// <param name="useCaseInsensitiveCollation">
    /// <see langword="true"/> to set NOCASE with UseCaseInsensitiveCollation, <see langword="false"/> with UseCollation on each property.
    /// </param>
    /// <returns>A <see cref="Task"/> that represents the asynchronous operation.</returns>
    [TestMethod]
    [DataRow(false, true, DisplayName = "ComplexTypeProperty Complex type, UseCaseInsensitiveCollation")]
    [DataRow(true, true, DisplayName = "ComplexTypeProperty Nested complex type, UseCaseInsensitiveCollation")]
    [DataRow(false, false, DisplayName = "ComplexTypeProperty Complex type, UseCollation")]
    [DataRow(true, false, DisplayName = "ComplexTypeProperty Nested complex type, UseCollation")]
    public async Task Translate_ComplexTypeProperty_UsesNoCaseInstr(bool nested, bool useCaseInsensitiveCollation)
    {
        // ARRANGE
        using Context context = await this.OpenComplexTypeDatabaseAsync(useCaseInsensitiveCollation).ConfigureAwait(false);
        IQueryable<CollationOwner> query = nested
            ? context.Set<CollationOwner>().Where(e => e.Part.SubPart.Text!.Contains("école"))
            : context.Set<CollationOwner>().Where(e => e.Part.Text!.Contains("école"));

        // ACT
        string sql = query.ToQueryString();
        int count = await query.CountAsync(this._testContext.CancellationToken).ConfigureAwait(false);

        // ASSERT
        Assert.Contains($"{SqliteUnicodeFunctionNames.NoCaseInstrFunction}(", sql, "nocase_instr not used");
        Assert.AreEqual(1, count, "Invalid count");
    }

    /// <summary>
    /// Test Contains on a NOCASE column's value returned by a subquery (FirstOrDefault, Max or Min) uses the nocase_* functions.
    /// </summary>
    /// <param name="method">The method returning the value (see <see cref="GetSubqueryQuery"/>).</param>
    /// <returns>A <see cref="Task"/> that represents the asynchronous operation.</returns>
    [TestMethod]
    [DataRow("FirstOrDefault", DisplayName = "ContainsOnSubqueryValue FirstOrDefault")]
    [DataRow("Max", DisplayName = "ContainsOnSubqueryValue Max")]
    [DataRow("Min", DisplayName = "ContainsOnSubqueryValue Min")]
    public async Task Translate_ContainsOnSubqueryValue_UsesNoCaseInstr(string method)
    {
        // ARRANGE
        using Context context = await this.OpenNavigationDatabaseAsync().ConfigureAwait(false);
        IQueryable<NavigationParent> query = GetSubqueryQuery(context.Set<NavigationParent>(), method, "école", contains: true);

        // ACT
        string sql = query.ToQueryString();
        int count = await query.CountAsync(this._testContext.CancellationToken).ConfigureAwait(false);

        // ASSERT
        Assert.Contains($"{SqliteUnicodeFunctionNames.NoCaseInstrFunction}(", sql, "nocase_instr not used");
        Assert.AreEqual(1, count, "Invalid count");
    }

    /// <summary>
    /// Test comparing a NOCASE column's value returned by a subquery (FirstOrDefault, Max or Min) ignores case, as on SQL Server, where the
    /// value keeps the column's collation. SQLite gives a subquery's value no collation, so it's given NOCASE explicitly.
    /// </summary>
    /// <param name="method">The method returning the value (see <see cref="GetSubqueryQuery"/>).</param>
    /// <returns>A <see cref="Task"/> that represents the asynchronous operation.</returns>
    [TestMethod]
    [DataRow("FirstOrDefault", DisplayName = "CompareSubqueryValue FirstOrDefault")]
    [DataRow("Max", DisplayName = "CompareSubqueryValue Max")]
    [DataRow("Min", DisplayName = "CompareSubqueryValue Min")]
    public async Task Translate_CompareSubqueryValue_IgnoresCase(string method)
    {
        // ARRANGE
        using Context context = await this.OpenNavigationDatabaseAsync().ConfigureAwait(false);
        IQueryable<NavigationParent> query = GetSubqueryQuery(context.Set<NavigationParent>(), method, "child école", contains: false);

        // ACT
        string sql = query.ToQueryString();
        int count = await query.CountAsync(this._testContext.CancellationToken).ConfigureAwait(false);

        // ASSERT
        Assert.Contains(@"COLLATE ""NOCASE""", sql, "The subquery's value wasn't given the NOCASE collation");
        Assert.AreEqual(1, count, "Invalid count");
    }

    /// <summary>
    /// Test Contains on, and comparing, a value returned by a subquery from a column without NOCASE keeps EF Core's translation, and is
    /// case sensitive.
    /// </summary>
    /// <param name="method">The method returning the value (see <see cref="GetSubqueryQuery"/>).</param>
    /// <returns>A <see cref="Task"/> that represents the asynchronous operation.</returns>
    [TestMethod]
    [DataRow("FirstOrDefault", DisplayName = "SubqueryValueWithoutNoCase FirstOrDefault")]
    [DataRow("Max", DisplayName = "SubqueryValueWithoutNoCase Max")]
    [DataRow("Min", DisplayName = "SubqueryValueWithoutNoCase Min")]
    public async Task Translate_SubqueryValueWithoutNoCase_IsUnchanged(string method)
    {
        // ARRANGE
        using Context context = await this.OpenNavigationDatabaseAsync(useCaseInsensitiveCollation: false).ConfigureAwait(false);
        IQueryable<NavigationParent> containsQuery = GetSubqueryQuery(context.Set<NavigationParent>(), method, "école", contains: true);
        IQueryable<NavigationParent> compareQuery = GetSubqueryQuery(context.Set<NavigationParent>(), method, "child école", contains: false);

        // ACT
        string containsSql = containsQuery.ToQueryString();
        string compareSql = compareQuery.ToQueryString();
        int containsCount = await containsQuery.CountAsync(this._testContext.CancellationToken).ConfigureAwait(false);
        int compareCount = await compareQuery.CountAsync(this._testContext.CancellationToken).ConfigureAwait(false);

        // ASSERT
        Assert.DoesNotContain(SqliteUnicodeFunctionNames.NoCasePrefix, containsSql, "nocase_* function used");
        Assert.DoesNotContain("COLLATE", compareSql, "A collation was added");
        Assert.AreEqual(0, containsCount, "Invalid Contains count");
        Assert.AreEqual(0, compareCount, "Invalid compare count");
    }

    /// <summary>
    /// Test a query whose own last operator returns a string (MaxAsync, FirstOrDefaultAsync), and a subquery's value in the final projection,
    /// still work. The query's own last operator returns the query's result, not a subquery's, so it isn't given the NOCASE collation.
    /// </summary>
    /// <returns>A <see cref="Task"/> that represents the asynchronous operation.</returns>
    [TestMethod]
    public async Task Translate_StringResultOperator_StillWorks()
    {
        // ARRANGE
        using Context context = await this.OpenNavigationDatabaseAsync().ConfigureAwait(false);

        // ACT
        string? max = await context.Set<NavigationChild>()
            .MaxAsync(c => c.Name, this._testContext.CancellationToken)
            .ConfigureAwait(false);

        string? first = await context.Set<NavigationChild>()
            .OrderBy(c => c.Id)
            .Select(c => c.Name)
            .FirstOrDefaultAsync(this._testContext.CancellationToken)
            .ConfigureAwait(false);

        List<string?> projected = await context.Set<NavigationParent>()
            .Select(p => p.Children.Max(c => c.Name))
            .ToListAsync(this._testContext.CancellationToken)
            .ConfigureAwait(false);

        // ASSERT
        Assert.AreEqual("Child ÉCOLE", max, "Invalid MaxAsync result");
        Assert.AreEqual("Child ÉCOLE", first, "Invalid FirstOrDefaultAsync result");
        Assert.HasCount(1, projected, "Invalid projection count");
        Assert.AreEqual("Child ÉCOLE", projected[0], "Invalid projection result");
    }

    #endregion Translate tests

    #region private methods

    /// <summary>
    /// Formats an Id as three digits. A normal .NET method, which EF Core can't translate, so it evaluates it on the client.
    /// </summary>
    /// <param name="id">The Id.</param>
    /// <returns>The formatted Id.</returns>
    private static string FormatId(long id)
    {
        return id.ToString("D3", CultureInfo.InvariantCulture);
    }

    /// <summary>
    /// Gets a query that compares, or calls Contains on, a concatenation, ?? or conditional expression on TestTable1.TestField.
    /// </summary>
    /// <param name="table">The table.</param>
    /// <param name="expression">The expression (see <see cref="Translate_CompareCombination_IgnoresCase"/>).</param>
    /// <param name="value">The value to compare the expression with, or to search for.</param>
    /// <param name="contains"><see langword="true"/> to call Contains(value) on the expression, <see langword="false"/> to compare it with value.</param>
    /// <returns>The query.</returns>
    private static IQueryable<TestTable1> GetCombinationQuery(IQueryable<TestTable1> table, string expression, string value, bool contains)
    {
        return (expression, contains) switch
        {
            ("Concat", false) => table.Where(e => e.TestField + "X" == value),
            ("Concat", true) => table.Where(e => (e.TestField + "X").Contains(value)),
            ("ConcatMethod", false) => table.Where(e => string.Concat(e.TestField, "X") == value),
            ("ConcatMethod", true) => table.Where(e => string.Concat(e.TestField, "X").Contains(value)),
            ("ConcatThree", false) => table.Where(e => e.TestField + "-" + e.TestField == value),
            ("ConcatThree", true) => table.Where(e => (e.TestField + "-" + e.TestField).Contains(value)),
            ("Conditional", false) => table.Where(e => (e.Id > 0 ? e.TestField : "z") == value),
            ("Conditional", true) => table.Where(e => (e.Id > 0 ? e.TestField : "z").Contains(value)),
            ("Coalesce", false) => table.Where(e => ((e.Id > 1000 ? null : e.TestField) ?? "z") == value),
            ("Coalesce", true) => table.Where(e => ((e.Id > 1000 ? null : e.TestField) ?? "z").Contains(value)),
            _ => throw new ArgumentOutOfRangeException(nameof(expression), expression, "Unknown expression"),
        };
    }

    /// <summary>
    /// Gets a query that calls Contains on, or compares, the name of a <see cref="NavigationParent"/>'s child returned by a subquery.
    /// </summary>
    /// <param name="parents">The parents.</param>
    /// <param name="method">The method returning the child's name: FirstOrDefault (of the names ordered by Id), Max or Min.</param>
    /// <param name="value">The value to search for, or to compare the name with.</param>
    /// <param name="contains"><see langword="true"/> to call Contains(value) on the name, <see langword="false"/> to compare it with value.</param>
    /// <returns>The query.</returns>
    private static IQueryable<NavigationParent> GetSubqueryQuery(IQueryable<NavigationParent> parents, string method, string value, bool contains)
    {
        return (method, contains) switch
        {
            ("FirstOrDefault", true) => parents.Where(p => p.Children.OrderBy(c => c.Id).Select(c => c.Name).FirstOrDefault()!.Contains(value)),
            ("FirstOrDefault", false) => parents.Where(p => p.Children.OrderBy(c => c.Id).Select(c => c.Name).FirstOrDefault() == value),
            ("Max", true) => parents.Where(p => p.Children.Max(c => c.Name)!.Contains(value)),
            ("Max", false) => parents.Where(p => p.Children.Max(c => c.Name) == value),
            ("Min", true) => parents.Where(p => p.Children.Min(c => c.Name)!.Contains(value)),
            ("Min", false) => parents.Where(p => p.Children.Min(c => c.Name) == value),
            _ => throw new ArgumentOutOfRangeException(nameof(method), method, "Unknown method"),
        };
    }

    /// <summary>
    /// Gets a query that calls a string method on TestTable1.TestField.
    /// </summary>
    /// <param name="table">The table.</param>
    /// <param name="method">
    /// The method: Contains, ContainsChar (Contains('é')), IndexOf, IndexOfChar (IndexOf('é')), IndexOfStartIndex (IndexOf(argument, 1)),
    /// IndexOfCharStartIndex (IndexOf('é', 1)), StartsWith, StartsWithChar (StartsWith('é')), EndsWith, EndsWithChar (EndsWith('é')),
    /// Like (EF.Functions.Like), LikeEscape (EF.Functions.Like with ! as the escape character), Replace (Replace(argument, "x")),
    /// ReplaceChar (Replace('é', 'x')), Trim, TrimStart and TrimEnd (with the constant 'é'), TrimArray, TrimStartArray and TrimEndArray
    /// (with the constant characters 'é' and 'x'), ToLower or ToUpper.
    /// </param>
    /// <param name="argument">The argument to the method, if it takes one that isn't a constant.</param>
    /// <returns>The query: the matching values for a method used in a Where, otherwise the method's result.</returns>
    [SuppressMessage("Globalization", "CA1304:Specify CultureInfo", Justification = "EF Core translates ToLower() / ToUpper(), but not the overloads that take a culture.")]
    [SuppressMessage("Globalization", "CA1311:Specify a culture or use an invariant version", Justification = "EF Core translates ToLower() / ToUpper(), but not the overloads that take a culture.")]
    [SuppressMessage("Performance", "CA1861:Avoid constant arrays as arguments", Justification = "EF Core only translates Trim(char[]) when the array is a constant in the query, not a field.")]
    private static IQueryable<object> GetMethodQuery(IQueryable<TestTable1> table, string method, string? argument)
    {
        return method switch
        {
            "Contains" => table.Where(e => e.TestField.Contains(argument!)).Select(e => (object)e.TestField),
            "ContainsChar" => table.Where(e => e.TestField.Contains('é')).Select(e => (object)e.TestField),
            "IndexOf" => table.Select(e => (object)e.TestField.IndexOf(argument!)),
            "IndexOfChar" => table.Select(e => (object)e.TestField.IndexOf('é')),
            "IndexOfStartIndex" => table.Select(e => (object)e.TestField.IndexOf(argument!, 1)),
            "IndexOfCharStartIndex" => table.Select(e => (object)e.TestField.IndexOf('é', 1)),
            "StartsWith" => table.Where(e => e.TestField.StartsWith(argument!)).Select(e => (object)e.TestField),
            "StartsWithChar" => table.Where(e => e.TestField.StartsWith('é')).Select(e => (object)e.TestField),
            "EndsWith" => table.Where(e => e.TestField.EndsWith(argument!)).Select(e => (object)e.TestField),
            "EndsWithChar" => table.Where(e => e.TestField.EndsWith('é')).Select(e => (object)e.TestField),
            "Like" => table.Where(e => EF.Functions.Like(e.TestField, argument!)).Select(e => (object)e.TestField),
            "LikeEscape" => table.Where(e => EF.Functions.Like(e.TestField, argument!, "!")).Select(e => (object)e.TestField),
            "Replace" => table.Select(e => (object)e.TestField.Replace(argument!, "x")),
            "ReplaceChar" => table.Select(e => (object)e.TestField.Replace('é', 'x')),
            "Trim" => table.Select(e => (object)e.TestField.Trim('é')),
            "TrimArray" => table.Select(e => (object)e.TestField.Trim(new[] { 'é', 'x' })),
            "TrimStart" => table.Select(e => (object)e.TestField.TrimStart('é')),
            "TrimStartArray" => table.Select(e => (object)e.TestField.TrimStart(new[] { 'é', 'x' })),
            "TrimEnd" => table.Select(e => (object)e.TestField.TrimEnd('é')),
            "TrimEndArray" => table.Select(e => (object)e.TestField.TrimEnd(new[] { 'é', 'x' })),
            "ToLower" => table.Select(e => (object)e.TestField.ToLower()),
            "ToUpper" => table.Select(e => (object)e.TestField.ToUpper()),
            _ => throw new ArgumentOutOfRangeException(nameof(method), method, "Unknown method"),
        };
    }

    /// <summary>
    /// Gets the result of a query from <see cref="GetMethodQuery"/>.
    /// </summary>
    /// <param name="query">The query.</param>
    /// <returns>The first result as a string, or <see langword="null"/> if there is none.</returns>
    private async Task<string?> GetResultAsync(IQueryable<object> query)
    {
        object? result = await query.FirstOrDefaultAsync(this._testContext.CancellationToken).ConfigureAwait(false);
        return result is null
            ? null
            : Convert.ToString(result, CultureInfo.InvariantCulture);
    }

    /// <summary>
    /// Opens a new SQLite test database with a single row in TestTable1.
    /// </summary>
    /// <param name="useNoCaseReplacement">Whether to call UseSqliteNoCaseReplacement.</param>
    /// <param name="useCaseInsensitiveCollation">Whether to call UseCaseInsensitiveCollation, which gives TestField the NOCASE collation.</param>
    /// <param name="value">The value of TestField.</param>
    /// <param name="addNavigationEntities">Whether to add <see cref="NavigationParent"/> and <see cref="NavigationChild"/> to the model.</param>
    /// <returns>The database context.</returns>
    private async Task<Context> OpenDatabaseAsync(bool useNoCaseReplacement, bool useCaseInsensitiveCollation, string value, bool addNavigationEntities = false)
    {
        Context context = await DatabaseUtils.OpenDatabaseAsync(
            this._testContext,
            DatabaseTypes.Sqlite,
            customConfigurationActions: optionsBuilder =>
            {
                if (useNoCaseReplacement)
                {
                    optionsBuilder.UseSqliteNoCaseReplacement();
                }
            },
            customModelCreationActions: modelBuilder =>
            {
                // Before UseCaseInsensitiveCollation, which only sets the collation on the entity types already in the model.
                if (addNavigationEntities)
                {
                    modelBuilder.Entity<NavigationParent>();
                    modelBuilder.Entity<NavigationChild>();
                }

                if (useCaseInsensitiveCollation)
                {
                    modelBuilder.UseCaseInsensitiveCollation(DatabaseTypes.Sqlite);
                }
            })
            .ConfigureAwait(false);

        context.TestTable1.Add(new TestTable1 { TestField = value });
        await context.SaveChangesAsync(this._testContext.CancellationToken).ConfigureAwait(false);

        return context;
    }

    /// <summary>
    /// Opens a new SQLite test database with the NOCASE replacement, and a <see cref="NavigationParent"/> with one <see cref="NavigationChild"/>,
    /// both with upper case names.
    /// </summary>
    /// <param name="useCaseInsensitiveCollation">Whether to call UseCaseInsensitiveCollation, which gives the names the NOCASE collation.</param>
    /// <returns>The database context.</returns>
    private async Task<Context> OpenNavigationDatabaseAsync(bool useCaseInsensitiveCollation = true)
    {
        Context context = await this.OpenDatabaseAsync(useNoCaseReplacement: true, useCaseInsensitiveCollation, "First", addNavigationEntities: true).ConfigureAwait(false);

        NavigationParent parent = new() { Name = "Parent ÉCOLE" };
        parent.Children.Add(new NavigationChild { Name = "Child ÉCOLE" });

        context.Add(parent);
        await context.SaveChangesAsync(this._testContext.CancellationToken).ConfigureAwait(false);

        return context;
    }

    /// <summary>
    /// Opens a new SQLite test database with the NOCASE replacement, and a <see cref="CollationOwner"/> whose <see cref="CollationPart"/> (a complex
    /// type) and <see cref="CollationSubPart"/> (a nested complex type) have NOCASE text, in upper case.
    /// </summary>
    /// <param name="useCaseInsensitiveCollation">
    /// <see langword="true"/> to set NOCASE with UseCaseInsensitiveCollation, <see langword="false"/> with UseCollation on each property.
    /// </param>
    /// <returns>The database context.</returns>
    private async Task<Context> OpenComplexTypeDatabaseAsync(bool useCaseInsensitiveCollation)
    {
        Context context = await DatabaseUtils.OpenDatabaseAsync(
            this._testContext,
            DatabaseTypes.Sqlite,
            customConfigurationActions: optionsBuilder => optionsBuilder.UseSqliteNoCaseReplacement(),
            customModelCreationActions: modelBuilder =>
            {
                modelBuilder.Entity<CollationOwner>().ComplexProperty(
                    e => e.Part,
                    part =>
                    {
                        part.ComplexProperty(p => p.SubPart);

                        if (!useCaseInsensitiveCollation)
                        {
                            part.Property(p => p.Text).UseCollation(SqliteUnicodeFunctionNames.NoCaseCollation);
                            part.ComplexProperty(p => p.SubPart).Property(p => p.Text).UseCollation(SqliteUnicodeFunctionNames.NoCaseCollation);
                        }
                    });

                // After CollationOwner is added, as UseCaseInsensitiveCollation only sets the collation on the entity types already in the model.
                if (useCaseInsensitiveCollation)
                {
                    modelBuilder.UseCaseInsensitiveCollation(DatabaseTypes.Sqlite);
                }
            })
            .ConfigureAwait(false);

        context.Add(new CollationOwner { Part = new CollationPart { Text = "Part ÉCOLE", SubPart = new CollationSubPart { Text = "SubPart ÉCOLE" } } });
        await context.SaveChangesAsync(this._testContext.CancellationToken).ConfigureAwait(false);

        return context;
    }

    #endregion private methods
}
