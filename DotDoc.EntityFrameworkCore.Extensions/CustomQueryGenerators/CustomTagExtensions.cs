// Copyright ©2021-2026 Mike King.
// This file is licensed to you under the MIT license.
// See the License.txt file in the solution root for more information.

using DotDoc.EntityFrameworkCore.Extensions.ExecuteUpdate;
using DotDoc.EntityFrameworkCore.Extensions.TableHints;
using Microsoft.EntityFrameworkCore;

namespace DotDoc.EntityFrameworkCore.Extensions.CustomQueryGenerators;

/// <summary>
/// Custom Tag Extensions.
/// </summary>
internal static class CustomTagExtensions
{
    #region private constants

    /// <summary>
    /// Execute Update tag prefix.
    /// </summary>
    private const string ExecuteUpdateTagPrefix = "$$EXECUTEUPDATE$$";

    /// <summary>
    /// Table hints tag prefix.
    /// </summary>
    private const string TableHintsTagPrefix = "$$TABLEHINTS$$";

    #endregion private constants

    #region public methods

    /// <summary>
    /// Tag a query to tell the custom query generator which ExecuteUpdate variant to generate.
    /// </summary>
    /// <typeparam name="TSource">The type of the elements of <paramref name="source" />.</typeparam>
    /// <param name="source">The source query.</param>
    /// <param name="queryType">The query type.</param>
    /// <returns>The tagged query.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="source"/> is <see langword="null"/>.</exception>
    public static IQueryable<TSource> TagWithExecuteUpdate<TSource>(this IQueryable<TSource> source, QueryType queryType)
    {
        ArgumentNullException.ThrowIfNull(source);

        return source.TagWith($"{ExecuteUpdateTagPrefix}{queryType}");
    }

    /// <summary>
    /// Check if a tag is an execute update tag.
    /// </summary>
    /// <param name="tag">The tag.</param>
    /// <returns><see langword="true"/> if the tag is an execute update tag; otherwise, <see langword="false"/>.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="tag"/> is <see langword="null"/>.</exception>
    public static bool IsExecuteUpdateTag(string tag)
    {
        ArgumentNullException.ThrowIfNull(tag);

        return tag.StartsWith(ExecuteUpdateTagPrefix, StringComparison.Ordinal);
    }

    /// <summary>
    /// Tag a query to tell the custom query generator which table hints to add.
    /// </summary>
    /// <typeparam name="TSource">The type of the elements of <paramref name="source" />.</typeparam>
    /// <param name="source">The source query.</param>
    /// <param name="tableHints">The table hints.</param>
    /// <returns>The tagged query.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="source"/> or <paramref name="tableHints"/> is <see langword="null"/>.</exception>
    public static IQueryable<TSource> TagWithTableHints<TSource>(this IQueryable<TSource> source, IEnumerable<ITableHint> tableHints)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(tableHints);

        return source.TagWith($"{TableHintsTagPrefix}{string.Join(", ", tableHints.Select(th => th.ToString()))}");
    }

    /// <summary>
    /// Check if a tag is a table hints tag.
    /// </summary>
    /// <param name="tag">The tag.</param>
    /// <returns><see langword="true"/> if the tag is a table hints tag; otherwise, <see langword="false"/>.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="tag"/> is <see langword="null"/>.</exception>
    public static bool IsTableHintsTag(string tag)
    {
        ArgumentNullException.ThrowIfNull(tag);

        return tag.StartsWith(TableHintsTagPrefix, StringComparison.Ordinal);
    }

    /// <summary>
    /// Get the custom values from a set of tags.
    /// </summary>
    /// <param name="tags">The tag list to process.</param>
    /// <returns>An instance of <see cref="CustomTags"/> holding the values.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="tags"/> is <see langword="null"/>.</exception>
    /// <exception cref="InvalidOperationException"><paramref name="tags"/> has more than one execute update tag, or more than one table hints tag.</exception>
    public static CustomTags GetCustomTags(ISet<string> tags)
    {
        ArgumentNullException.ThrowIfNull(tags);

        QueryType? queryType = null;
        string? tableHints = null;
        HashSet<string> remainingTags = [];

        foreach (string tag in tags)
        {
            if (IsExecuteUpdateTag(tag))
            {
                if (queryType is not null)
                {
                    throw new InvalidOperationException("Multiple execute update tags in one query are not supported.");
                }

                queryType = Enum.Parse<QueryType>(tag[ExecuteUpdateTagPrefix.Length..]);
            }
            else if (IsTableHintsTag(tag))
            {
                if (tableHints is not null)
                {
                    throw new InvalidOperationException("Multiple table hints tags in one query are not supported.");
                }

                tableHints = tag[TableHintsTagPrefix.Length..];
            }
            else
            {
                remainingTags.Add(tag);
            }
        }

        return new(queryType ?? QueryType.Unknown, tableHints, remainingTags);
    }

    #endregion public methods
}
