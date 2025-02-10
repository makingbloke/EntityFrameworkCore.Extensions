// Copyright ©2021-2025 Mike King.
// This file is licensed to you under the MIT license.
// See the License.txt file in the solution root for more information.

using DotDoc.EntityFrameworkCore.Extensions.Classes;
using DotDoc.EntityFrameworkCore.Extensions.Tests.Data;
using DotDoc.EntityFrameworkCore.Extensions.Tests.Extensions;
using DotDoc.EntityFrameworkCore.Extensions.Tests.Utilities;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq.Expressions;

namespace DotDoc.EntityFrameworkCore.Extensions.Tests.ExecuteUpdateExtensions;

/// <summary>
/// Test SetPropertyBuilder class.
/// </summary>
[TestClass]
public class SetPropertyBuilderTests
{
    #region public methods

    /// <summary>
    /// Test SetProperty with PropertyExpression and ValueExpression parameters Guard Clauses.
    /// </summary>
    /// <param name="propertyExpression">Property expression.</param>
    /// <param name="valueExpression">Value expression.</param>
    /// <param name="exceptionType">The type of exception raised.</param>
    /// <param name="paramName">Name of parameter being checked.</param>
    [TestMethod("SetProperty with PropertyExpression and ValueExpression parameters Guard Clauses")]
    [DynamicData(nameof(Get_SetProperty_ValueExpression_GuardClause_TestData), DynamicDataSourceType.Method, DynamicDataDisplayName = nameof(TestUtils.CreateDynamicDisplayName), DynamicDataDisplayNameDeclaringType = typeof(TestUtils))]
    public void Test_SetProperty_ValueExpression_GuardClauses(Expression<Func<TestTable1, string?>>? propertyExpression, Expression<Func<TestTable1, string?>>? valueExpression, Type exceptionType, string paramName)
    {
        // ARRANGE
        SetPropertyBuilder<TestTable1> builder = new();

        // ACT / ASSERT
        Exception e = Assert.That.ThrowsAnyException(() => builder.SetProperty(propertyExpression!, valueExpression!), "Unexpected exception");
        Assert.AreEqual(exceptionType, e.GetType(), "Invalid exception type");
        Assert.AreEqual(paramName, ((ArgumentException)e).ParamName, "Invalid parameter name");
    }

    /// <summary>
    /// Test SetProperty with PropertyExpression and Value parameters Guard Clause.
    /// </summary>
    [TestMethod("SetProperty with PropertyExpression and Value parameters Guard Clause")]
    public void Test_SetProperty_Value_GuardClauses()
    {
        // ARRANGE
        SetPropertyBuilder<TestTable1> builder = new();
        Expression<Func<TestTable1, string?>>? propertyExpression = null;
        string? value = "value";
        string paramName = "propertyExpression";

        // ACT / ASSERT
        ArgumentNullException e = Assert.ThrowsException<ArgumentNullException>(() => builder.SetProperty(propertyExpression!, value), "Unexpected exception");
        Assert.AreEqual(paramName, e.ParamName, "Invalid parameter name");
    }

    /// <summary>
    /// Test SetProperty with PropertyExpression and ValueExpression parameters.
    /// </summary>
    /// <param name="value">The value to test.</param>
    [TestMethod("SetProperty with PropertyExpression and ValueExpression parameters")]
    [DataRow(null, DisplayName = "ValueEpression null.")]
    [DataRow("", DisplayName = "ValueExpression empty string.")]
    [DataRow("value", DisplayName = "ValueExpression value.")]
    public void Test_SetProperty_ValueExpression(string? value)
    {
        // ARRANGE
        SetPropertyBuilder<TestTable1> builder = new();
        Expression<Func<TestTable1, string?>> propertyExpression = e => e.TestField;
        Expression<Func<TestTable1, string?>> valueExpression = e => value;

        // ACT / ASSERT
        builder.SetProperty(propertyExpression, valueExpression);
    }

    /// <summary>
    /// Test SetProperty with PropertyExpression and Value parameters.
    /// </summary>
    /// <param name="value">The value to test.</param>
    [TestMethod("SetProperty with PropertyExpression and Value parameters")]
    [DataRow(null, DisplayName = "Value null.")]
    [DataRow("", DisplayName = "Value empty string.")]
    [DataRow("value", DisplayName = "Value test value.")]
    public void Test_SetProperty_Value(string? value)
    {
        // ARRANGE
        SetPropertyBuilder<TestTable1> builder = new();
        Expression<Func<TestTable1, string?>> propertyExpression = e => e.TestField;

        // ACT / ASSERT
        builder.SetProperty(propertyExpression, value);
    }

    /// <summary>
    /// Test GenerateLambda no properties.
    /// </summary>
    [TestMethod("GenerateLambda no properties")]
    public void Test_GenerateLambda_NoProperties()
    {
        // ARRANGE
        SetPropertyBuilder<TestTable1> builder = new();

        // ACT / ASSERT
        Assert.ThrowsException<InvalidOperationException>(builder.GenerateLambda, "Unexpected exception");
    }

    #endregion public methods

    #region private methods

    /// <summary>
    /// Get test data for SetProperty(propertyExpression, valueExpression).
    /// </summary>
    /// <returns><see cref="IEnumerable{T}"/>.</returns>
    private static IEnumerable<object?[]> Get_SetProperty_ValueExpression_GuardClause_TestData()
    {
        Expression<Func<TestTable1, string?>> propertyExpression = e => e.TestField;
        Expression<Func<TestTable1, string?>> valueExpression = e => "value";

        // 0. Expression<Func<TestTable1, string?>> propertyExpression
        // 1. Expression<Func<TestTable1, string?>> valueExpression
        // 2. Type exceptionType
        // 3. string paramName
        yield return [
            null,
            valueExpression,
            typeof(ArgumentNullException),
            "propertyExpression"];

        yield return [
            propertyExpression,
            null,
            typeof(ArgumentNullException),
            "valueExpression"];
    }

    #endregion private methods
}