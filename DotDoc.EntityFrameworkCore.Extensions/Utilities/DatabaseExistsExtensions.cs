// Copyright ©2021-2026 Mike King.
// This file is licensed to you under the MIT license.
// See the License.txt file in the solution root for more information.

using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;

namespace DotDoc.EntityFrameworkCore.Extensions.Utilities;

/// <summary>
/// Database Exists Extensions.
/// </summary>
public static class DatabaseExistsExtensions
{
    #region public methods

    /// <summary>
    /// Check if database exists.
    /// </summary>
    /// <param name="database">The <see cref="DatabaseFacade"/> for the context.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
    /// <returns>A <see langword="bool"/> indicating if the database exists.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="database"/> is <see langword="null"/>.</exception>
    public static async Task<bool> DoesDatabaseExistAsync(this DatabaseFacade database, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(database);

        bool databaseExists = await database.GetService<IRelationalDatabaseCreator>().ExistsAsync(cancellationToken).ConfigureAwait(false);
        return databaseExists;
    }

    #endregion public methods
}
