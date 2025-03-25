// Copyright ©2021-2025 Mike King.
// This file is licensed to you under the MIT license.
// See the License.txt file in the solution root for more information.

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;

namespace DotDoc.EntityFrameworkCore.Extensions.Utilities;

/// <summary>
/// Get Context Extensions.
/// </summary>
/// <remarks>This is only needed internally, usually the <see cref="DatabaseFacade"/> is accessed via the <see cref="DbContext"/>.</remarks>
internal static class GetContextExtensions
{
    #region public methods

    /// <summary>
    /// Gets the DbContext object that is used by the specified <see cref="DatabaseFacade"/>.
    /// </summary>
    /// <param name="databaseFacade">The <see cref="DatabaseFacade"/>.</param>
    /// <returns><see cref="DbContext"/>.</returns>
    public static DbContext GetContext(this DatabaseFacade databaseFacade)
    {
        DbContext context = ((IDatabaseFacadeDependenciesAccessor)databaseFacade).Context;
        return context;
    }

    #endregion public methods
}
