// Copyright ©2021-2026 Mike King.
// This file is licensed to you under the MIT license.
// See the License.txt file in the solution root for more information.

using DotDoc.EntityFrameworkCore.Extensions.DatabaseType;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace DotDoc.EntityFrameworkCore.Extensions.Utilities;

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
                SetCaseInsensitiveCollationSqlite(modelBuilder);
                break;

            case DatabaseTypes.SqlServer:
                SetCaseInsensitiveCollationSqlServer(modelBuilder);
                break;

            default:
                throw new UnsupportedDatabaseTypeException(nameof(databaseType));
        }

        return modelBuilder;
    }

    #endregion public methods

    #region private methods

    /// <summary>
    /// Set case insensitive collation for SQLite.
    /// </summary>
    /// <param name="modelBuilder">The builder used to construct the model for the context.</param>
    /// <remarks>
    /// SQLite does not support the concept of a global collation setting so set the NOCASE
    /// collation on every text field in the entities: <see href="https://github.com/dotnet/efcore/issues/32051"/>.
    /// </remarks>
    private static void SetCaseInsensitiveCollationSqlite(ModelBuilder modelBuilder)
    {
        foreach (IMutableProperty property in modelBuilder.Model.GetEntityTypes()
                                                .SelectMany(t => t.GetProperties())
                                                .Where(p => p.ClrType == typeof(string)))
        {
            property.SetCollation("NOCASE");
        }
    }

    /// <summary>
    /// Set case insensitive collation for SQL Server.
    /// </summary>
    /// <param name="modelBuilder">The builder used to construct the model for the context.</param>
    private static void SetCaseInsensitiveCollationSqlServer(ModelBuilder modelBuilder)
    {
        modelBuilder.UseCollation("SQL_Latin1_General_CP1_CI_AS");
    }

    #endregion private methods
}
