#### [DotDoc\.EntityFrameworkCore\.Extensions](Home 'Home')
### [DotDoc\.EntityFrameworkCore\.Extensions\.CaseInsensitivity](DotDoc.EntityFrameworkCore.Extensions.CaseInsensitivity 'DotDoc\.EntityFrameworkCore\.Extensions\.CaseInsensitivity').[SqliteNoCaseReplacementExtensions](SqliteNoCaseReplacementExtensions 'DotDoc\.EntityFrameworkCore\.Extensions\.CaseInsensitivity\.SqliteNoCaseReplacementExtensions')

## SqliteNoCaseReplacementExtensions\.UseSqliteNoCaseReplacement\(this DbContextOptionsBuilder\) Method

Makes SQLite's case insensitive text handling work for all of Unicode, not just ASCII letters, matching the
SQL Server Latin1\_General\_100\_CI\_AS\_SC\_UTF8 collation used by UseCaseInsensitiveCollation \(case insensitive, accent sensitive\)\.

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

Does the following each time EF Core opens a connection:
- Replaces SQLite's NOCASE collation, which only ignores the case of ASCII letters, with a Unicode one, so comparisons, sorting,
  grouping and indexes on a NOCASE column ignore the case of every letter (é = É) but not accents (e ≠ é);
- Replaces SQLite's lower and upper functions, which only change ASCII letters, with Unicode ones, so ToLower() and ToUpper()
  change every letter, on every column;
- Makes Contains, IndexOf, StartsWith, EndsWith, Replace, Trim / TrimStart / TrimEnd (with constant characters) and
  EF.Functions.Like ignore case in the same way, on a NOCASE column or EF.Functions.Collate(..., "NOCASE"), by translating
  them to nocase_* functions it registers alongside SQLite's built-in ones.

Everything else is unchanged: on a column that isn't NOCASE, or an expression with no known collation (such as a concatenation or a
column from raw SQL), those methods use EF Core's own translation and SQLite's built-in functions.

Connection pooling: Microsoft.Data.Sqlite pools connections by default, with one pool per connection string. Once a connection that uses
UseSqliteNoCaseReplacement has been opened with a connection string, every later connection with the same connection string must use it too.
That includes every DbContext, any SqliteConnection opened directly, and design-time tools such as dotnet ef.
When a connection goes back to the pool, Microsoft.Data.Sqlite removes the collation and functions registered on it, and SQLite can't
fall back to its built-in NOCASE, lower and upper on that connection afterwards. The next connection that doesn't register them again
fails with "no such collation sequence: NOCASE" or "wrong number of arguments to function upper()" (or lower()).