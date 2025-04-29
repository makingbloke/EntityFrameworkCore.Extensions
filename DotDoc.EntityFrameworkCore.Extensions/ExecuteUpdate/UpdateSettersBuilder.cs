// Copyright ©2021-2025 Mike King.
// This file is licensed to you under the MIT license.
// See the License.txt file in the solution root for more information.

using Microsoft.EntityFrameworkCore.Query;
using System.Linq.Expressions;
using System.Reflection;

namespace DotDoc.EntityFrameworkCore.Extensions.ExecuteUpdate;

/// <summary>
/// Execute Update Extensions Update Setters Expression Builder.
/// </summary>
/// <typeparam name="TEntity">Type of Entity.</typeparam>
public sealed class UpdateSettersBuilder<TEntity>
    where TEntity : class
{
    #region private fields

    /// <summary>
    /// MethodInfo for SetProperty method that takes Func&lt;TSource, TProperty&gt; as a second parameter.
    /// </summary>
    private readonly MethodInfo _setPropertyGenericMethod = typeof(SetPropertyCalls<TEntity>)
        .GetMethods()
        .Single(m =>
            m.Name == "SetProperty" &&
            m.GetParameters().Length == 2 &&
            m.GetParameters()[1].ParameterType.IsGenericType);

    /// <summary>
    /// MethodInfo for SetProperty method that takes TProperty as a second parameter.
    /// </summary>
    private readonly MethodInfo _setPropertyConstantMethod = typeof(SetPropertyCalls<TEntity>)
        .GetMethods()
        .Single(m =>
            m.Name == "SetProperty" &&
            m.GetParameters().Length == 2 &&
            !m.GetParameters()[1].ParameterType.IsGenericType);

    /// <summary>
    /// Lambda expression parameter.
    /// </summary>
    private readonly ParameterExpression _parameter = Expression.Parameter(typeof(SetPropertyCalls<TEntity>));

    /// <summary>
    /// Lambda expression body.
    /// </summary>
    private Expression? _body;

    #endregion private fields

    #region internal constructors

    /// <summary>
    /// Initializes a new instance of the <see cref="UpdateSettersBuilder{TSource}"/> class.
    /// </summary>
    internal UpdateSettersBuilder()
    {
    }

    #endregion internal constructors

    #region public methods

    /// <summary>
    /// Specifies a property and corresponding value it should be updated to in an ExecuteUpdate method.
    /// </summary>
    /// <typeparam name="TProperty">The type of the property being set.</typeparam>
    /// <param name="propertyExpression">Property expression.</param>
    /// <param name="valueExpression">Value expression.</param>
    /// <returns>The same instance so that multiple calls to SetProperty can be chained.</returns>
    public UpdateSettersBuilder<TEntity> SetProperty<TProperty>(Expression<Func<TEntity, TProperty>> propertyExpression, Expression<Func<TEntity, TProperty>> valueExpression)
    {
        ArgumentNullException.ThrowIfNull(propertyExpression);
        ArgumentNullException.ThrowIfNull(valueExpression);

        MethodInfo method = this._setPropertyGenericMethod.MakeGenericMethod(typeof(TProperty));

        this._body = Expression.Call(this._body ?? this._parameter, method, propertyExpression, valueExpression);
        return this;
    }

    /// <summary>
    /// Specifies a property and corresponding value it should be updated to in an ExecuteUpdate method.
    /// </summary>
    /// <typeparam name="TProperty">The type of the property being set.</typeparam>
    /// <param name="propertyExpression">Property expression.</param>
    /// <param name="value">Value.</param>
    /// <returns>The same instance so that multiple calls to SetProperty can be chained.</returns>
    public UpdateSettersBuilder<TEntity> SetProperty<TProperty>(Expression<Func<TEntity, TProperty>> propertyExpression, TProperty value)
    {
        ArgumentNullException.ThrowIfNull(propertyExpression);

        MethodInfo method = this._setPropertyConstantMethod.MakeGenericMethod(typeof(TProperty));
        Expression valueExpression = Expression.Constant(value, typeof(TProperty));

        this._body = Expression.Call(this._body ?? this._parameter, method, propertyExpression, valueExpression);
        return this;
    }

    #endregion public methods

    #region internal methods

    /// <summary>
    /// Builds an expression containing the update setters.
    /// </summary>
    /// <returns>A lambda expression.</returns>
    internal Expression<Func<SetPropertyCalls<TEntity>, SetPropertyCalls<TEntity>>> CreateUpdateSettersExpression()
    {
        if (this._body == null)
        {
            throw new InvalidOperationException("No properties have been set");
        }

        return Expression.Lambda<Func<SetPropertyCalls<TEntity>, SetPropertyCalls<TEntity>>>(this._body, this._parameter);
    }

    #endregion internal methods
}
