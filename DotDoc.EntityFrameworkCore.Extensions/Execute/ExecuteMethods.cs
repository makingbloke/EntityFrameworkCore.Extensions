// Copyright ©2021-2026 Mike King.
// This file is licensed to you under the MIT license.
// See the License.txt file in the solution root for more information.

using DotDoc.EntityFrameworkCore.Extensions.DatabaseType;
using DotDoc.EntityFrameworkCore.Extensions.SqlParser;
using DotDoc.EntityFrameworkCore.Extensions.Utilities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
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
    /// <returns>The identity value (SQL Server) or rowid (SQLite) of the new row, or the default value of <typeparamref name="T"/> if no row was inserted.</returns>
    /// <exception cref="ArgumentException">The SQL doesn't contain a statement, or has an unclosed quote, comment or bracket.</exception>
    /// <exception cref="UnsupportedDatabaseTypeException">The database provider is not SQLite or SQL Server.</exception>
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
    /// <returns>An instance of <see cref="PagedQueryResult{TSource}"/> containing the page data (If the page number is past the end of the table then it will become the last page).</returns>
    /// <exception cref="ArgumentException">The SQL is empty, is more than one statement, or has an unclosed quote, comment or bracket.</exception>
    /// <exception cref="NotSupportedException">The SQL is not a SELECT query, is already paged, or uses a clause that can't be paged (see <see cref="SqlDialect"/>).</exception>
    /// <exception cref="UnsupportedDatabaseTypeException">The database provider is not SQLite or SQL Server.</exception>
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

            // Round up without adding to recordCount first, which overflows when pageSize is close to long.MaxValue. As page is then at most
            // pageCount - 1, the offset (page * pageSize) is less than recordCount, so it can't overflow either.
            pageCount = recordCount / pageSize;

            if (recordCount % pageSize != 0)
            {
                pageCount++;
            }

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
    /// <returns>The result of the query, converted to <typeparamref name="T"/>, or the default value of <typeparamref name="T"/> if the query returned a database NULL or no rows.</returns>
    public static async Task<T> ExecuteScalarAsync<T>(DatabaseFacade database, string sql, IEnumerable<object?> parameters, CancellationToken cancellationToken)
    {
        // Read the parameters once, so a retry uses the same values.
        object?[] parameterValues = [.. parameters];

        DbContext context = database.GetService<ICurrentDbContext>().Context;

        // Check for a second operation on the context at the same time only if thread safety checks are on (EnableThreadSafetyChecks), as EF Core does.
        IConcurrencyDetector? concurrencyDetector = database.GetService<ICoreSingletonOptions>().AreThreadSafetyChecksEnabled
            ? database.GetService<IConcurrencyDetector>()
            : null;

        using ConcurrencyDetectorCriticalSectionDisposer? criticalSectionDisposer = concurrencyDetector?.EnterCriticalSection();

        // Run the command inside the context's execution strategy, so a transient error is retried if retries are enabled (e.g. EnableRetryOnFailure),
        // and with the context and EF Core's command logger, so command interceptors, logging and diagnostic events see it.
        object? value = await database
            .CreateExecutionStrategy()
            .ExecuteAsync(
                (Database: database, Context: context, Sql: sql, Parameters: parameterValues),
                static async (state, cancellationToken) =>
                {
                    // Map the parameters with the model, as ExecuteSqlRawAsync does, so model-wide configuration (DefaultTypeMapping) applies to them.
                    RawSqlCommand rawSqlCommand = state.Database
                        .GetService<IRawSqlCommandBuilder>()
                        .Build(state.Sql, state.Parameters, state.Context.Model);

                    RelationalCommandParameterObject parameterObject = new(
                        connection: state.Database.GetService<IRelationalConnection>(),
                        parameterValues: rawSqlCommand.ParameterValues,
                        readerColumns: null,
                        context: state.Context,
                        logger: state.Database.GetService<IRelationalCommandDiagnosticsLogger>(),
                        commandSource: CommandSource.ExecuteSqlRaw);

                    return await rawSqlCommand
                        .RelationalCommand
                        .ExecuteScalarAsync(parameterObject, cancellationToken)
                        .ConfigureAwait(false);
                },
                cancellationToken)
            .ConfigureAwait(false);

        RelationalTypeMapping? typeMapping = database
            .GetService<IRelationalTypeMappingSource>()
            .FindMapping(typeof(T), context.Model);

        T result = ConvertScalarResult<T>(value, typeMapping);
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
    /// <exception cref="ArgumentException">The SQL doesn't contain a statement, or has an unclosed quote, comment or bracket.</exception>
    /// <exception cref="UnsupportedDatabaseTypeException"><paramref name="databaseType"/> is not SQLite or SQL Server.</exception>
    private static string GetLastInsertId(string? databaseType, string sql)
    {
        string lastInsertIdSql = databaseType switch
        {
            // last_insert_rowid() is the last insert on the connection, which pooling keeps between contexts, so on its own it returns an
            // earlier insert's rowid (possibly from another table) if the caller's SQL inserts nothing. changes() is the number of rows changed
            // by the caller's last INSERT, UPDATE or DELETE, as it runs in the same command, so nothing inserted gives NULL (and so default(T)),
            // as SCOPE_IDENTITY() does on SQL Server.
            DatabaseTypes.Sqlite => "SELECT CASE WHEN changes() > 0 THEN last_insert_rowid() END",

            DatabaseTypes.SqlServer => "SELECT SCOPE_IDENTITY()",

            _ => throw new UnsupportedDatabaseTypeException(nameof(databaseType))
        };

        // Cut the SQL at the end of its last token, so a trailing comment (a -- comment, or an unclosed /* comment on SQLite)
        // can't hide the SELECT that is added after it.
        IReadOnlyList<SqlToken> tokens = SqlDialect.For(databaseType).Tokenize(sql);

        if (tokens.Count == 0)
        {
            throw new ArgumentException("The SQL does not contain a statement.", nameof(sql));
        }

        SqlToken lastToken = tokens[^1];
        string separator = lastToken.Type == SqlTokenType.Semicolon ? string.Empty : ";";

        return $"{sql[..lastToken.End]}{separator}{Environment.NewLine}{lastInsertIdSql}";
    }

    /// <summary>
    /// Converts the value returned by a scalar query to the type the caller asked for.
    /// </summary>
    /// <typeparam name="T">The type to convert to.</typeparam>
    /// <param name="value">The value returned by the database provider.</param>
    /// <param name="typeMapping">The model's type mapping for <typeparamref name="T"/>, or <see langword="null"/> if there isn't one.</param>
    /// <returns>The converted value, or the default value of <typeparamref name="T"/> if <paramref name="value"/> is <see langword="null"/> or a database NULL.</returns>
    /// <exception cref="InvalidCastException">The value can't be converted to <typeparamref name="T"/>.</exception>
    /// <exception cref="FormatException">The value is text that isn't in a format <typeparamref name="T"/> can be read from.</exception>
    private static T ConvertScalarResult<T>(object? value, RelationalTypeMapping? typeMapping)
    {
        // A scalar query returns the value as the provider reads it, which isn't always the type EF Core would read a column as (SQLite returns
        // a GUID, date or time as text, and SQL Server returns a date as a DateTime). If the type mapping has a value converter (EF Core's own
        // for an enum, or one set with DefaultTypeMapping in ConfigureConventions), the value is converted to the converter's provider type and
        // then through the converter, as EF Core does when it reads a column. Otherwise it's converted with ConvertValue.
        if (value is null || Convert.IsDBNull(value))
        {
            return default!;
        }

        ValueConverter? converter = typeMapping?.Converter;

        object? result = converter is null
            ? ConvertValue(value, Nullable.GetUnderlyingType(typeof(T)) ?? typeof(T))
            : converter.ConvertFromProvider(ConvertValue(value, Nullable.GetUnderlyingType(converter.ProviderClrType) ?? converter.ProviderClrType));

        return (T)result!;
    }

    /// <summary>
    /// Converts a value returned by the database provider to a type, including the types <see cref="Convert.ChangeType(object, Type, IFormatProvider)"/> do not support.
    /// </summary>
    /// <param name="value">The value.</param>
    /// <param name="type">The type to convert to (not a nullable type).</param>
    /// <returns>The converted value.</returns>
    /// <exception cref="InvalidCastException">The value can't be converted to <paramref name="type"/>.</exception>
    /// <exception cref="FormatException">The value is text that isn't in a format <paramref name="type"/> can be read from.</exception>
    private static object ConvertValue(object value, Type type)
    {
        // Text is read with the invariant culture, in the formats SQLite stores these types in. A GUID can also be read from 16 bytes (a BLOB), as
        // Microsoft.Data.Sqlite does. An enum is read from its underlying numeric type.
        if (type.IsInstanceOfType(value))
        {
            return value;
        }

        if (type.IsEnum)
        {
            return Enum.ToObject(type, ConvertValue(value, Enum.GetUnderlyingType(type)));
        }

        return value switch
        {
            string text when type == typeof(Guid) => Guid.Parse(text),
            byte[] bytes when type == typeof(Guid) => new Guid(bytes),
            string text when type == typeof(DateTimeOffset) => DateTimeOffset.Parse(text, CultureInfo.InvariantCulture),
            string text when type == typeof(TimeSpan) => TimeSpan.Parse(text, CultureInfo.InvariantCulture),
            string text when type == typeof(DateOnly) => DateOnly.Parse(text, CultureInfo.InvariantCulture),
            DateTime dateTime when type == typeof(DateOnly) => DateOnly.FromDateTime(dateTime),
            string text when type == typeof(TimeOnly) => TimeOnly.Parse(text, CultureInfo.InvariantCulture),
            TimeSpan timeSpan when type == typeof(TimeOnly) => TimeOnly.FromTimeSpan(timeSpan),
            _ => Convert.ChangeType(value, type, CultureInfo.InvariantCulture),
        };
    }

    #endregion private methods
}
