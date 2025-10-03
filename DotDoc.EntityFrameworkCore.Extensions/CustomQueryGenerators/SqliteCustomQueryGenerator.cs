// Copyright ©2021-2025 Mike King.
// This file is licensed to you under the MIT license.
// See the License.txt file in the solution root for more information.
//
// This code contains parts adapted from the EF Core source code which is licensed under the Apache license.
// See https://github.com/dotnet/efcore

using DotDoc.EntityFrameworkCore.Extensions.ExecuteUpdate;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Metadata;
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
    protected override void GenerateRootCommand(Expression queryExpression)
    {
        switch (queryExpression)
        {
            case DeleteExpression deleteExpression when ExecuteUpdateExtensions.QueryParameters.QueryType == QueryType.DeleteGetRows:
                this.GenerateTagsHeaderComment(deleteExpression.Tags);
                this.VisitDeleteGetRows(deleteExpression);
                break;

            case UpdateExpression updateExpression when ExecuteUpdateExtensions.QueryParameters.QueryType == QueryType.InsertGetRow:
                this.GenerateTagsHeaderComment(updateExpression.Tags);
                this.VisitInsertGetRow(updateExpression);
                break;

            case UpdateExpression updateExpression when ExecuteUpdateExtensions.QueryParameters.QueryType == QueryType.UpdateGetRows:
                this.GenerateTagsHeaderComment(updateExpression.Tags);
                this.VisitUpdateGetRows(updateExpression);
                break;

            default:
                base.GenerateRootCommand(queryExpression);
                break;
        }
    }

    #endregion protected methods

    #region private methods

    /// <summary>
    /// Generates SQL for a Delete expression.
    /// (Based on the EF Core one but returns output).
    /// </summary>
    /// <param name="deleteExpression">The delete expression.</param>
    private void VisitDeleteGetRows(DeleteExpression deleteExpression)
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
            IEntityType entityType = deleteExpression.Table.GetEntityType();
            bool isSqlReturningClauseUsed = entityType.IsSqlReturningClauseUsed();

            if (!isSqlReturningClauseUsed)
            {
                SelectExpression deleteSelectExpression = selectExpression.Update(
                    tables: selectExpression.Tables,
                    predicate: selectExpression.Predicate,
                    groupBy: selectExpression.GroupBy,
                    having: selectExpression.Having,
                    projections: [new ProjectionExpression(new SqlFragmentExpression("*"), string.Empty)],
                    orderings: selectExpression.Orderings,
                    offset: selectExpression.Offset,
                    selectExpression.Limit);

                this.Visit(deleteSelectExpression);
                this.Sql.AppendLine().AppendLine(";");
            }

            this.Sql.Append("DELETE FROM ");
            this.Visit(deleteExpression.Table);

            if (selectExpression.Predicate is not null)
            {
                this.Sql.AppendLine().Append("WHERE ");
                this.Visit(selectExpression.Predicate);
            }

            if (isSqlReturningClauseUsed)
            {
                this.Sql.AppendLine().AppendLine("RETURNING *");
            }

            return;
        }

        throw new InvalidOperationException(
            RelationalStrings.ExecuteOperationWithUnsupportedOperatorInSqlGeneration(
                nameof(this.VisitDeleteGetRows)));
    }

    /// <summary>
    /// Generates SQL for an Insert expression.
    /// </summary>
    /// <param name="updateExpression">The update expression.</param>
    private void VisitInsertGetRow(UpdateExpression updateExpression)
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
            this.Sql.AppendLine($"INSERT INTO {this.Dependencies.SqlGenerationHelper.DelimitIdentifier(updateExpression.Table.Name)}");
            this.Sql.AppendLine("(");

            if (updateExpression.ColumnValueSetters.Count > 0)
            {
                using (this.Sql.Indent())
                {
                    for (int i = 0; i < updateExpression.ColumnValueSetters.Count; i++)
                    {
                        if (i > 0)
                        {
                            this.Sql.AppendLine(",");
                        }

                        this.Sql.Append(this.Dependencies.SqlGenerationHelper.DelimitIdentifier(updateExpression.ColumnValueSetters[0].Column.Name));
                    }

                    this.Sql.AppendLine();
                }
            }

            this.Sql.AppendLine(")");
            this.Sql.AppendLine("VALUES");
            this.Sql.AppendLine("(");

            if (updateExpression.ColumnValueSetters.Count > 0)
            {
                using (this.Sql.Indent())
                {
                    for (int i = 0; i < updateExpression.ColumnValueSetters.Count; i++)
                    {
                        if (i > 0)
                        {
                            this.Sql.AppendLine(",");
                        }

                        this.Visit(updateExpression.ColumnValueSetters[i].Value);
                    }

                    this.Sql.AppendLine();
                }
            }

            this.Sql.AppendLine(")");

            IEntityType entityType = updateExpression.Table.GetEntityType();
            bool isSqlReturningClauseUsed = entityType.IsSqlReturningClauseUsed();

            if (isSqlReturningClauseUsed)
            {
                this.Sql.AppendLine("RETURNING *");
            }
            else
            {
                string columnName = entityType.GetPrimaryKeyColumnName();

                this.Sql.AppendLine(";");
                this.Sql.AppendLine($"SELECT * FROM {this.Dependencies.SqlGenerationHelper.DelimitIdentifier(updateExpression.Table.Name)}");
                this.Sql.AppendLine($"WHERE {this.Dependencies.SqlGenerationHelper.DelimitIdentifier(columnName)} = LAST_INSERT_ROWID()");
            }

            return;
        }

        throw new InvalidOperationException(
            RelationalStrings.ExecuteOperationWithUnsupportedOperatorInSqlGeneration(
                nameof(this.VisitInsertGetRow)));
    }

    /// <summary>
    /// Generates SQL for an Update expression.
    /// (Based on the EF Core one but returns output).
    /// </summary>
    /// <param name="updateExpression">The update expression.</param>
    private void VisitUpdateGetRows(UpdateExpression updateExpression)
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
            this.Sql.Append("UPDATE ");
            this.Visit(updateExpression.Table);
            this.Sql.AppendLine();
            this.Sql.Append("SET ");
            this.Sql.Append($"{this.Dependencies.SqlGenerationHelper.DelimitIdentifier(updateExpression.ColumnValueSetters[0].Column.Name)} = ");
            this.Visit(updateExpression.ColumnValueSetters[0].Value);

            using (this.Sql.Indent())
            {
                foreach (ColumnValueSetter columnValueSetter in updateExpression.ColumnValueSetters.Skip(1))
                {
                    this.Sql.AppendLine(",");
                    this.Sql.Append($"{this.Dependencies.SqlGenerationHelper.DelimitIdentifier(columnValueSetter.Column.Name)} = ");
                    this.Visit(columnValueSetter.Value);
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

                    if (ReferenceEquals(updateExpression.Table, joinExpression?.Table ?? table))
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

            IEntityType entityType = updateExpression.Table.GetEntityType();
            bool isSqlReturningClauseUsed = entityType.IsSqlReturningClauseUsed();

            if (isSqlReturningClauseUsed)
            {
                this.Sql.AppendLine().AppendLine("RETURNING *");
            }
            else
            {
                SelectExpression updateSelectExpression = selectExpression.Update(
                    tables: selectExpression.Tables,
                    predicate: selectExpression.Predicate,
                    groupBy: selectExpression.GroupBy,
                    having: selectExpression.Having,
                    projections: [new ProjectionExpression(new SqlFragmentExpression("*"), string.Empty)],
                    orderings: selectExpression.Orderings,
                    offset: selectExpression.Offset,
                    selectExpression.Limit);

                this.Sql.AppendLine().AppendLine(";");
                this.Visit(updateSelectExpression);
            }

            return;
        }

        throw new InvalidOperationException(
            RelationalStrings.ExecuteOperationWithUnsupportedOperatorInSqlGeneration(
                nameof(this.VisitUpdateGetRows)));
    }

    #endregion private methods
}
