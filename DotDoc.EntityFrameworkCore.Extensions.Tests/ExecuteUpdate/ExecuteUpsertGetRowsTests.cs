// Copyright ©2021-2026 Mike King.
// This file is licensed to you under the MIT license.
// See the License.txt file in the solution root for more information.

using DotDoc.EntityFrameworkCore.Extensions.DatabaseType;
using DotDoc.EntityFrameworkCore.Extensions.ExecuteUpdate;
using DotDoc.EntityFrameworkCore.Extensions.TableHints;
using DotDoc.EntityFrameworkCore.Extensions.Tests.Data;
using DotDoc.EntityFrameworkCore.Extensions.Tests.Execute;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Logging;
using System.Data.Common;
using System.Transactions;

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
            this._testContext,
            databaseType,
            customConfigurationActions: optionsBuilder => optionsBuilder.UseExecuteUpdateExtensions())
            .ConfigureAwait(false);

        string originalValue = "Original TestValue";
        string updatedValue = "Updated TestValue";

        await DatabaseUtils.CreateTestTableEntriesAsync(context, originalValue, (count + 1) * 10, this._testContext.CancellationToken).ConfigureAwait(false);

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
            this._testContext,
            databaseType,
            customConfigurationActions: optionsBuilder => optionsBuilder.UseExecuteUpdateExtensions())
            .ConfigureAwait(false);

        string originalValue = "Original TestValue";
        string updatedValue = "Updated TestValue";
        string insertedValue = "Inserted TestValue";

        await DatabaseUtils.CreateTestTableEntriesAsync(context, originalValue, (count + 1) * 10, this._testContext.CancellationToken).ConfigureAwait(false);

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
            this._testContext,
            databaseType,
            customConfigurationActions: optionsBuilder => optionsBuilder.UseExecuteUpdateExtensions())
            .ConfigureAwait(false);

        int count = 1;

        string insertedValue = "Inserted TestValue";

        IQueryable<TestTable1> source = context.TestTable1.Where(e => e.Id == -1);

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
            this._testContext,
            databaseType,
            customConfigurationActions: optionsBuilder => optionsBuilder.UseExecuteUpdateExtensions())
            .ConfigureAwait(false);

        int count = 1;

        string updatedValue = "Updated TestValue";
        string insertedValue = "Inserted TestValue";

        IQueryable<TestTable1> source = context.TestTable1.Where(e => e.Id == -1);

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
            this._testContext,
            databaseType,
            customConfigurationActions: optionsBuilder => optionsBuilder.UseExecuteUpdateExtensions())
            .ConfigureAwait(false);

        string insertedValue = "Inserted TestValue";

        IQueryable<TestTable1> source = context.TestTable1.Where(e => e.Id == -1);

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
    /// Test ExecuteUpsertGetRowsAsync insert record without the RETURNING (SQLite) / OUTPUT (SQL Server) clause.
    /// </summary>
    /// <param name="databaseType">Database type.</param>
    /// <returns>A <see cref="Task"/> that represents the asynchronous operation.</returns>
    /// <remarks>
    /// No rows match, but the update runs first and needs the clause to return its rows, so it must throw before anything is
    /// inserted, and must not leave its transaction open.
    /// </remarks>
    [TestMethod(DisplayName = "ExecuteUpsertGetRowsAsync insert record without the RETURNING / OUTPUT clause")]
    [DataRow(DatabaseTypes.Sqlite, DisplayName = DatabaseTypes.Sqlite)]
    [DataRow(DatabaseTypes.SqlServer, DisplayName = DatabaseTypes.SqlServer)]
    public async Task ExecuteUpsertGetRowsTests_008_Async(string databaseType)
    {
        // ARRANGE
        using Context context = await DatabaseUtils.OpenDatabaseAsync(
            this._testContext,
            databaseType,
            customConfigurationActions: optionsBuilder => optionsBuilder.UseExecuteUpdateExtensions(),
            customModelCreationActions: modelBuilder => ExecuteUpdateTestUtils.TurnOffReturningClause(modelBuilder, databaseType))
            .ConfigureAwait(false);

        IQueryable<TestTable1> source = context.TestTable1.Where(e => e.Id == -1);

        // ACT / ASSERT
        NotSupportedException exception = await Assert.ThrowsExactlyAsync<NotSupportedException>(
            () => source.ExecuteUpsertGetRowsAsync(
                updateSetPropertyCalls: builder =>
                {
                    builder.SetProperty(e => e.TestField, "Inserted TestValue");
                },
                cancellationToken: this._testContext.CancellationToken),
            "Unexpected exception")
            .ConfigureAwait(false);

        ExecuteUpdateTestUtils.AssertReturningClauseNotUsedException(exception, databaseType, nameof(ExecuteUpdateExtensions.ExecuteUpdateGetRowsAsync));
        ExecuteUpdateTestUtils.AssertStateCleared(context);
        Assert.IsNull(context.Database.CurrentTransaction, "The upsert transaction has not been disposed");

        int rowCount = await context.TestTable1.CountAsync(this._testContext.CancellationToken).ConfigureAwait(false);
        Assert.AreEqual(0, rowCount, "Unexpected count");
    }

    /// <summary>
    /// Test ExecuteUpsertGetRowsAsync update records without the RETURNING (SQLite) / OUTPUT (SQL Server) clause.
    /// </summary>
    /// <param name="databaseType">Database type.</param>
    /// <returns>A <see cref="Task"/> that represents the asynchronous operation.</returns>
    /// <remarks>The updated rows can only be returned with the clause, so it must throw before anything is updated.</remarks>
    [TestMethod(DisplayName = "ExecuteUpsertGetRowsAsync update records without the RETURNING / OUTPUT clause")]
    [DataRow(DatabaseTypes.Sqlite, DisplayName = DatabaseTypes.Sqlite)]
    [DataRow(DatabaseTypes.SqlServer, DisplayName = DatabaseTypes.SqlServer)]
    public async Task ExecuteUpsertGetRowsTests_009_Async(string databaseType)
    {
        // ARRANGE
        using Context context = await DatabaseUtils.OpenDatabaseAsync(
            this._testContext,
            databaseType,
            customConfigurationActions: optionsBuilder => optionsBuilder.UseExecuteUpdateExtensions(),
            customModelCreationActions: modelBuilder => ExecuteUpdateTestUtils.TurnOffReturningClause(modelBuilder, databaseType))
            .ConfigureAwait(false);

        string originalValue = "Original TestValue";

        await DatabaseUtils.CreateTestTableEntriesAsync(context, originalValue, 10, this._testContext.CancellationToken).ConfigureAwait(false);

        IQueryable<TestTable1> source = context.TestTable1.Where(e => e.Id >= 2 && e.Id < 5);

        // ACT / ASSERT
        NotSupportedException exception = await Assert.ThrowsExactlyAsync<NotSupportedException>(
            () => source.ExecuteUpsertGetRowsAsync(
                updateSetPropertyCalls: builder =>
                {
                    builder.SetProperty(e => e.TestField, "Updated TestValue");
                },
                cancellationToken: this._testContext.CancellationToken),
            "Unexpected exception")
            .ConfigureAwait(false);

        ExecuteUpdateTestUtils.AssertReturningClauseNotUsedException(exception, databaseType, nameof(ExecuteUpdateExtensions.ExecuteUpdateGetRowsAsync));
        ExecuteUpdateTestUtils.AssertStateCleared(context);
        Assert.IsNull(context.Database.CurrentTransaction, "The upsert transaction has not been disposed");

        List<TestTable1> rows = await context.TestTable1
            .AsNoTracking()
            .ToListAsync(this._testContext.CancellationToken)
            .ConfigureAwait(false);

        Assert.HasCount(10, rows, "Unexpected count");
        Assert.IsTrue(rows.All(r => r.TestField == originalValue), "Rows have been updated");
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
            this._testContext,
            databaseType,
            customConfigurationActions: optionsBuilder => optionsBuilder.UseExecuteUpdateExtensions())
            .ConfigureAwait(false);

        string value = "TestValue";

        context.Add(new TestTable2 { TestField = value });
        await context.SaveChangesAsync(this._testContext.CancellationToken).ConfigureAwait(false);

        IQueryable<TestTable2> source = context.TestTable2.Where(e => e.Id == -1);

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
        using Context context = await DatabaseUtils.OpenDatabaseAsync(this._testContext, databaseType).ConfigureAwait(false);

        string originalValue = "Original TestValue";
        string updatedValue = "Updated TestValue";

        await DatabaseUtils.CreateTestTableEntriesAsync(context, originalValue, 5, this._testContext.CancellationToken).ConfigureAwait(false);

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

    /// <summary>
    /// Test ExecuteUpsertGetRowsAsync with table hints on SQL Server.
    /// </summary>
    /// <returns>A <see cref="Task"/> that represents the asynchronous operation.</returns>
    /// <remarks>The upsert adds its own lock hints, so it must throw before a transaction is started or anything is updated.</remarks>
    [TestMethod(DisplayName = "ExecuteUpsertGetRowsAsync with table hints on SQL Server")]
    public async Task ExecuteUpsertGetRowsTests_012_Async()
    {
        // ARRANGE
        using Context context = await DatabaseUtils.OpenDatabaseAsync(
            this._testContext,
            DatabaseTypes.SqlServer,
            customConfigurationActions: optionsBuilder => optionsBuilder.UseExecuteUpdateExtensions())
            .ConfigureAwait(false);

        string originalValue = "Original TestValue";
        string updatedValue = "Updated TestValue";

        await DatabaseUtils.CreateTestTableEntriesAsync(context, originalValue, 5, this._testContext.CancellationToken).ConfigureAwait(false);

        IQueryable<TestTable1> source = context.TestTable1
            .WithTableHints(SqlServerTableHints.RowLock)
            .Where(e => e.Id <= 2);

        // ACT / ASSERT
        await Assert.ThrowsExactlyAsync<NotSupportedException>(
            () => source.ExecuteUpsertGetRowsAsync(
                updateSetPropertyCalls: builder =>
                {
                    builder.SetProperty(e => e.TestField, updatedValue);
                },
                cancellationToken: this._testContext.CancellationToken),
            "Unexpected exception")
            .ConfigureAwait(false);

        Assert.IsNull(context.Database.CurrentTransaction, "A transaction has been left open");

        List<TestTable1> rows = await context.TestTable1
            .AsNoTracking()
            .ToListAsync(this._testContext.CancellationToken)
            .ConfigureAwait(false);

        Assert.HasCount(5, rows, "Unexpected count");
        Assert.IsTrue(rows.All(r => r.TestField == originalValue), "Rows have been updated");
    }

    /// <summary>
    /// Test ExecuteUpsertGetRowsAsync adds the lock hints to the update on SQL Server.
    /// </summary>
    /// <param name="rowExists">If true the row exists so it is updated, else it is inserted.</param>
    /// <returns>A <see cref="Task"/> that represents the asynchronous operation.</returns>
    /// <remarks>
    /// UPDLOCK and HOLDLOCK stop two upserts running at the same time from both inserting. Only the update needs them.
    /// The SQL is checked by logging the commands that are run.
    /// </remarks>
    [TestMethod(DisplayName = "ExecuteUpsertGetRowsAsync adds the lock hints to the update on SQL Server")]
    [DataRow(true, DisplayName = "Update")]
    [DataRow(false, DisplayName = "Insert")]
    public async Task ExecuteUpsertGetRowsTests_013_Async(bool rowExists)
    {
        // ARRANGE
        List<string> log = [];

        using Context context = await DatabaseUtils.OpenDatabaseAsync(
            this._testContext,
            DatabaseTypes.SqlServer,
            customConfigurationActions: optionsBuilder =>
            {
                optionsBuilder.UseExecuteUpdateExtensions();
                optionsBuilder.LogTo(log.Add, [RelationalEventId.CommandExecuted], LogLevel.Information);
            })
            .ConfigureAwait(false);

        await DatabaseUtils.CreateTestTableEntriesAsync(context, "Original TestValue", 5, this._testContext.CancellationToken).ConfigureAwait(false);

        string lockHints = $"WITH ({SqlServerTableHints.HoldLock}, {SqlServerTableHints.UpdLock})";
        string upsertedValue = "Upserted TestValue";
        long id = rowExists ? 1 : -1;

        IQueryable<TestTable1> source = context.TestTable1.Where(e => e.Id == id);

        log.Clear();

        // ACT
        IList<TestTable1> results = await source.ExecuteUpsertGetRowsAsync(
            updateSetPropertyCalls: builder =>
            {
                builder.SetProperty(e => e.TestField, upsertedValue);
            },
            cancellationToken: this._testContext.CancellationToken)
            .ConfigureAwait(false);

        // ASSERT
        Assert.HasCount(1, results, "Invalid count");
        Assert.AreEqual(upsertedValue, results[0].TestField, "Unexpected field value");

        // We are looking for the UPDATE / INSERT SQL STATEMENT so note the space on the end to stop matches like INSERTED.
        string? updateSql = log.FirstOrDefault(l => l.Contains("UPDATE ", StringComparison.Ordinal));
        string? insertSql = log.FirstOrDefault(l => l.Contains("INSERT ", StringComparison.Ordinal));

        Assert.IsNotNull(updateSql, "The update was not run");
        Assert.Contains(lockHints, updateSql, "Lock hints missing from the update");

        if (rowExists)
        {
            Assert.IsNull(insertSql, "The insert should not have been run");
        }
        else
        {
            Assert.IsNotNull(insertSql, "The insert was not run");
            Assert.DoesNotContain(lockHints, insertSql, "Lock hints should not be on the insert");
        }
    }

    /// <summary>
    /// Test ExecuteUpsertGetRowsAsync with table hints on SQLite.
    /// </summary>
    /// <returns>A <see cref="Task"/> that represents the asynchronous operation.</returns>
    /// <remarks>Table hints are SQL Server only and <c>WithTableHints</c> does nothing on SQLite, so the upsert runs as normal.</remarks>
    [TestMethod(DisplayName = "ExecuteUpsertGetRowsAsync with table hints on SQLite")]
    public async Task ExecuteUpsertGetRowsTests_014_Async()
    {
        // ARRANGE
        using Context context = await DatabaseUtils.OpenDatabaseAsync(
            this._testContext,
            DatabaseTypes.Sqlite,
            customConfigurationActions: optionsBuilder => optionsBuilder.UseExecuteUpdateExtensions())
            .ConfigureAwait(false);

        string updatedValue = "Updated TestValue";

        await DatabaseUtils.CreateTestTableEntriesAsync(context, "Original TestValue", 5, this._testContext.CancellationToken).ConfigureAwait(false);

        IQueryable<TestTable1> source = context.TestTable1
            .WithTableHints(SqlServerTableHints.RowLock)
            .Where(e => e.Id <= 2);

        // ACT
        IList<TestTable1> results = await source.ExecuteUpsertGetRowsAsync(
            updateSetPropertyCalls: builder =>
            {
                builder.SetProperty(e => e.TestField, updatedValue);
            },
            cancellationToken: this._testContext.CancellationToken)
            .ConfigureAwait(false);

        // ASSERT
        Assert.HasCount(2, results, "Invalid count");
        Assert.IsTrue(results.All(r => r.TestField == updatedValue), "Rows have not been updated");
    }

    /// <summary>
    /// Test ExecuteUpsertGetRowsAsync with table hints and a method call in the Where clause on SQL Server.
    /// </summary>
    /// <returns>A <see cref="Task"/> that represents the asynchronous operation.</returns>
    /// <remarks>
    /// The table hints must still be found when the Where lambda contains a method call (<c>Contains</c>), which is visited after the
    /// table hints tag. If they were missed, the upsert would add its own hints and the query would fail with a different exception.
    /// </remarks>
    [TestMethod(DisplayName = "ExecuteUpsertGetRowsAsync with table hints and a method call in the Where clause on SQL Server")]
    public async Task ExecuteUpsertGetRowsTests_015_Async()
    {
        // ARRANGE
        using Context context = await DatabaseUtils.OpenDatabaseAsync(
            this._testContext,
            DatabaseTypes.SqlServer,
            customConfigurationActions: optionsBuilder => optionsBuilder.UseExecuteUpdateExtensions())
            .ConfigureAwait(false);

        string originalValue = "Original TestValue";
        string updatedValue = "Updated TestValue";

        await DatabaseUtils.CreateTestTableEntriesAsync(context, originalValue, 5, this._testContext.CancellationToken).ConfigureAwait(false);

        IQueryable<TestTable1> source = context.TestTable1
            .WithTableHints(SqlServerTableHints.RowLock)
            .Where(e => e.TestField.Contains("Original"));

        // ACT / ASSERT
        NotSupportedException exception = await Assert.ThrowsExactlyAsync<NotSupportedException>(
            () => source.ExecuteUpsertGetRowsAsync(
                updateSetPropertyCalls: builder =>
                {
                    builder.SetProperty(e => e.TestField, updatedValue);
                },
                cancellationToken: this._testContext.CancellationToken),
            "Unexpected exception")
            .ConfigureAwait(false);

        Assert.Contains(nameof(ExecuteUpdateExtensions.ExecuteUpsertGetRowsAsync), exception.Message, "Unexpected exception message");
        Assert.IsNull(context.Database.CurrentTransaction, "A transaction has been left open");

        List<TestTable1> rows = await context.TestTable1
            .AsNoTracking()
            .ToListAsync(this._testContext.CancellationToken)
            .ConfigureAwait(false);

        Assert.HasCount(5, rows, "Unexpected count");
        Assert.IsTrue(rows.All(r => r.TestField == originalValue), "Rows have been updated");
    }

    /// <summary>
    /// Test ExecuteUpsertGetRowsAsync on an entity with an owned type.
    /// </summary>
    /// <param name="databaseType">Database type.</param>
    /// <returns>A <see cref="Task"/> that represents the asynchronous operation.</returns>
    /// <remarks>EF Core can't read an owned type from the upserted rows, so it must throw before anything is run, including the transaction.</remarks>
    [TestMethod(DisplayName = "ExecuteUpsertGetRowsAsync on an entity with an owned type")]
    [DataRow(DatabaseTypes.Sqlite, DisplayName = DatabaseTypes.Sqlite)]
    [DataRow(DatabaseTypes.SqlServer, DisplayName = DatabaseTypes.SqlServer)]
    public async Task ExecuteUpsertGetRowsTests_016_Async(string databaseType)
    {
        // ARRANGE
        CancellationToken cancellationToken = this._testContext.CancellationToken;

        using Context context = await DatabaseUtils.OpenDatabaseAsync(
            this._testContext,
            databaseType,
            customConfigurationActions: optionsBuilder => optionsBuilder.UseExecuteUpdateExtensions(),
            customModelCreationActions: modelBuilder => ExecuteUpdateTestUtils.AddCustomer(modelBuilder, owned: true))
            .ConfigureAwait(false);

        // ACT / ASSERT
        NotSupportedException exception = await Assert.ThrowsExactlyAsync<NotSupportedException>(
            () => context.Set<Customer>()
                .Where(e => e.Name == "Fred")
                .ExecuteUpsertGetRowsAsync(updateSetPropertyCalls: builder => builder.SetProperty(e => e.Name, "Fred"), cancellationToken: cancellationToken),
            "Unexpected exception")
            .ConfigureAwait(false);

        Assert.AreEqual("ExecuteUpsertGetRowsAsync does not support entity types with owned types (Customer.Address).", exception.Message, "Unexpected exception message");
        Assert.IsNull(context.Database.CurrentTransaction, "A transaction has been left open");

        int rowCount = await context.Set<Customer>().CountAsync(cancellationToken).ConfigureAwait(false);
        Assert.AreEqual(0, rowCount, "Unexpected count");
    }

    /// <summary>
    /// Test ExecuteUpsertGetRowsAsync with a retrying execution strategy.
    /// </summary>
    /// <param name="databaseType">Database type.</param>
    /// <param name="rowExists">If true the row exists so it is updated, else it is inserted.</param>
    /// <returns>A <see cref="Task"/> that represents the asynchronous operation.</returns>
    /// <remarks>
    /// A retrying execution strategy (e.g. <c>EnableRetryOnFailure</c>) throws if a transaction is begun outside it, so the upsert must begin
    /// its transaction inside the context's execution strategy.
    /// </remarks>
    [TestMethod(DisplayName = "ExecuteUpsertGetRowsAsync with a retrying execution strategy")]
    [DataRow(DatabaseTypes.Sqlite, true, DisplayName = $"{DatabaseTypes.Sqlite} Update")]
    [DataRow(DatabaseTypes.Sqlite, false, DisplayName = $"{DatabaseTypes.Sqlite} Insert")]
    [DataRow(DatabaseTypes.SqlServer, true, DisplayName = $"{DatabaseTypes.SqlServer} Update")]
    [DataRow(DatabaseTypes.SqlServer, false, DisplayName = $"{DatabaseTypes.SqlServer} Insert")]
    public async Task ExecuteUpsertGetRowsTests_017_Async(string databaseType, bool rowExists)
    {
        // ARRANGE
        using Context context = await DatabaseUtils.OpenDatabaseAsync(
            this._testContext,
            databaseType,
            customConfigurationActions: optionsBuilder => optionsBuilder
                .UseExecuteUpdateExtensions()
                .ReplaceService<IExecutionStrategyFactory, RetryOnceExecutionStrategyFactory>())
            .ConfigureAwait(false);

        string originalValue = "Original TestValue";
        string upsertedValue = "Upserted TestValue";

        await DatabaseUtils.CreateTestTableEntriesAsync(context, originalValue, 5, this._testContext.CancellationToken).ConfigureAwait(false);

        long id = rowExists ? 1 : -1;

        IQueryable<TestTable1> source = context.TestTable1.Where(e => e.Id == id);

        // ACT
        IList<TestTable1> upsertedRows = await source.ExecuteUpsertGetRowsAsync(
            updateSetPropertyCalls: builder =>
            {
                builder.SetProperty(e => e.TestField, upsertedValue);
            },
            cancellationToken: this._testContext.CancellationToken)
            .ConfigureAwait(false);

        // ASSERT
        Assert.HasCount(1, upsertedRows, "Invalid count");
        Assert.AreEqual(upsertedValue, upsertedRows[0].TestField, "Unexpected field value");
        Assert.IsNull(context.Database.CurrentTransaction, "A transaction has been left open");

        if (rowExists)
        {
            Assert.AreEqual(1, upsertedRows[0].Id, "The wrong row has been updated");
        }

        List<TestTable1> rows = await context.TestTable1
            .AsNoTracking()
            .ToListAsync(this._testContext.CancellationToken)
            .ConfigureAwait(false);

        Assert.HasCount(rowExists ? 5 : 6, rows, "Unexpected count");
        Assert.HasCount(1, rows.Where(r => r.TestField == upsertedValue), "Unexpected number of upserted rows");
    }

    /// <summary>
    /// Test ExecuteUpsertGetRowsAsync is run again in a new transaction when its commit fails with a transient error.
    /// </summary>
    /// <param name="databaseType">Database type.</param>
    /// <param name="rowExists">If true the row exists so it is updated, else it is inserted.</param>
    /// <returns>A <see cref="Task"/> that represents the asynchronous operation.</returns>
    /// <remarks>
    /// The first attempt's transaction must be rolled back before the upsert is run again, so the update (which appends to the current value)
    /// is applied once and the insert gives one row. The rows returned must be the second attempt's.
    /// </remarks>
    [TestMethod(DisplayName = "ExecuteUpsertGetRowsAsync is retried in a new transaction when the commit fails")]
    [DataRow(DatabaseTypes.Sqlite, true, DisplayName = $"{DatabaseTypes.Sqlite} Update")]
    [DataRow(DatabaseTypes.Sqlite, false, DisplayName = $"{DatabaseTypes.Sqlite} Insert")]
    [DataRow(DatabaseTypes.SqlServer, true, DisplayName = $"{DatabaseTypes.SqlServer} Update")]
    [DataRow(DatabaseTypes.SqlServer, false, DisplayName = $"{DatabaseTypes.SqlServer} Insert")]
    public async Task ExecuteUpsertGetRowsTests_018_Async(string databaseType, bool rowExists)
    {
        // ARRANGE
        CommitFailingTransactionInterceptor interceptor = new();

        using Context context = await DatabaseUtils.OpenDatabaseAsync(
            this._testContext,
            databaseType,
            customConfigurationActions: optionsBuilder => optionsBuilder
                .UseExecuteUpdateExtensions()
                .AddInterceptors(interceptor)
                .ReplaceService<IExecutionStrategyFactory, RetryOnceExecutionStrategyFactory>())
            .ConfigureAwait(false);

        string originalValue = "Original TestValue";
        string updatedValue = $"{originalValue} Updated";
        string insertedValue = "Inserted TestValue";

        await DatabaseUtils.CreateTestTableEntriesAsync(context, originalValue, 5, this._testContext.CancellationToken).ConfigureAwait(false);

        long id = rowExists ? 1 : -1;

        IQueryable<TestTable1> source = context.TestTable1.Where(e => e.Id == id);

        interceptor.CommitFailuresToThrow = 1;

        // ACT
        IList<TestTable1> upsertedRows = await source.ExecuteUpsertGetRowsAsync(
            updateSetPropertyCalls: builder =>
            {
                builder.SetProperty(e => e.TestField, e => e.TestField + " Updated");
            },
            insertSetPropertyCalls: builder =>
            {
                builder.SetProperty(e => e.TestField, insertedValue);
            },
            cancellationToken: this._testContext.CancellationToken)
            .ConfigureAwait(false);

        // ASSERT
        Assert.AreEqual(0, interceptor.CommitFailuresToThrow, "The simulated transient error wasn't thrown");
        Assert.HasCount(1, upsertedRows, "Invalid count");
        Assert.AreEqual(rowExists ? updatedValue : insertedValue, upsertedRows[0].TestField, "Unexpected field value");
        Assert.IsNull(context.Database.CurrentTransaction, "A transaction has been left open");

        List<TestTable1> rows = await context.TestTable1
            .AsNoTracking()
            .ToListAsync(this._testContext.CancellationToken)
            .ConfigureAwait(false);

        if (rowExists)
        {
            Assert.HasCount(5, rows, "Unexpected count");
            Assert.AreEqual(updatedValue, rows.Single(r => r.Id == 1).TestField, "The row should have been updated once");
        }
        else
        {
            Assert.HasCount(6, rows, "Unexpected count");
            Assert.HasCount(1, rows.Where(r => r.TestField == insertedValue), "One row should have been inserted");
            Assert.AreEqual(rows.Single(r => r.TestField == insertedValue).Id, upsertedRows[0].Id, "The row returned isn't the row inserted");
        }
    }

    /// <summary>
    /// Test ExecuteUpsertGetRowsAsync runs in a System.Transactions transaction on SQL Server.
    /// </summary>
    /// <param name="enlist">If true the connection is enlisted in the transaction with EnlistTransaction, else the transaction is an ambient one (TransactionScope).</param>
    /// <param name="commit">If true the transaction is committed, else it is rolled back.</param>
    /// <returns>A <see cref="Task"/> that represents the asynchronous operation.</returns>
    /// <remarks>
    /// Neither kind of transaction sets <c>CurrentTransaction</c>, but EF Core won't begin a new transaction while the connection is in one,
    /// so the upsert must run in it. EF Core's SQLite provider doesn't support System.Transactions, so this is SQL Server only.
    /// </remarks>
    [TestMethod(DisplayName = "ExecuteUpsertGetRowsAsync runs in a System.Transactions transaction on SQL Server")]
    [DataRow(false, true, DisplayName = "TransactionScope Commit")]
    [DataRow(false, false, DisplayName = "TransactionScope Rollback")]
    [DataRow(true, true, DisplayName = "EnlistTransaction Commit")]
    [DataRow(true, false, DisplayName = "EnlistTransaction Rollback")]
    public async Task ExecuteUpsertGetRowsTests_019_Async(bool enlist, bool commit)
    {
        // ARRANGE
        using Context context = await DatabaseUtils.OpenDatabaseAsync(
            this._testContext,
            DatabaseTypes.SqlServer,
            customConfigurationActions: optionsBuilder => optionsBuilder.UseExecuteUpdateExtensions())
            .ConfigureAwait(false);

        string insertedValue = "Inserted TestValue";

        IQueryable<TestTable1> source = context.TestTable1.Where(e => e.Id == -1);

        // ACT
        IList<TestTable1> upsertedRows;

        if (enlist)
        {
            using CommittableTransaction transaction = new();

            await context.Database.OpenConnectionAsync(this._testContext.CancellationToken).ConfigureAwait(false);
            context.Database.EnlistTransaction(transaction);

            upsertedRows = await source.ExecuteUpsertGetRowsAsync(
                updateSetPropertyCalls: builder =>
                {
                    builder.SetProperty(e => e.TestField, insertedValue);
                },
                cancellationToken: this._testContext.CancellationToken)
                .ConfigureAwait(false);

            if (commit)
            {
                transaction.Commit();
            }
            else
            {
                transaction.Rollback();
            }

            await context.Database.CloseConnectionAsync().ConfigureAwait(false);
        }
        else
        {
            using TransactionScope scope = new(TransactionScopeAsyncFlowOption.Enabled);

            upsertedRows = await source.ExecuteUpsertGetRowsAsync(
                updateSetPropertyCalls: builder =>
                {
                    builder.SetProperty(e => e.TestField, insertedValue);
                },
                cancellationToken: this._testContext.CancellationToken)
                .ConfigureAwait(false);

            if (commit)
            {
                scope.Complete();
            }
        }

        // ASSERT
        Assert.HasCount(1, upsertedRows, "Invalid count");
        Assert.AreEqual(insertedValue, upsertedRows[0].TestField, "Unexpected field value");
        Assert.IsNull(context.Database.CurrentTransaction, "The upsert has begun its own transaction");

        List<TestTable1> rows = await context.TestTable1
            .AsNoTracking()
            .ToListAsync(this._testContext.CancellationToken)
            .ConfigureAwait(false);

        Assert.HasCount(commit ? 1 : 0, rows, commit ? "The insert should have been committed with the transaction" : "The insert should have been rolled back with the transaction");
    }

    /// <summary>
    /// Test ExecuteUpsertGetRowsAsync insert record when the query has chained Where calls.
    /// </summary>
    /// <param name="databaseType">Database type.</param>
    /// <param name="whereCount">The number of chained Where calls.</param>
    /// <returns>A <see cref="Task"/> that represents the asynchronous operation.</returns>
    /// <remarks>Every Where must be removed from the query for the insert, not just the last one, otherwise the insert throws.</remarks>
    [TestMethod(DisplayName = "ExecuteUpsertGetRowsAsync insert record with chained Where calls")]
    [DataRow(DatabaseTypes.Sqlite, 2, DisplayName = $"{DatabaseTypes.Sqlite} 2 Where calls")]
    [DataRow(DatabaseTypes.Sqlite, 3, DisplayName = $"{DatabaseTypes.Sqlite} 3 Where calls")]
    [DataRow(DatabaseTypes.SqlServer, 2, DisplayName = $"{DatabaseTypes.SqlServer} 2 Where calls")]
    [DataRow(DatabaseTypes.SqlServer, 3, DisplayName = $"{DatabaseTypes.SqlServer} 3 Where calls")]
    public async Task ExecuteUpsertGetRowsTests_020_Async(string databaseType, int whereCount)
    {
        // ARRANGE
        using Context context = await DatabaseUtils.OpenDatabaseAsync(
            this._testContext,
            databaseType,
            customConfigurationActions: optionsBuilder => optionsBuilder.UseExecuteUpdateExtensions())
            .ConfigureAwait(false);

        string originalValue = "Original TestValue";
        string insertedValue = "Inserted TestValue";

        await DatabaseUtils.CreateTestTableEntriesAsync(context, originalValue, 5, this._testContext.CancellationToken).ConfigureAwait(false);

        // No row matches the first Where, so the upsert inserts a row.
        IQueryable<TestTable1> source = context.TestTable1.Where(e => e.Id == -1);

        for (int i = 1; i < whereCount; i++)
        {
            source = source.Where(e => e.TestField != insertedValue);
        }

        // ACT
        IList<TestTable1> upsertedRows = await source.ExecuteUpsertGetRowsAsync(
            updateSetPropertyCalls: builder =>
            {
                builder.SetProperty(e => e.TestField, insertedValue);
            },
            cancellationToken: this._testContext.CancellationToken)
            .ConfigureAwait(false);

        // ASSERT
        Assert.HasCount(1, upsertedRows, "Invalid count");
        Assert.AreEqual(insertedValue, upsertedRows[0].TestField, "Unexpected field value");

        List<TestTable1> rows = await context.TestTable1
            .AsNoTracking()
            .ToListAsync(this._testContext.CancellationToken)
            .ConfigureAwait(false);

        Assert.HasCount(6, rows, "Unexpected count");
        Assert.HasCount(1, rows.Where(r => r.TestField == insertedValue), "One row should have been inserted");
    }

    #endregion ExecuteUpsertGetRowsAsync tests
}
