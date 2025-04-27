// Copyright ©2021-2025 Mike King.
// This file is licensed to you under the MIT license.
// See the License.txt file in the solution root for more information.

using DotDoc.EntityFrameworkCore.Extensions.DatabaseType;
using DotDoc.EntityFrameworkCore.Extensions.Tests.Data;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DotDoc.EntityFrameworkCore.Extensions.Tests.DatabaseType;

/// <summary>
/// Tests for the GetDatabaseType extension.
/// </summary>
[TestClass]
public class DatabaseTypeTests
{
    #region public methods

    /// <summary>
    /// Test GetDatabaseType Guard Clause with a <see cref="DatabaseFacade"/> object.
    /// </summary>
    [TestMethod("GetDatabaseType Guard Clause with a DatabaseFacade object")]
    public void Test_GetDatabaseType_DatabaseFacade_GuardClause()
    {
        // ARRANGE
        DatabaseFacade? databaseFacade = null;
        string paramName = "databaseFacade";

        // ACT / ASSERT
        ArgumentNullException e = Assert.ThrowsExactly<ArgumentNullException>(() => _ = databaseFacade!.GetDatabaseType(), "Missing exception");
        Assert.AreEqual(paramName, e.ParamName, "Invalid parameter name");
    }

    /// <summary>
    /// Test GetDatabaseType Guard Clause with a <see cref="MigrationBuilder"/> object.
    /// </summary>
    [TestMethod("GetDatabaseType Guard Clause with a MigrationBuilder object")]
    public void Test_GetDatabaseType_MigrationBuilder_GuardClause()
    {
        // ARRANGE
        MigrationBuilder? migrationBuilder = null;
        string paramName = "migrationBuilder";

        // ACT / ASSERT
        ArgumentNullException e = Assert.ThrowsExactly<ArgumentNullException>(() => _ = migrationBuilder!.GetDatabaseType(), "Missing exception");
        Assert.AreEqual(paramName, e.ParamName, "Invalid parameter name");
    }

    /// <summary>
    /// Test GetDatabaseType extension with a <see cref="DatabaseFacade"/> object.
    /// </summary>
    /// <param name="databaseType">Database type.</param>
    [TestMethod("GetDatabaseType with a DatabaseFacade")]
    [DataRow(DatabaseTypes.Sqlite, DisplayName = DatabaseTypes.Sqlite)]
    [DataRow(DatabaseTypes.SqlServer, DisplayName = DatabaseTypes.SqlServer)]
    public void Test_GetDatabaseType_DatabaseFacade(string databaseType)
    {
        // ARRANGE
        using Context context = new(databaseType, "Dummy");

        // ACT
        string actualDatabaseType = context.Database.GetDatabaseType();

        // ASSERT
        Assert.AreEqual(databaseType, actualDatabaseType, "Invalid database type");
    }

    /// <summary>
    /// Test GetDatabaseType extension with a <see cref="MigrationBuilder"/> object.
    /// </summary>
    /// <param name="databaseType">Database type.</param>
    [TestMethod("GetDatabaseType with a MigrationBuilder object")]
    [DataRow(DatabaseTypes.Sqlite, DisplayName = DatabaseTypes.Sqlite)]
    [DataRow(DatabaseTypes.SqlServer, DisplayName = DatabaseTypes.SqlServer)]
    public void Test_GetDatabaseType_MigrationBuilder(string databaseType)
    {
        // ARRANGE
        using Context context = new(databaseType, "Dummy");
        MigrationBuilder migrationBuilder = new(context.Database.ProviderName);

        // ACT
        string actualDatabaseType = migrationBuilder.GetDatabaseType();

        // ASSERT
        Assert.AreEqual(databaseType, actualDatabaseType, "Invalid database type");
    }

    #endregion public methods
}
