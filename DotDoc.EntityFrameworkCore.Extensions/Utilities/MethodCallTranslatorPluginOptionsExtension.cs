// Copyright ©2021-2026 Mike King.
// This file is licensed to you under the MIT license.
// See the License.txt file in the solution root for more information.

using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace DotDoc.EntityFrameworkCore.Extensions.Utilities;

/// <summary>
/// Method Call Translator Plugin Options Extension.
/// Registers a method call translator plugin in the context's internal service provider.
/// </summary>
/// <typeparam name="TPlugin">The type of the plugin.</typeparam>
/// <remarks>
/// Each plugin type has its own extension type, so registering one plugin doesn't replace another.
/// </remarks>
internal sealed class MethodCallTranslatorPluginOptionsExtension<TPlugin> : IDbContextOptionsExtension
    where TPlugin : class, IMethodCallTranslatorPlugin
{
    #region public constructors

    /// <summary>
    /// Initializes a new instance of the <see cref="MethodCallTranslatorPluginOptionsExtension{TPlugin}"/> class.
    /// </summary>
    public MethodCallTranslatorPluginOptionsExtension()
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
        // EF Core registers method call translator plugins as scoped services that can have more than one registration.
        services.TryAddEnumerable(ServiceDescriptor.Scoped<IMethodCallTranslatorPlugin, TPlugin>());
    }

    /// <inheritdoc/>
    public void Validate(IDbContextOptions options)
    {
        // Nothing to validate.
    }

    #endregion public methods

    #region private ExtensionInfo class

    /// <summary>
    /// Method Call Translator Plugin Options Extension Info.
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
        public override string LogFragment => $"using {typeof(TPlugin).Name} ";

        #endregion public properties

        #region public methods

        /// <inheritdoc/>
        public override int GetServiceProviderHashCode() => 0;

        /// <inheritdoc/>
        public override bool ShouldUseSameServiceProvider(DbContextOptionsExtensionInfo other) => other is ExtensionInfo;

        /// <inheritdoc/>
        public override void PopulateDebugInfo(IDictionary<string, string> debugInfo)
        {
            debugInfo[$"DotDoc:{typeof(TPlugin).Name}"] = "1";
        }

        #endregion public methods
    }

    #endregion private ExtensionInfo class
}
