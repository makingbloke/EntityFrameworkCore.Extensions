// Copyright ©2021-2026 Mike King.
// This file is licensed to you under the MIT license.
// See the License.txt file in the solution root for more information.

using DotDoc.EntityFrameworkCore.Extensions.CustomQueryGenerators;
using DotDoc.EntityFrameworkCore.Extensions.DatabaseType;
using DotDoc.EntityFrameworkCore.Extensions.Utilities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.SqlServer.Query.Internal;
using System.Diagnostics.CodeAnalysis;

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
    /// <exception cref="ArgumentNullException"><paramref name="optionsBuilder"/> is <see langword="null"/>.</exception>
    /// <exception cref="UnsupportedDatabaseTypeException">The database provider is not SQL Server (table hints are SQL Server only).</exception>
    public static DbContextOptionsBuilder UseTableHintExtensions(this DbContextOptionsBuilder optionsBuilder)
    {
        ArgumentNullException.ThrowIfNull(optionsBuilder);

        switch (optionsBuilder.Options.GetDatabaseType())
        {
            case DatabaseTypes.SqlServer:
                optionsBuilder
                    .UseCustomQueryGenerator();
                break;

            default:
                throw new UnsupportedDatabaseTypeException(nameof(optionsBuilder));
        }

        return optionsBuilder;
    }

    /// <summary>
    /// Adds table hints to a query.
    /// </summary>
    /// <typeparam name="TSource">The type of the elements of <paramref name="source" />.</typeparam>
    /// <param name="source">The source query <see cref="IQueryable{TSource}" />.</param>
    /// <param name="tableHints">The table hints to add.</param>
    /// <returns>An instance of the <typeparamref name="TSource"/> with the table hints applied.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="source"/> or <paramref name="tableHints"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentException"><paramref name="source"/> is not an EF Core query, or on SQL Server <paramref name="tableHints"/> is empty or contains a <see langword="null"/> hint.</exception>
    /// <exception cref="InvalidOperationException">On SQL Server, <see cref="UseTableHintExtensions"/> was not called when configuring the DbContext, or another extension library has replaced its query generator.</exception>
    /// <remarks>
    /// Table hints are SQL Server only. On SQLite the query is returned unchanged.
    /// Calling <see cref="WithTableHints"/> more than once on the same query throws an <see cref="InvalidOperationException"/> when the query is run.
    /// </remarks>
    [SuppressMessage("Usage", "EF1001:Internal EF Core API usage.", Justification = "We need to know if EF Core's default query generator factory is installed.")]
    public static IQueryable<TSource> WithTableHints<TSource>(this IQueryable<TSource> source, params IEnumerable<ITableHint> tableHints)
        where TSource : class
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(tableHints);

        DbContext context = source.GetDbContext();

        if (context.Database.GetDatabaseType() == DatabaseTypes.SqlServer)
        {
            if (!tableHints.Any())
            {
                throw new ArgumentException("At least one table hint must be supplied", nameof(tableHints));
            }

            if (tableHints.Any(th => th is null))
            {
                throw new ArgumentException("Null table hints values are not supported", nameof(tableHints));
            }

            // The hints are written by the custom query generator. Without it the tag would just be written out as a comment and the hints ignored.
            // The generator is installed by UseTableHintExtensions (and also by UseExecuteUpdateExtensions, which the upsert methods rely on),
            // so check for the generator itself rather than the Use method. We check for the SqlServerQuerySqlGeneratorFactory by type rather
            // than using "is" as most custom query generator factories use it.
            IQuerySqlGeneratorFactory factory = context.GetService<IQuerySqlGeneratorFactory>();

            if (factory is not SqlServerCustomQueryGeneratorFactory)
            {
                string message = factory.GetType() == typeof(SqlServerQuerySqlGeneratorFactory)
                    ? $"{nameof(UseTableHintExtensions)} must be called when configuring the DbContext before {nameof(WithTableHints)} can be used."
                    : $"{nameof(WithTableHints)} cannot be used because another extension library has replaced the query generator (IQuerySqlGeneratorFactory) that {nameof(UseTableHintExtensions)} installs.";

                throw new InvalidOperationException(message);
            }

            source = source.TagWithTableHints(tableHints);
        }

        return source;
    }

    #endregion public methods
}
