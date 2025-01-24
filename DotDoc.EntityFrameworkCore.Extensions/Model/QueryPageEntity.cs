// Copyright ©2021-2025 Mike King.
// This file is licensed to you under the MIT license.
// See the License.txt file in the solution root for more information.

namespace DotDoc.EntityFrameworkCore.Extensions.Model;

/// <summary>
/// A page of query data. Returned by one of the ExecutePagedQueryxxxxx extensions.
/// </summary>
/// <typeparam name="TEntity">Entity Type.</typeparam>
public sealed class QueryPageEntity<TEntity> : QueryPageBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="QueryPageEntity{TEntity}"/> class.
    /// </summary>
    /// <param name="page">The page number.</param>
    /// <param name="pageSize">The page size.</param>
    /// <param name="recordCount">The number of records.</param>
    /// <param name="pageCount">The number of pages.</param>
    /// <param name="result">Result of the query.</param>
    internal QueryPageEntity(long page, long pageSize, long recordCount, long pageCount, IList<TEntity> result)
        : base(page, pageSize, recordCount, pageCount)
    {
        this.Result = result;
    }

    /// <summary>
    /// Gets the result of the query.
    /// </summary>
    public IList<TEntity> Result { get; }
}
