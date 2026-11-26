// Copyright ©2021-2026 Mike King.
// This file is licensed to you under the MIT license.
// See the License.txt file in the solution root for more information.

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using System.Runtime.CompilerServices;

namespace DotDoc.EntityFrameworkCore.Extensions.Utilities;

/// <summary>
/// Relational Database Extensions.
/// </summary>
internal static class RelationalDatabaseExtensions
{
    #region internal methods

    /// <summary>
    /// Throws an exception if the database is not relational.
    /// </summary>
    /// <param name="database">The <see cref="DatabaseFacade"/>.</param>
    /// <param name="paramName">The name of the parameter with which <paramref name="database"/> corresponds. If you omit this parameter, the name of <paramref name="database"/> is used.</param>
    /// <exception cref="ArgumentException">The database provider is not relational.</exception>
    internal static void ThrowIfNonRelationalDatabase(this DatabaseFacade database, [CallerArgumentExpression(nameof(database))] string? paramName = default)
    {
        if (!database.IsRelational())
        {
            throw new ArgumentException($"The action is only supported on relational databases ('{database.ProviderName}' is not a relational provider).", paramName);
        }
    }

    #endregion internal methods
}
