# EntityFrameworkCore.Extensions

Copyright ©2021-2025 Mike King.   
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

**`string GetDatabaseType(this DatabaseFacade databaseFacade)`**  
**`string GetDatabaseType(this MigrationBuilder migrationBuilder)`**  

Extension methods for the `DatabaseFacade` (`context.Database`) or MigrationBuilder (when performing a migration) that return the type of database in use or `null` if unknown: 

`sqlite` alias `DatabaseType.Sqlite`  
`sqlserver` alias `DatabaseType.SqlServer`  

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

All methods extend the `DatabaseFacade` object. They are available in both synchronous and asynchronous and can take an FormattableString containing SQL or a SQL string with parameters (see the test project for examples).

**`T ExecuteScalar<T>(this DatabaseFacade databaseFacade, FormattableString sql)`**  
**`T ExecuteScalar<T>(this DatabaseFacade databaseFacade, string sql, params object[] parameters)`**  
**`T ExecuteScalar<T>(this DatabaseFacade databaseFacade, string sql, IEnumerable<object> parameters)`**  
**`Task<T> ExecuteScalarAsync<T>(this DatabaseFacade databaseFacade, FormattableString sql, CancellationToken cancellationToken = default)`**  
**`Task<T> ExecuteScalarAsync<T>(this DatabaseFacade databaseFacade, string sql, CancellationToken cancellationToken = default, params object[] parameters)`**  
**`Task<T> ExecuteScalarAsync<T>(this DatabaseFacade databaseFacade, string sql, IEnumerable<object> parameters, CancellationToken cancellationToken = default)`**  

Executes a query with a single scalar result of type <T>.

**`DataTable ExecuteQuery(this DatabaseFacade databaseFacade, FormattableString sql)`**  
**`DataTable ExecuteQuery(this DatabaseFacade databaseFacade, string sql, params object[] parameters)`**  
**`DataTable ExecuteQuery(this DatabaseFacade databaseFacade, string sql, IEnumerable<object> parameters)`**  
**`Task<DataTable> ExecuteQueryAsync(this DatabaseFacade databaseFacade, FormattableString sql, CancellationToken cancellationToken = default)`**  
**`Task<DataTable> ExecuteQueryAsync(this DatabaseFacade databaseFacade, string sql, CancellationToken cancellationToken = default, params object[] parameters)`**  
**`Task<DataTable> ExecuteQueryAsync(this DatabaseFacade databaseFacade, string sql, IEnumerable<object> parameters, CancellationToken cancellationToken = default)`**  

Executes a query and returns the results in a DataTable.

**`QueryPage ExecutePagedQuery(this DatabaseFacade databaseFacade, FormattableString sql, long page, long pageSize)`**  
**`QueryPage ExecutePagedQuery(this DatabaseFacade databaseFacade, string sql, long page, long pageSize, params object[] parameters)`**  
**`QueryPage ExecutePagedQuery(this DatabaseFacade databaseFacade, string sql, long page, long pageSize, IEnumerable<object> parameters)`**  
**`Task<QueryPage> ExecutePagedQueryAsync(this DatabaseFacade databaseFacade, FormattableString sql, long page, long pageSize, CancellationToken cancellationToken = default)`**  
**`Task<QueryPage> ExecutePagedQueryAsync(this DatabaseFacade databaseFacade, string sql, long page, long pageSize, CancellationToken cancellationToken = default, params object[] parameters)`**  
**`Task<QueryPage> ExecutePagedQueryAsync(this DatabaseFacade databaseFacade, string sql, long page, long pageSize, IEnumerable<object> parameters, CancellationToken cancellationToken = default)`**  

Executes a query and returns the specified page of results in an instance of the QueryPage class. PageSize is the number of records per page. If page * pageSize is greater than the last record, then the page is set to be the last page.

**`int ExecuteNonQuery(this DatabaseFacade databaseFacade, FormattableString sql)`**  
**`int ExecuteNonQuery(this DatabaseFacade databaseFacade, string sql, params object[] parameters)`**  
**`int ExecuteNonQuery(this DatabaseFacade databaseFacade, string sql, IEnumerable<object> parameters)`**  
**`Task<int> ExecuteNonQueryAsync(this DatabaseFacade databaseFacade, FormattableString sql, CancellationToken cancellationToken = default)`**  
**`Task<int> ExecuteNonQueryAsync(this DatabaseFacade databaseFacade, string sql, CancellationToken cancellationToken = default, params object[] parameters)`**  
**`Task<int> ExecuteNonQueryAsync(this DatabaseFacade databaseFacade, string sql, IEnumerable<object> parameters, CancellationToken cancellationToken = default)`**  

Executes a non-query (such as Update or Delete) and return the number of records changed. These methods are here for completeness and are in fact a wrapper round the EF Core ExecuteSqlxxxxx extensions.

**`long ExecuteInsert(this DatabaseFacade databaseFacade, FormattableString sql)`**  
**`long ExecuteInsert(this DatabaseFacade databaseFacade, string sql, params object[] parameters)`**  
**`long ExecuteInsert(this DatabaseFacade databaseFacade, string sql, IEnumerable<object> parameters)`**  
**`Task<long> ExecuteInsertAsync(this DatabaseFacade databaseFacade, FormattableString sql, CancellationToken cancellationToken = default)`**  
**`Task<long> ExecuteInsertAsync(this DatabaseFacade databaseFacade, string sql, CancellationToken cancellationToken = default, params object[] parameters)`**  
**`Task<long> ExecuteInsertAsync(this DatabaseFacade databaseFacade, string sql, IEnumerable<object> parameters, CancellationToken cancellationToken = default)`**  

Executes an insert statement and return the ID of the newly inserted record.

### Get Context Extensions

**`DbContext GetContext<TSource>(this IQueryable<TSource> source)`**

Gets the DbContext object that is used by the specified IQueryable<T> created by EF Core. Returns null if the IQueryable object is not associated with a context.

### Unique Constraint Extensions

**`DbContextOptionsBuilder UseUniqueConstraintInterceptor(this DbContextOptionsBuilder optionsBuilder)`**

This method extends the `DbContextOptionsBuilder` object. It must be called from within the `OnConfiguring` override method in the database context. It attaches an interceptor to the context that captures unique constraint failures and throws a `UniqueConstraintException`. The exception contains a `UniqueConstraintDetails` property called Details which holds the schema, table name and field name that have caused the issue. The original exception is returned in the InnerException property.

**`UniqueConstraintDetails GetUniqueConstraintDetails(this DbContext context, Exception e)`**
**`UniqueConstraintDetails GetUniqueConstraintDetailsAsync(this DbContext context, Exception e)`**

These methods extend the `DbContext` object. They are passed an exception and if it is a unique constraint failure then they return a `UniqueConstraintDetails` object which contains the schema, table and field names. If the exception is not the result of a unique constraint failing, then `null` is returned. These methods should be used in a catch block after executing some SQL (see `TestGetUniqueConstraintDetailsFromSqlTableAsync` in GetUniqueConstraintDetailsTests.cs for an example).
