// Copyright ©2021-2025 Mike King.
// This file is licensed to you under the MIT license.
// See the License.txt file in the solution root for more information.

using DotDoc.EntityFrameworkCore.Extensions.Classes;
using DotDoc.EntityFrameworkCore.Extensions.Tests.Data;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq.Expressions;

namespace DotDoc.EntityFrameworkCore.Extensions.Tests.ExecuteUpdate;

/// <summary>
/// Test SetPropertyBuilder class.
/// </summary>
[TestClass]
public class SetPropertyBuilderTests
{
    /// <summary>
    /// Test SetProperty(propertyExpression, value) allows a null value.
    /// </summary>
    [TestMethod]
    public void Test_SetProperty_Value_AllowNullValue()
    {
        // ARRANGE
        SetPropertyBuilder<TestTable1> builder = new();
        Expression<Func<TestTable1, string?>> propertyExpression = e => e.TestField;
        string? value = null;

        // ACT / ASSERT
        builder.SetProperty(propertyExpression!, value);
    }

    /// <summary>
    /// Test GenerateLambda with no properties.
    /// </summary>
    [TestMethod]
    public void Test_GenerateLambda_NoProperties()
    {
        // ARRANGE
        SetPropertyBuilder<TestTable1> builder = new();

        // ACT / ASSERT
        Assert.ThrowsException<InvalidOperationException>(builder.GenerateLambda, "Unexpected exception");
    }
}
