#### [DotDoc\.EntityFrameworkCore\.Extensions](Home 'Home')
### [DotDoc\.EntityFrameworkCore\.Extensions\.Execute](DotDoc.EntityFrameworkCore.Extensions.Execute 'DotDoc\.EntityFrameworkCore\.Extensions\.Execute').[ExecuteExtensions](ExecuteExtensions 'DotDoc\.EntityFrameworkCore\.Extensions\.Execute\.ExecuteExtensions')

## ExecuteExtensions\.ExecutePagedQueryAsync Method

| Overloads | |
| :--- | :--- |
| [ExecutePagedQueryAsync&lt;TSource&gt;\(this DatabaseFacade, string, long, long, CancellationToken, IEnumerable&lt;object&gt;\)](ExecuteExtensions.ExecutePagedQueryAsync#DotDoc.EntityFrameworkCore.Extensions.Execute.ExecuteExtensions.ExecutePagedQueryAsync_TSource_(thisMicrosoft.EntityFrameworkCore.Infrastructure.DatabaseFacade,string,long,long,System.Threading.CancellationToken,System.Collections.Generic.IEnumerable_object_) 'DotDoc\.EntityFrameworkCore\.Extensions\.Execute\.ExecuteExtensions\.ExecutePagedQueryAsync\<TSource\>\(this Microsoft\.EntityFrameworkCore\.Infrastructure\.DatabaseFacade, string, long, long, System\.Threading\.CancellationToken, System\.Collections\.Generic\.IEnumerable\<object\>\)') | Executes a query and returns the specified page of results\. |
| [ExecutePagedQueryAsync&lt;TSource&gt;\(this DatabaseFacade, FormattableString, long, long, CancellationToken\)](ExecuteExtensions.ExecutePagedQueryAsync#DotDoc.EntityFrameworkCore.Extensions.Execute.ExecuteExtensions.ExecutePagedQueryAsync_TSource_(thisMicrosoft.EntityFrameworkCore.Infrastructure.DatabaseFacade,System.FormattableString,long,long,System.Threading.CancellationToken) 'DotDoc\.EntityFrameworkCore\.Extensions\.Execute\.ExecuteExtensions\.ExecutePagedQueryAsync\<TSource\>\(this Microsoft\.EntityFrameworkCore\.Infrastructure\.DatabaseFacade, System\.FormattableString, long, long, System\.Threading\.CancellationToken\)') | Executes a query and returns the specified page of results\. |

<a name='DotDoc.EntityFrameworkCore.Extensions.Execute.ExecuteExtensions.ExecutePagedQueryAsync_TSource_(thisMicrosoft.EntityFrameworkCore.Infrastructure.DatabaseFacade,string,long,long,System.Threading.CancellationToken,System.Collections.Generic.IEnumerable_object_)'></a>

## ExecuteExtensions\.ExecutePagedQueryAsync\<TSource\>\(this DatabaseFacade, string, long, long, CancellationToken, IEnumerable\<object\>\) Method

Executes a query and returns the specified page of results\.

```csharp
public static System.Threading.Tasks.Task<DotDoc.EntityFrameworkCore.Extensions.Execute.PagedQueryResult<TSource>> ExecutePagedQueryAsync<TSource>(this Microsoft.EntityFrameworkCore.Infrastructure.DatabaseFacade database, string sql, long page, long pageSize, System.Threading.CancellationToken cancellationToken=default(System.Threading.CancellationToken), System.Collections.Generic.IEnumerable<object?> parameters)
    where TSource : class;
```
#### Type parameters

<a name='DotDoc.EntityFrameworkCore.Extensions.Execute.ExecuteExtensions.ExecutePagedQueryAsync_TSource_(thisMicrosoft.EntityFrameworkCore.Infrastructure.DatabaseFacade,string,long,long,System.Threading.CancellationToken,System.Collections.Generic.IEnumerable_object_).TSource'></a>

`TSource`

The type of the elements of the source\.
#### Parameters

<a name='DotDoc.EntityFrameworkCore.Extensions.Execute.ExecuteExtensions.ExecutePagedQueryAsync_TSource_(thisMicrosoft.EntityFrameworkCore.Infrastructure.DatabaseFacade,string,long,long,System.Threading.CancellationToken,System.Collections.Generic.IEnumerable_object_).database'></a>

`database` [Microsoft\.EntityFrameworkCore\.Infrastructure\.DatabaseFacade](https://learn.microsoft.com/en-us/dotnet/api/microsoft.entityframeworkcore.infrastructure.databasefacade 'Microsoft\.EntityFrameworkCore\.Infrastructure\.DatabaseFacade')

The [Microsoft\.EntityFrameworkCore\.Infrastructure\.DatabaseFacade](https://learn.microsoft.com/en-us/dotnet/api/microsoft.entityframeworkcore.infrastructure.databasefacade 'Microsoft\.EntityFrameworkCore\.Infrastructure\.DatabaseFacade') for the context\.

<a name='DotDoc.EntityFrameworkCore.Extensions.Execute.ExecuteExtensions.ExecutePagedQueryAsync_TSource_(thisMicrosoft.EntityFrameworkCore.Infrastructure.DatabaseFacade,string,long,long,System.Threading.CancellationToken,System.Collections.Generic.IEnumerable_object_).sql'></a>

`sql` [System\.String](https://learn.microsoft.com/en-us/dotnet/api/system.string 'System\.String')

The SQL query to execute\.

<a name='DotDoc.EntityFrameworkCore.Extensions.Execute.ExecuteExtensions.ExecutePagedQueryAsync_TSource_(thisMicrosoft.EntityFrameworkCore.Infrastructure.DatabaseFacade,string,long,long,System.Threading.CancellationToken,System.Collections.Generic.IEnumerable_object_).page'></a>

`page` [System\.Int64](https://learn.microsoft.com/en-us/dotnet/api/system.int64 'System\.Int64')

Page number to return \(starting at 0\)\.

<a name='DotDoc.EntityFrameworkCore.Extensions.Execute.ExecuteExtensions.ExecutePagedQueryAsync_TSource_(thisMicrosoft.EntityFrameworkCore.Infrastructure.DatabaseFacade,string,long,long,System.Threading.CancellationToken,System.Collections.Generic.IEnumerable_object_).pageSize'></a>

`pageSize` [System\.Int64](https://learn.microsoft.com/en-us/dotnet/api/system.int64 'System\.Int64')

Number of records per page\.

<a name='DotDoc.EntityFrameworkCore.Extensions.Execute.ExecuteExtensions.ExecutePagedQueryAsync_TSource_(thisMicrosoft.EntityFrameworkCore.Infrastructure.DatabaseFacade,string,long,long,System.Threading.CancellationToken,System.Collections.Generic.IEnumerable_object_).cancellationToken'></a>

`cancellationToken` [System\.Threading\.CancellationToken](https://learn.microsoft.com/en-us/dotnet/api/system.threading.cancellationtoken 'System\.Threading\.CancellationToken')

A [System\.Threading\.CancellationToken](https://learn.microsoft.com/en-us/dotnet/api/system.threading.cancellationtoken 'System\.Threading\.CancellationToken') to observe while waiting for the task to complete\.

<a name='DotDoc.EntityFrameworkCore.Extensions.Execute.ExecuteExtensions.ExecutePagedQueryAsync_TSource_(thisMicrosoft.EntityFrameworkCore.Infrastructure.DatabaseFacade,string,long,long,System.Threading.CancellationToken,System.Collections.Generic.IEnumerable_object_).parameters'></a>

`parameters` [System\.Collections\.Generic\.IEnumerable&lt;](https://learn.microsoft.com/en-us/dotnet/api/system.collections.generic.ienumerable-1 'System\.Collections\.Generic\.IEnumerable\`1')[System\.Object](https://learn.microsoft.com/en-us/dotnet/api/system.object 'System\.Object')[&gt;](https://learn.microsoft.com/en-us/dotnet/api/system.collections.generic.ienumerable-1 'System\.Collections\.Generic\.IEnumerable\`1')

Parameters to use with the SQL\.

#### Returns
[System\.Threading\.Tasks\.Task&lt;](https://learn.microsoft.com/en-us/dotnet/api/system.threading.tasks.task-1 'System\.Threading\.Tasks\.Task\`1')[DotDoc\.EntityFrameworkCore\.Extensions\.Execute\.PagedQueryResult&lt;](PagedQueryResult_TSource_ 'DotDoc\.EntityFrameworkCore\.Extensions\.Execute\.PagedQueryResult\<TSource\>')[TSource](ExecuteExtensions#DotDoc.EntityFrameworkCore.Extensions.Execute.ExecuteExtensions.ExecutePagedQueryAsync_TSource_(thisMicrosoft.EntityFrameworkCore.Infrastructure.DatabaseFacade,string,long,long,System.Threading.CancellationToken,System.Collections.Generic.IEnumerable_object_).TSource 'DotDoc\.EntityFrameworkCore\.Extensions\.Execute\.ExecuteExtensions\.ExecutePagedQueryAsync\<TSource\>\(this Microsoft\.EntityFrameworkCore\.Infrastructure\.DatabaseFacade, string, long, long, System\.Threading\.CancellationToken, System\.Collections\.Generic\.IEnumerable\<object\>\)\.TSource')[&gt;](PagedQueryResult_TSource_ 'DotDoc\.EntityFrameworkCore\.Extensions\.Execute\.PagedQueryResult\<TSource\>')[&gt;](https://learn.microsoft.com/en-us/dotnet/api/system.threading.tasks.task-1 'System\.Threading\.Tasks\.Task\`1')  
An instance of [PagedQueryResult&lt;TSource&gt;](PagedQueryResult_TSource_ 'DotDoc\.EntityFrameworkCore\.Extensions\.Execute\.PagedQueryResult\<TSource\>') containing the page data \(If the page number is past the end of the table then the it will become the last page\)\.

<a name='DotDoc.EntityFrameworkCore.Extensions.Execute.ExecuteExtensions.ExecutePagedQueryAsync_TSource_(thisMicrosoft.EntityFrameworkCore.Infrastructure.DatabaseFacade,System.FormattableString,long,long,System.Threading.CancellationToken)'></a>

## ExecuteExtensions\.ExecutePagedQueryAsync\<TSource\>\(this DatabaseFacade, FormattableString, long, long, CancellationToken\) Method

Executes a query and returns the specified page of results\.

```csharp
public static System.Threading.Tasks.Task<DotDoc.EntityFrameworkCore.Extensions.Execute.PagedQueryResult<TSource>> ExecutePagedQueryAsync<TSource>(this Microsoft.EntityFrameworkCore.Infrastructure.DatabaseFacade database, System.FormattableString sql, long page, long pageSize, System.Threading.CancellationToken cancellationToken=default(System.Threading.CancellationToken))
    where TSource : class;
```
#### Type parameters

<a name='DotDoc.EntityFrameworkCore.Extensions.Execute.ExecuteExtensions.ExecutePagedQueryAsync_TSource_(thisMicrosoft.EntityFrameworkCore.Infrastructure.DatabaseFacade,System.FormattableString,long,long,System.Threading.CancellationToken).TSource'></a>

`TSource`

The type of the elements of the source\.
#### Parameters

<a name='DotDoc.EntityFrameworkCore.Extensions.Execute.ExecuteExtensions.ExecutePagedQueryAsync_TSource_(thisMicrosoft.EntityFrameworkCore.Infrastructure.DatabaseFacade,System.FormattableString,long,long,System.Threading.CancellationToken).database'></a>

`database` [Microsoft\.EntityFrameworkCore\.Infrastructure\.DatabaseFacade](https://learn.microsoft.com/en-us/dotnet/api/microsoft.entityframeworkcore.infrastructure.databasefacade 'Microsoft\.EntityFrameworkCore\.Infrastructure\.DatabaseFacade')

The [Microsoft\.EntityFrameworkCore\.Infrastructure\.DatabaseFacade](https://learn.microsoft.com/en-us/dotnet/api/microsoft.entityframeworkcore.infrastructure.databasefacade 'Microsoft\.EntityFrameworkCore\.Infrastructure\.DatabaseFacade') for the context\.

<a name='DotDoc.EntityFrameworkCore.Extensions.Execute.ExecuteExtensions.ExecutePagedQueryAsync_TSource_(thisMicrosoft.EntityFrameworkCore.Infrastructure.DatabaseFacade,System.FormattableString,long,long,System.Threading.CancellationToken).sql'></a>

`sql` [System\.FormattableString](https://learn.microsoft.com/en-us/dotnet/api/system.formattablestring 'System\.FormattableString')

The [System\.FormattableString](https://learn.microsoft.com/en-us/dotnet/api/system.formattablestring 'System\.FormattableString') representing a SQL query with parameters\.

<a name='DotDoc.EntityFrameworkCore.Extensions.Execute.ExecuteExtensions.ExecutePagedQueryAsync_TSource_(thisMicrosoft.EntityFrameworkCore.Infrastructure.DatabaseFacade,System.FormattableString,long,long,System.Threading.CancellationToken).page'></a>

`page` [System\.Int64](https://learn.microsoft.com/en-us/dotnet/api/system.int64 'System\.Int64')

Page number to return \(starting at 0\)\.

<a name='DotDoc.EntityFrameworkCore.Extensions.Execute.ExecuteExtensions.ExecutePagedQueryAsync_TSource_(thisMicrosoft.EntityFrameworkCore.Infrastructure.DatabaseFacade,System.FormattableString,long,long,System.Threading.CancellationToken).pageSize'></a>

`pageSize` [System\.Int64](https://learn.microsoft.com/en-us/dotnet/api/system.int64 'System\.Int64')

Number of records per page\.

<a name='DotDoc.EntityFrameworkCore.Extensions.Execute.ExecuteExtensions.ExecutePagedQueryAsync_TSource_(thisMicrosoft.EntityFrameworkCore.Infrastructure.DatabaseFacade,System.FormattableString,long,long,System.Threading.CancellationToken).cancellationToken'></a>

`cancellationToken` [System\.Threading\.CancellationToken](https://learn.microsoft.com/en-us/dotnet/api/system.threading.cancellationtoken 'System\.Threading\.CancellationToken')

A [System\.Threading\.CancellationToken](https://learn.microsoft.com/en-us/dotnet/api/system.threading.cancellationtoken 'System\.Threading\.CancellationToken') to observe while waiting for the task to complete\.

#### Returns
[System\.Threading\.Tasks\.Task&lt;](https://learn.microsoft.com/en-us/dotnet/api/system.threading.tasks.task-1 'System\.Threading\.Tasks\.Task\`1')[DotDoc\.EntityFrameworkCore\.Extensions\.Execute\.PagedQueryResult&lt;](PagedQueryResult_TSource_ 'DotDoc\.EntityFrameworkCore\.Extensions\.Execute\.PagedQueryResult\<TSource\>')[TSource](ExecuteExtensions#DotDoc.EntityFrameworkCore.Extensions.Execute.ExecuteExtensions.ExecutePagedQueryAsync_TSource_(thisMicrosoft.EntityFrameworkCore.Infrastructure.DatabaseFacade,System.FormattableString,long,long,System.Threading.CancellationToken).TSource 'DotDoc\.EntityFrameworkCore\.Extensions\.Execute\.ExecuteExtensions\.ExecutePagedQueryAsync\<TSource\>\(this Microsoft\.EntityFrameworkCore\.Infrastructure\.DatabaseFacade, System\.FormattableString, long, long, System\.Threading\.CancellationToken\)\.TSource')[&gt;](PagedQueryResult_TSource_ 'DotDoc\.EntityFrameworkCore\.Extensions\.Execute\.PagedQueryResult\<TSource\>')[&gt;](https://learn.microsoft.com/en-us/dotnet/api/system.threading.tasks.task-1 'System\.Threading\.Tasks\.Task\`1')  
An instance of [PagedQueryResult&lt;TSource&gt;](PagedQueryResult_TSource_ 'DotDoc\.EntityFrameworkCore\.Extensions\.Execute\.PagedQueryResult\<TSource\>') containing the page data \(If the page number is past the end of the table then the it will become the last page\)\.