// Copyright ©2021-2026 Mike King.
// This file is licensed to you under the MIT license.
// See the License.txt file in the solution root for more information.

using DotDoc.EntityFrameworkCore.Extensions.DatabaseType;

namespace DotDoc.EntityFrameworkCore.Extensions.Tests.DatabaseType;

/// <summary>
/// Tests for the GetDatabaseType extension.
/// </summary>
[TestClass]
public class UnsupportedDatabaseTypeExceptionTests
{
    #region public methods

    /// <summary>
    /// Test ThrowIfInvalidDatabaseType with a valid String DatabaseType.
    /// </summary>
    /// <param name="databaseType">Database type.</param>
    [TestMethod(DisplayName = "ThrowIfInvalidDatabaseType with a valid String DatabaseType")]
    [DataRow(DatabaseTypes.Sqlite, DisplayName = DatabaseTypes.Sqlite)]
    [DataRow(DatabaseTypes.SqlServer, DisplayName = DatabaseTypes.SqlServer)]
    public void UnsupportedDatabaseTypeExceptionTests_001(string databaseType)
    {
        // ARRANGE

        // ACT / ASSERT
        UnsupportedDatabaseTypeException.ThrowIfInvalidDatabaseType(databaseType);
    }

    /// <summary>
    /// Test ThrowIfInvalidDatabaseType with an unsupported String DatabaseType.
    /// </summary>
    /// <param name="databaseType">Database type.</param>
    [TestMethod(DisplayName = "ThrowIfInvalidDatabaseType with an unsupported String DatabaseType")]
    [DataRow(null, DisplayName = "Null")]
    [DataRow("", DisplayName = "Empty")]
    [DataRow("UnsupportedDatabaseType", DisplayName = "Unsupported")]
    public void UnsupportedDatabaseTypeExceptionTests_002(string? databaseType)
    {
        // ARRANGE

        // ACT / ASSERT
        Assert.ThrowsExactly<UnsupportedDatabaseTypeException>(() => UnsupportedDatabaseTypeException.ThrowIfInvalidDatabaseType(databaseType), "Unexpected exception");
    }

    /// <summary>
    /// Test UnsupportedDatabaseTypeException() constructor.
    /// </summary>
    [TestMethod(DisplayName = "UnsupportedDatabaseTypeException() constructor")]
    public void UnsupportedDatabaseTypeExceptionTests_003()
    {
        // ARRANGE / ACT
        UnsupportedDatabaseTypeException e = new();

        // ASSERT
        Assert.AreEqual("Unsupported database type", e.Message, "Invalid message");
        Assert.IsNull(e.ParamName, "Invalid parameter name");
        Assert.IsNull(e.InnerException, "Invalid inner exception");
    }

    /// <summary>
    /// Test UnsupportedDatabaseTypeException(string ParamName, Exception InnerException) constructor.
    /// </summary>
    [TestMethod(DisplayName = "UnsupportedDatabaseTypeException(string ParamName, Exception InnerException) constructor")]
    public void UnsupportedDatabaseTypeExceptionTests_004()
    {
        // ARRANGE
        string paramName = "databaseType";
        InvalidOperationException innerException = new();

        // ACT
        UnsupportedDatabaseTypeException e = new(paramName, innerException);

        // ASSERT
        Assert.StartsWith("Unsupported database type", e.Message, "Invalid message");
        Assert.AreEqual(paramName, e.ParamName, "Invalid parameter name");
        Assert.AreSame(innerException, e.InnerException, "Invalid inner exception");
    }

    /// <summary>
    /// Test ThrowIfInvalidDatabaseType uses the argument passed to it as the parameter name.
    /// </summary>
    [TestMethod(DisplayName = "ThrowIfInvalidDatabaseType uses the argument passed to it as the parameter name")]
    public void UnsupportedDatabaseTypeExceptionTests_005()
    {
        // ARRANGE
        string myDatabaseType = "UnsupportedDatabaseType";

        // ACT
        UnsupportedDatabaseTypeException e = Assert.ThrowsExactly<UnsupportedDatabaseTypeException>(() => UnsupportedDatabaseTypeException.ThrowIfInvalidDatabaseType(myDatabaseType), "Unexpected exception");

        // ASSERT
        Assert.AreEqual(nameof(myDatabaseType), e.ParamName, "Invalid parameter name");
    }

    #endregion public methods
}