#### [DotDoc\.EntityFrameworkCore\.Extensions](Home 'Home')
### [DotDoc\.EntityFrameworkCore\.Extensions\.CaseInsensitivity](DotDoc.EntityFrameworkCore.Extensions.CaseInsensitivity 'DotDoc\.EntityFrameworkCore\.Extensions\.CaseInsensitivity').[CaseInsensitiveCollationExtensions](CaseInsensitiveCollationExtensions 'DotDoc\.EntityFrameworkCore\.Extensions\.CaseInsensitivity\.CaseInsensitiveCollationExtensions')

## CaseInsensitiveCollationExtensions\.UseCaseInsensitiveCollation\(this ModelBuilder, string\) Method

Use Case Insensitive Collation\.

```csharp
public static Microsoft.EntityFrameworkCore.ModelBuilder UseCaseInsensitiveCollation(this Microsoft.EntityFrameworkCore.ModelBuilder modelBuilder, string? databaseType);
```
#### Parameters

<a name='DotDoc.EntityFrameworkCore.Extensions.CaseInsensitivity.CaseInsensitiveCollationExtensions.UseCaseInsensitiveCollation(thisMicrosoft.EntityFrameworkCore.ModelBuilder,string).modelBuilder'></a>

`modelBuilder` [Microsoft\.EntityFrameworkCore\.ModelBuilder](https://learn.microsoft.com/en-us/dotnet/api/microsoft.entityframeworkcore.modelbuilder 'Microsoft\.EntityFrameworkCore\.ModelBuilder')

The builder used to construct the model for the context\.

<a name='DotDoc.EntityFrameworkCore.Extensions.CaseInsensitivity.CaseInsensitiveCollationExtensions.UseCaseInsensitiveCollation(thisMicrosoft.EntityFrameworkCore.ModelBuilder,string).databaseType'></a>

`databaseType` [System\.String](https://learn.microsoft.com/en-us/dotnet/api/system.string 'System\.String')

The database type\.

#### Returns
[Microsoft\.EntityFrameworkCore\.ModelBuilder](https://learn.microsoft.com/en-us/dotnet/api/microsoft.entityframeworkcore.modelbuilder 'Microsoft\.EntityFrameworkCore\.ModelBuilder')  
The same builder instance so multiple calls can be chained\.

#### Exceptions

[System\.ArgumentNullException](https://learn.microsoft.com/en-us/dotnet/api/system.argumentnullexception 'System\.ArgumentNullException')  
[modelBuilder](CaseInsensitiveCollationExtensions.UseCaseInsensitiveCollation.U9BW2H88SP1XA2LFJRA89OJ72#DotDoc.EntityFrameworkCore.Extensions.CaseInsensitivity.CaseInsensitiveCollationExtensions.UseCaseInsensitiveCollation(thisMicrosoft.EntityFrameworkCore.ModelBuilder,string).modelBuilder 'DotDoc\.EntityFrameworkCore\.Extensions\.CaseInsensitivity\.CaseInsensitiveCollationExtensions\.UseCaseInsensitiveCollation\(this Microsoft\.EntityFrameworkCore\.ModelBuilder, string\)\.modelBuilder') is [null](https://docs.microsoft.com/en-us/dotnet/csharp/language-reference/keywords/null 'https://docs\.microsoft\.com/en\-us/dotnet/csharp/language\-reference/keywords/null')\.

[UnsupportedDatabaseTypeException](UnsupportedDatabaseTypeException 'DotDoc\.EntityFrameworkCore\.Extensions\.DatabaseType\.UnsupportedDatabaseTypeException')  
[databaseType](CaseInsensitiveCollationExtensions.UseCaseInsensitiveCollation.U9BW2H88SP1XA2LFJRA89OJ72#DotDoc.EntityFrameworkCore.Extensions.CaseInsensitivity.CaseInsensitiveCollationExtensions.UseCaseInsensitiveCollation(thisMicrosoft.EntityFrameworkCore.ModelBuilder,string).databaseType 'DotDoc\.EntityFrameworkCore\.Extensions\.CaseInsensitivity\.CaseInsensitiveCollationExtensions\.UseCaseInsensitiveCollation\(this Microsoft\.EntityFrameworkCore\.ModelBuilder, string\)\.databaseType') is not SQLite or SQL Server\.

### Remarks

On SQL Server it sets the database collation to Latin1_General_100_CI_AS_SC_UTF8 (case insensitive, accent sensitive), which every column
without its own collation uses. SQLite has no database collation, so it sets the NOCASE collation on every string property that doesn't
already have a collation, in the entity types already in the model: call it after they've been added. On SQLite, call
[UseSqliteNoCaseReplacement\(this DbContextOptionsBuilder\)](SqliteNoCaseReplacementExtensions.UseSqliteNoCaseReplacement#DotDoc.EntityFrameworkCore.Extensions.CaseInsensitivity.SqliteNoCaseReplacementExtensions.UseSqliteNoCaseReplacement(thisMicrosoft.EntityFrameworkCore.DbContextOptionsBuilder) 'DotDoc\.EntityFrameworkCore\.Extensions\.CaseInsensitivity\.SqliteNoCaseReplacementExtensions\.UseSqliteNoCaseReplacement\(this Microsoft\.EntityFrameworkCore\.DbContextOptionsBuilder\)') as well, so NOCASE ignores the case
of every letter and not just ASCII ones.

Comparing two columns with different collations (for example a property given its own collation with UseCollation, or an entity type added
to the model after this is called) behaves differently on the two databases:
- SQLite uses the collation of the left-hand column, and a column without a collation is BINARY. So when only one column is NOCASE,
  `a.Name == b.Code` and `b.Code == a.Name` can give different results, and a join's results depend on which side each key is on.
  With UseSqliteNoCaseReplacement, string methods follow the collation of the column they're called on, so `a.Name.Contains(b.Code)`
  ignores case when a.Name is NOCASE, whatever the collation of b.Code.
- SQL Server throws an exception ("Cannot resolve the collation conflict"), for the comparison and for the string methods.

To get the same result on both, give one side an explicit collation with `EF.Functions.Collate` (NOCASE on SQLite,
Latin1_General_100_CI_AS_SC_UTF8 on SQL Server), which takes precedence on both databases, e.g. `EF.Functions.Collate(a.Name, "NOCASE") == b.Code`.
Comparing a column with a parameter or a constant always uses the column's collation.