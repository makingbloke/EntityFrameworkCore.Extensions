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
    /// <typeparam name="TEntity">Type of Entity.</typeparam>
    /// <param name="query">The LINQ query.</param>
    /// <param name="setPropertyAction">Action used to set property values.</param>
    /// <returns>The number of rows updated in the database.</returns>
    public static int ExecuteUpdateGetCount<TEntity>(this IQueryable<TEntity> query, Action<SetPropertyBuilder<TEntity>> setPropertyAction)
        where TEntity : class
    {
        ArgumentNullException.ThrowIfNull(query);
        ArgumentNullException.ThrowIfNull(setPropertyAction);

        SetPropertyBuilder<TEntity> builder = new();
        setPropertyAction(builder);

        int count = query
            .AsNoTracking()
            .ExecuteUpdate(builder.GenerateLambda());

        return count;
    }

    /// <summary>
    /// Updates all database rows for the entity instances which match the LINQ query from the database.
    /// </summary>
    /// <typeparam name="TEntity">Type of Entity.</typeparam>
    /// <param name="query">The LINQ query.</param>
    /// <param name="setPropertyAction">Action used to set property values.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
    /// <returns>The number of rows updated in the database.</returns>
    public static async Task<int> ExecuteUpdateGetCountAsync<TEntity>(this IQueryable<TEntity> query, Action<SetPropertyBuilder<TEntity>> setPropertyAction, CancellationToken cancellationToken = default)
        where TEntity : class
    {
        ArgumentNullException.ThrowIfNull(query);
        ArgumentNullException.ThrowIfNull(setPropertyAction);

        SetPropertyBuilder<TEntity> builder = new();
        setPropertyAction(builder);

        int count = await query
            .AsNoTracking()
            .ExecuteUpdateAsync(builder.GenerateLambda(), cancellationToken)
            .ConfigureAwait(false);

        return count;
    }

    #endregion public ExecuteUpdateGetCount methods

    #region public ExecuteUpdateGetRows methods

    /// <summary>
    /// Updates all database rows for the entity instances which match the LINQ query from the database.
    /// </summary>
    /// <typeparam name="TEntity">Type of Entity.</typeparam>
    /// <param name="query">The LINQ query.</param>
    /// <param name="setPropertyAction">A method containing set property statements specifying properties to update.</param>
    /// <returns>An <see cref="IList{TEntity}"/> containing the rows that have been modified.</returns>
    public static IList<TEntity> ExecuteUpdateGetRows<TEntity>(this IQueryable<TEntity> query, Action<SetPropertyBuilder<TEntity>> setPropertyAction)
        where TEntity : class
    {
        ArgumentNullException.ThrowIfNull(query);
        ArgumentNullException.ThrowIfNull(setPropertyAction);

        // Get the properties to update.
        SetPropertyBuilder<TEntity> builder = new();
        setPropertyAction(builder);

        // Create the Unique update Id and insert it as a tag into the query.
        query = TagWithUpdateId(query, out Guid updateId);

        // Execute update gets trapped by ExecuteUpdateGetRowsCommandInterceptor.
        // It does not run the update but stores the SQL and parameters.
        int result = query.ExecuteUpdate(builder.GenerateLambda());
        if (result != ExecuteUpdateGetRowsCommandInterceptor.ExecuteUpdateGetRowsSentinelResult)
        {
            throw new InvalidOperationException("Update has been executed but cannot obtain updated rows - Check UseExecuteUpdateExtensions method has been called");
        }

        ExecuteUpdateGetRowsCommandInterceptor.Instance.FetchUpdateParameters(updateId, out DbContext context, out string sql, out object[] parameters);

        // Add a clause to the update to return any modified rows.
        sql = AddOutputClauseToSql(context, sql);

        // Execute the query and return the results.
        IList<TEntity> updateResult = context.Set<TEntity>()
            .FromSqlRaw(sql, parameters)
            .AsNoTracking()
            .ToList();

        return updateResult;
    }

    /// <summary>
    /// Updates all database rows for the entity instances which match the LINQ query from the database.
    /// </summary>
    /// <typeparam name="TEntity">Type of Entity.</typeparam>
    /// <param name="query">The LINQ query.</param>
    /// <param name="setPropertyAction">A method containing set property statements specifying properties to update.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
    /// <returns>An <see cref="IList{TEntity}"/> containing the rows that have been modified.</returns>
    public static Task<IList<TEntity>> ExecuteUpdateGetRowsAsync<TEntity>(this IQueryable<TEntity> query, Action<SetPropertyBuilder<TEntity>> setPropertyAction, CancellationToken cancellationToken = default)
        where TEntity : class
    {
        ArgumentNullException.ThrowIfNull(query);
        ArgumentNullException.ThrowIfNull(setPropertyAction);

        return ExecuteUpdateGetRowsInternalAsync();

        async Task<IList<TEntity>> ExecuteUpdateGetRowsInternalAsync()
        {
            // Get the properties to update.
            SetPropertyBuilder<TEntity> builder = new();
            setPropertyAction(builder);

            // Create the Unique update Id and insert it as a tag into the query.
            query = TagWithUpdateId(query, out Guid updateId);

            // Execute update gets trapped by ExecuteUpdateGetRowsCommandInterceptor.
            // It does not run the update but stores the SQL and parameters.
            int result = await query.ExecuteUpdateAsync(builder.GenerateLambda(), cancellationToken).ConfigureAwait(false);
            if (result != ExecuteUpdateGetRowsCommandInterceptor.ExecuteUpdateGetRowsSentinelResult)
            {
                throw new InvalidOperationException("Update has been executed but cannot obtain updated rows - Check UseExecuteUpdateExtensions method has been called");
            }

            ExecuteUpdateGetRowsCommandInterceptor.Instance.FetchUpdateParameters(updateId, out DbContext context, out string sql, out object[] parameters);

            // Add a clause to the update to return any modified rows.
            sql = AddOutputClauseToSql(context, sql);

            // Execute the query and return the results.
            IList<TEntity> updateResult = await context.Set<TEntity>()
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
    /// <param name="query">The LINQ query.</param>
    /// <param name="updateId">The updateId.</param>
    /// <returns>The modified query.</returns>
    private static IQueryable<TEntity> TagWithUpdateId<TEntity>(IQueryable<TEntity> query, out Guid updateId)
    {
        updateId = Guid.NewGuid();
        return query.TagWith($"ExecuteUpdateGetRows {updateId:N}");
    }

    /// <summary>
    /// Add an Output / Returning clause to a SQL update statement.
    /// </summary>
    /// <param name="context">The database context.</param>
    /// <param name="sql">The sql to add the clause to.</param>
    /// <returns>The SQL with the clause added.</returns>
    private static string AddOutputClauseToSql(DbContext context, string sql)
    {
        string databaseType = context.Database.GetDatabaseType();

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
