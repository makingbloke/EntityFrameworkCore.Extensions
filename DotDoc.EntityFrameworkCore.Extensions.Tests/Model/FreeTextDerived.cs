// Copyright ©2021-2026 Mike King.
// This file is licensed to you under the MIT license.
// See the License.txt file in the solution root for more information.

using DotDoc.EntityFrameworkCore.Extensions.Tests.Data;
using System.ComponentModel.DataAnnotations;

namespace DotDoc.EntityFrameworkCore.Extensions.Tests.Model;

/// <summary>
/// Free Text Derived, derived from <see cref="FreeTextBase"/> (table-per-hierarchy inheritance, so stored in the same table).
/// </summary>
/// <remarks>It isn't in <see cref="Context"/>'s model: a test adds it with its custom model creation actions.</remarks>
public class FreeTextDerived : FreeTextBase
{
    #region public properties

    /// <summary>
    /// Gets or sets the free text field declared on the derived type.
    /// </summary>
    [MaxLength]
    public string? DerivedField { get; set; }

    #endregion public properties
}
