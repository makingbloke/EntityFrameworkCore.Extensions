// Copyright ©2021-2026 Mike King.
// This file is licensed to you under the MIT license.
// See the License.txt file in the solution root for more information.

using DotDoc.EntityFrameworkCore.Extensions.Tests.Data;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DotDoc.EntityFrameworkCore.Extensions.Tests.Model;

/// <summary>
/// Mapped Entity, for tests that need properties mapped differently from their names and types (a column name, a value converter)
/// and a query filter.
/// </summary>
/// <remarks>It isn't in <see cref="Context"/>'s model: a test adds it with <c>ExecuteUpdateTestUtils.AddMappedEntities</c>.</remarks>
public class MappedEntity
{
    #region public properties

    /// <summary>
    /// Gets or sets the Database Id.
    /// </summary>
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    [Key]
    public long Id { get; set; }

    /// <summary>
    /// Gets or sets the Name (mapped to a column with a different name).
    /// </summary>
    [MaxLength(256)]
    [Required]
    public required string Name { get; set; }

    /// <summary>
    /// Gets or sets the Day (stored as a string by a value converter).
    /// </summary>
    public DayOfWeek Day { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the row has been soft deleted (hidden by a query filter).
    /// </summary>
    public bool IsDeleted { get; set; }

    #endregion public properties
}
