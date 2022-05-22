// Copyright ©2021-2022 Mike King.
// This file is licensed to you under the MIT license.
// See the License.txt file in the solution root for more information.

using DotDoc.EntityFrameworkCore.Extensions.Constants;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

namespace DotDoc.EntityFrameworkCore.Extensions;

/// <summary>
/// Entity Framework Core Database Type Exensions.
/// </summary>
public static class DatabaseTypeExtensions
{
    /// <summary>
    /// Returns the type of database in use by the <see cref="DatabaseFacade"/>.
    /// </summary>
    /// <param name="databaseFacade">The <see cref="DatabaseFacade"/> for the context.</param>
    /// <returns><see cref="DatabaseType"/>.</returns>
    public static DatabaseType GetDatabaseType(this DatabaseFacade databaseFacade) =>
        LookupDatabaseType(databaseFacade.ProviderName);

    /// <summary>
    /// Returns the type of database in use by the <see cref="MigrationBuilder"/>.
    /// </summary>
    /// <param name="migrationBuilder">The <see cref="MigrationBuilder"/> for the migration.</param>
    /// <returns><see cref="DatabaseType"/>.</returns>
    public static DatabaseType GetDatabaseType(this MigrationBuilder migrationBuilder) =>
        LookupDatabaseType(migrationBuilder.ActiveProvider);

    /// <summary>
    /// Looks up the database type for a database provider name.
    /// </summary>
    /// <param name="providerName">Name of database provider.</param>
    /// <returns><see cref="DatabaseType"/>.</returns>
    private static DatabaseType LookupDatabaseType(string providerName) =>
        providerName switch
        {
            "Microsoft.EntityFrameworkCore.Sqlite" => DatabaseType.Sqlite,
            "Microsoft.EntityFrameworkCore.SqlServer" => DatabaseType.SqlServer,
            _ => DatabaseType.Unknown
        };
}
