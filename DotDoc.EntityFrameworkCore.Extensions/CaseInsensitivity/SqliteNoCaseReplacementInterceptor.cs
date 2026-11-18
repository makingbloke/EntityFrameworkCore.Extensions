// Copyright ©2021-2026 Mike King.
// This file is licensed to you under the MIT license.
// See the License.txt file in the solution root for more information.

using DotDoc.EntityFrameworkCore.Extensions.DatabaseType;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore.Diagnostics;
using System.Data.Common;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Text;

namespace DotDoc.EntityFrameworkCore.Extensions.CaseInsensitivity;

/// <summary>
/// SQLite NoCase Replacement Interceptor.
/// </summary>
internal sealed class SqliteNoCaseReplacementInterceptor : DbConnectionInterceptor
{
    #region internal constants

    /// <summary>
    /// The name of SQLite's built-in <c>NOCASE</c> collation, which is replaced with a Unicode version.
    /// </summary>
    /// <remarks>
    /// This must be the built-in name, not a new one: registering a collation with the same name replaces the built-in one on
    /// the connection, so every column and expression that already uses NOCASE gets the replacement. Collation names are upper
    /// case by convention in SQLite (BINARY, NOCASE, RTRIM), although SQLite ignores their case.
    /// </remarks>
    internal const string NoCaseCollationName = "NOCASE";

    /// <summary>
    /// The name of SQLite's built-in <c>lower</c> function, which is replaced with a Unicode version.
    /// </summary>
    /// <remarks>
    /// This must be the built-in name, not a nocase_* one: registering a function with the same name and number of arguments
    /// replaces the built-in one on the connection, so <c>ToLower()</c> (which EF Core translates to <c>lower</c>) and raw SQL
    /// get the replacement with no translator. It doesn't depend on the collation, so it applies to every column.
    /// Function names are lower case by convention in SQLite, although SQLite ignores their case.
    /// </remarks>
    internal const string LowerFunctionName = "lower";

    /// <summary>
    /// The name of SQLite's built-in <c>upper</c> function, which is replaced with a Unicode version.
    /// </summary>
    /// <remarks>
    /// This must be the built-in name, not a nocase_* one: registering a function with the same name and number of arguments
    /// replaces the built-in one on the connection, so <c>ToUpper()</c> (which EF Core translates to <c>upper</c>) and raw SQL
    /// get the replacement with no translator. It doesn't depend on the collation, so it applies to every column.
    /// Function names are lower case by convention in SQLite, although SQLite ignores their case.
    /// </remarks>
    internal const string UpperFunctionName = "upper";

    /// <summary>
    /// The name of the SQLite function <c>nocase_instr(value, search)</c>, a case insensitive version of <c>instr</c>.
    /// </summary>
    internal const string NoCaseInstrFunctionName = "nocase_instr";

    /// <summary>
    /// The name of the SQLite function <c>nocase_like(pattern, value)</c> / <c>nocase_like(pattern, value, escape)</c>,
    /// a case insensitive version of the <c>like</c> function (note the pattern comes first, as it does in <c>like</c>).
    /// </summary>
    internal const string NoCaseLikeFunctionName = "nocase_like";

    /// <summary>
    /// The name of the SQLite function <c>nocase_replace(value, search, replacement)</c>, a case insensitive version of <c>replace</c>.
    /// </summary>
    internal const string NoCaseReplaceFunctionName = "nocase_replace";

    /// <summary>
    /// The name of the SQLite function <c>nocase_startswith(value, prefix)</c>, which returns 1 if the value starts with the prefix, ignoring case.
    /// </summary>
    internal const string NoCaseStartsWithFunctionName = "nocase_startswith";

    /// <summary>
    /// The name of the SQLite function <c>nocase_endswith(value, suffix)</c>, which returns 1 if the value ends with the suffix, ignoring case.
    /// </summary>
    internal const string NoCaseEndsWithFunctionName = "nocase_endswith";

    /// <summary>
    /// The name of the SQLite function <c>nocase_trim(value, characters)</c>, a case insensitive version of <c>trim(X, Y)</c>.
    /// </summary>
    internal const string NoCaseTrimFunctionName = "nocase_trim";

    /// <summary>
    /// The name of the SQLite function <c>nocase_ltrim(value, characters)</c>, a case insensitive version of <c>ltrim(X, Y)</c>.
    /// </summary>
    internal const string NoCaseLTrimFunctionName = "nocase_ltrim";

    /// <summary>
    /// The name of the SQLite function <c>nocase_rtrim(value, characters)</c>, a case insensitive version of <c>rtrim(X, Y)</c>.
    /// </summary>
    internal const string NoCaseRTrimFunctionName = "nocase_rtrim";

    /// <summary>
    /// The options used by every case insensitive comparison: case insensitive but accent sensitive,
    /// matching the SQL Server Latin1_General_100_CI_AS_SC_UTF8 collation.
    /// </summary>
    internal const CompareOptions NoCaseCompareOptions = CompareOptions.IgnoreCase | CompareOptions.IgnoreKanaType | CompareOptions.IgnoreWidth;

    /// <summary>
    /// The LIKE wildcard that matches any sequence of zero or more characters.
    /// </summary>
    internal const char LikeAnySequence = '%';

    /// <summary>
    /// The LIKE wildcard that matches exactly one character.
    /// </summary>
    internal const char LikeAnyCharacter = '_';

    #endregion internal constants

    #region internal fields

    /// <summary>
    /// Singleton instance of this interceptor.
    /// </summary>
    internal static readonly SqliteNoCaseReplacementInterceptor Instance = new();

    /// <summary>
    /// The compare information used by every case insensitive comparison (the invariant culture's).
    /// </summary>
    internal static readonly CompareInfo NoCaseCompareInfo = CultureInfo.InvariantCulture.CompareInfo;

    #endregion internal fields

    #region private constructors

    /// <summary>
    /// Initializes a new instance of the <see cref="SqliteNoCaseReplacementInterceptor"/> class.
    /// </summary>
    private SqliteNoCaseReplacementInterceptor()
    {
    }

    #endregion private constructors

    #region public methods

    /// <inheritdoc/>
    /// <exception cref="UnsupportedDatabaseTypeException"><paramref name="connection"/> is not a SQLite connection.</exception>
    public override void ConnectionOpened(DbConnection connection, ConnectionEndEventData eventData)
    {
        RegisterNoCaseReplacements(connection);
        base.ConnectionOpened(connection, eventData);
    }

    /// <inheritdoc/>
    /// <exception cref="UnsupportedDatabaseTypeException"><paramref name="connection"/> is not a SQLite connection.</exception>
    public override Task ConnectionOpenedAsync(DbConnection connection, ConnectionEndEventData eventData, CancellationToken cancellationToken = default)
    {
        RegisterNoCaseReplacements(connection);
        return base.ConnectionOpenedAsync(connection, eventData, cancellationToken);
    }

    #endregion public methods

    #region private methods

    /// <summary>
    /// Replace the SQLite NoCase collation and the lower and upper functions with custom ones which can handle Unicode characters
    /// (the originals are ASCII only), and add the nocase_* functions: case insensitive versions of instr, like, replace, trim,
    /// ltrim and rtrim, plus nocase_startswith and nocase_endswith.
    /// They are case insensitive but accent sensitive, matching the SQL Server Latin1_General_100_CI_AS_SC_UTF8 collation.
    /// </summary>
    /// <param name="connection">The connection.</param>
    /// <exception cref="UnsupportedDatabaseTypeException"><paramref name="connection"/> is not a SQLite connection.</exception>
    private static void RegisterNoCaseReplacements(DbConnection connection)
    {
        switch (connection)
        {
            case SqliteConnection sqliteConnection:
                sqliteConnection.CreateCollation(NoCaseCollationName, UnicodeNoCaseCompare);
                sqliteConnection.CreateFunction<string?, string?>(LowerFunctionName, UnicodeLower, isDeterministic: true);
                sqliteConnection.CreateFunction<string?, string?>(UpperFunctionName, UnicodeUpper, isDeterministic: true);
                sqliteConnection.CreateFunction<string?, string?, int?>(NoCaseInstrFunctionName, UnicodeNoCaseInstr, isDeterministic: true);
                sqliteConnection.CreateFunction<string?, string?, bool?>(NoCaseLikeFunctionName, UnicodeNoCaseLike, isDeterministic: true);
                sqliteConnection.CreateFunction<string?, string?, string?, bool?>(NoCaseLikeFunctionName, UnicodeNoCaseLike, isDeterministic: true);
                sqliteConnection.CreateFunction<string?, string?, string?, string?>(NoCaseReplaceFunctionName, UnicodeNoCaseReplace, isDeterministic: true);
                sqliteConnection.CreateFunction<string?, string?, bool?>(NoCaseStartsWithFunctionName, UnicodeNoCaseStartsWith, isDeterministic: true);
                sqliteConnection.CreateFunction<string?, string?, bool?>(NoCaseEndsWithFunctionName, UnicodeNoCaseEndsWith, isDeterministic: true);
                sqliteConnection.CreateFunction<string?, string?, string?>(NoCaseTrimFunctionName, UnicodeNoCaseTrim, isDeterministic: true);
                sqliteConnection.CreateFunction<string?, string?, string?>(NoCaseLTrimFunctionName, UnicodeNoCaseLTrim, isDeterministic: true);
                sqliteConnection.CreateFunction<string?, string?, string?>(NoCaseRTrimFunctionName, UnicodeNoCaseRTrim, isDeterministic: true);

                break;

            default:
                throw new UnsupportedDatabaseTypeException(nameof(connection));
        }
    }

    /// <summary>
    /// Compares two specified strings in a case insensitive, accent sensitive manner.
    /// </summary>
    /// <param name="str1">The first string to compare.</param>
    /// <param name="str2">The second string to compare.</param>
    /// <returns>An integer that indicates the lexical relationship between the two comparands.</returns>
    private static int UnicodeNoCaseCompare(string? str1, string? str2)
    {
        int result = NoCaseCompareInfo.Compare(str1, str2, NoCaseCompareOptions);
        return result;
    }

    /// <summary>
    /// Converts a string to lower case using the invariant culture.
    /// </summary>
    /// <param name="str">The string to convert.</param>
    /// <returns>The string in lower case, or <see langword="null"/> if <paramref name="str"/> is <see langword="null"/>.</returns>
    private static string? UnicodeLower(string? str)
    {
        return str?.ToLowerInvariant();
    }

    /// <summary>
    /// Converts a string to upper case using the invariant culture.
    /// </summary>
    /// <param name="str">The string to convert.</param>
    /// <returns>The string in upper case, or <see langword="null"/> if <paramref name="str"/> is <see langword="null"/>.</returns>
    private static string? UnicodeUpper(string? str)
    {
        return str?.ToUpperInvariant();
    }

    /// <summary>
    /// Finds the first match of a string in another, ignoring case (<c>nocase_instr</c>, a case insensitive version of <c>instr</c>).
    /// </summary>
    /// <param name="value">The string to search.</param>
    /// <param name="search">The string to find.</param>
    /// <returns>
    /// The position of the first match, counted in characters (code points) from 1 as <c>instr</c> does, or 0 if there is no match.
    /// An empty <paramref name="search"/> matches at position 1. <see langword="null"/> if either argument is <see langword="null"/>.
    /// </returns>
    private static int? UnicodeNoCaseInstr(string? value, string? search)
    {
        if (value is null || search is null)
        {
            return null;
        }

        int index = NoCaseCompareInfo.IndexOf(value, search, NoCaseCompareOptions);
        return index < 0 ? 0 : CountCodePoints(value.AsSpan(0, index)) + 1;
    }

    /// <summary>
    /// Matches a value against a LIKE pattern with no escape character, ignoring case
    /// (<c>nocase_like(pattern, value)</c>, a case insensitive version of <c>like(pattern, value)</c>).
    /// </summary>
    /// <param name="pattern">The pattern, where <c>%</c> matches any sequence of characters and <c>_</c> matches one character.</param>
    /// <param name="value">The value to match.</param>
    /// <returns>
    /// <see langword="true"/> if <paramref name="value"/> matches <paramref name="pattern"/>, otherwise <see langword="false"/>.
    /// <see langword="null"/> if either argument is <see langword="null"/>.
    /// </returns>
    private static bool? UnicodeNoCaseLike(string? pattern, string? value)
    {
        return pattern is null || value is null ? null : IsLikeMatch(pattern, value, null);
    }

    /// <summary>
    /// Matches a value against a LIKE pattern with an escape character, ignoring case
    /// (<c>nocase_like(pattern, value, escape)</c>, a case insensitive version of <c>like(pattern, value, escape)</c>).
    /// </summary>
    /// <param name="pattern">The pattern, where <c>%</c> matches any sequence of characters and <c>_</c> matches one character.</param>
    /// <param name="value">The value to match.</param>
    /// <param name="escape">The escape character, which makes the character after it match itself.</param>
    /// <returns>
    /// <see langword="true"/> if <paramref name="value"/> matches <paramref name="pattern"/>, otherwise <see langword="false"/>.
    /// <see langword="null"/> if any argument is <see langword="null"/>.
    /// </returns>
    /// <exception cref="ArgumentException"><paramref name="escape"/> is not a single character (as SQLite's <c>like</c>, even when another argument is <see langword="null"/>).</exception>
    private static bool? UnicodeNoCaseLike(string? pattern, string? value, string? escape)
    {
        if (escape is null)
        {
            return null;
        }

        if (CountCodePoints(escape) != 1)
        {
            throw new ArgumentException("ESCAPE expression must be a single character", nameof(escape));
        }

        return pattern is null || value is null ? null : IsLikeMatch(pattern, value, escape);
    }

    /// <summary>
    /// Replaces every match of a string in another, ignoring case
    /// (<c>nocase_replace(value, search, replacement)</c>, a case insensitive version of <c>replace</c>).
    /// </summary>
    /// <param name="value">The string to search.</param>
    /// <param name="search">The string to replace.</param>
    /// <param name="replacement">The replacement string.</param>
    /// <returns>
    /// <paramref name="value"/> with every match replaced. As <c>replace</c>: <see langword="null"/> if <paramref name="value"/> or
    /// <paramref name="search"/> is <see langword="null"/>, <paramref name="value"/> unchanged if <paramref name="search"/> is empty
    /// (even when <paramref name="replacement"/> is <see langword="null"/>), otherwise <see langword="null"/> if <paramref name="replacement"/> is <see langword="null"/>.
    /// </returns>
    /// <remarks>
    /// A match can be a different length from <paramref name="search"/>, so the length of each match is taken from the comparison.
    /// </remarks>
    private static string? UnicodeNoCaseReplace(string? value, string? search, string? replacement)
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

        StringBuilder result = new(value.Length);
        ReadOnlySpan<char> remaining = value;

        while (true)
        {
            int index = NoCaseCompareInfo.IndexOf(remaining, search, NoCaseCompareOptions, out int matchLength);

            // A zero length match (a search string of ignorable characters only) would never move on, so stop there.
            if (index < 0 || matchLength == 0)
            {
                break;
            }

            result.Append(remaining[..index]).Append(replacement);
            remaining = remaining[(index + matchLength)..];
        }

        return result.Append(remaining).ToString();
    }

    /// <summary>
    /// Determines whether a string starts with another, ignoring case (<c>nocase_startswith(value, prefix)</c>).
    /// </summary>
    /// <param name="value">The string to check.</param>
    /// <param name="prefix">The prefix.</param>
    /// <returns>
    /// <see langword="true"/> if <paramref name="value"/> starts with <paramref name="prefix"/> (always if <paramref name="prefix"/> is empty),
    /// otherwise <see langword="false"/>. <see langword="null"/> if either argument is <see langword="null"/>.
    /// </returns>
    private static bool? UnicodeNoCaseStartsWith(string? value, string? prefix)
    {
        return value is null || prefix is null ? null : NoCaseCompareInfo.IsPrefix(value, prefix, NoCaseCompareOptions);
    }

    /// <summary>
    /// Determines whether a string ends with another, ignoring case (<c>nocase_endswith(value, suffix)</c>).
    /// </summary>
    /// <param name="value">The string to check.</param>
    /// <param name="suffix">The suffix.</param>
    /// <returns>
    /// <see langword="true"/> if <paramref name="value"/> ends with <paramref name="suffix"/> (always if <paramref name="suffix"/> is empty),
    /// otherwise <see langword="false"/>. <see langword="null"/> if either argument is <see langword="null"/>.
    /// </returns>
    private static bool? UnicodeNoCaseEndsWith(string? value, string? suffix)
    {
        return value is null || suffix is null ? null : NoCaseCompareInfo.IsSuffix(value, suffix, NoCaseCompareOptions);
    }

    /// <summary>
    /// Removes a set of characters from both ends of a string, ignoring case
    /// (<c>nocase_trim(value, characters)</c>, a case insensitive version of <c>trim(X, Y)</c>).
    /// </summary>
    /// <param name="value">The string to trim.</param>
    /// <param name="characters">The characters to remove.</param>
    /// <returns>The trimmed string, or <see langword="null"/> if either argument is <see langword="null"/>.</returns>
    private static string? UnicodeNoCaseTrim(string? value, string? characters)
    {
        return TrimCharacters(value, characters, trimStart: true, trimEnd: true);
    }

    /// <summary>
    /// Removes a set of characters from the start of a string, ignoring case
    /// (<c>nocase_ltrim(value, characters)</c>, a case insensitive version of <c>ltrim(X, Y)</c>).
    /// </summary>
    /// <param name="value">The string to trim.</param>
    /// <param name="characters">The characters to remove.</param>
    /// <returns>The trimmed string, or <see langword="null"/> if either argument is <see langword="null"/>.</returns>
    private static string? UnicodeNoCaseLTrim(string? value, string? characters)
    {
        return TrimCharacters(value, characters, trimStart: true, trimEnd: false);
    }

    /// <summary>
    /// Removes a set of characters from the end of a string, ignoring case
    /// (<c>nocase_rtrim(value, characters)</c>, a case insensitive version of <c>rtrim(X, Y)</c>).
    /// </summary>
    /// <param name="value">The string to trim.</param>
    /// <param name="characters">The characters to remove.</param>
    /// <returns>The trimmed string, or <see langword="null"/> if either argument is <see langword="null"/>.</returns>
    private static string? UnicodeNoCaseRTrim(string? value, string? characters)
    {
        return TrimCharacters(value, characters, trimStart: false, trimEnd: true);
    }

    /// <summary>
    /// Determines whether a value matches a LIKE pattern, ignoring case.
    /// </summary>
    /// <param name="pattern">The pattern.</param>
    /// <param name="value">The value to match.</param>
    /// <param name="escape">The escape character (one code point), or <see langword="null"/> if there isn't one.</param>
    /// <returns><see langword="true"/> if <paramref name="value"/> matches <paramref name="pattern"/>, otherwise <see langword="false"/>.</returns>
    private static bool IsLikeMatch(string pattern, string value, string? escape)
    {
        return IsLikeMatch(pattern, 0, value, 0, escape, []);
    }

    /// <summary>
    /// Determines whether the rest of a value matches the rest of a LIKE pattern, ignoring case.
    /// </summary>
    /// <param name="pattern">The pattern.</param>
    /// <param name="patternIndex">The index in <paramref name="pattern"/> to match from.</param>
    /// <param name="value">The value to match.</param>
    /// <param name="valueIndex">The index in <paramref name="value"/> to match from.</param>
    /// <param name="escape">The escape character (one code point), or <see langword="null"/> if there isn't one.</param>
    /// <param name="failures">
    /// The pattern and value indexes already known not to match, so a pattern with several <c>%</c> wildcards
    /// doesn't take exponential time.
    /// </param>
    /// <returns><see langword="true"/> if the rest of <paramref name="value"/> matches the rest of <paramref name="pattern"/>, otherwise <see langword="false"/>.</returns>
    /// <exception cref="InsufficientExecutionStackException">The pattern is too complex to match.</exception>
    private static bool IsLikeMatch(string pattern, int patternIndex, string value, int valueIndex, string? escape, HashSet<(int PatternIndex, int ValueIndex)> failures)
    {
        if (patternIndex == pattern.Length)
        {
            return valueIndex == value.Length;
        }

        if (failures.Contains((patternIndex, valueIndex)))
        {
            return false;
        }

        // Each wildcard or literal recurses, so a long pattern could otherwise overflow the stack and end the process.
        RuntimeHelpers.EnsureSufficientExecutionStack();

        bool isMatch;

        if (IsLikeWildcard(pattern, patternIndex, LikeAnySequence, escape))
        {
            isMatch = IsLikeAnySequenceMatch(pattern, patternIndex, value, valueIndex, escape, failures);
        }
        else if (IsLikeWildcard(pattern, patternIndex, LikeAnyCharacter, escape))
        {
            isMatch = valueIndex < value.Length
                && IsLikeMatch(pattern, patternIndex + 1, value, valueIndex + GetCodePointLength(value.AsSpan(valueIndex)), escape, failures);
        }
        else
        {
            isMatch = TryReadLikeLiteral(pattern, patternIndex, escape, out string literal, out int nextPatternIndex)
                && NoCaseCompareInfo.IsPrefix(value.AsSpan(valueIndex), literal, NoCaseCompareOptions, out int matchLength)
                && IsLikeMatch(pattern, nextPatternIndex, value, valueIndex + matchLength, escape, failures);
        }

        if (!isMatch)
        {
            failures.Add((patternIndex, valueIndex));
        }

        return isMatch;
    }

    /// <summary>
    /// Determines whether the rest of a value matches the rest of a LIKE pattern which starts with a <c>%</c> wildcard, ignoring case.
    /// </summary>
    /// <param name="pattern">The pattern.</param>
    /// <param name="patternIndex">The index of the <c>%</c> wildcard in <paramref name="pattern"/>.</param>
    /// <param name="value">The value to match.</param>
    /// <param name="valueIndex">The index in <paramref name="value"/> to match from.</param>
    /// <param name="escape">The escape character (one code point), or <see langword="null"/> if there isn't one.</param>
    /// <param name="failures">The pattern and value indexes already known not to match.</param>
    /// <returns><see langword="true"/> if the rest of <paramref name="value"/> matches the rest of <paramref name="pattern"/>, otherwise <see langword="false"/>.</returns>
    private static bool IsLikeAnySequenceMatch(string pattern, int patternIndex, string value, int valueIndex, string? escape, HashSet<(int PatternIndex, int ValueIndex)> failures)
    {
        // Several % wildcards in a row match the same as one.
        int nextPatternIndex = patternIndex + 1;

        while (nextPatternIndex < pattern.Length && IsLikeWildcard(pattern, nextPatternIndex, LikeAnySequence, escape))
        {
            nextPatternIndex++;
        }

        if (nextPatternIndex == pattern.Length)
        {
            return true;
        }

        // Try the rest of the pattern after each number of characters the % could match, from none to all of them.
        int candidateIndex = valueIndex;

        while (!IsLikeMatch(pattern, nextPatternIndex, value, candidateIndex, escape, failures))
        {
            if (candidateIndex == value.Length)
            {
                return false;
            }

            candidateIndex += GetCodePointLength(value.AsSpan(candidateIndex));
        }

        return true;
    }

    /// <summary>
    /// Determines whether the character at an index in a LIKE pattern is a wildcard.
    /// </summary>
    /// <param name="pattern">The pattern.</param>
    /// <param name="patternIndex">The index in <paramref name="pattern"/>.</param>
    /// <param name="wildcard">The wildcard (<c>%</c> or <c>_</c>).</param>
    /// <param name="escape">The escape character (one code point), or <see langword="null"/> if there isn't one.</param>
    /// <returns>
    /// <see langword="true"/> if the character is <paramref name="wildcard"/>, and <paramref name="wildcard"/> isn't also the escape character
    /// (if it is, it's only the escape character, as in SQLite), otherwise <see langword="false"/>.
    /// </returns>
    private static bool IsLikeWildcard(string pattern, int patternIndex, char wildcard, string? escape)
    {
        return pattern[patternIndex] == wildcard && !IsLikeEscape(pattern, patternIndex, escape);
    }

    /// <summary>
    /// Determines whether the escape character is at an index in a LIKE pattern.
    /// </summary>
    /// <param name="pattern">The pattern.</param>
    /// <param name="patternIndex">The index in <paramref name="pattern"/>.</param>
    /// <param name="escape">The escape character (one code point), or <see langword="null"/> if there isn't one.</param>
    /// <returns><see langword="true"/> if <paramref name="escape"/> is at <paramref name="patternIndex"/>, otherwise <see langword="false"/>.</returns>
    private static bool IsLikeEscape(string pattern, int patternIndex, [NotNullWhen(true)] string? escape)
    {
        return escape is not null && pattern.AsSpan(patternIndex).StartsWith(escape, StringComparison.Ordinal);
    }

    /// <summary>
    /// Reads the literal text at an index in a LIKE pattern, up to the next wildcard or the end of the pattern, removing the escape characters.
    /// </summary>
    /// <param name="pattern">The pattern.</param>
    /// <param name="patternIndex">The index in <paramref name="pattern"/> to read from, which mustn't be a wildcard.</param>
    /// <param name="escape">The escape character (one code point), or <see langword="null"/> if there isn't one.</param>
    /// <param name="literal">The literal text.</param>
    /// <param name="nextPatternIndex">The index in <paramref name="pattern"/> after the literal text.</param>
    /// <returns>
    /// <see langword="true"/> if the literal text was read, or <see langword="false"/> if the pattern ends with the escape character,
    /// which never matches anything (as in SQLite).
    /// </returns>
    private static bool TryReadLikeLiteral(string pattern, int patternIndex, string? escape, out string literal, out int nextPatternIndex)
    {
        StringBuilder builder = new();
        nextPatternIndex = patternIndex;

        while (nextPatternIndex < pattern.Length
            && !IsLikeWildcard(pattern, nextPatternIndex, LikeAnySequence, escape)
            && !IsLikeWildcard(pattern, nextPatternIndex, LikeAnyCharacter, escape))
        {
            if (IsLikeEscape(pattern, nextPatternIndex, escape))
            {
                nextPatternIndex += escape.Length;

                if (nextPatternIndex == pattern.Length)
                {
                    literal = string.Empty;
                    return false;
                }
            }

            // One char at a time is enough: the low half of an escaped surrogate pair is appended on the next pass,
            // as it can never be a wildcard or the start of the escape character.
            builder.Append(pattern[nextPatternIndex]);
            nextPatternIndex++;
        }

        literal = builder.ToString();
        return true;
    }

    /// <summary>
    /// Removes a set of characters from one or both ends of a string, ignoring case.
    /// </summary>
    /// <param name="value">The string to trim.</param>
    /// <param name="characters">The characters to remove.</param>
    /// <param name="trimStart">Whether to remove the characters from the start of <paramref name="value"/>.</param>
    /// <param name="trimEnd">Whether to remove the characters from the end of <paramref name="value"/>.</param>
    /// <returns>
    /// The trimmed string, or <see langword="null"/> if either argument is <see langword="null"/>.
    /// An empty <paramref name="characters"/> removes nothing.
    /// </returns>
    private static string? TrimCharacters(string? value, string? characters, bool trimStart, bool trimEnd)
    {
        if (value is null || characters is null)
        {
            return null;
        }

        ReadOnlySpan<char> trimmed = value;

        while (trimStart && !trimmed.IsEmpty)
        {
            int length = GetCodePointLength(trimmed);

            if (!IsInCharacterSet(trimmed[..length], characters))
            {
                break;
            }

            trimmed = trimmed[length..];
        }

        while (trimEnd && !trimmed.IsEmpty)
        {
            int length = GetLastCodePointLength(trimmed);

            if (!IsInCharacterSet(trimmed[^length..], characters))
            {
                break;
            }

            trimmed = trimmed[..^length];
        }

        return trimmed.Length == value.Length ? value : trimmed.ToString();
    }

    /// <summary>
    /// Determines whether a character is in a set of characters, ignoring case.
    /// </summary>
    /// <param name="character">The character (one code point).</param>
    /// <param name="characters">The set of characters.</param>
    /// <returns><see langword="true"/> if <paramref name="character"/> matches one of <paramref name="characters"/>, otherwise <see langword="false"/>.</returns>
    private static bool IsInCharacterSet(ReadOnlySpan<char> character, string characters)
    {
        ReadOnlySpan<char> remaining = characters;

        while (!remaining.IsEmpty)
        {
            int length = GetCodePointLength(remaining);

            if (NoCaseCompareInfo.Compare(character, remaining[..length], NoCaseCompareOptions) == 0)
            {
                return true;
            }

            remaining = remaining[length..];
        }

        return false;
    }

    /// <summary>
    /// Counts the characters (code points) in a string, as SQLite does, rather than the UTF-16 units.
    /// </summary>
    /// <param name="text">The string.</param>
    /// <returns>The number of code points in <paramref name="text"/>.</returns>
    private static int CountCodePoints(ReadOnlySpan<char> text)
    {
        int count = 0;

        foreach (Rune rune in text.EnumerateRunes())
        {
            count++;
        }

        return count;
    }

    /// <summary>
    /// Gets the number of UTF-16 units in the first code point of a string.
    /// </summary>
    /// <param name="text">The string, which mustn't be empty.</param>
    /// <returns>2 for a surrogate pair, otherwise 1 (including for an unpaired surrogate).</returns>
    private static int GetCodePointLength(ReadOnlySpan<char> text)
    {
        Rune.DecodeFromUtf16(text, out _, out int charsConsumed);
        return Math.Max(charsConsumed, 1);
    }

    /// <summary>
    /// Gets the number of UTF-16 units in the last code point of a string.
    /// </summary>
    /// <param name="text">The string, which mustn't be empty.</param>
    /// <returns>2 for a surrogate pair, otherwise 1 (including for an unpaired surrogate).</returns>
    private static int GetLastCodePointLength(ReadOnlySpan<char> text)
    {
        Rune.DecodeLastFromUtf16(text, out _, out int charsConsumed);
        return Math.Max(charsConsumed, 1);
    }

    #endregion private methods
}
