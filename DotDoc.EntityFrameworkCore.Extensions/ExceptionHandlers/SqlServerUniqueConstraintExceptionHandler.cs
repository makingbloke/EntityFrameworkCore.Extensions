// Copyright ©2021-2022 Mike King.
// This file is licensed to you under the MIT license.
// See the License.txt file in the solution root for more information.

using DotDoc.EntityFrameworkCore.Extensions.Extensions;
using DotDoc.EntityFrameworkCore.Extensions.Model;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using System.Data;
using System.Text.RegularExpressions;

namespace DotDoc.EntityFrameworkCore.Extensions.ExceptionHandlers;

/// <inheritdoc/>
internal class SqlServerUniqueConstraintExceptionHandler : UniqueConstraintExceptionHandlerBase
{
    /// <summary>
    /// Name of the Sqlite exception type.
    /// </summary>
    private const string ExceptionTypeName = "SqlException";

    /// <summary>
    /// The default schema used by SQL server.
    /// </summary>
    private const string DefaultSchema = "dbo";

    /// <summary>
    /// A regex used to validate the exception message and extract the table and field names.
    /// </summary>
    private static readonly Regex ErrorMessRegex = new (
        @"^Cannot insert duplicate key row in object '(?<schema>[^\.]+)\.(?<tablename>[^']+)' with unique index '(?<indexname>[^']+)",
        RegexOptions.ExplicitCapture | RegexOptions.Compiled);

    /// <inheritdoc/>
    public override UniqueConstraintDetails GetUniqueConstraintDetails(DbContext context, Exception e)
    {
        UniqueConstraintDetails details = null;

        (bool success, string schema, string tableName, string indexName) = ParseException(e);
        if (success)
        {
            details = GetUniqueConstraintDetailsFromEntityFrameWork(context, schema, tableName, indexName) ??
                        GetUniqueConstraintDetailsFromSqlServer(context, schema, tableName, indexName);
        }

        return details;
    }

    /// <inheritdoc/>
    public override async Task<UniqueConstraintDetails> GetUniqueConstraintDetailsAsync(DbContext context, Exception e, CancellationToken cancellationToken = default)
    {
        UniqueConstraintDetails details = null;

        (bool success, string schema, string tableName, string indexName) = ParseException(e);
        if (success)
        {
            details = GetUniqueConstraintDetailsFromEntityFrameWork(context, schema, tableName, indexName) ??
                        await GetUniqueConstraintDetailsFromSqlServerAsync(context, schema, tableName, indexName, cancellationToken).ConfigureAwait(false);
        }

        return details;
    }

    /// <summary>
    /// Validate and parse the supplied exception to extract the constraint details.
    /// </summary>
    /// <param name="e">The exception to process.</param>
    /// <returns>
    /// A tuple containing the following information:
    /// <list type="bullet">
    /// <item>A <see cref="bool"/> Success flag.</item>
    /// <item>A <see cref="string"/> schema.</item>
    /// <item>A <see cref="string"/> table name.</item>
    /// <item>A <see cref="string"/> index name.</item>
    /// </list>
    /// </returns>
    private static (bool, string, string, string) ParseException(Exception e)
    {
        bool success = false;
        string schema = null;
        string tableName = null;
        string indexName = null;

        if (e is DbUpdateException)
        {
            e = e.GetBaseException();
        }

        string exceptionTypeName = e?.GetType().Name;
        if (exceptionTypeName == ExceptionTypeName)
        {
            Match match = ErrorMessRegex.Match(e.Message);

            if (match.Success)
            {
                success = true;
                schema = match.Groups["schema"].Value;
                tableName = match.Groups["tablename"].Value;
                indexName = match.Groups["indexname"].Value;
            }
        }

        return (success, schema, tableName, indexName);
    }

    /// <summary>
    /// Get the details of a unique constraint from EF core.
    /// </summary>
    /// <param name="context">The database context.</param>
    /// <param name="schema">The schema.</param>
    /// <param name="tableName">The table name.</param>
    /// <param name="indexName">The index name.</param>
    /// <returns>An instance <see cref="UniqueConstraintDetails"/> if the table can be found in EF Core.</returns>
    private static UniqueConstraintDetails GetUniqueConstraintDetailsFromEntityFrameWork(DbContext context, string schema, string tableName, string indexName)
    {
        UniqueConstraintDetails details = null;

        if (schema == DefaultSchema)
        {
            schema = null;
        }

        IEntityType entityType = context.Model.GetEntityTypes().FirstOrDefault(et => schema == et.GetSchema() && tableName == et.GetTableName());
        if (entityType != null)
        {
            string entityTableName = entityType.ShortName();

            StoreObjectIdentifier storeObjectIdentifier = StoreObjectIdentifier.Table(tableName, schema);

            IIndex index = entityType.GetIndexes().FirstOrDefault(i => indexName == i.GetDatabaseName(storeObjectIdentifier));
            if (index != null)
            {
                List<string> fieldNames = index.Properties
                    .Select(p => p.Name)
                    .ToList();

                details = new (schema, entityTableName, fieldNames);
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
    private static UniqueConstraintDetails GetUniqueConstraintDetailsFromSqlServer(DbContext context, string schema, string tableName, string indexName)
    {
        FormattableString sql = BuildSql(schema, tableName, indexName);

        DataTable table = context.Database.ExecuteQueryInterpolated(sql);

        UniqueConstraintDetails details = CreateUniqueConstraintDetails(schema, tableName, table);
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
    private static async Task<UniqueConstraintDetails> GetUniqueConstraintDetailsFromSqlServerAsync(DbContext context, string schema, string tableName, string indexName, CancellationToken cancellationToken)
    {
        FormattableString sql = BuildSql(schema, tableName, indexName);

        DataTable table = await context.Database.ExecuteQueryInterpolatedAsync(sql, cancellationToken).ConfigureAwait(false);

        UniqueConstraintDetails details = CreateUniqueConstraintDetails(schema, tableName, table);
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
WHERE si.object_id = OBJECT_ID({$"{schema}.{tableName}"}) AND si.name = {indexName}
AND sic.object_id = si.object_id AND sic.index_id = si.index_id
AND sc.object_id = sic.object_id AND sc.column_id = sic.column_id";

        return sql;
    }

    /// <summary>
    /// Create an instance of <see cref="UniqueConstraintDetails"/>.
    /// </summary>
    /// <param name="schema">The schema.</param>
    /// <param name="tableName">The table name.</param>
    /// <param name="table">A <see cref="DataTable"/> containing the field name query results.</param>
    /// <returns>An instance of <see cref="UniqueConstraintDetails"/>.</returns>
    private static UniqueConstraintDetails CreateUniqueConstraintDetails(string schema, string tableName, DataTable table)
    {
        UniqueConstraintDetails details = null;

        if (table.Rows.Count > 0)
        {
            if (schema == DefaultSchema)
            {
                schema = null;
            }

            List<string> fieldNames = table.Rows
                .Cast<DataRow>()
                .Select(r => r["name"].ToString())
                .ToList();

            details = new (schema, tableName, fieldNames);
        }

        return details;
    }
}