#### [DotDoc\.EntityFrameworkCore\.Extensions](Home 'Home')
### [DotDoc\.EntityFrameworkCore\.Extensions\.ExecuteUpdate](DotDoc.EntityFrameworkCore.Extensions.ExecuteUpdate 'DotDoc\.EntityFrameworkCore\.Extensions\.ExecuteUpdate').[ExecuteUpdateExtensions](ExecuteUpdateExtensions 'DotDoc\.EntityFrameworkCore\.Extensions\.ExecuteUpdate\.ExecuteUpdateExtensions')

## ExecuteUpdateExtensions\.UseExecuteUpdateExtensions\(this DbContextOptionsBuilder\) Method

Use the Execute Update Extensions\.

```csharp
public static Microsoft.EntityFrameworkCore.DbContextOptionsBuilder UseExecuteUpdateExtensions(this Microsoft.EntityFrameworkCore.DbContextOptionsBuilder optionsBuilder);
```
#### Parameters

<a name='DotDoc.EntityFrameworkCore.Extensions.ExecuteUpdate.ExecuteUpdateExtensions.UseExecuteUpdateExtensions(thisMicrosoft.EntityFrameworkCore.DbContextOptionsBuilder).optionsBuilder'></a>

`optionsBuilder` [Microsoft\.EntityFrameworkCore\.DbContextOptionsBuilder](https://learn.microsoft.com/en-us/dotnet/api/microsoft.entityframeworkcore.dbcontextoptionsbuilder 'Microsoft\.EntityFrameworkCore\.DbContextOptionsBuilder')

The builder being used to configure the context\.

#### Returns
[Microsoft\.EntityFrameworkCore\.DbContextOptionsBuilder](https://learn.microsoft.com/en-us/dotnet/api/microsoft.entityframeworkcore.dbcontextoptionsbuilder 'Microsoft\.EntityFrameworkCore\.DbContextOptionsBuilder')  
The same builder instance so multiple calls can be chained\.

#### Exceptions

[System\.ArgumentNullException](https://learn.microsoft.com/en-us/dotnet/api/system.argumentnullexception 'System\.ArgumentNullException')  
[optionsBuilder](ExecuteUpdateExtensions.UseExecuteUpdateExtensions.8EVJGRS4YF4T3EF3YO0SQPB69#DotDoc.EntityFrameworkCore.Extensions.ExecuteUpdate.ExecuteUpdateExtensions.UseExecuteUpdateExtensions(thisMicrosoft.EntityFrameworkCore.DbContextOptionsBuilder).optionsBuilder 'DotDoc\.EntityFrameworkCore\.Extensions\.ExecuteUpdate\.ExecuteUpdateExtensions\.UseExecuteUpdateExtensions\(this Microsoft\.EntityFrameworkCore\.DbContextOptionsBuilder\)\.optionsBuilder') is [null](https://docs.microsoft.com/en-us/dotnet/csharp/language-reference/keywords/null 'https://docs\.microsoft\.com/en\-us/dotnet/csharp/language\-reference/keywords/null')\.

[UnsupportedDatabaseTypeException](UnsupportedDatabaseTypeException 'DotDoc\.EntityFrameworkCore\.Extensions\.DatabaseType\.UnsupportedDatabaseTypeException')  
The database provider is not SQLite or SQL Server\.