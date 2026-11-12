// Copyright ©2021-2026 Mike King.
// This file is licensed to you under the MIT license.
// See the License.txt file in the solution root for more information.

namespace DotDoc.EntityFrameworkCore.Extensions.SqlParser;

/// <summary>
/// A token in a SQL statement. Whitespace and comments are not tokens.
/// </summary>
/// <param name="Type">The token type.</param>
/// <param name="Text">The token text, exactly as it appears in the SQL.</param>
/// <param name="Start">The position of the token in the SQL.</param>
/// <param name="Depth">The bracket depth of the token (0 = top level). Brackets have the depth of the text outside them.</param>
internal readonly record struct SqlToken(SqlTokenType Type, string Text, int Start, int Depth)
{
    #region public properties

    /// <summary>
    /// Gets the position in the SQL immediately after the token.
    /// </summary>
    public int End => this.Start + this.Text.Length;

    #endregion public properties

    #region public methods

    /// <summary>
    /// Check if the token is an unquoted word (e.g. a keyword) matching <paramref name="word"/>, ignoring case.
    /// </summary>
    /// <param name="word">The word to compare with.</param>
    /// <returns><see langword="true"/> if the token matches, otherwise <see langword="false"/>.</returns>
    public bool IsWord(string word)
    {
        return this.Type == SqlTokenType.Word && string.Equals(this.Text, word, StringComparison.OrdinalIgnoreCase);
    }

    #endregion public methods
}
