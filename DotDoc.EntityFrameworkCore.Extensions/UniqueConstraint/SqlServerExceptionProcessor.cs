// Copyright ©2021-2025 Mike King.
// This file is licensed to you under the MIT license.
// See the License.txt file in the solution root for more information.

using DotDoc.EntityFrameworkCore.Extensions.Utilities;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System.Data;
using System.Text.RegularExpressions;

namespace DotDoc.EntityFrameworkCore.Extensions.UniqueConstraint;

/// <summary>
/// SQL Server Exception Processor.
/// </summary>
internal sealed partial class SqlServerExceptionProcessor : IExceptionProcessor
{
    #region private fields

    /// <summary>
    /// The SQL Server default schema name.
    /// </summary>
    private const string _defaultSchema = "dbo";

    #endregion private fields

    #region public methods

    /// <inheritdoc/>
    public async Task<UniqueConstraintDetails?> GetUniqueConstraintDetailsAsync(DatabaseFacade database, Exception e, CancellationToken cancellationToken = default)
    {
        UniqueConstraintDetails? details = null;

        if (ParseException(e, out string? schema, out string? tableName, out string? indexName))
        {
            DbContext context = database.GetDbContext();

            details = GetUniqueConstraintDetailsFromEntityFrameWork(context, schema!, tableName!, indexName!)
                        ?? await GetUniqueConstraintDetailsFromSqlServerAsync(context, schema!, tableName!, indexName!, cancellationToken).ConfigureAwait(false);
        }

        return details;
    }

    #endregion public methods

    #region private methods

    /// <summary>
    /// Validate and parse the supplied exception to extract the constraint details.
    /// </summary>
    /// <param name="e">The exception to process.</param>
    /// <param name="schema">The schema (out).</param>
    /// <param name="tableName">The table name (out).</param>
    /// <param name="indexName">The index name (out).</param>
    /// <returns>
    /// <see langword="true"> if successful else <see langword="false"/>.</see>
    /// </returns>
    private static bool ParseException(Exception e, out string? schema, out string? tableName, out string? indexName)
    {
        bool success = false;
        schema = null;
        tableName = null;
        indexName = null;

        if (e is DbUpdateException)
        {
            e = e.GetBaseException();
        }

        if (e is SqlException)
        {
            Match match = ErrorMessageRegex().Match(e.Message);

            if (match.Success)
            {
                success = true;
                schema = match.Groups["schema"].Value;
                tableName = match.Groups["tablename"].Value;
                indexName = match.Groups["indexname"].Value;
            }
        }

        return success;
    }

    /// <summary>
    /// Get the details of a unique constraint from EF core.
    /// </summary>
    /// <param name="context">The database context.</param>
    /// <param name="schema">The schema.</param>
    /// <param name="tableName">The table name.</param>
    /// <param name="indexName">The index name.</param>
    /// <returns>An instance <see cref="UniqueConstraintDetails"/> if the table can be found in EF Core.</returns>
    private static UniqueConstraintDetails? GetUniqueConstraintDetailsFromEntityFrameWork(DbContext context, string schema, string tableName, string indexName)
    {
        UniqueConstraintDetails? details = null;

        // In EF Core the default schema for SQL Server dbo is stored as null.
        string? entitySchema = schema == _defaultSchema
            ? null
            : schema;

        IEntityType? entityType = context.Model
            .GetEntityTypes()
            .FirstOrDefault(et => et.GetSchema() == entitySchema && et.GetTableName() == tableName);

        if (entityType is not null)
        {
            string entityTableName = entityType.ShortName();

            StoreObjectIdentifier storeObjectIdentifier = StoreObjectIdentifier.Table(tableName, entitySchema);

            IIndex? index = entityType.GetIndexes()
                .FirstOrDefault(idx => indexName == idx
                .GetDatabaseName(storeObjectIdentifier));

            if (index is not null)
            {
                List<string> fieldNames = index.Properties
                    .Select(p => p.Name)
                    .ToList();

                details = new(schema, entityTableName, fieldNames);
            }
        }

        return details;
    }

    /// <summary>
    /// Get the details of a unique constraint from SQL Server.
    /// </summary>
    /// <param name="context">The database context.</param>
    /// <param name="schema">The schema.</param>
    /// <param name="tableName">The table name.</param>
    /// <param name="indexName">The index name.</param>
    /// <returns>An instance <see cref="UniqueConstraintDetails"/> if the table can be found in EF Core.</returns>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
    private static async Task<UniqueConstraintDetails?> GetUniqueConstraintDetailsFromSqlServerAsync(DbContext context, string schema, string tableName, string indexName, CancellationToken cancellationToken)
    {
        FormattableString sql = BuildSql(schema, tableName, indexName);

        List<string> fieldNames = await context.Database
            .SqlQuery<string>(sql)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        UniqueConstraintDetails? details = fieldNames.Count > 0
            ? new(schema, tableName, fieldNames)
            : null;

        return details;
    }

    /// <summary>
    /// Build the query to fetch field names used by a unique constraint.
    /// </summary>
    /// <param name="schema">The schema.</param>
    /// <param name="tableName">The table name.</param>
    /// <param name="indexName">The index name.</param>
    /// <returns>A <see cref="FormattableString"/> encapsulating the query and parameters.</returns>
    private static FormattableString BuildSql(string schema, string tableName, string indexName)
    {
        FormattableString sql =
$@"SELECT sc.name 
FROM sys.indexes si, sys.index_columns sic, sys.columns sc
WHERE si.object_id = OBJECT_ID('[' + {schema} + '].[' + {tableName} + ']') AND si.name = {indexName}
    AND sic.object_id = si.object_id AND sic.index_id = si.index_id
    AND sc.object_id = sic.object_id AND sc.column_id = sic.column_id";

        return sql;
    }

    /// <summary>
    /// A regex used to validate the exception message and extract the table and field names.
    /// </summary>
    [GeneratedRegex(@"^Cannot insert duplicate key row in object '(?<schema>[^\.]+)\.(?<tablename>[^']+)' with unique index '(?<indexname>[^']+)", RegexOptions.ExplicitCapture)]
    private static partial Regex ErrorMessageRegex();

    #endregion private methods
}