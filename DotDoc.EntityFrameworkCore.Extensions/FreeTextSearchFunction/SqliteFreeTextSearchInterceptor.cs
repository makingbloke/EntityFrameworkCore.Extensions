// Copyright ©2021-2025 Mike King.
// This file is licensed to you under the MIT license.
// See the License.txt file in the solution root for more information.

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Query;
using System.Linq.Expressions;
using System.Reflection;

namespace DotDoc.EntityFrameworkCore.Extensions.FreeTextSearchFunction;

/// <summary>
/// FreeTextSearch Function Query Expression Interceptor.
/// </summary>
internal sealed class SqliteFreeTextSearchInterceptor : IQueryExpressionInterceptor
{
    #region internal fields

    /// <summary>
    /// Singleton instance of this interceptor.
    /// </summary>
    internal static readonly SqliteFreeTextSearchInterceptor Instance = new();

    #endregion internal fields

    #region private constructors

    /// <summary>
    /// Initializes a new instance of the <see cref="SqliteFreeTextSearchInterceptor"/> class.
    /// </summary>
    private SqliteFreeTextSearchInterceptor()
    {
    }

    #endregion private constructors

    #region public method

    /// <inheritdoc/>
    public Expression QueryCompilationStarting(Expression queryExpression, QueryExpressionEventData eventData)
    {
        Expression expression = UseStemmingVisitor.GetValue(queryExpression)
            ? ReplaceFreeTextTableVisitor.Replace(eventData.Context!, queryExpression)
            : queryExpression;

        return expression;
    }

    #endregion public method

    #region private UseStemmingVisitor class.

    /// <summary>
    /// Find the FreeTextSearch function (if there is one) and get the UseStemming parameter.
    /// </summary>
    private sealed class UseStemmingVisitor : ExpressionVisitor
    {
        #region private fields

        /// <summary>
        /// Stemming flag, set the first time a value is encountered.
        /// </summary>
        private bool? _useStemming;

        #endregion private fields

        #region private constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="UseStemmingVisitor"/> class.
        /// </summary>
        private UseStemmingVisitor()
        {
        }

        #endregion private constructors

        #region public methods

        /// <summary>
        /// Get the first UseStemming value from the expression tree.
        /// </summary>
        /// <param name="expression">The expression to visit.</param>
        /// <returns>The UseStemming value.</returns>
        public static bool GetValue(Expression expression)
        {
            UseStemmingVisitor visitor = new();
            visitor.Visit(expression);
            return visitor._useStemming ?? false;
        }

        #endregion public methods

        #region protected methods

        /// <inheritdoc/>
        protected override Expression VisitMethodCall(MethodCallExpression? node)
        {
            ArgumentNullException.ThrowIfNull(node);

            MethodInfo? method = FreeTextSearchDbFunctionsExtensions.FreeTextSearchMethods.FirstOrDefault(m => m == node.Method);

            if (method is not null && this._useStemming is null)
            {
                ConstantExpression? constantExpression = (ConstantExpression?)method
                    .GetParameters()
                    .Zip(node.Arguments)
                    .Where(t => t.First.Name == "useStemming")
                    .Select(t => t.Second)
                    .FirstOrDefault();

                if (constantExpression is not null)
                {
                    this._useStemming = (bool)constantExpression.Value!;
                }

                return node;
            }

            return base.VisitMethodCall(node!);
        }

        #endregion protected methods.
    }

    #endregion private UseStemmingVisitor class.

    #region private ReplaceFreeTextTableVisitor class.

    /// <summary>
    /// Replace the free text table in the query with its stemming equivalent.
    /// </summary>
    private sealed class ReplaceFreeTextTableVisitor : ExpressionVisitor
    {
        #region private fields

        /// <summary>
        /// Database context.
        /// </summary>
        private readonly DbContext _context;

        #endregion private fields

        #region private constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="ReplaceFreeTextTableVisitor"/> class.
        /// </summary>
        /// <param name="context">Database context.</param>
        private ReplaceFreeTextTableVisitor(DbContext context)
        {
            this._context = context;
        }

        #endregion private constructors

        #region public methods

        /// <summary>
        /// Replace the free text table in the query with its stemming equivalent.
        /// </summary>
        /// <param name="context">Database context.</param>
        /// <param name="expression">The expression to visit.</param>
        /// <returns>The modified expression.</returns>
        public static Expression Replace(DbContext context, Expression expression)
        {
            ReplaceFreeTextTableVisitor visitor = new(context);
            return visitor.Visit(expression);
        }

        #endregion public methods

        #region protected methods

        /// <inheritdoc/>
        protected override Expression VisitExtension(Expression node)
        {
            if (node is EntityQueryRootExpression entityQueryRoot && entityQueryRoot.EntityType.HasSharedClrType)
            {
                string? stemmingTableName = entityQueryRoot.EntityType.GetStemmingTable();

                if (string.IsNullOrEmpty(stemmingTableName))
                {
                    throw new InvalidOperationException("Stemming table not specified.");
                }

                IEntityType? stemmingEntityType = this._context.Model
                    .FindEntityTypes(entityQueryRoot.EntityType.ClrType)
                    .FirstOrDefault(t => t.GetTableName() == stemmingTableName);

                if (stemmingEntityType is null)
                {
                    throw new InvalidOperationException($"Stemming table {stemmingTableName} not found.");
                }

                return new EntityQueryRootExpression(stemmingEntityType);
            }

            return base.VisitExtension(node);
        }

        #endregion protected methods
    }

    #endregion private ReplaceFreeTextTableVisitor class.
}
