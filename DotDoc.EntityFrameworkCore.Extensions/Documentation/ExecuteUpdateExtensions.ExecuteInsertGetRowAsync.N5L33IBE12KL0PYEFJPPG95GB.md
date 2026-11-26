#### [DotDoc\.EntityFrameworkCore\.Extensions](Home 'Home')
### [DotDoc\.EntityFrameworkCore\.Extensions\.ExecuteUpdate](DotDoc.EntityFrameworkCore.Extensions.ExecuteUpdate 'DotDoc\.EntityFrameworkCore\.Extensions\.ExecuteUpdate').[ExecuteUpdateExtensions](ExecuteUpdateExtensions 'DotDoc\.EntityFrameworkCore\.Extensions\.ExecuteUpdate\.ExecuteUpdateExtensions')

## ExecuteUpdateExtensions\.ExecuteInsertGetRowAsync\<TSource\>\(this IQueryable\<TSource\>, Action\<UpdateSettersBuilder\<TSource\>\>, CancellationToken\) Method

Insert a database row\.

```csharp
public static System.Threading.Tasks.Task<TSource> ExecuteInsertGetRowAsync<TSource>(this System.Linq.IQueryable<TSource> source, System.Action<Microsoft.EntityFrameworkCore.Query.UpdateSettersBuilder<TSource>> setPropertyCalls, System.Threading.CancellationToken cancellationToken=default(System.Threading.CancellationToken))
    where TSource : class;
```
#### Type parameters

<a name='DotDoc.EntityFrameworkCore.Extensions.ExecuteUpdate.ExecuteUpdateExtensions.ExecuteInsertGetRowAsync_TSource_(thisSystem.Linq.IQueryable_TSource_,System.Action_Microsoft.EntityFrameworkCore.Query.UpdateSettersBuilder_TSource__,System.Threading.CancellationToken).TSource'></a>

`TSource`

The type of the elements of [source](ExecuteUpdateExtensions.ExecuteInsertGetRowAsync.N5L33IBE12KL0PYEFJPPG95GB#DotDoc.EntityFrameworkCore.Extensions.ExecuteUpdate.ExecuteUpdateExtensions.ExecuteInsertGetRowAsync_TSource_(thisSystem.Linq.IQueryable_TSource_,System.Action_Microsoft.EntityFrameworkCore.Query.UpdateSettersBuilder_TSource__,System.Threading.CancellationToken).source 'DotDoc\.EntityFrameworkCore\.Extensions\.ExecuteUpdate\.ExecuteUpdateExtensions\.ExecuteInsertGetRowAsync\<TSource\>\(this System\.Linq\.IQueryable\<TSource\>, System\.Action\<Microsoft\.EntityFrameworkCore\.Query\.UpdateSettersBuilder\<TSource\>\>, System\.Threading\.CancellationToken\)\.source')\.
#### Parameters

<a name='DotDoc.EntityFrameworkCore.Extensions.ExecuteUpdate.ExecuteUpdateExtensions.ExecuteInsertGetRowAsync_TSource_(thisSystem.Linq.IQueryable_TSource_,System.Action_Microsoft.EntityFrameworkCore.Query.UpdateSettersBuilder_TSource__,System.Threading.CancellationToken).source'></a>

`source` [System\.Linq\.IQueryable&lt;](https://learn.microsoft.com/en-us/dotnet/api/system.linq.iqueryable-1 'System\.Linq\.IQueryable\`1')[TSource](ExecuteUpdateExtensions.ExecuteInsertGetRowAsync.N5L33IBE12KL0PYEFJPPG95GB#DotDoc.EntityFrameworkCore.Extensions.ExecuteUpdate.ExecuteUpdateExtensions.ExecuteInsertGetRowAsync_TSource_(thisSystem.Linq.IQueryable_TSource_,System.Action_Microsoft.EntityFrameworkCore.Query.UpdateSettersBuilder_TSource__,System.Threading.CancellationToken).TSource 'DotDoc\.EntityFrameworkCore\.Extensions\.ExecuteUpdate\.ExecuteUpdateExtensions\.ExecuteInsertGetRowAsync\<TSource\>\(this System\.Linq\.IQueryable\<TSource\>, System\.Action\<Microsoft\.EntityFrameworkCore\.Query\.UpdateSettersBuilder\<TSource\>\>, System\.Threading\.CancellationToken\)\.TSource')[&gt;](https://learn.microsoft.com/en-us/dotnet/api/system.linq.iqueryable-1 'System\.Linq\.IQueryable\`1')

The table to insert the row into \(e\.g\. `context.Set<T>()`\), with no query operators\.

<a name='DotDoc.EntityFrameworkCore.Extensions.ExecuteUpdate.ExecuteUpdateExtensions.ExecuteInsertGetRowAsync_TSource_(thisSystem.Linq.IQueryable_TSource_,System.Action_Microsoft.EntityFrameworkCore.Query.UpdateSettersBuilder_TSource__,System.Threading.CancellationToken).setPropertyCalls'></a>

`setPropertyCalls` [System\.Action&lt;](https://learn.microsoft.com/en-us/dotnet/api/system.action-1 'System\.Action\`1')[Microsoft\.EntityFrameworkCore\.Query\.UpdateSettersBuilder&lt;](https://learn.microsoft.com/en-us/dotnet/api/microsoft.entityframeworkcore.query.updatesettersbuilder-1 'Microsoft\.EntityFrameworkCore\.Query\.UpdateSettersBuilder\`1')[TSource](ExecuteUpdateExtensions.ExecuteInsertGetRowAsync.N5L33IBE12KL0PYEFJPPG95GB#DotDoc.EntityFrameworkCore.Extensions.ExecuteUpdate.ExecuteUpdateExtensions.ExecuteInsertGetRowAsync_TSource_(thisSystem.Linq.IQueryable_TSource_,System.Action_Microsoft.EntityFrameworkCore.Query.UpdateSettersBuilder_TSource__,System.Threading.CancellationToken).TSource 'DotDoc\.EntityFrameworkCore\.Extensions\.ExecuteUpdate\.ExecuteUpdateExtensions\.ExecuteInsertGetRowAsync\<TSource\>\(this System\.Linq\.IQueryable\<TSource\>, System\.Action\<Microsoft\.EntityFrameworkCore\.Query\.UpdateSettersBuilder\<TSource\>\>, System\.Threading\.CancellationToken\)\.TSource')[&gt;](https://learn.microsoft.com/en-us/dotnet/api/microsoft.entityframeworkcore.query.updatesettersbuilder-1 'Microsoft\.EntityFrameworkCore\.Query\.UpdateSettersBuilder\`1')[&gt;](https://learn.microsoft.com/en-us/dotnet/api/system.action-1 'System\.Action\`1')

A method containing set property statements specifying the values of the inserted row's properties\.

<a name='DotDoc.EntityFrameworkCore.Extensions.ExecuteUpdate.ExecuteUpdateExtensions.ExecuteInsertGetRowAsync_TSource_(thisSystem.Linq.IQueryable_TSource_,System.Action_Microsoft.EntityFrameworkCore.Query.UpdateSettersBuilder_TSource__,System.Threading.CancellationToken).cancellationToken'></a>

`cancellationToken` [System\.Threading\.CancellationToken](https://learn.microsoft.com/en-us/dotnet/api/system.threading.cancellationtoken 'System\.Threading\.CancellationToken')

A [System\.Threading\.CancellationToken](https://learn.microsoft.com/en-us/dotnet/api/system.threading.cancellationtoken 'System\.Threading\.CancellationToken') to observe while waiting for the task to complete\.

#### Returns
[System\.Threading\.Tasks\.Task&lt;](https://learn.microsoft.com/en-us/dotnet/api/system.threading.tasks.task-1 'System\.Threading\.Tasks\.Task\`1')[TSource](ExecuteUpdateExtensions.ExecuteInsertGetRowAsync.N5L33IBE12KL0PYEFJPPG95GB#DotDoc.EntityFrameworkCore.Extensions.ExecuteUpdate.ExecuteUpdateExtensions.ExecuteInsertGetRowAsync_TSource_(thisSystem.Linq.IQueryable_TSource_,System.Action_Microsoft.EntityFrameworkCore.Query.UpdateSettersBuilder_TSource__,System.Threading.CancellationToken).TSource 'DotDoc\.EntityFrameworkCore\.Extensions\.ExecuteUpdate\.ExecuteUpdateExtensions\.ExecuteInsertGetRowAsync\<TSource\>\(this System\.Linq\.IQueryable\<TSource\>, System\.Action\<Microsoft\.EntityFrameworkCore\.Query\.UpdateSettersBuilder\<TSource\>\>, System\.Threading\.CancellationToken\)\.TSource')[&gt;](https://learn.microsoft.com/en-us/dotnet/api/system.threading.tasks.task-1 'System\.Threading\.Tasks\.Task\`1')  
An instance of the [TSource](ExecuteUpdateExtensions.ExecuteInsertGetRowAsync.N5L33IBE12KL0PYEFJPPG95GB#DotDoc.EntityFrameworkCore.Extensions.ExecuteUpdate.ExecuteUpdateExtensions.ExecuteInsertGetRowAsync_TSource_(thisSystem.Linq.IQueryable_TSource_,System.Action_Microsoft.EntityFrameworkCore.Query.UpdateSettersBuilder_TSource__,System.Threading.CancellationToken).TSource 'DotDoc\.EntityFrameworkCore\.Extensions\.ExecuteUpdate\.ExecuteUpdateExtensions\.ExecuteInsertGetRowAsync\<TSource\>\(this System\.Linq\.IQueryable\<TSource\>, System\.Action\<Microsoft\.EntityFrameworkCore\.Query\.UpdateSettersBuilder\<TSource\>\>, System\.Threading\.CancellationToken\)\.TSource') containing the inserted row\.

#### Exceptions

[System\.ArgumentNullException](https://learn.microsoft.com/en-us/dotnet/api/system.argumentnullexception 'System\.ArgumentNullException')  
[source](ExecuteUpdateExtensions.ExecuteInsertGetRowAsync.N5L33IBE12KL0PYEFJPPG95GB#DotDoc.EntityFrameworkCore.Extensions.ExecuteUpdate.ExecuteUpdateExtensions.ExecuteInsertGetRowAsync_TSource_(thisSystem.Linq.IQueryable_TSource_,System.Action_Microsoft.EntityFrameworkCore.Query.UpdateSettersBuilder_TSource__,System.Threading.CancellationToken).source 'DotDoc\.EntityFrameworkCore\.Extensions\.ExecuteUpdate\.ExecuteUpdateExtensions\.ExecuteInsertGetRowAsync\<TSource\>\(this System\.Linq\.IQueryable\<TSource\>, System\.Action\<Microsoft\.EntityFrameworkCore\.Query\.UpdateSettersBuilder\<TSource\>\>, System\.Threading\.CancellationToken\)\.source') or [setPropertyCalls](ExecuteUpdateExtensions.ExecuteInsertGetRowAsync.N5L33IBE12KL0PYEFJPPG95GB#DotDoc.EntityFrameworkCore.Extensions.ExecuteUpdate.ExecuteUpdateExtensions.ExecuteInsertGetRowAsync_TSource_(thisSystem.Linq.IQueryable_TSource_,System.Action_Microsoft.EntityFrameworkCore.Query.UpdateSettersBuilder_TSource__,System.Threading.CancellationToken).setPropertyCalls 'DotDoc\.EntityFrameworkCore\.Extensions\.ExecuteUpdate\.ExecuteUpdateExtensions\.ExecuteInsertGetRowAsync\<TSource\>\(this System\.Linq\.IQueryable\<TSource\>, System\.Action\<Microsoft\.EntityFrameworkCore\.Query\.UpdateSettersBuilder\<TSource\>\>, System\.Threading\.CancellationToken\)\.setPropertyCalls') is [null](https://docs.microsoft.com/en-us/dotnet/csharp/language-reference/keywords/null 'https://docs\.microsoft\.com/en\-us/dotnet/csharp/language\-reference/keywords/null')\.

[System\.ArgumentException](https://learn.microsoft.com/en-us/dotnet/api/system.argumentexception 'System\.ArgumentException')  
[source](ExecuteUpdateExtensions.ExecuteInsertGetRowAsync.N5L33IBE12KL0PYEFJPPG95GB#DotDoc.EntityFrameworkCore.Extensions.ExecuteUpdate.ExecuteUpdateExtensions.ExecuteInsertGetRowAsync_TSource_(thisSystem.Linq.IQueryable_TSource_,System.Action_Microsoft.EntityFrameworkCore.Query.UpdateSettersBuilder_TSource__,System.Threading.CancellationToken).source 'DotDoc\.EntityFrameworkCore\.Extensions\.ExecuteUpdate\.ExecuteUpdateExtensions\.ExecuteInsertGetRowAsync\<TSource\>\(this System\.Linq\.IQueryable\<TSource\>, System\.Action\<Microsoft\.EntityFrameworkCore\.Query\.UpdateSettersBuilder\<TSource\>\>, System\.Threading\.CancellationToken\)\.source') is not an EF Core query\.

[System\.InvalidOperationException](https://learn.microsoft.com/en-us/dotnet/api/system.invalidoperationexception 'System\.InvalidOperationException')  
[UseExecuteUpdateExtensions\(this DbContextOptionsBuilder\)](ExecuteUpdateExtensions.UseExecuteUpdateExtensions.8EVJGRS4YF4T3EF3YO0SQPB69 'DotDoc\.EntityFrameworkCore\.Extensions\.ExecuteUpdate\.ExecuteUpdateExtensions\.UseExecuteUpdateExtensions\(this Microsoft\.EntityFrameworkCore\.DbContextOptionsBuilder\)') was not called when configuring the DbContext, another extension library has replaced its query generator, another ExecuteUpdate call is in progress on the DbContext, or the query is more than the table \(e\.g\. it has a `Where` or `OrderBy`\)\.

[System\.NotSupportedException](https://learn.microsoft.com/en-us/dotnet/api/system.notsupportedexception 'System\.NotSupportedException')  
The table is configured with `UseSqlOutputClause(false)` \(SQL Server\) or `UseSqlReturningClause(false)` \(SQLite\), or the entity type, or a type derived from it, has an owned type \(`OwnsOne` / `OwnsMany`\)\.