// Copyright ©2021-2026 Mike King.
// This file is licensed to you under the MIT license.
// See the License.txt file in the solution root for more information.

using System.ComponentModel.DataAnnotations;

namespace DotDoc.EntityFrameworkCore.Extensions.Tests.Model;

/// <summary>
/// Address of a <see cref="Customer"/>, mapped as an owned type or a complex type.
/// </summary>
public class Address
{
    #region public properties

    /// <summary>
    /// Gets or sets the City.
    /// </summary>
    [MaxLength(256)]
    public string? City { get; set; }

    #endregion public properties
}
