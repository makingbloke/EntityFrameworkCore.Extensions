// Copyright ©2021-2025 Mike King.
// This file is licensed to you under the MIT license.
// See the License.txt file in the solution root for more information.

using System.Collections.ObjectModel;
using System.Linq.Expressions;

namespace DotDoc.EntityFrameworkCore.Extensions.FreeTextSearchFunction;

/// <summary>
/// FreeTextSearch Function Arguments.
/// </summary>
internal sealed class FreeTextArguments
{
    #region public constructors

    /// <summary>
    /// Initializes a new instance of the <see cref="FreeTextArguments"/> class.
    /// </summary>
    /// <param name="expressions">Argument expressions.</param>
    public FreeTextArguments(ReadOnlyCollection<Expression> expressions)
    {
        ArgumentNullException.ThrowIfNull(expressions);

        // Map the arguments to the properties - validating where necessary.
        this.DbFunctions = expressions[0];

        if (expressions[1] is not MemberExpression propertyReference)
        {
            throw new InvalidOperationException("PropertyReference argument is not a property expression");
        }

        this.PropertyReference = propertyReference;

        this.FreeText = expressions[2];

        // Map the optional stemming and language term arguments.
        for (int i = 3; i < expressions.Count; i++)
        {
            ConstantExpression constantExpression = (ConstantExpression)expressions[i]!;

            switch (constantExpression.Value)
            {
                case bool useStemming:
                    this.UseStemming = useStemming;
                    break;

                case int _:
                    this.LanguageTerm = expressions[i];
                    break;

                default:
                    throw new ArgumentException("Unsupported argument type", nameof(expressions));
            }
        }
    }

    #endregion public constructors

    #region public properties

    /// <summary>
    /// Gets the DbFunctions argument.
    /// </summary>
    public Expression DbFunctions { get; }

    /// <summary>
    /// Gets the property reference argument.
    /// </summary>
    public MemberExpression PropertyReference { get; }

    /// <summary>
    /// Gets the free text argument.
    /// </summary>
    public Expression FreeText { get; }

    /// <summary>
    /// Gets a value indicating whether the search will use stemming.
    /// </summary>
    public bool UseStemming { get; }

    /// <summary>
    /// Gets the language term argument.
    /// </summary>
    public Expression? LanguageTerm { get; }

    #endregion public properties
}
