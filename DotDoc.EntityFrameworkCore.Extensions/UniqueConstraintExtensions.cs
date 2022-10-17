// Copyright ©2021-2022 Mike King.
// This file is licensed to you under the MIT license.
// See the License.txt file in the solution root for more information.

using DotDoc.EntityFrameworkCore.Extensions.ExceptionProcessors;
using DotDoc.EntityFrameworkCore.Extensions.Interceptors;
using DotDoc.EntityFrameworkCore.Extensions.Model;
using Microsoft.EntityFrameworkCore;

namespace DotDoc.EntityFrameworkCore.Extensions;

/// <summary>
/// Entity Framework Core Unique Constraint Exensions.
/// </summary>
public static class UniqueConstraintExtensions
{
    /// <summary>
    /// Add the unique constraint interceptor to EF Core.
    /// </summary>
    /// <param name="optionsBuilder"><see cref="DbContextOptionsBuilder"/>.</param>
    /// <returns>The options builder <see cref="DbContextOptionsBuilder"/>.</returns>
    public static DbContextOptionsBuilder UseUniqueConstraintInterceptor(this DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.AddInterceptors(new UniqueConstraintInterceptor());
        return optionsBuilder;
    }

    /// <summary>
    /// Get the details of an unique constraint from an exception.
    /// </summary>
    /// <param name="context">The database context.</param>
    /// <param name="e">The exception to extract the unique constraint details from.</param>
    /// <returns>An instance of <see cref="UniqueConstraintDetails"/>.</returns>
    public static UniqueConstraintDetails GetUniqueConstraintDetails(this DbContext context, Exception e)
    {
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
        UniqueConstraintExceptionProcessorBase exceptionProcessor = UniqueConstraintExceptionProcessorBase.Create(context);
        return exceptionProcessor.GetUniqueConstraintDetailsAsync(context, e);
    }
}
