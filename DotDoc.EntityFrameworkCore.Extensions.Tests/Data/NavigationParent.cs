// Copyright ©2021-2026 Mike King.
// This file is licensed to you under the MIT license.
// See the License.txt file in the solution root for more information.

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DotDoc.EntityFrameworkCore.Extensions.Tests.Data;

/// <summary>
/// Navigation Parent Table, the parent of <see cref="NavigationChild"/>, for tests that use navigation properties.
/// </summary>
/// <remarks>It isn't in <see cref="Context"/>'s model: a test adds it with its custom model creation actions.</remarks>
[Table("NavigationParent")]
public class NavigationParent
{
    #region public properties

    /// <summary>
    /// Gets or sets the Database Id.
    /// </summary>
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    [Key]
    public long Id { get; set; }

    /// <summary>
    /// Gets or sets the Name.
    /// </summary>
    [MaxLength(256)]
    [Required]
    public required string Name { get; set; }

    /// <summary>
    /// Gets the children (a collection navigation).
    /// </summary>
    public List<NavigationChild> Children { get; } = [];

    #endregion public properties
}
