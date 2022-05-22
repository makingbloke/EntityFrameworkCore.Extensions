// Copyright ©2021-2022 Mike King.
// This file is licensed to you under the MIT license.
// See the License.txt file in the solution root for more information.

using DotDoc.EntityFrameworkCore.Extensions.Attributes;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System.Reflection;

namespace DotDoc.EntityFrameworkCore.Extensions;

/// <summary>
/// Entity Framework Attribute Exensions.
/// </summary>
public static class AttributeExtensions
{
    /// <summary>
    /// Process any EF Core Extensions attributes in the entities.
    /// </summary>
    /// <remarks>
    /// This should be called from <c>OnModelCreating</c>.
    /// </remarks>
    /// <param name="modelBuilder">The <see cref="ModelBuilder"/> for the context.</param>
    public static void EfCoreExtensionsProcessAttributes(this ModelBuilder modelBuilder)
    {
        foreach (Type clrType in modelBuilder.Model.GetEntityTypes().Select(et => et.ClrType))
        {
            EntityTypeBuilder entityTypeBuilder = modelBuilder.Entity(clrType);

            // Find all the entities with IndexWithFilter attributes and create indexes for them.
            foreach (IndexWithFilterAttribute indexWithFilterAttribute in clrType.GetCustomAttributes<IndexWithFilterAttribute>())
            {
                IndexBuilder indexBuilder = string.IsNullOrEmpty(indexWithFilterAttribute.Name)
                    ? entityTypeBuilder.HasIndex(indexWithFilterAttribute.PropertyNames)
                    : entityTypeBuilder.HasIndex(indexWithFilterAttribute.PropertyNames, indexWithFilterAttribute.Name);

                indexBuilder.IsUnique(indexWithFilterAttribute.IsUnique);
                indexBuilder.HasFilter(string.IsNullOrEmpty(indexWithFilterAttribute.Filter) ? null : indexWithFilterAttribute.Filter);
            }
        }

    }
}
