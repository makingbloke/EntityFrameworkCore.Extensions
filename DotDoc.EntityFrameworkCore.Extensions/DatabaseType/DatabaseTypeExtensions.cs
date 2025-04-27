// Copyright ©2021-2025 Mike King.
// This file is licensed to you under the MIT license.
// See the License.txt file in the solution root for more information.

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

namespace DotDoc.EntityFrameworkCore.Extensions.DatabaseType;

/// <summary>
/// Entity Framework Core Database Type Exensions.
/// </summary>
public static class DatabaseTypeExtensions
{
    #region public methods

    /// <summary>
    /// Gets the type of database in use from a <see cref="DatabaseFacade"/>.
    /// </summary>
    /// <param name="databaseFacade">The <see cref="DatabaseFacade"/> for the context.</param>
    /// <returns><see cref="string"/> with the database type or <see langword="null"/> if none recognised.</returns>
    public static string GetDatabaseType(this DatabaseFacade databaseFacade)
    {
        ArgumentNullException.ThrowIfNull(databaseFacade);

        string databaseType;

        if (databaseFacade.IsSqlite())
        {
            databaseType = DatabaseTypes.Sqlite;
        }
        else if (databaseFacade.IsSqlServer())
        {
            databaseType = DatabaseTypes.SqlServer;
        }
        else
        {
            throw new InvalidOperationException("Unsupported database provider");
        }

        return databaseType;
    }

    /// <summary>
    /// Gets the type of database in use from a <see cref="MigrationBuilder"/>.
    /// </summary>
    /// <param name="migrationBuilder">The <see cref="MigrationBuilder"/> for the migration.</param>
    /// <returns><see cref="string"/> with the database type or <see langword="null"/> if none recognised.</returns>
    public static string GetDatabaseType(this MigrationBuilder migrationBuilder)
    {
        ArgumentNullException.ThrowIfNull(migrationBuilder);

        string databaseType;

        if (migrationBuilder.IsSqlite())
        {
            databaseType = DatabaseTypes.Sqlite;
        }
        else if (migrationBuilder.IsSqlServer())
        {
            databaseType = DatabaseTypes.SqlServer;
        }
        else
        {
            throw new InvalidOperationException("Unsupported database provider");
        }

        return databaseType;
    }

    #endregion public methods
}
