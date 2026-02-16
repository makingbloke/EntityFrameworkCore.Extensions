// Copyright ©2021-2026 Mike King.
// This file is licensed to you under the MIT license.
// See the License.txt file in the solution root for more information.

using DotDoc.EntityFrameworkCore.Extensions.DatabaseType;
using DotDoc.EntityFrameworkCore.Extensions.MatchFunction;
using DotDoc.EntityFrameworkCore.Extensions.Tests.Data;
using Microsoft.EntityFrameworkCore;

namespace DotDoc.EntityFrameworkCore.Extensions.Tests.MatchFunction;

/// <summary>
/// Tests for the MatchFunction extension.
/// </summary>
[TestClass]
public class MatchFunctionTests
{
    #region public methods

    /// <summary>
    /// Test UseMatchExtensions with a Null DbContextOptionsBuilder OptionsBuilder parameter.
    /// </summary>
    [TestMethod(DisplayName = "UseMatchExtensions with a Null DbContextOptionsBuilder OptionsBuilder parameter")]
    public void MatchFunctionTests_001()
    {
        // ARRANGE
        DbContextOptionsBuilder optionsBuilder = null!;

        // ACT / ASSERT
        Assert.ThrowsExactly<ArgumentNullException>(() => optionsBuilder.UseMatchExtensions(), "Unexpected exception");
    }

    /// <summary>
    /// Test Match.
    /// </summary>
    /// <returns>A <see cref="Task"/> that represents the asynchronous operation.</returns>
    [TestMethod(DisplayName = "Match")]
    public async Task MatchFunctionTests_002_Async()
    {
        // ARRANGE
        using Context context = await DatabaseUtils.OpenDatabaseAsync(
            DatabaseTypes.Sqlite,
            customConfigurationActions: optionsBuilder => optionsBuilder.UseMatchExtensions())
            .ConfigureAwait(false);

        int count = 1;
        string value = "Apple";
        string searchValue = "Apple";

        await DatabaseUtils.CreateTestFreeTextTableEntryAsync(context, value).ConfigureAwait(false);

        // ACT
        List<FreeText> rows = await context.Set<FreeText>(Context.TestFreeTextTableName)
            .Where(e => EF.Functions.Match(searchValue, e.FreeTextField!))
            .ToListAsync(CancellationToken.None)
            .ConfigureAwait(false);

        // ASSERT
        Assert.HasCount(count, rows, "Invalid count");
        Assert.AreEqual(value, rows[0].FreeTextField, "Unexpected field value");
    }

    #endregion public methods
}