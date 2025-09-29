// Copyright ©2021-2025 Mike King.
// This file is licensed to you under the MIT license.
// See the License.txt file in the solution root for more information.

using DotDoc.EntityFrameworkCore.Extensions.DatabaseType;
using DotDoc.EntityFrameworkCore.Extensions.Execute;
using DotDoc.EntityFrameworkCore.Extensions.Tests.Data;
using DotDoc.EntityFrameworkCore.Extensions.UniqueConstraint;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace DotDoc.EntityFrameworkCore.Extensions.Tests.UniqueConstraint;

/// <summary>
/// Tests for GetUniqueConstraintDetails extensions.
/// </summary>
[TestClass]
public class GetUniqueConstraintDetailsTests
{
    #region public methods

    /// <summary>
    /// Test GetUniqueConstraintDetailsAsync with Null DatabaseFacade Database parameter.
    /// </summary>
    /// <returns>A <see cref="Task"/> that represents the asynchronous operation.</returns>
    [TestMethod(DisplayName = "GetUniqueConstraintDetailsAsync with Null DatabaseFacade Database parameter")]
    public async Task GetUniqueConstraintDetailsAsyncTests_001_Async()
    {
        // ARRANGE
        DatabaseFacade database = null!;
        Exception e = new InvalidOperationException("Test Exception");

        // ACT / ASSERT
        await Assert.ThrowsExactlyAsync<ArgumentNullException>(() => database.GetUniqueConstraintDetailsAsync(e, CancellationToken.None), "Unexpected exception").ConfigureAwait(false);
    }

    /// <summary>
    /// Test GetUniqueConstraintDetailsAsync with Null Exception E parameter.
    /// </summary>
    /// <returns>A <see cref="Task"/> that represents the asynchronous operation.</returns>
    [TestMethod(DisplayName = "GetUniqueConstraintDetailsAsync with Null Exception E parameter.")]
    public async Task GetUniqueConstraintDetailsAsyncTests_002_Async()
    {
        // ARRANGE
        using Context context = await DatabaseUtils.CreateDatabaseAsync(DatabaseTypes.Sqlite).ConfigureAwait(false);
        Exception e = null!;

        // ACT / ASSERT
        await Assert.ThrowsExactlyAsync<ArgumentNullException>(() => context.Database.GetUniqueConstraintDetailsAsync(e, CancellationToken.None), "Unexpected exception").ConfigureAwait(false);
    }

    /// <summary>
    /// Test GetUniqueConstraintDetailsAsync with EF Core.
    /// </summary>
    /// <param name="databaseType">Database type.</param>
    /// <returns>A <see cref="Task"/> that represents the asynchronous operation.</returns>
    /// <remarks>
    /// Use EF to perform the insert so the exception raised is wrapped inside a DbUpdateException.
    /// The Unique Constraint Exception Processor will see the table details are held in EF Core and
    /// convert the database table name and field names into the ones used by EF Core.
    /// </remarks>
    [TestMethod(DisplayName = "GetUniqueConstraintDetailsAsync with EF Core")]
    [DataRow(DatabaseTypes.Sqlite, DisplayName = DatabaseTypes.Sqlite)]
    [DataRow(DatabaseTypes.SqlServer, DisplayName = DatabaseTypes.SqlServer)]
    public async Task GetUniqueConstraintDetailsAsyncTests_003_Async(string databaseType)
    {
        // ARRANGE
        using Context context = await DatabaseUtils.CreateDatabaseAsync(databaseType).ConfigureAwait(false);

        string? schema = context.DefaultSchema;
        string tableName = nameof(TestTable2);
        string fieldName = nameof(TestTable2.TestField);

        string value = "TestValue";

        TestTable2 testTable2 = new() { TestField = value };
        context.Add(testTable2);
        await context.SaveChangesAsync(CancellationToken.None).ConfigureAwait(false);

        testTable2 = new() { TestField = value };
        context.Add(testTable2);

        // ACT / ASSERT
        DbUpdateException e = await Assert.ThrowsExactlyAsync<DbUpdateException>(() => context.SaveChangesAsync(CancellationToken.None), "Unexpected exception").ConfigureAwait(false);
        UniqueConstraintDetails? details = await context.Database.GetUniqueConstraintDetailsAsync(e, CancellationToken.None).ConfigureAwait(false);

        // Check the details contain the EF Core table name and field name.
        Assert.IsNotNull(details, "Details are null");
        Assert.AreEqual(schema, details.Schema, "Invalid schema name");
        Assert.AreEqual(tableName, details.TableName, "Invalid table name");
        Assert.HasCount(1, details.FieldNames, "Invalid field names count");
        Assert.AreEqual(fieldName, details.FieldNames[0], "Invalid field name");
    }

    /// <summary>
    /// Test GetUniqueConstraintDetailsAsync with EF Core and SQL.
    /// </summary>
    /// <param name="databaseType">Database type.</param>
    /// <returns>A <see cref="Task"/> that represents the asynchronous operation.</returns>
    /// <remarks>
    /// Use SQL rather than EF to perform the insert so the exception raised comes from the database,
    /// not wrapped in a DbUpdateException. The Unique Constraint Exception Processor will see the table details
    /// are held in EF Core and convert the database table name and field names into the ones used by EF Core.
    /// </remarks>
    [TestMethod(DisplayName = "GetUniqueConstraintDetailsAsync with EF Core and SQL")]
    [DataRow(DatabaseTypes.Sqlite, DisplayName = DatabaseTypes.Sqlite)]
    [DataRow(DatabaseTypes.SqlServer, DisplayName = DatabaseTypes.SqlServer)]
    public async Task GetUniqueConstraintDetailsAsyncTests_004_Async(string databaseType)
    {
        // ARRANGE
        using Context context = await DatabaseUtils.CreateDatabaseAsync(databaseType).ConfigureAwait(false);

        string? schema = context.DefaultSchema;
        string tableName = nameof(TestTable2);
        string fieldName = nameof(TestTable2.TestField);

        string value = "TestValue";

        FormattableString sql = $"INSERT INTO TestTable2RealName (TestFieldRealName) VALUES ({value})";
        await context.Database.ExecuteInsertAsync<long>(sql, CancellationToken.None).ConfigureAwait(false);

        // ACT / ASSERT
        Exception e = await Assert.ThrowsAsync<Exception>(() => context.Database.ExecuteInsertAsync<long>(sql, CancellationToken.None), "Unexpected exception").ConfigureAwait(false);
        UniqueConstraintDetails? details = await context.Database.GetUniqueConstraintDetailsAsync(e, CancellationToken.None).ConfigureAwait(false);

        // Check the details contain the EF Core table name and field name.
        Assert.IsNotNull(details, "Details are null");
        Assert.AreEqual(schema, details.Schema, "Invalid schema name");
        Assert.AreEqual(tableName, details.TableName, "Invalid table name");
        Assert.HasCount(1, details.FieldNames, "Invalid field names count");
        Assert.AreEqual(fieldName, details.FieldNames[0], "Invalid field name");
    }

    /// <summary>
    /// Test GetUniqueConstraintDetailsAsync with SQL only.
    /// </summary>
    /// <param name="databaseType">Database type.</param>
    /// <returns>A <see cref="Task"/> that represents the asynchronous operation.</returns>
    /// <remarks>
    /// Use SQL to create the test table and perform the insert so the exception raised comes from the
    /// database, not wrapped in a DbUpdateException. The Unique Constraint Exception Processor will see
    /// the table details are not in EF Core and should return the database table and field names.
    /// </remarks>
    [TestMethod(DisplayName = "GetUniqueConstraintDetailsAsync with SQL only")]
    [DataRow(DatabaseTypes.Sqlite, DisplayName = DatabaseTypes.Sqlite)]
    [DataRow(DatabaseTypes.SqlServer, DisplayName = DatabaseTypes.SqlServer)]
    public async Task GetUniqueConstraintDetailsAsyncTests_005_Async(string databaseType)
    {
        // ARRANGE
        using Context context = await DatabaseUtils.CreateDatabaseAsync(databaseType).ConfigureAwait(false);

        string? schema = null;
        string tableName = "TestTable3";
        string fieldName = "TestField";

        string value = "TestValue";

        switch (databaseType)
        {
            case DatabaseTypes.Sqlite:
                await context.Database.ExecuteNonQueryAsync(
@"CREATE TABLE [TestTable3] (
    [Id] INTEGER NOT NULL CONSTRAINT [PK_TestTable3] PRIMARY KEY AUTOINCREMENT,
    [TestField] TEXT NOT NULL
);", CancellationToken.None)
                    .ConfigureAwait(false);

                await context.Database.ExecuteNonQueryAsync(
@"CREATE UNIQUE INDEX [IX_TestTable3_TestField] ON [TestTable3] ([TestField]);", CancellationToken.None)
                    .ConfigureAwait(false);
                break;

            case DatabaseTypes.SqlServer:
                await context.Database.ExecuteNonQueryAsync(
@"CREATE TABLE [TestTable3] (
    [Id] bigint NOT NULL IDENTITY,
    [TestField] nvarchar(256) NOT NULL,
    CONSTRAINT [PK_TestTable3] PRIMARY KEY ([Id])
);", CancellationToken.None)
                    .ConfigureAwait(false);

                await context.Database.ExecuteNonQueryAsync(
@"CREATE UNIQUE INDEX [IX_TestTable3_TestField] ON [TestTable3] ([TestField]);", CancellationToken.None)
                    .ConfigureAwait(false);
                break;

            default:
                throw new UnsupportedDatabaseTypeException();
        }

        FormattableString sql = $"INSERT INTO TestTable3 (TestField) VALUES ({value})";
        await context.Database.ExecuteInsertAsync<long>(sql, CancellationToken.None).ConfigureAwait(false);

        // ACT / ASSERT
        Exception e = await Assert.ThrowsAsync<Exception>(() => context.Database.ExecuteInsertAsync<long>(sql, CancellationToken.None), "Unexpected exception").ConfigureAwait(false);
        UniqueConstraintDetails? details = await context.Database.GetUniqueConstraintDetailsAsync(e, CancellationToken.None).ConfigureAwait(false);

        // Check the details contain the database table name and field name.
        Assert.IsNotNull(details, "Details are null");
        Assert.AreEqual(schema, details.Schema, "Invalid schema name");
        Assert.AreEqual(tableName, details.TableName, "Invalid EF table name");
        Assert.HasCount(1, details.FieldNames, "Invalid field names count");
        Assert.AreEqual(fieldName, details.FieldNames[0], "Invalid EF field name");
    }

    #endregion public methods
}