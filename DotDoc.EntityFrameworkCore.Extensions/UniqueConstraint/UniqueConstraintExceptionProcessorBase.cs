// Copyright ©2021-2025 Mike King.
// This file is licensed to you under the MIT license.
// See the License.txt file in the solution root for more information.

using DotDoc.EntityFrameworkCore.Extensions.DatabaseType;
using DotDoc.EntityFrameworkCore.Extensions.ExceptionProcessors;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace DotDoc.EntityFrameworkCore.Extensions.UniqueConstraint;

/// <summary>
/// Unique Constraint Exception Processor.
/// </summary>
internal abstract class UniqueConstraintExceptionProcessorBase
{
    #region public methods

    /// <summary>
    /// Create an instance of a Unique Constraint Exception Processor.
    /// </summary>
    /// <param name="databaseFacade">The <see cref="DatabaseFacade"/>.</param>
    /// <returns>An instance of the correct Unique Constraint Exception Processor.</returns>
    internal static UniqueConstraintExceptionProcessorBase Create(DatabaseFacade databaseFacade)
    {
        UniqueConstraintExceptionProcessorBase exceptionProcessor = databaseFacade.GetDatabaseType() switch
        {
            DatabaseTypes.Sqlite => new SqliteUniqueConstraintExceptionProcessor(),
            DatabaseTypes.SqlServer => new SqlServerUniqueConstraintExceptionProcessor(),
            _ => throw new InvalidOperationException("Unsupported database type")
        };

        return exceptionProcessor;
    }

    /// <summary>
    /// Get the details of a unique constraint from an exception.
    /// </summary>
    /// <param name="databaseFacade">The <see cref="DatabaseFacade"/>.</param>
    /// <param name="e">The exception to extract the unique constraint details from.</param>
    /// <returns>An instance of <see cref="UniqueConstraintDetails"/>.</returns>
    internal abstract UniqueConstraintDetails? GetUniqueConstraintDetails(DatabaseFacade databaseFacade, Exception e);

    /// <summary>
    /// Get the details of a unique constraint from an exception.
    /// </summary>
    /// <param name="databaseFacade">The <see cref="DatabaseFacade"/>.</param>
    /// <param name="e">The exception to extract the unique constraint details from.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
    /// <returns>An instance of <see cref="UniqueConstraintDetails"/>.</returns>
    internal abstract Task<UniqueConstraintDetails?> GetUniqueConstraintDetailsAsync(DatabaseFacade databaseFacade, Exception e, CancellationToken cancellationToken = default);

    #endregion public methods
}
