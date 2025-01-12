// Copyright ©2021-2025 Mike King.
// This file is licensed to you under the MIT license.
// See the License.txt file in the solution root for more information.

using DotDoc.EntityFrameworkCore.Extensions.Constants;
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
    public static bool DoesDatabaseExist(this DatabaseFacade databaseFacade) =>
        databaseFacade.GetService<IRelationalDatabaseCreator>().Exists();

    /// <summary>
    /// Check if database exists.
    /// </summary>
    /// <param name="databaseFacade">The <see cref="DatabaseFacade"/> for the context.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
    /// <returns>A <see langword="bool"/> indicating if the database exists.</returns>
    public static async Task<bool> DoesDatabaseExistAsync(this DatabaseFacade databaseFacade, CancellationToken cancellationToken = default) =>
        await databaseFacade.GetService<IRelationalDatabaseCreator>().ExistsAsync(cancellationToken).ConfigureAwait(false);

    #endregion public DoesDatabaseExist methods

    #region public DoesTableExist methods

    /// <summary>
    /// Check if a table exists in the database.
    /// </summary>
    /// <param name="databaseFacade">The <see cref="DatabaseFacade"/> for the context.</param>
    /// <param name="tableName">The table name.</param>
    /// <returns>A <see langword="bool"/> indicating if the table exists.</returns>
    public static bool DoesTableExist(this DatabaseFacade databaseFacade, string tableName)
    {
        GetDoesTableExistQuery(databaseFacade, tableName, out string sql, out object[] parameters);
        int count = QueryMethods.ExecuteScalar<int>(databaseFacade, sql, parameters);
        return count > 0;
    }

    /// <summary>
    /// Check if a table exists in the database.
    /// </summary>
    /// <param name="databaseFacade">The <see cref="DatabaseFacade"/> for the context.</param>
    /// <param name="tableName">The table name.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
    /// <returns>A <see langword="bool"/> indicating if the table exists.</returns>
    public static async Task<bool> DoesTableExistAsync(this DatabaseFacade databaseFacade, string tableName, CancellationToken cancellationToken = default)
    {
        GetDoesTableExistQuery(databaseFacade, tableName, out string sql, out object[] parameters);
        int count = await QueryMethods.ExecuteScalarAsync<int>(databaseFacade, sql, parameters, cancellationToken).ConfigureAwait(false);
        return count > 0;
    }

    #endregion public DoesTableExist methods

    #region private methods

    /// <summary>
    /// Gets the SQL to check if a table exists.
    /// </summary>
    /// <param name="databaseFacade">The <see cref="DatabaseFacade"/> for the context.</param>
    /// <param name="tableName">The table name.</param>
    /// <param name="sql">The sql (out).</param>
    /// <param name="parameters">The parameters (out).</param>
    private static void GetDoesTableExistQuery(DatabaseFacade databaseFacade, string tableName, out string sql, out object[] parameters)
    {
        switch (databaseFacade.GetDatabaseType())
        {
            case DatabaseType.Sqlite:
                sql = "SELECT COUNT(*) FROM sqlite_master WHERE name = {0} and type = {1}";
                parameters = [tableName, "table"];
                break;

            case DatabaseType.SqlServer:
                sql = "SELECT COUNT(*) FROM sys.Tables WHERE Name = {0} and Type = {1}";
                parameters = [tableName, "U"];
                break;

            default:
                throw new InvalidOperationException("Unsupported database type");
        }
    }

    #endregion private methods
}
