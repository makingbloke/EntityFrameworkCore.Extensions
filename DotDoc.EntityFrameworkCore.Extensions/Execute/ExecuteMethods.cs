// Copyright ©2021-2026 Mike King.
// This file is licensed to you under the MIT license.
// See the License.txt file in the solution root for more information.

using DotDoc.EntityFrameworkCore.Extensions.DatabaseType;
using DotDoc.EntityFrameworkCore.Extensions.Parser;
using DotDoc.EntityFrameworkCore.Extensions.Utilities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;
using System.Globalization;

namespace DotDoc.EntityFrameworkCore.Extensions.Execute;

/// <summary>
/// Internal methods for Execute Extensions.
/// </summary>
internal static class ExecuteMethods
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
    /// <exception cref="ArgumentException">The SQL is empty, is more than one statement, or has an unclosed quote, comment or bracket.</exception>
    /// <exception cref="NotSupportedException">The SQL is not a SELECT query, is already paged, or uses a clause that can't be paged (see <see cref="SqlDialect"/>).</exception>
    public static async Task<PagedQueryResult<TSource>> ExecutePagedQueryAsync<TSource>(DatabaseFacade database, string sql, IEnumerable<object?> parameters, long page, long pageSize, CancellationToken cancellationToken)
        where TSource : class
    {
        // Setup a list to hold the page query parameters and add two values for the limit and offset.
        List<object?> pageParameters = [.. parameters, 0, 0];

        int limitParamPos = pageParameters.Count - 2;
        int offsetParamPos = pageParameters.Count - 1;

        // Split the query into its parts (ignoring comments, strings and quoted identifiers) and build the count and page queries from them.
        SqlDialect dialect = SqlDialect.For(database.GetDatabaseType());
        SqlQueryShape shape = dialect.Analyse(sql);

        string countSql = dialect.BuildCountSql(shape);
        string pageSql = dialect.BuildPagedSql(shape, limitParamPos, offsetParamPos);
        long recordCount;
        long pageCount;
        IList<TSource> result;

        long retries = 0;

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

            retries++;
        } while (result.Count == 0 && page > 0 && retries < pageCount);

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

        List<TSource> results = await context.Database
            .SqlQueryRaw<TSource>(sql, parameters.ToArray()!)
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

        IRelationalConnection connection = database
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

        Type type = typeof(T);
        T result = value is null
            ? default!
            : (T)Convert.ChangeType(value, Nullable.GetUnderlyingType(type) ?? type, CultureInfo.InvariantCulture);

        return result;
    }

    #endregion public ExecuteScalar methods

    #region private methods

    /// <summary>
    /// Add a sql query to get the last created ID to a query.
    /// </summary>
    /// <param name="databaseType">The database type.</param>
    /// <param name="sql">The SQL query to execute.</param>
    /// <returns>The existing sql query with the query to fetch the last ID attached.</returns>
    private static string GetLastInsertId(string? databaseType, string sql)
    {
        sql = sql.TrimEnd();

        if (!sql.EndsWith(';'))
        {
            sql += ";";
        }

        string insertIdSql = sql +
            databaseType switch
            {
                DatabaseTypes.Sqlite => "SELECT LAST_INSERT_ROWID()",
                DatabaseTypes.SqlServer => "SELECT SCOPE_IDENTITY()",
                _ => throw new UnsupportedDatabaseTypeException(nameof(databaseType))
            };

        return insertIdSql;
    }

    #endregion private methods
}
