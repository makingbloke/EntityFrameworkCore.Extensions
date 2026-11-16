// Copyright ©2021-2026 Mike King.
// This file is licensed to you under the MIT license.
// See the License.txt file in the solution root for more information.

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;

namespace DotDoc.EntityFrameworkCore.Extensions.Utilities;

/// <summary>
/// Translator Extensions.
/// </summary>
internal static class TranslatorExtensions
{
    #region public methods

    /// <summary>
    /// Register a method call translator plugin in the context's internal service provider.
    /// </summary>
    /// <typeparam name="TPlugin">The type of the plugin.</typeparam>
    /// <param name="optionsBuilder">The builder being used to configure the context.</param>
    /// <returns>The same builder instance so multiple calls can be chained.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="optionsBuilder"/> is <see langword="null"/>.</exception>
    /// <remarks>
    /// Plugins are added to EF Core's own method call translators, which it asks after the plugins, so EF Core's translations
    /// (string methods, Math, DateTime and so on) still work, and plugins from different extensions can be used together.
    /// Replacing <see cref="IMethodCallTranslatorProvider"/> instead would lose EF Core's database specific translators, and only
    /// one replacement can be active. Registering the same plugin more than once has no effect.
    /// </remarks>
    public static DbContextOptionsBuilder AddMethodCallTranslatorPlugin<TPlugin>(this DbContextOptionsBuilder optionsBuilder)
        where TPlugin : class, IMethodCallTranslatorPlugin
    {
        ArgumentNullException.ThrowIfNull(optionsBuilder);

        ((IDbContextOptionsBuilderInfrastructure)optionsBuilder)
            .AddOrUpdateExtension(new MethodCallTranslatorPluginOptionsExtension<TPlugin>());

        return optionsBuilder;
    }

    /// <summary>
    /// Apply the default type mapping to a SQL expression if it does not already have one.
    /// </summary>
    /// <param name="sqlExpressionFactory">The SQL expression factory.</param>
    /// <param name="sqlExpression">The SQL expression.</param>
    /// <returns>The SQL expression with a type mapping.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="sqlExpressionFactory"/> or <paramref name="sqlExpression"/> is <see langword="null"/>.</exception>
    public static SqlExpression ApplyDefaultTypeMappingIfNone(this ISqlExpressionFactory sqlExpressionFactory, SqlExpression sqlExpression)
    {
        ArgumentNullException.ThrowIfNull(sqlExpressionFactory);
        ArgumentNullException.ThrowIfNull(sqlExpression);

        if (sqlExpression.TypeMapping is null)
        {
            sqlExpression = sqlExpressionFactory.ApplyDefaultTypeMapping(sqlExpression);
        }

        return sqlExpression;
    }

    #endregion public methods
}
