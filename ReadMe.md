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

* The exception processing only currently supports checking for unique constraint failures.

## Methods

### Case Insensitivity Extensions

**`DbContextOptionsBuilder UseSqliteUnicodeNoCase(this DbContextOptionsBuilder optionsBuilder)`**  

This method must be called from within the `OnConfiguring` override method in the database context (or similar). This method replaces the SQLite internal NoCase collation sequence with a Unicode compatible case and accent insensitive one.

**`ModelBuilder UseSqliteCaseInsensitiveCollation(this ModelBuilder modelBuilder)`**  

This method must be called from within the `OnModelCreating` override method in the database context (or similar). This method ensures all text fields in the entities use the SQLite NoCase collaction sequence. (This can be used in conjunction with `UseSqliteUnicodeNoCase` to ensure all text fields are case and accent insensitive).

**`ModelBuilder UseSqlServerCaseInsensitiveCollation(this ModelBuilder modelBuilder)`**  

This method must be called from within the `OnModelCreating` override method in the database context (or similar). This method sets the default collation sequence to be case and accent insensitive (`SQL_Latin1_General_CP1_CI_AS`).

### Database Type Extensions

**`string GetDatabaseType(this DatabaseFacade databaseFacade)`**  
**`string GetDatabaseType(this MigrationBuilder migrationBuilder)`**  

Extension methods for the `DatabaseFacade` (`context.Database`) or `MigrationBuilder` that return the type of database in use or, if unknown `null`: 

`DatabaseType.Sqlite` alias `"sqlite"`  
`DatabaseType.SqlServer` alias `"sqlserver"`  

### Does Exist Extensions

**`bool DoesDatabaseExist(this DatabaseFacade databaseFacade)`**  
**`Task<bool> DoesDatabaseExistAsync(this DatabaseFacade databaseFacade, CancellationToken cancellationToken = default)`**  

All methods extend the `DatabaseFacade` object. Returns a boolean indicating if the database referenced by the facade exists.

### Execute Update Extensions

**`DbContextOptionsBuilder UseExecuteUpdateExtensions(this DbContextOptionsBuilder optionsBuilder)`**

This method must be called from within the `OnConfiguring` override method in the database context (or similar). It attaches an interceptor to the context that is used by `ExecuteUpdateGetRows`. If this interceptor is not installed then ExecuteUpdateGetRows will not function correctly.  

**`int ExecuteUpdateGetCount<TEntity>(this IQueryable<TEntity> source, Action<SetPropertyBuilder<TEntity>> setPropertyAction)`**  
**`Task<int> ExecuteUpdateAsyncGetCount<TEntity>(this IQueryable<TEntity> source, Action<SetPropertyBuilder<TEntity>> setPropertyAction)`**  
**`IList<TEntity>> ExecuteUpdateGetRowsAsync<TEntity>(this IQueryable<TEntity> source, Action<SetPropertyBuilder<TEntity>> setPropertyAction)`**
**`Task<IList<TEntity>> ExecuteUpdateGetRowsAsync<TEntity>(this IQueryable<TEntity> source, Action<SetPropertyBuilder<TEntity>> setPropertyAction, CancellationToken cancellationToken = default)`**

Updates all database rows for the entity instances which match the LINQ query. SetPropertyAction is a method (not an expression) which is used to specify which properties to update. SetPropertyAction can contain code and logic to decide which fields will be updated (such as if statements etc.). Like ExecuteMethodGetCount in EntityFramework, the second argument of SetProperty can either a value or an expression. ExecuteUpdateGetCount methods return the number of rows altered. ExecuteUpdateGetRows methods return the actual rows altered.

**Example**

`int count = await context.TestTable1.Where(e => e.Id == id).ExecuteUpdateAsync(builder =>  
{  
    builder.SetProperty(e => e.TestField, e => updatedValue);  
});`

### Execute SQL Extensions

All methods extend the `DatabaseFacade` object. They are available in both synchronous and asynchronous and can take an FormattableString containing SQL or a SQL string with parameters (see the test project for examples).

**`T ExecuteScalar<T>(this DatabaseFacade databaseFacade, FormattableString sql)`**  
**`T ExecuteScalar<T>(this DatabaseFacade databaseFacade, string sql, params IEnumerable<object> parameters)`**  
**`Task<T> ExecuteScalarAsync<T>(this DatabaseFacade databaseFacade, FormattableString sql, CancellationToken cancellationToken = default)`**  
**`Task<T> ExecuteScalarAsync<T>(this DatabaseFacade databaseFacade, string sql, CancellationToken cancellationToken = default, params IEnumerable<object> parameters)`**  

Executes a query with a single scalar result of type <T>.

**`DataTable ExecuteQuery(this DatabaseFacade databaseFacade, FormattableString sql)`**  
**`DataTable ExecuteQuery(this DatabaseFacade databaseFacade, string sql, params IEnumerable<object> parameters)`**  
**`Task<DataTable> ExecuteQueryAsync(this DatabaseFacade databaseFacade, FormattableString sql, CancellationToken cancellationToken = default)`**  
**`Task<DataTable> ExecuteQueryAsync(this DatabaseFacade databaseFacade, string sql, CancellationToken cancellationToken = default, params IEnumerable<object> parameters)`**  

Executes a query and returns the results in a DataTable.

**`IList<TEntity> ExecuteQuery<TEntity>(this DatabaseFacade databaseFacade, FormattableString sql)`**  
**`IList<TEntity> ExecuteQuery<TEntity>(this DatabaseFacade databaseFacade, string sql, params IEnumerable<object> parameters)`**  
**`Task<IList<TEntity>> ExecuteQueryAsync<TEntity>(this DatabaseFacade databaseFacade, FormattableString sql, CancellationToken cancellationToken = default)`**  
**`Task<IList<TEntity>> ExecuteQueryAsync<TEntity>(this DatabaseFacade databaseFacade, string sql, CancellationToken cancellationToken = default, params IEnumerable<object> parameters)`**  

Executes a query and returns the results in a `IList<TEntity>`. `TEntity` must be a valid EF Core entity.

**`QueryPageTable ExecutePagedQuery(this DatabaseFacade databaseFacade, FormattableString sql, long page, long pageSize)`**  
**`QueryPageTable ExecutePagedQuery(this DatabaseFacade databaseFacade, string sql, long page, long pageSize, params IEnumerable<object> parameters)`**  
**`Task<QueryPageTable> ExecutePagedQueryAsync(this DatabaseFacade databaseFacade, FormattableString sql, long page, long pageSize, CancellationToken cancellationToken = default)`**  
**`Task<QueryPageTable> ExecutePagedQueryAsync(this DatabaseFacade databaseFacade, string sql, long page, long pageSize, CancellationToken cancellationToken = default, params IEnumerable<object> parameters)`**  

Executes a query and returns the specified page of results as a `DataTable` an instance of the `QueryPageTable` class. `PageSize` is the number of records per page. If `page * pageSize` is greater than the last record, then the page is set to be the last page.

**`QueryPageEntity<TEntity> ExecutePagedQuery<TEntity>(this DatabaseFacade databaseFacade, FormattableString sql, long page, long pageSize)`**  
**`QueryPageEntity<TEntity> ExecutePagedQuery<TEntity>(this DatabaseFacade databaseFacade, string sql, long page, long pageSize, params IEnumerable<object> parameters)`**  
**`Task<QueryPageEntity<TEntity>> ExecutePagedQueryAsync<TEntity>(this DatabaseFacade databaseFacade, FormattableString sql, long page, long pageSize, CancellationToken cancellationToken = default)`**  
**`Task<QueryPageEntity<TEntity>> ExecutePagedQueryAsync<TEntity>(this DatabaseFacade databaseFacade, string sql, long page, long pageSize, CancellationToken cancellationToken = default, params IEnumerable<object> parameters)`**  

Executes a query and returns the specified page of results as a `IList<TEntity>` in an instance of the `QueryPageEntity<TEntity>` class. `TEntity` must be a valid EF Core entity. `PageSize` is the number of records per page. If `page * pageSize` is greater than the last record, then the page is set to be the last page.

**`int ExecuteNonQuery(this DatabaseFacade databaseFacade, FormattableString sql)`**  
**`int ExecuteNonQuery(this DatabaseFacade databaseFacade, string sql, params IEnumerable<object> parameters)`**  
**`Task<int> ExecuteNonQueryAsync(this DatabaseFacade databaseFacade, FormattableString sql, CancellationToken cancellationToken = default)`**  
**`Task<int> ExecuteNonQueryAsync(this DatabaseFacade databaseFacade, string sql, CancellationToken cancellationToken = default, params IEnumerable<object> parameters)`**  

Executes a non-query (such as Update or Delete) and return the number of records changed. These methods are here for completeness and are in fact a wrapper round the EF Core ExecuteSqlxxxxx extensions.

**`long ExecuteInsert(this DatabaseFacade databaseFacade, FormattableString sql)`**  
**`T ExecuteInsert<T>(this DatabaseFacade databaseFacade, FormattableString sql)`**  
**`long ExecuteInsert(this DatabaseFacade databaseFacade, string sql, params IEnumerable<object> parameters)`**  
**`T ExecuteInsert<T>(this DatabaseFacade databaseFacade, string sql, params IEnumerable<object> parameters)`**  
**`Task<long> ExecuteInsertAsync(this DatabaseFacade databaseFacade, FormattableString sql, CancellationToken cancellationToken = default)`**  
**`Task<T> ExecuteInsertAsync<T>(this DatabaseFacade databaseFacade, FormattableString sql, CancellationToken cancellationToken = default)`**  
**`Task<long> ExecuteInsertAsync(this DatabaseFacade databaseFacade, string sql, CancellationToken cancellationToken = default, params IEnumerable<object> parameters)`**  
**`Task<T> ExecuteInsertAsync<T>(this DatabaseFacade databaseFacade, string sql, CancellationToken cancellationToken = default, params IEnumerable<object> parameters)`**  

Executes an insert statement and return the ID of the newly inserted record.

### Unique Constraint Extensions

**`DbContextOptionsBuilder UseUniqueConstraintInterceptor(this DbContextOptionsBuilder optionsBuilder)`**

This method must be called from within the `OnConfiguring` override method in the database context (or similar). It attaches an interceptor to the context that captures unique constraint failures and throws a `UniqueConstraintException`. The exception contains a `UniqueConstraintDetails` property called Details which holds the schema, table, and field names that have caused the issue. The original exception is returned in the InnerException property.

**`UniqueConstraintDetails GetUniqueConstraintDetails(this DatabaseFacade databaseFacade, Exception e)`**
**`UniqueConstraintDetails GetUniqueConstraintDetailsAsync(this DatabaseFacade databaseFacade, Exception e)`**

These methods extend the `DatabaseFacade` object. They are passed an exception and if it is a unique constraint failure then they return a `UniqueConstraintDetails` object which contains the schema, table, and field names. If the exception is not the result of a unique constraint failing, then `null` is returned. These methods should be used in a catch block after executing some SQL (see `Test_GetUniqueConstraintDetails_EfCore` in GetUniqueConstraintDetailsTests.cs for an example).
