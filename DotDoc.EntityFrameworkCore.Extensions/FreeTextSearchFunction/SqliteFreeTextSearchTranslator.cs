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
/// SQLite FreeTextSearch Function Translator.
/// </summary>
internal sealed class SqliteFreeTextSearchTranslator : IMethodCallTranslator
{
    #region private fields

    /// <summary>
    /// The SQL expression factory.
    /// </summary>
    private readonly ISqlExpressionFactory _sqlExpressionFactory;

    #endregion private fields

    #region public constructors

    /// <summary>
    /// Initializes a new instance of the <see cref="SqliteFreeTextSearchTranslator"/> class.
    /// </summary>
    /// <param name="sqlExpressionFactory">The SQL expression factory.</param>
    /// <exception cref="ArgumentNullException"><paramref name="sqlExpressionFactory"/> is <see langword="null"/>.</exception>
    public SqliteFreeTextSearchTranslator(ISqlExpressionFactory sqlExpressionFactory)
    {
        ArgumentNullException.ThrowIfNull(sqlExpressionFactory);

        this._sqlExpressionFactory = sqlExpressionFactory;
    }

    #endregion public constructors

    #region public methods

    /// <inheritdoc/>
    /// <exception cref="InvalidOperationException">
    /// The property reference argument of <c>FreeTextSearch</c> is not a column, or a search with stemming is on a table with no stemming table set.
    /// </exception>
    public SqlExpression? Translate(SqlExpression? instance, MethodInfo method, IReadOnlyList<SqlExpression> arguments, IDiagnosticsLogger<DbLoggerCategory.Query> logger)
    {
        if (method == SqliteFreeTextSearchInterceptor.StemmingSearchMethod)
        {
            return this.TranslateStemmingSearch(arguments);
        }

        if (FreeTextSearchDbFunctionsExtensions.FreeTextSearchMethods.Contains(method))
        {
            FreeTextSearchArguments inputArguments = new(method, arguments);

            SqlExpression sqlExpression = new SqlFunctionExpression(
                functionName: "MATCH",
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

    #region private methods

    /// <summary>
    /// Translates a candidate stemming search added by <see cref="SqliteFreeTextSearchInterceptor"/>.
    /// </summary>
    /// <param name="arguments">The arguments: the property reference, the name of the table the candidate is for, and its search of that table's stemming table.</param>
    /// <returns>The search if the property is a column of that table, otherwise <see langword="false"/>.</returns>
    /// <exception cref="InvalidOperationException">The property reference is not a column, or its table has no stemming table set.</exception>
    private SqlExpression TranslateStemmingSearch(IReadOnlyList<SqlExpression> arguments)
    {
        if (arguments[0] is not ColumnExpression { Column: { } column })
        {
            throw new InvalidOperationException("PropertyReference argument is not a column expression");
        }

        // The column's table is only known now, so check it has a stemming table (the interceptor only knew some table of its CLR type had one).
        IEntityType? entityType = column.PropertyMappings.Select(mapping => mapping.Property.DeclaringType).OfType<IEntityType>().FirstOrDefault();

        if (string.IsNullOrEmpty(entityType?.GetStemmingTable()))
        {
            throw new InvalidOperationException("Stemming table not specified.");
        }

        string? tableName = (arguments[1] as SqlConstantExpression)?.Value as string;

        return column.Table.Name == tableName
            ? arguments[2]
            : this._sqlExpressionFactory.ApplyDefaultTypeMapping(this._sqlExpressionFactory.Constant(false));
    }

    #endregion private methods
}
