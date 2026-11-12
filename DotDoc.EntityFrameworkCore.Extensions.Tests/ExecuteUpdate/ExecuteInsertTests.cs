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
/// Tests for ExecuteInsert extensions.
/// </summary>
[TestClass]
public class ExecuteInsertTests
{
    #region private fields

    /// <summary>
    /// The MSTest <see cref="TestContext"/> for the current test.
    /// </summary>
    private readonly TestContext _testContext;

    #endregion private fields

    #region public constructors

    /// <summary>
    /// Initializes a new instance of the <see cref="ExecuteInsertTests"/> class.
    /// </summary>
    /// <param name="testContext">The MSTest test context.</param>
    public ExecuteInsertTests(TestContext testContext)
    {
        this._testContext = testContext;
    }

    #endregion public constructors

    #region public methods

    /// <summary>
    /// Test ExecuteInsertAsync with Null IQueryable source parameter.
    /// </summary>
    /// <returns>A <see cref="Task"/> that represents the asynchronous operation.</returns>
    [TestMethod(DisplayName = "ExecuteInsertAsync with Null IQueryable source parameter")]
    public async Task ExecuteInsertTests_001_Async()
    {
        // ARRANGE
        IQueryable<TestTable1> source = null!;
        Action<UpdateSettersBuilder<TestTable1>> setPropertyCalls = new(builder => { });

        // ACT / ASSERT
        await Assert.ThrowsExactlyAsync<ArgumentNullException>(() => source.ExecuteInsertAsync(setPropertyCalls, this._testContext.CancellationToken), "Unexpected exception").ConfigureAwait(false);
    }

    /// <summary>
    /// Test ExecuteInsertAsync with Null UpdateSettersBuilder setPropertyCalls parameter.
    /// </summary>
    /// <returns>A <see cref="Task"/> that represents the asynchronous operation.</returns>
    [TestMethod(DisplayName = "ExecuteInsertAsync with Null UpdateSettersBuilder setPropertyCalls parameter")]
    public async Task ExecuteInsertTests_002_Async()
    {
        // ARRANGE
        IQueryable<TestTable1> source = Array.Empty<TestTable1>().AsQueryable();
        Action<UpdateSettersBuilder<TestTable1>> setPropertyCalls = null!;

        // ACT / ASSERT
        await Assert.ThrowsExactlyAsync<ArgumentNullException>(() => source.ExecuteInsertAsync(setPropertyCalls, this._testContext.CancellationToken), "Unexpected exception").ConfigureAwait(false);
    }

    /// <summary>
    /// Test ExecuteInsertAsync.
    /// </summary>
    /// <param name="databaseType">Database type.</param>
    /// <returns>A <see cref="Task"/> that represents the asynchronous operation.</returns>
    [TestMethod(DisplayName = "ExecuteInsertAsync")]
    [DataRow(DatabaseTypes.Sqlite, DisplayName = DatabaseTypes.Sqlite)]
    [DataRow(DatabaseTypes.SqlServer, DisplayName = DatabaseTypes.SqlServer)]
    public async Task ExecuteInsertTests_003_Async(string databaseType)
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
        string value = "TestValue";

        // ACT
        await context.TestTable1.ExecuteInsertAsync(
            builder =>
            {
                builder.SetProperty(e => e.TestField, value);
            },
            this._testContext.CancellationToken)
            .ConfigureAwait(false);

        // ASSERT
        Assert.IsNull(ExecuteUpdateExtensions.ExecuteUpdateParameters.Value, "Parameters have not been cleared.");

        TestTable1? row = await context.TestTable1
            .Where(e => e.Id == id)
            .SingleOrDefaultAsync(this._testContext.CancellationToken)
            .ConfigureAwait(false);

        Assert.IsNotNull(row, "No row returned");
        Assert.AreEqual(id, row.Id, "Unexpected Id value");
        Assert.AreEqual(value, row.TestField, "Unexpected field value");
    }

    /// <summary>
    /// Test ExecuteInsertAsync when the Execute Update Parameters are already set.
    /// </summary>
    /// <returns>A <see cref="Task"/> that represents the asynchronous operation.</returns>
    /// <remarks>The parameters are only set while a call is running, so a value already being set means another call is in progress in this async flow.</remarks>
    [TestMethod(DisplayName = "ExecuteInsertAsync when the Execute Update Parameters are already set")]
    public async Task ExecuteInsertTests_004_Async()
    {
        // ARRANGE
        IQueryable<TestTable1> source = Array.Empty<TestTable1>().AsQueryable();
        Action<UpdateSettersBuilder<TestTable1>> setPropertyCalls = new(builder => builder.SetProperty(e => e.TestField, "TestValue"));

        ExecuteUpdateExtensions.ExecuteUpdateParameters.Value = new(QueryType.Insert);

        try
        {
            // ACT / ASSERT
            await Assert.ThrowsExactlyAsync<InvalidOperationException>(() => source.ExecuteInsertAsync(setPropertyCalls, this._testContext.CancellationToken), "Unexpected exception").ConfigureAwait(false);
        }
        finally
        {
            ExecuteUpdateExtensions.ExecuteUpdateParameters.Value = null!;
        }
    }

    /// <summary>
    /// Test ExecuteInsertAsync with a source that has a Where clause.
    /// </summary>
    /// <param name="databaseType">Database type.</param>
    /// <returns>A <see cref="Task"/> that represents the asynchronous operation.</returns>
    /// <remarks>An insert can't have a Where clause, so it must be rejected rather than run as an update.</remarks>
    [TestMethod(DisplayName = "ExecuteInsertAsync with a source that has a Where clause")]
    [DataRow(DatabaseTypes.Sqlite, DisplayName = DatabaseTypes.Sqlite)]
    [DataRow(DatabaseTypes.SqlServer, DisplayName = DatabaseTypes.SqlServer)]
    public async Task ExecuteInsertTests_005_Async(string databaseType)
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
            () => source.ExecuteInsertAsync(
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

    #endregion public methods
}
