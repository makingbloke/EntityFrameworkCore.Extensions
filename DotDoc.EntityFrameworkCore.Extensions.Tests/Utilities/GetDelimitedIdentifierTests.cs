// Copyright ©2021-2026 Mike King.
// This file is licensed to you under the MIT license.
// See the License.txt file in the solution root for more information.

using DotDoc.EntityFrameworkCore.Extensions.DatabaseType;
using DotDoc.EntityFrameworkCore.Extensions.Tests.Data;
using DotDoc.EntityFrameworkCore.Extensions.Utilities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using System.Text;

namespace DotDoc.EntityFrameworkCore.Extensions.Tests.Utilities;

/// <summary>
/// Tests for GetDelimitedIdentifier extensions.
/// </summary>
[TestClass]
public class GetDelimitedIdentifierTests
{
    #region private fields

    /// <summary>
    /// The MSTest <see cref="TestContext"/> for the current test.
    /// </summary>
    private readonly TestContext _testContext;

    #endregion private fields

    #region public constructors

    /// <summary>
    /// Initializes a new instance of the <see cref="GetDelimitedIdentifierTests"/> class.
    /// </summary>
    /// <param name="testContext">The MSTest test context.</param>
    public GetDelimitedIdentifierTests(TestContext testContext)
    {
        this._testContext = testContext;
    }

    #endregion public constructors

    #region GetDelimitedIdentifier tests

    /// <summary>
    /// Test GetDelimitedIdentifier(name) with a Null DatabaseFacade Database parameter.
    /// </summary>
    [TestMethod]
    public void GetDelimitedIdentifier_NameWithNullDatabase_ThrowsArgumentNullException()
    {
        // ARRANGE
        DatabaseFacade database = null!;

        // ACT / ASSERT
        Assert.ThrowsExactly<ArgumentNullException>(() => database.GetDelimitedIdentifier("Name"), "Unexpected exception");
    }

    /// <summary>
    /// Test GetDelimitedIdentifier(name) with a Null, Empty or Whitespace name parameter.
    /// </summary>
    /// <param name="name">The identifier to delimit.</param>
    /// <param name="exceptionType">Type of exception raised.</param>
    /// <returns>A <see cref="Task"/> that represents the asynchronous operation.</returns>
    [TestMethod]
    [DataRow(null, typeof(ArgumentNullException), DisplayName = "NameWithInvalidName Null")]
    [DataRow("", typeof(ArgumentException), DisplayName = "NameWithInvalidName Empty")]
    [DataRow(" ", typeof(ArgumentException), DisplayName = "NameWithInvalidName Whitespace")]
    public async Task GetDelimitedIdentifier_NameWithInvalidName_ThrowsArgumentException(string name, Type exceptionType)
    {
        // ARRANGE
        using Context context = await DatabaseUtils.OpenDatabaseAsync(this._testContext, DatabaseTypes.Sqlite).ConfigureAwait(false);

        // ACT / ASSERT
        Exception e = Assert.Throws<Exception>(() => context.Database.GetDelimitedIdentifier(name), "Unexpected exception");
        Assert.IsInstanceOfType(e, exceptionType, "Unexpected exception type");
    }

    /// <summary>
    /// Test GetDelimitedIdentifier(name) generates the correctly delimited identifier.
    /// </summary>
    /// <param name="databaseType">The database type.</param>
    /// <param name="expected">The expected delimited identifier.</param>
    /// <returns>A <see cref="Task"/> that represents the asynchronous operation.</returns>
    [TestMethod]
    [DataRow(DatabaseTypes.Sqlite, @"""TestTable1""", DisplayName = $"Name {DatabaseTypes.Sqlite}")]
    [DataRow(DatabaseTypes.SqlServer, "[TestTable1]", DisplayName = $"Name {DatabaseTypes.SqlServer}")]
    public async Task GetDelimitedIdentifier_Name_ReturnsDelimitedIdentifier(string databaseType, string expected)
    {
        // ARRANGE
        using Context context = await DatabaseUtils.OpenDatabaseAsync(this._testContext, databaseType).ConfigureAwait(false);
        string name = "TestTable1";

        // ACT
        string result = context.Database.GetDelimitedIdentifier(name);

        // ASSERT
        Assert.AreEqual(expected, result, "Unexpected delimited identifier");
    }

    /// <summary>
    /// Test GetDelimitedIdentifier(name) escapes provider specific special characters within the identifier.
    /// </summary>
    /// <param name="databaseType">The database type.</param>
    /// <param name="expected">The expected delimited identifier.</param>
    /// <returns>A <see cref="Task"/> that represents the asynchronous operation.</returns>
    [TestMethod]
    [DataRow(DatabaseTypes.Sqlite, @"""Foo""""Bar]Baz""", DisplayName = $"NameWithSpecialCharacters {DatabaseTypes.Sqlite}")]
    [DataRow(DatabaseTypes.SqlServer, @"[Foo""Bar]]Baz]", DisplayName = $"NameWithSpecialCharacters {DatabaseTypes.SqlServer}")]
    public async Task GetDelimitedIdentifier_NameWithSpecialCharacters_EscapesThem(string databaseType, string expected)
    {
        // ARRANGE
        using Context context = await DatabaseUtils.OpenDatabaseAsync(this._testContext, databaseType).ConfigureAwait(false);
        string name = @"Foo""Bar]Baz";

        // ACT
        string result = context.Database.GetDelimitedIdentifier(name);

        // ASSERT
        Assert.AreEqual(expected, result, "Unexpected delimited identifier");
    }

    /// <summary>
    /// Test GetDelimitedIdentifier(name, schema) with a Null DatabaseFacade Database parameter.
    /// </summary>
    [TestMethod]
    public void GetDelimitedIdentifier_NameSchemaWithNullDatabase_ThrowsArgumentNullException()
    {
        // ARRANGE
        DatabaseFacade database = null!;

        // ACT / ASSERT
        Assert.ThrowsExactly<ArgumentNullException>(() => database.GetDelimitedIdentifier("Name", "Schema"), "Unexpected exception");
    }

    /// <summary>
    /// Test GetDelimitedIdentifier(name, schema) with a Null, Empty or Whitespace name parameter.
    /// </summary>
    /// <param name="name">The identifier to delimit.</param>
    /// <param name="exceptionType">Type of exception raised.</param>
    /// <returns>A <see cref="Task"/> that represents the asynchronous operation.</returns>
    [TestMethod]
    [DataRow(null, typeof(ArgumentNullException), DisplayName = "NameSchemaWithInvalidName Null")]
    [DataRow("", typeof(ArgumentException), DisplayName = "NameSchemaWithInvalidName Empty")]
    [DataRow(" ", typeof(ArgumentException), DisplayName = "NameSchemaWithInvalidName Whitespace")]
    public async Task GetDelimitedIdentifier_NameSchemaWithInvalidName_ThrowsArgumentException(string name, Type exceptionType)
    {
        // ARRANGE
        using Context context = await DatabaseUtils.OpenDatabaseAsync(this._testContext, DatabaseTypes.Sqlite).ConfigureAwait(false);

        // ACT / ASSERT
        Exception e = Assert.Throws<Exception>(() => context.Database.GetDelimitedIdentifier(name, "Schema"), "Unexpected exception");
        Assert.IsInstanceOfType(e, exceptionType, "Unexpected exception type");
    }

    /// <summary>
    /// Test GetDelimitedIdentifier(name, schema) generates the correctly delimited identifier including the schema.
    /// </summary>
    /// <param name="databaseType">The database type.</param>
    /// <param name="expected">The expected delimited identifier (SQLite ignores schema argument).</param>
    /// <returns>A <see cref="Task"/> that represents the asynchronous operation.</returns>
    [TestMethod]
    [DataRow(DatabaseTypes.Sqlite, @"""TestTable1""", DisplayName = $"NameSchema {DatabaseTypes.Sqlite}")]
    [DataRow(DatabaseTypes.SqlServer, "[TestSchema].[TestTable1]", DisplayName = $"NameSchema {DatabaseTypes.SqlServer}")]
    public async Task GetDelimitedIdentifier_NameSchema_ReturnsDelimitedIdentifierWithSchema(string databaseType, string expected)
    {
        // ARRANGE
        using Context context = await DatabaseUtils.OpenDatabaseAsync(this._testContext, databaseType).ConfigureAwait(false);
        string name = "TestTable1";
        string schema = "TestSchema";

        // ACT
        string result = context.Database.GetDelimitedIdentifier(name, schema);

        // ASSERT
        Assert.AreEqual(expected, result, "Unexpected delimited identifier");
    }

    /// <summary>
    /// Test GetDelimitedIdentifier(name, schema) ignores a Null, Empty or Whitespace schema parameter.
    /// </summary>
    /// <param name="databaseType">The database type.</param>
    /// <param name="schema">The schema of the identifier.</param>
    /// <returns>A <see cref="Task"/> that represents the asynchronous operation.</returns>
    [TestMethod]
    [DataRow(DatabaseTypes.Sqlite, null, DisplayName = "NameSchemaWithNoSchema Sqlite - Null")]
    [DataRow(DatabaseTypes.Sqlite, "", DisplayName = "NameSchemaWithNoSchema Sqlite - Empty")]
    [DataRow(DatabaseTypes.Sqlite, " ", DisplayName = "NameSchemaWithNoSchema Sqlite - Whitespace")]
    [DataRow(DatabaseTypes.SqlServer, null, DisplayName = "NameSchemaWithNoSchema SqlServer - Null")]
    [DataRow(DatabaseTypes.SqlServer, "", DisplayName = "NameSchemaWithNoSchema SqlServer - Empty")]
    [DataRow(DatabaseTypes.SqlServer, " ", DisplayName = "NameSchemaWithNoSchema SqlServer - Whitespace")]
    public async Task GetDelimitedIdentifier_NameSchemaWithNoSchema_IgnoresSchema(string databaseType, string schema)
    {
        // ARRANGE
        using Context context = await DatabaseUtils.OpenDatabaseAsync(this._testContext, databaseType).ConfigureAwait(false);
        string name = "TestTable1";

        string expected = context.Database.GetDelimitedIdentifier(name);

        // ACT
        string result = context.Database.GetDelimitedIdentifier(name, schema);

        // ASSERT
        Assert.AreEqual(expected, result, "Unexpected delimited identifier");
    }

    /// <summary>
    /// Test GetDelimitedIdentifier(builder, name) with a Null DatabaseFacade Database parameter.
    /// </summary>
    [TestMethod]
    public void GetDelimitedIdentifier_BuilderNameWithNullDatabase_ThrowsArgumentNullException()
    {
        // ARRANGE
        DatabaseFacade database = null!;
        StringBuilder builder = new();

        // ACT / ASSERT
        Assert.ThrowsExactly<ArgumentNullException>(() => database.GetDelimitedIdentifier(builder, "Name"), "Unexpected exception");
    }

    /// <summary>
    /// Test GetDelimitedIdentifier(builder, name) with a Null StringBuilder builder parameter.
    /// </summary>
    /// <returns>A <see cref="Task"/> that represents the asynchronous operation.</returns>
    [TestMethod]
    public async Task GetDelimitedIdentifier_BuilderNameWithNullBuilder_ThrowsArgumentNullException()
    {
        // ARRANGE
        using Context context = await DatabaseUtils.OpenDatabaseAsync(this._testContext, DatabaseTypes.Sqlite).ConfigureAwait(false);
        StringBuilder builder = null!;

        // ACT / ASSERT
        Assert.ThrowsExactly<ArgumentNullException>(() => context.Database.GetDelimitedIdentifier(builder, "Name"), "Unexpected exception");
    }

    /// <summary>
    /// Test GetDelimitedIdentifier(builder, name) with a Null, Empty or Whitespace name parameter.
    /// </summary>
    /// <param name="name">The identifier to delimit.</param>
    /// <param name="exceptionType">Type of exception raised.</param>
    /// <returns>A <see cref="Task"/> that represents the asynchronous operation.</returns>
    [TestMethod]
    [DataRow(null, typeof(ArgumentNullException), DisplayName = "BuilderNameWithInvalidName Null")]
    [DataRow("", typeof(ArgumentException), DisplayName = "BuilderNameWithInvalidName Empty")]
    [DataRow(" ", typeof(ArgumentException), DisplayName = "BuilderNameWithInvalidName Whitespace")]
    public async Task GetDelimitedIdentifier_BuilderNameWithInvalidName_ThrowsArgumentException(string name, Type exceptionType)
    {
        // ARRANGE
        using Context context = await DatabaseUtils.OpenDatabaseAsync(this._testContext, DatabaseTypes.Sqlite).ConfigureAwait(false);
        StringBuilder builder = new();

        // ACT / ASSERT
        Exception e = Assert.Throws<Exception>(() => context.Database.GetDelimitedIdentifier(builder, name), "Unexpected exception");
        Assert.IsInstanceOfType(e, exceptionType, "Unexpected exception type");
    }

    /// <summary>
    /// Test GetDelimitedIdentifier(builder, name) appends the correctly delimited identifier to any existing content.
    /// </summary>
    /// <param name="databaseType">The database type.</param>
    /// <returns>A <see cref="Task"/> that represents the asynchronous operation.</returns>
    [TestMethod]
    [DataRow(DatabaseTypes.Sqlite, DisplayName = $"BuilderName {DatabaseTypes.Sqlite}")]
    [DataRow(DatabaseTypes.SqlServer, DisplayName = $"BuilderName {DatabaseTypes.SqlServer}")]
    public async Task GetDelimitedIdentifier_BuilderName_AppendsDelimitedIdentifier(string databaseType)
    {
        // ARRANGE
        using Context context = await DatabaseUtils.OpenDatabaseAsync(this._testContext, databaseType).ConfigureAwait(false);
        string name = "TestTable1";

        string expected = $"Prefix{context.Database.GetDelimitedIdentifier(name)}";
        StringBuilder builder = new("Prefix");

        // ACT
        context.Database.GetDelimitedIdentifier(builder, name);

        // ASSERT
        Assert.AreEqual(expected, builder.ToString(), "Unexpected delimited identifier");
    }

    /// <summary>
    /// Test GetDelimitedIdentifier(builder, name, schema) with a Null DatabaseFacade Database parameter.
    /// </summary>
    [TestMethod]
    public void GetDelimitedIdentifier_BuilderNameSchemaWithNullDatabase_ThrowsArgumentNullException()
    {
        // ARRANGE
        DatabaseFacade database = null!;
        StringBuilder builder = new();

        // ACT / ASSERT
        Assert.ThrowsExactly<ArgumentNullException>(() => database.GetDelimitedIdentifier(builder, "Name", "Schema"), "Unexpected exception");
    }

    /// <summary>
    /// Test GetDelimitedIdentifier(builder, name, schema) with a Null StringBuilder builder parameter.
    /// </summary>
    /// <returns>A <see cref="Task"/> that represents the asynchronous operation.</returns>
    [TestMethod]
    public async Task GetDelimitedIdentifier_BuilderNameSchemaWithNullBuilder_ThrowsArgumentNullException()
    {
        // ARRANGE
        using Context context = await DatabaseUtils.OpenDatabaseAsync(this._testContext, DatabaseTypes.Sqlite).ConfigureAwait(false);
        StringBuilder builder = null!;

        // ACT / ASSERT
        Assert.ThrowsExactly<ArgumentNullException>(() => context.Database.GetDelimitedIdentifier(builder, "Name", "Schema"), "Unexpected exception");
    }

    /// <summary>
    /// Test GetDelimitedIdentifier(builder, name, schema) with a Null, Empty or Whitespace name parameter.
    /// </summary>
    /// <param name="name">The identifier to delimit.</param>
    /// <param name="exceptionType">Type of exception raised.</param>
    /// <returns>A <see cref="Task"/> that represents the asynchronous operation.</returns>
    [TestMethod]
    [DataRow(null, typeof(ArgumentNullException), DisplayName = "BuilderNameSchemaWithInvalidName Null")]
    [DataRow("", typeof(ArgumentException), DisplayName = "BuilderNameSchemaWithInvalidName Empty")]
    [DataRow(" ", typeof(ArgumentException), DisplayName = "BuilderNameSchemaWithInvalidName Whitespace")]
    public async Task GetDelimitedIdentifier_BuilderNameSchemaWithInvalidName_ThrowsArgumentException(string name, Type exceptionType)
    {
        // ARRANGE
        using Context context = await DatabaseUtils.OpenDatabaseAsync(this._testContext, DatabaseTypes.Sqlite).ConfigureAwait(false);
        StringBuilder builder = new();

        // ACT / ASSERT
        Exception e = Assert.Throws<Exception>(() => context.Database.GetDelimitedIdentifier(builder, name, "Schema"), "Unexpected exception");
        Assert.IsInstanceOfType(e, exceptionType, "Unexpected exception type");
    }

    /// <summary>
    /// Test GetDelimitedIdentifier(builder, name, schema) appends the correctly delimited identifier including the schema to any existing content.
    /// </summary>
    /// <param name="databaseType">The database type.</param>
    /// <returns>A <see cref="Task"/> that represents the asynchronous operation.</returns>
    [TestMethod]
    [DataRow(DatabaseTypes.Sqlite, DisplayName = $"BuilderNameSchema {DatabaseTypes.Sqlite}")]
    [DataRow(DatabaseTypes.SqlServer, DisplayName = $"BuilderNameSchema {DatabaseTypes.SqlServer}")]
    public async Task GetDelimitedIdentifier_BuilderNameSchema_AppendsDelimitedIdentifierWithSchema(string databaseType)
    {
        // ARRANGE
        using Context context = await DatabaseUtils.OpenDatabaseAsync(this._testContext, databaseType).ConfigureAwait(false);
        string name = "TestTable1";
        string schema = "TestSchema";

        string expected = $"Prefix{context.Database.GetDelimitedIdentifier(name, schema)}";
        StringBuilder builder = new("Prefix");

        // ACT
        context.Database.GetDelimitedIdentifier(builder, name, schema);

        // ASSERT
        Assert.AreEqual(expected, builder.ToString(), "Unexpected delimited identifier");
    }

    /// <summary>
    /// Test GetDelimitedIdentifier(builder, name, schema) ignores a Null, Empty or Whitespace schema parameter.
    /// </summary>
    /// <param name="databaseType">The database type.</param>
    /// <param name="schema">The schema of the identifier.</param>
    /// <returns>A <see cref="Task"/> that represents the asynchronous operation.</returns>
    [TestMethod]
    [DataRow(DatabaseTypes.Sqlite, null, DisplayName = "BuilderNameSchemaWithNoSchema Sqlite - Null")]
    [DataRow(DatabaseTypes.Sqlite, "", DisplayName = "BuilderNameSchemaWithNoSchema Sqlite - Empty")]
    [DataRow(DatabaseTypes.Sqlite, " ", DisplayName = "BuilderNameSchemaWithNoSchema Sqlite - Whitespace")]
    [DataRow(DatabaseTypes.SqlServer, null, DisplayName = "BuilderNameSchemaWithNoSchema SqlServer - Null")]
    [DataRow(DatabaseTypes.SqlServer, "", DisplayName = "BuilderNameSchemaWithNoSchema SqlServer - Empty")]
    [DataRow(DatabaseTypes.SqlServer, " ", DisplayName = "BuilderNameSchemaWithNoSchema SqlServer - Whitespace")]
    public async Task GetDelimitedIdentifier_BuilderNameSchemaWithNoSchema_IgnoresSchema(string databaseType, string schema)
    {
        // ARRANGE
        using Context context = await DatabaseUtils.OpenDatabaseAsync(this._testContext, databaseType).ConfigureAwait(false);
        string name = "TestTable1";

        StringBuilder expectedBuilder = new("Prefix");
        context.Database.GetDelimitedIdentifier(expectedBuilder, name);
        string expected = expectedBuilder.ToString();

        StringBuilder builder = new("Prefix");

        // ACT
        context.Database.GetDelimitedIdentifier(builder, name, schema);

        // ASSERT
        Assert.AreEqual(expected, builder.ToString(), "Unexpected delimited identifier");
    }

    /// <summary>
    /// Test GetDelimitedIdentifier with a database provider that is not relational.
    /// </summary>
    /// <param name="overload">The overload to call.</param>
    /// <remarks>Uses the EF Core in-memory provider, which is not relational. No database is opened.</remarks>
    [TestMethod]
    [DataRow("name", DisplayName = "NonRelationalProvider GetDelimitedIdentifier(name)")]
    [DataRow("name, schema", DisplayName = "NonRelationalProvider GetDelimitedIdentifier(name, schema)")]
    [DataRow("builder, name", DisplayName = "NonRelationalProvider GetDelimitedIdentifier(builder, name)")]
    [DataRow("builder, name, schema", DisplayName = "NonRelationalProvider GetDelimitedIdentifier(builder, name, schema)")]
    public void GetDelimitedIdentifier_NonRelationalProvider_ThrowsArgumentException(string overload)
    {
        // ARRANGE
        DbContextOptions options = new DbContextOptionsBuilder()
            .UseInMemoryDatabase(nameof(this.GetDelimitedIdentifier_NonRelationalProvider_ThrowsArgumentException))
            .Options;

        using DbContext context = new(options);
        StringBuilder builder = new();

        Action action = overload switch
        {
            "name" => () => context.Database.GetDelimitedIdentifier("Name"),
            "name, schema" => () => context.Database.GetDelimitedIdentifier("Name", "Schema"),
            "builder, name" => () => context.Database.GetDelimitedIdentifier(builder, "Name"),
            "builder, name, schema" => () => context.Database.GetDelimitedIdentifier(builder, "Name", "Schema"),
            _ => throw new ArgumentException($"Unknown overload {overload}", nameof(overload))
        };

        // ACT / ASSERT
        ArgumentException e = Assert.ThrowsExactly<ArgumentException>(action, "Unexpected exception");
        Assert.AreEqual("database", e.ParamName, "Unexpected parameter name");
    }

    #endregion GetDelimitedIdentifier tests
}
