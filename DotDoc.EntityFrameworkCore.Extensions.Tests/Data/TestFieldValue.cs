// Copyright ©2021-2026 Mike King.
// This file is licensed to you under the MIT license.
// See the License.txt file in the solution root for more information.

namespace DotDoc.EntityFrameworkCore.Extensions.Tests.Data;

/// <summary>
/// A single <see cref="TestTable1.TestField"/> value, used to test queries that don't return whole <see cref="TestTable1"/> rows (e.g. DISTINCT, GROUP BY or UNION queries).
/// </summary>
public class TestFieldValue
{
    #region public properties

    /// <summary>
    /// Gets or sets the Test Field.
    /// </summary>
    public required string TestField { get; set; }

    #endregion public properties
}
