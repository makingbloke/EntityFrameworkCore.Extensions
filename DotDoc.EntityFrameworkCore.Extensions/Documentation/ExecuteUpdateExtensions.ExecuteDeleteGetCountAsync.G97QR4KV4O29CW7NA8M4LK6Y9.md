#### [DotDoc\.EntityFrameworkCore\.Extensions](Home 'Home')
### [DotDoc\.EntityFrameworkCore\.Extensions\.ExecuteUpdate](DotDoc.EntityFrameworkCore.Extensions.ExecuteUpdate 'DotDoc\.EntityFrameworkCore\.Extensions\.ExecuteUpdate').[ExecuteUpdateExtensions](ExecuteUpdateExtensions 'DotDoc\.EntityFrameworkCore\.Extensions\.ExecuteUpdate\.ExecuteUpdateExtensions')

## ExecuteUpdateExtensions\.ExecuteDeleteGetCountAsync\<TSource\>\(this IQueryable\<TSource\>, CancellationToken\) Method

Deletes all database rows for the entity instances which match the LINQ query from the database\.

```csharp
public static System.Threading.Tasks.Task<int> ExecuteDeleteGetCountAsync<TSource>(this System.Linq.IQueryable<TSource> source, System.Threading.CancellationToken cancellationToken=default(System.Threading.CancellationToken))
    where TSource : class;
```
#### Type parameters

<a name='DotDoc.EntityFrameworkCore.Extensions.ExecuteUpdate.ExecuteUpdateExtensions.ExecuteDeleteGetCountAsync_TSource_(thisSystem.Linq.IQueryable_TSource_,System.Threading.CancellationToken).TSource'></a>

`TSource`

The type of the elements of [source](ExecuteUpdateExtensions.ExecuteDeleteGetCountAsync.G97QR4KV4O29CW7NA8M4LK6Y9#DotDoc.EntityFrameworkCore.Extensions.ExecuteUpdate.ExecuteUpdateExtensions.ExecuteDeleteGetCountAsync_TSource_(thisSystem.Linq.IQueryable_TSource_,System.Threading.CancellationToken).source 'DotDoc\.EntityFrameworkCore\.Extensions\.ExecuteUpdate\.ExecuteUpdateExtensions\.ExecuteDeleteGetCountAsync\<TSource\>\(this System\.Linq\.IQueryable\<TSource\>, System\.Threading\.CancellationToken\)\.source')\.
#### Parameters

<a name='DotDoc.EntityFrameworkCore.Extensions.ExecuteUpdate.ExecuteUpdateExtensions.ExecuteDeleteGetCountAsync_TSource_(thisSystem.Linq.IQueryable_TSource_,System.Threading.CancellationToken).source'></a>

`source` [System\.Linq\.IQueryable&lt;](https://learn.microsoft.com/en-us/dotnet/api/system.linq.iqueryable-1 'System\.Linq\.IQueryable\`1')[TSource](ExecuteUpdateExtensions.ExecuteDeleteGetCountAsync.G97QR4KV4O29CW7NA8M4LK6Y9#DotDoc.EntityFrameworkCore.Extensions.ExecuteUpdate.ExecuteUpdateExtensions.ExecuteDeleteGetCountAsync_TSource_(thisSystem.Linq.IQueryable_TSource_,System.Threading.CancellationToken).TSource 'DotDoc\.EntityFrameworkCore\.Extensions\.ExecuteUpdate\.ExecuteUpdateExtensions\.ExecuteDeleteGetCountAsync\<TSource\>\(this System\.Linq\.IQueryable\<TSource\>, System\.Threading\.CancellationToken\)\.TSource')[&gt;](https://learn.microsoft.com/en-us/dotnet/api/system.linq.iqueryable-1 'System\.Linq\.IQueryable\`1')

An [System\.Linq\.IQueryable&lt;&gt;](https://learn.microsoft.com/en-us/dotnet/api/system.linq.iqueryable-1 'System\.Linq\.IQueryable\`1') whose elements to test for a condition\.

<a name='DotDoc.EntityFrameworkCore.Extensions.ExecuteUpdate.ExecuteUpdateExtensions.ExecuteDeleteGetCountAsync_TSource_(thisSystem.Linq.IQueryable_TSource_,System.Threading.CancellationToken).cancellationToken'></a>

`cancellationToken` [System\.Threading\.CancellationToken](https://learn.microsoft.com/en-us/dotnet/api/system.threading.cancellationtoken 'System\.Threading\.CancellationToken')

A [System\.Threading\.CancellationToken](https://learn.microsoft.com/en-us/dotnet/api/system.threading.cancellationtoken 'System\.Threading\.CancellationToken') to observe while waiting for the task to complete\.

#### Returns
[System\.Threading\.Tasks\.Task&lt;](https://learn.microsoft.com/en-us/dotnet/api/system.threading.tasks.task-1 'System\.Threading\.Tasks\.Task\`1')[System\.Int32](https://learn.microsoft.com/en-us/dotnet/api/system.int32 'System\.Int32')[&gt;](https://learn.microsoft.com/en-us/dotnet/api/system.threading.tasks.task-1 'System\.Threading\.Tasks\.Task\`1')  
The number of rows deleted in the database\.