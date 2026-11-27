// Copyright ©2021-2026 Mike King.
// This file is licensed to you under the MIT license.
// See the License.txt file in the solution root for more information.

using DotDoc.EntityFrameworkCore.Extensions.Tests.Data;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DotDoc.EntityFrameworkCore.Extensions.Tests.Model;

/// <summary>
/// Free Text Entity, for tests of a free text table mapped as an ordinary entity type rather than a shared type entity.
/// </summary>
/// <remarks>It isn't in <see cref="Context"/>'s model: a test adds it with its custom model creation actions.</remarks>
public class UnsharedFreeText
{
    #region public properties

    /// <summary>
    /// Gets or sets the database Id.
    /// </summary>
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    [Key]
    public long Id { get; set; }

    /// <summary>
    /// Gets or sets the free text field.
    /// </summary>
    [MaxLength]
    [Required]
    public string? FreeTextField { get; set; }

    #endregion public properties
}
