// Copyright ©2021-2025 Mike King.
// This file is licensed to you under the MIT license.
// See the License.txt file in the solution root for more information.

using DotDoc.EntityFrameworkCore.Extensions.DatabaseType;
using Microsoft.EntityFrameworkCore.Diagnostics;
using System.Linq.Expressions;

namespace DotDoc.EntityFrameworkCore.Extensions.FreeTextSearchFunction;

/// <summary>
/// FreeTextSearch Function Query Expression Interceptor.
/// </summary>
internal sealed class FreeTextQueryExpressionInterceptor : IQueryExpressionInterceptor
{
    #region internal fields

    /// <summary>
    /// Singleton instance of this interceptor.
    /// </summary>
    internal static readonly FreeTextQueryExpressionInterceptor Instance = new();

    #endregion internal fields

    #region private constructors

    /// <summary>
    /// Initializes a new instance of the <see cref="FreeTextQueryExpressionInterceptor"/> class.
    /// </summary>
    private FreeTextQueryExpressionInterceptor()
    {
    }

    #endregion private constructors

    #region public method

    /// <inheritdoc/>
    public Expression QueryCompilationStarting(Expression queryExpression, QueryExpressionEventData eventData)
    {
        string databaseType = eventData.Context!.Database.GetDatabaseType();

        Expression expression = databaseType switch
        {
            DatabaseTypes.Sqlite => FreeTextSqliteExpressionVisitor.VisitExpression(eventData.Context, queryExpression),
            DatabaseTypes.SqlServer => FreeTextSqlServerExpressionVisitor.VisitExpression(queryExpression),
            _ => throw new InvalidOperationException("Unsupported database type")
        };

        return expression;
    }

    #endregion public method
}
