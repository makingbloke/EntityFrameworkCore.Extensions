// Copyright ©2021-2025 Mike King.
// This file is licensed to you under the MIT license.
// See the License.txt file in the solution root for more information.

using DotDoc.EntityFrameworkCore.Extensions.DatabaseType;
using DotDoc.EntityFrameworkCore.Extensions.Utilities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;
using System.Text.RegularExpressions;

namespace DotDoc.EntityFrameworkCore.Extensions.Execute;

/// <summary>
/// Internal methods for Execute Extensions.
/// </summary>
internal static partial class ExecuteMethods
{
    #region public ExecuteInsert methods

    /// <summary>
    /// Executes an insert command.
    /// </summary>
    /// <typeparam name="T">The type returned by the insert.</typeparam>
    /// <param name="database">The <see cref="DatabaseFacade"/> for the context.</param>
    /// <param name="sql">The SQL query to execute.</param>
    /// <param name="parameters">Parameters to use with the SQL.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
    /// <returns>The Id of the new record.</returns>
    public static async Task<T> ExecuteInsertAsync<T>(DatabaseFacade database, string sql, IEnumerable<object?> parameters, CancellationToken cancellationToken)
    {
        T id = await ExecuteScalarAsync<T>(database, GetLastInsertId(database.GetDatabaseType(), sql), parameters, cancellationToken).ConfigureAwait(false);
        return id;
    }

    #endregion public ExecuteInsert methods

    #region public ExecuteNonQuery methods

    /// <summary>
    /// Executes a non query.
    /// </summary>
    /// <param name="database">The <see cref="DatabaseFacade"/> for the context.</param>
    /// <param name="sql">The SQL query to execute.</param>
    /// <param name="parameters">Parameters to use with the SQL.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
    /// <returns>The number of rows affected.</returns>
    public static async Task<int> ExecuteNonQueryAsync(DatabaseFacade database, string sql, IEnumerable<object?> parameters, CancellationToken cancellationToken = default)
    {
        int count = await database.ExecuteSqlRawAsync(sql, parameters!, cancellationToken).ConfigureAwait(false);
        return count;
    }

    #endregion public ExecuteNonQuery methods

    #region public ExecutePagedQuery methods

    /// <summary>
    /// Executes a query and returns the specified page of results.
    /// </summary>
    /// <typeparam name="TSource">The type of the elements of the source.</typeparam>
    /// <param name="database">The <see cref="DatabaseFacade"/> for the context.</param>
    /// <param name="sql">The SQL query to execute.</param>
    /// <param name="parameters">Parameters to use with the SQL.</param>
    /// <param name="page">Page number to return (starting at 0).</param>
    /// <param name="pageSize">Number of records per page.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
    /// <returns>An instance of <see cref="PagedQueryResult{TSource}"/> containing the page data (If the page number is past the end of the table then the it will become the last page).</returns>
    public static async Task<PagedQueryResult<TSource>> ExecutePagedQueryAsync<TSource>(DatabaseFacade database, string sql, IEnumerable<object?> parameters, long page, long pageSize, CancellationToken cancellationToken)
        where TSource : class
    {
        // Setup a list to hold the page query parameters and add two values for the limit and offset.
        List<object?> pageParameters = [.. parameters, 0, 0];

        int limitParamPos = pageParameters.Count - 2;
        int offsetParamPos = pageParameters.Count - 1;

        string countSql = ConvertQueryToCount(sql);
        string pageSql = ConvertQueryToPaged(database.GetDatabaseType(), sql, limitParamPos, offsetParamPos);
        long recordCount;
        long pageCount;
        IList<TSource> result;

        do
        {
            recordCount = await ExecuteScalarAsync<long>(database, countSql, parameters, cancellationToken).ConfigureAwait(false);

            pageCount = (recordCount + pageSize - 1) / pageSize;
            if (page >= pageCount)
            {
                page = Math.Max(0, pageCount - 1);
            }

            pageParameters[limitParamPos] = pageSize;
            pageParameters[offsetParamPos] = page * pageSize;

            result = await ExecuteQueryAsync<TSource>(database, pageSql, pageParameters, cancellationToken).ConfigureAwait(false);

        } while (result.Count == 0 && page > 0);

        return new PagedQueryResult<TSource>(page, pageSize, recordCount, pageCount, result);
    }

    #endregion public ExecutePagedQuery methods

    #region public ExecuteQuery methods

    /// <summary>
    /// Executes a query.
    /// </summary>
    /// <typeparam name="TSource">The type of the elements of the source.</typeparam>
    /// <param name="database">The <see cref="DatabaseFacade"/> for the context.</param>
    /// <param name="sql">The SQL query to execute.</param>
    /// <param name="parameters">Parameters to use with the SQL.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
    /// <returns>A <see cref="IList{TSource}"/> containing the results of the query.</returns>
    public static async Task<IList<TSource>> ExecuteQueryAsync<TSource>(DatabaseFacade database, string sql, IEnumerable<object?> parameters, CancellationToken cancellationToken)
        where TSource : class
    {
        DbContext context = database.GetDbContext();

        List<TSource> results = await context.Set<TSource>()
            .FromSqlRaw(sql, parameters.ToArray())
            .AsNoTracking()
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        return results;
    }

    #endregion public ExecuteQuery methods

    #region public ExecuteScalar methods

    /// <summary>
    /// Executes a query with a single scalar result.
    /// </summary>
    /// <typeparam name="T">The type of result returned by the query.</typeparam>
    /// <param name="database">The <see cref="DatabaseFacade"/> for the context.</param>
    /// <param name="sql">The SQL query to execute.</param>
    /// <param name="parameters">Parameters to use with the SQL.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
    /// <returns>The result of the query.</returns>
    public static async Task<T> ExecuteScalarAsync<T>(DatabaseFacade database, string sql, IEnumerable<object?> parameters, CancellationToken cancellationToken)
    {
        using ConcurrencyDetectorCriticalSectionDisposer criticalSectionDisposer = database
            .GetService<IConcurrencyDetector>()
            .EnterCriticalSection();

        RawSqlCommand rawSqlCommand = database
            .GetService<IRawSqlCommandBuilder>()
            .Build(sql, parameters);

        using IRelationalConnection connection = database
            .GetService<IRelationalConnection>();

        RelationalCommandParameterObject parameterObject = new(
            connection: connection,
            parameterValues: rawSqlCommand.ParameterValues,
            readerColumns: null,
            context: null,
            logger: null);

        object? value = await rawSqlCommand
            .RelationalCommand
            .ExecuteScalarAsync(parameterObject, cancellationToken)
            .ConfigureAwait(false);

        T result = value is null
            ? default!
            : (T)value;

        return result;
    }

    #endregion public ExecuteScalar methods

    #region private methods

    /// <summary>
    /// Convert a SQL query into a query to count the number of records.
    /// </summary>
    /// <param name="sql">The SQL query to execute.</param>
    /// <returns>The modified SQL query.</returns>
    private static string ConvertQueryToCount(string sql)
    {
        // First remove the Order By clause if there is one.
        // These are meaningless in this situation as we are fetching a count of records.
        Match match = SqlSplitOrderByRegex().Match(sql);
        if (match.Success)
        {
            sql = match.Groups[1].Value;
        }

        // Next replace the selected columns with Count(*).
        match = SqlSplitColumnsRegex().Match(sql);
        if (!match.Success)
        {
            throw new InvalidOperationException("Unable to split query");
        }

        return $"{match.Groups[1]} COUNT(*) {match.Groups[3]}";
    }

    /// <summary>
    /// Add a limit / offset clause to a query.
    /// </summary>
    /// <param name="databaseType">The database type.</param>
    /// <param name="sql">The SQL query to execute.</param>
    /// <param name="limitParamPos">Position of the limit value in the parameters collection.</param>
    /// <param name="offsetParamPos">Position of the offset value in the parameters collection.</param>
    /// <returns>The modified SQL query.</returns>
    private static string ConvertQueryToPaged(string? databaseType, string sql, int limitParamPos, int offsetParamPos)
    {
        string pageSql;

        switch (databaseType)
        {
            case DatabaseTypes.Sqlite:
                // SQLite just needs "LIMIT x OFFSET y" at the end of a query
                pageSql = $"{sql} LIMIT {{{limitParamPos}}} OFFSET {{{offsetParamPos}}}";
                break;

            case DatabaseTypes.SqlServer:
                // SQL Server must have "OFFSET x ROWS FETCH NEXT y ROWS ONLY" following the final "ORDER BY" clause.
                // If there is no "ORDER BY" then add a dummy one.
                string offset = $" OFFSET {{{offsetParamPos}}} ROWS FETCH NEXT {{{limitParamPos}}} ROWS ONLY";

                Match match = SqlSplitOrderByRegex().Match(sql);

                pageSql = match.Success
                    ? $"{match.Groups[1]} {match.Groups[2]} {offset}"
                    : $"{sql} ORDER BY (SELECT NULL) {offset}";
                break;

            default:
                throw new UnsupportedDatabaseTypeException(nameof(databaseType));
        }

        return pageSql;
    }

    /// <summary>
    /// Add a sql query to get the last created ID to a query.
    /// </summary>
    /// <param name="databaseType">The database type.</param>
    /// <param name="sql">The SQL query to execute.</param>
    /// <returns>The existing sql query with the query to fetch the last ID attached.</returns>
    private static string GetLastInsertId(string? databaseType, string sql)
    {
        string insertIdSql = sql + ";" +
            databaseType switch
            {
                DatabaseTypes.Sqlite => "SELECT LAST_INSERT_ROWID()",
                DatabaseTypes.SqlServer => "SELECT SCOPE_IDENTITY()",
                _ => new UnsupportedDatabaseTypeException(nameof(databaseType))
            };

        return insertIdSql;
    }

    /// <summary>
    /// Split a SQL query into 2 groups:
    ///     1: Before the ORDER BY clause.
    ///     2: ORDER BY onwards.
    /// </summary>
    [GeneratedRegex(@"\A(.*)(\bORDER\s+BY\s+(?!.*?(?:\)|\s+)AS\s)(?:\((?>\((?<depth>)|\)(?<-depth>)|.?)*(?(depth)(?!))\)|[\[\]`""\w\(\)\.])+(?:\s+(?:ASC|DESC))?(?:\s*,\s*(?:\((?>\((?<depth>)|\)(?<-depth>)|.?)*(?(depth)(?!))\)|[\[\]`""\w\(\)\.])+(?:\s+(?:ASC|DESC))?)*)", RegexOptions.RightToLeft | RegexOptions.IgnoreCase | RegexOptions.Multiline)]
    private static partial Regex SqlSplitOrderByRegex();

    /// <summary>
    /// Split a SQL query into 3 groups:
    ///
    ///     1:  SELECT
    ///     2:  Field list
    ///     3:  FROM onwards.
    ///
    /// </summary>
    [GeneratedRegex(@"\A\s*(SELECT)\s+((?:\((?>\((?<depth>)|\)(?<-depth>)|.?)*(?(depth)(?!))\)|.)*?)(?<!,\s+)\b(FROM\b.+)", RegexOptions.IgnoreCase | RegexOptions.Multiline)]
    private static partial Regex SqlSplitColumnsRegex();

    #endregion private methods
}
