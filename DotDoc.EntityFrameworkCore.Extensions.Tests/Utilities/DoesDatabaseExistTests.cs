// Copyright ©2021-2025 Mike King.
// This file is licensed to you under the MIT license.
// See the License.txt file in the solution root for more information.

using DotDoc.EntityFrameworkCore.Extensions.DatabaseType;
using DotDoc.EntityFrameworkCore.Extensions.Tests.Data;
using DotDoc.EntityFrameworkCore.Extensions.Utilities;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace DotDoc.EntityFrameworkCore.Extensions.Tests.Utilities;

/// <summary>
/// Tests for DoesDatabaseExist extensions.
/// </summary>
[TestClass]
public class DoesDatabaseExistTests
{
    #region public methods

    /// <summary>
    /// Test DoesDatabaseExistAsync with a Null DatabaseFacade Database parameter.
    /// </summary>
    /// <returns>A <see cref="Task"/> that represents the asynchronous operation.</returns>
    [TestMethod(DisplayName = "DoesDatabaseExistAsync with a Null DatabaseFacade Database parameter")]
    public async Task DoesDatabaseExistTests_001_Async()
    {
        // ARRANGE
        DatabaseFacade database = null!;

        // ACT / ASSERT
        await Assert.ThrowsExactlyAsync<ArgumentNullException>(() => database.DoesDatabaseExistAsync(CancellationToken.None), "Unexpected exception").ConfigureAwait(false);
    }

    /// <summary>
    /// Test DoesDatabaseExistAsync when database exists.
    /// </summary>
    /// <param name="databaseType">Database type.</param>
    /// <returns>A <see cref="Task"/> that represents the asynchronous operation.</returns>
    [TestMethod(DisplayName = "DoesDatabaseExistAsync when database exists")]
    [DataRow(DatabaseTypes.Sqlite, DisplayName = DatabaseTypes.Sqlite)]
    [DataRow(DatabaseTypes.SqlServer, DisplayName = DatabaseTypes.SqlServer)]
    public async Task DoesDatabaseExistTests_002_Async(string databaseType)
    {
        // ARRANGE
        using Context context = await DatabaseUtils.CreateDatabaseAsync(databaseType).ConfigureAwait(false);

        // ACT
        bool result = await context.Database.DoesDatabaseExistAsync(CancellationToken.None).ConfigureAwait(false);

        // ASSERT
        Assert.IsTrue(result, "Database does not exist.");
    }

    /// <summary>
    /// Test DoesDatabaseExistAsync when database does not exist.
    /// </summary>
    /// <param name="databaseType">Database type.</param>
    /// <param name="connectionString">Connection string.</param>
    /// <returns>A <see cref="Task"/> that represents the asynchronous operation.</returns>
    [TestMethod(DisplayName = "DoesDatabaseExistAsync when database does not exist")]
    [DataRow(DatabaseTypes.Sqlite, @"Data Source = c:\NonExistantDatabase.db", DisplayName = DatabaseTypes.Sqlite)]
    [DataRow(DatabaseTypes.SqlServer, "Server=localhost;Initial Catalog=NonExistantDatabase;Trusted_Connection=True;TrustServerCertificate=True", DisplayName = DatabaseTypes.SqlServer)]
    public async Task DoesDatabaseExistTests_003_Async(string databaseType, string connectionString)
    {
        // ARRANGE
        using Context context = new(databaseType, connectionString);

        // ACT
        bool result = await context.Database.DoesDatabaseExistAsync(CancellationToken.None).ConfigureAwait(false);

        // ASSERT
        Assert.IsFalse(result, "Database exists.");
    }

    #endregion public methods
}
