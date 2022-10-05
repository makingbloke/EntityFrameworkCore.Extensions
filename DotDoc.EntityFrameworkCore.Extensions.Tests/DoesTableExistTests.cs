// Copyright ©2021-2022 Mike King.
// This file is licensed to you under the MIT license.
// See the License.txt file in the solution root for more information.

using DotDoc.EntityFrameworkCore.Extensions.Constants;
using DotDoc.EntityFrameworkCore.Extensions.Extensions;
using DotDoc.EntityFrameworkCore.Extensions.Tests.Data;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DotDoc.EntityFrameworkCore.Extensions.Tests;

/// <summary>
/// Tests for DoesTableExist extensions.
/// </summary>
[TestClass]
public class DoesTableExistTests
{
    /// <summary>
    /// Test DoesTableExist with a table that exists.
    /// </summary>
    /// <param name="databaseType">Database type.</param>
    [TestMethod]
    [DataRow(DatabaseType.Sqlite)]
    [DataRow(DatabaseType.SqlServer)]
    public void DoesTableExistWhereTableExists(DatabaseType databaseType)
    {
        using Context context = DatabaseUtils.CreateDatabase(databaseType);

        Assert.IsTrue(context.Database.DoesTableExist("TestTable"));
    }

    /// <summary>
    /// Test DoesTableExist with a table that exists.
    /// </summary>
    /// <param name="databaseType">Database type.</param>
    /// <returns><see cref="Task"/>.</returns>
    [TestMethod]
    [DataRow(DatabaseType.Sqlite)]
    [DataRow(DatabaseType.SqlServer)]
    public async Task DoesTableExistWhereTableExistsAsync(DatabaseType databaseType)
    {
        using Context context = DatabaseUtils.CreateDatabase(databaseType);

        Assert.IsTrue(await context.Database.DoesTableExistAsync("TestTable").ConfigureAwait(false));
    }

    /// <summary>
    /// Test DoesTableExist with a table that does not exist.
    /// </summary>
    /// <param name="databaseType">Database type.</param>
    [TestMethod]
    [DataRow(DatabaseType.Sqlite)]
    [DataRow(DatabaseType.SqlServer)]
    public void DoesTableExistWhereTableDoesNotExist(DatabaseType databaseType)
    {
        using Context context = DatabaseUtils.CreateDatabase(databaseType);

        Assert.IsFalse(context.Database.DoesTableExist("NonExistantTable"));
    }

    /// <summary>
    /// Test DoesTableExist with a table that does not exist.
    /// </summary>
    /// <param name="databaseType">Database type.</param>
    /// <returns><see cref="Task"/>.</returns>
    [TestMethod]
    [DataRow(DatabaseType.Sqlite)]
    [DataRow(DatabaseType.SqlServer)]
    public async Task DoesTableExistWhereTableDoesNotExistAsync(DatabaseType databaseType)
    {
        using Context context = DatabaseUtils.CreateDatabase(databaseType);

        Assert.IsFalse(await context.Database.DoesTableExistAsync("NonExistantTable").ConfigureAwait(false));
    }
}
