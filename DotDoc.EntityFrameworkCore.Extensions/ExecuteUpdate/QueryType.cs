// Copyright ©2021-2026 Mike King.
// This file is licensed to you under the MIT license.
// See the License.txt file in the solution root for more information.

namespace DotDoc.EntityFrameworkCore.Extensions.ExecuteUpdate;

/// <summary>
/// ExecuteUpdate Query Type.
/// </summary>
internal enum QueryType
{
    /// <summary>
    /// Unknown Query Type.
    /// </summary>
    Unknown,

    /// <summary>
    /// Delete Get Rows.
    /// </summary>
    DeleteGetRows,

    /// <summary>
    /// Insert.
    /// </summary>
    Insert,

    /// <summary>
    /// Insert Get Row.
    /// </summary>
    InsertGetRow,

    /// <summary>
    /// Update Get Rows.
    /// </summary>
    UpdateGetRows
}
