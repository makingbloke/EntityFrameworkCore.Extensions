// Copyright ©2021-2026 Mike King.
// This file is licensed to you under the MIT license.
// See the License.txt file in the solution root for more information.

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace DotDoc.EntityFrameworkCore.Extensions.FreeTextSearchFunction;

/// <summary>
/// FreeTextSearch Function DbFunctions Extension Methods.
/// </summary>
public static class FreeTextSearchDbFunctionsExtensions
{
    #region internal fields

    /// <summary>
    /// The <see cref="MethodInfo"/> for the extension Methods.
    /// </summary>
    internal static readonly MethodInfo[] FreeTextSearchMethods =
    [
        typeof(FreeTextSearchDbFunctionsExtensions)
            .GetMethod(nameof(FreeTextSearch), [typeof(DbFunctions), typeof(object), typeof(string)])!,

        typeof(FreeTextSearchDbFunctionsExtensions)
            .GetMethod(nameof(FreeTextSearch), [typeof(DbFunctions), typeof(object), typeof(string), typeof(bool)])!,

        typeof(FreeTextSearchDbFunctionsExtensions)
            .GetMethod(nameof(FreeTextSearch), [typeof(DbFunctions), typeof(object), typeof(string), typeof(int)])!,

        typeof(FreeTextSearchDbFunctionsExtensions)
            .GetMethod(nameof(FreeTextSearch), [typeof(DbFunctions), typeof(object), typeof(string), typeof(bool), typeof(int)])!
    ];

    #endregion internal fields

    #region public methods

    /// <summary>
    /// Perform a free text search.
    /// </summary>
    /// <param name="dbFunctions">The <see cref="DbFunctions"/> instance.</param>
    /// <param name="propertyReference">The property on which the search will be performed.</param>
    /// <param name="freeText">The text that will be searched for in the property.</param>
    /// <returns>A <see cref="bool"/> value indicating whether the text exists in the property.</returns>
    public static bool FreeTextSearch(
        this DbFunctions dbFunctions,
        object propertyReference,
        string freeText) =>
            throw new InvalidOperationException(GetExceptionMessage());

    /// <summary>
    /// Perform a free text search.
    /// </summary>
    /// <param name="dbFunctions">The <see cref="DbFunctions"/> instance.</param>
    /// <param name="propertyReference">The property on which the search will be performed.</param>
    /// <param name="freeText">The text that will be searched for in the property.</param>
    /// <param name="useStemming">A value indicating whether stemming will be used when searching.</param>
    /// <returns>A <see cref="bool"/> value indicating whether the text exists in the property.</returns>
    public static bool FreeTextSearch(
        this DbFunctions dbFunctions,
        object propertyReference,
        string freeText,
        [NotParameterized] bool useStemming) =>
            throw new InvalidOperationException(GetExceptionMessage());

    /// <summary>
    /// Perform a free text search.
    /// </summary>
    /// <param name="dbFunctions">The <see cref="DbFunctions"/> instance.</param>
    /// <param name="propertyReference">The property on which the search will be performed.</param>
    /// <param name="freeText">The text that will be searched for in the property.</param>
    /// <param name="languageTerm">A Language ID from the sys.syslanguages table (SQL Server only).</param>
    /// <returns>A <see cref="bool"/> value indicating whether the text exists in the property.</returns>
    public static bool FreeTextSearch(
        this DbFunctions dbFunctions,
        object propertyReference,
        string freeText,
        [NotParameterized] int languageTerm) =>
            throw new InvalidOperationException(GetExceptionMessage());

    /// <summary>
    /// Perform a free text search.
    /// </summary>
    /// <param name="dbFunctions">The <see cref="DbFunctions"/> instance.</param>
    /// <param name="propertyReference">The property on which the search will be performed.</param>
    /// <param name="freeText">The text that will be searched for in the property.</param>
    /// <param name="useStemming">A value indicating whether stemming will be used when searching.</param>
    /// <param name="languageTerm">A Language ID from the sys.syslanguages table (SQL Server only).</param>
    /// <returns>A <see cref="bool"/> value indicating whether the text exists in the property.</returns>
    public static bool FreeTextSearch(
        this DbFunctions dbFunctions,
        object propertyReference,
        string freeText,
        [NotParameterized] bool useStemming,
        [NotParameterized] int languageTerm) =>
            throw new InvalidOperationException(GetExceptionMessage());

    #endregion public methods

    #region private methods

    /// <summary>
    /// Get the method is not supported because the query has switched to client-evaluation message.
    /// </summary>
    /// <param name="methodName">The method name (auto populated).</param>
    /// <returns>The exception message.</returns>
    private static string GetExceptionMessage([CallerMemberName]string methodName = "") =>
        $"The '{methodName}' method is not supported because the query has switched to client-evaluation. This usually happens when the arguments to the method cannot be translated to server. Rewrite the query to avoid client evaluation of arguments so that method can be translated to server.";

    #endregion private methods
}
