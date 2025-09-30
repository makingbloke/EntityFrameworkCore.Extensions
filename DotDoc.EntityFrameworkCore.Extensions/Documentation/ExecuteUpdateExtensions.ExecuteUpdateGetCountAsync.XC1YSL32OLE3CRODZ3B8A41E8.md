#### [DotDoc\.EntityFrameworkCore\.Extensions](Home.md 'Home')
### [DotDoc\.EntityFrameworkCore\.Extensions\.ExecuteUpdate](DotDoc.EntityFrameworkCore.Extensions.ExecuteUpdate.md 'DotDoc\.EntityFrameworkCore\.Extensions\.ExecuteUpdate').[ExecuteUpdateExtensions](ExecuteUpdateExtensions.md 'DotDoc\.EntityFrameworkCore\.Extensions\.ExecuteUpdate\.ExecuteUpdateExtensions')

## ExecuteUpdateExtensions\.ExecuteUpdateGetCountAsync\<TSource\>\(this IQueryable\<TSource\>, Action\<UpdateSettersBuilder\<TSource\>\>, CancellationToken\) Method

Updates all database rows for the entity instances which match the LINQ query from the database\.

```csharp
public static System.Threading.Tasks.Task<int> ExecuteUpdateGetCountAsync<TSource>(this System.Linq.IQueryable<TSource> source, System.Action<Microsoft.EntityFrameworkCore.Query.UpdateSettersBuilder<TSource>> setPropertyCalls, System.Threading.CancellationToken cancellationToken=default(System.Threading.CancellationToken))
    where TSource : class;
```
#### Type parameters

<a name='DotDoc.EntityFrameworkCore.Extensions.ExecuteUpdate.ExecuteUpdateExtensions.ExecuteUpdateGetCountAsync_TSource_(thisSystem.Linq.IQueryable_TSource_,System.Action_Microsoft.EntityFrameworkCore.Query.UpdateSettersBuilder_TSource__,System.Threading.CancellationToken).TSource'></a>

`TSource`

The type of the elements of [source](ExecuteUpdateExtensions.ExecuteUpdateGetCountAsync.XC1YSL32OLE3CRODZ3B8A41E8.md#DotDoc.EntityFrameworkCore.Extensions.ExecuteUpdate.ExecuteUpdateExtensions.ExecuteUpdateGetCountAsync_TSource_(thisSystem.Linq.IQueryable_TSource_,System.Action_Microsoft.EntityFrameworkCore.Query.UpdateSettersBuilder_TSource__,System.Threading.CancellationToken).source 'DotDoc\.EntityFrameworkCore\.Extensions\.ExecuteUpdate\.ExecuteUpdateExtensions\.ExecuteUpdateGetCountAsync\<TSource\>\(this System\.Linq\.IQueryable\<TSource\>, System\.Action\<Microsoft\.EntityFrameworkCore\.Query\.UpdateSettersBuilder\<TSource\>\>, System\.Threading\.CancellationToken\)\.source')\.
#### Parameters

<a name='DotDoc.EntityFrameworkCore.Extensions.ExecuteUpdate.ExecuteUpdateExtensions.ExecuteUpdateGetCountAsync_TSource_(thisSystem.Linq.IQueryable_TSource_,System.Action_Microsoft.EntityFrameworkCore.Query.UpdateSettersBuilder_TSource__,System.Threading.CancellationToken).source'></a>

`source` [System\.Linq\.IQueryable&lt;](https://learn.microsoft.com/en-us/dotnet/api/system.linq.iqueryable-1 'System\.Linq\.IQueryable\`1')[TSource](ExecuteUpdateExtensions.ExecuteUpdateGetCountAsync.XC1YSL32OLE3CRODZ3B8A41E8.md#DotDoc.EntityFrameworkCore.Extensions.ExecuteUpdate.ExecuteUpdateExtensions.ExecuteUpdateGetCountAsync_TSource_(thisSystem.Linq.IQueryable_TSource_,System.Action_Microsoft.EntityFrameworkCore.Query.UpdateSettersBuilder_TSource__,System.Threading.CancellationToken).TSource 'DotDoc\.EntityFrameworkCore\.Extensions\.ExecuteUpdate\.ExecuteUpdateExtensions\.ExecuteUpdateGetCountAsync\<TSource\>\(this System\.Linq\.IQueryable\<TSource\>, System\.Action\<Microsoft\.EntityFrameworkCore\.Query\.UpdateSettersBuilder\<TSource\>\>, System\.Threading\.CancellationToken\)\.TSource')[&gt;](https://learn.microsoft.com/en-us/dotnet/api/system.linq.iqueryable-1 'System\.Linq\.IQueryable\`1')

An [System\.Linq\.IQueryable&lt;&gt;](https://learn.microsoft.com/en-us/dotnet/api/system.linq.iqueryable-1 'System\.Linq\.IQueryable\`1') whose elements to test for a condition\.

<a name='DotDoc.EntityFrameworkCore.Extensions.ExecuteUpdate.ExecuteUpdateExtensions.ExecuteUpdateGetCountAsync_TSource_(thisSystem.Linq.IQueryable_TSource_,System.Action_Microsoft.EntityFrameworkCore.Query.UpdateSettersBuilder_TSource__,System.Threading.CancellationToken).setPropertyCalls'></a>

`setPropertyCalls` [System\.Action&lt;](https://learn.microsoft.com/en-us/dotnet/api/system.action-1 'System\.Action\`1')[Microsoft\.EntityFrameworkCore\.Query\.UpdateSettersBuilder&lt;](https://learn.microsoft.com/en-us/dotnet/api/microsoft.entityframeworkcore.query.updatesettersbuilder-1 'Microsoft\.EntityFrameworkCore\.Query\.UpdateSettersBuilder\`1')[TSource](ExecuteUpdateExtensions.ExecuteUpdateGetCountAsync.XC1YSL32OLE3CRODZ3B8A41E8.md#DotDoc.EntityFrameworkCore.Extensions.ExecuteUpdate.ExecuteUpdateExtensions.ExecuteUpdateGetCountAsync_TSource_(thisSystem.Linq.IQueryable_TSource_,System.Action_Microsoft.EntityFrameworkCore.Query.UpdateSettersBuilder_TSource__,System.Threading.CancellationToken).TSource 'DotDoc\.EntityFrameworkCore\.Extensions\.ExecuteUpdate\.ExecuteUpdateExtensions\.ExecuteUpdateGetCountAsync\<TSource\>\(this System\.Linq\.IQueryable\<TSource\>, System\.Action\<Microsoft\.EntityFrameworkCore\.Query\.UpdateSettersBuilder\<TSource\>\>, System\.Threading\.CancellationToken\)\.TSource')[&gt;](https://learn.microsoft.com/en-us/dotnet/api/microsoft.entityframeworkcore.query.updatesettersbuilder-1 'Microsoft\.EntityFrameworkCore\.Query\.UpdateSettersBuilder\`1')[&gt;](https://learn.microsoft.com/en-us/dotnet/api/system.action-1 'System\.Action\`1')

A method containing set property statements specifying properties to update\.

<a name='DotDoc.EntityFrameworkCore.Extensions.ExecuteUpdate.ExecuteUpdateExtensions.ExecuteUpdateGetCountAsync_TSource_(thisSystem.Linq.IQueryable_TSource_,System.Action_Microsoft.EntityFrameworkCore.Query.UpdateSettersBuilder_TSource__,System.Threading.CancellationToken).cancellationToken'></a>

`cancellationToken` [System\.Threading\.CancellationToken](https://learn.microsoft.com/en-us/dotnet/api/system.threading.cancellationtoken 'System\.Threading\.CancellationToken')

A [System\.Threading\.CancellationToken](https://learn.microsoft.com/en-us/dotnet/api/system.threading.cancellationtoken 'System\.Threading\.CancellationToken') to observe while waiting for the task to complete\.

#### Returns
[System\.Threading\.Tasks\.Task&lt;](https://learn.microsoft.com/en-us/dotnet/api/system.threading.tasks.task-1 'System\.Threading\.Tasks\.Task\`1')[System\.Int32](https://learn.microsoft.com/en-us/dotnet/api/system.int32 'System\.Int32')[&gt;](https://learn.microsoft.com/en-us/dotnet/api/system.threading.tasks.task-1 'System\.Threading\.Tasks\.Task\`1')  
The number of rows updated in the database\.