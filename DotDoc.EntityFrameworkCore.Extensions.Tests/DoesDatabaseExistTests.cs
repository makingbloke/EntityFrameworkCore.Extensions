// Copyright ©2021-2022 Mike King.
// This file is licensed to you under the MIT license.
// See the License.txt file in the solution root for more information.

using DotDoc.EntityFrameworkCore.Extensions.Constants;
using DotDoc.EntityFrameworkCore.Extensions.Tests.Data;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DotDoc.EntityFrameworkCore.Extensions.Tests;

/// <summary>
/// Tests for DoesDatabaseExist extensions.
/// </summary>
[TestClass]
public class DoesDatabaseExistTests
{
    /// <summary>
    /// Test DoesDatabaseExist with a database that exists.
    /// </summary>
    /// <param name="databaseType">Database type.</param>
    [TestMethod]
    [DataRow(DatabaseType.Sqlite)]
    [DataRow(DatabaseType.SqlServer)]
    public void DoesDatabaseExistWhereDatabaseExists(DatabaseType databaseType)
    {
        using Context context = DatabaseUtils.CreateDatabase(databaseType);

        Assert.IsTrue(context.Database.DoesDatabaseExist());
    }

    /// <summary>
    /// Test DoesDatabaseExist with a database that exists.
    /// </summary>
    /// <param name="databaseType">Database type.</param>
    /// <returns><see cref="Task"/>.</returns>
    [TestMethod]
    [DataRow(DatabaseType.Sqlite)]
    [DataRow(DatabaseType.SqlServer)]
    public async Task DoesDatabaseExistWhereDatabaseExistsAsync(DatabaseType databaseType)
    {
        using Context context = DatabaseUtils.CreateDatabase(databaseType);

        Assert.IsTrue(await context.Database.DoesDatabaseExistAsync().ConfigureAwait(false));
    }

    /// <summary>
    /// Test DoesDatabaseExist with a database that does not exist.
    /// </summary>
    /// <param name="databaseType">Database type.</param>
    /// <param name="connectionString">Connection string.</param>
    [TestMethod]
    [DataRow(DatabaseType.Sqlite, "Data Source = c:\\NonExistantDatabase.db")]
    [DataRow(DatabaseType.SqlServer, "Server=localhost;Database=NonExistantDatabase;Trusted_Connection=True")]
    public void DoesDatabaseExistWhereDatabaseDoesNotExist(DatabaseType databaseType, string connectionString)
    {
        using Context context = new (databaseType, connectionString);

        Assert.IsFalse(context.Database.DoesDatabaseExist());
    }

    /// <summary>
    /// Test DoesDatabaseExist with a database that does not exist.
    /// </summary>
    /// <param name="databaseType">Database type.</param>
    /// <param name="connectionString">Connection string.</param>
    /// <returns><see cref="Task"/>.</returns>
    [TestMethod]
    [DataRow(DatabaseType.Sqlite, "Data Source = c:\\NonExistantDatabase.db")]
    [DataRow(DatabaseType.SqlServer, "Server=localhost;Database=NonExistantDatabase;Trusted_Connection=True")]
    public async Task DoesDatabaseExistWhereDatabaseDoesNotExistAsync(DatabaseType databaseType, string connectionString)
    {
        using Context context = new (databaseType, connectionString);

        Assert.IsFalse(await context.Database.DoesDatabaseExistAsync().ConfigureAwait(false));
    }

}
