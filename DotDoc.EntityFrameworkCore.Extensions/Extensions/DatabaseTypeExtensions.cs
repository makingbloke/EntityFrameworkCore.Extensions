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
    #region Public GetDatabaseType methods

    /// <summary>
    /// Gets the type of database in use from a <see cref="DatabaseFacade"/>.
    /// </summary>
    /// <param name="databaseFacade">The <see cref="DatabaseFacade"/> for the context.</param>
    /// <returns><see cref="DatabaseType"/>.</returns>
    public static string GetDatabaseType(this DatabaseFacade databaseFacade) =>
        GetDatabaseType(databaseFacade.ProviderName);

    /// <summary>
    /// Gets the type of database in use from a <see cref="MigrationBuilder"/>.
    /// </summary>
    /// <param name="migrationBuilder">The <see cref="MigrationBuilder"/> for the migration.</param>
    /// <returns><see cref="DatabaseType"/>.</returns>
    public static string GetDatabaseType(this MigrationBuilder migrationBuilder) =>
        GetDatabaseType(migrationBuilder.ActiveProvider);

    /// <summary>
    /// Gets the type of database in use from a provider name.
    /// </summary>
    /// <param name="providerName">Name of database provider.</param>
    /// <returns><see cref="DatabaseType"/>.</returns>
    public static string GetDatabaseType(string providerName) =>
        providerName switch
        {
            "Microsoft.EntityFrameworkCore.Sqlite" => DatabaseType.Sqlite,
            "Microsoft.EntityFrameworkCore.SqlServer" => DatabaseType.SqlServer,
            _ => null
        };

    #endregion Public GetDatabaseType methods
}
