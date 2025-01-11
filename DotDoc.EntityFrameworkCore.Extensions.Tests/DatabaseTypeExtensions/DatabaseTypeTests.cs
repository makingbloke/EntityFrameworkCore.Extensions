// Copyright ©2021-2025 Mike King.
// This file is licensed to you under the MIT license.
// See the License.txt file in the solution root for more information.

using DotDoc.EntityFrameworkCore.Extensions.Constants;
using DotDoc.EntityFrameworkCore.Extensions.Extensions;
using DotDoc.EntityFrameworkCore.Extensions.Tests.Data;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DotDoc.EntityFrameworkCore.Extensions.Tests.DatabaseTypeExtensions;

/// <summary>
/// Tests for the GetDatabaseType extension.
/// </summary>
[TestClass]
public class DatabaseTypeTests
{
    /// <summary>
    /// Test GetDatabaseType extension for a <see cref="DatabaseFacade"/> object.
    /// </summary>
    /// <param name="databaseType">Database type.</param>
    [TestMethod]
    [DataRow(DatabaseType.Sqlite, DisplayName = "SQLite GetDatabaseType for a DatabaseFacade object.")]
    [DataRow(DatabaseType.SqlServer, DisplayName = "SQL Server GetDatabaseType for a DatabaseFacade object.")]
    public void Test_GetDatabaseType_DatabaseFacade(DatabaseType databaseType)
    {
        using Context context = new(databaseType, "Dummy");
        Assert.AreEqual(databaseType, context.Database.GetDatabaseType(), "Invalid database type.");
    }

    /// <summary>
    /// Test GetDatabaseType extension for a <see cref="MigrationBuilder"/> object.
    /// </summary>
    /// <param name="databaseType">Database type.</param>
    [TestMethod]
    [DataRow(DatabaseType.Sqlite, DisplayName = "SQLite GetDatabaseType for a MigrationBuilder object.")]
    [DataRow(DatabaseType.SqlServer, DisplayName = "SQL Server GetDatabaseType for a MigrationBuilder object.")]
    public void Test_GetDatabaseType_MigrationBuilder(DatabaseType databaseType)
    {
        using Context context = new(databaseType, "Dummy");
        MigrationBuilder migrationBuilder = new(context.Database.ProviderName);
        Assert.AreEqual(databaseType, migrationBuilder.GetDatabaseType(), "Invalid database type.");
    }

    /// <summary>
    /// Test GetDatabaseType.
    /// </summary>
    /// <param name="databaseType">Database type.</param>
    /// <param name="providerName">Name of database provider.</param>
    [TestMethod]
    [DataRow(DatabaseType.Unknown, "Unknown", DisplayName = "Unknown GetDatabaseType.")]
    [DataRow(DatabaseType.Sqlite, "Microsoft.EntityFrameworkCore.Sqlite", DisplayName = "SQLite GetDatabaseType.")]
    [DataRow(DatabaseType.SqlServer, "Microsoft.EntityFrameworkCore.SqlServer", DisplayName = "SQL Server GetDatabaseType.")]
    public void Test_GetDatabaseType(DatabaseType databaseType, string providerName)
    {
        Assert.AreEqual(databaseType, Extensions.DatabaseTypeExtensions.GetDatabaseType(providerName), "Invalid database type.");
    }

}
