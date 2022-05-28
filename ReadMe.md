# EntityFrameworkCore.Extensions

Copyright ©2021-2022 Mike King.   
Licensed using the MIT licence. See the License.txt file in the solution root for more information.

## Overview

EntityFameworkCore.Extensions is a set of extension methods for Entity Framework Core primarily for executing SQL commands.

## Pre-Requisites

The library and tests use .Net 6.0 and Entity Framework Core v6. 

## Limitations

* Only SQLite and SQL Server are supported. Support for other databases may be added in the future.

* Currently the DoesExist extensions only support checking if databases and tables exists. This will be extended to include other object types such as indexes, views and stored procedures at some point.

## Methods

#### Database Type Extensions

**`DatabaseType GetDatabaseType(this DatabaseFacade databaseFacade)`**  
**`DatabaseType GetDatabaseType(this MigrationBuilder migrationBuilder)`**

Extension methods for the DatabaseFacade (available from `context.Database`) or MigrationBuilder (available when performing a migration) that return the type of database in use: 

`DatabaseType.Unknown`  
`DatabaseType.Sqlite`  
`DatabaseType.SqlServer`

#### Does Exist Extensions

**`bool DoesDatabaseExist(this DatabaseFacade databaseFacade)`**
**`Task<bool> DoesDatabaseExistAsync(this DatabaseFacade databaseFacade, CancellationToken cancellationToken = default)`**

Returns a boolean indicating if the database referenced by the facade exists.

**`bool DoesTableExist(this DatabaseFacade databaseFacade, string tableName)`**
**`Task<bool> DoesTableExistAsync(this DatabaseFacade databaseFacade, string tableName, CancellationToken cancellationToken = default)`**

Returns a boolean indicating if the table specified exists.

#### Execute SQL Extensions

All Execute methods extend the DatabaseFacade object. They are available in both synchronous and asynchronous and can take an interpolated SQL string or a raw SQL string with parameters (see the test project for examples).

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
