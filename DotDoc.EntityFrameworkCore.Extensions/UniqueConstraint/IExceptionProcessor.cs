using Microsoft.EntityFrameworkCore.Infrastructure;

namespace DotDoc.EntityFrameworkCore.Extensions.UniqueConstraint;

/// <summary>
/// Exception Processor.
/// </summary>
internal interface IExceptionProcessor
{
    #region public methods

    /// <summary>
    /// Get the details of a unique constraint from an exception.
    /// </summary>
    /// <param name="database">The <see cref="DatabaseFacade"/>.</param>
    /// <param name="e">The exception to extract the unique constraint details from.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
    /// <returns>If the exception is a unique constraint exception then returns the exception details in <see cref="UniqueConstraintDetails"/> else, <see langword="null"/>.</returns>
    public Task<UniqueConstraintDetails?> GetUniqueConstraintDetailsAsync(DatabaseFacade database, Exception e, CancellationToken cancellationToken = default);

    #endregion public methods
}
