// Copyright ©2021-2026 Mike King.
// This file is licensed to you under the MIT license.
// See the License.txt file in the solution root for more information.

using DotDoc.EntityFrameworkCore.Extensions.CustomQueryGenerators;
using DotDoc.EntityFrameworkCore.Extensions.DatabaseType;
using DotDoc.EntityFrameworkCore.Extensions.ExecuteUpdate;
using DotDoc.EntityFrameworkCore.Extensions.Tests.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace DotDoc.EntityFrameworkCore.Extensions.Tests.ExecuteUpdate;

/// <summary>
/// Tests that EF's query cache keeps the ExecuteXxx variants apart.
/// </summary>
/// <remarks>
/// <para>
/// EF compiles each query once and caches the generated SQL, keyed on the expression tree. Several variants build the same expression
/// (e.g. <c>ExecuteUpdateAsync</c> and <c>ExecuteInsertAsync</c> both call EF's <c>ExecuteUpdateAsync</c>), so each one tags its query
/// with its variant to get its own cache entry. Without that, whichever variant ran first decided the SQL for the others.
/// </para>
/// <para>
/// Each test runs two variants with the same query shape on one context, in both orders, and checks the data after each call.
/// <see cref="Context"/> turns service provider caching off, so every context has its own query cache and other tests can't fill it first.
/// </para>
/// </remarks>
[TestClass]
public class ExecuteUpdateQueryCacheTests
{
    #region private constants

    /// <summary>
    /// The value the test rows are created with.
    /// </summary>
    private const string OriginalValue = "Original TestValue";

    /// <summary>
    /// The number of test rows created.
    /// </summary>
    private const int RowCount = 5;

    #endregion private constants

    #region private fields

    /// <summary>
    /// The MSTest <see cref="TestContext"/> for the current test.
    /// </summary>
    private readonly TestContext _testContext;

    #endregion private fields

    #region public constructors

    /// <summary>
    /// Initializes a new instance of the <see cref="ExecuteUpdateQueryCacheTests"/> class.
    /// </summary>
    /// <param name="testContext">The MSTest test context.</param>
    public ExecuteUpdateQueryCacheTests(TestContext testContext)
    {
        this._testContext = testContext;
    }

    #endregion public constructors

    #region update and update get rows tests

    /// <summary>
    /// Test ExecuteUpdateAsync and ExecuteUpdateGetRowsAsync with the same query shape.
    /// </summary>
    /// <param name="databaseType">Database type.</param>
    /// <param name="plainUpdateFirst">If true the plain update runs first, else the update get rows runs first.</param>
    /// <returns>A <see cref="Task"/> that represents the asynchronous operation.</returns>
    [TestMethod(DisplayName = "ExecuteUpdateAsync and ExecuteUpdateGetRowsAsync with the same query shape")]
    [DataRow(DatabaseTypes.Sqlite, true, DisplayName = $"{DatabaseTypes.Sqlite} Plain Update First")]
    [DataRow(DatabaseTypes.Sqlite, false, DisplayName = $"{DatabaseTypes.Sqlite} Update Get Rows First")]
    [DataRow(DatabaseTypes.SqlServer, true, DisplayName = $"{DatabaseTypes.SqlServer} Plain Update First")]
    [DataRow(DatabaseTypes.SqlServer, false, DisplayName = $"{DatabaseTypes.SqlServer} Update Get Rows First")]
    public async Task ExecuteUpdateQueryCacheTests_001_Async(string databaseType, bool plainUpdateFirst)
    {
        // ARRANGE
        using Context context = await OpenDatabaseAsync(databaseType).ConfigureAwait(false);

        long plainUpdateId = 2;
        string plainUpdateValue = "Plain Update TestValue";
        long getRowsId = 3;
        string getRowsValue = "Update Get Rows TestValue";

        // ACT / ASSERT
        if (plainUpdateFirst)
        {
            await RunPlainUpdateAsync().ConfigureAwait(false);
            await RunUpdateGetRowsAsync().ConfigureAwait(false);
        }
        else
        {
            await RunUpdateGetRowsAsync().ConfigureAwait(false);
            await RunPlainUpdateAsync().ConfigureAwait(false);
        }

        Dictionary<long, string> values = await this.GetValuesAsync(context).ConfigureAwait(false);

        Assert.HasCount(RowCount, values, "Unexpected count");
        Assert.AreEqual(plainUpdateValue, values[plainUpdateId], "Unexpected value for the plain update row");
        Assert.AreEqual(getRowsValue, values[getRowsId], "Unexpected value for the update get rows row");
        Assert.AreEqual(RowCount - 2, values.Values.Count(v => v == OriginalValue), "Other rows have been changed");

        async Task RunPlainUpdateAsync()
        {
            int count = await QueryById(context, plainUpdateId)
                .ExecuteUpdateAsync(builder => builder.SetProperty(e => e.TestField, plainUpdateValue), this._testContext.CancellationToken)
                .ConfigureAwait(false);

            Assert.AreEqual(1, count, "Unexpected plain update count");
        }

        async Task RunUpdateGetRowsAsync()
        {
            IList<TestTable1> rows = await QueryById(context, getRowsId)
                .ExecuteUpdateGetRowsAsync(builder => builder.SetProperty(e => e.TestField, getRowsValue), this._testContext.CancellationToken)
                .ConfigureAwait(false);

            Assert.HasCount(1, rows, "Unexpected update get rows count");
            Assert.AreEqual(getRowsId, rows[0].Id, "Unexpected update get rows Id value");
            Assert.AreEqual(getRowsValue, rows[0].TestField, "Unexpected update get rows field value");
        }
    }

    #endregion update and update get rows tests

    #region update and insert tests

    /// <summary>
    /// Test ExecuteUpdateAsync and ExecuteInsertAsync with the same query shape.
    /// </summary>
    /// <param name="databaseType">Database type.</param>
    /// <param name="updateFirst">If true the update runs first, else the insert runs first.</param>
    /// <returns>A <see cref="Task"/> that represents the asynchronous operation.</returns>
    /// <remarks>An insert can't have a Where clause, so both run against the whole table.</remarks>
    [TestMethod(DisplayName = "ExecuteUpdateAsync and ExecuteInsertAsync with the same query shape")]
    [DataRow(DatabaseTypes.Sqlite, true, DisplayName = $"{DatabaseTypes.Sqlite} Update First")]
    [DataRow(DatabaseTypes.Sqlite, false, DisplayName = $"{DatabaseTypes.Sqlite} Insert First")]
    [DataRow(DatabaseTypes.SqlServer, true, DisplayName = $"{DatabaseTypes.SqlServer} Update First")]
    [DataRow(DatabaseTypes.SqlServer, false, DisplayName = $"{DatabaseTypes.SqlServer} Insert First")]
    public async Task ExecuteUpdateQueryCacheTests_002_Async(string databaseType, bool updateFirst)
    {
        // ARRANGE
        using Context context = await OpenDatabaseAsync(databaseType).ConfigureAwait(false);

        string updatedValue = "Updated TestValue";
        string insertedValue = "Inserted TestValue";

        // ACT / ASSERT
        if (updateFirst)
        {
            int count = await RunUpdateAsync().ConfigureAwait(false);
            Assert.AreEqual(RowCount, count, "Unexpected update count");

            await RunInsertAsync().ConfigureAwait(false);

            Dictionary<long, string> values = await this.GetValuesAsync(context).ConfigureAwait(false);
            Assert.HasCount(RowCount + 1, values, "Unexpected count");
            Assert.AreEqual(RowCount, values.Values.Count(v => v == updatedValue), "Unexpected number of updated rows");
            Assert.ContainsSingle(v => v == insertedValue, values.Values, "Unexpected number of inserted rows");
        }
        else
        {
            await RunInsertAsync().ConfigureAwait(false);

            Dictionary<long, string> values = await this.GetValuesAsync(context).ConfigureAwait(false);
            Assert.HasCount(RowCount + 1, values, "Unexpected count after the insert");
            Assert.AreEqual(RowCount, values.Values.Count(v => v == OriginalValue), "Existing rows have been changed by the insert");
            Assert.ContainsSingle(v => v == insertedValue, values.Values, "Unexpected number of inserted rows");

            int count = await RunUpdateAsync().ConfigureAwait(false);
            Assert.AreEqual(RowCount + 1, count, "Unexpected update count");

            values = await this.GetValuesAsync(context).ConfigureAwait(false);
            Assert.HasCount(RowCount + 1, values, "Unexpected count after the update");
            Assert.IsTrue(values.Values.All(v => v == updatedValue), "Not every row has been updated");
        }

        Task<int> RunUpdateAsync()
        {
            return context.TestTable1
                .ExecuteUpdateAsync(builder => builder.SetProperty(e => e.TestField, updatedValue), this._testContext.CancellationToken);
        }

        Task RunInsertAsync()
        {
            return context.TestTable1
                .ExecuteInsertAsync(builder => builder.SetProperty(e => e.TestField, insertedValue), this._testContext.CancellationToken);
        }
    }

    #endregion update and insert tests

    #region insert and insert get row tests

    /// <summary>
    /// Test ExecuteInsertAsync and ExecuteInsertGetRowAsync with the same query shape.
    /// </summary>
    /// <param name="databaseType">Database type.</param>
    /// <param name="insertFirst">If true the insert runs first, else the insert get row runs first.</param>
    /// <returns>A <see cref="Task"/> that represents the asynchronous operation.</returns>
    [TestMethod(DisplayName = "ExecuteInsertAsync and ExecuteInsertGetRowAsync with the same query shape")]
    [DataRow(DatabaseTypes.Sqlite, true, DisplayName = $"{DatabaseTypes.Sqlite} Insert First")]
    [DataRow(DatabaseTypes.Sqlite, false, DisplayName = $"{DatabaseTypes.Sqlite} Insert Get Row First")]
    [DataRow(DatabaseTypes.SqlServer, true, DisplayName = $"{DatabaseTypes.SqlServer} Insert First")]
    [DataRow(DatabaseTypes.SqlServer, false, DisplayName = $"{DatabaseTypes.SqlServer} Insert Get Row First")]
    public async Task ExecuteUpdateQueryCacheTests_003_Async(string databaseType, bool insertFirst)
    {
        // ARRANGE
        using Context context = await OpenDatabaseAsync(databaseType).ConfigureAwait(false);

        string insertValue = "Insert TestValue";
        string insertGetRowValue = "Insert Get Row TestValue";

        // ACT / ASSERT
        if (insertFirst)
        {
            await RunInsertAsync().ConfigureAwait(false);
            await RunInsertGetRowAsync().ConfigureAwait(false);
        }
        else
        {
            await RunInsertGetRowAsync().ConfigureAwait(false);
            await RunInsertAsync().ConfigureAwait(false);
        }

        Dictionary<long, string> values = await this.GetValuesAsync(context).ConfigureAwait(false);

        Assert.HasCount(RowCount + 2, values, "Unexpected count");
        Assert.ContainsSingle(v => v == insertValue, values.Values, "Unexpected number of rows from the insert");
        Assert.ContainsSingle(v => v == insertGetRowValue, values.Values, "Unexpected number of rows from the insert get row");
        Assert.AreEqual(RowCount, values.Values.Count(v => v == OriginalValue), "Existing rows have been changed");

        Task RunInsertAsync()
        {
            return context.TestTable1
                .ExecuteInsertAsync(builder => builder.SetProperty(e => e.TestField, insertValue), this._testContext.CancellationToken);
        }

        async Task RunInsertGetRowAsync()
        {
            TestTable1 row = await context.TestTable1
                .ExecuteInsertGetRowAsync(builder => builder.SetProperty(e => e.TestField, insertGetRowValue), this._testContext.CancellationToken)
                .ConfigureAwait(false);

            Assert.AreEqual(insertGetRowValue, row.TestField, "Unexpected insert get row field value");
        }
    }

    #endregion insert and insert get row tests

    #region delete and delete get rows tests

    /// <summary>
    /// Test ExecuteDeleteAsync and ExecuteDeleteGetRowsAsync with the same query shape.
    /// </summary>
    /// <param name="databaseType">Database type.</param>
    /// <param name="plainDeleteFirst">If true the plain delete runs first, else the delete get rows runs first.</param>
    /// <returns>A <see cref="Task"/> that represents the asynchronous operation.</returns>
    [TestMethod(DisplayName = "ExecuteDeleteAsync and ExecuteDeleteGetRowsAsync with the same query shape")]
    [DataRow(DatabaseTypes.Sqlite, true, DisplayName = $"{DatabaseTypes.Sqlite} Plain Delete First")]
    [DataRow(DatabaseTypes.Sqlite, false, DisplayName = $"{DatabaseTypes.Sqlite} Delete Get Rows First")]
    [DataRow(DatabaseTypes.SqlServer, true, DisplayName = $"{DatabaseTypes.SqlServer} Plain Delete First")]
    [DataRow(DatabaseTypes.SqlServer, false, DisplayName = $"{DatabaseTypes.SqlServer} Delete Get Rows First")]
    public async Task ExecuteUpdateQueryCacheTests_004_Async(string databaseType, bool plainDeleteFirst)
    {
        // ARRANGE
        using Context context = await OpenDatabaseAsync(databaseType).ConfigureAwait(false);

        long plainDeleteId = 2;
        long getRowsId = 3;

        // ACT / ASSERT
        if (plainDeleteFirst)
        {
            await RunPlainDeleteAsync().ConfigureAwait(false);
            await RunDeleteGetRowsAsync().ConfigureAwait(false);
        }
        else
        {
            await RunDeleteGetRowsAsync().ConfigureAwait(false);
            await RunPlainDeleteAsync().ConfigureAwait(false);
        }

        Dictionary<long, string> values = await this.GetValuesAsync(context).ConfigureAwait(false);

        Assert.HasCount(RowCount - 2, values, "Unexpected count");
        Assert.IsFalse(values.ContainsKey(plainDeleteId), "The plain delete row has not been deleted");
        Assert.IsFalse(values.ContainsKey(getRowsId), "The delete get rows row has not been deleted");

        async Task RunPlainDeleteAsync()
        {
            int count = await QueryById(context, plainDeleteId)
                .ExecuteDeleteAsync(this._testContext.CancellationToken)
                .ConfigureAwait(false);

            Assert.AreEqual(1, count, "Unexpected plain delete count");
        }

        async Task RunDeleteGetRowsAsync()
        {
            IList<TestTable1> rows = await QueryById(context, getRowsId)
                .ExecuteDeleteGetRowsAsync(this._testContext.CancellationToken)
                .ConfigureAwait(false);

            Assert.HasCount(1, rows, "Unexpected delete get rows count");
            Assert.AreEqual(getRowsId, rows[0].Id, "Unexpected delete get rows Id value");
            Assert.AreEqual(OriginalValue, rows[0].TestField, "Unexpected delete get rows field value");
        }
    }

    #endregion delete and delete get rows tests

    #region same variant tests

    /// <summary>
    /// Test two ExecuteUpdateGetRowsAsync calls with the same Where clause and different setters.
    /// </summary>
    /// <param name="databaseType">Database type.</param>
    /// <returns>A <see cref="Task"/> that represents the asynchronous operation.</returns>
    /// <remarks>The setters are part of the cache key, so each call must get its own SQL.</remarks>
    [TestMethod(DisplayName = "Two ExecuteUpdateGetRowsAsync calls with the same Where clause and different setters")]
    [DataRow(DatabaseTypes.Sqlite, DisplayName = DatabaseTypes.Sqlite)]
    [DataRow(DatabaseTypes.SqlServer, DisplayName = DatabaseTypes.SqlServer)]
    public async Task ExecuteUpdateQueryCacheTests_005_Async(string databaseType)
    {
        // ARRANGE
        using Context context = await OpenDatabaseAsync(databaseType).ConfigureAwait(false);

        long maxId = 2;
        string setValue = "Set TestValue";
        string suffix = " With Suffix";

        // ACT
        IList<TestTable1> setRows = await context.TestTable1
            .Where(e => e.Id <= maxId)
            .ExecuteUpdateGetRowsAsync(builder => builder.SetProperty(e => e.TestField, setValue), this._testContext.CancellationToken)
            .ConfigureAwait(false);

        IList<TestTable1> appendRows = await context.TestTable1
            .Where(e => e.Id <= maxId)
            .ExecuteUpdateGetRowsAsync(builder => builder.SetProperty(e => e.TestField, e => e.TestField + suffix), this._testContext.CancellationToken)
            .ConfigureAwait(false);

        // ASSERT
        Assert.HasCount(2, setRows, "Unexpected count from the first update");
        Assert.IsTrue(setRows.All(r => r.TestField == setValue), "Unexpected field value from the first update");

        Assert.HasCount(2, appendRows, "Unexpected count from the second update");
        Assert.IsTrue(appendRows.All(r => r.TestField == setValue + suffix), "Unexpected field value from the second update");

        Dictionary<long, string> values = await this.GetValuesAsync(context).ConfigureAwait(false);

        Assert.AreEqual(2, values.Values.Count(v => v == setValue + suffix), "Unexpected number of updated rows");
        Assert.AreEqual(RowCount - 2, values.Values.Count(v => v == OriginalValue), "Other rows have been changed");
    }

    /// <summary>
    /// Test the same variant called twice with different values.
    /// </summary>
    /// <param name="databaseType">Database type.</param>
    /// <returns>A <see cref="Task"/> that represents the asynchronous operation.</returns>
    /// <remarks>The calls share one cache entry, so the values must be passed as parameters rather than being cached with the SQL.</remarks>
    [TestMethod(DisplayName = "The same variant called twice with different values")]
    [DataRow(DatabaseTypes.Sqlite, DisplayName = DatabaseTypes.Sqlite)]
    [DataRow(DatabaseTypes.SqlServer, DisplayName = DatabaseTypes.SqlServer)]
    public async Task ExecuteUpdateQueryCacheTests_006_Async(string databaseType)
    {
        // ARRANGE
        using Context context = await OpenDatabaseAsync(databaseType).ConfigureAwait(false);

        // ACT / ASSERT
        foreach ((long id, string value) in new[] { (2L, "First Updated TestValue"), (3L, "Second Updated TestValue") })
        {
            IList<TestTable1> rows = await QueryById(context, id)
                .ExecuteUpdateGetRowsAsync(builder => builder.SetProperty(e => e.TestField, value), this._testContext.CancellationToken)
                .ConfigureAwait(false);

            Assert.HasCount(1, rows, $"Unexpected update count for {id}");
            Assert.AreEqual(id, rows[0].Id, $"Unexpected update Id value for {id}");
            Assert.AreEqual(value, rows[0].TestField, $"Unexpected update field value for {id}");
        }

        foreach (string value in new[] { "First Inserted TestValue", "Second Inserted TestValue" })
        {
            TestTable1 row = await context.TestTable1
                .ExecuteInsertGetRowAsync(builder => builder.SetProperty(e => e.TestField, value), this._testContext.CancellationToken)
                .ConfigureAwait(false);

            Assert.AreEqual(value, row.TestField, $"Unexpected insert field value for {value}");
        }

        Dictionary<long, string> values = await this.GetValuesAsync(context).ConfigureAwait(false);

        Assert.HasCount(RowCount + 2, values, "Unexpected count");
        Assert.AreEqual("First Updated TestValue", values[2], "Unexpected value for the first update");
        Assert.AreEqual("Second Updated TestValue", values[3], "Unexpected value for the second update");
        Assert.ContainsSingle(v => v == "First Inserted TestValue", values.Values, "Unexpected number of rows from the first insert");
        Assert.ContainsSingle(v => v == "Second Inserted TestValue", values.Values, "Unexpected number of rows from the second insert");
    }

    #endregion same variant tests

    #region tag tests

    /// <summary>
    /// Test the ExecuteUpdate tag is not included in the SQL.
    /// </summary>
    /// <param name="databaseType">Database type.</param>
    /// <returns>A <see cref="Task"/> that represents the asynchronous operation.</returns>
    /// <remarks>The generators read the tag to choose the SQL, then remove it so that it isn't written out in the header comment.</remarks>
    [TestMethod(DisplayName = "The ExecuteUpdate tag is not included in the SQL")]
    [DataRow(DatabaseTypes.Sqlite, DisplayName = DatabaseTypes.Sqlite)]
    [DataRow(DatabaseTypes.SqlServer, DisplayName = DatabaseTypes.SqlServer)]
    public async Task ExecuteUpdateQueryCacheTests_007_Async(string databaseType)
    {
        // ARRANGE
        List<string> commands = [];

        using Context context = await DatabaseUtils.OpenDatabaseAsync(
            databaseType,
            customConfigurationActions: optionsBuilder =>
            {
                optionsBuilder.UseExecuteUpdateExtensions();
                optionsBuilder.LogTo(commands.Add, [RelationalEventId.CommandExecuting]);
            })
            .ConfigureAwait(false);

        await DatabaseUtils.CreateTestTableEntriesAsync(context, OriginalValue, RowCount).ConfigureAwait(false);

        string userTag = "User Tag";
        commands.Clear();

        // ACT
        await context.TestTable1
            .TagWith(userTag)
            .ExecuteInsertAsync(builder => builder.SetProperty(e => e.TestField, "Inserted TestValue"), this._testContext.CancellationToken)
            .ConfigureAwait(false);

        await context.TestTable1
            .TagWith(userTag)
            .ExecuteInsertGetRowAsync(builder => builder.SetProperty(e => e.TestField, "Inserted Get Row TestValue"), this._testContext.CancellationToken)
            .ConfigureAwait(false);

        await QueryById(context, 1)
            .TagWith(userTag)
            .ExecuteUpdateGetRowsAsync(builder => builder.SetProperty(e => e.TestField, "Updated TestValue"), this._testContext.CancellationToken)
            .ConfigureAwait(false);

        await QueryById(context, 2)
            .TagWith(userTag)
            .ExecuteDeleteGetRowsAsync(this._testContext.CancellationToken)
            .ConfigureAwait(false);

        // ASSERT
        Assert.IsNotEmpty(commands, "No commands were logged");

        foreach (string command in commands)
        {
            Assert.DoesNotContain(CustomTagsHandler.ExecuteUpdateTagPrefix, command, "ExecuteUpdate tag included in SQL");
        }

        // Each call runs one command through EF (captured and suppressed for the get rows variants), and the get rows variants
        // then run the captured SQL. All of them must keep the caller's own tag.
        Assert.HasCount(7, commands, "Unexpected number of commands");
        Assert.IsTrue(commands.All(c => c.Contains(userTag, StringComparison.Ordinal)), "Caller's tag missing from SQL");
    }

    #endregion tag tests

    #region private methods

    /// <summary>
    /// Build a query for one row. Every call builds the same query shape, with the Id passed as a parameter.
    /// </summary>
    /// <param name="context">The database context.</param>
    /// <param name="id">The Id of the row.</param>
    /// <returns>The query.</returns>
    private static IQueryable<TestTable1> QueryById(Context context, long id)
    {
        return context.TestTable1.Where(e => e.Id == id);
    }

    /// <summary>
    /// Open a new database with the Execute Update Extensions and <see cref="RowCount"/> test rows.
    /// </summary>
    /// <param name="databaseType">Database type.</param>
    /// <returns>The database context.</returns>
    private static async Task<Context> OpenDatabaseAsync(string databaseType)
    {
        Context context = await DatabaseUtils.OpenDatabaseAsync(
            databaseType,
            customConfigurationActions: optionsBuilder => optionsBuilder.UseExecuteUpdateExtensions())
            .ConfigureAwait(false);

        await DatabaseUtils.CreateTestTableEntriesAsync(context, OriginalValue, RowCount).ConfigureAwait(false);

        return context;
    }

    /// <summary>
    /// Read the test table from the database (not tracked, so the values are the ones in the database).
    /// </summary>
    /// <param name="context">The database context.</param>
    /// <returns>The <see cref="TestTable1.TestField"/> value of each row, keyed by Id.</returns>
    private async Task<Dictionary<long, string>> GetValuesAsync(Context context)
    {
        return await context.TestTable1
            .AsNoTracking()
            .ToDictionaryAsync(e => e.Id, e => e.TestField, this._testContext.CancellationToken)
            .ConfigureAwait(false);
    }

    #endregion private methods
}
