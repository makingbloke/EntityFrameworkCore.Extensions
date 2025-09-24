// Copyright ©2021-2025 Mike King.
// This file is licensed to you under the MIT license.
// See the License.txt file in the solution root for more information.

using DotDoc.EntityFrameworkCore.Extensions.Constants;
using DotDoc.EntityFrameworkCore.Extensions.DatabaseType;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Query;

namespace DotDoc.EntityFrameworkCore.Extensions.FreeTextSearchFunction;

/// <summary>
/// FreeTextSearch Function Extensions.
/// </summary>
public static class FreeTextSearchExtensions
{
    #region public methods

    /// <summary>
    /// Use the Free Text Extensions.
    /// </summary>
    /// <param name="optionsBuilder">The builder being used to configure the context.</param>
    /// <returns>The same builder instance so multiple calls can be chained.</returns>
    public static DbContextOptionsBuilder UseFreeTextSearchExtensions(this DbContextOptionsBuilder optionsBuilder)
    {
        ArgumentNullException.ThrowIfNull(optionsBuilder);

        switch (optionsBuilder.GetDatabaseType())
        {
            case DatabaseTypes.Sqlite:
                optionsBuilder
                    .ReplaceService<IMethodCallTranslatorProvider, SqliteFreeTextSearchTranslatorProvider>()
                    .AddInterceptors(SqliteFreeTextSearchInterceptor.Instance);
                break;

            case DatabaseTypes.SqlServer:
                optionsBuilder
                    .ReplaceService<IMethodCallTranslatorProvider, SqlServerFreeTextSearchTranslatorProvider>();
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
    public static EntityTypeBuilder<TSource> SetStemmingTable<TSource>(this EntityTypeBuilder<TSource> entityBuilder, string tableName)
        where TSource : class
    {
        ArgumentNullException.ThrowIfNull(entityBuilder);
        ArgumentException.ThrowIfNullOrEmpty(tableName);

        entityBuilder.HasAnnotation(AnnotationNames.SqliteStemmingTable, tableName);

        return entityBuilder;
    }

    /// <summary>
    /// Get the stemming table name associated with this entity.
    /// </summary>
    /// <param name="entityType">The <see cref="IEntityType"/>.</param>
    /// <returns>The table name or <see langword="null"/> if none found.</returns>
    public static string? GetStemmingTable(this IEntityType entityType)
    {
        ArgumentNullException.ThrowIfNull(entityType);

        string? stemmingTableName = (string?)entityType.FindAnnotation(AnnotationNames.SqliteStemmingTable)?.Value;
        return stemmingTableName;
    }

    #endregion public methods
}
