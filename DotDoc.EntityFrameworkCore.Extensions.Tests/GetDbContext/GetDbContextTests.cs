// Copyright ©2021-2025 Mike King.
// This file is licensed to you under the MIT license.
// See the License.txt file in the solution root for more information.

using DotDoc.EntityFrameworkCore.Extensions.DatabaseType;
using DotDoc.EntityFrameworkCore.Extensions.Tests.Data;
using DotDoc.EntityFrameworkCore.Extensions.Tests.TestUtilities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace DotDoc.EntityFrameworkCore.Extensions.Tests.GetDbContext;

/// <summary>
/// Tests for GetDbContext extensions.
/// </summary>
[TestClass]
public class GetDbContextTests
{
    /// <summary>
    /// Test GetDbContext DatabaseFacade Guard Clause.
    /// </summary>
    [TestMethod(DisplayName = "GetDbContext DatabaseFacade Guard Clause")]
    public void Test_GetDbContext_DatabaseFacade_GuardClause()
    {
        // ARRANGE
        DatabaseFacade databaseFacade = null!;

        // ACT / ASSERT
        ArgumentNullException e = Assert.ThrowsExactly<ArgumentNullException>(() => _ = databaseFacade.GetDbContext(), "Unexpected exception");
        Assert.AreEqual(nameof(databaseFacade), e.ParamName, "Invalid parameter name");
    }

    /// <summary>
    /// Test GetDbContext IQueryable Guard Clause.
    /// </summary>
    [TestMethod(DisplayName = "GetDbContext IQueryable Guard Clause")]
    public void Test_GetDbContext_IQueryable_GuardClause()
    {
        // ARRANGE
        IQueryable<TestTable1> query = null!;

        // ACT / ASSERT
        ArgumentNullException e = Assert.ThrowsExactly<ArgumentNullException>(() => _ = query.GetDbContext(), "Unexpected exception");
        Assert.AreEqual(nameof(query), e.ParamName, "Invalid parameter name");
    }

    /// <summary>
    /// Test GetDbContext with General IQueryable (Non EF Core) Guard Clause.
    /// </summary>
    /// <param name="databaseType">Database type.</param>
    /// <returns>A <see cref="Task"/> that represents the asynchronous operation.</returns>
    [TestMethod(DisplayName = "GetDbContext General IQueryable Guard Clause")]
    [DataRow(DatabaseTypes.Sqlite, DisplayName = DatabaseTypes.Sqlite)]
    [DataRow(DatabaseTypes.SqlServer, DisplayName = DatabaseTypes.SqlServer)]
    public async Task Test_GetDbContext_GeneralIQueryable_GuardClause_Async(string databaseType)
    {
        // ARRANGE
        using Context context = await DatabaseUtils.CreateDatabaseAsync(databaseType).ConfigureAwait(false);
        IQueryable<string> query = new List<string>().AsQueryable();

        // ACT / ASSERT
        ArgumentException e = Assert.ThrowsExactly<ArgumentException>(() => _ = query.GetDbContext(), "Unexpected exception");
        Assert.AreEqual(nameof(query), e.ParamName, "Invalid parameter name");
    }

    /// <summary>
    /// Test GetDbContext with DatabaseFacade.
    /// </summary>
    /// <param name="databaseType">Database type.</param>
    /// <returns>A <see cref="Task"/> that represents the asynchronous operation.</returns>
    [TestMethod(DisplayName = "GetDbContext DatabaseFacade")]
    [DataRow(DatabaseTypes.Sqlite, DisplayName = DatabaseTypes.Sqlite)]
    [DataRow(DatabaseTypes.SqlServer, DisplayName = DatabaseTypes.SqlServer)]
    public async Task Test_GetDbContext_DatabaseFacade_Async(string databaseType)
    {
        // ARRANGE
        using Context context = await DatabaseUtils.CreateDatabaseAsync(databaseType).ConfigureAwait(false);

        // ACT
        DbContext result = context.Database.GetDbContext();

        // ASSERT
        Assert.AreSame(context, result, "Invalid context object value.");
    }

    /// <summary>
    /// Test GetDbContext with EF Core DBSet.
    /// </summary>
    /// <param name="databaseType">Database type.</param>
    /// <returns>A <see cref="Task"/> that represents the asynchronous operation.</returns>
    [TestMethod(DisplayName = "GetDbContext EF Core DbSet")]
    [DataRow(DatabaseTypes.Sqlite, DisplayName = DatabaseTypes.Sqlite)]
    [DataRow(DatabaseTypes.SqlServer, DisplayName = DatabaseTypes.SqlServer)]
    public async Task Test_GetDbContext_EfCoreDbSet_Async(string databaseType)
    {
        // ARRANGE
        using Context context = await DatabaseUtils.CreateDatabaseAsync(databaseType).ConfigureAwait(false);
        IQueryable<TestTable1> query = context.TestTable1;

        // ACT
        DbContext result = query.GetDbContext();

        // ASSERT
        Assert.AreSame(context, result, "Invalid context object value.");
    }

    /// <summary>
    /// Test GetDbContext with EF Core IQueryable.
    /// </summary>
    /// <param name="databaseType">Database type.</param>
    /// <returns>A <see cref="Task"/> that represents the asynchronous operation.</returns>
    [TestMethod(DisplayName = "GetDbContext EF Core IQueryable")]
    [DataRow(DatabaseTypes.Sqlite, DisplayName = DatabaseTypes.Sqlite)]
    [DataRow(DatabaseTypes.SqlServer, DisplayName = DatabaseTypes.SqlServer)]
    public async Task Test_GetDbContext_EfCoreIQueryable_Async(string databaseType)
    {
        // ARRANGE
        using Context context = await DatabaseUtils.CreateDatabaseAsync(databaseType).ConfigureAwait(false);
        IQueryable<TestTable1> query = context.TestTable1.Where(e => e.TestField == "test");

        // ACT
        DbContext result = query.GetDbContext();

        // ASSERT
        Assert.AreSame(context, result, "Invalid context object value.");
    }
}
