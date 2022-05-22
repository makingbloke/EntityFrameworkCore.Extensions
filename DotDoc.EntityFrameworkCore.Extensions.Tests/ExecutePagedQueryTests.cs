// Copyright ©2021-2022 Mike King.
// This file is licensed to you under the MIT license.
// See the License.txt file in the solution root for more information.

using DotDoc.EntityFrameworkCore.Extensions.Constants;
using DotDoc.EntityFrameworkCore.Extensions.Model;
using DotDoc.EntityFrameworkCore.Extensions.Tests.Data;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DotDoc.EntityFrameworkCore.Extensions.Tests;

/// <summary>
/// Tests for ExecutePagedQuery extensions.
/// </summary>
[TestClass]
public class ExecutePagedQueryTests
{
    /// <summary>
    /// Test ExecutePagedQueryInterpolated.
    /// </summary>
    /// <param name="databaseType">Database type.</param>
    [TestMethod]
    [DataRow(DatabaseType.Sqlite)]
    [DataRow(DatabaseType.SqlServer)]
    public void ExecutePagedQueryInterpolatedTest(DatabaseType databaseType)
    {
        const string value = nameof(this.ExecutePagedQueryInterpolatedTest);
        const int recordCount = 20;
        const long page = 2;
        const long pageSize = 5;

        using Context context = DatabaseUtils.CreateDatabase(databaseType);
        DatabaseUtils.CreateMultipleTestTableEntries(context, value, recordCount);

        QueryPage queryPage = context.Database.ExecutePagedQueryInterpolated($"SELECT * FROM TestTable WHERE ID <= {recordCount}", page, pageSize);

        Assert.AreEqual(page, queryPage.Page);
        Assert.AreEqual(pageSize, queryPage.PageSize);
        Assert.AreEqual(recordCount, queryPage.RecordCount);
        Assert.AreEqual((recordCount + pageSize - 1) / pageSize, queryPage.PageCount);
        Assert.AreEqual(pageSize, queryPage.DataTable.Rows.Count);

        for (int i = 0; i < pageSize; i++)
        {
            Assert.AreEqual((page * pageSize) + i + 1, queryPage.DataTable.Rows[i]["ID"]);
        }
    }

    /// <summary>
    /// Test ExecutePagedQueryRaw.
    /// </summary>
    /// <param name="databaseType">Database type.</param>
    [TestMethod]
    [DataRow(DatabaseType.Sqlite)]
    [DataRow(DatabaseType.SqlServer)]
    public void ExecutePagedQueryRawTest(DatabaseType databaseType)
    {
        const string value = nameof(this.ExecutePagedQueryInterpolatedTest);
        const int recordCount = 20;
        const long page = 2;
        const long pageSize = 5;

        using Context context = DatabaseUtils.CreateDatabase(databaseType);
        DatabaseUtils.CreateMultipleTestTableEntries(context, value, recordCount);

        QueryPage queryPage = context.Database.ExecutePagedQueryRaw("SELECT * FROM TestTable WHERE ID <= {0}", page, pageSize, recordCount);

        Assert.AreEqual(page, queryPage.Page);
        Assert.AreEqual(pageSize, queryPage.PageSize);
        Assert.AreEqual(recordCount, queryPage.RecordCount);
        Assert.AreEqual((recordCount + pageSize - 1) / pageSize, queryPage.PageCount);
        Assert.AreEqual(pageSize, queryPage.DataTable.Rows.Count);

        for (int i = 0; i < pageSize; i++)
        {
            Assert.AreEqual((page * pageSize) + i + 1, queryPage.DataTable.Rows[i]["ID"]);
        }
    }

    /// <summary>
    /// Test ExecutePagedQueryInterpolatedAsync.
    /// </summary>
    /// <param name="databaseType">Database type.</param>
    /// <returns><see cref="Task"/>.</returns>
    [TestMethod]
    [DataRow(DatabaseType.Sqlite)]
    [DataRow(DatabaseType.SqlServer)]
    public async Task ExecutePagedQueryInterpolatedTestAsync(DatabaseType databaseType)
    {
        const string value = nameof(this.ExecutePagedQueryInterpolatedTest);
        const int recordCount = 20;
        const long page = 2;
        const long pageSize = 5;

        using Context context = DatabaseUtils.CreateDatabase(databaseType);
        DatabaseUtils.CreateMultipleTestTableEntries(context, value, recordCount);

        QueryPage queryPage = await context.Database.ExecutePagedQueryInterpolatedAsync($"SELECT * FROM TestTable WHERE ID <= {recordCount}", page, pageSize).ConfigureAwait(false);

        Assert.AreEqual(page, queryPage.Page);
        Assert.AreEqual(pageSize, queryPage.PageSize);
        Assert.AreEqual(recordCount, queryPage.RecordCount);
        Assert.AreEqual((recordCount + pageSize - 1) / pageSize, queryPage.PageCount);
        Assert.AreEqual(pageSize, queryPage.DataTable.Rows.Count);

        for (int i = 0; i < pageSize; i++)
        {
            Assert.AreEqual((page * pageSize) + i + 1, queryPage.DataTable.Rows[i]["ID"]);
        }
    }

    /// <summary>
    /// Test ExecutePagedQueryRawAsync.
    /// </summary>
    /// <param name="databaseType">Database type.</param>
    /// <returns><see cref="Task"/>.</returns>
    [TestMethod]
    [DataRow(DatabaseType.Sqlite)]
    [DataRow(DatabaseType.SqlServer)]
    public async Task ExecutePagedQueryRawTestAsync(DatabaseType databaseType)
    {
        const string value = nameof(this.ExecutePagedQueryInterpolatedTest);
        const int recordCount = 20;
        const long page = 2;
        const long pageSize = 5;

        using Context context = DatabaseUtils.CreateDatabase(databaseType);
        DatabaseUtils.CreateMultipleTestTableEntries(context, value, recordCount);

        QueryPage queryPage = await context.Database.ExecutePagedQueryRawAsync("SELECT * FROM TestTable WHERE ID <= {0}", page, pageSize, parameters: recordCount).ConfigureAwait(false);

        Assert.AreEqual(page, queryPage.Page);
        Assert.AreEqual(pageSize, queryPage.PageSize);
        Assert.AreEqual(recordCount, queryPage.RecordCount);
        Assert.AreEqual((recordCount + pageSize - 1) / pageSize, queryPage.PageCount);
        Assert.AreEqual(pageSize, queryPage.DataTable.Rows.Count);

        for (int i = 0; i < pageSize; i++)
        {
            Assert.AreEqual((page * pageSize) + i + 1, queryPage.DataTable.Rows[i]["ID"]);
        }
    }

    /// <summary>
    /// Test the code that splits SQL queries and handles Order By clauses.
    /// </summary>
    /// <param name="databaseType">Database type.</param>
    [TestMethod]
    [DataRow(DatabaseType.Sqlite)]
    [DataRow(DatabaseType.SqlServer)]
    public void ExecutePagedQueryOrderByTest(DatabaseType databaseType)
    {
        const string value = nameof(this.ExecutePagedQueryOrderByTest);
        const int recordCount = 20;
        const long page = 2;
        const long pageSize = 5;

        using Context context = DatabaseUtils.CreateDatabase(databaseType);
        DatabaseUtils.CreateMultipleTestTableEntries(context, value, recordCount);

        QueryPage queryPage = context.Database.ExecutePagedQueryInterpolated($"SELECT * FROM TestTable WHERE ID <= {recordCount} ORDER BY ID", page, pageSize);

        Assert.AreEqual(page, queryPage.Page);
    }

    /// <summary>
    /// Test if the requested page number is greater than the total number of pages.
    /// The page number should be reduced to the last one.
    /// </summary>
    /// <param name="databaseType">Database type.</param>
    [TestMethod]
    [DataRow(DatabaseType.Sqlite)]
    [DataRow(DatabaseType.SqlServer)]
    public void ExecutePagedQueryPageNumberOverFlow(DatabaseType databaseType)
    {
        const string value = nameof(this.ExecutePagedQueryPageNumberOverFlow);
        const int recordCount = 20;
        const long page = 999;
        const long pageSize = 5;

        using Context context = DatabaseUtils.CreateDatabase(databaseType);
        DatabaseUtils.CreateMultipleTestTableEntries(context, value, recordCount);

        QueryPage queryPage = context.Database.ExecutePagedQueryInterpolated($"SELECT * FROM TestTable WHERE ID <= {recordCount}", page, pageSize);

        Assert.AreEqual(((recordCount + pageSize - 1) / pageSize) - 1, queryPage.Page);
    }
}
