// Copyright ©2021-2025 Mike King.
// This file is licensed to you under the MIT license.
// See the License.txt file in the solution root for more information.

using DotDoc.EntityFrameworkCore.Extensions.DatabaseType;
using DotDoc.EntityFrameworkCore.Extensions.GetDbContext;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using System.Text.RegularExpressions;

namespace DotDoc.EntityFrameworkCore.Extensions.ExecuteDelete;

/// <summary>
/// Entity Framework Core ExecuteDelete Extensions.
/// </summary>
public static partial class ExecuteDeleteExtensions
{
    #region public methods

    /// <summary>
    /// Use the Execute Delete Extensions.
    /// </summary>
    /// <param name="optionsBuilder">The builder being used to configure the context.</param>
    /// <returns>The same builder instance so multiple calls can be chained.</returns>
    public static DbContextOptionsBuilder UseExecuteDeleteExtensions(this DbContextOptionsBuilder optionsBuilder)
    {
        ArgumentNullException.ThrowIfNull(optionsBuilder);

        return optionsBuilder.AddInterceptors(ExecuteDeleteGetRowsCommandInterceptor.Instance);
    }

    #endregion public methods

    #region public ExecuteDeleteGetCount methods

    /// <summary>
    /// Deletes all database rows for the entity instances which match the LINQ query from the database.
    /// </summary>
    /// <typeparam name="TEntity">Type of Entity.</typeparam>
    /// <param name="query">The LINQ query.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
    /// <returns>The number of rows deleted in the database.</returns>
    public static async Task<int> ExecuteDeleteGetCountAsync<TEntity>(this IQueryable<TEntity> query, CancellationToken cancellationToken = default)
        where TEntity : class
    {
        ArgumentNullException.ThrowIfNull(query);

        int count = await query
            .ExecuteDeleteAsync(cancellationToken)
            .ConfigureAwait(false);

        return count;
    }

    #endregion public ExecuteDeleteGetCount methods

    #region public ExecuteDeleteGetRows methods

    /// <summary>
    /// Deletes all database rows for the entity instances which match the LINQ query from the database.
    /// </summary>
    /// <typeparam name="TEntity">Type of Entity.</typeparam>
    /// <param name="query">The LINQ query.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
    /// <returns>An <see cref="IList{TEntity}"/> containing the modified rows.</returns>
    public static async Task<IList<TEntity>> ExecuteDeleteGetRowsAsync<TEntity>(this IQueryable<TEntity> query, CancellationToken cancellationToken = default)
        where TEntity : class
    {
        ArgumentNullException.ThrowIfNull(query);

        bool useReturningClause = UseReturningClause(query);

        Task<IList<TEntity>> task = useReturningClause
            ? ExecuteDeleteGetRowsWithReturningClauseAsync(query, cancellationToken)
            : ExecuteDeleteGetRowsWithSelectQueryAsync(query, cancellationToken);

        IList<TEntity> results = await task.ConfigureAwait(false);
        return results;
    }

    #endregion public ExecuteDeleteGetRows methods

    #region private methods

    /// <summary>
    /// Find out if we are going to use a Returning/Output clause (the default) or just select the results after an delete.
    /// </summary>
    /// <typeparam name="TEntity">Type of Entity.</typeparam>
    /// <param name="query">The LINQ query.</param>
    /// <returns><see langword="true"/> if we are going to use Returning/Output else, <see langword="false"/>.</returns>
    private static bool UseReturningClause<TEntity>(IQueryable<TEntity> query)
    {
        DbContext context = query.GetDbContext();
        string databaseType = context.Database.GetDatabaseType();
        IEntityType? entityType = context.Model.FindEntityType(typeof(TEntity));

        if (entityType == null)
        {
            throw new InvalidOperationException($"Entity {typeof(TEntity).Name} not found.");
        }

        bool useReturningClause = databaseType switch
        {
            DatabaseTypes.Sqlite => entityType.IsSqlReturningClauseUsed(),
            DatabaseTypes.SqlServer => entityType.IsSqlOutputClauseUsed(),
            _ => throw new InvalidOperationException("Unsupported database type")
        };

        return useReturningClause;
    }

    /// <summary>
    /// Deletes all database rows for the entity instances which match the LINQ query from the database.
    /// (Using a Returning / Output clause to get any modified rows).
    /// </summary>
    /// <typeparam name="TEntity">Type of Entity.</typeparam>
    /// <param name="query">The LINQ query.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
    /// <returns>An <see cref="IQueryable{TEntity}"/> containing the modified rows.</returns>
    private static async Task<IList<TEntity>> ExecuteDeleteGetRowsWithReturningClauseAsync<TEntity>(IQueryable<TEntity> query, CancellationToken cancellationToken)
        where TEntity : class
    {
        DbContext context = query.GetDbContext();

        // Check the interceptor has been registered.
        // This is only needed when we use the Returning / Output clause as it is used to capture the delete SQL.
        if (!IsInterceptorRegisterered(context))
        {
            throw new InvalidOperationException($"The extensions have not been registered. Make sure {nameof(UseExecuteDeleteExtensions)} has been called.");
        }

        // Tag the delete with a unique delete id so the interceptor knows to capture the generated SQL.
        Guid deleteId = Guid.NewGuid();
        query = query.TagWith($"ExecuteDeleteGetRows {deleteId:N}");

        // Execute the delete and get the captured SQL and parameters from the interceptor.
        await query.ExecuteDeleteAsync(cancellationToken).ConfigureAwait(false);
        ExecuteDeleteQuery deleteQuery = ExecuteDeleteGetRowsCommandInterceptor.GetDeleteQuery(deleteId);

        // Modify the SQL to include the returning clause and execute it.
        string sql = AddReturningClauseToSql(context, deleteQuery.Sql);

        IList<TEntity> results = await context.Database.SqlQueryRaw<TEntity>(sql, deleteQuery.Parameters)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        return results;
    }

    /// <summary>
    /// Check the <see cref="ExecuteDeleteGetRowsCommandInterceptor"/> has been registered.
    /// </summary>
    /// <param name="context">The database context.</param>
    /// <returns><see langword="true"/> if the interceptor has been registered else, <see langword="false"/>.</returns>
    private static bool IsInterceptorRegisterered(DbContext context)
    {
        IDbContextOptions options = context.GetService<IDbContextOptions>();

        bool isRegistered = options.Extensions
            .OfType<CoreOptionsExtension>()
            .Where(e => e.Interceptors != null)
            .SelectMany(e => e.Interceptors!)
            .OfType<ExecuteDeleteGetRowsCommandInterceptor>()
            .Any();

        return isRegistered;
    }

    /// <summary>
    /// Add an Returning / Output clause to a SQL delete statement.
    /// </summary>
    /// <param name="context">The database context.</param>
    /// <param name="sql">The sql to add the clause to.</param>
    /// <returns>The modified SQL.</returns>
    private static string AddReturningClauseToSql(DbContext context, string sql)
    {
        string databaseType = context.Database.GetDatabaseType();

        switch (databaseType)
        {
            case DatabaseTypes.Sqlite:
                // In SQLite the RETURNING clause goes at the end of the delete statement.
                sql += Environment.NewLine + "RETURNING *";
                break;

            case DatabaseTypes.SqlServer:
                // In SQL Server the OUTPUT clause goes after the set and before the FROM / WHERE clauses.
                Match match = FindFromOrWhereClauseRegex().Match(sql);

                if (!match.Success)
                {
                    throw new InvalidOperationException("Unsupported SQL format");
                }

                int clausePos = match.Groups["Clause"].Index;
                sql = sql[..clausePos] + "OUTPUT INSERTED.*" + Environment.NewLine + sql[clausePos..];
                break;

            default:
                throw new InvalidOperationException("Unsupported database type");
        }

        return sql;
    }

    /// <summary>
    /// Find the FROM or WHERE clause in the EF generated SQL.
    /// EF puts the SELECT/FROM/WHERE etc. at the start of a line in upper case.
    /// </summary>
    [GeneratedRegex("^(?<Clause>(FROM |WHERE )|$)", RegexOptions.Multiline | RegexOptions.ExplicitCapture)]
    private static partial Regex FindFromOrWhereClauseRegex();

    /// <summary>
    /// Deletes all database rows for the entity instances which match the LINQ query from the database.
    /// (Using a select statement to get any modified rows).
    /// </summary>
    /// <typeparam name="TEntity">Type of Entity.</typeparam>
    /// <param name="query">The LINQ query.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
    /// <returns>An <see cref="IQueryable{TEntity}"/> containing the modified rows.</returns>
    private static async Task<IList<TEntity>> ExecuteDeleteGetRowsWithSelectQueryAsync<TEntity>(IQueryable<TEntity> query, CancellationToken cancellationToken)
        where TEntity : class
    {
        DbContext context = query.GetDbContext();

        await context.Database.BeginTransactionAsync(cancellationToken).ConfigureAwait(false);

        try
        {
            int count = await query.ExecuteDeleteAsync(cancellationToken).ConfigureAwait(false);

            IList<TEntity> results = count == 0
                ? []
                : await query
                    .AsNoTracking()
                    .ToListAsync(cancellationToken)
                    .ConfigureAwait(false);

            await context.Database.CommitTransactionAsync(cancellationToken).ConfigureAwait(false);
            return results;
        }
        catch (Exception)
        {
            await context.Database.RollbackTransactionAsync(cancellationToken).ConfigureAwait(false);
            throw;
        }
    }

    #endregion private methods
}
