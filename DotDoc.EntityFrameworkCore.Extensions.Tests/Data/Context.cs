// Copyright ©2021-2025 Mike King.
// This file is licensed to you under the MIT license.
// See the License.txt file in the solution root for more information.

using DotDoc.EntityFrameworkCore.Extensions.Constants;
using Microsoft.EntityFrameworkCore;

namespace DotDoc.EntityFrameworkCore.Extensions.Tests.Data;

/// <inheritdoc/>
public class Context : DbContext
{
    #region private fields

    /// <summary>
    /// Database Type.
    /// </summary>
    private readonly string _databaseType;

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
        if (string.IsNullOrEmpty(connectionString))
        {
            throw new ArgumentNullException(nameof(connectionString));
        }

        this._databaseType = databaseType;
        this._connectionString = connectionString;
        this._customConfigurationActions = customConfigurationActions;
        this._customModelCreationActions = customModelCreationActions;
    }

    #endregion public constructors

    #region public properties

    /// <summary>
    /// Gets or sets the Test table 1 <see cref="Data.TestTable1"/>.
    /// </summary>
    public DbSet<TestTable1> TestTable1 { get; set; }

    /// <summary>
    /// Gets or sets the Test table <see cref="Data.TestTable2"/>.
    /// </summary>
    public DbSet<TestTable2> TestTable2 { get; set; }

    #endregion public properties

    #region protected methods

    /// <inheritdoc/>
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (!optionsBuilder.IsConfigured)
        {
            switch (this._databaseType)
            {
                case DatabaseType.Sqlite:
                    optionsBuilder.UseSqlite(this._connectionString);
                    break;

                case DatabaseType.SqlServer:
                    optionsBuilder.UseSqlServer(this._connectionString);
                    break;

                default:
                    throw new InvalidOperationException("Unsupported database type");
            }

            this._customConfigurationActions?.Invoke(optionsBuilder);
        }
    }

    /// <inheritdoc/>
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        this._customModelCreationActions?.Invoke(modelBuilder);
    }

    #endregion protected methods
}
