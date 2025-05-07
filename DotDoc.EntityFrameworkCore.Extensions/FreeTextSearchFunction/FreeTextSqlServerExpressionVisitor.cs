// Copyright ©2021-2025 Mike King.
// This file is licensed to you under the MIT license.
// See the License.txt file in the solution root for more information.

using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using System.Reflection;

namespace DotDoc.EntityFrameworkCore.Extensions.FreeTextSearchFunction;

/// <summary>
/// FreeTextSearch Function SQL Server Expression Visitor.
/// </summary>
internal static class FreeTextSqlServerExpressionVisitor
{
    #region public methods

    /// <summary>
    /// Visit the expression.
    /// </summary>
    /// <param name="queryExpression">Query expression.</param>
    /// <returns>The modified expression.</returns>
    public static Expression VisitExpression(Expression queryExpression)
    {
        ArgumentNullException.ThrowIfNull(queryExpression);

        MethodCallExpressionVisitor visitor = new();
        Expression expression = visitor.Visit(queryExpression);

        return expression;
    }

    #endregion public methods

    /// <summary>
    /// Replace the FreeTextSearch Function calls with SQL Servers Contains and FreeText functions.
    /// </summary>
    private sealed class MethodCallExpressionVisitor : ExpressionVisitor
    {
        #region private fields

        /// <summary>
        /// SQL Server Contains and FreeText function <see cref="MethodInfo"/>.
        /// The order of this array is important as we use bit settings to determine which entry to use.
        /// </summary>
        private static readonly MethodInfo[] FreeTextMethods =
        [
            typeof(SqlServerDbFunctionsExtensions)
                .GetMethod(nameof(SqlServerDbFunctionsExtensions.Contains), [typeof(DbFunctions), typeof(object), typeof(string)])!,

            typeof(SqlServerDbFunctionsExtensions)
                .GetMethod(nameof(SqlServerDbFunctionsExtensions.FreeText), [typeof(DbFunctions), typeof(object), typeof(string)])!,

            typeof(SqlServerDbFunctionsExtensions)
                .GetMethod(nameof(SqlServerDbFunctionsExtensions.Contains), [typeof(DbFunctions), typeof(object), typeof(string), typeof(int)])!,

            typeof(SqlServerDbFunctionsExtensions)
                .GetMethod(nameof(SqlServerDbFunctionsExtensions.FreeText), [typeof(DbFunctions), typeof(object), typeof(string), typeof(int)])!
        ];

        #endregion private fields

        #region protected methods

        /// <inheritdoc/>
        protected override Expression VisitMethodCall(MethodCallExpression? node)
        {
            MethodInfo method = node!.Method;

            if (FreeTextDbFunctionsExtensions.FreeTextSearchMethods.Contains(method))
            {
                FreeTextArguments args = new(node.Arguments);

                int methodIndex = 0;
                List<Expression> arguments = [args.DbFunctions, args.PropertyReference, args.FreeText];

                if (args.UseStemming)
                {
                    methodIndex |= 1;
                }

                if (args.LanguageTerm != null)
                {
                    methodIndex |= 2;
                    arguments.Add(args.LanguageTerm);
                }

                return Expression.Call(null, FreeTextMethods[methodIndex], arguments);
            }

            return base.VisitMethodCall(node!);
        }

        #endregion protected methods
    }
}
