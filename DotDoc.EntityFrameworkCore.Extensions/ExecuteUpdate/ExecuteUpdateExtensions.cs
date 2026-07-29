// Copyright ©2021-2026 Mike King.
// This file is licensed to you under the MIT license.
// See the License.txt file in the solution root for more information.

using DotDoc.EntityFrameworkCore.Extensions.CustomQueryGenerators;
using DotDoc.EntityFrameworkCore.Extensions.Utilities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Storage;
using System.Linq.Expressions;

namespace DotDoc.EntityFrameworkCore.Extensions.ExecuteUpdate;

/// <summary>
/// Execute Update Extensions.
/// </summary>
public static partial class ExecuteUpdateExtensions
{
    #region internal fields

    /// <summary>
    /// Execute Update Query Parameters.
    /// </summary>
    internal static readonly AsyncLocal<ExecuteUpdateParameters> ExecuteUpdateParameters = new();

    #endregion internal fields

    #region public UseExecuteUpdateExtensions methods

    /// <summary>
    /// Use the Execute Update Extensions.
    /// </summary>
    /// <param name="optionsBuilder">The builder being used to configure the context.</param>
    /// <returns>The same builder instance so multiple calls can be chained.</returns>
    public static DbContextOptionsBuilder UseExecuteUpdateExtensions(this DbContextOptionsBuilder optionsBuilder)
    {
        ArgumentNullException.ThrowIfNull(optionsBuilder);

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
    public static async Task<IList<TSource>> ExecuteDeleteGetRowsAsync<TSource>(this IQueryable<TSource> source, CancellationToken cancellationToken = default)
        where TSource : class
    {
        ArgumentNullException.ThrowIfNull(source);

        ExecuteUpdateExtensions.ExecuteUpdateParameters.Value = new(QueryType.DeleteGetRows);

        try
        {
            // Execute the delete and capture SQL and parameters (This just generates the SQL, it is not executed).
            await source
                .ExecuteDeleteAsync(cancellationToken)
                .ConfigureAwait(false);

            // Execute the delete and get the deleted rows.
            DbContext context = source.GetDbContext();

            List<TSource> results = await context.Database.SqlQueryRaw<TSource>(
                ExecuteUpdateExtensions.ExecuteUpdateParameters.Value.Sql,
                ExecuteUpdateExtensions.ExecuteUpdateParameters.Value.Parameters)
                .AsNoTracking()
                .ToListAsync(cancellationToken)
                .ConfigureAwait(false);

            return results;
        }
        finally
        {
            ExecuteUpdateExtensions.ExecuteUpdateParameters.Value = null!;
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
    public static async Task ExecuteInsertAsync<TSource>(this IQueryable<TSource> source, Action<UpdateSettersBuilder<TSource>> setPropertyCalls, CancellationToken cancellationToken = default)
        where TSource : class
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(setPropertyCalls);

        ExecuteUpdateExtensions.ExecuteUpdateParameters.Value = new(QueryType.Insert);

        try
        {
            await source
                .ExecuteUpdateAsync(
                    setPropertyCalls,
                    cancellationToken)
                .ConfigureAwait(false);
        }
        finally
        {
            ExecuteUpdateExtensions.ExecuteUpdateParameters.Value = null!;
        }
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
    /// <returns>An instance of the <typeparamref name="TSource"/> containing the inserted row or <see langword="null"/> if the row cannot be retrieved.</returns>
    public static async Task<TSource> ExecuteInsertGetRowAsync<TSource>(this IQueryable<TSource> source, Action<UpdateSettersBuilder<TSource>> setPropertyCalls, CancellationToken cancellationToken = default)
        where TSource : class
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(setPropertyCalls);

        ExecuteUpdateExtensions.ExecuteUpdateParameters.Value = new(QueryType.InsertGetRow);

        try
        {
            // Execute the insert and capture SQL and parameters (This just generates the SQL, it is not executed).
            await source.ExecuteUpdateAsync(
                    setPropertyCalls,
                    cancellationToken)
                .ConfigureAwait(false);

            // Execute the insert and get the inserted rows.
            DbContext context = source.GetDbContext();

            List<TSource> results = await context.Database.SqlQueryRaw<TSource>(
                ExecuteUpdateExtensions.ExecuteUpdateParameters.Value.Sql,
                ExecuteUpdateExtensions.ExecuteUpdateParameters.Value.Parameters)
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
            ExecuteUpdateExtensions.ExecuteUpdateParameters.Value = null!;
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
    public static async Task<IList<TSource>> ExecuteUpdateGetRowsAsync<TSource>(this IQueryable<TSource> source, Action<UpdateSettersBuilder<TSource>> setPropertyCalls, CancellationToken cancellationToken = default)
        where TSource : class
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(setPropertyCalls);

        ExecuteUpdateExtensions.ExecuteUpdateParameters.Value = new(QueryType.UpdateGetRows);

        try
        {
            // Execute the update and capture SQL and parameters (This just generates the SQL, it is not executed).
            await source.ExecuteUpdateAsync(
                    setPropertyCalls,
                    cancellationToken)
                .ConfigureAwait(false);

            // Execute the update and get the updated rows.
            DbContext context = source.GetDbContext();

            List<TSource> results = await context.Database.SqlQueryRaw<TSource>(
                ExecuteUpdateExtensions.ExecuteUpdateParameters.Value.Sql,
                ExecuteUpdateExtensions.ExecuteUpdateParameters.Value.Parameters)
                .AsNoTracking()
                .ToListAsync(cancellationToken)
                .ConfigureAwait(false);

            return results;
        }
        finally
        {
            ExecuteUpdateExtensions.ExecuteUpdateParameters.Value = null!;
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
    public static async Task<int> ExecuteUpsertGetCountAsync<TSource>(this IQueryable<TSource> source, Action<UpdateSettersBuilder<TSource>> updateSetPropertyCalls, Action<UpdateSettersBuilder<TSource>>? insertSetPropertyCalls = null, CancellationToken cancellationToken = default)
        where TSource : class
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(updateSetPropertyCalls);

        // Note:
        // SQL Server supports MERGE which can be used to do an upsert but requires extra work to ensure the Update and Insert are run inside the same transaction.
        // SQLite does not support MERGE but has an INSERT ... ON CONFLICT DO UPDATE syntax. Our problem with this is that it requires the set property
        // calls to update a value with a unique constraint of some kind to determine if the row exists and any values not set in the update are reset to their
        // default values. We also want to be able to optionally specify a different set of properties to update and insert for cases where primary keys are
        // generated manually. So the simplest approach that works across all databases is to create a transaction, then execute the update first and if no rows are
        // updated then execute the insert.
        DbContext context = source.GetDbContext();
        IDbContextTransaction transaction = await context.Database.BeginTransactionAsync(cancellationToken).ConfigureAwait(false);

        try
        {
            int count = await source.ExecuteUpdateGetCountAsync(
                updateSetPropertyCalls,
                cancellationToken)
                .ConfigureAwait(false);

            if (count == 0)
            {
                IQueryable<TSource> insertSource = RemoveWhereClauseVisitor.Remove(source);

                await insertSource.ExecuteInsertAsync(
                    insertSetPropertyCalls ?? updateSetPropertyCalls,
                    cancellationToken)
                    .ConfigureAwait(false);

                count = 1;
            }

            await transaction.CommitAsync(cancellationToken).ConfigureAwait(false);
            return count;
        }
        catch
        {
            await transaction.RollbackAsync(cancellationToken).ConfigureAwait(false);
            throw;
        }
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
    public static async Task<IList<TSource>> ExecuteUpsertGetRowsAsync<TSource>(this IQueryable<TSource> source, Action<UpdateSettersBuilder<TSource>> updateSetPropertyCalls, Action<UpdateSettersBuilder<TSource>>? insertSetPropertyCalls = null, CancellationToken cancellationToken = default)
        where TSource : class
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(updateSetPropertyCalls);

        // See note in ExecuteUpsertGetCountAsync regarding approach.
        DbContext context = source.GetDbContext();
        IDbContextTransaction transaction = await context.Database.BeginTransactionAsync(cancellationToken).ConfigureAwait(false);

        try
        {
            IList<TSource> results = await source.ExecuteUpdateGetRowsAsync(
                updateSetPropertyCalls,
                cancellationToken)
                .ConfigureAwait(false);

            if (results.Count == 0)
            {
                IQueryable<TSource> insertSource = RemoveWhereClauseVisitor.Remove(source);

                results = [await insertSource.ExecuteInsertGetRowAsync(
                    insertSetPropertyCalls ?? updateSetPropertyCalls,
                    cancellationToken)
                    .ConfigureAwait(false)];
            }

            await transaction.CommitAsync(cancellationToken).ConfigureAwait(false);
            return results;
        }
        catch
        {
            await transaction.RollbackAsync(cancellationToken).ConfigureAwait(false);
            throw;
        }
    }

    #endregion public ExecuteUpsertGetRowsAsync methods

    #region private RemoveWhereClauseVisitor class

    /// <summary>
    /// Remove the Where clause from a query.
    /// </summary>
    private sealed class RemoveWhereClauseVisitor : ExpressionVisitor
    {
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

        // <inheritdoc/>
        protected override Expression VisitMethodCall(MethodCallExpression node)
        {
            Expression expression = node.Method.Name == nameof(Enumerable.Where) && node.Method.DeclaringType == typeof(Queryable)
                ? node.Arguments[0]
                : base.VisitMethodCall(node);

            return expression;
        }
    }

    #endregion private RemoveWhereClauseVisitor class
}
