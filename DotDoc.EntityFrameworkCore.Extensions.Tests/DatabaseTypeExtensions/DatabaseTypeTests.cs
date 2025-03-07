// Copyright �2021-2025 Mike King.
// This file is licensed to you under the MIT license.
// See the License.txt file in the solution root for more information.

using DotDoc.EntityFrameworkCore.Extensions.Constants;
using DotDoc.EntityFrameworkCore.Extensions.Extensions;
using DotDoc.EntityFrameworkCore.Extensions.Tests.Data;
using DotDoc.EntityFrameworkCore.Extensions.Tests.Utilities;
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
    /// Test GetDatabaseType Guard Clauses with a <see cref="string"/> object.
    /// </summary>
    /// <param name="providerName">Name of database provider.</param>
    /// <param name="exceptionType">The type of exception raised.</param>
    /// <param name="paramName">Name of parameter being checked.</param>
    [TestMethod("GetDatabaseType Guard Clause with a String object")]
    [DynamicData(nameof(Get_GetDatabaseType_String_GuardClause_TestData), DynamicDataSourceType.Method, DynamicDataDisplayName = nameof(TestUtils.CreateDynamicDisplayName), DynamicDataDisplayNameDeclaringType = typeof(TestUtils))]
    public void Test_GetDatabaseType_String_GuardClauses(string providerName, Type exceptionType, string paramName)
    {
        // ARRANGE

        // ACT / ASSERT
        Exception e = Assert.Throws<Exception>(() => EntityFrameworkCore.Extensions.Extensions.DatabaseTypeExtensions.GetDatabaseType(providerName), "Unexpected exception");
        Assert.AreEqual(exceptionType, e.GetType(), "Invalid exception type");
        Assert.AreEqual(paramName, ((ArgumentException)e).ParamName, "Invalid parameter name");
    }

    /// <summary>
    /// Test GetDatabaseType extension with a <see cref="DatabaseFacade"/> object.
    /// </summary>
    /// <param name="databaseType">Database type.</param>
    [TestMethod("GetDatabaseType with a DatabaseFacade")]
    [DataRow(DatabaseType.Sqlite, DisplayName = DatabaseType.Sqlite)]
    [DataRow(DatabaseType.SqlServer, DisplayName = DatabaseType.SqlServer)]
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
    [DataRow(DatabaseType.Sqlite, DisplayName = DatabaseType.Sqlite)]
    [DataRow(DatabaseType.SqlServer, DisplayName = DatabaseType.SqlServer)]
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

    /// <summary>
    /// Test GetDatabaseType with a <see cref="string"/> object.
    /// </summary>
    /// <param name="databaseType">Database type.</param>
    /// <param name="providerName">Name of database provider.</param>
    [TestMethod("GetDatabaseType with a String object")]
    [DataRow(DatabaseType.Sqlite, "Microsoft.EntityFrameworkCore.Sqlite", DisplayName = DatabaseType.Sqlite)]
    [DataRow(DatabaseType.SqlServer, "Microsoft.EntityFrameworkCore.SqlServer", DisplayName = DatabaseType.SqlServer)]
    public void Test_GetDatabaseType_String(string databaseType, string providerName)
    {
        // ARRANGE

        // ACT
        string actualDatabaseType = EntityFrameworkCore.Extensions.Extensions.DatabaseTypeExtensions.GetDatabaseType(providerName);

        // ASSERT
        Assert.AreEqual(databaseType, actualDatabaseType, "Invalid database type");
    }

    /// <summary>
    /// Test GetDatabaseType with a <see cref="string"/> object and an unsupported provider name.
    /// </summary>
    [TestMethod("GetDatabaseType with a String object with an unsupported provider name")]
    public void Test_GetDatabaseType_String_UnsupportedProviderName()
    {
        // ARRANGE
        string providerName = "unknown";
        string paramName = "providerName";

        // ACT / ASSERT
        ArgumentException e = Assert.ThrowsExactly<ArgumentException>(() => _ = EntityFrameworkCore.Extensions.Extensions.DatabaseTypeExtensions.GetDatabaseType(providerName), "Unexpected exception");
        Assert.AreEqual(paramName, e.ParamName, "Invalid parameter name");
    }

    #endregion public methods

    #region private methods

    /// <summary>
    /// Get test data for the GetDatabaseType method with a <see cref="string"/> object.
    /// </summary>
    /// <returns><see cref="IEnumerable{T}"/>.</returns>
    private static IEnumerable<object?[]> Get_GetDatabaseType_String_GuardClause_TestData()
    {
        // 0. string providerName
        // 1. Type exceptionType
        // 2. string paramName
        yield return [
            null,
            typeof(ArgumentNullException),
            "providerName"];

        yield return [
            string.Empty,
            typeof(ArgumentException),
            "providerName"];
    }

    #endregion private methods
}
