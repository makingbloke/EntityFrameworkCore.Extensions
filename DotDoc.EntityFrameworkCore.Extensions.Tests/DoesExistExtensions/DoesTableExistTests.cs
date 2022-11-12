// Copyright ©2021-2023 Mike King.
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
    /// <param name="useAsync">If <see langword="true"/> then tests the async method.</param>
    /// <returns><see cref="Task"/>.</returns>
    [TestMethod]
    [DataRow(DatabaseType.Sqlite, false, DisplayName = "SQLite DoesTableExist when table exists.")]
    [DataRow(DatabaseType.Sqlite, true, DisplayName = "SQLite DoesTableExistAsync when table exists.")]
    [DataRow(DatabaseType.SqlServer, false, DisplayName = "SQL Server DoesTableExist when table exists.")]
    [DataRow(DatabaseType.SqlServer, true, DisplayName = "SQL Server DoesTableExistAsync when table exists.")]
    public async Task TestDoesTableExistWhenTableExistsAsync(DatabaseType databaseType, bool useAsync)
    {
        using Context context = DatabaseUtils.CreateDatabase(databaseType);

        bool result = useAsync
            ? await context.Database.DoesTableExistAsync("TestTable1").ConfigureAwait(false)
            : context.Database.DoesTableExist("TestTable1");

        Assert.IsTrue(result, "Table does not exist");
    }

    /// <summary>
    /// Test DoesTableExist when table does not exist.
    /// </summary>
    /// <param name="databaseType">Database type.</param>
    /// <param name="useAsync">If <see langword="true"/> then tests the async method.</param>
    /// <returns><see cref="Task"/>.</returns>
    [TestMethod]
    [DataRow(DatabaseType.Sqlite, false, DisplayName = "SQLite DoesTableExist when table does not exist.")]
    [DataRow(DatabaseType.Sqlite, true, DisplayName = "SQLite DoesTableExistAsync when table does not exist.")]
    [DataRow(DatabaseType.SqlServer, false, DisplayName = "SQL Server DoesTableExist when table does not exist.")]
    [DataRow(DatabaseType.SqlServer, true, DisplayName = "SQL Server DoesTableExistAsync when table does not exist.")]
    public async Task TestDoesTableExistWhenTableDoesNotExistAsync(DatabaseType databaseType, bool useAsync)
    {
        using Context context = DatabaseUtils.CreateDatabase(databaseType);

        bool result = useAsync
            ? await context.Database.DoesTableExistAsync("NonExistantTable").ConfigureAwait(false)
            : context.Database.DoesTableExist("NonExistantTable");

        Assert.IsFalse(result, "Table does exist");
    }
}
