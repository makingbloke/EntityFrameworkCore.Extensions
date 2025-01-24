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
    #region public methods

    /// <summary>
    /// Test GetDatabaseType with a null <see cref="DatabaseFacade"/> object.
    /// </summary>
    [TestMethod]
    public void Test_GetDatabaseType_NullDatabaseFacade()
    {
        // ARRANGE
        DatabaseFacade? databaseFacade = null;

        // ACT / ASSERT
        Assert.ThrowsException<ArgumentNullException>(() => databaseFacade!.GetDatabaseType(), "Unexpected exception");
    }

    /// <summary>
    /// Test GetDatabaseType with a null <see cref="MigrationBuilder"/> object.
    /// </summary>
    [TestMethod]
    public void Test_GetDatabaseType_NullMigrationBuilder()
    {
        // ARRANGE
        MigrationBuilder? migrationBuilder = null;

        // ACT / ASSERT
        Assert.ThrowsException<ArgumentNullException>(() => migrationBuilder!.GetDatabaseType(), "Unexpected exception");
    }

    /// <summary>
    /// Test GetDatabaseType with invalid provider name.
    /// </summary>
    /// <param name="providerName">Name of database provider.</param>
    [TestMethod]
    [DataRow("unknown", DisplayName = "GetDatabaseType Unsupported string value.")]
    [DataRow(null, DisplayName = "GetDatabaseType Unsupported string value.")]
    public void Test_GetDatabaseType_UnsupportedProviderName(string providerName)
    {
        // ARRANGE
        string message = "Unsupported database type";

        // ACT / ASSERT
        InvalidOperationException e = Assert.ThrowsException<InvalidOperationException>(() => EntityFrameworkCore.Extensions.Extensions.DatabaseTypeExtensions.GetDatabaseType(providerName), "Unexpected exception");
        Assert.AreEqual(message, e.Message, "Unexpected exception message");
    }

    /// <summary>
    /// Test GetDatabaseType extension for a <see cref="DatabaseFacade"/> object.
    /// </summary>
    /// <param name="databaseType">Database type.</param>
    [TestMethod]
    [DataRow(DatabaseType.Sqlite, DisplayName = "SQLite GetDatabaseType for a DatabaseFacade object.")]
    [DataRow(DatabaseType.SqlServer, DisplayName = "SQL Server GetDatabaseType for a DatabaseFacade object.")]
    public void Test_GetDatabaseType_DatabaseFacade(string databaseType)
    {
        // ARRANGE
        using Context context = new(databaseType, "Dummy");

        // ACT
        string actualDatabaseType = context.Database.GetDatabaseType();

        // ASSERT
        Assert.AreEqual(databaseType, actualDatabaseType, "Invalid database type.");
    }

    /// <summary>
    /// Test GetDatabaseType extension for a <see cref="MigrationBuilder"/> object.
    /// </summary>
    /// <param name="databaseType">Database type.</param>
    [TestMethod]
    [DataRow(DatabaseType.Sqlite, DisplayName = "SQLite GetDatabaseType for a MigrationBuilder object.")]
    [DataRow(DatabaseType.SqlServer, DisplayName = "SQL Server GetDatabaseType for a MigrationBuilder object.")]
    public void Test_GetDatabaseType_MigrationBuilder(string databaseType)
    {
        // ARRANGE
        using Context context = new(databaseType, "Dummy");
        MigrationBuilder migrationBuilder = new(context.Database.ProviderName);

        // ACT
        string actualDatabaseType = migrationBuilder.GetDatabaseType();

        // ASSERT
        Assert.AreEqual(databaseType, actualDatabaseType, "Invalid database type.");
    }

    /// <summary>
    /// Test GetDatabaseType.
    /// </summary>
    /// <param name="databaseType">Database type.</param>
    /// <param name="providerName">Name of database provider.</param>
    [TestMethod]
    [DataRow(DatabaseType.Sqlite, "Microsoft.EntityFrameworkCore.Sqlite", DisplayName = "SQLite GetDatabaseType.")]
    [DataRow(DatabaseType.SqlServer, "Microsoft.EntityFrameworkCore.SqlServer", DisplayName = "SQL Server GetDatabaseType.")]
    public void Test_GetDatabaseType(string databaseType, string providerName)
    {
        // ARRANGE

        // ACT
        string actualDatabaseType = EntityFrameworkCore.Extensions.Extensions.DatabaseTypeExtensions.GetDatabaseType(providerName);

        // ASSERT
        Assert.AreEqual(databaseType, actualDatabaseType, "Invalid database type.");
    }

    #endregion public methods
}
