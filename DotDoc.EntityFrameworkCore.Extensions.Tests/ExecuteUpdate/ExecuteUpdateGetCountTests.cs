// Copyright ©2021-2025 Mike King.
// This file is licensed to you under the MIT license.
// See the License.txt file in the solution root for more information.

using DotDoc.EntityFrameworkCore.Extensions.Classes;
using DotDoc.EntityFrameworkCore.Extensions.Constants;
using DotDoc.EntityFrameworkCore.Extensions.Extensions;
using DotDoc.EntityFrameworkCore.Extensions.Tests.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DotDoc.EntityFrameworkCore.Extensions.Tests.ExecuteUpdate;

/// <summary>
/// Tests for ExecuteUpdateGetCount extensions.
/// </summary>
[TestClass]
public class ExecuteUpdateGetCountTests
{
    #region public methods

    /// <summary>
    /// Test ExecuteUpdateGetCount with an expression parameter in SetProperty.
    /// </summary>
    /// <param name="databaseType">Database type.</param>
    [TestMethod]
    [DataRow(DatabaseType.Sqlite, DisplayName = "SQLite ExecuteUpdateGetCount with expression in SetProperty.")]
    [DataRow(DatabaseType.SqlServer, DisplayName = "SQL Server ExecuteUpdateGetCount with expression in SetProperty.")]
    public void Test_ExecuteUpdateGetCount_Expression(string databaseType)
    {
        // ARRANGE
        using Context context = DatabaseUtils.CreateDatabase(databaseType);

        string value = DatabaseUtils.GetMethodName();
        string originalValue = $"Original {value}";
        string updatedValue = $"Updated {value}";
        long id = DatabaseUtils.CreateSingleTestTableEntry(context, originalValue);

        IQueryable<TestTable1> query = context.TestTable1.Where(e => e.Id == id);

        // ACT
        int count = query.ExecuteUpdateGetCount(builder =>
        {
            builder.SetProperty(e => e.TestField, e => updatedValue);
        });

        // ASSERT
        Assert.AreEqual(1, count, "Invalid count");

        // EF Core keeps the original record in it's cache. We could destroy and create a new context to clear it.
        // But in this case it's easier just to fire an ExecuteScalar to get the updated value.
        string actualValue = context.Database.ExecuteScalar<string>($"SELECT TestField FROM TestTable1 WHERE Id = {id}")!;
        Assert.AreEqual(updatedValue, actualValue, "Unexpected field value");
    }

    /// <summary>
    /// Test ExecuteUpdateGetCount with an expression parameter in SetProperty.
    /// </summary>
    /// <param name="databaseType">Database type.</param>
    /// <returns>A task that represents the asynchronous test operation.</returns>
    [TestMethod]
    [DataRow(DatabaseType.Sqlite, DisplayName = "SQLite ExecuteUpdateGetCount with expression in SetProperty.")]
    [DataRow(DatabaseType.SqlServer, DisplayName = "SQL Server ExecuteUpdateGetCount with expression in SetProperty.")]
    public async Task Test_ExecuteUpdateGetCount_ExpressionAsync(string databaseType)
    {
        // ARRANGE
        using Context context = DatabaseUtils.CreateDatabase(databaseType);

        string value = DatabaseUtils.GetMethodName();
        string originalValue = $"Original {value}";
        string updatedValue = $"Updated {value}";
        long id = DatabaseUtils.CreateSingleTestTableEntry(context, originalValue);

        IQueryable<TestTable1> query = context.TestTable1.Where(e => e.Id == id);

        // ACT
        int count = await query.ExecuteUpdateGetCountAsync(builder =>
        {
            builder.SetProperty(e => e.TestField, e => updatedValue);
        }).ConfigureAwait(false);

        // ASSERT
        Assert.AreEqual(1, count, "Invalid count");

        // EF Core keeps the original record in it's cache. We could destroy and create a new context to clear it.
        // But in this case it's easier just to fire an ExecuteScalar to get the updated value.
        string actualValue = (await context.Database.ExecuteScalarAsync<string>($"SELECT TestField FROM TestTable1 WHERE Id = {id}").ConfigureAwait(false))!;
        Assert.AreEqual(updatedValue, actualValue, "Unexpected field value");
    }

    /// <summary>
    /// Test ExecuteUpdateGetCount with an generic value parameter in SetProperty.
    /// </summary>
    /// <param name="databaseType">Database type.</param>
    [TestMethod]
    [DataRow(DatabaseType.Sqlite, DisplayName = "SQLite ExecuteUpdateGetCount with generic in SetProperty.")]
    [DataRow(DatabaseType.SqlServer, DisplayName = "SQL Server ExecuteUpdateGetCount with generic in SetProperty.")]
    public void Test_ExecuteUpdateGetCount_Generic(string databaseType)
    {
        // ARRANGE
        using Context context = DatabaseUtils.CreateDatabase(databaseType);

        string value = DatabaseUtils.GetMethodName();
        string originalValue = $"Original {value}";
        string updatedValue = $"Updated {value}";
        long id = DatabaseUtils.CreateSingleTestTableEntry(context, originalValue);

        IQueryable<TestTable1> query = context.TestTable1.Where(e => e.Id == id);

        // ACT
        int count = query.ExecuteUpdateGetCount(builder =>
        {
            builder.SetProperty(e => e.TestField, updatedValue);
        });

        // ASSERT
        Assert.AreEqual(1, count, "Invalid count");

        // EF Core keeps the original record in it's cache. We could destroy and create a new context to clear it.
        // But in this case it's easier just to fire an ExecuteScalar to get the updated value.
        string actualValue = context.Database.ExecuteScalar<string>($"SELECT TestField FROM TestTable1 WHERE Id = {id}")!;
        Assert.AreEqual(updatedValue, actualValue, "Unexpected field value");
    }

    /// <summary>
    /// Test ExecuteUpdateGetCount with an generic value parameter in SetProperty.
    /// </summary>
    /// <param name="databaseType">Database type.</param>
    /// <returns>A task that represents the asynchronous test operation.</returns>
    [TestMethod]
    [DataRow(DatabaseType.Sqlite, DisplayName = "SQLite ExecuteUpdateGetCount with generic in SetProperty.")]
    [DataRow(DatabaseType.SqlServer, DisplayName = "SQL Server ExecuteUpdateGetCount with generic in SetProperty.")]
    public async Task Test_ExecuteUpdateGetCount_GenericAsync(string databaseType)
    {
        // ARRANGE
        using Context context = DatabaseUtils.CreateDatabase(databaseType);

        string value = DatabaseUtils.GetMethodName();
        string originalValue = $"Original {value}";
        string updatedValue = $"Updated {value}";
        long id = DatabaseUtils.CreateSingleTestTableEntry(context, originalValue);

        IQueryable<TestTable1> query = context.TestTable1.Where(e => e.Id == id);

        // ACT
        int count = await query.ExecuteUpdateGetCountAsync(builder =>
        {
            builder.SetProperty(e => e.TestField, updatedValue);
        }).ConfigureAwait(false);

        // ASSERT
        Assert.AreEqual(1, count, "Invalid count");

        // EF Core keeps the original record in it's cache. We could destroy and create a new context to clear it.
        // But in this case it's easier just to fire an ExecuteScalar to get the updated value.
        string actualValue = (await context.Database.ExecuteScalarAsync<string>($"SELECT TestField FROM TestTable1 WHERE Id = {id}").ConfigureAwait(false))!;
        Assert.AreEqual(updatedValue, actualValue, "Unexpected field value");
    }

    #endregion public methods
}
