// Copyright ©2021-2026 Mike King.
// This file is licensed to you under the MIT license.
// See the License.txt file in the solution root for more information.

using DotDoc.EntityFrameworkCore.Extensions.CaseInsensitivity;
using DotDoc.EntityFrameworkCore.Extensions.DatabaseType;
using DotDoc.EntityFrameworkCore.Extensions.Tests.Data;
using Microsoft.EntityFrameworkCore;
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
    [TestMethod(DisplayName = "NOCASE column uses the nocase_* functions")]
    [DataRow("Contains", "Body ÉCOLE", "école", $"{SqliteUnicodeFunctionNames.NoCaseInstrFunction}(", "Body ÉCOLE", DisplayName = "Contains")]
    [DataRow("ContainsChar", "Body ÉCOLE", null, $"{SqliteUnicodeFunctionNames.NoCaseInstrFunction}(", "Body ÉCOLE", DisplayName = "Contains(char)")]
    [DataRow("IndexOf", "Body ÉCOLE", "école", $"{SqliteUnicodeFunctionNames.NoCaseInstrFunction}(", "5", DisplayName = "IndexOf")]
    [DataRow("IndexOfStartIndex", "Body ÉCOLE", "école", $"{SqliteUnicodeFunctionNames.NoCaseInstrFunction}(", "5", DisplayName = "IndexOf(string, int)")]
    [DataRow("StartsWith", "École", "éc", $"{SqliteUnicodeFunctionNames.NoCaseStartsWithFunction}(", "École", DisplayName = "StartsWith")]
    [DataRow("StartsWith", "50% off", "50%", $"{SqliteUnicodeFunctionNames.NoCaseStartsWithFunction}(", "50% off", DisplayName = "StartsWith % is a plain character")]
    [DataRow("StartsWith", "abc", "%", $"{SqliteUnicodeFunctionNames.NoCaseStartsWithFunction}(", null, DisplayName = "StartsWith % is not a wildcard")]
    [DataRow("EndsWith", "Body ÉCOLE", "école", $"{SqliteUnicodeFunctionNames.NoCaseEndsWithFunction}(", "Body ÉCOLE", DisplayName = "EndsWith")]
    [DataRow("Like", "Body ÉCOLE", "%école", $"{SqliteUnicodeFunctionNames.NoCaseLikeFunction}(", "Body ÉCOLE", DisplayName = "EF.Functions.Like")]
    [DataRow("LikeEscape", "50% ÉCOLE", "50!% é%", $"{SqliteUnicodeFunctionNames.NoCaseLikeFunction}(", "50% ÉCOLE", DisplayName = "EF.Functions.Like with an escape character")]
    [DataRow("Replace", "École école", "ÉCOLE", $"{SqliteUnicodeFunctionNames.NoCaseReplaceFunction}(", "x x", DisplayName = "Replace")]
    [DataRow("Trim", "ÉabcÉ", null, $"{SqliteUnicodeFunctionNames.NoCaseTrimFunction}(", "abc", DisplayName = "Trim(char)")]
    [DataRow("TrimArray", "ÉxabcxÉ", null, $"{SqliteUnicodeFunctionNames.NoCaseTrimFunction}(", "abc", DisplayName = "Trim(char[])")]
    [DataRow("TrimStart", "ÉabcÉ", null, $"{SqliteUnicodeFunctionNames.NoCaseLTrimFunction}(", "abcÉ", DisplayName = "TrimStart(char)")]
    [DataRow("TrimEnd", "ÉabcÉ", null, $"{SqliteUnicodeFunctionNames.NoCaseRTrimFunction}(", "Éabc", DisplayName = "TrimEnd(char)")]
    public async Task SqliteNoCaseTranslatorTests_001_Async(string method, string value, string? argument, string expectedFunction, string? expected)
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
    [TestMethod(DisplayName = "Column without NOCASE keeps EF Core's translation")]
    [DataRow("Contains", "Body ÉCOLE", "école", "instr(", null, DisplayName = "Contains")]
    [DataRow("IndexOf", "Body ÉCOLE", "école", "instr(", "-1", DisplayName = "IndexOf")]
    [DataRow("StartsWith", "École", "éc", " LIKE ", null, DisplayName = "StartsWith")]
    [DataRow("EndsWith", "Body ÉCOLE", "école", " LIKE ", null, DisplayName = "EndsWith")]
    [DataRow("Like", "Body ÉCOLE", "%école", " LIKE ", null, DisplayName = "EF.Functions.Like")]
    [DataRow("Replace", "École école", "ÉCOLE", "replace(", "École école", DisplayName = "Replace")]
    [DataRow("Trim", "ÉabcÉ", null, "trim(", "ÉabcÉ", DisplayName = "Trim(char)")]
    public async Task SqliteNoCaseTranslatorTests_002_Async(string method, string value, string? argument, string expectedSql, string? expected)
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
    [TestMethod(DisplayName = "ToLower and ToUpper ignore the collation")]
    [DataRow("ToLower", "ÉCOLE", $"{SqliteUnicodeFunctionNames.LowerFunction}(", "école", DisplayName = "ToLower")]
    [DataRow("ToUpper", "école", $"{SqliteUnicodeFunctionNames.UpperFunction}(", "ÉCOLE", DisplayName = "ToUpper")]
    public async Task SqliteNoCaseTranslatorTests_003_Async(string method, string value, string expectedFunction, string expected)
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
    [TestMethod(DisplayName = "EF.Functions.Collate NOCASE uses the nocase_* functions")]
    public async Task SqliteNoCaseTranslatorTests_004_Async()
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
    [TestMethod(DisplayName = "Nothing is translated without UseSqliteNoCaseReplacement")]
    [DataRow("Contains", "école", DisplayName = "Contains")]
    [DataRow("IndexOf", "école", DisplayName = "IndexOf")]
    [DataRow("StartsWith", "éc", DisplayName = "StartsWith")]
    [DataRow("EndsWith", "école", DisplayName = "EndsWith")]
    [DataRow("Like", "%école", DisplayName = "EF.Functions.Like")]
    [DataRow("Replace", "ÉCOLE", DisplayName = "Replace")]
    [DataRow("Trim", null, DisplayName = "Trim(char)")]
    [DataRow("ToLower", null, DisplayName = "ToLower")]
    [DataRow("ToUpper", null, DisplayName = "ToUpper")]
    public async Task SqliteNoCaseTranslatorTests_005_Async(string method, string? argument)
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
    [TestMethod(DisplayName = "NOCASE column after a join")]
    public async Task SqliteNoCaseTranslatorTests_006_Async()
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
    /// Test an expression with no known collation (a concatenation) keeps EF Core's own translation, even when it includes a NOCASE column
    /// (SQLite does the same: only COLLATE carries a collation through an expression).
    /// </summary>
    /// <returns>A <see cref="Task"/> that represents the asynchronous operation.</returns>
    [TestMethod(DisplayName = "Expression with no known collation keeps EF Core's translation")]
    public async Task SqliteNoCaseTranslatorTests_007_Async()
    {
        // ARRANGE
        using Context context = await this.OpenDatabaseAsync(useNoCaseReplacement: true, useCaseInsensitiveCollation: true, "Body ÉCOLE").ConfigureAwait(false);
        IQueryable<TestTable1> query = context.TestTable1.Where(e => (e.TestField + "!").Contains("école"));

        // ACT
        string sql = query.ToQueryString();
        int count = await query.CountAsync(this._testContext.CancellationToken).ConfigureAwait(false);

        // ASSERT
        Assert.DoesNotContain(SqliteUnicodeFunctionNames.NoCasePrefix, sql, "nocase_* function used");
        Assert.AreEqual(0, count, "Invalid count");
    }

    #endregion Translate tests

    #region private methods

    /// <summary>
    /// Gets a query that calls a string method on TestTable1.TestField.
    /// </summary>
    /// <param name="table">The table.</param>
    /// <param name="method">
    /// The method: Contains, ContainsChar (Contains('é')), IndexOf, IndexOfStartIndex (IndexOf(argument, 1)), StartsWith, EndsWith,
    /// Like (EF.Functions.Like), LikeEscape (EF.Functions.Like with ! as the escape character), Replace (Replace(argument, "x")),
    /// Trim, TrimStart and TrimEnd (with the constant 'é'), TrimArray (Trim with the constant characters 'é' and 'x'), ToLower or ToUpper.
    /// </param>
    /// <param name="argument">The argument to the method, if it takes one that isn't a constant.</param>
    /// <returns>The query: the matching values for a method used in a Where, otherwise the method's result.</returns>
    private static IQueryable<object> GetMethodQuery(IQueryable<TestTable1> table, string method, string? argument)
    {
#pragma warning disable CA1304, CA1311 // EF Core translates ToLower() / ToUpper(), but not the overloads that take a culture.
#pragma warning disable CA1861 // EF Core only translates Trim(char[]) when the array is a constant in the query, not a field.
        return method switch
        {
            "Contains" => table.Where(e => e.TestField.Contains(argument!)).Select(e => (object)e.TestField),
            "ContainsChar" => table.Where(e => e.TestField.Contains('é')).Select(e => (object)e.TestField),
            "IndexOf" => table.Select(e => (object)e.TestField.IndexOf(argument!)),
            "IndexOfStartIndex" => table.Select(e => (object)e.TestField.IndexOf(argument!, 1)),
            "StartsWith" => table.Where(e => e.TestField.StartsWith(argument!)).Select(e => (object)e.TestField),
            "EndsWith" => table.Where(e => e.TestField.EndsWith(argument!)).Select(e => (object)e.TestField),
            "Like" => table.Where(e => EF.Functions.Like(e.TestField, argument!)).Select(e => (object)e.TestField),
            "LikeEscape" => table.Where(e => EF.Functions.Like(e.TestField, argument!, "!")).Select(e => (object)e.TestField),
            "Replace" => table.Select(e => (object)e.TestField.Replace(argument!, "x")),
            "Trim" => table.Select(e => (object)e.TestField.Trim('é')),
            "TrimArray" => table.Select(e => (object)e.TestField.Trim(new[] { 'é', 'x' })),
            "TrimStart" => table.Select(e => (object)e.TestField.TrimStart('é')),
            "TrimEnd" => table.Select(e => (object)e.TestField.TrimEnd('é')),
            "ToLower" => table.Select(e => (object)e.TestField.ToLower()),
            "ToUpper" => table.Select(e => (object)e.TestField.ToUpper()),
            _ => throw new ArgumentOutOfRangeException(nameof(method), method, "Unknown method"),
        };
#pragma warning restore CA1861
#pragma warning restore CA1304, CA1311
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
    /// <returns>The database context.</returns>
    private async Task<Context> OpenDatabaseAsync(bool useNoCaseReplacement, bool useCaseInsensitiveCollation, string value)
    {
        Context context = await DatabaseUtils.OpenDatabaseAsync(
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

    #endregion private methods
}
