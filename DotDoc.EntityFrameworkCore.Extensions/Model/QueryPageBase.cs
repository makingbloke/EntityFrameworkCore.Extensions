// Copyright ©2021-2025 Mike King.
// This file is licensed to you under the MIT license.
// See the License.txt file in the solution root for more information.

namespace DotDoc.EntityFrameworkCore.Extensions.Model;

/// <summary>
/// A page of query data. Returned by one of the ExecutePagedQueryxxxxx extensions.
/// </summary>
public abstract class QueryPageBase
{
    #region private protected constructors

    /// <summary>
    /// Initializes a new instance of the <see cref="QueryPageBase"/> class.
    /// </summary>
    /// <param name="page">The page number.</param>
    /// <param name="pageSize">The page size.</param>
    /// <param name="recordCount">The number of records.</param>
    /// <param name="pageCount">The number of pages.</param>
    private protected QueryPageBase(long page, long pageSize, long recordCount, long pageCount)
    {
        this.Page = page;
        this.PageSize = pageSize;
        this.RecordCount = recordCount;
        this.PageCount = pageCount;
    }

    #endregion private protected constructors

    #region public properties

    /// <summary>
    /// Gets the page number.
    /// </summary>
    public long Page { get; }

    /// <summary>
    /// Gets the page size.
    /// </summary>
    public long PageSize { get; }

    /// <summary>
    /// Gets the number of records.
    /// </summary>
    public long RecordCount { get; }

    /// <summary>
    /// Gets the number of pages.
    /// </summary>
    public long PageCount { get; }

    #endregion public properties
}