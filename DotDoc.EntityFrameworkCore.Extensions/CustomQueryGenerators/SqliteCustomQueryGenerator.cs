// Copyright ©2021-2026 Mike King.
// This file is licensed to you under the MIT license.
// See the License.txt file in the solution root for more information.

// This code contains parts adapted from the EF Core source code which is licensed under the Apache license.
// See https://github.com/dotnet/efcore
using DotDoc.EntityFrameworkCore.Extensions.ExecuteUpdate;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;
using Microsoft.EntityFrameworkCore.Sqlite.Query.Internal;
using Microsoft.EntityFrameworkCore.Storage;
using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;

namespace DotDoc.EntityFrameworkCore.Extensions.CustomQueryGenerators;

/// <summary>
/// SQLite Custom Query Generator.
/// </summary>
[SuppressMessage("Usage", "EF1001:Internal EF Core API usage.", Justification = "We need to replace query generator with a custom one.")]
internal sealed class SqliteCustomQueryGenerator : SqliteQuerySqlGenerator
{
    #region private fields

    /// <summary>
    /// The stemming table to use for each table alias, decided by the first <c>FreeTextSearch</c> on that alias
    /// (<see langword="null"/> if it doesn't use stemming).
    /// </summary>
    private readonly Dictionary<string, string?> _stemmingTables = [];

    /// <summary>
    /// Query type (if any).
    /// </summary>
    private QueryType _queryType;

    #endregion private fields

    #region public constructors

    /// <summary>
    /// Initializes a new instance of the <see cref="SqliteCustomQueryGenerator"/> class.
    /// </summary>
    /// <param name="dependencies">Dependencies.</param>
    public SqliteCustomQueryGenerator(QuerySqlGeneratorDependencies dependencies)
        : base(dependencies)
    {
    }

    #endregion public constructors

    #region public methods

    /// <inheritdoc/>
    /// <remarks>
    /// Before any SQL is generated, finds the first <c>FreeTextSearch</c> (<c>MATCH</c>) on each table alias, which decides whether
    /// that alias's table is replaced with its stemming table. EF Core makes table aliases unique across the whole query, so this
    /// covers subqueries and set operations too. The decision is taken from the query expression only, so the cached SQL is always correct.
    /// </remarks>
    public override IRelationalCommand GetCommand(Expression queryExpression)
    {
        this._stemmingTables.Clear();
        StemmingTableFinder.Find(queryExpression, this._stemmingTables);

        return base.GetCommand(queryExpression);
    }

    #endregion public methods

    #region protected methods

    /// <inheritdoc/>
    /// <exception cref="InvalidOperationException">The query has more than one execute update tag, or more than one table hints tag.</exception>
    /// <remarks>
    /// The base class calls this before <see cref="VisitDelete(DeleteExpression)"/> / <see cref="VisitUpdate(UpdateExpression)"/>,
    /// so the query type is known by the time they run.
    /// </remarks>
    protected override void GenerateTagsHeaderComment(ISet<string> tags)
    {
        CustomTags customTags = CustomTagExtensions.GetCustomTags(tags);

        this._queryType = customTags.QueryType;

        base.GenerateTagsHeaderComment(customTags.RemainingTags);
    }

    /// <inheritdoc/>
    /// <exception cref="InvalidOperationException">For <c>ExecuteDeleteGetRowsAsync</c>, the query uses an operator that can't be used in the delete.</exception>
    /// <exception cref="NotSupportedException">For <c>ExecuteDeleteGetRowsAsync</c>, the table is configured with <c>UseSqlReturningClause(false)</c>.</exception>
    protected override Expression VisitDelete(DeleteExpression deleteExpression)
    {
        switch (this._queryType)
        {
            case QueryType.DeleteGetRows:
                this.GenerateDeleteGetRows(deleteExpression);
                return deleteExpression;

            default:
                return base.VisitDelete(deleteExpression);
        }
    }

    /// <inheritdoc/>
    /// <remarks>
    /// Writes the <c>MATCH</c> of a <c>FreeTextSearch</c> without its third argument, the stemming table name, which is only there for
    /// <see cref="GetCommand(Expression)"/>.
    /// </remarks>
    protected override Expression VisitSqlFunction(SqlFunctionExpression sqlFunctionExpression)
    {
        if (IsFreeTextSearchMatch(sqlFunctionExpression, out SqlExpression? freeText, out ColumnExpression? propertyReference, out _))
        {
            return base.VisitSqlFunction(new SqlFunctionExpression(
                functionName: sqlFunctionExpression.Name,
                arguments: [freeText, propertyReference],
                nullable: sqlFunctionExpression.IsNullable,
                argumentsPropagateNullability: [.. sqlFunctionExpression.ArgumentsPropagateNullability!.Take(2)],
                type: sqlFunctionExpression.Type,
                typeMapping: sqlFunctionExpression.TypeMapping));
        }

        return base.VisitSqlFunction(sqlFunctionExpression);
    }

    /// <inheritdoc/>
    /// <remarks>
    /// Replaces a table with its stemming table (keeping its alias, so every column reference still works) if the first
    /// <c>FreeTextSearch</c> on that alias uses stemming.
    /// </remarks>
    protected override Expression VisitTable(TableExpression tableExpression)
    {
        if (this._stemmingTables.TryGetValue(tableExpression.Alias, out string? stemmingTableName) && stemmingTableName is not null)
        {
            ISqlGenerationHelper sqlGenerationHelper = this.Dependencies.SqlGenerationHelper;

            this.Sql
                .Append(sqlGenerationHelper.DelimitIdentifier(stemmingTableName, tableExpression.Schema))
                .Append(this.AliasSeparator)
                .Append(sqlGenerationHelper.DelimitIdentifier(tableExpression.Alias));

            return tableExpression;
        }

        return base.VisitTable(tableExpression);
    }

    /// <inheritdoc/>
    /// <exception cref="InvalidOperationException">For <c>ExecuteInsertAsync</c> or <c>ExecuteInsertGetRowAsync</c>, the query is more than the table; for <c>ExecuteUpdateGetRowsAsync</c>, the query uses an operator that can't be used in the update.</exception>
    /// <exception cref="NotSupportedException">For <c>ExecuteInsertGetRowAsync</c> or <c>ExecuteUpdateGetRowsAsync</c>, the table is configured with <c>UseSqlReturningClause(false)</c>.</exception>
    protected override Expression VisitUpdate(UpdateExpression updateExpression)
    {
        switch (this._queryType)
        {
            case QueryType.Insert:
                this.GenerateInsert(updateExpression, false);
                return updateExpression;

            case QueryType.InsertGetRow:
                this.GenerateInsert(updateExpression, true);
                return updateExpression;

            case QueryType.UpdateGetRows:
                this.GenerateUpdateGetRows(updateExpression);
                return updateExpression;

            default:
                return base.VisitUpdate(updateExpression);
        }
    }

    #endregion protected methods

    #region private methods

    /// <summary>
    /// Determines whether a function is the <c>MATCH</c> of a <c>FreeTextSearch</c>, which has the stemming table name
    /// (or <see langword="null"/> if it doesn't use stemming) as a third argument.
    /// </summary>
    /// <param name="sqlFunctionExpression">The function.</param>
    /// <param name="freeText">The text being searched for.</param>
    /// <param name="propertyReference">The column being searched.</param>
    /// <param name="stemmingTableName">The stemming table name, or <see langword="null"/> if the search doesn't use stemming.</param>
    /// <returns><see langword="true"/> if the function is the <c>MATCH</c> of a <c>FreeTextSearch</c>, otherwise <see langword="false"/>.</returns>
    private static bool IsFreeTextSearchMatch(
        SqlFunctionExpression sqlFunctionExpression,
        [NotNullWhen(true)] out SqlExpression? freeText,
        [NotNullWhen(true)] out ColumnExpression? propertyReference,
        out string? stemmingTableName)
    {
        if (sqlFunctionExpression is
            {
                Name: "MATCH",
                IsBuiltIn: true,
                Instance: null,
                Arguments: [SqlExpression text, ColumnExpression column, SqlConstantExpression { Value: var value }]
            })
        {
            freeText = text;
            propertyReference = column;
            stemmingTableName = value as string;
            return true;
        }

        freeText = null;
        propertyReference = null;
        stemmingTableName = null;
        return false;
    }

    /// <summary>
    /// Throws a <see cref="NotSupportedException"/> if a table is configured not to use the RETURNING clause.
    /// </summary>
    /// <param name="tableExpression">The table being modified.</param>
    /// <param name="methodName">The name of the extension method being called, used in the exception message.</param>
    /// <exception cref="NotSupportedException">The table is configured with <c>UseSqlReturningClause(false)</c>.</exception>
    private static void ThrowIfReturningClauseNotUsed(TableExpression tableExpression, string methodName)
    {
        if (!tableExpression.GetEntityType().IsSqlReturningClauseUsed())
        {
            throw new NotSupportedException($"{methodName} requires the RETURNING clause, but the table {tableExpression.Name} is configured with UseSqlReturningClause(false).");
        }
    }

    /// <summary>
    /// Generates SQL for a Delete expression that returns the deleted rows.
    /// </summary>
    /// <param name="deleteExpression">The delete expression.</param>
    /// <exception cref="InvalidOperationException">The query uses an operator that can't be used in the delete.</exception>
    /// <exception cref="NotSupportedException">The table is configured with <c>UseSqlReturningClause(false)</c>.</exception>
    private void GenerateDeleteGetRows(DeleteExpression deleteExpression)
    {
        SelectExpression selectExpression = deleteExpression.SelectExpression;

        if (selectExpression is
            {
                GroupBy: [],
                Having: null,
                Limit: null,
                Offset: null,
                Orderings: [],
                Projection: [],
                Tables: [TableExpressionBase table]
            }

            && table.Equals(deleteExpression.Table))
        {
            ThrowIfReturningClauseNotUsed(deleteExpression.Table, nameof(ExecuteUpdateExtensions.ExecuteDeleteGetRowsAsync));

            this.Sql.Append("DELETE FROM ");
            this.Visit(deleteExpression.Table);

            if (selectExpression.Predicate is not null)
            {
                this.Sql.AppendLine().Append("WHERE ");
                this.Visit(selectExpression.Predicate);
            }

            this.Sql.AppendLine();
            this.GenerateReturningClause();
            this.Sql.AppendLine();

            return;
        }

        throw new InvalidOperationException(
            RelationalStrings.ExecuteOperationWithUnsupportedOperatorInSqlGeneration(
                nameof(ExecuteUpdateExtensions.ExecuteDeleteGetRowsAsync)));
    }

    /// <summary>
    /// Generates SQL for an Insert expression.
    /// </summary>
    /// <param name="updateExpression">The update expression.</param>
    /// <param name="getRow">A value indicating whether the SQL should return the inserted row.</param>
    /// <exception cref="InvalidOperationException">The query is more than the table (e.g. it has a <c>Where</c> or <c>OrderBy</c>).</exception>
    /// <exception cref="NotSupportedException"><paramref name="getRow"/> is <see langword="true"/> and the table is configured with <c>UseSqlReturningClause(false)</c>.</exception>
    private void GenerateInsert(UpdateExpression updateExpression, bool getRow)
    {
        SelectExpression selectExpression = updateExpression.SelectExpression;

        if (selectExpression is
            {
                GroupBy: [],
                Having: null,
                Limit: null,
                Offset: null,
                Orderings: [],
                Predicate: null,
                Projection: [],
                Tables: [TableExpressionBase table]
            }

            && table.Equals(updateExpression.Table))
        {
            if (getRow)
            {
                ThrowIfReturningClauseNotUsed(updateExpression.Table, nameof(ExecuteUpdateExtensions.ExecuteInsertGetRowAsync));
            }

            this.Sql.AppendLine($"INSERT INTO {this.Dependencies.SqlGenerationHelper.DelimitIdentifier(updateExpression.Table.Name)}");

            this.Sql.AppendLine("(");

            using (_ = this.Sql.Indent())
            {
                for (int i = 0; i < updateExpression.ColumnValueSetters.Count; i++)
                {
                    ColumnValueSetter setter = updateExpression.ColumnValueSetters[i];

                    if (i > 0)
                    {
                        this.Sql.AppendLine(",");
                    }

                    this.Sql.Append(this.Dependencies.SqlGenerationHelper.DelimitIdentifier(setter.Column.Name));
                }

                this.Sql.AppendLine();
            }

            this.Sql.AppendLine(")");
            this.Sql.AppendLine("VALUES");
            this.Sql.AppendLine("(");

            using (_ = this.Sql.Indent())
            {
                for (int i = 0; i < updateExpression.ColumnValueSetters.Count; i++)
                {
                    ColumnValueSetter setter = updateExpression.ColumnValueSetters[i];

                    if (i > 0)
                    {
                        this.Sql.AppendLine(",");
                    }

                    this.Visit(setter.Value);
                }

                this.Sql.AppendLine();
            }

            this.Sql.AppendLine(")");

            if (getRow)
            {
                this.GenerateReturningClause();
                this.Sql.AppendLine();
            }

            return;
        }

        throw new InvalidOperationException(
            RelationalStrings.ExecuteOperationWithUnsupportedOperatorInSqlGeneration(
                getRow ? nameof(ExecuteUpdateExtensions.ExecuteInsertGetRowAsync) : nameof(ExecuteUpdateExtensions.ExecuteInsertAsync)));
    }

    /// <summary>
    /// Generates SQL for an Update expression that returns the updated rows.
    /// </summary>
    /// <param name="updateExpression">The update expression.</param>
    /// <exception cref="InvalidOperationException">The query uses an operator that can't be used in the update.</exception>
    /// <exception cref="NotSupportedException">The table is configured with <c>UseSqlReturningClause(false)</c>.</exception>
    private void GenerateUpdateGetRows(UpdateExpression updateExpression)
    {
        SelectExpression selectExpression = updateExpression.SelectExpression;

        if (selectExpression is
            {
                GroupBy: [],
                Having: null,
                Limit: null,
                Offset: null,
                Orderings: [],
                Projection: []
            }

            && (selectExpression.Tables.Count == 1
                || !ReferenceEquals(selectExpression.Tables[0], updateExpression.Table)
                || selectExpression.Tables[1] is InnerJoinExpression
                || selectExpression.Tables[1] is CrossJoinExpression))
        {
            ThrowIfReturningClauseNotUsed(updateExpression.Table, nameof(ExecuteUpdateExtensions.ExecuteUpdateGetRowsAsync));

            this.Sql.Append("UPDATE ");

            this.Visit(updateExpression.Table);

            this.Sql.AppendLine();
            this.Sql.AppendLine("SET");

            using (this.Sql.Indent())
            {
                for (int i = 0; i < updateExpression.ColumnValueSetters.Count; i++)
                {
                    ColumnValueSetter setter = updateExpression.ColumnValueSetters[i];

                    if (i > 0)
                    {
                        this.Sql.AppendLine(",");
                    }

                    this.Sql.Append(this.Dependencies.SqlGenerationHelper.DelimitIdentifier(setter.Column.Name));
                    this.Sql.Append(" = ");
                    this.Visit(setter.Value);
                }
            }

            SqlExpression? predicate = selectExpression.Predicate;
            bool firstTablePrinted = false;
            if (selectExpression.Tables.Count > 1)
            {
                this.Sql.AppendLine().Append("FROM ");
                for (int i = 0; i < selectExpression.Tables.Count; i++)
                {
                    TableExpressionBase table = selectExpression.Tables[i];
                    JoinExpressionBase? joinExpression = table as JoinExpressionBase;

                    if (updateExpression.Table.Alias == (joinExpression?.Table.Alias ?? table.Alias))
                    {
                        LiftPredicate(table);
                        continue;
                    }

                    if (firstTablePrinted)
                    {
                        this.Sql.AppendLine();
                    }
                    else
                    {
                        firstTablePrinted = true;
                        LiftPredicate(table);
                        table = joinExpression?.Table ?? table;
                    }

                    this.Visit(table);

                    void LiftPredicate(TableExpressionBase joinTable)
                    {
                        if (joinTable is PredicateJoinExpressionBase predicateJoinExpression)
                        {
                            predicate = predicate is null
                                ? predicateJoinExpression.JoinPredicate
                                : new SqlBinaryExpression(
                                    ExpressionType.AndAlso,
                                    predicateJoinExpression.JoinPredicate,
                                    predicate,
                                    typeof(bool),
                                    predicate.TypeMapping);
                        }
                    }
                }
            }

            if (predicate is not null)
            {
                this.Sql.AppendLine().Append("WHERE ");
                this.Visit(predicate);
            }

            this.Sql.AppendLine();
            this.GenerateReturningClause();
            this.Sql.AppendLine();

            return;
        }

        throw new InvalidOperationException(
            RelationalStrings.ExecuteOperationWithUnsupportedOperatorInSqlGeneration(
                nameof(ExecuteUpdateExtensions.ExecuteUpdateGetRowsAsync)));
    }

    /// <summary>
    /// Generates a RETURNING clause.
    /// </summary>
    private void GenerateReturningClause()
    {
        this.Sql.Append("RETURNING *");
    }

    #endregion private methods

    #region private StemmingTableFinder class

    /// <summary>
    /// Finds the first <c>FreeTextSearch</c> on each table alias, and the stemming table it uses (if any).
    /// </summary>
    private sealed class StemmingTableFinder : ExpressionVisitor
    {
        #region private fields

        /// <summary>
        /// The stemming table for each table alias (<see langword="null"/> if it doesn't use stemming).
        /// </summary>
        private readonly Dictionary<string, string?> _stemmingTables;

        #endregion private fields

        #region private constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="StemmingTableFinder"/> class.
        /// </summary>
        /// <param name="stemmingTables">The dictionary to add the stemming table for each table alias to.</param>
        private StemmingTableFinder(Dictionary<string, string?> stemmingTables)
        {
            this._stemmingTables = stemmingTables;
        }

        #endregion private constructors

        #region public methods

        /// <summary>
        /// Finds the first <c>FreeTextSearch</c> on each table alias in a query, and adds its stemming table (or <see langword="null"/>)
        /// to <paramref name="stemmingTables"/>.
        /// </summary>
        /// <param name="queryExpression">The query expression.</param>
        /// <param name="stemmingTables">The dictionary to add the stemming table for each table alias to.</param>
        public static void Find(Expression queryExpression, Dictionary<string, string?> stemmingTables)
        {
            StemmingTableFinder finder = new(stemmingTables);
            finder.Visit(queryExpression);
        }

        #endregion public methods

        #region protected methods

        /// <inheritdoc/>
        protected override Expression VisitExtension(Expression node)
        {
            // Only the first search on an alias counts, so later ones don't replace it.
            if (node is SqlFunctionExpression sqlFunctionExpression
                && IsFreeTextSearchMatch(sqlFunctionExpression, out _, out ColumnExpression? propertyReference, out string? stemmingTableName))
            {
                this._stemmingTables.TryAdd(propertyReference.TableAlias, stemmingTableName);
            }

            return base.VisitExtension(node);
        }

        #endregion protected methods
    }

    #endregion private StemmingTableFinder class
}
