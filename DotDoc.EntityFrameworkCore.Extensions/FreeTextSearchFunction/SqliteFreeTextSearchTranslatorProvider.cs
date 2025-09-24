// Copyright ©2021-2025 Mike King.
// This file is licensed to you under the MIT license.
// See the License.txt file in the solution root for more information.

using DotDoc.EntityFrameworkCore.Extensions.Utilities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;
using System.Reflection;

namespace DotDoc.EntityFrameworkCore.Extensions.FreeTextSearchFunction;

/// <summary>
/// SQLite FreeTextSearch Function Translator Provider.
/// </summary>
internal sealed class SqliteFreeTextSearchTranslatorProvider : TranslatorProviderBase
{
    #region public constructors

    /// <inheritdoc/>
    public SqliteFreeTextSearchTranslatorProvider(RelationalMethodCallTranslatorProviderDependencies dependencies)
        : base(dependencies)
    {
    }

    #endregion public constructors

    #region public members

    /// <inheritdoc/>
    public override SqlExpression? Translate(IModel model, SqlExpression? instance, MethodInfo method, IReadOnlyList<SqlExpression> arguments, IDiagnosticsLogger<DbLoggerCategory.Query> logger)
    {
        if (FreeTextSearchDbFunctionsExtensions.FreeTextSearchMethods.Contains(method))
        {
            FreeTextSearchArguments inputArguments = new(method, arguments);

            SqlExpression sqlExpression = new SqlFunctionExpression(
                functionName: "MATCH",
                arguments: [this.ApplyTypeMapping(inputArguments.FreeText), inputArguments.PropertyReference],
                nullable: true,
                argumentsPropagateNullability: [true, false],
                type: typeof(bool),
                typeMapping: null);

            return sqlExpression;
        }

        return base.Translate(model, instance, method, arguments, logger);
    }

    #endregion public methods
}
