// Copyright ©2021-2026 Mike King.
// This file is licensed to you under the MIT license.
// See the License.txt file in the solution root for more information.

using DotDoc.EntityFrameworkCore.Extensions.CaseInsensitivity;
using DotDoc.EntityFrameworkCore.Extensions.DatabaseType;
using DotDoc.EntityFrameworkCore.Extensions.Tests.Data;
using DotDoc.EntityFrameworkCore.Extensions.Utilities;
using Microsoft.EntityFrameworkCore;

namespace DotDoc.EntityFrameworkCore.Extensions.Tests.Utilities;

/// <summary>
/// Tests for the UseCaseInsensitiveCollation extension.
/// </summary>
[TestClass]
public class UseCaseInsensitiveCollationTests
{
    #region private fields

    /// <summary>
    /// The MSTest <see cref="TestContext"/> for the current test.
    /// </summary>
    private readonly TestContext _testContext;

    #endregion private fields

    #region public constructors

    /// <summary>
    /// Initializes a new instance of the <see cref="UseCaseInsensitiveCollationTests"/> class.
    /// </summary>
    /// <param name="testContext">The MSTest test context.</param>
    public UseCaseInsensitiveCollationTests(TestContext testContext)
    {
        this._testContext = testContext;
    }

    #endregion public constructors

    #region UseCaseInsensitiveCollation tests

    /// <summary>
    /// Test UseCaseInsensitiveCollation with a Null ModelBuilder parameter.
    /// </summary>
    [TestMethod(DisplayName = "UseCaseInsensitiveCollation with a Null ModelBuilder parameter")]
    public void UseCaseInsensitiveCollationTests_001()
    {
        // ARRANGE
        ModelBuilder modelBuilder = null!;
        string databaseType = DatabaseTypes.Sqlite;

        // ACT / ASSERT
        Assert.ThrowsExactly<ArgumentNullException>(() => modelBuilder.UseCaseInsensitiveCollation(databaseType), "Unexpected exception");
    }

    /// <summary>
    /// Test UseCaseInsensitiveCollation with an unsupported database type.
    /// </summary>
    /// <param name="databaseType">The database type.</param>
    /// <returns>A <see cref="Task"/> that represents the asynchronous operation.</returns>
    [TestMethod(DisplayName = "UseCaseInsensitiveCollation with an unsupported database type")]
    [DataRow(null, DisplayName = "Null")]
    [DataRow("", DisplayName = "Empty")]
    [DataRow("Invalid", DisplayName = "Invalid")]
    public async Task UseCaseInsensitiveCollationTests_002_Async(string databaseType)
    {
        // ARRANGE

        // ACT / ASSERT
        await Assert.ThrowsExactlyAsync<UnsupportedDatabaseTypeException>(
            async () =>
            {
                using Context context = await DatabaseUtils.OpenDatabaseAsync(
                    DatabaseTypes.Sqlite,
                    customModelCreationActions: modelBuilder => modelBuilder.UseCaseInsensitiveCollation(databaseType))
                    .ConfigureAwait(false);
            },
            "Unexpected exception")
            .ConfigureAwait(false);
    }

    /// <summary>
    /// Test UseCaseInsensitiveCollation.
    /// </summary>
    /// <param name="databaseType">The database type.</param>
    /// <returns>A <see cref="Task"/> that represents the asynchronous operation.</returns>
    [TestMethod(DisplayName = "UseCaseInsensitiveCollation")]
    [DataRow(DatabaseTypes.Sqlite, DisplayName = DatabaseTypes.Sqlite)]
    [DataRow(DatabaseTypes.SqlServer, DisplayName = DatabaseTypes.SqlServer)]
    public async Task UseCaseInsensitiveCollationTests_003_Async(string databaseType)
    {
        // ARRANGE / ACT
        using Context context = await DatabaseUtils.OpenDatabaseAsync(
            databaseType,
            customModelCreationActions: modelBuilder => modelBuilder.UseCaseInsensitiveCollation(databaseType))
            .ConfigureAwait(false);

        string originalValue = new('a', 10);
        string searchValue = originalValue.ToUpperInvariant();

        await DatabaseUtils.CreateTestTableEntriesAsync(context, originalValue, 1).ConfigureAwait(false);

        // ASSERT
        List<TestTable1> results = await context.TestTable1
            .Where(e => e.TestField == searchValue)
            .ToListAsync(this._testContext.CancellationToken)
            .ConfigureAwait(false);

        Assert.HasCount(1, results, "Invalid count");
    }

    /// <summary>
    /// Test UseCaseInsensitiveCollation is case insensitive but accent sensitive on both databases.
    /// </summary>
    /// <param name="databaseType">The database type.</param>
    /// <param name="originalValue">The value stored in the database.</param>
    /// <param name="searchValue">The value to search for.</param>
    /// <param name="expectedCount">The expected number of matching records.</param>
    /// <returns>A <see cref="Task"/> that represents the asynchronous operation.</returns>
    /// <remarks>
    /// SQLite uses the NOCASE collation, replaced by UseSqliteNoCaseReplacement so that it handles non-ASCII characters.
    /// SQL Server uses SQL_Latin1_General_CP1_CI_AS. The same values must give the same results on both.
    /// </remarks>
    [TestMethod(DisplayName = "UseCaseInsensitiveCollation is case insensitive and accent sensitive")]
    [DataRow(DatabaseTypes.Sqlite, "ááá", "ÁÁÁ", 1, DisplayName = $"{DatabaseTypes.Sqlite} Accented lower case matches accented upper case")]
    [DataRow(DatabaseTypes.Sqlite, "ááá", "AAA", 0, DisplayName = $"{DatabaseTypes.Sqlite} Accented does not match unaccented")]
    [DataRow(DatabaseTypes.Sqlite, "aaa", "ÁÁÁ", 0, DisplayName = $"{DatabaseTypes.Sqlite} Unaccented does not match accented")]
    [DataRow(DatabaseTypes.Sqlite, "abc", "ABC", 1, DisplayName = $"{DatabaseTypes.Sqlite} ASCII lower case matches upper case")]
    [DataRow(DatabaseTypes.Sqlite, "æøå", "ÆØÅ", 1, DisplayName = $"{DatabaseTypes.Sqlite} Non-ASCII lower case matches upper case")]
    [DataRow(DatabaseTypes.Sqlite, "ＡＢＣ", "abc", 1, DisplayName = $"{DatabaseTypes.Sqlite} Full width matches half width")]
    [DataRow(DatabaseTypes.SqlServer, "ááá", "ÁÁÁ", 1, DisplayName = $"{DatabaseTypes.SqlServer} Accented lower case matches accented upper case")]
    [DataRow(DatabaseTypes.SqlServer, "ááá", "AAA", 0, DisplayName = $"{DatabaseTypes.SqlServer} Accented does not match unaccented")]
    [DataRow(DatabaseTypes.SqlServer, "aaa", "ÁÁÁ", 0, DisplayName = $"{DatabaseTypes.SqlServer} Unaccented does not match accented")]
    [DataRow(DatabaseTypes.SqlServer, "abc", "ABC", 1, DisplayName = $"{DatabaseTypes.SqlServer} ASCII lower case matches upper case")]
    [DataRow(DatabaseTypes.SqlServer, "æøå", "ÆØÅ", 1, DisplayName = $"{DatabaseTypes.SqlServer} Non-ASCII lower case matches upper case")]
    [DataRow(DatabaseTypes.SqlServer, "ＡＢＣ", "abc", 1, DisplayName = $"{DatabaseTypes.SqlServer} Full width matches half width")]
    public async Task UseCaseInsensitiveCollationTests_004_Async(string databaseType, string originalValue, string searchValue, int expectedCount)
    {
        // ARRANGE
        using Context context = await DatabaseUtils.OpenDatabaseAsync(
            databaseType,
            customConfigurationActions: optionsBuilder =>
            {
                if (databaseType == DatabaseTypes.Sqlite)
                {
                    optionsBuilder.UseSqliteNoCaseReplacement();
                }
            },
            customModelCreationActions: modelBuilder => modelBuilder.UseCaseInsensitiveCollation(databaseType))
            .ConfigureAwait(false);

        await DatabaseUtils.CreateTestTableEntriesAsync(context, originalValue, 1).ConfigureAwait(false);

        // ACT
        List<TestTable1> results = await context.TestTable1
            .Where(e => e.TestField == searchValue)
            .ToListAsync(this._testContext.CancellationToken)
            .ConfigureAwait(false);

        // ASSERT
        Assert.HasCount(expectedCount, results, "Invalid record count");
    }

    #endregion UseCaseInsensitiveCollation tests
}