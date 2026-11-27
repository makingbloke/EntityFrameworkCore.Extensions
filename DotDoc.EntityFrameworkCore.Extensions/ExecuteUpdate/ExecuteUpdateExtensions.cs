// Copyright ©2021-2026 Mike King.
// This file is licensed to you under the MIT license.
// See the License.txt file in the solution root for more information.

using DotDoc.EntityFrameworkCore.Extensions.CustomQueryGenerators;
using DotDoc.EntityFrameworkCore.Extensions.DatabaseType;
using DotDoc.EntityFrameworkCore.Extensions.TableHints;
using DotDoc.EntityFrameworkCore.Extensions.Utilities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Sqlite.Query.Internal;
using Microsoft.EntityFrameworkCore.SqlServer.Query.Internal;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.DependencyInjection;
using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;
using System.Reflection;
using System.Transactions;

namespace DotDoc.EntityFrameworkCore.Extensions.ExecuteUpdate;

/// <summary>
/// Execute Update Extensions.
/// </summary>
public static partial class ExecuteUpdateExtensions
{
    #region private fields

    /// <summary>
    /// The <see cref="ReadDerivedRowsAsync{TRoot, TSource}"/> method, which is called with the root type of a hierarchy only known at run time.
    /// </summary>
    [SuppressMessage("Major Code Smell", "S3011:Reflection should not be used to increase accessibility of classes, methods, or fields", Justification = "The method is this class's own private method, called with a type only known at run time.")]
    private static readonly MethodInfo ReadDerivedRowsMethod = typeof(ExecuteUpdateExtensions).GetMethod(nameof(ReadDerivedRowsAsync), BindingFlags.NonPublic | BindingFlags.Static)!;

    #endregion private fields

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
    /// <exception cref="NotSupportedException">The table is configured with <c>UseSqlOutputClause(false)</c> (SQL Server) / <c>UseSqlReturningClause(false)</c> (SQLite), the entity type (or a type derived from it) has an owned type (<c>OwnsOne</c> / <c>OwnsMany</c>), or the entity type is in an inheritance hierarchy mapped with TPT or TPC (<c>UseTptMappingStrategy</c> / <c>UseTpcMappingStrategy</c>).</exception>
    public static async Task<IList<TSource>> ExecuteDeleteGetRowsAsync<TSource>(this IQueryable<TSource> source, CancellationToken cancellationToken = default)
        where TSource : class
    {
        ArgumentNullException.ThrowIfNull(source);

        DbContext context = source.GetDbContext();
        ExecuteUpdateState state = GetExecuteUpdateState(context, nameof(ExecuteDeleteGetRowsAsync));
        ThrowIfEntityTypeNotSupported(source, context, nameof(ExecuteDeleteGetRowsAsync));

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
            List<TSource> results = await ReadRowsAsync(source, context, state.Sql, state.Parameters, cancellationToken).ConfigureAwait(false);

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
    /// <param name="source">The table to insert the row into (e.g. <c>context.Set&lt;T&gt;()</c>), with no query operators.</param>
    /// <param name="setPropertyCalls">A method containing set property statements specifying the values of the inserted row's properties.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
    /// <returns>A <see cref="Task"/> that represents the asynchronous operation.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="source"/> or <paramref name="setPropertyCalls"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentException"><paramref name="source"/> is not an EF Core query.</exception>
    /// <exception cref="InvalidOperationException"><see cref="UseExecuteUpdateExtensions"/> was not called when configuring the DbContext, another extension library has replaced its query generator, another ExecuteUpdate call is in progress on the DbContext, or the query is more than the table (e.g. it has a <c>Where</c> or <c>OrderBy</c>).</exception>
    public static async Task ExecuteInsertAsync<TSource>(this IQueryable<TSource> source, Action<UpdateSettersBuilder<TSource>> setPropertyCalls, CancellationToken cancellationToken = default)
        where TSource : class
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(setPropertyCalls);

        _ = GetExecuteUpdateState(source.GetDbContext(), nameof(ExecuteInsertAsync));

        // The tag tells the custom query generator to generate an INSERT rather than an UPDATE.
        // Global query filters are ignored, as EF Core would otherwise add them to the query as a WHERE, which
        // an INSERT cannot have, and they limit the rows read, which an INSERT doesn't do.
        await source
            .IgnoreQueryFilters()
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
    /// <param name="source">The table to insert the row into (e.g. <c>context.Set&lt;T&gt;()</c>), with no query operators.</param>
    /// <param name="setPropertyCalls">A method containing set property statements specifying the values of the inserted row's properties.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
    /// <returns>An instance of the <typeparamref name="TSource"/> containing the inserted row.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="source"/> or <paramref name="setPropertyCalls"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentException"><paramref name="source"/> is not an EF Core query.</exception>
    /// <exception cref="InvalidOperationException"><see cref="UseExecuteUpdateExtensions"/> was not called when configuring the DbContext, another extension library has replaced its query generator, another ExecuteUpdate call is in progress on the DbContext, or the query is more than the table (e.g. it has a <c>Where</c> or <c>OrderBy</c>).</exception>
    /// <exception cref="NotSupportedException">The table is configured with <c>UseSqlOutputClause(false)</c> (SQL Server) / <c>UseSqlReturningClause(false)</c> (SQLite), the entity type (or a type derived from it) has an owned type (<c>OwnsOne</c> / <c>OwnsMany</c>), or the entity type is in an inheritance hierarchy mapped with TPT or TPC (<c>UseTptMappingStrategy</c> / <c>UseTpcMappingStrategy</c>).</exception>
    public static async Task<TSource> ExecuteInsertGetRowAsync<TSource>(this IQueryable<TSource> source, Action<UpdateSettersBuilder<TSource>> setPropertyCalls, CancellationToken cancellationToken = default)
        where TSource : class
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(setPropertyCalls);

        DbContext context = source.GetDbContext();
        ExecuteUpdateState state = GetExecuteUpdateState(context, nameof(ExecuteInsertGetRowAsync));
        ThrowIfEntityTypeNotSupported(source, context, nameof(ExecuteInsertGetRowAsync));

        state.SetQueryType(QueryType.InsertGetRow);

        try
        {
            // Execute the insert and capture SQL and parameters (This just generates the SQL, it is not executed).
            // Global query filters are ignored, as EF Core would otherwise add them to the query as a WHERE, which
            // an INSERT cannot have, and they limit the rows read, which an INSERT doesn't do.
            await source
                .IgnoreQueryFilters()
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
            List<TSource> results = await ReadRowsAsync(source, context, state.Sql, state.Parameters, cancellationToken).ConfigureAwait(false);

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
    /// <exception cref="NotSupportedException">The table is configured with <c>UseSqlOutputClause(false)</c> (SQL Server) / <c>UseSqlReturningClause(false)</c> (SQLite), the entity type (or a type derived from it) has an owned type (<c>OwnsOne</c> / <c>OwnsMany</c>), or the entity type is in an inheritance hierarchy mapped with TPT or TPC (<c>UseTptMappingStrategy</c> / <c>UseTpcMappingStrategy</c>).</exception>
    public static async Task<IList<TSource>> ExecuteUpdateGetRowsAsync<TSource>(this IQueryable<TSource> source, Action<UpdateSettersBuilder<TSource>> setPropertyCalls, CancellationToken cancellationToken = default)
        where TSource : class
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(setPropertyCalls);

        DbContext context = source.GetDbContext();
        ExecuteUpdateState state = GetExecuteUpdateState(context, nameof(ExecuteUpdateGetRowsAsync));
        ThrowIfEntityTypeNotSupported(source, context, nameof(ExecuteUpdateGetRowsAsync));

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
            List<TSource> results = await ReadRowsAsync(source, context, state.Sql, state.Parameters, cancellationToken).ConfigureAwait(false);

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
    /// <exception cref="InvalidOperationException"><see cref="UseExecuteUpdateExtensions"/> was not called when configuring the DbContext, another extension library has replaced its query generator, another ExecuteUpdate call is in progress on the DbContext, or the query (without its <c>Where</c>) is more than the table, so it can't be used for the insert (e.g. it has an <c>OrderBy</c>).</exception>
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

        return await ExecuteInTransactionAsync(
            context,
            async cancellationToken =>
            {
                int count = await source
                    .WithTableHints(tableHints => tableHints.For(
                        FindEntityType(source, context) ?? throw new InvalidOperationException($"{typeof(TSource).Name} is not an entity type in the model."),
                        SqlServerTableHints.HoldLock,
                        SqlServerTableHints.UpdLock))
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

                return count;
            },
            cancellationToken)
            .ConfigureAwait(false);
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
    /// <exception cref="NotSupportedException">The query has table hints (<c>WithTableHints</c>) on SQL Server, the table is configured with <c>UseSqlOutputClause(false)</c> (SQL Server) / <c>UseSqlReturningClause(false)</c> (SQLite), the entity type (or a type derived from it) has an owned type (<c>OwnsOne</c> / <c>OwnsMany</c>), or the entity type is in an inheritance hierarchy mapped with TPT or TPC (<c>UseTptMappingStrategy</c> / <c>UseTpcMappingStrategy</c>).</exception>
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
        ThrowIfEntityTypeNotSupported(source, context, nameof(ExecuteUpsertGetRowsAsync));
        _ = GetExecuteUpdateState(context, nameof(ExecuteUpsertGetRowsAsync));

        return await ExecuteInTransactionAsync(
            context,
            async cancellationToken =>
            {
                IList<TSource> results = await source
                    .WithTableHints(tableHints => tableHints.For(
                        FindEntityType(source, context) ?? throw new InvalidOperationException($"{typeof(TSource).Name} is not an entity type in the model."),
                        SqlServerTableHints.HoldLock,
                        SqlServerTableHints.UpdLock))
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

                return results;
            },
            cancellationToken)
            .ConfigureAwait(false);
    }

    #endregion public ExecuteUpsertGetRowsAsync methods

    #region private methods

    /// <summary>
    /// Runs the captured SQL, which returns the rows it changed (RETURNING / OUTPUT), and reads them as entities.
    /// </summary>
    /// <typeparam name="TSource">The type of the elements of <paramref name="source" />.</typeparam>
    /// <param name="source">The query the SQL was captured from.</param>
    /// <param name="context">The query's <see cref="DbContext"/>.</param>
    /// <param name="sql">The captured SQL, with its braces escaped.</param>
    /// <param name="parameters">The captured parameters.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
    /// <returns>The rows.</returns>
    /// <exception cref="InvalidOperationException"><typeparamref name="TSource"/> is a shared type and its entity type can't be found in <paramref name="source"/>.</exception>
    private static async Task<List<TSource>> ReadRowsAsync<TSource>(IQueryable<TSource> source, DbContext context, string sql, object[] parameters, CancellationToken cancellationToken)
        where TSource : class
    {
        // The rows are read through the entity type, so its mapping (column names, value converters, inheritance) is used. SqlQueryRaw
        // would read each property from the column with the same name.
        Type? rootType = context.Model.FindEntityType(typeof(TSource))?.GetRootType().ClrType;

        if (rootType is not null && rootType != typeof(TSource))
        {
            // A derived type in an inheritance hierarchy. EF Core adds a filter on the discriminator to FromSqlRaw on a derived type, which
            // wraps the SQL in a subquery, so the rows are read through the root type. They're all of the derived type, as only its rows changed.
            Task<List<TSource>> task = (Task<List<TSource>>)ReadDerivedRowsMethod
                .MakeGenericMethod(rootType, typeof(TSource))
                .Invoke(null, [context, sql, parameters, cancellationToken])!;

            return await task.ConfigureAwait(false);
        }

        return await ReadEntityRowsAsync(GetEntitySet(source, context), sql, parameters, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Runs the captured SQL through the root type of a derived type's inheritance hierarchy, and returns the rows as the derived type.
    /// </summary>
    /// <typeparam name="TRoot">The root type of the hierarchy.</typeparam>
    /// <typeparam name="TSource">The derived type.</typeparam>
    /// <param name="context">The query's <see cref="DbContext"/>.</param>
    /// <param name="sql">The captured SQL, with its braces escaped.</param>
    /// <param name="parameters">The captured parameters.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
    /// <returns>The rows.</returns>
    private static async Task<List<TSource>> ReadDerivedRowsAsync<TRoot, TSource>(DbContext context, string sql, object[] parameters, CancellationToken cancellationToken)
        where TRoot : class
        where TSource : class, TRoot
    {
        List<TRoot> rows = await ReadEntityRowsAsync(context.Set<TRoot>(), sql, parameters, cancellationToken).ConfigureAwait(false);
        return [.. rows.Cast<TSource>()];
    }

    /// <summary>
    /// Runs the captured SQL through an entity type's <see cref="DbSet{TEntity}"/>.
    /// </summary>
    /// <typeparam name="TEntity">The entity type.</typeparam>
    /// <param name="entitySet">The entity type's <see cref="DbSet{TEntity}"/>.</param>
    /// <param name="sql">The captured SQL, with its braces escaped.</param>
    /// <param name="parameters">The captured parameters.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
    /// <returns>The rows.</returns>
    private static Task<List<TEntity>> ReadEntityRowsAsync<TEntity>(DbSet<TEntity> entitySet, string sql, object[] parameters, CancellationToken cancellationToken)
        where TEntity : class
    {
        // The query filters were applied when the rows were chosen (they're in the captured SQL). Applying them again would test the changed
        // rows, and wrap the SQL in a subquery, which a statement with RETURNING / OUTPUT can't be.
        return entitySet
            .FromSqlRaw(sql, parameters)
            .IgnoreQueryFilters()
            .AsNoTracking()
            .ToListAsync(cancellationToken);
    }

    /// <summary>
    /// Gets the <see cref="DbSet{TEntity}"/> of the entity type a query is on.
    /// </summary>
    /// <typeparam name="TSource">The type of the elements of <paramref name="source" />.</typeparam>
    /// <param name="source">The query.</param>
    /// <param name="context">The query's <see cref="DbContext"/>.</param>
    /// <returns>The <see cref="DbSet{TEntity}"/>.</returns>
    /// <exception cref="InvalidOperationException"><typeparamref name="TSource"/> is a shared type and its entity type can't be found in <paramref name="source"/>.</exception>
    private static DbSet<TSource> GetEntitySet<TSource>(IQueryable<TSource> source, DbContext context)
        where TSource : class
    {
        if (context.Model.FindEntityType(typeof(TSource)) is not null)
        {
            return context.Set<TSource>();
        }

        // A shared type entity (e.g. a property bag) has a name as well as a type, which is only known from the query's root.
        string entityTypeName = EntityQueryRootFinder.FindEntityTypeName(source.Expression, typeof(TSource))
            ?? throw new InvalidOperationException($"The entity type of {typeof(TSource).Name} could not be found in the query.");

        return context.Set<TSource>(entityTypeName);
    }

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
    /// Throws an exception if the rows of the entity type of <paramref name="source"/> can't be read back from the SQL that changed them.
    /// </summary>
    /// <typeparam name="TSource">The type of the elements of <paramref name="source" />.</typeparam>
    /// <param name="source">The query to check.</param>
    /// <param name="context">The query's <see cref="DbContext"/>.</param>
    /// <param name="methodName">The name of the calling method, used in the exception message.</param>
    /// <exception cref="NotSupportedException">The entity type (or a type derived from it) has an owned type, or the entity type is in an inheritance hierarchy mapped with TPT or TPC.</exception>
    private static void ThrowIfEntityTypeNotSupported<TSource>(IQueryable<TSource> source, DbContext context, string methodName)
        where TSource : class
    {
        IEntityType? rootType = FindEntityType(source, context)?.GetRootType();

        if (rootType is null)
        {
            return;
        }

        // EF Core reads an owned type with a join (to its own table, or to the same table), so it would wrap the captured SQL in a subquery,
        // which a statement with RETURNING / OUTPUT can't be. The whole hierarchy is checked, as a derived type's rows are read through its root type.
        INavigation? ownedNavigation = rootType
            .GetDerivedTypesInclusive()
            .SelectMany(type => type.GetDeclaredNavigations())
            .FirstOrDefault(navigation => navigation.TargetEntityType.IsOwned());

        if (ownedNavigation is not null)
        {
            throw new NotSupportedException($"{methodName} does not support entity types with owned types ({ownedNavigation.DeclaringEntityType.DisplayName()}.{ownedNavigation.Name}).");
        }

        // The rows are read with FromSqlRaw, which EF Core refuses for an inheritance hierarchy without a discriminator (TPT and TPC). With TPT
        // the RETURNING / OUTPUT clause would only give the columns of one of the type's tables anyway.
        if (rootType.FindDiscriminatorProperty() is null && rootType.GetDirectlyDerivedTypes().Any())
        {
            throw new NotSupportedException($"{methodName} does not support entity types in an inheritance hierarchy mapped with TPT or TPC ({rootType.DisplayName()}).");
        }
    }

    /// <summary>
    /// Find the entity type of a query, including a shared type entity type (found from the query root).
    /// </summary>
    /// <typeparam name="TSource">The type of the elements of <paramref name="source" />.</typeparam>
    /// <param name="source">The query.</param>
    /// <param name="context">The query's <see cref="DbContext"/>.</param>
    /// <returns>The entity type, or <see langword="null"/> if it can't be found.</returns>
    private static IEntityType? FindEntityType<TSource>(IQueryable<TSource> source, DbContext context)
        where TSource : class
    {
        return context.Model.FindEntityType(typeof(TSource))
            ?? (EntityQueryRootFinder.FindEntityTypeName(source.Expression, typeof(TSource)) is { } entityTypeName
                ? context.Model.FindEntityType(entityTypeName)
                : null);
    }

    /// <summary>
    /// Gets the <see cref="ExecuteUpdateState"/> for a context.
    /// </summary>
    /// <param name="context">The context.</param>
    /// <param name="methodName">The name of the calling method, used in the exception message.</param>
    /// <returns>The <see cref="ExecuteUpdateState"/>.</returns>
    /// <exception cref="InvalidOperationException"><see cref="UseExecuteUpdateExtensions"/> was not called when configuring the DbContext, another extension library has replaced its query generator, or another ExecuteUpdate call is in progress on the DbContext.</exception>
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

        if (state.QueryType != QueryType.Unknown)
        {
            throw new InvalidOperationException("Another ExecuteUpdate call is already in progress on this DbContext.");
        }

        return state;
    }

    /// <summary>
    /// Run an operation in a transaction: the context's own transaction if it has one, otherwise a new one that is committed when the operation succeeds.
    /// </summary>
    /// <typeparam name="TResult">The type of the operation's result.</typeparam>
    /// <param name="context">The <see cref="DbContext"/> to run the operation on.</param>
    /// <param name="operation">The operation to run. It may be run more than once, so it must not change any state outside the database.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
    /// <returns>The result of the operation.</returns>
    private static async Task<TResult> ExecuteInTransactionAsync<TResult>(DbContext context, Func<CancellationToken, Task<TResult>> operation, CancellationToken cancellationToken)
    {
        // The caller owns the transaction, and so retrying it if they use a retrying execution strategy, so just run in it. This includes a
        // System.Transactions transaction: an ambient one (TransactionScope) or one the connection was enlisted in with EnlistTransaction. Neither
        // sets CurrentTransaction, but EF Core won't begin a new transaction while the connection is in one.
        if (context.Database.CurrentTransaction is not null
            || context.Database.GetEnlistedTransaction() is not null
            || Transaction.Current is not null)
        {
            return await operation(cancellationToken).ConfigureAwait(false);
        }

        // Begin the transaction inside the context's execution strategy. A retrying strategy (e.g. EnableRetryOnFailure) throws if a transaction is
        // begun outside it, as it can't retry part of a transaction. Inside it, a transient error runs the whole operation again in a new transaction.
        // A strategy that doesn't retry runs the operation once.
        return await context.Database
            .CreateExecutionStrategy()
            .ExecuteAsync(
                (Context: context, Operation: operation),
                static async (state, cancellationToken) =>
                {
                    // Disposing the transaction without committing it rolls it back, so a failed attempt leaves nothing behind.
                    await using IDbContextTransaction transaction = await state.Context.Database.BeginTransactionAsync(cancellationToken).ConfigureAwait(false);

                    TResult result = await state.Operation(cancellationToken).ConfigureAwait(false);

                    await transaction.CommitAsync(cancellationToken).ConfigureAwait(false);

                    return result;
                },
                cancellationToken)
            .ConfigureAwait(false);
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

        /// <inheritdoc/>
        protected override Expression VisitMethodCall(MethodCallExpression node)
        {
            // Visit the Where's source too, so every Where in a chain (Where(Where(source, a), b)) is removed, not just the last one.
            Expression expression = node.Method.Name == nameof(Enumerable.Where) && node.Method.DeclaringType == typeof(Queryable)
                ? this.Visit(node.Arguments[0])
                : base.VisitMethodCall(node);

            return expression;
        }

        #endregion protected methods
    }

    #endregion private RemoveWhereClauseVisitor class

    #region private EntityQueryRootFinder class

    /// <summary>
    /// Finds the entity type of a query's root.
    /// </summary>
    private sealed class EntityQueryRootFinder : ExpressionVisitor
    {
        #region private fields

        /// <summary>
        /// The CLR type of the entity type to find.
        /// </summary>
        private readonly Type _clrType;

        /// <summary>
        /// The name of the first entity type found, or <see langword="null"/> if none has been found.
        /// </summary>
        private string? _entityTypeName;

        #endregion private fields

        #region private constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="EntityQueryRootFinder"/> class.
        /// </summary>
        /// <param name="clrType">The CLR type of the entity type to find.</param>
        private EntityQueryRootFinder(Type clrType)
        {
            this._clrType = clrType;
        }

        #endregion private constructors

        #region public methods

        /// <summary>
        /// Finds the name of the first entity type with a CLR type of <paramref name="clrType"/> that a query reads from.
        /// </summary>
        /// <param name="expression">The query's expression.</param>
        /// <param name="clrType">The CLR type of the entity type.</param>
        /// <returns>The entity type's name, or <see langword="null"/> if none was found.</returns>
        public static string? FindEntityTypeName(Expression expression, Type clrType)
        {
            EntityQueryRootFinder finder = new(clrType);
            finder.Visit(expression);
            return finder._entityTypeName;
        }

        #endregion public methods

        #region protected methods

        /// <inheritdoc/>
        protected override Expression VisitExtension(Expression node)
        {
            if (this._entityTypeName is null && node is EntityQueryRootExpression root && root.EntityType.ClrType == this._clrType)
            {
                this._entityTypeName = root.EntityType.Name;
                return node;
            }

            return base.VisitExtension(node);
        }

        #endregion protected methods
    }

    #endregion private EntityQueryRootFinder class

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
