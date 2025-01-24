// Copyright ©2021-2025 Mike King.
// This file is licensed to you under the MIT license.
// See the License.txt file in the solution root for more information.

using DotDoc.EntityFrameworkCore.Extensions.Constants;
using DotDoc.EntityFrameworkCore.Extensions.Extensions;
using DotDoc.EntityFrameworkCore.Extensions.Tests.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DotDoc.EntityFrameworkCore.Extensions.Tests.QueryExtensions;

/// <summary>
/// Tests for ExecuteInsert extensions.
/// </summary>
[TestClass]
public class ExecuteInsertTests
{
    #region public methods

    /// <summary>
    /// Test long ExecuteInsert(DatabaseFacade, FormattableString) with a null <see cref="DatabaseFacade"/> object.
    /// </summary>
    [TestMethod]
    public void Test_Long_ExecuteInsert_DatabaseFacade_FormattableString_NullDatabaseFacade()
    {
        // ARRANGE
        DatabaseFacade? databaseFacade = null;
        FormattableString sql = $"dummy";

        // ACT / ASSERT
        Assert.ThrowsException<ArgumentNullException>(() => databaseFacade!.ExecuteInsert(sql), "Unexpected exception");
    }

    /// <summary>
    /// Test long ExecuteInsert(DatabaseFacade, FormattableString) with a null <see cref="FormattableString"/> object.
    /// </summary>
    [TestMethod]
    public void Test_Long_ExecuteInsert_DatabaseFacade_FormattableString_NullFormattableString()
    {
        // ARRANGE
        using Context context = DatabaseUtils.CreateDatabase(DatabaseType.Sqlite);

        DatabaseFacade databaseFacade = context.Database;
        FormattableString? sql = null;

        // ACT / ASSERT
        Assert.ThrowsException<ArgumentNullException>(() => databaseFacade!.ExecuteInsert(sql!), "Unexpected exception");
    }

    /// <summary>
    /// Test T ExecuteInsert(DatabaseFacade, FormattableString) with a null <see cref="DatabaseFacade"/> object.
    /// </summary>
    [TestMethod]
    public void Test_T_ExecuteInsert_DatabaseFacade_FormattableString_NullDatabaseFacade()
    {
        // ARRANGE
        DatabaseFacade? databaseFacade = null;
        FormattableString sql = $"dummy";

        // ACT / ASSERT
        Assert.ThrowsException<ArgumentNullException>(() => databaseFacade!.ExecuteInsert<TestTable1>(sql), "Unexpected exception");
    }

    /// <summary>
    /// Test T ExecuteInsert(DatabaseFacade, FormattableString) with a null <see cref="FormattableString"/> object.
    /// </summary>
    [TestMethod]
    public void Test_T_ExecuteInsert_DatabaseFacade_FormattableString_NullFormattableString()
    {
        // ARRANGE
        using Context context = DatabaseUtils.CreateDatabase(DatabaseType.Sqlite);

        DatabaseFacade databaseFacade = context.Database;
        FormattableString? sql = null;

        // ACT / ASSERT
        Assert.ThrowsException<ArgumentNullException>(() => databaseFacade!.ExecuteInsert<TestTable1>(sql!), "Unexpected exception");
    }

    /// <summary>
    /// Test long ExecuteInsert(DatabaseFacade, string, IEnumerable) with a null <see cref="DatabaseFacade"/> object.
    /// </summary>
    [TestMethod]
    public void Test_Long_ExecuteInsert_DatabaseFacade_String_IEnumerable_NullDatabaseFacade()
    {
        // ARRANGE
        DatabaseFacade? databaseFacade = null;
        string sql = "dummy";
        object?[] parameters = [];

        // ACT / ASSERT
        Assert.ThrowsException<ArgumentNullException>(() => databaseFacade!.ExecuteInsert(sql, parameters), "Unexpected exception");
    }

    /// <summary>
    /// Test long ExecuteInsert(DatabaseFacade, string, IEnumerable) with a null <see cref="string"/> object.
    /// </summary>
    [TestMethod]
    public void Test_Long_ExecuteInsert_DatabaseFacade_String_IEnumerable_NullString()
    {
        // ARRANGE
        using Context context = DatabaseUtils.CreateDatabase(DatabaseType.Sqlite);

        DatabaseFacade databaseFacade = context.Database;
        string? sql = null;
        object?[] parameters = [];

        // ACT / ASSERT
        Assert.ThrowsException<ArgumentNullException>(() => databaseFacade.ExecuteInsert(sql!, parameters), "Unexpected exception");
    }

    /// <summary>
    /// Test long ExecuteInsert(DatabaseFacade, string, IEnumerable) with a null <see cref="IEnumerable{Object}"/> object.
    /// </summary>
    [TestMethod]
    public void Test_Long_ExecuteInsert_DatabaseFacade_String_IEnumerable_NullIEnumerable()
    {
        // ARRANGE
        using Context context = DatabaseUtils.CreateDatabase(DatabaseType.Sqlite);

        DatabaseFacade databaseFacade = context.Database;
        string sql = "dummy";
        object?[]? parameters = null;

        // ACT / ASSERT
        Assert.ThrowsException<ArgumentNullException>(() => databaseFacade.ExecuteInsert(sql, parameters!), "Unexpected exception");
    }

    /// <summary>
    /// Test T ExecuteInsert(DatabaseFacade, string, IEnumerable) with a null <see cref="DatabaseFacade"/> object.
    /// </summary>
    [TestMethod]
    public void Test_T_ExecuteInsert_DatabaseFacade_String_IEnumerable_NullDatabaseFacade()
    {
        // ARRANGE
        DatabaseFacade? databaseFacade = null;
        string sql = "dummy";
        object?[] parameters = [];

        // ACT / ASSERT
        Assert.ThrowsException<ArgumentNullException>(() => databaseFacade!.ExecuteInsert<TestTable1>(sql, parameters), "Unexpected exception");
    }

    /// <summary>
    /// Test T ExecuteInsert(DatabaseFacade, string, IEnumerable) with a null <see cref="string"/> object.
    /// </summary>
    [TestMethod]
    public void Test_T_ExecuteInsert_DatabaseFacade_String_IEnumerable_NullString()
    {
        // ARRANGE
        using Context context = DatabaseUtils.CreateDatabase(DatabaseType.Sqlite);

        DatabaseFacade databaseFacade = context.Database;
        string? sql = null;
        object?[] parameters = [];

        // ACT / ASSERT
        Assert.ThrowsException<ArgumentNullException>(() => databaseFacade.ExecuteInsert<TestTable1>(sql!, parameters), "Unexpected exception");
    }

    /// <summary>
    /// Test T ExecuteInsert(DatabaseFacade, string, IEnumerable) with a null <see cref="IEnumerable{Object}"/> object.
    /// </summary>
    [TestMethod]
    public void Test_T_ExecuteInsert_DatabaseFacade_String_IEnumerable_NullIEnumerable()
    {
        // ARRANGE
        using Context context = DatabaseUtils.CreateDatabase(DatabaseType.Sqlite);

        DatabaseFacade databaseFacade = context.Database;
        string sql = "dummy";
        object?[]? parameters = null;

        // ACT / ASSERT
        Assert.ThrowsException<ArgumentNullException>(() => databaseFacade.ExecuteInsert<TestTable1>(sql, parameters!), "Unexpected exception");
    }

    /// <summary>
    /// Test long ExecuteInsert(DatabaseFacade, FormattableString) with a null <see cref="DatabaseFacade"/> object.
    /// </summary>
    /// <returns>A task that represents the asynchronous test operation.</returns>
    [TestMethod]
    public async Task Test_Long_ExecuteInsert_DatabaseFacade_FormattableString_NullDatabaseFacadeAsync()
    {
        // ARRANGE
        DatabaseFacade? databaseFacade = null;
        FormattableString sql = $"dummy";

        // ACT / ASSERT
        await Assert.ThrowsExceptionAsync<ArgumentNullException>(() => databaseFacade!.ExecuteInsertAsync(sql), "Unexpected exception").ConfigureAwait(false);
    }

    /// <summary>
    /// Test long ExecuteInsert(DatabaseFacade, FormattableString) with a null <see cref="FormattableString"/> object.
    /// </summary>
    /// <returns>A task that represents the asynchronous test operation.</returns>
    [TestMethod]
    public async Task Test_Long_ExecuteInsert_DatabaseFacade_FormattableString_NullFormattableStringAsync()
    {
        // ARRANGE
        using Context context = DatabaseUtils.CreateDatabase(DatabaseType.Sqlite);

        DatabaseFacade databaseFacade = context.Database;
        FormattableString? sql = null;

        // ACT / ASSERT
        await Assert.ThrowsExceptionAsync<ArgumentNullException>(() => databaseFacade!.ExecuteInsertAsync(sql!), "Unexpected exception").ConfigureAwait(false);
    }

    /// <summary>
    /// Test T ExecuteInsert(DatabaseFacade, FormattableString) with a null <see cref="DatabaseFacade"/> object.
    /// </summary>
    /// <returns>A task that represents the asynchronous test operation.</returns>
    [TestMethod]
    public async Task Test_T_ExecuteInsert_DatabaseFacade_FormattableString_NullDatabaseFacadeAsync()
    {
        // ARRANGE
        DatabaseFacade? databaseFacade = null;
        FormattableString sql = $"dummy";

        // ACT / ASSERT
        await Assert.ThrowsExceptionAsync<ArgumentNullException>(() => databaseFacade!.ExecuteInsertAsync<TestTable1>(sql), "Unexpected exception").ConfigureAwait(false);
    }

    /// <summary>
    /// Test T ExecuteInsert(DatabaseFacade, FormattableString) with a null <see cref="FormattableString"/> object.
    /// </summary>
    /// <returns>A task that represents the asynchronous test operation.</returns>
    [TestMethod]
    public async Task Test_T_ExecuteInsert_DatabaseFacade_FormattableString_NullFormattableStringAsync()
    {
        // ARRANGE
        using Context context = DatabaseUtils.CreateDatabase(DatabaseType.Sqlite);

        DatabaseFacade databaseFacade = context.Database;
        FormattableString? sql = null;

        // ACT / ASSERT
        await Assert.ThrowsExceptionAsync<ArgumentNullException>(() => databaseFacade!.ExecuteInsertAsync<TestTable1>(sql!), "Unexpected exception").ConfigureAwait(false);
    }

    /// <summary>
    /// Test long ExecuteInsert(DatabaseFacade, string, IEnumerable) with a null <see cref="DatabaseFacade"/> object.
    /// </summary>
    /// <returns>A task that represents the asynchronous test operation.</returns>
    [TestMethod]
    public async Task Test_Long_ExecuteInsert_DatabaseFacade_String_IEnumerable_NullDatabaseFacadeAsync()
    {
        // ARRANGE
        DatabaseFacade? databaseFacade = null;
        string sql = "dummy";
        object?[] parameters = [];

        // ACT / ASSERT
        await Assert.ThrowsExceptionAsync<ArgumentNullException>(() => databaseFacade!.ExecuteInsertAsync(sql, parameters: parameters), "Unexpected exception").ConfigureAwait(false);
    }

    /// <summary>
    /// Test long ExecuteInsert(DatabaseFacade, string, IEnumerable) with a null <see cref="string"/> object.
    /// </summary>
    /// <returns>A task that represents the asynchronous test operation.</returns>
    [TestMethod]
    public async Task Test_Long_ExecuteInsert_DatabaseFacade_String_IEnumerable_NullStringAsync()
    {
        // ARRANGE
        using Context context = DatabaseUtils.CreateDatabase(DatabaseType.Sqlite);

        DatabaseFacade databaseFacade = context.Database;
        string? sql = null;
        object?[] parameters = [];

        // ACT / ASSERT
        await Assert.ThrowsExceptionAsync<ArgumentNullException>(() => databaseFacade.ExecuteInsertAsync(sql!, parameters: parameters), "Unexpected exception").ConfigureAwait(false);
    }

    /// <summary>
    /// Test long ExecuteInsert(DatabaseFacade, string, IEnumerable) with a null <see cref="IEnumerable{Object}"/> object.
    /// </summary>
    /// <returns>A task that represents the asynchronous test operation.</returns>
    [TestMethod]
    public async Task Test_Long_ExecuteInsert_DatabaseFacade_String_IEnumerable_NullIEnumerableAsync()
    {
        // ARRANGE
        using Context context = DatabaseUtils.CreateDatabase(DatabaseType.Sqlite);

        DatabaseFacade databaseFacade = context.Database;
        string sql = "dummy";
        object?[]? parameters = null;

        // ACT / ASSERT
        await Assert.ThrowsExceptionAsync<ArgumentNullException>(() => databaseFacade.ExecuteInsertAsync(sql, parameters: parameters!), "Unexpected exception").ConfigureAwait(false);
    }

    /// <summary>
    /// Test T ExecuteInsert(DatabaseFacade, string, IEnumerable) with a null <see cref="DatabaseFacade"/> object.
    /// </summary>
    /// <returns>A task that represents the asynchronous test operation.</returns>
    [TestMethod]
    public async Task Test_T_ExecuteInsert_DatabaseFacade_String_IEnumerable_NullDatabaseFacadeAsync()
    {
        // ARRANGE
        DatabaseFacade? databaseFacade = null;
        string sql = "dummy";
        object?[] parameters = [];

        // ACT / ASSERT
        await Assert.ThrowsExceptionAsync<ArgumentNullException>(() => databaseFacade!.ExecuteInsertAsync<TestTable1>(sql, parameters: parameters), "Unexpected exception").ConfigureAwait(false);
    }

    /// <summary>
    /// Test T ExecuteInsert(DatabaseFacade, string, IEnumerable) with a null <see cref="string"/> object.
    /// </summary>
    /// <returns>A task that represents the asynchronous test operation.</returns>
    [TestMethod]
    public async Task Test_T_ExecuteInsert_DatabaseFacade_String_IEnumerable_NullStringAsync()
    {
        // ARRANGE
        using Context context = DatabaseUtils.CreateDatabase(DatabaseType.Sqlite);

        DatabaseFacade databaseFacade = context.Database;
        string? sql = null;
        object?[] parameters = [];

        // ACT / ASSERT
        await Assert.ThrowsExceptionAsync<ArgumentNullException>(() => databaseFacade.ExecuteInsertAsync<TestTable1>(sql!, parameters: parameters), "Unexpected exception").ConfigureAwait(false);
    }

    /// <summary>
    /// Test T ExecuteInsert(DatabaseFacade, string, IEnumerable) with a null <see cref="IEnumerable{Object}"/> object.
    /// </summary>
    /// <returns>A task that represents the asynchronous test operation.</returns>
    [TestMethod]
    public async Task Test_T_ExecuteInsert_DatabaseFacade_String_IEnumerable_NullIEnumerableAsync()
    {
        // ARRANGE
        using Context context = DatabaseUtils.CreateDatabase(DatabaseType.Sqlite);

        DatabaseFacade databaseFacade = context.Database;
        string sql = "dummy";
        object?[]? parameters = null;

        // ACT / ASSERT
        await Assert.ThrowsExceptionAsync<ArgumentNullException>(() => databaseFacade.ExecuteInsertAsync<TestTable1>(sql, parameters: parameters!), "Unexpected exception").ConfigureAwait(false);
    }

    /// <summary>
    /// Test ExecuteInsert with FormattableString parameter returning a long Id.
    /// </summary>
    /// <param name="databaseType">Database type.</param>
    [TestMethod]
    [DataRow(DatabaseType.Sqlite, DisplayName = "SQLite ExecuteInsert FormattableString Long Id.")]
    [DataRow(DatabaseType.SqlServer, DisplayName = "SQL Server ExecuteInsert FormattableString Long Id.")]
    public void Test_ExecuteInsert_FormattableString_Long(string databaseType)
    {
        // ARRANGE
        using Context context = DatabaseUtils.CreateDatabase(databaseType);

        string value = DatabaseUtils.GetMethodName();
        FormattableString sql = $"INSERT INTO TestTable1 (TestField) VALUES ({value})";

        // ACT
        long id = context.Database.ExecuteInsert(sql);

        // ASSERT
        int count = context.TestTable1.Count(e => e.Id == id);
        Assert.AreEqual(1, count, "Invalid count");
    }

    /// <summary>
    /// Test ExecuteInsert with FormattableString parameter returning a generic Id.
    /// </summary>
    /// <param name="databaseType">Database type.</param>
    [TestMethod]
    [DataRow(DatabaseType.Sqlite, DisplayName = "SQLite ExecuteInsert FormattableString Generic Id.")]
    [DataRow(DatabaseType.SqlServer, DisplayName = "SQL Server ExecuteInsert FormattableString Generic Id.")]
    public void Test_ExecuteInsert_FormattableString_Generic(string databaseType)
    {
        // ARRANGE
        using Context context = DatabaseUtils.CreateDatabase(databaseType);

        string value = DatabaseUtils.GetMethodName();
        FormattableString sql = $"INSERT INTO TestTable1 (TestField) VALUES ({value})";

        // ACT
        long id = context.Database.ExecuteInsert<long>(sql);

        // ASSERT
        int count = context.TestTable1.Count(e => e.Id == id);
        Assert.AreEqual(1, count, "Invalid count");
    }

    /// <summary>
    /// Test ExecuteInsert with FormattableString parameter returning a long Id.
    /// </summary>
    /// <param name="databaseType">Database type.</param>
    /// <returns>A task that represents the asynchronous test operation.</returns>
    [TestMethod]
    [DataRow(DatabaseType.Sqlite, DisplayName = "SQLite ExecuteInsert FormattableString Long Id.")]
    [DataRow(DatabaseType.SqlServer, DisplayName = "SQL Server ExecuteInsert FormattableString Long Id.")]
    public async Task Test_ExecuteInsert_FormattableString_LongAsync(string databaseType)
    {
        // ARRANGE
        using Context context = DatabaseUtils.CreateDatabase(databaseType);

        string value = DatabaseUtils.GetMethodName();
        FormattableString sql = $"INSERT INTO TestTable1 (TestField) VALUES ({value})";

        // ACT
        long id = await context.Database.ExecuteInsertAsync(sql).ConfigureAwait(false);

        // ASSERT
        int count = await context.TestTable1.CountAsync(e => e.Id == id).ConfigureAwait(false);
        Assert.AreEqual(1, count, "Invalid count");
    }

    /// <summary>
    /// Test ExecuteInsert with FormattableString parameter returning a generic Id.
    /// </summary>
    /// <param name="databaseType">Database type.</param>
    /// <returns>A task that represents the asynchronous test operation.</returns>
    [TestMethod]
    [DataRow(DatabaseType.Sqlite, DisplayName = "SQLite ExecuteInsert FormattableString Generic Id.")]
    [DataRow(DatabaseType.SqlServer, DisplayName = "SQL Server ExecuteInsert FormattableString Generic Id.")]
    public async Task Test_ExecuteInsert_FormattableString_GenericAsync(string databaseType)
    {
        // ARRANGE
        using Context context = DatabaseUtils.CreateDatabase(databaseType);

        string value = DatabaseUtils.GetMethodName();
        FormattableString sql = $"INSERT INTO TestTable1 (TestField) VALUES ({value})";

        // ACT
        long id = await context.Database.ExecuteInsertAsync<long>(sql).ConfigureAwait(false);

        // ASSERT
        int count = await context.TestTable1.CountAsync(e => e.Id == id).ConfigureAwait(false);
        Assert.AreEqual(1, count, "Invalid count");
    }

    /// <summary>
    /// Test ExecuteInsert with params parameters returning a long Id.
    /// </summary>
    /// <param name="databaseType">Database type.</param>
    [TestMethod]
    [DataRow(DatabaseType.Sqlite, DisplayName = "SQLite ExecuteInsert params Long Id.")]
    [DataRow(DatabaseType.SqlServer, DisplayName = "SQL Server ExecuteInsert params Long Id.")]
    public void Test_ExecuteInsert_Params_Long(string databaseType)
    {
        // ARRANGE
        using Context context = DatabaseUtils.CreateDatabase(databaseType);

        string value = DatabaseUtils.GetMethodName();
        string sql = "INSERT INTO TestTable1 (TestField) VALUES ({0})";

        // ACT
        long id = context.Database.ExecuteInsert(sql, value);

        // ASSERT
        int count = context.TestTable1.Count(e => e.Id == id);
        Assert.AreEqual(1, count, "Invalid count");
    }

    /// <summary>
    /// Test ExecuteInsert with params parameters returning a generic Id.
    /// </summary>
    /// <param name="databaseType">Database type.</param>
    [TestMethod]
    [DataRow(DatabaseType.Sqlite, DisplayName = "SQLite ExecuteInsert params Generic Id.")]
    [DataRow(DatabaseType.SqlServer, DisplayName = "SQL Server ExecuteInsert params Generic Id.")]
    public void Test_ExecuteInsert_Params_Generic(string databaseType)
    {
        // ARRANGE
        using Context context = DatabaseUtils.CreateDatabase(databaseType);

        string value = DatabaseUtils.GetMethodName();
        string sql = "INSERT INTO TestTable1 (TestField) VALUES ({0})";

        // ACT
        long id = context.Database.ExecuteInsert<long>(sql, value);

        // ASSERT
        int count = context.TestTable1.Count(e => e.Id == id);
        Assert.AreEqual(1, count, "Invalid count");
    }

    /// <summary>
    /// Test ExecuteInsert with params parameters returning a long Id.
    /// </summary>
    /// <param name="databaseType">Database type.</param>
    /// <returns>A task that represents the asynchronous test operation.</returns>
    [TestMethod]
    [DataRow(DatabaseType.Sqlite, DisplayName = "SQLite ExecuteInsert params Long Id.")]
    [DataRow(DatabaseType.SqlServer, DisplayName = "SQL Server ExecuteInsert params Long Id.")]
    public async Task Test_ExecuteInsert_Params_LongAsync(string databaseType)
    {
        // ARRANGE
        using Context context = DatabaseUtils.CreateDatabase(databaseType);

        string value = DatabaseUtils.GetMethodName();
        string sql = "INSERT INTO TestTable1 (TestField) VALUES ({0})";

        // ACT
        long id = await context.Database.ExecuteInsertAsync(sql, parameters: value).ConfigureAwait(false);

        // ASSERT
        int count = await context.TestTable1.CountAsync(e => e.Id == id).ConfigureAwait(false);
        Assert.AreEqual(1, count, "Invalid count");
    }

    /// <summary>
    /// Test ExecuteInsert with params parameters returning a generic Id.
    /// </summary>
    /// <param name="databaseType">Database type.</param>
    /// <returns>A task that represents the asynchronous test operation.</returns>
    [TestMethod]
    [DataRow(DatabaseType.Sqlite, DisplayName = "SQLite ExecuteInsert params Generic Id.")]
    [DataRow(DatabaseType.SqlServer, DisplayName = "SQL Server ExecuteInsert params Generic Id.")]
    public async Task Test_ExecuteInsert_Params_GenericAsync(string databaseType)
    {
        // ARRANGE
        using Context context = DatabaseUtils.CreateDatabase(databaseType);

        string value = DatabaseUtils.GetMethodName();
        string sql = "INSERT INTO TestTable1 (TestField) VALUES ({0})";

        // ACT
        long id = await context.Database.ExecuteInsertAsync<long>(sql, parameters: value).ConfigureAwait(false);

        // ASSERT
        int count = await context.TestTable1.CountAsync(e => e.Id == id).ConfigureAwait(false);
        Assert.AreEqual(1, count, "Invalid count");
    }

    #endregion public methods
}
