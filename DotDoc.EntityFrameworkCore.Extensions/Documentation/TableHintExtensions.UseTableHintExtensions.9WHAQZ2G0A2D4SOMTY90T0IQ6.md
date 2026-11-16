#### [DotDoc\.EntityFrameworkCore\.Extensions](Home 'Home')
### [DotDoc\.EntityFrameworkCore\.Extensions\.TableHints](DotDoc.EntityFrameworkCore.Extensions.TableHints 'DotDoc\.EntityFrameworkCore\.Extensions\.TableHints').[TableHintExtensions](TableHintExtensions 'DotDoc\.EntityFrameworkCore\.Extensions\.TableHints\.TableHintExtensions')

## TableHintExtensions\.UseTableHintExtensions\(this DbContextOptionsBuilder\) Method

Use the Table Hint Extensions\.

```csharp
public static Microsoft.EntityFrameworkCore.DbContextOptionsBuilder UseTableHintExtensions(this Microsoft.EntityFrameworkCore.DbContextOptionsBuilder optionsBuilder);
```
#### Parameters

<a name='DotDoc.EntityFrameworkCore.Extensions.TableHints.TableHintExtensions.UseTableHintExtensions(thisMicrosoft.EntityFrameworkCore.DbContextOptionsBuilder).optionsBuilder'></a>

`optionsBuilder` [Microsoft\.EntityFrameworkCore\.DbContextOptionsBuilder](https://learn.microsoft.com/en-us/dotnet/api/microsoft.entityframeworkcore.dbcontextoptionsbuilder 'Microsoft\.EntityFrameworkCore\.DbContextOptionsBuilder')

The builder being used to configure the context\.

#### Returns
[Microsoft\.EntityFrameworkCore\.DbContextOptionsBuilder](https://learn.microsoft.com/en-us/dotnet/api/microsoft.entityframeworkcore.dbcontextoptionsbuilder 'Microsoft\.EntityFrameworkCore\.DbContextOptionsBuilder')  
The same builder instance so multiple calls can be chained\.

#### Exceptions

[System\.ArgumentNullException](https://learn.microsoft.com/en-us/dotnet/api/system.argumentnullexception 'System\.ArgumentNullException')  
[optionsBuilder](TableHintExtensions.UseTableHintExtensions.9WHAQZ2G0A2D4SOMTY90T0IQ6#DotDoc.EntityFrameworkCore.Extensions.TableHints.TableHintExtensions.UseTableHintExtensions(thisMicrosoft.EntityFrameworkCore.DbContextOptionsBuilder).optionsBuilder 'DotDoc\.EntityFrameworkCore\.Extensions\.TableHints\.TableHintExtensions\.UseTableHintExtensions\(this Microsoft\.EntityFrameworkCore\.DbContextOptionsBuilder\)\.optionsBuilder') is [null](https://docs.microsoft.com/en-us/dotnet/csharp/language-reference/keywords/null 'https://docs\.microsoft\.com/en\-us/dotnet/csharp/language\-reference/keywords/null')\.

[UnsupportedDatabaseTypeException](UnsupportedDatabaseTypeException 'DotDoc\.EntityFrameworkCore\.Extensions\.DatabaseType\.UnsupportedDatabaseTypeException')  
The database provider is not SQL Server \(table hints are SQL Server only\)\.