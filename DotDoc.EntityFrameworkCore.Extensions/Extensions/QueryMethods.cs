// Copyright ©2021-2022 Mike King.
// This file is licensed to you under the MIT license.
// See the License.txt file in the solution root for more information.

using DotDoc.EntityFrameworkCore.Extensions.Constants;
using DotDoc.EntityFrameworkCore.Extensions.Model;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;
using System.Data;
using System.Text.RegularExpressions;

namespace DotDoc.EntityFrameworkCore.Extensions.Extensions;

/// <summary>
/// Internal methods for executing queries.
/// </summary>
internal static class QueryMethods
{
    #region Private Variables

    /// <summary>
    /// Split a SQL query into 2 groups:
    ///     1: Before the ORDER BY clause.
    ///     2: ORDER BY onwards.
    /// </summary>
    private static readonly Regex _sqlSplitOrderByRegex =
        new (@"\A(.*)(\bORDER\s+BY\s+(?!.*?(?:\)|\s+)AS\s)(?:\((?>\((?<depth>)|\)(?<-depth>)|.?)*(?(depth)(?!))\)|[\[\]`""\w\(\)\.])+(?:\s+(?:ASC|DESC))?(?:\s*,\s*(?:\((?>\((?<depth>)|\)(?<-depth>)|.?)*(?(depth)(?!))\)|[\[\]`""\w\(\)\.])+(?:\s+(?:ASC|DESC))?)*)",
            RegexOptions.RightToLeft | RegexOptions.IgnoreCase | RegexOptions.Multiline | RegexOptions.Singleline | RegexOptions.Compiled);

    /// <summary>
    /// Split a SQL query into 3 groups:
    ///
    ///     1:  SELECT
    ///     2:  Field list
    ///     3:  FROM onwards.
    ///
    /// </summary>
    private static readonly Regex _sqlSplitColumnsRegex =
        new (@"\A\s*(SELECT)\s+((?:\((?>\((?<depth>)|\)(?<-depth>)|.?)*(?(depth)(?!))\)|.)*?)(?<!,\s+)\b(FROM\b.+)",
            RegexOptions.IgnoreCase | RegexOptions.Multiline | RegexOptions.Singleline | RegexOptions.Compiled);

    #endregion Private Variables

    #region Internal ExecuteScalar methods

    /// <summary>
    /// Executes a query with a single scalar result.
    /// </summary>
    /// <typeparam name="T">The type of result returned by the query.</typeparam>
    /// <param name="databaseFacade">The <see cref="DatabaseFacade"/> for the context.</param>
    /// <param name="sql">The SQL query to execute.</param>
    /// <param name="parameters">Parameters to use with the SQL.</param>
    /// <returns>The result of the query.</returns>
    internal static T ExecuteScalar<T>(DatabaseFacade databaseFacade, string sql, object[] parameters)
    {
        using ConcurrencyDetectorCriticalSectionDisposer criticalSectionDisposer = databaseFacade
            .GetService<IConcurrencyDetector>()
            .EnterCriticalSection();

        RawSqlCommand rawSqlCommand = databaseFacade
            .GetService<IRawSqlCommandBuilder>()
            .Build(sql, parameters);

        object value = rawSqlCommand
            .RelationalCommand
            .ExecuteScalar(new RelationalCommandParameterObject(databaseFacade.GetService<IRelationalConnection>(), rawSqlCommand.ParameterValues, null, null, null));

        Type type = typeof(T);
        return value == null ? default : (T)Convert.ChangeType(value, Nullable.GetUnderlyingType(type) ?? type);
    }

    /// <summary>
    /// Executes a query with a single scalar result.
    /// </summary>
    /// <typeparam name="T">The type of result returned by the query.</typeparam>
    /// <param name="databaseFacade">The <see cref="DatabaseFacade"/> for the context.</param>
    /// <param name="sql">The SQL query to execute.</param>
    /// <param name="parameters">Parameters to use with the SQL.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
    /// <returns>The result of the query.</returns>
    internal static async Task<T> ExecuteScalarAsync<T>(DatabaseFacade databaseFacade, string sql, object[] parameters, CancellationToken cancellationToken)
    {
        using ConcurrencyDetectorCriticalSectionDisposer criticalSectionDisposer = databaseFacade
            .GetService<IConcurrencyDetector>()
            .EnterCriticalSection();

        RawSqlCommand rawSqlCommand = databaseFacade
            .GetService<IRawSqlCommandBuilder>()
            .Build(sql, parameters);

        object value = await rawSqlCommand
            .RelationalCommand
            .ExecuteScalarAsync(new RelationalCommandParameterObject(databaseFacade.GetService<IRelationalConnection>(), rawSqlCommand.ParameterValues, null, null, null), cancellationToken)
            .ConfigureAwait(false);

        Type type = typeof(T);
        return value == null ? default : (T)Convert.ChangeType(value, Nullable.GetUnderlyingType(type) ?? type);
    }

    #endregion Internal ExecuteScalar methods

    #region Internal ExecuteQuery methods

    /// <summary>
    /// Executes a query.
    /// </summary>
    /// <param name="databaseFacade">The <see cref="DatabaseFacade"/> for the context.</param>
    /// <param name="sql">The SQL query to execute.</param>
    /// <param name="parameters">Parameters to use with the SQL.</param>
    /// <returns>A <see cref="DataTable"/> containing the results of the query.</returns>
    internal static DataTable ExecuteQuery(DatabaseFacade databaseFacade, string sql, object[] parameters)
    {
        using ConcurrencyDetectorCriticalSectionDisposer criticalSectionDisposer = databaseFacade
            .GetService<IConcurrencyDetector>()
            .EnterCriticalSection();

        RawSqlCommand rawSqlCommand = databaseFacade
            .GetService<IRawSqlCommandBuilder>()
            .Build(sql, parameters);

        using RelationalDataReader relationalDataReader = rawSqlCommand
            .RelationalCommand
            .ExecuteReader(new RelationalCommandParameterObject(databaseFacade.GetService<IRelationalConnection>(), rawSqlCommand.ParameterValues, null, null, null));

        DataTable dataTable = new ();
        dataTable.Load(relationalDataReader.DbDataReader);
        return dataTable;
    }

    /// <summary>
    /// Executes a query.
    /// </summary>
    /// <param name="databaseFacade">The <see cref="DatabaseFacade"/> for the context.</param>
    /// <param name="sql">The SQL query to execute.</param>
    /// <param name="parameters">Parameters to use with the SQL.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
    /// <returns>A <see cref="DataTable"/> containing the results of the query.</returns>
    internal static async Task<DataTable> ExecuteQueryAsync(DatabaseFacade databaseFacade, string sql, object[] parameters, CancellationToken cancellationToken)
    {
        using ConcurrencyDetectorCriticalSectionDisposer criticalSectionDisposer = databaseFacade
            .GetService<IConcurrencyDetector>()
            .EnterCriticalSection();

        RawSqlCommand rawSqlCommand = databaseFacade
            .GetService<IRawSqlCommandBuilder>()
            .Build(sql, parameters);

        await using RelationalDataReader relationalDataReader = await rawSqlCommand
            .RelationalCommand
            .ExecuteReaderAsync(new RelationalCommandParameterObject(databaseFacade.GetService<IRelationalConnection>(), rawSqlCommand.ParameterValues, null, null, null), cancellationToken)
            .ConfigureAwait(false);

        DataTable dataTable = new ();
        dataTable.Load(relationalDataReader.DbDataReader);
        return dataTable;
    }

    #endregion Internal ExecuteQuery methods

    #region Internal ExecutePagedQuery methods

    /// <summary>
    /// Executes a query and returns the specified page of results.
    /// </summary>
    /// <param name="databaseFacade">The <see cref="DatabaseFacade"/> for the context.</param>
    /// <param name="sql">The SQL query to execute.</param>
    /// <param name="parameters">Parameters to use with the SQL.</param>
    /// <param name="page">Page number to return (starting at 0).</param>
    /// <param name="pageSize">Number of records per page.</param>
    /// <returns>An instance of <see cref="QueryPage"/> containing the page data (If the page number is past the end of the table then the it will become the last page).</returns>
    internal static QueryPage ExecutePagedQuery(DatabaseFacade databaseFacade, string sql, object[] parameters, long page, long pageSize)
    {
        string countSql = ConvertQueryToCount(sql);
        long recordCount;
        long pageCount;
        DataTable dataTable;

        do
        {
            recordCount = ExecuteScalar<long>(databaseFacade, countSql, parameters);

            pageCount = (recordCount + pageSize - 1) / pageSize;
            if (page >= pageCount)
            {
                page = pageCount > 0 ? pageCount - 1 : 0;
            }

            (string pageSql, object[] pageParameters) = LimitQuery(databaseFacade, sql, page, pageSize, parameters);

            dataTable = ExecuteQuery(databaseFacade, pageSql, pageParameters);

        } while (dataTable.Rows.Count == 0 && page > 0);

        return new QueryPage(page, pageSize, recordCount, pageCount, dataTable);
    }

    /// <summary>
    /// Executes a query and returns the specified page of results.
    /// </summary>
    /// <param name="databaseFacade">The <see cref="DatabaseFacade"/> for the context.</param>
    /// <param name="sql">The SQL query to execute.</param>
    /// <param name="parameters">Parameters to use with the SQL.</param>
    /// <param name="page">Page number to return (starting at 0).</param>
    /// <param name="pageSize">Number of records per page.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
    /// <returns>An instance of <see cref="QueryPage"/> containing the page data (If the page number is past the end of the table then the it will become the last page).</returns>
    internal static async Task<QueryPage> ExecutePagedQueryAsync(DatabaseFacade databaseFacade, string sql, object[] parameters, long page, long pageSize, CancellationToken cancellationToken)
    {
        string countSql = ConvertQueryToCount(sql);
        long recordCount;
        long pageCount;
        DataTable dataTable;

        do
        {
            recordCount = await ExecuteScalarAsync<long>(databaseFacade, countSql, parameters, cancellationToken).ConfigureAwait(false);

            pageCount = (recordCount + pageSize - 1) / pageSize;
            if (page >= pageCount)
            {
                page = pageCount > 0 ? pageCount - 1 : 0;
            }

            (string pageSql, object[] pageParameters) = LimitQuery(databaseFacade, sql, page, pageSize, parameters);

            dataTable = await ExecuteQueryAsync(databaseFacade, pageSql, pageParameters, cancellationToken).ConfigureAwait(false);

        } while (dataTable.Rows.Count == 0 && page > 0);

        return new QueryPage(page, pageSize, recordCount, pageCount, dataTable);
    }

    #endregion Internal ExecutePagedQuery methods

    #region Internal ExecuteNonQuery methods

    /// <summary>
    /// Executes a non query.
    /// </summary>
    /// <param name="databaseFacade">The <see cref="DatabaseFacade"/> for the context.</param>
    /// <param name="sql">The SQL query to execute.</param>
    /// <param name="parameters">Parameters to use with the SQL.</param>
    /// <returns>The number of rows affected.</returns>
    internal static int ExecuteNonQuery(this DatabaseFacade databaseFacade, string sql, object[] parameters) =>
        databaseFacade.ExecuteSqlRaw(sql, parameters);

    /// <summary>
    /// Executes a non query.
    /// </summary>
    /// <param name="databaseFacade">The <see cref="DatabaseFacade"/> for the context.</param>
    /// <param name="sql">The SQL query to execute.</param>
    /// <param name="parameters">Parameters to use with the SQL.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
    /// <returns>The number of rows affected.</returns>
    internal static async Task<int> ExecuteNonQueryAsync(this DatabaseFacade databaseFacade, string sql, object[] parameters, CancellationToken cancellationToken = default) =>
        await databaseFacade.ExecuteSqlRawAsync(sql, parameters, cancellationToken).ConfigureAwait(false);

    #endregion Internal ExecuteNonQuery methods

    #region Internal ExecuteInsert methods

    /// <summary>
    /// Executes an insert command.
    /// </summary>
    /// <param name="databaseFacade">The <see cref="DatabaseFacade"/> for the context.</param>
    /// <param name="sql">The SQL query to execute.</param>
    /// <param name="parameters">Parameters to use with the SQL.</param>
    /// <returns>The ID of the new record.</returns>
    internal static long ExecuteInsert(DatabaseFacade databaseFacade, string sql, object[] parameters) =>
        ExecuteScalar<long>(databaseFacade, GetLastInsertId(databaseFacade, sql), parameters);

    /// <summary>
    /// Executes an insert command.
    /// </summary>
    /// <param name="databaseFacade">The <see cref="DatabaseFacade"/> for the context.</param>
    /// <param name="sql">The SQL query to execute.</param>
    /// <param name="parameters">Parameters to use with the SQL.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
    /// <returns>The ID of the new record.</returns>
    internal static Task<long> ExecuteInsertAsync(DatabaseFacade databaseFacade, string sql, object[] parameters, CancellationToken cancellationToken) =>
        ExecuteScalarAsync<long>(databaseFacade, GetLastInsertId(databaseFacade, sql), parameters, cancellationToken);

    #endregion Internal ExecuteInsert methods

    #region Private Utility methods

    /// <summary>
    /// Convert a SQL query into a query to count the number of records.
    /// </summary>
    /// <param name="sql">The SQL query to execute.</param>
    /// <returns>The SQL query with the field list changed to COUNT(*).</returns>
    private static string ConvertQueryToCount(string sql)
    {
        // First remove the Order By clause if there is one.
        // These are meaningless in this situation as we are fetching a count of records.
        Match match = _sqlSplitOrderByRegex.Match(sql);
        if (match.Success)
        {
            sql = match.Groups[1].Value;
        }

        // Next replace the selected columns with Count(*).
        match = _sqlSplitColumnsRegex.Match(sql);
        if (!match.Success)
        {
            throw new InvalidOperationException("Unable to split query.");
        }

        return $"{match.Groups[1]} COUNT(*) {match.Groups[3]}";
    }

    /// <summary>
    /// Add a limit / offset clause to a query.
    /// </summary>
    /// <param name="databaseFacade">The <see cref="DatabaseFacade"/> for the context.</param>
    /// <param name="sql">The SQL query to execute.</param>
    /// <param name="page">Page number to return (starting at 0).</param>
    /// <param name="pageSize">Number of records per page.</param>
    /// <param name="parameters">Parameters to use with the SQL.</param>
    /// <returns>A tuple containting the modified SQL and parameters array.</returns>
    private static (string, object[]) LimitQuery(DatabaseFacade databaseFacade, string sql, long page, long pageSize, object[] parameters)
    {
        switch (databaseFacade.GetDatabaseType())
        {
            case DatabaseType.Sqlite:
                // Sqlite just needs "LIMIT x OFFSET y" at the end of a query
                parameters = parameters.Concat(new object[] { pageSize, page * pageSize }).ToArray();

                sql = $"{sql} LIMIT {{{parameters.Length - 2}}} OFFSET {{{parameters.Length - 1}}}";
                break;

            case DatabaseType.SqlServer:
                // SQL Server must have "OFFSET x ROWS FETCH NEXT y ROWS ONLY" following the final "ORDER BY" clause.
                // If there is no "Order By" then add a dummy one "Order By
                parameters = parameters.Concat(new object[] { pageSize, page * pageSize }).ToArray();

                Match match = _sqlSplitOrderByRegex.Match(sql);
                sql = match.Success
                    ? $"{match.Groups[1]} {match.Groups[2]}"
                    : $"{sql} ORDER BY (SELECT NULL)";

                sql += $" OFFSET {{{parameters.Length - 1}}} ROWS FETCH NEXT {{{parameters.Length - 2}}} ROWS ONLY";
                break;

            default:
                throw new InvalidOperationException("Unsupported database type");
        }

        return (sql, parameters);
    }

    /// <summary>
    /// Add a sql query to get the last created ID to a query.
    /// </summary>
    /// <param name="databaseFacade">The <see cref="DatabaseFacade"/> for the context.</param>
    /// <param name="sql">The SQL query to execute.</param>
    /// <returns>The existing sql query with the query to fetch the last ID attached.</returns>
    private static string GetLastInsertId(DatabaseFacade databaseFacade, string sql) =>
        databaseFacade.GetDatabaseType() switch
        {
            DatabaseType.Sqlite => $"{sql}; SELECT LAST_INSERT_ROWID();",
            DatabaseType.SqlServer => $"{sql}; SELECT SCOPE_IDENTITY();",
            _ => throw new InvalidOperationException("Unsupported database type"),
        };

    #endregion Private Utility methods
}