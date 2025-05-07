// Copyright ©2021-2025 Mike King.
// This file is licensed to you under the MIT license.
// See the License.txt file in the solution root for more information.

using Microsoft.EntityFrameworkCore;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace DotDoc.EntityFrameworkCore.Extensions.MatchFunction;

/// <summary>
/// Match Function DbFunctions Extension Methods.
/// </summary>
public static class MatchDbFunctionsExtensions
{
    #region internal fields

    /// <summary>
    /// The <see cref="MethodInfo"/> values for the Match method.
    /// </summary>
    internal static readonly MethodInfo MatchMethod = typeof(MatchDbFunctionsExtensions)
        .GetMethod(nameof(Match), [typeof(DbFunctions), typeof(string), typeof(object)])!;

    #endregion internal fields

    #region public methods

    /// <summary>
    /// Perform a SQLite free text match.
    /// </summary>
    /// <param name="dbFunctions">The <see cref="DbFunctions"/> instance.</param>
    /// <param name="freeText">The text that will be searched for in the property.</param>
    /// <param name="propertyReference">The property on which the search will be performed.</param>
    /// <returns>A <see cref="bool"/> value indicating whether the text exists in the property.</returns>
    public static bool Match(
        this DbFunctions dbFunctions,
        string freeText,
        object propertyReference) =>
            throw new InvalidOperationException(GetExceptionMessage());

    #endregion public methods

    #region private methods

    /// <summary>
    /// Get the method is not supported because the query has switched to client-evaluation message.
    /// </summary>
    /// <param name="methodName">The method name (auto populated).</param>
    /// <returns>The exception message.</returns>
    private static string GetExceptionMessage([CallerMemberName] string methodName = "") =>
        $"The '{methodName}' method is not supported because the query has switched to client-evaluation. This usually happens when the arguments to the method cannot be translated to server. Rewrite the query to avoid client evaluation of arguments so that method can be translated to server.";

    #endregion private methods
}
