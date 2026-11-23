// Copyright ©2021-2026 Mike King.
// This file is licensed to you under the MIT license.
// See the License.txt file in the solution root for more information.

using Microsoft.EntityFrameworkCore.Storage;

namespace DotDoc.EntityFrameworkCore.Extensions.Tests.Execute;

/// <summary>
/// Retry Once Execution Strategy Factory.
/// Replaces the provider's <see cref="IExecutionStrategyFactory"/> in a test, to create a <see cref="RetryOnceExecutionStrategy"/>.
/// </summary>
internal sealed class RetryOnceExecutionStrategyFactory : IExecutionStrategyFactory
{
    #region private fields

    /// <summary>
    /// The dependencies for the execution strategy.
    /// </summary>
    private readonly ExecutionStrategyDependencies _dependencies;

    #endregion private fields

    #region public constructors

    /// <summary>
    /// Initializes a new instance of the <see cref="RetryOnceExecutionStrategyFactory"/> class.
    /// </summary>
    /// <param name="dependencies">Dependencies.</param>
    public RetryOnceExecutionStrategyFactory(ExecutionStrategyDependencies dependencies)
    {
        this._dependencies = dependencies;
    }

    #endregion public constructors

    #region public methods

    /// <inheritdoc/>
    public IExecutionStrategy Create()
    {
        return new RetryOnceExecutionStrategy(this._dependencies);
    }

    #endregion public methods
}
