// Copyright ©2021-2026 Mike King.
// This file is licensed to you under the MIT license.
// See the License.txt file in the solution root for more information.

using Microsoft.EntityFrameworkCore.Diagnostics;
using System.Linq.Expressions;
using System.Reflection;

namespace DotDoc.EntityFrameworkCore.Extensions.CaseInsensitivity;

/// <summary>
/// SQLite NoCase Query Expression Interceptor.
/// </summary>
/// <remarks>
/// Wraps each string concatenation, <c>??</c> and conditional expression in the query, and each string returned by a subquery
/// (e.g. <c>FirstOrDefault</c> or <c>Max</c>), in a call to <see cref="KeepCollation"/>, so <see cref="SqliteNoCaseTranslator"/> can give
/// its result the NOCASE collation when it comes from NOCASE values, as SQL Server gives the result their collation. SQLite gives these
/// expressions no collation, so they would otherwise compare as BINARY.
/// </remarks>
internal sealed class SqliteNoCaseQueryExpressionInterceptor : IQueryExpressionInterceptor
{
    #region internal fields

    /// <summary>
    /// Singleton instance of this interceptor.
    /// </summary>
    internal static readonly SqliteNoCaseQueryExpressionInterceptor Instance = new();

    /// <summary>
    /// The <see cref="KeepCollation"/> method.
    /// </summary>
    internal static readonly MethodInfo KeepCollationMethod = typeof(SqliteNoCaseQueryExpressionInterceptor).GetMethod(nameof(KeepCollation))!;

    #endregion internal fields

    #region private constructors

    /// <summary>
    /// Initializes a new instance of the <see cref="SqliteNoCaseQueryExpressionInterceptor"/> class.
    /// </summary>
    private SqliteNoCaseQueryExpressionInterceptor()
    {
    }

    #endregion private constructors

    #region public methods

    /// <summary>
    /// Marks a string expression whose result should keep the NOCASE collation of its operands.
    /// </summary>
    /// <param name="value">The value.</param>
    /// <returns>
    /// The value: in SQL with the NOCASE collation if one of its operands is NOCASE, and unchanged when EF Core evaluates it on the client
    /// (a .NET string has no collation).
    /// </returns>
    public static string? KeepCollation(string? value)
    {
        // Unlike Match and FreeTextSearch, this doesn't throw when it's evaluated on the client. The interceptor adds it to the user's own
        // query, and EF Core evaluates it on the client only where it legitimately runs that part of the final projection in .NET
        // (e.g. a concatenation with a method EF Core can't translate), which worked before it was added.
        return value;
    }

    /// <inheritdoc/>
    public Expression QueryCompilationStarting(Expression queryExpression, QueryExpressionEventData eventData)
    {
        // The query's own last operator (e.g. query.Max(e => e.Name) or query.FirstOrDefault()) returns the query's result rather than a
        // subquery's, so only its arguments are visited: wrapped, EF Core couldn't translate the query.
        return queryExpression is MethodCallExpression root && KeepCollationVisitor.IsStringSubqueryResult(root)
            ? root.Update(KeepCollationVisitor.Default.Visit(root.Object), KeepCollationVisitor.Default.Visit(root.Arguments))
            : KeepCollationVisitor.Default.Visit(queryExpression);
    }

    #endregion public methods

    #region private KeepCollationVisitor class

    /// <summary>
    /// Wraps each string concatenation, <c>??</c> and conditional expression, and each string returned by a subquery, in a call to
    /// <see cref="KeepCollation"/>.
    /// </summary>
    private sealed class KeepCollationVisitor : ExpressionVisitor
    {
        #region public fields

        /// <summary>
        /// Singleton instance of this visitor (it has no state).
        /// </summary>
        public static readonly KeepCollationVisitor Default = new();

        #endregion public fields

        #region private constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="KeepCollationVisitor"/> class.
        /// </summary>
        private KeepCollationVisitor()
        {
        }

        #endregion private constructors

        #region public methods

        /// <summary>
        /// Determines whether a method call returns one string from a sequence: <c>First</c>, <c>FirstOrDefault</c>, <c>Single</c>,
        /// <c>SingleOrDefault</c>, <c>Last</c>, <c>LastOrDefault</c>, <c>ElementAt</c>, <c>ElementAtOrDefault</c>, <c>Min</c> or <c>Max</c>
        /// (of <see cref="Queryable"/> or <see cref="Enumerable"/>). Inside a query, EF Core translates it into a scalar subquery.
        /// </summary>
        /// <param name="node">The method call.</param>
        /// <returns><see langword="true"/> if it does, otherwise <see langword="false"/>.</returns>
        public static bool IsStringSubqueryResult(MethodCallExpression node)
        {
            return node.Type == typeof(string)
                && (node.Method.DeclaringType == typeof(Queryable) || node.Method.DeclaringType == typeof(Enumerable))
                && node.Method.Name is nameof(Queryable.First) or nameof(Queryable.FirstOrDefault) or nameof(Queryable.Single)
                    or nameof(Queryable.SingleOrDefault) or nameof(Queryable.Last) or nameof(Queryable.LastOrDefault) or nameof(Queryable.ElementAt)
                    or nameof(Queryable.ElementAtOrDefault) or nameof(Queryable.Min) or nameof(Queryable.Max);
        }

        #endregion public methods

        #region protected methods

        /// <inheritdoc/>
        protected override Expression VisitBinary(BinaryExpression node)
        {
            return IsStringCombination(node)
                ? Wrap(this.VisitCombination(node))
                : base.VisitBinary(node);
        }

        /// <inheritdoc/>
        protected override Expression VisitConditional(ConditionalExpression node)
        {
            return IsStringCombination(node)
                ? Wrap(this.VisitCombination(node))
                : base.VisitConditional(node);
        }

        /// <inheritdoc/>
        protected override Expression VisitMethodCall(MethodCallExpression node)
        {
            if (IsStringCombination(node))
            {
                return Wrap(this.VisitCombination(node));
            }

            return IsStringSubqueryResult(node)
                ? Wrap(base.VisitMethodCall(node))
                : base.VisitMethodCall(node);
        }

        #endregion protected methods

        #region private methods

        /// <summary>
        /// Determines whether an expression is a string concatenation (<c>+</c> or <see cref="string.Concat(string, string)"/>), <c>??</c>
        /// or conditional expression.
        /// </summary>
        /// <param name="node">The expression.</param>
        /// <returns><see langword="true"/> if it is, otherwise <see langword="false"/>.</returns>
        private static bool IsStringCombination(Expression node)
        {
            return node.Type == typeof(string) && node switch
            {
                BinaryExpression { NodeType: ExpressionType.Add or ExpressionType.Coalesce } => true,
                ConditionalExpression => true,
                MethodCallExpression { Method: { Name: nameof(string.Concat), IsStatic: true } method } => method.DeclaringType == typeof(string),
                _ => false,
            };
        }

        /// <summary>
        /// Wraps an expression in a call to <see cref="KeepCollation"/>.
        /// </summary>
        /// <param name="expression">The expression.</param>
        /// <returns>The call.</returns>
        private static MethodCallExpression Wrap(Expression expression)
        {
            return Expression.Call(KeepCollationMethod, expression);
        }

        /// <summary>
        /// Visits the operands of a string concatenation, <c>??</c> or conditional expression.
        /// </summary>
        /// <param name="node">The expression.</param>
        /// <returns>The expression with its operands visited.</returns>
        private Expression VisitCombination(Expression node)
        {
            return node switch
            {
                BinaryExpression binary => binary.Update(
                    this.VisitOperand(binary.Left),
                    this.VisitAndConvert(binary.Conversion, nameof(this.VisitCombination)),
                    this.VisitOperand(binary.Right)),

                ConditionalExpression conditional => conditional.Update(
                    this.Visit(conditional.Test),
                    this.VisitOperand(conditional.IfTrue),
                    this.VisitOperand(conditional.IfFalse)),

                MethodCallExpression methodCall => methodCall.Update(
                    methodCall.Object,
                    methodCall.Arguments.Select(this.VisitOperand)),

                _ => node,
            };
        }

        /// <summary>
        /// Visits an operand of a string concatenation, <c>??</c> or conditional expression.
        /// </summary>
        /// <param name="operand">The operand.</param>
        /// <returns>The visited operand.</returns>
        private Expression VisitOperand(Expression operand)
        {
            // An operand that is itself a concatenation, ?? or conditional expression isn't wrapped, as the outer one is,
            // so a + b + c is wrapped once rather than at each +.
            return IsStringCombination(operand)
                ? this.VisitCombination(operand)
                : this.Visit(operand);
        }

        #endregion private methods
    }

    #endregion private KeepCollationVisitor class
}
