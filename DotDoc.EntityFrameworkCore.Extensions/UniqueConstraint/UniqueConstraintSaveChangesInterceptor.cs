// Copyright ©2021-2025 Mike King.
// This file is licensed to you under the MIT license.
// See the License.txt file in the solution root for more information.

using Microsoft.EntityFrameworkCore.Diagnostics;
using System.Threading;

namespace DotDoc.EntityFrameworkCore.Extensions.UniqueConstraint;

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
        UniqueConstraintExceptionProcessorBase exceptionProcessor = UniqueConstraintExceptionProcessorBase.Create(eventData.Context!.Database);
        UniqueConstraintDetails? details = exceptionProcessor.GetUniqueConstraintDetailsAsync(eventData.Context.Database, eventData.Exception).Result;

        if (details != null)
        {
            throw new UniqueConstraintException(eventData.Exception, details);
        }

        base.SaveChangesFailed(eventData);
    }

    /// <inheritdoc />
    public override async Task SaveChangesFailedAsync(DbContextErrorEventData eventData, CancellationToken cancellationToken = default)
    {
        UniqueConstraintExceptionProcessorBase exceptionProcessor = UniqueConstraintExceptionProcessorBase.Create(eventData.Context!.Database);
        UniqueConstraintDetails? details = await exceptionProcessor.GetUniqueConstraintDetailsAsync(eventData.Context.Database, eventData.Exception, cancellationToken).ConfigureAwait(false);

        if (details != null)
        {
            throw new UniqueConstraintException(eventData.Exception, details);
        }

        await base.SaveChangesFailedAsync(eventData, cancellationToken).ConfigureAwait(false);
    }

    #endregion public methods
}
