#### [DotDoc\.EntityFrameworkCore\.Extensions](Home 'Home')
### [DotDoc\.EntityFrameworkCore\.Extensions\.Execute](DotDoc.EntityFrameworkCore.Extensions.Execute 'DotDoc\.EntityFrameworkCore\.Extensions\.Execute').[ExecuteExtensions](ExecuteExtensions 'DotDoc\.EntityFrameworkCore\.Extensions\.Execute\.ExecuteExtensions')

## ExecuteExtensions\.ExecuteQueryAsync Method

| Overloads | |
| :--- | :--- |
| [ExecuteQueryAsync&lt;TSource&gt;\(this DatabaseFacade, string, CancellationToken, IEnumerable&lt;object&gt;\)](ExecuteExtensions.ExecuteQueryAsync#DotDoc.EntityFrameworkCore.Extensions.Execute.ExecuteExtensions.ExecuteQueryAsync_TSource_(thisMicrosoft.EntityFrameworkCore.Infrastructure.DatabaseFacade,string,System.Threading.CancellationToken,System.Collections.Generic.IEnumerable_object_) 'DotDoc\.EntityFrameworkCore\.Extensions\.Execute\.ExecuteExtensions\.ExecuteQueryAsync\<TSource\>\(this Microsoft\.EntityFrameworkCore\.Infrastructure\.DatabaseFacade, string, System\.Threading\.CancellationToken, System\.Collections\.Generic\.IEnumerable\<object\>\)') | Executes a query\. |
| [ExecuteQueryAsync&lt;TSource&gt;\(this DatabaseFacade, FormattableString, CancellationToken\)](ExecuteExtensions.ExecuteQueryAsync#DotDoc.EntityFrameworkCore.Extensions.Execute.ExecuteExtensions.ExecuteQueryAsync_TSource_(thisMicrosoft.EntityFrameworkCore.Infrastructure.DatabaseFacade,System.FormattableString,System.Threading.CancellationToken) 'DotDoc\.EntityFrameworkCore\.Extensions\.Execute\.ExecuteExtensions\.ExecuteQueryAsync\<TSource\>\(this Microsoft\.EntityFrameworkCore\.Infrastructure\.DatabaseFacade, System\.FormattableString, System\.Threading\.CancellationToken\)') | Executes a query\. |

<a name='DotDoc.EntityFrameworkCore.Extensions.Execute.ExecuteExtensions.ExecuteQueryAsync_TSource_(thisMicrosoft.EntityFrameworkCore.Infrastructure.DatabaseFacade,string,System.Threading.CancellationToken,System.Collections.Generic.IEnumerable_object_)'></a>

## ExecuteExtensions\.ExecuteQueryAsync\<TSource\>\(this DatabaseFacade, string, CancellationToken, IEnumerable\<object\>\) Method

Executes a query\.

```csharp
public static System.Threading.Tasks.Task<System.Collections.Generic.IList<TSource>> ExecuteQueryAsync<TSource>(this Microsoft.EntityFrameworkCore.Infrastructure.DatabaseFacade database, string sql, System.Threading.CancellationToken cancellationToken=default(System.Threading.CancellationToken), System.Collections.Generic.IEnumerable<object?> parameters)
    where TSource : class;
```
#### Type parameters

<a name='DotDoc.EntityFrameworkCore.Extensions.Execute.ExecuteExtensions.ExecuteQueryAsync_TSource_(thisMicrosoft.EntityFrameworkCore.Infrastructure.DatabaseFacade,string,System.Threading.CancellationToken,System.Collections.Generic.IEnumerable_object_).TSource'></a>

`TSource`

The type of the elements of the source\.
#### Parameters

<a name='DotDoc.EntityFrameworkCore.Extensions.Execute.ExecuteExtensions.ExecuteQueryAsync_TSource_(thisMicrosoft.EntityFrameworkCore.Infrastructure.DatabaseFacade,string,System.Threading.CancellationToken,System.Collections.Generic.IEnumerable_object_).database'></a>

`database` [Microsoft\.EntityFrameworkCore\.Infrastructure\.DatabaseFacade](https://learn.microsoft.com/en-us/dotnet/api/microsoft.entityframeworkcore.infrastructure.databasefacade 'Microsoft\.EntityFrameworkCore\.Infrastructure\.DatabaseFacade')

The [Microsoft\.EntityFrameworkCore\.Infrastructure\.DatabaseFacade](https://learn.microsoft.com/en-us/dotnet/api/microsoft.entityframeworkcore.infrastructure.databasefacade 'Microsoft\.EntityFrameworkCore\.Infrastructure\.DatabaseFacade') for the context\.

<a name='DotDoc.EntityFrameworkCore.Extensions.Execute.ExecuteExtensions.ExecuteQueryAsync_TSource_(thisMicrosoft.EntityFrameworkCore.Infrastructure.DatabaseFacade,string,System.Threading.CancellationToken,System.Collections.Generic.IEnumerable_object_).sql'></a>

`sql` [System\.String](https://learn.microsoft.com/en-us/dotnet/api/system.string 'System\.String')

The SQL query to execute\.

<a name='DotDoc.EntityFrameworkCore.Extensions.Execute.ExecuteExtensions.ExecuteQueryAsync_TSource_(thisMicrosoft.EntityFrameworkCore.Infrastructure.DatabaseFacade,string,System.Threading.CancellationToken,System.Collections.Generic.IEnumerable_object_).cancellationToken'></a>

`cancellationToken` [System\.Threading\.CancellationToken](https://learn.microsoft.com/en-us/dotnet/api/system.threading.cancellationtoken 'System\.Threading\.CancellationToken')

A [System\.Threading\.CancellationToken](https://learn.microsoft.com/en-us/dotnet/api/system.threading.cancellationtoken 'System\.Threading\.CancellationToken') to observe while waiting for the task to complete\.

<a name='DotDoc.EntityFrameworkCore.Extensions.Execute.ExecuteExtensions.ExecuteQueryAsync_TSource_(thisMicrosoft.EntityFrameworkCore.Infrastructure.DatabaseFacade,string,System.Threading.CancellationToken,System.Collections.Generic.IEnumerable_object_).parameters'></a>

`parameters` [System\.Collections\.Generic\.IEnumerable&lt;](https://learn.microsoft.com/en-us/dotnet/api/system.collections.generic.ienumerable-1 'System\.Collections\.Generic\.IEnumerable\`1')[System\.Object](https://learn.microsoft.com/en-us/dotnet/api/system.object 'System\.Object')[&gt;](https://learn.microsoft.com/en-us/dotnet/api/system.collections.generic.ienumerable-1 'System\.Collections\.Generic\.IEnumerable\`1')

Parameters to use with the SQL\.

#### Returns
[System\.Threading\.Tasks\.Task&lt;](https://learn.microsoft.com/en-us/dotnet/api/system.threading.tasks.task-1 'System\.Threading\.Tasks\.Task\`1')[System\.Collections\.Generic\.IList&lt;](https://learn.microsoft.com/en-us/dotnet/api/system.collections.generic.ilist-1 'System\.Collections\.Generic\.IList\`1')[TSource](ExecuteExtensions#DotDoc.EntityFrameworkCore.Extensions.Execute.ExecuteExtensions.ExecuteQueryAsync_TSource_(thisMicrosoft.EntityFrameworkCore.Infrastructure.DatabaseFacade,string,System.Threading.CancellationToken,System.Collections.Generic.IEnumerable_object_).TSource 'DotDoc\.EntityFrameworkCore\.Extensions\.Execute\.ExecuteExtensions\.ExecuteQueryAsync\<TSource\>\(this Microsoft\.EntityFrameworkCore\.Infrastructure\.DatabaseFacade, string, System\.Threading\.CancellationToken, System\.Collections\.Generic\.IEnumerable\<object\>\)\.TSource')[&gt;](https://learn.microsoft.com/en-us/dotnet/api/system.collections.generic.ilist-1 'System\.Collections\.Generic\.IList\`1')[&gt;](https://learn.microsoft.com/en-us/dotnet/api/system.threading.tasks.task-1 'System\.Threading\.Tasks\.Task\`1')  
A [System\.Collections\.Generic\.IList&lt;&gt;](https://learn.microsoft.com/en-us/dotnet/api/system.collections.generic.ilist-1 'System\.Collections\.Generic\.IList\`1') containing the results of the query\.

<a name='DotDoc.EntityFrameworkCore.Extensions.Execute.ExecuteExtensions.ExecuteQueryAsync_TSource_(thisMicrosoft.EntityFrameworkCore.Infrastructure.DatabaseFacade,System.FormattableString,System.Threading.CancellationToken)'></a>

## ExecuteExtensions\.ExecuteQueryAsync\<TSource\>\(this DatabaseFacade, FormattableString, CancellationToken\) Method

Executes a query\.

```csharp
public static System.Threading.Tasks.Task<System.Collections.Generic.IList<TSource>> ExecuteQueryAsync<TSource>(this Microsoft.EntityFrameworkCore.Infrastructure.DatabaseFacade database, System.FormattableString sql, System.Threading.CancellationToken cancellationToken=default(System.Threading.CancellationToken))
    where TSource : class;
```
#### Type parameters

<a name='DotDoc.EntityFrameworkCore.Extensions.Execute.ExecuteExtensions.ExecuteQueryAsync_TSource_(thisMicrosoft.EntityFrameworkCore.Infrastructure.DatabaseFacade,System.FormattableString,System.Threading.CancellationToken).TSource'></a>

`TSource`

The type of the elements of the source\.
#### Parameters

<a name='DotDoc.EntityFrameworkCore.Extensions.Execute.ExecuteExtensions.ExecuteQueryAsync_TSource_(thisMicrosoft.EntityFrameworkCore.Infrastructure.DatabaseFacade,System.FormattableString,System.Threading.CancellationToken).database'></a>

`database` [Microsoft\.EntityFrameworkCore\.Infrastructure\.DatabaseFacade](https://learn.microsoft.com/en-us/dotnet/api/microsoft.entityframeworkcore.infrastructure.databasefacade 'Microsoft\.EntityFrameworkCore\.Infrastructure\.DatabaseFacade')

The [Microsoft\.EntityFrameworkCore\.Infrastructure\.DatabaseFacade](https://learn.microsoft.com/en-us/dotnet/api/microsoft.entityframeworkcore.infrastructure.databasefacade 'Microsoft\.EntityFrameworkCore\.Infrastructure\.DatabaseFacade') for the context\.

<a name='DotDoc.EntityFrameworkCore.Extensions.Execute.ExecuteExtensions.ExecuteQueryAsync_TSource_(thisMicrosoft.EntityFrameworkCore.Infrastructure.DatabaseFacade,System.FormattableString,System.Threading.CancellationToken).sql'></a>

`sql` [System\.FormattableString](https://learn.microsoft.com/en-us/dotnet/api/system.formattablestring 'System\.FormattableString')

The [System\.FormattableString](https://learn.microsoft.com/en-us/dotnet/api/system.formattablestring 'System\.FormattableString') representing a SQL query with parameters\.

<a name='DotDoc.EntityFrameworkCore.Extensions.Execute.ExecuteExtensions.ExecuteQueryAsync_TSource_(thisMicrosoft.EntityFrameworkCore.Infrastructure.DatabaseFacade,System.FormattableString,System.Threading.CancellationToken).cancellationToken'></a>

`cancellationToken` [System\.Threading\.CancellationToken](https://learn.microsoft.com/en-us/dotnet/api/system.threading.cancellationtoken 'System\.Threading\.CancellationToken')

A [System\.Threading\.CancellationToken](https://learn.microsoft.com/en-us/dotnet/api/system.threading.cancellationtoken 'System\.Threading\.CancellationToken') to observe while waiting for the task to complete\.

#### Returns
[System\.Threading\.Tasks\.Task&lt;](https://learn.microsoft.com/en-us/dotnet/api/system.threading.tasks.task-1 'System\.Threading\.Tasks\.Task\`1')[System\.Collections\.Generic\.IList&lt;](https://learn.microsoft.com/en-us/dotnet/api/system.collections.generic.ilist-1 'System\.Collections\.Generic\.IList\`1')[TSource](ExecuteExtensions#DotDoc.EntityFrameworkCore.Extensions.Execute.ExecuteExtensions.ExecuteQueryAsync_TSource_(thisMicrosoft.EntityFrameworkCore.Infrastructure.DatabaseFacade,System.FormattableString,System.Threading.CancellationToken).TSource 'DotDoc\.EntityFrameworkCore\.Extensions\.Execute\.ExecuteExtensions\.ExecuteQueryAsync\<TSource\>\(this Microsoft\.EntityFrameworkCore\.Infrastructure\.DatabaseFacade, System\.FormattableString, System\.Threading\.CancellationToken\)\.TSource')[&gt;](https://learn.microsoft.com/en-us/dotnet/api/system.collections.generic.ilist-1 'System\.Collections\.Generic\.IList\`1')[&gt;](https://learn.microsoft.com/en-us/dotnet/api/system.threading.tasks.task-1 'System\.Threading\.Tasks\.Task\`1')  
A [System\.Collections\.Generic\.IList&lt;&gt;](https://learn.microsoft.com/en-us/dotnet/api/system.collections.generic.ilist-1 'System\.Collections\.Generic\.IList\`1') containing the results of the query\.