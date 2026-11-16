// Copyright ©2021-2026 Mike King.
// This file is licensed to you under the MIT license.
// See the License.txt file in the solution root for more information.

using DotDoc.EntityFrameworkCore.Extensions.ExecuteUpdate;

namespace DotDoc.EntityFrameworkCore.Extensions.CustomQueryGenerators;

/// <summary>
/// The custom values read from a query's tags by <see cref="CustomTagExtensions.GetCustomTags"/>.
/// </summary>
internal sealed class CustomTags
{
    #region public constructors

    /// <summary>
    /// Initializes a new instance of the <see cref="CustomTags"/> class.
    /// </summary>
    /// <param name="queryType">The query type value.</param>
    /// <param name="tableHints">The table hints value.</param>
    /// <param name="remainingTags">The remaining tags.</param>
    public CustomTags(QueryType queryType, string? tableHints, ISet<string> remainingTags)
    {
        this.QueryType = queryType;
        this.TableHints = tableHints;
        this.RemainingTags = remainingTags;
    }

    #endregion public constructors

    #region public properties

    /// <summary>
    /// Gets the query type value (<see cref="QueryType.Unknown"/> if the query has no execute update tag).
    /// </summary>
    public QueryType QueryType { get; }

    /// <summary>
    /// Gets the table hints value (<see langword="null"/> if the query has no table hints tag).
    /// </summary>
    public string? TableHints { get; }

    /// <summary>
    /// Gets the remaining tags (a copy of the tags without the custom tags).
    /// </summary>
    public ISet<string> RemainingTags { get; }

    #endregion public properties
}
