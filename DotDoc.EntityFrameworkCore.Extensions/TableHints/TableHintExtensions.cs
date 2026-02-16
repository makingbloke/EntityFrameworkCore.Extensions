// Copyright ©2021-2026 Mike King.
// This file is licensed to you under the MIT license.
// See the License.txt file in the solution root for more information.

using DotDoc.EntityFrameworkCore.Extensions.CustomQueryGenerators;
using DotDoc.EntityFrameworkCore.Extensions.DatabaseType;
using DotDoc.EntityFrameworkCore.Extensions.Utilities;
using Microsoft.EntityFrameworkCore;

namespace DotDoc.EntityFrameworkCore.Extensions.TableHints;

/// <summary>
/// Table Hint Extensions.
/// </summary>
public static class TableHintExtensions
{
    #region public methods

    /// <summary>
    /// Use the Table Hint Extensions.
    /// </summary>
    /// <param name="optionsBuilder">The builder being used to configure the context.</param>
    /// <returns>The same builder instance so multiple calls can be chained.</returns>
    public static DbContextOptionsBuilder UseTableHintExtensions(this DbContextOptionsBuilder optionsBuilder)
    {
        ArgumentNullException.ThrowIfNull(optionsBuilder);

        switch (optionsBuilder.GetDatabaseType())
        {
            case DatabaseTypes.SqlServer:
                optionsBuilder
                    .UseCustomQueryGenerator();
                break;

            default:
                throw new UnsupportedDatabaseTypeException();
        }

        return optionsBuilder;
    }

    /// <summary>
    /// Adds table hints to a query.
    /// </summary>
    /// <typeparam name="TSource">The type of the elements of <paramref name="source" />.</typeparam>
    /// <param name="source">The source query <see cref="IQueryable{TSource}" />.</param>
    /// <param name="tableHints">The table hints to add.</param>
    /// <returns>The number of rows updated in the database.</returns>
    public static IQueryable<TSource> WithTableHints<TSource>(this IQueryable<TSource> source, params IEnumerable<ITableHint> tableHints)
        where TSource : class
    {
        ArgumentNullException.ThrowIfNull(source);

        DbContext context = source.GetDbContext();

        if (context.Database.GetDatabaseType() == DatabaseTypes.SqlServer)
        {
            if (tableHints is null || !tableHints.Any())
            {
                throw new ArgumentNullException(nameof(tableHints), "At least one table hint must be supplied");
            }

            if (tableHints.Any(th => th is null))
            {
                throw new ArgumentException("Null table hints values are not supported", nameof(tableHints));
            }

            CustomQueryGeneratorParameters.TableHints.Value = tableHints.ToList();
        }

        return source;
    }

    #endregion public methods
}
