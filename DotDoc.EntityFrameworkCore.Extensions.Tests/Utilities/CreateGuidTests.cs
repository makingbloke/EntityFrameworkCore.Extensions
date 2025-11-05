// Copyright ©2021-2025 Mike King.
// This file is licensed to you under the MIT license.
// See the License.txt file in the solution root for more information.

using DotDoc.EntityFrameworkCore.Extensions.DatabaseType;
using DotDoc.EntityFrameworkCore.Extensions.Tests.Data;
using DotDoc.EntityFrameworkCore.Extensions.Utilities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using System.Data.SqlTypes;

namespace DotDoc.EntityFrameworkCore.Extensions.Tests.Utilities;

/// <summary>
/// Tests for CreateGuid extensions.
/// </summary>
[TestClass]
public class CreateGuidTests
{
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
    /// Test CreateGuidTests creates a valid Guid.
    /// </summary>
    /// <param name="databaseType">The database type.</param>
    /// <returns>A <see cref="Task"/> that represents the asynchronous operation.</returns>
    [TestMethod(DisplayName = "CreateGuid creates a valid guid")]
    [DataRow(DatabaseTypes.Sqlite, DisplayName = DatabaseTypes.Sqlite)]
    [DataRow(DatabaseTypes.SqlServer, DisplayName = DatabaseTypes.SqlServer)]
    public async Task CreateGuidTestsTests_002(string databaseType)
    {
        // ARRANGE
        using Context context = await DatabaseUtils.OpenDatabaseAsync(databaseType).ConfigureAwait(false);

        // ACT
        Guid guid = context.Database.CreateGuid();

        // ASSERT
        Assert.AreNotEqual(Guid.Empty, guid, "Unexpected Guid value (Guid.Empty)");
        Assert.AreNotEqual(Guid.AllBitsSet, guid, "Unexpected Guid value (Guid.AllBitsSet)");
    }

    /// <summary>
    /// Test CreateGuidTests does not create a duplicate over a reasonable range of records and the results are in the correct order.
    /// </summary>
    /// <param name="databaseType">The database type.</param>
    /// <returns>A <see cref="Task"/> that represents the asynchronous operation.</returns>
    [TestMethod(DisplayName = "CreateGuidTests")]
    [DataRow(DatabaseTypes.Sqlite, DisplayName = DatabaseTypes.Sqlite)]
    [DataRow(DatabaseTypes.SqlServer, DisplayName = DatabaseTypes.SqlServer)]
    public async Task CreateGuidTestsTests_003(string databaseType)
    {
        // ARRANGE
        int taskCount = 10;
        int recordCount = 1000;

        using Context testContext = await DatabaseUtils.OpenDatabaseAsync(
            databaseType)
            .ConfigureAwait(false);

        // ACT
        Task[] tasks = new Task[taskCount];

        for (int i = 0; i < taskCount; i++)
        {
            tasks[i] = Task.Run(CreateRecords, CancellationToken.None);
        }

        await Task.WhenAll(tasks).ConfigureAwait(false);

        // ASSERT
        List<GuidTable> guidRows = await testContext.GuidTable
            .OrderBy(r => r.Id)
            .ToListAsync(CancellationToken.None)
            .ConfigureAwait(false);

        Assert.HasCount(taskCount * recordCount, guidRows, "Unexpected number of records");

        for (int i = 0; i < guidRows.Count - 1; i++)
        {
            // In Sqlite Guid comparison operators work as expected.
            // In SqlServer the Guid is stored differently so we need to cast to SqlGuid for comparison.
            Guid currentId = guidRows[i].Id;
            Guid nextId = guidRows[i + 1].Id;

            bool orderCorrect = databaseType == DatabaseTypes.Sqlite
                ? currentId < nextId
                : ((SqlGuid)currentId).CompareTo((SqlGuid)nextId) < 0;

            Assert.IsTrue(orderCorrect, $"Guids are not in order at record index: {i}");
        }

        async Task CreateRecords()
        {
            using Context createContext = await DatabaseUtils.OpenDatabaseAsync(
                databaseType,
                forceCreate: false)
                .ConfigureAwait(false);

            for (int i = 0; i < recordCount; i++)
            {
                GuidTable guidTable = new()
                {
                    Id = createContext.Database.CreateGuid()
                };

                await createContext.AddAsync(guidTable, CancellationToken.None).ConfigureAwait(false);
            }

            await createContext.SaveChangesAsync(CancellationToken.None).ConfigureAwait(false);
        }
    }
}
