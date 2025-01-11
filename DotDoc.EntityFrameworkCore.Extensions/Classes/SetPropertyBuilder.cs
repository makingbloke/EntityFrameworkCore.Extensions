// Copyright ©2021-2025 Mike King.
// This file is licensed to you under the MIT license.
// See the License.txt file in the solution root for more information.

using Microsoft.EntityFrameworkCore.Query;
using System.Linq.Expressions;
using System.Reflection;

namespace DotDoc.EntityFrameworkCore.Extensions.Classes;

/// <summary>
/// Execute update extensions set property builder.
/// </summary>
/// <remarks>
/// An instance of this class is passed to the builder function when the CreateUpdate extension method is called
/// and is used to generate a lambda method with the desired SetProperty calls.
/// </remarks>
/// <typeparam name="TSource">Type of source.</typeparam>
public class SetPropertyBuilder<TSource>
    where TSource : class
{
    /// <summary>
    /// Method Info for EF Core Set Property Method which has an expression as the second parameter.
    /// </summary>
    private readonly MethodInfo _setPropertyMethodWithExpressionParameter = GetSetPropertyMethod(false);

    /// <summary>
    /// Method Info for EF Core Set Property Method which has a generic value as the second parameter.
    /// </summary>
    private readonly MethodInfo _setPropertyMethodWithGenericParameter = GetSetPropertyMethod(true);

    /// <summary>
    /// Lambda expression parameter.
    /// </summary>
    private readonly ParameterExpression _parameter;

    /// <summary>
    /// Lambda expression body.
    /// </summary>
    private Expression _body;

    /// <summary>
    /// Initializes a new instance of the <see cref="SetPropertyBuilder{TSource}"/> class.
    /// </summary>
    public SetPropertyBuilder()
    {
        this._parameter = Expression.Parameter(typeof(SetPropertyCalls<TSource>));
        this._body = this._parameter;
    }

    /// <summary>
    /// Specifies a property and corresponding value it should be updated to in ExecuteUpdate method.
    /// </summary>
    /// <typeparam name="TProperty">The type of the property being set.</typeparam>
    /// <param name="propertyExpression">Property expression.</param>
    /// <param name="valueExpression">Value expression.</param>
    /// <returns>The same instance so that multiple calls to SetProperty can be chained.</returns>
    public SetPropertyBuilder<TSource> SetProperty<TProperty>(Expression<Func<TSource, TProperty>> propertyExpression, Expression<Func<TSource, TProperty>> valueExpression)
    {
        MethodInfo method = this._setPropertyMethodWithExpressionParameter.MakeGenericMethod(typeof(TProperty));

        this._body = Expression.Call(this._body, method, propertyExpression, valueExpression);
        return this;
    }

    /// <summary>
    /// Specifies a property and corresponding value it should be updated to in ExecuteUpdate method.
    /// </summary>
    /// <typeparam name="TProperty">The type of the property being set.</typeparam>
    /// <param name="propertyExpression">Property expression.</param>
    /// <param name="value">Value.</param>
    /// <returns>The same instance so that multiple calls to SetProperty can be chained.</returns>
    public SetPropertyBuilder<TSource> SetProperty<TProperty>(Expression<Func<TSource, TProperty>> propertyExpression, TProperty value)
    {
        MethodInfo method = this._setPropertyMethodWithGenericParameter.MakeGenericMethod(typeof(TProperty));
        Expression valueExpression = Expression.Constant(value, typeof(TProperty));

        this._body = Expression.Call(this._body, method, propertyExpression, valueExpression);
        return this;
    }

    /// <summary>
    /// Creates a lambda expression containing the SetProperty calls.
    /// </summary>
    /// <returns>A lambda expression.</returns>
    internal Expression<Func<SetPropertyCalls<TSource>, SetPropertyCalls<TSource>>> GenerateLambda()
    {
        return Expression.Lambda<Func<SetPropertyCalls<TSource>, SetPropertyCalls<TSource>>>(this._body, this._parameter);
    }

    /// <summary>
    /// Find the SetProperty method in Microsoft.EntityFrameworkCore.Query.
    /// </summary>
    /// <param name="isParameterGeneric">If <see langword="true"/> find the method with a generic second parameter.</param>
    /// <returns>The <see cref="MethodInfo"/> of the method.</returns>
    private static MethodInfo GetSetPropertyMethod(bool isParameterGeneric)
    {
        string methodName = nameof(SetPropertyCalls<TSource>.SetProperty);
        string parameterTypeName = isParameterGeneric ? "TProperty" : "Func`2";

        return typeof(SetPropertyCalls<TSource>).GetMethods()
            .Single(method =>
            {
                ParameterInfo[] property = method.GetParameters();
                return method.Name == methodName && property.Length == 2 && property[1].ParameterType.Name == parameterTypeName;
            });
    }
}
