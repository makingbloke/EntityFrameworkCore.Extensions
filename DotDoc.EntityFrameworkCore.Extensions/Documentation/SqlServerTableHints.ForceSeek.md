#### [DotDoc\.EntityFrameworkCore\.Extensions](Home 'Home')
### [DotDoc\.EntityFrameworkCore\.Extensions\.TableHints](DotDoc.EntityFrameworkCore.Extensions.TableHints 'DotDoc\.EntityFrameworkCore\.Extensions\.TableHints').[SqlServerTableHints](SqlServerTableHints 'DotDoc\.EntityFrameworkCore\.Extensions\.TableHints\.SqlServerTableHints')

## SqlServerTableHints\.ForceSeek Method

| Overloads | |
| :--- | :--- |
| [ForceSeek\(\)](SqlServerTableHints.ForceSeek#DotDoc.EntityFrameworkCore.Extensions.TableHints.SqlServerTableHints.ForceSeek() 'DotDoc\.EntityFrameworkCore\.Extensions\.TableHints\.SqlServerTableHints\.ForceSeek\(\)') | FORCESEEK\[\( \< index\_value \> \( \< index\_column\_name \> \[ , \.\.\. \]\)\)\]\. |
| [ForceSeek\(string, IEnumerable&lt;string&gt;\)](SqlServerTableHints.ForceSeek#DotDoc.EntityFrameworkCore.Extensions.TableHints.SqlServerTableHints.ForceSeek(string,System.Collections.Generic.IEnumerable_string_) 'DotDoc\.EntityFrameworkCore\.Extensions\.TableHints\.SqlServerTableHints\.ForceSeek\(string, System\.Collections\.Generic\.IEnumerable\<string\>\)') | FORCESEEK\[\( \< index\_value \> \( \< index\_column\_name \> \[ , \.\.\. \]\)\)\]\. |

<a name='DotDoc.EntityFrameworkCore.Extensions.TableHints.SqlServerTableHints.ForceSeek()'></a>

## SqlServerTableHints\.ForceSeek\(\) Method

FORCESEEK\[\( \< index\_value \> \( \< index\_column\_name \> \[ , \.\.\. \]\)\)\]\.

```csharp
public static DotDoc.EntityFrameworkCore.Extensions.TableHints.SqlServerTableHints ForceSeek();
```

#### Returns
[SqlServerTableHints](SqlServerTableHints 'DotDoc\.EntityFrameworkCore\.Extensions\.TableHints\.SqlServerTableHints')  
[SqlServerTableHints](SqlServerTableHints 'DotDoc\.EntityFrameworkCore\.Extensions\.TableHints\.SqlServerTableHints')\.

<a name='DotDoc.EntityFrameworkCore.Extensions.TableHints.SqlServerTableHints.ForceSeek(string,System.Collections.Generic.IEnumerable_string_)'></a>

## SqlServerTableHints\.ForceSeek\(string, IEnumerable\<string\>\) Method

FORCESEEK\[\( \< index\_value \> \( \< index\_column\_name \> \[ , \.\.\. \]\)\)\]\.

```csharp
public static DotDoc.EntityFrameworkCore.Extensions.TableHints.SqlServerTableHints ForceSeek(string indexValue, params System.Collections.Generic.IEnumerable<string> indexColumnNames);
```
#### Parameters

<a name='DotDoc.EntityFrameworkCore.Extensions.TableHints.SqlServerTableHints.ForceSeek(string,System.Collections.Generic.IEnumerable_string_).indexValue'></a>

`indexValue` [System\.String](https://learn.microsoft.com/en-us/dotnet/api/system.string 'System\.String')

The index value \(name\)\.

<a name='DotDoc.EntityFrameworkCore.Extensions.TableHints.SqlServerTableHints.ForceSeek(string,System.Collections.Generic.IEnumerable_string_).indexColumnNames'></a>

`indexColumnNames` [System\.Collections\.Generic\.IEnumerable&lt;](https://learn.microsoft.com/en-us/dotnet/api/system.collections.generic.ienumerable-1 'System\.Collections\.Generic\.IEnumerable\`1')[System\.String](https://learn.microsoft.com/en-us/dotnet/api/system.string 'System\.String')[&gt;](https://learn.microsoft.com/en-us/dotnet/api/system.collections.generic.ienumerable-1 'System\.Collections\.Generic\.IEnumerable\`1')

The index column names\.

#### Returns
[SqlServerTableHints](SqlServerTableHints 'DotDoc\.EntityFrameworkCore\.Extensions\.TableHints\.SqlServerTableHints')  
[SqlServerTableHints](SqlServerTableHints 'DotDoc\.EntityFrameworkCore\.Extensions\.TableHints\.SqlServerTableHints')\.