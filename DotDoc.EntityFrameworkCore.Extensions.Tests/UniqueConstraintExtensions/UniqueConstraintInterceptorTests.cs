// Copyright ©2021-2022 Mike King.
// This file is licensed to you under the MIT license.
// See the License.txt file in the solution root for more information.

using DotDoc.EntityFrameworkCore.Extensions.Constants;
using DotDoc.EntityFrameworkCore.Extensions.Exceptions;
using DotDoc.EntityFrameworkCore.Extensions.Tests.Data;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DotDoc.EntityFrameworkCore.Extensions.Tests.QueryExtensions;

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
    /// <returns><see cref="Task"/>.</returns>
    [TestMethod]
    [DataRow(DatabaseType.Sqlite, false, DisplayName = "SQLite UniqueConstraintInterceptor.")]
    [DataRow(DatabaseType.Sqlite, true, DisplayName = "SQLite UniqueConstraintInterceptorAsync.")]
    [DataRow(DatabaseType.SqlServer, false, DisplayName = "SQL Server UniqueConstraintInterceptor.")]
    [DataRow(DatabaseType.SqlServer, true, DisplayName = "SQL Server UniqueConstraintInterceptorAsync.")]
    public async Task TestUniqueConstraintInterceptorAsync(DatabaseType databaseType, bool useAsync)
    {
        string value = DatabaseUtils.GetMethodName();

        using Context context = DatabaseUtils.CreateDatabase(databaseType, true);

        DatabaseUtils.CreateSingleTestTableEntry(context, value);

        TestTable1 testTable1 = new () { TestField = value };
        context.Add(testTable1);

        UniqueConstraintException e = useAsync
            ? await Assert.ThrowsExceptionAsync<UniqueConstraintException>(() => context.SaveChangesAsync(), "Unexpected exception").ConfigureAwait(false)
            : Assert.ThrowsException<UniqueConstraintException>(() => context.SaveChanges(), "Unexpected exception");

        Assert.IsNotNull(e.Details, "Details are null");
        Assert.AreEqual("TestTable1", e.Details.TableName, "Invalid EF table name");
        Assert.AreEqual(1, e.Details.FieldNames?.Count, "Invalid field names count");
        Assert.AreEqual("TestField", e.Details.FieldNames[0], "Invalid EF field name");
    }
}
