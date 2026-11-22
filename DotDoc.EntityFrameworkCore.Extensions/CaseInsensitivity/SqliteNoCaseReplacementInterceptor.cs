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
    /// Replace SQLite's NOCASE collation and its lower and upper functions with Unicode versions and add the nocase_* functions.
    /// </summary>
    /// <param name="connection">The connection.</param>
    /// <exception cref="UnsupportedDatabaseTypeException"><paramref name="connection"/> is not a SQLite connection.</exception>
    private static void RegisterNoCaseReplacements(DbConnection connection)
    {
        switch (connection)
        {
            case SqliteConnection sqliteConnection:
                sqliteConnection.CreateCollation(SqliteUnicodeFunctionNames.NoCaseCollation, SqliteUnicodeMethods.Compare);
                sqliteConnection.CreateFunction<string?, string?>(SqliteUnicodeFunctionNames.LowerFunction, SqliteUnicodeMethods.Lower, isDeterministic: true);
                sqliteConnection.CreateFunction<string?, string?>(SqliteUnicodeFunctionNames.UpperFunction, SqliteUnicodeMethods.Upper, isDeterministic: true);
                sqliteConnection.CreateFunction<string?, string?, bool?>(SqliteUnicodeFunctionNames.NoCaseEndsWithFunction, SqliteUnicodeMethods.EndsWith, isDeterministic: true);
                sqliteConnection.CreateFunction<string?, string?, int?>(SqliteUnicodeFunctionNames.NoCaseInstrFunction, SqliteUnicodeMethods.Instr, isDeterministic: true);
                sqliteConnection.CreateFunction<string?, string?, int?, int?>(SqliteUnicodeFunctionNames.NoCaseInstrFunction, SqliteUnicodeMethods.Instr, isDeterministic: true);
                sqliteConnection.CreateFunction<string?, string?, bool?>(SqliteUnicodeFunctionNames.NoCaseLikeFunction, SqliteUnicodeMethods.Like, isDeterministic: true);
                sqliteConnection.CreateFunction<string?, string?, string?, bool?>(SqliteUnicodeFunctionNames.NoCaseLikeFunction, SqliteUnicodeMethods.Like, isDeterministic: true);
                sqliteConnection.CreateFunction<string?, string?, string?>(SqliteUnicodeFunctionNames.NoCaseLTrimFunction, SqliteUnicodeMethods.LTrim, isDeterministic: true);
                sqliteConnection.CreateFunction<string?, string?, string?, string?>(SqliteUnicodeFunctionNames.NoCaseReplaceFunction, SqliteUnicodeMethods.Replace, isDeterministic: true);
                sqliteConnection.CreateFunction<string?, string?, string?>(SqliteUnicodeFunctionNames.NoCaseRTrimFunction, SqliteUnicodeMethods.RTrim, isDeterministic: true);
                sqliteConnection.CreateFunction<string?, string?, bool?>(SqliteUnicodeFunctionNames.NoCaseStartsWithFunction, SqliteUnicodeMethods.StartsWith, isDeterministic: true);
                sqliteConnection.CreateFunction<string?, string?, string?>(SqliteUnicodeFunctionNames.NoCaseTrimFunction, SqliteUnicodeMethods.Trim, isDeterministic: true);

                break;

            default:
                throw new UnsupportedDatabaseTypeException(nameof(connection));
        }
    }

    #endregion private methods
}
