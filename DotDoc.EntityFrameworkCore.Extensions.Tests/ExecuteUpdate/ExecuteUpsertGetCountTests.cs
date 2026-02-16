// Copyright ©2021-2026 Mike King.
// This file is licensed to you under the MIT license.
// See the License.txt file in the solution root for more information.

using DotDoc.EntityFrameworkCore.Extensions.DatabaseType;
using DotDoc.EntityFrameworkCore.Extensions.ExecuteUpdate;
using DotDoc.EntityFrameworkCore.Extensions.Tests.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;

namespace DotDoc.EntityFrameworkCore.Extensions.Tests.ExecuteUpdate;

/// <summary>
/// Tests for ExecuteUpsertGetCount extensions.
/// </summary>
[TestClass]
public class ExecuteUpsertGetCountTests
{
    #region public methods

    /// <summary>
    /// Test ExecuteUpsertGetCountAsync with Null IQueryable source parameter.
    /// </summary>
    /// <returns>A <see cref="Task"/> that represents the asynchronous operation.</returns>
    [TestMethod(DisplayName = "ExecuteUpsertGetCountAsync with Null IQueryable source parameter")]
    public async Task ExecuteUpsertGetCountTests_001_Async()
    {
        // ARRANGE
        IQueryable<TestTable1> source = null!;
        Action<UpdateSettersBuilder<TestTable1>> setPropertyCalls = new(builder => { });

        // ACT / ASSERT
        await Assert.ThrowsExactlyAsync<ArgumentNullException>(() => source.ExecuteUpsertGetCountAsync(setPropertyCalls, cancellationToken: CancellationToken.None), "Unexpected exception").ConfigureAwait(false);
    }

    /// <summary>
    /// Test ExecuteUpsertGetCountAsync with Null UpsertSettersBuilder setPropertyCalls parameter.
    /// </summary>
    /// <returns>A <see cref="Task"/> that represents the asynchronous operation.</returns>
    [TestMethod(DisplayName = "ExecuteUpsertGetCountAsync with Null UpsertSettersBuilder setPropertyCalls parameter")]
    public async Task ExecuteUpsertGetCountTests_002_Async()
    {
        // ARRANGE
        IQueryable<TestTable1> source = Array.Empty<TestTable1>().AsQueryable();
        Action<UpdateSettersBuilder<TestTable1>> setPropertyCalls = null!;

        // ACT / ASSERT
        await Assert.ThrowsExactlyAsync<ArgumentNullException>(() => source.ExecuteUpsertGetCountAsync(setPropertyCalls, cancellationToken: CancellationToken.None), "Unexpected exception").ConfigureAwait(false);
    }

    /// <summary>
    /// Test ExecuteUpsertGetCountAsync update records with update parameters only.
    /// </summary>
    /// <param name="databaseType">Database type.</param>
    /// <param name="count">Number of records to upsert.</param>
    /// <returns>A <see cref="Task"/> that represents the asynchronous operation.</returns>
    [TestMethod(DisplayName = "ExecuteUpsertGetCountAsync with update parameters only")]
    [DataRow(DatabaseTypes.Sqlite, 1, DisplayName = $"{DatabaseTypes.Sqlite} Update 1 record")]
    [DataRow(DatabaseTypes.Sqlite, 10, DisplayName = $"{DatabaseTypes.Sqlite} Update 10 records")]
    [DataRow(DatabaseTypes.SqlServer, 1, DisplayName = $"{DatabaseTypes.SqlServer} Update 1 record")]
    [DataRow(DatabaseTypes.SqlServer, 10, DisplayName = $"{DatabaseTypes.SqlServer} Update 10 records")]
    public async Task ExecuteUpsertGetCountTests_002_Async(string databaseType, int count)
    {
        // ARRANGE
        using Context context = await DatabaseUtils.OpenDatabaseAsync(
            databaseType,
            customConfigurationActions: optionsBuilder => optionsBuilder.UseExecuteUpdateExtensions())
            .ConfigureAwait(false);

        string originalValue = "Original TestValue";
        string updatedValue = "Updated TestValue";

        await DatabaseUtils.CreateTestTableEntriesAsync(context, originalValue, (count + 1) * 10).ConfigureAwait(false);

        long startId = 2;
        long endId = startId + count;

        IQueryable<TestTable1> source = context.TestTable1.Where(e => e.Id >= startId && e.Id < endId);

        // ACT
        int upsertRowCount = await source.ExecuteUpsertGetCountAsync(
            updateSetPropertyCalls: builder =>
            {
                builder.SetProperty(e => e.TestField, updatedValue);
            },
            cancellationToken: CancellationToken.None)
            .ConfigureAwait(false);

        // ASSERT
        Assert.AreEqual(count, upsertRowCount, "Invalid count");

        IList<TestTable1> rows = await source
            .AsNoTracking()
            .ToListAsync(CancellationToken.None)
            .ConfigureAwait(false);

        foreach (TestTable1 row in rows)
        {
            Assert.AreEqual(updatedValue, row.TestField, "Unexpected field value");
        }
    }

    /// <summary>
    /// Test ExecuteUpsertGetCountAsync update records with update and insert parameters.
    /// </summary>
    /// <param name="databaseType">Database type.</param>
    /// <param name="count">Number of records to upsert.</param>
    /// <returns>A <see cref="Task"/> that represents the asynchronous operation.</returns>
    [TestMethod(DisplayName = "ExecuteUpsertGetCountAsync update records with update and insert parameters")]
    [DataRow(DatabaseTypes.Sqlite, 1, DisplayName = $"{DatabaseTypes.Sqlite} Update 1 record")]
    [DataRow(DatabaseTypes.Sqlite, 10, DisplayName = $"{DatabaseTypes.Sqlite} Update 10 records")]
    [DataRow(DatabaseTypes.SqlServer, 1, DisplayName = $"{DatabaseTypes.SqlServer} Update 1 record")]
    [DataRow(DatabaseTypes.SqlServer, 10, DisplayName = $"{DatabaseTypes.SqlServer} Update 10 records")]
    public async Task ExecuteUpsertGetCountTests_003_Async(string databaseType, int count)
    {
        // ARRANGE
        using Context context = await DatabaseUtils.OpenDatabaseAsync(
            databaseType,
            customConfigurationActions: optionsBuilder => optionsBuilder.UseExecuteUpdateExtensions())
            .ConfigureAwait(false);

        string originalValue = "Original TestValue";
        string updatedValue = "Updated TestValue";
        string insertedValue = "Inserted TestValue";

        await DatabaseUtils.CreateTestTableEntriesAsync(context, originalValue, (count + 1) * 10).ConfigureAwait(false);

        long startId = 2;
        long endId = startId + count;

        IQueryable<TestTable1> source = context.TestTable1.Where(e => e.Id >= startId && e.Id < endId);

        // ACT
        int upsertRowCount = await source.ExecuteUpsertGetCountAsync(
            updateSetPropertyCalls: builder =>
            {
                builder.SetProperty(e => e.TestField, updatedValue);
            },
            insertSetPropertyCalls: builder =>
            {
                builder.SetProperty(e => e.TestField, insertedValue);
            },
            cancellationToken: CancellationToken.None)
            .ConfigureAwait(false);

        // ASSERT
        Assert.AreEqual(count, upsertRowCount, "Invalid count");

        IList<TestTable1> rows = await source
            .AsNoTracking()
            .ToListAsync(CancellationToken.None)
            .ConfigureAwait(false);

        foreach (TestTable1 row in rows)
        {
            Assert.AreEqual(updatedValue, row.TestField, "Unexpected field value");
        }
    }

    /// <summary>
    /// Test ExecuteUpsertGetCountAsync insert record with update parameters only.
    /// </summary>
    /// <param name="databaseType">Database type.</param>
    /// <returns>A <see cref="Task"/> that represents the asynchronous operation.</returns>
    [TestMethod(DisplayName = "ExecuteUpsertGetCountAsync insert record with update parameters only")]
    [DataRow(DatabaseTypes.Sqlite, DisplayName = DatabaseTypes.Sqlite)]
    [DataRow(DatabaseTypes.SqlServer, DisplayName = DatabaseTypes.SqlServer)]
    public async Task ExecuteUpsertGetCountTests_004_Async(string databaseType)
    {
        // ARRANGE
        using Context context = await DatabaseUtils.OpenDatabaseAsync(
            databaseType,
            customConfigurationActions: optionsBuilder => optionsBuilder.UseExecuteUpdateExtensions())
            .ConfigureAwait(false);

        int count = 1;

        string insertedValue = "Inserted TestValue";

        IQueryable<TestTable1> source = context.TestTable1.Where(e => e.Id >= long.MaxValue);

        // ACT
        int upsertRowCount = await source.ExecuteUpsertGetCountAsync(
            updateSetPropertyCalls: builder =>
            {
                builder.SetProperty(e => e.TestField, insertedValue);
            },
            cancellationToken: CancellationToken.None)
            .ConfigureAwait(false);

        // ASSERT
        Assert.AreEqual(count, upsertRowCount, "Invalid count");

        List<TestTable1> rows = await context.TestTable1
            .AsNoTracking()
            .ToListAsync(CancellationToken.None)
            .ConfigureAwait(false);

        Assert.HasCount(count, rows, "Unexpected count");
        Assert.AreEqual(insertedValue, rows[0].TestField, "Unexpected field value");
    }

    /// <summary>
    /// Test ExecuteUpsertGetCountAsync insert record with update and insert parameters.
    /// </summary>
    /// <param name="databaseType">Database type.</param>
    /// <returns>A <see cref="Task"/> that represents the asynchronous operation.</returns>
    [TestMethod(DisplayName = "ExecuteUpsertGetCountAsync insert record with update and insert parameters")]
    [DataRow(DatabaseTypes.Sqlite, DisplayName = DatabaseTypes.Sqlite)]
    [DataRow(DatabaseTypes.SqlServer, DisplayName = DatabaseTypes.SqlServer)]
    public async Task ExecuteUpsertGetCountTests_005_Async(string databaseType)
    {
        // ARRANGE
        using Context context = await DatabaseUtils.OpenDatabaseAsync(
            databaseType,
            customConfigurationActions: optionsBuilder => optionsBuilder.UseExecuteUpdateExtensions())
            .ConfigureAwait(false);

        int count = 1;

        string updatedValue = "Updated TestValue";
        string insertedValue = "Inserted TestValue";

        IQueryable<TestTable1> source = context.TestTable1.Where(e => e.Id >= long.MaxValue);

        // ACT
        int upsertRowCount = await source.ExecuteUpsertGetCountAsync(
            updateSetPropertyCalls: builder =>
            {
                builder.SetProperty(e => e.TestField, updatedValue);
            },
            insertSetPropertyCalls: builder =>
            {
                builder.SetProperty(e => e.TestField, insertedValue);
            },
            cancellationToken: CancellationToken.None)
            .ConfigureAwait(false);

        // ASSERT
        Assert.AreEqual(count, upsertRowCount, "Invalid count");

        List<TestTable1> rows = await context.TestTable1
            .AsNoTracking()
            .ToListAsync(CancellationToken.None)
            .ConfigureAwait(false);

        Assert.HasCount(count, rows, "Unexpected count");
        Assert.AreEqual(insertedValue, rows[0].TestField, "Unexpected field value");
    }

    #endregion public methods
}
