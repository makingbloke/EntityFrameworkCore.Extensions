#### [DotDoc\.EntityFrameworkCore\.Extensions](Home 'Home')
### [DotDoc\.EntityFrameworkCore\.Extensions\.TableHints](DotDoc.EntityFrameworkCore.Extensions.TableHints 'DotDoc\.EntityFrameworkCore\.Extensions\.TableHints').[SqlServerTableHints](SqlServerTableHints 'DotDoc\.EntityFrameworkCore\.Extensions\.TableHints\.SqlServerTableHints')

## SqlServerTableHints\.Index\(IEnumerable\<string\>\) Method

INDEX\( \<index\_value\> \[ , \.\.\.n\] \) \| INDEX = \( \<index\_value\> \)\.

```csharp
public static DotDoc.EntityFrameworkCore.Extensions.TableHints.SqlServerTableHints Index(params System.Collections.Generic.IEnumerable<string> indexValues);
```
#### Parameters

<a name='DotDoc.EntityFrameworkCore.Extensions.TableHints.SqlServerTableHints.Index(System.Collections.Generic.IEnumerable_string_).indexValues'></a>

`indexValues` [System\.Collections\.Generic\.IEnumerable&lt;](https://learn.microsoft.com/en-us/dotnet/api/system.collections.generic.ienumerable-1 'System\.Collections\.Generic\.IEnumerable\`1')[System\.String](https://learn.microsoft.com/en-us/dotnet/api/system.string 'System\.String')[&gt;](https://learn.microsoft.com/en-us/dotnet/api/system.collections.generic.ienumerable-1 'System\.Collections\.Generic\.IEnumerable\`1')

The index values \(names\)\.

#### Returns
[SqlServerTableHints](SqlServerTableHints 'DotDoc\.EntityFrameworkCore\.Extensions\.TableHints\.SqlServerTableHints')  
[SqlServerTableHints](SqlServerTableHints 'DotDoc\.EntityFrameworkCore\.Extensions\.TableHints\.SqlServerTableHints')\.

#### Exceptions

[System\.ArgumentNullException](https://learn.microsoft.com/en-us/dotnet/api/system.argumentnullexception 'System\.ArgumentNullException')  
[indexValues](SqlServerTableHints.Index.C0MPK4EUBLJE9PAEJQ30HMWBE#DotDoc.EntityFrameworkCore.Extensions.TableHints.SqlServerTableHints.Index(System.Collections.Generic.IEnumerable_string_).indexValues 'DotDoc\.EntityFrameworkCore\.Extensions\.TableHints\.SqlServerTableHints\.Index\(System\.Collections\.Generic\.IEnumerable\<string\>\)\.indexValues') is [null](https://docs.microsoft.com/en-us/dotnet/csharp/language-reference/keywords/null 'https://docs\.microsoft\.com/en\-us/dotnet/csharp/language\-reference/keywords/null')\.

[System\.ArgumentException](https://learn.microsoft.com/en-us/dotnet/api/system.argumentexception 'System\.ArgumentException')  
[indexValues](SqlServerTableHints.Index.C0MPK4EUBLJE9PAEJQ30HMWBE#DotDoc.EntityFrameworkCore.Extensions.TableHints.SqlServerTableHints.Index(System.Collections.Generic.IEnumerable_string_).indexValues 'DotDoc\.EntityFrameworkCore\.Extensions\.TableHints\.SqlServerTableHints\.Index\(System\.Collections\.Generic\.IEnumerable\<string\>\)\.indexValues') is empty, or contains a [null](https://docs.microsoft.com/en-us/dotnet/csharp/language-reference/keywords/null 'https://docs\.microsoft\.com/en\-us/dotnet/csharp/language\-reference/keywords/null') or empty value\.