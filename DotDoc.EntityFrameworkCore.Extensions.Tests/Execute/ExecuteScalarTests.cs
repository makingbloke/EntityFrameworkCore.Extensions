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
        using Context context = await DatabaseUtils.OpenDatabaseAsync(this._testContext, DatabaseTypes.Sqlite).ConfigureAwait(false);
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
        using Context context = await DatabaseUtils.OpenDatabaseAsync(this._testContext, DatabaseTypes.Sqlite).ConfigureAwait(false);

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
        using Context context = await DatabaseUtils.OpenDatabaseAsync(this._testContext, databaseType).ConfigureAwait(false);

        string value = "TestValue";
        await DatabaseUtils.CreateTestTableEntriesAsync(context, value, 1, this._testContext.CancellationToken).ConfigureAwait(false);

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
        using Context context = await DatabaseUtils.OpenDatabaseAsync(this._testContext, databaseType).ConfigureAwait(false);

        string value = "TestValue";
        await DatabaseUtils.CreateTestTableEntriesAsync(context, value, 1, this._testContext.CancellationToken).ConfigureAwait(false);

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
        using Context context = await DatabaseUtils.OpenDatabaseAsync(this._testContext, databaseType).ConfigureAwait(false);

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
        using Context context = await DatabaseUtils.OpenDatabaseAsync(this._testContext, databaseType).ConfigureAwait(false);

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
        using Context context = await DatabaseUtils.OpenDatabaseAsync(this._testContext, databaseType).ConfigureAwait(false);

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
        using Context context = await DatabaseUtils.OpenDatabaseAsync(this._testContext, DatabaseTypes.Sqlite).ConfigureAwait(false);

        string sql = "SELECT COUNT(*) FROM TestTable1";
        IEnumerable<object?> parameters = null!;

        // ACT / ASSERT
        await Assert.ThrowsExactlyAsync<ArgumentNullException>(() => context.Database.ExecuteScalarAsync<int>(sql, this._testContext.CancellationToken, parameters), "Unexpected exception").ConfigureAwait(false);
    }

    /// <summary>
    /// Test ExecuteScalarAsync maps its parameters with the model, so model-wide configuration (DefaultTypeMapping) applies to them,
    /// as it does with EF Core's ExecuteSqlRawAsync.
    /// </summary>
    /// <param name="databaseType">Database type.</param>
    /// <returns>A <see cref="Task"/> that represents the asynchronous operation.</returns>
    [TestMethod(DisplayName = "ExecuteScalarAsync parameters use the model's DefaultTypeMapping")]
    [DataRow(DatabaseTypes.Sqlite, DisplayName = DatabaseTypes.Sqlite)]
    [DataRow(DatabaseTypes.SqlServer, DisplayName = DatabaseTypes.SqlServer)]
    public async Task ExecuteScalarTests_011_Async(string databaseType)
    {
        // ARRANGE
        using Context context = await DatabaseUtils.OpenDatabaseAsync(
            this._testContext,
            databaseType,
            customConventionActions: configurationBuilder => configurationBuilder.DefaultTypeMapping<DayOfWeek>().HasConversion<string>())
            .ConfigureAwait(false);

        string sql = "SELECT {0}";

        // ACT
        string? result = await context.Database.ExecuteScalarAsync<string>(sql, this._testContext.CancellationToken, DayOfWeek.Monday).ConfigureAwait(false);

        // ASSERT
        Assert.AreEqual(nameof(DayOfWeek.Monday), result, "The parameter wasn't converted with the model's DefaultTypeMapping");
    }

    /// <summary>
    /// Test ExecuteScalarAsync checks for a second operation on the context at the same time only if thread safety checks are on.
    /// </summary>
    /// <param name="threadSafetyChecksEnabled">Whether thread safety checks are on (EnableThreadSafetyChecks).</param>
    /// <returns>A <see cref="Task"/> that represents the asynchronous operation.</returns>
    /// <remarks>
    /// Another operation is simulated by entering the context's concurrency detector's critical section before the call, from another task
    /// (EF Core lets the same thread or async flow enter it again, so entering it from the test itself wouldn't count as another operation).
    /// </remarks>
    [TestMethod(DisplayName = "ExecuteScalarAsync during another operation on the context")]
    [DataRow(true, DisplayName = "Thread safety checks on")]
    [DataRow(false, DisplayName = "Thread safety checks off")]
    public async Task ExecuteScalarTests_012_Async(bool threadSafetyChecksEnabled)
    {
        // ARRANGE
        using Context context = await DatabaseUtils.OpenDatabaseAsync(
            this._testContext,
            DatabaseTypes.Sqlite,
            customConfigurationActions: optionsBuilder => optionsBuilder.EnableThreadSafetyChecks(threadSafetyChecksEnabled))
            .ConfigureAwait(false);

        FormattableString sql = $"SELECT COUNT(*) FROM TestTable1";

        IConcurrencyDetector concurrencyDetector = context.GetService<IConcurrencyDetector>();
        using ConcurrencyDetectorCriticalSectionDisposer otherOperation = await Task.Run(concurrencyDetector.EnterCriticalSection, this._testContext.CancellationToken).ConfigureAwait(false);

        // ACT / ASSERT
        if (threadSafetyChecksEnabled)
        {
            await Assert.ThrowsExactlyAsync<InvalidOperationException>(() => context.Database.ExecuteScalarAsync<int>(sql, this._testContext.CancellationToken), "Unexpected exception").ConfigureAwait(false);
        }
        else
        {
            int count = await context.Database.ExecuteScalarAsync<int>(sql, this._testContext.CancellationToken).ConfigureAwait(false);
            Assert.AreEqual(0, count, "Invalid count");
        }
    }

    /// <summary>
    /// Test ExecuteScalarAsync returning a GUID stored by EF Core (as text on SQLite, as a uniqueidentifier on SQL Server).
    /// </summary>
    /// <param name="databaseType">Database type.</param>
    /// <returns>A <see cref="Task"/> that represents the asynchronous operation.</returns>
    [TestMethod(DisplayName = "ExecuteScalarAsync returning a Guid")]
    [DataRow(DatabaseTypes.Sqlite, DisplayName = DatabaseTypes.Sqlite)]
    [DataRow(DatabaseTypes.SqlServer, DisplayName = DatabaseTypes.SqlServer)]
    public async Task ExecuteScalarTests_013_Async(string databaseType)
    {
        // ARRANGE
        using Context context = await DatabaseUtils.OpenDatabaseAsync(this._testContext, databaseType).ConfigureAwait(false);

        Guid expected = Guid.NewGuid();
        context.GuidTable.Add(new GuidTable { Id = expected });
        await context.SaveChangesAsync(this._testContext.CancellationToken).ConfigureAwait(false);

        FormattableString sql = $"SELECT Id FROM GuidTable";

        // ACT
        Guid result = await context.Database.ExecuteScalarAsync<Guid>(sql, this._testContext.CancellationToken).ConfigureAwait(false);

        // ASSERT
        Assert.AreEqual(expected, result, "Invalid value");
    }

    /// <summary>
    /// Test ExecuteScalarAsync returning a DateTimeOffset.
    /// </summary>
    /// <param name="databaseType">Database type.</param>
    /// <param name="sql">The SQL, which returns the value in the way the database stores it.</param>
    /// <returns>A <see cref="Task"/> that represents the asynchronous operation.</returns>
    [TestMethod(DisplayName = "ExecuteScalarAsync returning a DateTimeOffset")]
    [DataRow(DatabaseTypes.Sqlite, "SELECT '2026-11-23 10:30:15+01:00'", DisplayName = DatabaseTypes.Sqlite)]
    [DataRow(DatabaseTypes.SqlServer, "SELECT CAST('2026-11-23 10:30:15 +01:00' AS datetimeoffset)", DisplayName = DatabaseTypes.SqlServer)]
    public async Task ExecuteScalarTests_014_Async(string databaseType, string sql)
    {
        // ARRANGE
        using Context context = await DatabaseUtils.OpenDatabaseAsync(this._testContext, databaseType, createNew: false).ConfigureAwait(false);

        // ACT
        DateTimeOffset result = await context.Database.ExecuteScalarAsync<DateTimeOffset>(sql, this._testContext.CancellationToken).ConfigureAwait(false);

        // ASSERT
        Assert.AreEqual(new DateTimeOffset(2026, 11, 23, 10, 30, 15, TimeSpan.FromHours(1)), result, "Invalid value");
    }

    /// <summary>
    /// Test ExecuteScalarAsync returning a TimeSpan.
    /// </summary>
    /// <param name="databaseType">Database type.</param>
    /// <param name="sql">The SQL, which returns the value in the way the database stores it.</param>
    /// <returns>A <see cref="Task"/> that represents the asynchronous operation.</returns>
    [TestMethod(DisplayName = "ExecuteScalarAsync returning a TimeSpan")]
    [DataRow(DatabaseTypes.Sqlite, "SELECT '10:30:15'", DisplayName = DatabaseTypes.Sqlite)]
    [DataRow(DatabaseTypes.SqlServer, "SELECT CAST('10:30:15' AS time)", DisplayName = DatabaseTypes.SqlServer)]
    public async Task ExecuteScalarTests_015_Async(string databaseType, string sql)
    {
        // ARRANGE
        using Context context = await DatabaseUtils.OpenDatabaseAsync(this._testContext, databaseType, createNew: false).ConfigureAwait(false);

        // ACT
        TimeSpan result = await context.Database.ExecuteScalarAsync<TimeSpan>(sql, this._testContext.CancellationToken).ConfigureAwait(false);

        // ASSERT
        Assert.AreEqual(new TimeSpan(10, 30, 15), result, "Invalid value");
    }

    /// <summary>
    /// Test ExecuteScalarAsync returning a DateOnly (text on SQLite, a DateTime from a SQL Server date).
    /// </summary>
    /// <param name="databaseType">Database type.</param>
    /// <param name="sql">The SQL, which returns the value in the way the database stores it.</param>
    /// <returns>A <see cref="Task"/> that represents the asynchronous operation.</returns>
    [TestMethod(DisplayName = "ExecuteScalarAsync returning a DateOnly")]
    [DataRow(DatabaseTypes.Sqlite, "SELECT '2026-11-23'", DisplayName = DatabaseTypes.Sqlite)]
    [DataRow(DatabaseTypes.SqlServer, "SELECT CAST('2026-11-23' AS date)", DisplayName = DatabaseTypes.SqlServer)]
    public async Task ExecuteScalarTests_016_Async(string databaseType, string sql)
    {
        // ARRANGE
        using Context context = await DatabaseUtils.OpenDatabaseAsync(this._testContext, databaseType, createNew: false).ConfigureAwait(false);

        // ACT
        DateOnly result = await context.Database.ExecuteScalarAsync<DateOnly>(sql, this._testContext.CancellationToken).ConfigureAwait(false);

        // ASSERT
        Assert.AreEqual(new DateOnly(2026, 11, 23), result, "Invalid value");
    }

    /// <summary>
    /// Test ExecuteScalarAsync returning a TimeOnly (text on SQLite, a TimeSpan from a SQL Server time).
    /// </summary>
    /// <param name="databaseType">Database type.</param>
    /// <param name="sql">The SQL, which returns the value in the way the database stores it.</param>
    /// <returns>A <see cref="Task"/> that represents the asynchronous operation.</returns>
    [TestMethod(DisplayName = "ExecuteScalarAsync returning a TimeOnly")]
    [DataRow(DatabaseTypes.Sqlite, "SELECT '10:30:15.5000000'", DisplayName = DatabaseTypes.Sqlite)]
    [DataRow(DatabaseTypes.SqlServer, "SELECT CAST('10:30:15.5' AS time)", DisplayName = DatabaseTypes.SqlServer)]
    public async Task ExecuteScalarTests_017_Async(string databaseType, string sql)
    {
        // ARRANGE
        using Context context = await DatabaseUtils.OpenDatabaseAsync(this._testContext, databaseType, createNew: false).ConfigureAwait(false);

        // ACT
        TimeOnly result = await context.Database.ExecuteScalarAsync<TimeOnly>(sql, this._testContext.CancellationToken).ConfigureAwait(false);

        // ASSERT
        Assert.AreEqual(new TimeOnly(10, 30, 15, 500), result, "Invalid value");
    }

    /// <summary>
    /// Test ExecuteScalarAsync returning an enum, which EF Core maps to its number by default.
    /// </summary>
    /// <param name="databaseType">Database type.</param>
    /// <returns>A <see cref="Task"/> that represents the asynchronous operation.</returns>
    [TestMethod(DisplayName = "ExecuteScalarAsync returning an enum")]
    [DataRow(DatabaseTypes.Sqlite, DisplayName = DatabaseTypes.Sqlite)]
    [DataRow(DatabaseTypes.SqlServer, DisplayName = DatabaseTypes.SqlServer)]
    public async Task ExecuteScalarTests_018_Async(string databaseType)
    {
        // ARRANGE
        using Context context = await DatabaseUtils.OpenDatabaseAsync(this._testContext, databaseType, createNew: false).ConfigureAwait(false);

        FormattableString sql = $"SELECT 3";

        // ACT
        DayOfWeek result = await context.Database.ExecuteScalarAsync<DayOfWeek>(sql, this._testContext.CancellationToken).ConfigureAwait(false);

        // ASSERT
        Assert.AreEqual(DayOfWeek.Wednesday, result, "Invalid value");
    }

    /// <summary>
    /// Test ExecuteScalarAsync converts the result with a value converter set with the model's DefaultTypeMapping, as EF Core does when it reads a column.
    /// </summary>
    /// <param name="databaseType">Database type.</param>
    /// <returns>A <see cref="Task"/> that represents the asynchronous operation.</returns>
    [TestMethod(DisplayName = "ExecuteScalarAsync result uses the model's DefaultTypeMapping")]
    [DataRow(DatabaseTypes.Sqlite, DisplayName = DatabaseTypes.Sqlite)]
    [DataRow(DatabaseTypes.SqlServer, DisplayName = DatabaseTypes.SqlServer)]
    public async Task ExecuteScalarTests_019_Async(string databaseType)
    {
        // ARRANGE
        using Context context = await DatabaseUtils.OpenDatabaseAsync(
            this._testContext,
            databaseType,
            createNew: false,
            customConventionActions: configurationBuilder => configurationBuilder.DefaultTypeMapping<DayOfWeek>().HasConversion<string>())
            .ConfigureAwait(false);

        FormattableString sql = $"SELECT 'Wednesday'";

        // ACT
        DayOfWeek result = await context.Database.ExecuteScalarAsync<DayOfWeek>(sql, this._testContext.CancellationToken).ConfigureAwait(false);

        // ASSERT
        Assert.AreEqual(DayOfWeek.Wednesday, result, "Invalid value");
    }

    /// <summary>
    /// Test ExecuteScalarAsync returning a database NULL as a nullable type that needs converting (a Guid).
    /// </summary>
    /// <param name="databaseType">Database type.</param>
    /// <returns>A <see cref="Task"/> that represents the asynchronous operation.</returns>
    [TestMethod(DisplayName = "ExecuteScalarAsync returning a database NULL as a nullable Guid")]
    [DataRow(DatabaseTypes.Sqlite, DisplayName = DatabaseTypes.Sqlite)]
    [DataRow(DatabaseTypes.SqlServer, DisplayName = DatabaseTypes.SqlServer)]
    public async Task ExecuteScalarTests_020_Async(string databaseType)
    {
        // ARRANGE
        using Context context = await DatabaseUtils.OpenDatabaseAsync(this._testContext, databaseType, createNew: false).ConfigureAwait(false);

        FormattableString sql = $"SELECT NULL";

        // ACT
        Guid? result = await context.Database.ExecuteScalarAsync<Guid?>(sql, this._testContext.CancellationToken).ConfigureAwait(false);

        // ASSERT
        Assert.IsNull(result, "Invalid value");
    }

    #endregion ExecuteScalarAsync tests
}
