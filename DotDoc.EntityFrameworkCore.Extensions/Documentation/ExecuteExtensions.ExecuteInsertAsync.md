#### [DotDoc\.EntityFrameworkCore\.Extensions](Home 'Home')
### [DotDoc\.EntityFrameworkCore\.Extensions\.Execute](DotDoc.EntityFrameworkCore.Extensions.Execute 'DotDoc\.EntityFrameworkCore\.Extensions\.Execute').[ExecuteExtensions](ExecuteExtensions 'DotDoc\.EntityFrameworkCore\.Extensions\.Execute\.ExecuteExtensions')

## ExecuteExtensions\.ExecuteInsertAsync Method

| Overloads | |
| :--- | :--- |
| [ExecuteInsertAsync&lt;T&gt;\(this DatabaseFacade, string, CancellationToken, IEnumerable&lt;object&gt;\)](ExecuteExtensions.ExecuteInsertAsync#DotDoc.EntityFrameworkCore.Extensions.Execute.ExecuteExtensions.ExecuteInsertAsync_T_(thisMicrosoft.EntityFrameworkCore.Infrastructure.DatabaseFacade,string,System.Threading.CancellationToken,System.Collections.Generic.IEnumerable_object_) 'DotDoc\.EntityFrameworkCore\.Extensions\.Execute\.ExecuteExtensions\.ExecuteInsertAsync\<T\>\(this Microsoft\.EntityFrameworkCore\.Infrastructure\.DatabaseFacade, string, System\.Threading\.CancellationToken, System\.Collections\.Generic\.IEnumerable\<object\>\)') | Executes an insert command\. |
| [ExecuteInsertAsync&lt;T&gt;\(this DatabaseFacade, FormattableString, CancellationToken\)](ExecuteExtensions.ExecuteInsertAsync#DotDoc.EntityFrameworkCore.Extensions.Execute.ExecuteExtensions.ExecuteInsertAsync_T_(thisMicrosoft.EntityFrameworkCore.Infrastructure.DatabaseFacade,System.FormattableString,System.Threading.CancellationToken) 'DotDoc\.EntityFrameworkCore\.Extensions\.Execute\.ExecuteExtensions\.ExecuteInsertAsync\<T\>\(this Microsoft\.EntityFrameworkCore\.Infrastructure\.DatabaseFacade, System\.FormattableString, System\.Threading\.CancellationToken\)') | Executes an insert command\. |

<a name='DotDoc.EntityFrameworkCore.Extensions.Execute.ExecuteExtensions.ExecuteInsertAsync_T_(thisMicrosoft.EntityFrameworkCore.Infrastructure.DatabaseFacade,string,System.Threading.CancellationToken,System.Collections.Generic.IEnumerable_object_)'></a>

## ExecuteExtensions\.ExecuteInsertAsync\<T\>\(this DatabaseFacade, string, CancellationToken, IEnumerable\<object\>\) Method

Executes an insert command\.

```csharp
public static System.Threading.Tasks.Task<T> ExecuteInsertAsync<T>(this Microsoft.EntityFrameworkCore.Infrastructure.DatabaseFacade database, string sql, System.Threading.CancellationToken cancellationToken=default(System.Threading.CancellationToken), System.Collections.Generic.IEnumerable<object?> parameters);
```
#### Type parameters

<a name='DotDoc.EntityFrameworkCore.Extensions.Execute.ExecuteExtensions.ExecuteInsertAsync_T_(thisMicrosoft.EntityFrameworkCore.Infrastructure.DatabaseFacade,string,System.Threading.CancellationToken,System.Collections.Generic.IEnumerable_object_).T'></a>

`T`

The type returned by the insert\.
#### Parameters

<a name='DotDoc.EntityFrameworkCore.Extensions.Execute.ExecuteExtensions.ExecuteInsertAsync_T_(thisMicrosoft.EntityFrameworkCore.Infrastructure.DatabaseFacade,string,System.Threading.CancellationToken,System.Collections.Generic.IEnumerable_object_).database'></a>

`database` [Microsoft\.EntityFrameworkCore\.Infrastructure\.DatabaseFacade](https://learn.microsoft.com/en-us/dotnet/api/microsoft.entityframeworkcore.infrastructure.databasefacade 'Microsoft\.EntityFrameworkCore\.Infrastructure\.DatabaseFacade')

The [Microsoft\.EntityFrameworkCore\.Infrastructure\.DatabaseFacade](https://learn.microsoft.com/en-us/dotnet/api/microsoft.entityframeworkcore.infrastructure.databasefacade 'Microsoft\.EntityFrameworkCore\.Infrastructure\.DatabaseFacade') for the context\.

<a name='DotDoc.EntityFrameworkCore.Extensions.Execute.ExecuteExtensions.ExecuteInsertAsync_T_(thisMicrosoft.EntityFrameworkCore.Infrastructure.DatabaseFacade,string,System.Threading.CancellationToken,System.Collections.Generic.IEnumerable_object_).sql'></a>

`sql` [System\.String](https://learn.microsoft.com/en-us/dotnet/api/system.string 'System\.String')

The SQL query to execute\.

<a name='DotDoc.EntityFrameworkCore.Extensions.Execute.ExecuteExtensions.ExecuteInsertAsync_T_(thisMicrosoft.EntityFrameworkCore.Infrastructure.DatabaseFacade,string,System.Threading.CancellationToken,System.Collections.Generic.IEnumerable_object_).cancellationToken'></a>

`cancellationToken` [System\.Threading\.CancellationToken](https://learn.microsoft.com/en-us/dotnet/api/system.threading.cancellationtoken 'System\.Threading\.CancellationToken')

A [System\.Threading\.CancellationToken](https://learn.microsoft.com/en-us/dotnet/api/system.threading.cancellationtoken 'System\.Threading\.CancellationToken') to observe while waiting for the task to complete\.

<a name='DotDoc.EntityFrameworkCore.Extensions.Execute.ExecuteExtensions.ExecuteInsertAsync_T_(thisMicrosoft.EntityFrameworkCore.Infrastructure.DatabaseFacade,string,System.Threading.CancellationToken,System.Collections.Generic.IEnumerable_object_).parameters'></a>

`parameters` [System\.Collections\.Generic\.IEnumerable&lt;](https://learn.microsoft.com/en-us/dotnet/api/system.collections.generic.ienumerable-1 'System\.Collections\.Generic\.IEnumerable\`1')[System\.Object](https://learn.microsoft.com/en-us/dotnet/api/system.object 'System\.Object')[&gt;](https://learn.microsoft.com/en-us/dotnet/api/system.collections.generic.ienumerable-1 'System\.Collections\.Generic\.IEnumerable\`1')

Parameters to use with the SQL\.

#### Returns
[System\.Threading\.Tasks\.Task&lt;](https://learn.microsoft.com/en-us/dotnet/api/system.threading.tasks.task-1 'System\.Threading\.Tasks\.Task\`1')[T](ExecuteExtensions#DotDoc.EntityFrameworkCore.Extensions.Execute.ExecuteExtensions.ExecuteInsertAsync_T_(thisMicrosoft.EntityFrameworkCore.Infrastructure.DatabaseFacade,string,System.Threading.CancellationToken,System.Collections.Generic.IEnumerable_object_).T 'DotDoc\.EntityFrameworkCore\.Extensions\.Execute\.ExecuteExtensions\.ExecuteInsertAsync\<T\>\(this Microsoft\.EntityFrameworkCore\.Infrastructure\.DatabaseFacade, string, System\.Threading\.CancellationToken, System\.Collections\.Generic\.IEnumerable\<object\>\)\.T')[&gt;](https://learn.microsoft.com/en-us/dotnet/api/system.threading.tasks.task-1 'System\.Threading\.Tasks\.Task\`1')  
The Id of the new record\.

<a name='DotDoc.EntityFrameworkCore.Extensions.Execute.ExecuteExtensions.ExecuteInsertAsync_T_(thisMicrosoft.EntityFrameworkCore.Infrastructure.DatabaseFacade,System.FormattableString,System.Threading.CancellationToken)'></a>

## ExecuteExtensions\.ExecuteInsertAsync\<T\>\(this DatabaseFacade, FormattableString, CancellationToken\) Method

Executes an insert command\.

```csharp
public static System.Threading.Tasks.Task<T> ExecuteInsertAsync<T>(this Microsoft.EntityFrameworkCore.Infrastructure.DatabaseFacade database, System.FormattableString sql, System.Threading.CancellationToken cancellationToken=default(System.Threading.CancellationToken));
```
#### Type parameters

<a name='DotDoc.EntityFrameworkCore.Extensions.Execute.ExecuteExtensions.ExecuteInsertAsync_T_(thisMicrosoft.EntityFrameworkCore.Infrastructure.DatabaseFacade,System.FormattableString,System.Threading.CancellationToken).T'></a>

`T`

The type returned by the insert\.
#### Parameters

<a name='DotDoc.EntityFrameworkCore.Extensions.Execute.ExecuteExtensions.ExecuteInsertAsync_T_(thisMicrosoft.EntityFrameworkCore.Infrastructure.DatabaseFacade,System.FormattableString,System.Threading.CancellationToken).database'></a>

`database` [Microsoft\.EntityFrameworkCore\.Infrastructure\.DatabaseFacade](https://learn.microsoft.com/en-us/dotnet/api/microsoft.entityframeworkcore.infrastructure.databasefacade 'Microsoft\.EntityFrameworkCore\.Infrastructure\.DatabaseFacade')

The [Microsoft\.EntityFrameworkCore\.Infrastructure\.DatabaseFacade](https://learn.microsoft.com/en-us/dotnet/api/microsoft.entityframeworkcore.infrastructure.databasefacade 'Microsoft\.EntityFrameworkCore\.Infrastructure\.DatabaseFacade') for the context\.

<a name='DotDoc.EntityFrameworkCore.Extensions.Execute.ExecuteExtensions.ExecuteInsertAsync_T_(thisMicrosoft.EntityFrameworkCore.Infrastructure.DatabaseFacade,System.FormattableString,System.Threading.CancellationToken).sql'></a>

`sql` [System\.FormattableString](https://learn.microsoft.com/en-us/dotnet/api/system.formattablestring 'System\.FormattableString')

The [System\.FormattableString](https://learn.microsoft.com/en-us/dotnet/api/system.formattablestring 'System\.FormattableString') representing a SQL query with parameters\.

<a name='DotDoc.EntityFrameworkCore.Extensions.Execute.ExecuteExtensions.ExecuteInsertAsync_T_(thisMicrosoft.EntityFrameworkCore.Infrastructure.DatabaseFacade,System.FormattableString,System.Threading.CancellationToken).cancellationToken'></a>

`cancellationToken` [System\.Threading\.CancellationToken](https://learn.microsoft.com/en-us/dotnet/api/system.threading.cancellationtoken 'System\.Threading\.CancellationToken')

A [System\.Threading\.CancellationToken](https://learn.microsoft.com/en-us/dotnet/api/system.threading.cancellationtoken 'System\.Threading\.CancellationToken') to observe while waiting for the task to complete\.

#### Returns
[System\.Threading\.Tasks\.Task&lt;](https://learn.microsoft.com/en-us/dotnet/api/system.threading.tasks.task-1 'System\.Threading\.Tasks\.Task\`1')[T](ExecuteExtensions#DotDoc.EntityFrameworkCore.Extensions.Execute.ExecuteExtensions.ExecuteInsertAsync_T_(thisMicrosoft.EntityFrameworkCore.Infrastructure.DatabaseFacade,System.FormattableString,System.Threading.CancellationToken).T 'DotDoc\.EntityFrameworkCore\.Extensions\.Execute\.ExecuteExtensions\.ExecuteInsertAsync\<T\>\(this Microsoft\.EntityFrameworkCore\.Infrastructure\.DatabaseFacade, System\.FormattableString, System\.Threading\.CancellationToken\)\.T')[&gt;](https://learn.microsoft.com/en-us/dotnet/api/system.threading.tasks.task-1 'System\.Threading\.Tasks\.Task\`1')  
The Id of the new record\.