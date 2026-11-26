// Copyright ©2021-2026 Mike King.
// This file is licensed to you under the MIT license.
// See the License.txt file in the solution root for more information.

using System.ComponentModel.DataAnnotations;

namespace DotDoc.EntityFrameworkCore.Extensions.Tests.Data;

/// <summary>
/// Collation Sub Part of a <see cref="CollationPart"/>, mapped as a nested complex type.
/// </summary>
public class CollationSubPart
{
    #region public properties

    /// <summary>
    /// Gets or sets the text.
    /// </summary>
    [MaxLength(256)]
    public string? Text { get; set; }

    #endregion public properties
}
