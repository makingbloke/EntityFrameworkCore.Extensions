// Copyright ©2021-2026 Mike King.
// This file is licensed to you under the MIT license.
// See the License.txt file in the solution root for more information.

using DotDoc.EntityFrameworkCore.Extensions.Tests.Data;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DotDoc.EntityFrameworkCore.Extensions.Tests.Model;

/// <summary>
/// Animal, the base type of <see cref="Dog"/>, for tests that use table-per-hierarchy inheritance.
/// </summary>
/// <remarks>It isn't in <see cref="Context"/>'s model: a test adds it with <c>ExecuteUpdateTestUtils.AddMappedEntities</c>.</remarks>
public class Animal
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

    #endregion public properties
}
