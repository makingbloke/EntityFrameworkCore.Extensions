﻿// Copyright ©2021-2025 Mike King.
// This file is licensed to you under the MIT license.
// See the License.txt file in the solution root for more information.

using DotDoc.EntityFrameworkCore.Extensions.ExceptionProcessors;
using DotDoc.EntityFrameworkCore.Extensions.Exceptions;
using DotDoc.EntityFrameworkCore.Extensions.Model;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace DotDoc.EntityFrameworkCore.Extensions.Interceptors;

/// <summary>
/// Interceptor for catching and raising unique constraint exceptions.
/// </summary>
public class UniqueConstraintSaveChangesInterceptor : SaveChangesInterceptor
{
    /// <summary>
    /// Singleton instance of this interceptor.
    /// </summary>
    internal static readonly UniqueConstraintSaveChangesInterceptor Instance = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="UniqueConstraintSaveChangesInterceptor"/> class.
    /// </summary>
    private UniqueConstraintSaveChangesInterceptor()
    {
    }

    /// <inheritdoc />
    public override void SaveChangesFailed(DbContextErrorEventData eventData)
    {
        UniqueConstraintExceptionProcessorBase exceptionProcessor = UniqueConstraintExceptionProcessorBase.Create(eventData.Context);
        UniqueConstraintDetails details = exceptionProcessor.GetUniqueConstraintDetails(eventData.Context, eventData.Exception);

        if (details != null)
        {
            throw new UniqueConstraintException(eventData.Exception, details);
        }

        base.SaveChangesFailed(eventData);
    }

    /// <inheritdoc />
    public override async Task SaveChangesFailedAsync(DbContextErrorEventData eventData, CancellationToken cancellationToken = default)
    {
        UniqueConstraintExceptionProcessorBase exceptionProcessor = UniqueConstraintExceptionProcessorBase.Create(eventData.Context);
        UniqueConstraintDetails details = await exceptionProcessor.GetUniqueConstraintDetailsAsync(eventData.Context, eventData.Exception, cancellationToken).ConfigureAwait(false);

        if (details != null)
        {
            throw new UniqueConstraintException(eventData.Exception, details);
        }

        await base.SaveChangesFailedAsync(eventData, cancellationToken).ConfigureAwait(false);
    }
}