// Copyright ©2021-2026 Mike King.
// This file is licensed to you under the MIT license.
// See the License.txt file in the solution root for more information.

using DotDoc.EntityFrameworkCore.Extensions.DatabaseType;
using DotDoc.EntityFrameworkCore.Extensions.ExecuteUpdate;
using DotDoc.EntityFrameworkCore.Extensions.Tests.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Storage;
using System.Data.Common;

namespace DotDoc.EntityFrameworkCore.Extensions.Tests.ExecuteUpdate;

/// <summary>
/// Tests for ExecuteUpsertGetRows extensions.
/// </summary>
[TestClass]
public class ExecuteUpsertGetRowsTests
{
    #region private fields

    /// <summary>
    /// The MSTest <see cref="TestContext"/> for the current test.
    /// </summary>
    private readonly TestContext _testContext;

    #endregion private fields

    #region public constructors

    /// <summary>
    /// Initializes a new instance of the <see cref="ExecuteUpsertGetRowsTests"/> class.
    /// </summary>
    /// <param name="testContext">The MSTest test context.</param>
    public ExecuteUpsertGetRowsTests(TestContext testContext)
    {
        this._testContext = testContext;
    }

    #endregion public constructors

    #region ExecuteUpsertGetRowsAsync tests

    /// <summary>
    /// Test ExecuteUpsertGetRowsAsync with Null IQueryable source parameter.
    /// </summary>
    /// <returns>A <see cref="Task"/> that represents the asynchronous operation.</returns>
    [TestMethod(DisplayName = "ExecuteUpsertGetRowsAsync with Null IQueryable source parameter")]
    public async Task ExecuteUpsertGetRowsTests_001_Async()
    {
        // ARRANGE
        IQueryable<TestTable1> source = null!;
        Action<UpdateSettersBuilder<TestTable1>> setPropertyCalls = new(builder => { });

        // ACT / ASSERT
        await Assert.ThrowsExactlyAsync<ArgumentNullException>(() => source.ExecuteUpsertGetRowsAsync(setPropertyCalls, cancellationToken: this._testContext.CancellationToken), "Unexpected exception").ConfigureAwait(false);
    }

    /// <summary>
    /// Test ExecuteUpsertGetRowsAsync with Null UpsertSettersBuilder setPropertyCalls parameter.
    /// </summary>
    /// <returns>A <see cref="Task"/> that represents the asynchronous operation.</returns>
    [TestMethod(DisplayName = "ExecuteUpsertGetRowsAsync with Null UpsertSettersBuilder setPropertyCalls parameter")]
    public async Task ExecuteUpsertGetRowsTests_002_Async()
    {
        // ARRANGE
        IQueryable<TestTable1> source = Array.Empty<TestTable1>().AsQueryable();
        Action<UpdateSettersBuilder<TestTable1>> setPropertyCalls = null!;

        // ACT / ASSERT
        await Assert.ThrowsExactlyAsync<ArgumentNullException>(() => source.ExecuteUpsertGetRowsAsync(setPropertyCalls, cancellationToken: this._testContext.CancellationToken), "Unexpected exception").ConfigureAwait(false);
    }

    /// <summary>
    /// Test ExecuteUpsertGetRowsAsync update records with update parameters only.
    /// </summary>
    /// <param name="databaseType">Database type.</param>
    /// <param name="count">Number of records to upsert.</param>
    /// <returns>A <see cref="Task"/> that represents the asynchronous operation.</returns>
    [TestMethod(DisplayName = "ExecuteUpsertGetRowsAsync with update parameters only")]
    [DataRow(DatabaseTypes.Sqlite, 1, DisplayName = $"{DatabaseTypes.Sqlite} Update 1 record")]
    [DataRow(DatabaseTypes.Sqlite, 10, DisplayName = $"{DatabaseTypes.Sqlite} Update 10 records")]
    [DataRow(DatabaseTypes.SqlServer, 1, DisplayName = $"{DatabaseTypes.SqlServer} Update 1 record")]
    [DataRow(DatabaseTypes.SqlServer, 10, DisplayName = $"{DatabaseTypes.SqlServer} Update 10 records")]
    public async Task ExecuteUpsertGetRowsTests_003_Async(string databaseType, int count)
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
        IList<TestTable1> rows = await source.ExecuteUpsertGetRowsAsync(
            updateSetPropertyCalls: builder =>
            {
                builder.SetProperty(e => e.TestField, updatedValue);
            },
            cancellationToken: this._testContext.CancellationToken)
            .ConfigureAwait(false);

        // ASSERT
        long id = startId;
        foreach (TestTable1 row in rows.OrderBy(r => r.Id))
        {
            Assert.AreEqual(id, row.Id, "Unexpected Id value");
            Assert.AreEqual(updatedValue, row.TestField, "Unexpected field value");

            id++;
        }
    }

    /// <summary>
    /// Test ExecuteUpsertGetRowsAsync update records with update and insert parameters.
    /// </summary>
    /// <param name="databaseType">Database type.</param>
    /// <param name="count">Number of records to upsert.</param>
    /// <returns>A <see cref="Task"/> that represents the asynchronous operation.</returns>
    [TestMethod(DisplayName = "ExecuteUpsertGetRowsAsync update records with update and insert parameters")]
    [DataRow(DatabaseTypes.Sqlite, 1, DisplayName = $"{DatabaseTypes.Sqlite} Update 1 record")]
    [DataRow(DatabaseTypes.Sqlite, 10, DisplayName = $"{DatabaseTypes.Sqlite} Update 10 records")]
    [DataRow(DatabaseTypes.SqlServer, 1, DisplayName = $"{DatabaseTypes.SqlServer} Update 1 record")]
    [DataRow(DatabaseTypes.SqlServer, 10, DisplayName = $"{DatabaseTypes.SqlServer} Update 10 records")]
    public async Task ExecuteUpsertGetRowsTests_004_Async(string databaseType, int count)
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
        IList<TestTable1> rows = await source.ExecuteUpsertGetRowsAsync(
            updateSetPropertyCalls: builder =>
            {
                builder.SetProperty(e => e.TestField, updatedValue);
            },
            insertSetPropertyCalls: builder =>
            {
                builder.SetProperty(e => e.TestField, insertedValue);
            },
            cancellationToken: this._testContext.CancellationToken)
            .ConfigureAwait(false);

        // ASSERT
        long id = startId;
        foreach (TestTable1 row in rows.OrderBy(r => r.Id))
        {
            Assert.AreEqual(id, row.Id, "Unexpected Id value");
            Assert.AreEqual(updatedValue, row.TestField, "Unexpected field value");

            id++;
        }
    }

    /// <summary>
    /// Test ExecuteUpsertGetRowsAsync insert record with update parameters only.
    /// </summary>
    /// <param name="databaseType">Database type.</param>
    /// <returns>A <see cref="Task"/> that represents the asynchronous operation.</returns>
    [TestMethod(DisplayName = "ExecuteUpsertGetRowsAsync insert record with update parameters only")]
    [DataRow(DatabaseTypes.Sqlite, DisplayName = DatabaseTypes.Sqlite)]
    [DataRow(DatabaseTypes.SqlServer, DisplayName = DatabaseTypes.SqlServer)]
    public async Task ExecuteUpsertGetRowsTests_005_Async(string databaseType)
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
        IList<TestTable1> rows = await source.ExecuteUpsertGetRowsAsync(
            updateSetPropertyCalls: builder =>
            {
                builder.SetProperty(e => e.TestField, insertedValue);
            },
            cancellationToken: this._testContext.CancellationToken)
            .ConfigureAwait(false);

        // ASSERT
        Assert.HasCount(count, rows, "Unexpected count");
        Assert.AreEqual(insertedValue, rows[0].TestField, "Unexpected field value");
    }

    /// <summary>
    /// Test ExecuteUpsertGetRowsAsync insert record with update and insert parameters.
    /// </summary>
    /// <param name="databaseType">Database type.</param>
    /// <returns>A <see cref="Task"/> that represents the asynchronous operation.</returns>
    [TestMethod(DisplayName = "ExecuteUpsertGetRowsAsync insert record with update and insert parameters")]
    [DataRow(DatabaseTypes.Sqlite, DisplayName = DatabaseTypes.Sqlite)]
    [DataRow(DatabaseTypes.SqlServer, DisplayName = DatabaseTypes.SqlServer)]
    public async Task ExecuteUpsertGetRowsTests_006_Async(string databaseType)
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
        IList<TestTable1> rows = await source.ExecuteUpsertGetRowsAsync(
            updateSetPropertyCalls: builder =>
            {
                builder.SetProperty(e => e.TestField, updatedValue);
            },
            insertSetPropertyCalls: builder =>
            {
                builder.SetProperty(e => e.TestField, insertedValue);
            },
            cancellationToken: this._testContext.CancellationToken)
            .ConfigureAwait(false);

        // ASSERT
        Assert.HasCount(count, rows, "Unexpected count");
        Assert.AreEqual(insertedValue, rows[0].TestField, "Unexpected field value");
    }

    /// <summary>
    /// Test ExecuteUpsertGetRowsAsync enlists in a transaction already started by the caller.
    /// </summary>
    /// <param name="databaseType">Database type.</param>
    /// <returns>A <see cref="Task"/> that represents the asynchronous operation.</returns>
    [TestMethod(DisplayName = "ExecuteUpsertGetRowsAsync enlists in an existing transaction")]
    [DataRow(DatabaseTypes.Sqlite, DisplayName = DatabaseTypes.Sqlite)]
    [DataRow(DatabaseTypes.SqlServer, DisplayName = DatabaseTypes.SqlServer)]
    public async Task ExecuteUpsertGetRowsTests_007_Async(string databaseType)
    {
        // ARRANGE
        using Context context = await DatabaseUtils.OpenDatabaseAsync(
            databaseType,
            customConfigurationActions: optionsBuilder => optionsBuilder.UseExecuteUpdateExtensions())
            .ConfigureAwait(false);

        string insertedValue = "Inserted TestValue";

        IQueryable<TestTable1> source = context.TestTable1.Where(e => e.Id >= long.MaxValue);

        // ACT
        IList<TestTable1> rows;
        await using (IDbContextTransaction transaction = await context.Database.BeginTransactionAsync(this._testContext.CancellationToken).ConfigureAwait(false))
        {
            rows = await source.ExecuteUpsertGetRowsAsync(
                updateSetPropertyCalls: builder =>
                {
                    builder.SetProperty(e => e.TestField, insertedValue);
                },
                cancellationToken: this._testContext.CancellationToken)
                .ConfigureAwait(false);

            Assert.AreSame(transaction, context.Database.CurrentTransaction, "Caller transaction should still be current");

            await transaction.RollbackAsync(this._testContext.CancellationToken).ConfigureAwait(false);
        }

        // ASSERT
        Assert.HasCount(1, rows, "Unexpected count");
        Assert.AreEqual(insertedValue, rows[0].TestField, "Unexpected field value");

        List<TestTable1> tableRows = await context.TestTable1
            .AsNoTracking()
            .ToListAsync(this._testContext.CancellationToken)
            .ConfigureAwait(false);

        Assert.IsEmpty(tableRows, "Insert should have been rolled back with the caller transaction");
    }

    /// <summary>
    /// Test ExecuteUpsertGetRowsAsync insert record with and without the RETURNING (SQLite) / OUTPUT (SQL Server) clause.
    /// </summary>
    /// <param name="databaseType">Database type.</param>
    /// <param name="useReturningClause">If true insert uses returning clause in sql else uses a select after insert is completed.</param>
    /// <returns>A <see cref="Task"/> that represents the asynchronous operation.</returns>
    [TestMethod(DisplayName = "ExecuteUpsertGetRowsAsync insert record with and without the returning clause")]
    [DataRow(DatabaseTypes.Sqlite, true, DisplayName = $"{DatabaseTypes.Sqlite} Use Returning Clause")]
    [DataRow(DatabaseTypes.Sqlite, false, DisplayName = $"{DatabaseTypes.Sqlite} Use Select Statement")]
    [DataRow(DatabaseTypes.SqlServer, true, DisplayName = $"{DatabaseTypes.SqlServer} Use Returning Clause")]
    [DataRow(DatabaseTypes.SqlServer, false, DisplayName = $"{DatabaseTypes.SqlServer} Use Select Statement")]
    public async Task ExecuteUpsertGetRowsTests_008_Async(string databaseType, bool useReturningClause)
    {
        // ARRANGE
        using Context context = await DatabaseUtils.OpenDatabaseAsync(
            databaseType,
            customConfigurationActions: optionsBuilder => optionsBuilder.UseExecuteUpdateExtensions(),
            customModelCreationActions: modelBuilder => UseReturningClause(modelBuilder, databaseType, useReturningClause))
            .ConfigureAwait(false);

        long id = 1;
        string insertedValue = "Inserted TestValue";

        IQueryable<TestTable1> source = context.TestTable1.Where(e => e.Id >= long.MaxValue);

        // ACT
        IList<TestTable1> rows = await source.ExecuteUpsertGetRowsAsync(
            updateSetPropertyCalls: builder =>
            {
                builder.SetProperty(e => e.TestField, insertedValue);
            },
            cancellationToken: this._testContext.CancellationToken)
            .ConfigureAwait(false);

        // ASSERT
        Assert.HasCount(1, rows, "Unexpected count");
        Assert.AreEqual(id, rows[0].Id, "Unexpected Id value");
        Assert.AreEqual(insertedValue, rows[0].TestField, "Unexpected field value");
    }

    /// <summary>
    /// Test ExecuteUpsertGetRowsAsync update records with and without the RETURNING (SQLite) / OUTPUT (SQL Server) clause.
    /// </summary>
    /// <param name="databaseType">Database type.</param>
    /// <param name="useReturningClause">If true update uses returning clause in sql else uses a select after update is completed.</param>
    /// <returns>A <see cref="Task"/> that represents the asynchronous operation.</returns>
    [TestMethod(DisplayName = "ExecuteUpsertGetRowsAsync update records with and without the returning clause")]
    [DataRow(DatabaseTypes.Sqlite, true, DisplayName = $"{DatabaseTypes.Sqlite} Use Returning Clause")]
    [DataRow(DatabaseTypes.Sqlite, false, DisplayName = $"{DatabaseTypes.Sqlite} Use Select Statement")]
    [DataRow(DatabaseTypes.SqlServer, true, DisplayName = $"{DatabaseTypes.SqlServer} Use Returning Clause")]
    [DataRow(DatabaseTypes.SqlServer, false, DisplayName = $"{DatabaseTypes.SqlServer} Use Select Statement")]
    public async Task ExecuteUpsertGetRowsTests_009_Async(string databaseType, bool useReturningClause)
    {
        // ARRANGE
        using Context context = await DatabaseUtils.OpenDatabaseAsync(
            databaseType,
            customConfigurationActions: optionsBuilder => optionsBuilder.UseExecuteUpdateExtensions(),
            customModelCreationActions: modelBuilder => UseReturningClause(modelBuilder, databaseType, useReturningClause))
            .ConfigureAwait(false);

        string originalValue = "Original TestValue";
        string updatedValue = "Updated TestValue";

        await DatabaseUtils.CreateTestTableEntriesAsync(context, originalValue, 10).ConfigureAwait(false);

        long startId = 2;
        long endId = 5;

        IQueryable<TestTable1> source = context.TestTable1.Where(e => e.Id >= startId && e.Id < endId);

        // ACT
        IList<TestTable1> rows = await source.ExecuteUpsertGetRowsAsync(
            updateSetPropertyCalls: builder =>
            {
                builder.SetProperty(e => e.TestField, updatedValue);
            },
            cancellationToken: this._testContext.CancellationToken)
            .ConfigureAwait(false);

        // ASSERT
        Assert.HasCount((int)(endId - startId), rows, "Unexpected count");

        long id = startId;
        foreach (TestTable1 row in rows.OrderBy(r => r.Id))
        {
            Assert.AreEqual(id, row.Id, "Unexpected Id value");
            Assert.AreEqual(updatedValue, row.TestField, "Unexpected field value");

            id++;
        }
    }

    /// <summary>
    /// Test ExecuteUpsertGetRowsAsync passes on the database exception and disposes its transaction when the insert fails.
    /// </summary>
    /// <param name="databaseType">Database type.</param>
    /// <returns>A <see cref="Task"/> that represents the asynchronous operation.</returns>
    /// <remarks>No rows match the update, so a row is inserted, which breaks the unique index on <see cref="TestTable2.TestField"/>.</remarks>
    [TestMethod(DisplayName = "ExecuteUpsertGetRowsAsync passes on the database exception when the insert fails")]
    [DataRow(DatabaseTypes.Sqlite, DisplayName = DatabaseTypes.Sqlite)]
    [DataRow(DatabaseTypes.SqlServer, DisplayName = DatabaseTypes.SqlServer)]
    public async Task ExecuteUpsertGetRowsTests_010_Async(string databaseType)
    {
        // ARRANGE
        using Context context = await DatabaseUtils.OpenDatabaseAsync(
            databaseType,
            customConfigurationActions: optionsBuilder => optionsBuilder.UseExecuteUpdateExtensions())
            .ConfigureAwait(false);

        string value = "TestValue";

        context.Add(new TestTable2 { TestField = value });
        await context.SaveChangesAsync(this._testContext.CancellationToken).ConfigureAwait(false);

        IQueryable<TestTable2> source = context.TestTable2.Where(e => e.Id >= long.MaxValue);

        // ACT / ASSERT
        await Assert.ThrowsAsync<DbException>(
            () => source.ExecuteUpsertGetRowsAsync(
                updateSetPropertyCalls: builder =>
                {
                    builder.SetProperty(e => e.TestField, value);
                },
                cancellationToken: this._testContext.CancellationToken),
            "Unexpected exception")
            .ConfigureAwait(false);

        Assert.IsNull(context.Database.CurrentTransaction, "The upsert transaction has not been disposed");

        List<TestTable2> rows = await context.TestTable2
            .AsNoTracking()
            .ToListAsync(this._testContext.CancellationToken)
            .ConfigureAwait(false);

        Assert.HasCount(1, rows, "Unexpected count");
    }

    /// <summary>
    /// Test ExecuteUpsertGetRowsAsync without UseExecuteUpdateExtensions.
    /// </summary>
    /// <param name="databaseType">Database type.</param>
    /// <returns>A <see cref="Task"/> that represents the asynchronous operation.</returns>
    /// <remarks>It must throw before a transaction is started or anything is updated.</remarks>
    [TestMethod(DisplayName = "ExecuteUpsertGetRowsAsync without UseExecuteUpdateExtensions")]
    [DataRow(DatabaseTypes.Sqlite, DisplayName = DatabaseTypes.Sqlite)]
    [DataRow(DatabaseTypes.SqlServer, DisplayName = DatabaseTypes.SqlServer)]
    public async Task ExecuteUpsertGetRowsTests_011_Async(string databaseType)
    {
        // ARRANGE
        using Context context = await DatabaseUtils.OpenDatabaseAsync(databaseType).ConfigureAwait(false);

        string originalValue = "Original TestValue";
        string updatedValue = "Updated TestValue";

        await DatabaseUtils.CreateTestTableEntriesAsync(context, originalValue, 5).ConfigureAwait(false);

        IQueryable<TestTable1> source = context.TestTable1.Where(e => e.Id <= 2);

        // ACT / ASSERT
        InvalidOperationException exception = await Assert.ThrowsExactlyAsync<InvalidOperationException>(
            () => source.ExecuteUpsertGetRowsAsync(
                updateSetPropertyCalls: builder =>
                {
                    builder.SetProperty(e => e.TestField, updatedValue);
                },
                cancellationToken: this._testContext.CancellationToken),
            "Unexpected exception")
            .ConfigureAwait(false);

        Assert.Contains(nameof(ExecuteUpdateExtensions.UseExecuteUpdateExtensions), exception.Message, "Unexpected exception message");
        Assert.IsNull(context.Database.CurrentTransaction, "A transaction has been left open");

        List<TestTable1> rows = await context.TestTable1
            .AsNoTracking()
            .ToListAsync(this._testContext.CancellationToken)
            .ConfigureAwait(false);

        Assert.HasCount(5, rows, "Unexpected count");
        Assert.IsTrue(rows.All(r => r.TestField == originalValue), "Rows have been updated");
    }

    #endregion ExecuteUpsertGetRowsAsync tests

    #region private methods

    /// <summary>
    /// Turn the RETURNING (SQLite) / OUTPUT (SQL Server) clause on or off for <see cref="TestTable1"/>.
    /// </summary>
    /// <param name="modelBuilder">The model builder.</param>
    /// <param name="databaseType">Database type.</param>
    /// <param name="useReturningClause">If true the returning clause is used, else the rows are read with a separate select.</param>
    private static void UseReturningClause(ModelBuilder modelBuilder, string databaseType, bool useReturningClause)
    {
        modelBuilder.Entity<TestTable1>()
            .ToTable(tableBuilder =>
            {
                switch (databaseType)
                {
                    case DatabaseTypes.Sqlite:
                        tableBuilder.UseSqlReturningClause(useReturningClause);
                        break;

                    case DatabaseTypes.SqlServer:
                        tableBuilder.UseSqlOutputClause(useReturningClause);
                        break;

                    default:
                        throw new UnsupportedDatabaseTypeException();
                }
            });
    }

    #endregion private methods
}
