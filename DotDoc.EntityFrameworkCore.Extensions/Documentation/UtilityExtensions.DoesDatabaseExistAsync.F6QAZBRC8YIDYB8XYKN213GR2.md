#### [DotDoc\.EntityFrameworkCore\.Extensions](Home 'Home')
### [DotDoc\.EntityFrameworkCore\.Extensions\.Utilities](DotDoc.EntityFrameworkCore.Extensions.Utilities 'DotDoc\.EntityFrameworkCore\.Extensions\.Utilities').[UtilityExtensions](UtilityExtensions 'DotDoc\.EntityFrameworkCore\.Extensions\.Utilities\.UtilityExtensions')

## UtilityExtensions\.DoesDatabaseExistAsync\(this DatabaseFacade, CancellationToken\) Method

Check if database exists\.

```csharp
public static System.Threading.Tasks.Task<bool> DoesDatabaseExistAsync(this Microsoft.EntityFrameworkCore.Infrastructure.DatabaseFacade database, System.Threading.CancellationToken cancellationToken=default(System.Threading.CancellationToken));
```
#### Parameters

<a name='DotDoc.EntityFrameworkCore.Extensions.Utilities.UtilityExtensions.DoesDatabaseExistAsync(thisMicrosoft.EntityFrameworkCore.Infrastructure.DatabaseFacade,System.Threading.CancellationToken).database'></a>

`database` [Microsoft\.EntityFrameworkCore\.Infrastructure\.DatabaseFacade](https://learn.microsoft.com/en-us/dotnet/api/microsoft.entityframeworkcore.infrastructure.databasefacade 'Microsoft\.EntityFrameworkCore\.Infrastructure\.DatabaseFacade')

The [Microsoft\.EntityFrameworkCore\.Infrastructure\.DatabaseFacade](https://learn.microsoft.com/en-us/dotnet/api/microsoft.entityframeworkcore.infrastructure.databasefacade 'Microsoft\.EntityFrameworkCore\.Infrastructure\.DatabaseFacade') for the context\.

<a name='DotDoc.EntityFrameworkCore.Extensions.Utilities.UtilityExtensions.DoesDatabaseExistAsync(thisMicrosoft.EntityFrameworkCore.Infrastructure.DatabaseFacade,System.Threading.CancellationToken).cancellationToken'></a>

`cancellationToken` [System\.Threading\.CancellationToken](https://learn.microsoft.com/en-us/dotnet/api/system.threading.cancellationtoken 'System\.Threading\.CancellationToken')

A [System\.Threading\.CancellationToken](https://learn.microsoft.com/en-us/dotnet/api/system.threading.cancellationtoken 'System\.Threading\.CancellationToken') to observe while waiting for the task to complete\.

#### Returns
[System\.Threading\.Tasks\.Task&lt;](https://learn.microsoft.com/en-us/dotnet/api/system.threading.tasks.task-1 'System\.Threading\.Tasks\.Task\`1')[System\.Boolean](https://learn.microsoft.com/en-us/dotnet/api/system.boolean 'System\.Boolean')[&gt;](https://learn.microsoft.com/en-us/dotnet/api/system.threading.tasks.task-1 'System\.Threading\.Tasks\.Task\`1')  
A [bool](https://docs.microsoft.com/en-us/dotnet/csharp/language-reference/keywords/bool 'https://docs\.microsoft\.com/en\-us/dotnet/csharp/language\-reference/keywords/bool') indicating if the database exists\.