// Copyright ©2021-2026 Mike King.
// This file is licensed to you under the MIT license.
// See the License.txt file in the solution root for more information.

using Microsoft.EntityFrameworkCore.Query;

namespace DotDoc.EntityFrameworkCore.Extensions.FreeTextSearchFunction;

/// <summary>
/// SqlServer FreeTextSearch Function Translator Plugin.
/// </summary>
internal sealed class SqlServerFreeTextSearchTranslatorPlugin : IMethodCallTranslatorPlugin
{
    #region public constructors

    /// <summary>
    /// Initializes a new instance of the <see cref="SqlServerFreeTextSearchTranslatorPlugin"/> class.
    /// </summary>
    /// <param name="sqlExpressionFactory">The SQL expression factory.</param>
    /// <exception cref="ArgumentNullException"><paramref name="sqlExpressionFactory"/> is <see langword="null"/>.</exception>
    public SqlServerFreeTextSearchTranslatorPlugin(ISqlExpressionFactory sqlExpressionFactory)
    {
        this.Translators = [new SqlServerFreeTextSearchTranslator(sqlExpressionFactory)];
    }

    #endregion public constructors

    #region public properties

    /// <inheritdoc/>
    public IEnumerable<IMethodCallTranslator> Translators { get; }

    #endregion public properties
}
