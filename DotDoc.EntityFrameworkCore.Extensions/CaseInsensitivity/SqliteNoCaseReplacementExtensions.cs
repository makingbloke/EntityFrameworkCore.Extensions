// Copyright ©2021-2025 Mike King.
// This file is licensed to you under the MIT license.
// See the License.txt file in the solution root for more information.

using DotDoc.EntityFrameworkCore.Extensions.DatabaseType;
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
    /// <remarks>
    /// Replaces the SQLite NoCase collation with a custom one which can handle Unicode
    /// characters and is accent and case insensitive (the original is ASCII only).
    /// </remarks>
    public static DbContextOptionsBuilder UseSqliteNoCaseReplacement(this DbContextOptionsBuilder optionsBuilder)
    {
        ArgumentNullException.ThrowIfNull(optionsBuilder);

        switch (optionsBuilder.GetDatabaseType())
        {
            case DatabaseTypes.Sqlite:
                optionsBuilder.
                    AddInterceptors(SqliteNoCaseReplacementInterceptor.Instance);
                break;

            default:
                throw new UnsupportedDatabaseTypeException();
        }

        return optionsBuilder;
    }

    #endregion public methods
}
