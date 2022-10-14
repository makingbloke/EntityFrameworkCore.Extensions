// Copyright ©2021-2022 Mike King.
// This file is licensed to you under the MIT license.
// See the License.txt file in the solution root for more information.

using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DotDoc.EntityFrameworkCore.Extensions.Tests.Data;

/// <summary>
/// Test Table.
/// </summary>
[Index(nameof(TestField), IsUnique = true)]
[Table("TestTable2RealName")]
public class TestTable2
{
    /// <summary>
    /// Gets or sets the Database Id.
    /// </summary>
    [Key]
    public long Id { get; set; }

    /// <summary>
    /// Gets or sets the Test Field.
    /// </summary>
    [Column("TestFieldRealName")]
    [Required]
    [MaxLength(256)]
    public string TestField { get; set; }
}
