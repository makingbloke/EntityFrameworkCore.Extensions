// Copyright ©2021-2026 Mike King.
// This file is licensed to you under the MIT license.
// See the License.txt file in the solution root for more information.

using DotDoc.EntityFrameworkCore.Extensions.CaseInsensitivity;
using DotDoc.EntityFrameworkCore.Extensions.DatabaseType;
using DotDoc.EntityFrameworkCore.Extensions.Tests.Data;
using Microsoft.EntityFrameworkCore;

namespace DotDoc.EntityFrameworkCore.Extensions.Tests.CaseInsensitivity;

/// <summary>
/// Tests for the UseSqliteNoCaseReplacement extension.
/// </summary>
[TestClass]
public class CaseInsensitivityTests
{
    #region private fields

    /// <summary>
    /// The MSTest <see cref="TestContext"/> for the current test.
    /// </summary>
    private readonly TestContext _testContext;

    #endregion private fields

    #region public constructors

    /// <summary>
    /// Initializes a new instance of the <see cref="CaseInsensitivityTests"/> class.
    /// </summary>
    /// <param name="testContext">The MSTest test context.</param>
    public CaseInsensitivityTests(TestContext testContext)
    {
        this._testContext = testContext;
    }

    #endregion public constructors

    #region public methods

    /// <summary>
    /// Test UseSqliteNoCaseReplacement with a Null DbContextOptionsBuilder OptionsBuilder parameter.
    /// </summary>
    [TestMethod(DisplayName = "UseSqliteNoCaseReplacement with a Null DbContextOptionsBuilder OptionsBuilder parameter")]
    public void CaseInsensitivityTests_001()
    {
        // ARRANGE
        DbContextOptionsBuilder optionsBuilder = null!;

        // ACT / ASSERT
        Assert.ThrowsExactly<ArgumentNullException>(() => optionsBuilder.UseSqliteNoCaseReplacement(), "Unexpected exception");
    }

    /// <summary>
    /// Test UseSqliteNoCaseReplacement with an unsupported database type.
    /// </summary>
    /// <returns>A <see cref="Task"/> that represents the asynchronous operation.</returns>
    [TestMethod(DisplayName = "UseSqliteNoCaseReplacement with an unsupported database type")]
    public async Task CaseInsensitivityTests_002_Async()
    {
        // ARRANGE

        // ACT / ASSERT
        await Assert.ThrowsExactlyAsync<UnsupportedDatabaseTypeException>(
            async () =>
            {
                using Context context = await DatabaseUtils.OpenDatabaseAsync(
                    DatabaseTypes.SqlServer,
                    customConfigurationActions: optionsBuilder => optionsBuilder.UseSqliteNoCaseReplacement())
                    .ConfigureAwait(false);
            },
            "Unexpected exception")
            .ConfigureAwait(false);
    }

    /// <summary>
    /// Test UseSqliteNoCaseReplacement is case insensitive but accent sensitive (matching SQL Server SQL_Latin1_General_CP1_CI_AS).
    /// </summary>
    /// <param name="originalValue">The value stored in the database.</param>
    /// <param name="searchValue">The value to search for.</param>
    /// <param name="expectedCount">The expected number of matching records.</param>
    /// <returns>A <see cref="Task"/> that represents the asynchronous operation.</returns>
    [TestMethod(DisplayName = "UseSqliteNoCaseReplacement")]
    [DataRow("\u00E1\u00E1\u00E1", "\u00C1\u00C1\u00C1", 1, DisplayName = "Accented lower case matches accented upper case")]
    [DataRow("\u00E1\u00E1\u00E1", "AAA", 0, DisplayName = "Accented does not match unaccented")]
    [DataRow("aaa", "\u00C1\u00C1\u00C1", 0, DisplayName = "Unaccented does not match accented")]
    [DataRow("abc", "ABC", 1, DisplayName = "ASCII lower case matches upper case")]
    [DataRow("\u00E6\u00F8\u00E5", "\u00C6\u00D8\u00C5", 1, DisplayName = "Non-ASCII lower case matches upper case")]
    [DataRow("\uFF21\uFF22\uFF23", "abc", 1, DisplayName = "Full width matches half width")]
    public async Task CaseInsensitivityTests_003_Async(string originalValue, string searchValue, int expectedCount)
    {
        // ARRANGE
        using Context context = await DatabaseUtils.OpenDatabaseAsync(
            DatabaseTypes.Sqlite,
            customConfigurationActions: optionsBuilder => optionsBuilder.UseSqliteNoCaseReplacement())
            .ConfigureAwait(false);

        await DatabaseUtils.CreateTestTableEntriesAsync(context, originalValue, 1).ConfigureAwait(false);

        // ACT
        List<TestTable1> results = await context.TestTable1
            .Where(e => EF.Functions.Collate(e.TestField, "NOCASE") == searchValue)
            .ToListAsync(this._testContext.CancellationToken)
            .ConfigureAwait(false);

        // ASSERT
        Assert.HasCount(expectedCount, results, "Invalid record count");
    }

    /// <summary>
    /// Test UseSqliteNoCaseReplacement when the connection is opened synchronously.
    /// </summary>
    /// <remarks>
    /// The collation is replaced when the connection is opened, and the synchronous and asynchronous opens are intercepted separately.
    /// The data is created with a different context, so the context being tested has never opened its connection asynchronously.
    /// </remarks>
    [TestMethod(DisplayName = "UseSqliteNoCaseReplacement when the connection is opened synchronously")]
    public void CaseInsensitivityTests_004()
    {
        // ARRANGE
        string originalValue = "ááá";
        string searchValue = "ÁÁÁ";

        using (Context createContext = DatabaseUtils.OpenDatabaseAsync(DatabaseTypes.Sqlite).Result)
        {
            createContext.Add(new TestTable1 { TestField = originalValue });
            createContext.SaveChanges();
        }

        using Context context = DatabaseUtils.OpenDatabaseAsync(
            DatabaseTypes.Sqlite,
            createNew: false,
            customConfigurationActions: optionsBuilder => optionsBuilder.UseSqliteNoCaseReplacement())
            .Result;

        // ACT
        List<TestTable1> results = context.TestTable1
            .Where(e => EF.Functions.Collate(e.TestField, "NOCASE") == searchValue)
            .ToList();

        // ASSERT
        Assert.HasCount(1, results, "Invalid record count");
    }

    /// <summary>
    /// Test UseSqliteNoCaseReplacement sorts case insensitively, including non-ASCII characters.
    /// </summary>
    /// <returns>A <see cref="Task"/> that represents the asynchronous operation.</returns>
    /// <remarks>The built in SQLite NOCASE collation only handles ASCII characters, so it would sort "Éclair" after "Zebra".</remarks>
    [TestMethod(DisplayName = "UseSqliteNoCaseReplacement sort order")]
    public async Task CaseInsensitivityTests_005_Async()
    {
        // ARRANGE
        using Context context = await DatabaseUtils.OpenDatabaseAsync(
            DatabaseTypes.Sqlite,
            customConfigurationActions: optionsBuilder => optionsBuilder.UseSqliteNoCaseReplacement())
            .ConfigureAwait(false);

        await DatabaseUtils.CreateTestTableEntriesAsync(context, "Zebra", 1).ConfigureAwait(false);
        await DatabaseUtils.CreateTestTableEntriesAsync(context, "Éclair", 1).ConfigureAwait(false);
        await DatabaseUtils.CreateTestTableEntriesAsync(context, "apple", 1).ConfigureAwait(false);

        string expectedOrder = "apple,Éclair,Zebra";

        // ACT
        List<string> results = await context.TestTable1
            .OrderBy(e => EF.Functions.Collate(e.TestField, "NOCASE"))
            .Select(e => e.TestField)
            .ToListAsync(this._testContext.CancellationToken)
            .ConfigureAwait(false);

        // ASSERT
        Assert.AreEqual(expectedOrder, string.Join(",", results), "Invalid sort order");
    }

    #endregion public methods
}