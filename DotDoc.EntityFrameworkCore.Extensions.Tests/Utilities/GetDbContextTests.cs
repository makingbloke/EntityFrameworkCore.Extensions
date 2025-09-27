// Copyright ©2021-2025 Mike King.
// This file is licensed to you under the MIT license.
// See the License.txt file in the solution root for more information.

using DotDoc.EntityFrameworkCore.Extensions.DatabaseType;
using DotDoc.EntityFrameworkCore.Extensions.Tests.Data;
using DotDoc.EntityFrameworkCore.Extensions.Tests.TestUtilities;
using DotDoc.EntityFrameworkCore.Extensions.Utilities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace DotDoc.EntityFrameworkCore.Extensions.Tests.Utilities;

/// <summary>
/// Tests for GetDbContext extensions.
/// </summary>
[TestClass]
public class GetDbContextTests
{
    /// <summary>
    /// Test GetDbContext with Null DatabaseFacade Database parameter.
    /// </summary>
    [TestMethod(DisplayName = "GetDbContext DatabaseFacade Guard Clause")]
    public void GetDbContextTests_001()
    {
        // ARRANGE
        DatabaseFacade database = null!;

        // ACT / ASSERT
        Assert.ThrowsExactly<ArgumentNullException>(() => _ = database.GetDbContext(), "Unexpected exception");
    }

    /// <summary>
    /// Test GetDbContext with Null IQueryable source parameter.
    /// </summary>
    [TestMethod(DisplayName = "GetDbContext with Null IQueryable source parameter")]
    public void GetDbContextTests_002()
    {
        // ARRANGE
        IQueryable source = null!;

        // ACT / ASSERT
        Assert.ThrowsExactly<ArgumentNullException>(() => _ = source.GetDbContext(), "Unexpected exception");
    }

    /// <summary>
    /// Test GetDbContext with General IQueryable (Non EF Core) source parameter.
    /// </summary>
    [TestMethod(DisplayName = "GetDbContext with General IQueryable (Non EF Core) source parameter")]
    public void GetDbContextTests_003()
    {
        // ARRANGE
        IQueryable<string> source = new List<string>().AsQueryable();

        // ACT / ASSERT
        Assert.ThrowsExactly<ArgumentException>(() => _ = source.GetDbContext(), "Unexpected exception");
    }

    /// <summary>
    /// Test GetDbContext with DatabaseFacade Database parameter.
    /// </summary>
    /// <param name="databaseType">Database type.</param>
    /// <returns>A <see cref="Task"/> that represents the asynchronous operation.</returns>
    [TestMethod(DisplayName = "GetDbContext with DatabaseFacade Database parameter")]
    [DataRow(DatabaseTypes.Sqlite, DisplayName = DatabaseTypes.Sqlite)]
    [DataRow(DatabaseTypes.SqlServer, DisplayName = DatabaseTypes.SqlServer)]
    public async Task GetDbContextTests_004_Async(string databaseType)
    {
        // ARRANGE
        using Context context = await DatabaseUtils.CreateDatabaseAsync(databaseType).ConfigureAwait(false);

        // ACT
        DbContext result = context.Database.GetDbContext();

        // ASSERT
        Assert.AreSame(context, result, "Invalid context object");
    }

    /// <summary>
    /// Test GetDbContext with DbSet object.
    /// </summary>
    /// <param name="databaseType">Database type.</param>
    /// <returns>A <see cref="Task"/> that represents the asynchronous operation.</returns>
    [TestMethod(DisplayName = "GetDbContext with DbSet object")]
    [DataRow(DatabaseTypes.Sqlite, DisplayName = DatabaseTypes.Sqlite)]
    [DataRow(DatabaseTypes.SqlServer, DisplayName = DatabaseTypes.SqlServer)]
    public async Task GetDbContextTests_005_Async(string databaseType)
    {
        // ARRANGE
        using Context context = await DatabaseUtils.CreateDatabaseAsync(databaseType).ConfigureAwait(false);
        IQueryable<TestTable1> source = context.TestTable1;

        // ACT
        DbContext result = source.GetDbContext();

        // ASSERT
        Assert.AreSame(context, result, "Invalid context object");
    }

    /// <summary>
    /// Test GetDbContext with IQueryable object.
    /// </summary>
    /// <param name="databaseType">Database type.</param>
    /// <returns>A <see cref="Task"/> that represents the asynchronous operation.</returns>
    [TestMethod(DisplayName = "GetDbContext with IQueryable object")]
    [DataRow(DatabaseTypes.Sqlite, DisplayName = DatabaseTypes.Sqlite)]
    [DataRow(DatabaseTypes.SqlServer, DisplayName = DatabaseTypes.SqlServer)]
    public async Task GetDbContextTests_006_Async(string databaseType)
    {
        // ARRANGE
        using Context context = await DatabaseUtils.CreateDatabaseAsync(databaseType).ConfigureAwait(false);
        IQueryable<TestTable1> source = context.TestTable1.Where(e => e.TestField == "test");

        // ACT
        DbContext result = source.GetDbContext();

        // ASSERT
        Assert.AreSame(context, result, "Invalid context object");
    }
}
