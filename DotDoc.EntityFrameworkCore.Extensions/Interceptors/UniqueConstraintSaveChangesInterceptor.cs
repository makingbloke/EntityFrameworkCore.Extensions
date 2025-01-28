// Copyright ©2021-2025 Mike King.
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
internal sealed class UniqueConstraintSaveChangesInterceptor : SaveChangesInterceptor
{
    #region internal fields

    /// <summary>
    /// Singleton instance of this interceptor.
    /// </summary>
    internal static readonly UniqueConstraintSaveChangesInterceptor Instance = new();

    #endregion internal fields

    #region private constructors

    /// <summary>
    /// Initializes a new instance of the <see cref="UniqueConstraintSaveChangesInterceptor"/> class.
    /// </summary>
    private UniqueConstraintSaveChangesInterceptor()
    {
    }

    #endregion private constructors

    #region public methods

    /// <inheritdoc />
    public override void SaveChangesFailed(DbContextErrorEventData eventData)
    {
        ArgumentNullException.ThrowIfNull(eventData);

        if (eventData.Context == null)
        {
            throw new InvalidOperationException("Context not initialised");
        }

        UniqueConstraintExceptionProcessorBase exceptionProcessor = UniqueConstraintExceptionProcessorBase.Create(eventData.Context);
        UniqueConstraintDetails? details = exceptionProcessor.GetUniqueConstraintDetails(eventData.Context!, eventData.Exception);

        if (details != null)
        {
            throw new UniqueConstraintException(eventData.Exception, details);
        }

        base.SaveChangesFailed(eventData);
    }

    /// <inheritdoc />
    public override async Task SaveChangesFailedAsync(DbContextErrorEventData eventData, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(eventData);

        if (eventData.Context == null)
        {
            throw new InvalidOperationException("Context not initialised");
        }

        UniqueConstraintExceptionProcessorBase exceptionProcessor = UniqueConstraintExceptionProcessorBase.Create(eventData.Context);
        UniqueConstraintDetails? details = await exceptionProcessor.GetUniqueConstraintDetailsAsync(eventData.Context, eventData.Exception, cancellationToken).ConfigureAwait(false);

        if (details != null)
        {
            throw new UniqueConstraintException(eventData.Exception, details);
        }

        await base.SaveChangesFailedAsync(eventData, cancellationToken).ConfigureAwait(false);
    }

    #endregion public methods
}
