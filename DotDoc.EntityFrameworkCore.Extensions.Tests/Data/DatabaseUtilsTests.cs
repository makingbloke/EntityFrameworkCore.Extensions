// Copyright ©2021-2026 Mike King.
// This file is licensed to you under the MIT license.
// See the License.txt file in the solution root for more information.

using Microsoft.Data.SqlClient;

namespace DotDoc.EntityFrameworkCore.Extensions.Tests.Data;

/// <summary>
/// Tests for the test database settings in <see cref="DatabaseUtils"/>.
/// </summary>
/// <remarks>The environment variable itself isn't changed, as it is shared by every test; its value is passed in instead.</remarks>
[TestClass]
public class DatabaseUtilsTests
{
    #region private constants

    /// <summary>
    /// The database name used by the tests.
    /// </summary>
    private const string DatabaseName = "TestDatabase";

    #endregion private constants

    #region BuildSqlServerConnectionString tests

    /// <summary>
    /// Test BuildSqlServerConnectionString when the environment variable isn't set (or is empty).
    /// </summary>
    /// <param name="setting">The value of the environment variable.</param>
    [TestMethod(DisplayName = "BuildSqlServerConnectionString when the environment variable isn't set")]
    [DataRow(null, DisplayName = "Not set")]
    [DataRow("", DisplayName = "Empty")]
    [DataRow("  ", DisplayName = "White space")]
    public void DatabaseUtilsTests_001(string? setting)
    {
        // ARRANGE

        // ACT
        SqlConnectionStringBuilder builder = new(DatabaseUtils.BuildSqlServerConnectionString(setting, DatabaseName));

        // ASSERT
        Assert.AreEqual("localhost", builder.DataSource, "Invalid server");
        Assert.IsTrue(builder.IntegratedSecurity, "Invalid authentication");
        Assert.IsTrue(builder.TrustServerCertificate, "Invalid trust server certificate");
        Assert.AreEqual(DatabaseName, builder.InitialCatalog, "Invalid database");
    }

    /// <summary>
    /// Test BuildSqlServerConnectionString uses the server and login from the environment variable, and replaces its database.
    /// </summary>
    [TestMethod(DisplayName = "BuildSqlServerConnectionString uses the server and login from the environment variable")]
    public void DatabaseUtilsTests_002()
    {
        // ARRANGE
        string setting = "Server=sqlserver,1433;User Id=sa;Password=Test Password;TrustServerCertificate=True;Initial Catalog=OtherDatabase";

        // ACT
        SqlConnectionStringBuilder builder = new(DatabaseUtils.BuildSqlServerConnectionString(setting, DatabaseName));

        // ASSERT
        Assert.AreEqual("sqlserver,1433", builder.DataSource, "Invalid server");
        Assert.IsFalse(builder.IntegratedSecurity, "Invalid authentication");
        Assert.AreEqual("sa", builder.UserID, "Invalid user");
        Assert.AreEqual("Test Password", builder.Password, "Invalid password");
        Assert.AreEqual(DatabaseName, builder.InitialCatalog, "Invalid database");
    }

    /// <summary>
    /// Test BuildSqlServerConnectionString skips the test when the environment variable turns the SQL Server tests off.
    /// </summary>
    /// <param name="setting">The value of the environment variable.</param>
    [TestMethod(DisplayName = "BuildSqlServerConnectionString when the SQL Server tests are turned off")]
    [DataRow(DatabaseUtils.DatabaseTurnedOff, DisplayName = "Lower case")]
    [DataRow("NONE", DisplayName = "Upper case")]
    [DataRow(" none ", DisplayName = "With spaces")]
    public void DatabaseUtilsTests_003(string setting)
    {
        // ARRANGE

        // ACT / ASSERT
        AssertInconclusiveException exception = Assert.ThrowsExactly<AssertInconclusiveException>(() => DatabaseUtils.BuildSqlServerConnectionString(setting, DatabaseName), "Unexpected exception");
        Assert.Contains(DatabaseUtils.SqlServerConnectionStringVariable, exception.Message, "Unexpected exception message");
    }

    #endregion BuildSqlServerConnectionString tests
}
