using Microsoft.EntityFrameworkCore.Utilities;

namespace DotDoc.EntityFrameworkCore.Extensions.Attributes;

/// <summary>
/// Allows the definition of an index that contains an optional filter.
/// </summary>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
public sealed class IndexWithFilterAttribute : Attribute
{
    /// <summary>
    /// Initializes a new instance of the <see cref="IndexWithFilterAttribute"/> class.
    /// </summary>
    /// <param name="propertyNames">The properties which constitute the index, in order (there must be at least one).</param>
    public IndexWithFilterAttribute(params string[] propertyNames)
    {
        if (propertyNames.Length == 0)
        {
            throw new ArgumentException("No property names specified", nameof(propertyNames));
        }

        if (propertyNames.Any(p => string.IsNullOrEmpty(p)))
        {
            throw new ArgumentException("Null or empty property name", nameof(propertyNames));
        }

        this.PropertyNames = propertyNames;
    }

    /// <summary>
    /// Gets the property names which make up the index.
    /// </summary>
    public string[] PropertyNames { get; }

    /// <summary>
    /// Gets or sets the name of the index.
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the index is unique.
    /// </summary>
    public bool IsUnique { get; set; }

    /// <summary>
    /// Gets or sets the index filter.
    /// </summary>
    public string Filter { get; set; }
}
