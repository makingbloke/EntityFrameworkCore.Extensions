// Copyright ©2021-2025 Mike King.
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
    /// <summary>
    /// Test GetDbContext with Null DatabaseFacade Database parameter.
    /// </summary>
    [TestMethod(DisplayName = "GGetDbContext with Null DatabaseFacade Database parameter")]
    public void CreateGuidTests_001()
    {
        // ARRANGE
        DatabaseFacade database = null!;

        // ACT / ASSERT
        Assert.ThrowsExactly<ArgumentNullException>(() => database.GetDbContext(), "Unexpected exception");
    }

    /// <summary>
    /// Test CreateGuidTests.
    /// </summary>
    /// <param name="databaseType">The database type.</param>
    /// <returns>A <see cref="Task"/> that represents the asynchronous operation.</returns>
    [TestMethod(DisplayName = "CreateGuidTests")]
    [DataRow(DatabaseTypes.Sqlite, DisplayName = DatabaseTypes.Sqlite)]
    [DataRow(DatabaseTypes.SqlServer, DisplayName = DatabaseTypes.SqlServer)]
    public async Task CreateGuidTestsTests_002(string databaseType)
    {
        // ARRANGE
        using Context context = await DatabaseUtils.CreateDatabaseAsync(databaseType).ConfigureAwait(false);

        // ACT
        Guid guid = context.Database.CreateGuid();

        // ASSERT
        Assert.AreNotEqual(Guid.Empty, guid, "Unexpected Guid value");
    }
}
