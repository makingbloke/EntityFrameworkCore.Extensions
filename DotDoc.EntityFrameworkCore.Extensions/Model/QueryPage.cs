// Copyright ©2021-2024 Mike King.
// This file is licensed to you under the MIT license.
// See the License.txt file in the solution root for more information.

using System.Data;

namespace DotDoc.EntityFrameworkCore.Extensions.Model;

/// <summary>
/// A page of query data. Returned by one of the ExecutePagedQueryxxxxx extensions.
/// </summary>
/// <param name="page">The page number.</param>
/// <param name="pageSize">The page size.</param>
/// <param name="recordCount">The number of records.</param>
/// <param name="pageCount">The number of pages.</param>
/// <param name="dataTable">A <see cref="DataTable"/> containing the page.</param>
public class QueryPage(long page, long pageSize, long recordCount, long pageCount, DataTable dataTable)
{
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

    /// <summary>
    /// Gets a <see cref="DataTable"/> containing the page.
    /// </summary>
    public DataTable DataTable { get; } = dataTable;
}