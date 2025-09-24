// Copyright ©2021-2025 Mike King.
// This file is licensed to you under the MIT license.
// See the License.txt file in the solution root for more information.
//
// This code contains parts adapted from the EF Core source code which is licensed under the Apache license.
// See https://github.com/dotnet/efcore

using DotDoc.EntityFrameworkCore.Extensions.Constants;
using DotDoc.EntityFrameworkCore.Extensions.Utilities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;
using Microsoft.EntityFrameworkCore.SqlServer.Infrastructure.Internal;
using Microsoft.EntityFrameworkCore.SqlServer.Query.Internal;
using Microsoft.EntityFrameworkCore.Storage;
using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;
using System.Reflection;

namespace DotDoc.EntityFrameworkCore.Extensions.CustomQueryGenerators;

/// <summary>
/// SQL Server Custom Query Generator.
/// </summary>
[SuppressMessage("Usage", "EF1001:Internal EF Core API usage.", Justification = "We need to replace query generator with a custom one.")]
[SuppressMessage("Major Code Smell", "S3011:Reflection should not be used to increase accessibility of classes, methods, or fields", Justification = "Need to access _withinTable in base class.")]
internal sealed class SqlServerCustomQueryGenerator : SqlServerQuerySqlGenerator
{
    #region private fields

    /// <summary>
    /// <see cref="FieldInfo"/> for private _withinTable bool used in base class.
    /// </summary>
    private static readonly FieldInfo WithinTableField =
        typeof(SqlServerQuerySqlGenerator).GetField("_withinTable", BindingFlags.Instance | BindingFlags.NonPublic)!;

    /// <summary>
    /// Query table hints (if any).
    /// </summary>
    private string? _tableHintsText;

    #endregion private fields

    #region public constructor

    /// <summary>
    /// Initializes a new instance of the <see cref="SqlServerCustomQueryGenerator"/> class.
    /// </summary>
    /// <param name="dependencies">Dependencies.</param>
    /// <param name="typeMappingSource">Type mapping source.</param>
    /// <param name="sqlServerSingletonOptions">SQL Server Singleton options.</param>
    public SqlServerCustomQueryGenerator(QuerySqlGeneratorDependencies dependencies, IRelationalTypeMappingSource typeMappingSource, ISqlServerSingletonOptions sqlServerSingletonOptions)
        : base(dependencies, typeMappingSource, sqlServerSingletonOptions)
    {
    }

    #endregion public constructor

    #region protected methods

    /// <inheritdoc/>
    protected override void GenerateRootCommand(Expression queryExpression)
    {
        TagCollection tagCollection = GetTagCollection(queryExpression);
        tagCollection.TryGetValue(TagNames.TableHint, out this._tableHintsText);

        switch (queryExpression)
        {
            case DeleteExpression deleteExpression when tagCollection.ContainsTag(TagNames.ExecuteDelete):
                this.GenerateTagsHeaderComment(deleteExpression.Tags);
                this.VisitDeleteGetRows(deleteExpression);
                break;

            case DeleteExpression deleteExpression:
                this.GenerateTagsHeaderComment(deleteExpression.Tags);
                this.VisitDelete(deleteExpression);
                break;

            case UpdateExpression updateExpression when tagCollection.ContainsTag(TagNames.ExecuteInsert):
                this.GenerateTagsHeaderComment(updateExpression.Tags);
                this.VisitInsertGetRows(updateExpression);
                break;

            case UpdateExpression updateExpression when tagCollection.ContainsTag(TagNames.ExecuteUpdate):
                this.GenerateTagsHeaderComment(updateExpression.Tags);
                this.VisitUpdateGetRows(updateExpression);
                break;

            case UpdateExpression updateExpression:
                this.GenerateTagsHeaderComment(updateExpression.Tags);
                this.VisitUpdate(updateExpression);
                break;

            default:
                base.GenerateRootCommand(queryExpression);
                break;
        }
    }

    /// <inheritdoc/>
    protected override Expression VisitTable(TableExpression tableExpression)
    {
        Expression expression = base.VisitTable(tableExpression);
        this.GenerateTableHints();
        return expression;
    }

    #endregion protected methods

    #region private methods

    /// <summary>
    /// Get the tags from an expression.
    /// </summary>
    /// <param name="expression">The expression.</param>
    /// <returns><see cref="TagCollection"/>.</returns>
    private static TagCollection GetTagCollection(Expression expression)
    {
        ISet<string> tags = expression switch
        {
            DeleteExpression deleteExpression => deleteExpression.Tags,
            SelectExpression selectExpression => selectExpression.Tags,
            UpdateExpression updateExpression => updateExpression.Tags,
            _ => new HashSet<string>()
        };

        return new(tags);
    }

    /// <summary>
    /// Generates SQL for a Delete expression.
    /// (Based on the EF Core one but inserts table hints and returns output).
    /// </summary>
    /// <param name="deleteExpression">The delete expression.</param>
    private void VisitDeleteGetRows(DeleteExpression deleteExpression)
    {
        SelectExpression selectExpression = deleteExpression.SelectExpression;

        if (selectExpression is
            {
                GroupBy: [],
                Having: null,
                Offset: null,
                Orderings: [],
                Projection: []
            })
        {
            IEntityType entityType = deleteExpression.Table.GetEntityType();
            bool isSqlOutputClauseUsed = entityType.IsSqlOutputClauseUsed();

            if (!isSqlOutputClauseUsed)
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

            this.Sql.Append("DELETE ");
            this.GenerateTop(selectExpression);

            this.SetWithinTable(true);
            this.Sql.AppendLine($"FROM {this.Dependencies.SqlGenerationHelper.DelimitIdentifier(deleteExpression.Table.Alias)}");

            if (isSqlOutputClauseUsed)
            {
                this.Sql.AppendLine("OUTPUT DELETED.*");
            }

            this.Sql.Append("FROM ");
            this.GenerateList(selectExpression.Tables, e => this.Visit(e), sql => sql.AppendLine());
            this.SetWithinTable(false);

            if (selectExpression.Predicate is not null)
            {
                this.Sql.AppendLine().Append("WHERE ");

                this.Visit(selectExpression.Predicate);
            }

            this.GenerateLimitOffset(selectExpression);

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
    private void VisitInsertGetRows(UpdateExpression updateExpression)
    {
        SelectExpression selectExpression = updateExpression.SelectExpression;

        if (selectExpression is
            {
                GroupBy: [],
                Having: null,
                Offset: null,
                Orderings: [],
                Predicate: null,
                Projection: [],
                Tables: [TableExpressionBase table]
            }

            && table.Equals(updateExpression.Table))
        {
            this.Sql.Append("INSERT ");

            this.GenerateTop(selectExpression);

            this.Sql.Append($"INTO {this.Dependencies.SqlGenerationHelper.DelimitIdentifier(updateExpression.Table.Name, updateExpression.Table.Schema)}");

            this.GenerateTableHints();

            this.Sql.AppendLine();
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

                        this.Sql.Append(this.Dependencies.SqlGenerationHelper.DelimitIdentifier(updateExpression.ColumnValueSetters[i].Column.Name));
                    }

                    this.Sql.AppendLine();
                }
            }

            this.Sql.AppendLine(")");

            IEntityType entityType = updateExpression.Table.GetEntityType();
            bool isSqlOutputClauseUsed = entityType.IsSqlOutputClauseUsed();

            if (isSqlOutputClauseUsed)
            {
                this.Sql.AppendLine("OUTPUT INSERTED.*");
            }

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

            if (!isSqlOutputClauseUsed)
            {
                string columnName = entityType.GetPrimaryKeyColumnName();

                this.Sql.AppendLine(";");
                this.Sql.AppendLine($"SELECT * FROM {this.Dependencies.SqlGenerationHelper.DelimitIdentifier(updateExpression.Table.Name, updateExpression.Table.Schema)}");
                this.Sql.AppendLine($"WHERE {this.Dependencies.SqlGenerationHelper.DelimitIdentifier(columnName)} = SCOPE_IDENTITY()");
            }

            return;
        }

        throw new InvalidOperationException(
            RelationalStrings.ExecuteOperationWithUnsupportedOperatorInSqlGeneration(
                nameof(this.VisitInsertGetRows)));
    }

    /// <summary>
    /// Generates SQL for an Update expression.
    /// (Based on the EF Core one but inserts table hints and returns output).
    /// </summary>
    private void VisitUpdateGetRows(UpdateExpression updateExpression)
    {
        SelectExpression selectExpression = updateExpression.SelectExpression;

        if (selectExpression is
            {
                GroupBy: [],
                Having: null,
                Offset: null,
                Orderings: [],
                Projection: []
            })
        {
            this.Sql.Append("UPDATE ");
            this.GenerateTop(selectExpression);

            this.Sql.AppendLine($"{this.Dependencies.SqlGenerationHelper.DelimitIdentifier(updateExpression.Table.Alias)}");
            this.Sql.Append("SET ");
            this.Visit(updateExpression.ColumnValueSetters[0].Column);
            this.Sql.Append(" = ");
            this.Visit(updateExpression.ColumnValueSetters[0].Value);

            using (this.Sql.Indent())
            {
                foreach (var columnValueSetter in updateExpression.ColumnValueSetters.Skip(1))
                {
                    this.Sql.AppendLine(",");
                    this.Visit(columnValueSetter.Column);
                    this.Sql.Append(" = ");
                    this.Visit(columnValueSetter.Value);
                }
            }

            this.Sql.AppendLine();

            IEntityType entityType = updateExpression.Table.GetEntityType();
            bool isSqlOutputClauseUsed = entityType.IsSqlOutputClauseUsed();

            if (isSqlOutputClauseUsed)
            {
                this.Sql.AppendLine("OUTPUT INSERTED.*");
            }

            this.SetWithinTable(true);
            this.Sql.Append("FROM ");
            this.GenerateList(selectExpression.Tables, e => this.Visit(e), sql => sql.AppendLine());
            this.SetWithinTable(false);

            if (selectExpression.Predicate is not null)
            {
                this.Sql.AppendLine().Append("WHERE ");
                this.Visit(selectExpression.Predicate);
            }

            if (!isSqlOutputClauseUsed)
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

    /// <summary>
    /// Process a list of items.
    /// </summary>
    /// <typeparam name="T">The item type.</typeparam>
    /// <param name="items">The items.</param>
    /// <param name="generationAction">The action to perform on each action.</param>
    /// <param name="joinAction">The action to join one item to another.</param>
    private void GenerateList<T>(IReadOnlyList<T> items, Action<T> generationAction, Action<IRelationalCommandBuilder> joinAction)
    {
        for (int i = 0; i < items.Count; i++)
        {
            if (i > 0)
            {
                joinAction(this.Sql);
            }

            generationAction(items[i]);
        }
    }

    /// <summary>
    /// Set the private _withinTable value in the base class.
    /// </summary>
    /// <param name="value">The value.</param>
    private void SetWithinTable(bool value)
    {
        WithinTableField.SetValue(this, value);
    }

    /// <summary>
    /// Generate a WITH clause containing a list of table hints.
    /// </summary>
    private void GenerateTableHints()
    {
        if (!string.IsNullOrEmpty(this._tableHintsText))
        {
            this.Sql.Append($" WITH ({this._tableHintsText})");
        }
    }

    #endregion private methods
}

