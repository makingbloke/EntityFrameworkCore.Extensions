// Copyright ©2021-2025 Mike King.
// This file is licensed to you under the MIT license.
// See the License.txt file in the solution root for more information.

using DotDoc.EntityFrameworkCore.Extensions.MatchFunction;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Query;
using System.Linq.Expressions;
using System.Reflection;

namespace DotDoc.EntityFrameworkCore.Extensions.FreeTextSearchFunction;

/// <summary>
/// FreeTextSearch Function SQLite Expression Visitor.
/// </summary>
internal static class FreeTextSqliteExpressionVisitor
{
    #region public methods

    /// <summary>
    /// Visit the expression.
    /// </summary>
    /// <param name="context">Database context.</param>
    /// <param name="queryExpression">Query expression.</param>
    /// <returns>The modified expression.</returns>
    public static Expression VisitExpression(DbContext context, Expression queryExpression)
    {
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(queryExpression);

        MethodCallExpressionVisitor methodCallVisitor = new();
        Expression expression = methodCallVisitor.Visit(queryExpression);

        if (methodCallVisitor.UseStemming)
        {
            EntityReplacementExpressionVisitor entityReplacementVisitor = new(context);
            expression = entityReplacementVisitor.Visit(expression);
        }

        return expression;
    }

    #endregion public methods

    /// <summary>
    /// Replace the FreeTextSearch Function calls with SQLites MATCH function.
    /// </summary>
    private sealed class MethodCallExpressionVisitor : ExpressionVisitor
    {
        #region private fields

        /// <summary>
        /// SQLite Match function <see cref="MethodInfo"/>.
        /// </summary>
        private static readonly MethodInfo MatchMethod =
            typeof(MatchDbFunctionsExtensions).GetMethod(nameof(MatchDbFunctionsExtensions.Match), [typeof(DbFunctions), typeof(string), typeof(object)])!;

        /// <summary>
        /// Boolean stemming flag, set the first time a value is encountered.
        /// </summary>
        private bool? _useStemming;

        #endregion private fields

        #region public properties

        /// <summary>
        /// Gets a value indicating whether stemming will be used.
        /// </summary>
        public bool UseStemming => this._useStemming ?? false;

        #endregion public properties

        #region protected methods

        /// <inheritdoc/>
        protected override Expression VisitMethodCall(MethodCallExpression? node)
        {
            Expression? expression = null;
            MethodInfo method = node!.Method;

            if (FreeTextDbFunctionsExtensions.FreeTextSearchMethods.Contains(method))
            {
                FreeTextArguments args = new(node.Arguments);
                this._useStemming ??= args.UseStemming;

                expression = Expression.Call(MatchMethod, args.DbFunctions, args.FreeText, args.PropertyReference);
            }

            return expression ?? base.VisitMethodCall(node!);
        }

        #endregion protected methods.
    }

    /// <summary>
    /// Replace standard free text table names with their stemming table names.
    /// </summary>
    /// <param name="context">Database context.</param>
    private sealed class EntityReplacementExpressionVisitor(DbContext context) : ExpressionVisitor
    {
        #region protected methods

        /// <inheritdoc/>
        protected override Expression VisitExtension(Expression node)
        {
            Expression? expression = null;

            if (node is EntityQueryRootExpression entityQueryRoot && entityQueryRoot.EntityType.HasSharedClrType)
            {
                string? stemmingTableName = entityQueryRoot.EntityType.GetStemmingTable();

                if (string.IsNullOrEmpty(stemmingTableName))
                {
                    throw new InvalidOperationException("Stemming table not specified.");
                }

                IEntityType? stemmingEntityType = context.Model
                    .FindEntityTypes(entityQueryRoot.EntityType.ClrType)
                    .FirstOrDefault(t => t.GetTableName() == stemmingTableName);

                if (stemmingEntityType == null)
                {
                    throw new InvalidOperationException($"Stemming table {stemmingTableName} not found.");
                }

                expression = new EntityQueryRootExpression(stemmingEntityType);
            }

            return expression ?? base.VisitExtension(node);
        }

        #endregion protected methods
    }
}