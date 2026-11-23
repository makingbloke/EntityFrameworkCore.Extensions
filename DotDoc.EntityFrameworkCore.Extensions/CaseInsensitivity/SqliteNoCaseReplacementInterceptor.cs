// Copyright ©2021-2026 Mike King.
// This file is licensed to you under the MIT license.
// See the License.txt file in the solution root for more information.

using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore.Diagnostics;
using System.Data.Common;
using System.Runtime.CompilerServices;

namespace DotDoc.EntityFrameworkCore.Extensions.CaseInsensitivity;

/// <summary>
/// SQLite NoCase Replacement Interceptor.
/// </summary>
internal sealed class SqliteNoCaseReplacementInterceptor : DbCommandInterceptor
{
    #region internal fields

    /// <summary>
    /// Singleton instance of this interceptor.
    /// </summary>
    internal static readonly SqliteNoCaseReplacementInterceptor Instance = new();

    #endregion internal fields

    #region private fields

    /// <summary>
    /// The connections the replacements have been registered on. The table doesn't keep a connection alive.
    /// </summary>
    private static readonly ConditionalWeakTable<SqliteConnection, object> RegisteredConnections = [];

    #endregion private fields

    #region private constructors

    /// <summary>
    /// Initializes a new instance of the <see cref="SqliteNoCaseReplacementInterceptor"/> class.
    /// </summary>
    private SqliteNoCaseReplacementInterceptor()
    {
    }

    #endregion private constructors

    #region public methods

    /// <inheritdoc/>
    /// <exception cref="InvalidOperationException">The command's connection is not a SqliteConnection.</exception>
    public override DbCommand CommandCreated(CommandEndEventData eventData, DbCommand result)
    {
        if (eventData.Connection is not SqliteConnection connection)
        {
            throw new InvalidOperationException(
                $"{nameof(SqliteNoCaseReplacementExtensions.UseSqliteNoCaseReplacement)} needs a {nameof(SqliteConnection)}, but the command's connection is a {eventData.Connection.GetType().Name}.");
        }

        // Register the Unicode collation and functions before the connection's first command runs, rather than when it's opened.
        // EF Core doesn't raise ConnectionOpened for a connection that was already open when it was given to EF Core.
        // Once per connection is enough: Microsoft.Data.Sqlite keeps the collation and functions on the SqliteConnection object,
        // registers them when it's opened if it's closed now, and registers them again each time it's reopened.
        // Only record the connection once everything is registered, so it's tried again if a registration fails.
        if (!RegisteredConnections.TryGetValue(connection, out _))
        {
            connection.UseSqliteNoCaseReplacement();
            RegisteredConnections.TryAdd(connection, new object());
        }

        return base.CommandCreated(eventData, result);
    }

    #endregion public methods
}
