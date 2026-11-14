// Copyright ©2021-2026 Mike King.
// This file is licensed to you under the MIT license.
// See the License.txt file in the solution root for more information.

using DotDoc.EntityFrameworkCore.Extensions.DatabaseType;
using DotDoc.EntityFrameworkCore.Extensions.Execute;
using DotDoc.EntityFrameworkCore.Extensions.Tests.Data;
using DotDoc.EntityFrameworkCore.Extensions.UniqueConstraint;
using Microsoft.EntityFrameworkCore;

namespace DotDoc.EntityFrameworkCore.Extensions.Tests.UniqueConstraint;

/// <summary>
/// Tests for the <see cref="UniqueConstraintExtensions.IsUniqueConstraintException"/> extension.
/// </summary>
[TestClass]
public class IsUniqueConstraintExceptionTests
{
    #region private fields

    /// <summary>
    /// The MSTest <see cref="TestContext"/> for the current test.
    /// </summary>
    private readonly TestContext _testContext;

    #endregion private fields

    #region public constructors

    /// <summary>
    /// Initializes a new instance of the <see cref="IsUniqueConstraintExceptionTests"/> class.
    /// </summary>
    /// <param name="testContext">The MSTest test context.</param>
    public IsUniqueConstraintExceptionTests(TestContext testContext)
    {
        this._testContext = testContext;
    }

    #endregion public constructors

    #region IsUniqueConstraintException tests

    /// <summary>
    /// Test IsUniqueConstraintException with a Null Exception Ex parameter.
    /// </summary>
    [TestMethod(DisplayName = "IsUniqueConstraintException with a Null Exception Ex parameter")]
    public void IsUniqueConstraintExceptionTests_001()
    {
        // ARRANGE
        Exception ex = null!;

        // ACT / ASSERT
        Assert.ThrowsExactly<ArgumentNullException>(() => ex.IsUniqueConstraintException(), "Unexpected exception");
    }

    /// <summary>
    /// Test IsUniqueConstraintException with an exception that did not come from the database.
    /// </summary>
    /// <remarks>
    /// The message looks like a unique constraint violation from each database, to check the result
    /// depends on the database error code and not on the message.
    /// </remarks>
    [TestMethod(DisplayName = "IsUniqueConstraintException with an exception that did not come from the database")]
    public void IsUniqueConstraintExceptionTests_002()
    {
        // ARRANGE
        Exception ex = new InvalidOperationException(
            "Cannot insert duplicate key row in object 'dbo.TestTable2RealName' with unique index 'IX_TestTable2RealName_TestFieldRealName'. "
            + "UNIQUE constraint failed: TestTable2RealName.TestFieldRealName");

        // ACT
        bool result = ex.IsUniqueConstraintException();

        // ASSERT
        Assert.IsFalse(result, "Exception is a unique constraint exception");
    }

    /// <summary>
    /// Test IsUniqueConstraintException with a unique index violation (using EF Core).
    /// </summary>
    /// <param name="databaseType">Database type.</param>
    /// <returns>A <see cref="Task"/> that represents the asynchronous operation.</returns>
    /// <remarks>
    /// EF Core wraps the database exception inside a <see cref="DbUpdateException"/>.
    /// SQL Server raises error 2601 and SQLite raises SQLITE_CONSTRAINT_UNIQUE.
    /// </remarks>
    [TestMethod(DisplayName = "IsUniqueConstraintException with a unique index violation (EF Core)")]
    [DataRow(DatabaseTypes.Sqlite, DisplayName = DatabaseTypes.Sqlite)]
    [DataRow(DatabaseTypes.SqlServer, DisplayName = DatabaseTypes.SqlServer)]
    public async Task IsUniqueConstraintExceptionTests_003_Async(string databaseType)
    {
        // ARRANGE
        using Context context = await DatabaseUtils.OpenDatabaseAsync(databaseType).ConfigureAwait(false);

        string value = "TestValue";

        context.Add(new TestTable2 { TestField = value });
        await context.SaveChangesAsync(this._testContext.CancellationToken).ConfigureAwait(false);

        context.Add(new TestTable2 { TestField = value });

        DbUpdateException ex = await Assert.ThrowsExactlyAsync<DbUpdateException>(() => context.SaveChangesAsync(this._testContext.CancellationToken), "Unexpected exception").ConfigureAwait(false);

        // ACT
        bool result = ex.IsUniqueConstraintException();

        // ASSERT
        Assert.IsTrue(result, "Exception is not a unique constraint exception");
    }

    /// <summary>
    /// Test IsUniqueConstraintException with a primary key violation (using EF Core).
    /// </summary>
    /// <param name="databaseType">Database type.</param>
    /// <returns>A <see cref="Task"/> that represents the asynchronous operation.</returns>
    /// <remarks>
    /// The change tracker is cleared after the first save, otherwise EF Core rejects the second entity
    /// itself (it is already tracking an entity with the same key) and the database is never reached.
    /// SQL Server raises error 2627 and SQLite raises SQLITE_CONSTRAINT_PRIMARYKEY.
    /// </remarks>
    [TestMethod(DisplayName = "IsUniqueConstraintException with a primary key violation (EF Core)")]
    [DataRow(DatabaseTypes.Sqlite, DisplayName = DatabaseTypes.Sqlite)]
    [DataRow(DatabaseTypes.SqlServer, DisplayName = DatabaseTypes.SqlServer)]
    public async Task IsUniqueConstraintExceptionTests_004_Async(string databaseType)
    {
        // ARRANGE
        using Context context = await DatabaseUtils.OpenDatabaseAsync(databaseType).ConfigureAwait(false);

        Guid id = Guid.NewGuid();

        context.Add(new GuidTable { Id = id });
        await context.SaveChangesAsync(this._testContext.CancellationToken).ConfigureAwait(false);

        context.ChangeTracker.Clear();
        context.Add(new GuidTable { Id = id });

        DbUpdateException ex = await Assert.ThrowsExactlyAsync<DbUpdateException>(() => context.SaveChangesAsync(this._testContext.CancellationToken), "Unexpected exception").ConfigureAwait(false);

        // ACT
        bool result = ex.IsUniqueConstraintException();

        // ASSERT
        Assert.IsTrue(result, "Exception is not a unique constraint exception");
    }

    /// <summary>
    /// Test IsUniqueConstraintException with a unique index violation (using SQL).
    /// </summary>
    /// <param name="databaseType">Database type.</param>
    /// <returns>A <see cref="Task"/> that represents the asynchronous operation.</returns>
    /// <remarks>Use SQL rather than EF Core so the exception comes from the database, not wrapped in a <see cref="DbUpdateException"/>.</remarks>
    [TestMethod(DisplayName = "IsUniqueConstraintException with a unique index violation (SQL)")]
    [DataRow(DatabaseTypes.Sqlite, DisplayName = DatabaseTypes.Sqlite)]
    [DataRow(DatabaseTypes.SqlServer, DisplayName = DatabaseTypes.SqlServer)]
    public async Task IsUniqueConstraintExceptionTests_005_Async(string databaseType)
    {
        // ARRANGE
        using Context context = await DatabaseUtils.OpenDatabaseAsync(databaseType).ConfigureAwait(false);

        string sql = "INSERT INTO TestTable2RealName (TestFieldRealName) VALUES ('TestValue')";
        await context.Database.ExecuteNonQueryAsync(sql, this._testContext.CancellationToken).ConfigureAwait(false);

        Exception ex = await Assert.ThrowsAsync<Exception>(() => context.Database.ExecuteNonQueryAsync(sql, this._testContext.CancellationToken), "Unexpected exception").ConfigureAwait(false);
        Assert.IsNotInstanceOfType<DbUpdateException>(ex, "Exception is wrapped in a DbUpdateException");

        // ACT
        bool result = ex.IsUniqueConstraintException();

        // ASSERT
        Assert.IsTrue(result, "Exception is not a unique constraint exception");
    }

    /// <summary>
    /// Test IsUniqueConstraintException with a unique constraint violation (using SQL).
    /// </summary>
    /// <param name="databaseType">Database type.</param>
    /// <returns>A <see cref="Task"/> that represents the asynchronous operation.</returns>
    /// <remarks>
    /// The table has a UNIQUE constraint rather than a unique index. SQL Server raises error 2627 for a
    /// constraint (2601 is only for a unique index) and SQLite raises SQLITE_CONSTRAINT_UNIQUE.
    /// </remarks>
    [TestMethod(DisplayName = "IsUniqueConstraintException with a unique constraint violation (SQL)")]
    [DataRow(DatabaseTypes.Sqlite, DisplayName = DatabaseTypes.Sqlite)]
    [DataRow(DatabaseTypes.SqlServer, DisplayName = DatabaseTypes.SqlServer)]
    public async Task IsUniqueConstraintExceptionTests_006_Async(string databaseType)
    {
        // ARRANGE
        using Context context = await DatabaseUtils.OpenDatabaseAsync(databaseType).ConfigureAwait(false);

        switch (databaseType)
        {
            case DatabaseTypes.Sqlite:
                await context.Database.ExecuteNonQueryAsync(
@"CREATE TABLE [TestTable3] (
    [Id] INTEGER NOT NULL CONSTRAINT [PK_TestTable3] PRIMARY KEY AUTOINCREMENT,
    [TestField] TEXT NOT NULL,
    CONSTRAINT [UQ_TestTable3_TestField] UNIQUE ([TestField])
);", this._testContext.CancellationToken)
                    .ConfigureAwait(false);
                break;

            case DatabaseTypes.SqlServer:
                await context.Database.ExecuteNonQueryAsync(
@"CREATE TABLE [TestTable3] (
    [Id] bigint NOT NULL IDENTITY,
    [TestField] nvarchar(256) NOT NULL,
    CONSTRAINT [PK_TestTable3] PRIMARY KEY ([Id]),
    CONSTRAINT [UQ_TestTable3_TestField] UNIQUE ([TestField])
);", this._testContext.CancellationToken)
                    .ConfigureAwait(false);
                break;

            default:
                throw new UnsupportedDatabaseTypeException();
        }

        string sql = "INSERT INTO TestTable3 (TestField) VALUES ('TestValue')";
        await context.Database.ExecuteNonQueryAsync(sql, this._testContext.CancellationToken).ConfigureAwait(false);

        Exception ex = await Assert.ThrowsAsync<Exception>(() => context.Database.ExecuteNonQueryAsync(sql, this._testContext.CancellationToken), "Unexpected exception").ConfigureAwait(false);

        // ACT
        bool result = ex.IsUniqueConstraintException();

        // ASSERT
        Assert.IsTrue(result, "Exception is not a unique constraint exception");
    }

    /// <summary>
    /// Test IsUniqueConstraintException with a database exception that is not a unique constraint violation (using EF Core).
    /// </summary>
    /// <param name="databaseType">Database type.</param>
    /// <returns>A <see cref="Task"/> that represents the asynchronous operation.</returns>
    /// <remarks>
    /// Inserting a NULL into a NOT NULL field raises a constraint error, but not a unique constraint error.
    /// On SQLite both have the primary error code 19 (SQLITE_CONSTRAINT), so this checks the extended error code is used.
    /// </remarks>
    [TestMethod(DisplayName = "IsUniqueConstraintException with a database exception that is not a unique constraint violation (EF Core)")]
    [DataRow(DatabaseTypes.Sqlite, DisplayName = DatabaseTypes.Sqlite)]
    [DataRow(DatabaseTypes.SqlServer, DisplayName = DatabaseTypes.SqlServer)]
    public async Task IsUniqueConstraintExceptionTests_007_Async(string databaseType)
    {
        // ARRANGE
        using Context context = await DatabaseUtils.OpenDatabaseAsync(databaseType).ConfigureAwait(false);

        context.Add(new TestTable1 { TestField = null! });

        DbUpdateException ex = await Assert.ThrowsExactlyAsync<DbUpdateException>(() => context.SaveChangesAsync(this._testContext.CancellationToken), "Unexpected exception").ConfigureAwait(false);

        // ACT
        bool result = ex.IsUniqueConstraintException();

        // ASSERT
        Assert.IsFalse(result, "Exception is a unique constraint exception");
    }

    /// <summary>
    /// Test IsUniqueConstraintException with a database exception that is not a unique constraint violation (using SQL).
    /// </summary>
    /// <param name="databaseType">Database type.</param>
    /// <returns>A <see cref="Task"/> that represents the asynchronous operation.</returns>
    /// <remarks>Inserting a NULL into a NOT NULL field raises a constraint error, but not a unique constraint error.</remarks>
    [TestMethod(DisplayName = "IsUniqueConstraintException with a database exception that is not a unique constraint violation (SQL)")]
    [DataRow(DatabaseTypes.Sqlite, DisplayName = DatabaseTypes.Sqlite)]
    [DataRow(DatabaseTypes.SqlServer, DisplayName = DatabaseTypes.SqlServer)]
    public async Task IsUniqueConstraintExceptionTests_008_Async(string databaseType)
    {
        // ARRANGE
        using Context context = await DatabaseUtils.OpenDatabaseAsync(databaseType).ConfigureAwait(false);

        string sql = "INSERT INTO TestTable1 (TestField) VALUES (NULL)";

        Exception ex = await Assert.ThrowsAsync<Exception>(() => context.Database.ExecuteNonQueryAsync(sql, this._testContext.CancellationToken), "Unexpected exception").ConfigureAwait(false);

        // ACT
        bool result = ex.IsUniqueConstraintException();

        // ASSERT
        Assert.IsFalse(result, "Exception is a unique constraint exception");
    }

    /// <summary>
    /// Test IsUniqueConstraintException with a unique index violation wrapped in other exceptions.
    /// </summary>
    /// <param name="databaseType">Database type.</param>
    /// <returns>A <see cref="Task"/> that represents the asynchronous operation.</returns>
    /// <remarks>The database exception is two levels down, to check all the inner exceptions are searched.</remarks>
    [TestMethod(DisplayName = "IsUniqueConstraintException with a unique index violation wrapped in other exceptions")]
    [DataRow(DatabaseTypes.Sqlite, DisplayName = DatabaseTypes.Sqlite)]
    [DataRow(DatabaseTypes.SqlServer, DisplayName = DatabaseTypes.SqlServer)]
    public async Task IsUniqueConstraintExceptionTests_009_Async(string databaseType)
    {
        // ARRANGE
        using Context context = await DatabaseUtils.OpenDatabaseAsync(databaseType).ConfigureAwait(false);

        string sql = "INSERT INTO TestTable2RealName (TestFieldRealName) VALUES ('TestValue')";
        await context.Database.ExecuteNonQueryAsync(sql, this._testContext.CancellationToken).ConfigureAwait(false);

        Exception databaseEx = await Assert.ThrowsAsync<Exception>(() => context.Database.ExecuteNonQueryAsync(sql, this._testContext.CancellationToken), "Unexpected exception").ConfigureAwait(false);
        Exception ex = new InvalidOperationException("Outer exception", new InvalidOperationException("Middle exception", databaseEx));

        // ACT
        bool result = ex.IsUniqueConstraintException();

        // ASSERT
        Assert.IsTrue(result, "Exception is not a unique constraint exception");
    }

    #endregion IsUniqueConstraintException tests
}
