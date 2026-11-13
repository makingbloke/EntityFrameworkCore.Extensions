// Copyright ©2021-2026 Mike King.
// This file is licensed to you under the MIT license.
// See the License.txt file in the solution root for more information.

using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;

namespace DotDoc.EntityFrameworkCore.Extensions.ExecuteUpdate;

/// <summary>
/// Execute Update Options Extension.
/// Registers the services used by the Execute Update Extensions in the context's internal service provider.
/// </summary>
internal sealed class ExecuteUpdateOptionsExtension : IDbContextOptionsExtension
{
    #region public constructors

    /// <summary>
    /// Initializes a new instance of the <see cref="ExecuteUpdateOptionsExtension"/> class.
    /// </summary>
    public ExecuteUpdateOptionsExtension()
    {
        this.Info = new ExtensionInfo(this);
    }

    #endregion public constructors

    #region public properties

    /// <inheritdoc/>
    public DbContextOptionsExtensionInfo Info { get; }

    #endregion public properties

    #region public methods

    /// <inheritdoc/>
    public void ApplyServices(IServiceCollection services)
    {
        // ExecuteUpdateState is scoped, so there is one instance per DbContext. It is also registered as an IResettableService
        // that resolves to the same instance, so that EF resets it when a pooled context is returned to the pool.
        services.AddScoped<ExecuteUpdateState>();
        services.AddScoped<IResettableService>(sp => sp.GetRequiredService<ExecuteUpdateState>());
    }

    /// <inheritdoc/>
    public void Validate(IDbContextOptions options)
    {
        // Nothing to validate.
    }

    #endregion public methods

    /// <summary>
    /// Execute Update Options Extension Info.
    /// </summary>
    private sealed class ExtensionInfo : DbContextOptionsExtensionInfo
    {
        #region public constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="ExtensionInfo"/> class.
        /// </summary>
        /// <param name="extension">The extension.</param>
        public ExtensionInfo(IDbContextOptionsExtension extension)
            : base(extension)
        {
        }

        #endregion public constructors

        #region public properties

        /// <inheritdoc/>
        public override bool IsDatabaseProvider => false;

        /// <inheritdoc/>
        public override string LogFragment => "using ExecuteUpdateExtensions ";

        #endregion public properties

        #region public methods

        /// <inheritdoc/>
        public override int GetServiceProviderHashCode() => 0;

        /// <inheritdoc/>
        public override bool ShouldUseSameServiceProvider(DbContextOptionsExtensionInfo other) => other is ExtensionInfo;

        /// <inheritdoc/>
        public override void PopulateDebugInfo(IDictionary<string, string> debugInfo)
        {
            debugInfo["DotDoc:ExecuteUpdateExtensions"] = "1";
        }

        #endregion public methods
    }
}
