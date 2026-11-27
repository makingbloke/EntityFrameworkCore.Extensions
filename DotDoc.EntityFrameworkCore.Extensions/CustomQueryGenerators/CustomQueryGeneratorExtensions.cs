// Copyright ©2021-2026 Mike King.
// This file is licensed to you under the MIT license.
// See the License.txt file in the solution root for more information.

using DotDoc.EntityFrameworkCore.Extensions.DatabaseType;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;
using Microsoft.EntityFrameworkCore.Storage;
using System.Linq.Expressions;

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
    /// <exception cref="ArgumentNullException"><paramref name="optionsBuilder"/> is <see langword="null"/>.</exception>
    /// <exception cref="UnsupportedDatabaseTypeException">The database provider is not SQLite or SQL Server.</exception>
    public static DbContextOptionsBuilder UseCustomQueryGenerator(this DbContextOptionsBuilder optionsBuilder)
    {
        ArgumentNullException.ThrowIfNull(optionsBuilder);

        switch (optionsBuilder.Options.GetDatabaseType())
        {
            case DatabaseTypes.Sqlite:
                optionsBuilder.ReplaceService<IQuerySqlGeneratorFactory, SqliteCustomQueryGeneratorFactory>();
                break;

            case DatabaseTypes.SqlServer:
                optionsBuilder.ReplaceService<IQuerySqlGeneratorFactory, SqlServerCustomQueryGeneratorFactory>();
                break;

            default:
                throw new UnsupportedDatabaseTypeException(nameof(optionsBuilder));
        }

        return optionsBuilder;
    }

    /// <summary>
    /// Get the <see cref="IEntityType"/> with a given name from the entity types that a table expression is mapped to.
    /// </summary>
    /// <param name="tableExpression">The table expression.</param>
    /// <param name="entityTypeName">The name of the entity type, or <see langword="null"/> for the first entity type mapped to the table.</param>
    /// <returns>The <see cref="IEntityType"/>.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="tableExpression"/> is <see langword="null"/>.</exception>
    /// <exception cref="InvalidOperationException">The entity type is not mapped to the table.</exception>
    public static IEntityType GetEntityType(this TableExpression tableExpression, string? entityTypeName = null)
    {
        ArgumentNullException.ThrowIfNull(tableExpression);

        if (entityTypeName is null)
        {
            return tableExpression.Table.EntityTypeMappings.FirstOrDefault()?.TypeBase as IEntityType
                ?? throw new InvalidOperationException($"No entity type is mapped to the table {tableExpression.Name}.");
        }

        // Every entity type in a table-per-hierarchy inheritance hierarchy is mapped to the same table.
        foreach (ITableMappingBase mapping in tableExpression.Table.EntityTypeMappings)
        {
            if (mapping.TypeBase is IEntityType entityType && entityType.Name == entityTypeName)
            {
                return entityType;
            }
        }

        throw new InvalidOperationException($"The entity type {entityTypeName} is not mapped to the table {tableExpression.Name}.");
    }

    /// <summary>
    /// Get a setter for the discriminator column of an insert, if the entity type has a discriminator and the insert's setters don't set it.
    /// </summary>
    /// <param name="updateExpression">The update expression the insert is generated from.</param>
    /// <param name="entityType">The entity type of the row being inserted.</param>
    /// <returns>The setter, or <see langword="null"/> if the entity type has no discriminator or the insert's setters already set it.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="updateExpression"/> or <paramref name="entityType"/> is <see langword="null"/>.</exception>
    /// <exception cref="InvalidOperationException">The discriminator is not mapped to a column of the table.</exception>
    public static ColumnValueSetter? GetDiscriminatorSetter(this UpdateExpression updateExpression, IEntityType entityType)
    {
        ArgumentNullException.ThrowIfNull(updateExpression);
        ArgumentNullException.ThrowIfNull(entityType);

        IProperty? discriminatorProperty = entityType.FindDiscriminatorProperty();

        if (discriminatorProperty is null)
        {
            return null;
        }

        string columnName = updateExpression.Table.Table.FindColumn(discriminatorProperty)?.Name
            ?? throw new InvalidOperationException($"The discriminator {discriminatorProperty.Name} is not mapped to a column of the table {updateExpression.Table.Name}.");

        // The discriminator is decided by the entity type, so EF Core's own inserts (SaveChanges) always write it, usually to a shadow property
        // the caller wouldn't think to set. A value the caller did set is kept.
        if (updateExpression.ColumnValueSetters.Any(setter => setter.Column.Name == columnName))
        {
            return null;
        }

        RelationalTypeMapping typeMapping = discriminatorProperty.GetRelationalTypeMapping();

        return new ColumnValueSetter(
            new ColumnExpression(columnName, updateExpression.Table.Alias, discriminatorProperty.ClrType, typeMapping, nullable: false),
            new SqlConstantExpression(entityType.GetDiscriminatorValue(), discriminatorProperty.ClrType, typeMapping));
    }

    /// <summary>
    /// Check if the predicate of an insert's query only tests the discriminator column against constants, which is the filter EF Core adds to
    /// a query on a derived type in a table-per-hierarchy inheritance hierarchy.
    /// </summary>
    /// <param name="updateExpression">The update expression the insert is generated from.</param>
    /// <param name="entityType">The entity type of the row being inserted.</param>
    /// <returns><see langword="true"/> if the predicate is the discriminator filter, otherwise <see langword="false"/>.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="updateExpression"/> or <paramref name="entityType"/> is <see langword="null"/>.</exception>
    public static bool IsDiscriminatorFilter(this UpdateExpression updateExpression, IEntityType entityType)
    {
        ArgumentNullException.ThrowIfNull(updateExpression);
        ArgumentNullException.ThrowIfNull(entityType);

        IProperty? discriminatorProperty = entityType.FindDiscriminatorProperty();
        string? columnName = discriminatorProperty is null
            ? null
            : updateExpression.Table.Table.FindColumn(discriminatorProperty)?.Name;

        if (columnName is null)
        {
            return false;
        }

        // EF Core tests a type without derived types with =, and a type with derived types with IN.
        return updateExpression.SelectExpression.Predicate switch
        {
            SqlBinaryExpression { OperatorType: ExpressionType.Equal, Left: ColumnExpression column, Right: SqlConstantExpression }
                => IsTableColumn(column, updateExpression.Table, columnName),

            InExpression { Item: ColumnExpression column, Values: { } values }
                => IsTableColumn(column, updateExpression.Table, columnName) && values.All(value => value is SqlConstantExpression),

            _ => false,
        };
    }

    #endregion public methods

    #region private methods

    /// <summary>
    /// Check if a column expression is a column of a table expression.
    /// </summary>
    /// <param name="column">The column expression.</param>
    /// <param name="tableExpression">The table expression.</param>
    /// <param name="columnName">The column's name.</param>
    /// <returns><see langword="true"/> if the column expression is the column of the table expression, otherwise <see langword="false"/>.</returns>
    private static bool IsTableColumn(ColumnExpression column, TableExpression tableExpression, string columnName)
    {
        return column.Name == columnName && column.TableAlias == tableExpression.Alias;
    }

    #endregion private methods
}
