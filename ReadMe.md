# EntityFrameworkCore.Extensions

Copyright ©2021-2023 Mike King.   
Licensed using the MIT licence. See the License.txt file in the solution root for more information.

## Overview

EntityFameworkCore.Extensions is a set of utility extension methods for Entity Framework Core.

## Pre-Requisites

The library and tests use .Net 7.0 and Entity Framework Core v7. 

## Limitations

* Only SQLite and SQL Server are currently supported.

* The DoesExist extensions only currently support checking if databases and tables exists.

* The exception processing only currently supports checking for unique constraint failures.

## Methods

### Database Type Extensions

**`DatabaseType GetDatabaseType(this DatabaseFacade databaseFacade)`**  
**`DatabaseType GetDatabaseType(this MigrationBuilder migrationBuilder)`**  

Extension methods for the `DatabaseFacade` (`context.Database`) or MigrationBuilder (when performing a migration) that return the type of database in use: 

`DatabaseType.Unknown`  
`DatabaseType.Sqlite`  
`DatabaseType.SqlServer`  

### Does Exist Extensions

**`bool DoesDatabaseExist(this DatabaseFacade databaseFacade)`**  
**`Task<bool> DoesDatabaseExistAsync(this DatabaseFacade databaseFacade, CancellationToken cancellationToken = default)`**  

All methods extend the `DatabaseFacade` object. Returns a boolean indicating if the database referenced by the facade exists.

**`bool DoesTableExist(this DatabaseFacade databaseFacade, string tableName)`**  
**`Task<bool> DoesTableExistAsync(this DatabaseFacade databaseFacade, string tableName, CancellationToken cancellationToken = default)`**  

Returns a boolean indicating if the table specified exists.

### Execute Update Extensions

**`int ExecuteUpdate<TSource>(this IQueryable<TSource> source, Action<SetPropertyBuilder<TSource>> setPropertyAction)`**  
**`Task<int> ExecuteUpdateAsync<TSource>(this IQueryable<TSource> source, Action<SetPropertyBuilder<TSource>> setPropertyAction)`**  

Updates all database rows for the entity instances which match the LINQ query from the database. SetPropertyAction is a method (not an expression) which is used to specify which properties to update. SetPropertyAction can contain code to decide which fields must be updated (such as if statements etc.). Like ExecuteMethod in EntityFramework, the second argument of SetProperty can either a value or an expression.

**Example**

`int count = await context.TestTable1.Where(e => e.Id == id).ExecuteUpdateAsync(builder =>  
{  
    builder.SetProperty(e => e.TestField, e => updatedValue);  
});`  

### Execute SQL Extensions

All methods extend the `DatabaseFacade` object. They are available in both synchronous and asynchronous and can take an interpolated SQL string or a raw SQL string with parameters (see the test project for examples).

**`T ExecuteScalarInterpolated<T>(this DatabaseFacade databaseFacade, FormattableString sql)`**  
**`T ExecuteScalarRaw<T>(this DatabaseFacade databaseFacade, string sql, params object[] parameters)`**  
**`Task<T> ExecuteScalarInterpolatedAsync<T>(this DatabaseFacade databaseFacade, FormattableString sql, CancellationToken cancellationToken = default)`**  
**`Task<T> ExecuteScalarRawAsync<T>(this DatabaseFacade databaseFacade, string sql, CancellationToken cancellationToken = default, params object[] parameters)`**  

Executes a query with a single scalar result of type <T>.

**`DataTable ExecuteQueryInterpolated(this DatabaseFacade databaseFacade, FormattableString sql)`**  
**`DataTable ExecuteQueryRaw(this DatabaseFacade databaseFacade, string sql, params object[] parameters)`**  
**`Task<DataTable> ExecuteQueryInterpolatedAsync(this DatabaseFacade databaseFacade, FormattableString sql, CancellationToken cancellationToken = default)`**  
**`Task<DataTable> ExecuteQueryRawAsync(this DatabaseFacade databaseFacade, string sql, CancellationToken cancellationToken = default, params object[] parameters)`**  

Executes a query and returns the results in a DataTable.

**`QueryPage ExecutePagedQueryInterpolated(this DatabaseFacade databaseFacade, FormattableString sql, long page, long pageSize)`**  
**`QueryPage ExecutePagedQueryRaw(this DatabaseFacade databaseFacade, string sql, long page, long pageSize, params object[] parameters)`**  
**`Task<QueryPage> ExecutePagedQueryInterpolatedAsync(this DatabaseFacade databaseFacade, FormattableString sql, long page, long pageSize, CancellationToken cancellationToken = default)`**  
**`Task<QueryPage> ExecutePagedQueryRawAsync(this DatabaseFacade databaseFacade, string sql, long page, long pageSize, CancellationToken cancellationToken = default, params object[] parameters)`**  

Executes a query and returns the specified page of results in an instance of the QueryPage class. PageSize is the number of records per page. If page * pageSize is greater than the last record, then the page is set to be the last page.

**`int ExecuteNonQueryInterpolated(this DatabaseFacade databaseFacade, FormattableString sql)`**  
**`int ExecuteNonQueryRaw(this DatabaseFacade databaseFacade, string sql, params object[] parameters)`**  
**`Task<int> ExecuteNonQueryInterpolatedAsync(this DatabaseFacade databaseFacade, FormattableString sql, CancellationToken cancellationToken = default)`**  
**`Task<int> ExecuteNonQueryRawAsync(this DatabaseFacade databaseFacade, string sql, CancellationToken cancellationToken = default, params object[] parameters)`**  

Executes a non-query (such as Update or Delete) and return the number of records changed. These methods are here for completeness and are in fact a wrapper round the EF Core ExecuteSqlxxxxx extensions.

**`long ExecuteInsertInterpolated(this DatabaseFacade databaseFacade, FormattableString sql)`**  
**`long ExecuteInsertRaw(this DatabaseFacade databaseFacade, string sql, params object[] parameters)`**  
**`Task<long> ExecuteInsertInterpolatedAsync(this DatabaseFacade databaseFacade, FormattableString sql, CancellationToken cancellationToken = default)`**  
**`Task<long> ExecuteInsertRawAsync(this DatabaseFacade databaseFacade, string sql, CancellationToken cancellationToken = default, params object[] parameters)`**  

Executes an insert statement and return the ID of the newly inserted record.

### Get Context Extensions

**`DbContext GetContext<TSource>(this IQueryable<TSource> source)`**

Gets the DbContext object that is used by the specified IQueryable<T> created by EF Core. If the IQueryable object is not associated with a context or has not been created by EF Core returns null.

### Unique Constraint Extensions

**`DbContextOptionsBuilder UseUniqueConstraintInterceptor(this DbContextOptionsBuilder optionsBuilder)`**

This method extends the `DbContextOptionsBuilder` object. It must be called from within the `OnConfiguring` override method in the database context. It attaches an interceptor to the context that captures unique constraint failures and throws a `UniqueConstraintException`. The exception contains a `UniqueConstraintDetails` property called Details which holds the schema, table name and field name that have caused the issue. The original exception is returned in the InnerException property.

**`UniqueConstraintDetails GetUniqueConstraintDetails(this DbContext context, Exception e)`**
**`UniqueConstraintDetails GetUniqueConstraintDetailsAsync(this DbContext context, Exception e)`**

These methods extend the `DbContext` object. They are passed an exception and if it is a unique constraint failure then they return a `UniqueConstraintDetails` object which contains the schema, table and field names. If the exception is not the result of a unique constraint failing, then `null` is returned. These methods should be used in a catch block after executing some SQL (see `TestGetUniqueConstraintDetailsFromSqlTableAsync` in GetUniqueConstraintDetailsTests.cs for an example).
