// Copyright ©2021-2026 Mike King.
// This file is licensed to you under the MIT license.
// See the License.txt file in the solution root for more information.

using DotDoc.EntityFrameworkCore.Extensions.DatabaseType;

namespace DotDoc.EntityFrameworkCore.Extensions.Tests.DatabaseType;

/// <summary>
/// Tests for <see cref="UnsupportedDatabaseTypeException"/>.
/// </summary>
[TestClass]
public class UnsupportedDatabaseTypeExceptionTests
{
    #region constructor tests

    /// <summary>
    /// Test UnsupportedDatabaseTypeException() constructor.
    /// </summary>
    [TestMethod]
    public void Constructor_NoArguments_HasNoParamNameOrInnerException()
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
    [TestMethod]
    public void Constructor_ParamNameAndInnerException_SetsBoth()
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

    #endregion constructor tests

    #region ThrowIfInvalidDatabaseType tests

    /// <summary>
    /// Test ThrowIfInvalidDatabaseType with a valid String DatabaseType.
    /// </summary>
    /// <param name="databaseType">Database type.</param>
    [TestMethod]
    [DataRow(DatabaseTypes.Sqlite, DisplayName = $"SupportedType {DatabaseTypes.Sqlite}")]
    [DataRow(DatabaseTypes.SqlServer, DisplayName = $"SupportedType {DatabaseTypes.SqlServer}")]
    public void ThrowIfInvalidDatabaseType_SupportedType_DoesNotThrow(string databaseType)
    {
        // ARRANGE

        // ACT / ASSERT
        UnsupportedDatabaseTypeException.ThrowIfInvalidDatabaseType(databaseType);
    }

    /// <summary>
    /// Test ThrowIfInvalidDatabaseType with an unsupported String DatabaseType.
    /// </summary>
    /// <param name="databaseType">Database type.</param>
    [TestMethod]
    [DataRow(null, DisplayName = "UnsupportedType Null")]
    [DataRow("", DisplayName = "UnsupportedType Empty")]
    [DataRow("UnsupportedDatabaseType", DisplayName = "UnsupportedType Unsupported")]
    public void ThrowIfInvalidDatabaseType_UnsupportedType_ThrowsUnsupportedDatabaseTypeException(string? databaseType)
    {
        // ARRANGE

        // ACT / ASSERT
        Assert.ThrowsExactly<UnsupportedDatabaseTypeException>(() => UnsupportedDatabaseTypeException.ThrowIfInvalidDatabaseType(databaseType), "Unexpected exception");
    }

    /// <summary>
    /// Test ThrowIfInvalidDatabaseType uses the argument passed to it as the parameter name.
    /// </summary>
    [TestMethod]
    public void ThrowIfInvalidDatabaseType_UnsupportedType_UsesArgumentAsParamName()
    {
        // ARRANGE
        string myDatabaseType = "UnsupportedDatabaseType";

        // ACT
        UnsupportedDatabaseTypeException e = Assert.ThrowsExactly<UnsupportedDatabaseTypeException>(() => UnsupportedDatabaseTypeException.ThrowIfInvalidDatabaseType(myDatabaseType), "Unexpected exception");

        // ASSERT
        Assert.AreEqual(nameof(myDatabaseType), e.ParamName, "Invalid parameter name");
    }

    #endregion ThrowIfInvalidDatabaseType tests
}