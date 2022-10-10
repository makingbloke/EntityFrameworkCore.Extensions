// Copyright ©2021-2022 Mike King.
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
    /// <summary>
    /// Create an a test database.
    /// </summary>
    /// <param name="databaseType">The type of database to create.</param>
    /// <param name="useUniqueConstraintInterceptor">If <see langword="true"/> use the UniqueConstraintInterceptor.</param>
    /// <returns>An instance of <see cref="Context"/> for the database.</returns>
    public static Context CreateDatabase(DatabaseType databaseType, bool useUniqueConstraintInterceptor = false)
    {
        Context context;

        switch (databaseType)
        {
            case DatabaseType.Sqlite:
                // For Sqlite use an in memory database. This creates a new instance every time, we just need to open it before we use it.
                context = new (databaseType, "Data Source = :memory:", useUniqueConstraintInterceptor);
                context.Database.OpenConnection();
                context.Database.EnsureCreated();
                break;

            case DatabaseType.SqlServer:
                // For Sql Server create a test database, deleting the previous instance if there was one.
                // I use Sql Server Developer Edition with Windows Authentication to keep things simple.
                context = new (databaseType, "Server=localhost;Database=DotDoc.EntityFrameworkCore.Extensions.Tests;Trusted_Connection=True", useUniqueConstraintInterceptor);
                context.Database.EnsureDeleted();
                context.Database.EnsureCreated();
                break;

            default:
                throw new InvalidOperationException("Unsupported database type");
        }

        return context;
    }

    /// <summary>
    /// Create a record in the TestTable_1 database table.
    /// </summary>
    /// <param name="context">An instance of <see cref="Context"/> for the database.</param>
    /// <param name="value">The value to insert into the TestField1 field.</param>
    /// <returns>The ID of the record.</returns>
    public static long CreateSingleTestTableEntry(Context context, string value)
    {
        TestTable1 testTable1 = new () { TestField1 = value };
        context.Add(testTable1);
        context.SaveChanges();
        return testTable1.Id;
    }

    /// <summary>
    /// Create multiple records in the TestTable_1 database table.
    /// </summary>
    /// <param name="context">An instance of <see cref="Context"/> for the database.</param>
    /// <param name="value">The value to insert into the TestField1 field.</param>
    /// <param name="recordCount">The number of records to create.</param>
    public static void CreateMultipleTestTableEntries(Context context, string value, int recordCount)
    {
        for (int i = 0; i < recordCount; i++)
        {
            TestTable1 testTable1 = new () { TestField1 = $"{value} {i}" };
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
    public static string GetMethodName([CallerMemberName] string methodName = "") => methodName;
}
