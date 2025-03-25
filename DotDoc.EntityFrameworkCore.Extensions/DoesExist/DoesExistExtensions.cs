// Copyright ©2021-2025 Mike King.
// This file is licensed to you under the MIT license.
// See the License.txt file in the solution root for more information.

using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;

namespace DotDoc.EntityFrameworkCore.Extensions.Extensions;

/// <summary>
/// Entity Framework Core Does Exist Exensions.
/// </summary>
public static class DoesExistExtensions
{
    #region public DoesDatabaseExist methods

    /// <summary>
    /// Check if database exists.
    /// </summary>
    /// <param name="databaseFacade">The <see cref="DatabaseFacade"/> for the context.</param>
    /// <returns>A <see langword="bool"/> indicating if the database exists.</returns>
    public static bool DoesDatabaseExist(this DatabaseFacade databaseFacade)
    {
        ArgumentNullException.ThrowIfNull(databaseFacade);

        bool databaseExists = databaseFacade.GetService<IRelationalDatabaseCreator>().Exists();
        return databaseExists;
    }

    /// <summary>
    /// Check if database exists.
    /// </summary>
    /// <param name="databaseFacade">The <see cref="DatabaseFacade"/> for the context.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
    /// <returns>A <see langword="bool"/> indicating if the database exists.</returns>
    public static async Task<bool> DoesDatabaseExistAsync(this DatabaseFacade databaseFacade, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(databaseFacade);

        bool databaseExists = await databaseFacade.GetService<IRelationalDatabaseCreator>().ExistsAsync(cancellationToken).ConfigureAwait(false);
        return databaseExists;
    }

    #endregion public DoesDatabaseExist methods
}
