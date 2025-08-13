// Copyright ©2021-2025 Mike King.
// This file is licensed to you under the MIT license.
// See the License.txt file in the solution root for more information.

using DotDoc.EntityFrameworkCore.Extensions.CaseInsensitivity;
using DotDoc.EntityFrameworkCore.Extensions.DatabaseType;
using DotDoc.EntityFrameworkCore.Extensions.Tests.Data;
using DotDoc.EntityFrameworkCore.Extensions.Tests.Utilities;
using Microsoft.EntityFrameworkCore;

namespace DotDoc.EntityFrameworkCore.Extensions.Tests.CaseInsensitivity;

/// <summary>
/// Tests for the GetCaseInsensitivity extension.
/// </summary>
[TestClass]
public class CaseInsensitivityTests
{
    #region private fields

    /// <summary>
    /// Test Context <see cref="TestContext"/>.
    /// </summary>
    private readonly TestContext _testContext;

    #endregion private fields

    #region public constructors

    /// <summary>
    /// Initializes a new instance of the <see cref="CaseInsensitivityTests"/> class.
    /// </summary>
    /// <param name="testContext">Test context.</param>
    public CaseInsensitivityTests(TestContext testContext)
    {
        ArgumentNullException.ThrowIfNull(testContext);

        this._testContext = testContext;
    }

    #endregion public constructors

    #region public methods

    /// <summary>
    /// Test UseSqliteUnicodeNoCase Guard Clause.
    /// </summary>
    [TestMethod("UseSqliteUnicodeNoCase Guard Clause")]
    public void Test_UseSqliteUnicodeNoCase_GuardClause()
    {
        // ARRANGE
        DbContextOptionsBuilder? optionsBuilder = null;

        // ACT / ASSERT
        ArgumentNullException e = Assert.ThrowsExactly<ArgumentNullException>(() => _ = optionsBuilder!.UseSqliteUnicodeNoCase(), "Missing exception");
        Assert.AreEqual(nameof(optionsBuilder), e.ParamName, "Invalid parameter name");
    }

    /// <summary>
    /// Test UseSqliteCaseInsensitiveCollation Guard Clause.
    /// </summary>
    [TestMethod("UseSqliteCaseInsensitiveCollation Guard Clause")]
    public void Test_UseSqliteCaseInsensitiveCollation_GuardClause()
    {
        // ARRANGE
        ModelBuilder? modelBuilder = null;

        // ACT / ASSERT
        ArgumentNullException e = Assert.ThrowsExactly<ArgumentNullException>(() => _ = modelBuilder!.UseSqliteCaseInsensitiveCollation(), "Missing exception");
        Assert.AreEqual(nameof(modelBuilder), e.ParamName, "Invalid parameter name");
    }

    /// <summary>
    /// Test UseSqlServerCaseInsensitiveCollation Guard Clause.
    /// </summary>
    [TestMethod("UseSqlServerCaseInsensitiveCollation Guard Clause")]
    public void Test_UseSqlServerCaseInsensitiveCollation_GuardClause()
    {
        // ARRANGE
        ModelBuilder? modelBuilder = null;

        // ACT / ASSERT
        ArgumentNullException e = Assert.ThrowsExactly<ArgumentNullException>(() => _ = modelBuilder!.UseSqlServerCaseInsensitiveCollation(), "Missing exception");
        Assert.AreEqual(nameof(modelBuilder), e.ParamName, "Invalid parameter name");
    }

    /// <summary>
    /// Test UseSqliteUnicodeNoCase.
    /// </summary>
    /// <returns>A <see cref="Task"/> that represents the asynchronous operation.</returns>
    [TestMethod("UseSqliteUnicodeNoCase")]
    public async Task Test_UseSqliteUnicodeNoCase_Async()
    {
        // ARRANGE / ACT
        using Context context = await DatabaseUtils.CreateDatabaseAsync(
            DatabaseTypes.Sqlite,
            customConfigurationActions: (optionsBuilder) => optionsBuilder.UseSqliteUnicodeNoCase())
            .ConfigureAwait(false);

        string originalValue = new('\u00E1', 10);       // \u00E1 = Latin Small Letter A with Acute.
        string searchValue = new('A', 10);

        await DatabaseUtils.CreateTestTableEntriesAsync(context, originalValue, 1).ConfigureAwait(false);

        // ASSERT
        List<TestTable1> results = await context.TestTable1
            .Where(e => EF.Functions.Collate(e.TestField, "NOCASE") == searchValue)
            .ToListAsync(this._testContext.CancellationTokenSource.Token)
            .ConfigureAwait(false);

        Assert.IsGreaterThan(0, results.Count, "Invalid record count");
    }

    /// <summary>
    /// Test UseSqliteUnicodeNoCase with an unsupported database type.
    /// </summary>
    /// <returns>A <see cref="Task"/> that represents the asynchronous operation.</returns>
    [TestMethod("UseSqliteUnicodeNoCase with an unsupported database type")]
    public async Task Test_UseSqliteUnicodeNoCase_NonSqlite_Async()
    {
        // ARRANGE
        string message = "Unsupported database type";

        // ACT / ASSERT
        InvalidOperationException e = await Assert.ThrowsExactlyAsync<InvalidOperationException>(
            async () =>
            {
                using Context context = await DatabaseUtils.CreateDatabaseAsync(
                    DatabaseTypes.SqlServer,
                    customConfigurationActions: (optionsBuilder) => optionsBuilder.UseSqliteUnicodeNoCase())
                    .ConfigureAwait(false);
            },
            "Unexpected exception")
            .ConfigureAwait(false);

        Assert.AreEqual(message, e.Message, "Invalid exception message");
    }

    /// <summary>
    /// Test UseSqliteCaseInsensitiveCollation.
    /// </summary>
    /// <returns>A <see cref="Task"/> that represents the asynchronous operation.</returns>
    [TestMethod("UseSqliteCaseInsensitiveCollation")]
    public async Task Test_UseSqliteCaseInsensitiveCollation()
    {
        // ARRANGE / ACT
        using Context context = await DatabaseUtils.CreateDatabaseAsync(
            DatabaseTypes.Sqlite,
            customModelCreationActions: (modelBuilder) => modelBuilder.UseSqliteCaseInsensitiveCollation())
            .ConfigureAwait(false);

        string originalValue = new('a', 10);
        string searchValue = originalValue.ToUpperInvariant();

        await DatabaseUtils.CreateTestTableEntriesAsync(context, originalValue, 1).ConfigureAwait(false);

        // ASSERT
        List<TestTable1> results = await context.TestTable1
            .Where(e => e.TestField == searchValue)
            .ToListAsync(this._testContext.CancellationTokenSource.Token)
            .ConfigureAwait(false);

        Assert.IsGreaterThan(0, results.Count, "Invalid record count");
    }

    /// <summary>
    /// Test UseSqlServerCaseInsensitiveCollation.
    /// </summary>
    /// <returns>A <see cref="Task"/> that represents the asynchronous operation.</returns>
    [TestMethod("UseSqlServerCaseInsensitiveCollation")]
    public async Task Test_UseSqlServerCaseInsensitiveCollation()
    {
        // ARRANGE / ACT
        using Context context = await DatabaseUtils.CreateDatabaseAsync(
            DatabaseTypes.SqlServer,
            customModelCreationActions: (modelBuilder) => modelBuilder.UseSqlServerCaseInsensitiveCollation())
            .ConfigureAwait(false);

        string originalValue = new('a', 10);
        string searchValue = originalValue.ToUpperInvariant();

        await DatabaseUtils.CreateTestTableEntriesAsync(context, originalValue, 1).ConfigureAwait(false);

        // ASSERT
        List<TestTable1> results = await context.TestTable1
            .Where(e => e.TestField == searchValue)
            .ToListAsync(this._testContext.CancellationTokenSource.Token)
            .ConfigureAwait(false);

        Assert.IsGreaterThan(0, results.Count, "Invalid record count");
    }

    #endregion public methods
}