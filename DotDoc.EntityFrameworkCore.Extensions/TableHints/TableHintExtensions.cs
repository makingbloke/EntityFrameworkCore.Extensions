// Copyright ©2021-2025 Mike King.
// This file is licensed to you under the MIT license.
// See the License.txt file in the solution root for more information.

using DotDoc.EntityFrameworkCore.Extensions.Constants;
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
        ArgumentNullException.ThrowIfNull(tableHints);

        DbContext context = source.GetDbContext();

        switch (context.Database.GetDatabaseType())
        {
            case DatabaseTypes.SqlServer:
                string value = string.Join(
                    ", ",
                    tableHints
                        .Where(h => h is not null)
                        .Select(h => h.ToString())
                        .Distinct());

                if (string.IsNullOrEmpty(value))
                {
                    throw new ArgumentException("No valid table hints provided", nameof(tableHints));
                }

                source = TagCollection.TagQuery(source, TagNames.TableHint, value);
                break;
        }

        return source;
    }

    #endregion public methods
}
