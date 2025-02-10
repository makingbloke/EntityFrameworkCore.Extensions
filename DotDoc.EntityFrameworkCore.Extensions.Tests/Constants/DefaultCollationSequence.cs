// Copyright ©2021-2025 Mike King.
// This file is licensed to you under the MIT license.
// See the License.txt file in the solution root for more information.

namespace DotDoc.EntityFrameworkCore.Extensions.Tests.Constants;

/// <summary>
/// Test Database Default Collation Sequence.
/// </summary>
public enum DefaultCollationSequence
{
    /// <summary>
    /// No Collation.
    /// </summary>
    None = 0,

    /// <summary>
    /// SQLite Case Insensitive.
    /// </summary>
    SqliteCaseInsensitive = 1,

    /// <summary>
    /// SQL Server Case Insensitive.
    /// </summary>
    SqlServerCaseInsensitive = 2
}
