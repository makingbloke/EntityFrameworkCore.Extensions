// Copyright ©2021-2026 Mike King.
// This file is licensed to you under the MIT license.
// See the License.txt file in the solution root for more information.

using DotDoc.EntityFrameworkCore.Extensions.DatabaseType;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace DotDoc.EntityFrameworkCore.Extensions.CaseInsensitivity;

/// <summary>
/// Case Insensitive Collation Extensions.
/// </summary>
public static class CaseInsensitiveCollationExtensions
{
    #region public methods

    /// <summary>
    /// Use Case Insensitive Collation.
    /// </summary>
    /// <param name="modelBuilder">The builder used to construct the model for the context.</param>
    /// <param name="databaseType">The database type.</param>
    /// <returns>The same builder instance so multiple calls can be chained.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="modelBuilder"/> is <see langword="null"/>.</exception>
    /// <exception cref="UnsupportedDatabaseTypeException"><paramref name="databaseType"/> is not SQLite or SQL Server.</exception>
    public static ModelBuilder UseCaseInsensitiveCollation(this ModelBuilder modelBuilder, string? databaseType)
    {
        ArgumentNullException.ThrowIfNull(modelBuilder);

        switch (databaseType)
        {
            case DatabaseTypes.Sqlite:
                // SQLite does not support the concept of a global collation setting so set the NOCASE collation
                // on every text field that does not have a collation specified.
                foreach (IMutableProperty property in modelBuilder.Model.GetEntityTypes()
                    .SelectMany(t => t.GetProperties())
                    .Where(p => p.ClrType == typeof(string) && p.GetCollation() is null))
                {
                    property.SetCollation("NOCASE");
                }

                break;

            case DatabaseTypes.SqlServer:
                modelBuilder.UseCollation("Latin1_General_100_CI_AS_SC_UTF8");
                break;

            default:
                throw new UnsupportedDatabaseTypeException(nameof(databaseType));
        }

        return modelBuilder;
    }

    #endregion public methods
}