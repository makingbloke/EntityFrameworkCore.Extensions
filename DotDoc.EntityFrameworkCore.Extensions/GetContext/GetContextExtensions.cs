// Copyright ©2021-2025 Mike King.
// This file is licensed to you under the MIT license.
// See the License.txt file in the solution root for more information.

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Query.Internal;
using Microsoft.EntityFrameworkCore.Storage;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;

namespace DotDoc.EntityFrameworkCore.Extensions.GetContext;

/// <summary>
/// Get Context Extensions.
/// </summary>
public static class GetContextExtensions
{
    #region public methods

    /// <summary>
    /// Gets the DbContext object that is used by the specified <see cref="DatabaseFacade"/>.
    /// </summary>
    /// <param name="databaseFacade">The <see cref="DatabaseFacade"/>.</param>
    /// <returns><see cref="DbContext"/>.</returns>
    public static DbContext GetContext(this DatabaseFacade databaseFacade)
    {
        ArgumentNullException.ThrowIfNull(databaseFacade);

        DbContext context = ((IDatabaseFacadeDependenciesAccessor)databaseFacade).Context;
        return context;
    }

    /// <summary>
    /// Gets the DbContext object that is used by the specified <see cref="IQueryable{T}"/>.
    /// </summary>
    /// <typeparam name = "TEntity" > Type of entity to return.</typeparam>
    /// <param name="query">The LINQ query.</param>
    /// <returns><see cref="DbContext"/>.</returns>
    [SuppressMessage("Usage", "EF1001:Internal EF Core API usage.", Justification = "We need to retrieve the context from the query.")]
    [SuppressMessage("Major Code Smell", "S3011:Reflection should not be used to increase accessibility of classes, methods, or fields", Justification = "We need to retrieve the context from the query.")]
    public static DbContext GetDbContext<TEntity>(this IQueryable<TEntity> query)
        where TEntity : class
    {
        ArgumentNullException.ThrowIfNull(query);

        if (query is not DbSet<TEntity> && query is not EntityQueryable<TEntity>)
        {
            throw new ArgumentException("Query was not created by EF Core", nameof(query));
        }

        QueryCompiler queryCompiler = (QueryCompiler)typeof(EntityQueryProvider)
            .GetField("_queryCompiler", BindingFlags.NonPublic | BindingFlags.Instance)!
            .GetValue(query.Provider)!;

        RelationalQueryContextFactory contextFactory = (RelationalQueryContextFactory)queryCompiler.GetType()
            .GetField("_queryContextFactory", BindingFlags.NonPublic | BindingFlags.Instance)!
            .GetValue(queryCompiler)!;

        QueryContextDependencies dependencies = (QueryContextDependencies)typeof(RelationalQueryContextFactory)
            .GetProperty("Dependencies", BindingFlags.NonPublic | BindingFlags.Instance)!
            .GetValue(contextFactory)!;

        DbContext context = dependencies.CurrentContext!.Context!;
        return context;
    }

    #endregion public methods
}
