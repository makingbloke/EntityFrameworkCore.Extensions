// Copyright ©2021-2025 Mike King.
// This file is licensed to you under the MIT license.
// See the License.txt file in the solution root for more information.

namespace DotDoc.EntityFrameworkCore.Extensions.Execute;

/// <summary>
/// Paged Query Result.
/// </summary>
/// <typeparam name="TSource">The type of the elements of the source.</typeparam>
public sealed class PagedQueryResult<TSource>
    where TSource : class
{
    #region internal constructors

    /// <summary>
    /// Initializes a new instance of the <see cref="PagedQueryResult{Source}"/> class.
    /// </summary>
    /// <param name="page">The page number.</param>
    /// <param name="pageSize">The page size.</param>
    /// <param name="recordCount">The number of records.</param>
    /// <param name="pageCount">The number of pages.</param>
    /// <param name="result">Result of the query.</param>
    internal PagedQueryResult(long page, long pageSize, long recordCount, long pageCount, IList<TSource> result)
    {
        this.Page = page;
        this.PageSize = pageSize;
        this.RecordCount = recordCount;
        this.PageCount = pageCount;
        this.Result = result;
    }

    #endregion internal constructors

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
    /// Gets the result of the query.
    /// </summary>
    public IList<TSource> Result { get; }

    #endregion public properties
}
