// Copyright ©2021-2025 Mike King.
// This file is licensed to you under the MIT license.
// See the License.txt file in the solution root for more information.

using DotDoc.EntityFrameworkCore.Extensions.Classes;
using DotDoc.EntityFrameworkCore.Extensions.Constants;
using DotDoc.EntityFrameworkCore.Extensions.Interceptors;
using Microsoft.EntityFrameworkCore;
using System.Text.RegularExpressions;

namespace DotDoc.EntityFrameworkCore.Extensions.Extensions;

/// <summary>
/// Entity Framework Core ExecuteUpdate Extensions.
/// </summary>
public static partial class ExecuteUpdateExtensions
{
    #region public methods

    /// <summary>
    /// Use the Execute Update Extensions.
    /// </summary>
    /// <param name="optionsBuilder">The builder being used to configure the context.</param>
    /// <returns>The same builder instance so multiple calls can be chained.</returns>
    public static DbContextOptionsBuilder UseExecuteUpdateExtensions(this DbContextOptionsBuilder optionsBuilder)
    {
        ArgumentNullException.ThrowIfNull(optionsBuilder);
        return optionsBuilder.AddInterceptors(ExecuteUpdateGetRowsCommandInterceptor.Instance);
    }

    #endregion public methods

    #region public ExecuteUpdateGetCount methods

    /// <summary>
    /// Updates all database rows for the entity instances which match the LINQ query from the database.
    /// </summary>
    /// <typeparam name="TSource">Type of source.</typeparam>
    /// <param name="source">The LINQ query.</param>
    /// <param name="setPropertyAction">Action used to set property values.</param>
    /// <returns>The number of rows updated in the database.</returns>
    public static int ExecuteUpdateGetCount<TSource>(this IQueryable<TSource> source, Action<SetPropertyBuilder<TSource>> setPropertyAction)
        where TSource : class
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(setPropertyAction);

        SetPropertyBuilder<TSource> builder = new();
        setPropertyAction(builder);

        int count = source.ExecuteUpdate(builder.GenerateLambda());
        return count;
    }

    /// <summary>
    /// Updates all database rows for the entity instances which match the LINQ query from the database.
    /// </summary>
    /// <typeparam name="TSource">Type of source.</typeparam>
    /// <param name="source">The LINQ query.</param>
    /// <param name="setPropertyAction">Action used to set property values.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
    /// <returns>The number of rows updated in the database.</returns>
    public static async Task<int> ExecuteUpdateGetCountAsync<TSource>(this IQueryable<TSource> source, Action<SetPropertyBuilder<TSource>> setPropertyAction, CancellationToken cancellationToken = default)
        where TSource : class
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(setPropertyAction);

        SetPropertyBuilder<TSource> builder = new();
        setPropertyAction(builder);

        int count = await source.ExecuteUpdateAsync(builder.GenerateLambda(), cancellationToken).ConfigureAwait(false);
        return count;
    }

    #endregion public ExecuteUpdateGetCount methods

    #region public ExecuteUpdateGetRows methods

    /// <summary>
    /// Updates all database rows for the entity instances which match the LINQ query from the database.
    /// </summary>
    /// <typeparam name="TSource">Type of source.</typeparam>
    /// <param name="source">The source query.</param>
    /// <param name="setPropertyAction">A method containing set property statements specifying properties to update.</param>
    /// <returns>An <see cref="IList{TSource}"/> containing the rows that have been modified.</returns>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0305:Simplify collection initialization", Justification = "Keep toList code similar in standard/async methods.")]
    public static IList<TSource> ExecuteUpdateGetRows<TSource>(this IQueryable<TSource> source, Action<SetPropertyBuilder<TSource>> setPropertyAction)
        where TSource : class
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(setPropertyAction);

        // Get the properties to update.
        SetPropertyBuilder<TSource> builder = new();
        setPropertyAction(builder);

        // Create the Unique update Id and insert it as a tag into the query.
        source = TagWithUpdateId(source, out Guid updateId);

        // Execute update gets trapped by ExecuteUpdateGetRowsCommandInterceptor.
        // It does not run the update but stores the SQL and parameters.
        int result = source.ExecuteUpdate(builder.GenerateLambda());
        if (result != ExecuteUpdateGetRowsCommandInterceptor.ExecuteUpdateGetRowsSentinelResult)
        {
            throw new InvalidOperationException("Update has been executed but cannot obtain updated rows. Check UseExecuteUpdateExtensions method has been called.");
        }

        ExecuteUpdateGetRowsCommandInterceptor.Instance.FetchUpdateParameters(updateId, out string sql, out object[] parameters);

        // Add a clause to the update to return any modified rows.
        DbContext context = source.GetContext();
        string databaseType = context.Database.GetDatabaseType();
        sql = AddOutputClauseToSql(databaseType, sql);

        // Execute the query and return the results.
        IList<TSource> updateResult = context.Set<TSource>()
            .FromSqlRaw(sql, parameters)
            .AsNoTracking()
            .ToList();

        return updateResult;
    }

    /// <summary>
    /// Updates all database rows for the entity instances which match the LINQ query from the database.
    /// </summary>
    /// <typeparam name="TSource">Type of source.</typeparam>
    /// <param name="source">The source query.</param>
    /// <param name="setPropertyAction">A method containing set property statements specifying properties to update.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
    /// <returns>An <see cref="IList{TSource}"/> containing the rows that have been modified.</returns>
    public static Task<IList<TSource>> ExecuteUpdateGetRowsAsync<TSource>(this IQueryable<TSource> source, Action<SetPropertyBuilder<TSource>> setPropertyAction, CancellationToken cancellationToken = default)
        where TSource : class
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(setPropertyAction);

        return ExecuteUpdateGetRowsInternalAsync();

        async Task<IList<TSource>> ExecuteUpdateGetRowsInternalAsync()
        {
            // Get the properties to update.
            SetPropertyBuilder<TSource> builder = new();
            setPropertyAction(builder);

            // Create the Unique update Id and insert it as a tag into the query.
            source = TagWithUpdateId(source, out Guid updateId);

            // Execute update gets trapped by ExecuteUpdateGetRowsCommandInterceptor.
            // It does not run the update but stores the SQL and parameters.
            int result = await source.ExecuteUpdateAsync(builder.GenerateLambda(), cancellationToken).ConfigureAwait(false);
            if (result != ExecuteUpdateGetRowsCommandInterceptor.ExecuteUpdateGetRowsSentinelResult)
            {
                throw new InvalidOperationException("Update has been executed but cannot obtain updated rows. Check UseExecuteUpdateExtensions method has been called.");
            }

            ExecuteUpdateGetRowsCommandInterceptor.Instance.FetchUpdateParameters(updateId, out string sql, out object[] parameters);

            // Add a clause to the update to return any modified rows.
            DbContext context = source.GetContext();
            string databaseType = context.Database.GetDatabaseType();
            sql = AddOutputClauseToSql(databaseType, sql);

            // Execute the query and return the results.
            IList<TSource> updateResult = await context.Set<TSource>()
                .FromSqlRaw(sql, parameters)
                .AsNoTracking()
                .ToListAsync(cancellationToken)
                .ConfigureAwait(false);

            return updateResult;
        }
    }

    #endregion public ExecuteUpdateGetRows methods

    #region private methods

    /// <summary>
    /// Tag a query with the name ExecuteUpdateGetRows and the updateId GUID in number format.
    /// </summary>
    /// <param name="source">The source query.</param>
    /// <param name="updateId">The updateId.</param>
    /// <returns>The modified query.</returns>
    private static IQueryable<TSource> TagWithUpdateId<TSource>(IQueryable<TSource> source, out Guid updateId)
    {
        updateId = Guid.NewGuid();
        return source.TagWith($"ExecuteUpdateGetRows {updateId:N}");
    }

    /// <summary>
    /// Add an Output / Returning clause to a SQL update statement.
    /// </summary>
    /// <param name="databaseType">The database type.</param>
    /// <param name="sql">The sql to add the clause to.</param>
    /// <returns>The SQL with the clause added.</returns>
    private static string AddOutputClauseToSql(string databaseType, string sql)
    {
        switch (databaseType)
        {
            case DatabaseType.Sqlite:
                sql += Environment.NewLine + "RETURNING *";
                break;

            case DatabaseType.SqlServer:
                Match match = FindFromClauseRegex().Match(sql);

                if (!match.Success)
                {
                    throw new InvalidOperationException("Unsupported SQL format");
                }

                sql = sql[..match.Index] + "OUTPUT INSERTED.*" + Environment.NewLine + sql[match.Index..];
                break;

            default:
                throw new InvalidOperationException("Unsupported database type");
        }

        return sql;
    }

    /// <summary>
    /// Find the FROM clause in the EF generated SQL.
    /// EF puts the SELECT/FROM/WHERE etc. at the start of a line in upper case.
    /// </summary>
    [GeneratedRegex("^FROM ", RegexOptions.Multiline)]
    private static partial Regex FindFromClauseRegex();

    #endregion private methods
}
