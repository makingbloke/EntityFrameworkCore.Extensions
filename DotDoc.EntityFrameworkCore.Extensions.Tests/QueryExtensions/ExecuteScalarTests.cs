// Copyright ©2021-2025 Mike King.
// This file is licensed to you under the MIT license.
// See the License.txt file in the solution root for more information.

using DotDoc.EntityFrameworkCore.Extensions.Constants;
using DotDoc.EntityFrameworkCore.Extensions.Extensions;
using DotDoc.EntityFrameworkCore.Extensions.Tests.Data;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DotDoc.EntityFrameworkCore.Extensions.Tests.QueryExtensions;

/// <summary>
/// Tests for ExecuteScalar extensions.
/// </summary>
[TestClass]
public class ExecuteScalarTests
{
    #region public methods

    /// <summary>
    /// Test ExecuteScalar(DatabaseFacade, FormattableString) with a null <see cref="DatabaseFacade"/> object.
    /// </summary>
    [TestMethod]
    public void Test_ExecuteScalar_DatabaseFacade_FormattableString_NullDatabaseFacade()
    {
        // ARRANGE
        DatabaseFacade? databaseFacade = null;
        FormattableString sql = $"dummy";

        // ACT / ASSERT
        Assert.ThrowsException<ArgumentNullException>(() => databaseFacade!.ExecuteScalar<int>(sql), "Unexpected exception");
    }

    /// <summary>
    /// Test ExecuteScalar(DatabaseFacade, FormattableString) with a null <see cref="FormattableString"/> object.
    /// </summary>
    [TestMethod]
    public void Test_ExecuteScalar_DatabaseFacade_FormattableString_NullFormattableString()
    {
        // ARRANGE
        using Context context = DatabaseUtils.CreateDatabase(DatabaseType.Sqlite);

        DatabaseFacade databaseFacade = context.Database;
        FormattableString? sql = null;

        // ACT / ASSERT
        Assert.ThrowsException<ArgumentNullException>(() => databaseFacade!.ExecuteScalar<int>(sql!), "Unexpected exception");
    }

    /// <summary>
    /// Test ExecuteScalar(DatabaseFacade, string, IEnumerable) with a null <see cref="DatabaseFacade"/> object.
    /// </summary>
    [TestMethod]
    public void Test_ExecuteScalar_DatabaseFacade_String_IEnumerable_NullDatabaseFacade()
    {
        // ARRANGE
        DatabaseFacade? databaseFacade = null;
        string sql = "dummy";
        object?[] parameters = [];

        // ACT / ASSERT
        Assert.ThrowsException<ArgumentNullException>(() => databaseFacade!.ExecuteScalar<int>(sql, parameters), "Unexpected exception");
    }

    /// <summary>
    /// Test ExecuteScalar(DatabaseFacade, string, IEnumerable) with a null <see cref="string"/> object.
    /// </summary>
    [TestMethod]
    public void Test_ExecuteScalar_DatabaseFacade_String_IEnumerable_NullString()
    {
        // ARRANGE
        using Context context = DatabaseUtils.CreateDatabase(DatabaseType.Sqlite);

        DatabaseFacade databaseFacade = context.Database;
        string? sql = null;
        object?[] parameters = [];

        // ACT / ASSERT
        Assert.ThrowsException<ArgumentNullException>(() => databaseFacade.ExecuteScalar<int>(sql!, parameters), "Unexpected exception");
    }

    /// <summary>
    /// Test ExecuteScalar(DatabaseFacade, string, IEnumerable) with a null <see cref="IEnumerable{Object}"/> object.
    /// </summary>
    [TestMethod]
    public void Test_ExecuteScalar_DatabaseFacade_String_IEnumerable_NullIEnumerable()
    {
        // ARRANGE
        using Context context = DatabaseUtils.CreateDatabase(DatabaseType.Sqlite);

        DatabaseFacade databaseFacade = context.Database;
        string sql = "dummy";
        object?[]? parameters = null;

        // ACT / ASSERT
        Assert.ThrowsException<ArgumentNullException>(() => databaseFacade.ExecuteScalar<int>(sql, parameters!), "Unexpected exception");
    }

    /// <summary>
    /// Test ExecuteScalar(DatabaseFacade, FormattableString) with a null <see cref="DatabaseFacade"/> object.
    /// </summary>
    /// <returns>A task that represents the asynchronous test operation.</returns>
    [TestMethod]
    public async Task Test_ExecuteScalar_DatabaseFacade_FormattableString_NullDatabaseFacadeAsync()
    {
        // ARRANGE
        DatabaseFacade? databaseFacade = null;
        FormattableString sql = $"dummy";

        // ACT / ASSERT
        await Assert.ThrowsExceptionAsync<ArgumentNullException>(() => databaseFacade!.ExecuteScalarAsync<int>(sql), "Unexpected exception").ConfigureAwait(false);
    }

    /// <summary>
    /// Test ExecuteScalar(DatabaseFacade, FormattableString) with a null <see cref="FormattableString"/> object.
    /// </summary>
    /// <returns>A task that represents the asynchronous test operation.</returns>
    [TestMethod]
    public async Task Test_ExecuteScalar_DatabaseFacade_FormattableString_NullFormattableStringAsync()
    {
        // ARRANGE
        using Context context = DatabaseUtils.CreateDatabase(DatabaseType.Sqlite);

        DatabaseFacade databaseFacade = context.Database;
        FormattableString? sql = null;

        // ACT / ASSERT
        await Assert.ThrowsExceptionAsync<ArgumentNullException>(() => databaseFacade!.ExecuteScalarAsync<int>(sql!), "Unexpected exception").ConfigureAwait(false);
    }

    /// <summary>
    /// Test ExecuteScalar(DatabaseFacade, string, IEnumerable) with a null <see cref="DatabaseFacade"/> object.
    /// </summary>
    /// <returns>A task that represents the asynchronous test operation.</returns>
    [TestMethod]
    public async Task Test_ExecuteScalar_DatabaseFacade_String_IEnumerable_NullDatabaseFacadeAsync()
    {
        // ARRANGE
        DatabaseFacade? databaseFacade = null;
        string sql = "dummy";
        object?[] parameters = [];

        // ACT / ASSERT
        await Assert.ThrowsExceptionAsync<ArgumentNullException>(() => databaseFacade!.ExecuteScalarAsync<int>(sql, parameters: parameters), "Unexpected exception").ConfigureAwait(false);
    }

    /// <summary>
    /// Test ExecuteScalar(DatabaseFacade, string, IEnumerable) with a null <see cref="string"/> object.
    /// </summary>
    /// <returns>A task that represents the asynchronous test operation.</returns>
    [TestMethod]
    public async Task Test_ExecuteScalar_DatabaseFacade_String_IEnumerable_NullStringAsync()
    {
        // ARRANGE
        using Context context = DatabaseUtils.CreateDatabase(DatabaseType.Sqlite);

        DatabaseFacade databaseFacade = context.Database;
        string? sql = null;
        object?[] parameters = [];

        // ACT / ASSERT
        await Assert.ThrowsExceptionAsync<ArgumentNullException>(() => databaseFacade.ExecuteScalarAsync<int>(sql!, parameters: parameters), "Unexpected exception").ConfigureAwait(false);
    }

    /// <summary>
    /// Test ExecuteScalar(DatabaseFacade, string, IEnumerable) with a null <see cref="IEnumerable{Object}"/> object.
    /// </summary>
    /// <returns>A task that represents the asynchronous test operation.</returns>
    [TestMethod]
    public async Task Test_ExecuteScalar_DatabaseFacade_String_IEnumerable_NullIEnumerableAsync()
    {
        // ARRANGE
        using Context context = DatabaseUtils.CreateDatabase(DatabaseType.Sqlite);

        DatabaseFacade databaseFacade = context.Database;
        string sql = "dummy";
        object?[]? parameters = null;

        // ACT / ASSERT
        await Assert.ThrowsExceptionAsync<ArgumentNullException>(() => databaseFacade.ExecuteScalarAsync<int>(sql, parameters: parameters!), "Unexpected exception").ConfigureAwait(false);
    }

    /// <summary>
    /// Test ExecuteScalar with FormattableString parameter.
    /// </summary>
    /// <param name="databaseType">Database type.</param>
    [TestMethod]
    [DataRow(DatabaseType.Sqlite, DisplayName = "SQLite ExecuteScalar FormattableString.")]
    [DataRow(DatabaseType.SqlServer, DisplayName = "SQL Server ExecuteScalar FormattableString.")]
    public void Test_ExecuteScalar_FormattableString(string databaseType)
    {
        // ARRANGE
        using Context context = DatabaseUtils.CreateDatabase(databaseType);

        string value = DatabaseUtils.GetMethodName();
        long id = DatabaseUtils.CreateSingleTestTableEntry(context, value);
        FormattableString sql = $"SELECT COUNT(*) FROM TestTable1 WHERE ID = {id}";

        // ACT
        int count = context.Database.ExecuteScalar<int>(sql);

        // ASSERT
        Assert.AreEqual(1, count, "Invalid count");
    }

    /// <summary>
    /// Test ExecuteScalar with FormattableString parameter.
    /// </summary>
    /// <param name="databaseType">Database type.</param>
    /// <returns>A task that represents the asynchronous test operation.</returns>
    [TestMethod]
    [DataRow(DatabaseType.Sqlite, DisplayName = "SQLite ExecuteScalar FormattableString.")]
    [DataRow(DatabaseType.SqlServer, DisplayName = "SQL Server ExecuteScalar FormattableString.")]
    public async Task Test_ExecuteScalar_FormattableStringAsync(string databaseType)
    {
        using Context context = DatabaseUtils.CreateDatabase(databaseType);

        string value = DatabaseUtils.GetMethodName();
        long id = DatabaseUtils.CreateSingleTestTableEntry(context, value);
        FormattableString sql = $"SELECT COUNT(*) FROM TestTable1 WHERE ID = {id}";

        int count = await context.Database.ExecuteScalarAsync<int>(sql).ConfigureAwait(false);

        Assert.AreEqual(1, count, "Invalid count");
    }

    /// <summary>
    /// Test ExecuteScalar with params parameter.
    /// </summary>
    /// <param name="databaseType">Database type.</param>
    [TestMethod]
    [DataRow(DatabaseType.Sqlite, DisplayName = "SQLite ExecuteScalar params.")]
    [DataRow(DatabaseType.SqlServer, DisplayName = "SQL Server ExecuteScalar params.")]
    public void Test_ExecuteScalar_Params(string databaseType)
    {
        using Context context = DatabaseUtils.CreateDatabase(databaseType);

        string value = DatabaseUtils.GetMethodName();
        long id = DatabaseUtils.CreateSingleTestTableEntry(context, value);
        string sql = "SELECT COUNT(*) FROM TestTable1 WHERE ID = {0}";

        int count = context.Database.ExecuteScalar<int>(sql, id);

        Assert.AreEqual(1, count, "Invalid count");
    }

    /// <summary>
    /// Test ExecuteScalar with params parameter.
    /// </summary>
    /// <param name="databaseType">Database type.</param>
    /// <returns>A task that represents the asynchronous test operation.</returns>
    [TestMethod]
    [DataRow(DatabaseType.Sqlite, DisplayName = "SQLite ExecuteScalar params.")]
    [DataRow(DatabaseType.SqlServer, DisplayName = "SQL Server ExecuteScalar params.")]
    public async Task Test_ExecuteScalar_ParamsAsync(string databaseType)
    {
        using Context context = DatabaseUtils.CreateDatabase(databaseType);

        string value = DatabaseUtils.GetMethodName();
        long id = DatabaseUtils.CreateSingleTestTableEntry(context, value);
        string sql = "SELECT COUNT(*) FROM TestTable1 WHERE ID = {0}";

        int count = await context.Database.ExecuteScalarAsync<int>(sql, parameters: id).ConfigureAwait(false);

        Assert.AreEqual(1, count, "Invalid count");
    }

    #endregion public methods
}
