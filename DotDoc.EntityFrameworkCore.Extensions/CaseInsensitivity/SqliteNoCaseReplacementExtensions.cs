// Copyright ©2021-2026 Mike King.
// This file is licensed to you under the MIT license.
// See the License.txt file in the solution root for more information.

using DotDoc.EntityFrameworkCore.Extensions.DatabaseType;
using DotDoc.EntityFrameworkCore.Extensions.Utilities;
using Microsoft.EntityFrameworkCore;

namespace DotDoc.EntityFrameworkCore.Extensions.CaseInsensitivity;

/// <summary>
/// SQLite NoCase Replacement Extensions.
/// </summary>
public static class SqliteNoCaseReplacementExtensions
{
    #region public methods

    /// <summary>
    /// Makes SQLite's case insensitive text handling work for all of Unicode, not just ASCII letters, matching the
    /// SQL Server Latin1_General_100_CI_AS_SC_UTF8 collation used by UseCaseInsensitiveCollation (case insensitive, accent sensitive).
    /// </summary>
    /// <param name="optionsBuilder">The builder being used to configure the context.</param>
    /// <returns>The same builder instance so multiple calls can be chained.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="optionsBuilder"/> is <see langword="null"/>.</exception>
    /// <exception cref="UnsupportedDatabaseTypeException">The database provider is not SQLite.</exception>
    /// <remarks>
    /// <para>
    /// Does the following each time EF Core open a connection:
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
    /// them to nocase_* functions it registers alongside SQLite's built-in ones.
    /// </description></item>
    /// </list>
    /// <para>
    /// Everything else is unchanged: on a column that isn't NOCASE, or an expression with no known collation (such as a concatenation or a
    /// column from raw SQL), those methods use EF Core's own translation and SQLite's built-in functions.
    /// </para>
    /// <para>
    /// Connection pooling: Microsoft.Data.Sqlite pools connections by default, with one pool per connection string. Once a connection that uses
    /// UseSqliteNoCaseReplacement has been opened with a connection string, every later connection with the same connection string must use it too.
    /// That includes every DbContext, any SqliteConnection opened directly, and design-time tools such as dotnet ef.
    /// When a connection goes back to the pool, Microsoft.Data.Sqlite removes the collation and functions registered on it, and SQLite can't
    /// fall back to its built-in NOCASE, lower and upper on that connection afterwards. The next connection that doesn't register them again
    /// fails with "no such collation sequence: NOCASE" or "wrong number of arguments to function upper()" (or lower()).
    /// </para>
    /// </remarks>
    public static DbContextOptionsBuilder UseSqliteNoCaseReplacement(this DbContextOptionsBuilder optionsBuilder)
    {
        ArgumentNullException.ThrowIfNull(optionsBuilder);

        switch (optionsBuilder.Options.GetDatabaseType())
        {
            case DatabaseTypes.Sqlite:
                optionsBuilder
                    .AddMethodCallTranslatorPlugin<SqliteNoCaseTranslatorPlugin>()
                    .AddInterceptors(SqliteNoCaseReplacementInterceptor.Instance);
                break;

            default:
                throw new UnsupportedDatabaseTypeException(nameof(optionsBuilder));
        }

        return optionsBuilder;
    }

    #endregion public methods
}
