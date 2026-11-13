// Copyright ©2021-2026 Mike King.
// This file is licensed to you under the MIT license.
// See the License.txt file in the solution root for more information.

using DotDoc.EntityFrameworkCore.Extensions.ExecuteUpdate;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace DotDoc.EntityFrameworkCore.Extensions.Tests.ExecuteUpdate;

/// <summary>
/// Shared helpers for the Execute Update tests.
/// </summary>
internal static class ExecuteUpdateTestUtils
{
    #region internal methods

    /// <summary>
    /// Assert that a context's <see cref="ExecuteUpdateState"/> holds no call (as it should once a call has finished).
    /// </summary>
    /// <param name="context">The context.</param>
    internal static void AssertStateCleared(DbContext context)
    {
        ExecuteUpdateState state = context.GetService<ExecuteUpdateState>();

        Assert.AreEqual(QueryType.Unknown, state.QueryType, "The query type has not been cleared.");
        Assert.IsNull(state.Sql, "The SQL has not been cleared.");
        Assert.IsNull(state.Parameters, "The parameters have not been cleared.");
    }

    #endregion internal methods
}
