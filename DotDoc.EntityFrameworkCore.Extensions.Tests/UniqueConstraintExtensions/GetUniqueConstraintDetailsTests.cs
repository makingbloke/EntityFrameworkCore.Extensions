// Copyright ©2021-2022 Mike King.
// This file is licensed to you under the MIT license.
// See the License.txt file in the solution root for more information.

using DotDoc.EntityFrameworkCore.Extensions.Constants;
using DotDoc.EntityFrameworkCore.Extensions.Extensions;
using DotDoc.EntityFrameworkCore.Extensions.Model;
using DotDoc.EntityFrameworkCore.Extensions.Tests.Data;
using EntityFrameworkTest.UniqueConstraint.Extensions;
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
    /// <param name="useAsync">If <see langword="true"/> then tests the async method.</param>
    /// <returns><see cref="Task"/>.</returns>
    /// <remarks>
    /// Use EF to perform the insert so the exception raised is wrapped inside a DbUpdateException.
    /// The Unique Constraint Handler will see the table details are held in EF Core and convert the
    /// database table name and field names into the ones used by EF Core.
    /// </remarks>
    [TestMethod]
    [DataRow(DatabaseType.Sqlite, false, DisplayName = "SQLite GetUniqueConstraintDetails with EF Core.")]
    [DataRow(DatabaseType.Sqlite, true, DisplayName = "SQLite GetUniqueConstraintDetailsAsync with EF Core.")]
    [DataRow(DatabaseType.SqlServer, false, DisplayName = "SQL Server GetUniqueConstraintDetails with EF Core.")]
    [DataRow(DatabaseType.SqlServer, true, DisplayName = "SQL Server GetUniqueConstraintDetailsAsync with EF Core.")]
    public async Task TestGetUniqueConstraintDetailsWithEfCoreAsync(DatabaseType databaseType, bool useAsync)
    {
        string value = DatabaseUtils.GetMethodName();

        using Context context = DatabaseUtils.CreateDatabase(databaseType);

        TestTable2 testTable2 = new () { TestField = value };
        context.Add(testTable2);
        context.SaveChanges();

        testTable2 = new () { TestField = value };
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

        Assert.IsInstanceOfType(saveException, typeof(DbUpdateException), "Invalid exception type.");

        UniqueConstraintDetails details = useAsync
                ? await context.GetUniqueConstraintDetailsAsync(saveException).ConfigureAwait(false)
                : context.GetUniqueConstraintDetails(saveException);

        // Test it retrieves the table name and field name used by EF Core.
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
    /// <param name="useAsync">If <see langword="true"/> then tests the async method.</param>
    /// <returns><see cref="Task"/>.</returns>
    /// <remarks>
    /// Use SQL rather than EF to perform the insert so the exception raised comes from the database,
    /// not wrapped in a DbUpdateException. The Unique Constraint Handler will see the table details
    /// are held in EF Core and convert the database table name and field names into the ones used by EF Core.
    /// </remarks>
    [TestMethod]
    [DataRow(DatabaseType.Sqlite, false, DisplayName = "SQLite GetUniqueConstraintDetails with EF Core and SQL.")]
    [DataRow(DatabaseType.Sqlite, true, DisplayName = "SQLite GetUniqueConstraintDetailsAsync with EF Core and SQL.")]
    [DataRow(DatabaseType.SqlServer, false, DisplayName = "SQL Server GetUniqueConstraintDetails with EF Core and SQL.")]
    [DataRow(DatabaseType.SqlServer, true, DisplayName = "SQL Server GetUniqueConstraintDetailsAsync with EF Core and SQL.")]
    public async Task TestGetUniqueConstraintDetailsWithEfCoreAndSqlAsync(DatabaseType databaseType, bool useAsync)
    {
        string value = DatabaseUtils.GetMethodName();

        using Context context = DatabaseUtils.CreateDatabase(databaseType);

        FormattableString sql = $"INSERT INTO TestTable2RealName (TestFieldRealName) VALUES ({value})";
        context.Database.ExecuteInsertInterpolated(sql);

        Exception saveException = null;

        try
        {
            context.Database.ExecuteInsertInterpolated(sql);
        }
        catch (Exception e)
        {
            saveException = e;
        }

        Assert.IsNotInstanceOfType(saveException, typeof(DbUpdateException), "Invalid exception type.");

        UniqueConstraintDetails details = useAsync
                ? await context.GetUniqueConstraintDetailsAsync(saveException).ConfigureAwait(false)
                : context.GetUniqueConstraintDetails(saveException);

        // Test it retrieves the table name and field name used by EF Core.
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
    /// <param name="useAsync">If <see langword="true"/> then tests the async method.</param>
    /// <returns><see cref="Task"/>.</returns>
    /// Use SQL to create the test table and perform the insert so the exception raised comes from the
    /// database, not wrapped in a DbUpdateException. The Unique Constraint Handler will see the table
    /// details are not in EF Core and should return the database table and field names.
    [TestMethod]
    [DataRow(DatabaseType.Sqlite, false, DisplayName = "SQLite GetUniqueConstraintDetails with SQL.")]
    [DataRow(DatabaseType.Sqlite, true, DisplayName = "SQLite GetUniqueConstraintDetailsAsync with SQL.")]
    [DataRow(DatabaseType.SqlServer, false, DisplayName = "SQL Server GetUniqueConstraintDetails with SQL.")]
    [DataRow(DatabaseType.SqlServer, true, DisplayName = "SQL Server GetUniqueConstraintDetailsAsync with SQL.")]
    public async Task TestGetUniqueConstraintDetailsFromSqlTableAsync(DatabaseType databaseType, bool useAsync)
    {
        string value = DatabaseUtils.GetMethodName();

        using Context context = DatabaseUtils.CreateDatabase(databaseType);

        switch (databaseType)
        {
            case DatabaseType.Sqlite:
                context.Database.ExecuteNonQueryRaw(
@"CREATE TABLE [TestTable3] (
    [Id] INTEGER NOT NULL CONSTRAINT [PK_TestTable3] PRIMARY KEY AUTOINCREMENT,
    [TestField] TEXT NOT NULL
);");

                context.Database.ExecuteNonQueryRaw(
@"CREATE UNIQUE INDEX [IX_TestTable3_TestField] ON [TestTable3] ([TestField]);");
                break;

            case DatabaseType.SqlServer:
                context.Database.ExecuteNonQueryRaw(
@"CREATE TABLE [TestTable3] (
    [Id] bigint NOT NULL IDENTITY,
    [TestField] nvarchar(256) NOT NULL,
    CONSTRAINT [PK_TestTable3] PRIMARY KEY ([Id])
);");

                context.Database.ExecuteNonQueryRaw(
@"CREATE UNIQUE INDEX [IX_TestTable3_TestField] ON [TestTable3] ([TestField]);");
                break;

            default:
                throw new InvalidOperationException("Unsupported database provider");
        }

        FormattableString sql = $"INSERT INTO TestTable3 (TestField) VALUES ({value})";
        context.Database.ExecuteInsertInterpolated(sql);

        Exception saveException = null;

        try
        {
            context.Database.ExecuteInsertInterpolated(sql);
        }
        catch (Exception e)
        {
            saveException = e;
        }

        Assert.IsNotInstanceOfType(saveException, typeof(DbUpdateException), "Invalid exception type.");

        UniqueConstraintDetails details = useAsync
                ? await context.GetUniqueConstraintDetailsAsync(saveException).ConfigureAwait(false)
                : context.GetUniqueConstraintDetails(saveException);

        // Test it retrieves the table name and field name used by EF Core.
        Assert.IsNotNull(details, "Details are null");
        Assert.IsNull(details.Schema, "Invalid schema name");
        Assert.AreEqual("TestTable3", details.TableName, "Invalid EF table name");
        Assert.AreEqual(1, details.FieldNames?.Count, "Invalid field names count");
        Assert.AreEqual("TestField", details.FieldNames[0], "Invalid EF field name");
    }
}