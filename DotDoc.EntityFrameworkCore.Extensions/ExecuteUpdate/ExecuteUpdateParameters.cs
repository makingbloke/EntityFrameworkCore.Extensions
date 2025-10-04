// Copyright ©2021-2025 Mike King.
// This file is licensed to you under the MIT license.
// See the License.txt file in the solution root for more information.

using System.Data.Common;

namespace DotDoc.EntityFrameworkCore.Extensions.ExecuteUpdate;

/// <summary>
/// Execute Update Query Parameters.
/// </summary>
internal sealed class ExecuteUpdateParameters
{
    #region public constructors

    /// <summary>
    /// Initializes a new instance of the <see cref="ExecuteUpdateParameters"/> class.
    /// </summary>
    /// <param name="queryType">The Query Type.</param>
    public ExecuteUpdateParameters(QueryType queryType)
    {
        this.QueryType = queryType;
        this.Sql = null;
        this.Parameters = null;
    }

    #endregion public constructors

    #region public properties

    /// <summary>
    /// Gets the Query Type.
    /// </summary>
    public QueryType QueryType { get; }

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
    /// Set the Query Result.
    /// </summary>
    /// <param name="sql">The Sql Statement.</param>
    /// <param name="parameters">The Sql Parameters.</param>
    public void SetQuery(string sql, DbParameter[] parameters)
    {
        this.Sql = sql;
        this.Parameters = parameters;
    }

    #endregion public methods
}
