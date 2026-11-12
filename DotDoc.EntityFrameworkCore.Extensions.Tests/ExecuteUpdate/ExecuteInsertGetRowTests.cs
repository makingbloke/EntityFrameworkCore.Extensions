// Copyright ©2021-2026 Mike King.
// This file is licensed to you under the MIT license.
// See the License.txt file in the solution root for more information.

using DotDoc.EntityFrameworkCore.Extensions.DatabaseType;
using DotDoc.EntityFrameworkCore.Extensions.ExecuteUpdate;
using DotDoc.EntityFrameworkCore.Extensions.Tests.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;

namespace DotDoc.EntityFrameworkCore.Extensions.Tests.ExecuteUpdate;

/// <summary>
/// Tests for ExecuteInsertGetRow extensions.
/// </summary>
[TestClass]
public class ExecuteInsertGetRowTests
{
    #region private fields

    /// <summary>
    /// The MSTest <see cref="TestContext"/> for the current test.
    /// </summary>
    private readonly TestContext _testContext;

    #endregion private fields

    #region public constructors

    /// <summary>
    /// Initializes a new instance of the <see cref="ExecuteInsertGetRowTests"/> class.
    /// </summary>
    /// <param name="testContext">The MSTest test context.</param>
    public ExecuteInsertGetRowTests(TestContext testContext)
    {
        this._testContext = testContext;
    }

    #endregion public constructors

    #region public methods

    /// <summary>
    /// Test ExecuteInsertGetRowAsync with Null IQueryable source parameter.
    /// </summary>
    /// <returns>A <see cref="Task"/> that represents the asynchronous operation.</returns>
    [TestMethod(DisplayName = "ExecuteInsertGetRowAsync with Null IQueryable source parameter")]
    public async Task ExecuteInsertGetRowTests_001_Async()
    {
        // ARRANGE
        IQueryable<TestTable1> source = null!;
        Action<UpdateSettersBuilder<TestTable1>> setPropertyCalls = new(builder => { });

        // ACT / ASSERT
        await Assert.ThrowsExactlyAsync<ArgumentNullException>(() => source.ExecuteInsertGetRowAsync(setPropertyCalls, this._testContext.CancellationToken), "Unexpected exception").ConfigureAwait(false);
    }

    /// <summary>
    /// Test ExecuteInsertGetRowAsync with Null UpdateSettersBuilder setPropertyCalls parameter.
    /// </summary>
    /// <returns>A <see cref="Task"/> that represents the asynchronous operation.</returns>
    [TestMethod(DisplayName = "ExecuteInsertGetRowAsync with Null UpdateSettersBuilder setPropertyCalls parameter")]
    public async Task ExecuteInsertGetRowTests_002_Async()
    {
        // ARRANGE
        IQueryable<TestTable1> source = Array.Empty<TestTable1>().AsQueryable();
        Action<UpdateSettersBuilder<TestTable1>> setPropertyCalls = null!;

        // ACT / ASSERT
        await Assert.ThrowsExactlyAsync<ArgumentNullException>(() => source.ExecuteInsertGetRowAsync(setPropertyCalls, this._testContext.CancellationToken), "Unexpected exception").ConfigureAwait(false);
    }

    /// <summary>
    /// Test ExecuteInsertGetRowAsync.
    /// </summary>
    /// <param name="databaseType">Database type.</param>
    /// <param name="useReturningClause">If true update uses returning clause in sql else uses a select after update is completed.</param>
    /// <returns>A <see cref="Task"/> that represents the asynchronous operation.</returns>
    [TestMethod(DisplayName = "ExecuteInsertGetRowAsync")]
    [DataRow(DatabaseTypes.Sqlite, true, DisplayName = $"{DatabaseTypes.Sqlite} Use Returning Clause")]
    [DataRow(DatabaseTypes.Sqlite, false, DisplayName = $"{DatabaseTypes.Sqlite} Use Select Statement")]
    [DataRow(DatabaseTypes.SqlServer, true, DisplayName = $"{DatabaseTypes.SqlServer} Use Returning Clause")]
    [DataRow(DatabaseTypes.SqlServer, false, DisplayName = $"{DatabaseTypes.SqlServer} Use Select Statement")]
    public async Task ExecuteInsertGetRowTests_003_Async(string databaseType, bool useReturningClause)
    {
        // ARRANGE
        using Context context = await DatabaseUtils.OpenDatabaseAsync(
            databaseType: databaseType,
            customConfigurationActions: (optionsBuilder) =>
            {
                // Stop EF core caching the service provider so a new model gets
                // created every time and uses the desired useReturningClause value.
                optionsBuilder.EnableServiceProviderCaching(false);

                optionsBuilder.UseExecuteUpdateExtensions();
            },
            customModelCreationActions: modelBuilder =>
            {
                modelBuilder.Entity<TestTable1>()
                    .ToTable(tableBuilder =>
                    {
                        switch (databaseType)
                        {
                            case DatabaseTypes.Sqlite:
                                tableBuilder.UseSqlReturningClause(useReturningClause);
                                break;

                            case DatabaseTypes.SqlServer:
                                tableBuilder.UseSqlOutputClause(useReturningClause);
                                break;

                            default:
                                throw new UnsupportedDatabaseTypeException();
                        }
                    });
            })
            .ConfigureAwait(false);

        long id = 1;
        string value = "TestValue";

        // ACT
        TestTable1 row = await context.TestTable1.ExecuteInsertGetRowAsync(
            builder =>
            {
                builder.SetProperty(e => e.TestField, value);
            },
            this._testContext.CancellationToken)
            .ConfigureAwait(false);

        // ASSERT
        Assert.IsNull(ExecuteUpdateExtensions.ExecuteUpdateParameters.Value, "Parameters have not been cleared.");

        Assert.IsNotNull(row, "No row returned");
        Assert.AreEqual(id, row.Id, "Unexpected Id value");
        Assert.AreEqual(value, row.TestField, "Unexpected field value");
    }

    #endregion public methods
}
