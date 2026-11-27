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
    #region private fields

    /// <summary>
    /// Execute Update tag prefix (The <see cref="Guid"/> makes the tag prefix unique for this process to stop it being guessed).
    /// </summary>
    private static readonly string ExecuteUpdateTagPrefix = $"$$EXECUTEUPDATE{Guid.NewGuid():n}$$";

    /// <summary>
    /// Table hints tag prefix (The <see cref="Guid"/> makes the tag prefix unique for this process to stop it being guessed).
    /// </summary>
    private static readonly string TableHintsTagPrefix = $"$$TABLEHINTS{Guid.NewGuid():n}$$";

    #endregion private fields

    #region public methods

    /// <summary>
    /// Tag a query to tell the custom query generator which ExecuteUpdate variant to generate.
    /// </summary>
    /// <typeparam name="TSource">The type of the elements of <paramref name="source" />.</typeparam>
    /// <param name="source">The source query.</param>
    /// <param name="queryType">The query type.</param>
    /// <param name="entityTypeName">The name of the entity type the query is on, or <see langword="null"/> if the generator doesn't need it.</param>
    /// <returns>The tagged query.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="source"/> is <see langword="null"/>.</exception>
    public static IQueryable<TSource> TagWithExecuteUpdate<TSource>(this IQueryable<TSource> source, QueryType queryType, string? entityTypeName = null)
    {
        ArgumentNullException.ThrowIfNull(source);

        return source.TagWith($"{ExecuteUpdateTagPrefix}{queryType}:{entityTypeName}");
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
    /// Tag a query to tell the custom query generator which table hints to add to a table.
    /// </summary>
    /// <typeparam name="TSource">The type of the elements of <paramref name="source" />.</typeparam>
    /// <param name="source">The source query.</param>
    /// <param name="tableKey">The table the hints are for, from <see cref="GetTableKey(string, string?)"/>.</param>
    /// <param name="tableHints">The table hints.</param>
    /// <returns>The tagged query.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="source"/>, <paramref name="tableKey"/> or <paramref name="tableHints"/> is <see langword="null"/>.</exception>
    public static IQueryable<TSource> TagWithTableHints<TSource>(this IQueryable<TSource> source, string tableKey, IEnumerable<ITableHint> tableHints)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(tableKey);
        ArgumentNullException.ThrowIfNull(tableHints);

        return source.TagWith($"{TableHintsTagPrefix}{tableKey} {string.Join(", ", tableHints.Select(th => th.ToString()))}");
    }

    /// <summary>
    /// Get the key that identifies a table (or view) in a table hints tag: its delimited name, with its schema if it has one.
    /// </summary>
    /// <param name="name">The table name.</param>
    /// <param name="schema">The schema, or <see langword="null"/> if the table has no schema.</param>
    /// <returns>The key, e.g. <c>[dbo].[Customers]</c> or <c>[Customers]</c>.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="name"/> is <see langword="null"/>.</exception>
    /// <remarks>
    /// <see cref="TableHintExtensions.WithTableHints"/> makes the key from the entity type and the custom query generator from the table it is writing, so the
    /// hints are only added to their own table. Any <c>]</c> in a name is doubled, so a key always ends at a <c>]</c> that isn't doubled
    /// and isn't followed by a <c>.</c>, which is how <see cref="GetCustomTags"/> finds the end of the key.
    /// </remarks>
    public static string GetTableKey(string name, string? schema)
    {
        ArgumentNullException.ThrowIfNull(name);

        return string.IsNullOrEmpty(schema)
            ? DelimitIdentifier(name)
            : $"{DelimitIdentifier(schema)}.{DelimitIdentifier(name)}";
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
    /// <exception cref="InvalidOperationException"><paramref name="tags"/> has more than one execute update tag, different table hints for the same table, or a table hints tag that isn't valid.</exception>
    public static CustomTags GetCustomTags(ISet<string> tags)
    {
        ArgumentNullException.ThrowIfNull(tags);

        QueryType? queryType = null;
        string? entityTypeName = null;
        Dictionary<string, string> tableHints = [];
        HashSet<string> remainingTags = [];

        foreach (string tag in tags)
        {
            if (IsExecuteUpdateTag(tag))
            {
                if (queryType is not null)
                {
                    throw new InvalidOperationException("Multiple execute update tags in one query are not supported.");
                }

                string value = tag[ExecuteUpdateTagPrefix.Length..];
                int colon = value.IndexOf(':', StringComparison.Ordinal);

                queryType = Enum.Parse<QueryType>(value[..colon]);

                entityTypeName = value[(colon + 1)..];

                if (entityTypeName.Length == 0)
                {
                    entityTypeName = null;
                }
            }
            else if (IsTableHintsTag(tag))
            {
                // EF Core keeps the tags in a set, so the same hints for the same table only appear once. Another tag for the table must have different hints.
                string value = tag[TableHintsTagPrefix.Length..];
                int keyLength = GetTableKeyLength(value);
                string tableKey = value[..keyLength];

                if (!tableHints.TryAdd(tableKey, value[(keyLength + 1)..]))
                {
                    throw new InvalidOperationException($"Different table hints for the table {tableKey} in one query are not supported.");
                }
            }
            else
            {
                remainingTags.Add(tag);
            }
        }

        return new(queryType ?? QueryType.Unknown, entityTypeName, tableHints, remainingTags);
    }

    #endregion public methods

    #region private methods

    /// <summary>
    /// Delimit an identifier with square brackets, doubling any <c>]</c> in it.
    /// </summary>
    /// <param name="identifier">The identifier.</param>
    /// <returns>The delimited identifier.</returns>
    private static string DelimitIdentifier(string identifier)
    {
        return $"[{identifier.Replace("]", "]]", StringComparison.Ordinal)}]";
    }

    /// <summary>
    /// Find the length of the table key at the start of a table hints tag's value (the key, a space, then the hints).
    /// </summary>
    /// <param name="value">The tag's value, after the prefix.</param>
    /// <returns>The length of the key.</returns>
    /// <exception cref="InvalidOperationException">The value doesn't start with a table key followed by a space.</exception>
    private static int GetTableKeyLength(string value)
    {
        int i = 0;

        // The key is one or two delimited identifiers ([schema].[table]), with any ] in a name doubled (see GetTableKey).
        while (i < value.Length && value[i] == '[')
        {
            int close = value.IndexOf(']', i + 1);

            while (close >= 0 && close + 1 < value.Length && value[close + 1] == ']')
            {
                close = value.IndexOf(']', close + 2);
            }

            if (close < 0)
            {
                break;
            }

            i = close + 1;

            if (i < value.Length && value[i] == ' ')
            {
                return i;
            }

            if (i >= value.Length || value[i] != '.')
            {
                break;
            }

            i++;
        }

        throw new InvalidOperationException($"The table hints tag '{TableHintsTagPrefix}{value}' is not valid.");
    }

    #endregion private methods
}
