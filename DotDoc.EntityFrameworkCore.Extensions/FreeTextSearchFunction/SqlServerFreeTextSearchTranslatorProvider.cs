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

namespace DotDoc.EntityFrameworkCore.Extensions.FreeTextSearchFunction;

/// <summary>
/// SqlServer FreeTextSearch Function Translator Provider.
/// </summary>
internal sealed class SqlServerFreeTextSearchTranslatorProvider : TranslatorProviderBase
{
    #region public constructors

    /// <inheritdoc/>
    public SqlServerFreeTextSearchTranslatorProvider(RelationalMethodCallTranslatorProviderDependencies dependencies)
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

            string functionName = inputArguments.UseStemming is SqlConstantExpression { Value: true }
                ? "FREETEXT"
                : "CONTAINS";

            List<SqlExpression> outputArguments = [inputArguments.PropertyReference, this.ApplyTypeMapping(inputArguments.FreeText)];
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

        return base.Translate(model, instance, method, arguments, logger);
    }

    #endregion public methods
}
