﻿// Copyright ©2021-2025 Mike King.
// This file is licensed to you under the MIT license.
// See the License.txt file in the solution root for more information.

using DotDoc.EntityFrameworkCore.Extensions.Constants;
using DotDoc.EntityFrameworkCore.Extensions.Exceptions;
using DotDoc.EntityFrameworkCore.Extensions.Extensions;
using DotDoc.EntityFrameworkCore.Extensions.Tests.Data;
using DotDoc.EntityFrameworkCore.Extensions.Tests.Utilities;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DotDoc.EntityFrameworkCore.Extensions.Tests.UniqueConstraintExtensions;

/// <summary>
/// Tests for UniqueConstraintInterceptor extensions.
/// </summary>
[TestClass]
public class UniqueConstraintInterceptorTests
{
    #region public methods

    /// <summary>
    /// Test UniqueConstraintInterceptor.
    /// </summary>
    /// <param name="databaseType">Database type.</param>
    /// <remarks>
    /// Test our interceptor captures Unique Constraint Exceptions inside EF Core.
    /// The Unique Constraint Exception Processor will see the table details are held in EF Core and
    /// convert the database table name and field names into the ones used by EF Core.
    /// </remarks>
    [TestMethod("UniqueConstraintInterceptor")]
    [DataRow(DatabaseType.Sqlite, DisplayName = DatabaseType.Sqlite)]
    [DataRow(DatabaseType.SqlServer, DisplayName = DatabaseType.SqlServer)]
    public void Test_UniqueConstraintInterceptor(string databaseType)
    {
        // ARRANGE
        using Context context = DatabaseUtils.CreateDatabase(
            databaseType,
            customConfigurationActions: (optionsBuilder) => optionsBuilder.UseUniqueConstraintInterceptor());

        string? schema = DatabaseUtils.GetDefaultSchema(databaseType);
        string value = TestUtils.GetMethodName();

        TestTable2 testTable2 = new() { TestField = value };
        context.Add(testTable2);
        context.SaveChanges();

        testTable2 = new() { TestField = value };
        context.Add(testTable2);

        // ACT / ASSERT
        UniqueConstraintException e = Assert.ThrowsExactly<UniqueConstraintException>(() => _ = context.SaveChanges(), "Unexpected exception");

        // Check the details contain the EF Core table name and field name.
        Assert.IsNotNull(e.Details, "Details are null");
        Assert.AreEqual(schema, e.Details.Schema, "Invalid schema name");
        Assert.AreEqual("TestTable2", e.Details.TableName, "Invalid table name");
        Assert.AreEqual(1, e.Details.FieldNames.Count, "Invalid field names count");
        Assert.AreEqual("TestField", e.Details.FieldNames[0], "Invalid field name");
    }

    /// <summary>
    /// Test UniqueConstraintInterceptorAsync.
    /// </summary>
    /// <param name="databaseType">Database type.</param>
    /// <returns>A task that represents the asynchronous test operation.</returns>
    /// <remarks>
    /// Test our interceptor captures Unique Constraint Exceptions inside EF Core.
    /// The Unique Constraint Exception Processor will see the table details are held in EF Core and
    /// convert the database table name and field names into the ones used by EF Core.
    /// </remarks>
    [TestMethod("UniqueConstraintInterceptorAsync")]
    [DataRow(DatabaseType.Sqlite, DisplayName = DatabaseType.Sqlite)]
    [DataRow(DatabaseType.SqlServer, DisplayName = DatabaseType.SqlServer)]
    public async Task Test_UniqueConstraintInterceptorAsync(string databaseType)
    {
        // ARRANGE
        using Context context = DatabaseUtils.CreateDatabase(
            databaseType,
            customConfigurationActions: (optionsBuilder) => optionsBuilder.UseUniqueConstraintInterceptor());

        string? schema = DatabaseUtils.GetDefaultSchema(databaseType);
        string value = TestUtils.GetMethodName();

        TestTable2 testTable2 = new() { TestField = value };
        context.Add(testTable2);
        await context.SaveChangesAsync().ConfigureAwait(false);

        testTable2 = new() { TestField = value };
        context.Add(testTable2);

        // ACT / ASSERT
        UniqueConstraintException e = await Assert.ThrowsExactlyAsync<UniqueConstraintException>(() => context.SaveChangesAsync(), "Unexpected exception").ConfigureAwait(false);

        // Check the details contain the EF Core table name and field name.
        Assert.IsNotNull(e.Details, "Details are null");
        Assert.AreEqual(schema, e.Details.Schema, "Invalid schema name");
        Assert.AreEqual("TestTable2", e.Details.TableName, "Invalid table name");
        Assert.AreEqual(1, e.Details.FieldNames.Count, "Invalid field names count");
        Assert.AreEqual("TestField", e.Details.FieldNames[0], "Invalid field name");
    }

    #endregion public methods
}
