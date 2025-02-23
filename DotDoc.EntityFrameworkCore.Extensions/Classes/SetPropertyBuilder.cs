﻿// Copyright ©2021-2025 Mike King.
// This file is licensed to you under the MIT license.
// See the License.txt file in the solution root for more information.

using Microsoft.EntityFrameworkCore.Query;
using System.Linq.Expressions;
using System.Reflection;

namespace DotDoc.EntityFrameworkCore.Extensions.Classes;

/// <summary>
/// Execute Update Extensions Set Property Builder.
/// </summary>
/// <remarks>
/// An instance of this class is passed as the builder method when an ExecuteUpdatexxxxx extension method is called
/// and is used to generate a lambda method containing the required SetProperty calls.
/// </remarks>
/// <typeparam name="TEntity">Type of Entity.</typeparam>
public sealed class SetPropertyBuilder<TEntity>
    where TEntity : class
{
    #region private fields

    /// <summary>
    /// MethodInfo for SetProperty method that takes Func&lt;TSource, TProperty&gt; as a second parameter.
    /// </summary>
    private readonly MethodInfo _setPropertyMethodGeneric = FindSetPropertyMethod(true);

    /// <summary>
    /// MethodInfo for SetProperty method that takes TProperty as a second parameter.
    /// </summary>
    private readonly MethodInfo _setPropertyMethodConstant = FindSetPropertyMethod(false);

    /// <summary>
    /// Lambda expression parameter.
    /// </summary>
    private readonly ParameterExpression _parameter;

    /// <summary>
    /// Lambda expression body.
    /// </summary>
    private Expression _body;

    #endregion private fields

    #region public constructors

    /// <summary>
    /// Initializes a new instance of the <see cref="SetPropertyBuilder{TSource}"/> class.
    /// </summary>
    internal SetPropertyBuilder()
    {
        this._body = this._parameter = Expression.Parameter(typeof(SetPropertyCalls<TEntity>));
    }

    #endregion public constructors

    #region public methods

    /// <summary>
    /// Specifies a property and corresponding value it should be updated to in an ExecuteUpdate method.
    /// </summary>
    /// <typeparam name="TProperty">The type of the property being set.</typeparam>
    /// <param name="propertyExpression">Property expression.</param>
    /// <param name="valueExpression">Value expression.</param>
    /// <returns>The same instance so that multiple calls to SetProperty can be chained.</returns>
    public SetPropertyBuilder<TEntity> SetProperty<TProperty>(Expression<Func<TEntity, TProperty>> propertyExpression, Expression<Func<TEntity, TProperty>> valueExpression)
    {
        ArgumentNullException.ThrowIfNull(propertyExpression);
        ArgumentNullException.ThrowIfNull(valueExpression);

        MethodInfo method = this._setPropertyMethodGeneric.MakeGenericMethod(typeof(TProperty));

        this._body = Expression.Call(this._body, method, propertyExpression, valueExpression);
        return this;
    }

    /// <summary>
    /// Specifies a property and corresponding value it should be updated to in an ExecuteUpdate method.
    /// </summary>
    /// <typeparam name="TProperty">The type of the property being set.</typeparam>
    /// <param name="propertyExpression">Property expression.</param>
    /// <param name="value">Value.</param>
    /// <returns>The same instance so that multiple calls to SetProperty can be chained.</returns>
    public SetPropertyBuilder<TEntity> SetProperty<TProperty>(Expression<Func<TEntity, TProperty>> propertyExpression, TProperty value)
    {
        ArgumentNullException.ThrowIfNull(propertyExpression);

        MethodInfo method = this._setPropertyMethodConstant.MakeGenericMethod(typeof(TProperty));
        Expression valueExpression = Expression.Constant(value, typeof(TProperty));

        this._body = Expression.Call(this._body, method, propertyExpression, valueExpression);
        return this;
    }

    #endregion public methods

    #region internal methods

    /// <summary>
    /// Creates a lambda expression containing the SetProperty calls.
    /// </summary>
    /// <returns>A lambda expression.</returns>
    internal Expression<Func<SetPropertyCalls<TEntity>, SetPropertyCalls<TEntity>>> GenerateLambda()
    {
        if (object.ReferenceEquals(this._body, this._parameter))
        {
            throw new InvalidOperationException("No properties have been set");
        }

        return Expression.Lambda<Func<SetPropertyCalls<TEntity>, SetPropertyCalls<TEntity>>>(this._body, this._parameter);
    }

    #endregion internal methods

    #region private methods

    /// <summary>
    /// Find the SetProperty method in Microsoft.EntityFrameworkCore.Query.
    /// </summary>
    /// <param name="isGenericType">If <see langword="true"/> find the method that takes a generic Func as the second parameter.</param>
    /// <returns>The <see cref="MethodInfo"/> of the method.</returns>
    private static MethodInfo FindSetPropertyMethod(bool isGenericType)
    {
        return typeof(SetPropertyCalls<TEntity>).GetMethods()
            .Single(method => method.Name == nameof(SetPropertyCalls<TEntity>.SetProperty) && method.GetParameters()[^1].ParameterType.IsGenericType == isGenericType);
    }

    #endregion private methods
}
