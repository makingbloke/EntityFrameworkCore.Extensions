// Copyright ©2021-2026 Mike King.
// This file is licensed to you under the MIT license.
// See the License.txt file in the solution root for more information.

using DotDoc.EntityFrameworkCore.Extensions.ExecuteUpdate;

namespace DotDoc.EntityFrameworkCore.Extensions.CustomQueryGenerators;

/// <summary>
/// Custom Tags Handler.
/// </summary>
internal sealed class CustomTagsHandler
{
    #region public constants

    /// <summary>
    /// Execute Update tag prefix.
    /// </summary>
    public const string ExecuteUpdateTagPrefix = "$$EXECUTEUPDATE$$";

    /// <summary>
    /// Table hints tag name.
    /// </summary>
    public const string TableHintsTagPrefix = "$$TABLEHINTS$$";

    #endregion public constants

    #region private constructors

    /// <summary>
    /// Initializes a new instance of the <see cref="CustomTagsHandler"/> class.
    /// </summary>
    /// <param name="queryType">The query type value.</param>
    /// <param name="tableHints">The table hints value.</param>
    /// <param name="remainingTags">The remaining tags.</param>
    private CustomTagsHandler(QueryType queryType, string? tableHints, ISet<string> remainingTags)
    {
        this.QueryType = queryType;
        this.TableHints = tableHints;
        this.RemainingTags = remainingTags;
    }

    #endregion private constructors

    #region public properties

    /// <summary>
    /// Gets the query type value.
    /// </summary>
    public QueryType QueryType { get; }

    /// <summary>
    /// Gets the table hints value.
    /// </summary>
    public string? TableHints { get; }

    /// <summary>
    /// Gets the remaining tags.
    /// </summary>
    public ISet<string> RemainingTags { get; }

    #endregion public properties

    #region public methods

    /// <summary>
    /// process a set of tags and extract the custom values.
    /// </summary>
    /// <param name="tags">The tag list to process.</param>
    /// <returns>An instance of <see cref="CustomTagsHandler"/> holding the values.</returns>
    public static CustomTagsHandler ProcessTags(ISet<string> tags)
    {
        QueryType queryType = QueryType.Unknown;
        string? tableHints = null;
        HashSet<string> remainingTags = [];

        foreach (string tag in tags)
        {
            if (tag.StartsWith(ExecuteUpdateTagPrefix, StringComparison.Ordinal))
            {
                if (queryType != QueryType.Unknown)
                {
                    throw new InvalidOperationException("Multiple execute update tags in one query are not supported.");
                }

                queryType = Enum.Parse<QueryType>(tag[ExecuteUpdateTagPrefix.Length..]);
            }
            else if (tag.StartsWith(TableHintsTagPrefix, StringComparison.Ordinal))
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

        return new(queryType, tableHints, remainingTags);
    }

    #endregion public methods
}
