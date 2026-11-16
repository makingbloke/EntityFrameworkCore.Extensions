// Copyright ©2021-2026 Mike King.
// This file is licensed to you under the MIT license.
// See the License.txt file in the solution root for more information.

using DotDoc.EntityFrameworkCore.Extensions.DatabaseType;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.ValueGeneration;
using UUIDNext;

namespace DotDoc.EntityFrameworkCore.Extensions.Utilities;

/// <summary>
/// Guid Extensions.
/// </summary>
public static class GuidExtensions
{
    #region public methods

    /// <summary>
    /// Create a Guid that is suitable for use as a database key value.
    /// </summary>
    /// <param name="database">The <see cref="DatabaseFacade"/> for the context.</param>
    /// <returns>A <see cref="Guid"/>.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="database"/> is <see langword="null"/>.</exception>
    /// <exception cref="UnsupportedDatabaseTypeException">The database provider is not SQLite or SQL Server.</exception>
    /// <remarks>
    /// SQL Server can store GUIDs but because of how they are handled date stamped GUIDs
    /// are not sorted in the correct order.
    /// This method uses a third party library (UUIDNext) to create GUIDS that are
    /// compatible with the supported database types.
    /// (The inbuilt EF Core <see cref="SequentialGuidValueGenerator"/>> class creates
    /// GUIDS for SQL Server that look correct but with have an incorrect version number).
    /// </remarks>
    public static Guid CreateGuid(this DatabaseFacade database)
    {
        ArgumentNullException.ThrowIfNull(database);

        string? databaseType = database.GetDatabaseType();

        Guid guid = databaseType switch
        {
            DatabaseTypes.Sqlite => Uuid.NewDatabaseFriendly(UUIDNext.Database.SQLite),
            DatabaseTypes.SqlServer => Uuid.NewDatabaseFriendly(UUIDNext.Database.SqlServer),
            _ => throw new UnsupportedDatabaseTypeException(nameof(database)),
        };

        return guid;
    }

    #endregion public methods
}
