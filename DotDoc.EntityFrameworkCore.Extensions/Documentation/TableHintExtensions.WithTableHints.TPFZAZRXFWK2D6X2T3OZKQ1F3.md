#### [DotDoc\.EntityFrameworkCore\.Extensions](Home 'Home')
### [DotDoc\.EntityFrameworkCore\.Extensions\.TableHints](DotDoc.EntityFrameworkCore.Extensions.TableHints 'DotDoc\.EntityFrameworkCore\.Extensions\.TableHints').[TableHintExtensions](TableHintExtensions 'DotDoc\.EntityFrameworkCore\.Extensions\.TableHints\.TableHintExtensions')

## TableHintExtensions\.WithTableHints\<TSource\>\(this IQueryable\<TSource\>, Action\<TableHintsBuilder\>\) Method

Adds table hints to a query, for the tables of the entity types given to the builder\.

```csharp
public static System.Linq.IQueryable<TSource> WithTableHints<TSource>(this System.Linq.IQueryable<TSource> source, System.Action<DotDoc.EntityFrameworkCore.Extensions.TableHints.TableHintsBuilder> buildAction)
    where TSource : class;
```
#### Type parameters

<a name='DotDoc.EntityFrameworkCore.Extensions.TableHints.TableHintExtensions.WithTableHints_TSource_(thisSystem.Linq.IQueryable_TSource_,System.Action_DotDoc.EntityFrameworkCore.Extensions.TableHints.TableHintsBuilder_).TSource'></a>

`TSource`

The type of the elements of [source](TableHintExtensions.WithTableHints.TPFZAZRXFWK2D6X2T3OZKQ1F3#DotDoc.EntityFrameworkCore.Extensions.TableHints.TableHintExtensions.WithTableHints_TSource_(thisSystem.Linq.IQueryable_TSource_,System.Action_DotDoc.EntityFrameworkCore.Extensions.TableHints.TableHintsBuilder_).source 'DotDoc\.EntityFrameworkCore\.Extensions\.TableHints\.TableHintExtensions\.WithTableHints\<TSource\>\(this System\.Linq\.IQueryable\<TSource\>, System\.Action\<DotDoc\.EntityFrameworkCore\.Extensions\.TableHints\.TableHintsBuilder\>\)\.source')\.
#### Parameters

<a name='DotDoc.EntityFrameworkCore.Extensions.TableHints.TableHintExtensions.WithTableHints_TSource_(thisSystem.Linq.IQueryable_TSource_,System.Action_DotDoc.EntityFrameworkCore.Extensions.TableHints.TableHintsBuilder_).source'></a>

`source` [System\.Linq\.IQueryable&lt;](https://learn.microsoft.com/en-us/dotnet/api/system.linq.iqueryable-1 'System\.Linq\.IQueryable\`1')[TSource](TableHintExtensions.WithTableHints.TPFZAZRXFWK2D6X2T3OZKQ1F3#DotDoc.EntityFrameworkCore.Extensions.TableHints.TableHintExtensions.WithTableHints_TSource_(thisSystem.Linq.IQueryable_TSource_,System.Action_DotDoc.EntityFrameworkCore.Extensions.TableHints.TableHintsBuilder_).TSource 'DotDoc\.EntityFrameworkCore\.Extensions\.TableHints\.TableHintExtensions\.WithTableHints\<TSource\>\(this System\.Linq\.IQueryable\<TSource\>, System\.Action\<DotDoc\.EntityFrameworkCore\.Extensions\.TableHints\.TableHintsBuilder\>\)\.TSource')[&gt;](https://learn.microsoft.com/en-us/dotnet/api/system.linq.iqueryable-1 'System\.Linq\.IQueryable\`1')

The source query [System\.Linq\.IQueryable&lt;&gt;](https://learn.microsoft.com/en-us/dotnet/api/system.linq.iqueryable-1 'System\.Linq\.IQueryable\`1')\.

<a name='DotDoc.EntityFrameworkCore.Extensions.TableHints.TableHintExtensions.WithTableHints_TSource_(thisSystem.Linq.IQueryable_TSource_,System.Action_DotDoc.EntityFrameworkCore.Extensions.TableHints.TableHintsBuilder_).buildAction'></a>

`buildAction` [System\.Action&lt;](https://learn.microsoft.com/en-us/dotnet/api/system.action-1 'System\.Action\`1')[TableHintsBuilder](TableHintsBuilder 'DotDoc\.EntityFrameworkCore\.Extensions\.TableHints\.TableHintsBuilder')[&gt;](https://learn.microsoft.com/en-us/dotnet/api/system.action-1 'System\.Action\`1')

A method that adds the table hints for each entity type's table, e\.g\. `h => h.For<Blog>(SqlServerTableHints.NoLock).For<Post>(SqlServerTableHints.NoLock)`\.

#### Returns
[System\.Linq\.IQueryable&lt;](https://learn.microsoft.com/en-us/dotnet/api/system.linq.iqueryable-1 'System\.Linq\.IQueryable\`1')[TSource](TableHintExtensions.WithTableHints.TPFZAZRXFWK2D6X2T3OZKQ1F3#DotDoc.EntityFrameworkCore.Extensions.TableHints.TableHintExtensions.WithTableHints_TSource_(thisSystem.Linq.IQueryable_TSource_,System.Action_DotDoc.EntityFrameworkCore.Extensions.TableHints.TableHintsBuilder_).TSource 'DotDoc\.EntityFrameworkCore\.Extensions\.TableHints\.TableHintExtensions\.WithTableHints\<TSource\>\(this System\.Linq\.IQueryable\<TSource\>, System\.Action\<DotDoc\.EntityFrameworkCore\.Extensions\.TableHints\.TableHintsBuilder\>\)\.TSource')[&gt;](https://learn.microsoft.com/en-us/dotnet/api/system.linq.iqueryable-1 'System\.Linq\.IQueryable\`1')  
An instance of the [TSource](TableHintExtensions.WithTableHints.TPFZAZRXFWK2D6X2T3OZKQ1F3#DotDoc.EntityFrameworkCore.Extensions.TableHints.TableHintExtensions.WithTableHints_TSource_(thisSystem.Linq.IQueryable_TSource_,System.Action_DotDoc.EntityFrameworkCore.Extensions.TableHints.TableHintsBuilder_).TSource 'DotDoc\.EntityFrameworkCore\.Extensions\.TableHints\.TableHintExtensions\.WithTableHints\<TSource\>\(this System\.Linq\.IQueryable\<TSource\>, System\.Action\<DotDoc\.EntityFrameworkCore\.Extensions\.TableHints\.TableHintsBuilder\>\)\.TSource') with the table hints applied\.

#### Exceptions

[System\.ArgumentNullException](https://learn.microsoft.com/en-us/dotnet/api/system.argumentnullexception 'System\.ArgumentNullException')  
[source](TableHintExtensions.WithTableHints.TPFZAZRXFWK2D6X2T3OZKQ1F3#DotDoc.EntityFrameworkCore.Extensions.TableHints.TableHintExtensions.WithTableHints_TSource_(thisSystem.Linq.IQueryable_TSource_,System.Action_DotDoc.EntityFrameworkCore.Extensions.TableHints.TableHintsBuilder_).source 'DotDoc\.EntityFrameworkCore\.Extensions\.TableHints\.TableHintExtensions\.WithTableHints\<TSource\>\(this System\.Linq\.IQueryable\<TSource\>, System\.Action\<DotDoc\.EntityFrameworkCore\.Extensions\.TableHints\.TableHintsBuilder\>\)\.source') or [buildAction](TableHintExtensions.WithTableHints.TPFZAZRXFWK2D6X2T3OZKQ1F3#DotDoc.EntityFrameworkCore.Extensions.TableHints.TableHintExtensions.WithTableHints_TSource_(thisSystem.Linq.IQueryable_TSource_,System.Action_DotDoc.EntityFrameworkCore.Extensions.TableHints.TableHintsBuilder_).buildAction 'DotDoc\.EntityFrameworkCore\.Extensions\.TableHints\.TableHintExtensions\.WithTableHints\<TSource\>\(this System\.Linq\.IQueryable\<TSource\>, System\.Action\<DotDoc\.EntityFrameworkCore\.Extensions\.TableHints\.TableHintsBuilder\>\)\.buildAction') is [null](https://docs.microsoft.com/en-us/dotnet/csharp/language-reference/keywords/null 'https://docs\.microsoft\.com/en\-us/dotnet/csharp/language\-reference/keywords/null')\.

[System\.ArgumentException](https://learn.microsoft.com/en-us/dotnet/api/system.argumentexception 'System\.ArgumentException')  
[source](TableHintExtensions.WithTableHints.TPFZAZRXFWK2D6X2T3OZKQ1F3#DotDoc.EntityFrameworkCore.Extensions.TableHints.TableHintExtensions.WithTableHints_TSource_(thisSystem.Linq.IQueryable_TSource_,System.Action_DotDoc.EntityFrameworkCore.Extensions.TableHints.TableHintsBuilder_).source 'DotDoc\.EntityFrameworkCore\.Extensions\.TableHints\.TableHintExtensions\.WithTableHints\<TSource\>\(this System\.Linq\.IQueryable\<TSource\>, System\.Action\<DotDoc\.EntityFrameworkCore\.Extensions\.TableHints\.TableHintsBuilder\>\)\.source') is not an EF Core query, or on SQL Server [buildAction](TableHintExtensions.WithTableHints.TPFZAZRXFWK2D6X2T3OZKQ1F3#DotDoc.EntityFrameworkCore.Extensions.TableHints.TableHintExtensions.WithTableHints_TSource_(thisSystem.Linq.IQueryable_TSource_,System.Action_DotDoc.EntityFrameworkCore.Extensions.TableHints.TableHintsBuilder_).buildAction 'DotDoc\.EntityFrameworkCore\.Extensions\.TableHints\.TableHintExtensions\.WithTableHints\<TSource\>\(this System\.Linq\.IQueryable\<TSource\>, System\.Action\<DotDoc\.EntityFrameworkCore\.Extensions\.TableHints\.TableHintsBuilder\>\)\.buildAction') adds no table hints, or a call to
            [For&lt;TEntity&gt;\(IEnumerable&lt;ITableHint&gt;\)](TableHintsBuilder.For.TKRHZJOJ3D7U6MO2POY92WKN3 'DotDoc\.EntityFrameworkCore\.Extensions\.TableHints\.TableHintsBuilder\.For\<TEntity\>\(System\.Collections\.Generic\.IEnumerable\<DotDoc\.EntityFrameworkCore\.Extensions\.TableHints\.ITableHint\>\)') isn't valid \(see its exceptions\)\.

[System\.InvalidOperationException](https://learn.microsoft.com/en-us/dotnet/api/system.invalidoperationexception 'System\.InvalidOperationException')  
On SQL Server, [UseTableHintExtensions\(this DbContextOptionsBuilder\)](TableHintExtensions.UseTableHintExtensions.9WHAQZ2G0A2D4SOMTY90T0IQ6 'DotDoc\.EntityFrameworkCore\.Extensions\.TableHints\.TableHintExtensions\.UseTableHintExtensions\(this Microsoft\.EntityFrameworkCore\.DbContextOptionsBuilder\)') was not called when configuring the DbContext, or another extension library has replaced its query generator\.

### Remarks
Table hints are SQL Server only\. On SQLite the query is returned unchanged and [buildAction](TableHintExtensions.WithTableHints.TPFZAZRXFWK2D6X2T3OZKQ1F3#DotDoc.EntityFrameworkCore.Extensions.TableHints.TableHintExtensions.WithTableHints_TSource_(thisSystem.Linq.IQueryable_TSource_,System.Action_DotDoc.EntityFrameworkCore.Extensions.TableHints.TableHintsBuilder_).buildAction 'DotDoc\.EntityFrameworkCore\.Extensions\.TableHints\.TableHintExtensions\.WithTableHints\<TSource\>\(this System\.Linq\.IQueryable\<TSource\>, System\.Action\<DotDoc\.EntityFrameworkCore\.Extensions\.TableHints\.TableHintsBuilder\>\)\.buildAction') isn't called\.
The hints for an entity type are added to every use of its table in the query \(including joined, included and subquery tables\), and only to that
table\. Calling [WithTableHints&lt;TSource&gt;\(this IQueryable&lt;TSource&gt;, Action&lt;TableHintsBuilder&gt;\)](TableHintExtensions.WithTableHints.TPFZAZRXFWK2D6X2T3OZKQ1F3 'DotDoc\.EntityFrameworkCore\.Extensions\.TableHints\.TableHintExtensions\.WithTableHints\<TSource\>\(this System\.Linq\.IQueryable\<TSource\>, System\.Action\<DotDoc\.EntityFrameworkCore\.Extensions\.TableHints\.TableHintsBuilder\>\)') more than once on the same query is allowed for different tables, but different hints for the same
table throw an [System\.InvalidOperationException](https://learn.microsoft.com/en-us/dotnet/api/system.invalidoperationexception 'System\.InvalidOperationException') when the query is run\.