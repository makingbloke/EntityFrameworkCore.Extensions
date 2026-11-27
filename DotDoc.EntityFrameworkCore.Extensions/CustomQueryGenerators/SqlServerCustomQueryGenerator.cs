// Copyright ©2021-2026 Mike King.
// This file is licensed to you under the MIT license.
// See the License.txt file in the solution root for more information.

// This code contains parts adapted from the EF Core source code which is licensed under the Apache license.
// See https://github.com/dotnet/efcore
using DotDoc.EntityFrameworkCore.Extensions.ExecuteUpdate;
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
    /// The name of the entity type an insert is for (if any).
    /// </summary>
    private string? _entityTypeName;

    /// <summary>
    /// Query type (if any).
    /// </summary>
    private QueryType _queryType;

    /// <summary>
    /// Table hints for each table, keyed by <see cref="CustomTagExtensions.GetTableKey(string, string?)"/>.
    /// </summary>
    private IReadOnlyDictionary<string, string> _tableHints = new Dictionary<string, string>();

    #endregion private fields

    #region public constructors

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

    #endregion public constructors

    #region protected methods

    /// <inheritdoc/>
    /// <exception cref="InvalidOperationException">The query has more than one execute update tag, or different table hints for the same table (e.g. <c>WithTableHints</c> was called twice for it).</exception>
    /// <remarks>
    /// The base class calls this before <see cref="VisitDelete(DeleteExpression)"/> / <see cref="VisitUpdate(UpdateExpression)"/>,
    /// so the query type and table hints are known by the time they run.
    /// </remarks>
    protected override void GenerateTagsHeaderComment(ISet<string> tags)
    {
        CustomTags customTags = CustomTagExtensions.GetCustomTags(tags);

        this._queryType = customTags.QueryType;
        this._entityTypeName = customTags.EntityTypeName;
        this._tableHints = customTags.TableHints;

        base.GenerateTagsHeaderComment(customTags.RemainingTags);
    }

    /// <inheritdoc/>
    /// <exception cref="InvalidOperationException">For <c>ExecuteDeleteGetRowsAsync</c>, the query uses an operator that can't be used in the delete.</exception>
    /// <exception cref="NotSupportedException">For <c>ExecuteDeleteGetRowsAsync</c>, the table is configured with <c>UseSqlOutputClause(false)</c>.</exception>
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
    protected override Expression VisitTable(TableExpression tableExpression)
    {
        Expression expression = base.VisitTable(tableExpression);
        this.GenerateTableHints(tableExpression);
        return expression;
    }

    /// <inheritdoc/>
    /// <exception cref="InvalidOperationException">For <c>ExecuteInsertAsync</c> or <c>ExecuteInsertGetRowAsync</c>, the query is more than the table; for <c>ExecuteUpdateGetRowsAsync</c>, the query uses an operator that can't be used in the update.</exception>
    /// <exception cref="NotSupportedException">For <c>ExecuteInsertGetRowAsync</c> or <c>ExecuteUpdateGetRowsAsync</c>, the table is configured with <c>UseSqlOutputClause(false)</c>.</exception>
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
    /// Throws if the query of a delete that returns its rows uses an operator SQL Server can't have in a DELETE.
    /// </summary>
    /// <param name="deleteExpression">The delete expression.</param>
    /// <exception cref="InvalidOperationException">The query has a GROUP BY, HAVING, OFFSET, ORDER BY or projection.</exception>
    private static void ThrowIfDeleteOperatorNotSupported(DeleteExpression deleteExpression)
    {
        // The same checks as EF Core's own SQL Server generator. EF Core moves anything SQL Server can't have in a DELETE into a subquery
        // before the SQL is generated, so this would be an internal bug, which EF Core's message says.
        if (deleteExpression.SelectExpression is not
            {
                GroupBy: [],
                Having: null,
                Offset: null,
                Orderings: [],
                Projection: []
            })
        {
            throw new InvalidOperationException(RelationalStrings.ExecuteOperationWithUnsupportedOperatorInSqlGeneration(nameof(ExecuteUpdateExtensions.ExecuteDeleteGetRowsAsync)));
        }
    }

    /// <summary>
    /// Throws if the query that <see cref="ExecuteUpdateExtensions.ExecuteInsertAsync"/> or <see cref="ExecuteUpdateExtensions.ExecuteInsertGetRowAsync"/>
    /// was called on is more than the table (e.g. it has a <c>Where</c> or <c>OrderBy</c>).
    /// </summary>
    /// <param name="updateExpression">The update expression the insert is generated from.</param>
    /// <param name="entityType">The entity type of the row being inserted.</param>
    /// <param name="methodName">The name of the extension method being called, used in the exception message.</param>
    /// <exception cref="InvalidOperationException">The query is more than the table.</exception>
    private static void ThrowIfInsertSourceNotTable(UpdateExpression updateExpression, IEntityType entityType, string methodName)
    {
        // An INSERT has nowhere to put anything else, so the query must select from the table being inserted into and nothing more. The only
        // predicate allowed is the filter on the discriminator that EF Core adds to a query on a derived type (table-per-hierarchy).
        if (updateExpression.SelectExpression is not
            {
                GroupBy: [],
                Having: null,
                Limit: null,
                Offset: null,
                Orderings: [],
                Projection: [],
                Tables: [TableExpressionBase table]
            }

            || !table.Equals(updateExpression.Table)
            || (updateExpression.SelectExpression.Predicate is not null && !updateExpression.IsDiscriminatorFilter(entityType)))
        {
            throw new InvalidOperationException($"{methodName} inserts a row into the table it's called on ({updateExpression.Table.Name}), so it must be called on the table itself without Where, OrderBy or any other query operator.");
        }
    }

    /// <summary>
    /// Throws if the query of an update that returns its rows uses an operator SQL Server can't have in an UPDATE.
    /// </summary>
    /// <param name="updateExpression">The update expression.</param>
    /// <exception cref="InvalidOperationException">The query has a GROUP BY, HAVING, OFFSET, ORDER BY or projection.</exception>
    private static void ThrowIfUpdateOperatorNotSupported(UpdateExpression updateExpression)
    {
        // The same checks as EF Core's own SQL Server generator. EF Core moves anything SQL Server can't have in an UPDATE into a subquery
        // before the SQL is generated, so this would be an internal bug, which EF Core's message says.
        if (updateExpression.SelectExpression is not
            {
                GroupBy: [],
                Having: null,
                Offset: null,
                Orderings: [],
                Projection: []
            })
        {
            throw new InvalidOperationException(RelationalStrings.ExecuteOperationWithUnsupportedOperatorInSqlGeneration(nameof(ExecuteUpdateExtensions.ExecuteUpdateGetRowsAsync)));
        }
    }

    /// <summary>
    /// Throws a <see cref="NotSupportedException"/> if a table is configured not to use the OUTPUT clause.
    /// </summary>
    /// <param name="tableExpression">The table being modified.</param>
    /// <param name="methodName">The name of the extension method being called, used in the exception message.</param>
    /// <exception cref="NotSupportedException">The table is configured with <c>UseSqlOutputClause(false)</c>.</exception>
    private static void ThrowIfOutputClauseNotUsed(TableExpression tableExpression, string methodName)
    {
        if (!tableExpression.GetEntityType().IsSqlOutputClauseUsed())
        {
            throw new NotSupportedException($"{methodName} requires the OUTPUT clause, but the table {tableExpression.Name} is configured with UseSqlOutputClause(false).");
        }
    }

    /// <summary>
    /// Generates SQL for a Delete expression that returns the deleted rows.
    /// </summary>
    /// <param name="deleteExpression">The delete expression.</param>
    /// <exception cref="InvalidOperationException">The query uses an operator that can't be used in the delete.</exception>
    /// <exception cref="NotSupportedException">The table is configured with <c>UseSqlOutputClause(false)</c>.</exception>
    private void GenerateDeleteGetRows(DeleteExpression deleteExpression)
    {
        SelectExpression selectExpression = deleteExpression.SelectExpression;

        ThrowIfDeleteOperatorNotSupported(deleteExpression);
        ThrowIfOutputClauseNotUsed(deleteExpression.Table, nameof(ExecuteUpdateExtensions.ExecuteDeleteGetRowsAsync));

        this.Sql.Append("DELETE ");
        this.GenerateTop(selectExpression);

        this.SetWithinTable(true);
        this.Sql.AppendLine($"FROM {this.Dependencies.SqlGenerationHelper.DelimitIdentifier(deleteExpression.Table.Alias)}");

        this.GenerateOutputClause(false);
        this.Sql.AppendLine();

        this.Sql.Append("FROM ");
        this.GenerateList(selectExpression.Tables, e => this.Visit(e), sql => sql.AppendLine());
        this.SetWithinTable(false);

        if (selectExpression.Predicate is not null)
        {
            this.Sql.AppendLine().Append("WHERE ");
            this.Visit(selectExpression.Predicate);
        }

        this.GenerateLimitOffset(selectExpression);
    }

    /// <summary>
    /// Generates SQL for an Insert expression.
    /// </summary>
    /// <param name="updateExpression">The update expression.</param>
    /// <param name="getRow">A value indicating whether the SQL should return the inserted row.</param>
    /// <exception cref="InvalidOperationException">The query is more than the table (e.g. it has a <c>Where</c> or <c>OrderBy</c>).</exception>
    /// <exception cref="NotSupportedException"><paramref name="getRow"/> is <see langword="true"/> and the table is configured with <c>UseSqlOutputClause(false)</c>.</exception>
    private void GenerateInsert(UpdateExpression updateExpression, bool getRow)
    {
        string methodName = getRow
            ? nameof(ExecuteUpdateExtensions.ExecuteInsertGetRowAsync)
            : nameof(ExecuteUpdateExtensions.ExecuteInsertAsync);

        IEntityType entityType = updateExpression.Table.GetEntityType(this._entityTypeName);

        ThrowIfInsertSourceNotTable(updateExpression, entityType, methodName);

        if (getRow)
        {
            ThrowIfOutputClauseNotUsed(updateExpression.Table, methodName);
        }

        IReadOnlyList<ColumnValueSetter> setters = updateExpression.GetDiscriminatorSetter(entityType) is { } discriminatorSetter
            ? [.. updateExpression.ColumnValueSetters, discriminatorSetter]
            : updateExpression.ColumnValueSetters;

        this.Sql.Append($"INSERT INTO {this.Dependencies.SqlGenerationHelper.DelimitIdentifier(updateExpression.Table.Name, updateExpression.Table.Schema)}");

        this.GenerateTableHints(updateExpression.Table);

        this.Sql.AppendLine();
        this.Sql.AppendLine("(");

        using (_ = this.Sql.Indent())
        {
            for (int i = 0; i < setters.Count; i++)
            {
                ColumnValueSetter setter = setters[i];

                if (i > 0)
                {
                    this.Sql.AppendLine(",");
                }

                this.Visit(setter.Column);
            }

            this.Sql.AppendLine();
        }

        this.Sql.AppendLine(")");

        if (getRow)
        {
            this.GenerateOutputClause(true);
            this.Sql.AppendLine();
        }

        this.Sql.AppendLine("VALUES");
        this.Sql.AppendLine("(");

        using (_ = this.Sql.Indent())
        {
            for (int i = 0; i < setters.Count; i++)
            {
                ColumnValueSetter setter = setters[i];

                if (i > 0)
                {
                    this.Sql.AppendLine(",");
                }

                this.Visit(setter.Value);
            }

            this.Sql.AppendLine();
        }

        this.Sql.AppendLine(")");
    }

    /// <summary>
    /// Generates SQL for an Update expression that returns the updated rows.
    /// </summary>
    /// <param name="updateExpression">The update expression.</param>
    /// <exception cref="InvalidOperationException">The query uses an operator that can't be used in the update.</exception>
    /// <exception cref="NotSupportedException">The table is configured with <c>UseSqlOutputClause(false)</c>.</exception>
    private void GenerateUpdateGetRows(UpdateExpression updateExpression)
    {
        SelectExpression selectExpression = updateExpression.SelectExpression;

        ThrowIfUpdateOperatorNotSupported(updateExpression);
        ThrowIfOutputClauseNotUsed(updateExpression.Table, nameof(ExecuteUpdateExtensions.ExecuteUpdateGetRowsAsync));

        this.Sql.Append("UPDATE ");
        this.GenerateTop(selectExpression);

        this.Sql.AppendLine($"{this.Dependencies.SqlGenerationHelper.DelimitIdentifier(updateExpression.Table.Alias)}");
        this.Sql.AppendLine("SET ");

        using (_ = this.Sql.Indent())
        {
            for (int i = 0; i < updateExpression.ColumnValueSetters.Count; i++)
            {
                ColumnValueSetter setter = updateExpression.ColumnValueSetters[i];

                if (i > 0)
                {
                    this.Sql.AppendLine(",");
                }

                // SQL Server 2025 modify method (https://learn.microsoft.com/sql/t-sql/data-types/json-data-type#modify-method)
                // This requires special handling since modify isn't a standard setter of the form SET x = y, but rather just
                // SET [x].modify(...).
                if (setter.Value is SqlFunctionExpression
                    {
                        Name: "modify",
                        IsBuiltIn: true,
                        Instance: ColumnExpression { TypeMapping.StoreType: "json" }
                    })
                {
                    this.Visit(setter.Value);
                }
                else
                {
                    this.Visit(setter.Column);
                    this.Sql.Append(" = ");
                    this.Visit(setter.Value);
                }
            }

            this.Sql.AppendLine();
        }

        this.GenerateOutputClause(true);
        this.Sql.AppendLine();

        this.SetWithinTable(true);
        this.Sql.Append("FROM ");
        this.GenerateList(selectExpression.Tables, e => this.Visit(e), sql => sql.AppendLine());
        this.SetWithinTable(false);

        if (selectExpression.Predicate is not null)
        {
            this.Sql.AppendLine().Append("WHERE ");
            this.Visit(selectExpression.Predicate);
        }
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
    /// Generate a WITH clause containing the table hints for a table, if it has any.
    /// </summary>
    /// <param name="tableExpression">The table.</param>
    private void GenerateTableHints(TableExpression tableExpression)
    {
        if (this._tableHints.TryGetValue(CustomTagExtensions.GetTableKey(tableExpression.Name, tableExpression.Schema), out string? tableHints))
        {
            this.Sql.Append($" WITH ({tableHints})");
        }
    }

    /// <summary>
    /// Generates an OUTPUT clause.
    /// </summary>
    /// <param name="useInsertedColumns">If <see langword="true"/> return inserted column values else deleted.</param>
    private void GenerateOutputClause(bool useInsertedColumns)
    {
        this.Sql.Append($"OUTPUT {(useInsertedColumns ? "INSERTED" : "DELETED")}.*");
    }

    #endregion private methods
}

