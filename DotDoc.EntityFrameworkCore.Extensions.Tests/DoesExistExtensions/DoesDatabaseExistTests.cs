// Copyright ©2021-2025 Mike King.
// This file is licensed to you under the MIT license.
// See the License.txt file in the solution root for more information.

using DotDoc.EntityFrameworkCore.Extensions.Constants;
using DotDoc.EntityFrameworkCore.Extensions.Extensions;
using DotDoc.EntityFrameworkCore.Extensions.Tests.Data;
using DotDoc.EntityFrameworkCore.Extensions.Tests.Utilities;
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
    /// Test DoesDatabaseExist Guard Clause.
    /// </summary>
    [TestMethod("DoesDatabaseExist Guard Clause")]
    public void Test_DoesDatabaseExist_GuardClause()
    {
        // ARRANGE
        DatabaseFacade? databaseFacade = null;
        string paramName = "databaseFacade";

        // ACT / ASSERT
        ArgumentNullException e = Assert.ThrowsExactly<ArgumentNullException>(() => _ = databaseFacade!.DoesDatabaseExist(), "Missing exception");
        Assert.AreEqual(paramName, e.ParamName, "Invalid parameter name");
    }

    /// <summary>
    /// Test DoesDatabaseExistAsync Guard Clause.
    /// </summary>
    /// <returns>A task that represents the asynchronous test operation.</returns>
    [TestMethod("DoesDatabaseExistAsync Guard Clause")]
    public async Task Test_DoesDatabaseExist_GuardClauseAsync()
    {
        // ARRANGE
        DatabaseFacade? databaseFacade = null;
        string paramName = "databaseFacade";

        // ACT / ASSERT
        ArgumentNullException e = await Assert.ThrowsExactlyAsync<ArgumentNullException>(() => databaseFacade!.DoesDatabaseExistAsync(), "Missing exception").ConfigureAwait(false);
        Assert.AreEqual(paramName, e.ParamName, "Invalid parameter name");
    }

    /// <summary>
    /// Test DoesDatabaseExist when database exists.
    /// </summary>
    /// <param name="databaseType">Database type.</param>
    [TestMethod("DoesDatabaseExist when database exists")]
    [DataRow(DatabaseType.Sqlite, DisplayName = DatabaseType.Sqlite)]
    [DataRow(DatabaseType.SqlServer, DisplayName = DatabaseType.SqlServer)]
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
    /// Test DoesDatabaseExistAsync when database exists.
    /// </summary>
    /// <param name="databaseType">Database type.</param>
    /// <returns>A task that represents the asynchronous test operation.</returns>
    [TestMethod("DoesDatabaseExistAsync when database exists")]
    [DataRow(DatabaseType.Sqlite, DisplayName = DatabaseType.Sqlite)]
    [DataRow(DatabaseType.SqlServer, DisplayName = DatabaseType.SqlServer)]
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
    [TestMethod("DoesDatabaseExist when database does not exist")]
    [DataRow(DatabaseType.Sqlite, "Data Source = c:\\NonExistantDatabase.db", DisplayName = DatabaseType.Sqlite)]
    [DataRow(DatabaseType.SqlServer, "Server=localhost;Initial Catalog=NonExistantDatabase;Trusted_Connection=True;TrustServerCertificate=True", DisplayName = DatabaseType.SqlServer)]
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
    /// Test DoesDatabaseExistAsync when database does not exist.
    /// </summary>
    /// <param name="databaseType">Database type.</param>
    /// <param name="connectionString">Connection string.</param>
    /// <returns>A task that represents the asynchronous test operation.</returns>
    [TestMethod("DoesDatabaseExistAsync when database does not exist")]
    [DataRow(DatabaseType.Sqlite, "Data Source = c:\\NonExistantDatabase.db", DisplayName = DatabaseType.Sqlite)]
    [DataRow(DatabaseType.SqlServer, "Server=localhost;Initial Catalog=NonExistantDatabase;Trusted_Connection=True;TrustServerCertificate=True", DisplayName = DatabaseType.SqlServer)]
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
