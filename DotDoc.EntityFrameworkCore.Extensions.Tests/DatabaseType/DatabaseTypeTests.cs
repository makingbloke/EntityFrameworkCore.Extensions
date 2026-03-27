// Copyright ©2021-2026 Mike King.
// This file is licensed to you under the MIT license.
// See the License.txt file in the solution root for more information.

using DotDoc.EntityFrameworkCore.Extensions.DatabaseType;
using DotDoc.EntityFrameworkCore.Extensions.Tests.Data;
using DotDoc.EntityFrameworkCore.Extensions.Utilities;
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
    #region public methods

    /// <summary>
    /// Test GetDatabaseType with a Null DatabaseFacade Database parameter.
    /// </summary>
    [TestMethod(DisplayName = "GetDatabaseType with a Null DatabaseFacade Database parameter")]
    public void DatabaseTypeTests_001()
    {
        // ARRANGE
        DatabaseFacade database = null!;

        // ACT / ASSERT
        Assert.ThrowsExactly<ArgumentNullException>(() => database.GetDatabaseType(), "Unexpected exception");
    }

    /// <summary>
    /// Test GetDatabaseType with a Null DbContextOptions parameter.
    /// </summary>
    [TestMethod(DisplayName = "GetDatabaseType with a Null DbContextOptions parameter")]
    public void DatabaseTypeTests_002()
    {
        // ARRANGE
        DbContextOptions options = null!;

        // ACT / ASSERT
        Assert.ThrowsExactly<ArgumentNullException>(() => options.GetDatabaseType(), "Unexpected exception");
    }

    /// <summary>
    /// Test GetDatabaseType with a Null MigrationBuilder parameter.
    /// </summary>
    [TestMethod(DisplayName = "GetDatabaseType with a Null MigrationBuilder parameter")]
    public void DatabaseTypeTests_003()
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
    [TestMethod(DisplayName = "GetDatabaseType extension with a DatabaseFacade parameter")]
    [DataRow(DatabaseTypes.Sqlite, DisplayName = DatabaseTypes.Sqlite)]
    [DataRow(DatabaseTypes.SqlServer, DisplayName = DatabaseTypes.SqlServer)]
    public void DatabaseTypeTests_004(string databaseType)
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
    [TestMethod(DisplayName = "GetDatabaseType extension with a DbContextOptions parameter")]
    [DataRow(DatabaseTypes.Sqlite, DisplayName = DatabaseTypes.Sqlite)]
    [DataRow(DatabaseTypes.SqlServer, DisplayName = DatabaseTypes.SqlServer)]
    public async Task DatabaseTypeTests_005_Async(string databaseType)
    {
        // ARRANGE / ACT
        string? actualDatabaseType = null;

        using Context context = await DatabaseUtils.OpenDatabaseAsync(
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
    [TestMethod(DisplayName = "GetDatabaseType extension with a MigrationBuilder parameter")]
    [DataRow(DatabaseTypes.Sqlite, DisplayName = DatabaseTypes.Sqlite)]
    [DataRow(DatabaseTypes.SqlServer, DisplayName = DatabaseTypes.SqlServer)]
    public void DatabaseTypeTests_006(string databaseType)
    {
        // ARRANGE
        using Context context = new(databaseType, "Dummy");
        MigrationBuilder migrationBuilder = new(context.Database.ProviderName);

        // ACT
        string? actualDatabaseType = migrationBuilder.GetDatabaseType();

        // ASSERT
        Assert.AreEqual(databaseType, actualDatabaseType, "Unexpected database type");
    }

    #endregion public methods
}
