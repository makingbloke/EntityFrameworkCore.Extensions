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
        // Only ExecuteUpdate / ExecuteDelete commands can be captured, so the command source is checked first and the state isn't looked up for
        // any other command (migrations, raw SQL...). The state's query type is only set while a ...GetRows method is running, so that command is
        // captured, its SQL stored in the state and its execution suppressed.
        if (!result.HasResult
            && eventData.CommandSource is CommandSource.ExecuteUpdate or CommandSource.ExecuteDelete
            && eventData.Context?.GetInfrastructure().GetService<ExecuteUpdateState>() is { QueryType: not QueryType.Unknown } state)
        {
            state.SetQuery(command.CommandText, command.Parameters.Cast<DbParameter>().ToArray());
            result = InterceptionResult<int>.SuppressWithResult(0);
        }

        return new ValueTask<InterceptionResult<int>>(result);
    }

    #endregion public methods
}