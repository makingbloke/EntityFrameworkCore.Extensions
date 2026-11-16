// Copyright ©2021-2026 Mike King.
// This file is licensed to you under the MIT license.
// See the License.txt file in the solution root for more information.

using DotDoc.EntityFrameworkCore.Extensions.Utilities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;
using System.Reflection;

namespace DotDoc.EntityFrameworkCore.Extensions.FreeTextSearchFunction;

/// <summary>
/// SqlServer FreeTextSearch Function Translator.
/// </summary>
internal sealed class SqlServerFreeTextSearchTranslator : IMethodCallTranslator
{
    #region private fields

    /// <summary>
    /// The SQL expression factory.
    /// </summary>
    private readonly ISqlExpressionFactory _sqlExpressionFactory;

    #endregion private fields

    #region public constructors

    /// <summary>
    /// Initializes a new instance of the <see cref="SqlServerFreeTextSearchTranslator"/> class.
    /// </summary>
    /// <param name="sqlExpressionFactory">The SQL expression factory.</param>
    /// <exception cref="ArgumentNullException"><paramref name="sqlExpressionFactory"/> is <see langword="null"/>.</exception>
    public SqlServerFreeTextSearchTranslator(ISqlExpressionFactory sqlExpressionFactory)
    {
        ArgumentNullException.ThrowIfNull(sqlExpressionFactory);

        this._sqlExpressionFactory = sqlExpressionFactory;
    }

    #endregion public constructors

    #region public methods

    /// <inheritdoc/>
    /// <exception cref="InvalidOperationException">The property reference argument of <c>FreeTextSearch</c> is not a column.</exception>
    public SqlExpression? Translate(SqlExpression? instance, MethodInfo method, IReadOnlyList<SqlExpression> arguments, IDiagnosticsLogger<DbLoggerCategory.Query> logger)
    {
        if (FreeTextSearchDbFunctionsExtensions.FreeTextSearchMethods.Contains(method))
        {
            FreeTextSearchArguments inputArguments = new(method, arguments);

            string functionName = inputArguments.UseStemming is SqlConstantExpression { Value: true }
                ? "FREETEXT"
                : "CONTAINS";

            List<SqlExpression> outputArguments = [inputArguments.PropertyReference, this._sqlExpressionFactory.ApplyDefaultTypeMappingIfNone(inputArguments.FreeText)];
            List<bool> argumentsPropagateNullability = [false, true];

            if (inputArguments.LanguageTerm is not null)
            {
                SqlFragmentExpression language = new($"LANGUAGE {inputArguments.LanguageTerm.Value}");

                outputArguments.Add(language);
                argumentsPropagateNullability.Add(false);
            }

            SqlExpression sqlExpression = new SqlFunctionExpression(
                functionName: functionName,
                arguments: outputArguments,
                nullable: true,
                argumentsPropagateNullability: argumentsPropagateNullability,
                type: typeof(bool),
                typeMapping: null);

            return sqlExpression;
        }

        return null;
    }

    #endregion public methods
}
