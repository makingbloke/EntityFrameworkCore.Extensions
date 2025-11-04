// Copyright ©2021-2025 Mike King.
// This file is licensed to you under the MIT license.
// See the License.txt file in the solution root for more information.

using DotDoc.EntityFrameworkCore.Extensions.DatabaseType;
using DotDoc.EntityFrameworkCore.Extensions.FreeTextSearchFunction;
using Microsoft.EntityFrameworkCore;

namespace DotDoc.EntityFrameworkCore.Extensions.Tests.Data;

/// <inheritdoc/>
public class Context : DbContext
{
    #region private fields

    /// <summary>
    /// Database Connection String.
    /// </summary>
    private readonly string _connectionString;

    /// <summary>
    /// Custom Configuration Actions..
    /// </summary>
    private readonly Action<DbContextOptionsBuilder>? _customConfigurationActions;

    /// <summary>
    /// Custom Configuration Actions..
    /// </summary>
    private readonly Action<ModelBuilder>? _customModelCreationActions;

    #endregion private fields

    #region public constructors

    /// <summary>
    /// Initializes a new instance of the <see cref="Context"/> class.
    /// </summary>
    /// <param name="databaseType">Database Type.</param>
    /// <param name="connectionString">Database Connection String.</param>
    /// <param name="customConfigurationActions">Custom Configuration Actions (optional).</param>
    /// <param name="customModelCreationActions">Custom Model Creation Actions (optional).</param>
    public Context(
        string databaseType,
        string connectionString,
        Action<DbContextOptionsBuilder>? customConfigurationActions = null,
        Action<ModelBuilder>? customModelCreationActions = null)
    {
        ArgumentException.ThrowIfNullOrEmpty(databaseType);
        ArgumentException.ThrowIfNullOrEmpty(connectionString);

        this._connectionString = connectionString;
        this._customConfigurationActions = customConfigurationActions;
        this._customModelCreationActions = customModelCreationActions;

        this.DatabaseType = databaseType;
    }

    #endregion public constructors

    #region public properties

    /// <summary>
    /// Gets the SQL Server Language Term for English (from sys.syslanguages).
    /// </summary>
    public static int EnglishLanguageTerm => 1033;

    /// <summary>
    /// Gets the FreeText table name.
    /// </summary>
    public static string TestFreeTextTableName => "TestFreeText";

    /// <summary>
    /// Gets the FreeText Stemming table name.
    /// </summary>
    public static string TestFreeTextStemmingTableName => "TestFreeText_Stemming";

    /// <summary>
    /// Gets the database Type.
    /// </summary>
    public string DatabaseType { get; }

    /// <summary>
    /// Gets the default schema name.
    /// </summary>
    public string? DefaultSchema => this.DatabaseType == DatabaseTypes.SqlServer ? "dbo" : null;

    /// <summary>
    /// Gets or sets the Test Table 1 <see cref="Data.TestTable1"/>.
    /// </summary>
    public DbSet<TestTable1> TestTable1 { get; set; }

    /// <summary>
    /// Gets or sets the Test Table 2 <see cref="Data.TestTable2"/>.
    /// </summary>
    public DbSet<TestTable2> TestTable2 { get; set; }

    /// <summary>
    /// Gets or sets the Test Table 3 <see cref="Data.GuidTable"/>.
    /// </summary>
    public DbSet<GuidTable> GuidTable { get; set; }

    /// <summary>
    /// Gets the Test Free Text Table <see cref="FreeText"/>.
    /// </summary>
    public DbSet<FreeText> TestFreeText => this.Set<FreeText>(TestFreeTextTableName);

    /// <summary>
    /// Gets the Test Free Text Stemming Table <see cref="FreeText"/>.
    /// </summary>
    public DbSet<FreeText> TestFreeTextStemming => this.Set<FreeText>(TestFreeTextStemmingTableName);

    #endregion public properties

    #region protected methods

    /// <inheritdoc/>
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        // Some of the tests (such as Test_SetStemmingTable_TableName_GuardClause in FreeTextSearchFunctionTests)
        // throw exceptions in OnModelCreating. By the default the model is cached so these do not get raised for
        // the second test run onwards so turn caching off.
        optionsBuilder.EnableServiceProviderCaching(false);

        switch (this.DatabaseType)
        {
            case DatabaseTypes.Sqlite:
                optionsBuilder.UseSqlite(this._connectionString);
                break;

            case DatabaseTypes.SqlServer:
                optionsBuilder.UseSqlServer(this._connectionString);
                break;

            default:
                throw new UnsupportedDatabaseTypeException();
        }

        optionsBuilder.UseFreeTextSearchExtensions();

        this._customConfigurationActions?.Invoke(optionsBuilder);
    }

    /// <inheritdoc/>
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Setup the model for the FreeText and FreeText_Stemming tables.
        modelBuilder.SharedTypeEntity<FreeText>(TestFreeTextTableName);

        // In SQLite associate the stemming table with the non stemming table
        // and the Id column with the value of the ROWID column.
        if (this.DatabaseType == DatabaseTypes.Sqlite)
        {
            modelBuilder.SharedTypeEntity<FreeText>(TestFreeTextTableName)
                .SetStemmingTable(TestFreeTextStemmingTableName)
                .Property<long>(nameof(FreeText.Id))
                .HasColumnName("ROWID");

            modelBuilder.SharedTypeEntity<FreeText>(TestFreeTextStemmingTableName)
                .Property<long>(nameof(FreeText.Id))
                .HasColumnName("ROWID");
        }

        this._customModelCreationActions?.Invoke(modelBuilder);
    }

    #endregion protected methods
}
