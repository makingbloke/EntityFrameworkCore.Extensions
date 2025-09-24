// Copyright ©2021-2025 Mike King.
// This file is licensed to you under the MIT license.
// See the License.txt file in the solution root for more information.

using DotDoc.EntityFrameworkCore.Extensions.DatabaseType;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore.Diagnostics;
using System.Data.Common;
using System.Globalization;

namespace DotDoc.EntityFrameworkCore.Extensions.CaseInsensitivity;

/// <summary>
/// SQLite NoCase Replacement Interceptor.
/// </summary>
internal sealed class SqliteNoCaseReplacementInterceptor : DbConnectionInterceptor
{
    #region internal fields

    /// <summary>
    /// Singleton instance of this interceptor.
    /// </summary>
    internal static readonly SqliteNoCaseReplacementInterceptor Instance = new();

    #endregion internal fields

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
    public override void ConnectionOpened(DbConnection connection, ConnectionEndEventData eventData)
    {
        ReplaceNoCaseCollation(connection);
        base.ConnectionOpened(connection, eventData);
    }

    /// <inheritdoc/>
    public override Task ConnectionOpenedAsync(DbConnection connection, ConnectionEndEventData eventData, CancellationToken cancellationToken = default)
    {
        ReplaceNoCaseCollation(connection);
        return base.ConnectionOpenedAsync(connection, eventData, cancellationToken);
    }

    #endregion public methods

    #region private methods

    /// <summary>
    /// Replace the SQLite NoCase collation with a custom one which can handle Unicode
    /// characters and is accent and case insensitive (the original is ASCII only).
    /// </summary>
    /// <param name="connection">The connection.</param>
    private static void ReplaceNoCaseCollation(DbConnection connection)
    {
        if (connection is SqliteConnection sqliteConnection)
        {
            sqliteConnection.CreateCollation("NOCASE", UnicodeNoCaseCompare);
        }
        else
        {
            throw new UnsupportedDatabaseTypeException();
        }
    }

    /// <summary>
    /// Compares two specified strings in a case and accent insensitive manner.
    /// Returns an integer that indicates their relative position in the sort order.
    /// </summary>
    /// <param name="str1">The first string to compare.</param>
    /// <param name="str2">The first second to compare.</param>
    /// <returns>A 32-bit signed integer that indicates the lexical relationship between the two comparands.</returns>
    private static int UnicodeNoCaseCompare(string? str1, string? str2)
    {
        int result = string.Compare(str1, str2, CultureInfo.InvariantCulture, CompareOptions.IgnoreNonSpace | CompareOptions.IgnoreCase);
        return result;
    }

    #endregion private methods
}
