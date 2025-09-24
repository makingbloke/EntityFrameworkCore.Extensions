// Copyright ©2021-2025 Mike King.
// This file is licensed to you under the MIT license.
// See the License.txt file in the solution root for more information.

namespace DotDoc.EntityFrameworkCore.Extensions.TableHints;

/// <summary>
/// Table Hint Interface.
/// </summary>
public interface ITableHint
{
    #region public methods

    /// <summary>
    /// Convert a table hint to a string.
    /// </summary>
    /// <returns>The hint string.</returns>
    public string ToString();

    #endregion public methods
}
