// Copyright ©2021-2026 Mike King.
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
    /// Test UniqueConstraintException with a Null UniqueConstraintDetails Details parameter.
    /// </summary>
    [TestMethod(DisplayName = "UniqueConstraintException with a Null UniqueConstraintDetails Details parameter")]
    public void UniqueConstraintExceptionTests_001()
    {
        // ARRANGE
        InvalidOperationException innerException = new();
        UniqueConstraintDetails details = null!;

        // ACT / ASSERT
        Assert.ThrowsExactly<ArgumentNullException>(() => new UniqueConstraintException(innerException, details), "Unexpected exception");
    }

    /// <summary>
    /// Test UniqueConstraintException(Exception InnerException, UniqueConstraintDetails Details) constructor.
    /// </summary>
    [TestMethod(DisplayName = "UniqueConstraintException(Exception InnerException, UniqueConstraintDetails Details) constructor")]
    public void UniqueConstraintExceptionTests_002()
    {
        // ARRANGE
        InvalidOperationException innerException = new();
        UniqueConstraintDetails details = new("schema", "tableName", []);
        UniqueConstraintException e = new(innerException, details);

        // ACT / ASSERT
        Assert.AreEqual(details, e.Details, "Invalid unique constraint details");
    }

    /// <summary>
    /// Test UniqueConstraintException() constructor.
    /// </summary>
    [TestMethod(DisplayName = "UniqueConstraintException() constructor")]
    public void UniqueConstraintExceptionTests_003()
    {
        // ARRANGE / ACT
        UniqueConstraintException e = new();

        // ASSERT
        Assert.IsNull(e.Details, "Invalid unique constraint details");
        Assert.IsNull(e.InnerException, "Invalid inner exception");
    }

    /// <summary>
    /// Test UniqueConstraintException(string Message) constructor.
    /// </summary>
    [TestMethod(DisplayName = "UniqueConstraintException(string Message) constructor")]
    public void UniqueConstraintExceptionTests_004()
    {
        // ARRANGE
        string message = "Test message";

        // ACT
        UniqueConstraintException e = new(message);

        // ASSERT
        Assert.AreEqual(message, e.Message, "Invalid message");
        Assert.IsNull(e.Details, "Invalid unique constraint details");
        Assert.IsNull(e.InnerException, "Invalid inner exception");
    }

    /// <summary>
    /// Test UniqueConstraintException(string Message, Exception InnerException) constructor.
    /// </summary>
    [TestMethod(DisplayName = "UniqueConstraintException(string Message, Exception InnerException) constructor")]
    public void UniqueConstraintExceptionTests_005()
    {
        // ARRANGE
        string message = "Test message";
        InvalidOperationException innerException = new();

        // ACT
        UniqueConstraintException e = new(message, innerException);

        // ASSERT
        Assert.AreEqual(message, e.Message, "Invalid message");
        Assert.AreSame(innerException, e.InnerException, "Invalid inner exception");
        Assert.IsNull(e.Details, "Invalid unique constraint details");
    }

    #endregion public methods
}