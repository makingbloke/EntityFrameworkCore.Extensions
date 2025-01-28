// Copyright ©2021-2025 Mike King.
// This file is licensed to you under the MIT license.
// See the License.txt file in the solution root for more information.

using DotDoc.EntityFrameworkCore.Extensions.Extensions;
using DotDoc.EntityFrameworkCore.Extensions.Model;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using System.Data;
using System.Text.RegularExpressions;

namespace DotDoc.EntityFrameworkCore.Extensions.ExceptionProcessors;

/// <inheritdoc/>
internal sealed partial class SqliteUniqueConstraintExceptionProcessor : UniqueConstraintExceptionProcessorBase
{
    #region private fields

    /// <summary>
    /// Name of the Sqlite exception type.
    /// </summary>
    private const string _exceptionTypeName = "SqliteException";

    #endregion private fields

    #region public methods

    /// <inheritdoc/>
    internal override UniqueConstraintDetails? GetUniqueConstraintDetails(DatabaseFacade databaseFacade, Exception e)
    {
        UniqueConstraintDetails? details = null;

        if (ParseException(e, out string? tableName, out List<string>? fieldNames))
        {
            DbContext context = databaseFacade.GetContext();

            details = GetUniqueConstraintDetailsFromEntityFrameWork(context, tableName!, fieldNames!)
                        ?? new(null, tableName!, fieldNames!);
        }

        return details;
    }

    /// <inheritdoc/>
    internal override Task<UniqueConstraintDetails?> GetUniqueConstraintDetailsAsync(DatabaseFacade databaseFacade, Exception e, CancellationToken cancellationToken = default)
    {
        UniqueConstraintDetails? details = this.GetUniqueConstraintDetails(databaseFacade, e);
        return Task.FromResult(details);
    }

    #endregion public methods

    #region private methods

    /// <summary>
    /// Validate and parse the supplied exception to extract the constraint details.
    /// </summary>
    /// <param name="e">The exception to process.</param>
    /// <param name="tableName">The table name (out).</param>
    /// <param name="fieldNames">The field names (out).</param>
    /// <returns>
    /// <see langword="true"> if successful else <see langword="false"/>.</see>
    /// </returns>
    private static bool ParseException(Exception e, out string? tableName, out List<string>? fieldNames)
    {
        bool success = false;
        tableName = null;
        fieldNames = null;

        if (e is DbUpdateException)
        {
            e = e.GetBaseException();
        }

        if (e != null)
        {
            string exceptionTypeName = e.GetType().Name;
            if (exceptionTypeName == _exceptionTypeName)
            {
                Match match = ErrorMessageRegex().Match(e.Message);

                if (match.Success)
                {
                    success = true;
                    tableName = match.Groups["tablename"].Captures[0].Value;
                    fieldNames = match.Groups["fieldname"].Captures.Select(c => c.Value).ToList();
                }
            }
        }

        return success;
    }

    /// <summary>
    /// Get the details of a unique constraint from EF core.
    /// </summary>
    /// <param name="context">The database context.</param>
    /// <param name="tableName">The table name.</param>
    /// <param name="fieldNames">The field names.</param>
    /// <returns>An instance <see cref="UniqueConstraintDetails"/> if the table can be found in EF Core.</returns>
    /// <remarks>If the TableName or ColumnName attributes have been used then the details will contain these values.</remarks>
    private static UniqueConstraintDetails? GetUniqueConstraintDetailsFromEntityFrameWork(DbContext context, string tableName, List<string> fieldNames)
    {
        UniqueConstraintDetails? details = null;

        IEntityType? entityType = context.Model
            .GetEntityTypes()
            .FirstOrDefault(et => et.GetSchema() == null && et.GetTableName() == tableName);

        if (entityType != null)
        {
            string entityTableName = entityType.ShortName();

            StoreObjectIdentifier storeObjectIdentifier = StoreObjectIdentifier.Table(tableName);
            IEnumerable<IProperty> properties = entityType.GetProperties();

            List<string> entityFieldNames = fieldNames
                .Select(n => properties.First(p => p.GetColumnName(storeObjectIdentifier) == n).Name)
                .ToList();

            details = new(null, entityTableName, entityFieldNames);
        }

        return details;
    }

    /// <summary>
    /// A regex used to validate the exception message and extract the table and field names.
    /// </summary>
    [GeneratedRegex(@"^SQLite Error 19: 'UNIQUE constraint failed: ((?<tablename>[^\.]+)\.(?<fieldname>[^,]+)((, )|('\.$)))+$", RegexOptions.ExplicitCapture)]
    private static partial Regex ErrorMessageRegex();

    #endregion private methods
}