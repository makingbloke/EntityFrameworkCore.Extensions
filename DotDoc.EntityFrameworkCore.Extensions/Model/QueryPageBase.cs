// Copyright ©2021-2025 Mike King.
// This file is licensed to you under the MIT license.
// See the License.txt file in the solution root for more information.

namespace DotDoc.EntityFrameworkCore.Extensions.Model;

/// <summary>
/// A page of query data. Returned by one of the ExecutePagedQueryxxxxx extensions.
/// </summary>
/// <param name="page">The page number.</param>
/// <param name="pageSize">The page size.</param>
/// <param name="recordCount">The number of records.</param>
/// <param name="pageCount">The number of pages.</param>
public abstract class QueryPageBase(long page, long pageSize, long recordCount, long pageCount)
{
    #region public properties

    /// <summary>
    /// Gets the page number.
    /// </summary>
    public long Page { get; } = page;

    /// <summary>
    /// Gets the page size.
    /// </summary>
    public long PageSize { get; } = pageSize;

    /// <summary>
    /// Gets the number of records.
    /// </summary>
    public long RecordCount { get; } = recordCount;

    /// <summary>
    /// Gets the number of pages.
    /// </summary>
    public long PageCount { get; } = pageCount;

    #endregion public properties
}