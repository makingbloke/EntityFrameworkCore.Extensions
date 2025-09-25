// Copyright ©2021-2025 Mike King.
// This file is licensed to you under the MIT license.
// See the License.txt file in the solution root for more information.

using DotDoc.EntityFrameworkCore.Extensions.DatabaseType;
using DotDoc.EntityFrameworkCore.Extensions.MatchFunction;
using DotDoc.EntityFrameworkCore.Extensions.Tests.Data;
using DotDoc.EntityFrameworkCore.Extensions.Tests.TestUtilities;
using Microsoft.EntityFrameworkCore;

namespace DotDoc.EntityFrameworkCore.Extensions.Tests.MatchFunction;

/// <summary>
/// Tests for the MatchFunction extension.
/// </summary>
[TestClass]
public class MatchFunctionTests
{
    #region private fields

    /// <summary>
    /// FreeText table name.
    /// </summary>
    private const string TableName = "TestFreeText";

    #endregion private fields

    #region public methods

    /// <summary>
    /// Test UseMatchExtensions Guard Clause.
    /// </summary>
    [TestMethod(DisplayName = "UseMatchExtensions Guard Clause")]
    public void Test_UseMatchExtensions_GuardClause()
    {
        // ARRANGE
        DbContextOptionsBuilder? optionsBuilder = null;

        // ACT / ASSERT
        ArgumentNullException e = Assert.ThrowsExactly<ArgumentNullException>(() => _ = optionsBuilder!.UseMatchExtensions(), "Unexpected exception");
        Assert.AreEqual(nameof(optionsBuilder), e.ParamName, "Invalid parameter name");
    }

    /// <summary>
    /// Test Match function.
    /// </summary>
    /// <returns>A <see cref="Task"/> that represents the asynchronous operation.</returns>
    [TestMethod(DisplayName = "Match")]
    public async Task Test_Match_Async()
    {
        // ARRANGE
        using Context context = await DatabaseUtils.CreateDatabaseAsync(DatabaseTypes.Sqlite).ConfigureAwait(false);

        int count = 1;
        string value = "Apple";
        string searchValue = "Apple";

        await DatabaseUtils.CreateTestFreeTextTableEntryAsync(context, value).ConfigureAwait(false);

        // ACT
        List<FreeText> rows = await context.Set<FreeText>(TableName)
            .Where(e => EF.Functions.Match(searchValue, e.FreeTextField!))
            .ToListAsync(CancellationToken.None)
            .ConfigureAwait(false);

        // ASSERT
        Assert.HasCount(count, rows, "Invalid count");
        Assert.AreEqual(value, rows[0].FreeTextField, "Unexpected field value");
    }

    #endregion public methods
}