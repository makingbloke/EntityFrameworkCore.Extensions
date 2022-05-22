// Copyright ©2021-2022 Mike King.
// This file is licensed to you under the MIT license.
// See the License.txt file in the solution root for more information.

using DotDoc.EntityFrameworkCore.Extensions.Constants;
using Microsoft.EntityFrameworkCore;

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
    /// <returns>An instance of <see cref="Context"/> for the database.</returns>
    public static Context CreateDatabase(DatabaseType databaseType)
    {
        Context context;

        switch (databaseType)
        {
            case DatabaseType.Sqlite:
                // For Sqlite use an in memory database. This creates a new instance every time, we just need to open it before we use it.
                context = new (databaseType, "Data Source = :memory:");
                context.Database.OpenConnection();
                context.Database.EnsureCreated();
                break;

            case DatabaseType.SqlServer:
                // For Sql Server create a test database, deleting the previous instance if there was one.
                // I use Sql Server Developer Edition with Windows Authentication to keep things simple.
                context = new (databaseType, "Server=localhost;Database=DotDoc.EntityFrameworkCore.Extensions.Tests;Trusted_Connection=True");
                context.Database.EnsureDeleted();
                context.Database.EnsureCreated();
                break;

            default:
                throw new InvalidOperationException("Unsupported database type");
        }

        return context;
    }

    /// <summary>
    /// Create a record in the TestTable database table.
    /// </summary>
    /// <param name="context">An instance of <see cref="Context"/> for the database.</param>
    /// <param name="value">The value to insert into the TestField field.</param>
    /// <returns>The ID of the record.</returns>
    public static long CreateSingleTestTableEntry(Context context, string value)
    {
        TestTable testTable = new () { TestField = value };
        context.Add(testTable);
        context.SaveChanges();
        return testTable.Id;
    }

    /// <summary>
    /// Create multiple records in the TestTable database table.
    /// </summary>
    /// <param name="context">An instance of <see cref="Context"/> for the database.</param>
    /// <param name="value">The value to insert into the TestField field.</param>
    /// <param name="recordCount">The number of records to create.</param>
    public static void CreateMultipleTestTableEntries(Context context, string value, int recordCount)
    {
        for (int i = 0; i < recordCount; i++)
        {
            TestTable testTable = new () { TestField = $"{value} {i}" };
            context.Add(testTable);
        }

        context.SaveChanges();
    }
}
