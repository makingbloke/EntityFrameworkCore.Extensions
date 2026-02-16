// Copyright ©2021-2026 Mike King.
// This file is licensed to you under the MIT license.
// See the License.txt file in the solution root for more information.

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DotDoc.EntityFrameworkCore.Extensions.Tests.Data;

/// <summary>
/// Test Table.
/// </summary>
[Table("GuidTable")]
public class GuidTable
{
    #region public properties

    /// <summary>
    /// Gets or sets the Database Id.
    /// </summary>
    [DatabaseGenerated(DatabaseGeneratedOption.None)]
    [Key]
    public Guid Id { get; set; }

    #endregion public properties
}
