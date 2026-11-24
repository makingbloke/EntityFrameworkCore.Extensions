// Copyright ©2021-2026 Mike King.
// This file is licensed to you under the MIT license.
// See the License.txt file in the solution root for more information.

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DotDoc.EntityFrameworkCore.Extensions.Tests.Data;

/// <summary>
/// Customer, with an <see cref="Data.Address"/> mapped as an owned type or a complex type.
/// </summary>
/// <remarks>It isn't in <see cref="Context"/>'s model: a test adds it with <c>ExecuteUpdateTestUtils.AddCustomer</c>.</remarks>
public class Customer
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
    /// Gets or sets the Address.
    /// </summary>
    public Address Address { get; set; } = new();

    #endregion public properties
}
