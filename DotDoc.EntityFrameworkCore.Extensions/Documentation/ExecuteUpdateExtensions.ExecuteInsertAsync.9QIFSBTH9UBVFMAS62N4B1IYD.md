#### [DotDoc\.EntityFrameworkCore\.Extensions](Home 'Home')
### [DotDoc\.EntityFrameworkCore\.Extensions\.ExecuteUpdate](DotDoc.EntityFrameworkCore.Extensions.ExecuteUpdate 'DotDoc\.EntityFrameworkCore\.Extensions\.ExecuteUpdate').[ExecuteUpdateExtensions](ExecuteUpdateExtensions 'DotDoc\.EntityFrameworkCore\.Extensions\.ExecuteUpdate\.ExecuteUpdateExtensions')

## ExecuteUpdateExtensions\.ExecuteInsertAsync\<TSource\>\(this IQueryable\<TSource\>, Action\<UpdateSettersBuilder\<TSource\>\>, CancellationToken\) Method

Insert a database row\.

```csharp
public static System.Threading.Tasks.Task ExecuteInsertAsync<TSource>(this System.Linq.IQueryable<TSource> source, System.Action<Microsoft.EntityFrameworkCore.Query.UpdateSettersBuilder<TSource>> setPropertyCalls, System.Threading.CancellationToken cancellationToken=default(System.Threading.CancellationToken))
    where TSource : class;
```
#### Type parameters

<a name='DotDoc.EntityFrameworkCore.Extensions.ExecuteUpdate.ExecuteUpdateExtensions.ExecuteInsertAsync_TSource_(thisSystem.Linq.IQueryable_TSource_,System.Action_Microsoft.EntityFrameworkCore.Query.UpdateSettersBuilder_TSource__,System.Threading.CancellationToken).TSource'></a>

`TSource`

The type of the elements of [source](ExecuteUpdateExtensions.ExecuteInsertAsync.9QIFSBTH9UBVFMAS62N4B1IYD#DotDoc.EntityFrameworkCore.Extensions.ExecuteUpdate.ExecuteUpdateExtensions.ExecuteInsertAsync_TSource_(thisSystem.Linq.IQueryable_TSource_,System.Action_Microsoft.EntityFrameworkCore.Query.UpdateSettersBuilder_TSource__,System.Threading.CancellationToken).source 'DotDoc\.EntityFrameworkCore\.Extensions\.ExecuteUpdate\.ExecuteUpdateExtensions\.ExecuteInsertAsync\<TSource\>\(this System\.Linq\.IQueryable\<TSource\>, System\.Action\<Microsoft\.EntityFrameworkCore\.Query\.UpdateSettersBuilder\<TSource\>\>, System\.Threading\.CancellationToken\)\.source')\.
#### Parameters

<a name='DotDoc.EntityFrameworkCore.Extensions.ExecuteUpdate.ExecuteUpdateExtensions.ExecuteInsertAsync_TSource_(thisSystem.Linq.IQueryable_TSource_,System.Action_Microsoft.EntityFrameworkCore.Query.UpdateSettersBuilder_TSource__,System.Threading.CancellationToken).source'></a>

`source` [System\.Linq\.IQueryable&lt;](https://learn.microsoft.com/en-us/dotnet/api/system.linq.iqueryable-1 'System\.Linq\.IQueryable\`1')[TSource](ExecuteUpdateExtensions.ExecuteInsertAsync.9QIFSBTH9UBVFMAS62N4B1IYD#DotDoc.EntityFrameworkCore.Extensions.ExecuteUpdate.ExecuteUpdateExtensions.ExecuteInsertAsync_TSource_(thisSystem.Linq.IQueryable_TSource_,System.Action_Microsoft.EntityFrameworkCore.Query.UpdateSettersBuilder_TSource__,System.Threading.CancellationToken).TSource 'DotDoc\.EntityFrameworkCore\.Extensions\.ExecuteUpdate\.ExecuteUpdateExtensions\.ExecuteInsertAsync\<TSource\>\(this System\.Linq\.IQueryable\<TSource\>, System\.Action\<Microsoft\.EntityFrameworkCore\.Query\.UpdateSettersBuilder\<TSource\>\>, System\.Threading\.CancellationToken\)\.TSource')[&gt;](https://learn.microsoft.com/en-us/dotnet/api/system.linq.iqueryable-1 'System\.Linq\.IQueryable\`1')

An [System\.Linq\.IQueryable&lt;&gt;](https://learn.microsoft.com/en-us/dotnet/api/system.linq.iqueryable-1 'System\.Linq\.IQueryable\`1') whose elements to test for a condition\.

<a name='DotDoc.EntityFrameworkCore.Extensions.ExecuteUpdate.ExecuteUpdateExtensions.ExecuteInsertAsync_TSource_(thisSystem.Linq.IQueryable_TSource_,System.Action_Microsoft.EntityFrameworkCore.Query.UpdateSettersBuilder_TSource__,System.Threading.CancellationToken).setPropertyCalls'></a>

`setPropertyCalls` [System\.Action&lt;](https://learn.microsoft.com/en-us/dotnet/api/system.action-1 'System\.Action\`1')[Microsoft\.EntityFrameworkCore\.Query\.UpdateSettersBuilder&lt;](https://learn.microsoft.com/en-us/dotnet/api/microsoft.entityframeworkcore.query.updatesettersbuilder-1 'Microsoft\.EntityFrameworkCore\.Query\.UpdateSettersBuilder\`1')[TSource](ExecuteUpdateExtensions.ExecuteInsertAsync.9QIFSBTH9UBVFMAS62N4B1IYD#DotDoc.EntityFrameworkCore.Extensions.ExecuteUpdate.ExecuteUpdateExtensions.ExecuteInsertAsync_TSource_(thisSystem.Linq.IQueryable_TSource_,System.Action_Microsoft.EntityFrameworkCore.Query.UpdateSettersBuilder_TSource__,System.Threading.CancellationToken).TSource 'DotDoc\.EntityFrameworkCore\.Extensions\.ExecuteUpdate\.ExecuteUpdateExtensions\.ExecuteInsertAsync\<TSource\>\(this System\.Linq\.IQueryable\<TSource\>, System\.Action\<Microsoft\.EntityFrameworkCore\.Query\.UpdateSettersBuilder\<TSource\>\>, System\.Threading\.CancellationToken\)\.TSource')[&gt;](https://learn.microsoft.com/en-us/dotnet/api/microsoft.entityframeworkcore.query.updatesettersbuilder-1 'Microsoft\.EntityFrameworkCore\.Query\.UpdateSettersBuilder\`1')[&gt;](https://learn.microsoft.com/en-us/dotnet/api/system.action-1 'System\.Action\`1')

A method containing set property statements specifying properties to update\.

<a name='DotDoc.EntityFrameworkCore.Extensions.ExecuteUpdate.ExecuteUpdateExtensions.ExecuteInsertAsync_TSource_(thisSystem.Linq.IQueryable_TSource_,System.Action_Microsoft.EntityFrameworkCore.Query.UpdateSettersBuilder_TSource__,System.Threading.CancellationToken).cancellationToken'></a>

`cancellationToken` [System\.Threading\.CancellationToken](https://learn.microsoft.com/en-us/dotnet/api/system.threading.cancellationtoken 'System\.Threading\.CancellationToken')

A [System\.Threading\.CancellationToken](https://learn.microsoft.com/en-us/dotnet/api/system.threading.cancellationtoken 'System\.Threading\.CancellationToken') to observe while waiting for the task to complete\.

#### Returns
[System\.Threading\.Tasks\.Task](https://learn.microsoft.com/en-us/dotnet/api/system.threading.tasks.task 'System\.Threading\.Tasks\.Task')  
A [System\.Threading\.Tasks\.Task](https://learn.microsoft.com/en-us/dotnet/api/system.threading.tasks.task 'System\.Threading\.Tasks\.Task') that represents the asynchronous operation\.