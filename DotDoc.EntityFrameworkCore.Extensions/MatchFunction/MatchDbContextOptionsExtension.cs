// Copyright ©2021-2025 Mike King.
// This file is licensed to you under the MIT license.
// See the License.txt file in the solution root for more information.

using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.Extensions.DependencyInjection;

namespace DotDoc.EntityFrameworkCore.Extensions.MatchFunction;

/// <summary>
/// Match Function DB Context Options Extension.
/// </summary>
public sealed class MatchDbContextOptionsExtension : IDbContextOptionsExtension
{
    #region public constructors

    /// <summary>
    /// Initializes a new instance of the <see cref="MatchDbContextOptionsExtension"/> class.
    /// </summary>
    public MatchDbContextOptionsExtension()
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
        services.AddSingleton<IMethodCallTranslatorPlugin, MatchTranslatorPlugin>();
    }

    /// <inheritdoc/>
    public void Validate(IDbContextOptions options)
    {
    }

    #endregion public methods

    /// <inheritdoc/>
    public class ExtensionInfo(IDbContextOptionsExtension extension) : DbContextOptionsExtensionInfo(extension)
    {
        #region public properties

        /// <inheritdoc/>
        public override bool IsDatabaseProvider => false;

        /// <inheritdoc/>
        public override string LogFragment => "'MatchFunctionSupport'=true";

        #endregion public properties

        #region public methods

        /// <inheritdoc/>
        public override int GetServiceProviderHashCode()
        {
            return 0;
        }

        /// <inheritdoc/>
        public override void PopulateDebugInfo(IDictionary<string, string> debugInfo)
        {
        }

        /// <inheritdoc/>
        public override bool ShouldUseSameServiceProvider(DbContextOptionsExtensionInfo other)
        {
            return false;
        }

        #endregion public methods
    }
}
