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

    #region protected methods

    /// <inheritdoc/>
    /// <exception cref="InvalidOperationException">The query has more than one execute update tag, or more than one table hints tag.</exception>
    /// <remarks>
    /// The base class calls this before <see cref="VisitDelete(DeleteExpression)"/> / <see cref="VisitUpdate(UpdateExpression)"/>,
    /// so the query type is known by the time they run.
    /// </remarks>
    protected override void GenerateTagsHeaderComment(ISet<string> tags)
    {
        CustomTagsHandler tagsHandler = CustomTagsHandler.ProcessTags(tags);

        this._queryType = tagsHandler.QueryType;

        base.GenerateTagsHeaderComment(tagsHandler.RemainingTags);
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
                for (var i = 0; i < updateExpression.ColumnValueSetters.Count; i++)
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
}
