// Copyright ©2021-2023 Mike King.
// This file is licensed to you under the MIT license.
// See the License.txt file in the solution root for more information.

using DotDoc.EntityFrameworkCore.Extensions.Constants;
using DotDoc.EntityFrameworkCore.Extensions.Exceptions;
using DotDoc.EntityFrameworkCore.Extensions.Tests.Data;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DotDoc.EntityFrameworkCore.Extensions.Tests.UniqueConstraintExtensions;

/// <summary>
/// Tests for UniqueConstraintInterceptor extensions.
/// </summary>
[TestClass]
public class UniqueConstraintInterceptorTests
{
    /// <summary>
    /// Test UniqueConstraintInterceptor.
    /// </summary>
    /// <param name="databaseType">Database type.</param>
    /// <param name="useAsync">If <see langword="true"/> then tests the async method.</param>
    /// <returns>A task that represents the asynchronous test operation.</returns>
    /// <remarks>
    /// Test our interceptor captures Unique Constraint Exceptions inside EF Core.
    /// The Unique Constraint Exception Processor will see the table details are held in EF Core and
    /// convert the database table name and field names into the ones used by EF Core.
    /// </remarks>
    [TestMethod]
    [DataRow(DatabaseType.Sqlite, false, DisplayName = "SQLite UniqueConstraintInterceptor.")]
    [DataRow(DatabaseType.Sqlite, true, DisplayName = "SQLite UniqueConstraintInterceptorAsync.")]
    [DataRow(DatabaseType.SqlServer, false, DisplayName = "SQL Server UniqueConstraintInterceptor.")]
    [DataRow(DatabaseType.SqlServer, true, DisplayName = "SQL Server UniqueConstraintInterceptorAsync.")]
    public async Task TestUniqueConstraintInterceptorAsync(DatabaseType databaseType, bool useAsync)
    {
        string value = DatabaseUtils.GetMethodName();

        using Context context = DatabaseUtils.CreateDatabase(databaseType, true);

        TestTable2 testTable2 = new () { TestField = value };
        context.Add(testTable2);
        context.SaveChanges();

        testTable2 = new () { TestField = value };
        context.Add(testTable2);

        UniqueConstraintException e = useAsync
            ? await Assert.ThrowsExceptionAsync<UniqueConstraintException>(() => context.SaveChangesAsync(), "Unexpected exception").ConfigureAwait(false)
            : Assert.ThrowsException<UniqueConstraintException>(() => context.SaveChanges(), "Unexpected exception");

        // Check the details contain the EF Core table name and field name.
        Assert.IsNotNull(e.Details, "Details are null");
        Assert.IsNull(e.Details.Schema, "Invalid schema name");
        Assert.AreEqual("TestTable2", e.Details.TableName, "Invalid table name");
        Assert.AreEqual(1, e.Details.FieldNames?.Count, "Invalid field names count");
        Assert.AreEqual("TestField", e.Details.FieldNames[0], "Invalid field name");
    }
}
