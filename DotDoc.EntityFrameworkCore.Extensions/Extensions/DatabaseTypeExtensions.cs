// Copyright ©2021-2025 Mike King.
// This file is licensed to you under the MIT license.
// See the License.txt file in the solution root for more information.

using DotDoc.EntityFrameworkCore.Extensions.Constants;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

namespace DotDoc.EntityFrameworkCore.Extensions.Extensions;

/// <summary>
/// Entity Framework Core Database Type Exensions.
/// </summary>
public static class DatabaseTypeExtensions
{
    #region public GetDatabaseType methods

    /// <summary>
    /// Gets the type of database in use from a <see cref="DatabaseFacade"/>.
    /// </summary>
    /// <param name="databaseFacade">The <see cref="DatabaseFacade"/> for the context.</param>
    /// <returns><see cref="string"/> with the database type or <see langword="null"/> if none recognised.</returns>
    public static string GetDatabaseType(this DatabaseFacade databaseFacade)
    {
        ArgumentNullException.ThrowIfNull(databaseFacade);

        string databaseType = GetDatabaseType(databaseFacade.ProviderName);
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

        string databaseType = GetDatabaseType(migrationBuilder.ActiveProvider);
        return databaseType;
    }

    /// <summary>
    /// Gets the type of database in use from a provider name.
    /// </summary>
    /// <param name="providerName">Name of database provider.</param>
    /// <returns>A <see cref="string"/> with the database type.</returns>
    public static string GetDatabaseType(string? providerName)
    {
        string databaseType = providerName switch
        {
            "Microsoft.EntityFrameworkCore.Sqlite" => DatabaseType.Sqlite,
            "Microsoft.EntityFrameworkCore.SqlServer" => DatabaseType.SqlServer,
            _ => throw new InvalidOperationException("Unsupported database type")
        };

        return databaseType;
    }

    #endregion public GetDatabaseType methods
}
