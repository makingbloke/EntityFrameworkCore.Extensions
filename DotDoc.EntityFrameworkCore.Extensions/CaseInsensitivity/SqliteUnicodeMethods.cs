// Copyright ©2021-2026 Mike King.
// This file is licensed to you under the MIT license.
// See the License.txt file in the solution root for more information.

using System.Globalization;
using System.Text;

namespace DotDoc.EntityFrameworkCore.Extensions.CaseInsensitivity;

/// <summary>
/// SQLite Unicode Methods.
/// </summary>
/// <remarks>
/// These are Unicode compatible string methods for SQLite. <see cref="SqliteNoCaseReplacementInterceptor"/> registers them on each connection,
/// with the names in <see cref="SqliteUnicodeFunctionNames"/>. SQLite's NOCASE collation and its upper and lower functions are replaced with
/// <see cref="SqliteUnicodeMethods.Compare"/>, <see cref="SqliteUnicodeMethods.Lower"/> and <see cref="SqliteUnicodeMethods.Upper"/>.
/// The other methods are registered alongside SQLite's built-in functions, with their names prefixed with "nocase_".
/// Each method has the same interface as the SQLite function it's based on: the same arguments in the same order, the same handling of <see langword="null"/>
/// and empty arguments, the same results (positions are one-based and count code points) and an error where SQLite raises one. So a nocase_* function can be
/// used in place of the built-in in the SQL EF Core generates for SQLite. SQLite has no endswith or startswith, so nocase_endswith and nocase_startswith follow
/// the same conventions.
/// <list type="bullet">
/// <item><description>The way they process Unicode follows the SQL Server Latin1_General_100_CI_AS_SC_UTF8 collation.</description></item>
/// <item><description>They all use the invariant culture and are case insensitive and accent sensitive.</description></item>
/// <item><description>Ignorable characters (such as most control characters and soft hyphens) are ignored in comparisons.</description></item>
/// </list>
/// </remarks>
internal static class SqliteUnicodeMethods
{
    #region private constants

    /// <summary>
    /// Default compare options.
    /// </summary>
    private const CompareOptions CompareOptions = CompareOptions.IgnoreCase | CompareOptions.IgnoreKanaType | CompareOptions.IgnoreWidth;

    /// <summary>
    /// The LIKE wildcard that matches any sequence of zero or more characters.
    /// </summary>
    private const char LikeAnySequence = '%';

    /// <summary>
    /// The LIKE wildcard that matches exactly one character.
    /// </summary>
    private const char LikeAnyCharacter = '_';

    /// <summary>
    /// The maximum length of a LIKE pattern in UTF-8 bytes, SQLite's default SQLITE_LIMIT_LIKE_PATTERN_LENGTH (SQLITE_MAX_LIKE_PATTERN_LENGTH).
    /// </summary>
    private const int MaxLikePatternLength = 50_000;

    #endregion private constants

    #region private fields

    /// <summary>
    /// Default culture used in comparisons and case conversions.
    /// </summary>
    private static readonly CultureInfo Culture = CultureInfo.InvariantCulture;

    #endregion private fields

    #region public methods

    /// <summary>
    /// Compares two strings.
    /// </summary>
    /// <param name="value1">The first string to compare.</param>
    /// <param name="value2">The second string to compare.</param>
    /// <returns>An integer that indicates the lexical relationship between the two strings.</returns>
    /// <remarks>
    /// Uses the culture-aware compare on the whole strings, so letters are compared before accents (a natural sort order),
    /// precomposed and decomposed accents are equal and ignorable characters are skipped.
    /// Trailing spaces are ignored, as SQL Server does (so 'abc' equals 'abc ').
    /// </remarks>
    public static int Compare(string? value1, string? value2)
    {
        return value1 is null || value2 is null
            ? Culture.CompareInfo.Compare(value1, value2, CompareOptions)
            : Culture.CompareInfo.Compare(value1.AsSpan().TrimEnd(' '), value2.AsSpan().TrimEnd(' '), CompareOptions);
    }

    /// <summary>
    /// Converts a string to lower case.
    /// </summary>
    /// <param name="value">The string to convert.</param>
    /// <returns>The string in lower case, or <see langword="null"/> if <paramref name="value"/> is <see langword="null"/>.</returns>
    /// <remarks>Uses the invariant culture's simple case mapping (one character to one character), so the length never changes.</remarks>
    public static string? Lower(string? value)
    {
        return value is null
            ? null
            : Culture.TextInfo.ToLower(value);
    }

    /// <summary>
    /// Converts a string to upper case.
    /// </summary>
    /// <param name="value">The string to convert.</param>
    /// <returns>The string in upper case, or <see langword="null"/> if <paramref name="value"/> is <see langword="null"/>.</returns>
    /// <remarks>Uses the invariant culture's simple case mapping (one character to one character), so the length never changes.</remarks>
    public static string? Upper(string? value)
    {
        return value is null
            ? null
            : Culture.TextInfo.ToUpper(value);
    }

    /// <summary>
    /// Determines whether a string ends with another.
    /// </summary>
    /// <param name="value">The string to check.</param>
    /// <param name="suffix">The suffix to look for.</param>
    /// <returns>
    /// <see langword="true"/> if <paramref name="value"/> ends with <paramref name="suffix"/>, otherwise <see langword="false"/>. <see langword="null"/> if either argument is <see langword="null"/>.
    /// </returns>
    /// <remarks>Uses the culture-aware IsSuffix, so it matches as <see cref="Instr(string?, string?)"/> does.</remarks>
    public static bool? EndsWith(string? value, string? suffix)
    {
        return value is null || suffix is null
            ? null
            : Culture.CompareInfo.IsSuffix(value, suffix, CompareOptions);
    }

    /// <summary>
    /// Finds the first occurrence of a string in another.
    /// </summary>
    /// <param name="value">The string to search.</param>
    /// <param name="search">The string to find.</param>
    /// <returns>
    /// The one-based code point position of the first match (as SQLite's instr), 0 if there is no match or <see langword="null"/> if either argument is <see langword="null"/>.
    /// An empty <paramref name="search"/> matches at position 1.
    /// </returns>
    /// <remarks>
    /// Uses the culture-aware IndexOf on the original string, so it matches whole characters (é matches e + a combining accent, ignorable characters are skipped)
    /// and the position counts code points of the original string, as SQLite's substr expects.
    /// </remarks>
    public static int? Instr(string? value, string? search)
    {
        if (value is null || search is null)
        {
            return null;
        }

        int index = Culture.CompareInfo.IndexOf(value, search, CompareOptions);
        return index < 0
            ? 0
            : CountCodePoints(value.AsSpan(0, index)) + 1;
    }

    /// <summary>
    /// Determines whether a string matches a LIKE pattern.
    /// </summary>
    /// <param name="pattern">The pattern to match. <c>%</c> matches any sequence of zero or more characters, <c>_</c> matches exactly one character and any other character matches itself.</param>
    /// <param name="value">The string to check.</param>
    /// <returns>
    /// <see langword="true"/> if <paramref name="value"/> matches <paramref name="pattern"/>, otherwise <see langword="false"/>. <see langword="null"/> if either argument is <see langword="null"/>.
    /// </returns>
    /// <exception cref="ArgumentException">
    /// <paramref name="pattern"/> is longer than 50,000 UTF-8 bytes (as SQLite, even when <paramref name="value"/> is <see langword="null"/>).
    /// </exception>
    /// <remarks>
    /// As SQLite's like function, the pattern comes first (SQLite runs <c>value LIKE pattern</c> as <c>like(pattern, value)</c>),
    /// so <c>nocase_like(pattern, value)</c> is the case insensitive version of <c>value LIKE pattern</c>.
    /// Literal text is compared a run at a time with the culture-aware compare, so it matches as <see cref="Instr(string?, string?)"/> does (é matches e + a combining accent).
    /// <c>_</c> matches exactly one code point and <c>%</c> absorbs one code point at a time, as SQLite does.
    /// The maximum pattern length is SQLite's default limit (SQLITE_LIMIT_LIKE_PATTERN_LENGTH), so a different limit set on the connection isn't used.
    /// </remarks>
    public static bool? Like(string? pattern, string? value)
    {
        CheckLikePatternLength(pattern);

        return value is null || pattern is null
            ? null
            : IsLikeMatch(value, pattern, escape: null);
    }

    /// <summary>
    /// Determines whether a string matches a LIKE pattern with an escape character.
    /// </summary>
    /// <param name="pattern">The pattern to match, as <see cref="Like(string?, string?)"/>.</param>
    /// <param name="value">The string to check.</param>
    /// <param name="escape">The escape character. The character after it in <paramref name="pattern"/> is matched literally.</param>
    /// <returns>
    /// <see langword="true"/> if <paramref name="value"/> matches <paramref name="pattern"/>, otherwise <see langword="false"/>. <see langword="null"/> if any argument is <see langword="null"/>.
    /// </returns>
    /// <exception cref="ArgumentException">
    /// <paramref name="pattern"/> is longer than 50,000 UTF-8 bytes (as SQLite, even when <paramref name="value"/> or <paramref name="escape"/> is <see langword="null"/>),
    /// or <paramref name="escape"/> is not exactly one character (as SQLite, even when <paramref name="value"/> or <paramref name="pattern"/> is <see langword="null"/>).
    /// </exception>
    /// <remarks>
    /// As SQLite's like function, the pattern comes first (SQLite runs <c>value LIKE pattern ESCAPE escape</c> as <c>like(pattern, value, escape)</c>),
    /// so <c>nocase_like(pattern, value, escape)</c> is the case insensitive version of <c>value LIKE pattern ESCAPE escape</c>.
    /// Matches as <see cref="Like(string?, string?)"/> does.
    /// </remarks>
    public static bool? Like(string? pattern, string? value, string? escape)
    {
        CheckLikePatternLength(pattern);

        if (escape is null)
        {
            return null;
        }

        // The escape must be exactly one code point (SQLite raises an error otherwise, with this message).
        if (escape.Length == 0 || GetFirstCodePointLength(escape) != escape.Length)
        {
            throw new ArgumentException("ESCAPE expression must be a single character", nameof(escape));
        }

        return value is null || pattern is null
            ? null
            : IsLikeMatch(value, pattern, escape);
    }

    /// <summary>
    /// Removes a set of characters from the start of a string.
    /// </summary>
    /// <param name="value">The string to trim.</param>
    /// <param name="characters">The characters to remove.</param>
    /// <returns>The trimmed string, or <see langword="null"/> if either argument is <see langword="null"/>.</returns>
    /// <remarks>Works one code point at a time, as SQLite's ltrim does, comparing each with every character in the set using the culture-aware compare.</remarks>
    public static string? LTrim(string? value, string? characters)
    {
        return TrimCharacters(value, characters, trimStart: true, trimEnd: false);
    }

    /// <summary>
    /// Replaces every match of a string with another.
    /// </summary>
    /// <param name="value">The string to search.</param>
    /// <param name="search">The string to replace.</param>
    /// <param name="replacement">The replacement string.</param>
    /// <returns>
    /// <paramref name="value"/> with every match replaced. As SQLite's replace: <see langword="null"/> if <paramref name="value"/> or <paramref name="search"/> is <see langword="null"/>,
    /// <paramref name="value"/> unchanged if <paramref name="search"/> is empty (even when <paramref name="replacement"/> is <see langword="null"/>),
    /// otherwise <see langword="null"/> if <paramref name="replacement"/> is <see langword="null"/>.
    /// </returns>
    /// <remarks>Uses the culture-aware IndexOf on the original string, so it matches as <see cref="Instr(string?, string?)"/> does and leaves the rest of the string unchanged.</remarks>
    public static string? Replace(string? value, string? search, string? replacement)
    {
        if (value is null || search is null)
        {
            return null;
        }

        if (search.Length == 0)
        {
            return value;
        }

        if (replacement is null)
        {
            return null;
        }

        // Do the first match, so we can return the value as it is without building a new string.
        ReadOnlySpan<char> remaining = value;
        int index = Culture.CompareInfo.IndexOf(remaining, search, CompareOptions, out int matchLength);

        if (index < 0 || matchLength == 0)
        {
            return value;
        }

        StringBuilder result = new(value.Length);

        // A zero length match (a search string of ignorable characters only) would never move on, so stop there.
        while (index >= 0 && matchLength != 0)
        {
            result.Append(remaining[..index]).Append(replacement);
            remaining = remaining[(index + matchLength)..];
            index = Culture.CompareInfo.IndexOf(remaining, search, CompareOptions, out matchLength);
        }

        return result.Append(remaining).ToString();
    }

    /// <summary>
    /// Removes a set of characters from the end of a string.
    /// </summary>
    /// <param name="value">The string to trim.</param>
    /// <param name="characters">The characters to remove.</param>
    /// <returns>The trimmed string, or <see langword="null"/> if either argument is <see langword="null"/>.</returns>
    /// <remarks>Works one code point at a time, as SQLite's rtrim does, comparing each with every character in the set using the culture-aware compare.</remarks>
    public static string? RTrim(string? value, string? characters)
    {
        return TrimCharacters(value, characters, trimStart: false, trimEnd: true);
    }

    /// <summary>
    /// Determines whether a string starts with another.
    /// </summary>
    /// <param name="value">The string to check.</param>
    /// <param name="prefix">The prefix to look for.</param>
    /// <returns>
    /// <see langword="true"/> if <paramref name="value"/> starts with <paramref name="prefix"/>, otherwise <see langword="false"/>. <see langword="null"/> if either argument is <see langword="null"/>.
    /// </returns>
    /// <remarks>Uses the culture-aware IsPrefix, so it matches as <see cref="Instr(string?, string?)"/> does.</remarks>
    public static bool? StartsWith(string? value, string? prefix)
    {
        return value is null || prefix is null
            ? null
            : Culture.CompareInfo.IsPrefix(value, prefix, CompareOptions);
    }

    /// <summary>
    /// Removes a set of characters from both ends of a string.
    /// </summary>
    /// <param name="value">The string to trim.</param>
    /// <param name="characters">The characters to remove.</param>
    /// <returns>The trimmed string, or <see langword="null"/> if either argument is <see langword="null"/>.</returns>
    /// <remarks>Works one code point at a time, as SQLite's trim does, comparing each with every character in the set using the culture-aware compare.</remarks>
    public static string? Trim(string? value, string? characters)
    {
        return TrimCharacters(value, characters, trimStart: true, trimEnd: true);
    }

    #endregion public methods

    #region private methods

    /// <summary>
    /// Checks a LIKE pattern isn't longer than SQLite's limit.
    /// </summary>
    /// <param name="pattern">The pattern, or <see langword="null"/>.</param>
    /// <exception cref="ArgumentException"><paramref name="pattern"/> is longer than <see cref="MaxLikePatternLength"/> UTF-8 bytes.</exception>
    private static void CheckLikePatternLength(string? pattern)
    {
        // SQLite measures the pattern in UTF-8 bytes and raises an error with this message.
        if (pattern is not null && Encoding.UTF8.GetByteCount(pattern) > MaxLikePatternLength)
        {
            throw new ArgumentException("LIKE or GLOB pattern too complex", nameof(pattern));
        }
    }

    /// <summary>
    /// Determines whether a string matches a LIKE pattern.
    /// </summary>
    /// <param name="value">The string to check.</param>
    /// <param name="pattern">The pattern to match.</param>
    /// <param name="escape">The escape character (already checked to be one code point), or <see langword="null"/> for none.</param>
    /// <returns><see langword="true"/> if <paramref name="value"/> matches <paramref name="pattern"/>, otherwise <see langword="false"/>.</returns>
    private static bool IsLikeMatch(string value, string pattern, string? escape)
    {
        ReadOnlySpan<char> escapeChar = escape;
        ReadOnlySpan<char> valueSpan = value;
        ReadOnlySpan<char> patternSpan = pattern;

        // After a mismatch, retry from the pattern after the last % with that % absorbing one more character of the value.
        bool hasPercent = false;
        ReadOnlySpan<char> percentPattern = default;
        ReadOnlySpan<char> percentValue = default;

        while (true)
        {
            // Get the length of the next code point in the value (0 at the end) and check whether the pattern starts with the escape character.
            int valueCodePointLength = GetFirstCodePointLength(valueSpan);
            bool isEscaped = IsLikeEscape(patternSpan, escapeChar);

            if (patternSpan.IsEmpty)
            {
                // If the value is used up too then it's a match, otherwise a mismatch (handled below).
                if (valueCodePointLength == 0)
                {
                    return true;
                }
            }
            else if (!isEscaped && patternSpan[0] == LikeAnySequence)
            {
                // %: skip it and remember where we are, so a later mismatch can come back and let it absorb more of the value.
                patternSpan = patternSpan[1..];

                // A trailing % matches the rest of the value.
                if (patternSpan.IsEmpty)
                {
                    return true;
                }

                hasPercent = true;
                percentPattern = patternSpan;
                percentValue = valueSpan;
                continue;
            }
            else if (!isEscaped && patternSpan[0] == LikeAnyCharacter)
            {
                // _: matches any one character (code point).
                if (valueCodePointLength != 0)
                {
                    patternSpan = patternSpan[1..];
                    valueSpan = valueSpan[valueCodePointLength..];
                    continue;
                }
            }
            else
            {
                // Literal text up to the next wildcard. An escape at the end of the pattern never matches (as SQLite).
                if (!TryReadLikeLiteral(patternSpan, escapeChar, out ReadOnlySpan<char> literal, out int literalLength))
                {
                    return false;
                }

                // Compare all of it at once with the culture-aware compare, so it matches whole characters (é matches e + a combining accent).
                if (literalLength == patternSpan.Length)
                {
                    // Last in the pattern, so it must match all the rest of the value. When it comes straight after the last %
                    // (the pattern is where that % left it), it's tried at every position of the value down to the end.
                    bool isAfterPercent = hasPercent && patternSpan == percentPattern;

                    if (IsLikeEndMatch(valueSpan, literal, isAfterPercent))
                    {
                        return true;
                    }
                }
                else if (Culture.CompareInfo.IsPrefix(valueSpan, literal, CompareOptions, out int matchLength))
                {
                    // The match can be a different length from the literal (é against e + a combining accent).
                    patternSpan = patternSpan[literalLength..];
                    valueSpan = valueSpan[matchLength..];
                    continue;
                }
            }

            // Mismatch, with no earlier % (or one that has already absorbed all of the value) there's no other way to match.
            if (!hasPercent || percentValue.IsEmpty)
            {
                return false;
            }

            // Go back to the last %, let it absorb one more character of the value and try the rest of the pattern again.
            percentValue = percentValue[GetFirstCodePointLength(percentValue)..];
            valueSpan = percentValue;
            patternSpan = percentPattern;
        }
    }

    /// <summary>
    /// Determines whether the rest of a value matches the literal text at the end of a LIKE pattern.
    /// </summary>
    /// <param name="value">The rest of the value.</param>
    /// <param name="literal">The literal text.</param>
    /// <param name="isAfterPercent">Whether the literal comes straight after a <c>%</c>, so it's tried at every position of the value down to the end.</param>
    /// <returns><see langword="true"/> if all of <paramref name="value"/> matches <paramref name="literal"/>, otherwise <see langword="false"/>.</returns>
    private static bool IsLikeEndMatch(ReadOnlySpan<char> value, ReadOnlySpan<char> literal, bool isAfterPercent)
    {
        // Tried once, or at the end of the value: compare all of it.
        if (!isAfterPercent || value.IsEmpty)
        {
            return Culture.CompareInfo.Compare(value, literal, CompareOptions) == 0;
        }

        // Tried at every position after a %, so use IsPrefix, which fails fast (about 10 times quicker than comparing all the rest each time),
        // and check anything it leaves is ignorable. IsPrefix never matches a literal of ignorable characters only, but those are still
        // matched by the full compare at the end of the value. IsPrefix leaves trailing ignorable characters (such as a soft hyphen) out of matchLength,
        // so the rest only needs to compare equal to nothing, not be empty.
        return Culture.CompareInfo.IsPrefix(value, literal, CompareOptions, out int matchLength)
            && (matchLength == value.Length || Culture.CompareInfo.Compare(value[matchLength..], [], CompareOptions) == 0);
    }

    /// <summary>
    /// Determines whether a LIKE pattern starts with the escape character.
    /// </summary>
    /// <param name="pattern">The pattern.</param>
    /// <param name="escape">The escape character, or empty for none.</param>
    /// <returns><see langword="true"/> if <paramref name="pattern"/> starts with <paramref name="escape"/>, otherwise <see langword="false"/>.</returns>
    private static bool IsLikeEscape(ReadOnlySpan<char> pattern, ReadOnlySpan<char> escape)
    {
        return !escape.IsEmpty && pattern.StartsWith(escape);
    }

    /// <summary>
    /// Reads the literal text at the start of a LIKE pattern, up to the next wildcard (or the end of the pattern), removing escape characters.
    /// </summary>
    /// <param name="pattern">The pattern, which mustn't start with a wildcard.</param>
    /// <param name="escape">The escape character, or empty for none.</param>
    /// <param name="literal">The literal text.</param>
    /// <param name="literalLength">The number of UTF-16 units of <paramref name="pattern"/> read.</param>
    /// <returns><see langword="true"/> if the literal text was read, or <see langword="false"/> if it ends with the escape character.</returns>
    private static bool TryReadLikeLiteral(ReadOnlySpan<char> pattern, ReadOnlySpan<char> escape, out ReadOnlySpan<char> literal, out int literalLength)
    {
        // Only built when there's an escape character to remove, otherwise the literal is just part of the pattern.
        StringBuilder? unescaped = null;
        int index = 0;

        while (index < pattern.Length)
        {
            ReadOnlySpan<char> rest = pattern[index..];

            if (IsLikeEscape(rest, escape))
            {
                // Drop the escape character and take the character after it as it is.
                int escapedLength = GetFirstCodePointLength(rest[escape.Length..]);

                if (escapedLength == 0)
                {
                    literal = default;
                    literalLength = 0;
                    return false;
                }

                unescaped ??= new StringBuilder().Append(pattern[..index]);
                unescaped.Append(rest.Slice(escape.Length, escapedLength));
                index += escape.Length + escapedLength;
            }
            else if (rest[0] is LikeAnySequence or LikeAnyCharacter)
            {
                // An unescaped wildcard ends the literal text.
                break;
            }
            else
            {
                // One UTF-16 unit at a time is enough, as half a surrogate pair can't be a wildcard or the escape character.
                unescaped?.Append(rest[0]);
                index++;
            }
        }

        literal = unescaped is null
            ? pattern[..index]
            : unescaped.ToString();

        literalLength = index;

        return true;
    }

    /// <summary>
    /// Removes a set of characters from one or both ends of a string.
    /// </summary>
    /// <param name="value">The string to trim.</param>
    /// <param name="characters">The characters to remove.</param>
    /// <param name="trimStart">Whether to remove the characters from the start of <paramref name="value"/>.</param>
    /// <param name="trimEnd">Whether to remove the characters from the end of <paramref name="value"/>.</param>
    /// <returns>The trimmed string, or <see langword="null"/> if either argument is <see langword="null"/>.</returns>
    private static string? TrimCharacters(string? value, string? characters, bool trimStart, bool trimEnd)
    {
        if (value is null || characters is null)
        {
            return null;
        }

        ReadOnlySpan<char> trimmedValue = value;

        while (trimStart && !trimmedValue.IsEmpty)
        {
            // Never 0 as trimmedValue isn't empty (a 0 would stop the loop moving on, so keep the IsEmpty check).
            int codePointLength = GetFirstCodePointLength(trimmedValue);

            if (!IsInCharacterSet(trimmedValue[..codePointLength], characters))
            {
                break;
            }

            trimmedValue = trimmedValue[codePointLength..];
        }

        while (trimEnd && !trimmedValue.IsEmpty)
        {
            // Never 0 as trimmedValue isn't empty (a 0 would stop the loop moving on, so keep the IsEmpty check).
            int codePointLength = GetLastCodePointLength(trimmedValue);

            if (!IsInCharacterSet(trimmedValue[^codePointLength..], characters))
            {
                break;
            }

            trimmedValue = trimmedValue[..^codePointLength];
        }

        return trimmedValue.Length == value.Length
            ? value
            : trimmedValue.ToString();
    }

    /// <summary>
    /// Counts the number of code points in a string.
    /// </summary>
    /// <param name="value">The string.</param>
    /// <returns>The number of code points.</returns>
    private static int CountCodePoints(ReadOnlySpan<char> value)
    {
        int count = 0;
        foreach (Rune rune in value.EnumerateRunes())
        {
            count++;
        }

        return count;
    }

    /// <summary>
    /// Gets the number of UTF-16 units in the first code point of a string.
    /// </summary>
    /// <param name="value">The string.</param>
    /// <returns>0 if <paramref name="value"/> is empty, 2 for a surrogate pair, otherwise 1 (including for an unpaired surrogate).</returns>
    private static int GetFirstCodePointLength(ReadOnlySpan<char> value)
    {
        Rune.DecodeFromUtf16(value, out _, out int charsConsumed);
        return charsConsumed;
    }

    /// <summary>
    /// Gets the number of UTF-16 units in the last code point of a string.
    /// </summary>
    /// <param name="value">The string.</param>
    /// <returns>0 if <paramref name="value"/> is empty, 2 for a surrogate pair, otherwise 1 (including for an unpaired surrogate).</returns>
    private static int GetLastCodePointLength(ReadOnlySpan<char> value)
    {
        Rune.DecodeLastFromUtf16(value, out _, out int charsConsumed);
        return charsConsumed;
    }

    /// <summary>
    /// Determines whether a character is in a set of characters.
    /// </summary>
    /// <param name="character">The character (one code point).</param>
    /// <param name="characters">The set of characters.</param>
    /// <returns><see langword="true"/> if <paramref name="character"/> matches one of <paramref name="characters"/>, otherwise <see langword="false"/>.</returns>
    /// <remarks>
    /// Each character in the set is compared on its own. Searching the whole set with IndexOf instead would match an ignorable character
    /// (such as a soft hyphen or a variation selector) against any set, even an empty one, and could match across neighbouring characters
    /// in the set (e + a combining accent matches é).
    /// </remarks>
    private static bool IsInCharacterSet(ReadOnlySpan<char> character, string characters)
    {
        ReadOnlySpan<char> characterSpan = characters;

        while (!characterSpan.IsEmpty)
        {
            // Never 0 as characterSpan isn't empty (a 0 would stop the loop moving on, so keep the IsEmpty check).
            int codePointLength = GetFirstCodePointLength(characterSpan);

            if (CharactersEqual(character, characterSpan[..codePointLength]))
            {
                return true;
            }

            characterSpan = characterSpan[codePointLength..];
        }

        return false;
    }

    /// <summary>
    /// Determines whether two characters are equal using the default culture and compare options.
    /// </summary>
    /// <param name="character1">The first character (one code point).</param>
    /// <param name="character2">The second character (one code point).</param>
    /// <returns><see langword="true"/> if the characters are equal, otherwise <see langword="false"/>.</returns>
    /// <remarks>
    /// Identical characters and pairs of printable ASCII characters are compared without the much slower culture-aware compare.
    /// Every pair of printable ASCII characters gives the same result both ways. Control characters are left to the culture-aware compare,
    /// as it ignores most of them (so treats them as equal to each other).
    /// </remarks>
    private static bool CharactersEqual(ReadOnlySpan<char> character1, ReadOnlySpan<char> character2)
    {
        // Identical characters always match.
        if (character1.SequenceEqual(character2))
        {
            return true;
        }

        // Check printable ASCII characters match.
        if (character1.Length == 1 && char.IsBetween(character1[0], ' ', '~') && character2.Length == 1 && char.IsBetween(character2[0], ' ', '~'))
        {
            return Ascii.EqualsIgnoreCase(character1, character2);
        }

        return Culture.CompareInfo.Compare(character1, character2, CompareOptions) == 0;
    }

    #endregion private methods
}
