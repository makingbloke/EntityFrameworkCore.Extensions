// Copyright ©2021-2026 Mike King.
// This file is licensed to you under the MIT license.
// See the License.txt file in the solution root for more information.

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DotDoc.EntityFrameworkCore.Extensions.Tests.Data;

/// <summary>
/// Test Table.
/// </summary>
public class TestTable1
{
    #region public properties

    /// <summary>
    /// Gets or sets the Database Id.
    /// </summary>
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    [Key]
    public long Id { get; set; }

    /// <summary>
    /// Gets or sets the Test Field.
    /// </summary>
    [MaxLength(256)]
    [Required]
    public required string TestField { get; set; }

    #endregion public properties
}
