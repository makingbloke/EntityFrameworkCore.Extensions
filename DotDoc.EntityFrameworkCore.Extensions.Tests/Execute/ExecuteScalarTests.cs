// Copyright ©2021-2026 Mike King.
// This file is licensed to you under the MIT license.
// See the License.txt file in the solution root for more information.

using DotDoc.EntityFrameworkCore.Extensions.DatabaseType;
using DotDoc.EntityFrameworkCore.Extensions.Execute;
using DotDoc.EntityFrameworkCore.Extensions.Tests.Data;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace DotDoc.EntityFrameworkCore.Extensions.Tests.Execute;

/// <summary>
/// Tests for ExecuteScalar extensions.
/// </summary>
[TestClass]
public class ExecuteScalarTests
{
    #region private fields

    /// <summary>
    /// The MSTest <see cref="TestContext"/> for the current test.
    /// </summary>
    private readonly TestContext _testContext;

    #endregion private fields

    #region public constructors

    /// <summary>
    /// Initializes a new instance of the <see cref="ExecuteScalarTests"/> class.
    /// </summary>
    /// <param name="testContext">The MSTest test context.</param>
    public ExecuteScalarTests(TestContext testContext)
    {
        this._testContext = testContext;
    }

    #endregion public constructors

    #region ExecuteScalarAsync tests

    /// <summary>
    /// Test ExecuteScalarAsync with Null DatabaseFacade Database and FormattableString Sql parameters.
    /// </summary>
    /// <returns>A <see cref="Task"/> that represents the asynchronous operation.</returns>
    [TestMethod(DisplayName = "ExecuteScalarAsync with Null DatabaseFacade Database and FormattableString Sql parameters")]
    public async Task ExecuteScalarTests_001_Async()
    {
        // ARRANGE
        DatabaseFacade database = null!;
        FormattableString sql = $"dummy";

        // ACT / ASSERT
        await Assert.ThrowsExactlyAsync<ArgumentNullException>(() => database.ExecuteScalarAsync<int>(sql, this._testContext.CancellationToken), "Unexpected exception").ConfigureAwait(false);
    }

    /// <summary>
    /// Test ExecuteScalarAsync with Null DatabaseFacade Database and String Sql parameters.
    /// </summary>
    /// <returns>A <see cref="Task"/> that represents the asynchronous operation.</returns>
    [TestMethod(DisplayName = "ExecuteScalarAsync with Null DatabaseFacade Database and String Sql parameters")]
    public async Task ExecuteScalarTests_002_Async()
    {
        // ARRANGE
        DatabaseFacade database = null!;
        string sql = "dummy";

        // ACT / ASSERT
        await Assert.ThrowsExactlyAsync<ArgumentNullException>(() => database.ExecuteScalarAsync<int>(sql, this._testContext.CancellationToken), "Unexpected exception").ConfigureAwait(false);
    }

    /// <summary>
    /// Test ExecuteScalarAsync with a Null FormattableString Sql parameter.
    /// </summary>
    /// <returns>A <see cref="Task"/> that represents the asynchronous operation.</returns>
    [TestMethod(DisplayName = "ExecuteScalarAsync with a Null FormattableString Sql parameter")]
    public async Task ExecuteScalarTests_003_Async()
    {
        // ARRANGE
        using Context context = await DatabaseUtils.OpenDatabaseAsync(DatabaseTypes.Sqlite).ConfigureAwait(false);
        FormattableString sql = null!;

        // ACT / ASSERT
        await Assert.ThrowsExactlyAsync<ArgumentNullException>(() => context.Database.ExecuteScalarAsync<int>(sql, this._testContext.CancellationToken), "Unexpected exception").ConfigureAwait(false);
    }

    /// <summary>
    /// Test ExecuteScalarAsync with a Null or empty String Sql parameter.
    /// </summary>
    /// <param name="sql">The SQL string.</param>
    /// <param name="exceptionType">The type of exception raised.</param>
    /// <returns>A <see cref="Task"/> that represents the asynchronous operation.</returns>
    [TestMethod(DisplayName = "ExecuteScalarAsync with a Null or empty String Sql parameter")]
    [DataRow(null, typeof(ArgumentNullException), DisplayName = "Null")]
    [DataRow("", typeof(ArgumentException), DisplayName = "Empty")]
    public async Task ExecuteScalarTests_004_Async(string sql, Type exceptionType)
    {
        // ARRANGE
        using Context context = await DatabaseUtils.OpenDatabaseAsync(DatabaseTypes.Sqlite).ConfigureAwait(false);

        // ACT / ASSERT
        Exception e = await Assert.ThrowsAsync<Exception>(() => context.Database.ExecuteScalarAsync<int>(sql, this._testContext.CancellationToken), "Unexpected exception").ConfigureAwait(false);
        Assert.IsInstanceOfType(e, exceptionType, "Invalid exception type");
    }

    /// <summary>
    /// Test ExecuteScalarAsync with FormattableString Sql parameter.
    /// </summary>
    /// <param name="databaseType">Database type.</param>
    /// <returns>A <see cref="Task"/> that represents the asynchronous operation.</returns>
    [TestMethod(DisplayName = "ExecuteScalarAsync with FormattableString Sql parameter")]
    [DataRow(DatabaseTypes.Sqlite, DisplayName = DatabaseTypes.Sqlite)]
    [DataRow(DatabaseTypes.SqlServer, DisplayName = DatabaseTypes.SqlServer)]
    public async Task ExecuteScalarTests_005_Async(string databaseType)
    {
        // ARRANGE
        using Context context = await DatabaseUtils.OpenDatabaseAsync(databaseType).ConfigureAwait(false);

        string value = "TestValue";
        await DatabaseUtils.CreateTestTableEntriesAsync(context, value, 1).ConfigureAwait(false);

        long minId = 0;
        FormattableString sql = $"SELECT COUNT(*) FROM TestTable1 WHERE ID > {minId}";

        // ACT
        int count = await context.Database.ExecuteScalarAsync<int>(sql, this._testContext.CancellationToken).ConfigureAwait(false);

        // ASSERT
        Assert.AreEqual(1, count, "Invalid count");
    }

    /// <summary>
    /// Test ExecuteScalarAsync with a String Sql parameter.
    /// </summary>
    /// <param name="databaseType">Database type.</param>
    /// <returns>A <see cref="Task"/> that represents the asynchronous operation.</returns>
    [TestMethod(DisplayName = "ExecuteScalarAsync with a String Sql parameter")]
    [DataRow(DatabaseTypes.Sqlite, DisplayName = DatabaseTypes.Sqlite)]
    [DataRow(DatabaseTypes.SqlServer, DisplayName = DatabaseTypes.SqlServer)]
    public async Task ExecuteScalarTests_006_Async(string databaseType)
    {
        // ARRANGE
        using Context context = await DatabaseUtils.OpenDatabaseAsync(databaseType).ConfigureAwait(false);

        string value = "TestValue";
        await DatabaseUtils.CreateTestTableEntriesAsync(context, value, 1).ConfigureAwait(false);

        long minId = 0;
        string sql = "SELECT COUNT(*) FROM TestTable1 WHERE ID > {0}";

        // ACT
        int count = await context.Database.ExecuteScalarAsync<int>(sql, this._testContext.CancellationToken, minId).ConfigureAwait(false);

        // ASSERT
        Assert.AreEqual(1, count, "Invalid count");
    }

    /// <summary>
    /// Test ExecuteScalarAsync returning a database NULL as a nullable value type.
    /// </summary>
    /// <param name="databaseType">Database type.</param>
    /// <returns>A <see cref="Task"/> that represents the asynchronous operation.</returns>
    /// <remarks>MAX of an empty table returns one row containing NULL, which the provider returns as <see cref="DBNull"/> (not <see langword="null"/>).</remarks>
    [TestMethod(DisplayName = "ExecuteScalarAsync returning a database NULL as a nullable value type")]
    [DataRow(DatabaseTypes.Sqlite, DisplayName = DatabaseTypes.Sqlite)]
    [DataRow(DatabaseTypes.SqlServer, DisplayName = DatabaseTypes.SqlServer)]
    public async Task ExecuteScalarTests_007_Async(string databaseType)
    {
        // ARRANGE
        using Context context = await DatabaseUtils.OpenDatabaseAsync(databaseType).ConfigureAwait(false);

        FormattableString sql = $"SELECT MAX(Id) FROM TestTable1";

        // ACT
        long? maxId = await context.Database.ExecuteScalarAsync<long?>(sql, this._testContext.CancellationToken).ConfigureAwait(false);

        // ASSERT
        Assert.IsNull(maxId, "Invalid value");
    }

    /// <summary>
    /// Test ExecuteScalarAsync returning a database NULL as a string.
    /// </summary>
    /// <param name="databaseType">Database type.</param>
    /// <returns>A <see cref="Task"/> that represents the asynchronous operation.</returns>
    /// <remarks>MAX of an empty table returns one row containing NULL, which the provider returns as <see cref="DBNull"/> (not <see langword="null"/>).</remarks>
    [TestMethod(DisplayName = "ExecuteScalarAsync returning a database NULL as a string")]
    [DataRow(DatabaseTypes.Sqlite, DisplayName = DatabaseTypes.Sqlite)]
    [DataRow(DatabaseTypes.SqlServer, DisplayName = DatabaseTypes.SqlServer)]
    public async Task ExecuteScalarTests_008_Async(string databaseType)
    {
        // ARRANGE
        using Context context = await DatabaseUtils.OpenDatabaseAsync(databaseType).ConfigureAwait(false);

        FormattableString sql = $"SELECT MAX(TestField) FROM TestTable1";

        // ACT
        string? maxValue = await context.Database.ExecuteScalarAsync<string?>(sql, this._testContext.CancellationToken).ConfigureAwait(false);

        // ASSERT
        Assert.IsNull(maxValue, "Invalid value");
    }

    /// <summary>
    /// Test ExecuteScalarAsync with a query that returns no rows.
    /// </summary>
    /// <param name="databaseType">Database type.</param>
    /// <returns>A <see cref="Task"/> that represents the asynchronous operation.</returns>
    [TestMethod(DisplayName = "ExecuteScalarAsync with a query that returns no rows")]
    [DataRow(DatabaseTypes.Sqlite, DisplayName = DatabaseTypes.Sqlite)]
    [DataRow(DatabaseTypes.SqlServer, DisplayName = DatabaseTypes.SqlServer)]
    public async Task ExecuteScalarTests_009_Async(string databaseType)
    {
        // ARRANGE
        using Context context = await DatabaseUtils.OpenDatabaseAsync(databaseType).ConfigureAwait(false);

        FormattableString sql = $"SELECT Id FROM TestTable1";

        // ACT
        long id = await context.Database.ExecuteScalarAsync<long>(sql, this._testContext.CancellationToken).ConfigureAwait(false);

        // ASSERT
        Assert.AreEqual(0, id, "Invalid value");
    }

    /// <summary>
    /// Test ExecuteScalarAsync with a Null IEnumerable Parameters parameter.
    /// </summary>
    /// <returns>A <see cref="Task"/> that represents the asynchronous operation.</returns>
    [TestMethod(DisplayName = "ExecuteScalarAsync with a Null IEnumerable Parameters parameter")]
    public async Task ExecuteScalarTests_010_Async()
    {
        // ARRANGE
        using Context context = await DatabaseUtils.OpenDatabaseAsync(DatabaseTypes.Sqlite).ConfigureAwait(false);

        string sql = "SELECT COUNT(*) FROM TestTable1";
        IEnumerable<object?> parameters = null!;

        // ACT / ASSERT
        await Assert.ThrowsExactlyAsync<ArgumentNullException>(() => context.Database.ExecuteScalarAsync<int>(sql, this._testContext.CancellationToken, parameters), "Unexpected exception").ConfigureAwait(false);
    }

    #endregion ExecuteScalarAsync tests
}
