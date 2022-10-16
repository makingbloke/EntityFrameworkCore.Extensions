// Copyright ©2021-2022 Mike King.
// This file is licensed to you under the MIT license.
// See the License.txt file in the solution root for more information.

using DotDoc.EntityFrameworkCore.Extensions.Model;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using System.Data;
using System.Text.RegularExpressions;

namespace DotDoc.EntityFrameworkCore.Extensions.ExceptionProcessors;

/// <inheritdoc/>
internal class SqliteUniqueConstraintExceptionProcessor : UniqueConstraintExceptionProcessorBase
{
    /// <summary>
    /// Name of the Sqlite exception type.
    /// </summary>
    private const string _exceptionTypeName = "SqliteException";

    /// <summary>
    /// A regex used to validate the exception message and extract the table and field names.
    /// </summary>
    private static readonly Regex _errorMessRegex = new (
        @"^SQLite Error 19: 'UNIQUE constraint failed: ((?<tablename>[^\.]+)\.(?<fieldname>[^,]+)((, )|('\.$)))+$",
        RegexOptions.ExplicitCapture | RegexOptions.Compiled);

    /// <inheritdoc/>
    public override UniqueConstraintDetails GetUniqueConstraintDetails(DbContext context, Exception e)
    {
        UniqueConstraintDetails details = null;

        (bool success, string tableName, List<string> fieldNames) = ParseException(e);
        if (success)
        {
            details = GetUniqueConstraintDetailsFromEntityFrameWork(context, tableName, fieldNames)
                        ?? new (null, tableName, fieldNames);
        }

        return details;
    }

    /// <inheritdoc/>
    public override Task<UniqueConstraintDetails> GetUniqueConstraintDetailsAsync(DbContext context, Exception e, CancellationToken cancellationToken = default)
    {
        UniqueConstraintDetails details = this.GetUniqueConstraintDetails(context, e);
        return Task.FromResult(details);
    }

    /// <summary>
    /// Validate and parse the supplied exception to extract the constraint details.
    /// </summary>
    /// <param name="e">The exception to process.</param>
    /// <returns>
    /// A tuple containing the following information:
    /// <list type="bullet">
    /// <item>A <see cref="bool"/> Success flag.</item>
    /// <item>A <see cref="string"/> table name.</item>
    /// <item>A <see cref="List{String}"/> of field names.</item>
    /// </list>
    /// </returns>
    private static (bool, string, List<string>) ParseException(Exception e)
    {
        bool success = false;
        string tableName = null;
        List<string> fieldNames = null;

        if (e is DbUpdateException)
        {
            e = e.GetBaseException();
        }

        string exceptionTypeName = e?.GetType().Name;
        if (exceptionTypeName == _exceptionTypeName)
        {
            Match match = _errorMessRegex.Match(e.Message);

            if (match.Success)
            {
                success = true;
                tableName = match.Groups["tablename"].Captures[0].Value;
                fieldNames = match.Groups["fieldname"].Captures.Select(c => c.Value).ToList();
            }
        }

        return (success, tableName, fieldNames);
    }

    /// <summary>
    /// Get the details of a unique constraint from EF core.
    /// </summary>
    /// <param name="context">The database context.</param>
    /// <param name="tableName">The table name.</param>
    /// <param name="fieldNames">The field names.</param>
    /// <returns>An instance <see cref="UniqueConstraintDetails"/> if the table can be found in EF Core.</returns>
    /// <remarks>If the TableName or ColumnName attributes have been used then the details will contain these values.</remarks>
    private static UniqueConstraintDetails GetUniqueConstraintDetailsFromEntityFrameWork(DbContext context, string tableName, List<string> fieldNames)
    {
        UniqueConstraintDetails details = null;

        IEntityType entityType = context.Model.GetEntityTypes().FirstOrDefault(et => tableName == et.GetTableName());
        if (entityType != null)
        {
            string entityTableName = entityType.ShortName();

            StoreObjectIdentifier storeObjectIdentifier = StoreObjectIdentifier.Table(tableName);
            IEnumerable<IProperty> properties = entityType.GetProperties();

            List<string> entityFieldNames = fieldNames
                .Select(n => properties.First(p => p.GetColumnName(storeObjectIdentifier) == n).Name)
                .ToList();

            details = new (null, entityTableName, entityFieldNames);
        }

        return details;
    }
}