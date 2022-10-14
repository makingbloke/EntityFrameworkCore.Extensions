// Copyright ©2021-2022 Mike King.
// This file is licensed to you under the MIT license.
// See the License.txt file in the solution root for more information.

using DotDoc.EntityFrameworkCore.Extensions.Constants;
using EntityFrameworkTest.UniqueConstraint.Extensions;
using Microsoft.EntityFrameworkCore;

namespace DotDoc.EntityFrameworkCore.Extensions.Tests.Data;

/// <inheritdoc/>
public class Context : DbContext
{
    private readonly DatabaseType _databaseType;
    private readonly string _connectionString;
    private readonly bool _useUniqueConstraintInterceptor;

    /// <summary>
    /// Initializes a new instance of the <see cref="Context"/> class.
    /// </summary>
    /// <param name="databaseType">Database type.</param>
    /// <param name="connectionString">Connection string.</param>
    /// <param name="useUniqueConstraintInterceptor">If <see langword="true"/> use the UniqueConstraintInterceptor.</param>
    public Context(DatabaseType databaseType, string connectionString, bool useUniqueConstraintInterceptor = false)
    {
        if (string.IsNullOrEmpty(connectionString))
        {
            throw new ArgumentNullException(nameof(connectionString));
        }

        this._databaseType = databaseType;
        this._connectionString = connectionString;
        this._useUniqueConstraintInterceptor = useUniqueConstraintInterceptor;
    }

    /// <summary>
    /// Gets or sets the Test table 1 <see cref="Data.TestTable1"/>.
    /// </summary>
    public DbSet<TestTable1> TestTable1 { get; set; }

    /// <summary>
    /// Gets or sets the Test table <see cref="Data.TestTable2"/>.
    /// </summary>
    public DbSet<TestTable2> TestTable2 { get; set; }

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
        }
    }
}
