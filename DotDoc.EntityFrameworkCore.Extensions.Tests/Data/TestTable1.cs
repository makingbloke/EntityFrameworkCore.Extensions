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
[Table("TestTable_1")]
[Index(nameof(TestField1), IsUnique = true)]
public class TestTable1
{
    /// <summary>
    /// Gets or sets the Database Id.
    /// </summary>
    [Key]
    public long Id { get; set; }

    /// <summary>
    /// Gets or sets the Test Field.
    /// </summary>
    [Column("TestField_1")]
    [Required]
    [MaxLength(256)]
    public string TestField1 { get; set; }
}
