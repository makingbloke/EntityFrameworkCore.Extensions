// Copyright ©2021-2025 Mike King.
// This file is licensed to you under the MIT license.
// See the License.txt file in the solution root for more information.

using DotDoc.EntityFrameworkCore.Extensions.ExecuteUpdate;
using DotDoc.EntityFrameworkCore.Extensions.TableHints;

namespace DotDoc.EntityFrameworkCore.Extensions.CustomQueryGenerators;

/// <summary>
/// Extra pieces of information which will be used by the custom query generators.
/// </summary>
internal static class CustomQueryGeneratorParameters
{
    #region internal fields

    /// <summary>
    /// Execute Update Query Parameters.
    /// </summary>
    internal static readonly AsyncLocal<ExecuteUpdateParameters> ExecuteUpdateParameters = new();

    /// <summary>
    /// Table hints.
    /// </summary>
    internal static readonly AsyncLocal<List<ITableHint>> TableHints = new();

    #endregion internal fields
}
