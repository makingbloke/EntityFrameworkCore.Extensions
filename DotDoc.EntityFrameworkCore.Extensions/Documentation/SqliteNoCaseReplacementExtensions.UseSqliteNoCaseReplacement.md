#### [DotDoc\.EntityFrameworkCore\.Extensions](Home 'Home')
### [DotDoc\.EntityFrameworkCore\.Extensions\.CaseInsensitivity](DotDoc.EntityFrameworkCore.Extensions.CaseInsensitivity 'DotDoc\.EntityFrameworkCore\.Extensions\.CaseInsensitivity').[SqliteNoCaseReplacementExtensions](SqliteNoCaseReplacementExtensions 'DotDoc\.EntityFrameworkCore\.Extensions\.CaseInsensitivity\.SqliteNoCaseReplacementExtensions')

## SqliteNoCaseReplacementExtensions\.UseSqliteNoCaseReplacement Method

| Overloads | |
| :--- | :--- |
| [UseSqliteNoCaseReplacement\(this SqliteConnection\)](SqliteNoCaseReplacementExtensions.UseSqliteNoCaseReplacement#DotDoc.EntityFrameworkCore.Extensions.CaseInsensitivity.SqliteNoCaseReplacementExtensions.UseSqliteNoCaseReplacement(thisMicrosoft.Data.Sqlite.SqliteConnection) 'DotDoc\.EntityFrameworkCore\.Extensions\.CaseInsensitivity\.SqliteNoCaseReplacementExtensions\.UseSqliteNoCaseReplacement\(this Microsoft\.Data\.Sqlite\.SqliteConnection\)') | Makes SQLite's case insensitive text handling on a connection work for all of Unicode, not just ASCII letters, matching the SQL Server Latin1\_General\_100\_CI\_AS\_SC\_UTF8 collation used by UseCaseInsensitiveCollation \(case insensitive, accent sensitive\)\. |
| [UseSqliteNoCaseReplacement\(this DbContextOptionsBuilder\)](SqliteNoCaseReplacementExtensions.UseSqliteNoCaseReplacement#DotDoc.EntityFrameworkCore.Extensions.CaseInsensitivity.SqliteNoCaseReplacementExtensions.UseSqliteNoCaseReplacement(thisMicrosoft.EntityFrameworkCore.DbContextOptionsBuilder) 'DotDoc\.EntityFrameworkCore\.Extensions\.CaseInsensitivity\.SqliteNoCaseReplacementExtensions\.UseSqliteNoCaseReplacement\(this Microsoft\.EntityFrameworkCore\.DbContextOptionsBuilder\)') | Makes SQLite's case insensitive text handling work for all of Unicode, not just ASCII letters, matching the SQL Server Latin1\_General\_100\_CI\_AS\_SC\_UTF8 collation used by UseCaseInsensitiveCollation \(case insensitive, accent sensitive\)\. |

<a name='DotDoc.EntityFrameworkCore.Extensions.CaseInsensitivity.SqliteNoCaseReplacementExtensions.UseSqliteNoCaseReplacement(thisMicrosoft.Data.Sqlite.SqliteConnection)'></a>

## SqliteNoCaseReplacementExtensions\.UseSqliteNoCaseReplacement\(this SqliteConnection\) Method

Makes SQLite's case insensitive text handling on a connection work for all of Unicode, not just ASCII letters, matching the
SQL Server Latin1\_General\_100\_CI\_AS\_SC\_UTF8 collation used by UseCaseInsensitiveCollation \(case insensitive, accent sensitive\)\.

```csharp
public static Microsoft.Data.Sqlite.SqliteConnection UseSqliteNoCaseReplacement(this Microsoft.Data.Sqlite.SqliteConnection connection);
```
#### Parameters

<a name='DotDoc.EntityFrameworkCore.Extensions.CaseInsensitivity.SqliteNoCaseReplacementExtensions.UseSqliteNoCaseReplacement(thisMicrosoft.Data.Sqlite.SqliteConnection).connection'></a>

`connection` [Microsoft\.Data\.Sqlite\.SqliteConnection](https://learn.microsoft.com/en-us/dotnet/api/microsoft.data.sqlite.sqliteconnection 'Microsoft\.Data\.Sqlite\.SqliteConnection')

The connection\.

#### Returns
[Microsoft\.Data\.Sqlite\.SqliteConnection](https://learn.microsoft.com/en-us/dotnet/api/microsoft.data.sqlite.sqliteconnection 'Microsoft\.Data\.Sqlite\.SqliteConnection')  
The same connection, so multiple calls can be chained\.

#### Exceptions

[System\.ArgumentNullException](https://learn.microsoft.com/en-us/dotnet/api/system.argumentnullexception 'System\.ArgumentNullException')  
[connection](SqliteNoCaseReplacementExtensions#DotDoc.EntityFrameworkCore.Extensions.CaseInsensitivity.SqliteNoCaseReplacementExtensions.UseSqliteNoCaseReplacement(thisMicrosoft.Data.Sqlite.SqliteConnection).connection 'DotDoc\.EntityFrameworkCore\.Extensions\.CaseInsensitivity\.SqliteNoCaseReplacementExtensions\.UseSqliteNoCaseReplacement\(this Microsoft\.Data\.Sqlite\.SqliteConnection\)\.connection') is [null](https://docs.microsoft.com/en-us/dotnet/csharp/language-reference/keywords/null 'https://docs\.microsoft\.com/en\-us/dotnet/csharp/language\-reference/keywords/null')\.

[System\.InvalidOperationException](https://learn.microsoft.com/en-us/dotnet/api/system.invalidoperationexception 'System\.InvalidOperationException')  
\.NET is running in globalization\-invariant mode, which has no ICU to compare text with\.

### Remarks

Registers the same collation and functions on the connection as [UseSqliteNoCaseReplacement\(this DbContextOptionsBuilder\)](SqliteNoCaseReplacementExtensions.UseSqliteNoCaseReplacement#DotDoc.EntityFrameworkCore.Extensions.CaseInsensitivity.SqliteNoCaseReplacementExtensions.UseSqliteNoCaseReplacement(thisMicrosoft.EntityFrameworkCore.DbContextOptionsBuilder) 'DotDoc\.EntityFrameworkCore\.Extensions\.CaseInsensitivity\.SqliteNoCaseReplacementExtensions\.UseSqliteNoCaseReplacement\(this Microsoft\.EntityFrameworkCore\.DbContextOptionsBuilder\)')
does on each connection EF Core uses:
- Replaces SQLite's NOCASE collation, which only ignores the case of ASCII letters, with a Unicode one that ignores the case of
  every letter (é = É) but not accents (e ≠ é);
- Replaces SQLite's lower and upper functions, which only change ASCII letters, with Unicode ones;
- Adds the nocase_* functions (nocase_instr, nocase_like, nocase_replace, nocase_trim, nocase_ltrim, nocase_rtrim, nocase_startswith
  and nocase_endswith), case insensitive versions of SQLite's string functions that SQL can call directly.

Use it on a SqliteConnection that is used without EF Core, such as one opened directly with the same connection string as a
DbContext that uses UseSqliteNoCaseReplacement (see the connection pooling note on [UseSqliteNoCaseReplacement\(this DbContextOptionsBuilder\)](SqliteNoCaseReplacementExtensions.UseSqliteNoCaseReplacement#DotDoc.EntityFrameworkCore.Extensions.CaseInsensitivity.SqliteNoCaseReplacementExtensions.UseSqliteNoCaseReplacement(thisMicrosoft.EntityFrameworkCore.DbContextOptionsBuilder) 'DotDoc\.EntityFrameworkCore\.Extensions\.CaseInsensitivity\.SqliteNoCaseReplacementExtensions\.UseSqliteNoCaseReplacement\(this Microsoft\.EntityFrameworkCore\.DbContextOptionsBuilder\)')).
A connection used by such a DbContext doesn't need it, as EF Core registers the collation and functions on it itself.
It doesn't change how LINQ queries are translated.

In SQL you write yourself, call the nocase_* functions directly, and add COLLATE NOCASE to a concatenation or a function's result
that should ignore case, as SQLite gives them no collation (e.g. `a || b COLLATE NOCASE = 'X'`).

It can be called before or after the connection is opened, and only needs calling once: the collation and functions stay registered
on the connection when it's closed and opened again.

See the sort order note on [UseSqliteNoCaseReplacement\(this DbContextOptionsBuilder\)](SqliteNoCaseReplacementExtensions.UseSqliteNoCaseReplacement#DotDoc.EntityFrameworkCore.Extensions.CaseInsensitivity.SqliteNoCaseReplacementExtensions.UseSqliteNoCaseReplacement(thisMicrosoft.EntityFrameworkCore.DbContextOptionsBuilder) 'DotDoc\.EntityFrameworkCore\.Extensions\.CaseInsensitivity\.SqliteNoCaseReplacementExtensions\.UseSqliteNoCaseReplacement\(this Microsoft\.EntityFrameworkCore\.DbContextOptionsBuilder\)').

<a name='DotDoc.EntityFrameworkCore.Extensions.CaseInsensitivity.SqliteNoCaseReplacementExtensions.UseSqliteNoCaseReplacement(thisMicrosoft.EntityFrameworkCore.DbContextOptionsBuilder)'></a>

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
[optionsBuilder](SqliteNoCaseReplacementExtensions#DotDoc.EntityFrameworkCore.Extensions.CaseInsensitivity.SqliteNoCaseReplacementExtensions.UseSqliteNoCaseReplacement(thisMicrosoft.EntityFrameworkCore.DbContextOptionsBuilder).optionsBuilder 'DotDoc\.EntityFrameworkCore\.Extensions\.CaseInsensitivity\.SqliteNoCaseReplacementExtensions\.UseSqliteNoCaseReplacement\(this Microsoft\.EntityFrameworkCore\.DbContextOptionsBuilder\)\.optionsBuilder') is [null](https://docs.microsoft.com/en-us/dotnet/csharp/language-reference/keywords/null 'https://docs\.microsoft\.com/en\-us/dotnet/csharp/language\-reference/keywords/null')\.

[UnsupportedDatabaseTypeException](UnsupportedDatabaseTypeException 'DotDoc\.EntityFrameworkCore\.Extensions\.DatabaseType\.UnsupportedDatabaseTypeException')  
The database provider is not SQLite\.

[System\.InvalidOperationException](https://learn.microsoft.com/en-us/dotnet/api/system.invalidoperationexception 'System\.InvalidOperationException')  
\.NET is running in globalization\-invariant mode, which has no ICU to compare text with\.

### Remarks

Does the following on each connection EF Core uses, including one that was already open when it was given to EF Core
(e.g. for an in-memory database):
- Replaces SQLite's NOCASE collation, which only ignores the case of ASCII letters, with a Unicode one, so comparisons, sorting,
  grouping and indexes on a NOCASE column ignore the case of every letter (é = É) but not accents (e ≠ é);
- Replaces SQLite's lower and upper functions, which only change ASCII letters, with Unicode ones, so ToLower() and ToUpper()
  change every letter, on every column;
- Makes Contains, IndexOf, StartsWith, EndsWith, Replace, Trim / TrimStart / TrimEnd (with constant characters) and
  EF.Functions.Like ignore case in the same way, on a NOCASE column or EF.Functions.Collate(..., "NOCASE"), by translating
  them to nocase_* functions it registers alongside SQLite's built-in ones;
- Gives the result of Replace, Substring, ToLower, ToUpper and Trim / TrimStart / TrimEnd on a NOCASE column, and of a concatenation,
  ?? or conditional expression with a NOCASE operand, the NOCASE collation, as SQL Server gives the result its operands' collation,
  so comparing the result, sorting or grouping by it, or calling one of these methods on it, ignores case too.

Everything else is unchanged: on a column that isn't NOCASE, or an expression with no known collation (such as a constant, a number
converted to a string, or a column from raw SQL), those methods use EF Core's own translation and SQLite's built-in functions.

Computed values read from a subquery: a concatenation, ?? or conditional expression, or the result of Replace, Trim, ToLower and so on,
that EF Core reads from a subquery (e.g. a GroupBy key, or a value from a Union) keeps the NOCASE collation for comparing, sorting and grouping,
but Contains, StartsWith and the other methods on it don't ignore case, as its collation can't be seen there. Give it the collation where
the method is called, e.g. `EF.Functions.Collate(key, "NOCASE").StartsWith(value)`.

Connection pooling: Microsoft.Data.Sqlite pools connections by default, with one pool per connection string. Once a connection that uses
UseSqliteNoCaseReplacement has been opened with a connection string, every later connection with the same connection string must use it too.
That includes every DbContext, any SqliteConnection opened directly (by calling [UseSqliteNoCaseReplacement\(this SqliteConnection\)](SqliteNoCaseReplacementExtensions.UseSqliteNoCaseReplacement#DotDoc.EntityFrameworkCore.Extensions.CaseInsensitivity.SqliteNoCaseReplacementExtensions.UseSqliteNoCaseReplacement(thisMicrosoft.Data.Sqlite.SqliteConnection) 'DotDoc\.EntityFrameworkCore\.Extensions\.CaseInsensitivity\.SqliteNoCaseReplacementExtensions\.UseSqliteNoCaseReplacement\(this Microsoft\.Data\.Sqlite\.SqliteConnection\)')
on it), and design-time tools such as dotnet ef.
When a connection goes back to the pool, Microsoft.Data.Sqlite removes the collation and functions registered on it, and SQLite can't
fall back to its built-in NOCASE, lower and upper on that connection afterwards. The next connection that doesn't register them again
fails with "no such collation sequence: NOCASE" or "wrong number of arguments to function upper()" (or lower()).

Sort order: the NOCASE collation compares text with ICU, and SQLite stores indexes on NOCASE columns (including primary keys and unique
indexes) in that order. Every process that writes to the database must use the replacement (with
[UseSqliteNoCaseReplacement\(this SqliteConnection\)](SqliteNoCaseReplacementExtensions.UseSqliteNoCaseReplacement#DotDoc.EntityFrameworkCore.Extensions.CaseInsensitivity.SqliteNoCaseReplacementExtensions.UseSqliteNoCaseReplacement(thisMicrosoft.Data.Sqlite.SqliteConnection) 'DotDoc\.EntityFrameworkCore\.Extensions\.CaseInsensitivity\.SqliteNoCaseReplacementExtensions\.UseSqliteNoCaseReplacement\(this Microsoft\.Data\.Sqlite\.SqliteConnection\)') if it doesn't use EF Core), and `REINDEX NOCASE` should be run after first
using it on an existing database, and after an operating system, .NET or ICU upgrade, or moving the database to another machine, as the order
can change. It can't be used in globalization-invariant mode (InvariantGlobalization), which has no ICU.