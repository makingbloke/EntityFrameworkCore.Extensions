// Copyright ©2021-2026 Mike King.
// This file is licensed to you under the MIT license.
// See the License.txt file in the solution root for more information.

using Microsoft.EntityFrameworkCore.Diagnostics;

namespace DotDoc.EntityFrameworkCore.Extensions.UniqueConstraint;

/// <summary>
/// Unique Constraint Interceptor.
/// </summary>
internal sealed class UniqueConstraintInterceptor : SaveChangesInterceptor
{
    #region internal fields

    /// <summary>
    /// Singleton instance of this interceptor.
    /// </summary>
    internal static readonly UniqueConstraintInterceptor Instance = new();

    #endregion internal fields

    #region private constructors

    /// <summary>
    /// Initializes a new instance of the <see cref="UniqueConstraintInterceptor"/> class.
    /// </summary>
    private UniqueConstraintInterceptor()
    {
    }

    #endregion private constructors

    #region public methods

    /// <inheritdoc />
    public override void SaveChangesFailed(DbContextErrorEventData eventData)
    {
        if (eventData.Context is not null)
        {
            UniqueConstraintDetails? details = eventData.Context.Database.GetUniqueConstraintDetailsAsync(eventData.Exception).Result;

            if (details is not null)
            {
                throw new UniqueConstraintException(eventData.Exception, details);
            }
        }

        base.SaveChangesFailed(eventData);
    }

    /// <inheritdoc />
    public override async Task SaveChangesFailedAsync(DbContextErrorEventData eventData, CancellationToken cancellationToken = default)
    {
        if (eventData.Context is not null)
        {
            UniqueConstraintDetails? details = await eventData.Context.Database.GetUniqueConstraintDetailsAsync(eventData.Exception, cancellationToken).ConfigureAwait(false);

            if (details is not null)
            {
                throw new UniqueConstraintException(eventData.Exception, details);
            }
        }

        await base.SaveChangesFailedAsync(eventData, cancellationToken).ConfigureAwait(false);
    }

    #endregion public methods
}
