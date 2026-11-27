// Copyright ©2021-2026 Mike King.
// This file is licensed to you under the MIT license.
// See the License.txt file in the solution root for more information.

using System.ComponentModel.DataAnnotations;

namespace DotDoc.EntityFrameworkCore.Extensions.Tests.Model;

/// <summary>
/// Collation Part of a <see cref="CollationOwner"/>, mapped as a complex type, with a <see cref="CollationSubPart"/> mapped as a nested complex type.
/// </summary>
public class CollationPart
{
    #region public properties

    /// <summary>
    /// Gets or sets the text.
    /// </summary>
    [MaxLength(256)]
    public string? Text { get; set; }

    /// <summary>
    /// Gets or sets the sub part, mapped as a nested complex type.
    /// </summary>
    public CollationSubPart SubPart { get; set; } = new();

    #endregion public properties
}
