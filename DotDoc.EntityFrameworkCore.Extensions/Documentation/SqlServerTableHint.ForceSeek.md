#### [DotDoc\.EntityFrameworkCore\.Extensions](Home.md 'Home')
### [DotDoc\.EntityFrameworkCore\.Extensions\.TableHints](DotDoc.EntityFrameworkCore.Extensions.TableHints.md 'DotDoc\.EntityFrameworkCore\.Extensions\.TableHints').[SqlServerTableHint](SqlServerTableHint.md 'DotDoc\.EntityFrameworkCore\.Extensions\.TableHints\.SqlServerTableHint')

## SqlServerTableHint\.ForceSeek Method

| Overloads | |
| :--- | :--- |
| [ForceSeek\(\)](SqlServerTableHint.ForceSeek.md#DotDoc.EntityFrameworkCore.Extensions.TableHints.SqlServerTableHint.ForceSeek() 'DotDoc\.EntityFrameworkCore\.Extensions\.TableHints\.SqlServerTableHint\.ForceSeek\(\)') | FORCESEEK\[\( \< index\_value \> \( \< index\_column\_name \> \[ , \.\.\. \]\)\)\]\. |
| [ForceSeek\(string, IEnumerable&lt;string&gt;\)](SqlServerTableHint.ForceSeek.md#DotDoc.EntityFrameworkCore.Extensions.TableHints.SqlServerTableHint.ForceSeek(string,System.Collections.Generic.IEnumerable_string_) 'DotDoc\.EntityFrameworkCore\.Extensions\.TableHints\.SqlServerTableHint\.ForceSeek\(string, System\.Collections\.Generic\.IEnumerable\<string\>\)') | FORCESEEK\[\( \< index\_value \> \( \< index\_column\_name \> \[ , \.\.\. \]\)\)\]\. |

<a name='DotDoc.EntityFrameworkCore.Extensions.TableHints.SqlServerTableHint.ForceSeek()'></a>

## SqlServerTableHint\.ForceSeek\(\) Method

FORCESEEK\[\( \< index\_value \> \( \< index\_column\_name \> \[ , \.\.\. \]\)\)\]\.

```csharp
public static DotDoc.EntityFrameworkCore.Extensions.TableHints.SqlServerTableHint ForceSeek();
```

#### Returns
[SqlServerTableHint](SqlServerTableHint.md 'DotDoc\.EntityFrameworkCore\.Extensions\.TableHints\.SqlServerTableHint')  
[SqlServerTableHint](SqlServerTableHint.md 'DotDoc\.EntityFrameworkCore\.Extensions\.TableHints\.SqlServerTableHint')\.

<a name='DotDoc.EntityFrameworkCore.Extensions.TableHints.SqlServerTableHint.ForceSeek(string,System.Collections.Generic.IEnumerable_string_)'></a>

## SqlServerTableHint\.ForceSeek\(string, IEnumerable\<string\>\) Method

FORCESEEK\[\( \< index\_value \> \( \< index\_column\_name \> \[ , \.\.\. \]\)\)\]\.

```csharp
public static DotDoc.EntityFrameworkCore.Extensions.TableHints.SqlServerTableHint ForceSeek(string indexValue, System.Collections.Generic.IEnumerable<string> indexColumnNames);
```
#### Parameters

<a name='DotDoc.EntityFrameworkCore.Extensions.TableHints.SqlServerTableHint.ForceSeek(string,System.Collections.Generic.IEnumerable_string_).indexValue'></a>

`indexValue` [System\.String](https://learn.microsoft.com/en-us/dotnet/api/system.string 'System\.String')

The index value \(name\)\.

<a name='DotDoc.EntityFrameworkCore.Extensions.TableHints.SqlServerTableHint.ForceSeek(string,System.Collections.Generic.IEnumerable_string_).indexColumnNames'></a>

`indexColumnNames` [System\.Collections\.Generic\.IEnumerable&lt;](https://learn.microsoft.com/en-us/dotnet/api/system.collections.generic.ienumerable-1 'System\.Collections\.Generic\.IEnumerable\`1')[System\.String](https://learn.microsoft.com/en-us/dotnet/api/system.string 'System\.String')[&gt;](https://learn.microsoft.com/en-us/dotnet/api/system.collections.generic.ienumerable-1 'System\.Collections\.Generic\.IEnumerable\`1')

The index column names\.

#### Returns
[SqlServerTableHint](SqlServerTableHint.md 'DotDoc\.EntityFrameworkCore\.Extensions\.TableHints\.SqlServerTableHint')  
[SqlServerTableHint](SqlServerTableHint.md 'DotDoc\.EntityFrameworkCore\.Extensions\.TableHints\.SqlServerTableHint')\.