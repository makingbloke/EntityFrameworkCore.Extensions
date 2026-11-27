// Copyright ©2021-2026 Mike King.
// This file is licensed to you under the MIT license.
// See the License.txt file in the solution root for more information.

using System.ComponentModel.DataAnnotations;

namespace DotDoc.EntityFrameworkCore.Extensions.Tests.Model;

/// <summary>
/// Free Text Part of a <see cref="FreeTextWithPart"/>, mapped as an owned type or a complex type.
/// </summary>
public class FreeTextPart
{
    #region public properties

    /// <summary>
    /// Gets or sets the free text field.
    /// </summary>
    [MaxLength]
    public string? Text { get; set; }

    #endregion public properties
}
