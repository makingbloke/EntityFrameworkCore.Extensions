// Copyright ©2021-2025 Mike King.
// This file is licensed to you under the MIT license.
// See the License.txt file in the solution root for more information.

using System.Runtime.CompilerServices;

namespace DotDoc.EntityFrameworkCore.Extensions.DatabaseType;

/// <summary>
/// Unsupported Database Type Exception.
/// </summary>
public sealed class UnsupportedDatabaseTypeException : ArgumentException
{
    #region private fields

    /// <summary>
    /// Exception message.
    /// </summary>
    private const string ErrorMessage = "Unsupported database type";

    #endregion private fields

    #region public constructors

    /// <summary>
    /// Initializes a new instance of the <see cref="UnsupportedDatabaseTypeException"/> class.
    /// </summary>
    /// <param name="paramName">The name of the parameter that caused the current exception.</param>
    /// <param name="innerException">The exception that is the cause of the current exception.</param>
    public UnsupportedDatabaseTypeException(string? paramName = null, Exception? innerException = null)
        : base(ErrorMessage, paramName, innerException)
    {
    }

    #endregion public constructors

    #region public methods

    /// <summary>
    /// Throws an exception if the database type is not valid.
    /// </summary>
    /// <param name="databaseType">The databaseType to validate.</param>
    /// <param name="paramName">The name of the parameter with which <paramref name="databaseType"/> corresponds.</param>
    public static void ThrowIfInvalidDatabaseType(string? databaseType, [CallerArgumentExpression(nameof(databaseType))] string? paramName = null)
    {
        if (databaseType != DatabaseTypes.Sqlite && databaseType != DatabaseTypes.SqlServer)
        {
            throw new UnsupportedDatabaseTypeException(paramName);
        }
    }

    #endregion public methods
}
