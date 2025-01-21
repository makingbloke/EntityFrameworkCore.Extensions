// Copyright ©2021-2025 Mike King.
// This file is licensed to you under the MIT license.
// See the License.txt file in the solution root for more information.

using DotDoc.EntityFrameworkCore.Extensions.Model;
using Microsoft.EntityFrameworkCore.Infrastructure;
using System.Data;

namespace DotDoc.EntityFrameworkCore.Extensions.Extensions;

/// <summary>
/// Entity Framework Core Query Exensions.
/// </summary>
public static class QueryExtensions
{
    #region public ExecuteScalar methods

    /// <summary>
    /// Executes a query with a single scalar result.
    /// </summary>
    /// <typeparam name="T">The type of result returned by the query.</typeparam>
    /// <param name="databaseFacade">The <see cref="DatabaseFacade"/> for the context.</param>
    /// <param name="sql">The <see cref="FormattableString"/> representing a SQL query with parameters.</param>
    /// <returns>The result of the query.</returns>
    public static T? ExecuteScalar<T>(this DatabaseFacade databaseFacade, FormattableString sql)
    {
        ArgumentNullException.ThrowIfNull(databaseFacade);
        ArgumentNullException.ThrowIfNull(sql);

        T? result = QueryMethods.ExecuteScalar<T>(databaseFacade, sql.Format, sql.GetArguments()!);
        return result;
    }

    /// <summary>
    /// Executes a query with a single scalar result.
    /// </summary>
    /// <typeparam name="T">The type of result returned by the query.</typeparam>
    /// <param name="databaseFacade">The <see cref="DatabaseFacade"/> for the context.</param>
    /// <param name="sql">The SQL query to execute.</param>
    /// <param name="parameters">Parameters to use with the SQL.</param>
    /// <returns>The result of the query.</returns>
    public static T? ExecuteScalar<T>(this DatabaseFacade databaseFacade, string sql, params IEnumerable<object?> parameters)
    {
        ArgumentNullException.ThrowIfNull(databaseFacade);
        ArgumentNullException.ThrowIfNull(sql);
        ArgumentNullException.ThrowIfNull(parameters);

        T? result = QueryMethods.ExecuteScalar<T>(databaseFacade, sql, parameters!);
        return result;
    }

    /// <summary>
    /// Executes a query with a single scalar result.
    /// </summary>
    /// <typeparam name="T">The type of result returned by the query.</typeparam>
    /// <param name="databaseFacade">The <see cref="DatabaseFacade"/> for the context.</param>
    /// <param name="sql">The <see cref="FormattableString"/> representing a SQL query with parameters.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
    /// <returns>The result of the query.</returns>
    public static async Task<T?> ExecuteScalarAsync<T>(this DatabaseFacade databaseFacade, FormattableString sql, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(databaseFacade);
        ArgumentNullException.ThrowIfNull(sql);

        T? result = await QueryMethods.ExecuteScalarAsync<T?>(databaseFacade, sql.Format, sql.GetArguments()!, cancellationToken).ConfigureAwait(false);
        return result;
    }

    /// <summary>
    /// Executes a query with a single scalar result.
    /// </summary>
    /// <typeparam name="T">The type of result returned by the query.</typeparam>
    /// <param name="databaseFacade">The <see cref="DatabaseFacade"/> for the context.</param>
    /// <param name="sql">The SQL query to execute.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
    /// <param name="parameters">Parameters to use with the SQL.</param>
    /// <returns>The result of the query.</returns>
    public static async Task<T?> ExecuteScalarAsync<T>(this DatabaseFacade databaseFacade, string sql, CancellationToken cancellationToken = default, params IEnumerable<object?> parameters)
    {
        ArgumentNullException.ThrowIfNull(databaseFacade);
        ArgumentNullException.ThrowIfNull(sql);
        ArgumentNullException.ThrowIfNull(parameters);

        T? result = await QueryMethods.ExecuteScalarAsync<T?>(databaseFacade, sql, parameters!, cancellationToken).ConfigureAwait(false);
        return result;
    }

    #endregion public ExecuteScalar methods

    #region public ExecuteQuery methods

    /// <summary>
    /// Executes a query.
    /// </summary>
    /// <param name="databaseFacade">The <see cref="DatabaseFacade"/> for the context.</param>
    /// <param name="sql">The <see cref="FormattableString"/> representing a SQL query with parameters.</param>
    /// <returns>A <see cref="DataTable"/> containing the results of the query.</returns>
    public static DataTable ExecuteQuery(this DatabaseFacade databaseFacade, FormattableString sql)
    {
        ArgumentNullException.ThrowIfNull(databaseFacade);
        ArgumentNullException.ThrowIfNull(sql);

        DataTable dataTable = QueryMethods.ExecuteQuery(databaseFacade, sql.Format, sql.GetArguments()!);
        return dataTable;
    }

    /// <summary>
    /// Executes a query.
    /// </summary>
    /// <param name="databaseFacade">The <see cref="DatabaseFacade"/> for the context.</param>
    /// <param name="sql">The SQL query to execute.</param>
    /// <param name="parameters">Parameters to use with the SQL.</param>
    /// <returns>A <see cref="DataTable"/> containing the results of the query.</returns>
    public static DataTable ExecuteQuery(this DatabaseFacade databaseFacade, string sql, params IEnumerable<object?> parameters)
    {
        ArgumentNullException.ThrowIfNull(databaseFacade);
        ArgumentNullException.ThrowIfNull(sql);
        ArgumentNullException.ThrowIfNull(parameters);

        DataTable dataTable = QueryMethods.ExecuteQuery(databaseFacade, sql, parameters!);
        return dataTable;
    }

    /// <summary>
    /// Executes a query.
    /// </summary>
    /// <typeparam name="TEntity">Type of Entity.</typeparam>
    /// <param name="databaseFacade">The <see cref="DatabaseFacade"/> for the context.</param>
    /// <param name="sql">The <see cref="FormattableString"/> representing a SQL query with parameters.</param>
    /// <returns>A <see cref="IList{TEntity}"/> containing the results of the query.</returns>
    public static IList<TEntity> ExecuteQuery<TEntity>(this DatabaseFacade databaseFacade, FormattableString sql)
        where TEntity : class
    {
        ArgumentNullException.ThrowIfNull(databaseFacade);
        ArgumentNullException.ThrowIfNull(sql);

        IList<TEntity> results = QueryMethods.ExecuteQuery<TEntity>(databaseFacade, sql.Format, sql.GetArguments()!);
        return results;
    }

    /// <summary>
    /// Executes a query.
    /// </summary>
    /// <typeparam name="TEntity">Type of Entity.</typeparam>
    /// <param name="databaseFacade">The <see cref="DatabaseFacade"/> for the context.</param>
    /// <param name="sql">The SQL query to execute.</param>
    /// <param name="parameters">Parameters to use with the SQL.</param>
    /// <returns>A <see cref="IList{TEntity}"/> containing the results of the query.</returns>
    public static IList<TEntity> ExecuteQuery<TEntity>(this DatabaseFacade databaseFacade, string sql, params IEnumerable<object?> parameters)
        where TEntity : class
    {
        ArgumentNullException.ThrowIfNull(databaseFacade);
        ArgumentNullException.ThrowIfNull(sql);
        ArgumentNullException.ThrowIfNull(parameters);

        IList<TEntity> results = QueryMethods.ExecuteQuery<TEntity>(databaseFacade, sql, parameters!);
        return results;
    }

    /// <summary>
    /// Executes a query.
    /// </summary>
    /// <param name="databaseFacade">The <see cref="DatabaseFacade"/> for the context.</param>
    /// <param name="sql">The <see cref="FormattableString"/> representing a SQL query with parameters.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
    /// <returns>A <see cref="DataTable"/> containing the results of the query.</returns>
    public static async Task<DataTable> ExecuteQueryAsync(this DatabaseFacade databaseFacade, FormattableString sql, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(databaseFacade);
        ArgumentNullException.ThrowIfNull(sql);

        DataTable dataTable = await QueryMethods.ExecuteQueryAsync(databaseFacade, sql.Format, sql.GetArguments()!, cancellationToken).ConfigureAwait(false);
        return dataTable;
    }

    /// <summary>
    /// Executes a query.
    /// </summary>
    /// <param name="databaseFacade">The <see cref="DatabaseFacade"/> for the context.</param>
    /// <param name="sql">The SQL query to execute.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
    /// <param name="parameters">Parameters to use with the SQL.</param>
    /// <returns>A <see cref="DataTable"/> containing the results of the query.</returns>
    public static async Task<DataTable> ExecuteQueryAsync(this DatabaseFacade databaseFacade, string sql, CancellationToken cancellationToken = default, params IEnumerable<object?> parameters)
    {
        ArgumentNullException.ThrowIfNull(databaseFacade);
        ArgumentNullException.ThrowIfNull(sql);
        ArgumentNullException.ThrowIfNull(parameters);

        DataTable dataTable = await QueryMethods.ExecuteQueryAsync(databaseFacade, sql, parameters!, cancellationToken).ConfigureAwait(false);
        return dataTable;
    }

    /// <summary>
    /// Executes a query.
    /// </summary>
    /// <typeparam name="TEntity">Type of Entity.</typeparam>
    /// <param name="databaseFacade">The <see cref="DatabaseFacade"/> for the context.</param>
    /// <param name="sql">The <see cref="FormattableString"/> representing a SQL query with parameters.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
    /// <returns>A <see cref="IList{TEntity}"/> containing the results of the query.</returns>
    public static async Task<IList<TEntity>> ExecuteQueryAsync<TEntity>(this DatabaseFacade databaseFacade, FormattableString sql, CancellationToken cancellationToken = default)
        where TEntity : class
    {
        ArgumentNullException.ThrowIfNull(databaseFacade);
        ArgumentNullException.ThrowIfNull(sql);

        IList<TEntity> results = await QueryMethods.ExecuteQueryAsync<TEntity>(databaseFacade, sql.Format, sql.GetArguments()!, cancellationToken).ConfigureAwait(false);
        return results;
    }

    /// <summary>
    /// Executes a query.
    /// </summary>
    /// <typeparam name="TEntity">Type of Entity.</typeparam>
    /// <param name="databaseFacade">The <see cref="DatabaseFacade"/> for the context.</param>
    /// <param name="sql">The SQL query to execute.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
    /// <param name="parameters">Parameters to use with the SQL.</param>
    /// <returns>A <see cref="IList{TEntity}"/> containing the results of the query.</returns>
    public static async Task<IList<TEntity>> ExecuteQueryAsync<TEntity>(this DatabaseFacade databaseFacade, string sql, CancellationToken cancellationToken = default, params IEnumerable<object?> parameters)
        where TEntity : class
    {
        ArgumentNullException.ThrowIfNull(databaseFacade);
        ArgumentNullException.ThrowIfNull(sql);
        ArgumentNullException.ThrowIfNull(parameters);

        IList<TEntity> results = await QueryMethods.ExecuteQueryAsync<TEntity>(databaseFacade, sql, parameters!, cancellationToken).ConfigureAwait(false);
        return results;
    }

    #endregion public ExecuteQuery methods

    #region public ExecutePagedQuery methods

    /// <summary>
    /// Executes a query and returns the specified page of results.
    /// </summary>
    /// <param name="databaseFacade">The <see cref="DatabaseFacade"/> for the context.</param>
    /// <param name="sql">The <see cref="FormattableString"/> representing a SQL query with parameters.</param>
    /// <param name="page">Page number to return (starting at 0).</param>
    /// <param name="pageSize">Number of records per page.</param>
    /// <returns>An instance of <see cref="QueryPageTable"/> containing the page data (If the page number is past the end of the table then the it will become the last page).</returns>
    public static QueryPageTable ExecutePagedQuery(this DatabaseFacade databaseFacade, FormattableString sql, long page, long pageSize)
    {
        ArgumentNullException.ThrowIfNull(databaseFacade);
        ArgumentNullException.ThrowIfNull(sql);
        ArgumentOutOfRangeException.ThrowIfNegative(page);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(pageSize);

        QueryPageTable queryPage = QueryMethods.ExecutePagedQuery(databaseFacade, sql.Format, sql.GetArguments()!, page, pageSize);
        return queryPage;
    }

    /// <summary>
    /// Executes a query and returns the specified page of results.
    /// </summary>
    /// <param name="databaseFacade">The <see cref="DatabaseFacade"/> for the context.</param>
    /// <param name="sql">The SQL query to execute.</param>
    /// <param name="page">Page number to return (starting at 0).</param>
    /// <param name="pageSize">Number of records per page.</param>
    /// <param name="parameters">Parameters to use with the SQL.</param>
    /// <returns>An instance of <see cref="QueryPageTable"/> containing the page data (If the page number is past the end of the table then the it will become the last page).</returns>
    public static QueryPageTable ExecutePagedQuery(this DatabaseFacade databaseFacade, string sql, long page, long pageSize, params IEnumerable<object?> parameters)
    {
        ArgumentNullException.ThrowIfNull(databaseFacade);
        ArgumentNullException.ThrowIfNull(sql);
        ArgumentOutOfRangeException.ThrowIfNegative(page);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(pageSize);
        ArgumentNullException.ThrowIfNull(parameters);

        QueryPageTable queryPage = QueryMethods.ExecutePagedQuery(databaseFacade, sql, parameters!, page, pageSize);
        return queryPage;
    }

    /// <summary>
    /// Executes a query and returns the specified page of results.
    /// </summary>
    /// <typeparam name="TEntity">Type of Entity.</typeparam>
    /// <param name="databaseFacade">The <see cref="DatabaseFacade"/> for the context.</param>
    /// <param name="sql">The <see cref="FormattableString"/> representing a SQL query with parameters.</param>
    /// <param name="page">Page number to return (starting at 0).</param>
    /// <param name="pageSize">Number of records per page.</param>
    /// <returns>An instance of <see cref="QueryPageEntity{TEntity}"/> containing the page data (If the page number is past the end of the table then the it will become the last page).</returns>
    public static QueryPageEntity<TEntity> ExecutePagedQuery<TEntity>(this DatabaseFacade databaseFacade, FormattableString sql, long page, long pageSize)
        where TEntity : class
    {
        ArgumentNullException.ThrowIfNull(databaseFacade);
        ArgumentNullException.ThrowIfNull(sql);
        ArgumentOutOfRangeException.ThrowIfNegative(page);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(pageSize);

        QueryPageEntity<TEntity> queryPage = QueryMethods.ExecutePagedQuery<TEntity>(databaseFacade, sql.Format, sql.GetArguments()!, page, pageSize);
        return queryPage;
    }

    /// <summary>
    /// Executes a query and returns the specified page of results.
    /// </summary>
    /// <typeparam name="TEntity">Type of Entity.</typeparam>
    /// <param name="databaseFacade">The <see cref="DatabaseFacade"/> for the context.</param>
    /// <param name="sql">The SQL query to execute.</param>
    /// <param name="page">Page number to return (starting at 0).</param>
    /// <param name="pageSize">Number of records per page.</param>
    /// <param name="parameters">Parameters to use with the SQL.</param>
    /// <returns>An instance of <see cref="QueryPageEntity{TEntity}"/> containing the page data (If the page number is past the end of the table then the it will become the last page).</returns>
    public static QueryPageEntity<TEntity> ExecutePagedQuery<TEntity>(this DatabaseFacade databaseFacade, string sql, long page, long pageSize, params IEnumerable<object?> parameters)
        where TEntity : class
    {
        ArgumentNullException.ThrowIfNull(databaseFacade);
        ArgumentNullException.ThrowIfNull(sql);
        ArgumentOutOfRangeException.ThrowIfNegative(page);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(pageSize);
        ArgumentNullException.ThrowIfNull(parameters);

        QueryPageEntity<TEntity> queryPage = QueryMethods.ExecutePagedQuery<TEntity>(databaseFacade, sql, parameters!, page, pageSize);
        return queryPage;
    }

    /// <summary>
    /// Executes a query and returns the specified page of results.
    /// </summary>
    /// <param name="databaseFacade">The <see cref="DatabaseFacade"/> for the context.</param>
    /// <param name="sql">The <see cref="FormattableString"/> representing a SQL query with parameters.</param>
    /// <param name="page">Page number to return (starting at 0).</param>
    /// <param name="pageSize">Number of records per page.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
    /// <returns>An instance of <see cref="QueryPageTable"/> containing the page data (If the page number is past the end of the table then the it will become the last page).</returns>
    public static async Task<QueryPageTable> ExecutePagedQueryAsync(this DatabaseFacade databaseFacade, FormattableString sql, long page, long pageSize, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(databaseFacade);
        ArgumentNullException.ThrowIfNull(sql);
        ArgumentOutOfRangeException.ThrowIfNegative(page);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(pageSize);

        QueryPageTable queryPage = await QueryMethods.ExecutePagedQueryAsync(databaseFacade, sql.Format, sql.GetArguments()!, page, pageSize, cancellationToken).ConfigureAwait(false);
        return queryPage;
    }

    /// <summary>
    /// Executes a query and returns the specified page of results.
    /// </summary>
    /// <param name="databaseFacade">The <see cref="DatabaseFacade"/> for the context.</param>
    /// <param name="sql">The SQL query to execute.</param>
    /// <param name="page">Page number to return (starting at 0).</param>
    /// <param name="pageSize">Number of records per page.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
    /// <param name="parameters">Parameters to use with the SQL.</param>
    /// <returns>An instance of <see cref="QueryPageEntity{TEntity}"/> containing the page data (If the page number is past the end of the table then the it will become the last page).</returns>
    public static async Task<QueryPageTable> ExecutePagedQueryAsync(this DatabaseFacade databaseFacade, string sql, long page, long pageSize, CancellationToken cancellationToken = default, params IEnumerable<object?> parameters)
    {
        ArgumentNullException.ThrowIfNull(databaseFacade);
        ArgumentNullException.ThrowIfNull(sql);
        ArgumentOutOfRangeException.ThrowIfNegative(page);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(pageSize);
        ArgumentNullException.ThrowIfNull(parameters);

        QueryPageTable queryPage = await QueryMethods.ExecutePagedQueryAsync(databaseFacade, sql, parameters!, page, pageSize, cancellationToken).ConfigureAwait(false);
        return queryPage;
    }

    /// <summary>
    /// Executes a query and returns the specified page of results.
    /// </summary>
    /// <typeparam name="TEntity">Type of Entity.</typeparam>
    /// <param name="databaseFacade">The <see cref="DatabaseFacade"/> for the context.</param>
    /// <param name="sql">The <see cref="FormattableString"/> representing a SQL query with parameters.</param>
    /// <param name="page">Page number to return (starting at 0).</param>
    /// <param name="pageSize">Number of records per page.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
    /// <returns>An instance of <see cref="QueryPageEntity{TEntity}"/> containing the page data (If the page number is past the end of the table then the it will become the last page).</returns>
    public static async Task<QueryPageEntity<TEntity>> ExecutePagedQueryAsync<TEntity>(this DatabaseFacade databaseFacade, FormattableString sql, long page, long pageSize, CancellationToken cancellationToken = default)
        where TEntity : class
    {
        ArgumentNullException.ThrowIfNull(databaseFacade);
        ArgumentNullException.ThrowIfNull(sql);
        ArgumentOutOfRangeException.ThrowIfNegative(page);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(pageSize);

        QueryPageEntity<TEntity> queryPage = await QueryMethods.ExecutePagedQueryAsync<TEntity>(databaseFacade, sql.Format, sql.GetArguments()!, page, pageSize, cancellationToken).ConfigureAwait(false);
        return queryPage;
    }

    /// <summary>
    /// Executes a query and returns the specified page of results.
    /// </summary>
    /// <typeparam name="TEntity">Type of Entity.</typeparam>
    /// <param name="databaseFacade">The <see cref="DatabaseFacade"/> for the context.</param>
    /// <param name="sql">The SQL query to execute.</param>
    /// <param name="page">Page number to return (starting at 0).</param>
    /// <param name="pageSize">Number of records per page.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
    /// <param name="parameters">Parameters to use with the SQL.</param>
    /// <returns>An instance of <see cref="QueryPageTable"/> containing the page data (If the page number is past the end of the table then the it will become the last page).</returns>
    public static async Task<QueryPageEntity<TEntity>> ExecutePagedQueryAsync<TEntity>(this DatabaseFacade databaseFacade, string sql, long page, long pageSize, CancellationToken cancellationToken = default, params IEnumerable<object?> parameters)
        where TEntity : class
    {
        ArgumentNullException.ThrowIfNull(databaseFacade);
        ArgumentNullException.ThrowIfNull(sql);
        ArgumentOutOfRangeException.ThrowIfNegative(page);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(pageSize);
        ArgumentNullException.ThrowIfNull(parameters);

        QueryPageEntity<TEntity> queryPage = await QueryMethods.ExecutePagedQueryAsync<TEntity>(databaseFacade, sql, parameters!, page, pageSize, cancellationToken).ConfigureAwait(false);
        return queryPage;
    }

    #endregion public ExecutePagedQuery methods

    #region public ExecuteNonQuery methods

    /// <summary>
    /// Executes a non query.
    /// </summary>
    /// <param name="databaseFacade">The <see cref="DatabaseFacade"/> for the context.</param>
    /// <param name="sql">The <see cref="FormattableString"/> representing a SQL query with parameters.</param>
    /// <returns>The number of rows affected.</returns>
    public static int ExecuteNonQuery(this DatabaseFacade databaseFacade, FormattableString sql)
    {
        ArgumentNullException.ThrowIfNull(databaseFacade);
        ArgumentNullException.ThrowIfNull(sql);

        int count = QueryMethods.ExecuteNonQuery(databaseFacade, sql.Format, sql.GetArguments()!);
        return count;
    }

    /// <summary>
    /// Executes a non query.
    /// </summary>
    /// <param name="databaseFacade">The <see cref="DatabaseFacade"/> for the context.</param>
    /// <param name="sql">The SQL query to execute.</param>
    /// <param name="parameters">Parameters to use with the SQL.</param>
    /// <returns>The number of rows affected.</returns>
    public static int ExecuteNonQuery(this DatabaseFacade databaseFacade, string sql, params IEnumerable<object?> parameters)
    {
        ArgumentNullException.ThrowIfNull(databaseFacade);
        ArgumentNullException.ThrowIfNull(sql);
        ArgumentNullException.ThrowIfNull(parameters);

        int count = QueryMethods.ExecuteNonQuery(databaseFacade, sql, parameters!);
        return count;
    }

    /// <summary>
    /// Executes a non query.
    /// </summary>
    /// <param name="databaseFacade">The <see cref="DatabaseFacade"/> for the context.</param>
    /// <param name="sql">The <see cref="FormattableString"/> representing a SQL query with parameters.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
    /// <returns>The number of rows affected.</returns>
    public static async Task<int> ExecuteNonQueryAsync(this DatabaseFacade databaseFacade, FormattableString sql, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(databaseFacade);
        ArgumentNullException.ThrowIfNull(sql);

        int count = await QueryMethods.ExecuteNonQueryAsync(databaseFacade, sql.Format, sql.GetArguments()!, cancellationToken).ConfigureAwait(false);
        return count;
    }

    /// <summary>
    /// Executes a non query.
    /// </summary>
    /// <param name="databaseFacade">The <see cref="DatabaseFacade"/> for the context.</param>
    /// <param name="sql">The SQL query to execute.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
    /// <param name="parameters">Parameters to use with the SQL.</param>
    /// <returns>The number of rows affected.</returns>
    public static async Task<int> ExecuteNonQueryAsync(this DatabaseFacade databaseFacade, string sql, CancellationToken cancellationToken = default, params IEnumerable<object?> parameters)
    {
        ArgumentNullException.ThrowIfNull(databaseFacade);
        ArgumentNullException.ThrowIfNull(sql);
        ArgumentNullException.ThrowIfNull(parameters);

        int count = await QueryMethods.ExecuteNonQueryAsync(databaseFacade, sql, parameters!, cancellationToken).ConfigureAwait(false);
        return count;
    }

    #endregion public ExecuteNonQuery methods

    #region public ExecuteInsert methods

    /// <summary>
    /// Executes an insert command.
    /// </summary>
    /// <param name="databaseFacade">The <see cref="DatabaseFacade"/> for the context.</param>
    /// <param name="sql">The <see cref="FormattableString"/> representing a SQL query with parameters.</param>
    /// <returns>The Id of the new record.</returns>
    public static long ExecuteInsert(this DatabaseFacade databaseFacade, FormattableString sql)
    {
        ArgumentNullException.ThrowIfNull(databaseFacade);
        ArgumentNullException.ThrowIfNull(sql);

        long id = QueryMethods.ExecuteInsert<long>(databaseFacade, sql.Format, sql.GetArguments()!);
        return id;
    }

    /// <summary>
    /// Executes an insert command.
    /// </summary>
    /// <typeparam name="T">The type returned by the insert.</typeparam>
    /// <param name="databaseFacade">The <see cref="DatabaseFacade"/> for the context.</param>
    /// <param name="sql">The <see cref="FormattableString"/> representing a SQL query with parameters.</param>
    /// <returns>The Id of the new record.</returns>
    public static T? ExecuteInsert<T>(this DatabaseFacade databaseFacade, FormattableString sql)
    {
        ArgumentNullException.ThrowIfNull(databaseFacade);
        ArgumentNullException.ThrowIfNull(sql);

        T? id = QueryMethods.ExecuteInsert<T?>(databaseFacade, sql.Format, sql.GetArguments()!);
        return id;
    }

    /// <summary>
    /// Executes an insert command.
    /// </summary>
    /// <param name="databaseFacade">The <see cref="DatabaseFacade"/> for the context.</param>
    /// <param name="sql">The SQL query to execute.</param>
    /// <param name="parameters">Parameters to use with the SQL.</param>
    /// <returns>The Id of the new record.</returns>
    public static long ExecuteInsert(this DatabaseFacade databaseFacade, string sql, params IEnumerable<object?> parameters)
    {
        ArgumentNullException.ThrowIfNull(databaseFacade);
        ArgumentNullException.ThrowIfNull(sql);
        ArgumentNullException.ThrowIfNull(parameters);

        long id = QueryMethods.ExecuteInsert<long>(databaseFacade, sql, parameters!);
        return id;
    }

    /// <summary>
    /// Executes an insert command.
    /// </summary>
    /// <typeparam name="T">The type returned by the insert.</typeparam>
    /// <param name="databaseFacade">The <see cref="DatabaseFacade"/> for the context.</param>
    /// <param name="sql">The SQL query to execute.</param>
    /// <param name="parameters">Parameters to use with the SQL.</param>
    /// <returns>The Id of the new record.</returns>
    public static T? ExecuteInsert<T>(this DatabaseFacade databaseFacade, string sql, params IEnumerable<object?> parameters)
    {
        ArgumentNullException.ThrowIfNull(databaseFacade);
        ArgumentNullException.ThrowIfNull(sql);
        ArgumentNullException.ThrowIfNull(parameters);

        T? id = QueryMethods.ExecuteInsert<T?>(databaseFacade, sql, parameters!);
        return id;
    }

    /// <summary>
    /// Executes an insert command.
    /// </summary>
    /// <param name="databaseFacade">The <see cref="DatabaseFacade"/> for the context.</param>
    /// <param name="sql">The <see cref="FormattableString"/> representing a SQL query with parameters.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
    /// <returns>The Id of the new record.</returns>
    public static async Task<long> ExecuteInsertAsync(this DatabaseFacade databaseFacade, FormattableString sql, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(databaseFacade);
        ArgumentNullException.ThrowIfNull(sql);

        long id = await QueryMethods.ExecuteInsertAsync<long>(databaseFacade, sql.Format, sql.GetArguments()!, cancellationToken).ConfigureAwait(false);
        return id;
    }

    /// <summary>
    /// Executes an insert command.
    /// </summary>
    /// <typeparam name="T">The type returned by the insert.</typeparam>
    /// <param name="databaseFacade">The <see cref="DatabaseFacade"/> for the context.</param>
    /// <param name="sql">The <see cref="FormattableString"/> representing a SQL query with parameters.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
    /// <returns>The Id of the new record.</returns>
    public static async Task<T?> ExecuteInsertAsync<T>(this DatabaseFacade databaseFacade, FormattableString sql, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(databaseFacade);
        ArgumentNullException.ThrowIfNull(sql);

        T? id = await QueryMethods.ExecuteInsertAsync<T>(databaseFacade, sql.Format, sql.GetArguments()!, cancellationToken).ConfigureAwait(false);
        return id;
    }

    /// <summary>
    /// Executes an insert command.
    /// </summary>
    /// <param name="databaseFacade">The <see cref="DatabaseFacade"/> for the context.</param>
    /// <param name="sql">The SQL query to execute.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
    /// <param name="parameters">Parameters to use with the SQL.</param>
    /// <returns>The Id of the new record.</returns>
    public static async Task<long> ExecuteInsertAsync(this DatabaseFacade databaseFacade, string sql, CancellationToken cancellationToken = default, params IEnumerable<object?> parameters)
    {
        ArgumentNullException.ThrowIfNull(databaseFacade);
        ArgumentNullException.ThrowIfNull(sql);
        ArgumentNullException.ThrowIfNull(parameters);

        long id = await QueryMethods.ExecuteInsertAsync<long>(databaseFacade, sql, parameters!, cancellationToken).ConfigureAwait(false);
        return id;
    }

    /// <summary>
    /// Executes an insert command.
    /// </summary>
    /// <typeparam name="T">The type returned by the insert.</typeparam>
    /// <param name="databaseFacade">The <see cref="DatabaseFacade"/> for the context.</param>
    /// <param name="sql">The SQL query to execute.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
    /// <param name="parameters">Parameters to use with the SQL.</param>
    /// <returns>The Id of the new record.</returns>
    public static async Task<T?> ExecuteInsertAsync<T>(this DatabaseFacade databaseFacade, string sql, CancellationToken cancellationToken = default, params IEnumerable<object?> parameters)
    {
        ArgumentNullException.ThrowIfNull(databaseFacade);
        ArgumentNullException.ThrowIfNull(sql);
        ArgumentNullException.ThrowIfNull(parameters);

        T? id = await QueryMethods.ExecuteInsertAsync<T>(databaseFacade, sql, parameters!, cancellationToken).ConfigureAwait(false);
        return id;
    }

    #endregion public ExecuteInsert methods
}
