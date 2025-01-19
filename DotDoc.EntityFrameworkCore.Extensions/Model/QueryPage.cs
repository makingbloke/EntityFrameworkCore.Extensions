// Copyright ©2021-2025 Mike King.
// This file is licensed to you under the MIT license.
// See the License.txt file in the solution root for more information.

namespace DotDoc.EntityFrameworkCore.Extensions.Model;

/// <summary>
/// A page of query data. Returned by one of the ExecutePagedQueryxxxxx extensions.
/// </summary>
/// <typeparam name="T">The type of page data.</typeparam>
public class QueryPage<T>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="QueryPage{T}"/> class.
    /// </summary>
    /// <param name="page">The page number.</param>
    /// <param name="pageSize">The page size.</param>
    /// <param name="recordCount">The number of records.</param>
    /// <param name="pageCount">The number of pages.</param>
    /// <param name="pageData">The page data.</param>
    internal QueryPage(long page, long pageSize, long recordCount, long pageCount, T pageData)
    {
        this.Page = page;
        this.PageSize = pageSize;
        this.RecordCount = recordCount;
        this.PageCount = pageCount;
        this.PageData = pageData;
    }

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

    /// <summary>
    /// Gets the page data.
    /// </summary>
    public T PageData { get; }

    #endregion public properties
}