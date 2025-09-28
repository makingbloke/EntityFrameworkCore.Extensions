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
/// Tests for ExecuteDeleteGetRows extensions.
/// </summary>
[TestClass]
public class ExecuteDeleteGetRowsTests
{
    #region public methods

    /// <summary>
    /// Test ExecuteDeleteGetRowsAsync with Null IQueryable source parameter.
    /// </summary>
    /// <returns>A <see cref="Task"/> that represents the asynchronous operation.</returns>
    [TestMethod(DisplayName = "ExecuteDeleteGetRowsAsync with Null IQueryable source parameter")]
    public async Task ExecuteDeleteGetRowsTests_001_Async()
    {
        // ARRANGE
        IQueryable<TestTable1> source = null!;

        // ACT / ASSERT
        await Assert.ThrowsExactlyAsync<ArgumentNullException>(() => source.ExecuteDeleteGetRowsAsync(CancellationToken.None), "Unexpected exception").ConfigureAwait(false);
    }

    /// <summary>
    /// Test ExecuteDeleteGetRowsAsync.
    /// </summary>
    /// <param name="databaseType">Database type.</param>
    /// <param name="useReturningClause">If true delete uses returning clause in sql else uses a select after delete is completed.</param>
    /// <param name="count">Number of records to delete.</param>
    /// <returns>A <see cref="Task"/> that represents the asynchronous operation.</returns>
    [TestMethod(DisplayName = "ExecuteDeleteGetRowsAsync")]
    [DataRow(DatabaseTypes.Sqlite, true, 0, DisplayName = $"{DatabaseTypes.Sqlite} Use Returning Clause Delete 0 Records.")]
    [DataRow(DatabaseTypes.Sqlite, false, 0, DisplayName = $"{DatabaseTypes.Sqlite} Use Select Statement Delete 0 Records.")]
    [DataRow(DatabaseTypes.Sqlite, true, 1, DisplayName = $"{DatabaseTypes.Sqlite} Use Returning Clause Delete 1 Record.")]
    [DataRow(DatabaseTypes.Sqlite, false, 1, DisplayName = $"{DatabaseTypes.Sqlite} Use Select Statement Delete 1 Record.")]
    [DataRow(DatabaseTypes.Sqlite, true, 10, DisplayName = $"{DatabaseTypes.Sqlite} Use Returning Clause Delete 10 Records.")]
    [DataRow(DatabaseTypes.Sqlite, false, 10, DisplayName = $"{DatabaseTypes.Sqlite} Use Select Statement Delete 10 Records.")]
    [DataRow(DatabaseTypes.SqlServer, true, 0, DisplayName = $"{DatabaseTypes.SqlServer} Use Returning Clause Delete 0 Records.")]
    [DataRow(DatabaseTypes.SqlServer, false, 0, DisplayName = $"{DatabaseTypes.SqlServer} Use Select Statement Delete 0 Records.")]
    [DataRow(DatabaseTypes.SqlServer, true, 1, DisplayName = $"{DatabaseTypes.SqlServer} Use Returning Clause Delete 1 Record.")]
    [DataRow(DatabaseTypes.SqlServer, false, 1, DisplayName = $"{DatabaseTypes.SqlServer} Use Select Statement Delete 1 Record.")]
    [DataRow(DatabaseTypes.SqlServer, true, 10, DisplayName = $"{DatabaseTypes.SqlServer} Use Returning Clause Delete 10 Records.")]
    [DataRow(DatabaseTypes.SqlServer, false, 10, DisplayName = $"{DatabaseTypes.SqlServer} Use Select Statement Delete 10 Records.")]
    public async Task ExecuteDeleteGetRowsTests_002_Async(string databaseType, bool useReturningClause, int count)
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
            customModelCreationActions: modelBuilder =>
            {
                Action<TableBuilder<TestTable1>> buildAction = databaseType switch
                {
                    DatabaseTypes.Sqlite => tableBuilder => tableBuilder.UseSqlReturningClause(useReturningClause),
                    DatabaseTypes.SqlServer => tableBuilder => tableBuilder.UseSqlOutputClause(useReturningClause),
                    _ => throw new UnsupportedDatabaseTypeException()
                };

                modelBuilder.Entity<TestTable1>()
                    .ToTable(buildAction);
            })
            .ConfigureAwait(false);

        string value = "TestValue";
        await DatabaseUtils.CreateTestTableEntriesAsync(context, value, (count + 1) * 10).ConfigureAwait(false);

        long startId = 2;
        long endId = startId + count - 1;

        IQueryable<TestTable1> source = context.TestTable1.Where(e => e.Id >= startId && e.Id <= endId);

        // ACT
        IList<TestTable1> rows = await source.ExecuteDeleteGetRowsAsync(CancellationToken.None).ConfigureAwait(false);

        // ASSERT
        Assert.HasCount(count, rows, "Invalid count");

        long id = startId;
        foreach (TestTable1 row in rows.OrderBy(r => r.Id))
        {
            Assert.AreEqual(id, row.Id, "Unexpected Id value");
            Assert.AreEqual(value, row.TestField, "Unexpected field value");

            id++;
        }
    }

    #endregion public methods
}
