// Copyright ©2021-2026 Mike King.
// This file is licensed to you under the MIT license.
// See the License.txt file in the solution root for more information.

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using System.Data.Common;

namespace DotDoc.EntityFrameworkCore.Extensions.Tests.Execute;

/// <summary>
/// Command Recording Interceptor.
/// Records each command EF Core runs, so a test can check that a command went through EF Core's command pipeline,
/// and can simulate a transient error.
/// </summary>
internal sealed class CommandRecordingInterceptor : DbCommandInterceptor
{
    #region public properties

    /// <summary>
    /// Gets the commands seen, in the order they were run: the SQL, where EF Core says the command came from, and the context it was run for.
    /// </summary>
    public List<(string CommandText, CommandSource CommandSource, DbContext? Context)> Commands { get; } = [];

    /// <summary>
    /// Gets or sets the number of scalar commands still to fail with a <see cref="TimeoutException"/> before they run,
    /// to simulate a transient error that an execution strategy can retry.
    /// </summary>
    public int ScalarFailuresToThrow { get; set; }

    #endregion public properties

    #region public methods

    /// <inheritdoc/>
    /// <exception cref="TimeoutException"><see cref="ScalarFailuresToThrow"/> is greater than zero.</exception>
    public override ValueTask<InterceptionResult<object>> ScalarExecutingAsync(DbCommand command, CommandEventData eventData, InterceptionResult<object> result, CancellationToken cancellationToken = default)
    {
        this.Record(command, eventData);

        if (this.ScalarFailuresToThrow > 0)
        {
            this.ScalarFailuresToThrow--;
            throw new TimeoutException("Simulated transient error.");
        }

        return base.ScalarExecutingAsync(command, eventData, result, cancellationToken);
    }

    /// <inheritdoc/>
    public override ValueTask<InterceptionResult<DbDataReader>> ReaderExecutingAsync(DbCommand command, CommandEventData eventData, InterceptionResult<DbDataReader> result, CancellationToken cancellationToken = default)
    {
        this.Record(command, eventData);
        return base.ReaderExecutingAsync(command, eventData, result, cancellationToken);
    }

    /// <inheritdoc/>
    public override ValueTask<InterceptionResult<int>> NonQueryExecutingAsync(DbCommand command, CommandEventData eventData, InterceptionResult<int> result, CancellationToken cancellationToken = default)
    {
        this.Record(command, eventData);
        return base.NonQueryExecutingAsync(command, eventData, result, cancellationToken);
    }

    #endregion public methods

    #region private methods

    /// <summary>
    /// Records a command.
    /// </summary>
    /// <param name="command">The command.</param>
    /// <param name="eventData">The event data.</param>
    private void Record(DbCommand command, CommandEventData eventData)
    {
        this.Commands.Add((command.CommandText, eventData.CommandSource, eventData.Context));
    }

    #endregion private methods
}
