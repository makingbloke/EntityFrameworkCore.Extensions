// Copyright ©2021-2026 Mike King.
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
        ExecuteUpdateParameters? queryParameters = CustomQueryGeneratorParameters.ExecuteUpdateParameters.Value;

        switch (queryExpression)
        {
            case DeleteExpression deleteExpression when queryParameters is { QueryType: QueryType.DeleteGetRows }:
                this.GenerateTagsHeaderComment(deleteExpression.Tags);
                this.VisitDelete(deleteExpression, true);
                break;

            case DeleteExpression deleteExpression:
                this.GenerateTagsHeaderComment(deleteExpression.Tags);
                this.VisitDelete(deleteExpression, false);
                break;

            case UpdateExpression updateExpression when queryParameters is { QueryType: QueryType.Insert }:
                this.GenerateTagsHeaderComment(updateExpression.Tags);
                this.VisitInsert(updateExpression, false);
                break;

            case UpdateExpression updateExpression when queryParameters is { QueryType: QueryType.InsertGetRow }:
                this.GenerateTagsHeaderComment(updateExpression.Tags);
                this.VisitInsert(updateExpression, true);
                break;

            case UpdateExpression updateExpression when queryParameters is { QueryType: QueryType.UpdateGetRows }:
                this.GenerateTagsHeaderComment(updateExpression.Tags);
                this.VisitUpdate(updateExpression, true);
                break;

            case UpdateExpression updateExpression:
                this.GenerateTagsHeaderComment(updateExpression.Tags);
                this.VisitUpdate(updateExpression, false);
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
    /// </summary>
    /// <param name="deleteExpression">The delete expression.</param>
    /// <param name="getRows">A value indicating whether the SQL should return the deleted rows.</param>
    private void VisitDelete(DeleteExpression deleteExpression, bool getRows)
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
            bool isSqlOutputClauseUsed = false;

            if (getRows)
            {
                IEntityType entityType = deleteExpression.Table.GetEntityType();
                isSqlOutputClauseUsed = entityType.IsSqlOutputClauseUsed();
            }

            if (getRows && !isSqlOutputClauseUsed)
            {
                this.GenerateSelectModifiedRows(selectExpression);
                this.Sql.AppendLine(";");
            }

            this.Sql.Append("DELETE FROM ");
            this.Visit(deleteExpression.Table);

            if (selectExpression.Predicate is not null)
            {
                this.Sql.AppendLine().Append("WHERE ");
                this.Visit(selectExpression.Predicate);
            }

            if (getRows && isSqlOutputClauseUsed)
            {
                this.Sql.AppendLine();
                this.GenerateReturningClause();
                this.Sql.AppendLine();
            }

            return;
        }

        throw new InvalidOperationException(
            RelationalStrings.ExecuteOperationWithUnsupportedOperatorInSqlGeneration(
                nameof(this.VisitDelete)));
    }

    /// <summary>
    /// Generates SQL for an Insert expression.
    /// </summary>
    /// <param name="updateExpression">The update expression.</param>
    /// <param name="getRow">A value indicating whether the SQL should return the inserted row.</param>
    private void VisitInsert(UpdateExpression updateExpression, bool getRow)
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
                IEntityType entityType = updateExpression.Table.GetEntityType();
                bool isSqlReturningClauseUsed = entityType.IsSqlReturningClauseUsed();

                if (isSqlReturningClauseUsed)
                {
                    this.GenerateReturningClause();
                    this.Sql.AppendLine();
                }
                else
                {
                    string columnName = entityType.GetPrimaryKeyColumnName();

                    this.Sql.AppendLine(";");
                    this.Sql.AppendLine($"SELECT * FROM {this.Dependencies.SqlGenerationHelper.DelimitIdentifier(updateExpression.Table.Name)}");
                    this.Sql.AppendLine($"WHERE {this.Dependencies.SqlGenerationHelper.DelimitIdentifier(columnName)} = LAST_INSERT_ROWID()");
                }
            }

            return;
        }

        throw new InvalidOperationException(
            RelationalStrings.ExecuteOperationWithUnsupportedOperatorInSqlGeneration(
                nameof(this.VisitInsert)));
    }

    /// <summary>
    /// Generates SQL for an Update expression.
    /// </summary>
    /// <param name="updateExpression">The update expression.</param>
    /// <param name="getRows">A value indicating whether the SQL should return the updated rows.</param>
    private void VisitUpdate(UpdateExpression updateExpression, bool getRows)
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

            if (getRows)
            {
                IEntityType entityType = updateExpression.Table.GetEntityType();
                bool isSqlReturningClauseUsed = entityType.IsSqlReturningClauseUsed();

                if (isSqlReturningClauseUsed)
                {
                    this.Sql.AppendLine();
                    this.GenerateReturningClause();
                    this.Sql.AppendLine();
                }
                else
                {
                    this.Sql.AppendLine(";");
                    this.GenerateSelectModifiedRows(selectExpression);
                }
            }

            return;
        }

        throw new InvalidOperationException(
            RelationalStrings.ExecuteOperationWithUnsupportedOperatorInSqlGeneration(
                nameof(this.VisitUpdate)));
    }

    /// <summary>
    /// Generates a RETURNING clause.
    /// </summary>
    private void GenerateReturningClause()
    {
        this.Sql.Append("RETURNING *");
    }

    /// <summary>
    /// Generates a SELECT statement to retrieve modified rows.
    /// </summary>
    /// <param name="selectExpression">The select expression used by the query.</param>
    private void GenerateSelectModifiedRows(SelectExpression selectExpression)
    {
        SelectExpression selectModifiedRowsExpression = selectExpression.Update(
            tables: selectExpression.Tables,
            predicate: selectExpression.Predicate,
            groupBy: selectExpression.GroupBy,
            having: selectExpression.Having,
            projections: [new ProjectionExpression(new SqlFragmentExpression("*"), string.Empty)],
            orderings: selectExpression.Orderings,
            offset: selectExpression.Offset,
            selectExpression.Limit);

        this.Visit(selectModifiedRowsExpression);
    }

    #endregion private methods
}
