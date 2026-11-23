// Copyright ©2021-2026 Mike King.
// This file is licensed to you under the MIT license.
// See the License.txt file in the solution root for more information.

using Microsoft.EntityFrameworkCore.Storage;

namespace DotDoc.EntityFrameworkCore.Extensions.Tests.Execute;

/// <summary>
/// Retry Once Execution Strategy.
/// An execution strategy that retries an operation once, straight away, if it fails with a <see cref="TimeoutException"/>
/// (the transient error <see cref="CommandRecordingInterceptor"/> simulates).
/// </summary>
internal sealed class RetryOnceExecutionStrategy : ExecutionStrategy
{
    #region public constructors

    /// <summary>
    /// Initializes a new instance of the <see cref="RetryOnceExecutionStrategy"/> class.
    /// </summary>
    /// <param name="dependencies">Dependencies.</param>
    public RetryOnceExecutionStrategy(ExecutionStrategyDependencies dependencies)
        : base(dependencies, maxRetryCount: 1, maxRetryDelay: TimeSpan.Zero)
    {
    }

    #endregion public constructors

    #region protected methods

    /// <inheritdoc/>
    protected override bool ShouldRetryOn(Exception exception)
    {
        return exception is TimeoutException;
    }

    #endregion protected methods
}
