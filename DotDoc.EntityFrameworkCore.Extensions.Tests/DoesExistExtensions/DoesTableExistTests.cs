// Copyright ©2021-2025 Mike King.
// This file is licensed to you under the MIT license.
// See the License.txt file in the solution root for more information.

using DotDoc.EntityFrameworkCore.Extensions.Constants;
using DotDoc.EntityFrameworkCore.Extensions.Tests.Data;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DotDoc.EntityFrameworkCore.Extensions.Tests.DoesExistExtensions;

/// <summary>
/// Tests for DoesTableExist extensions.
/// </summary>
[TestClass]
public class DoesTableExistTests
{
    /// <summary>
    /// Test DoesTableExist when table exists.
    /// </summary>
    /// <param name="databaseType">Database type.</param>
    [TestMethod]
    [DataRow(DatabaseType.Sqlite, DisplayName = "SQLite DoesTableExist when table exists.")]
    [DataRow(DatabaseType.SqlServer, DisplayName = "SQL Server DoesTableExist when table exists.")]
    public void Test_DoesTableExist_TableExists(DatabaseType databaseType)
    {
        using Context context = DatabaseUtils.CreateDatabase(databaseType);

        bool result = context.Database.DoesTableExist("TestTable1");

        Assert.IsTrue(result, "Table does not exist");
    }

    /// <summary>
    /// Test DoesTableExist when table exists.
    /// </summary>
    /// <param name="databaseType">Database type.</param>
    /// <returns>A task that represents the asynchronous test operation.</returns>
    [TestMethod]
    [DataRow(DatabaseType.Sqlite, DisplayName = "SQLite DoesTableExist when table exists.")]
    [DataRow(DatabaseType.SqlServer, DisplayName = "SQL Server DoesTableExist when table exists.")]
    public async Task TestDoesTableExistWhenTableExistsAsync(DatabaseType databaseType)
    {
        using Context context = DatabaseUtils.CreateDatabase(databaseType);

        bool result = await context.Database.DoesTableExistAsync("TestTable1").ConfigureAwait(false);

        Assert.IsTrue(result, "Table does not exist");
    }

    /// <summary>
    /// Test DoesTableExist when table does not exist.
    /// </summary>
    /// <param name="databaseType">Database type.</param>
    [TestMethod]
    [DataRow(DatabaseType.Sqlite, DisplayName = "SQLite DoesTableExist when table does not exist.")]
    [DataRow(DatabaseType.SqlServer, DisplayName = "SQL Server DoesTableExist when table does not exist.")]
    public void Test_DoesTableExist_TableDoesNotExist(DatabaseType databaseType)
    {
        using Context context = DatabaseUtils.CreateDatabase(databaseType);

        bool result = context.Database.DoesTableExist("NonExistantTable");

        Assert.IsFalse(result, "Table does exist");
    }

    /// <summary>
    /// Test DoesTableExist when table does not exist.
    /// </summary>
    /// <param name="databaseType">Database type.</param>
    /// <returns>A task that represents the asynchronous test operation.</returns>
    [TestMethod]
    [DataRow(DatabaseType.Sqlite, DisplayName = "SQLite DoesTableExist when table does not exist.")]
    [DataRow(DatabaseType.SqlServer, DisplayName = "SQL Server DoesTableExist when table does not exist.")]
    public async Task Test_DoesTableExist_TableDoesNotExistAsync(DatabaseType databaseType)
    {
        using Context context = DatabaseUtils.CreateDatabase(databaseType);

        bool result = await context.Database.DoesTableExistAsync("NonExistantTable").ConfigureAwait(false);

        Assert.IsFalse(result, "Table does exist");
    }
}
