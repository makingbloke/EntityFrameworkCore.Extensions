// Copyright ©2021-2026 Mike King.
// This file is licensed to you under the MIT license.
// See the License.txt file in the solution root for more information.

using DotDoc.EntityFrameworkCore.Extensions.Tests.Data;
using System.ComponentModel.DataAnnotations;

namespace DotDoc.EntityFrameworkCore.Extensions.Tests.Model;

/// <summary>
/// Dog, derived from <see cref="Animal"/>.
/// </summary>
/// <remarks>
/// It isn't in <see cref="Context"/>'s model: a test adds it with <c>ExecuteUpdateTestUtils.AddMappedEntities</c> (table-per-hierarchy, so stored
/// in the same table) or <c>ExecuteUpdateTestUtils.AddAnimals</c> (table-per-type or table-per-concrete-type).
/// </remarks>
public class Dog : Animal
{
    #region public properties

    /// <summary>
    /// Gets or sets the Breed.
    /// </summary>
    [MaxLength(256)]
    public string? Breed { get; set; }

    #endregion public properties
}
