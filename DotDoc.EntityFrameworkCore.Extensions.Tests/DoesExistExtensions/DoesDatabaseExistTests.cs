// Copyright ©2021-2023 Mike King.
// This file is licensed to you under the MIT license.
// See the License.txt file in the solution root for more information.

using DotDoc.EntityFrameworkCore.Extensions.Constants;
using DotDoc.EntityFrameworkCore.Extensions.Tests.Data;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DotDoc.EntityFrameworkCore.Extensions.Tests.DoesExistExtensions;

/// <summary>
/// Tests for DoesDatabaseExist extensions.
/// </summary>
[TestClass]
public class DoesDatabaseExistTests
{
    /// <summary>
    /// Test DoesDatabaseExist when database exists.
    /// </summary>
    /// <param name="databaseType">Database type.</param>
    /// <param name="useAsync">If <see langword="true"/> then tests the async method.</param>
    /// <returns>A task that represents the asynchronous test operation.</returns>
    [TestMethod]
    [DataRow(DatabaseType.Sqlite, false, DisplayName = "SQLite DoesDatabaseExist when database exists.")]
    [DataRow(DatabaseType.Sqlite, true, DisplayName = "SQLite DoesDatabaseExistAsync when database exists.")]
    [DataRow(DatabaseType.SqlServer, false, DisplayName = "SQL Server DoesDatabaseExist when database exists.")]
    [DataRow(DatabaseType.SqlServer, true, DisplayName = "SQL Server DoesDatabaseExistAsync when database exists.")]
    public async Task TestDoesDatabaseExistWhenDatabaseExistsAsync(DatabaseType databaseType, bool useAsync)
    {
        using Context context = DatabaseUtils.CreateDatabase(databaseType);

        bool result = useAsync
            ? await context.Database.DoesDatabaseExistAsync().ConfigureAwait(false)
            : context.Database.DoesDatabaseExist();

        Assert.IsTrue(result, "Database does not exist.");
    }

    /// <summary>
    /// Test DoesDatabaseExist when database does not exist.
    /// </summary>
    /// <param name="databaseType">Database type.</param>
    /// <param name="connectionString">Connection string.</param>
    /// <param name="useAsync">If <see langword="true"/> then tests the async method.</param>
    /// <returns>A task that represents the asynchronous test operation.</returns>
    [TestMethod]
    [DataRow(DatabaseType.Sqlite, "Data Source = c:\\NonExistantDatabase.db", false, DisplayName = "SQLite DoesDatabaseExist when database does not exist.")]
    [DataRow(DatabaseType.Sqlite, "Data Source = c:\\NonExistantDatabase.db", true, DisplayName = "SQLite DoesDatabaseExistAsync when database does not exist.")]
    [DataRow(DatabaseType.SqlServer, "Server=localhost;Initial Catalog=NonExistantDatabase;Trusted_Connection=True;TrustServerCertificate=True", false, DisplayName = "SQL Server DoesDatabaseExist when database does not exist.")]
    [DataRow(DatabaseType.SqlServer, "Server=localhost;Initial Catalog=NonExistantDatabase;Trusted_Connection=True;TrustServerCertificate=True", true, DisplayName = "SQL Server DoesDatabaseExistAsync when database does not exist.")]
    public async Task TestDoesDatabaseExistWhenDatabaseDoesNotExistAsync(DatabaseType databaseType, string connectionString, bool useAsync)
    {
        using Context context = new (databaseType, connectionString);

        bool result = useAsync
            ? await context.Database.DoesDatabaseExistAsync().ConfigureAwait(false)
            : context.Database.DoesDatabaseExist();

        Assert.IsFalse(result, "Database exists.");
    }
}
