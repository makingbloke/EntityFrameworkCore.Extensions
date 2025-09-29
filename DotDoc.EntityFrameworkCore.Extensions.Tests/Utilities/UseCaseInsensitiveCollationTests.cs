// Copyright ©2021-2025 Mike King.
// This file is licensed to you under the MIT license.
// See the License.txt file in the solution root for more information.

using DotDoc.EntityFrameworkCore.Extensions.DatabaseType;
using DotDoc.EntityFrameworkCore.Extensions.Tests.Data;
using DotDoc.EntityFrameworkCore.Extensions.Tests.TestUtilities;
using DotDoc.EntityFrameworkCore.Extensions.Utilities;
using Microsoft.EntityFrameworkCore;

namespace DotDoc.EntityFrameworkCore.Extensions.Tests.Utilities;

/// <summary>
/// Tests for the UseCaseInsensitiveCollation extension.
/// </summary>
[TestClass]
public class UseCaseInsensitiveCollationTests
{
    #region public methods

    /// <summary>
    /// Test UseCaseInsensitiveCollation with a Null ModelBuilder parameter.
    /// </summary>
    [TestMethod(DisplayName = "UseCaseInsensitiveCollation with a Null ModelBuilder parameter")]
    public void UseCaseInsensitiveCollationTests_001()
    {
        // ARRANGE
        ModelBuilder modelBuilder = null!;
        string databaseType = DatabaseTypes.Sqlite;

        // ACT / ASSERT
        Assert.ThrowsExactly<ArgumentNullException>(() => _ = modelBuilder.UseCaseInsensitiveCollation(databaseType), "Unexpected exception");
    }

    /// <summary>
    /// Test UseCaseInsensitiveCollation with an unsupported database type.
    /// </summary>
    /// <param name="databaseType">The database type.</param>
    /// <returns>A <see cref="Task"/> that represents the asynchronous operation.</returns>
    [TestMethod(DisplayName = "UseCaseInsensitiveCollation with an unsupported database type")]
    [DataRow(null, DisplayName = "Null")]
    [DataRow("", DisplayName = "Empty")]
    [DataRow("Invalid", DisplayName = "Invalid")]
    public async Task UseCaseInsensitiveCollationTests_002_Async(string databaseType)
    {
        // ARRANGE

        // ACT / ASSERT
        await Assert.ThrowsExactlyAsync<UnsupportedDatabaseTypeException>(
            async () =>
            {
                using Context context = await DatabaseUtils.CreateDatabaseAsync(
                    DatabaseTypes.Sqlite,
                    customModelCreationActions: modelBuilder => modelBuilder.UseCaseInsensitiveCollation(databaseType))
                    .ConfigureAwait(false);
            },
            "Unexpected exception")
            .ConfigureAwait(false);
    }

    /// <summary>
    /// Test UseCaseInsensitiveCollation.
    /// </summary>
    /// <param name="databaseType">The database type.</param>
    /// <returns>A <see cref="Task"/> that represents the asynchronous operation.</returns>
    [TestMethod(DisplayName = "UseCaseInsensitiveCollation")]
    [DataRow(DatabaseTypes.Sqlite, DisplayName = DatabaseTypes.Sqlite)]
    [DataRow(DatabaseTypes.SqlServer, DisplayName = DatabaseTypes.SqlServer)]
    public async Task UseCaseInsensitiveCollationTests_003_Async(string databaseType)
    {
        // ARRANGE / ACT
        using Context context = await DatabaseUtils.CreateDatabaseAsync(
            databaseType,
            customModelCreationActions: modelBuilder => modelBuilder.UseCaseInsensitiveCollation(databaseType))
            .ConfigureAwait(false);

        string originalValue = new('a', 10);
        string searchValue = originalValue.ToUpperInvariant();

        await DatabaseUtils.CreateTestTableEntriesAsync(context, originalValue, 1).ConfigureAwait(false);

        // ASSERT
        List<TestTable1> results = await context.TestTable1
            .Where(e => e.TestField == searchValue)
            .ToListAsync(CancellationToken.None)
            .ConfigureAwait(false);

        Assert.HasCount(1, results, "Invalid record count");
    }

    #endregion public methods
}