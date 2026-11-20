// Copyright ©2021-2026 Mike King.
// This file is licensed to you under the MIT license.
// See the License.txt file in the solution root for more information.

using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Query;

namespace DotDoc.EntityFrameworkCore.Extensions.CaseInsensitivity;

/// <summary>
/// SQLite NoCase Translator Plugin.
/// </summary>
internal sealed class SqlNoCaseTranslatorPlugin : IMethodCallTranslatorPlugin
{
    #region public constructors

    /// <summary>
    /// Initializes a new instance of the <see cref="SqlNoCaseTranslatorPlugin"/> class.
    /// </summary>
    /// <param name="sqlExpressionFactory">The SQL expression factory.</param>
    /// <param name="designTimeModel">The design-time model.</param>
    /// <exception cref="ArgumentNullException"><paramref name="sqlExpressionFactory"/> or <paramref name="designTimeModel"/> is <see langword="null"/>.</exception>
    public SqlNoCaseTranslatorPlugin(ISqlExpressionFactory sqlExpressionFactory, IDesignTimeModel designTimeModel)
    {
        this.Translators = [new SqlNoCaseTranslator(sqlExpressionFactory, designTimeModel)];
    }

    #endregion public constructors

    #region public properties

    /// <inheritdoc/>
    public IEnumerable<IMethodCallTranslator> Translators { get; }

    #endregion public properties
}
