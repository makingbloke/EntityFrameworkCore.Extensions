// Copyright ©2021-2025 Mike King.
// This file is licensed to you under the MIT license.
// See the License.txt file in the solution root for more information.

using DotDoc.EntityFrameworkCore.Extensions.Constants;
using DotDoc.EntityFrameworkCore.Extensions.Utilities;
using Microsoft.EntityFrameworkCore.Diagnostics;
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
        if (!result.HasResult && StoreQueryResult(eventData.CommandSource, command))
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
    /// <param name="commandSource">The source of the command.</param>
    /// <param name="command">The command.</param>
    /// <returns><see langword="true"/> the SQL was intercepted, <see langword="false"/> otherwise.</returns>
    private static bool StoreQueryResult(CommandSource commandSource, DbCommand command)
    {
        // There is an issue with EF core 9 where Db Interceptors receive an obsolete value in CommandSource
        // of BulkUpdate(8) instead of the (correct) value ExecuteUpdate(8). Convert the values to int's to
        // handle this: https://github.com/dotnet/efcore/issues/34678.
        if ((int)commandSource == (int)CommandSource.ExecuteUpdate || commandSource == CommandSource.ExecuteDelete)
        {
            TagCollection tagCollection = new(command.CommandText);

            if (tagCollection.TryGetValue(TagNames.ExecuteDelete, out string? value) ||
                tagCollection.TryGetValue(TagNames.ExecuteInsert, out value) ||
                tagCollection.TryGetValue(TagNames.ExecuteUpdate, out value))
            {
                Guid queryId = Guid.Parse(value!);
                string sql = command.CommandText;
                DbParameter[] parameters = command.Parameters.Cast<DbParameter>().ToArray();

                ExecuteUpdateExtensions.StoreQueryResult(queryId, sql, parameters);
                return true;
            }
        }

        return false;
    }

    #endregion private methods
}