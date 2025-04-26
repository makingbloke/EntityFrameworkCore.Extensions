// Copyright ©2021-2025 Mike King.
// This file is licensed to you under the MIT license.
// See the License.txt file in the solution root for more information.

using DotDoc.EntityFrameworkCore.Extensions.DatabaseType;
using DotDoc.EntityFrameworkCore.Extensions.GetContext;
using DotDoc.EntityFrameworkCore.Extensions.Tests.Data;
using DotDoc.EntityFrameworkCore.Extensions.Tests.Utilities;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DotDoc.EntityFrameworkCore.Extensions.Tests.GetContext;

/// <summary>
/// Tests for GetContext extensions.
/// </summary>
[TestClass]
public class GetContextTests
{
    /// <summary>
    /// Test GetContext with EF Core DBSet.
    /// </summary>
    /// <param name="databaseType">Database type.</param>
    [TestMethod]
    [DataRow(DatabaseTypes.Sqlite, DisplayName = $"{DatabaseTypes.Sqlite} GetContext EF Core DbSet.")]
    [DataRow(DatabaseTypes.SqlServer, DisplayName = $"{DatabaseTypes.SqlServer} GetContext EF Core DbSet.")]
    public void Test_GetContext_EfCoreDbSet(string databaseType)
    {
        using Context context = DatabaseUtils.CreateDatabase(databaseType);

        IQueryable<TestTable1> query = context.TestTable1;

        DbContext? result = GetContextExtensions.GetDbContext<TestTable1>(query);

        Assert.AreSame(context, result, "Invalid context object value.");
    }

    /// <summary>
    /// Test GetContext with EF Core IQueryable.
    /// </summary>
    /// <param name="databaseType">Database type.</param>
    [TestMethod]
    [DataRow(DatabaseTypes.Sqlite, DisplayName = $"{DatabaseTypes.Sqlite} GetContext EF Core IQueryable.")]
    [DataRow(DatabaseTypes.SqlServer, DisplayName = $"{DatabaseTypes.SqlServer} GetContext EF Core IQueryable.")]
    public void Test_GetContext_EfCoreIQueryable(string databaseType)
    {
        using Context context = DatabaseUtils.CreateDatabase(databaseType);

        IQueryable<TestTable1> query = context.TestTable1.Where(e => e.TestField == "test");

        DbContext? result = query.GetDbContext();

        Assert.AreSame(context, result, "Invalid context object value.");
    }

    /// <summary>
    /// Test GetContext with General (Non EF Core) IQueryable.
    /// </summary>
    /// <param name="databaseType">Database type.</param>
    [TestMethod]
    [DataRow(DatabaseTypes.Sqlite, DisplayName = $"{DatabaseTypes.Sqlite} GetContext General IQueryable.")]
    [DataRow(DatabaseTypes.SqlServer, DisplayName = $"{DatabaseTypes.SqlServer} GetContext General IQueryable.")]
    public void Test_GetContext_GeneralIQueryable(string databaseType)
    {
        using Context context = DatabaseUtils.CreateDatabase(databaseType);

        IQueryable<string> query = new List<string>().AsQueryable();

        DbContext? result = query.GetDbContext();

        Assert.IsNull(result, "Invalid context object value.");
    }
}
