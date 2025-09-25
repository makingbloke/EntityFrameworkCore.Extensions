// Copyright ©2021-2025 Mike King.
// This file is licensed to you under the MIT license.
// See the License.txt file in the solution root for more information.

using DotDoc.EntityFrameworkCore.Extensions.DatabaseType;
using DotDoc.EntityFrameworkCore.Extensions.ExecuteUpdate;
using DotDoc.EntityFrameworkCore.Extensions.Tests.Data;
using DotDoc.EntityFrameworkCore.Extensions.Tests.TestUtilities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DotDoc.EntityFrameworkCore.Extensions.Tests.ExecuteUpdate;

/// <summary>
/// Tests for ExecuteUpdateGetRows extensions.
/// </summary>
[TestClass]
public class ExecuteUpdateGetRowsTests
{
    #region public methods

    /// <summary>
    /// Test ExecuteUpdateGetRowsAsync Guard Clauses.
    /// </summary>
    /// <param name="query">The LINQ query.</param>
    /// <param name="setPropertyCalls">A method containing set property statements specifying properties to update.</param>
    /// <param name="exceptionType">The type of exception raised.</param>
    /// <param name="paramName">Name of parameter being checked.</param>
    /// <returns>A <see cref="Task"/> that represents the asynchronous operation.</returns>
    [TestMethod(DisplayName = "ExecuteUpdateGetRowsAsync Guard Clauses")]
    [DynamicData(nameof(Get_ExecuteUpdateGetRowsAsync_GuardClause_TestData), DynamicDataSourceType.Method, DynamicDataDisplayName = nameof(TestUtils.CreateDynamicDisplayName), DynamicDataDisplayNameDeclaringType = typeof(TestUtils))]
    public async Task Test_ExecuteUpdateGetRowsAsync_GuardClauses_Async(IQueryable<TestTable1> query, Action<UpdateSettersBuilder<TestTable1>> setPropertyCalls, Type exceptionType, string paramName)
    {
        // ARRANGE

        // ACT / ASSERT
        Exception e = await Assert.ThrowsAsync<Exception>(() => query.ExecuteUpdateGetRowsAsync(setPropertyCalls!, CancellationToken.None), "Unexpected exception").ConfigureAwait(false);
        Assert.AreEqual(exceptionType, e.GetType(), "Invalid exception type");
        Assert.AreEqual(paramName, ((ArgumentException)e).ParamName, "Invalid parameter name");
    }

    /// <summary>
    /// Test ExecuteUpdateGetRowsAsync.
    /// </summary>
    /// <param name="databaseType">Database type.</param>
    /// <param name="useReturningClause">If true update uses returning clause in sql else uses a select after update is completed.</param>
    /// <param name="count">Number of records to update.</param>
    /// <returns>A <see cref="Task"/> that represents the asynchronous operation.</returns>
    [TestMethod(DisplayName = "ExecuteUpdateGetRowsAsync")]
    [DataRow(DatabaseTypes.Sqlite, true, 0, DisplayName = $"{DatabaseTypes.Sqlite} Use Returning Clause Update 0 Records.")]
    [DataRow(DatabaseTypes.Sqlite, false, 0, DisplayName = $"{DatabaseTypes.Sqlite} Use Select Statement Update 0 Records.")]
    [DataRow(DatabaseTypes.Sqlite, true, 1, DisplayName = $"{DatabaseTypes.Sqlite} Use Returning Clause Update 1 Record.")]
    [DataRow(DatabaseTypes.Sqlite, false, 1, DisplayName = $"{DatabaseTypes.Sqlite} Use Select Statement Update 1 Record.")]
    [DataRow(DatabaseTypes.Sqlite, true, 10, DisplayName = $"{DatabaseTypes.Sqlite} Use Returning Clause Update 10 Records.")]
    [DataRow(DatabaseTypes.Sqlite, false, 10, DisplayName = $"{DatabaseTypes.Sqlite} Use Select Statement Update 10 Records.")]
    [DataRow(DatabaseTypes.SqlServer, true, 0, DisplayName = $"{DatabaseTypes.SqlServer} Use Returning Clause Update 0 Records.")]
    [DataRow(DatabaseTypes.SqlServer, false, 0, DisplayName = $"{DatabaseTypes.SqlServer} Use Select Statement Update 0 Records.")]
    [DataRow(DatabaseTypes.SqlServer, true, 1, DisplayName = $"{DatabaseTypes.SqlServer} Use Returning Clause Update 1 Record.")]
    [DataRow(DatabaseTypes.SqlServer, false, 1, DisplayName = $"{DatabaseTypes.SqlServer} Use Select Statement Update 1 Record.")]
    [DataRow(DatabaseTypes.SqlServer, true, 10, DisplayName = $"{DatabaseTypes.SqlServer} Use Returning Clause Update 10 Records.")]
    [DataRow(DatabaseTypes.SqlServer, false, 10, DisplayName = $"{DatabaseTypes.SqlServer} Use Select Statement Update 10 Records.")]
    public async Task Test_ExecuteUpdateGetRowsAsync_Async(string databaseType, bool useReturningClause, int count)
    {
        // ARRANGE
        using Context context = await DatabaseUtils.CreateDatabaseAsync(
            databaseType: databaseType,
            customConfigurationActions: (optionsBuilder) =>
            {
                // Stop EF core caching the service provider so a new model gets
                // created every time and uses the desired useReturningClause value.
                optionsBuilder.EnableServiceProviderCaching(false);

                optionsBuilder.UseExecuteUpdateExtensions();
            },
            customModelCreationActions: (modelBuilder) =>
            {
                Action<TableBuilder<TestTable1>> buildAction = databaseType switch
                {
                    DatabaseTypes.Sqlite => tableBuilder => tableBuilder.UseSqlReturningClause(useReturningClause),
                    DatabaseTypes.SqlServer => tableBuilder => tableBuilder.UseSqlOutputClause(useReturningClause),
                    _ => throw new InvalidOperationException("Unsupported database type")
                };

                modelBuilder.Entity<TestTable1>()
                    .ToTable(buildAction);
            })
            .ConfigureAwait(false);

        string value = TestUtils.GetMethodName();
        string originalValue = $"Original {value}";
        string updatedValue = $"Updated {value}";

        await DatabaseUtils.CreateTestTableEntriesAsync(context, originalValue, (count + 1) * 10).ConfigureAwait(false);

        long startId = 2;
        long endId = startId + count - 1;

        IQueryable<TestTable1> query = context.TestTable1.Where(e => e.Id >= startId && e.Id <= endId);

        // ACT
        IList<TestTable1> rows = await query.ExecuteUpdateGetRowsAsync(
            builder =>
            {
                builder.SetProperty(e => e.TestField, updatedValue);
            },
            CancellationToken.None)
            .ConfigureAwait(false);

        // ASSERT
        long id = startId;
        foreach (TestTable1 row in rows.OrderBy(r => r.Id))
        {
            Assert.AreEqual(id, row.Id, "Unexpected Id value");
            Assert.AreEqual(updatedValue, row.TestField, "Unexpected field value");

            id++;
        }
    }

    #endregion public methods

    #region private methods

    /// <summary>
    /// Get test data for ExecuteUpdateGetRows methods.
    /// </summary>
    /// <returns><see cref="IEnumerable{T}"/>.</returns>
    private static IEnumerable<object?[]> Get_ExecuteUpdateGetRowsAsync_GuardClause_TestData()
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
