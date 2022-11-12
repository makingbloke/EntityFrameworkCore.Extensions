// Copyright ©2021-2023 Mike King.
// This file is licensed to you under the MIT license.
// See the License.txt file in the solution root for more information.

using DotDoc.EntityFrameworkCore.Extensions.Model;
using Microsoft.EntityFrameworkCore.Infrastructure;
using System.Data;

namespace DotDoc.EntityFrameworkCore.Extensions;

/// <summary>
/// Entity Framework Core Query Exensions.
/// </summary>
public static class QueryExtensions
{
    #region Public ExecuteScalar methods

    /// <summary>
    /// Executes a query with a single scalar result.
    /// </summary>
    /// <typeparam name="T">The type of result returned by the query.</typeparam>
    /// <param name="databaseFacade">The <see cref="DatabaseFacade"/> for the context.</param>
    /// <param name="sql">The interpolated string representing a SQL query with parameters.</param>
    /// <returns>The result of the query.</returns>
    public static T ExecuteScalarInterpolated<T>(this DatabaseFacade databaseFacade, FormattableString sql) =>
        QueryMethods.ExecuteScalar<T>(databaseFacade, sql.Format, sql.GetArguments());

    /// <summary>
    /// Executes a query with a single scalar result.
    /// </summary>
    /// <typeparam name="T">The type of result returned by the query.</typeparam>
    /// <param name="databaseFacade">The <see cref="DatabaseFacade"/> for the context.</param>
    /// <param name="sql">The SQL query to execute.</param>
    /// <param name="parameters">Parameters to use with the SQL.</param>
    /// <returns>The result of the query.</returns>
    public static T ExecuteScalarRaw<T>(this DatabaseFacade databaseFacade, string sql, params object[] parameters) =>
        QueryMethods.ExecuteScalar<T>(databaseFacade, sql, parameters);

    /// <summary>
    /// Executes a query with a single scalar result.
    /// </summary>
    /// <typeparam name="T">The type of result returned by the query.</typeparam>
    /// <param name="databaseFacade">The <see cref="DatabaseFacade"/> for the context.</param>
    /// <param name="sql">The interpolated string representing a SQL query with parameters.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
    /// <returns>The result of the query.</returns>
    public static Task<T> ExecuteScalarInterpolatedAsync<T>(this DatabaseFacade databaseFacade, FormattableString sql, CancellationToken cancellationToken = default) =>
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
    public static Task<T> ExecuteScalarRawAsync<T>(this DatabaseFacade databaseFacade, string sql, CancellationToken cancellationToken = default, params object[] parameters) =>
        QueryMethods.ExecuteScalarAsync<T>(databaseFacade, sql, parameters, cancellationToken);

    #endregion Public ExecuteScalar methods

    #region Public ExecuteQuery methods

    /// <summary>
    /// Executes a query.
    /// </summary>
    /// <param name="databaseFacade">The <see cref="DatabaseFacade"/> for the context.</param>
    /// <param name="sql">The interpolated string representing a SQL query with parameters.</param>
    /// <returns>A <see cref="DataTable"/> containing the results of the query.</returns>
    public static DataTable ExecuteQueryInterpolated(this DatabaseFacade databaseFacade, FormattableString sql) =>
        QueryMethods.ExecuteQuery(databaseFacade, sql.Format, sql.GetArguments());

    /// <summary>
    /// Executes a query.
    /// </summary>
    /// <param name="databaseFacade">The <see cref="DatabaseFacade"/> for the context.</param>
    /// <param name="sql">The SQL query to execute.</param>
    /// <param name="parameters">Parameters to use with the SQL.</param>
    /// <returns>A <see cref="DataTable"/> containing the results of the query.</returns>
    public static DataTable ExecuteQueryRaw(this DatabaseFacade databaseFacade, string sql, params object[] parameters) =>
        QueryMethods.ExecuteQuery(databaseFacade, sql, parameters);

    /// <summary>
    /// Executes a query.
    /// </summary>
    /// <param name="databaseFacade">The <see cref="DatabaseFacade"/> for the context.</param>
    /// <param name="sql">The interpolated string representing a SQL query with parameters.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
    /// <returns>A <see cref="DataTable"/> containing the results of the query.</returns>
    public static Task<DataTable> ExecuteQueryInterpolatedAsync(this DatabaseFacade databaseFacade, FormattableString sql, CancellationToken cancellationToken = default) =>
        QueryMethods.ExecuteQueryAsync(databaseFacade, sql.Format, sql.GetArguments(), cancellationToken);

    /// <summary>
    /// Executes a query.
    /// </summary>
    /// <param name="databaseFacade">The <see cref="DatabaseFacade"/> for the context.</param>
    /// <param name="sql">The SQL query to execute.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
    /// <param name="parameters">Parameters to use with the SQL.</param>
    /// <returns>A <see cref="DataTable"/> containing the results of the query.</returns>
    public static Task<DataTable> ExecuteQueryRawAsync(this DatabaseFacade databaseFacade, string sql, CancellationToken cancellationToken = default, params object[] parameters) =>
        QueryMethods.ExecuteQueryAsync(databaseFacade, sql, parameters, cancellationToken);

    #endregion Public ExecuteQuery methods

    #region Public ExecutePagedQuery methods

    /// <summary>
    /// Executes a query and returns the specified page of results.
    /// </summary>
    /// <param name="databaseFacade">The <see cref="DatabaseFacade"/> for the context.</param>
    /// <param name="sql">The interpolated string representing a SQL query with parameters.</param>
    /// <param name="page">Page number to return (starting at 0).</param>
    /// <param name="pageSize">Number of records per page.</param>
    /// <returns>An instance of <see cref="QueryPage"/> containing the page data (If the page number is past the end of the table then the it will become the last page).</returns>
    public static QueryPage ExecutePagedQueryInterpolated(this DatabaseFacade databaseFacade, FormattableString sql, long page, long pageSize) =>
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
    public static QueryPage ExecutePagedQueryRaw(this DatabaseFacade databaseFacade, string sql, long page, long pageSize, params object[] parameters) =>
        QueryMethods.ExecutePagedQuery(databaseFacade, sql, parameters, page, pageSize);

    /// <summary>
    /// Executes a query and returns the specified page of results.
    /// </summary>
    /// <param name="databaseFacade">The <see cref="DatabaseFacade"/> for the context.</param>
    /// <param name="sql">The interpolated string representing a SQL query with parameters.</param>
    /// <param name="page">Page number to return (starting at 0).</param>
    /// <param name="pageSize">Number of records per page.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
    /// <returns>An instance of <see cref="QueryPage"/> containing the page data (If the page number is past the end of the table then the it will become the last page).</returns>
    public static Task<QueryPage> ExecutePagedQueryInterpolatedAsync(this DatabaseFacade databaseFacade, FormattableString sql, long page, long pageSize, CancellationToken cancellationToken = default) =>
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
    public static Task<QueryPage> ExecutePagedQueryRawAsync(this DatabaseFacade databaseFacade, string sql, long page, long pageSize, CancellationToken cancellationToken = default, params object[] parameters) =>
        QueryMethods.ExecutePagedQueryAsync(databaseFacade, sql, parameters, page, pageSize, cancellationToken);

    #endregion Public ExecutePagedQuery methods

    #region Public ExecuteNonQuery methods

    /// <summary>
    /// Executes a non query.
    /// </summary>
    /// <param name="databaseFacade">The <see cref="DatabaseFacade"/> for the context.</param>
    /// <param name="sql">The interpolated string representing a SQL query with parameters.</param>
    /// <returns>The number of rows affected.</returns>
    public static int ExecuteNonQueryInterpolated(this DatabaseFacade databaseFacade, FormattableString sql) =>
        databaseFacade.ExecuteNonQuery(sql.Format, sql.GetArguments());

    /// <summary>
    /// Executes a non query.
    /// </summary>
    /// <param name="databaseFacade">The <see cref="DatabaseFacade"/> for the context.</param>
    /// <param name="sql">The SQL query to execute.</param>
    /// <param name="parameters">Parameters to use with the SQL.</param>
    /// <returns>The number of rows affected.</returns>
    public static int ExecuteNonQueryRaw(this DatabaseFacade databaseFacade, string sql, params object[] parameters) =>
        databaseFacade.ExecuteNonQuery(sql, parameters);

    /// <summary>
    /// Executes a non query.
    /// </summary>
    /// <param name="databaseFacade">The <see cref="DatabaseFacade"/> for the context.</param>
    /// <param name="sql">The interpolated string representing a SQL query with parameters.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
    /// <returns>The number of rows affected.</returns>
    public static Task<int> ExecuteNonQueryInterpolatedAsync(this DatabaseFacade databaseFacade, FormattableString sql, CancellationToken cancellationToken = default) =>
        databaseFacade.ExecuteNonQueryAsync(sql.Format, sql.GetArguments(), cancellationToken);

    /// <summary>
    /// Executes a non query.
    /// </summary>
    /// <param name="databaseFacade">The <see cref="DatabaseFacade"/> for the context.</param>
    /// <param name="sql">The SQL query to execute.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
    /// <param name="parameters">Parameters to use with the SQL.</param>
    /// <returns>The number of rows affected.</returns>
    public static Task<int> ExecuteNonQueryRawAsync(this DatabaseFacade databaseFacade, string sql, CancellationToken cancellationToken = default, params object[] parameters) =>
        databaseFacade.ExecuteNonQueryAsync(sql, parameters, cancellationToken);

    #endregion Public ExecuteNonQuery methods

    #region Public ExecuteInsert methods

    /// <summary>
    /// Executes an insert command.
    /// </summary>
    /// <param name="databaseFacade">The <see cref="DatabaseFacade"/> for the context.</param>
    /// <param name="sql">The interpolated string representing a SQL query with parameters.</param>
    /// <returns>The ID of the new record.</returns>
    public static long ExecuteInsertInterpolated(this DatabaseFacade databaseFacade, FormattableString sql) =>
        QueryMethods.ExecuteInsert(databaseFacade, sql.Format, sql.GetArguments());

    /// <summary>
    /// Executes an insert command.
    /// </summary>
    /// <param name="databaseFacade">The <see cref="DatabaseFacade"/> for the context.</param>
    /// <param name="sql">The SQL query to execute.</param>
    /// <param name="parameters">Parameters to use with the SQL.</param>
    /// <returns>The ID of the new record.</returns>
    public static long ExecuteInsertRaw(this DatabaseFacade databaseFacade, string sql, params object[] parameters) =>
        QueryMethods.ExecuteInsert(databaseFacade, sql, parameters);

    /// <summary>
    /// Executes an insert command.
    /// </summary>
    /// <param name="databaseFacade">The <see cref="DatabaseFacade"/> for the context.</param>
    /// <param name="sql">The interpolated string representing a SQL query with parameters.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
    /// <returns>The ID of the new record.</returns>
    public static Task<long> ExecuteInsertInterpolatedAsync(this DatabaseFacade databaseFacade, FormattableString sql, CancellationToken cancellationToken = default) =>
        QueryMethods.ExecuteInsertAsync(databaseFacade, sql.Format, sql.GetArguments(), cancellationToken);

    /// <summary>
    /// Executes an insert command.
    /// </summary>
    /// <param name="databaseFacade">The <see cref="DatabaseFacade"/> for the context.</param>
    /// <param name="sql">The SQL query to execute.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
    /// <param name="parameters">Parameters to use with the SQL.</param>
    /// <returns>The ID of the new record.</returns>
    public static Task<long> ExecuteInsertRawAsync(this DatabaseFacade databaseFacade, string sql, CancellationToken cancellationToken = default, params object[] parameters) =>
        QueryMethods.ExecuteInsertAsync(databaseFacade, sql, parameters, cancellationToken);

    #endregion Public ExecuteInsert methods
}
