#### [DotDoc\.EntityFrameworkCore\.Extensions](Home 'Home')
### [DotDoc\.EntityFrameworkCore\.Extensions\.FreeTextSearchFunction](DotDoc.EntityFrameworkCore.Extensions.FreeTextSearchFunction 'DotDoc\.EntityFrameworkCore\.Extensions\.FreeTextSearchFunction').[FreeTextSearchExtensions](FreeTextSearchExtensions 'DotDoc\.EntityFrameworkCore\.Extensions\.FreeTextSearchFunction\.FreeTextSearchExtensions')

## FreeTextSearchExtensions\.UseFreeTextSearchExtensions\(this DbContextOptionsBuilder\) Method

Use the Free Text Extensions\.

```csharp
public static Microsoft.EntityFrameworkCore.DbContextOptionsBuilder UseFreeTextSearchExtensions(this Microsoft.EntityFrameworkCore.DbContextOptionsBuilder optionsBuilder);
```
#### Parameters

<a name='DotDoc.EntityFrameworkCore.Extensions.FreeTextSearchFunction.FreeTextSearchExtensions.UseFreeTextSearchExtensions(thisMicrosoft.EntityFrameworkCore.DbContextOptionsBuilder).optionsBuilder'></a>

`optionsBuilder` [Microsoft\.EntityFrameworkCore\.DbContextOptionsBuilder](https://learn.microsoft.com/en-us/dotnet/api/microsoft.entityframeworkcore.dbcontextoptionsbuilder 'Microsoft\.EntityFrameworkCore\.DbContextOptionsBuilder')

The builder being used to configure the context\.

#### Returns
[Microsoft\.EntityFrameworkCore\.DbContextOptionsBuilder](https://learn.microsoft.com/en-us/dotnet/api/microsoft.entityframeworkcore.dbcontextoptionsbuilder 'Microsoft\.EntityFrameworkCore\.DbContextOptionsBuilder')  
The same builder instance so multiple calls can be chained\.

#### Exceptions

[System\.ArgumentNullException](https://learn.microsoft.com/en-us/dotnet/api/system.argumentnullexception 'System\.ArgumentNullException')  
[optionsBuilder](FreeTextSearchExtensions.UseFreeTextSearchExtensions.YXCRZJGSY65SJXJOR0T4ZTWJB#DotDoc.EntityFrameworkCore.Extensions.FreeTextSearchFunction.FreeTextSearchExtensions.UseFreeTextSearchExtensions(thisMicrosoft.EntityFrameworkCore.DbContextOptionsBuilder).optionsBuilder 'DotDoc\.EntityFrameworkCore\.Extensions\.FreeTextSearchFunction\.FreeTextSearchExtensions\.UseFreeTextSearchExtensions\(this Microsoft\.EntityFrameworkCore\.DbContextOptionsBuilder\)\.optionsBuilder') is [null](https://docs.microsoft.com/en-us/dotnet/csharp/language-reference/keywords/null 'https://docs\.microsoft\.com/en\-us/dotnet/csharp/language\-reference/keywords/null')\.

[UnsupportedDatabaseTypeException](UnsupportedDatabaseTypeException 'DotDoc\.EntityFrameworkCore\.Extensions\.DatabaseType\.UnsupportedDatabaseTypeException')  
The database provider is not SQLite or SQL Server\.