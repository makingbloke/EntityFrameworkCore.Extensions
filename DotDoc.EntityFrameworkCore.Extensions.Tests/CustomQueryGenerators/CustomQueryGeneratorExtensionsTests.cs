// Copyright ©2021-2026 Mike King.
// This file is licensed to you under the MIT license.
// See the License.txt file in the solution root for more information.

using DotDoc.EntityFrameworkCore.Extensions.CustomQueryGenerators;
using DotDoc.EntityFrameworkCore.Extensions.DatabaseType;
using DotDoc.EntityFrameworkCore.Extensions.Tests.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;

namespace DotDoc.EntityFrameworkCore.Extensions.Tests.CustomQueryGenerators;

/// <summary>
/// Tests for the <see cref="CustomQueryGeneratorExtensions.GetEntityType"/> extension.
/// </summary>
/// <remarks>
/// Only the model is built (no database connection is opened), so these tests need no database.
/// The database specific RETURNING / OUTPUT clause checks that use it are private to the query generators, and are tested
/// through the ExecuteUpdate get rows methods.
/// </remarks>
[TestClass]
public class CustomQueryGeneratorExtensionsTests
{
    #region GetEntityType tests

    /// <summary>
    /// Test GetEntityType with a Null TableExpression TableExpression parameter.
    /// </summary>
    [TestMethod(DisplayName = "GetEntityType with a Null TableExpression TableExpression parameter")]
    public void CustomQueryGeneratorExtensionsTests_001()
    {
        // ARRANGE
        TableExpression tableExpression = null!;

        // ACT / ASSERT
        Assert.ThrowsExactly<ArgumentNullException>(() => tableExpression.GetEntityType(), "Unexpected exception");
    }

    /// <summary>
    /// Test GetEntityType returns the entity type the table is mapped to.
    /// </summary>
    /// <param name="databaseType">Database type.</param>
    /// <param name="connectionString">A connection string for the database type (no connection is opened).</param>
    [TestMethod(DisplayName = "GetEntityType returns the entity type the table is mapped to")]
    [DataRow(DatabaseTypes.Sqlite, "Data Source=:memory:", DisplayName = DatabaseTypes.Sqlite)]
    [DataRow(DatabaseTypes.SqlServer, "Server=localhost;Initial Catalog=Dummy", DisplayName = DatabaseTypes.SqlServer)]
    public void CustomQueryGeneratorExtensionsTests_002(string databaseType, string connectionString)
    {
        // ARRANGE
        using Context context = new(databaseType, connectionString);

        IEntityType expectedEntityType = context.Model.FindEntityType(typeof(TestTable1))!;
        TableExpression tableExpression = new("t", expectedEntityType.GetTableMappings().First().Table);

        // ACT
        IEntityType entityType = tableExpression.GetEntityType();

        // ASSERT
        Assert.AreSame(expectedEntityType, entityType, "Unexpected entity type");
    }

    #endregion GetEntityType tests
}
