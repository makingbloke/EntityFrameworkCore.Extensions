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
    public static T ExecuteScalar<T>(this DatabaseFacade databaseFacade, FormattableString sql) =>
        QueryMethods.ExecuteScalar<T>(databaseFacade, sql.Format, sql.GetArguments());

    /// <summary>
    /// Executes a query with a single scalar result.
    /// </summary>
    /// <typeparam name="T">The type of result returned by the query.</typeparam>
    /// <param name="databaseFacade">The <see cref="DatabaseFacade"/> for the context.</param>
    /// <param name="sql">The SQL query to execute.</param>
    /// <param name="parameters">Parameters to use with the SQL.</param>
    /// <returns>The result of the query.</returns>
    public static T ExecuteScalar<T>(this DatabaseFacade databaseFacade, string sql, params object[] parameters) =>
        QueryMethods.ExecuteScalar<T>(databaseFacade, sql, parameters);

    /// <summary>
    /// Executes a query with a single scalar result.
    /// </summary>
    /// <typeparam name="T">The type of result returned by the query.</typeparam>
    /// <param name="databaseFacade">The <see cref="DatabaseFacade"/> for the context.</param>
    /// <param name="sql">The SQL query to execute.</param>
    /// <param name="parameters">Parameters to use with the SQL.</param>
    /// <returns>The result of the query.</returns>
    public static T ExecuteScalar<T>(this DatabaseFacade databaseFacade, string sql, IEnumerable<object> parameters) =>
        QueryMethods.ExecuteScalar<T>(databaseFacade, sql, parameters);

    /// <summary>
    /// Executes a query with a single scalar result.
    /// </summary>
    /// <typeparam name="T">The type of result returned by the query.</typeparam>
    /// <param name="databaseFacade">The <see cref="DatabaseFacade"/> for the context.</param>
    /// <param name="sql">The <see cref="FormattableString"/> representing a SQL query with parameters.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
    /// <returns>The result of the query.</returns>
    public static Task<T> ExecuteScalarAsync<T>(this DatabaseFacade databaseFacade, FormattableString sql, CancellationToken cancellationToken = default) =>
        QueryMethods.ExecuteScalarAsync<T>(databaseFacade, sql.Format, sql.GetArguments(), cancellationToken);

    /// <summary>
    /// Executes a query with a single scalar result.
    /// </summary>
    /// <typeparam name="T">The type of result returned by the query.</typeparam>
    /// <param name="databaseFacade">The <see cref="DatabaseFacade"/> for the context.</param>
    /// <param name="sql">The SQL query to execute.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
    /// <param name="parameters">Parameters to use with the SQL.</param>
    /// <returns>The result of the query.</returns>
    public static Task<T> ExecuteScalarAsync<T>(this DatabaseFacade databaseFacade, string sql, CancellationToken cancellationToken = default, params object[] parameters) =>
        QueryMethods.ExecuteScalarAsync<T>(databaseFacade, sql, parameters, cancellationToken);

    /// <summary>
    /// Executes a query with a single scalar result.
    /// </summary>
    /// <typeparam name="T">The type of result returned by the query.</typeparam>
    /// <param name="databaseFacade">The <see cref="DatabaseFacade"/> for the context.</param>
    /// <param name="sql">The SQL query to execute.</param>
    /// <param name="parameters">Parameters to use with the SQL.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
    /// <returns>The result of the query.</returns>
    public static Task<T> ExecuteScalarAsync<T>(this DatabaseFacade databaseFacade, string sql, IEnumerable<object> parameters, CancellationToken cancellationToken = default) =>
        QueryMethods.ExecuteScalarAsync<T>(databaseFacade, sql, parameters, cancellationToken);

    #endregion public ExecuteScalar methods

    #region public ExecuteQuery methods

    /// <summary>
    /// Executes a query.
    /// </summary>
    /// <param name="databaseFacade">The <see cref="DatabaseFacade"/> for the context.</param>
    /// <param name="sql">The <see cref="FormattableString"/> representing a SQL query with parameters.</param>
    /// <returns>A <see cref="DataTable"/> containing the results of the query.</returns>
    public static DataTable ExecuteQuery(this DatabaseFacade databaseFacade, FormattableString sql) =>
        QueryMethods.ExecuteQuery(databaseFacade, sql.Format, sql.GetArguments());

    /// <summary>
    /// Executes a query.
    /// </summary>
    /// <param name="databaseFacade">The <see cref="DatabaseFacade"/> for the context.</param>
    /// <param name="sql">The SQL query to execute.</param>
    /// <param name="parameters">Parameters to use with the SQL.</param>
    /// <returns>A <see cref="DataTable"/> containing the results of the query.</returns>
    public static DataTable ExecuteQuery(this DatabaseFacade databaseFacade, string sql, params object[] parameters) =>
        QueryMethods.ExecuteQuery(databaseFacade, sql, parameters);

    /// <summary>
    /// Executes a query.
    /// </summary>
    /// <param name="databaseFacade">The <see cref="DatabaseFacade"/> for the context.</param>
    /// <param name="sql">The SQL query to execute.</param>
    /// <param name="parameters">Parameters to use with the SQL.</param>
    /// <returns>A <see cref="DataTable"/> containing the results of the query.</returns>
    public static DataTable ExecuteQuery(this DatabaseFacade databaseFacade, string sql, IEnumerable<object> parameters) =>
        QueryMethods.ExecuteQuery(databaseFacade, sql, parameters);

    /// <summary>
    /// Executes a query.
    /// </summary>
    /// <param name="databaseFacade">The <see cref="DatabaseFacade"/> for the context.</param>
    /// <param name="sql">The <see cref="FormattableString"/> representing a SQL query with parameters.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
    /// <returns>A <see cref="DataTable"/> containing the results of the query.</returns>
    public static Task<DataTable> ExecuteQueryAsync(this DatabaseFacade databaseFacade, FormattableString sql, CancellationToken cancellationToken = default) =>
        QueryMethods.ExecuteQueryAsync(databaseFacade, sql.Format, sql.GetArguments(), cancellationToken);

    /// <summary>
    /// Executes a query.
    /// </summary>
    /// <param name="databaseFacade">The <see cref="DatabaseFacade"/> for the context.</param>
    /// <param name="sql">The SQL query to execute.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
    /// <param name="parameters">Parameters to use with the SQL.</param>
    /// <returns>A <see cref="DataTable"/> containing the results of the query.</returns>
    public static Task<DataTable> ExecuteQueryAsync(this DatabaseFacade databaseFacade, string sql, CancellationToken cancellationToken = default, params object[] parameters) =>
        QueryMethods.ExecuteQueryAsync(databaseFacade, sql, parameters, cancellationToken);

    /// <summary>
    /// Executes a query.
    /// </summary>
    /// <param name="databaseFacade">The <see cref="DatabaseFacade"/> for the context.</param>
    /// <param name="sql">The SQL query to execute.</param>
    /// <param name="parameters">Parameters to use with the SQL.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
    /// <returns>A <see cref="DataTable"/> containing the results of the query.</returns>
    public static Task<DataTable> ExecuteQueryAsync(this DatabaseFacade databaseFacade, string sql, IEnumerable<object> parameters, CancellationToken cancellationToken = default) =>
        QueryMethods.ExecuteQueryAsync(databaseFacade, sql, parameters, cancellationToken);

    #endregion public ExecuteQuery methods

    #region public ExecutePagedQuery methods

    /// <summary>
    /// Executes a query and returns the specified page of results.
    /// </summary>
    /// <param name="databaseFacade">The <see cref="DatabaseFacade"/> for the context.</param>
    /// <param name="sql">The <see cref="FormattableString"/> representing a SQL query with parameters.</param>
    /// <param name="page">Page number to return (starting at 0).</param>
    /// <param name="pageSize">Number of records per page.</param>
    /// <returns>An instance of <see cref="QueryPage"/> containing the page data (If the page number is past the end of the table then the it will become the last page).</returns>
    public static QueryPage ExecutePagedQuery(this DatabaseFacade databaseFacade, FormattableString sql, long page, long pageSize) =>
        QueryMethods.ExecutePagedQuery(databaseFacade, sql.Format, sql.GetArguments(), page, pageSize);

    /// <summary>
    /// Executes a query and returns the specified page of results.
    /// </summary>
    /// <param name="databaseFacade">The <see cref="DatabaseFacade"/> for the context.</param>
    /// <param name="sql">The SQL query to execute.</param>
    /// <param name="page">Page number to return (starting at 0).</param>
    /// <param name="pageSize">Number of records per page.</param>
    /// <param name="parameters">Parameters to use with the SQL.</param>
    /// <returns>An instance of <see cref="QueryPage"/> containing the page data (If the page number is past the end of the table then the it will become the last page).</returns>
    public static QueryPage ExecutePagedQuery(this DatabaseFacade databaseFacade, string sql, long page, long pageSize, params object[] parameters) =>
        QueryMethods.ExecutePagedQuery(databaseFacade, sql, parameters, page, pageSize);

    /// <summary>
    /// Executes a query and returns the specified page of results.
    /// </summary>
    /// <param name="databaseFacade">The <see cref="DatabaseFacade"/> for the context.</param>
    /// <param name="sql">The SQL query to execute.</param>
    /// <param name="page">Page number to return (starting at 0).</param>
    /// <param name="pageSize">Number of records per page.</param>
    /// <param name="parameters">Parameters to use with the SQL.</param>
    /// <returns>An instance of <see cref="QueryPage"/> containing the page data (If the page number is past the end of the table then the it will become the last page).</returns>
    public static QueryPage ExecutePagedQuery(this DatabaseFacade databaseFacade, string sql, long page, long pageSize, IEnumerable<object> parameters) =>
        QueryMethods.ExecutePagedQuery(databaseFacade, sql, parameters, page, pageSize);

    /// <summary>
    /// Executes a query and returns the specified page of results.
    /// </summary>
    /// <param name="databaseFacade">The <see cref="DatabaseFacade"/> for the context.</param>
    /// <param name="sql">The <see cref="FormattableString"/> representing a SQL query with parameters.</param>
    /// <param name="page">Page number to return (starting at 0).</param>
    /// <param name="pageSize">Number of records per page.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
    /// <returns>An instance of <see cref="QueryPage"/> containing the page data (If the page number is past the end of the table then the it will become the last page).</returns>
    public static Task<QueryPage> ExecutePagedQueryAsync(this DatabaseFacade databaseFacade, FormattableString sql, long page, long pageSize, CancellationToken cancellationToken = default) =>
        QueryMethods.ExecutePagedQueryAsync(databaseFacade, sql.Format, sql.GetArguments(), page, pageSize, cancellationToken);

    /// <summary>
    /// Executes a query and returns the specified page of results.
    /// </summary>
    /// <param name="databaseFacade">The <see cref="DatabaseFacade"/> for the context.</param>
    /// <param name="sql">The SQL query to execute.</param>
    /// <param name="page">Page number to return (starting at 0).</param>
    /// <param name="pageSize">Number of records per page.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
    /// <param name="parameters">Parameters to use with the SQL.</param>
    /// <returns>An instance of <see cref="QueryPage"/> containing the page data (If the page number is past the end of the table then the it will become the last page).</returns>
    public static Task<QueryPage> ExecutePagedQueryAsync(this DatabaseFacade databaseFacade, string sql, long page, long pageSize, CancellationToken cancellationToken = default, params object[] parameters) =>
        QueryMethods.ExecutePagedQueryAsync(databaseFacade, sql, parameters, page, pageSize, cancellationToken);

    /// <summary>
    /// Executes a query and returns the specified page of results.
    /// </summary>
    /// <param name="databaseFacade">The <see cref="DatabaseFacade"/> for the context.</param>
    /// <param name="sql">The SQL query to execute.</param>
    /// <param name="page">Page number to return (starting at 0).</param>
    /// <param name="pageSize">Number of records per page.</param>
    /// <param name="parameters">Parameters to use with the SQL.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
    /// <returns>An instance of <see cref="QueryPage"/> containing the page data (If the page number is past the end of the table then the it will become the last page).</returns>
    public static Task<QueryPage> ExecutePagedQueryAsync(this DatabaseFacade databaseFacade, string sql, long page, long pageSize, IEnumerable<object> parameters, CancellationToken cancellationToken = default) =>
        QueryMethods.ExecutePagedQueryAsync(databaseFacade, sql, parameters, page, pageSize, cancellationToken);

    #endregion public ExecutePagedQuery methods

    #region public ExecuteNonQuery methods

    /// <summary>
    /// Executes a non query.
    /// </summary>
    /// <param name="databaseFacade">The <see cref="DatabaseFacade"/> for the context.</param>
    /// <param name="sql">The <see cref="FormattableString"/> representing a SQL query with parameters.</param>
    /// <returns>The number of rows affected.</returns>
    public static int ExecuteNonQuery(this DatabaseFacade databaseFacade, FormattableString sql) =>
        QueryMethods.ExecuteNonQuery(databaseFacade, sql.Format, sql.GetArguments());

    /// <summary>
    /// Executes a non query.
    /// </summary>
    /// <param name="databaseFacade">The <see cref="DatabaseFacade"/> for the context.</param>
    /// <param name="sql">The SQL query to execute.</param>
    /// <param name="parameters">Parameters to use with the SQL.</param>
    /// <returns>The number of rows affected.</returns>
    public static int ExecuteNonQuery(this DatabaseFacade databaseFacade, string sql, params object[] parameters) =>
        QueryMethods.ExecuteNonQuery(databaseFacade, sql, parameters);

    /// <summary>
    /// Executes a non query.
    /// </summary>
    /// <param name="databaseFacade">The <see cref="DatabaseFacade"/> for the context.</param>
    /// <param name="sql">The SQL query to execute.</param>
    /// <param name="parameters">Parameters to use with the SQL.</param>
    /// <returns>The number of rows affected.</returns>
    public static int ExecuteNonQuery(this DatabaseFacade databaseFacade, string sql, IEnumerable<object> parameters) =>
        QueryMethods.ExecuteNonQuery(databaseFacade, sql, parameters);

    /// <summary>
    /// Executes a non query.
    /// </summary>
    /// <param name="databaseFacade">The <see cref="DatabaseFacade"/> for the context.</param>
    /// <param name="sql">The <see cref="FormattableString"/> representing a SQL query with parameters.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
    /// <returns>The number of rows affected.</returns>
    public static Task<int> ExecuteNonQueryAsync(this DatabaseFacade databaseFacade, FormattableString sql, CancellationToken cancellationToken = default) =>
        QueryMethods.ExecuteNonQueryAsync(databaseFacade, sql.Format, sql.GetArguments(), cancellationToken);

    /// <summary>
    /// Executes a non query.
    /// </summary>
    /// <param name="databaseFacade">The <see cref="DatabaseFacade"/> for the context.</param>
    /// <param name="sql">The SQL query to execute.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
    /// <param name="parameters">Parameters to use with the SQL.</param>
    /// <returns>The number of rows affected.</returns>
    public static Task<int> ExecuteNonQueryAsync(this DatabaseFacade databaseFacade, string sql, CancellationToken cancellationToken = default, params object[] parameters) =>
        QueryMethods.ExecuteNonQueryAsync(databaseFacade, sql, parameters, cancellationToken);

    /// <summary>
    /// Executes a non query.
    /// </summary>
    /// <param name="databaseFacade">The <see cref="DatabaseFacade"/> for the context.</param>
    /// <param name="sql">The SQL query to execute.</param>
    /// <param name="parameters">Parameters to use with the SQL.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
    /// <returns>The number of rows affected.</returns>
    public static Task<int> ExecuteNonQueryAsync(this DatabaseFacade databaseFacade, string sql, IEnumerable<object> parameters, CancellationToken cancellationToken = default) =>
        QueryMethods.ExecuteNonQueryAsync(databaseFacade, sql, parameters, cancellationToken);

    #endregion public ExecuteNonQuery methods

    #region public ExecuteInsert methods

    /// <summary>
    /// Executes an insert command.
    /// </summary>
    /// <param name="databaseFacade">The <see cref="DatabaseFacade"/> for the context.</param>
    /// <param name="sql">The <see cref="FormattableString"/> representing a SQL query with parameters.</param>
    /// <returns>The ID of the new record.</returns>
    public static long ExecuteInsert(this DatabaseFacade databaseFacade, FormattableString sql) =>
        QueryMethods.ExecuteInsert(databaseFacade, sql.Format, sql.GetArguments());

    /// <summary>
    /// Executes an insert command.
    /// </summary>
    /// <param name="databaseFacade">The <see cref="DatabaseFacade"/> for the context.</param>
    /// <param name="sql">The SQL query to execute.</param>
    /// <param name="parameters">Parameters to use with the SQL.</param>
    /// <returns>The ID of the new record.</returns>
    public static long ExecuteInsert(this DatabaseFacade databaseFacade, string sql, params object[] parameters) =>
        QueryMethods.ExecuteInsert(databaseFacade, sql, parameters);

    /// <summary>
    /// Executes an insert command.
    /// </summary>
    /// <param name="databaseFacade">The <see cref="DatabaseFacade"/> for the context.</param>
    /// <param name="sql">The SQL query to execute.</param>
    /// <param name="parameters">Parameters to use with the SQL.</param>
    /// <returns>The ID of the new record.</returns>
    public static long ExecuteInsert(this DatabaseFacade databaseFacade, string sql, IEnumerable<object> parameters) =>
        QueryMethods.ExecuteInsert(databaseFacade, sql, parameters);

    /// <summary>
    /// Executes an insert command.
    /// </summary>
    /// <param name="databaseFacade">The <see cref="DatabaseFacade"/> for the context.</param>
    /// <param name="sql">The <see cref="FormattableString"/> representing a SQL query with parameters.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
    /// <returns>The ID of the new record.</returns>
    public static Task<long> ExecuteInsertAsync(this DatabaseFacade databaseFacade, FormattableString sql, CancellationToken cancellationToken = default) =>
        QueryMethods.ExecuteInsertAsync(databaseFacade, sql.Format, sql.GetArguments(), cancellationToken);

    /// <summary>
    /// Executes an insert command.
    /// </summary>
    /// <param name="databaseFacade">The <see cref="DatabaseFacade"/> for the context.</param>
    /// <param name="sql">The SQL query to execute.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
    /// <param name="parameters">Parameters to use with the SQL.</param>
    /// <returns>The ID of the new record.</returns>
    public static Task<long> ExecuteInsertAsync(this DatabaseFacade databaseFacade, string sql, CancellationToken cancellationToken = default, params object[] parameters) =>
        QueryMethods.ExecuteInsertAsync(databaseFacade, sql, parameters, cancellationToken);

    /// <summary>
    /// Executes an insert command.
    /// </summary>
    /// <param name="databaseFacade">The <see cref="DatabaseFacade"/> for the context.</param>
    /// <param name="sql">The SQL query to execute.</param>
    /// <param name="parameters">Parameters to use with the SQL.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
    /// <returns>The ID of the new record.</returns>
    public static Task<long> ExecuteInsertAsync(this DatabaseFacade databaseFacade, string sql, IEnumerable<object> parameters, CancellationToken cancellationToken = default) =>
        QueryMethods.ExecuteInsertAsync(databaseFacade, sql, parameters, cancellationToken);

    #endregion public ExecuteInsert methods
}
