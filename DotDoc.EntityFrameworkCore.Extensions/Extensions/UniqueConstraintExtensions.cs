// Copyright ©2021-2025 Mike King.
// This file is licensed to you under the MIT license.
// See the License.txt file in the solution root for more information.

using DotDoc.EntityFrameworkCore.Extensions.ExceptionProcessors;
using DotDoc.EntityFrameworkCore.Extensions.Interceptors;
using DotDoc.EntityFrameworkCore.Extensions.Model;
using Microsoft.EntityFrameworkCore;

namespace DotDoc.EntityFrameworkCore.Extensions.Extensions;

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

    #endregion public methods

    #region public Unique Constraint methods

    /// <summary>
    /// Get the details of an unique constraint from an exception.
    /// </summary>
    /// <param name="context">The database context.</param>
    /// <param name="e">The exception to extract the unique constraint details from.</param>
    /// <returns>An instance of <see cref="UniqueConstraintDetails"/>.</returns>
    public static UniqueConstraintDetails GetUniqueConstraintDetails(this DbContext context, Exception e)
    {
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(e);

        UniqueConstraintExceptionProcessorBase exceptionProcessor = UniqueConstraintExceptionProcessorBase.Create(context);
        return exceptionProcessor.GetUniqueConstraintDetails(context, e);
    }

    /// <summary>
    /// Get the details of an unique constraint from an exception.
    /// </summary>
    /// <param name="context">The database context.</param>
    /// <param name="e">The exception to extract the unique constraint details from.</param>
    /// <returns>An instance of <see cref="UniqueConstraintDetails"/>.</returns>
    public static Task<UniqueConstraintDetails> GetUniqueConstraintDetailsAsync(this DbContext context, Exception e)
    {
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(e);

        UniqueConstraintExceptionProcessorBase exceptionProcessor = UniqueConstraintExceptionProcessorBase.Create(context);
        return exceptionProcessor.GetUniqueConstraintDetailsAsync(context, e);
    }

    #endregion public Unique Constraint methods
}
