// Copyright ©2021-2023 Mike King.
// This file is licensed to you under the MIT license.
// See the License.txt file in the solution root for more information.

using DotDoc.EntityFrameworkCore.Extensions.Constants;
using DotDoc.EntityFrameworkCore.Extensions.Tests.Data;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DotDoc.EntityFrameworkCore.Extensions.Tests.ExecuteUpdate;

/// <summary>
/// Tests for ExecuteUpdate extensions.
/// </summary>
[TestClass]
public class ExecuteUpdateTests
{
    /// <summary>
    /// Test ExecuteUpdate with an expression parameter in SetProperty.
    /// </summary>
    /// <param name="databaseType">Database type.</param>
    /// <param name="useAsync">If <see langword="true"/> then tests the async method.</param>
    /// <returns><see cref="Task"/>.</returns>
    [TestMethod]
    [DataRow(DatabaseType.Sqlite, false, DisplayName = "SQLite ExecuteUpdate with expression in SetProperty")]
    [DataRow(DatabaseType.Sqlite, true, DisplayName = "SQLite ExecuteUpdateAsync with expression in SetProperty")]
    [DataRow(DatabaseType.SqlServer, false, DisplayName = "SQL Server ExecuteUpdate with expression in SetProperty")]
    [DataRow(DatabaseType.SqlServer, true, DisplayName = "SQL Server ExecuteUpdateAsync with expression in SetProperty")]
    public async Task TestExecuteUpdateWithExpressionAsync(DatabaseType databaseType, bool useAsync)
    {
        string originalValue = $"Original {DatabaseUtils.GetMethodName()}";
        string updatedValue = $"Updated {DatabaseUtils.GetMethodName()}";

        using Context context = DatabaseUtils.CreateDatabase(databaseType);
        long id = DatabaseUtils.CreateSingleTestTableEntry(context, originalValue);

        IQueryable<TestTable1> query = context.TestTable1.Where(e => e.Id == id);

        int count;
        if (useAsync)
        {
            count = await query.ExecuteUpdateAsync(builder =>
            {
                builder.SetProperty(e => e.TestField, e => updatedValue);
            }).ConfigureAwait(false);
        }
        else
        {
            count = query.ExecuteUpdate(builder =>
            {
                builder.SetProperty(e => e.TestField, e => updatedValue);
            });
        }

        Assert.AreEqual(1, count, "Invalid count");

        // EF Core keeps the original record in it's cache. We could destroy and create a new context to clear it.
        // But in this case it's easier just to fire an ExecuteScalar to get the updated value.
        string actualValue = context.Database.ExecuteScalarInterpolated<string>($"SELECT TestField FROM TestTable1 WHERE Id = {id}");
        Assert.AreEqual(updatedValue, actualValue, "Unexpected field value");
    }

    /// <summary>
    /// Test ExecuteUpdate with an generic value parameter in SetProperty.
    /// </summary>
    /// <param name="databaseType">Database type.</param>
    /// <param name="useAsync">If <see langword="true"/> then tests the async method.</param>
    /// <returns><see cref="Task"/>.</returns>
    [TestMethod]
    [DataRow(DatabaseType.Sqlite, false, DisplayName = "SQLite ExecuteUpdate with generic in SetProperty")]
    [DataRow(DatabaseType.Sqlite, true, DisplayName = "SQLite ExecuteUpdateAsync with generic in SetProperty")]
    [DataRow(DatabaseType.SqlServer, false, DisplayName = "SQL Server ExecuteUpdate with generic in SetProperty")]
    [DataRow(DatabaseType.SqlServer, true, DisplayName = "SQL Server ExecuteUpdateAsync with generic in SetProperty")]
    public async Task TestExecuteUpdateWithGenericAsync(DatabaseType databaseType, bool useAsync)
    {
        string originalValue = $"Original {DatabaseUtils.GetMethodName()}";
        string updatedValue = $"Updated {DatabaseUtils.GetMethodName()}";

        using Context context = DatabaseUtils.CreateDatabase(databaseType);
        long id = DatabaseUtils.CreateSingleTestTableEntry(context, originalValue);

        IQueryable<TestTable1> query = context.TestTable1.Where(e => e.Id == id);

        int count;
        if (useAsync)
        {
            count = await query.ExecuteUpdateAsync(builder =>
            {
                builder.SetProperty<string>(e => e.TestField, updatedValue);
            }).ConfigureAwait(false);
        }
        else
        {
            count = query.ExecuteUpdate(builder =>
            {
                builder.SetProperty<string>(e => e.TestField, updatedValue);
            });
        }

        Assert.AreEqual(1, count, "Invalid count");

        // EF Core keeps the original record in it's cache. We could destroy and create a new context to clear it.
        // But in this case it's easier just to fire an ExecuteScalar to get the updated value.
        string actualValue = context.Database.ExecuteScalarInterpolated<string>($"SELECT TestField FROM TestTable1 WHERE Id = {id}");
        Assert.AreEqual(updatedValue, actualValue, "Unexpected field value");
    }
}
