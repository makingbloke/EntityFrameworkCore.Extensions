// Copyright ©2021-2025 Mike King.
// This file is licensed to you under the MIT license.
// See the License.txt file in the solution root for more information.

using DotDoc.EntityFrameworkCore.Extensions.CustomQueryGenerators;
using DotDoc.EntityFrameworkCore.Extensions.Utilities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;

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
    public static async Task<int> ExecuteDeleteGetCountAsync<TSource>(this IQueryable<TSource> source, CancellationToken cancellationToken = default)
        where TSource : class
    {
        ArgumentNullException.ThrowIfNull(source);

        int count = await source
            .ExecuteDeleteAsync(cancellationToken)
            .ConfigureAwait(false);

        return count;
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

        CustomQueryGeneratorParameters.ExecuteUpdateParameters.Value = new(QueryType.DeleteGetRows);

        // Execute the delete and capture SQL and parameters (This just generates the SQL, it is not executed).
        await source
            .ExecuteDeleteAsync(cancellationToken)
            .ConfigureAwait(false);

        // Execute the delete and get the deleted rows.
        DbContext context = source.GetDbContext();

        List<TSource> results = await context.Database.SqlQueryRaw<TSource>(
                CustomQueryGeneratorParameters.ExecuteUpdateParameters.Value.Sql!,
                CustomQueryGeneratorParameters.ExecuteUpdateParameters.Value.Parameters!)
            .AsNoTracking()
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        return results;
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

        CustomQueryGeneratorParameters.ExecuteUpdateParameters.Value = new(QueryType.Insert);

        // Execute the insert.
        await source
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
    /// <returns>An instance of the <typeparamref name="TSource"/> containing the inserted row or <see langword="null"/> if the row cannot be retrieved.</returns>
    public static async Task<TSource> ExecuteInsertGetRowAsync<TSource>(this IQueryable<TSource> source, Action<UpdateSettersBuilder<TSource>> setPropertyCalls, CancellationToken cancellationToken = default)
        where TSource : class
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(setPropertyCalls);

        CustomQueryGeneratorParameters.ExecuteUpdateParameters.Value = new(QueryType.InsertGetRow);

        // Execute the insert and capture SQL and parameters (This just generates the SQL, it is not executed).
        await source.ExecuteUpdateAsync(
                setPropertyCalls,
                cancellationToken)
            .ConfigureAwait(false);

        // Execute the insert and get the inserted rows.
        DbContext context = source.GetDbContext();

        List<TSource> results = await context.Database.SqlQueryRaw<TSource>(
                CustomQueryGeneratorParameters.ExecuteUpdateParameters.Value.Sql!,
                CustomQueryGeneratorParameters.ExecuteUpdateParameters.Value.Parameters!)
            .AsNoTracking()
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        if (results.Count != 1)
        {
            throw new InvalidOperationException($"Unexpected entry count from insert: {results.Count}");
        }

        return results[0];
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
    public static async Task<int> ExecuteUpdateGetCountAsync<TSource>(this IQueryable<TSource> source, Action<UpdateSettersBuilder<TSource>> setPropertyCalls, CancellationToken cancellationToken = default)
        where TSource : class
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(setPropertyCalls);

        int count = await source
            .ExecuteUpdateAsync(
                setPropertyCalls,
                cancellationToken)
            .ConfigureAwait(false);

        return count;
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

        CustomQueryGeneratorParameters.ExecuteUpdateParameters.Value = new(QueryType.UpdateGetRows);

        // Execute the update and capture SQL and parameters (This just generates the SQL, it is not executed).
        await source.ExecuteUpdateAsync(
                setPropertyCalls,
                cancellationToken)
            .ConfigureAwait(false);

        // Execute the update and get the updated rows.
        DbContext context = source.GetDbContext();

        List<TSource> results = await context.Database.SqlQueryRaw<TSource>(
                CustomQueryGeneratorParameters.ExecuteUpdateParameters.Value.Sql!,
                CustomQueryGeneratorParameters.ExecuteUpdateParameters.Value.Parameters!)
            .AsNoTracking()
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        return results;
    }

    #endregion public ExecuteUpdateGetRowsAsync methods

    #region public ExecuteUpsertGetCountAsync methods

    /// <summary>
    /// Update the entity instances which match the LINQ query from the database or insert them if they do not exist.
    /// </summary>
    /// <typeparam name="TSource">The type of the elements of <paramref name="source" />.</typeparam>
    /// <param name="source">An <see cref="IQueryable{TSource}" /> whose elements to test for a condition.</param>
    /// <param name="setPropertyCalls">A method containing set property statements specifying properties to upsert.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
    /// <returns>The number of rows updated / inserted in the database.</returns>
    public static async Task<int> ExecuteUpsertGetCountAsync<TSource>(this IQueryable<TSource> source, Action<UpdateSettersBuilder<TSource>> setPropertyCalls, CancellationToken cancellationToken = default)
        where TSource : class
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(setPropertyCalls);

        CustomQueryGeneratorParameters.ExecuteUpdateParameters.Value = new(QueryType.UpsertGetCount);

        // Execute the upsert.
        int count = await source.ExecuteUpdateAsync(
                setPropertyCalls,
                cancellationToken)
            .ConfigureAwait(false);

        return count;
    }

    #endregion public ExecuteUpsertGetCountAsync methods

    #region public ExecuteUpsertGetRowsAsync methods

    /// <summary>
    /// Update the entity instances which match the LINQ query from the database or insert them if they do not exist.
    /// </summary>
    /// <typeparam name="TSource">The type of the elements of <paramref name="source" />.</typeparam>
    /// <param name="source">An <see cref="IQueryable{TSource}" /> whose elements to test for a condition.</param>
    /// <param name="setPropertyCalls">A method containing set property statements specifying properties to upsert.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
    /// <returns>An <see cref="IList{TSource}"/> containing the updated / inserted rows.</returns>
    public static async Task<IList<TSource>> ExecuteUpsertGetRowsAsync<TSource>(this IQueryable<TSource> source, Action<UpdateSettersBuilder<TSource>> setPropertyCalls, CancellationToken cancellationToken = default)
        where TSource : class
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(setPropertyCalls);

        CustomQueryGeneratorParameters.ExecuteUpdateParameters.Value = new(QueryType.UpsertGetRows);

        // Execute the upsert and capture SQL and parameters (This just generates the SQL, it is not executed).
        await source.ExecuteUpdateAsync(
                setPropertyCalls,
                cancellationToken)
            .ConfigureAwait(false);

        // Execute the upsert and get the updated / inserted rows.
        DbContext context = source.GetDbContext();

        List<TSource> results = await context.Database.SqlQueryRaw<TSource>(
                CustomQueryGeneratorParameters.ExecuteUpdateParameters.Value.Sql!,
                CustomQueryGeneratorParameters.ExecuteUpdateParameters.Value.Parameters!)
            .AsNoTracking()
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        return results;
    }

    #endregion public ExecuteUpsertGetRowsAsync methods
}
