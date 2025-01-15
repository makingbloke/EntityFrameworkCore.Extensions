// Copyright ©2021-2025 Mike King.
// This file is licensed to you under the MIT license.
// See the License.txt file in the solution root for more information.

using DotDoc.EntityFrameworkCore.Extensions.Constants;
using DotDoc.EntityFrameworkCore.Extensions.Extensions;
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
    /// If <see langword="true"/> use the Unique Constraint Interceptor.
    /// </summary>
    private readonly bool _useUniqueConstraintInterceptor;

    /// <summary>
    /// If <see langword="true"/> use the Execute Update Extensions.
    /// </summary>
    private readonly bool _useExecuteUpdateExtensions;

    #endregion private fields

    #region public constructors

    /// <summary>
    /// Initializes a new instance of the <see cref="Context"/> class.
    /// </summary>
    /// <param name="databaseType">Database Type.</param>
    /// <param name="connectionString">Database Connection String.</param>
    /// <param name="useUniqueConstraintInterceptor">If <see langword="true"/> use the Unique Constraint Interceptor.</param>
    /// <param name="useExecuteUpdateExtensions">If <see langword="true"/> use the Execute Update Extensions.</param>
    public Context(string databaseType, string connectionString, bool useUniqueConstraintInterceptor = false, bool useExecuteUpdateExtensions = false)
    {
        if (string.IsNullOrEmpty(connectionString))
        {
            throw new ArgumentNullException(nameof(connectionString));
        }

        this._databaseType = databaseType;
        this._connectionString = connectionString;
        this._useUniqueConstraintInterceptor = useUniqueConstraintInterceptor;
        this._useExecuteUpdateExtensions = useExecuteUpdateExtensions;
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

            if (this._useUniqueConstraintInterceptor)
            {
                optionsBuilder.UseUniqueConstraintInterceptor();
            }

            if (this._useExecuteUpdateExtensions)
            {
                optionsBuilder.UseExecuteUpdateExtensions();
            }
        }
    }

    #endregion protected methods
}
