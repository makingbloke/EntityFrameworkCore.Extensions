// Copyright ©2021-2026 Mike King.
// This file is licensed to you under the MIT license.
// See the License.txt file in the solution root for more information.

using DotDoc.EntityFrameworkCore.Extensions.CaseInsensitivity;
using DotDoc.EntityFrameworkCore.Extensions.DatabaseType;
using DotDoc.EntityFrameworkCore.Extensions.Tests.Data;
using Microsoft.EntityFrameworkCore;

namespace DotDoc.EntityFrameworkCore.Extensions.Tests.CaseInsensitivity;

/// <summary>
/// Tests for the UseSqliteNoCaseReplacement extension.
/// </summary>
[TestClass]
public class CaseInsensitivityTests
{
    #region public methods

    /// <summary>
    /// Test UseSqliteNoCaseReplacement with a Null DbContextOptionsBuilder OptionsBuilder parameter.
    /// </summary>
    [TestMethod(DisplayName = "UseSqliteNoCaseReplacement with a Null DbContextOptionsBuilder OptionsBuilder parameter")]
    public void CaseInsensitivityTests_001()
    {
        // ARRANGE
        DbContextOptionsBuilder optionsBuilder = null!;

        // ACT / ASSERT
        Assert.ThrowsExactly<ArgumentNullException>(() => optionsBuilder.UseSqliteNoCaseReplacement(), "Unexpected exception");
    }

    /// <summary>
    /// Test UseSqliteNoCaseReplacement with an unsupported database type.
    /// </summary>
    /// <returns>A <see cref="Task"/> that represents the asynchronous operation.</returns>
    [TestMethod(DisplayName = "UseSqliteNoCaseReplacement with an unsupported database type")]
    public async Task CaseInsensitivityTests_002_Async()
    {
        // ARRANGE

        // ACT / ASSERT
        await Assert.ThrowsExactlyAsync<UnsupportedDatabaseTypeException>(
            async () =>
            {
                using Context context = await DatabaseUtils.OpenDatabaseAsync(
                    DatabaseTypes.SqlServer,
                    customConfigurationActions: optionsBuilder => optionsBuilder.UseSqliteNoCaseReplacement())
                    .ConfigureAwait(false);
            },
            "Unexpected exception")
            .ConfigureAwait(false);
    }

    /// <summary>
    /// Test UseSqliteNoCaseReplacement.
    /// </summary>
    /// <returns>A <see cref="Task"/> that represents the asynchronous operation.</returns>
    [TestMethod(DisplayName = "UseSqliteNoCaseReplacement")]
    public async Task CaseInsensitivityTests_003_Async()
    {
        // ARRANGE / ACT
        using Context context = await DatabaseUtils.OpenDatabaseAsync(
            DatabaseTypes.Sqlite,
            customConfigurationActions: optionsBuilder => optionsBuilder.UseSqliteNoCaseReplacement())
            .ConfigureAwait(false);

        string originalValue = new('\u00E1', 10);       // \u00E1 = Latin Small Letter A with Acute.
        string searchValue = new('A', 10);

        await DatabaseUtils.CreateTestTableEntriesAsync(context, originalValue, 1).ConfigureAwait(false);

        // ASSERT
        List<TestTable1> results = await context.TestTable1
            .Where(e => EF.Functions.Collate(e.TestField, "NOCASE") == searchValue)
            .ToListAsync(CancellationToken.None)
            .ConfigureAwait(false);

        Assert.HasCount(1, results, "Invalid record count");
    }

    #endregion public methods
}