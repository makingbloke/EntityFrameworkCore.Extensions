// Copyright ©2021-2026 Mike King.
// This file is licensed to you under the MIT license.
// See the License.txt file in the solution root for more information.

namespace DotDoc.EntityFrameworkCore.Extensions.SqlParser;

/// <summary>
/// The type of a <see cref="SqlToken"/>.
/// </summary>
internal enum SqlTokenType
{
    /// <summary>
    /// A keyword, unquoted identifier or variable (e.g. SELECT, Name or @p0).
    /// </summary>
    Word,

    /// <summary>
    /// A quoted identifier (e.g. [Name], "Name" or `Name`).
    /// </summary>
    QuotedIdentifier,

    /// <summary>
    /// A string literal (e.g. 'text').
    /// </summary>
    String,

    /// <summary>
    /// A number.
    /// </summary>
    Number,

    /// <summary>
    /// An opening bracket.
    /// </summary>
    OpenBracket,

    /// <summary>
    /// A closing bracket.
    /// </summary>
    CloseBracket,

    /// <summary>
    /// A statement separator.
    /// </summary>
    Semicolon,

    /// <summary>
    /// Any other character (e.g. an operator, a comma or a parameter placeholder brace).
    /// </summary>
    Symbol
}
