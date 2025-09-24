# EntityFrameworkCore.Extensions

Copyright ©2021-2025 Mike King.   
Licensed using the MIT licence. See the License.txt file in the solution root for more information.

## Overview

EntityFameworkCore.Extensions is a set of utility extension methods for Entity Framework Core.

## Pre-Requisites

The library and tests use .Net 9.0 and Entity Framework Core v9. 

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
