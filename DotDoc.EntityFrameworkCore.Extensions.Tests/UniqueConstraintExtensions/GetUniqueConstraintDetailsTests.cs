// Copyright ©2021-2025 Mike King.
// This file is licensed to you under the MIT license.
// See the License.txt file in the solution root for more information.

using DotDoc.EntityFrameworkCore.Extensions.Constants;
using DotDoc.EntityFrameworkCore.Extensions.Model;
using DotDoc.EntityFrameworkCore.Extensions.Tests.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DotDoc.EntityFrameworkCore.Extensions.Tests.UniqueConstraintExtensions;

/// <summary>
/// Tests for GetUniqueConstraintDetails extensions.
/// </summary>
[TestClass]
public class GetUniqueConstraintDetailsTests
{
    /// <summary>
    /// Test GetUniqueConstraintDetails with EF Core.
    /// </summary>
    /// <param name="databaseType">Database type.</param>
    /// <remarks>
    /// Use EF to perform the insert so the exception raised is wrapped inside a DbUpdateException.
    /// The Unique Constraint Exception Processor will see the table details are held in EF Core and
    /// convert the database table name and field names into the ones used by EF Core.
    /// </remarks>
    [TestMethod]
    [DataRow(DatabaseType.Sqlite, DisplayName = "SQLite GetUniqueConstraintDetails with EF Core.")]
    [DataRow(DatabaseType.SqlServer, DisplayName = "SQL Server GetUniqueConstraintDetails with EF Core.")]
    public void Test_GetUniqueConstraintDetails_EfCore(DatabaseType databaseType)
    {
        string value = DatabaseUtils.GetMethodName();

        using Context context = DatabaseUtils.CreateDatabase(databaseType);

        TestTable2 testTable2 = new() { TestField = value };
        context.Add(testTable2);
        context.SaveChanges();

        testTable2 = new() { TestField = value };
        context.Add(testTable2);

        Exception saveException = null;

        try
        {
            context.SaveChanges();
        }
        catch (Exception e)
        {
            saveException = e;
        }

        Assert.IsInstanceOfType<DbUpdateException>(saveException, "Invalid exception type.");

        UniqueConstraintDetails details = context.GetUniqueConstraintDetails(saveException);

        // Check the details contain the EF Core table name and field name.
        Assert.IsNotNull(details, "Details are null");
        Assert.IsNull(details.Schema, "Invalid schema name");
        Assert.AreEqual("TestTable2", details.TableName, "Invalid table name");
        Assert.AreEqual(1, details.FieldNames?.Count, "Invalid field names count");
        Assert.AreEqual("TestField", details.FieldNames[0], "Invalid field name");
    }

    /// <summary>
    /// Test GetUniqueConstraintDetails with EF Core.
    /// </summary>
    /// <param name="databaseType">Database type.</param>
    /// <returns>A task that represents the asynchronous test operation.</returns>
    /// <remarks>
    /// Use EF to perform the insert so the exception raised is wrapped inside a DbUpdateException.
    /// The Unique Constraint Exception Processor will see the table details are held in EF Core and
    /// convert the database table name and field names into the ones used by EF Core.
    /// </remarks>
    [TestMethod]
    [DataRow(DatabaseType.Sqlite, DisplayName = "SQLite GetUniqueConstraintDetails with EF Core.")]
    [DataRow(DatabaseType.SqlServer, DisplayName = "SQL Server GetUniqueConstraintDetails with EF Core.")]
    public async Task Test_GetUniqueConstraintDetails_EfCoreAsync(DatabaseType databaseType)
    {
        string value = DatabaseUtils.GetMethodName();

        using Context context = DatabaseUtils.CreateDatabase(databaseType);

        TestTable2 testTable2 = new() { TestField = value };
        context.Add(testTable2);
        await context.SaveChangesAsync().ConfigureAwait(false);

        testTable2 = new() { TestField = value };
        context.Add(testTable2);

        Exception saveException = null;

        try
        {
            await context.SaveChangesAsync().ConfigureAwait(false);
        }
        catch (Exception e)
        {
            saveException = e;
        }

        Assert.IsInstanceOfType<DbUpdateException>(saveException, "Invalid exception type.");

        UniqueConstraintDetails details = await context.GetUniqueConstraintDetailsAsync(saveException).ConfigureAwait(false);

        // Check the details contain the EF Core table name and field name.
        Assert.IsNotNull(details, "Details are null");
        Assert.IsNull(details.Schema, "Invalid schema name");
        Assert.AreEqual("TestTable2", details.TableName, "Invalid table name");
        Assert.AreEqual(1, details.FieldNames?.Count, "Invalid field names count");
        Assert.AreEqual("TestField", details.FieldNames[0], "Invalid field name");
    }

    /// <summary>
    /// Test GetUniqueConstraintDetails with EF Core and SQL.
    /// </summary>
    /// <param name="databaseType">Database type.</param>
    /// <remarks>
    /// Use SQL rather than EF to perform the insert so the exception raised comes from the database,
    /// not wrapped in a DbUpdateException. The Unique Constraint Exception Processor will see the table details
    /// are held in EF Core and convert the database table name and field names into the ones used by EF Core.
    /// </remarks>
    [TestMethod]
    [DataRow(DatabaseType.Sqlite, DisplayName = "SQLite GetUniqueConstraintDetails with EF Core and SQL.")]
    [DataRow(DatabaseType.SqlServer, DisplayName = "SQL Server GetUniqueConstraintDetails with EF Core and SQL.")]
    public void Test_GetUniqueConstraintDetails_EfCoreAndSql(DatabaseType databaseType)
    {
        string value = DatabaseUtils.GetMethodName();

        using Context context = DatabaseUtils.CreateDatabase(databaseType);

        FormattableString sql = $"INSERT INTO TestTable2RealName (TestFieldRealName) VALUES ({value})";
        context.Database.ExecuteInsert(sql);

        Exception saveException = null;

        try
        {
            context.Database.ExecuteInsert(sql);
        }
        catch (Exception e)
        {
            saveException = e;
        }

        Assert.IsNotInstanceOfType<DbUpdateException>(saveException, "Invalid exception type.");

        UniqueConstraintDetails details = context.GetUniqueConstraintDetails(saveException);

        // Check the details contain the EF Core table name and field name.
        Assert.IsNotNull(details, "Details are null");
        Assert.IsNull(details.Schema, "Invalid schema name");
        Assert.AreEqual("TestTable2", details.TableName, "Invalid table name");
        Assert.AreEqual(1, details.FieldNames?.Count, "Invalid field names count");
        Assert.AreEqual("TestField", details.FieldNames[0], "Invalid field name");
    }

    /// <summary>
    /// Test GetUniqueConstraintDetails with EF Core and SQL.
    /// </summary>
    /// <param name="databaseType">Database type.</param>
    /// <returns>A task that represents the asynchronous test operation.</returns>
    /// <remarks>
    /// Use SQL rather than EF to perform the insert so the exception raised comes from the database,
    /// not wrapped in a DbUpdateException. The Unique Constraint Exception Processor will see the table details
    /// are held in EF Core and convert the database table name and field names into the ones used by EF Core.
    /// </remarks>
    [TestMethod]
    [DataRow(DatabaseType.Sqlite, DisplayName = "SQLite GetUniqueConstraintDetails with EF Core and SQL.")]
    [DataRow(DatabaseType.SqlServer, DisplayName = "SQL Server GetUniqueConstraintDetails with EF Core and SQL.")]
    public async Task Test_GetUniqueConstraintDetails_EfCoreAndSqlAsync(DatabaseType databaseType)
    {
        string value = DatabaseUtils.GetMethodName();

        using Context context = DatabaseUtils.CreateDatabase(databaseType);

        FormattableString sql = $"INSERT INTO TestTable2RealName (TestFieldRealName) VALUES ({value})";
        await context.Database.ExecuteInsertAsync(sql).ConfigureAwait(false);

        Exception saveException = null;

        try
        {
            await context.Database.ExecuteInsertAsync(sql).ConfigureAwait(false);
        }
        catch (Exception e)
        {
            saveException = e;
        }

        Assert.IsNotInstanceOfType<DbUpdateException>(saveException, "Invalid exception type.");

        UniqueConstraintDetails details = await context.GetUniqueConstraintDetailsAsync(saveException).ConfigureAwait(false);

        // Check the details contain the EF Core table name and field name.
        Assert.IsNotNull(details, "Details are null");
        Assert.IsNull(details.Schema, "Invalid schema name");
        Assert.AreEqual("TestTable2", details.TableName, "Invalid table name");
        Assert.AreEqual(1, details.FieldNames?.Count, "Invalid field names count");
        Assert.AreEqual("TestField", details.FieldNames[0], "Invalid field name");
    }

    /// <summary>
    /// Test GetUniqueConstraintDetails with SQL.
    /// </summary>
    /// <param name="databaseType">Database type.</param>
    /// <remarks>
    /// Use SQL to create the test table and perform the insert so the exception raised comes from the
    /// database, not wrapped in a DbUpdateException. The Unique Constraint Exception Processor will see
    /// the table details are not in EF Core and should return the database table and field names.
    /// </remarks>
    [TestMethod]
    [DataRow(DatabaseType.Sqlite, DisplayName = "SQLite GetUniqueConstraintDetails with SQL.")]
    [DataRow(DatabaseType.SqlServer, DisplayName = "SQL Server GetUniqueConstraintDetails with SQL.")]
    public void Test_GetUniqueConstraintDetails_SqlTable(DatabaseType databaseType)
    {
        string value = DatabaseUtils.GetMethodName();

        using Context context = DatabaseUtils.CreateDatabase(databaseType);

        switch (databaseType)
        {
            case DatabaseType.Sqlite:
                context.Database.ExecuteNonQuery(
@"CREATE TABLE [TestTable3] (
    [Id] INTEGER NOT NULL CONSTRAINT [PK_TestTable3] PRIMARY KEY AUTOINCREMENT,
    [TestField] TEXT NOT NULL
);");

                context.Database.ExecuteNonQuery(
@"CREATE UNIQUE INDEX [IX_TestTable3_TestField] ON [TestTable3] ([TestField]);");
                break;

            case DatabaseType.SqlServer:
                context.Database.ExecuteNonQuery(
@"CREATE TABLE [TestTable3] (
    [Id] bigint NOT NULL IDENTITY,
    [TestField] nvarchar(256) NOT NULL,
    CONSTRAINT [PK_TestTable3] PRIMARY KEY ([Id])
);");

                context.Database.ExecuteNonQuery(
@"CREATE UNIQUE INDEX [IX_TestTable3_TestField] ON [TestTable3] ([TestField]);");
                break;

            default:
                throw new InvalidOperationException("Unsupported database provider");
        }

        FormattableString sql = $"INSERT INTO TestTable3 (TestField) VALUES ({value})";
        context.Database.ExecuteInsert(sql);

        Exception saveException = null;

        try
        {
            context.Database.ExecuteInsert(sql);
        }
        catch (Exception e)
        {
            saveException = e;
        }

        Assert.IsNotInstanceOfType<DbUpdateException>(saveException, "Invalid exception type.");

        UniqueConstraintDetails details = context.GetUniqueConstraintDetails(saveException);

        // Check the details contain the database table name and field name.
        Assert.IsNotNull(details, "Details are null");
        Assert.IsNull(details.Schema, "Invalid schema name");
        Assert.AreEqual("TestTable3", details.TableName, "Invalid EF table name");
        Assert.AreEqual(1, details.FieldNames?.Count, "Invalid field names count");
        Assert.AreEqual("TestField", details.FieldNames[0], "Invalid EF field name");
    }

    /// <summary>
    /// Test GetUniqueConstraintDetails with SQL.
    /// </summary>
    /// <param name="databaseType">Database type.</param>
    /// <returns>A task that represents the asynchronous test operation.</returns>
    /// <remarks>
    /// Use SQL to create the test table and perform the insert so the exception raised comes from the
    /// database, not wrapped in a DbUpdateException. The Unique Constraint Exception Processor will see
    /// the table details are not in EF Core and should return the database table and field names.
    /// </remarks>
    [TestMethod]
    [DataRow(DatabaseType.Sqlite, DisplayName = "SQLite GetUniqueConstraintDetails with SQL.")]
    [DataRow(DatabaseType.SqlServer, DisplayName = "SQL Server GetUniqueConstraintDetails with SQL.")]
    public async Task Test_GetUniqueConstraintDetails_SqlTableAsync(DatabaseType databaseType)
    {
        string value = DatabaseUtils.GetMethodName();

        using Context context = DatabaseUtils.CreateDatabase(databaseType);

        switch (databaseType)
        {
            case DatabaseType.Sqlite:
                await context.Database.ExecuteNonQueryAsync(
@"CREATE TABLE [TestTable3] (
    [Id] INTEGER NOT NULL CONSTRAINT [PK_TestTable3] PRIMARY KEY AUTOINCREMENT,
    [TestField] TEXT NOT NULL
);")
                    .ConfigureAwait(false);

                await context.Database.ExecuteNonQueryAsync(
@"CREATE UNIQUE INDEX [IX_TestTable3_TestField] ON [TestTable3] ([TestField]);")
                    .ConfigureAwait(false);
                break;

            case DatabaseType.SqlServer:
                await context.Database.ExecuteNonQueryAsync(
@"CREATE TABLE [TestTable3] (
    [Id] bigint NOT NULL IDENTITY,
    [TestField] nvarchar(256) NOT NULL,
    CONSTRAINT [PK_TestTable3] PRIMARY KEY ([Id])
);")
                    .ConfigureAwait(false);

                await context.Database.ExecuteNonQueryAsync(
@"CREATE UNIQUE INDEX [IX_TestTable3_TestField] ON [TestTable3] ([TestField]);")
                    .ConfigureAwait(false);
                break;

            default:
                throw new InvalidOperationException("Unsupported database provider");
        }

        FormattableString sql = $"INSERT INTO TestTable3 (TestField) VALUES ({value})";
        await context.Database.ExecuteInsertAsync(sql).ConfigureAwait(false);

        Exception saveException = null;

        try
        {
            await context.Database.ExecuteInsertAsync(sql).ConfigureAwait(false);
        }
        catch (Exception e)
        {
            saveException = e;
        }

        Assert.IsNotInstanceOfType<DbUpdateException>(saveException, "Invalid exception type.");

        UniqueConstraintDetails details = await context.GetUniqueConstraintDetailsAsync(saveException).ConfigureAwait(false);

        // Check the details contain the database table name and field name.
        Assert.IsNotNull(details, "Details are null");
        Assert.IsNull(details.Schema, "Invalid schema name");
        Assert.AreEqual("TestTable3", details.TableName, "Invalid EF table name");
        Assert.AreEqual(1, details.FieldNames?.Count, "Invalid field names count");
        Assert.AreEqual("TestField", details.FieldNames[0], "Invalid EF field name");
    }
}