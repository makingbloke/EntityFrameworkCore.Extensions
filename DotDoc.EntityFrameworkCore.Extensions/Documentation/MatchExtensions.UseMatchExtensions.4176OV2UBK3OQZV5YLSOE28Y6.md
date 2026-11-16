#### [DotDoc\.EntityFrameworkCore\.Extensions](Home 'Home')
### [DotDoc\.EntityFrameworkCore\.Extensions\.MatchFunction](DotDoc.EntityFrameworkCore.Extensions.MatchFunction 'DotDoc\.EntityFrameworkCore\.Extensions\.MatchFunction').[MatchExtensions](MatchExtensions 'DotDoc\.EntityFrameworkCore\.Extensions\.MatchFunction\.MatchExtensions')

## MatchExtensions\.UseMatchExtensions\(this DbContextOptionsBuilder\) Method

Use the Match Extensions\.

```csharp
public static Microsoft.EntityFrameworkCore.DbContextOptionsBuilder UseMatchExtensions(this Microsoft.EntityFrameworkCore.DbContextOptionsBuilder optionsBuilder);
```
#### Parameters

<a name='DotDoc.EntityFrameworkCore.Extensions.MatchFunction.MatchExtensions.UseMatchExtensions(thisMicrosoft.EntityFrameworkCore.DbContextOptionsBuilder).optionsBuilder'></a>

`optionsBuilder` [Microsoft\.EntityFrameworkCore\.DbContextOptionsBuilder](https://learn.microsoft.com/en-us/dotnet/api/microsoft.entityframeworkcore.dbcontextoptionsbuilder 'Microsoft\.EntityFrameworkCore\.DbContextOptionsBuilder')

The builder being used to configure the context\.

#### Returns
[Microsoft\.EntityFrameworkCore\.DbContextOptionsBuilder](https://learn.microsoft.com/en-us/dotnet/api/microsoft.entityframeworkcore.dbcontextoptionsbuilder 'Microsoft\.EntityFrameworkCore\.DbContextOptionsBuilder')  
The same builder instance so multiple calls can be chained\.

#### Exceptions

[System\.ArgumentNullException](https://learn.microsoft.com/en-us/dotnet/api/system.argumentnullexception 'System\.ArgumentNullException')  
[optionsBuilder](MatchExtensions.UseMatchExtensions.4176OV2UBK3OQZV5YLSOE28Y6#DotDoc.EntityFrameworkCore.Extensions.MatchFunction.MatchExtensions.UseMatchExtensions(thisMicrosoft.EntityFrameworkCore.DbContextOptionsBuilder).optionsBuilder 'DotDoc\.EntityFrameworkCore\.Extensions\.MatchFunction\.MatchExtensions\.UseMatchExtensions\(this Microsoft\.EntityFrameworkCore\.DbContextOptionsBuilder\)\.optionsBuilder') is [null](https://docs.microsoft.com/en-us/dotnet/csharp/language-reference/keywords/null 'https://docs\.microsoft\.com/en\-us/dotnet/csharp/language\-reference/keywords/null')\.

[UnsupportedDatabaseTypeException](UnsupportedDatabaseTypeException 'DotDoc\.EntityFrameworkCore\.Extensions\.DatabaseType\.UnsupportedDatabaseTypeException')  
The database provider is not SQLite\.