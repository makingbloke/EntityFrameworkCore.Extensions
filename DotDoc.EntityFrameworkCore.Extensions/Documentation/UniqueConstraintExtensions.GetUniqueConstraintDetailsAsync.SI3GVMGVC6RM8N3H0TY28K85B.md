#### [DotDoc\.EntityFrameworkCore\.Extensions](index.md 'index')
### [DotDoc\.EntityFrameworkCore\.Extensions\.UniqueConstraint](DotDoc.EntityFrameworkCore.Extensions.UniqueConstraint.md 'DotDoc\.EntityFrameworkCore\.Extensions\.UniqueConstraint').[UniqueConstraintExtensions](UniqueConstraintExtensions.md 'DotDoc\.EntityFrameworkCore\.Extensions\.UniqueConstraint\.UniqueConstraintExtensions')

## UniqueConstraintExtensions\.GetUniqueConstraintDetailsAsync\(this DatabaseFacade, Exception, CancellationToken\) Method

Get the details of an unique constraint from an exception\.

```csharp
public static System.Threading.Tasks.Task<DotDoc.EntityFrameworkCore.Extensions.UniqueConstraint.UniqueConstraintDetails?> GetUniqueConstraintDetailsAsync(this Microsoft.EntityFrameworkCore.Infrastructure.DatabaseFacade database, System.Exception e, System.Threading.CancellationToken cancellationToken=default(System.Threading.CancellationToken));
```
#### Parameters

<a name='DotDoc.EntityFrameworkCore.Extensions.UniqueConstraint.UniqueConstraintExtensions.GetUniqueConstraintDetailsAsync(thisMicrosoft.EntityFrameworkCore.Infrastructure.DatabaseFacade,System.Exception,System.Threading.CancellationToken).database'></a>

`database` [Microsoft\.EntityFrameworkCore\.Infrastructure\.DatabaseFacade](https://learn.microsoft.com/en-us/dotnet/api/microsoft.entityframeworkcore.infrastructure.databasefacade 'Microsoft\.EntityFrameworkCore\.Infrastructure\.DatabaseFacade')

The [Microsoft\.EntityFrameworkCore\.Infrastructure\.DatabaseFacade](https://learn.microsoft.com/en-us/dotnet/api/microsoft.entityframeworkcore.infrastructure.databasefacade 'Microsoft\.EntityFrameworkCore\.Infrastructure\.DatabaseFacade')\.

<a name='DotDoc.EntityFrameworkCore.Extensions.UniqueConstraint.UniqueConstraintExtensions.GetUniqueConstraintDetailsAsync(thisMicrosoft.EntityFrameworkCore.Infrastructure.DatabaseFacade,System.Exception,System.Threading.CancellationToken).e'></a>

`e` [System\.Exception](https://learn.microsoft.com/en-us/dotnet/api/system.exception 'System\.Exception')

The exception to extract the unique constraint details from\.

<a name='DotDoc.EntityFrameworkCore.Extensions.UniqueConstraint.UniqueConstraintExtensions.GetUniqueConstraintDetailsAsync(thisMicrosoft.EntityFrameworkCore.Infrastructure.DatabaseFacade,System.Exception,System.Threading.CancellationToken).cancellationToken'></a>

`cancellationToken` [System\.Threading\.CancellationToken](https://learn.microsoft.com/en-us/dotnet/api/system.threading.cancellationtoken 'System\.Threading\.CancellationToken')

A [System\.Threading\.CancellationToken](https://learn.microsoft.com/en-us/dotnet/api/system.threading.cancellationtoken 'System\.Threading\.CancellationToken') to observe while waiting for the task to complete\.

#### Returns
[System\.Threading\.Tasks\.Task&lt;](https://learn.microsoft.com/en-us/dotnet/api/system.threading.tasks.task-1 'System\.Threading\.Tasks\.Task\`1')[UniqueConstraintDetails](UniqueConstraintDetails.md 'DotDoc\.EntityFrameworkCore\.Extensions\.UniqueConstraint\.UniqueConstraintDetails')[&gt;](https://learn.microsoft.com/en-us/dotnet/api/system.threading.tasks.task-1 'System\.Threading\.Tasks\.Task\`1')  
An instance of [UniqueConstraintDetails](UniqueConstraintDetails.md 'DotDoc\.EntityFrameworkCore\.Extensions\.UniqueConstraint\.UniqueConstraintDetails')\.