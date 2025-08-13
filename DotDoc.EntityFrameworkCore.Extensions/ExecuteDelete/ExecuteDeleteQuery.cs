// Copyright ©2021-2025 Mike King.
// This file is licensed to you under the MIT license.
// See the License.txt file in the solution root for more information.

using System.Data.Common;

namespace DotDoc.EntityFrameworkCore.Extensions.ExecuteDelete;

/// <summary>
/// SQL and parameters used in an ExecuteDelete statement.
/// </summary>
/// <param name="sql">The SQL statement.</param>
/// <param name="parameters">The parameters.</param>
internal class ExecuteDeleteQuery(string sql, DbParameter[] parameters)
{
    /// <summary>
    /// Gets the Sql statement.
    /// </summary>
    public string Sql { get; } = sql;

    /// <summary>
    /// Gets the parameters.
    /// </summary>
    public DbParameter[] Parameters { get; } = parameters;
}
