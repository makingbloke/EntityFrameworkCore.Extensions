// Copyright ©2021-2025 Mike King.
// This file is licensed to you under the MIT license.
// See the License.txt file in the solution root for more information.

using Microsoft.EntityFrameworkCore.Query.SqlExpressions;
using System.Reflection;

namespace DotDoc.EntityFrameworkCore.Extensions.FreeTextSearchFunction;

/// <summary>
/// FreeTextSearch Function Arguments.
/// </summary>
internal sealed class FreeTextSearchArguments
{
    #region public constructors

    /// <summary>
    /// Initializes a new instance of the <see cref="FreeTextSearchArguments"/> class.
    /// </summary>
    /// <param name="method">The <see cref="MethodInfo"/> of the method being called.</param>
    /// <param name="arguments">Method arguments.</param>
    public FreeTextSearchArguments(MethodInfo method, IReadOnlyList<SqlExpression> arguments)
    {
        ArgumentNullException.ThrowIfNull(method);
        ArgumentNullException.ThrowIfNull(arguments);

        this.DbFunctions = null!;
        this.PropertyReference = null!;
        this.FreeText = null!;
        this.UseStemming = null;
        this.LanguageTerm = null;

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

                case "useStemming":
                    this.UseStemming = (SqlConstantExpression)argument;
                    break;

                case "languageTerm":
                    this.LanguageTerm = (SqlConstantExpression)argument;
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
    /// Gets the property reference argument.
    /// </summary>
    public ColumnExpression PropertyReference { get; }

    /// <summary>
    /// Gets the free text argument.
    /// </summary>
    public SqlExpression FreeText { get; }

    /// <summary>
    /// Gets a value indicating whether the search will use stemming.
    /// </summary>
    public SqlConstantExpression? UseStemming { get; }

    /// <summary>
    /// Gets the language term argument.
    /// </summary>
    public SqlConstantExpression? LanguageTerm { get; }

    #endregion public properties
}
