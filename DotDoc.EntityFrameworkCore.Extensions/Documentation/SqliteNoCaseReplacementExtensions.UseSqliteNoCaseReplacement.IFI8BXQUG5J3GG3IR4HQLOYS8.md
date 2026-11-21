#### [DotDoc\.EntityFrameworkCore\.Extensions](Home 'Home')
### [DotDoc\.EntityFrameworkCore\.Extensions\.CaseInsensitivity](DotDoc.EntityFrameworkCore.Extensions.CaseInsensitivity 'DotDoc\.EntityFrameworkCore\.Extensions\.CaseInsensitivity').[SqliteNoCaseReplacementExtensions](SqliteNoCaseReplacementExtensions 'DotDoc\.EntityFrameworkCore\.Extensions\.CaseInsensitivity\.SqliteNoCaseReplacementExtensions')

## SqliteNoCaseReplacementExtensions\.UseSqliteNoCaseReplacement\(this DbContextOptionsBuilder\) Method

Use the SQLite NoCase replacement\.

```csharp
public static Microsoft.EntityFrameworkCore.DbContextOptionsBuilder UseSqliteNoCaseReplacement(this Microsoft.EntityFrameworkCore.DbContextOptionsBuilder optionsBuilder);
```
#### Parameters

<a name='DotDoc.EntityFrameworkCore.Extensions.CaseInsensitivity.SqliteNoCaseReplacementExtensions.UseSqliteNoCaseReplacement(thisMicrosoft.EntityFrameworkCore.DbContextOptionsBuilder).optionsBuilder'></a>

`optionsBuilder` [Microsoft\.EntityFrameworkCore\.DbContextOptionsBuilder](https://learn.microsoft.com/en-us/dotnet/api/microsoft.entityframeworkcore.dbcontextoptionsbuilder 'Microsoft\.EntityFrameworkCore\.DbContextOptionsBuilder')

The builder being used to configure the context\.

#### Returns
[Microsoft\.EntityFrameworkCore\.DbContextOptionsBuilder](https://learn.microsoft.com/en-us/dotnet/api/microsoft.entityframeworkcore.dbcontextoptionsbuilder 'Microsoft\.EntityFrameworkCore\.DbContextOptionsBuilder')  
The same builder instance so multiple calls can be chained\.

#### Exceptions

[System\.ArgumentNullException](https://learn.microsoft.com/en-us/dotnet/api/system.argumentnullexception 'System\.ArgumentNullException')  
[optionsBuilder](SqliteNoCaseReplacementExtensions.UseSqliteNoCaseReplacement.IFI8BXQUG5J3GG3IR4HQLOYS8#DotDoc.EntityFrameworkCore.Extensions.CaseInsensitivity.SqliteNoCaseReplacementExtensions.UseSqliteNoCaseReplacement(thisMicrosoft.EntityFrameworkCore.DbContextOptionsBuilder).optionsBuilder 'DotDoc\.EntityFrameworkCore\.Extensions\.CaseInsensitivity\.SqliteNoCaseReplacementExtensions\.UseSqliteNoCaseReplacement\(this Microsoft\.EntityFrameworkCore\.DbContextOptionsBuilder\)\.optionsBuilder') is [null](https://docs.microsoft.com/en-us/dotnet/csharp/language-reference/keywords/null 'https://docs\.microsoft\.com/en\-us/dotnet/csharp/language\-reference/keywords/null')\.

[UnsupportedDatabaseTypeException](UnsupportedDatabaseTypeException 'DotDoc\.EntityFrameworkCore\.Extensions\.DatabaseType\.UnsupportedDatabaseTypeException')  
The database provider is not SQLite\.

### Remarks
Replaces the SQLite NoCase collation with a custom one which can handle Unicode
characters \(the original is ASCII only\)\. It is case insensitive but accent sensitive,
matching the SQL Server Latin1\_General\_100\_CI\_AS\_SC\_UTF8 collation used by UseCaseInsensitiveCollation\.

Connection pooling: Microsoft.Data.Sqlite pools connections by default, with one pool per connection string. Once a connection that uses
UseSqliteNoCaseReplacement has been opened with a connection string, every later connection with the same connection string must use it too.
That includes every DbContext, any SqliteConnection opened directly, and design-time tools such as dotnet ef.
When a connection goes back to the pool, Microsoft.Data.Sqlite removes the collation and functions registered on it, and SQLite can't
fall back to its built-in NOCASE, lower and upper on that connection afterwards. The next connection that doesn't register them again
fails with "no such collation sequence: NOCASE" or "wrong number of arguments to function upper()" (or lower()).

A connection with a different connection string uses a different pool and isn't affected. To use connections with and without the replacement
on the same database, give them different connection strings, or turn pooling off (Pooling=False) for one set of them.