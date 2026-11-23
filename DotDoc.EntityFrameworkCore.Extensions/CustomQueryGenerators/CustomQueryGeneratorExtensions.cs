// Copyright ©2021-2026 Mike King.
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
    /// Get the <see cref="IEntityType"/> that a table expression is mapped to.
    /// </summary>
    /// <param name="tableExpression">The table expression.</param>
    /// <returns>The <see cref="IEntityType"/>.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="tableExpression"/> is <see langword="null"/>.</exception>
    /// <exception cref="InvalidOperationException">The table is not mapped to an entity type.</exception>
    public static IEntityType GetEntityType(this TableExpression tableExpression)
    {
        ArgumentNullException.ThrowIfNull(tableExpression);

        IEntityType entityType = tableExpression.Table.EntityTypeMappings.FirstOrDefault()?.TypeBase as IEntityType
            ?? throw new InvalidOperationException($"No entity type is mapped to the table {tableExpression.Name}.");

        return entityType;
    }

    #endregion public methods
}
