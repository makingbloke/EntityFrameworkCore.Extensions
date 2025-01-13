// Copyright ©2021-2025 Mike King.
// This file is licensed to you under the MIT license.
// See the License.txt file in the solution root for more information.

using System.ComponentModel.DataAnnotations;

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
    [Key]
    public long Id { get; set; }

    /// <summary>
    /// Gets or sets the Test Field.
    /// </summary>
    [Required]
    [MaxLength(256)]
    public string TestField { get; set; }

    #endregion public properties
}
