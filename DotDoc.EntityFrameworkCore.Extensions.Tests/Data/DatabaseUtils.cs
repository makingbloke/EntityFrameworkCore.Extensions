// Copyright ©2021-2025 Mike King.
// This file is licensed to you under the MIT license.
// See the License.txt file in the solution root for more information.

using DotDoc.EntityFrameworkCore.Extensions.Constants;
using Microsoft.EntityFrameworkCore;
using System.Runtime.CompilerServices;

namespace DotDoc.EntityFrameworkCore.Extensions.Tests.Data;

/// <summary>
/// Test Database Utilities.
/// </summary>
public static class DatabaseUtils
{
    #region public methods

    /// <summary>
    /// Create an a test database.
    /// </summary>
    /// <param name="databaseType">The type of database to create.</param>
    /// <param name="useUniqueConstraintInterceptor">If <see langword="true"/> use the Unique Constraint Interceptor.</param>
    /// <param name="useExecuteUpdateExtensions">If <see langword="true"/> use the Execute Update Extensions.</param>
    /// <returns>An instance of <see cref="Context"/> for the database.</returns>
    public static Context CreateDatabase(string databaseType, bool useUniqueConstraintInterceptor = false, bool useExecuteUpdateExtensions = false)
    {
        Context context;

        switch (databaseType)
        {
            case DatabaseType.Sqlite:
                // For Sqlite use an in memory database. This creates a new instance every time, we just need to open it before we use it.
                context = new(databaseType, "Data Source = :memory:", useUniqueConstraintInterceptor, useExecuteUpdateExtensions);
                context.Database.OpenConnection();
                context.Database.EnsureCreated();
                break;

            case DatabaseType.SqlServer:
                // For Sql Server create a test database, deleting the previous instance if there was one.
                // I use Sql Server Developer Edition with Windows Authentication to keep things simple.
                context = new(databaseType, "Server=localhost;Initial Catalog=DotDoc.EntityFrameworkCore.Extensions.Tests;Trusted_Connection=True;TrustServerCertificate=True", useUniqueConstraintInterceptor, useExecuteUpdateExtensions);
                context.Database.EnsureDeleted();
                context.Database.EnsureCreated();
                break;

            default:
                throw new InvalidOperationException("Unsupported database type");
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
    /// Create a record in the TestTable1 database table.
    /// </summary>
    /// <param name="context">The database context.</param>
    /// <param name="value">The value to insert into the TestField field.</param>
    /// <returns>The ID of the record.</returns>
    public static long CreateSingleTestTableEntry(Context context, string value)
    {
        TestTable1 testTable1 = new() { TestField = value };
        context.Add(testTable1);
        context.SaveChanges();
        return testTable1.Id;
    }

    /// <summary>
    /// Create multiple records in the TestTable1 database table.
    /// </summary>
    /// <param name="context">The database context.</param>
    /// <param name="value">The value to insert into the TestField field.</param>
    /// <param name="recordCount">The number of records to create.</param>
    public static void CreateMultipleTestTableEntries(Context context, string value, int recordCount)
    {
        for (int i = 0; i < recordCount; i++)
        {
            TestTable1 testTable1 = new() { TestField = $"{value} {i}" };
            context.Add(testTable1);
        }

        context.SaveChanges();
    }

    /// <summary>
    /// Returns the name of a calling method.
    /// (Using this instead of nameof means if the name of the method changes it doesn't break the compilation).
    /// </summary>
    /// <param name="methodName">Calling method name.</param>
    /// <returns>Method name.</returns>
    public static string GetMethodName([CallerMemberName] string methodName = "")
    {
        return methodName;
    }

    #endregion public methods
}
