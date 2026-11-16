#### [DotDoc\.EntityFrameworkCore\.Extensions](Home 'Home')
### [DotDoc\.EntityFrameworkCore\.Extensions\.Utilities](DotDoc.EntityFrameworkCore.Extensions.Utilities 'DotDoc\.EntityFrameworkCore\.Extensions\.Utilities').[UtilityExtensions](UtilityExtensions 'DotDoc\.EntityFrameworkCore\.Extensions\.Utilities\.UtilityExtensions')

## UtilityExtensions\.UseCaseInsensitiveCollation\(this ModelBuilder, string\) Method

Use Case Insensitive Collation\.

```csharp
public static Microsoft.EntityFrameworkCore.ModelBuilder UseCaseInsensitiveCollation(this Microsoft.EntityFrameworkCore.ModelBuilder modelBuilder, string? databaseType);
```
#### Parameters

<a name='DotDoc.EntityFrameworkCore.Extensions.Utilities.UtilityExtensions.UseCaseInsensitiveCollation(thisMicrosoft.EntityFrameworkCore.ModelBuilder,string).modelBuilder'></a>

`modelBuilder` [Microsoft\.EntityFrameworkCore\.ModelBuilder](https://learn.microsoft.com/en-us/dotnet/api/microsoft.entityframeworkcore.modelbuilder 'Microsoft\.EntityFrameworkCore\.ModelBuilder')

The builder used to construct the model for the context\.

<a name='DotDoc.EntityFrameworkCore.Extensions.Utilities.UtilityExtensions.UseCaseInsensitiveCollation(thisMicrosoft.EntityFrameworkCore.ModelBuilder,string).databaseType'></a>

`databaseType` [System\.String](https://learn.microsoft.com/en-us/dotnet/api/system.string 'System\.String')

The database type\.

#### Returns
[Microsoft\.EntityFrameworkCore\.ModelBuilder](https://learn.microsoft.com/en-us/dotnet/api/microsoft.entityframeworkcore.modelbuilder 'Microsoft\.EntityFrameworkCore\.ModelBuilder')  
The same builder instance so multiple calls can be chained\.

#### Exceptions

[System\.ArgumentNullException](https://learn.microsoft.com/en-us/dotnet/api/system.argumentnullexception 'System\.ArgumentNullException')  
[modelBuilder](UtilityExtensions.UseCaseInsensitiveCollation.OOAJ0C9TAY9KQT6XA7CZXJJN6#DotDoc.EntityFrameworkCore.Extensions.Utilities.UtilityExtensions.UseCaseInsensitiveCollation(thisMicrosoft.EntityFrameworkCore.ModelBuilder,string).modelBuilder 'DotDoc\.EntityFrameworkCore\.Extensions\.Utilities\.UtilityExtensions\.UseCaseInsensitiveCollation\(this Microsoft\.EntityFrameworkCore\.ModelBuilder, string\)\.modelBuilder') is [null](https://docs.microsoft.com/en-us/dotnet/csharp/language-reference/keywords/null 'https://docs\.microsoft\.com/en\-us/dotnet/csharp/language\-reference/keywords/null')\.

[UnsupportedDatabaseTypeException](UnsupportedDatabaseTypeException 'DotDoc\.EntityFrameworkCore\.Extensions\.DatabaseType\.UnsupportedDatabaseTypeException')  
[databaseType](UtilityExtensions.UseCaseInsensitiveCollation.OOAJ0C9TAY9KQT6XA7CZXJJN6#DotDoc.EntityFrameworkCore.Extensions.Utilities.UtilityExtensions.UseCaseInsensitiveCollation(thisMicrosoft.EntityFrameworkCore.ModelBuilder,string).databaseType 'DotDoc\.EntityFrameworkCore\.Extensions\.Utilities\.UtilityExtensions\.UseCaseInsensitiveCollation\(this Microsoft\.EntityFrameworkCore\.ModelBuilder, string\)\.databaseType') is not SQLite or SQL Server\.