# EntityFrameworkCore.Extensions

Copyright ©2021-2026 Mike King.  
Licensed using the MIT licence. See the License.txt file in the solution root for more information.  

UUIDNext by Matthieu Mourisson.  
Licensed using the BSD Zero Clause.  

https://github.com/mareek/UUIDNext  

## Overview

EntityFrameworkCore.Extensions is a set of utility extension methods for Entity Framework Core.

## Pre-Requisites

The library and tests use .Net 11.0 and Entity Framework Core v11. 

## Limitations

* SQLite and SQL Server are the only databases currently supported.

#### FreeTextSearch Extensions Example

This provides methods for database independent free text searching (meaning the same LINQ statements can be used to query free text tables in either SQL Server or SQLite).  

##### Pre-Requisites

* Free text tables must be configured in the context as shared type entities.  

* In SQL Server a full text catalogue must be created and a full text index created for each field to be searched.  

* In SQLite the table being searched must be a FTS table (content less or external content tables are supported). If stemming (searching by the root of an English word e.g. searching for Swim will find Swim, Swam, Swum etc) is required then an identical table must be configured (using the porter tokeniser) and populated with the same data.  

* In SQLite the non-stemming table must be associated with the stemming table using the SetStemmingTable annotation (see example below).  

**from ModelCreating in Context.cs**  

```
// Setup the model for the FreeText and FreeText_Stemming tables.
modelBuilder.SharedTypeEntity<FreeText>(TestFreeTextTableName);

// In SQLite associate the stemming table with the non stemming table
// and the Id column with the value of the ROWID column.
if (this.DatabaseType == DatabaseTypes.Sqlite)
{
    modelBuilder.SharedTypeEntity<FreeText>(TestFreeTextTableName)
        .SetStemmingTable(TestFreeTextStemmingTableName)
        .Property<long>(nameof(FreeText.Id))
        .HasColumnName("ROWID");

    modelBuilder.SharedTypeEntity<FreeText>(TestFreeTextStemmingTableName)
        .Property<long>(nameof(FreeText.Id))
        .HasColumnName("ROWID");
}
```

(For an example of creating a full text catalogue/index and FTS tables see the `CreateDatabaseAsync` method in DatabaseUtils.cs).  

**Using the FreeTextSearch methods**  

```
List<FreeText> rows = await context.TestFreeText
    .Where(e => EF.Functions.FreeTextSearch(e.FreeTextField!, "apple", true))
    .ToListAsync()
    .ConfigureAwait(false);
```

This searches the field FreeTextField in the table TestFreeText for the word "apple" using stemming.  

Behind the scenes:

* In SQL Server the query being generated is modified to use the correct free text function (either FreeText or Contains) depending on whether stemming is required.  

* In SQLite the query being generated is modified to use either the non stemming or stemming table.  

LanguageTerm is a value used by SQL Server to specify the language for the search. The default is the database language (see the SQL Server documentation for details). This value is ignored when using SQLite.  

**Important**  

Because of the way EF Core parses expressions there is a limitation when multiple FreeTextSearch functions are used in a SQLite query. The value of useStemming is determined by the first call to `FreeTextSearch`, the value on any subsequent calls is ignored. This is not a limitation with SQL Server.  

#### IsUniqueConstraintException Example

**Note:** In version 7.0.0.0 `IsUniqueConstraintException` has replaced the unique constraint interceptor (`UseUniqueConstraintInterceptor()`), `UniqueConstraintException`, `UniqueConstraintDetails` and `GetUniqueConstraintDetailsAsync` (which have been removed). The table and field names they returned were not reliable, so callers that used them should catch the `DbUpdateException` and use `IsUniqueConstraintException` instead.  

This provides a database independent way to check if an exception was caused by a unique constraint violation (a unique index, a unique constraint or a primary key), so the same code works with SQL Server and SQLite.  

`IsUniqueConstraintException` is an extension method on `Exception` in the `DotDoc.EntityFrameworkCore.Extensions.UniqueConstraint` namespace. It checks the exception and its inner exceptions for the database error code (SQL Server errors 2601 and 2627, SQLite `SQLITE_CONSTRAINT_UNIQUE` and `SQLITE_CONSTRAINT_PRIMARYKEY`) rather than the error message, so it doesn't depend on the language the database uses for its messages. It works with exceptions from `SaveChanges` (where EF Core wraps the database exception in a `DbUpdateException`) and from other methods, such as `ExecuteUpdate` or raw SQL.  

**Using IsUniqueConstraintException**  

```
try
{
    context.Customers.Add(customer);
    await context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
}
catch (DbUpdateException ex) when (ex.IsUniqueConstraintException())
{
    // A row with the same unique value already exists.
}
```

**Finding which value clashed**  

If a table has more than one unique constraint (for example, unique `Email` and `UserName` fields), the exception doesn't say which one failed.  

Don't rely on checking for an existing value before saving: another user can insert the same value between the check and the save. Instead, check after the exception has been caught. The database has already rejected the row, so a duplicate can't get in. If the clashing row is deleted between the exception and the check, the worst that happens is a less specific message.  

```
catch (DbUpdateException ex) when (ex.IsUniqueConstraintException())
{
    if (await context.Customers.AnyAsync(c => c.Email == customer.Email, cancellationToken).ConfigureAwait(false))
    {
        message = "That email address is already registered.";
    }
    else if (await context.Customers.AnyAsync(c => c.UserName == customer.UserName, cancellationToken).ConfigureAwait(false))
    {
        message = "That user name is taken.";
    }
    else
    {
        message = "A matching customer already exists.";
    }
}
```

If you started a transaction yourself, roll it back before running these queries (on SQL Server a unique constraint error can leave the transaction unusable, e.g. when `XACT_ABORT` is on). `SaveChanges` rolls back its own transaction when it fails.  
