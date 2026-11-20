// Copyright ©2021-2026 Mike King.
// This file is licensed to you under the MIT license.
// See the License.txt file in the solution root for more information.

using DotDoc.EntityFrameworkCore.Extensions.DatabaseType;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore.Diagnostics;
using System.Data.Common;

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
    /// <exception cref="UnsupportedDatabaseTypeException"><paramref name="connection"/> is not a SQLite connection.</exception>
    public override void ConnectionOpened(DbConnection connection, ConnectionEndEventData eventData)
    {
        RegisterNoCaseReplacements(connection);
        base.ConnectionOpened(connection, eventData);
    }

    /// <inheritdoc/>
    /// <exception cref="UnsupportedDatabaseTypeException"><paramref name="connection"/> is not a SQLite connection.</exception>
    public override Task ConnectionOpenedAsync(DbConnection connection, ConnectionEndEventData eventData, CancellationToken cancellationToken = default)
    {
        RegisterNoCaseReplacements(connection);
        return base.ConnectionOpenedAsync(connection, eventData, cancellationToken);
    }

    #endregion public methods

    #region private methods

    /// <summary>
    /// Replace SQLite's NOCASE collation and its lower, upper, instr, like, replace and two-argument trim, ltrim and rtrim functions
    /// with Unicode versions (the originals are ASCII only, or case sensitive), and add startswith and endswith.
    /// Each replacement keeps the interface of the SQLite function it replaces (its arguments, its handling of NULL and empty arguments,
    /// its results and its errors), so the SQL EF Core generates for SQLite works unchanged. Only the Unicode processing follows the
    /// SQL Server Latin1_General_100_CI_AS_SC_UTF8 collation: case insensitive but accent sensitive.
    /// The one-argument trim, ltrim and rtrim (which only remove spaces) are left as SQLite's own.
    /// </summary>
    /// <param name="connection">The connection.</param>
    /// <exception cref="UnsupportedDatabaseTypeException"><paramref name="connection"/> is not a SQLite connection.</exception>
    private static void RegisterNoCaseReplacements(DbConnection connection)
    {
        switch (connection)
        {
            case SqliteConnection sqliteConnection:
                sqliteConnection.CreateCollation("NOCASE", SqliteUnicodeMethods.Compare);
                sqliteConnection.CreateFunction<string?, string?>("lower", SqliteUnicodeMethods.Lower, isDeterministic: true);
                sqliteConnection.CreateFunction<string?, string?>("upper", SqliteUnicodeMethods.Upper, isDeterministic: true);
                sqliteConnection.CreateFunction<string?, string?, int?>("instr", SqliteUnicodeMethods.Instr, isDeterministic: true);
                sqliteConnection.CreateFunction<string?, string?, bool?>("like", SqliteUnicodeMethods.Like, isDeterministic: true);
                sqliteConnection.CreateFunction<string?, string?, string?, bool?>("like", SqliteUnicodeMethods.Like, isDeterministic: true);
                sqliteConnection.CreateFunction<string?, string?, string?, string?>("replace", SqliteUnicodeMethods.Replace, isDeterministic: true);
                sqliteConnection.CreateFunction<string?, string?, bool?>("startswith", SqliteUnicodeMethods.StartsWith, isDeterministic: true);
                sqliteConnection.CreateFunction<string?, string?, bool?>("endswith", SqliteUnicodeMethods.EndsWith, isDeterministic: true);
                sqliteConnection.CreateFunction<string?, string?, string?>("trim", SqliteUnicodeMethods.Trim, isDeterministic: true);
                sqliteConnection.CreateFunction<string?, string?, string?>("ltrim", SqliteUnicodeMethods.LTrim, isDeterministic: true);
                sqliteConnection.CreateFunction<string?, string?, string?>("rtrim", SqliteUnicodeMethods.RTrim, isDeterministic: true);

                break;

            default:
                throw new UnsupportedDatabaseTypeException(nameof(connection));
        }
    }

    #endregion private methods
}
