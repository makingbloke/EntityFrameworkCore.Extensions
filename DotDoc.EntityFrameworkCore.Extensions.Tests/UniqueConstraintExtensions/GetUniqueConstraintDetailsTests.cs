// Copyright ©2021-2022 Mike King.
// This file is licensed to you under the MIT license.
// See the License.txt file in the solution root for more information.

// ADD tests for aliases [TableName] and [ColumnName]
using DotDoc.EntityFrameworkCore.Extensions.Constants;
using DotDoc.EntityFrameworkCore.Extensions.Extensions;
using DotDoc.EntityFrameworkCore.Extensions.Model;
using DotDoc.EntityFrameworkCore.Extensions.Tests.Data;
using EntityFrameworkTest.UniqueConstraint.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DotDoc.EntityFrameworkCore.Extensions.Tests.QueryExtensions;

/// <summary>
/// Tests for GetUniqueConstraintDetails extensions.
/// </summary>
[TestClass]
public class GetUniqueConstraintDetailsTests
{
    /// <summary>
    /// Test GetUniqueConstraintDetails from an exception wrapped in a DbUpdateException.
    /// </summary>
    /// <param name="databaseType">Database type.</param>
    /// <param name="useAsync">If <see langword="true"/> then tests the async method.</param>
    /// <returns><see cref="Task"/>.</returns>
    [TestMethod]
    [DataRow(DatabaseType.Sqlite, false, DisplayName = "SQLite GetUniqueConstraintDetails from exception wrapped in DbUpdateException.")]
    [DataRow(DatabaseType.Sqlite, true, DisplayName = "SQLite GetUniqueConstraintDetailsAsync from exception wrapped in DbUpdateException.")]
    [DataRow(DatabaseType.SqlServer, false, DisplayName = "SQL Server GetUniqueConstraintDetails from exception wrapped in DbUpdateException.")]
    [DataRow(DatabaseType.SqlServer, true, DisplayName = "SQL Server GetUniqueConstraintDetailsAsync from exception wrapped in DbUpdateException.")]
    public async Task TestGetUniqueConstraintDetailsWrappedInDbUpdateExceptionAsync(DatabaseType databaseType, bool useAsync)
    {
        string value = DatabaseUtils.GetMethodName();

        using Context context = DatabaseUtils.CreateDatabase(databaseType);

        DatabaseUtils.CreateSingleTestTableEntry(context, value);

        TestTable1 testTable1 = new () { TestField = value };
        context.Add(testTable1);

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

        Assert.IsNotNull(details, $"Details are null. Exception: {saveException}");
        Assert.AreEqual("TestTable1", details.TableName, "Invalid EF table name");
        Assert.AreEqual(1, details.FieldNames?.Count, "Invalid field names count");
        Assert.AreEqual("TestField", details.FieldNames[0], "Invalid EF field name");
    }

    /// <summary>
    /// Test GetUniqueConstraintDetails from a database exception.
    /// </summary>
    /// <param name="databaseType">Database type.</param>
    /// <param name="useAsync">If <see langword="true"/> then tests the async method.</param>
    /// <returns><see cref="Task"/>.</returns>
    [TestMethod]
    [DataRow(DatabaseType.Sqlite, false, DisplayName = "SQLite GetUniqueConstraintDetails from a database exception.")]
    [DataRow(DatabaseType.Sqlite, true, DisplayName = "SQLite GetUniqueConstraintDetailsAsync from a database exception.")]
    [DataRow(DatabaseType.SqlServer, false, DisplayName = "SQL Server GetUniqueConstraintDetails from a database exception.")]
    [DataRow(DatabaseType.SqlServer, true, DisplayName = "SQL Server GetUniqueConstraintDetailsAsync from a database exception.")]
    public async Task TestGetUniqueConstraintDetailsFromDatabaseExceptionAsync(DatabaseType databaseType, bool useAsync)
    {
        string value = DatabaseUtils.GetMethodName();

        using Context context = DatabaseUtils.CreateDatabase(databaseType);

        DatabaseUtils.CreateSingleTestTableEntry(context, value);

        Exception saveException = null;

        try
        {
            context.Database.ExecuteInsertInterpolated($"INSERT INTO TestTable1 (TestField) VALUES ({value})");
        }
        catch (Exception e)
        {
            saveException = e;
        }

        Assert.IsNotInstanceOfType(saveException, typeof(DbUpdateException), "Invalid exception type.");

        UniqueConstraintDetails details = useAsync
                ? await context.GetUniqueConstraintDetailsAsync(saveException).ConfigureAwait(false)
                : context.GetUniqueConstraintDetails(saveException);

        Assert.IsNotNull(details, $"Details are null. Exception: {saveException}");
        Assert.AreEqual("TestTable1", details.TableName, "Invalid EF table name");
        Assert.AreEqual(1, details.FieldNames?.Count, "Invalid field names count");
        Assert.AreEqual("TestField", details.FieldNames[0], "Invalid EF field name");
    }
}