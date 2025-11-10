// Copyright ©2021-2025 Mike King.
// This file is licensed to you under the MIT license.
// See the License.txt file in the solution root for more information.

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Sqlite.Infrastructure.Internal;
using Microsoft.EntityFrameworkCore.SqlServer.Infrastructure.Internal;
using System.Diagnostics.CodeAnalysis;

namespace DotDoc.EntityFrameworkCore.Extensions.DatabaseType;

/// <summary>
/// Database Type Extensions.
/// </summary>
[SuppressMessage("Usage", "EF1001:Internal EF Core API usage.", Justification = "Internal APIs used by GetDatabaseType methods.")]
public static class DatabaseTypeExtensions
{
    #region public methods

    /// <summary>
    /// Gets the type of database in use from a <see cref="DatabaseFacade"/> object.
    /// </summary>
    /// <param name="database">The <see cref="DatabaseFacade"/> for the context.</param>
    /// <returns><see cref="string"/> with the database type or <see langword="null"/> if none recognised.</returns>
    public static string? GetDatabaseType(this DatabaseFacade database)
    {
        ArgumentNullException.ThrowIfNull(database);

        string? databaseType = null;

        if (database.IsSqlite())
        {
            databaseType = DatabaseTypes.Sqlite;
        }
        else if (database.IsSqlServer())
        {
            databaseType = DatabaseTypes.SqlServer;
        }

        return databaseType;
    }

    /// <summary>
    /// Gets the type of database in use from a <see cref="DbContextOptionsBuilder"/> object.
    /// </summary>
    /// <param name="optionsBuilder">A builder used to create or modify options for this context.</param>
    /// <returns><see cref="string"/> with the database type or <see langword="null"/> if none recognised.</returns>
    public static string? GetDatabaseType(this DbContextOptionsBuilder optionsBuilder)
    {
        ArgumentNullException.ThrowIfNull(optionsBuilder);

        string? databaseType = null;

        if (optionsBuilder.Options.FindExtension<SqliteOptionsExtension>() is not null)
        {
            databaseType = DatabaseTypes.Sqlite;
        }
        else if (optionsBuilder.Options.FindExtension<SqlServerOptionsExtension>() is not null)
        {
            databaseType = DatabaseTypes.SqlServer;
        }

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

        string? databaseType = null;

        if (migrationBuilder.IsSqlite())
        {
            databaseType = DatabaseTypes.Sqlite;
        }
        else if (migrationBuilder.IsSqlServer())
        {
            databaseType = DatabaseTypes.SqlServer;
        }

        return databaseType;
    }

    #endregion public methods
}
