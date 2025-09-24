// Copyright ©2021-2025 Mike King.
// This file is licensed to you under the MIT license.
// See the License.txt file in the solution root for more information.

using Microsoft.EntityFrameworkCore.Infrastructure;

namespace DotDoc.EntityFrameworkCore.Extensions.Execute;

/// <summary>
/// Execute Exensions.
/// </summary>
public static class ExecuteExtensions
{
    #region public ExecuteInsert methods

    /// <summary>
    /// Executes an insert command.
    /// </summary>
    /// <typeparam name="T">The type returned by the insert.</typeparam>
    /// <param name="database">The <see cref="DatabaseFacade"/> for the context.</param>
    /// <param name="sql">The <see cref="FormattableString"/> representing a SQL query with parameters.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
    /// <returns>The Id of the new record.</returns>
    public static async Task<T> ExecuteInsertAsync<T>(this DatabaseFacade database, FormattableString sql, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(database);
        ArgumentNullException.ThrowIfNull(sql);

        T id = await ExecuteMethods.ExecuteInsertAsync<T>(database, sql.Format, sql.GetArguments(), cancellationToken).ConfigureAwait(false);
        return id;
    }

    /// <summary>
    /// Executes an insert command.
    /// </summary>
    /// <typeparam name="T">The type returned by the insert.</typeparam>
    /// <param name="database">The <see cref="DatabaseFacade"/> for the context.</param>
    /// <param name="sql">The SQL query to execute.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
    /// <param name="parameters">Parameters to use with the SQL.</param>
    /// <returns>The Id of the new record.</returns>
    public static async Task<T> ExecuteInsertAsync<T>(this DatabaseFacade database, string sql, CancellationToken cancellationToken = default, params IEnumerable<object?> parameters)
    {
        ArgumentNullException.ThrowIfNull(database);
        ArgumentException.ThrowIfNullOrEmpty(sql);
        ArgumentNullException.ThrowIfNull(parameters);

        T id = await ExecuteMethods.ExecuteInsertAsync<T>(database, sql, parameters, cancellationToken).ConfigureAwait(false);
        return id;
    }

    #endregion public ExecuteInsert methods

    #region public ExecuteNonQuery methods

    /// <summary>
    /// Executes a non query.
    /// </summary>
    /// <param name="database">The <see cref="DatabaseFacade"/> for the context.</param>
    /// <param name="sql">The <see cref="FormattableString"/> representing a SQL query with parameters.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
    /// <returns>The number of rows affected.</returns>
    public static async Task<int> ExecuteNonQueryAsync(this DatabaseFacade database, FormattableString sql, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(database);
        ArgumentNullException.ThrowIfNull(sql);

        int count = await ExecuteMethods.ExecuteNonQueryAsync(database, sql.Format, sql.GetArguments(), cancellationToken).ConfigureAwait(false);
        return count;
    }

    /// <summary>
    /// Executes a non query.
    /// </summary>
    /// <param name="database">The <see cref="DatabaseFacade"/> for the context.</param>
    /// <param name="sql">The SQL query to execute.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
    /// <param name="parameters">Parameters to use with the SQL.</param>
    /// <returns>The number of rows affected.</returns>
    public static async Task<int> ExecuteNonQueryAsync(this DatabaseFacade database, string sql, CancellationToken cancellationToken = default, params IEnumerable<object?> parameters)
    {
        ArgumentNullException.ThrowIfNull(database);
        ArgumentException.ThrowIfNullOrEmpty(sql);
        ArgumentNullException.ThrowIfNull(parameters);

        int count = await ExecuteMethods.ExecuteNonQueryAsync(database, sql, parameters, cancellationToken).ConfigureAwait(false);
        return count;
    }

    #endregion public ExecuteNonQuery methods

    #region public ExecutePagedQuery methods

    /// <summary>
    /// Executes a query and returns the specified page of results.
    /// </summary>
    /// <typeparam name="TSource">The type of the elements of the source.</typeparam>
    /// <param name="database">The <see cref="DatabaseFacade"/> for the context.</param>
    /// <param name="sql">The <see cref="FormattableString"/> representing a SQL query with parameters.</param>
    /// <param name="page">Page number to return (starting at 0).</param>
    /// <param name="pageSize">Number of records per page.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
    /// <returns>An instance of <see cref="PagedQueryResult{TSource}"/> containing the page data (If the page number is past the end of the table then the it will become the last page).</returns>
    public static async Task<PagedQueryResult<TSource>> ExecutePagedQueryAsync<TSource>(this DatabaseFacade database, FormattableString sql, long page, long pageSize, CancellationToken cancellationToken = default)
        where TSource : class
    {
        ArgumentNullException.ThrowIfNull(database);
        ArgumentNullException.ThrowIfNull(sql);
        ArgumentOutOfRangeException.ThrowIfNegative(page);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(pageSize);

        PagedQueryResult<TSource> pageResult = await ExecuteMethods.ExecutePagedQueryAsync<TSource>(database, sql.Format, sql.GetArguments(), page, pageSize, cancellationToken).ConfigureAwait(false);
        return pageResult;
    }

    /// <summary>
    /// Executes a query and returns the specified page of results.
    /// </summary>
    /// <typeparam name="TSource">The type of the elements of the source.</typeparam>
    /// <param name="database">The <see cref="DatabaseFacade"/> for the context.</param>
    /// <param name="sql">The SQL query to execute.</param>
    /// <param name="page">Page number to return (starting at 0).</param>
    /// <param name="pageSize">Number of records per page.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
    /// <param name="parameters">Parameters to use with the SQL.</param>
    /// <returns>An instance of <see cref="PagedQueryResult{TSource}"/> containing the page data (If the page number is past the end of the table then the it will become the last page).</returns>
    public static async Task<PagedQueryResult<TSource>> ExecutePagedQueryAsync<TSource>(this DatabaseFacade database, string sql, long page, long pageSize, CancellationToken cancellationToken = default, params IEnumerable<object?> parameters)
        where TSource : class
    {
        ArgumentNullException.ThrowIfNull(database);
        ArgumentException.ThrowIfNullOrEmpty(sql);
        ArgumentOutOfRangeException.ThrowIfNegative(page);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(pageSize);
        ArgumentNullException.ThrowIfNull(parameters);

        PagedQueryResult<TSource> pageResult = await ExecuteMethods.ExecutePagedQueryAsync<TSource>(database, sql, parameters, page, pageSize, cancellationToken).ConfigureAwait(false);
        return pageResult;
    }

    #endregion public ExecutePagedQuery methods

    #region public ExecuteQuery methods

    /// <summary>
    /// Executes a query.
    /// </summary>
    /// <typeparam name="TSource">The type of the elements of the source.</typeparam>
    /// <param name="database">The <see cref="DatabaseFacade"/> for the context.</param>
    /// <param name="sql">The <see cref="FormattableString"/> representing a SQL query with parameters.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
    /// <returns>A <see cref="IList{TSource}"/> containing the results of the query.</returns>
    public static async Task<IList<TSource>> ExecuteQueryAsync<TSource>(this DatabaseFacade database, FormattableString sql, CancellationToken cancellationToken = default)
        where TSource : class
    {
        ArgumentNullException.ThrowIfNull(database);
        ArgumentNullException.ThrowIfNull(sql);

        IList<TSource> results = await ExecuteMethods.ExecuteQueryAsync<TSource>(database, sql.Format, sql.GetArguments(), cancellationToken).ConfigureAwait(false);
        return results;
    }

    /// <summary>
    /// Executes a query.
    /// </summary>
    /// <typeparam name="TSource">The type of the elements of the source.</typeparam>
    /// <param name="database">The <see cref="DatabaseFacade"/> for the context.</param>
    /// <param name="sql">The SQL query to execute.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
    /// <param name="parameters">Parameters to use with the SQL.</param>
    /// <returns>A <see cref="IList{TSource}"/> containing the results of the query.</returns>
    public static async Task<IList<TSource>> ExecuteQueryAsync<TSource>(this DatabaseFacade database, string sql, CancellationToken cancellationToken = default, params IEnumerable<object?> parameters)
        where TSource : class
    {
        ArgumentNullException.ThrowIfNull(database);
        ArgumentException.ThrowIfNullOrEmpty(sql);
        ArgumentNullException.ThrowIfNull(parameters);

        IList<TSource> results = await ExecuteMethods.ExecuteQueryAsync<TSource>(database, sql, parameters, cancellationToken).ConfigureAwait(false);
        return results;
    }

    #endregion public ExecuteQuery methods

    #region public ExecuteScalar methods

    /// <summary>
    /// Executes a query with a single scalar result.
    /// </summary>
    /// <typeparam name="T">The type of result returned by the query.</typeparam>
    /// <param name="database">The <see cref="DatabaseFacade"/> for the context.</param>
    /// <param name="sql">The <see cref="FormattableString"/> representing a SQL query with parameters.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
    /// <returns>The result of the query.</returns>
    public static async Task<T> ExecuteScalarAsync<T>(this DatabaseFacade database, FormattableString sql, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(database);
        ArgumentNullException.ThrowIfNull(sql);

        T result = (await ExecuteMethods.ExecuteScalarAsync<T>(database, sql.Format, sql.GetArguments(), cancellationToken).ConfigureAwait(false))!;
        return result;
    }

    /// <summary>
    /// Executes a query with a single scalar result.
    /// </summary>
    /// <typeparam name="T">The type of result returned by the query.</typeparam>
    /// <param name="database">The <see cref="DatabaseFacade"/> for the context.</param>
    /// <param name="sql">The SQL query to execute.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
    /// <param name="parameters">Parameters to use with the SQL.</param>
    /// <returns>The result of the query.</returns>
    public static async Task<T> ExecuteScalarAsync<T>(this DatabaseFacade database, string sql, CancellationToken cancellationToken = default, params IEnumerable<object?> parameters)
    {
        ArgumentNullException.ThrowIfNull(database);
        ArgumentException.ThrowIfNullOrEmpty(sql);
        ArgumentNullException.ThrowIfNull(parameters);

        T result = await ExecuteMethods.ExecuteScalarAsync<T>(database, sql, parameters, cancellationToken).ConfigureAwait(false);
        return result;
    }

    #endregion public ExecuteScalar methods
}
