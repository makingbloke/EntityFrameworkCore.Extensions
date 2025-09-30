#### [DotDoc\.EntityFrameworkCore\.Extensions](Home.md 'Home')
### [DotDoc\.EntityFrameworkCore\.Extensions\.Utilities](DotDoc.EntityFrameworkCore.Extensions.Utilities.md 'DotDoc\.EntityFrameworkCore\.Extensions\.Utilities').[UtilityExtensions](UtilityExtensions.md 'DotDoc\.EntityFrameworkCore\.Extensions\.Utilities\.UtilityExtensions')

## UtilityExtensions\.GetDbContext Method

| Overloads | |
| :--- | :--- |
| [GetDbContext\(this DatabaseFacade\)](UtilityExtensions.GetDbContext.md#DotDoc.EntityFrameworkCore.Extensions.Utilities.UtilityExtensions.GetDbContext(thisMicrosoft.EntityFrameworkCore.Infrastructure.DatabaseFacade) 'DotDoc\.EntityFrameworkCore\.Extensions\.Utilities\.UtilityExtensions\.GetDbContext\(this Microsoft\.EntityFrameworkCore\.Infrastructure\.DatabaseFacade\)') | Gets the [Microsoft\.EntityFrameworkCore\.DbContext](https://learn.microsoft.com/en-us/dotnet/api/microsoft.entityframeworkcore.dbcontext 'Microsoft\.EntityFrameworkCore\.DbContext') object that is used by the specified [Microsoft\.EntityFrameworkCore\.Infrastructure\.DatabaseFacade](https://learn.microsoft.com/en-us/dotnet/api/microsoft.entityframeworkcore.infrastructure.databasefacade 'Microsoft\.EntityFrameworkCore\.Infrastructure\.DatabaseFacade')\. |
| [GetDbContext\(this IQueryable\)](UtilityExtensions.GetDbContext.md#DotDoc.EntityFrameworkCore.Extensions.Utilities.UtilityExtensions.GetDbContext(thisSystem.Linq.IQueryable) 'DotDoc\.EntityFrameworkCore\.Extensions\.Utilities\.UtilityExtensions\.GetDbContext\(this System\.Linq\.IQueryable\)') | Gets the [Microsoft\.EntityFrameworkCore\.DbContext](https://learn.microsoft.com/en-us/dotnet/api/microsoft.entityframeworkcore.dbcontext 'Microsoft\.EntityFrameworkCore\.DbContext') object that is used by the specified [System\.Linq\.IQueryable&lt;&gt;](https://learn.microsoft.com/en-us/dotnet/api/system.linq.iqueryable-1 'System\.Linq\.IQueryable\`1')\. |

<a name='DotDoc.EntityFrameworkCore.Extensions.Utilities.UtilityExtensions.GetDbContext(thisMicrosoft.EntityFrameworkCore.Infrastructure.DatabaseFacade)'></a>

## UtilityExtensions\.GetDbContext\(this DatabaseFacade\) Method

Gets the [Microsoft\.EntityFrameworkCore\.DbContext](https://learn.microsoft.com/en-us/dotnet/api/microsoft.entityframeworkcore.dbcontext 'Microsoft\.EntityFrameworkCore\.DbContext') object that is used by the specified [Microsoft\.EntityFrameworkCore\.Infrastructure\.DatabaseFacade](https://learn.microsoft.com/en-us/dotnet/api/microsoft.entityframeworkcore.infrastructure.databasefacade 'Microsoft\.EntityFrameworkCore\.Infrastructure\.DatabaseFacade')\.

```csharp
public static Microsoft.EntityFrameworkCore.DbContext GetDbContext(this Microsoft.EntityFrameworkCore.Infrastructure.DatabaseFacade database);
```
#### Parameters

<a name='DotDoc.EntityFrameworkCore.Extensions.Utilities.UtilityExtensions.GetDbContext(thisMicrosoft.EntityFrameworkCore.Infrastructure.DatabaseFacade).database'></a>

`database` [Microsoft\.EntityFrameworkCore\.Infrastructure\.DatabaseFacade](https://learn.microsoft.com/en-us/dotnet/api/microsoft.entityframeworkcore.infrastructure.databasefacade 'Microsoft\.EntityFrameworkCore\.Infrastructure\.DatabaseFacade')

The [Microsoft\.EntityFrameworkCore\.Infrastructure\.DatabaseFacade](https://learn.microsoft.com/en-us/dotnet/api/microsoft.entityframeworkcore.infrastructure.databasefacade 'Microsoft\.EntityFrameworkCore\.Infrastructure\.DatabaseFacade')\.

#### Returns
[Microsoft\.EntityFrameworkCore\.DbContext](https://learn.microsoft.com/en-us/dotnet/api/microsoft.entityframeworkcore.dbcontext 'Microsoft\.EntityFrameworkCore\.DbContext')  
[Microsoft\.EntityFrameworkCore\.DbContext](https://learn.microsoft.com/en-us/dotnet/api/microsoft.entityframeworkcore.dbcontext 'Microsoft\.EntityFrameworkCore\.DbContext')\.

<a name='DotDoc.EntityFrameworkCore.Extensions.Utilities.UtilityExtensions.GetDbContext(thisSystem.Linq.IQueryable)'></a>

## UtilityExtensions\.GetDbContext\(this IQueryable\) Method

Gets the [Microsoft\.EntityFrameworkCore\.DbContext](https://learn.microsoft.com/en-us/dotnet/api/microsoft.entityframeworkcore.dbcontext 'Microsoft\.EntityFrameworkCore\.DbContext') object that is used by the specified [System\.Linq\.IQueryable&lt;&gt;](https://learn.microsoft.com/en-us/dotnet/api/system.linq.iqueryable-1 'System\.Linq\.IQueryable\`1')\.

```csharp
public static Microsoft.EntityFrameworkCore.DbContext GetDbContext(this System.Linq.IQueryable source);
```
#### Parameters

<a name='DotDoc.EntityFrameworkCore.Extensions.Utilities.UtilityExtensions.GetDbContext(thisSystem.Linq.IQueryable).source'></a>

`source` [System\.Linq\.IQueryable](https://learn.microsoft.com/en-us/dotnet/api/system.linq.iqueryable 'System\.Linq\.IQueryable')

The source query [System\.Linq\.IQueryable&lt;&gt;](https://learn.microsoft.com/en-us/dotnet/api/system.linq.iqueryable-1 'System\.Linq\.IQueryable\`1')\.

#### Returns
[Microsoft\.EntityFrameworkCore\.DbContext](https://learn.microsoft.com/en-us/dotnet/api/microsoft.entityframeworkcore.dbcontext 'Microsoft\.EntityFrameworkCore\.DbContext')  
[Microsoft\.EntityFrameworkCore\.DbContext](https://learn.microsoft.com/en-us/dotnet/api/microsoft.entityframeworkcore.dbcontext 'Microsoft\.EntityFrameworkCore\.DbContext')\.