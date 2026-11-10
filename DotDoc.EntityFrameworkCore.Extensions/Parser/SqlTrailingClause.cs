// Copyright ©2021-2026 Mike King.
// This file is licensed to you under the MIT license.
// See the License.txt file in the solution root for more information.

namespace DotDoc.EntityFrameworkCore.Extensions.Parser;

/// <summary>
/// The type of clause a top level keyword starts at the end of a query (after the main query and any ORDER BY).
/// </summary>
internal enum SqlTrailingClause
{
    /// <summary>
    /// Not a trailing clause.
    /// </summary>
    None,

    /// <summary>
    /// A paging clause (e.g. LIMIT, OFFSET or FETCH) - the query is already paged.
    /// </summary>
    Paging,

    /// <summary>
    /// A clause that must stay at the end of the query (e.g. SQL Server OPTION (...) or PostgreSQL FOR UPDATE).
    /// </summary>
    Hint,

    /// <summary>
    /// A clause that can't be paged (e.g. SQL Server FOR XML / FOR JSON).
    /// </summary>
    Unsupported
}
