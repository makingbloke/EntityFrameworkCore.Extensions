// Copyright ©2021-2026 Mike King.
// This file is licensed to you under the MIT license.
// See the License.txt file in the solution root for more information.

using DotDoc.EntityFrameworkCore.Extensions.DatabaseType;
using DotDoc.EntityFrameworkCore.Extensions.Execute;
using Microsoft.Data.SqlClient;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace DotDoc.EntityFrameworkCore.Extensions.Tests.Data;

/// <summary>
/// Database Utilities.
/// </summary>
public static class DatabaseUtils
{
    #region public constants

    /// <summary>
    /// The environment variable holding the SQL Server connection string (server and login) for the tests.
    /// </summary>
    /// <remarks>
    /// If it isn't set, <see cref="SqlServerDefaultConnectionString"/> is used. If it is <see cref="DatabaseTurnedOff"/>, the SQL Server tests
    /// are skipped (reported as inconclusive). Any database name in it is replaced by the test's own.
    /// </remarks>
    public const string SqlServerConnectionStringVariable = "DOTDOC_EFCORE_TESTS_SQLSERVER";

    /// <summary>
    /// The value of a database's connection string environment variable that skips that database's tests.
    /// </summary>
    public const string DatabaseTurnedOff = "none";

    /// <summary>
    /// The SQL Server connection string used if <see cref="SqlServerConnectionStringVariable"/> isn't set: the local server, with Windows authentication.
    /// </summary>
    public const string SqlServerDefaultConnectionString = "Server=localhost;Trusted_Connection=True;TrustServerCertificate=True";

    /// <summary>
    /// The name of the test database.
    /// </summary>
    public const string TestDatabaseName = "DotDoc.EntityFrameworkCore.Extensions.Tests";

    #endregion public constants

    #region public methods

    /// <summary>
    /// Create a test database.
    /// </summary>
    /// <param name="testContext">The MSTest test context, for the test's temporary directory (SQLite) and cancellation token.</param>
    /// <param name="databaseType">The type of database to create.</param>
    /// <param name="createNew">Create new database (default true).</param>
    /// <param name="customConfigurationActions">Custom Configuration Actions (optional).</param>
    /// <param name="customModelCreationActions">Custom Model Creation Actions (optional).</param>
    /// <param name="customConventionActions">Custom Convention Actions, for model-wide configuration such as DefaultTypeMapping (optional).</param>
    /// <returns>An instance of <see cref="Context"/> for the database.</returns>
    public static async Task<Context> OpenDatabaseAsync(
        TestContext testContext,
        string databaseType,
        bool createNew = true,
        Action<DbContextOptionsBuilder>? customConfigurationActions = null,
        Action<ModelBuilder>? customModelCreationActions = null,
        Action<ModelConfigurationBuilder>? customConventionActions = null)
    {
        CancellationToken cancellationToken = testContext.CancellationToken;
        Context context;

        switch (databaseType)
        {
            case DatabaseTypes.Sqlite:
                context = new(
                    databaseType,
                    GetConnectionString(testContext, databaseType),
                    customConfigurationActions,
                    customModelCreationActions,
                    customConventionActions);

                if (createNew)
                {
                    await context.Database.EnsureDeletedAsync(cancellationToken).ConfigureAwait(false);
                    await context.Database.EnsureCreatedAsync(cancellationToken).ConfigureAwait(false);

                    await SqliteInitialiseFreeTextAsync(context, cancellationToken).ConfigureAwait(false);
                }

                break;

            case DatabaseTypes.SqlServer:
                context = new(
                    databaseType,
                    GetConnectionString(testContext, databaseType),
                    customConfigurationActions,
                    customModelCreationActions,
                    customConventionActions);

                if (createNew)
                {
                    // The full text catalog and index are in the model (see Context.OnModelCreating), so EnsureCreated creates them.
                    await context.Database.EnsureDeletedAsync(cancellationToken).ConfigureAwait(false);
                    await context.Database.EnsureCreatedAsync(cancellationToken).ConfigureAwait(false);
                }

                break;

            default:
                throw new UnsupportedDatabaseTypeException();
        }

        return context;
    }

    /// <summary>
    /// Get the connection string for a test database.
    /// </summary>
    /// <param name="testContext">The MSTest test context. The SQLite database is a file in its <see cref="TestContext.TestTempDirectory"/>.</param>
    /// <param name="databaseType">The type of database.</param>
    /// <returns>The connection string.</returns>
    /// <exception cref="AssertInconclusiveException">The database's tests are turned off (see <see cref="SqlServerConnectionStringVariable"/>).</exception>
    public static string GetConnectionString(TestContext testContext, string databaseType)
    {
        return databaseType switch
        {
            DatabaseTypes.Sqlite => new SqliteConnectionStringBuilder
            {
                DataSource = Path.Combine(testContext.TestTempDirectory!, $"{TestDatabaseName}.db"),
            }.ToString(),
            DatabaseTypes.SqlServer => GetSqlServerConnectionString(TestDatabaseName),
            _ => throw new UnsupportedDatabaseTypeException()
        };
    }

    /// <summary>
    /// Get a SQL Server connection string for a database, using the server and login from <see cref="SqlServerConnectionStringVariable"/>.
    /// </summary>
    /// <param name="databaseName">The database name.</param>
    /// <returns>The connection string.</returns>
    /// <exception cref="AssertInconclusiveException">The SQL Server tests are turned off.</exception>
    public static string GetSqlServerConnectionString(string databaseName)
    {
        return BuildSqlServerConnectionString(Environment.GetEnvironmentVariable(SqlServerConnectionStringVariable), databaseName);
    }

    /// <summary>
    /// Build a SQL Server connection string for a database from the value of <see cref="SqlServerConnectionStringVariable"/>.
    /// </summary>
    /// <param name="setting">The value of the environment variable, or <see langword="null"/> if it isn't set.</param>
    /// <param name="databaseName">The database name.</param>
    /// <returns>The connection string.</returns>
    /// <exception cref="AssertInconclusiveException"><paramref name="setting"/> is <see cref="DatabaseTurnedOff"/>, so the SQL Server tests are turned off.</exception>
    public static string BuildSqlServerConnectionString(string? setting, string databaseName)
    {
        if (string.Equals(setting?.Trim(), DatabaseTurnedOff, StringComparison.OrdinalIgnoreCase))
        {
            Assert.Inconclusive($"The SQL Server tests are turned off ({SqlServerConnectionStringVariable} is '{DatabaseTurnedOff}').");
        }

        // Each test chooses its own database, so the setting only needs the server and login.
        SqlConnectionStringBuilder builder = new(string.IsNullOrWhiteSpace(setting) ? SqlServerDefaultConnectionString : setting)
        {
            InitialCatalog = databaseName,
        };

        return builder.ConnectionString;
    }

    /// <summary>
    /// Create multiple records in the TestTable1 database table.
    /// </summary>
    /// <param name="context">The database context.</param>
    /// <param name="value">The value to insert into the TestField field.</param>
    /// <param name="count">The number of records to create.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A <see cref="Task"/> that represents the asynchronous operation.</returns>
    public static async Task CreateTestTableEntriesAsync(Context context, string value, int count, CancellationToken cancellationToken)
    {
        for (int i = 0; i < count; i++)
        {
            TestTable1 testTable1 = new() { TestField = value };
            context.Add(testTable1);
        }

        await context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Create an entry in the FreeText table(s).
    /// </summary>
    /// <param name="context">The database context.</param>
    /// <param name="value">The value to insert.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A <see cref="Task"/> that represents the asynchronous operation.</returns>
    public static async Task CreateTestFreeTextTableEntryAsync(Context context, string value, CancellationToken cancellationToken)
    {
        switch (context.DatabaseType)
        {
            case DatabaseTypes.Sqlite:
                await SqliteCreateTestFreeTextTableEntryAsync(context, value, cancellationToken).ConfigureAwait(false);
                break;

            case DatabaseTypes.SqlServer:
                await SqlServerCreateTestFreeTextTableEntryAsync(context, value, cancellationToken).ConfigureAwait(false);
                break;

            default:
                throw new UnsupportedDatabaseTypeException();
        }
    }

    #endregion public methods

    #region private methods

    /// <summary>
    /// Initialise the SQLite FreeText tables.
    /// </summary>
    /// <param name="context">The database context.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A <see cref="Task"/> that represents the asynchronous operation.</returns>
    private static async Task SqliteInitialiseFreeTextAsync(Context context, CancellationToken cancellationToken)
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
);";

        await context.Database.ExecuteNonQueryAsync(sql, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Create an entry in SQLite FreeText table(s).
    /// </summary>
    /// <param name="context">The database context.</param>
    /// <param name="value">The value to insert.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A <see cref="Task"/> that represents the asynchronous operation.</returns>
    private static async Task SqliteCreateTestFreeTextTableEntryAsync(Context context, string value, CancellationToken cancellationToken)
    {
        FreeText freeText = new() { FreeTextField = value };

        await context.TestFreeText.AddAsync(freeText, cancellationToken).ConfigureAwait(false);
        await context.TestFreeTextStemming.AddAsync(freeText, cancellationToken).ConfigureAwait(false);
        await context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Create an entry in a SQL Server FreeText table.
    /// </summary>
    /// <param name="context">The database context.</param>
    /// <param name="value">The value to insert.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A <see cref="Task"/> that represents the asynchronous operation.</returns>
    private static async Task SqlServerCreateTestFreeTextTableEntryAsync(Context context, string value, CancellationToken cancellationToken)
    {
        FreeText freeText = new() { FreeTextField = value };

        await context.TestFreeText.AddAsync(freeText, cancellationToken).ConfigureAwait(false);
        await context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        // Wait for SQL Server (maximum 30 seconds) to update the full text catalog otherwise when we query the entry just written will not be found.
        // Obviously, we do not need to do this in a real scenario as the full text catalog will be updated in the background.
        const int maxRetries = 300;
        const int millisecondsDelay = 100;

        bool finished = false;
        for (int i = 0; i < maxRetries; i++)
        {
            await Task.Delay(millisecondsDelay, cancellationToken).ConfigureAwait(false);

            if (await GetFullTextPendingChangesAsync(context, cancellationToken).ConfigureAwait(false) == 0)
            {
                finished = true;
                break;
            }
        }

        if (!finished)
        {
            throw new InvalidOperationException("Timeout waiting for full text catalog update.");
        }
    }

    /// <summary>
    /// Get the number of changes to the test free text table that are still waiting to be added to its full text index.
    /// </summary>
    /// <param name="context">The database context.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The number of pending changes.</returns>
    /// <exception cref="InvalidOperationException">Table or full text index is missing.</exception>
    private static async Task<int> GetFullTextPendingChangesAsync(Context context, CancellationToken cancellationToken)
    {
        // The status of the full text catalog can't be used: it's still idle after the insert commits, until the background
        // process picks up the change and starts a crawl. The pending changes count includes the change as soon as it commits.
        // OBJECTPROPERTYEX returns NULL if the table or its full text index isn't found.
        string sql = $"SELECT CAST(OBJECTPROPERTYEX(OBJECT_ID('{Context.TestFreeTextTableName}'), 'TableFulltextPendingChanges') AS int)";
        int? pendingChanges = await context.Database.ExecuteScalarAsync<int?>(sql, cancellationToken).ConfigureAwait(false);

        if (!pendingChanges.HasValue)
        {
            throw new InvalidOperationException("Table or full text index is missing.");
        }

        return pendingChanges.Value;
    }

    #endregion private methods
}
