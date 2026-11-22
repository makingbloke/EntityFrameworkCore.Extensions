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
    /// Use the SQLite NoCase replacement.
    /// </summary>
    /// <param name="optionsBuilder">The builder being used to configure the context.</param>
    /// <returns>The same builder instance so multiple calls can be chained.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="optionsBuilder"/> is <see langword="null"/>.</exception>
    /// <exception cref="UnsupportedDatabaseTypeException">The database provider is not SQLite.</exception>
    /// <remarks>
    /// Connection pooling: Microsoft.Data.Sqlite pools connections by default, with one pool per connection string. Once a connection that uses
    /// UseSqliteNoCaseReplacement has been opened with a connection string, every later connection with the same connection string must use it too.
    /// That includes every DbContext, any SqliteConnection opened directly, and design-time tools such as dotnet ef.
    /// When a connection goes back to the pool, Microsoft.Data.Sqlite removes the collation and functions registered on it, and SQLite can't
    /// fall back to its built-in NOCASE, lower and upper on that connection afterwards. The next connection that doesn't register them again
    /// fails with "no such collation sequence: NOCASE" or "wrong number of arguments to function upper()" (or lower()).
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
