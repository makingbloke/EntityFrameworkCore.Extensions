// Copyright ©2021-2025 Mike King.
// This file is licensed to you under the MIT license.
// See the License.txt file in the solution root for more information.

using DotDoc.EntityFrameworkCore.Extensions.Constants;
using DotDoc.EntityFrameworkCore.Extensions.Extensions;
using DotDoc.EntityFrameworkCore.Extensions.Tests.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DotDoc.EntityFrameworkCore.Extensions.Tests.GetContext;

/// <summary>
/// Tests for GetContext extensions.
/// </summary>
[TestClass]
public class GetContextTests
{
    #region public methods

    /// <summary>
    /// Test GetContext with <see cref="DbSet{TestTable1}"/>.
    /// </summary>
    /// <param name="databaseType">Database type.</param>
    [TestMethod]
    [DataRow(DatabaseType.Sqlite, DisplayName = "SQLite GetContext DbSet.")]
    [DataRow(DatabaseType.SqlServer, DisplayName = "SQL Server GetContext DbSet.")]
    public void Test_GetContext_DbSet(string databaseType)
    {
        // ARRANGE
        using Context context = DatabaseUtils.CreateDatabase(databaseType);

        IQueryable<TestTable1> query = context.TestTable1;

        // ACT
        DbContext result = query.GetContext();

        // ASSERT
        Assert.AreSame(context, result, "Invalid context object value.");
    }

    /// <summary>
    /// Test GetContext with EF Core IQueryable.
    /// </summary>
    /// <param name="databaseType">Database type.</param>
    [TestMethod]
    [DataRow(DatabaseType.Sqlite, DisplayName = "SQLite GetContext EF Core IQueryable.")]
    [DataRow(DatabaseType.SqlServer, DisplayName = "SQL Server GetContext EF Core IQueryable.")]
    public void Test_GetContext_IQueryable(string databaseType)
    {
        // ARRANGE
        using Context context = DatabaseUtils.CreateDatabase(databaseType);

        IQueryable<TestTable1> query = context.TestTable1.Where(e => e.TestField == "test");

        // ACT
        DbContext result = query.GetContext();

        // ASSERT
        Assert.AreSame(context, result, "Invalid context object value.");
    }

    /// <summary>
    /// Test GetContext with General (Non EF Core) IQueryable.
    /// </summary>
    /// <param name="databaseType">Database type.</param>
    [TestMethod]
    [DataRow(DatabaseType.Sqlite, DisplayName = "SQLite GetContext General IQueryable.")]
    [DataRow(DatabaseType.SqlServer, DisplayName = "SQL Server GetContext General IQueryable.")]
    public void Test_GetContext_GeneralIQueryable(string databaseType)
    {
        // ARRANGE
        using Context context = DatabaseUtils.CreateDatabase(databaseType);

        IQueryable<string> query = new List<string>().AsQueryable();

        // ACT / ASSERT
        Assert.ThrowsException<ArgumentException>(() => query.GetContext(), "Unexpected exception");
    }

    /// <summary>
    /// Test GetContext with DatabaseFacade.
    /// </summary>
    /// <param name="databaseType">Database type.</param>
    [TestMethod]
    [DataRow(DatabaseType.Sqlite, DisplayName = "SQLite GetContext DatabaseFacade.")]
    [DataRow(DatabaseType.SqlServer, DisplayName = "SQL Server GetContext DatabaseFacade.")]
    public void Test_GetContext_DatabaseFacade(string databaseType)
    {
        // ARRANGE
        using Context context = DatabaseUtils.CreateDatabase(databaseType);

        // ACT
        // In real life, the GetContext method will never be called in this scenario as we already have a context.
        // However, this method will probably only be useful in methods (see ExecuteQuery<> in QueryMethods.cs)
        // where a DatabaseFacade is passed in but no DbContext.
        DbContext result = context.Database.GetContext();

        // ASSERT
        Assert.AreSame(context, result, "Invalid context object value.");
    }

    #endregion public methods
}
