#### [DotDoc\.EntityFrameworkCore\.Extensions](Home 'Home')
### [DotDoc\.EntityFrameworkCore\.Extensions\.TableHints](DotDoc.EntityFrameworkCore.Extensions.TableHints 'DotDoc\.EntityFrameworkCore\.Extensions\.TableHints').[TableHintsBuilder](TableHintsBuilder 'DotDoc\.EntityFrameworkCore\.Extensions\.TableHints\.TableHintsBuilder')

## TableHintsBuilder\.For\<TEntity\>\(IEnumerable\<ITableHint\>\) Method

Adds table hints to the table \(or view\) of an entity type\.

```csharp
public DotDoc.EntityFrameworkCore.Extensions.TableHints.TableHintsBuilder For<TEntity>(params System.Collections.Generic.IEnumerable<DotDoc.EntityFrameworkCore.Extensions.TableHints.ITableHint> tableHints)
    where TEntity : class;
```
#### Type parameters

<a name='DotDoc.EntityFrameworkCore.Extensions.TableHints.TableHintsBuilder.For_TEntity_(System.Collections.Generic.IEnumerable_DotDoc.EntityFrameworkCore.Extensions.TableHints.ITableHint_).TEntity'></a>

`TEntity`

The entity type\.
#### Parameters

<a name='DotDoc.EntityFrameworkCore.Extensions.TableHints.TableHintsBuilder.For_TEntity_(System.Collections.Generic.IEnumerable_DotDoc.EntityFrameworkCore.Extensions.TableHints.ITableHint_).tableHints'></a>

`tableHints` [System\.Collections\.Generic\.IEnumerable&lt;](https://learn.microsoft.com/en-us/dotnet/api/system.collections.generic.ienumerable-1 'System\.Collections\.Generic\.IEnumerable\`1')[ITableHint](ITableHint 'DotDoc\.EntityFrameworkCore\.Extensions\.TableHints\.ITableHint')[&gt;](https://learn.microsoft.com/en-us/dotnet/api/system.collections.generic.ienumerable-1 'System\.Collections\.Generic\.IEnumerable\`1')

The table hints to add\.

#### Returns
[TableHintsBuilder](TableHintsBuilder 'DotDoc\.EntityFrameworkCore\.Extensions\.TableHints\.TableHintsBuilder')  
The same builder instance so multiple calls can be chained\.

#### Exceptions

[System\.ArgumentNullException](https://learn.microsoft.com/en-us/dotnet/api/system.argumentnullexception 'System\.ArgumentNullException')  
[tableHints](TableHintsBuilder.For.TKRHZJOJ3D7U6MO2POY92WKN3#DotDoc.EntityFrameworkCore.Extensions.TableHints.TableHintsBuilder.For_TEntity_(System.Collections.Generic.IEnumerable_DotDoc.EntityFrameworkCore.Extensions.TableHints.ITableHint_).tableHints 'DotDoc\.EntityFrameworkCore\.Extensions\.TableHints\.TableHintsBuilder\.For\<TEntity\>\(System\.Collections\.Generic\.IEnumerable\<DotDoc\.EntityFrameworkCore\.Extensions\.TableHints\.ITableHint\>\)\.tableHints') is [null](https://docs.microsoft.com/en-us/dotnet/csharp/language-reference/keywords/null 'https://docs\.microsoft\.com/en\-us/dotnet/csharp/language\-reference/keywords/null')\.

[System\.ArgumentException](https://learn.microsoft.com/en-us/dotnet/api/system.argumentexception 'System\.ArgumentException')  
[tableHints](TableHintsBuilder.For.TKRHZJOJ3D7U6MO2POY92WKN3#DotDoc.EntityFrameworkCore.Extensions.TableHints.TableHintsBuilder.For_TEntity_(System.Collections.Generic.IEnumerable_DotDoc.EntityFrameworkCore.Extensions.TableHints.ITableHint_).tableHints 'DotDoc\.EntityFrameworkCore\.Extensions\.TableHints\.TableHintsBuilder\.For\<TEntity\>\(System\.Collections\.Generic\.IEnumerable\<DotDoc\.EntityFrameworkCore\.Extensions\.TableHints\.ITableHint\>\)\.tableHints') is empty or contains a [null](https://docs.microsoft.com/en-us/dotnet/csharp/language-reference/keywords/null 'https://docs\.microsoft\.com/en\-us/dotnet/csharp/language\-reference/keywords/null') hint, [TEntity](TableHintsBuilder.For.TKRHZJOJ3D7U6MO2POY92WKN3#DotDoc.EntityFrameworkCore.Extensions.TableHints.TableHintsBuilder.For_TEntity_(System.Collections.Generic.IEnumerable_DotDoc.EntityFrameworkCore.Extensions.TableHints.ITableHint_).TEntity 'DotDoc\.EntityFrameworkCore\.Extensions\.TableHints\.TableHintsBuilder\.For\<TEntity\>\(System\.Collections\.Generic\.IEnumerable\<DotDoc\.EntityFrameworkCore\.Extensions\.TableHints\.ITableHint\>\)\.TEntity') isn't an entity type in the
            model \(or is a shared type entity type\), isn't mapped to a table or view, or its table has already been given hints\.

### Remarks
The hints are added to every use of the table in the query, and only to that table\. For an entity type in a table\-per\-type hierarchy that's
the entity type's own table, not the tables of its base types\.