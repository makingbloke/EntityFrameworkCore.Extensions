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
    /// Replace SQLite's NOCASE collation with a Unicode version (the original is ASCII only), and add the nocase_* functions:
    /// nocase_lower and nocase_upper (Unicode versions of lower and upper), case insensitive Unicode versions of instr, like, replace
    /// and the two-argument trim, ltrim and rtrim (nocase_instr, nocase_like, nocase_replace, nocase_trim, nocase_ltrim and nocase_rtrim),
    /// plus nocase_startswith and nocase_endswith. They are registered alongside SQLite's built-in functions, which are left unchanged.
    /// Each keeps the interface of the SQLite function it's based on (its arguments, its handling of NULL and empty arguments,
    /// its results and its errors), so it can be used in place of the built-in in the SQL EF Core generates for SQLite.
    /// Only the Unicode processing follows the SQL Server Latin1_General_100_CI_AS_SC_UTF8 collation: case insensitive but accent sensitive.
    /// There are no one-argument nocase_trim, nocase_ltrim and nocase_rtrim, as the built-in versions only remove spaces.
    /// </summary>
    /// <param name="connection">The connection.</param>
    /// <exception cref="UnsupportedDatabaseTypeException"><paramref name="connection"/> is not a SQLite connection.</exception>
    private static void RegisterNoCaseReplacements(DbConnection connection)
    {
        switch (connection)
        {
            case SqliteConnection sqliteConnection:
                sqliteConnection.CreateCollation("NOCASE", SqliteUnicodeMethods.Compare);
                sqliteConnection.CreateFunction<string?, string?>("nocase_lower", SqliteUnicodeMethods.Lower, isDeterministic: true);
                sqliteConnection.CreateFunction<string?, string?>("nocase_upper", SqliteUnicodeMethods.Upper, isDeterministic: true);
                sqliteConnection.CreateFunction<string?, string?, int?>("nocase_instr", SqliteUnicodeMethods.Instr, isDeterministic: true);
                sqliteConnection.CreateFunction<string?, string?, bool?>("nocase_like", SqliteUnicodeMethods.Like, isDeterministic: true);
                sqliteConnection.CreateFunction<string?, string?, string?, bool?>("nocase_like", SqliteUnicodeMethods.Like, isDeterministic: true);
                sqliteConnection.CreateFunction<string?, string?, string?, string?>("nocase_replace", SqliteUnicodeMethods.Replace, isDeterministic: true);
                sqliteConnection.CreateFunction<string?, string?, bool?>("nocase_startswith", SqliteUnicodeMethods.StartsWith, isDeterministic: true);
                sqliteConnection.CreateFunction<string?, string?, bool?>("nocase_endswith", SqliteUnicodeMethods.EndsWith, isDeterministic: true);
                sqliteConnection.CreateFunction<string?, string?, string?>("nocase_trim", SqliteUnicodeMethods.Trim, isDeterministic: true);
                sqliteConnection.CreateFunction<string?, string?, string?>("nocase_ltrim", SqliteUnicodeMethods.LTrim, isDeterministic: true);
                sqliteConnection.CreateFunction<string?, string?, string?>("nocase_rtrim", SqliteUnicodeMethods.RTrim, isDeterministic: true);

                break;

            default:
                throw new UnsupportedDatabaseTypeException(nameof(connection));
        }
    }

    #endregion private methods
}
