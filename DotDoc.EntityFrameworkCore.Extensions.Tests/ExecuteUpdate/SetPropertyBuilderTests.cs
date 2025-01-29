// Copyright ©2021-2025 Mike King.
// This file is licensed to you under the MIT license.
// See the License.txt file in the solution root for more information.

using DotDoc.EntityFrameworkCore.Extensions.Classes;
using DotDoc.EntityFrameworkCore.Extensions.Tests.Data;
using DotDoc.EntityFrameworkCore.Extensions.Tests.Extensions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq.Expressions;

namespace DotDoc.EntityFrameworkCore.Extensions.Tests.ExecuteUpdate;

/// <summary>
/// Test SetPropertyBuilder class.
/// </summary>
[TestClass]
public class SetPropertyBuilderTests
{
    #region public methods

    /// <summary>
    /// Test SetProperty(propertyExpression, valueExpression) Guard Clauses.
    /// </summary>
    /// <param name="propertyExpression">Property expression.</param>
    /// <param name="valueExpression">Value expression.</param>
    /// <param name="exceptionType">The type of exception raised.</param>
    /// <param name="paramName">Name of parameter being checked.</param>
    [TestMethod]
    [DynamicData(nameof(Get_SetProperty_ValueExpression_GuardClause_TestData), DynamicDataSourceType.Method)]
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
    /// Test SetProperty(propertyExpression, value) Guard Clause.
    /// </summary>
    [TestMethod]
    public void Test_SetProperty_Value_GuardClauses()
    {
        // ARRANGE
        SetPropertyBuilder<TestTable1> builder = new();
        Expression<Func<TestTable1, string?>>? propertyExpression = null;
        string? value = "value";
        string paramName = "propertyExpression";

        // ACT / ASSERT
        ArgumentNullException e = Assert.ThrowsException<ArgumentNullException>(() => builder.SetProperty<string?>(propertyExpression!, value), "Unexpected exception");
        Assert.AreEqual(paramName, e.ParamName, "Invalid parameter name");
    }

    /// <summary>
    /// Test SetProperty(propertyExpression, valueExpression).
    /// </summary>
    /// <param name="value">The value to test.</param>
    [TestMethod]
    [DataRow(null, DisplayName = "SetProperty ValueExpression null.")]
    [DataRow("", DisplayName = "SetProperty ValueExpression empty string.")]
    [DataRow("value", DisplayName = "SetProperty ValueExpression test value.")]
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
    /// Test SetProperty(propertyExpression, value).
    /// </summary>
    /// <param name="value">The value to test.</param>
    [TestMethod]
    [DataRow(null, DisplayName = "SetProperty Value null.")]
    [DataRow("", DisplayName = "SetProperty Value empty string.")]
    [DataRow("value", DisplayName = "SetProperty Value test value.")]
    public void Test_SetProperty_Value(string? value)
    {
        // ARRANGE
        SetPropertyBuilder<TestTable1> builder = new();
        Expression<Func<TestTable1, string?>> propertyExpression = e => e.TestField;

        // ACT / ASSERT
        builder.SetProperty(propertyExpression, value);
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