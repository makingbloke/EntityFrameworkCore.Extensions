// Copyright ©2021-2026 Mike King.
// This file is licensed to you under the MIT license.
// See the License.txt file in the solution root for more information.

using DotDoc.EntityFrameworkCore.Extensions.DatabaseType;
using DotDoc.EntityFrameworkCore.Extensions.Tests.Data;
using DotDoc.EntityFrameworkCore.Extensions.Tests.Model;
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
    #region private fields

    /// <summary>
    /// The MSTest <see cref="TestContext"/> for the current test.
    /// </summary>
    private readonly TestContext _testContext;

    #endregion private fields

    #region public constructors

    /// <summary>
    /// Initializes a new instance of the <see cref="GetDbContextTests"/> class.
    /// </summary>
    /// <param name="testContext">The MSTest test context.</param>
    public GetDbContextTests(TestContext testContext)
    {
        this._testContext = testContext;
    }

    #endregion public constructors

    #region GetDbContext tests

    /// <summary>
    /// Test GetDbContext with Null DatabaseFacade Database parameter.
    /// </summary>
    [TestMethod]
    public void GetDbContext_NullDatabase_ThrowsArgumentNullException()
    {
        // ARRANGE
        DatabaseFacade database = null!;

        // ACT / ASSERT
        Assert.ThrowsExactly<ArgumentNullException>(() => database.GetDbContext(), "Unexpected exception");
    }

    /// <summary>
    /// Test GetDbContext with Null IQueryable source parameter.
    /// </summary>
    [TestMethod]
    public void GetDbContext_NullSource_ThrowsArgumentNullException()
    {
        // ARRANGE
        IQueryable source = null!;

        // ACT / ASSERT
        Assert.ThrowsExactly<ArgumentNullException>(() => source.GetDbContext(), "Unexpected exception");
    }

    /// <summary>
    /// Test GetDbContext with General IQueryable (Non EF Core) source parameter.
    /// </summary>
    [TestMethod]
    public void GetDbContext_NonEfCoreSource_ThrowsArgumentException()
    {
        // ARRANGE
        IQueryable<string> source = new List<string>().AsQueryable();

        // ACT / ASSERT
        Assert.ThrowsExactly<ArgumentException>(() => source.GetDbContext(), "Unexpected exception");
    }

    /// <summary>
    /// Test GetDbContext with DatabaseFacade Database parameter.
    /// </summary>
    /// <param name="databaseType">Database type.</param>
    /// <returns>A <see cref="Task"/> that represents the asynchronous operation.</returns>
    [TestMethod]
    [DataRow(DatabaseTypes.Sqlite, DisplayName = $"DatabaseFacade {DatabaseTypes.Sqlite}")]
    [DataRow(DatabaseTypes.SqlServer, DisplayName = $"DatabaseFacade {DatabaseTypes.SqlServer}")]
    public async Task GetDbContext_DatabaseFacade_ReturnsContext(string databaseType)
    {
        // ARRANGE
        using Context context = await DatabaseUtils.OpenDatabaseAsync(this._testContext, databaseType).ConfigureAwait(false);

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
    [TestMethod]
    [DataRow(DatabaseTypes.Sqlite, DisplayName = $"DbSet {DatabaseTypes.Sqlite}")]
    [DataRow(DatabaseTypes.SqlServer, DisplayName = $"DbSet {DatabaseTypes.SqlServer}")]
    public async Task GetDbContext_DbSet_ReturnsContext(string databaseType)
    {
        // ARRANGE
        using Context context = await DatabaseUtils.OpenDatabaseAsync(this._testContext, databaseType).ConfigureAwait(false);
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
    [TestMethod]
    [DataRow(DatabaseTypes.Sqlite, DisplayName = $"Queryable {DatabaseTypes.Sqlite}")]
    [DataRow(DatabaseTypes.SqlServer, DisplayName = $"Queryable {DatabaseTypes.SqlServer}")]
    public async Task GetDbContext_Queryable_ReturnsContext(string databaseType)
    {
        // ARRANGE
        using Context context = await DatabaseUtils.OpenDatabaseAsync(this._testContext, databaseType).ConfigureAwait(false);
        IQueryable<TestTable1> source = context.TestTable1.Where(e => e.TestField == "test");

        // ACT
        DbContext result = source.GetDbContext();

        // ASSERT
        Assert.AreSame(context, result, "Invalid context object");
    }

    #endregion GetDbContext tests
}
