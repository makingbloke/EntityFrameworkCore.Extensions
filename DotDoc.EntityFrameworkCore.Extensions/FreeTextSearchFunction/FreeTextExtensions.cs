// Copyright ©2021-2025 Mike King.
// This file is licensed to you under the MIT license.
// See the License.txt file in the solution root for more information.

using DotDoc.EntityFrameworkCore.Extensions.MatchFunction;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DotDoc.EntityFrameworkCore.Extensions.FreeTextSearchFunction;

/// <summary>
/// FreeTextSearch Function Extensions.
/// </summary>
public static class FreeTextExtensions
{
    #region private fields

    /// <summary>
    /// The key used to store the stemming table name in entity annotations.
    /// </summary>
    private const string StemmingEntityAnnotationKey = "Sqlite:StemmingTable";

    #endregion private fields

    #region public methods

    /// <summary>
    /// Use the Free Text Extensions.
    /// </summary>
    /// <param name="optionsBuilder">The builder being used to configure the context.</param>
    /// <returns>The same builder instance so multiple calls can be chained.</returns>
    public static DbContextOptionsBuilder UseFreeTextExtensions(this DbContextOptionsBuilder optionsBuilder)
    {
        ArgumentNullException.ThrowIfNull(optionsBuilder);

        optionsBuilder.AddInterceptors(FreeTextQueryExpressionInterceptor.Instance);
        optionsBuilder.UseMatchExtensions();

        return optionsBuilder;
    }

    /// <summary>
    /// Set the stemming table Name associated with this table.
    /// </summary>
    /// <typeparam name="TEntity">Type of Entity.</typeparam>
    /// <param name="entityBuilder">The builder used to construct the entity for the context.</param>
    /// <param name="tableName">The stemming table name.</param>
    /// <returns>The same builder instance so multiple calls can be chained.</returns>
    public static EntityTypeBuilder<TEntity> SetStemmingTable<TEntity>(this EntityTypeBuilder<TEntity> entityBuilder, string tableName)
        where TEntity : class
    {
        ArgumentNullException.ThrowIfNull(entityBuilder);
        ArgumentException.ThrowIfNullOrEmpty(tableName);

        entityBuilder.HasAnnotation(StemmingEntityAnnotationKey, tableName);

        return entityBuilder;
    }

    /// <summary>
    /// Get the stemming table name associated with this table.
    /// </summary>
    /// <param name="entityType">The <see cref="IEntityType"/> of the table.</param>
    /// <returns>The table name or <see langword="null"/> if none found.</returns>
    public static string? GetStemmingTable(this IEntityType entityType)
    {
        ArgumentNullException.ThrowIfNull(entityType);

        string? stemmingTableName = (string?)entityType.FindAnnotation(StemmingEntityAnnotationKey)?.Value;
        return stemmingTableName;
    }

    #endregion public methods
}
