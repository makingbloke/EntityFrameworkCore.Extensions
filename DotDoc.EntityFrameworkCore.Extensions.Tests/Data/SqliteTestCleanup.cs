// Copyright ©2021-2026 Mike King.
// This file is licensed to you under the MIT license.
// See the License.txt file in the solution root for more information.

using Microsoft.Data.Sqlite;
using System.Diagnostics.CodeAnalysis;

namespace DotDoc.EntityFrameworkCore.Extensions.Tests.Data;

/// <summary>
/// Releases the SQLite test database files after each test.
/// </summary>
[TestClass]
[SuppressMessage("Blocker Code Smell", "S2187:Test classes should contain at least one test case", Justification = "MSTest only runs a global test cleanup method in a test class.")]
public static class SqliteTestCleanup
{
    #region test lifecycle

    /// <summary>
    /// Clear the SQLite connection pools after each test, so the test's database file is closed.
    /// </summary>
    /// <param name="testContext">The MSTest test context.</param>
    [GlobalTestCleanup]
    public static void GlobalTestCleanup(TestContext testContext)
    {
        SqliteConnection.ClearAllPools();
    }

    #endregion test lifecycle
}
