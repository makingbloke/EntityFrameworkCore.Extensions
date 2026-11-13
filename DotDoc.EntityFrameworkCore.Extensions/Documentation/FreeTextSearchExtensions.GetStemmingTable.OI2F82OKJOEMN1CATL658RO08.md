#### [DotDoc\.EntityFrameworkCore\.Extensions](Home 'Home')
### [DotDoc\.EntityFrameworkCore\.Extensions\.FreeTextSearchFunction](DotDoc.EntityFrameworkCore.Extensions.FreeTextSearchFunction 'DotDoc\.EntityFrameworkCore\.Extensions\.FreeTextSearchFunction').[FreeTextSearchExtensions](FreeTextSearchExtensions 'DotDoc\.EntityFrameworkCore\.Extensions\.FreeTextSearchFunction\.FreeTextSearchExtensions')

## FreeTextSearchExtensions\.GetStemmingTable\(this IEntityType\) Method

Get the stemming table name associated with this entity\.

```csharp
public static string? GetStemmingTable(this Microsoft.EntityFrameworkCore.Metadata.IEntityType entityType);
```
#### Parameters

<a name='DotDoc.EntityFrameworkCore.Extensions.FreeTextSearchFunction.FreeTextSearchExtensions.GetStemmingTable(thisMicrosoft.EntityFrameworkCore.Metadata.IEntityType).entityType'></a>

`entityType` [Microsoft\.EntityFrameworkCore\.Metadata\.IEntityType](https://learn.microsoft.com/en-us/dotnet/api/microsoft.entityframeworkcore.metadata.ientitytype 'Microsoft\.EntityFrameworkCore\.Metadata\.IEntityType')

The [Microsoft\.EntityFrameworkCore\.Metadata\.IEntityType](https://learn.microsoft.com/en-us/dotnet/api/microsoft.entityframeworkcore.metadata.ientitytype 'Microsoft\.EntityFrameworkCore\.Metadata\.IEntityType')\.

#### Returns
[System\.String](https://learn.microsoft.com/en-us/dotnet/api/system.string 'System\.String')  
The table name or [null](https://docs.microsoft.com/en-us/dotnet/csharp/language-reference/keywords/null 'https://docs\.microsoft\.com/en\-us/dotnet/csharp/language\-reference/keywords/null') if none found\.