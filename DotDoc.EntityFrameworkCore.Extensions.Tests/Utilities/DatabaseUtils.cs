// Copyright ©2021-2025 Mike King.
// This file is licensed to you under the MIT license.
// See the License.txt file in the solution root for more information.

using DotDoc.EntityFrameworkCore.Extensions.DatabaseType;
using DotDoc.EntityFrameworkCore.Extensions.Execute;
using DotDoc.EntityFrameworkCore.Extensions.Tests.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

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
        // Create a new instance of the database context.
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

        // Remove the previous database (if there was one) and create a new blank one.
        context.Database.EnsureDeleted();
        context.Database.EnsureCreated();

        // Initialise the FreeText tables.
        switch (databaseType)
        {
            case DatabaseTypes.Sqlite:
                SqliteInitialiseFreeText(context);
                break;

            case DatabaseTypes.SqlServer:
                SqlServerInitialiseFreeText(context);
                break;

            default:
                throw new InvalidOperationException("Unsupported database type");
        }

        return context;
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

    /// <summary>
    /// Create an entry in the FreeText table(s).
    /// </summary>
    /// <param name="context">The database context.</param>
    /// <param name="value">The value to insert.</param>
    /// <returns>A <see cref="Task"/> that represents the asynchronous operation.</returns>
    public static async Task CreateTestFreeTextTableEntryAsync(Context context, string value)
    {
        switch (context.DatabaseType)
        {
            case DatabaseTypes.Sqlite:
                await SqliteCreateTestFreeTextTableEntryAsync(context, value).ConfigureAwait(false);
                break;

            case DatabaseTypes.SqlServer:
                await SqlServerCreateTestFreeTextTableEntryAsync(context, value).ConfigureAwait(false);
                break;

            default:
                throw new InvalidOperationException("Unsupported database type");
        }
    }

    #endregion public methods

    #region private methods

    /// <summary>
    /// Get the primary key Name for a table.
    /// </summary>
    /// <param name="context">The database context.</param>
    /// <param name="tableName">The table name.</param>
    /// <returns>The primary key name.</returns>
    private static string GetPrimaryKeyName(Context context, string tableName)
    {
        IEntityType? entityType = context.Model.FindEntityType(tableName);

        if (entityType == null)
        {
            throw new InvalidOperationException($"Entity {tableName} not found.");
        }

        IReadOnlyKey? primaryKey = entityType.FindPrimaryKey();

        if (primaryKey == null)
        {
            throw new InvalidOperationException($"Primary key for entity {tableName} not found.");
        }

        return primaryKey.GetName()!;
    }

    /// <summary>
    /// Get the SQL server full text catalog name for a table.
    /// </summary>
    /// <returns>The name of the full text catalog.</returns>
    private static string GetFullTextCatalogName() => $"{Context.TestFreeTextTableName}FtCatalog";

    /// <summary>
    /// Initialise the SQLite FreeText tables.
    /// </summary>
    /// <param name="context">The database context.</param>
    private static void SqliteInitialiseFreeText(Context context)
    {
        string sql =
@$"
DROP TABLE IF EXISTS {Context.TestFreeTextTableName};

CREATE VIRTUAL TABLE {Context.TestFreeTextTableName} USING FTS5 (
    {nameof(FreeText.FreeTextField)},
    tokenize = 'unicode61 remove_diacritics 2'
);

DROP TABLE IF EXISTS {Context.TestFreeTextStemmingTableName};

CREATE VIRTUAL TABLE {Context.TestFreeTextStemmingTableName} USING FTS5 (
    {nameof(FreeText.FreeTextField)},
    tokenize = 'porter unicode61 remove_diacritics 2'
);
";

        context.Database.ExecuteSqlRaw(sql);
    }

    /// <summary>
    /// Initialise the SQLServer FreeText table.
    /// </summary>
    /// <param name="context">The database context.</param>
    private static void SqlServerInitialiseFreeText(Context context)
    {
        string fullTextCatalogName = GetFullTextCatalogName();
        string primaryKeyName = GetPrimaryKeyName(context, Context.TestFreeTextTableName);

        string sql =
@$"
CREATE FULLTEXT CATALOG {fullTextCatalogName} WITH ACCENT_SENSITIVITY = OFF;
CREATE FULLTEXT INDEX ON {Context.TestFreeTextTableName}(FreeTextField) KEY INDEX {primaryKeyName} ON {fullTextCatalogName};
";

        context.Database.ExecuteSqlRaw(sql);
    }

    /// <summary>
    /// Create an entry in SQLite FreeText table(s).
    /// </summary>
    /// <param name="context">The database context.</param>
    /// <param name="value">The value to insert.</param>
    /// <returns>A <see cref="Task"/> that represents the asynchronous operation.</returns>
    private static async Task SqliteCreateTestFreeTextTableEntryAsync(Context context, string value)
    {
        FreeText freeText = new() { FreeTextField = value };

        await context.TestFreeText.AddAsync(freeText).ConfigureAwait(false);
        await context.TestFreeTextStemming.AddAsync(freeText).ConfigureAwait(false);
        await context.SaveChangesAsync().ConfigureAwait(false);
    }

    /// <summary>
    /// Create an entry in a SQL Server FreeText table.
    /// </summary>
    /// <param name="context">The database context.</param>
    /// <param name="value">The value to insert.</param>
    /// <returns>A <see cref="Task"/> that represents the asynchronous operation.</returns>
    private static async Task SqlServerCreateTestFreeTextTableEntryAsync(Context context, string value)
    {
        FreeText freeText = new() { FreeTextField = value };

        await context.TestFreeText.AddAsync(freeText).ConfigureAwait(false);
        await context.SaveChangesAsync().ConfigureAwait(false);

        // Check the populate status of the full text catalog and wait (maximum 30 seconds) for it to be updated.
        // (otherwise when we query the entry will not be found).
        int maxRetries = 60;
        int millisecondsDelay = 500;

        string sql = $"SELECT FULLTEXTCATALOGPROPERTY('{GetFullTextCatalogName()}', 'PopulateStatus')";

        for (int i = 0; i < maxRetries; i++)
        {
            int status = await context.Database.ExecuteScalarAsync<int>(sql).ConfigureAwait(false);

            if (status == 0)
            {
                break;
            }

            if (i == maxRetries - 1)
            {
                throw new InvalidOperationException($"Timeout waiting for full text catalog update. Status: {status}");
            }

            await Task.Delay(millisecondsDelay).ConfigureAwait(false);
        }
    }

    #endregion private methods
}
