// Copyright ©2021-2022 Mike King.
// This file is licensed to you under the MIT license.
// See the License.txt file in the solution root for more information.

// ADD tests for aliases [TableName] and [ColumnName]
// ADD tests for errors not wrapped in DbUpdateException?
using DotDoc.EntityFrameworkCore.Extensions.Constants;
using DotDoc.EntityFrameworkCore.Extensions.Model;
using DotDoc.EntityFrameworkCore.Extensions.Tests.Data;
using EntityFrameworkTest.UniqueConstraint.Extensions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DotDoc.EntityFrameworkCore.Extensions.Tests.QueryExtensions;

/// <summary>
/// Tests for GetUniqueConstraintDetails extensions.
/// </summary>
[TestClass]
public class GetUniqueConstraintDetailsTests
{
    /// <summary>
    /// Test GetUniqueConstraintDetails.
    /// </summary>
    /// <param name="databaseType">Database type.</param>
    /// <param name="useAsync">If <see langword="true"/> then tests the async method.</param>
    /// <returns><see cref="Task"/>.</returns>
    [TestMethod]
    [DataRow(DatabaseType.Sqlite, false, DisplayName = "SQLite GetUniqueConstraintDetailsInterpolated.")]
    [DataRow(DatabaseType.Sqlite, true, DisplayName = "SQLite GetUniqueConstraintDetailsInterpolatedAsync.")]
    [DataRow(DatabaseType.SqlServer, false, DisplayName = "SQL Server GetUniqueConstraintDetailsInterpolated.")]
    [DataRow(DatabaseType.SqlServer, true, DisplayName = "SQL Server GetUniqueConstraintDetailsInterpolatedAsync.")]
    public async Task TestGetUniqueConstraintDetailsAsync(DatabaseType databaseType, bool useAsync)
    {
        string value = DatabaseUtils.GetMethodName();

        using Context context = DatabaseUtils.CreateDatabase(databaseType);

        DatabaseUtils.CreateSingleTestTableEntry(context, value);

        TestTable1 testTable1 = new () { TestField = value };
        context.Add(testTable1);

        Exception saveException = null;

        try
        {
            if (useAsync)
            {
                await context.SaveChangesAsync().ConfigureAwait(false);
            }
            else
            {
                context.SaveChanges();
            }
        }
        catch (Exception e)
        {
            saveException = e;
        }

        Assert.IsNotNull(saveException, "No exception raised.");

        UniqueConstraintDetails details = useAsync
                ? await context.GetUniqueConstraintDetailsAsync(saveException).ConfigureAwait(false)
                : context.GetUniqueConstraintDetails(saveException);

        Assert.IsNotNull(details, $"Details are null. Exception: {saveException}");
        Assert.AreEqual("TestTable1", details.TableName, "Invalid EF table name");
        Assert.AreEqual(1, details.FieldNames?.Count, "Invalid field names count");
        Assert.AreEqual("TestField", details.FieldNames[0], "Invalid EF field name");
    }
}