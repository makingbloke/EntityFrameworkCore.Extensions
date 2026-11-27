// Copyright ©2021-2026 Mike King.
// This file is licensed to you under the MIT license.
// See the License.txt file in the solution root for more information.

using DotDoc.EntityFrameworkCore.Extensions.CustomQueryGenerators;
using DotDoc.EntityFrameworkCore.Extensions.DatabaseType;
using DotDoc.EntityFrameworkCore.Extensions.Tests.Data;
using DotDoc.EntityFrameworkCore.Extensions.Tests.ExecuteUpdate;
using DotDoc.EntityFrameworkCore.Extensions.Tests.Model;
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
    [TestMethod]
    public void GetEntityType_NullTableExpression_ThrowsArgumentNullException()
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
    [TestMethod]
    [DataRow(DatabaseTypes.Sqlite, "Data Source=:memory:", DisplayName = $"MappedTable {DatabaseTypes.Sqlite}")]
    [DataRow(DatabaseTypes.SqlServer, "Server=localhost;Initial Catalog=Dummy", DisplayName = $"MappedTable {DatabaseTypes.SqlServer}")]
    public void GetEntityType_MappedTable_ReturnsEntityType(string databaseType, string connectionString)
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

    /// <summary>
    /// Test GetEntityType with an entity type name returns that entity type from the ones the table is mapped to.
    /// </summary>
    /// <param name="databaseType">Database type.</param>
    /// <param name="connectionString">A connection string for the database type (no connection is opened).</param>
    /// <remarks>Every entity type in a table-per-hierarchy inheritance hierarchy is mapped to the same table.</remarks>
    [TestMethod]
    [DataRow(DatabaseTypes.Sqlite, "Data Source=:memory:", DisplayName = $"DerivedTypeName {DatabaseTypes.Sqlite}")]
    [DataRow(DatabaseTypes.SqlServer, "Server=localhost;Initial Catalog=Dummy", DisplayName = $"DerivedTypeName {DatabaseTypes.SqlServer}")]
    public void GetEntityType_DerivedTypeName_ReturnsDerivedEntityType(string databaseType, string connectionString)
    {
        // ARRANGE
        using Context context = new(databaseType, connectionString, customModelCreationActions: ExecuteUpdateTestUtils.AddMappedEntities);

        IEntityType animalEntityType = context.Model.FindEntityType(typeof(Animal))!;
        IEntityType dogEntityType = context.Model.FindEntityType(typeof(Dog))!;
        TableExpression tableExpression = new("a", animalEntityType.GetTableMappings().First().Table);

        // ACT
        IEntityType entityType = tableExpression.GetEntityType(dogEntityType.Name);

        // ASSERT
        Assert.AreSame(dogEntityType, entityType, "Unexpected entity type");
    }

    /// <summary>
    /// Test GetEntityType with the name of an entity type that isn't mapped to the table.
    /// </summary>
    /// <param name="databaseType">Database type.</param>
    /// <param name="connectionString">A connection string for the database type (no connection is opened).</param>
    [TestMethod]
    [DataRow(DatabaseTypes.Sqlite, "Data Source=:memory:", DisplayName = $"EntityTypeNotMapped {DatabaseTypes.Sqlite}")]
    [DataRow(DatabaseTypes.SqlServer, "Server=localhost;Initial Catalog=Dummy", DisplayName = $"EntityTypeNotMapped {DatabaseTypes.SqlServer}")]
    public void GetEntityType_EntityTypeNotMapped_ThrowsInvalidOperationException(string databaseType, string connectionString)
    {
        // ARRANGE
        using Context context = new(databaseType, connectionString, customModelCreationActions: ExecuteUpdateTestUtils.AddMappedEntities);

        IEntityType testTable1EntityType = context.Model.FindEntityType(typeof(TestTable1))!;
        IEntityType dogEntityType = context.Model.FindEntityType(typeof(Dog))!;
        TableExpression tableExpression = new("t", testTable1EntityType.GetTableMappings().First().Table);

        // ACT / ASSERT
        InvalidOperationException exception = Assert.ThrowsExactly<InvalidOperationException>(() => tableExpression.GetEntityType(dogEntityType.Name), "Unexpected exception");
        Assert.AreEqual($"The entity type {dogEntityType.Name} is not mapped to the table {nameof(TestTable1)}.", exception.Message, "Unexpected exception message");
    }

    #endregion GetEntityType tests
}
