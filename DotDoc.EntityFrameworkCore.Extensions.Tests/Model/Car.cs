// Copyright ©2021-2026 Mike King.
// This file is licensed to you under the MIT license.
// See the License.txt file in the solution root for more information.

using DotDoc.EntityFrameworkCore.Extensions.Tests.Data;

namespace DotDoc.EntityFrameworkCore.Extensions.Tests.Model;

/// <summary>
/// Car, derived from the abstract <see cref="Vehicle"/>.
/// </summary>
/// <remarks>It isn't in <see cref="Context"/>'s model: a test adds it with <c>ExecuteUpdateTestUtils.AddVehicles</c> (table-per-hierarchy).</remarks>
public class Car : Vehicle
{
    #region public properties

    /// <summary>
    /// Gets or sets the number of Doors.
    /// </summary>
    public int Doors { get; set; }

    #endregion public properties
}
