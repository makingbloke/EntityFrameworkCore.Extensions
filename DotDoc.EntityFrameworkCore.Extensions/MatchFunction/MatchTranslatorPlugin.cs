// Copyright ©2021-2025 Mike King.
// This file is licensed to you under the MIT license.
// See the License.txt file in the solution root for more information.

using Microsoft.EntityFrameworkCore.Query;

namespace DotDoc.EntityFrameworkCore.Extensions.MatchFunction;

/// <summary>
/// Match Function Translator Plugin.
/// </summary>
public sealed class MatchTranslatorPlugin : IMethodCallTranslatorPlugin
{
    #region public properties

    /// <inheritdoc/>
    public IEnumerable<IMethodCallTranslator> Translators => [new MatchTranslator()];

    #endregion public properties
}
