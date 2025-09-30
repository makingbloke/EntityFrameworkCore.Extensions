// Copyright ©2021-2025 Mike King.
// This file is licensed to you under the MIT license.
// See the License.txt file in the solution root for more information.

using DotDoc.EntityFrameworkCore.Extensions.DatabaseType;
using Microsoft.EntityFrameworkCore.Infrastructure;
using System;
using System.Collections.Generic;
using System.Text;

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
    [DataRow("UnsupportedDatabaseType", DisplayName = "Empty")]
    public void UnsupportedDatabaseTypeExceptionTests_002(string? databaseType)
    {
        // ARRANGE

        // ACT / ASSERT
        Assert.ThrowsExactly<UnsupportedDatabaseTypeException>(() => UnsupportedDatabaseTypeException.ThrowIfInvalidDatabaseType(databaseType), "Unexpected exception");
    }

    #endregion public methods
}