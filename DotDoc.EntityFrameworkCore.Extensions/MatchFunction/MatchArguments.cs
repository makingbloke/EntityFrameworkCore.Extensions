// Copyright ©2021-2025 Mike King.
// This file is licensed to you under the MIT license.
// See the License.txt file in the solution root for more information.

using Microsoft.EntityFrameworkCore.Query.SqlExpressions;

namespace DotDoc.EntityFrameworkCore.Extensions.MatchFunction;

/// <summary>
/// Match Function Arguments.
/// </summary>
internal sealed class MatchArguments
{
    #region public constructors

    /// <summary>
    /// Initializes a new instance of the <see cref="MatchArguments"/> class.
    /// </summary>
    /// <param name="expressions">Argument expressions.</param>
    public MatchArguments(IReadOnlyList<SqlExpression> expressions)
    {
        ArgumentNullException.ThrowIfNull(expressions);

        // Map the arguments to the properties - validating where necessary.
        this.DbFunctions = expressions[0];
        this.FreeText = expressions[1];

        if (expressions[2] is not ColumnExpression propertyReference)
        {
            throw new InvalidOperationException("PropertyReference argument is not a column expression");
        }

        this.PropertyReference = propertyReference;
    }

    #endregion public constructors

    #region public properties

    /// <summary>
    /// Gets the DbFunctions argument.
    /// </summary>
    public SqlExpression DbFunctions { get; }

    /// <summary>
    /// Gets the free text argument.
    /// </summary>
    public SqlExpression FreeText { get; }

    /// <summary>
    /// Gets the property reference argument.
    /// </summary>
    public ColumnExpression PropertyReference { get; }

    #endregion public properties
}
