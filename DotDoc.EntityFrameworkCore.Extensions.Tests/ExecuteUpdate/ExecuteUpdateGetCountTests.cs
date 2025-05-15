// Copyright ©2021-2025 Mike King.
// This file is licensed to you under the MIT license.
// See the License.txt file in the solution root for more information.

using DotDoc.EntityFrameworkCore.Extensions.DatabaseType;
using DotDoc.EntityFrameworkCore.Extensions.ExecuteUpdate;
using DotDoc.EntityFrameworkCore.Extensions.Tests.Data;
using DotDoc.EntityFrameworkCore.Extensions.Tests.Utilities;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DotDoc.EntityFrameworkCore.Extensions.Tests.ExecuteUpdate;

/// <summary>
/// Tests for ExecuteUpdateGetCount extensions.
/// </summary>
[TestClass]
public class ExecuteUpdateGetCountTests
{
    #region public methods

    /// <summary>
    /// Test ExecuteUpdateGetCountAsync Guard Clauses.
    /// </summary>
    /// <param name="query">The LINQ query.</param>
    /// <param name="setPropertyCalls">A method containing set property statements specifying properties to update.</param>
    /// <param name="exceptionType">The type of exception raised.</param>
    /// <param name="paramName">Name of parameter being checked.</param>
    /// <returns>A <see cref="Task"/> that represents the asynchronous operation.</returns>
    [TestMethod("ExecuteUpdateGetCountAsync Guard Clauses")]
    [DynamicData(nameof(Get_ExecuteUpdateGetCount_GuardClause_TestData), DynamicDataSourceType.Method, DynamicDataDisplayName = nameof(TestUtils.CreateDynamicDisplayName), DynamicDataDisplayNameDeclaringType = typeof(TestUtils))]
    public async Task Test_ExecuteUpdateGetCount_GuardClausesAsync(IQueryable<TestTable1> query, Action<UpdateSettersBuilder<TestTable1>> setPropertyCalls, Type exceptionType, string paramName)
    {
        // ARRANGE

        // ACT / ASSERT
        Exception e = await Assert.ThrowsAsync<Exception>(() => query.ExecuteUpdateGetCountAsync(setPropertyCalls!), "Missing exception").ConfigureAwait(false);
        Assert.AreEqual(exceptionType, e.GetType(), "Invalid exception type");
        Assert.AreEqual(paramName, ((ArgumentException)e).ParamName, "Invalid parameter name");
    }

    /// <summary>
    /// Test ExecuteUpdateGetCountAsync.
    /// </summary>
    /// <param name="databaseType">Database type.</param>
    /// <param name="count">Number of records to update.</param>
    /// <returns>A <see cref="Task"/> that represents the asynchronous operation.</returns>
    [TestMethod("ExecuteUpdateGetCountAsync")]
    [DataRow(DatabaseTypes.Sqlite, 0, DisplayName = $"{DatabaseTypes.Sqlite} Update 0 Records.")]
    [DataRow(DatabaseTypes.Sqlite, 1, DisplayName = $"{DatabaseTypes.Sqlite} Update 1 Record.")]
    [DataRow(DatabaseTypes.Sqlite, 10, DisplayName = $"{DatabaseTypes.Sqlite} Update 10 Records.")]
    [DataRow(DatabaseTypes.SqlServer, 0, DisplayName = $"{DatabaseTypes.SqlServer} Update 0 Records.")]
    [DataRow(DatabaseTypes.SqlServer, 1, DisplayName = $"{DatabaseTypes.SqlServer} Update 1 Record.")]
    [DataRow(DatabaseTypes.SqlServer, 10, DisplayName = $"{DatabaseTypes.SqlServer} Update 10 Records.")]
    public async Task Test_ExecuteUpdateGetCountAsync(string databaseType, int count)
    {
        // ARRANGE
        using Context context = await DatabaseUtils.CreateDatabaseAsync(
            databaseType,
            customConfigurationActions: (optionsBuilder) => optionsBuilder.UseExecuteUpdateExtensions())
            .ConfigureAwait(false);

        string value = TestUtils.GetMethodName();
        string originalValue = $"Original {value}";
        string updatedValue = $"Updated {value}";

        await DatabaseUtils.CreateTestTableEntriesAsync(context, originalValue, (count + 1) * 10).ConfigureAwait(false);

        long startId = 2;
        long endId = startId + count - 1;

        IQueryable<TestTable1> query = context.TestTable1.Where(e => e.Id >= startId && e.Id <= endId);

        // ACT
        int updateRowCount = await query.ExecuteUpdateGetCountAsync(builder =>
        {
            builder.SetProperty(e => e.TestField, updatedValue);
        }).ConfigureAwait(false);

        // ASSERT
        Assert.AreEqual(count, updateRowCount, "Invalid count");

        IList<TestTable1> rows = await query
            .AsNoTracking()
            .ToListAsync()
            .ConfigureAwait(false);

        foreach (TestTable1 row in rows)
        {
            Assert.AreEqual(updatedValue, row.TestField, "Unexpected field value");
        }
    }

    #endregion public methods

    #region private methods

    /// <summary>
    /// Get test data for ExecuteUpdateGetCount methods.
    /// </summary>
    /// <returns><see cref="IEnumerable{T}"/>.</returns>
    private static IEnumerable<object?[]> Get_ExecuteUpdateGetCount_GuardClause_TestData()
    {
        // 0. IQueryable<TestTable1> query
        // 1. Action<UpdateSettersBuilder<TestTable1>> setPropertyCalls
        // 2. Type exceptionType
        // 3. string paramName
        yield return [
            null,
            new Action<UpdateSettersBuilder<TestTable1>>(builder => { }),
            typeof(ArgumentNullException),
            "query"];

        yield return [
            Array.Empty<TestTable1>().AsQueryable(),
            null,
            typeof(ArgumentNullException),
            "setPropertyCalls"];
    }

    #endregion private methods
}
