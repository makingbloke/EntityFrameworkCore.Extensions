// Copyright ©2021-2026 Mike King.
// This file is licensed to you under the MIT license.
// See the License.txt file in the solution root for more information.

using DotDoc.EntityFrameworkCore.Extensions.CaseInsensitivity;
using DotDoc.EntityFrameworkCore.Extensions.DatabaseType;
using DotDoc.EntityFrameworkCore.Extensions.Execute;
using DotDoc.EntityFrameworkCore.Extensions.Tests.Data;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;

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

    #region UseSqliteNoCaseReplacement tests

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
                    this._testContext,
                    DatabaseTypes.SqlServer,
                    customConfigurationActions: optionsBuilder => optionsBuilder.UseSqliteNoCaseReplacement())
                    .ConfigureAwait(false);
            },
            "Unexpected exception")
            .ConfigureAwait(false);
    }

    /// <summary>
    /// Test UseSqliteNoCaseReplacement is case insensitive but accent sensitive (matching SQL Server Latin1_General_100_CI_AS_SC_UTF8).
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
            this._testContext,
            DatabaseTypes.Sqlite,
            customConfigurationActions: optionsBuilder => optionsBuilder.UseSqliteNoCaseReplacement())
            .ConfigureAwait(false);

        await DatabaseUtils.CreateTestTableEntriesAsync(context, originalValue, 1, this._testContext.CancellationToken).ConfigureAwait(false);

        // ACT
        List<TestTable1> results = await context.TestTable1
            .Where(e => EF.Functions.Collate(e.TestField, SqliteUnicodeFunctionNames.NoCaseCollation) == searchValue)
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

        using (Context createContext = DatabaseUtils.OpenDatabaseAsync(this._testContext, DatabaseTypes.Sqlite).Result)
        {
            createContext.Add(new TestTable1 { TestField = originalValue });
            createContext.SaveChanges();
        }

        using Context context = DatabaseUtils.OpenDatabaseAsync(
            this._testContext,
            DatabaseTypes.Sqlite,
            createNew: false,
            customConfigurationActions: optionsBuilder => optionsBuilder.UseSqliteNoCaseReplacement())
            .Result;

        // ACT
        List<TestTable1> results = context.TestTable1
            .Where(e => EF.Functions.Collate(e.TestField, SqliteUnicodeFunctionNames.NoCaseCollation) == searchValue)
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
            this._testContext,
            DatabaseTypes.Sqlite,
            customConfigurationActions: optionsBuilder => optionsBuilder.UseSqliteNoCaseReplacement())
            .ConfigureAwait(false);

        await DatabaseUtils.CreateTestTableEntriesAsync(context, "Zebra", 1, this._testContext.CancellationToken).ConfigureAwait(false);
        await DatabaseUtils.CreateTestTableEntriesAsync(context, "Éclair", 1, this._testContext.CancellationToken).ConfigureAwait(false);
        await DatabaseUtils.CreateTestTableEntriesAsync(context, "apple", 1, this._testContext.CancellationToken).ConfigureAwait(false);

        string expectedOrder = "apple,Éclair,Zebra";

        // ACT
        List<string> results = await context.TestTable1
            .OrderBy(e => EF.Functions.Collate(e.TestField, SqliteUnicodeFunctionNames.NoCaseCollation))
            .Select(e => e.TestField)
            .ToListAsync(this._testContext.CancellationToken)
            .ConfigureAwait(false);

        // ASSERT
        Assert.AreEqual(expectedOrder, string.Join(",", results), "Invalid sort order");
    }

    /// <summary>
    /// Test UseSqliteNoCaseReplacement with a connection that was opened before it was given to EF Core.
    /// </summary>
    /// <param name="inMemory">Whether the database is in memory (the usual reason for opening the connection first) or a file.</param>
    /// <returns>A <see cref="Task"/> that represents the asynchronous operation.</returns>
    /// <remarks>EF Core doesn't raise ConnectionOpened for a connection it didn't open itself.</remarks>
    [TestMethod(DisplayName = "UseSqliteNoCaseReplacement with a connection opened before it was given to EF Core")]
    [DataRow(true, DisplayName = "In memory")]
    [DataRow(false, DisplayName = "File")]
    public async Task CaseInsensitivityTests_006_Async(bool inMemory)
    {
        // ARRANGE
        string dataSource = inMemory ? ":memory:" : Path.Combine(this._testContext.TestTempDirectory!, "OpenConnection.db");

        using SqliteConnection connection = new($"Data Source={dataSource}");
        await connection.OpenAsync(this._testContext.CancellationToken).ConfigureAwait(false);

        DbContextOptionsBuilder optionsBuilder = new DbContextOptionsBuilder().UseSqlite(connection);
        optionsBuilder.UseSqliteNoCaseReplacement();

        using DbContext context = new(optionsBuilder.Options);

        // ACT / ASSERT
        await this.AssertNoCaseReplacementsRegisteredAsync(context.Database).ConfigureAwait(false);
    }

    /// <summary>
    /// Test UseSqliteNoCaseReplacement with an open connection set with SetDbConnection, through a query EF Core translates to a nocase_* function.
    /// </summary>
    /// <returns>A <see cref="Task"/> that represents the asynchronous operation.</returns>
    [TestMethod(DisplayName = "UseSqliteNoCaseReplacement with an open connection set with SetDbConnection")]
    public async Task CaseInsensitivityTests_007_Async()
    {
        // ARRANGE
        using SqliteConnection connection = new("Data Source=:memory:");
        await connection.OpenAsync(this._testContext.CancellationToken).ConfigureAwait(false);

        using Context context = new(
            DatabaseTypes.Sqlite,
            DatabaseUtils.GetConnectionString(this._testContext, DatabaseTypes.Sqlite),
            customConfigurationActions: optionsBuilder => optionsBuilder.UseSqliteNoCaseReplacement());

        context.Database.SetDbConnection(connection);
        await context.Database.EnsureCreatedAsync(this._testContext.CancellationToken).ConfigureAwait(false);
        await DatabaseUtils.CreateTestTableEntriesAsync(context, "École", 1, this._testContext.CancellationToken).ConfigureAwait(false);

        // ACT
        int count = await context.TestTable1
            .CountAsync(e => EF.Functions.Collate(e.TestField, SqliteUnicodeFunctionNames.NoCaseCollation).Contains("éc"), this._testContext.CancellationToken)
            .ConfigureAwait(false);

        // ASSERT
        Assert.AreEqual(1, count, "Invalid count");
    }

    /// <summary>
    /// Test UseSqliteNoCaseReplacement with context pooling, on the first and second lease of the same context.
    /// </summary>
    /// <returns>A <see cref="Task"/> that represents the asynchronous operation.</returns>
    [TestMethod(DisplayName = "UseSqliteNoCaseReplacement with context pooling")]
    public async Task CaseInsensitivityTests_008_Async()
    {
        // ARRANGE
        DbContextOptionsBuilder<PooledContext> optionsBuilder = new();
        optionsBuilder.UseSqlite(DatabaseUtils.GetConnectionString(this._testContext, DatabaseTypes.Sqlite));
        optionsBuilder.UseSqliteNoCaseReplacement();

        PooledDbContextFactory<PooledContext> factory = new(optionsBuilder.Options, poolSize: 1);

        PooledContext firstLease;

        await using (PooledContext context = await factory.CreateDbContextAsync(this._testContext.CancellationToken).ConfigureAwait(false))
        {
            firstLease = context;
            await this.AssertNoCaseReplacementsRegisteredAsync(context.Database).ConfigureAwait(false);
        }

        // ACT
        await using PooledContext secondLease = await factory.CreateDbContextAsync(this._testContext.CancellationToken).ConfigureAwait(false);

        // ASSERT
        Assert.AreSame(firstLease, secondLease, "The context has not been reused from the pool");
        await this.AssertNoCaseReplacementsRegisteredAsync(secondLease.Database).ConfigureAwait(false);
    }

    /// <summary>
    /// Test UseSqliteNoCaseReplacement with a connection that wraps a SqliteConnection but isn't one (as a profiling or tracing wrapper would).
    /// </summary>
    /// <returns>A <see cref="Task"/> that represents the asynchronous operation.</returns>
    /// <remarks>The collation and functions can only be registered on a SqliteConnection, so the first command fails rather than NOCASE silently staying ASCII only.</remarks>
    [TestMethod(DisplayName = "UseSqliteNoCaseReplacement with a connection that isn't a SqliteConnection")]
    public async Task CaseInsensitivityTests_009_Async()
    {
        // ARRANGE
        using WrappedSqliteConnection connection = new("Data Source=:memory:");

        DbContextOptionsBuilder optionsBuilder = new DbContextOptionsBuilder().UseSqlite(connection);
        optionsBuilder.UseSqliteNoCaseReplacement();

        using DbContext context = new(optionsBuilder.Options);

        // ACT / ASSERT
        InvalidOperationException ex = await Assert.ThrowsExactlyAsync<InvalidOperationException>(
            () => context.Database.ExecuteScalarAsync<int>("SELECT 1", this._testContext.CancellationToken),
            "Unexpected exception")
            .ConfigureAwait(false);

        Assert.AreEqual(
            $"UseSqliteNoCaseReplacement needs a SqliteConnection, but the command's connection is a {nameof(WrappedSqliteConnection)}.",
            ex.Message,
            "Invalid exception message");
    }

    #endregion UseSqliteNoCaseReplacement tests

    #region private methods

    /// <summary>
    /// Asserts that the NOCASE collation, the upper function and the nocase_* functions have been replaced or registered on a context's connection.
    /// </summary>
    /// <param name="database">The context's database.</param>
    /// <returns>A <see cref="Task"/> that represents the asynchronous operation.</returns>
    private async Task AssertNoCaseReplacementsRegisteredAsync(DatabaseFacade database)
    {
        CancellationToken cancellationToken = this._testContext.CancellationToken;

        bool noCaseEquals = await database.ExecuteScalarAsync<bool>("SELECT 'é' = 'É' COLLATE NOCASE", cancellationToken).ConfigureAwait(false);
        string? upper = await database.ExecuteScalarAsync<string>("SELECT upper('é')", cancellationToken).ConfigureAwait(false);
        int position = await database.ExecuteScalarAsync<int>("SELECT nocase_instr('ÉCOLE', 'é')", cancellationToken).ConfigureAwait(false);

        Assert.IsTrue(noCaseEquals, "The NOCASE collation has not been replaced");
        Assert.AreEqual("É", upper, "The upper function has not been replaced");
        Assert.AreEqual(1, position, "The nocase_* functions have not been registered");
    }

    #endregion private methods
}