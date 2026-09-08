// Copyright ©2021-2026 Mike King.
// This file is licensed to you under the MIT license.
// See the License.txt file in the solution root for more information.

using DotDoc.EntityFrameworkCore.Extensions.DatabaseType;
using DotDoc.EntityFrameworkCore.Extensions.Tests.Data;
using DotDoc.EntityFrameworkCore.Extensions.Utilities;
using Microsoft.EntityFrameworkCore.Infrastructure;
using System.Text;

namespace DotDoc.EntityFrameworkCore.Extensions.Tests.Utilities;

/// <summary>
/// Tests for GetDelimitedIdentifier extensions.
/// </summary>
[TestClass]
public class GetDelimitedIdentifierTests
{
    #region public methods

    /// <summary>
    /// Test GetDelimitedIdentifier(name) with a Null DatabaseFacade Database parameter.
    /// </summary>
    [TestMethod(DisplayName = "GetDelimitedIdentifier(name) with a Null DatabaseFacade Database parameter")]
    public void GetDelimitedIdentifierTests_001()
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
    [TestMethod(DisplayName = "GetDelimitedIdentifier(name) with a Null, Empty or Whitespace name parameter")]
    [DataRow(null, typeof(ArgumentNullException), DisplayName = "Null")]
    [DataRow("", typeof(ArgumentException), DisplayName = "Empty")]
    [DataRow(" ", typeof(ArgumentException), DisplayName = "Whitespace")]
    public async Task GetDelimitedIdentifierTests_002_Async(string name, Type exceptionType)
    {
        // ARRANGE
        using Context context = await DatabaseUtils.OpenDatabaseAsync(DatabaseTypes.Sqlite).ConfigureAwait(false);

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
    [TestMethod(DisplayName = "GetDelimitedIdentifier(name) generates the correctly delimited identifier")]
    [DataRow(DatabaseTypes.Sqlite, @"""TestTable1""", DisplayName = DatabaseTypes.Sqlite)]
    [DataRow(DatabaseTypes.SqlServer, "[TestTable1]", DisplayName = DatabaseTypes.SqlServer)]
    public async Task GetDelimitedIdentifierTests_003_Async(string databaseType, string expected)
    {
        // ARRANGE
        using Context context = await DatabaseUtils.OpenDatabaseAsync(databaseType).ConfigureAwait(false);
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
    [TestMethod(DisplayName = "GetDelimitedIdentifier(name) escapes provider specific special characters within the identifier")]
    [DataRow(DatabaseTypes.Sqlite, @"""Foo""""Bar]Baz""", DisplayName = DatabaseTypes.Sqlite)]
    [DataRow(DatabaseTypes.SqlServer, @"[Foo""Bar]]Baz]", DisplayName = DatabaseTypes.SqlServer)]
    public async Task GetDelimitedIdentifierTests_004_Async(string databaseType, string expected)
    {
        // ARRANGE
        using Context context = await DatabaseUtils.OpenDatabaseAsync(databaseType).ConfigureAwait(false);
        string name = @"Foo""Bar]Baz";

        // ACT
        string result = context.Database.GetDelimitedIdentifier(name);

        // ASSERT
        Assert.AreEqual(expected, result, "Unexpected delimited identifier");
    }

    /// <summary>
    /// Test GetDelimitedIdentifier(name, schema) with a Null DatabaseFacade Database parameter.
    /// </summary>
    [TestMethod(DisplayName = "GetDelimitedIdentifier(name, schema) with a Null DatabaseFacade Database parameter")]
    public void GetDelimitedIdentifierTests_005()
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
    [TestMethod(DisplayName = "GetDelimitedIdentifier(name, schema) with a Null, Empty or Whitespace name parameter")]
    [DataRow(null, typeof(ArgumentNullException), DisplayName = "Null")]
    [DataRow("", typeof(ArgumentException), DisplayName = "Empty")]
    [DataRow(" ", typeof(ArgumentException), DisplayName = "Whitespace")]
    public async Task GetDelimitedIdentifierTests_006_Async(string name, Type exceptionType)
    {
        // ARRANGE
        using Context context = await DatabaseUtils.OpenDatabaseAsync(DatabaseTypes.Sqlite).ConfigureAwait(false);

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
    [TestMethod(DisplayName = "GetDelimitedIdentifier(name, schema) generates the correctly delimited identifier including the schema")]
    [DataRow(DatabaseTypes.Sqlite, @"""TestTable1""", DisplayName = DatabaseTypes.Sqlite)]
    [DataRow(DatabaseTypes.SqlServer, "[TestSchema].[TestTable1]", DisplayName = DatabaseTypes.SqlServer)]
    public async Task GetDelimitedIdentifierTests_007_Async(string databaseType, string expected)
    {
        // ARRANGE
        using Context context = await DatabaseUtils.OpenDatabaseAsync(databaseType).ConfigureAwait(false);
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
    [TestMethod(DisplayName = "GetDelimitedIdentifier(name, schema) ignores a Null, Empty or Whitespace schema parameter")]
    [DataRow(DatabaseTypes.Sqlite, null, DisplayName = "Sqlite - Null")]
    [DataRow(DatabaseTypes.Sqlite, "", DisplayName = "Sqlite - Empty")]
    [DataRow(DatabaseTypes.Sqlite, " ", DisplayName = "Sqlite - Whitespace")]
    [DataRow(DatabaseTypes.SqlServer, null, DisplayName = "SqlServer - Null")]
    [DataRow(DatabaseTypes.SqlServer, "", DisplayName = "SqlServer - Empty")]
    [DataRow(DatabaseTypes.SqlServer, " ", DisplayName = "SqlServer - Whitespace")]
    public async Task GetDelimitedIdentifierTests_008_Async(string databaseType, string schema)
    {
        // ARRANGE
        using Context context = await DatabaseUtils.OpenDatabaseAsync(databaseType).ConfigureAwait(false);
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
    [TestMethod(DisplayName = "GetDelimitedIdentifier(builder, name) with a Null DatabaseFacade Database parameter")]
    public void GetDelimitedIdentifierTests_009()
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
    [TestMethod(DisplayName = "GetDelimitedIdentifier(builder, name) with a Null StringBuilder builder parameter")]
    public async Task GetDelimitedIdentifierTests_010_Async()
    {
        // ARRANGE
        using Context context = await DatabaseUtils.OpenDatabaseAsync(DatabaseTypes.Sqlite).ConfigureAwait(false);
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
    [TestMethod(DisplayName = "GetDelimitedIdentifier(builder, name) with a Null, Empty or Whitespace name parameter")]
    [DataRow(null, typeof(ArgumentNullException), DisplayName = "Null")]
    [DataRow("", typeof(ArgumentException), DisplayName = "Empty")]
    [DataRow(" ", typeof(ArgumentException), DisplayName = "Whitespace")]
    public async Task GetDelimitedIdentifierTests_011_Async(string name, Type exceptionType)
    {
        // ARRANGE
        using Context context = await DatabaseUtils.OpenDatabaseAsync(DatabaseTypes.Sqlite).ConfigureAwait(false);
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
    [TestMethod(DisplayName = "GetDelimitedIdentifier(builder, name) appends the correctly delimited identifier to any existing content")]
    [DataRow(DatabaseTypes.Sqlite, DisplayName = DatabaseTypes.Sqlite)]
    [DataRow(DatabaseTypes.SqlServer, DisplayName = DatabaseTypes.SqlServer)]
    public async Task GetDelimitedIdentifierTests_012_Async(string databaseType)
    {
        // ARRANGE
        using Context context = await DatabaseUtils.OpenDatabaseAsync(databaseType).ConfigureAwait(false);
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
    [TestMethod(DisplayName = "GetDelimitedIdentifier(builder, name, schema) with a Null DatabaseFacade Database parameter")]
    public void GetDelimitedIdentifierTests_013()
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
    [TestMethod(DisplayName = "GetDelimitedIdentifier(builder, name, schema) with a Null StringBuilder builder parameter")]
    public async Task GetDelimitedIdentifierTests_014_Async()
    {
        // ARRANGE
        using Context context = await DatabaseUtils.OpenDatabaseAsync(DatabaseTypes.Sqlite).ConfigureAwait(false);
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
    [TestMethod(DisplayName = "GetDelimitedIdentifier(builder, name, schema) with a Null, Empty or Whitespace name parameter")]
    [DataRow(null, typeof(ArgumentNullException), DisplayName = "Null")]
    [DataRow("", typeof(ArgumentException), DisplayName = "Empty")]
    [DataRow(" ", typeof(ArgumentException), DisplayName = "Whitespace")]
    public async Task GetDelimitedIdentifierTests_015_Async(string name, Type exceptionType)
    {
        // ARRANGE
        using Context context = await DatabaseUtils.OpenDatabaseAsync(DatabaseTypes.Sqlite).ConfigureAwait(false);
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
    [TestMethod(DisplayName = "GetDelimitedIdentifier(builder, name, schema) appends the correctly delimited identifier including the schema to any existing content")]
    [DataRow(DatabaseTypes.Sqlite, DisplayName = DatabaseTypes.Sqlite)]
    [DataRow(DatabaseTypes.SqlServer, DisplayName = DatabaseTypes.SqlServer)]
    public async Task GetDelimitedIdentifierTests_016_Async(string databaseType)
    {
        // ARRANGE
        using Context context = await DatabaseUtils.OpenDatabaseAsync(databaseType).ConfigureAwait(false);
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
    [TestMethod(DisplayName = "GetDelimitedIdentifier(builder, name, schema) ignores a Null, Empty or Whitespace schema parameter")]
    [DataRow(DatabaseTypes.Sqlite, null, DisplayName = "Sqlite - Null")]
    [DataRow(DatabaseTypes.Sqlite, "", DisplayName = "Sqlite - Empty")]
    [DataRow(DatabaseTypes.Sqlite, " ", DisplayName = "Sqlite - Whitespace")]
    [DataRow(DatabaseTypes.SqlServer, null, DisplayName = "SqlServer - Null")]
    [DataRow(DatabaseTypes.SqlServer, "", DisplayName = "SqlServer - Empty")]
    [DataRow(DatabaseTypes.SqlServer, " ", DisplayName = "SqlServer - Whitespace")]
    public async Task GetDelimitedIdentifierTests_017_Async(string databaseType, string schema)
    {
        // ARRANGE
        using Context context = await DatabaseUtils.OpenDatabaseAsync(databaseType).ConfigureAwait(false);
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

    #endregion public methods
}
