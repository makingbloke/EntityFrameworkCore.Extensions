// Copyright ©2021-2026 Mike King.
// This file is licensed to you under the MIT license.
// See the License.txt file in the solution root for more information.

using DotDoc.EntityFrameworkCore.Extensions.DatabaseType;
using DotDoc.EntityFrameworkCore.Extensions.Execute;
using DotDoc.EntityFrameworkCore.Extensions.Tests.Data;
using DotDoc.EntityFrameworkCore.Extensions.Tests.Model;
using Microsoft.EntityFrameworkCore.Infrastructure;
using System.Data;

namespace DotDoc.EntityFrameworkCore.Extensions.Tests.Execute;

/// <summary>
/// Tests for ExecuteQuery extensions.
/// </summary>
[TestClass]
public class ExecuteQueryTests
{
    #region private fields

    /// <summary>
    /// The MSTest <see cref="TestContext"/> for the current test.
    /// </summary>
    private readonly TestContext _testContext;

    #endregion private fields

    #region public constructors

    /// <summary>
    /// Initializes a new instance of the <see cref="ExecuteQueryTests"/> class.
    /// </summary>
    /// <param name="testContext">The MSTest test context.</param>
    public ExecuteQueryTests(TestContext testContext)
    {
        this._testContext = testContext;
    }

    #endregion public constructors

    #region ExecuteQueryAsync tests

    /// <summary>
    /// Test ExecuteQueryAsync with Null DatabaseFacade Database and FormattableString Sql parameters.
    /// </summary>
    /// <returns>A <see cref="Task"/> that represents the asynchronous operation.</returns>
    [TestMethod]
    public async Task ExecuteQueryAsync_NullDatabaseWithFormattableSql_ThrowsArgumentNullException()
    {
        // ARRANGE
        DatabaseFacade database = null!;
        FormattableString sql = $"dummy";

        // ACT / ASSERT
        await Assert.ThrowsExactlyAsync<ArgumentNullException>(() => database.ExecuteQueryAsync<TestTable1>(sql, this._testContext.CancellationToken), "Unexpected exception").ConfigureAwait(false);
    }

    /// <summary>
    /// Test ExecuteQueryAsync with Null DatabaseFacade Database and String Sql parameters.
    /// </summary>
    /// <returns>A <see cref="Task"/> that represents the asynchronous operation.</returns>
    [TestMethod]
    public async Task ExecuteQueryAsync_NullDatabaseWithStringSql_ThrowsArgumentNullException()
    {
        // ARRANGE
        DatabaseFacade database = null!;
        string sql = "dummy";

        // ACT / ASSERT
        await Assert.ThrowsExactlyAsync<ArgumentNullException>(() => database.ExecuteQueryAsync<TestTable1>(sql, this._testContext.CancellationToken), "Unexpected exception").ConfigureAwait(false);
    }

    /// <summary>
    /// Test ExecuteQueryAsync with a Null FormattableString Sql parameter.
    /// </summary>
    /// <returns>A <see cref="Task"/> that represents the asynchronous operation.</returns>
    [TestMethod]
    public async Task ExecuteQueryAsync_NullFormattableSql_ThrowsArgumentNullException()
    {
        // ARRANGE
        using Context context = await DatabaseUtils.OpenDatabaseAsync(this._testContext, DatabaseTypes.Sqlite).ConfigureAwait(false);
        FormattableString sql = null!;

        // ACT / ASSERT
        await Assert.ThrowsExactlyAsync<ArgumentNullException>(() => context.Database.ExecuteQueryAsync<TestTable1>(sql, this._testContext.CancellationToken), "Unexpected exception").ConfigureAwait(false);
    }

    /// <summary>
    /// Test ExecuteQueryAsync with a Null or empty String Sql parameter.
    /// </summary>
    /// <param name="sql">The SQL string.</param>
    /// <param name="exceptionType">The type of exception raised.</param>
    /// <returns>A <see cref="Task"/> that represents the asynchronous operation.</returns>
    [TestMethod]
    [DataRow(null, typeof(ArgumentNullException), DisplayName = "NullOrEmptyStringSql Null")]
    [DataRow("", typeof(ArgumentException), DisplayName = "NullOrEmptyStringSql Empty")]
    public async Task ExecuteQueryAsync_NullOrEmptyStringSql_ThrowsArgumentException(string sql, Type exceptionType)
    {
        // ARRANGE
        using Context context = await DatabaseUtils.OpenDatabaseAsync(this._testContext, DatabaseTypes.Sqlite).ConfigureAwait(false);

        // ACT / ASSERT
        Exception e = await Assert.ThrowsAsync<Exception>(() => context.Database.ExecuteQueryAsync<TestTable1>(sql, this._testContext.CancellationToken), "Unexpected exception").ConfigureAwait(false);
        Assert.IsInstanceOfType(e, exceptionType, "Invalid exception type");
    }

    /// <summary>
    /// Test ExecuteQueryAsync with FormattableString Sql parameter.
    /// </summary>
    /// <param name="databaseType">Database type.</param>
    /// <returns>A <see cref="Task"/> that represents the asynchronous operation.</returns>
    [TestMethod]
    [DataRow(DatabaseTypes.Sqlite, DisplayName = $"FormattableSql {DatabaseTypes.Sqlite}")]
    [DataRow(DatabaseTypes.SqlServer, DisplayName = $"FormattableSql {DatabaseTypes.SqlServer}")]
    public async Task ExecuteQueryAsync_FormattableSql_ReturnsRows(string databaseType)
    {
        // ARRANGE
        using Context context = await DatabaseUtils.OpenDatabaseAsync(this._testContext, databaseType).ConfigureAwait(false);

        const int recordCount = 20;
        string value = "TestValue";

        await DatabaseUtils.CreateTestTableEntriesAsync(context, value, recordCount, this._testContext.CancellationToken).ConfigureAwait(false);

        FormattableString sql = $"SELECT * FROM TestTable1 WHERE ID <= {recordCount}";

        // ACT
        IList<TestTable1> results = await context.Database.ExecuteQueryAsync<TestTable1>(sql, this._testContext.CancellationToken).ConfigureAwait(false);

        // ASSERT
        Assert.HasCount(recordCount, results, "Invalid record count");
    }

    /// <summary>
    /// Test ExecuteQueryAsync with a String Sql parameter.
    /// </summary>
    /// <param name="databaseType">Database type.</param>
    /// <returns>A <see cref="Task"/> that represents the asynchronous operation.</returns>
    [TestMethod]
    [DataRow(DatabaseTypes.Sqlite, DisplayName = $"StringSql {DatabaseTypes.Sqlite}")]
    [DataRow(DatabaseTypes.SqlServer, DisplayName = $"StringSql {DatabaseTypes.SqlServer}")]
    public async Task ExecuteQueryAsync_StringSql_ReturnsRows(string databaseType)
    {
        // ARRANGE
        using Context context = await DatabaseUtils.OpenDatabaseAsync(this._testContext, databaseType).ConfigureAwait(false);

        const int recordCount = 20;
        string value = "TestValue";

        await DatabaseUtils.CreateTestTableEntriesAsync(context, value, recordCount, this._testContext.CancellationToken).ConfigureAwait(false);

        string sql = "SELECT * FROM TestTable1 WHERE ID <= {0}";

        // ACT
        IList<TestTable1> results = await context.Database.ExecuteQueryAsync<TestTable1>(sql, this._testContext.CancellationToken, recordCount).ConfigureAwait(false);

        // ASSERT
        Assert.HasCount(recordCount, results, "Invalid record count");
    }

    /// <summary>
    /// Test ExecuteQueryAsync with a Null IEnumerable Parameters parameter.
    /// </summary>
    /// <returns>A <see cref="Task"/> that represents the asynchronous operation.</returns>
    [TestMethod]
    public async Task ExecuteQueryAsync_NullParameters_ThrowsArgumentNullException()
    {
        // ARRANGE
        using Context context = await DatabaseUtils.OpenDatabaseAsync(this._testContext, DatabaseTypes.Sqlite).ConfigureAwait(false);

        string sql = "SELECT * FROM TestTable1";
        IEnumerable<object?> parameters = null!;

        // ACT / ASSERT
        await Assert.ThrowsExactlyAsync<ArgumentNullException>(() => context.Database.ExecuteQueryAsync<TestTable1>(sql, this._testContext.CancellationToken, parameters), "Unexpected exception").ConfigureAwait(false);
    }

    /// <summary>
    /// Test ExecuteQueryAsync returning a type that is not part of the model.
    /// </summary>
    /// <param name="databaseType">Database type.</param>
    /// <returns>A <see cref="Task"/> that represents the asynchronous operation.</returns>
    [TestMethod]
    [DataRow(DatabaseTypes.Sqlite, DisplayName = $"TypeNotInModel {DatabaseTypes.Sqlite}")]
    [DataRow(DatabaseTypes.SqlServer, DisplayName = $"TypeNotInModel {DatabaseTypes.SqlServer}")]
    public async Task ExecuteQueryAsync_TypeNotInModel_ReturnsRows(string databaseType)
    {
        // ARRANGE
        using Context context = await DatabaseUtils.OpenDatabaseAsync(this._testContext, databaseType).ConfigureAwait(false);

        string value = "B";

        await DatabaseUtils.CreateTestTableEntriesAsync(context, "A", 3, this._testContext.CancellationToken).ConfigureAwait(false);
        await DatabaseUtils.CreateTestTableEntriesAsync(context, value, 2, this._testContext.CancellationToken).ConfigureAwait(false);

        FormattableString sql = $"SELECT TestField FROM TestTable1 WHERE TestField = {value}";

        // ACT
        IList<TestFieldValue> results = await context.Database.ExecuteQueryAsync<TestFieldValue>(sql, this._testContext.CancellationToken).ConfigureAwait(false);

        // ASSERT
        Assert.HasCount(2, results, "Invalid record count");

        foreach (TestFieldValue result in results)
        {
            Assert.AreEqual(value, result.TestField, "Unexpected field value");
        }
    }

    /// <summary>
    /// Test ExecuteQueryAsync with a query that returns no records.
    /// </summary>
    /// <param name="databaseType">Database type.</param>
    /// <returns>A <see cref="Task"/> that represents the asynchronous operation.</returns>
    [TestMethod]
    [DataRow(DatabaseTypes.Sqlite, DisplayName = $"NoMatchingRows {DatabaseTypes.Sqlite}")]
    [DataRow(DatabaseTypes.SqlServer, DisplayName = $"NoMatchingRows {DatabaseTypes.SqlServer}")]
    public async Task ExecuteQueryAsync_NoMatchingRows_ReturnsEmptyList(string databaseType)
    {
        // ARRANGE
        using Context context = await DatabaseUtils.OpenDatabaseAsync(this._testContext, databaseType).ConfigureAwait(false);

        FormattableString sql = $"SELECT * FROM TestTable1";

        // ACT
        IList<TestTable1> results = await context.Database.ExecuteQueryAsync<TestTable1>(sql, this._testContext.CancellationToken).ConfigureAwait(false);

        // ASSERT
        Assert.IsEmpty(results, "Invalid record count");
    }

    #endregion ExecuteQueryAsync tests
}
