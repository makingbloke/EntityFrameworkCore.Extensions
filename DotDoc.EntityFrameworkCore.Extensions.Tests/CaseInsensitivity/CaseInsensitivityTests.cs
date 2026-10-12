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
    /// Test UseSqliteNoCaseReplacement is case insensitive but accent sensitive (matching SQL Server SQL_Latin1_General_CP1_CI_AS).
    /// </summary>
    /// <param name="originalValue">The value stored in the database.</param>
    /// <param name="searchValue">The value to search for.</param>
    /// <param name="expectedCount">The expected number of matching records.</param>
    /// <returns>A <see cref="Task"/> that represents the asynchronous operation.</returns>
    [TestMethod(DisplayName = "UseSqliteNoCaseReplacement")]
    [DataRow("\u00E1\u00E1\u00E1", "\u00C1\u00C1\u00C1", 1, DisplayName = "Accented lower case matches accented upper case")]
    [DataRow("\u00E1\u00E1\u00E1", "AAA", 0, DisplayName = "Accented does not match unaccented")]
    [DataRow("aaa", "\u00C1\u00C1\u00C1", 0, DisplayName = "Unaccented does not match accented")]
    [DataRow("abc", "ABC", 1, DisplayName = "ASCII lower case matches upper case")]
    [DataRow("\u00E6\u00F8\u00E5", "\u00C6\u00D8\u00C5", 1, DisplayName = "Non-ASCII lower case matches upper case")]
    [DataRow("\uFF21\uFF22\uFF23", "abc", 1, DisplayName = "Full width matches half width")]
    public async Task CaseInsensitivityTests_003_Async(string originalValue, string searchValue, int expectedCount)
    {
        // ARRANGE
        using Context context = await DatabaseUtils.OpenDatabaseAsync(
            DatabaseTypes.Sqlite,
            customConfigurationActions: optionsBuilder => optionsBuilder.UseSqliteNoCaseReplacement())
            .ConfigureAwait(false);

        await DatabaseUtils.CreateTestTableEntriesAsync(context, originalValue, 1).ConfigureAwait(false);

        // ACT
        List<TestTable1> results = await context.TestTable1
            .Where(e => EF.Functions.Collate(e.TestField, "NOCASE") == searchValue)
            .ToListAsync(CancellationToken.None)
            .ConfigureAwait(false);

        // ASSERT
        Assert.HasCount(expectedCount, results, "Invalid record count");
    }

    #endregion public methods
}