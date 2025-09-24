// Copyright ©2021-2025 Mike King.
// This file is licensed to you under the MIT license.
// See the License.txt file in the solution root for more information.

using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;

namespace DotDoc.EntityFrameworkCore.Extensions.Utilities;

/// <summary>
/// Translator Provider Base.
/// </summary>
internal abstract class TranslatorProviderBase : RelationalMethodCallTranslatorProvider
{
    #region protected constructors

    /// <inheritdoc/>
    protected TranslatorProviderBase(RelationalMethodCallTranslatorProviderDependencies dependencies)
        : base(dependencies)
    {
    }

    #endregion protected constructors

    #region protected methods

    /// <summary>
    /// Apply the default type mapping to a SQL expression if it does not already have one.
    /// </summary>
    /// <param name="sqlExpression">The SQL expression.</param>
    /// <returns>The modified expression.</returns>
    protected SqlExpression ApplyTypeMapping(SqlExpression sqlExpression)
    {
        if (sqlExpression.TypeMapping is null)
        {
            sqlExpression = this.Dependencies.SqlExpressionFactory.ApplyDefaultTypeMapping(sqlExpression);
        }

        return sqlExpression;
    }

    #endregion protected methods
}
