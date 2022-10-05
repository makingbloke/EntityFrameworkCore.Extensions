// Copyright ©2021-2022 Mike King.
// This file is licensed to you under the MIT license.
// See the License.txt file in the solution root for more information.

using DotDoc.EntityFrameworkCore.Extensions.Constants;
using DotDoc.EntityFrameworkCore.Extensions.Extensions;
using DotDoc.EntityFrameworkCore.Extensions.Tests.Data;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DotDoc.EntityFrameworkCore.Extensions.Tests;

/// <summary>
/// Tests for the GetDatabaseType extension.
/// </summary>
[TestClass]
public class DatabaseTypeTests
{
    /// <summary>
    /// Test GetDatabaseType extension method for a <see cref="DatabaseFacade"/> can detect a database.
    /// </summary>
    /// <param name="databaseType">Database type.</param>
    [TestMethod]
    [DataRow(DatabaseType.Sqlite)]
    [DataRow(DatabaseType.SqlServer)]
    public void GetDatabaseTypeTestForDatabaseFacade(DatabaseType databaseType)
    {
        using Context context = new (databaseType, "Dummy");
        Assert.AreEqual(databaseType, context.Database.GetDatabaseType());
    }

    /// <summary>
    /// Test GetDatabaseType extension method for a <see cref="MigrationBuilder"/> can detect a database.
    /// </summary>
    /// <param name="databaseType">Database type.</param>
    [TestMethod]
    [DataRow(DatabaseType.Sqlite)]
    [DataRow(DatabaseType.SqlServer)]
    public void GetDatabaseTypeTestForMigrationBuilder(DatabaseType databaseType)
    {
        using Context context = new (databaseType, "Dummy");
        MigrationBuilder migrationBuilder = new (context.Database.ProviderName);
        Assert.AreEqual(databaseType, migrationBuilder.GetDatabaseType());
    }
}
