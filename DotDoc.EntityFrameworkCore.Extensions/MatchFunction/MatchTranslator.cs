// Copyright ©2021-2025 Mike King.
// This file is licensed to you under the MIT license.
// See the License.txt file in the solution root for more information.

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;
using Microsoft.EntityFrameworkCore.Storage;
using System.Data;
using System.Reflection;

namespace DotDoc.EntityFrameworkCore.Extensions.MatchFunction;

/// <summary>
/// Match Function Translator.
/// </summary>
public sealed class MatchTranslator : IMethodCallTranslator
{
    #region public methods

    /// <inheritdoc/>
    public SqlExpression? Translate(SqlExpression? instance, MethodInfo method, IReadOnlyList<SqlExpression> arguments, IDiagnosticsLogger<DbLoggerCategory.Query> logger)
    {
        SqlExpression? expression = null;

        if (method == MatchDbFunctionsExtensions.MatchMethod)
        {
            MatchArguments args = new(arguments);

            SqlExpression freeTextExpression = ApplyStringTypeMapping(args.FreeText);

            expression = new SqlFunctionExpression(
                functionName: nameof(MatchDbFunctionsExtensions.Match),
                arguments: [freeTextExpression, args.PropertyReference],
                nullable: true,
                [false, false],
                type: typeof(bool),
                typeMapping: null);
        }

        return expression;
    }

    #endregion public methods

    #region private methods

    /// <summary>
    /// Apply a string type mapping to an expression.
    /// </summary>
    /// <param name="expression">The expression.</param>
    /// <returns>The modified expression (or the original if the expression does not support type mapping).</returns>
    private static SqlExpression ApplyStringTypeMapping(SqlExpression expression)
    {
        MethodInfo? applyTypeMappingMethod = expression.GetType()
            .GetMethod("ApplyTypeMapping", [typeof(RelationalTypeMapping)]);

        SqlExpression newExpression = applyTypeMappingMethod != null
            ? (SqlExpression)applyTypeMappingMethod.Invoke(expression, [new StringTypeMapping("TEXT", DbType.String, true)])!
            : expression;

        return newExpression;
    }

    #endregion private methods
}
