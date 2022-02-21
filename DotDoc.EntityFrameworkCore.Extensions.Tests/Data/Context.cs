// Copyright ©2021-2022 Mike King.
// This file is licensed to you under the MIT license.
// See the License.txt file in the solution root for more information.

using DotDoc.EntityFrameworkCore.Extensions.Constants;
using Microsoft.EntityFrameworkCore;

namespace DotDoc.EntityFrameworkCore.Extensions.Tests.Data
{
    /// <inheritdoc/>
    public class Context : DbContext
    {
        private readonly DatabaseType _databaseType;
        private readonly string _connectionString;

        /// <summary>
        /// Initializes a new instance of the <see cref="Context"/> class.
        /// </summary>
        /// <param name="databaseType">Database type.</param>
        /// <param name="connectionString">Connection string.</param>
        public Context(DatabaseType databaseType, string connectionString)
        {
            if (string.IsNullOrEmpty(connectionString))
            {
                throw new ArgumentNullException(nameof(connectionString));
            }

            this._databaseType = databaseType;
            this._connectionString = connectionString;
        }

        /// <summary>
        /// Gets or sets the Test table <see cref="DbSet{TestTable}"/>.
        /// </summary>
        public DbSet<TestTable> TestTable { get; set; }

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
            }
        }
    }
}
