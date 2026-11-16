// Copyright ©2021-2026 Mike King.
// This file is licensed to you under the MIT license.
// See the License.txt file in the solution root for more information.

using DotDoc.EntityFrameworkCore.Extensions.CustomQueryGenerators;
using DotDoc.EntityFrameworkCore.Extensions.DatabaseType;
using DotDoc.EntityFrameworkCore.Extensions.TableHints;
using DotDoc.EntityFrameworkCore.Extensions.Utilities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Sqlite.Query.Internal;
using Microsoft.EntityFrameworkCore.SqlServer.Query.Internal;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.DependencyInjection;
using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;

namespace DotDoc.EntityFrameworkCore.Extensions.ExecuteUpdate;

/// <summary>
/// Execute Update Extensions.
/// </summary>
public static partial class ExecuteUpdateExtensions
{
    #region public UseExecuteUpdateExtensions methods

    /// <summary>
    /// Use the Execute Update Extensions.
    /// </summary>
    /// <param name="optionsBuilder">The builder being used to configure the context.</param>
    /// <returns>The same builder instance so multiple calls can be chained.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="optionsBuilder"/> is <see langword="null"/>.</exception>
    /// <exception cref="UnsupportedDatabaseTypeException">The database provider is not SQLite or SQL Server.</exception>
    public static DbContextOptionsBuilder UseExecuteUpdateExtensions(this DbContextOptionsBuilder optionsBuilder)
    {
        ArgumentNullException.ThrowIfNull(optionsBuilder);

        ((IDbContextOptionsBuilderInfrastructure)optionsBuilder)
            .AddOrUpdateExtension(new ExecuteUpdateOptionsExtension());

        optionsBuilder
            .UseCustomQueryGenerator()
            .AddInterceptors(ExecuteUpdateInterceptor.Instance);

        return optionsBuilder;
    }

    #endregion public UseExecuteUpdateExtensions methods

    #region public ExecuteDeleteGetCountAsync methods

    /// <summary>
    /// Deletes all database rows for the entity instances which match the LINQ query from the database.
    /// </summary>
    /// <typeparam name="TSource">The type of the elements of <paramref name="source" />.</typeparam>
    /// <param name="source">An <see cref="IQueryable{TSource}" /> whose elements to test for a condition.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
    /// <returns>The number of rows deleted in the database.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="source"/> is <see langword="null"/>.</exception>
    public static Task<int> ExecuteDeleteGetCountAsync<TSource>(this IQueryable<TSource> source, CancellationToken cancellationToken = default)
        where TSource : class
    {
        ArgumentNullException.ThrowIfNull(source);

        // Just a wrapper around ExecuteDeleteAsync for consistency.
        return source.ExecuteDeleteAsync(cancellationToken);
    }

    #endregion public ExecuteDeleteGetCountAsync methods

    #region public ExecuteDeleteGetRowsAsync methods

    /// <summary>
    /// Deletes all database rows for the entity instances which match the LINQ query from the database.
    /// </summary>
    /// <typeparam name="TSource">The type of the elements of <paramref name="source" />.</typeparam>
    /// <param name="source">An <see cref="IQueryable{TSource}" /> whose elements to test for a condition.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
    /// <returns>An <see cref="IList{TSource}"/> containing the deleted rows.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="source"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentException"><paramref name="source"/> is not an EF Core query.</exception>
    /// <exception cref="InvalidOperationException"><see cref="UseExecuteUpdateExtensions"/> was not called when configuring the DbContext, another extension library has replaced its query generator, another ExecuteUpdate call is in progress on the DbContext, or the query uses an operator that can't be used in a delete.</exception>
    /// <exception cref="NotSupportedException">The table is configured with <c>UseSqlOutputClause(false)</c> (SQL Server) or <c>UseSqlReturningClause(false)</c> (SQLite).</exception>
    public static async Task<IList<TSource>> ExecuteDeleteGetRowsAsync<TSource>(this IQueryable<TSource> source, CancellationToken cancellationToken = default)
        where TSource : class
    {
        ArgumentNullException.ThrowIfNull(source);

        DbContext context = source.GetDbContext();
        ExecuteUpdateState state = GetExecuteUpdateState(context, nameof(ExecuteDeleteGetRowsAsync));

        if (state.QueryType != QueryType.Unknown)
        {
            throw new InvalidOperationException("Another ExecuteUpdate call is already in progress on this DbContext.");
        }

        state.SetQueryType(QueryType.DeleteGetRows);

        try
        {
            // Execute the delete and capture SQL and parameters (This just generates the SQL, it is not executed).
            await source
                .TagWithExecuteUpdate(QueryType.DeleteGetRows)
                .ExecuteDeleteAsync(cancellationToken)
                .ConfigureAwait(false);

            if (state.Sql is null || state.Parameters is null)
            {
                throw new InvalidOperationException("The SQL for the delete was not captured.");
            }

            // Execute the delete and get the deleted rows.
            List<TSource> results = await context.Database.SqlQueryRaw<TSource>(state.Sql, state.Parameters)
                .AsNoTracking()
                .ToListAsync(cancellationToken)
                .ConfigureAwait(false);

            return results;
        }
        finally
        {
            await state.ResetStateAsync(CancellationToken.None).ConfigureAwait(false);
        }
    }

    #endregion public ExecuteDeleteGetRowsAsync methods

    #region public ExecuteInsertAsync methods

    /// <summary>
    /// Insert a database row.
    /// </summary>
    /// <typeparam name="TSource">The type of the elements of <paramref name="source" />.</typeparam>
    /// <param name="source">An <see cref="IQueryable{TSource}" /> whose elements to test for a condition.</param>
    /// <param name="setPropertyCalls">A method containing set property statements specifying properties to update.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
    /// <returns>A <see cref="Task"/> that represents the asynchronous operation.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="source"/> or <paramref name="setPropertyCalls"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentException"><paramref name="source"/> is not an EF Core query.</exception>
    /// <exception cref="InvalidOperationException"><see cref="UseExecuteUpdateExtensions"/> was not called when configuring the DbContext, another extension library has replaced its query generator, or the query is more than the table (e.g. it has a <c>Where</c> or <c>OrderBy</c>).</exception>
    public static async Task ExecuteInsertAsync<TSource>(this IQueryable<TSource> source, Action<UpdateSettersBuilder<TSource>> setPropertyCalls, CancellationToken cancellationToken = default)
        where TSource : class
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(setPropertyCalls);

        _ = GetExecuteUpdateState(source.GetDbContext(), nameof(ExecuteInsertAsync));

        // The tag tells the custom query generator to generate an INSERT rather than an UPDATE.
        await source
            .TagWithExecuteUpdate(QueryType.Insert)
            .ExecuteUpdateAsync(
                setPropertyCalls,
                cancellationToken)
            .ConfigureAwait(false);
    }

    #endregion public ExecuteInsertAsync methods

    #region public ExecuteInsertGetRowAsync methods

    /// <summary>
    /// Insert a database row.
    /// </summary>
    /// <typeparam name="TSource">The type of the elements of <paramref name="source" />.</typeparam>
    /// <param name="source">An <see cref="IQueryable{TSource}" /> whose elements to test for a condition.</param>
    /// <param name="setPropertyCalls">A method containing set property statements specifying properties to update.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
    /// <returns>An instance of the <typeparamref name="TSource"/> containing the inserted row.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="source"/> or <paramref name="setPropertyCalls"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentException"><paramref name="source"/> is not an EF Core query.</exception>
    /// <exception cref="InvalidOperationException"><see cref="UseExecuteUpdateExtensions"/> was not called when configuring the DbContext, another extension library has replaced its query generator, another ExecuteUpdate call is in progress on the DbContext, or the query is more than the table (e.g. it has a <c>Where</c> or <c>OrderBy</c>).</exception>
    /// <exception cref="NotSupportedException">The table is configured with <c>UseSqlOutputClause(false)</c> (SQL Server) or <c>UseSqlReturningClause(false)</c> (SQLite).</exception>
    public static async Task<TSource> ExecuteInsertGetRowAsync<TSource>(this IQueryable<TSource> source, Action<UpdateSettersBuilder<TSource>> setPropertyCalls, CancellationToken cancellationToken = default)
        where TSource : class
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(setPropertyCalls);

        DbContext context = source.GetDbContext();
        ExecuteUpdateState state = GetExecuteUpdateState(context, nameof(ExecuteInsertGetRowAsync));

        if (state.QueryType != QueryType.Unknown)
        {
            throw new InvalidOperationException("Another ExecuteUpdate call is already in progress on this DbContext.");
        }

        state.SetQueryType(QueryType.InsertGetRow);

        try
        {
            // Execute the insert and capture SQL and parameters (This just generates the SQL, it is not executed).
            await source
                .TagWithExecuteUpdate(QueryType.InsertGetRow)
                .ExecuteUpdateAsync(
                    setPropertyCalls,
                    cancellationToken)
                .ConfigureAwait(false);

            if (state.Sql is null || state.Parameters is null)
            {
                throw new InvalidOperationException("The SQL for the insert was not captured.");
            }

            // Execute the insert and get the inserted rows.
            List<TSource> results = await context.Database.SqlQueryRaw<TSource>(state.Sql, state.Parameters)
                .AsNoTracking()
                .ToListAsync(cancellationToken)
                .ConfigureAwait(false);

            if (results.Count != 1)
            {
                throw new InvalidOperationException($"Unexpected entry count from insert: {results.Count}");
            }

            return results[0];
        }
        finally
        {
            await state.ResetStateAsync(CancellationToken.None).ConfigureAwait(false);
        }
    }

    #endregion public ExecuteInsertGetRowAsync methods

    #region public ExecuteUpdateGetCountAsync methods

    /// <summary>
    /// Updates all database rows for the entity instances which match the LINQ query from the database.
    /// </summary>
    /// <typeparam name="TSource">The type of the elements of <paramref name="source" />.</typeparam>
    /// <param name="source">An <see cref="IQueryable{TSource}" /> whose elements to test for a condition.</param>
    /// <param name="setPropertyCalls">A method containing set property statements specifying properties to update.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
    /// <returns>The number of rows updated in the database.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="source"/> or <paramref name="setPropertyCalls"/> is <see langword="null"/>.</exception>
    public static Task<int> ExecuteUpdateGetCountAsync<TSource>(this IQueryable<TSource> source, Action<UpdateSettersBuilder<TSource>> setPropertyCalls, CancellationToken cancellationToken = default)
        where TSource : class
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(setPropertyCalls);

        // Just a wrapper around ExecuteUpdateAsync for consistency.
        return source.ExecuteUpdateAsync(setPropertyCalls, cancellationToken);
    }

    #endregion public ExecuteUpdateGetCountAsync methods

    #region public ExecuteUpdateGetRowsAsync methods

    /// <summary>
    /// Updates all database rows for the entity instances which match the LINQ query from the database.
    /// </summary>
    /// <typeparam name="TSource">The type of the elements of <paramref name="source" />.</typeparam>
    /// <param name="source">An <see cref="IQueryable{TSource}" /> whose elements to test for a condition.</param>
    /// <param name="setPropertyCalls">A method containing set property statements specifying properties to update.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
    /// <returns>An <see cref="IList{TSource}"/> containing the updated rows.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="source"/> or <paramref name="setPropertyCalls"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentException"><paramref name="source"/> is not an EF Core query.</exception>
    /// <exception cref="InvalidOperationException"><see cref="UseExecuteUpdateExtensions"/> was not called when configuring the DbContext, another extension library has replaced its query generator, another ExecuteUpdate call is in progress on the DbContext, or the query uses an operator that can't be used in an update.</exception>
    /// <exception cref="NotSupportedException">The table is configured with <c>UseSqlOutputClause(false)</c> (SQL Server) or <c>UseSqlReturningClause(false)</c> (SQLite).</exception>
    public static async Task<IList<TSource>> ExecuteUpdateGetRowsAsync<TSource>(this IQueryable<TSource> source, Action<UpdateSettersBuilder<TSource>> setPropertyCalls, CancellationToken cancellationToken = default)
        where TSource : class
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(setPropertyCalls);

        DbContext context = source.GetDbContext();
        ExecuteUpdateState state = GetExecuteUpdateState(context, nameof(ExecuteUpdateGetRowsAsync));

        if (state.QueryType != QueryType.Unknown)
        {
            throw new InvalidOperationException("Another ExecuteUpdate call is already in progress on this DbContext.");
        }

        state.SetQueryType(QueryType.UpdateGetRows);

        try
        {
            // Execute the update and capture SQL and parameters (This just generates the SQL, it is not executed).
            await source
                .TagWithExecuteUpdate(QueryType.UpdateGetRows)
                .ExecuteUpdateAsync(
                    setPropertyCalls,
                    cancellationToken)
                .ConfigureAwait(false);

            if (state.Sql is null || state.Parameters is null)
            {
                throw new InvalidOperationException("The SQL for the update was not captured.");
            }

            // Execute the update and get the updated rows.
            List<TSource> results = await context.Database.SqlQueryRaw<TSource>(state.Sql, state.Parameters)
                .AsNoTracking()
                .ToListAsync(cancellationToken)
                .ConfigureAwait(false);

            return results;
        }
        finally
        {
            await state.ResetStateAsync(CancellationToken.None).ConfigureAwait(false);
        }
    }

    #endregion public ExecuteUpdateGetRowsAsync methods

    #region public ExecuteUpsertGetCountAsync methods

    /// <summary>
    /// Update the entity instances which match the LINQ query from the database or insert an entry if none exist.
    /// </summary>
    /// <typeparam name="TSource">The type of the elements of <paramref name="source" />.</typeparam>
    /// <param name="source">An <see cref="IQueryable{TSource}" /> whose elements to test for a condition.</param>
    /// <param name="updateSetPropertyCalls">A method containing set property statements specifying properties to update.</param>
    /// <param name="insertSetPropertyCalls">A method containing set property statements specifying properties to insert - if not specified the same properties as the update are used.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
    /// <returns>The number of rows updated / inserted in the database.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="source"/> or <paramref name="updateSetPropertyCalls"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentException"><paramref name="source"/> is not an EF Core query.</exception>
    /// <exception cref="InvalidOperationException"><see cref="UseExecuteUpdateExtensions"/> was not called when configuring the DbContext, another extension library has replaced its query generator, or the query (without its <c>Where</c>) is more than the table, so it can't be used for the insert (e.g. it has an <c>OrderBy</c>).</exception>
    /// <exception cref="NotSupportedException">The query has table hints (<c>WithTableHints</c>) on SQL Server.</exception>
    /// <remarks>
    /// SQL Server supports MERGE which can be used to do an upsert but requires extra work to ensure the Update and Insert are run inside the same transaction.
    /// SQLite does not support MERGE but has an INSERT ... ON CONFLICT DO UPDATE syntax. The problem with this is that it requires the set property
    /// calls to update a value with a unique constraint of some kind to determine if the row exists and any values not set in the update are reset to their
    /// default values. We also want to be able to optionally specify a different set of properties to update and insert for cases where primary keys are
    /// generated manually. So the simplest approach is to enlist in an existing transaction, or create one, then execute the update first and if no rows are
    /// updated then execute the insert. On SQL Server the update uses table hints (HoldLock, UpdLock) to lock the rows matching the query's Where clause (including
    /// the range where a missing row would go) until the transaction ends, so a second upsert for the same row waits instead of also inserting. This means that
    /// table hints in the IQueryable are not supported for upsert on SQL Server, and the columns in the query's Where clause should be indexed, otherwise a larger
    /// range (possibly the whole table) is locked for the duration of the transaction.
    /// </remarks>
    public static async Task<int> ExecuteUpsertGetCountAsync<TSource>(this IQueryable<TSource> source, Action<UpdateSettersBuilder<TSource>> updateSetPropertyCalls, Action<UpdateSettersBuilder<TSource>>? insertSetPropertyCalls = null, CancellationToken cancellationToken = default)
        where TSource : class
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(updateSetPropertyCalls);

        DbContext context = source.GetDbContext();

        ThrowIfTableHintsPresent(source, context, nameof(ExecuteUpsertGetCountAsync));
        _ = GetExecuteUpdateState(context, nameof(ExecuteUpsertGetCountAsync));

        // When we own the transaction, disposing it without committing rolls it back, so no explicit rollback is needed on failure.
        await using IDbContextTransaction? transaction = await BeginTransactionIfRequiredAsync(context, cancellationToken).ConfigureAwait(false);

        int count = await source
            .WithTableHints(SqlServerTableHints.HoldLock, SqlServerTableHints.UpdLock)
            .ExecuteUpdateGetCountAsync(
                updateSetPropertyCalls,
                cancellationToken)
            .ConfigureAwait(false);

        if (count == 0)
        {
            IQueryable<TSource> insertSource = RemoveWhereClauseVisitor.Remove(source);

            await insertSource
                .ExecuteInsertAsync(
                    insertSetPropertyCalls ?? updateSetPropertyCalls,
                    cancellationToken)
                .ConfigureAwait(false);

            count = 1;
        }

        if (transaction is not null)
        {
            await transaction.CommitAsync(cancellationToken).ConfigureAwait(false);
        }

        return count;
    }

    #endregion public ExecuteUpsertGetCountAsync methods

    #region public ExecuteUpsertGetRowsAsync methods

    /// <summary>
    /// Update the entity instances which match the LINQ query from the database or insert an entry if none exist.
    /// </summary>
    /// <typeparam name="TSource">The type of the elements of <paramref name="source" />.</typeparam>
    /// <param name="source">An <see cref="IQueryable{TSource}" /> whose elements to test for a condition.</param>
    /// <param name="updateSetPropertyCalls">A method containing set property statements specifying properties to update.</param>
    /// <param name="insertSetPropertyCalls">A method containing set property statements specifying properties to insert - if not specified the same properties as the update are used.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
    /// <returns>An <see cref="IList{TSource}"/> containing the updated / inserted rows.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="source"/> or <paramref name="updateSetPropertyCalls"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentException"><paramref name="source"/> is not an EF Core query.</exception>
    /// <exception cref="InvalidOperationException"><see cref="UseExecuteUpdateExtensions"/> was not called when configuring the DbContext, another extension library has replaced its query generator, another ExecuteUpdate call is in progress on the DbContext, or the query uses an operator that can't be used in the update or (without its <c>Where</c>) the insert.</exception>
    /// <exception cref="NotSupportedException">The query has table hints (<c>WithTableHints</c>) on SQL Server, or the table is configured with <c>UseSqlOutputClause(false)</c> (SQL Server) or <c>UseSqlReturningClause(false)</c> (SQLite).</exception>
    /// <remarks>
    /// SQL Server supports MERGE which can be used to do an upsert but requires extra work to ensure the Update and Insert are run inside the same transaction.
    /// SQLite does not support MERGE but has an INSERT ... ON CONFLICT DO UPDATE syntax. The problem with this is that it requires the set property
    /// calls to update a value with a unique constraint of some kind to determine if the row exists and any values not set in the update are reset to their
    /// default values. We also want to be able to optionally specify a different set of properties to update and insert for cases where primary keys are
    /// generated manually. So the simplest approach is to enlist in an existing transaction, or create one, then execute the update first and if no rows are
    /// updated then execute the insert. On SQL Server the update uses table hints (HoldLock, UpdLock) to lock the rows matching the query's Where clause (including
    /// the range where a missing row would go) until the transaction ends, so a second upsert for the same row waits instead of also inserting. This means that
    /// table hints in the IQueryable are not supported for upsert on SQL Server, and the columns in the query's Where clause should be indexed, otherwise a larger
    /// range (possibly the whole table) is locked for the duration of the transaction.
    /// </remarks>
    public static async Task<IList<TSource>> ExecuteUpsertGetRowsAsync<TSource>(this IQueryable<TSource> source, Action<UpdateSettersBuilder<TSource>> updateSetPropertyCalls, Action<UpdateSettersBuilder<TSource>>? insertSetPropertyCalls = null, CancellationToken cancellationToken = default)
        where TSource : class
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(updateSetPropertyCalls);

        DbContext context = source.GetDbContext();

        ThrowIfTableHintsPresent(source, context, nameof(ExecuteUpsertGetRowsAsync));
        _ = GetExecuteUpdateState(context, nameof(ExecuteUpsertGetRowsAsync));

        // When we own the transaction, disposing it without committing rolls it back, so no explicit rollback is needed on failure.
        await using IDbContextTransaction? transaction = await BeginTransactionIfRequiredAsync(context, cancellationToken).ConfigureAwait(false);

        IList<TSource> results = await source
            .WithTableHints(SqlServerTableHints.HoldLock, SqlServerTableHints.UpdLock)
            .ExecuteUpdateGetRowsAsync(
                updateSetPropertyCalls,
                cancellationToken)
            .ConfigureAwait(false);

        if (results.Count == 0)
        {
            IQueryable<TSource> insertSource = RemoveWhereClauseVisitor.Remove(source);

            results = [await insertSource
                .ExecuteInsertGetRowAsync(
                    insertSetPropertyCalls ?? updateSetPropertyCalls,
                    cancellationToken)
                .ConfigureAwait(false)];
        }

        if (transaction is not null)
        {
            await transaction.CommitAsync(cancellationToken).ConfigureAwait(false);
        }

        return results;
    }

    #endregion public ExecuteUpsertGetRowsAsync methods

    #region private methods

    /// <summary>
    /// Throws an exception if the <paramref name="source"/> has table hints.
    /// </summary>
    /// <typeparam name="TSource">The type of the elements of <paramref name="source" />.</typeparam>
    /// <param name="source">The query to check.</param>
    /// <param name="context">The query's <see cref="DbContext"/>, used to check the database type.</param>
    /// <param name="methodName">The name of the calling method, used in the exception message.</param>
    /// <exception cref="NotSupportedException">The database is SQL Server and <paramref name="source"/> has table hints.</exception>
    private static void ThrowIfTableHintsPresent<TSource>(IQueryable<TSource> source, DbContext context, string methodName)
        where TSource : class
    {
        if (context.Database.GetDatabaseType() == DatabaseTypes.SqlServer && HasTableHintsVisitor.HasTableHints(source))
        {
            throw new NotSupportedException($"{methodName} does not support queries with table hints on SQL Server.");
        }
    }

    /// <summary>
    /// Gets the <see cref="ExecuteUpdateState"/> for a context.
    /// </summary>
    /// <param name="context">The context.</param>
    /// <param name="methodName">The name of the calling method, used in the exception message.</param>
    /// <returns>The <see cref="ExecuteUpdateState"/>.</returns>
    /// <exception cref="InvalidOperationException"><see cref="UseExecuteUpdateExtensions"/> was not called when configuring the DbContext, or another extension library has replaced its query generator.</exception>
    [SuppressMessage("Usage", "EF1001:Internal EF Core API usage.", Justification = "We need to know if EF Core's default query generator factory is installed.")]
    private static ExecuteUpdateState GetExecuteUpdateState(DbContext context, string methodName)
    {
        // The custom SQL is written by the custom query generator. The generator is installed by UseExecuteUpdateExtensions (and also by
        // UseTableHintExtensions) so check for the generator itself rather than the Use method. We check for the SqlServerQuerySqlGeneratorFactory
        // / SqliteQuerySqlGeneratorFactory by type rather than using "is" as most custom query generator factories use it.
        IQuerySqlGeneratorFactory factory = context.GetService<IQuerySqlGeneratorFactory>();

        if (factory is not (SqlServerCustomQueryGeneratorFactory or SqliteCustomQueryGeneratorFactory))
        {
            Type factoryType = factory.GetType();

            string message = factoryType == typeof(SqlServerQuerySqlGeneratorFactory) || factoryType == typeof(SqliteQuerySqlGeneratorFactory)
                ? $"{nameof(UseExecuteUpdateExtensions)} must be called when configuring the DbContext, before using {methodName}."
                : $"{methodName} cannot be used because another extension library has replaced the query generator (IQuerySqlGeneratorFactory) that {nameof(UseExecuteUpdateExtensions)} installs.";

            throw new InvalidOperationException(message);
        }

        ExecuteUpdateState state = context.GetInfrastructure().GetService<ExecuteUpdateState>()
            ?? throw new InvalidOperationException($"{nameof(UseExecuteUpdateExtensions)} must be called when configuring the DbContext, before using {methodName}.");

        return state;
    }

    /// <summary>
    /// Begin a new transaction unless the context already has one open.
    /// </summary>
    /// <param name="context">The <see cref="DbContext"/> to begin the transaction on.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
    /// <returns>The new <see cref="IDbContextTransaction"/>, or <see langword="null"/> if the caller already owns a transaction.</returns>
    private static async Task<IDbContextTransaction?> BeginTransactionIfRequiredAsync(DbContext context, CancellationToken cancellationToken)
    {
        return context.Database.CurrentTransaction is null
            ? await context.Database.BeginTransactionAsync(cancellationToken).ConfigureAwait(false)
            : null;
    }

    #endregion private methods

    #region private RemoveWhereClauseVisitor class

    /// <summary>
    /// Remove the Where clause from a query.
    /// </summary>
    private sealed class RemoveWhereClauseVisitor : ExpressionVisitor
    {
        #region public methods

        /// <summary>
        /// Remove the where clause from a query.
        /// </summary>
        /// <typeparam name="T">The type of the elements of <paramref name="query" />.</typeparam>
        /// <param name="query">The <see cref="IQueryable{T}"/> to modify.</param>
        /// <returns>The query with the where clause removed.</returns>
        public static IQueryable<T> Remove<T>(IQueryable<T> query)
        {
            IQueryable<T> modifiedQuery = query.Provider.CreateQuery<T>(new RemoveWhereClauseVisitor().Visit(query.Expression));
            return modifiedQuery;
        }

        #endregion public methods

        #region protected methods

        // <inheritdoc/>
        protected override Expression VisitMethodCall(MethodCallExpression node)
        {
            Expression expression = node.Method.Name == nameof(Enumerable.Where) && node.Method.DeclaringType == typeof(Queryable)
                ? node.Arguments[0]
                : base.VisitMethodCall(node);

            return expression;
        }

        #endregion protected methods
    }

    #endregion private RemoveWhereClauseVisitor class

    #region private HasTableHintsVisitor class

    /// <summary>
    /// Check to see if a query has table hints.
    /// </summary>
    private sealed class HasTableHintsVisitor : ExpressionVisitor
    {
        #region private fields

        /// <summary>
        /// Set to <see langword="true"/> when a table hints tag is found.
        /// </summary>
        private bool _hasTableHints;

        #endregion private fields

        #region public methods

        /// <summary>
        /// Check if a query has table hints.
        /// </summary>
        /// <typeparam name="T">The type of the elements of <paramref name="query" />.</typeparam>
        /// <param name="query">The <see cref="IQueryable{T}"/> to check.</param>
        /// <returns><see langword="true"/> if the query has table hints; otherwise, <see langword="false"/>.</returns>
        public static bool HasTableHints<T>(IQueryable<T> query)
        {
            HasTableHintsVisitor visitor = new();
            visitor.Visit(query.Expression);
            return visitor._hasTableHints;
        }

        #endregion public methods

        #region protected methods

        /// <inheritdoc/>
        protected override Expression VisitMethodCall(MethodCallExpression node)
        {
            if (node.Method.DeclaringType == typeof(EntityFrameworkQueryableExtensions)
                && node.Method.Name == nameof(EntityFrameworkQueryableExtensions.TagWith)
                && node.Arguments[1] is ConstantExpression { Value: string tag }
                && CustomTagExtensions.IsTableHintsTag(tag))
            {
                this._hasTableHints = true;
                return node;
            }

            return base.VisitMethodCall(node);
        }

        #endregion protected methods
    }

    #endregion private HasTableHintsVisitor class
}
