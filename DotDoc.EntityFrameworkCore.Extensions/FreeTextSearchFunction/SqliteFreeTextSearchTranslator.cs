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
    /// <exception cref="InvalidOperationException">The property reference argument of <c>FreeTextSearch</c> is not a column, or the search uses stemming and the column's table has no stemming table set.
    /// </exception>
    public SqlExpression? Translate(SqlExpression? instance, MethodInfo method, IReadOnlyList<SqlExpression> arguments, IDiagnosticsLogger<DbLoggerCategory.Query> logger)
    {
        if (FreeTextSearchDbFunctionsExtensions.FreeTextSearchMethods.Contains(method))
        {
            FreeTextSearchArguments inputArguments = new(method, arguments);

            // The third argument is the stemming table name, or null if the search doesn't use stemming. It's used by the
            // custom query generator to swap the column's table for its stemming table and is left out of the generated SQL.
            string? stemmingTableName = inputArguments.UseStemming?.Value is true
                ? GetStemmingTableName(inputArguments.PropertyReference)
                : null;

            SqlExpression sqlExpression = new SqlFunctionExpression(
                functionName: "MATCH",
                arguments:
                [
                    this._sqlExpressionFactory.ApplyDefaultTypeMappingIfNone(inputArguments.FreeText),
                    inputArguments.PropertyReference,
                    this._sqlExpressionFactory.ApplyDefaultTypeMappingIfNone(this._sqlExpressionFactory.Constant(stemmingTableName, typeof(string))),
                ],
                nullable: true,
                argumentsPropagateNullability: [true, false, false],
                type: typeof(bool),
                typeMapping: null);

            return sqlExpression;
        }

        return null;
    }

    #endregion public methods

    #region private methods

    /// <summary>
    /// Gets the name of the stemming table of the column's table.
    /// </summary>
    /// <param name="propertyReference">The column being searched.</param>
    /// <returns>The stemming table name.</returns>
    /// <exception cref="InvalidOperationException">The column's table has no stemming table set (<c>SetStemmingTable</c>).</exception>
    private static string GetStemmingTableName(ColumnExpression propertyReference)
    {
        IEntityType? entityType = propertyReference.Column?.PropertyMappings
            .Select(mapping => mapping.Property.DeclaringType)
            .OfType<IEntityType>()
            .FirstOrDefault();

        string? stemmingTableName = entityType?.GetStemmingTable();

        return string.IsNullOrEmpty(stemmingTableName)
            ? throw new InvalidOperationException("Stemming table not specified.")
            : stemmingTableName;
    }

    #endregion private methods
}
