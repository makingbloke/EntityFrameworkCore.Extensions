// Copyright ©2021-2025 Mike King.
// This file is licensed to you under the MIT license.
// See the License.txt file in the solution root for more information.

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DotDoc.EntityFrameworkCore.Extensions.Tests.Data;

/// <summary>
/// Free Text Shared Entity.
/// </summary>
public class FreeText
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
