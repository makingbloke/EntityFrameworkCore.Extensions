// Copyright ©2021-2025 Mike King.
// This file is licensed to you under the MIT license.
// See the License.txt file in the solution root for more information.

using DotDoc.EntityFrameworkCore.Extensions;
using Microsoft.EntityFrameworkCore;

namespace DotDoc.EntityFrameworkCore.Extensions;

/// <summary>
/// Entity Framework Core ExecuteUpdate Extensions.
/// </summary>
public static class ExecuteUpdateExtensions
{
    #region Public ExecuteUpdate methods

    /// <summary>
    /// Updates all database rows for the entity instances which match the LINQ query from the database.
    /// </summary>
    /// <typeparam name="TSource">Type of source.</typeparam>
    /// <param name="source">The LINQ query.</param>
    /// <param name="setPropertyAction">Action used to set property values.</param>
    /// <returns>The number of rows updated in the database.</returns>
    public static int ExecuteUpdate<TSource>(this IQueryable<TSource> source, Action<SetPropertyBuilder<TSource>> setPropertyAction)
        where TSource : class
    {
        SetPropertyBuilder<TSource> builder = new();
        setPropertyAction(builder);

        return source.ExecuteUpdate(builder.GenerateLambda());
    }

    /// <summary>
    /// Updates all database rows for the entity instances which match the LINQ query from the database.
    /// </summary>
    /// <typeparam name="TSource">Type of source.</typeparam>
    /// <param name="source">The LINQ query.</param>
    /// <param name="setPropertyAction">Action used to set property values.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
    /// <returns>The number of rows updated in the database.</returns>
    public static async Task<int> ExecuteUpdateAsync<TSource>(this IQueryable<TSource> source, Action<SetPropertyBuilder<TSource>> setPropertyAction, CancellationToken cancellationToken = default)
        where TSource : class
    {
        SetPropertyBuilder<TSource> builder = new();
        setPropertyAction(builder);

        return await source.ExecuteUpdateAsync(builder.GenerateLambda(), cancellationToken).ConfigureAwait(false);
    }

    #endregion Public ExecuteUpdate methods
}
