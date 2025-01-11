// Copyright ©2021-2025 Mike King.
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
    [TestMethod]
    [DataRow(DatabaseType.Sqlite, DisplayName = "SQLite ExecuteUpdate with expression in SetProperty.")]
    [DataRow(DatabaseType.SqlServer, DisplayName = "SQL Server ExecuteUpdate with expression in SetProperty.")]
    public void Test_ExecuteUpdate_Expression(DatabaseType databaseType)
    {
        using Context context = DatabaseUtils.CreateDatabase(databaseType);

        string value = DatabaseUtils.GetMethodName();
        string originalValue = $"Original {value}";
        string updatedValue = $"Updated {value}";
        long id = DatabaseUtils.CreateSingleTestTableEntry(context, originalValue);

        IQueryable<TestTable1> query = context.TestTable1.Where(e => e.Id == id);

        int count = query.ExecuteUpdate(builder =>
        {
            builder.SetProperty(e => e.TestField, e => updatedValue);
        });

        Assert.AreEqual(1, count, "Invalid count");

        // EF Core keeps the original record in it's cache. We could destroy and create a new context to clear it.
        // But in this case it's easier just to fire an ExecuteScalar to get the updated value.
        string actualValue = context.Database.ExecuteScalar<string>($"SELECT TestField FROM TestTable1 WHERE Id = {id}");
        Assert.AreEqual(updatedValue, actualValue, "Unexpected field value");
    }

    /// <summary>
    /// Test ExecuteUpdate with an expression parameter in SetProperty.
    /// </summary>
    /// <param name="databaseType">Database type.</param>
    /// <returns>A task that represents the asynchronous test operation.</returns>
    [TestMethod]
    [DataRow(DatabaseType.Sqlite, DisplayName = "SQLite ExecuteUpdate with expression in SetProperty.")]
    [DataRow(DatabaseType.SqlServer, DisplayName = "SQL Server ExecuteUpdate with expression in SetProperty.")]
    public async Task Test_ExecuteUpdate_ExpressionAsync(DatabaseType databaseType)
    {
        using Context context = DatabaseUtils.CreateDatabase(databaseType);

        string value = DatabaseUtils.GetMethodName();
        string originalValue = $"Original {value}";
        string updatedValue = $"Updated {value}";
        long id = DatabaseUtils.CreateSingleTestTableEntry(context, originalValue);

        IQueryable<TestTable1> query = context.TestTable1.Where(e => e.Id == id);

        int count = await query.ExecuteUpdateAsync(builder =>
        {
            builder.SetProperty(e => e.TestField, e => updatedValue);
        }).ConfigureAwait(false);

        Assert.AreEqual(1, count, "Invalid count");

        // EF Core keeps the original record in it's cache. We could destroy and create a new context to clear it.
        // But in this case it's easier just to fire an ExecuteScalar to get the updated value.
        string actualValue = await context.Database.ExecuteScalarAsync<string>($"SELECT TestField FROM TestTable1 WHERE Id = {id}").ConfigureAwait(false);
        Assert.AreEqual(updatedValue, actualValue, "Unexpected field value");
    }

    /// <summary>
    /// Test ExecuteUpdate with an generic value parameter in SetProperty.
    /// </summary>
    /// <param name="databaseType">Database type.</param>
    [TestMethod]
    [DataRow(DatabaseType.Sqlite, DisplayName = "SQLite ExecuteUpdate with generic in SetProperty.")]
    [DataRow(DatabaseType.SqlServer, DisplayName = "SQL Server ExecuteUpdate with generic in SetProperty.")]
    public void Test_ExecuteUpdate_Generic(DatabaseType databaseType)
    {
        using Context context = DatabaseUtils.CreateDatabase(databaseType);

        string value = DatabaseUtils.GetMethodName();
        string originalValue = $"Original {value}";
        string updatedValue = $"Updated {value}";
        long id = DatabaseUtils.CreateSingleTestTableEntry(context, originalValue);

        IQueryable<TestTable1> query = context.TestTable1.Where(e => e.Id == id);

        int count = query.ExecuteUpdate(builder =>
        {
            builder.SetProperty<string>(e => e.TestField, updatedValue);
        });

        Assert.AreEqual(1, count, "Invalid count");

        // EF Core keeps the original record in it's cache. We could destroy and create a new context to clear it.
        // But in this case it's easier just to fire an ExecuteScalar to get the updated value.
        string actualValue = context.Database.ExecuteScalar<string>($"SELECT TestField FROM TestTable1 WHERE Id = {id}");
        Assert.AreEqual(updatedValue, actualValue, "Unexpected field value");
    }

    /// <summary>
    /// Test ExecuteUpdate with an generic value parameter in SetProperty.
    /// </summary>
    /// <param name="databaseType">Database type.</param>
    /// <returns>A task that represents the asynchronous test operation.</returns>
    [TestMethod]
    [DataRow(DatabaseType.Sqlite, DisplayName = "SQLite ExecuteUpdate with generic in SetProperty.")]
    [DataRow(DatabaseType.SqlServer, DisplayName = "SQL Server ExecuteUpdate with generic in SetProperty.")]
    public async Task Test_ExecuteUpdate_GenericAsync(DatabaseType databaseType)
    {
        using Context context = DatabaseUtils.CreateDatabase(databaseType);

        string value = DatabaseUtils.GetMethodName();
        string originalValue = $"Original {value}";
        string updatedValue = $"Updated {value}";
        long id = DatabaseUtils.CreateSingleTestTableEntry(context, originalValue);

        IQueryable<TestTable1> query = context.TestTable1.Where(e => e.Id == id);

        int count = await query.ExecuteUpdateAsync(builder =>
        {
            builder.SetProperty<string>(e => e.TestField, updatedValue);
        }).ConfigureAwait(false);

        Assert.AreEqual(1, count, "Invalid count");

        // EF Core keeps the original record in it's cache. We could destroy and create a new context to clear it.
        // But in this case it's easier just to fire an ExecuteScalar to get the updated value.
        string actualValue = await context.Database.ExecuteScalarAsync<string>($"SELECT TestField FROM TestTable1 WHERE Id = {id}").ConfigureAwait(false);
        Assert.AreEqual(updatedValue, actualValue, "Unexpected field value");
    }
}
