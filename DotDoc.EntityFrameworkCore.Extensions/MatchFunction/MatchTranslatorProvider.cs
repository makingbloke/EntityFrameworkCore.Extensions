// Copyright ©2021-2026 Mike King.
// This file is licensed to you under the MIT license.
// See the License.txt file in the solution root for more information.

using DotDoc.EntityFrameworkCore.Extensions.Utilities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;
using System.Reflection;

namespace DotDoc.EntityFrameworkCore.Extensions.MatchFunction;

/// <summary>
/// Match Function Translator Provider.
/// </summary>
internal sealed class MatchTranslatorProvider : TranslatorProviderBase
{
    #region public constructors

    /// <inheritdoc/>
    public MatchTranslatorProvider(RelationalMethodCallTranslatorProviderDependencies dependencies)
        : base(dependencies)
    {
    }

    #endregion public constructors

    #region public members

    /// <inheritdoc/>
    public override SqlExpression? Translate(IModel model, SqlExpression? instance, MethodInfo method, IReadOnlyList<SqlExpression> arguments, IDiagnosticsLogger<DbLoggerCategory.Query> logger)
    {
        if (method == MatchDbFunctionsExtensions.MatchMethod)
        {
            MatchArguments inputArguments = new(method, arguments);

            SqlExpression sqlExpression = new SqlFunctionExpression(
                functionName: nameof(MatchDbFunctionsExtensions.Match),
                arguments: [this.ApplyTypeMapping(inputArguments.FreeText), inputArguments.PropertyReference],
                nullable: true,
                argumentsPropagateNullability: [false, false],
                type: typeof(bool),
                typeMapping: null);

            return sqlExpression;
        }

        return base.Translate(model, instance, method, arguments, logger);
    }

    #endregion public methods
}
