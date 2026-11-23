// Copyright ©2021-2026 Mike King.
// This file is licensed to you under the MIT license.
// See the License.txt file in the solution root for more information.

using Microsoft.Data.Sqlite;
using System.Data;
using System.Data.Common;
using System.Diagnostics.CodeAnalysis;

namespace DotDoc.EntityFrameworkCore.Extensions.Tests.CaseInsensitivity;

/// <summary>
/// Wrapped SQLite Connection.
/// A <see cref="DbConnection"/> that passes everything to a <see cref="SqliteConnection"/> but isn't one, as a profiling or tracing
/// wrapper (such as MiniProfiler's ProfiledDbConnection) would be.
/// </summary>
internal sealed class WrappedSqliteConnection : DbConnection
{
    #region private fields

    /// <summary>
    /// The wrapped connection.
    /// </summary>
    private readonly SqliteConnection _connection;

    #endregion private fields

    #region public constructors

    /// <summary>
    /// Initializes a new instance of the <see cref="WrappedSqliteConnection"/> class.
    /// </summary>
    /// <param name="connectionString">The connection string for the wrapped connection.</param>
    public WrappedSqliteConnection(string connectionString)
    {
        this._connection = new SqliteConnection(connectionString);
    }

    #endregion public constructors

    #region public properties

    /// <inheritdoc/>
    [AllowNull]
    public override string ConnectionString
    {
        get => this._connection.ConnectionString;
        set => this._connection.ConnectionString = value;
    }

    /// <inheritdoc/>
    public override string Database => this._connection.Database;

    /// <inheritdoc/>
    public override string DataSource => this._connection.DataSource;

    /// <inheritdoc/>
    public override string ServerVersion => this._connection.ServerVersion;

    /// <inheritdoc/>
    public override ConnectionState State => this._connection.State;

    #endregion public properties

    #region public methods

    /// <inheritdoc/>
    public override void ChangeDatabase(string databaseName)
    {
        this._connection.ChangeDatabase(databaseName);
    }

    /// <inheritdoc/>
    public override void Close()
    {
        this._connection.Close();
    }

    /// <inheritdoc/>
    public override void Open()
    {
        this._connection.Open();
    }

    #endregion public methods

    #region protected methods

    /// <inheritdoc/>
    protected override DbTransaction BeginDbTransaction(IsolationLevel isolationLevel)
    {
        return this._connection.BeginTransaction(isolationLevel);
    }

    /// <inheritdoc/>
    protected override DbCommand CreateDbCommand()
    {
        return this._connection.CreateCommand();
    }

    /// <inheritdoc/>
    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            this._connection.Dispose();
        }

        base.Dispose(disposing);
    }

    #endregion protected methods
}
