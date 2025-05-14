// Copyright ©2021-2025 Mike King.
// This file is licensed to you under the MIT license.
// See the License.txt file in the solution root for more information.

using DotDoc.EntityFrameworkCore.Extensions.DatabaseType;
using DotDoc.EntityFrameworkCore.Extensions.Execute;
using DotDoc.EntityFrameworkCore.Extensions.Tests.Data;
using Microsoft.EntityFrameworkCore;
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
        string sql;

        switch (databaseType)
        {
            case DatabaseTypes.Sqlite:

                sql = $"DROP TABLE IF EXISTS {Context.TestFreeTextTableName};{Environment.NewLine}{Environment.NewLine}" +
                    $"CREATE VIRTUAL TABLE {Context.TestFreeTextTableName} USING FTS5 ({Environment.NewLine}" +
                    $"    {nameof(FreeText.FreeTextField)},{Environment.NewLine}" +
                    $"    tokenize = 'unicode61 remove_diacritics 2'{Environment.NewLine}" +
                    $");{Environment.NewLine}";

                if (!string.IsNullOrEmpty(Context.TestFreeTextStemmingTableName))
                {
                    sql += $"DROP TABLE IF EXISTS {Context.TestFreeTextStemmingTableName};{Environment.NewLine}{Environment.NewLine}" +
                        $"CREATE VIRTUAL TABLE {Context.TestFreeTextStemmingTableName} USING FTS5 ({Environment.NewLine}" +
                        $"    {nameof(FreeText.FreeTextField)},{Environment.NewLine}" +
                        $"    tokenize = 'porter unicode61 remove_diacritics 2'{Environment.NewLine}" +
                        $");{Environment.NewLine}";
                }

                break;

            case DatabaseTypes.SqlServer:
                string ftCatalogName = $"{Context.TestFreeTextTableName}FtCatalog";
                string primaryKeyName = $"PK_{Context.TestFreeTextTableName}";

                sql = $"CREATE FULLTEXT CATALOG {ftCatalogName} WITH ACCENT_SENSITIVITY = OFF;{Environment.NewLine}" +
                    $"CREATE FULLTEXT INDEX ON {Context.TestFreeTextTableName}(FreeTextField) KEY INDEX {primaryKeyName} ON {ftCatalogName};{Environment.NewLine}";
                break;

            default:
                throw new InvalidOperationException("Unsupported database type");
        }

        context.Database.ExecuteSqlRaw(sql);

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
    public static void CreateTestFreeTextTableEntry(Context context, string value)
    {
        FreeText freeText = new() { FreeTextField = value };

        context.TestFreeText.Add(freeText);

        switch (context.DatabaseType)
        {
            case DatabaseTypes.Sqlite:
                context.TestFreeTextStemming.Add(freeText);
                context.SaveChanges();
                break;

            case DatabaseTypes.SqlServer:

                context.SaveChanges();

                string ftCatalogName = $"{Context.TestFreeTextTableName}FtCatalog";

                int status = -1;

                for (int i = 0; i < 60 && status != 0; i++)
                {
                    Thread.Sleep(500);
                    status = context.Database.ExecuteScalarAsync<int>("SELECT FULLTEXTCATALOGPROPERTY('" + ftCatalogName + "', 'PopulateStatus')").Result;
                }

                if (status != 0)
                {
                    throw new InvalidOperationException($"Fulltext failure - status: {status}");
                }

                break;

            default:
                throw new InvalidOperationException("Unsupported database type");
        }
    }

    #endregion public methods
}
