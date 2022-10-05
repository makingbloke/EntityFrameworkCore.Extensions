// Copyright ©2021-2022 Mike King.
// This file is licensed to you under the MIT license.
// See the License.txt file in the solution root for more information.

using DotDoc.EntityFrameworkCore.Extensions.ExceptionHandlers;
using DotDoc.EntityFrameworkCore.Extensions.Exceptions;
using DotDoc.EntityFrameworkCore.Extensions.Model;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace DotDoc.EntityFrameworkCore.Extensions.Interceptors;

/// <summary>
/// Interceptor for catching and raising unique constraint exceptions.
/// </summary>
public class UniqueConstraintInterceptor : SaveChangesInterceptor
{
    private const string UniqueConstraintExceptionText = "Unique constraint exception";

    /// <inheritdoc />
    public override void SaveChangesFailed(DbContextErrorEventData eventData)
    {
        UniqueConstraintExceptionHandlerBase exceptionHandler = UniqueConstraintExceptionHandlerBase.Create(eventData.Context);
        UniqueConstraintDetails details = exceptionHandler.GetUniqueConstraintDetails(eventData.Context, eventData.Exception);

        if (details != null)
        {
            throw new UniqueConstraintException(UniqueConstraintExceptionText, eventData.Exception, details);
        }

        base.SaveChangesFailed(eventData);
    }

    /// <inheritdoc />
    public override async Task SaveChangesFailedAsync(DbContextErrorEventData eventData, CancellationToken cancellationToken = default)
    {
        UniqueConstraintExceptionHandlerBase exceptionHandler = UniqueConstraintExceptionHandlerBase.Create(eventData.Context);
        UniqueConstraintDetails details = await exceptionHandler.GetUniqueConstraintDetailsAsync(eventData.Context, eventData.Exception, cancellationToken).ConfigureAwait(false);

        if (details != null)
        {
            throw new UniqueConstraintException(UniqueConstraintExceptionText, eventData.Exception, details);
        }

        await base.SaveChangesFailedAsync(eventData, cancellationToken).ConfigureAwait(false);
    }
}
