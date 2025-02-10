// Copyright ©2021-2025 Mike King.
// This file is licensed to you under the MIT license.
// See the License.txt file in the solution root for more information.

using Microsoft.EntityFrameworkCore.Diagnostics;
using System.Data.Common;
using System.Globalization;
using System.Reflection;

namespace DotDoc.EntityFrameworkCore.Extensions.Interceptors;

/// <summary>
/// SQLite Case Insensitivity Db Connection Interceptor.
/// </summary>
internal sealed class SqliteCaseInsensitivityDbConnectionInterceptor : DbConnectionInterceptor
{
    #region internal fields

    /// <summary>
    /// Singleton instance of this interceptor.
    /// </summary>
    internal static readonly SqliteCaseInsensitivityDbConnectionInterceptor Instance = new();

    #endregion internal fields

    #region private constructors

    /// <summary>
    /// Initializes a new instance of the <see cref="SqliteCaseInsensitivityDbConnectionInterceptor"/> class.
    /// </summary>
    private SqliteCaseInsensitivityDbConnectionInterceptor()
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
        // Check that that connection is a SqliteConnection and it hasn't been wired up incorrectly.
        // We check the typename and use reflection so this package does not have to include the MS SQLite driver.
        Type connectionType = connection.GetType();

        if (connectionType.FullName != "Microsoft.Data.Sqlite.SqliteConnection")
        {
            throw new ArgumentException("Unsupported connection type", nameof(connection));
        }

        // Invoke CreateCollection passing in the UnicodeNoCaseCompare method as a replacement for NOCASE.
        connectionType.InvokeMember(
            name: "CreateCollation",
            invokeAttr: BindingFlags.Instance | BindingFlags.Public | BindingFlags.InvokeMethod,
            binder: null,
            target: connection,
            args: ["NOCASE", (Comparison<string>?)UnicodeNoCaseCompare],
            culture: CultureInfo.InvariantCulture);
    }

    /// <summary>
    /// Compares two specified <see cref="string"/> objects in a case and accent insensitive manner and
    /// returns an integer that indicates their relative position in the sort order.
    /// </summary>
    /// <param name="strA">The first string to compare.</param>
    /// <param name="strB">The first second to compare.</param>
    /// <returns>A 32-bit signed integer that indicates the lexical relationship between the two comparands.</returns>
    private static int UnicodeNoCaseCompare(string? strA, string? strB)
    {
        int result = string.Compare(strA, strB, CultureInfo.InvariantCulture, CompareOptions.IgnoreNonSpace | CompareOptions.IgnoreCase);
        return result;
    }

    #endregion private methods
}
