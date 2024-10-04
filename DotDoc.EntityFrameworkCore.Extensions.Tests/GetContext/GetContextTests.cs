// Copyright ©2021-2024 Mike King.
// This file is licensed to you under the MIT license.
// See the License.txt file in the solution root for more information.

using DotDoc.EntityFrameworkCore.Extensions.Constants;
using DotDoc.EntityFrameworkCore.Extensions.Tests.Data;
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
    /// Test GetContext with EF Core IQueryable.
    /// </summary>
    /// <param name="databaseType">Database type.</param>
    [TestMethod]
    [DataRow(DatabaseType.Sqlite, DisplayName = "SQLite GetContext EF Core IQueryable.")]
    [DataRow(DatabaseType.SqlServer, DisplayName = "SQL Server GetContext EF Core IQueryable.")]
    public void Test_GetContext_EfCoreIQueryable(DatabaseType databaseType)
    {
        using Context context = DatabaseUtils.CreateDatabase(databaseType);

        IQueryable<TestTable1> query = context.TestTable1;

        DbContext result = query.GetContext();

        Assert.AreSame(context, result, "Invalid context object value.");
    }

    /// <summary>
    /// Test GetContext with General (Non EF Core) IQueryable.
    /// </summary>
    /// <param name="databaseType">Database type.</param>
    [TestMethod]
    [DataRow(DatabaseType.Sqlite, DisplayName = "SQLite GetContext General IQueryable.")]
    [DataRow(DatabaseType.SqlServer, DisplayName = "SQL Server GetContext General IQueryable.")]
    public void Test_GetContext_GeneralIQueryable(DatabaseType databaseType)
    {
        using Context context = DatabaseUtils.CreateDatabase(databaseType);

        IQueryable<string> query = new List<string>().AsQueryable();

        DbContext result = query.GetContext();

        Assert.IsNull(result, "Invalid context object value.");
    }
}
