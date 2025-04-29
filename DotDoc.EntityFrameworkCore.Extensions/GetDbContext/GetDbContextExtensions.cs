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

namespace DotDoc.EntityFrameworkCore.Extensions.GetDbContext;

/// <summary>
/// Get DbContext Extensions.
/// </summary>
[SuppressMessage("Usage", "EF1001:Internal EF Core API usage.", Justification = "Internal APIs used by GetDbContext(this IQueryable query).")]
[SuppressMessage("Major Code Smell", "S3011:Reflection should not be used to increase accessibility of classes, methods, or fields", Justification = "Internal APIs used by GetDbContext(this IQueryable query).")]
public static class GetDbContextExtensions
{
    #region private fields

    /// <summary>
    /// EF Core - Internal Query Compiler.
    /// Used by GetDbContext(this IQueryable query) to get the internal DbContext value.
    /// </summary>
    private static readonly FieldInfo QueryCompilerField = typeof(EntityQueryProvider)
        .GetField("_queryCompiler", BindingFlags.NonPublic | BindingFlags.Instance)!;

    /// <summary>
    /// EF Core - Internal Query Context Factory.
    /// Used by GetDbContext(this IQueryable query) to get the internal DbContext value.
    /// </summary>
    private static readonly FieldInfo ContextFactoryField = typeof(QueryCompiler)
        .GetField("_queryContextFactory", BindingFlags.NonPublic | BindingFlags.Instance)!;

    /// <summary>
    /// EF Core - Internal Query Context Factory Dependencies.
    /// Used by GetDbContext(this IQueryable query) to get the internal DbContext value.
    /// </summary>
    private static readonly PropertyInfo DependenciesProperty = typeof(RelationalQueryContextFactory)
        .GetProperty("Dependencies", BindingFlags.NonPublic | BindingFlags.Instance)!;

    #endregion private fields

    #region public methods

    /// <summary>
    /// Gets the <see cref="DbContext"/> object that is used by the specified <see cref="DatabaseFacade"/>.
    /// </summary>
    /// <param name="databaseFacade">The <see cref="DatabaseFacade"/>.</param>
    /// <returns><see cref="DbContext"/>.</returns>
    public static DbContext GetDbContext(this DatabaseFacade databaseFacade)
    {
        ArgumentNullException.ThrowIfNull(databaseFacade);

        DbContext context = ((IDatabaseFacadeDependenciesAccessor)databaseFacade).Context;
        return context;
    }

    /// <summary>
    /// Gets the <see cref="DbContext"/> object that is used by the specified <see cref="IQueryable{T}"/>.
    /// </summary>
    /// <param name="query">The LINQ query.</param>
    /// <returns><see cref="DbContext"/>.</returns>
    public static DbContext GetDbContext(this IQueryable query)
    {
        ArgumentNullException.ThrowIfNull(query);

        if (query.Provider is not EntityQueryProvider)
        {
            throw new ArgumentException("Query was not created by EF Core", nameof(query));
        }

        QueryCompiler queryCompiler = (QueryCompiler)QueryCompilerField.GetValue(query.Provider)!;
        RelationalQueryContextFactory contextFactory = (RelationalQueryContextFactory)ContextFactoryField.GetValue(queryCompiler)!;
        QueryContextDependencies dependencies = (QueryContextDependencies)DependenciesProperty.GetValue(contextFactory)!;

        DbContext context = dependencies.CurrentContext!.Context!;
        return context;
    }

    #endregion public methods
}
