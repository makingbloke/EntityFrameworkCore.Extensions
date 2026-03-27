// Copyright ©2021-2026 Mike King.
// This file is licensed to you under the MIT license.
// See the License.txt file in the solution root for more information.

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Sqlite.Infrastructure.Internal;
using Microsoft.EntityFrameworkCore.SqlServer.Infrastructure.Internal;

namespace DotDoc.EntityFrameworkCore.Extensions.DatabaseType;

/// <summary>
/// Database Type Extensions.
/// </summary>
public static class DatabaseTypeExtensions
{
    #region private constants

    /// <summary>
    /// SQLite provider name.
    /// </summary>
    private const string SqliteProviderName = "Microsoft.EntityFrameworkCore.Sqlite";

    /// <summary>
    /// SQL Server provider name.
    /// </summary>
    private const string SqlServerProviderName = "Microsoft.EntityFrameworkCore.SqlServer";

    #endregion private constants

    #region public methods

    /// <summary>
    /// Gets the type of database in use from a <see cref="DatabaseFacade"/> object.
    /// </summary>
    /// <param name="database">The <see cref="DatabaseFacade"/> for the context.</param>
    /// <returns><see cref="string"/> with the database type or <see langword="null"/> if none recognised.</returns>
    public static string? GetDatabaseType(this DatabaseFacade database)
    {
        ArgumentNullException.ThrowIfNull(database);

        string? databaseType = database.ProviderName switch
        {
            SqliteProviderName => DatabaseTypes.Sqlite,
            SqlServerProviderName => DatabaseTypes.SqlServer,
            _ => null
        };

        return databaseType;
    }

    /// <summary>
    /// Gets the type of database in use from a <see cref="MigrationBuilder"/> object.
    /// </summary>
    /// <param name="migrationBuilder">A builder used to create or modify options for migration operations.</param>
    /// <returns><see cref="string"/> with the database type or <see langword="null"/> if none recognised.</returns>
    public static string? GetDatabaseType(this MigrationBuilder migrationBuilder)
    {
        ArgumentNullException.ThrowIfNull(migrationBuilder);

        string? databaseType = migrationBuilder.ActiveProvider switch
        {
            SqliteProviderName => DatabaseTypes.Sqlite,
            SqlServerProviderName => DatabaseTypes.SqlServer,
            _ => null
        };

        return databaseType;
    }

    /// <summary>
    /// Gets the type of database in use from a <see cref="DbContextOptions"/> object.
    /// </summary>
    /// <param name="options">An options object for this context.</param>
    /// <returns><see cref="string"/> with the database type or <see langword="null"/> if none recognised.</returns>
    public static string? GetDatabaseType(this DbContextOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);

        string? databaseType = options.Extensions
            .Select(e => e.GetType().Name switch
            {
                nameof(SqliteOptionsExtension) => DatabaseTypes.Sqlite,
                nameof(SqlServerOptionsExtension) => DatabaseTypes.SqlServer,
                _ => null
            })
            .FirstOrDefault(t => t is not null);

        return databaseType;
    }

    #endregion public methods
}
