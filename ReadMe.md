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

### FreeTextSearch Extensions Example

This provides methods for database independent free text searching (meaning the same LINQ statements can be used to query free text tables in either SQL Server or SQLite).  

#### Pre-Requisites

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

### IsUniqueConstraintException Example

**Note:** In version 7.0.0.0 `IsUniqueConstraintException` has replaced `UseUniqueConstraintInterceptor()` (the unique constraint interceptor), `UniqueConstraintException`, `UniqueConstraintDetails` and `GetUniqueConstraintDetailsAsync`, which have been removed. The table and field names they returned were not reliable, so callers that used them should catch the `DbUpdateException` and use `IsUniqueConstraintException` instead.  

`IsUniqueConstraintException` is an extension method on `Exception` in the `DotDoc.EntityFrameworkCore.Extensions.Utilities` namespace. This provides a database independent way to check if an exception was caused by a unique constraint violation (a unique index, a unique constraint or a primary key). The same code works with SQL Server and SQLite. It checks the database error code rather than the error message, so it works whatever language the database uses, and with exceptions from `SaveChanges` and from other methods such as `ExecuteUpdate` or raw SQL.  

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

### SQLite NoCase Replacement

`UseSqliteNoCaseReplacement()` replaces SQLite's `NOCASE` collation and its `lower` and `upper` functions with Unicode versions (SQLite's own only handle ASCII letters). They are case insensitive but accent sensitive, matching the SQL Server `Latin1_General_100_CI_AS_SC_UTF8` collation set by `UseCaseInsensitiveCollation()`.

#### Connection Pooling

Microsoft.Data.Sqlite pools connections by default, with one pool per connection string. Once any connection using a connection string has called `UseSqliteNoCaseReplacement()`, **every** later connection with that connection string must call it too: every `DbContext`, any `SqliteConnection` opened directly, and design-time tools such as `dotnet ef`.

When a connection goes back to the pool, Microsoft.Data.Sqlite removes the collation and functions registered on it, and SQLite can't fall back to its built-in `NOCASE`, `lower` and `upper` on that connection. A later connection that doesn't register them again fails with:

* `SQLite Error 1: 'no such collation sequence: NOCASE'`
* `SQLite Error 1: 'wrong number of arguments to function upper()'` (or `lower()`)

To use connections with and without the replacement on the same database, give them different connection strings, or turn pooling off (`Pooling=False`) for one set of them.
