// Copyright ©2021-2025 Mike King.
// This file is licensed to you under the MIT license.
// See the License.txt file in the solution root for more information.

using DotDoc.EntityFrameworkCore.Extensions.DatabaseType;
using DotDoc.EntityFrameworkCore.Extensions.Tests.Data;
using Microsoft.EntityFrameworkCore;

namespace DotDoc.EntityFrameworkCore.Extensions.Tests.Utilities;

/// <summary>
/// Database Utilities.
/// </summary>
public static class DatabaseUtils
{
    #region public methods

    /// <summary>
    /// Create an a test database.
    /// </summary>
    /// <param name="databaseType">The type of database to create.</param>
    /// <param name="customConfigurationActions">Custom Configuration Actions (optional).</param>
    /// <param name="customModelCreationActions">Custom Model Creation Actions (optional).</param>
    /// <returns>An instance of <see cref="Context"/> for the database.</returns>
    public static Context CreateDatabase(
        string databaseType,
        Action<DbContextOptionsBuilder>? customConfigurationActions = null,
        Action<ModelBuilder>? customModelCreationActions = null)
    {
        Context context = databaseType switch
        {
            DatabaseTypes.Sqlite => new(
                databaseType,
                "Data Source=DotDoc.EntityFrameworkCore.Extensions.Tests.db",
                customConfigurationActions,
                customModelCreationActions),

            // Note: We use Sql Server Developer Edition with Windows Authentication to keep things simple.
            DatabaseTypes.SqlServer => new(
                databaseType,
                "Server=localhost;Initial Catalog=DotDoc.EntityFrameworkCore.Extensions.Tests;Trusted_Connection=True;TrustServerCertificate=True",
                customConfigurationActions,
                customModelCreationActions),

            _ => throw new ArgumentException("Unsupported database type", nameof(databaseType))
        };

        context.Database.EnsureDeleted();
        context.Database.EnsureCreated();

        return context;
    }

    /// <summary>
    /// Get the default schema name for a database.
    /// </summary>
    /// <param name="databaseType">The type of database.</param>
    /// <returns>The schema name.</returns>
    public static string? GetDefaultSchema(string databaseType)
    {
        string? schema = databaseType == DatabaseTypes.SqlServer
            ? "dbo"
            : null;

        return schema;
    }

    /// <summary>
    /// Create multiple records in the TestTable1 database table.
    /// </summary>
    /// <param name="context">The database context.</param>
    /// <param name="value">The value to insert into the TestField field.</param>
    /// <param name="count">The number of records to create.</param>
    public static void CreateTestTableEntries(Context context, string value, int count)
    {
        for (int i = 0; i < count; i++)
        {
            TestTable1 testTable1 = new() { TestField = value };
            context.Add(testTable1);
        }

        context.SaveChanges();
    }

    #endregion public methods
}
