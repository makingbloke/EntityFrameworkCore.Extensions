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
    /// Adds table hints to a query, for the tables of the entity types given to the builder.
    /// </summary>
    /// <typeparam name="TSource">The type of the elements of <paramref name="source" />.</typeparam>
    /// <param name="source">The source query <see cref="IQueryable{TSource}" />.</param>
    /// <param name="buildAction">A method that adds the table hints for each entity type's table, e.g. <c>h => h.For&lt;Blog&gt;(SqlServerTableHints.NoLock).For&lt;Post&gt;(SqlServerTableHints.NoLock)</c>.</param>
    /// <returns>An instance of the <typeparamref name="TSource"/> with the table hints applied.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="source"/> or <paramref name="buildAction"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentException">
    /// <paramref name="source"/> is not an EF Core query, or on SQL Server <paramref name="buildAction"/> adds no table hints, or a call to
    /// <see cref="TableHintsBuilder.For{TEntity}"/> isn't valid (see its exceptions).
    /// </exception>
    /// <exception cref="InvalidOperationException">On SQL Server, <see cref="UseTableHintExtensions"/> was not called when configuring the DbContext, or another extension library has replaced its query generator.</exception>
    /// <remarks>
    /// Table hints are SQL Server only. On SQLite the query is returned unchanged and <paramref name="buildAction"/> isn't called.
    /// The hints for an entity type are added to every use of its table in the query (including joined, included and subquery tables), and only to that
    /// table. Calling <see cref="WithTableHints"/> more than once on the same query is allowed for different tables, but different hints for the same
    /// table throw an <see cref="InvalidOperationException"/> when the query is run.
    /// </remarks>
    [SuppressMessage("Usage", "EF1001:Internal EF Core API usage.", Justification = "We need to know if EF Core's default query generator factory is installed.")]
    public static IQueryable<TSource> WithTableHints<TSource>(this IQueryable<TSource> source, Action<TableHintsBuilder> buildAction)
        where TSource : class
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(buildAction);

        DbContext context = source.GetDbContext();

        if (context.Database.GetDatabaseType() == DatabaseTypes.SqlServer)
        {
            TableHintsBuilder builder = new(context.Model);
            buildAction(builder);

            if (builder.TableHints.Count == 0)
            {
                throw new ArgumentException("At least one table's hints must be added", nameof(buildAction));
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

            // One tag for each table, so the custom query generator adds each table's hints only to that table.
            foreach (KeyValuePair<string, ITableHint[]> tableHints in builder.TableHints)
            {
                source = source.TagWithTableHints(tableHints.Key, tableHints.Value);
            }
        }

        return source;
    }

    #endregion public methods
}
