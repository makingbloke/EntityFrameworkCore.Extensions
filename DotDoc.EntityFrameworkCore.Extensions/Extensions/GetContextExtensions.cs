// Copyright ©2021-2025 Mike King.
// This file is licensed to you under the MIT license.
// See the License.txt file in the solution root for more information.

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking.Internal;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Query.Internal;
using Microsoft.EntityFrameworkCore.Storage;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;

namespace DotDoc.EntityFrameworkCore.Extensions.Extensions;

/// <summary>
/// Entity Framework Core Get Context Extensions.
/// </summary>
public static class GetContextExtensions
{
    #region public GetContext methods

    /// <summary>
    /// Gets the DbContext object that is used by the specified <see cref="IQueryable{T}"/>.
    /// </summary>
    /// <typeparam name = "TEntity" > Type of entity to return.</typeparam>
    /// <param name="query">The LINQ query.</param>
    /// <returns><see cref="DbContext"/> or <see langword="null"/> if it is not available (such as object not created by EF core).</returns>
    /// <remarks>
    /// This code comes from https://stackoverflow.com/questions/53198376/net-ef-core-2-1-get-dbcontext-from-iqueryable-argument .
    /// </remarks>
    [SuppressMessage("Major Code Smell", "S3011:Reflection should not be used to increase accessibility of classes, methods, or fields", Justification = "We have cases where we need to get the context from the source.")]
    [SuppressMessage("Usage", "EF1001:Internal EF Core API usage.", Justification = "We have cases where we need to get the context from the source.")]
    public static DbContext GetContext<TEntity>(this IQueryable<TEntity> query)
    {
        ArgumentNullException.ThrowIfNull(query);

        DbContext context;

        try
        {
            object queryCompiler = typeof(EntityQueryProvider).GetField("_queryCompiler", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(query.Provider);
            object queryContextFactory = queryCompiler.GetType().GetField("_queryContextFactory", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(queryCompiler);

            object dependencies = typeof(RelationalQueryContextFactory).GetProperty("Dependencies", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(queryContextFactory);
            Type queryContextDependencies = typeof(DbContext).Assembly.GetType(typeof(QueryContextDependencies).FullName);
            object stateManagerProperty = queryContextDependencies.GetProperty("StateManager", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public).GetValue(dependencies);
            IStateManager stateManager = (IStateManager)stateManagerProperty;

            context = stateManager.Context;
        }
        catch
        {
            context = null;
        }

        return context;
    }

    /// <summary>
    /// Gets the DbContext object that is used by the specified <see cref="DatabaseFacade"/>.
    /// </summary>
    /// <param name="databaseFacade">The <see cref="DatabaseFacade"/>.</param>
    /// <returns><see cref="DbContext"/> or <see langword="null"/> if it is not available (such as object not created by EF core).</returns>
    public static DbContext GetContext(this DatabaseFacade databaseFacade)
    {
        ArgumentNullException.ThrowIfNull(databaseFacade);

        DbContext context = ((IDatabaseFacadeDependenciesAccessor)databaseFacade).Context;
        return context;
    }
        #endregion public GetContext methods
}
