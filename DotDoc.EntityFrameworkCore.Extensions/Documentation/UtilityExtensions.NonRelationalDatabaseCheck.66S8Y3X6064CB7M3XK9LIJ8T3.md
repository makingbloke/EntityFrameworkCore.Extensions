#### [DotDoc\.EntityFrameworkCore\.Extensions](Home 'Home')
### [DotDoc\.EntityFrameworkCore\.Extensions\.Utilities](DotDoc.EntityFrameworkCore.Extensions.Utilities 'DotDoc\.EntityFrameworkCore\.Extensions\.Utilities').[UtilityExtensions](UtilityExtensions 'DotDoc\.EntityFrameworkCore\.Extensions\.Utilities\.UtilityExtensions')

## UtilityExtensions\.NonRelationalDatabaseCheck\(DatabaseFacade, string\) Method

Throws an exception if the database is not relational\.

```csharp
public static void NonRelationalDatabaseCheck(Microsoft.EntityFrameworkCore.Infrastructure.DatabaseFacade database, string? paramName=null);
```
#### Parameters

<a name='DotDoc.EntityFrameworkCore.Extensions.Utilities.UtilityExtensions.NonRelationalDatabaseCheck(Microsoft.EntityFrameworkCore.Infrastructure.DatabaseFacade,string).database'></a>

`database` [Microsoft\.EntityFrameworkCore\.Infrastructure\.DatabaseFacade](https://learn.microsoft.com/en-us/dotnet/api/microsoft.entityframeworkcore.infrastructure.databasefacade 'Microsoft\.EntityFrameworkCore\.Infrastructure\.DatabaseFacade')

The [Microsoft\.EntityFrameworkCore\.Infrastructure\.DatabaseFacade](https://learn.microsoft.com/en-us/dotnet/api/microsoft.entityframeworkcore.infrastructure.databasefacade 'Microsoft\.EntityFrameworkCore\.Infrastructure\.DatabaseFacade')\.

<a name='DotDoc.EntityFrameworkCore.Extensions.Utilities.UtilityExtensions.NonRelationalDatabaseCheck(Microsoft.EntityFrameworkCore.Infrastructure.DatabaseFacade,string).paramName'></a>

`paramName` [System\.String](https://learn.microsoft.com/en-us/dotnet/api/system.string 'System\.String')

The name of the parameter with which [database](UtilityExtensions.NonRelationalDatabaseCheck.66S8Y3X6064CB7M3XK9LIJ8T3#DotDoc.EntityFrameworkCore.Extensions.Utilities.UtilityExtensions.NonRelationalDatabaseCheck(Microsoft.EntityFrameworkCore.Infrastructure.DatabaseFacade,string).database 'DotDoc\.EntityFrameworkCore\.Extensions\.Utilities\.UtilityExtensions\.NonRelationalDatabaseCheck\(Microsoft\.EntityFrameworkCore\.Infrastructure\.DatabaseFacade, string\)\.database') corresponds\. If you omit this parameter, the name of [database](UtilityExtensions.NonRelationalDatabaseCheck.66S8Y3X6064CB7M3XK9LIJ8T3#DotDoc.EntityFrameworkCore.Extensions.Utilities.UtilityExtensions.NonRelationalDatabaseCheck(Microsoft.EntityFrameworkCore.Infrastructure.DatabaseFacade,string).database 'DotDoc\.EntityFrameworkCore\.Extensions\.Utilities\.UtilityExtensions\.NonRelationalDatabaseCheck\(Microsoft\.EntityFrameworkCore\.Infrastructure\.DatabaseFacade, string\)\.database') is used\.