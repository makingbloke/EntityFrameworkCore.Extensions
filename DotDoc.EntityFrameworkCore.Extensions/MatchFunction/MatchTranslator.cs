// Copyright ©2021-2026 Mike King.
// This file is licensed to you under the MIT license.
// See the License.txt file in the solution root for more information.

using DotDoc.EntityFrameworkCore.Extensions.Utilities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;
using System.Reflection;

namespace DotDoc.EntityFrameworkCore.Extensions.MatchFunction;

/// <summary>
/// Match Function Translator.
/// </summary>
internal sealed class MatchTranslator : IMethodCallTranslator
{
    #region private fields

    /// <summary>
    /// The SQL expression factory.
    /// </summary>
    private readonly ISqlExpressionFactory _sqlExpressionFactory;

    #endregion private fields

    #region public constructors

    /// <summary>
    /// Initializes a new instance of the <see cref="MatchTranslator"/> class.
    /// </summary>
    /// <param name="sqlExpressionFactory">The SQL expression factory.</param>
    /// <exception cref="ArgumentNullException"><paramref name="sqlExpressionFactory"/> is <see langword="null"/>.</exception>
    public MatchTranslator(ISqlExpressionFactory sqlExpressionFactory)
    {
        ArgumentNullException.ThrowIfNull(sqlExpressionFactory);

        this._sqlExpressionFactory = sqlExpressionFactory;
    }

    #endregion public constructors

    #region public methods

    /// <inheritdoc/>
    /// <exception cref="InvalidOperationException">The property reference argument of <c>Match</c> is not a column.</exception>
    public SqlExpression? Translate(SqlExpression? instance, MethodInfo method, IReadOnlyList<SqlExpression> arguments, IDiagnosticsLogger<DbLoggerCategory.Query> logger)
    {
        if (method == MatchDbFunctionsExtensions.MatchMethod)
        {
            MatchArguments inputArguments = new(method, arguments);

            SqlExpression sqlExpression = new SqlFunctionExpression(
                functionName: nameof(MatchDbFunctionsExtensions.Match),
                arguments: [this._sqlExpressionFactory.ApplyDefaultTypeMappingIfNone(inputArguments.FreeText), inputArguments.PropertyReference],
                nullable: true,
                argumentsPropagateNullability: [true, false],
                type: typeof(bool),
                typeMapping: null);

            return sqlExpression;
        }

        return null;
    }

    #endregion public methods
}
