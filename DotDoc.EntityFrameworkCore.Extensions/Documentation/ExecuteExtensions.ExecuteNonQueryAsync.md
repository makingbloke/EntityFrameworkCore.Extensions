#### [DotDoc\.EntityFrameworkCore\.Extensions](Home.md 'Home')
### [DotDoc\.EntityFrameworkCore\.Extensions\.Execute](DotDoc.EntityFrameworkCore.Extensions.Execute.md 'DotDoc\.EntityFrameworkCore\.Extensions\.Execute').[ExecuteExtensions](ExecuteExtensions.md 'DotDoc\.EntityFrameworkCore\.Extensions\.Execute\.ExecuteExtensions')

## ExecuteExtensions\.ExecuteNonQueryAsync Method

| Overloads | |
| :--- | :--- |
| [ExecuteNonQueryAsync\(this DatabaseFacade, string, CancellationToken, IEnumerable&lt;object&gt;\)](ExecuteExtensions.ExecuteNonQueryAsync.md#DotDoc.EntityFrameworkCore.Extensions.Execute.ExecuteExtensions.ExecuteNonQueryAsync(thisMicrosoft.EntityFrameworkCore.Infrastructure.DatabaseFacade,string,System.Threading.CancellationToken,System.Collections.Generic.IEnumerable_object_) 'DotDoc\.EntityFrameworkCore\.Extensions\.Execute\.ExecuteExtensions\.ExecuteNonQueryAsync\(this Microsoft\.EntityFrameworkCore\.Infrastructure\.DatabaseFacade, string, System\.Threading\.CancellationToken, System\.Collections\.Generic\.IEnumerable\<object\>\)') | Executes a non query\. |
| [ExecuteNonQueryAsync\(this DatabaseFacade, FormattableString, CancellationToken\)](ExecuteExtensions.ExecuteNonQueryAsync.md#DotDoc.EntityFrameworkCore.Extensions.Execute.ExecuteExtensions.ExecuteNonQueryAsync(thisMicrosoft.EntityFrameworkCore.Infrastructure.DatabaseFacade,System.FormattableString,System.Threading.CancellationToken) 'DotDoc\.EntityFrameworkCore\.Extensions\.Execute\.ExecuteExtensions\.ExecuteNonQueryAsync\(this Microsoft\.EntityFrameworkCore\.Infrastructure\.DatabaseFacade, System\.FormattableString, System\.Threading\.CancellationToken\)') | Executes a non query\. |

<a name='DotDoc.EntityFrameworkCore.Extensions.Execute.ExecuteExtensions.ExecuteNonQueryAsync(thisMicrosoft.EntityFrameworkCore.Infrastructure.DatabaseFacade,string,System.Threading.CancellationToken,System.Collections.Generic.IEnumerable_object_)'></a>

## ExecuteExtensions\.ExecuteNonQueryAsync\(this DatabaseFacade, string, CancellationToken, IEnumerable\<object\>\) Method

Executes a non query\.

```csharp
public static System.Threading.Tasks.Task<int> ExecuteNonQueryAsync(this Microsoft.EntityFrameworkCore.Infrastructure.DatabaseFacade database, string sql, System.Threading.CancellationToken cancellationToken=default(System.Threading.CancellationToken), System.Collections.Generic.IEnumerable<object?> parameters);
```
#### Parameters

<a name='DotDoc.EntityFrameworkCore.Extensions.Execute.ExecuteExtensions.ExecuteNonQueryAsync(thisMicrosoft.EntityFrameworkCore.Infrastructure.DatabaseFacade,string,System.Threading.CancellationToken,System.Collections.Generic.IEnumerable_object_).database'></a>

`database` [Microsoft\.EntityFrameworkCore\.Infrastructure\.DatabaseFacade](https://learn.microsoft.com/en-us/dotnet/api/microsoft.entityframeworkcore.infrastructure.databasefacade 'Microsoft\.EntityFrameworkCore\.Infrastructure\.DatabaseFacade')

The [Microsoft\.EntityFrameworkCore\.Infrastructure\.DatabaseFacade](https://learn.microsoft.com/en-us/dotnet/api/microsoft.entityframeworkcore.infrastructure.databasefacade 'Microsoft\.EntityFrameworkCore\.Infrastructure\.DatabaseFacade') for the context\.

<a name='DotDoc.EntityFrameworkCore.Extensions.Execute.ExecuteExtensions.ExecuteNonQueryAsync(thisMicrosoft.EntityFrameworkCore.Infrastructure.DatabaseFacade,string,System.Threading.CancellationToken,System.Collections.Generic.IEnumerable_object_).sql'></a>

`sql` [System\.String](https://learn.microsoft.com/en-us/dotnet/api/system.string 'System\.String')

The SQL query to execute\.

<a name='DotDoc.EntityFrameworkCore.Extensions.Execute.ExecuteExtensions.ExecuteNonQueryAsync(thisMicrosoft.EntityFrameworkCore.Infrastructure.DatabaseFacade,string,System.Threading.CancellationToken,System.Collections.Generic.IEnumerable_object_).cancellationToken'></a>

`cancellationToken` [System\.Threading\.CancellationToken](https://learn.microsoft.com/en-us/dotnet/api/system.threading.cancellationtoken 'System\.Threading\.CancellationToken')

A [System\.Threading\.CancellationToken](https://learn.microsoft.com/en-us/dotnet/api/system.threading.cancellationtoken 'System\.Threading\.CancellationToken') to observe while waiting for the task to complete\.

<a name='DotDoc.EntityFrameworkCore.Extensions.Execute.ExecuteExtensions.ExecuteNonQueryAsync(thisMicrosoft.EntityFrameworkCore.Infrastructure.DatabaseFacade,string,System.Threading.CancellationToken,System.Collections.Generic.IEnumerable_object_).parameters'></a>

`parameters` [System\.Collections\.Generic\.IEnumerable&lt;](https://learn.microsoft.com/en-us/dotnet/api/system.collections.generic.ienumerable-1 'System\.Collections\.Generic\.IEnumerable\`1')[System\.Object](https://learn.microsoft.com/en-us/dotnet/api/system.object 'System\.Object')[&gt;](https://learn.microsoft.com/en-us/dotnet/api/system.collections.generic.ienumerable-1 'System\.Collections\.Generic\.IEnumerable\`1')

Parameters to use with the SQL\.

#### Returns
[System\.Threading\.Tasks\.Task&lt;](https://learn.microsoft.com/en-us/dotnet/api/system.threading.tasks.task-1 'System\.Threading\.Tasks\.Task\`1')[System\.Int32](https://learn.microsoft.com/en-us/dotnet/api/system.int32 'System\.Int32')[&gt;](https://learn.microsoft.com/en-us/dotnet/api/system.threading.tasks.task-1 'System\.Threading\.Tasks\.Task\`1')  
The number of rows affected\.

<a name='DotDoc.EntityFrameworkCore.Extensions.Execute.ExecuteExtensions.ExecuteNonQueryAsync(thisMicrosoft.EntityFrameworkCore.Infrastructure.DatabaseFacade,System.FormattableString,System.Threading.CancellationToken)'></a>

## ExecuteExtensions\.ExecuteNonQueryAsync\(this DatabaseFacade, FormattableString, CancellationToken\) Method

Executes a non query\.

```csharp
public static System.Threading.Tasks.Task<int> ExecuteNonQueryAsync(this Microsoft.EntityFrameworkCore.Infrastructure.DatabaseFacade database, System.FormattableString sql, System.Threading.CancellationToken cancellationToken=default(System.Threading.CancellationToken));
```
#### Parameters

<a name='DotDoc.EntityFrameworkCore.Extensions.Execute.ExecuteExtensions.ExecuteNonQueryAsync(thisMicrosoft.EntityFrameworkCore.Infrastructure.DatabaseFacade,System.FormattableString,System.Threading.CancellationToken).database'></a>

`database` [Microsoft\.EntityFrameworkCore\.Infrastructure\.DatabaseFacade](https://learn.microsoft.com/en-us/dotnet/api/microsoft.entityframeworkcore.infrastructure.databasefacade 'Microsoft\.EntityFrameworkCore\.Infrastructure\.DatabaseFacade')

The [Microsoft\.EntityFrameworkCore\.Infrastructure\.DatabaseFacade](https://learn.microsoft.com/en-us/dotnet/api/microsoft.entityframeworkcore.infrastructure.databasefacade 'Microsoft\.EntityFrameworkCore\.Infrastructure\.DatabaseFacade') for the context\.

<a name='DotDoc.EntityFrameworkCore.Extensions.Execute.ExecuteExtensions.ExecuteNonQueryAsync(thisMicrosoft.EntityFrameworkCore.Infrastructure.DatabaseFacade,System.FormattableString,System.Threading.CancellationToken).sql'></a>

`sql` [System\.FormattableString](https://learn.microsoft.com/en-us/dotnet/api/system.formattablestring 'System\.FormattableString')

The [System\.FormattableString](https://learn.microsoft.com/en-us/dotnet/api/system.formattablestring 'System\.FormattableString') representing a SQL query with parameters\.

<a name='DotDoc.EntityFrameworkCore.Extensions.Execute.ExecuteExtensions.ExecuteNonQueryAsync(thisMicrosoft.EntityFrameworkCore.Infrastructure.DatabaseFacade,System.FormattableString,System.Threading.CancellationToken).cancellationToken'></a>

`cancellationToken` [System\.Threading\.CancellationToken](https://learn.microsoft.com/en-us/dotnet/api/system.threading.cancellationtoken 'System\.Threading\.CancellationToken')

A [System\.Threading\.CancellationToken](https://learn.microsoft.com/en-us/dotnet/api/system.threading.cancellationtoken 'System\.Threading\.CancellationToken') to observe while waiting for the task to complete\.

#### Returns
[System\.Threading\.Tasks\.Task&lt;](https://learn.microsoft.com/en-us/dotnet/api/system.threading.tasks.task-1 'System\.Threading\.Tasks\.Task\`1')[System\.Int32](https://learn.microsoft.com/en-us/dotnet/api/system.int32 'System\.Int32')[&gt;](https://learn.microsoft.com/en-us/dotnet/api/system.threading.tasks.task-1 'System\.Threading\.Tasks\.Task\`1')  
The number of rows affected\.