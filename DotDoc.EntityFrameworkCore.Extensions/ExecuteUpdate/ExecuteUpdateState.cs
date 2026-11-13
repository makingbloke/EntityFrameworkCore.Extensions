// Copyright ©2021-2026 Mike King.
// This file is licensed to you under the MIT license.
// See the License.txt file in the solution root for more information.

using Microsoft.EntityFrameworkCore.Infrastructure;
using System.Data.Common;

namespace DotDoc.EntityFrameworkCore.Extensions.ExecuteUpdate;

/// <summary>
/// Execute Update State.
/// </summary>
internal sealed class ExecuteUpdateState : IResettableService
{
    #region public constructors

    /// <summary>
    /// Initializes a new instance of the <see cref="ExecuteUpdateState"/> class.
    /// </summary>
    public ExecuteUpdateState()
    {
        this.ResetState();
    }

    #endregion public constructors

    #region public properties

    /// <summary>
    /// Gets the Query Type.
    /// </summary>
    public QueryType QueryType { get; private set; }

    /// <summary>
    /// Gets the Sql Statement.
    /// </summary>
    public string? Sql { get; private set; }

    /// <summary>
    /// Gets the Sql Parameters.
    /// </summary>
    public DbParameter[]? Parameters { get; private set; }

    #endregion public properties

    #region public methods

    /// <summary>
    /// Set the Query Type.
    /// </summary>
    /// <param name="queryType">The Query Type.</param>
    public void SetQueryType(QueryType queryType)
    {
        this.QueryType = queryType;
    }

    /// <summary>
    /// Set the Query Result.
    /// </summary>
    /// <param name="sql">The Sql Statement.</param>
    /// <param name="parameters">The Sql Parameters.</param>
    public void SetQuery(string sql, DbParameter[] parameters)
    {
        this.Sql = sql;
        this.Parameters = parameters;
    }

    /// <inheritdoc/>
    public void ResetState()
    {
        this.QueryType = QueryType.Unknown;
        this.Sql = null!;
        this.Parameters = null!;
    }

    /// <inheritdoc/>
    public Task ResetStateAsync(CancellationToken cancellationToken = default)
    {
        this.ResetState();
        return Task.CompletedTask;
    }

    #endregion public methods
}
