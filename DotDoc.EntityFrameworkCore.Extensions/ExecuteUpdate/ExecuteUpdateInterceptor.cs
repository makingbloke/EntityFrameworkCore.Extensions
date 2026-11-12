// Copyright ©2021-2026 Mike King.
// This file is licensed to you under the MIT license.
// See the License.txt file in the solution root for more information.

using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using System.Data.Common;

namespace DotDoc.EntityFrameworkCore.Extensions.ExecuteUpdate;

/// <summary>
/// Interceptor used to capture ExecuteDelete / ExecuteInsert and ExecuteUpdate queries and stop them executing.
/// </summary>
internal sealed class ExecuteUpdateInterceptor : DbCommandInterceptor
{
    #region internal fields

    /// <summary>
    /// Singleton instance of this interceptor.
    /// </summary>
    internal static readonly ExecuteUpdateInterceptor Instance = new();

    #endregion internal fields

    #region public methods

    /// <inheritdoc/>
    public override ValueTask<InterceptionResult<int>> NonQueryExecutingAsync(DbCommand command, CommandEventData eventData, InterceptionResult<int> result, CancellationToken cancellationToken = default)
    {
        ExecuteUpdateState? state = eventData.Context?.GetInfrastructure().GetService<ExecuteUpdateState>();

        if (!result.HasResult && state is not null && StoreQueryResult(state, eventData.CommandSource, command))
        {
            result = InterceptionResult<int>.SuppressWithResult(0);
        }

        return new ValueTask<InterceptionResult<int>>(result);
    }

    #endregion public methods

    #region private methods

    /// <summary>
    /// If this is a query with an ExecuteDelete/Insert/Update tag then intercept and store the SQL and parameters.
    /// </summary>
    /// <param name="state">The execute update state.</param>
    /// <param name="commandSource">The source of the command.</param>
    /// <param name="command">The command.</param>
    /// <returns><see langword="true"/> the SQL was intercepted, <see langword="false"/> otherwise.</returns>
    private static bool StoreQueryResult(ExecuteUpdateState state, CommandSource commandSource, DbCommand command)
    {
        if ((commandSource == CommandSource.ExecuteUpdate && (state.QueryType == QueryType.InsertGetRow || state.QueryType == QueryType.UpdateGetRows)) ||
            (commandSource == CommandSource.ExecuteDelete && state.QueryType == QueryType.DeleteGetRows))
        {
            state.SetQuery(command.CommandText, command.Parameters.Cast<DbParameter>().ToArray());
            return true;
        }

        return false;
    }

    #endregion private methods
}