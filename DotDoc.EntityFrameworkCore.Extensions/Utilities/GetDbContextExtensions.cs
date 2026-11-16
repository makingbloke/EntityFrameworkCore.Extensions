// Copyright ©2021-2026 Mike King.
// This file is licensed to you under the MIT license.
// See the License.txt file in the solution root for more information.

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Query.Internal;
using Microsoft.EntityFrameworkCore.Storage;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;

namespace DotDoc.EntityFrameworkCore.Extensions.Utilities;

/// <summary>
/// Get DbContext Extensions.
/// </summary>
[SuppressMessage("Usage", "EF1001:Internal EF Core API usage.", Justification = "Internal APIs are needed to get the DbContext from a query.")]
[SuppressMessage("Major Code Smell", "S3011:Reflection should not be used to increase accessibility of classes, methods, or fields", Justification = "Internal values are needed to get the DbContext from a query.")]
public static class GetDbContextExtensions
{
    #region private fields

    /// <summary>
    /// EF Core - Internal Query Compiler.
    /// Used by GetDbContext(this IQueryable source) to get the internal DbContext value.
    /// </summary>
    private static readonly FieldInfo QueryCompilerField = typeof(EntityQueryProvider)
        .GetField("_queryCompiler", BindingFlags.NonPublic | BindingFlags.Instance)!;

    /// <summary>
    /// EF Core - Internal Query Context Factory.
    /// Used by GetDbContext(this IQueryable source) to get the internal DbContext value.
    /// </summary>
    private static readonly FieldInfo ContextFactoryField = typeof(QueryCompiler)
        .GetField("_queryContextFactory", BindingFlags.NonPublic | BindingFlags.Instance)!;

    /// <summary>
    /// EF Core - Internal Query Context Factory Dependencies.
    /// Used by GetDbContext(this IQueryable source) to get the internal DbContext value.
    /// </summary>
    private static readonly PropertyInfo DependenciesProperty = typeof(RelationalQueryContextFactory)
        .GetProperty("Dependencies", BindingFlags.NonPublic | BindingFlags.Instance)!;

    #endregion private fields

    #region public methods

    /// <summary>
    /// Gets the <see cref="DbContext"/> object that is used by the specified <see cref="DatabaseFacade"/>.
    /// </summary>
    /// <param name="database">The <see cref="DatabaseFacade"/>.</param>
    /// <returns><see cref="DbContext"/>.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="database"/> is <see langword="null"/>.</exception>
    public static DbContext GetDbContext(this DatabaseFacade database)
    {
        ArgumentNullException.ThrowIfNull(database);

        DbContext context = ((IDatabaseFacadeDependenciesAccessor)database).Context;
        return context;
    }

    /// <summary>
    /// Gets the <see cref="DbContext"/> object that is used by the specified <see cref="IQueryable{T}"/>.
    /// </summary>
    /// <param name="source">The source query <see cref="IQueryable{TSource}" />.</param>
    /// <returns><see cref="DbContext"/>.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="source"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentException"><paramref name="source"/> is not an EF Core query.</exception>
    public static DbContext GetDbContext(this IQueryable source)
    {
        ArgumentNullException.ThrowIfNull(source);

        if (source.Provider is not EntityQueryProvider)
        {
            throw new ArgumentException("Query provider is not EF Core", nameof(source));
        }

        QueryCompiler queryCompiler = (QueryCompiler)QueryCompilerField.GetValue(source.Provider)!;
        RelationalQueryContextFactory contextFactory = (RelationalQueryContextFactory)ContextFactoryField.GetValue(queryCompiler)!;
        QueryContextDependencies dependencies = (QueryContextDependencies)DependenciesProperty.GetValue(contextFactory)!;

        DbContext context = dependencies.CurrentContext.Context;
        return context;
    }

    #endregion public methods
}
