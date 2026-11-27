// Copyright ©2021-2026 Mike King.
// This file is licensed to you under the MIT license.
// See the License.txt file in the solution root for more information.

using DotDoc.EntityFrameworkCore.Extensions.Tests.Data;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DotDoc.EntityFrameworkCore.Extensions.Tests.Model;

/// <summary>
/// Converted Text, for collation tests on properties whose CLR type isn't <see cref="string"/>: values stored through a value converter, and chars.
/// </summary>
/// <remarks>It isn't in <see cref="Context"/>'s model: a test adds it with its custom model creation actions, which set the value converters.</remarks>
public class ConvertedText
{
    #region public properties

    /// <summary>
    /// Gets or sets the database Id.
    /// </summary>
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    [Key]
    public long Id { get; set; }

    /// <summary>
    /// Gets or sets the Code (stored as a string by a value converter).
    /// </summary>
    public TextCode Code { get; set; } = new(string.Empty);

    /// <summary>
    /// Gets or sets the Grade (a char, stored as text).
    /// </summary>
    public char Grade { get; set; }

    /// <summary>
    /// Gets or sets the Nullable Grade (a nullable char, stored as text).
    /// </summary>
    public char? NullableGrade { get; set; }

    /// <summary>
    /// Gets or sets the Grade Number (a char, stored as a number by a value converter).
    /// </summary>
    public char GradeNumber { get; set; }

    /// <summary>
    /// Gets or sets the Day (stored as a string by a value converter).
    /// </summary>
    public DayOfWeek Day { get; set; }

    /// <summary>
    /// Gets or sets the Day Number (stored as a number).
    /// </summary>
    public DayOfWeek DayNumber { get; set; }

    #endregion public properties
}
