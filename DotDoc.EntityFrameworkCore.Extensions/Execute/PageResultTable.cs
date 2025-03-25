// Copyright ©2021-2025 Mike King.
// This file is licensed to you under the MIT license.
// See the License.txt file in the solution root for more information.

using System.Data;

namespace DotDoc.EntityFrameworkCore.Extensions.Execute;

/// <summary>
/// Paged Result with Data Table.
/// Returned by the ExecutePagedQueryxxxxx extensions.
/// </summary>
public sealed class PageResultTable : PageResultBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="PageResultTable"/> class.
    /// </summary>
    /// <param name="page">The page number.</param>
    /// <param name="pageSize">The page size.</param>
    /// <param name="recordCount">The number of records.</param>
    /// <param name="pageCount">The number of pages.</param>
    /// <param name="result">Result of the query.</param>
    internal PageResultTable(long page, long pageSize, long recordCount, long pageCount, DataTable result)
        : base(page, pageSize, recordCount, pageCount)
    {
        this.Result = result;
    }

    /// <summary>
    /// Gets the result of the query.
    /// </summary>
    public DataTable Result { get; }
}
