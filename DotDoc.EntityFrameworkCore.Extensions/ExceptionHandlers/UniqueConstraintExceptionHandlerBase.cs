// Copyright ©2021-2022 Mike King.
// This file is licensed to you under the MIT license.
// See the License.txt file in the solution root for more information.

using DotDoc.EntityFrameworkCore.Extensions.Constants;
using DotDoc.EntityFrameworkCore.Extensions.Extensions;
using DotDoc.EntityFrameworkCore.Extensions.Model;
using Microsoft.EntityFrameworkCore;

namespace DotDoc.EntityFrameworkCore.Extensions.ExceptionHandlers;

/// <summary>
/// Unique Constraint Exception Handler.
/// </summary>
internal abstract class UniqueConstraintExceptionHandlerBase
{
    /// <summary>
    /// Create an instance of a unique constraint exception handler.
    /// </summary>
    /// <param name="context">The database context.</param>
    /// <returns>An instance of the correct unique constraint handler for the context.</returns>
    public static UniqueConstraintExceptionHandlerBase Create(DbContext context)
    {
        return context.Database.GetDatabaseType() switch
        {
            DatabaseType.Sqlite => new SqliteUniqueConstraintExceptionHandler(),
            DatabaseType.SqlServer => new SqlServerUniqueConstraintExceptionHandler(),
            _ => throw new InvalidOperationException("Unsupported database type")
        };
    }

    /// <summary>
    /// Get the details of a unique constraint from an exception.
    /// </summary>
    /// <param name="context">The database context.</param>
    /// <param name="e">The exception to extract the unique constraint details from.</param>
    /// <returns>An instance of <see cref="UniqueConstraintDetails"/>.</returns>
    public abstract UniqueConstraintDetails GetUniqueConstraintDetails(DbContext context, Exception e);

    /// <summary>
    /// Get the details of a unique constraint from an exception.
    /// </summary>
    /// <param name="context">The database context.</param>
    /// <param name="e">The exception to extract the unique constraint details from.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
    /// <returns>An instance of <see cref="UniqueConstraintDetails"/>.</returns>
    public abstract Task<UniqueConstraintDetails> GetUniqueConstraintDetailsAsync(DbContext context, Exception e, CancellationToken cancellationToken = default);
}
