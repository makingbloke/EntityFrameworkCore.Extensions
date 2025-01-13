// Copyright ©2021-2025 Mike King.
// This file is licensed to you under the MIT license.
// See the License.txt file in the solution root for more information.

namespace DotDoc.EntityFrameworkCore.Extensions.Model;

/// <summary>
/// Unique constraint details.
/// </summary>
public class UniqueConstraintDetails
{
    #region internal constructors

    /// <summary>
    /// Initializes a new instance of the <see cref="UniqueConstraintDetails"/> class.
    /// </summary>
    /// <param name="schema">Constraint schema.</param>
    /// <param name="tableName">Constraint table name.</param>
    /// <param name="fieldNames">Constraint field names.</param>
    internal UniqueConstraintDetails(string schema, string tableName, IReadOnlyList<string> fieldNames)
    {
        this.Schema = schema;
        this.TableName = tableName;
        this.FieldNames = fieldNames;
    }

    #endregion internal constructors

    #region public properties

    /// <summary>
    /// Gets the constraint schema.
    /// </summary>
    public string Schema { get; }

    /// <summary>
    /// Gets the constraint table name.
    /// </summary>
    public string TableName { get; }

    /// <summary>
    /// Gets the constraint field names.
    /// </summary>
    public IReadOnlyList<string> FieldNames { get; }

    #endregion public properties
}
