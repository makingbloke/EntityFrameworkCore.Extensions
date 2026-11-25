// Copyright ©2021-2026 Mike King.
// This file is licensed to you under the MIT license.
// See the License.txt file in the solution root for more information.

using Microsoft.EntityFrameworkCore.Diagnostics;
using System.Data.Common;

namespace DotDoc.EntityFrameworkCore.Extensions.Tests.ExecuteUpdate;

/// <summary>
/// Commit Failing Transaction Interceptor.
/// Makes committing a transaction fail with a <see cref="TimeoutException"/> before it is committed, to simulate a transient error
/// that an execution strategy can retry. The transaction is then rolled back when it is disposed.
/// </summary>
internal sealed class CommitFailingTransactionInterceptor : DbTransactionInterceptor
{
    #region public properties

    /// <summary>
    /// Gets or sets the number of commits still to fail.
    /// </summary>
    public int CommitFailuresToThrow { get; set; }

    #endregion public properties

    #region public methods

    /// <inheritdoc/>
    /// <exception cref="TimeoutException"><see cref="CommitFailuresToThrow"/> is greater than zero.</exception>
    public override ValueTask<InterceptionResult> TransactionCommittingAsync(DbTransaction transaction, TransactionEventData eventData, InterceptionResult result, CancellationToken cancellationToken = default)
    {
        if (this.CommitFailuresToThrow > 0)
        {
            this.CommitFailuresToThrow--;
            throw new TimeoutException("Simulated transient error.");
        }

        return base.TransactionCommittingAsync(transaction, eventData, result, cancellationToken);
    }

    #endregion public methods
}
