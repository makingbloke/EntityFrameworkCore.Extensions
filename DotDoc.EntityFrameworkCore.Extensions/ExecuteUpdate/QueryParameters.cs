// Copyright ©2021-2025 Mike King.
// This file is licensed to you under the MIT license.
// See the License.txt file in the solution root for more information.

using System.Data.Common;

namespace DotDoc.EntityFrameworkCore.Extensions.ExecuteUpdate;

/// <summary>
/// ExecuteUpdate Query Parameters.
/// </summary>
internal sealed class QueryParameters
{
    #region private members

    private readonly Lock _lock = new Lock();

    #endregion private members

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
    /// Reset the Query Parameters.
    /// </summary>
    /// <param name="queryType">The new Query Type.</param>
    public void Reset(QueryType queryType = QueryType.Unknown)
    {
        lock (this._lock)
        {
            if (this.QueryType != QueryType.Unknown && queryType != QueryType.Unknown)
            {
                throw new InvalidOperationException("Query parameters in use");
            }

            this.QueryType = queryType;
        }

        this.Sql = null;
        this.Parameters = null;
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

    #endregion public methods
}
