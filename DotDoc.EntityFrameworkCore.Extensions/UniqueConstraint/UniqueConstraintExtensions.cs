// Copyright ©2021-2025 Mike King.
// This file is licensed to you under the MIT license.
// See the License.txt file in the solution root for more information.

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace DotDoc.EntityFrameworkCore.Extensions.UniqueConstraint;

/// <summary>
/// Entity Framework Core Unique Constraint Exensions.
/// </summary>
public static class UniqueConstraintExtensions
{
    #region public methods

    /// <summary>
    /// Use the Unique Constraint Extensions.
    /// </summary>
    /// <param name="optionsBuilder">The builder being used to configure the context.</param>
    /// <returns>The same builder instance so multiple calls can be chained.</returns>
    public static DbContextOptionsBuilder UseUniqueConstraintInterceptor(this DbContextOptionsBuilder optionsBuilder)
    {
        ArgumentNullException.ThrowIfNull(optionsBuilder);

        return optionsBuilder.AddInterceptors(UniqueConstraintSaveChangesInterceptor.Instance);
    }

    /// <summary>
    /// Get the details of an unique constraint from an exception.
    /// </summary>
    /// <param name="databaseFacade">The <see cref="DatabaseFacade"/>.</param>
    /// <param name="e">The exception to extract the unique constraint details from.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
    /// <returns>An instance of <see cref="UniqueConstraintDetails"/>.</returns>
    public static async Task<UniqueConstraintDetails?> GetUniqueConstraintDetailsAsync(this DatabaseFacade databaseFacade, Exception e, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(databaseFacade);
        ArgumentNullException.ThrowIfNull(e);

        UniqueConstraintExceptionProcessorBase exceptionProcessor = UniqueConstraintExceptionProcessorBase.Create(databaseFacade);
        UniqueConstraintDetails? details = await exceptionProcessor.GetUniqueConstraintDetailsAsync(databaseFacade, e, cancellationToken).ConfigureAwait(false);

        return details;
    }

    #endregion public methods
}
