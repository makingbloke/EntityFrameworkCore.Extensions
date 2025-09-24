// Copyright ©2021-2025 Mike King.
// This file is licensed to you under the MIT license.
// See the License.txt file in the solution root for more information.

namespace DotDoc.EntityFrameworkCore.Extensions.Constants;

/// <summary>
/// Annotation Names.
/// </summary>
internal static class AnnotationNames
{
    #region public constants

    /// <summary>
    /// Annotation name prefix.
    /// </summary>
    public const string AnnotationNamePrefix = "DotDoc.";

    /// <summary>
    /// Name of the annotation used to store the SQLite stemming table name.
    /// </summary>
    public const string SqliteStemmingTable = $"{AnnotationNamePrefix}SqliteStemmingTable";

    #endregion public constants
}
