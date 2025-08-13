// Copyright ©2021-2025 Mike King.
// This file is licensed to you under the MIT license.
// See the License.txt file in the solution root for more information.

using DotDoc.EntityFrameworkCore.Extensions.UniqueConstraint;

namespace DotDoc.EntityFrameworkCore.Extensions.Tests.UniqueConstraint;

/// <summary>
/// Tests for UniqueConstraintException extensions.
/// </summary>
[TestClass]
public class UniqueConstraintExceptionTests
{
    #region public methods

    /// <summary>
    /// Test UniqueConstraintException constructor Guard Clause.
    /// </summary>
    [TestMethod("UniqueConstraintException constructor Guard Clause")]
    public void Test_UniqueConstraintException_GuardClause()
    {
        // ARRANGE
        InvalidOperationException innerException = new();
        UniqueConstraintDetails? details = null;

        // ACT / ASSERT
        ArgumentNullException e = Assert.ThrowsExactly<ArgumentNullException>(() => _ = new UniqueConstraintException(innerException, details!), "Missing exception");
        Assert.AreEqual(nameof(details), e.ParamName, "Invalid parameter name");
    }

    /// <summary>
    /// Test UniqueConstraintException(innerException, details) constructor.
    /// </summary>
    [TestMethod("UniqueConstraintException constructor")]
    public void Test_UniqueConstraintException()
    {
        // ARRANGE
        InvalidOperationException innerException = new();
        UniqueConstraintDetails? details = new("schema", "tableName", []);
        UniqueConstraintException e = new(innerException, details);

        // ACT / ASSERT
        Assert.AreEqual(details, e.Details, "Invalid unique constraint details");
    }

    #endregion public methods
}