// Copyright ©2021-2026 Mike King.
// This file is licensed to you under the MIT license.
// See the License.txt file in the solution root for more information.

using Microsoft.Data.SqlClient;
using Microsoft.Data.Sqlite;
using SQLitePCL;

namespace DotDoc.EntityFrameworkCore.Extensions.Utilities;

/// <summary>
/// Entity Framework Core Unique Constraint Extensions.
/// </summary>
public static class UniqueConstraintExtensions
{
    #region private constants

    /// <summary>
    /// SQL Server error number for a duplicate key in a unique index.
    /// </summary>
    private const int SqlServerDuplicateKeyInUniqueIndex = 2601;

    /// <summary>
    /// SQL Server error number for a duplicate key in a primary key or unique constraint.
    /// </summary>
    private const int SqlServerDuplicateKeyInConstraint = 2627;

    #endregion private constants

    #region public methods

    /// <summary>
    /// Checks if an exception has been raised as a result of a unique constraint violation.
    /// </summary>
    /// <param name="ex">The exception to check.</param>
    /// <returns><see langword="true"/> if the exception is a unique constraint violation else, <see langword="false"/>.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="ex"/> is <see langword="null"/>.</exception>
    public static bool IsUniqueConstraintException(this Exception ex)
    {
        ArgumentNullException.ThrowIfNull(ex);

        return ex switch
        {
            // An AggregateException's InnerException is only the first of its InnerExceptions, so check them all.
            AggregateException aggregateEx => aggregateEx.InnerExceptions.Any(IsUniqueConstraintException),
            SqliteException sqliteEx when sqliteEx.SqliteExtendedErrorCode is raw.SQLITE_CONSTRAINT_PRIMARYKEY or raw.SQLITE_CONSTRAINT_UNIQUE => true,
            SqlException sqlEx when sqlEx.Errors.Cast<SqlError>().Any(e => e.Number is SqlServerDuplicateKeyInUniqueIndex or SqlServerDuplicateKeyInConstraint) => true,
            _ => ex.InnerException?.IsUniqueConstraintException() ?? false,
        };
    }

    #endregion public methods
}
