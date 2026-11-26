// Copyright ©2021-2026 Mike King.
// This file is licensed to you under the MIT license.
// See the License.txt file in the solution root for more information.

using DotDoc.EntityFrameworkCore.Extensions.DatabaseType;
using DotDoc.EntityFrameworkCore.Extensions.Utilities;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using System.Globalization;

namespace DotDoc.EntityFrameworkCore.Extensions.CaseInsensitivity;

/// <summary>
/// SQLite NoCase Replacement Extensions.
/// </summary>
public static class SqliteNoCaseReplacementExtensions
{
    #region private fields

    /// <summary>
    /// Whether .NET is running in globalization-invariant mode, where culture-aware comparisons are ordinal.
    /// </summary>
    /// <remarks>
    /// State is determined by how two equal strings compare (é, and e followed by a combining acute accent), rather than from a setting, as the mode
    /// can be turned on in the project, the runtime config or an environment variable. With ICU (or NLS on Windows) they're equal, ordinally they aren't.
    /// </remarks>
    private static readonly bool IsGlobalizationInvariant = CultureInfo.InvariantCulture.CompareInfo.Compare("\u00E9", "e\u0301", CompareOptions.None) != 0;

    #endregion private fields

    #region public methods

    /// <summary>
    /// Makes SQLite's case insensitive text handling work for all of Unicode, not just ASCII letters, matching the
    /// SQL Server Latin1_General_100_CI_AS_SC_UTF8 collation used by UseCaseInsensitiveCollation (case insensitive, accent sensitive).
    /// </summary>
    /// <param name="optionsBuilder">The builder being used to configure the context.</param>
    /// <returns>The same builder instance so multiple calls can be chained.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="optionsBuilder"/> is <see langword="null"/>.</exception>
    /// <exception cref="UnsupportedDatabaseTypeException">The database provider is not SQLite.</exception>
    /// <exception cref="InvalidOperationException">.NET is running in globalization-invariant mode, which has no ICU to compare text with.</exception>
    /// <remarks>
    /// <para>
    /// Does the following on each connection EF Core uses, including one that was already open when it was given to EF Core
    /// (e.g. for an in-memory database):
    /// </para>
    /// <list type="bullet">
    /// <item><description>
    /// Replaces SQLite's NOCASE collation, which only ignores the case of ASCII letters, with a Unicode one, so comparisons, sorting,
    /// grouping and indexes on a NOCASE column ignore the case of every letter (é = É) but not accents (e ≠ é);
    /// </description></item>
    /// <item><description>
    /// Replaces SQLite's lower and upper functions, which only change ASCII letters, with Unicode ones, so ToLower() and ToUpper()
    /// change every letter, on every column;
    /// </description></item>
    /// <item><description>
    /// Makes Contains, IndexOf, StartsWith, EndsWith, Replace, Trim / TrimStart / TrimEnd (with constant characters) and
    /// EF.Functions.Like ignore case in the same way, on a NOCASE column or EF.Functions.Collate(..., "NOCASE"), by translating
    /// them to nocase_* functions it registers alongside SQLite's built-in ones;
    /// </description></item>
    /// <item><description>
    /// Gives the result of Replace, Substring, ToLower, ToUpper and Trim / TrimStart / TrimEnd on a NOCASE column, and of a concatenation,
    /// ?? or conditional expression with a NOCASE operand, the NOCASE collation, as SQL Server gives the result its operands' collation,
    /// so comparing the result, sorting or grouping by it, or calling one of these methods on it, ignores case too.
    /// </description></item>
    /// </list>
    /// <para>
    /// Everything else is unchanged: on a column that isn't NOCASE, or an expression with no known collation (such as a constant, a number
    /// converted to a string, or a column from raw SQL), those methods use EF Core's own translation and SQLite's built-in functions.
    /// </para>
    /// <para>
    /// Connection pooling: Microsoft.Data.Sqlite pools connections by default, with one pool per connection string. Once a connection that uses
    /// UseSqliteNoCaseReplacement has been opened with a connection string, every later connection with the same connection string must use it too.
    /// That includes every DbContext, any SqliteConnection opened directly (by calling <see cref="UseSqliteNoCaseReplacement(SqliteConnection)"/>
    /// on it), and design-time tools such as dotnet ef.
    /// When a connection goes back to the pool, Microsoft.Data.Sqlite removes the collation and functions registered on it, and SQLite can't
    /// fall back to its built-in NOCASE, lower and upper on that connection afterwards. The next connection that doesn't register them again
    /// fails with "no such collation sequence: NOCASE" or "wrong number of arguments to function upper()" (or lower()).
    /// </para>
    /// <para>
    /// Sort order: the NOCASE collation compares text with ICU, and SQLite stores indexes on NOCASE columns (including primary keys and unique
    /// indexes) in that order. Every process that writes to the database must use the replacement (with
    /// <see cref="UseSqliteNoCaseReplacement(SqliteConnection)"/> if it doesn't use EF Core), and <c>REINDEX NOCASE</c> should be run after first
    /// using it on an existing database, and after an operating system, .NET or ICU upgrade, or moving the database to another machine, as the order
    /// can change. It can't be used in globalization-invariant mode (InvariantGlobalization), which has no ICU.
    /// </para>
    /// </remarks>
    public static DbContextOptionsBuilder UseSqliteNoCaseReplacement(this DbContextOptionsBuilder optionsBuilder)
    {
        ArgumentNullException.ThrowIfNull(optionsBuilder);

        switch (optionsBuilder.Options.GetDatabaseType())
        {
            case DatabaseTypes.Sqlite:
                ThrowIfGlobalizationInvariant();

                optionsBuilder
                    .AddMethodCallTranslatorPlugin<SqliteNoCaseTranslatorPlugin>()
                    .AddInterceptors(SqliteNoCaseReplacementInterceptor.Instance)
                    .AddInterceptors(SqliteNoCaseQueryExpressionInterceptor.Instance);
                break;

            default:
                throw new UnsupportedDatabaseTypeException(nameof(optionsBuilder));
        }

        return optionsBuilder;
    }

    /// <summary>
    /// Makes SQLite's case insensitive text handling on a connection work for all of Unicode, not just ASCII letters, matching the
    /// SQL Server Latin1_General_100_CI_AS_SC_UTF8 collation used by UseCaseInsensitiveCollation (case insensitive, accent sensitive).
    /// </summary>
    /// <param name="connection">The connection.</param>
    /// <returns>The same connection, so multiple calls can be chained.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="connection"/> is <see langword="null"/>.</exception>
    /// <exception cref="InvalidOperationException">.NET is running in globalization-invariant mode, which has no ICU to compare text with.</exception>
    /// <remarks>
    /// <para>
    /// Registers the same collation and functions on the connection as <see cref="UseSqliteNoCaseReplacement(DbContextOptionsBuilder)"/>
    /// does on each connection EF Core uses:
    /// </para>
    /// <list type="bullet">
    /// <item><description>
    /// Replaces SQLite's NOCASE collation, which only ignores the case of ASCII letters, with a Unicode one that ignores the case of
    /// every letter (é = É) but not accents (e ≠ é);
    /// </description></item>
    /// <item><description>
    /// Replaces SQLite's lower and upper functions, which only change ASCII letters, with Unicode ones;
    /// </description></item>
    /// <item><description>
    /// Adds the nocase_* functions (nocase_instr, nocase_like, nocase_replace, nocase_trim, nocase_ltrim, nocase_rtrim, nocase_startswith
    /// and nocase_endswith), case insensitive versions of SQLite's string functions that SQL can call directly.
    /// </description></item>
    /// </list>
    /// <para>
    /// Use it on a SqliteConnection that is used without EF Core, such as one opened directly with the same connection string as a
    /// DbContext that uses UseSqliteNoCaseReplacement (see the connection pooling note on <see cref="UseSqliteNoCaseReplacement(DbContextOptionsBuilder)"/>).
    /// A connection used by such a DbContext doesn't need it, as EF Core registers the collation and functions on it itself.
    /// It doesn't change how LINQ queries are translated.
    /// </para>
    /// <para>
    /// In SQL you write yourself, call the nocase_* functions directly, and add COLLATE NOCASE to a concatenation or a function's result
    /// that should ignore case, as SQLite gives them no collation (e.g. <c>a || b COLLATE NOCASE = 'X'</c>).
    /// </para>
    /// <para>
    /// It can be called before or after the connection is opened, and only needs calling once: the collation and functions stay registered
    /// on the connection when it's closed and opened again.
    /// </para>
    /// <para>
    /// See the sort order note on <see cref="UseSqliteNoCaseReplacement(DbContextOptionsBuilder)"/>.
    /// </para>
    /// </remarks>
    public static SqliteConnection UseSqliteNoCaseReplacement(this SqliteConnection connection)
    {
        ArgumentNullException.ThrowIfNull(connection);
        ThrowIfGlobalizationInvariant();

        connection.CreateCollation(SqliteUnicodeFunctionNames.NoCaseCollation, SqliteUnicodeMethods.Compare);
        connection.CreateFunction<string?, string?>(SqliteUnicodeFunctionNames.LowerFunction, SqliteUnicodeMethods.Lower, isDeterministic: true);
        connection.CreateFunction<string?, string?>(SqliteUnicodeFunctionNames.UpperFunction, SqliteUnicodeMethods.Upper, isDeterministic: true);
        connection.CreateFunction<string?, string?, bool?>(SqliteUnicodeFunctionNames.NoCaseEndsWithFunction, SqliteUnicodeMethods.EndsWith, isDeterministic: true);
        connection.CreateFunction<string?, string?, int?>(SqliteUnicodeFunctionNames.NoCaseInstrFunction, SqliteUnicodeMethods.Instr, isDeterministic: true);
        connection.CreateFunction<string?, string?, bool?>(SqliteUnicodeFunctionNames.NoCaseLikeFunction, SqliteUnicodeMethods.Like, isDeterministic: true);
        connection.CreateFunction<string?, string?, string?, bool?>(SqliteUnicodeFunctionNames.NoCaseLikeFunction, SqliteUnicodeMethods.Like, isDeterministic: true);
        connection.CreateFunction<string?, string?, string?>(SqliteUnicodeFunctionNames.NoCaseLTrimFunction, SqliteUnicodeMethods.LTrim, isDeterministic: true);
        connection.CreateFunction<string?, string?, string?, string?>(SqliteUnicodeFunctionNames.NoCaseReplaceFunction, SqliteUnicodeMethods.Replace, isDeterministic: true);
        connection.CreateFunction<string?, string?, string?>(SqliteUnicodeFunctionNames.NoCaseRTrimFunction, SqliteUnicodeMethods.RTrim, isDeterministic: true);
        connection.CreateFunction<string?, string?, bool?>(SqliteUnicodeFunctionNames.NoCaseStartsWithFunction, SqliteUnicodeMethods.StartsWith, isDeterministic: true);
        connection.CreateFunction<string?, string?, string?>(SqliteUnicodeFunctionNames.NoCaseTrimFunction, SqliteUnicodeMethods.Trim, isDeterministic: true);

        return connection;
    }

    #endregion public methods

    #region private methods

    /// <summary>
    /// Throws if .NET is running in globalization-invariant mode.
    /// </summary>
    /// <exception cref="InvalidOperationException">.NET is running in globalization-invariant mode.</exception>
    private static void ThrowIfGlobalizationInvariant()
    {
        // Without ICU, culture-aware comparisons are ordinal, so the NOCASE replacement couldn't match SQL Server (é sorts after f, and é doesn't
        // equal e followed by a combining accent), and indexes on NOCASE columns would be stored in a different order from a database written
        // with ICU. It fails here, before anything is compared or written, rather than giving different results silently.
        if (IsGlobalizationInvariant)
        {
            throw new InvalidOperationException(
                "UseSqliteNoCaseReplacement can't be used in globalization-invariant mode, as it needs ICU to compare text. " +
                "Turn invariant mode off (InvariantGlobalization / DOTNET_SYSTEM_GLOBALIZATION_INVARIANT) and make ICU available, or do not use UseSqliteNoCaseReplacement.");
        }
    }

    #endregion private methods
}
