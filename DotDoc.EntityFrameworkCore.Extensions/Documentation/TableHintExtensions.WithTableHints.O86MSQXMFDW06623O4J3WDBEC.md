#### [DotDoc\.EntityFrameworkCore\.Extensions](Home.md 'Home')
### [DotDoc\.EntityFrameworkCore\.Extensions\.TableHints](DotDoc.EntityFrameworkCore.Extensions.TableHints.md 'DotDoc\.EntityFrameworkCore\.Extensions\.TableHints').[TableHintExtensions](TableHintExtensions.md 'DotDoc\.EntityFrameworkCore\.Extensions\.TableHints\.TableHintExtensions')

## TableHintExtensions\.WithTableHints\<TSource\>\(this IQueryable\<TSource\>, IEnumerable\<ITableHint\>\) Method

Adds table hints to a query\.

```csharp
public static System.Linq.IQueryable<TSource> WithTableHints<TSource>(this System.Linq.IQueryable<TSource> source, System.Collections.Generic.IEnumerable<DotDoc.EntityFrameworkCore.Extensions.TableHints.ITableHint> tableHints)
    where TSource : class;
```
#### Type parameters

<a name='DotDoc.EntityFrameworkCore.Extensions.TableHints.TableHintExtensions.WithTableHints_TSource_(thisSystem.Linq.IQueryable_TSource_,System.Collections.Generic.IEnumerable_DotDoc.EntityFrameworkCore.Extensions.TableHints.ITableHint_).TSource'></a>

`TSource`

The type of the elements of [source](TableHintExtensions.WithTableHints.O86MSQXMFDW06623O4J3WDBEC.md#DotDoc.EntityFrameworkCore.Extensions.TableHints.TableHintExtensions.WithTableHints_TSource_(thisSystem.Linq.IQueryable_TSource_,System.Collections.Generic.IEnumerable_DotDoc.EntityFrameworkCore.Extensions.TableHints.ITableHint_).source 'DotDoc\.EntityFrameworkCore\.Extensions\.TableHints\.TableHintExtensions\.WithTableHints\<TSource\>\(this System\.Linq\.IQueryable\<TSource\>, System\.Collections\.Generic\.IEnumerable\<DotDoc\.EntityFrameworkCore\.Extensions\.TableHints\.ITableHint\>\)\.source')\.
#### Parameters

<a name='DotDoc.EntityFrameworkCore.Extensions.TableHints.TableHintExtensions.WithTableHints_TSource_(thisSystem.Linq.IQueryable_TSource_,System.Collections.Generic.IEnumerable_DotDoc.EntityFrameworkCore.Extensions.TableHints.ITableHint_).source'></a>

`source` [System\.Linq\.IQueryable&lt;](https://learn.microsoft.com/en-us/dotnet/api/system.linq.iqueryable-1 'System\.Linq\.IQueryable\`1')[TSource](TableHintExtensions.WithTableHints.O86MSQXMFDW06623O4J3WDBEC.md#DotDoc.EntityFrameworkCore.Extensions.TableHints.TableHintExtensions.WithTableHints_TSource_(thisSystem.Linq.IQueryable_TSource_,System.Collections.Generic.IEnumerable_DotDoc.EntityFrameworkCore.Extensions.TableHints.ITableHint_).TSource 'DotDoc\.EntityFrameworkCore\.Extensions\.TableHints\.TableHintExtensions\.WithTableHints\<TSource\>\(this System\.Linq\.IQueryable\<TSource\>, System\.Collections\.Generic\.IEnumerable\<DotDoc\.EntityFrameworkCore\.Extensions\.TableHints\.ITableHint\>\)\.TSource')[&gt;](https://learn.microsoft.com/en-us/dotnet/api/system.linq.iqueryable-1 'System\.Linq\.IQueryable\`1')

The source query [System\.Linq\.IQueryable&lt;&gt;](https://learn.microsoft.com/en-us/dotnet/api/system.linq.iqueryable-1 'System\.Linq\.IQueryable\`1')\.

<a name='DotDoc.EntityFrameworkCore.Extensions.TableHints.TableHintExtensions.WithTableHints_TSource_(thisSystem.Linq.IQueryable_TSource_,System.Collections.Generic.IEnumerable_DotDoc.EntityFrameworkCore.Extensions.TableHints.ITableHint_).tableHints'></a>

`tableHints` [System\.Collections\.Generic\.IEnumerable&lt;](https://learn.microsoft.com/en-us/dotnet/api/system.collections.generic.ienumerable-1 'System\.Collections\.Generic\.IEnumerable\`1')[ITableHint](ITableHint.md 'DotDoc\.EntityFrameworkCore\.Extensions\.TableHints\.ITableHint')[&gt;](https://learn.microsoft.com/en-us/dotnet/api/system.collections.generic.ienumerable-1 'System\.Collections\.Generic\.IEnumerable\`1')

The table hints to add\.

#### Returns
[System\.Linq\.IQueryable&lt;](https://learn.microsoft.com/en-us/dotnet/api/system.linq.iqueryable-1 'System\.Linq\.IQueryable\`1')[TSource](TableHintExtensions.WithTableHints.O86MSQXMFDW06623O4J3WDBEC.md#DotDoc.EntityFrameworkCore.Extensions.TableHints.TableHintExtensions.WithTableHints_TSource_(thisSystem.Linq.IQueryable_TSource_,System.Collections.Generic.IEnumerable_DotDoc.EntityFrameworkCore.Extensions.TableHints.ITableHint_).TSource 'DotDoc\.EntityFrameworkCore\.Extensions\.TableHints\.TableHintExtensions\.WithTableHints\<TSource\>\(this System\.Linq\.IQueryable\<TSource\>, System\.Collections\.Generic\.IEnumerable\<DotDoc\.EntityFrameworkCore\.Extensions\.TableHints\.ITableHint\>\)\.TSource')[&gt;](https://learn.microsoft.com/en-us/dotnet/api/system.linq.iqueryable-1 'System\.Linq\.IQueryable\`1')  
The number of rows updated in the database\.