// Copyright ©2021-2026 Mike King.
// This file is licensed to you under the MIT license.
// See the License.txt file in the solution root for more information.

using DotDoc.EntityFrameworkCore.Extensions.DatabaseType;
using DotDoc.EntityFrameworkCore.Extensions.Utilities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DotDoc.EntityFrameworkCore.Extensions.FreeTextSearchFunction;

/// <summary>
/// FreeTextSearch Function Extensions.
/// </summary>
public static class FreeTextSearchExtensions
{
    #region private constants

    /// <summary>
    /// Name of the annotation used to store the SQLite stemming table name.
    /// </summary>
    private const string SqliteStemmingTable = "DotDoc.SqliteStemmingTable";

    #endregion private constants

    #region public methods

    /// <summary>
    /// Use the Free Text Extensions.
    /// </summary>
    /// <param name="optionsBuilder">The builder being used to configure the context.</param>
    /// <returns>The same builder instance so multiple calls can be chained.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="optionsBuilder"/> is <see langword="null"/>.</exception>
    /// <exception cref="UnsupportedDatabaseTypeException">The database provider is not SQLite or SQL Server.</exception>
    public static DbContextOptionsBuilder UseFreeTextSearchExtensions(this DbContextOptionsBuilder optionsBuilder)
    {
        ArgumentNullException.ThrowIfNull(optionsBuilder);

        switch (optionsBuilder.Options.GetDatabaseType())
        {
            case DatabaseTypes.Sqlite:
                optionsBuilder
                    .AddMethodCallTranslatorPlugin<SqliteFreeTextSearchTranslatorPlugin>()
                    .AddInterceptors(SqliteFreeTextSearchInterceptor.Instance);
                break;

            case DatabaseTypes.SqlServer:
                optionsBuilder
                    .AddMethodCallTranslatorPlugin<SqlServerFreeTextSearchTranslatorPlugin>();
                break;

            default:
                throw new UnsupportedDatabaseTypeException();
        }

        return optionsBuilder;
    }

    /// <summary>
    /// Set the stemming table name associated with this entity.
    /// </summary>
    /// <typeparam name="TSource">The type of the elements of the source.</typeparam>
    /// <param name="entityBuilder">The builder used to construct the entity for the context.</param>
    /// <param name="tableName">The stemming table name.</param>
    /// <returns>The same builder instance so multiple calls can be chained.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="entityBuilder"/> or <paramref name="tableName"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentException"><paramref name="tableName"/> is empty.</exception>
    public static EntityTypeBuilder<TSource> SetStemmingTable<TSource>(this EntityTypeBuilder<TSource> entityBuilder, string tableName)
        where TSource : class
    {
        ArgumentNullException.ThrowIfNull(entityBuilder);
        ArgumentException.ThrowIfNullOrEmpty(tableName);

        entityBuilder.HasAnnotation(SqliteStemmingTable, tableName);

        return entityBuilder;
    }

    /// <summary>
    /// Get the stemming table name associated with this entity.
    /// </summary>
    /// <param name="entityType">The <see cref="IEntityType"/>.</param>
    /// <returns>The table name or <see langword="null"/> if none found.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="entityType"/> is <see langword="null"/>.</exception>
    public static string? GetStemmingTable(this IEntityType entityType)
    {
        ArgumentNullException.ThrowIfNull(entityType);

        string? stemmingTableName = (string?)entityType.FindAnnotation(SqliteStemmingTable)?.Value;
        return stemmingTableName;
    }

    #endregion public methods
}
