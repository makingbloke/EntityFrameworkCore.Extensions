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

        if (tableExpression.Table.EntityTypeMappings.FirstOrDefault()?.TypeBase is not IEntityType entityType)
        {
            throw new InvalidOperationException("Entity type not found");
        }

        return entityType;
    }

    #endregion public methods
}
