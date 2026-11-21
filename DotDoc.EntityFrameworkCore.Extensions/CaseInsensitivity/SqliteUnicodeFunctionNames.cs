// Copyright ©2021-2026 Mike King.
// This file is licensed to you under the MIT license.
// See the License.txt file in the solution root for more information.

namespace DotDoc.EntityFrameworkCore.Extensions.CaseInsensitivity;

/// <summary>
/// SQLite Unicode Function Names.
/// </summary>
/// <remarks>
/// In SQLite collation and function names are case insensitive but, the standard seems to be
/// collation names in upper case and function names in lower. I've kept to that.
/// </remarks>
internal static class SqliteUnicodeFunctionNames
{
    #region public constants

    /// <summary>
    /// No case function name prefix.
    /// </summary>
    public const string NoCasePrefix = "nocase_";

    /// <summary>
    /// The name of the nocase replacement collation.
    /// </summary>
    public const string NoCaseCollation = "NOCASE";

    /// <summary>
    /// The name of the lower replacement function.
    /// </summary>
    public const string LowerFunction = "lower";

    /// <summary>
    /// The name of the upper replacement function.
    /// </summary>
    public const string UpperFunction = "upper";

    /// <summary>
    /// The name of the nocase_endswith function.
    /// </summary>
    public const string NoCaseEndsWithFunction = $"{NoCasePrefix}endswith";

    /// <summary>
    /// The name of the nocase_instr function.
    /// </summary>
    public const string NoCaseInstrFunction = $"{NoCasePrefix}instr";

    /// <summary>
    /// The name of the nocase_like function.
    /// </summary>
    public const string NoCaseLikeFunction = $"{NoCasePrefix}like";

    /// <summary>
    /// The name of the nocase_ltrim function.
    /// </summary>
    public const string NoCaseLTrimFunction = $"{NoCasePrefix}ltrim";

    /// <summary>
    /// The name of the nocase_replace function.
    /// </summary>
    public const string NoCaseReplaceFunction = $"{NoCasePrefix}replace";

    /// <summary>
    /// The name of the nocase_rtrim function.
    /// </summary>
    public const string NoCaseRTrimFunction = $"{NoCasePrefix}rtrim";

    /// <summary>
    /// The name of the nocase_startswith function.
    /// </summary>
    public const string NoCaseStartsWithFunction = $"{NoCasePrefix}startswith";

    /// <summary>
    /// The name of the nocase_trim function.
    /// </summary>
    public const string NoCaseTrimFunction = $"{NoCasePrefix}trim";

    #endregion public constants
}
