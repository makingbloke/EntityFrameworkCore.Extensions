#### [DotDoc\.EntityFrameworkCore\.Extensions](Home 'Home')
### [DotDoc\.EntityFrameworkCore\.Extensions\.TableHints](DotDoc.EntityFrameworkCore.Extensions.TableHints 'DotDoc\.EntityFrameworkCore\.Extensions\.TableHints').[TableHintExtensions](TableHintExtensions 'DotDoc\.EntityFrameworkCore\.Extensions\.TableHints\.TableHintExtensions')

## TableHintExtensions\.WithTableHints\<TSource\>\(this IQueryable\<TSource\>, IEnumerable\<ITableHint\>\) Method

Adds table hints to a query\.

```csharp
public static System.Linq.IQueryable<TSource> WithTableHints<TSource>(this System.Linq.IQueryable<TSource> source, params System.Collections.Generic.IEnumerable<DotDoc.EntityFrameworkCore.Extensions.TableHints.ITableHint> tableHints)
    where TSource : class;
```
#### Type parameters

<a name='DotDoc.EntityFrameworkCore.Extensions.TableHints.TableHintExtensions.WithTableHints_TSource_(thisSystem.Linq.IQueryable_TSource_,System.Collections.Generic.IEnumerable_DotDoc.EntityFrameworkCore.Extensions.TableHints.ITableHint_).TSource'></a>

`TSource`

The type of the elements of [source](TableHintExtensions.WithTableHints.O86MSQXMFDW06623O4J3WDBEC#DotDoc.EntityFrameworkCore.Extensions.TableHints.TableHintExtensions.WithTableHints_TSource_(thisSystem.Linq.IQueryable_TSource_,System.Collections.Generic.IEnumerable_DotDoc.EntityFrameworkCore.Extensions.TableHints.ITableHint_).source 'DotDoc\.EntityFrameworkCore\.Extensions\.TableHints\.TableHintExtensions\.WithTableHints\<TSource\>\(this System\.Linq\.IQueryable\<TSource\>, System\.Collections\.Generic\.IEnumerable\<DotDoc\.EntityFrameworkCore\.Extensions\.TableHints\.ITableHint\>\)\.source')\.
#### Parameters

<a name='DotDoc.EntityFrameworkCore.Extensions.TableHints.TableHintExtensions.WithTableHints_TSource_(thisSystem.Linq.IQueryable_TSource_,System.Collections.Generic.IEnumerable_DotDoc.EntityFrameworkCore.Extensions.TableHints.ITableHint_).source'></a>

`source` [System\.Linq\.IQueryable&lt;](https://learn.microsoft.com/en-us/dotnet/api/system.linq.iqueryable-1 'System\.Linq\.IQueryable\`1')[TSource](TableHintExtensions.WithTableHints.O86MSQXMFDW06623O4J3WDBEC#DotDoc.EntityFrameworkCore.Extensions.TableHints.TableHintExtensions.WithTableHints_TSource_(thisSystem.Linq.IQueryable_TSource_,System.Collections.Generic.IEnumerable_DotDoc.EntityFrameworkCore.Extensions.TableHints.ITableHint_).TSource 'DotDoc\.EntityFrameworkCore\.Extensions\.TableHints\.TableHintExtensions\.WithTableHints\<TSource\>\(this System\.Linq\.IQueryable\<TSource\>, System\.Collections\.Generic\.IEnumerable\<DotDoc\.EntityFrameworkCore\.Extensions\.TableHints\.ITableHint\>\)\.TSource')[&gt;](https://learn.microsoft.com/en-us/dotnet/api/system.linq.iqueryable-1 'System\.Linq\.IQueryable\`1')

The source query [System\.Linq\.IQueryable&lt;&gt;](https://learn.microsoft.com/en-us/dotnet/api/system.linq.iqueryable-1 'System\.Linq\.IQueryable\`1')\.

<a name='DotDoc.EntityFrameworkCore.Extensions.TableHints.TableHintExtensions.WithTableHints_TSource_(thisSystem.Linq.IQueryable_TSource_,System.Collections.Generic.IEnumerable_DotDoc.EntityFrameworkCore.Extensions.TableHints.ITableHint_).tableHints'></a>

`tableHints` [System\.Collections\.Generic\.IEnumerable&lt;](https://learn.microsoft.com/en-us/dotnet/api/system.collections.generic.ienumerable-1 'System\.Collections\.Generic\.IEnumerable\`1')[ITableHint](ITableHint 'DotDoc\.EntityFrameworkCore\.Extensions\.TableHints\.ITableHint')[&gt;](https://learn.microsoft.com/en-us/dotnet/api/system.collections.generic.ienumerable-1 'System\.Collections\.Generic\.IEnumerable\`1')

The table hints to add\.

#### Returns
[System\.Linq\.IQueryable&lt;](https://learn.microsoft.com/en-us/dotnet/api/system.linq.iqueryable-1 'System\.Linq\.IQueryable\`1')[TSource](TableHintExtensions.WithTableHints.O86MSQXMFDW06623O4J3WDBEC#DotDoc.EntityFrameworkCore.Extensions.TableHints.TableHintExtensions.WithTableHints_TSource_(thisSystem.Linq.IQueryable_TSource_,System.Collections.Generic.IEnumerable_DotDoc.EntityFrameworkCore.Extensions.TableHints.ITableHint_).TSource 'DotDoc\.EntityFrameworkCore\.Extensions\.TableHints\.TableHintExtensions\.WithTableHints\<TSource\>\(this System\.Linq\.IQueryable\<TSource\>, System\.Collections\.Generic\.IEnumerable\<DotDoc\.EntityFrameworkCore\.Extensions\.TableHints\.ITableHint\>\)\.TSource')[&gt;](https://learn.microsoft.com/en-us/dotnet/api/system.linq.iqueryable-1 'System\.Linq\.IQueryable\`1')  
An instance of the [TSource](TableHintExtensions.WithTableHints.O86MSQXMFDW06623O4J3WDBEC#DotDoc.EntityFrameworkCore.Extensions.TableHints.TableHintExtensions.WithTableHints_TSource_(thisSystem.Linq.IQueryable_TSource_,System.Collections.Generic.IEnumerable_DotDoc.EntityFrameworkCore.Extensions.TableHints.ITableHint_).TSource 'DotDoc\.EntityFrameworkCore\.Extensions\.TableHints\.TableHintExtensions\.WithTableHints\<TSource\>\(this System\.Linq\.IQueryable\<TSource\>, System\.Collections\.Generic\.IEnumerable\<DotDoc\.EntityFrameworkCore\.Extensions\.TableHints\.ITableHint\>\)\.TSource') with the table hints applied\.

#### Exceptions

[System\.InvalidOperationException](https://learn.microsoft.com/en-us/dotnet/api/system.invalidoperationexception 'System\.InvalidOperationException')  
Thrown on SQL Server if the custom query generator that writes the hints is not registered
\([UseTableHintExtensions\(this DbContextOptionsBuilder\)](TableHintExtensions.UseTableHintExtensions.9WHAQZ2G0A2D4SOMTY90T0IQ6 'DotDoc\.EntityFrameworkCore\.Extensions\.TableHints\.TableHintExtensions\.UseTableHintExtensions\(this Microsoft\.EntityFrameworkCore\.DbContextOptionsBuilder\)') was not called when configuring the DbContext\)\.

### Remarks
Table hints are SQL Server only\. On SQLite the query is returned unchanged\.