#### [DotDoc\.EntityFrameworkCore\.Extensions](Home 'Home')
### [DotDoc\.EntityFrameworkCore\.Extensions\.Utilities](DotDoc.EntityFrameworkCore.Extensions.Utilities 'DotDoc\.EntityFrameworkCore\.Extensions\.Utilities').[UtilityExtensions](UtilityExtensions 'DotDoc\.EntityFrameworkCore\.Extensions\.Utilities\.UtilityExtensions')

## UtilityExtensions\.CreateGuid\(this DatabaseFacade\) Method

Create a Guid that is suitable for use as a database key value\.

```csharp
public static System.Guid CreateGuid(this Microsoft.EntityFrameworkCore.Infrastructure.DatabaseFacade database);
```
#### Parameters

<a name='DotDoc.EntityFrameworkCore.Extensions.Utilities.UtilityExtensions.CreateGuid(thisMicrosoft.EntityFrameworkCore.Infrastructure.DatabaseFacade).database'></a>

`database` [Microsoft\.EntityFrameworkCore\.Infrastructure\.DatabaseFacade](https://learn.microsoft.com/en-us/dotnet/api/microsoft.entityframeworkcore.infrastructure.databasefacade 'Microsoft\.EntityFrameworkCore\.Infrastructure\.DatabaseFacade')

The [Microsoft\.EntityFrameworkCore\.Infrastructure\.DatabaseFacade](https://learn.microsoft.com/en-us/dotnet/api/microsoft.entityframeworkcore.infrastructure.databasefacade 'Microsoft\.EntityFrameworkCore\.Infrastructure\.DatabaseFacade') for the context\.

#### Returns
[System\.Guid](https://learn.microsoft.com/en-us/dotnet/api/system.guid 'System\.Guid')  
A [System\.Guid](https://learn.microsoft.com/en-us/dotnet/api/system.guid 'System\.Guid')\.

### Remarks
SQL Server can store GUIDs but because of how they are handled date stamped GUIDs
are not sorted in the correct order\.
This method uses a third party library \(UUIDNext\) to create GUIDS that are
compatible with the supported database types\.
\(The inbuilt EF Core [Microsoft\.EntityFrameworkCore\.ValueGeneration\.SequentialGuidValueGenerator](https://learn.microsoft.com/en-us/dotnet/api/microsoft.entityframeworkcore.valuegeneration.sequentialguidvaluegenerator 'Microsoft\.EntityFrameworkCore\.ValueGeneration\.SequentialGuidValueGenerator')\> class creates
GUIDS for SQL Server that look correct but with have an incorrect version number\)\.