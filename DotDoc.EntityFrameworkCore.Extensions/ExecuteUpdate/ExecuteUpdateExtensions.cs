// Copyright ©2021-2025 Mike King.
// This file is licensed to you under the MIT license.
// See the License.txt file in the solution root for more information.

using DotDoc.EntityFrameworkCore.Extensions.CustomQueryGenerators;
using DotDoc.EntityFrameworkCore.Extensions.Utilities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using System.Data.Common;

namespace DotDoc.EntityFrameworkCore.Extensions.ExecuteUpdate;

/// <summary>
/// Execute Update Extensions.
/// </summary>
public static partial class ExecuteUpdateExtensions
{
    #region private fields

    /// <summary>
    /// The Query Parameters.
    /// </summary>
    internal static readonly QueryParameters QueryParameters = new();

    #endregion private fields

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
    /// <returns>An <see cref="IList{TSource}"/> containing the modified rows.</returns>
    public static async Task<IList<TSource>> ExecuteDeleteGetRowsAsync<TSource>(this IQueryable<TSource> source, CancellationToken cancellationToken = default)
        where TSource : class
    {
        ArgumentNullException.ThrowIfNull(source);

        QueryParameters.Reset(QueryType.DeleteGetRows);

        try
        {
            // Execute the delete and capture SQL and parameters (This just generates the SQL, it is not executed).
            await source.ExecuteDeleteAsync(cancellationToken).ConfigureAwait(false);

            // Execute the delete and get the deleted rows.
            DbContext context = source.GetDbContext();

            List<TSource> results = await context.Database.SqlQueryRaw<TSource>(QueryParameters.Sql!, QueryParameters.Parameters!)
                .ToListAsync(cancellationToken)
                .ConfigureAwait(false);

            return results;
        }
        finally
        {
            QueryParameters.Reset();
        }
    }

    #endregion public ExecuteDeleteGetRowsAsync methods

    #region public ExecuteInsertGetRowAsync methods

    /// <summary>
    /// Insert a database row.
    /// </summary>
    /// <typeparam name="TSource">The type of the elements of <paramref name="source" />.</typeparam>
    /// <param name="source">An <see cref="IQueryable{TSource}" /> whose elements to test for a condition.</param>
    /// <param name="setPropertyCalls">A method containing set property statements specifying properties to update.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
    /// <returns>An instance of the <typeparamref name="TSource"/> containing the inserted row or <see langword="null"/> if the row cannot be retrieved.</returns>
    public static async Task<TSource?> ExecuteInsertGetRowAsync<TSource>(this IQueryable<TSource> source, Action<UpdateSettersBuilder<TSource>> setPropertyCalls, CancellationToken cancellationToken = default)
        where TSource : class
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(setPropertyCalls);

        QueryParameters.Reset(QueryType.InsertGetRow);

        try
        {
            // Execute the insert and capture SQL and parameters (This just generates the SQL, it is not executed).
            await source.ExecuteUpdateAsync(setPropertyCalls, cancellationToken).ConfigureAwait(false);

            // Execute the insert and get the inserted rows.
            DbContext context = source.GetDbContext();

            List<TSource> results = await context.Database.SqlQueryRaw<TSource>(QueryParameters.Sql!, QueryParameters.Parameters!)
                .ToListAsync(cancellationToken)
                .ConfigureAwait(false);

            return results.Count > 0
                ? results[0]
                : null;
        }
        finally
        {
            QueryParameters.Reset();
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
    public static async Task<int> ExecuteUpdateGetCountAsync<TSource>(this IQueryable<TSource> source, Action<UpdateSettersBuilder<TSource>> setPropertyCalls, CancellationToken cancellationToken = default)
        where TSource : class
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(setPropertyCalls);

        int count = await source
            .ExecuteUpdateAsync(setPropertyCalls, cancellationToken)
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
    /// <returns>An <see cref="IList{TSource}"/> containing the modified rows.</returns>
    public static async Task<IList<TSource>> ExecuteUpdateGetRowsAsync<TSource>(this IQueryable<TSource> source, Action<UpdateSettersBuilder<TSource>> setPropertyCalls, CancellationToken cancellationToken = default)
        where TSource : class
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(setPropertyCalls);

        QueryParameters.Reset(QueryType.UpdateGetRows);

        try
        {
            // Execute the update and capture SQL and parameters (This just generates the SQL, it is not executed).
            await source.ExecuteUpdateAsync(setPropertyCalls, cancellationToken).ConfigureAwait(false);

            // Execute the update and get the updated rows.
            DbContext context = source.GetDbContext();

            List<TSource> results = await context.Database.SqlQueryRaw<TSource>(QueryParameters.Sql!, QueryParameters.Parameters!)
                .ToListAsync(cancellationToken)
                .ConfigureAwait(false);

            return results;
        }
        finally
        {
            QueryParameters.Reset();
        }
    }

    #endregion public ExecuteUpdateGetRowsAsync methods
}
