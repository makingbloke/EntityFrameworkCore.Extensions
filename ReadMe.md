# EntityFrameworkCore.Extensions

Copyright ©2021-2025 Mike King.   
Licensed using the MIT licence. See the License.txt file in the solution root for more information.

## Overview

EntityFameworkCore.Extensions is a set of utility extension methods for Entity Framework Core.

## Pre-Requisites

The library and tests use .Net 9.0 and Entity Framework Core v9. 

## Limitations

* Only SQLite and SQL Server are currently supported.

* The DoesExist extensions only currently support checking if a database exists.

## Methods

### Case Insensitivity Extensions (Namespace `DotDoc.EntityFrameworkCore.Extensions.CaseInsensitivity`)

**`DbContextOptionsBuilder UseSqliteUnicodeNoCase(this DbContextOptionsBuilder optionsBuilder)`**  

This method must be called from within the `OnConfiguring` override method in the database context (or similar). This method replaces the SQLite internal NoCase collation sequence with a Unicode compatible case and accent insensitive one.

**`ModelBuilder UseSqliteCaseInsensitiveCollation(this ModelBuilder modelBuilder)`**  

This method must be called from within the `OnModelCreating` override method in the database context (or similar). This method ensures all text fields in the entities use the SQLite NoCase collaction sequence. (This can be used in conjunction with `UseSqliteUnicodeNoCase` to ensure all text fields are case and accent insensitive).

**`ModelBuilder UseSqlServerCaseInsensitiveCollation(this ModelBuilder modelBuilder)`**  

This method must be called from within the `OnModelCreating` override method in the database context (or similar). This method sets the default collation sequence to be case and accent insensitive (`SQL_Latin1_General_CP1_CI_AS`).

### Database Type Extensions (Namespace `DotDoc.EntityFrameworkCore.Extensions.DatabaseType`)

**`string GetDatabaseType(this DatabaseFacade databaseFacade)`**  
**`string GetDatabaseType(this MigrationBuilder migrationBuilder)`**  

Extension methods for the `DatabaseFacade` (`context.Database`) or `MigrationBuilder` that return the type of database in use or, if unknown throws an `InvalidOperationException` exception: 

`DatabaseType.Sqlite` which is the string value `"sqlite"`  
`DatabaseType.SqlServer` which is the string value `"sqlserver"`  

### Does Exist Extensions (Namespace `DotDoc.EntityFrameworkCore.Extensions.DoesExist`)

**`Task<bool> DoesDatabaseExistAsync(this DatabaseFacade databaseFacade, CancellationToken cancellationToken = default)`**  

All methods extend the `DatabaseFacade` object. Returns a boolean indicating if the database referenced by the facade exists.

### Execute Extensions (Namespace `DotDoc.EntityFrameworkCore.Extensions.Execute`)

All methods extend the `DatabaseFacade` object. They take a FormattableString containing SQL or a SQL string with parameters (see the test project for examples).

**`Task<T> ExecuteScalarAsync<T>(this DatabaseFacade databaseFacade, FormattableString sql, CancellationToken cancellationToken = default)`**  
**`Task<T> ExecuteScalarAsync<T>(this DatabaseFacade databaseFacade, string sql, CancellationToken cancellationToken = default, params IEnumerable<object> parameters)`**  

Executes a query with a single scalar result of type <T>.

**`Task<DataTable> ExecuteQueryAsync(this DatabaseFacade databaseFacade, FormattableString sql, CancellationToken cancellationToken = default)`**  
**`Task<DataTable> ExecuteQueryAsync(this DatabaseFacade databaseFacade, string sql, CancellationToken cancellationToken = default, params IEnumerable<object> parameters)`**  

Executes a query and returns the results in a DataTable.

**`Task<IList<TEntity>> ExecuteQueryAsync<TEntity>(this DatabaseFacade databaseFacade, FormattableString sql, CancellationToken cancellationToken = default)`**  
**`Task<IList<TEntity>> ExecuteQueryAsync<TEntity>(this DatabaseFacade databaseFacade, string sql, CancellationToken cancellationToken = default, params IEnumerable<object> parameters)`**  

Executes a query and returns the results in a `IList<TEntity>`. `TEntity` must be a valid EF Core entity.

**`Task<PageResultTable> ExecutePagedQueryAsync(this DatabaseFacade databaseFacade, FormattableString sql, long page, long pageSize, CancellationToken cancellationToken = default)`**  
**`Task<PageResultTable> ExecutePagedQueryAsync(this DatabaseFacade databaseFacade, string sql, long page, long pageSize, CancellationToken cancellationToken = default, params IEnumerable<object> parameters)`**  

Executes a query and returns the specified page of results as a `DataTable` an instance of the `PageResultTable` class. `PageSize` is the number of records per page. If `page * pageSize` is greater than the last record, then the page is set to be the last page.

**`Task<PageResultEntity<TEntity>> ExecutePagedQueryAsync<TEntity>(this DatabaseFacade databaseFacade, FormattableString sql, long page, long pageSize, CancellationToken cancellationToken = default)`**  
**`Task<PageResultEntity<TEntity>> ExecutePagedQueryAsync<TEntity>(this DatabaseFacade databaseFacade, string sql, long page, long pageSize, CancellationToken cancellationToken = default, params IEnumerable<object> parameters)`**  

Executes a query and returns the specified page of results as a `IList<TEntity>` in an instance of the `PageResultEntity<TEntity>` class. `TEntity` must be a valid EF Core entity. `PageSize` is the number of records per page. If `page * pageSize` is greater than the last record, then the page is set to be the last page.

**`Task<int> ExecuteNonQueryAsync(this DatabaseFacade databaseFacade, FormattableString sql, CancellationToken cancellationToken = default)`**  
**`Task<int> ExecuteNonQueryAsync(this DatabaseFacade databaseFacade, string sql, CancellationToken cancellationToken = default, params IEnumerable<object> parameters)`**  

Executes a non-query (such as Update or Delete) and return the number of records changed. These methods are here for completeness and are in fact a wrapper round the EF Core ExecuteSqlxxxxx extensions.

**`Task<long> ExecuteInsertAsync(this DatabaseFacade databaseFacade, FormattableString sql, CancellationToken cancellationToken = default)`**  
**`Task<T> ExecuteInsertAsync<T>(this DatabaseFacade databaseFacade, FormattableString sql, CancellationToken cancellationToken = default)`**  
**`Task<long> ExecuteInsertAsync(this DatabaseFacade databaseFacade, string sql, CancellationToken cancellationToken = default, params IEnumerable<object> parameters)`**  
**`Task<T> ExecuteInsertAsync<T>(this DatabaseFacade databaseFacade, string sql, CancellationToken cancellationToken = default, params IEnumerable<object> parameters)`**  

Executes an insert statement and return the ID of the newly inserted record.

### Execute Update Extensions (Namespace `DotDoc.EntityFrameworkCore.Extensions.ExecuteUpdate`)

**`DbContextOptionsBuilder UseExecuteUpdateExtensions(this DbContextOptionsBuilder optionsBuilder)`**

This method must be called from within the `OnConfiguring` override method in the database context (or similar). It attaches an interceptor to the context that is used by `ExecuteUpdateGetRows`. 

**`Task<int> ExecuteUpdateAsyncGetCount<TEntity>(this IQueryable<TEntity> source, Action<UpdateSettersBuilder<TEntity>> setPropertyCalls)`**  
**`Task<IList<TEntity>> ExecuteUpdateGetRowsAsync<TEntity>(this IQueryable<TEntity> source, Action<UpdateSettersBuilder<TEntity>> setPropertyCalls, CancellationToken cancellationToken = default)`**

Updates all database rows for the entity instances which match the LINQ query. setPropertyCalls is a method (not an expression) which is used to specify which properties to update. setPropertyCalls can contain code and logic to decide which fields will be updated (such as if statements etc.). Like ExecuteMethodGetCount in EntityFramework, the second argument of SetProperty can either a value or an expression. ExecuteUpdateGetCount methods return the number of rows altered. ExecuteUpdateGetRows methods return the actual rows altered.

ExecuteUpdateGetRows uses the value set by IsSqlReturningClauseUsed / IsSqlOutputClauseUsed to determine how the rows are retrieved. If your table contains a trigger on the field being updated then IsSqlReturningClauseUsed / IsSqlOutputClauseUsed need to be set to false (see the EF Core documentation for further details).  

**Example**

`int count = await context.TestTable1
    .Where(e => e.Id == id)
    .ExecuteUpdateAsync(builder =>  
    {  
        builder.SetProperty(e => e.TestField, e => updatedValue);  
    });`  

### Get DbContext Extensions (Namespace `DotDoc.EntityFrameworkCore.Extensions.GetDbContext`)

**`DbContext GetDbContext(this IQueryable query)`**
**`DbContext GetDbContext(this DatabaseFacade databaseFacade)`**

Gets the DbContext object that is used by the specified IQueryable / DatabaseFacade object.

### Unique Constraint Extensions (Namespace `DotDoc.EntityFrameworkCore.Extensions.UniqueConstraint`)

**`DbContextOptionsBuilder UseUniqueConstraintInterceptor(this DbContextOptionsBuilder optionsBuilder)`**

This method must be called from within the `OnConfiguring` override method in the database context (or similar). It attaches an interceptor to the context that captures unique constraint failures and throws a `UniqueConstraintException`. The exception contains a `UniqueConstraintDetails` property called Details which holds the schema, table, and field names that have caused the issue. The original exception is returned in the InnerException property.

**`UniqueConstraintDetails GetUniqueConstraintDetailsAsync(this DatabaseFacade databaseFacade, Exception e)`**

These methods extend the `DatabaseFacade` object. They are passed an exception and if it is a unique constraint failure then they return a `UniqueConstraintDetails` object which contains the schema, table, and field names. If the exception is not the result of a unique constraint failing, then `null` is returned. These methods should be used in a catch block after executing some SQL (see `Test_GetUniqueConstraintDetails_EfCore` in GetUniqueConstraintDetailsTests.cs for an example).

### Free Text Search LINQ Functions

#### Match Function

This provides a wrapper around the SQLite Match function.  

**`DbContextOptionsBuilder UseMatchExtensions(this DbContextOptionsBuilder optionsBuilder)`**

Installs the Match function extension. This method must be called from within the `OnConfiguring` override method in the database context (or similar).  

**`bool Match(string freeText, object propertyReference)`**

Perform a match (See the SQLite documentation for details). The first argument is the text to search for, the second the field in the entity to search.  

**Example**

`List<FreeText> rows = await context.MyFreeTextTable
    .Where(e => EF.Functions.Match("apple", e.FreeTextField))
    .ToListAsync()
    .ConfigureAwait(false);`

#### FreeTextSearch Functions

This provides a database independent free text search function (meaning the same LINQ statements can be used to query free text tables in either SQL Server or SQLite).  

##### Pre-Requisites

* Free text tables must be configured in the context as shared type entities.  

* In SQL Server a full text catalogue must be created and a full text index created for each field to searched.  

* In SQLite the table being searched must be a FTS table (content less or external content tables are supported). If stemming (searching by the root of an English word e.g. searching for Swim will find Swim, Swam, Swum etc) is required then an identical table must be configured (using the porter tokeniser) and populated with the same data.  

* In SQLite the non-stemming table must be associated with the stemming table using the SetStemmingTable annotation (see example below).  

**Example (from ModelCreating in Context.cs)**

`// Setup the model for the FreeText and FreeText_Stemming tables.
modelBuilder.SharedTypeEntity<FreeText>(TestFreeTextTableName);`

`// In SQLite associate the stemming table with the non stemming table
// and the Id column with the value of the ROWID column.
if (this.DatabaseType == DatabaseTypes.Sqlite)
{
    modelBuilder.SharedTypeEntity<FreeText>(TestFreeTextTableName)
        .SetStemmingTable(TestFreeTextStemmingTableName)
        .Property<long>(nameof(FreeText.Id))
        .HasColumnName("ROWID");`

`    modelBuilder.SharedTypeEntity<FreeText>(TestFreeTextStemmingTableName)
        .Property<long>(nameof(FreeText.Id))
        .HasColumnName("ROWID");
}`

For an example of creating a full text catalogue/index and FTS tables see the `CreateDatabaseAsync` method in DatabaseUtils.cs.  

**`DbContextOptionsBuilder UseFreeTextExtensions(this DbContextOptionsBuilder optionsBuilder)`**

Installs the FreeTextSearch (and Match) functions. This method must be called from within the `OnConfiguring` override method in the database context (or similar).  

**`EntityTypeBuilder<TEntity> SetStemmingTable<TEntity>(this EntityTypeBuilder<TEntity> entityBuilder, string tableName)`**

Associates the stemming table with the default non-stemming table (For SQLServer this value is ignored).  

**`string? GetStemmingTable(this IEntityType entityType)`**

Gets the name of the stemming table (or null if none has been specified).  

**`bool FreeTextSearch(object propertyReference, string freeText)`**
**`bool FreeTextSearch(object propertyReference, string freeText, bool useStemming)`**
**`bool FreeTextSearch(object propertyReference, string freeText, int languageTerm)`**
**`bool FreeTextSearch(object propertyReference, string freeText, bool useStemming, int languageTerm)`**

Perform a free text search. The first argument is the field in the entity to search, the second the text to search for.  

UseStemming is used to determine if stemming should be used. By default it is off.  

Behind the scenes:

* In SQL Server the query being generated is modified to use the correct free text function (either FreeText or Contains) depending on whether stemming is required.  

* In SQLite the query being generated is modified to use either the non stemming or stemming table.  

LanguageTerm is a value used by SQL Server to specify the language for the search. The default is the database language (see the SQL Server documentation for details). This value is ignored when using SQLite.  

**Important**

Because of the way EF Core parses expressions there is a limitation where multiple FreeTextSearch functions are used in a SQLite query. Whether to use stemming is determined by the first call to `FreeTextSearch`, the useStemming value on any subsequent calls is ignored. This is not a limitation with SQL Server.  

**Example**

`List<FreeText> rows = await context.TestFreeText
    .Where(e => EF.Functions.FreeTextSearch(e.FreeTextField!, "apple", true))
    .ToListAsync()
    .ConfigureAwait(false);`

This searches the field FreeTextField in the table TestFreeText for the word "apple" using stemming.
