// Copyright ©2021-2026 Mike King.
// This file is licensed to you under the MIT license.
// See the License.txt file in the solution root for more information.

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DotDoc.EntityFrameworkCore.Extensions.Tests.Data;

/// <summary>
/// Free Text With Part, for free text tests where the field searched is in a <see cref="FreeTextPart"/> mapped as an owned type or a complex type.
/// </summary>
/// <remarks>It isn't in <see cref="Context"/>'s model: a test adds it with its custom model creation actions.</remarks>
public class FreeTextWithPart
{
    #region public properties

    /// <summary>
    /// Gets or sets the database Id.
    /// </summary>
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    [Key]
    public long Id { get; set; }

    /// <summary>
    /// Gets or sets the part holding the free text field.
    /// </summary>
    public FreeTextPart Part { get; set; } = new();

    #endregion public properties
}
