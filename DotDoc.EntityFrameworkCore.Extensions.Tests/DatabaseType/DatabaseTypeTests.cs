// Copyright ©2021-2026 Mike King.
// This file is licensed to you under the MIT license.
// See the License.txt file in the solution root for more information.

using DotDoc.EntityFrameworkCore.Extensions.CaseInsensitivity;
using DotDoc.EntityFrameworkCore.Extensions.DatabaseType;
using DotDoc.EntityFrameworkCore.Extensions.Tests.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

namespace DotDoc.EntityFrameworkCore.Extensions.Tests.DatabaseType;

/// <summary>
/// Tests for the GetDatabaseType extension.
/// </summary>
[TestClass]
public class DatabaseTypeTests
{
    #region private fields

    /// <summary>
    /// The MSTest <see cref="TestContext"/> for the current test.
    /// </summary>
    private readonly TestContext _testContext;

    #endregion private fields

    #region public constructors

    /// <summary>
    /// Initializes a new instance of the <see cref="DatabaseTypeTests"/> class.
    /// </summary>
    /// <param name="testContext">The MSTest test context.</param>
    public DatabaseTypeTests(TestContext testContext)
    {
        this._testContext = testContext;
    }

    #endregion public constructors

    #region GetDatabaseType tests

    /// <summary>
    /// Test GetDatabaseType with a Null DatabaseFacade Database parameter.
    /// </summary>
    [TestMethod]
    public void GetDatabaseType_NullDatabase_ThrowsArgumentNullException()
    {
        // ARRANGE
        DatabaseFacade database = null!;

        // ACT / ASSERT
        Assert.ThrowsExactly<ArgumentNullException>(() => database.GetDatabaseType(), "Unexpected exception");
    }

    /// <summary>
    /// Test GetDatabaseType with a Null DbContextOptions parameter.
    /// </summary>
    [TestMethod]
    public void GetDatabaseType_NullOptions_ThrowsArgumentNullException()
    {
        // ARRANGE
        DbContextOptions options = null!;

        // ACT / ASSERT
        Assert.ThrowsExactly<ArgumentNullException>(() => options.GetDatabaseType(), "Unexpected exception");
    }

    /// <summary>
    /// Test GetDatabaseType with a Null MigrationBuilder parameter.
    /// </summary>
    [TestMethod]
    public void GetDatabaseType_NullMigrationBuilder_ThrowsArgumentNullException()
    {
        // ARRANGE
        MigrationBuilder database = null!;

        // ACT / ASSERT
        Assert.ThrowsExactly<ArgumentNullException>(() => database.GetDatabaseType(), "Unexpected exception");
    }

    /// <summary>
    /// Test GetDatabaseType extension with a DatabaseFacade parameter.
    /// </summary>
    /// <param name="databaseType">Database type.</param>
    [TestMethod]
    [DataRow(DatabaseTypes.Sqlite, DisplayName = $"DatabaseFacade {DatabaseTypes.Sqlite}")]
    [DataRow(DatabaseTypes.SqlServer, DisplayName = $"DatabaseFacade {DatabaseTypes.SqlServer}")]
    public void GetDatabaseType_DatabaseFacade_ReturnsDatabaseType(string databaseType)
    {
        // ARRANGE
        using Context context = new(databaseType, "Dummy");

        // ACT
        string? actualDatabaseType = context.Database.GetDatabaseType();

        // ASSERT
        Assert.AreEqual(databaseType, actualDatabaseType, "Unexpected database type");
    }

    /// <summary>
    /// Test GetDatabaseType extension with a <see cref="DbContextOptions"/> parameter.
    /// </summary>
    /// <param name="databaseType">Database type.</param>
    /// <returns>A <see cref="Task"/> that represents the asynchronous operation.</returns>
    [TestMethod]
    [DataRow(DatabaseTypes.Sqlite, DisplayName = $"DbContextOptions {DatabaseTypes.Sqlite}")]
    [DataRow(DatabaseTypes.SqlServer, DisplayName = $"DbContextOptions {DatabaseTypes.SqlServer}")]
    public async Task GetDatabaseType_DbContextOptions_ReturnsDatabaseType(string databaseType)
    {
        // ARRANGE / ACT
        string? actualDatabaseType = null;

        using Context context = await DatabaseUtils.OpenDatabaseAsync(
            this._testContext,
            databaseType,
            customConfigurationActions: optionsBuilder => actualDatabaseType = optionsBuilder.Options.GetDatabaseType(),
            customModelCreationActions: modelBuilder => modelBuilder.UseCaseInsensitiveCollation(databaseType))
            .ConfigureAwait(false);

        // ASSERT
        Assert.AreEqual(databaseType, actualDatabaseType, "Unexpected database type");
    }

    /// <summary>
    /// Test GetDatabaseType extension with a MigrationBuilder parameter.
    /// </summary>
    /// <param name="databaseType">Database type.</param>
    [TestMethod]
    [DataRow(DatabaseTypes.Sqlite, DisplayName = $"MigrationBuilder {DatabaseTypes.Sqlite}")]
    [DataRow(DatabaseTypes.SqlServer, DisplayName = $"MigrationBuilder {DatabaseTypes.SqlServer}")]
    public void GetDatabaseType_MigrationBuilder_ReturnsDatabaseType(string databaseType)
    {
        // ARRANGE
        using Context context = new(databaseType, "Dummy");
        MigrationBuilder migrationBuilder = new(context.Database.ProviderName);

        // ACT
        string? actualDatabaseType = migrationBuilder.GetDatabaseType();

        // ASSERT
        Assert.AreEqual(databaseType, actualDatabaseType, "Unexpected database type");
    }

    /// <summary>
    /// Test GetDatabaseType extension with a MigrationBuilder parameter for a provider that is not supported.
    /// </summary>
    /// <param name="activeProvider">The migration builder active provider name.</param>
    [TestMethod]
    [DataRow(null, DisplayName = "MigrationBuilderUnsupportedProvider Null")]
    [DataRow("Microsoft.EntityFrameworkCore.InMemory", DisplayName = "MigrationBuilderUnsupportedProvider Microsoft.EntityFrameworkCore.InMemory")]
    public void GetDatabaseType_MigrationBuilderUnsupportedProvider_ReturnsNull(string? activeProvider)
    {
        // ARRANGE
        MigrationBuilder migrationBuilder = new(activeProvider);

        // ACT
        string? actualDatabaseType = migrationBuilder.GetDatabaseType();

        // ASSERT
        Assert.IsNull(actualDatabaseType, "Unexpected database type");
    }

    /// <summary>
    /// Test GetDatabaseType extension with a DbContextOptions parameter that has no database provider.
    /// </summary>
    [TestMethod]
    public void GetDatabaseType_DbContextOptionsWithoutProvider_ReturnsNull()
    {
        // ARRANGE
        DbContextOptions options = new DbContextOptionsBuilder().Options;

        // ACT
        string? actualDatabaseType = options.GetDatabaseType();

        // ASSERT
        Assert.IsNull(actualDatabaseType, "Unexpected database type");
    }

    #endregion GetDatabaseType tests
}
