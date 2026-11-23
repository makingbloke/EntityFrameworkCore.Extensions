#### [DotDoc\.EntityFrameworkCore\.Extensions](Home 'Home')
### [DotDoc\.EntityFrameworkCore\.Extensions\.CaseInsensitivity](DotDoc.EntityFrameworkCore.Extensions.CaseInsensitivity 'DotDoc\.EntityFrameworkCore\.Extensions\.CaseInsensitivity')

## SqliteNoCaseReplacementExtensions Class

SQLite NoCase Replacement Extensions\.

```csharp
public static class SqliteNoCaseReplacementExtensions
```

Inheritance [System\.Object](https://learn.microsoft.com/en-us/dotnet/api/system.object 'System\.Object') → SqliteNoCaseReplacementExtensions

| Methods | |
| :--- | :--- |
| [UseSqliteNoCaseReplacement\(this SqliteConnection\)](SqliteNoCaseReplacementExtensions.UseSqliteNoCaseReplacement#DotDoc.EntityFrameworkCore.Extensions.CaseInsensitivity.SqliteNoCaseReplacementExtensions.UseSqliteNoCaseReplacement(thisMicrosoft.Data.Sqlite.SqliteConnection) 'DotDoc\.EntityFrameworkCore\.Extensions\.CaseInsensitivity\.SqliteNoCaseReplacementExtensions\.UseSqliteNoCaseReplacement\(this Microsoft\.Data\.Sqlite\.SqliteConnection\)') | Makes SQLite's case insensitive text handling on a connection work for all of Unicode, not just ASCII letters, matching the SQL Server Latin1\_General\_100\_CI\_AS\_SC\_UTF8 collation used by UseCaseInsensitiveCollation \(case insensitive, accent sensitive\)\. |
| [UseSqliteNoCaseReplacement\(this DbContextOptionsBuilder\)](SqliteNoCaseReplacementExtensions.UseSqliteNoCaseReplacement#DotDoc.EntityFrameworkCore.Extensions.CaseInsensitivity.SqliteNoCaseReplacementExtensions.UseSqliteNoCaseReplacement(thisMicrosoft.EntityFrameworkCore.DbContextOptionsBuilder) 'DotDoc\.EntityFrameworkCore\.Extensions\.CaseInsensitivity\.SqliteNoCaseReplacementExtensions\.UseSqliteNoCaseReplacement\(this Microsoft\.EntityFrameworkCore\.DbContextOptionsBuilder\)') | Makes SQLite's case insensitive text handling work for all of Unicode, not just ASCII letters, matching the SQL Server Latin1\_General\_100\_CI\_AS\_SC\_UTF8 collation used by UseCaseInsensitiveCollation \(case insensitive, accent sensitive\)\. |
