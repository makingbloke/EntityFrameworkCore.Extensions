// Copyright ©2021-2025 Mike King.
// This file is licensed to you under the MIT license.
// See the License.txt file in the solution root for more information.

using DotDoc.EntityFrameworkCore.Extensions.Constants;
using DotDoc.EntityFrameworkCore.Extensions.Extensions;
using DotDoc.EntityFrameworkCore.Extensions.Tests.Constants;
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
    /// If <see langword="true"/> use the Execute Update Interceptor.
    /// </summary>
    private readonly bool _useExecuteUpdateInterceptor;

    /// <summary>
    /// If <see langword="true"/> use the SQLite Case Insensitivity Interceptor.
    /// </summary>
    private readonly bool _useSqliteCaseInsensitivityInterceptor;

    /// <summary>
    /// If <see langword="true"/> use the Unique Constraint Interceptor.
    /// </summary>
    private readonly bool _useUniqueConstraintInterceptor;

    /// <summary>
    /// Default Collation Sequence.
    /// </summary>
    private readonly DefaultCollationSequence _collation;

    #endregion private fields

    #region public constructors

    /// <summary>
    /// Initializes a new instance of the <see cref="Context"/> class.
    /// </summary>
    /// <param name="databaseType">Database Type.</param>
    /// <param name="connectionString">Database Connection String.</param>
    /// <param name="useExecuteUpdateInterceptor">If <see langword="true"/> use the Execute Update Interceptor.</param>
    /// <param name="useSqliteCaseInsensitivityInterceptor">If <see langword="true"/> use the SQLite Case Insensitivity Interceptor.</param>
    /// <param name="useUniqueConstraintInterceptor">If <see langword="true"/> use the Unique Constraint Interceptor.</param>
    /// <param name="collation">Default Collation Sequence.</param>
    public Context(
        string databaseType,
        string connectionString,
        bool useExecuteUpdateInterceptor = false,
        bool useSqliteCaseInsensitivityInterceptor = false,
        bool useUniqueConstraintInterceptor = false,
        DefaultCollationSequence collation = DefaultCollationSequence.None)
    {
        if (string.IsNullOrEmpty(connectionString))
        {
            throw new ArgumentNullException(nameof(connectionString));
        }

        this._databaseType = databaseType;
        this._connectionString = connectionString;
        this._useExecuteUpdateInterceptor = useExecuteUpdateInterceptor;
        this._useSqliteCaseInsensitivityInterceptor = useSqliteCaseInsensitivityInterceptor;
        this._useUniqueConstraintInterceptor = useUniqueConstraintInterceptor;
        this._collation = collation;
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

            if (this._useExecuteUpdateInterceptor)
            {
                optionsBuilder.UseExecuteUpdateExtensions();
            }

            if (this._useSqliteCaseInsensitivityInterceptor)
            {
                optionsBuilder.UseSqliteUnicodeNoCase();
            }

            if (this._useUniqueConstraintInterceptor)
            {
                optionsBuilder.UseUniqueConstraintInterceptor();
            }
        }
    }

    /// <inheritdoc/>
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        switch (this._collation)
        {
            case DefaultCollationSequence.SqliteCaseInsensitive:
                modelBuilder.UseSqliteCaseInsensitiveCollation();
                break;

            case DefaultCollationSequence.SqlServerCaseInsensitive:
                modelBuilder.UseSqlServerCaseInsensitiveCollation();
                break;

            default:
                break;
        }
    }

    #endregion protected methods
}
