// Copyright ©2021-2023 Mike King.
// This file is licensed to you under the MIT license.
// See the License.txt file in the solution root for more information.

using DotDoc.EntityFrameworkCore.Extensions.Constants;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;

namespace DotDoc.EntityFrameworkCore.Extensions;

/// <summary>
/// Entity Framework Core Does Exist Exensions.
/// </summary>
public static class DoesExistExtensions
{
    #region Public DoesDatabaseExist Methods

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

    #endregion Public DoesDatabaseExist Methods

    #region Public DoesTableExist Methods

    /// <summary>
    /// Check if a table exists in the database.
    /// </summary>
    /// <param name="databaseFacade">The <see cref="DatabaseFacade"/> for the context.</param>
    /// <param name="tableName">The table name.</param>
    /// <returns>A <see langword="bool"/> indicating if the table exists.</returns>
    public static bool DoesTableExist(this DatabaseFacade databaseFacade, string tableName)
    {
        (string sql, object[] parameters) = GetDoesTableExistQuery(databaseFacade, tableName);
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
        (string sql, object[] parameters) = GetDoesTableExistQuery(databaseFacade, tableName);
        int count = await QueryMethods.ExecuteScalarAsync<int>(databaseFacade, sql, parameters, cancellationToken).ConfigureAwait(false);
        return count > 0;
    }

    #endregion Public DoesTableExist Methods

    #region Private methods

    /// <summary>
    /// Gets the SQL to check if a table exists.
    /// </summary>
    /// <param name="databaseFacade">The <see cref="DatabaseFacade"/> for the context.</param>
    /// <param name="tableName">The table name.</param>
    /// <returns>A tuple containting the SQL and parameters array.</returns>
    private static (string, object[]) GetDoesTableExistQuery(DatabaseFacade databaseFacade, string tableName)
    {
        string sql;
        object[] parameters;

        switch (databaseFacade.GetDatabaseType())
        {
            case DatabaseType.Sqlite:
                sql = "SELECT COUNT(*) FROM sqlite_master WHERE name = {0} and type = {1}";
                parameters = new[] { tableName, "table" };
                break;

            case DatabaseType.SqlServer:
                sql = "SELECT COUNT(*) FROM sys.Tables WHERE Name = {0} and Type = {1}";
                parameters = new[] { tableName, "U" };
                break;

            default:
                throw new InvalidOperationException("Unsupported database type");
        }

        return (sql, parameters);
    }

    #endregion Private methods
}
