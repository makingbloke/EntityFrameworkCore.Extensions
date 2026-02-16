// Copyright ©2021-2026 Mike King.
// This file is licensed to you under the MIT license.
// See the License.txt file in the solution root for more information.

using Microsoft.EntityFrameworkCore.Query.SqlExpressions;
using System.Reflection;

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
    /// <param name="method">The <see cref="MethodInfo"/> of the method being called.</param>
    /// <param name="arguments">Method arguments.</param>
    public MatchArguments(MethodInfo method, IReadOnlyList<SqlExpression> arguments)
    {
        ArgumentNullException.ThrowIfNull(method);
        ArgumentNullException.ThrowIfNull(arguments);

        this.DbFunctions = null!;
        this.PropertyReference = null!;
        this.FreeText = null!;

        foreach (var (parameter, argument) in method.GetParameters().Zip(arguments))
        {
            switch (parameter.Name)
            {
                case "dbFunctions":
                    this.DbFunctions = argument;
                    break;

                case "propertyReference":
                    this.PropertyReference = argument is ColumnExpression propertyReference
                        ? propertyReference
                        : throw new InvalidOperationException("PropertyReference argument is not a column expression");
                    break;

                case "freeText":
                    this.FreeText = argument;
                    break;

                default:
                    throw new InvalidOperationException($"Unsupported argument name: {parameter.Name}");
            }
        }
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
