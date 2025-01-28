// Copyright ©2021-2025 Mike King.
// This file is licensed to you under the MIT license.
// See the License.txt file in the solution root for more information.

using DotDoc.EntityFrameworkCore.Extensions.Exceptions;
using DotDoc.EntityFrameworkCore.Extensions.Model;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DotDoc.EntityFrameworkCore.Extensions.Tests.UniqueConstraintExtensions;

/// <summary>
/// Tests for UniqueConstraintException extensions.
/// </summary>
[TestClass]
public class UniqueConstraintExceptionTests
{
    #region public methods

    /// <summary>
    /// Test UniqueConstraintException(innerException, details) constructor Guard Clause.
    /// </summary>
    [TestMethod]
    public void Test_UniqueConstraintException_GuardClause()
    {
        // ARRANGE
        InvalidOperationException innerException = new();
        UniqueConstraintDetails? details = null;
        string paramName = "details";

        // ACT / ASSERT
        ArgumentNullException e = Assert.ThrowsException<ArgumentNullException>(() => new UniqueConstraintException(innerException, details!), "Missing exception");
        Assert.AreEqual(paramName, e.ParamName, "Invalid parameter name");
    }

    /// <summary>
    /// Test UniqueConstraintException(innerException, details) constructor.
    /// </summary>
    [TestMethod]
    public void Test_UniqueConstraintException()
    {
        // ARRANGE
        InvalidOperationException innerException = new();
        UniqueConstraintDetails? details = new("schema", "tableName", new List<string>());
        UniqueConstraintException e = new UniqueConstraintException(innerException, details);

        // ACT / ASSERT
        Assert.AreEqual(details, e.Details, "Invalid unique constraint details");
    }

    #endregion public methods
}