// Copyright ©2021-2025 Mike King.
// This file is licensed to you under the MIT license.
// See the License.txt file in the solution root for more information.

namespace DotDoc.EntityFrameworkCore.Extensions.Constants;

/// <summary>
/// Tag Names.
/// </summary>
internal static class TagNames
{
    #region public constants

    /// <summary>
    /// Tag name prefix.
    /// </summary>
    public const string TagNamePrefix = "DotDoc.";

    /// <summary>
    /// Name of the tag used to store the table hints.
    /// </summary>
    public const string TableHint = $"{TagNamePrefix}TableHints";

    #endregion public constants
}
