// Copyright ©2021-2025 Mike King.
// This file is licensed to you under the MIT license.
// See the License.txt file in the solution root for more information.

using DotDoc.EntityFrameworkCore.Extensions.Constants;
using DotDoc.EntityFrameworkCore.Extensions.Extensions;
using DotDoc.EntityFrameworkCore.Extensions.Tests.Data;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DotDoc.EntityFrameworkCore.Extensions.Tests.DoesExistExtensions;

/// <summary>
/// Tests for DoesDatabaseExist extensions.
/// </summary>
[TestClass]
public class DoesDatabaseExistTests
{
    #region public methods

    /// <summary>
    /// Test DoesDatabaseExist with a null <see cref="DatabaseFacade"/> object.
    /// </summary>
    [TestMethod]
    public void Test_DoesDatabaseExist_NullDatabaseFacade()
    {
        // ARRANGE
        DatabaseFacade? databaseFacade = null;

        // ACT / ASSERT
        Assert.ThrowsException<ArgumentNullException>(() => databaseFacade!.DoesDatabaseExist(), "Unexpected exception");
    }

    /// <summary>
    /// Test DoesDatabaseExist with a null <see cref="DatabaseFacade"/> object.
    /// </summary>
    /// <returns>A task that represents the asynchronous test operation.</returns>
    [TestMethod]
    public async Task Test_DoesDatabaseExist_NullDatabaseFacadeAsync()
    {
        // ARRANGE
        DatabaseFacade? databaseFacade = null;

        // ACT / ASSERT
        await Assert.ThrowsExceptionAsync<ArgumentNullException>(() => databaseFacade!.DoesDatabaseExistAsync(), "Unexpected exception").ConfigureAwait(false);
    }

    /// <summary>
    /// Test DoesDatabaseExist when database exists.
    /// </summary>
    /// <param name="databaseType">Database type.</param>
    [TestMethod]
    [DataRow(DatabaseType.Sqlite, DisplayName = "SQLite DoesDatabaseExist when database exists.")]
    [DataRow(DatabaseType.SqlServer, DisplayName = "SQL Server DoesDatabaseExist when database exists.")]
    public void Test_DoesDatabaseExist_DatabaseExists(string databaseType)
    {
        // ARRANGE
        using Context context = DatabaseUtils.CreateDatabase(databaseType);

        // ACT
        bool result = context.Database.DoesDatabaseExist();

        // ASSERT
        Assert.IsTrue(result, "Database does not exist.");
    }

    /// <summary>
    /// Test DoesDatabaseExist when database exists.
    /// </summary>
    /// <param name="databaseType">Database type.</param>
    /// <returns>A task that represents the asynchronous test operation.</returns>
    [TestMethod]
    [DataRow(DatabaseType.Sqlite, DisplayName = "SQLite DoesDatabaseExistAsync when database exists.")]
    [DataRow(DatabaseType.SqlServer, DisplayName = "SQL Server DoesDatabaseExistAsync when database exists.")]
    public async Task Test_DoesDatabaseExist_DatabaseExistsAsync(string databaseType)
    {
        // ARRANGE
        using Context context = DatabaseUtils.CreateDatabase(databaseType);

        // ACT
        bool result = await context.Database.DoesDatabaseExistAsync().ConfigureAwait(false);

        // ASSERT
        Assert.IsTrue(result, "Database does not exist.");
    }

    /// <summary>
    /// Test DoesDatabaseExist when database does not exist.
    /// </summary>
    /// <param name="databaseType">Database type.</param>
    /// <param name="connectionString">Connection string.</param>
    [TestMethod]
    [DataRow(DatabaseType.Sqlite, "Data Source = c:\\NonExistantDatabase.db", DisplayName = "SQLite DoesDatabaseExist when database does not exist.")]
    [DataRow(DatabaseType.SqlServer, "Server=localhost;Initial Catalog=NonExistantDatabase;Trusted_Connection=True;TrustServerCertificate=True", DisplayName = "SQL Server DoesDatabaseExist when database does not exist.")]
    public void Test_DoesDatabaseExist_DatabaseDoesNotExist(string databaseType, string connectionString)
    {
        // ARRANGE
        using Context context = new(databaseType, connectionString);

        // ACT
        bool result = context.Database.DoesDatabaseExist();

        // ASSERT
        Assert.IsFalse(result, "Database exists.");
    }

    /// <summary>
    /// Test DoesDatabaseExist when database does not exist.
    /// </summary>
    /// <param name="databaseType">Database type.</param>
    /// <param name="connectionString">Connection string.</param>
    /// <returns>A task that represents the asynchronous test operation.</returns>
    [TestMethod]
    [DataRow(DatabaseType.Sqlite, "Data Source = c:\\NonExistantDatabase.db", DisplayName = "SQLite DoesDatabaseExistAsync when database does not exist.")]
    [DataRow(DatabaseType.SqlServer, "Server=localhost;Initial Catalog=NonExistantDatabase;Trusted_Connection=True;TrustServerCertificate=True", DisplayName = "SQL Server DoesDatabaseExistAsync when database does not exist.")]
    public async Task Test_DoesDatabaseExist_DatabaseDoesNotExistAsync(string databaseType, string connectionString)
    {
        // ARRANGE
        using Context context = new(databaseType, connectionString);

        // ACT
        bool result = await context.Database.DoesDatabaseExistAsync().ConfigureAwait(false);

        // ASSERT
        Assert.IsFalse(result, "Database exists.");
    }

    #endregion public methods
}
