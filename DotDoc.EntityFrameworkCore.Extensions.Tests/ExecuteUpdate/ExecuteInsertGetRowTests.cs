// Copyright ©2021-2026 Mike King.
// This file is licensed to you under the MIT license.
// See the License.txt file in the solution root for more information.

using DotDoc.EntityFrameworkCore.Extensions.DatabaseType;
using DotDoc.EntityFrameworkCore.Extensions.ExecuteUpdate;
using DotDoc.EntityFrameworkCore.Extensions.Tests.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using System.Data.Common;

namespace DotDoc.EntityFrameworkCore.Extensions.Tests.ExecuteUpdate;

/// <summary>
/// Tests for ExecuteInsertGetRow extensions.
/// </summary>
[TestClass]
public class ExecuteInsertGetRowTests
{
    #region private fields

    /// <summary>
    /// The MSTest <see cref="TestContext"/> for the current test.
    /// </summary>
    private readonly TestContext _testContext;

    #endregion private fields

    #region public constructors

    /// <summary>
    /// Initializes a new instance of the <see cref="ExecuteInsertGetRowTests"/> class.
    /// </summary>
    /// <param name="testContext">The MSTest test context.</param>
    public ExecuteInsertGetRowTests(TestContext testContext)
    {
        this._testContext = testContext;
    }

    #endregion public constructors

    #region public methods

    /// <summary>
    /// Test ExecuteInsertGetRowAsync with Null IQueryable source parameter.
    /// </summary>
    /// <returns>A <see cref="Task"/> that represents the asynchronous operation.</returns>
    [TestMethod(DisplayName = "ExecuteInsertGetRowAsync with Null IQueryable source parameter")]
    public async Task ExecuteInsertGetRowTests_001_Async()
    {
        // ARRANGE
        IQueryable<TestTable1> source = null!;
        Action<UpdateSettersBuilder<TestTable1>> setPropertyCalls = new(builder => { });

        // ACT / ASSERT
        await Assert.ThrowsExactlyAsync<ArgumentNullException>(() => source.ExecuteInsertGetRowAsync(setPropertyCalls, this._testContext.CancellationToken), "Unexpected exception").ConfigureAwait(false);
    }

    /// <summary>
    /// Test ExecuteInsertGetRowAsync with Null UpdateSettersBuilder setPropertyCalls parameter.
    /// </summary>
    /// <returns>A <see cref="Task"/> that represents the asynchronous operation.</returns>
    [TestMethod(DisplayName = "ExecuteInsertGetRowAsync with Null UpdateSettersBuilder setPropertyCalls parameter")]
    public async Task ExecuteInsertGetRowTests_002_Async()
    {
        // ARRANGE
        IQueryable<TestTable1> source = Array.Empty<TestTable1>().AsQueryable();
        Action<UpdateSettersBuilder<TestTable1>> setPropertyCalls = null!;

        // ACT / ASSERT
        await Assert.ThrowsExactlyAsync<ArgumentNullException>(() => source.ExecuteInsertGetRowAsync(setPropertyCalls, this._testContext.CancellationToken), "Unexpected exception").ConfigureAwait(false);
    }

    /// <summary>
    /// Test ExecuteInsertGetRowAsync.
    /// </summary>
    /// <param name="databaseType">Database type.</param>
    /// <param name="useReturningClause">If true update uses returning clause in sql else uses a select after update is completed.</param>
    /// <returns>A <see cref="Task"/> that represents the asynchronous operation.</returns>
    [TestMethod(DisplayName = "ExecuteInsertGetRowAsync")]
    [DataRow(DatabaseTypes.Sqlite, true, DisplayName = $"{DatabaseTypes.Sqlite} Use Returning Clause")]
    [DataRow(DatabaseTypes.Sqlite, false, DisplayName = $"{DatabaseTypes.Sqlite} Use Select Statement")]
    [DataRow(DatabaseTypes.SqlServer, true, DisplayName = $"{DatabaseTypes.SqlServer} Use Returning Clause")]
    [DataRow(DatabaseTypes.SqlServer, false, DisplayName = $"{DatabaseTypes.SqlServer} Use Select Statement")]
    public async Task ExecuteInsertGetRowTests_003_Async(string databaseType, bool useReturningClause)
    {
        // ARRANGE
        using Context context = await DatabaseUtils.OpenDatabaseAsync(
            databaseType: databaseType,
            customConfigurationActions: (optionsBuilder) =>
            {
                // Stop EF core caching the service provider so a new model gets
                // created every time and uses the desired useReturningClause value.
                optionsBuilder.EnableServiceProviderCaching(false);

                optionsBuilder.UseExecuteUpdateExtensions();
            },
            customModelCreationActions: modelBuilder =>
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
            })
            .ConfigureAwait(false);

        long id = 1;
        string value = "TestValue";

        // ACT
        TestTable1 row = await context.TestTable1.ExecuteInsertGetRowAsync(
            builder =>
            {
                builder.SetProperty(e => e.TestField, value);
            },
            this._testContext.CancellationToken)
            .ConfigureAwait(false);

        // ASSERT
        Assert.IsNull(ExecuteUpdateExtensions.ExecuteUpdateParameters.Value, "Parameters have not been cleared.");

        Assert.IsNotNull(row, "No row returned");
        Assert.AreEqual(id, row.Id, "Unexpected Id value");
        Assert.AreEqual(value, row.TestField, "Unexpected field value");
    }

    /// <summary>
    /// Test ExecuteInsertGetRowAsync when the Execute Update Parameters are already set.
    /// </summary>
    /// <returns>A <see cref="Task"/> that represents the asynchronous operation.</returns>
    /// <remarks>The parameters are only set while a call is running, so a value already being set means another call is in progress in this async flow.</remarks>
    [TestMethod(DisplayName = "ExecuteInsertGetRowAsync when the Execute Update Parameters are already set")]
    public async Task ExecuteInsertGetRowTests_004_Async()
    {
        // ARRANGE
        IQueryable<TestTable1> source = Array.Empty<TestTable1>().AsQueryable();
        Action<UpdateSettersBuilder<TestTable1>> setPropertyCalls = new(builder => builder.SetProperty(e => e.TestField, "TestValue"));

        ExecuteUpdateExtensions.ExecuteUpdateParameters.Value = new(QueryType.InsertGetRow);

        try
        {
            // ACT / ASSERT
            await Assert.ThrowsExactlyAsync<InvalidOperationException>(() => source.ExecuteInsertGetRowAsync(setPropertyCalls, this._testContext.CancellationToken), "Unexpected exception").ConfigureAwait(false);
        }
        finally
        {
            ExecuteUpdateExtensions.ExecuteUpdateParameters.Value = null!;
        }
    }

    /// <summary>
    /// Test ExecuteInsertGetRowAsync with a source that has a Where clause.
    /// </summary>
    /// <param name="databaseType">Database type.</param>
    /// <returns>A <see cref="Task"/> that represents the asynchronous operation.</returns>
    /// <remarks>An insert can't have a Where clause, so it must be rejected rather than run as an update.</remarks>
    [TestMethod(DisplayName = "ExecuteInsertGetRowAsync with a source that has a Where clause")]
    [DataRow(DatabaseTypes.Sqlite, DisplayName = DatabaseTypes.Sqlite)]
    [DataRow(DatabaseTypes.SqlServer, DisplayName = DatabaseTypes.SqlServer)]
    public async Task ExecuteInsertGetRowTests_005_Async(string databaseType)
    {
        // ARRANGE
        using Context context = await DatabaseUtils.OpenDatabaseAsync(
            databaseType: databaseType,
            customConfigurationActions: (optionsBuilder) =>
            {
                optionsBuilder.UseExecuteUpdateExtensions();
            })
            .ConfigureAwait(false);

        long id = 1;
        string originalValue = "Original TestValue";
        string insertedValue = "Inserted TestValue";

        await DatabaseUtils.CreateTestTableEntriesAsync(context, originalValue, 1).ConfigureAwait(false);

        IQueryable<TestTable1> source = context.TestTable1.Where(e => e.Id == id);

        // ACT / ASSERT
        await Assert.ThrowsExactlyAsync<InvalidOperationException>(
            () => source.ExecuteInsertGetRowAsync(
                builder =>
                {
                    builder.SetProperty(e => e.TestField, insertedValue);
                },
                this._testContext.CancellationToken),
            "Unexpected exception")
            .ConfigureAwait(false);

        List<TestTable1> rows = await context.TestTable1
            .AsNoTracking()
            .ToListAsync(this._testContext.CancellationToken)
            .ConfigureAwait(false);

        Assert.HasCount(1, rows, "Unexpected count");
        Assert.AreEqual(originalValue, rows[0].TestField, "Unexpected field value");
    }

    /// <summary>
    /// Test ExecuteInsertGetRowAsync passes on the database exception when the insert fails.
    /// </summary>
    /// <param name="databaseType">Database type.</param>
    /// <returns>A <see cref="Task"/> that represents the asynchronous operation.</returns>
    /// <remarks>The second insert breaks the unique index on <see cref="TestTable2.TestField"/>.</remarks>
    [TestMethod(DisplayName = "ExecuteInsertGetRowAsync passes on the database exception when the insert fails")]
    [DataRow(DatabaseTypes.Sqlite, DisplayName = DatabaseTypes.Sqlite)]
    [DataRow(DatabaseTypes.SqlServer, DisplayName = DatabaseTypes.SqlServer)]
    public async Task ExecuteInsertGetRowTests_006_Async(string databaseType)
    {
        // ARRANGE
        using Context context = await DatabaseUtils.OpenDatabaseAsync(
            databaseType: databaseType,
            customConfigurationActions: (optionsBuilder) =>
            {
                optionsBuilder.UseExecuteUpdateExtensions();
            })
            .ConfigureAwait(false);

        string value = "TestValue";
        Action<UpdateSettersBuilder<TestTable2>> setPropertyCalls = new(builder => builder.SetProperty(e => e.TestField, value));

        await context.TestTable2.ExecuteInsertGetRowAsync(setPropertyCalls, this._testContext.CancellationToken).ConfigureAwait(false);

        // ACT / ASSERT
        await Assert.ThrowsAsync<DbException>(() => context.TestTable2.ExecuteInsertGetRowAsync(setPropertyCalls, this._testContext.CancellationToken), "Unexpected exception").ConfigureAwait(false);

        List<TestTable2> rows = await context.TestTable2
            .AsNoTracking()
            .ToListAsync(this._testContext.CancellationToken)
            .ConfigureAwait(false);

        Assert.HasCount(1, rows, "Unexpected count");
    }

    #endregion public methods
}
