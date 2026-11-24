// Copyright ©2021-2026 Mike King.
// This file is licensed to you under the MIT license.
// See the License.txt file in the solution root for more information.

using DotDoc.EntityFrameworkCore.Extensions.DatabaseType;
using DotDoc.EntityFrameworkCore.Extensions.Tests.Data;
using DotDoc.EntityFrameworkCore.Extensions.Utilities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace DotDoc.EntityFrameworkCore.Extensions.Tests.Utilities;

/// <summary>
/// Tests for CreateGuid extensions.
/// </summary>
[TestClass]
public class CreateGuidTests
{
    #region private fields

    /// <summary>
    /// The MSTest <see cref="TestContext"/> for the current test.
    /// </summary>
    private readonly TestContext _testContext;

    #endregion private fields

    #region public constructors

    /// <summary>
    /// Initializes a new instance of the <see cref="CreateGuidTests"/> class.
    /// </summary>
    /// <param name="testContext">The MSTest test context.</param>
    public CreateGuidTests(TestContext testContext)
    {
        this._testContext = testContext;
    }

    #endregion public constructors

    #region CreateGuid tests

    /// <summary>
    /// Test CreateGuid with Null DatabaseFacade Database parameter.
    /// </summary>
    [TestMethod(DisplayName = "CreateGuid with Null DatabaseFacade Database parameter")]
    public void CreateGuidTests_001()
    {
        // ARRANGE
        DatabaseFacade database = null!;

        // ACT / ASSERT
        Assert.ThrowsExactly<ArgumentNullException>(() => database.CreateGuid(), "Unexpected exception");
    }

    /// <summary>
    /// Test CreateGuid creates a valid Guid.
    /// </summary>
    /// <param name="databaseType">The database type.</param>
    /// <returns>A <see cref="Task"/> that represents the asynchronous operation.</returns>
    [TestMethod(DisplayName = "CreateGuid creates a valid Guid")]
    [DataRow(DatabaseTypes.Sqlite, DisplayName = DatabaseTypes.Sqlite)]
    [DataRow(DatabaseTypes.SqlServer, DisplayName = DatabaseTypes.SqlServer)]
    public async Task CreateGuidTests_002_Async(string databaseType)
    {
        // ARRANGE
        using Context context = await DatabaseUtils.OpenDatabaseAsync(this._testContext, databaseType).ConfigureAwait(false);

        // ACT
        Guid guid = context.Database.CreateGuid();

        // ASSERT
        Assert.AreNotEqual(Guid.Empty, guid, "Unexpected Guid value (Guid.Empty)");
        Assert.AreNotEqual(Guid.AllBitsSet, guid, "Unexpected Guid value (Guid.AllBitsSet)");
    }

    /// <summary>
    /// Test CreateGuid does not duplicate values while creating multiple records on different threads.
    /// </summary>
    /// <param name="databaseType">The database type.</param>
    /// <returns>A <see cref="Task"/> that represents the asynchronous operation.</returns>
    [TestMethod(DisplayName = "CreateGuid does not duplicate values while creating multiple records on different threads")]
    [DataRow(DatabaseTypes.Sqlite, DisplayName = DatabaseTypes.Sqlite)]
    [DataRow(DatabaseTypes.SqlServer, DisplayName = DatabaseTypes.SqlServer)]
    public async Task CreateGuidTests_003_Async(string databaseType)
    {
        // ARRANGE
        int taskCount = 10;
        int recordCount = 1000;

        using Context testContext = await DatabaseUtils.OpenDatabaseAsync(
            this._testContext,
            databaseType)
            .ConfigureAwait(false);

        // ACT
        Task[] tasks = new Task[taskCount];

        for (int i = 0; i < taskCount; i++)
        {
            tasks[i] = Task.Run(CreateRecords, this._testContext.CancellationToken);
        }

        // ASSERT (This will throw if there are any duplicates).
        await Task.WhenAll(tasks).ConfigureAwait(false);

        // Create a batch of records
        async Task CreateRecords()
        {
            using Context createContext = await DatabaseUtils.OpenDatabaseAsync(
                this._testContext,
                databaseType,
                createNew: false)
                .ConfigureAwait(false);

            for (int i = 0; i < recordCount; i++)
            {
                GuidTable guidTable = new()
                {
                    Id = createContext.Database.CreateGuid()
                };

                await createContext.AddAsync(guidTable, this._testContext.CancellationToken).ConfigureAwait(false);
            }

            await createContext.SaveChangesAsync(this._testContext.CancellationToken).ConfigureAwait(false);
        }
    }

    /// <summary>
    /// Test CreateGuid creates GUIDs that the database sorts in the order they were created.
    /// </summary>
    /// <param name="databaseType">The database type.</param>
    /// <returns>A <see cref="Task"/> that represents the asynchronous operation.</returns>
    [TestMethod(DisplayName = "CreateGuid creates GUIDs that the database sorts in the order they were created")]
    [DataRow(DatabaseTypes.Sqlite, DisplayName = DatabaseTypes.Sqlite)]
    [DataRow(DatabaseTypes.SqlServer, DisplayName = DatabaseTypes.SqlServer)]
    public async Task CreateGuidTests_004_Async(string databaseType)
    {
        // ARRANGE
        int recordCount = 10000;

        using Context testContext = await DatabaseUtils.OpenDatabaseAsync(
            this._testContext,
            databaseType)
            .ConfigureAwait(false);

        // ACT
        List<Guid> createdIds = [];

        for (int i = 0; i < recordCount; i++)
        {
            createdIds.Add(testContext.Database.CreateGuid());
        }

        // Insert the rows in reverse, so the insertion order can't give the right answer.
        await testContext.GuidTable.AddRangeAsync(
            createdIds.Select(id => new GuidTable() { Id = id }).Reverse(),
            this._testContext.CancellationToken)
            .ConfigureAwait(false);

        await testContext.SaveChangesAsync(this._testContext.CancellationToken).ConfigureAwait(false);

        // ASSERT
        // The database sorts the GUIDs, so the order they are read back in must be the order they were created in.
        List<Guid> readIds = await testContext.GuidTable
            .OrderBy(r => r.Id)
            .Select(r => r.Id)
            .ToListAsync(this._testContext.CancellationToken)
            .ConfigureAwait(false);

        Assert.AreSequenceEqual(createdIds, readIds, "Guids are not sorted in the order they were created");
    }

    #endregion CreateGuid tests
}
