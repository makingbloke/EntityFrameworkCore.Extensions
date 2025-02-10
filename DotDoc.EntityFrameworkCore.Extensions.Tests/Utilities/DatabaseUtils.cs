// Copyright ©2021-2025 Mike King.
// This file is licensed to you under the MIT license.
// See the License.txt file in the solution root for more information.

using DotDoc.EntityFrameworkCore.Extensions.Constants;
using DotDoc.EntityFrameworkCore.Extensions.Tests.Constants;
using DotDoc.EntityFrameworkCore.Extensions.Tests.Data;
using Microsoft.EntityFrameworkCore;

namespace DotDoc.EntityFrameworkCore.Extensions.Tests.Utilities;

/// <summary>
/// Database Utilities.
/// </summary>
public static class DatabaseUtils
{
    #region public methods

    /// <summary>
    /// Create an a test database.
    /// </summary>
    /// <param name="databaseType">The type of database to create.</param>
    /// <param name="useExecuteUpdateInterceptor">If <see langword="true"/> use the Execute Update Interceptor.</param>
    /// <param name="useSqliteCaseInsensitivityInterceptor">If <see langword="true"/> use the SQLite Case Insensitivity Interceptor.</param>
    /// <param name="useUniqueConstraintInterceptor">If <see langword="true"/> use the Unique Constraint Interceptor.</param>
    /// <param name="collation">Default Collation Sequence.</param>
    /// <returns>An instance of <see cref="Context"/> for the database.</returns>
    public static Context CreateDatabase(
        string databaseType,
        bool useExecuteUpdateInterceptor = false,
        bool useSqliteCaseInsensitivityInterceptor = false,
        bool useUniqueConstraintInterceptor = false,
        DefaultCollationSequence collation = DefaultCollationSequence.None)
    {
        Context context;

        switch (databaseType)
        {
            case DatabaseType.Sqlite:
                // For Sqlite use an in memory database. This creates a new instance every time, we just need to open it before we use it.
                context = new(
                    databaseType,
                    "Data Source = :memory:",
                    useExecuteUpdateInterceptor,
                    useSqliteCaseInsensitivityInterceptor,
                    useUniqueConstraintInterceptor,
                    collation);

                context.Database.OpenConnection();
                context.Database.EnsureCreated();
                break;

            case DatabaseType.SqlServer:
                // For Sql Server create a test database, deleting the previous instance if there was one.
                // I use Sql Server Developer Edition with Windows Authentication to keep things simple.
                context = new(
                    databaseType,
                    "Server=localhost;Initial Catalog=DotDoc.EntityFrameworkCore.Extensions.Tests;Trusted_Connection=True;TrustServerCertificate=True",
                    useExecuteUpdateInterceptor,
                    useSqliteCaseInsensitivityInterceptor,
                    useUniqueConstraintInterceptor,
                    collation);

                context.Database.EnsureDeleted();
                context.Database.EnsureCreated();
                break;

            default:
                throw new ArgumentException("Unsupported database type", nameof(databaseType));
        }

        return context;
    }

    /// <summary>
    /// Get the default schema name for a database.
    /// </summary>
    /// <param name="databaseType">The type of database.</param>
    /// <returns>The schema name.</returns>
    public static string? GetDefaultSchema(string databaseType)
    {
        string? schema = databaseType == DatabaseType.SqlServer
            ? "dbo"
            : null;

        return schema;
    }

    /// <summary>
    /// Create multiple records in the TestTable1 database table.
    /// </summary>
    /// <param name="context">The database context.</param>
    /// <param name="value">The value to insert into the TestField field.</param>
    /// <param name="count">The number of records to create.</param>
    public static void CreateTestTableEntries(Context context, string value, int count)
    {
        for (int i = 0; i < count; i++)
        {
            TestTable1 testTable1 = new() { TestField = value };
            context.Add(testTable1);
        }

        context.SaveChanges();
    }

    #endregion public methods
}
