// Copyright ©2021-2026 Mike King.
// This file is licensed to you under the MIT license.
// See the License.txt file in the solution root for more information.

using Microsoft.EntityFrameworkCore;

namespace DotDoc.EntityFrameworkCore.Extensions.Tests.Data;

/// <summary>
/// A context that can be used with context pooling (<see cref="Context"/> can't, because it is configured in
/// <see cref="DbContext.OnConfiguring(DbContextOptionsBuilder)"/> rather than through its constructor).
/// </summary>
/// <remarks>It maps <see cref="Data.TestTable1"/> to the same table as <see cref="Context"/>, so use <see cref="DatabaseUtils.OpenDatabaseAsync"/> to create the database first.</remarks>
public class PooledContext : DbContext
{
    #region public constructors

    /// <summary>
    /// Initializes a new instance of the <see cref="PooledContext"/> class.
    /// </summary>
    /// <param name="options">The options for this context.</param>
    public PooledContext(DbContextOptions<PooledContext> options)
        : base(options)
    {
    }

    #endregion public constructors

    #region public properties

    /// <summary>
    /// Gets or sets the Test Table 1 <see cref="Data.TestTable1"/>.
    /// </summary>
    public DbSet<TestTable1> TestTable1 { get; set; }

    #endregion public properties
}
