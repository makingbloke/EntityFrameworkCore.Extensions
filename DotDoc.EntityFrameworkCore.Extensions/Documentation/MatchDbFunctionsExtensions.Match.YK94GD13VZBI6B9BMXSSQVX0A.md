#### [DotDoc\.EntityFrameworkCore\.Extensions](Home.md 'Home')
### [DotDoc\.EntityFrameworkCore\.Extensions\.MatchFunction](DotDoc.EntityFrameworkCore.Extensions.MatchFunction.md 'DotDoc\.EntityFrameworkCore\.Extensions\.MatchFunction').[MatchDbFunctionsExtensions](MatchDbFunctionsExtensions.md 'DotDoc\.EntityFrameworkCore\.Extensions\.MatchFunction\.MatchDbFunctionsExtensions')

## MatchDbFunctionsExtensions\.Match\(this DbFunctions, string, object\) Method

Perform a SQLite free text match\.

```csharp
public static bool Match(this Microsoft.EntityFrameworkCore.DbFunctions dbFunctions, string freeText, object propertyReference);
```
#### Parameters

<a name='DotDoc.EntityFrameworkCore.Extensions.MatchFunction.MatchDbFunctionsExtensions.Match(thisMicrosoft.EntityFrameworkCore.DbFunctions,string,object).dbFunctions'></a>

`dbFunctions` [Microsoft\.EntityFrameworkCore\.DbFunctions](https://learn.microsoft.com/en-us/dotnet/api/microsoft.entityframeworkcore.dbfunctions 'Microsoft\.EntityFrameworkCore\.DbFunctions')

The [Microsoft\.EntityFrameworkCore\.DbFunctions](https://learn.microsoft.com/en-us/dotnet/api/microsoft.entityframeworkcore.dbfunctions 'Microsoft\.EntityFrameworkCore\.DbFunctions') instance\.

<a name='DotDoc.EntityFrameworkCore.Extensions.MatchFunction.MatchDbFunctionsExtensions.Match(thisMicrosoft.EntityFrameworkCore.DbFunctions,string,object).freeText'></a>

`freeText` [System\.String](https://learn.microsoft.com/en-us/dotnet/api/system.string 'System\.String')

The text that will be searched for in the property\.

<a name='DotDoc.EntityFrameworkCore.Extensions.MatchFunction.MatchDbFunctionsExtensions.Match(thisMicrosoft.EntityFrameworkCore.DbFunctions,string,object).propertyReference'></a>

`propertyReference` [System\.Object](https://learn.microsoft.com/en-us/dotnet/api/system.object 'System\.Object')

The property on which the search will be performed\.

#### Returns
[System\.Boolean](https://learn.microsoft.com/en-us/dotnet/api/system.boolean 'System\.Boolean')  
A [System\.Boolean](https://learn.microsoft.com/en-us/dotnet/api/system.boolean 'System\.Boolean') value indicating whether the text exists in the property\.