// Copyright ©2021-2025 Mike King.
// This file is licensed to you under the MIT license.
// See the License.txt file in the solution root for more information.

using DotDoc.EntityFrameworkCore.Extensions.DatabaseType;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;

namespace DotDoc.EntityFrameworkCore.Extensions.CustomQueryGenerators;

/// <summary>
/// Custom Query Generator Extensions.
/// </summary>
internal static class CustomQueryGeneratorExtensions
{
    #region public methods

    /// <summary>
    /// Use the Custom Query Generator.
    /// </summary>
    /// <param name="optionsBuilder">The builder being used to configure the context.</param>
    /// <returns>The same builder instance so multiple calls can be chained.</returns>
    public static DbContextOptionsBuilder UseCustomQueryGenerator(this DbContextOptionsBuilder optionsBuilder)
    {
        ArgumentNullException.ThrowIfNull(optionsBuilder);

        switch (optionsBuilder.GetDatabaseType())
        {
            case DatabaseTypes.Sqlite:
                optionsBuilder.ReplaceService<IQuerySqlGeneratorFactory, SqliteCustomQueryGeneratorFactory>();
                break;

            case DatabaseTypes.SqlServer:
                optionsBuilder.ReplaceService<IQuerySqlGeneratorFactory, SqlServerCustomQueryGeneratorFactory>();
                break;

            default:
                throw new UnsupportedDatabaseTypeException();
        }

        return optionsBuilder;
    }

    /// <summary>
    /// Get the <see cref="IEntityType"/> of a table expression.
    /// </summary>
    /// <param name="tableExpression">The table expression.</param>
    /// <returns><see cref="IEntityType"/>.</returns>
    public static IEntityType GetEntityType(this TableExpression tableExpression)
    {
        ArgumentNullException.ThrowIfNull(tableExpression);

        Type type = tableExpression.Table.EntityTypeMappings.Single().TypeBase.ClrType;
        IEntityType? entityType = tableExpression.Table.Model.Model.FindEntityType(type);

        if (entityType is null)
        {
            throw new InvalidOperationException("Entity type not found");
        }

        return entityType;
    }

    /// <summary>
    /// Get the primary key column name for an entity type.
    /// </summary>
    /// <param name="entityType">The entity type.</param>
    /// <returns>The column name.</returns>
    public static string GetPrimaryKeyColumnName(this IEntityType entityType)
    {
        IReadOnlyKey? key = entityType.FindPrimaryKey();

        if (key is null || key.Properties.Count != 1)
        {
            throw new InvalidOperationException("Primary key not found.");
        }

        StoreObjectIdentifier storeObjectIdentifier = StoreObjectIdentifier.Table(entityType.GetTableName()!, entityType.GetSchema());
        string columnName = key.Properties[0].GetColumnName(storeObjectIdentifier)!;

        if (string.IsNullOrEmpty(columnName))
        {
            throw new InvalidOperationException("Primary key column name not found.");
        }

        return columnName;
    }

    #endregion public methods
}
