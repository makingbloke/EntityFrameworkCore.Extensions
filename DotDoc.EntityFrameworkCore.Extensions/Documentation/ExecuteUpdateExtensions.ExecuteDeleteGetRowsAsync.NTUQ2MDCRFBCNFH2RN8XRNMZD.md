#### [DotDoc\.EntityFrameworkCore\.Extensions](Home 'Home')
### [DotDoc\.EntityFrameworkCore\.Extensions\.ExecuteUpdate](DotDoc.EntityFrameworkCore.Extensions.ExecuteUpdate 'DotDoc\.EntityFrameworkCore\.Extensions\.ExecuteUpdate').[ExecuteUpdateExtensions](ExecuteUpdateExtensions 'DotDoc\.EntityFrameworkCore\.Extensions\.ExecuteUpdate\.ExecuteUpdateExtensions')

## ExecuteUpdateExtensions\.ExecuteDeleteGetRowsAsync\<TSource\>\(this IQueryable\<TSource\>, CancellationToken\) Method

Deletes all database rows for the entity instances which match the LINQ query from the database\.

```csharp
public static System.Threading.Tasks.Task<System.Collections.Generic.IList<TSource>> ExecuteDeleteGetRowsAsync<TSource>(this System.Linq.IQueryable<TSource> source, System.Threading.CancellationToken cancellationToken=default(System.Threading.CancellationToken))
    where TSource : class;
```
#### Type parameters

<a name='DotDoc.EntityFrameworkCore.Extensions.ExecuteUpdate.ExecuteUpdateExtensions.ExecuteDeleteGetRowsAsync_TSource_(thisSystem.Linq.IQueryable_TSource_,System.Threading.CancellationToken).TSource'></a>

`TSource`

The type of the elements of [source](ExecuteUpdateExtensions.ExecuteDeleteGetRowsAsync.NTUQ2MDCRFBCNFH2RN8XRNMZD#DotDoc.EntityFrameworkCore.Extensions.ExecuteUpdate.ExecuteUpdateExtensions.ExecuteDeleteGetRowsAsync_TSource_(thisSystem.Linq.IQueryable_TSource_,System.Threading.CancellationToken).source 'DotDoc\.EntityFrameworkCore\.Extensions\.ExecuteUpdate\.ExecuteUpdateExtensions\.ExecuteDeleteGetRowsAsync\<TSource\>\(this System\.Linq\.IQueryable\<TSource\>, System\.Threading\.CancellationToken\)\.source')\.
#### Parameters

<a name='DotDoc.EntityFrameworkCore.Extensions.ExecuteUpdate.ExecuteUpdateExtensions.ExecuteDeleteGetRowsAsync_TSource_(thisSystem.Linq.IQueryable_TSource_,System.Threading.CancellationToken).source'></a>

`source` [System\.Linq\.IQueryable&lt;](https://learn.microsoft.com/en-us/dotnet/api/system.linq.iqueryable-1 'System\.Linq\.IQueryable\`1')[TSource](ExecuteUpdateExtensions.ExecuteDeleteGetRowsAsync.NTUQ2MDCRFBCNFH2RN8XRNMZD#DotDoc.EntityFrameworkCore.Extensions.ExecuteUpdate.ExecuteUpdateExtensions.ExecuteDeleteGetRowsAsync_TSource_(thisSystem.Linq.IQueryable_TSource_,System.Threading.CancellationToken).TSource 'DotDoc\.EntityFrameworkCore\.Extensions\.ExecuteUpdate\.ExecuteUpdateExtensions\.ExecuteDeleteGetRowsAsync\<TSource\>\(this System\.Linq\.IQueryable\<TSource\>, System\.Threading\.CancellationToken\)\.TSource')[&gt;](https://learn.microsoft.com/en-us/dotnet/api/system.linq.iqueryable-1 'System\.Linq\.IQueryable\`1')

An [System\.Linq\.IQueryable&lt;&gt;](https://learn.microsoft.com/en-us/dotnet/api/system.linq.iqueryable-1 'System\.Linq\.IQueryable\`1') whose elements to test for a condition\.

<a name='DotDoc.EntityFrameworkCore.Extensions.ExecuteUpdate.ExecuteUpdateExtensions.ExecuteDeleteGetRowsAsync_TSource_(thisSystem.Linq.IQueryable_TSource_,System.Threading.CancellationToken).cancellationToken'></a>

`cancellationToken` [System\.Threading\.CancellationToken](https://learn.microsoft.com/en-us/dotnet/api/system.threading.cancellationtoken 'System\.Threading\.CancellationToken')

A [System\.Threading\.CancellationToken](https://learn.microsoft.com/en-us/dotnet/api/system.threading.cancellationtoken 'System\.Threading\.CancellationToken') to observe while waiting for the task to complete\.

#### Returns
[System\.Threading\.Tasks\.Task&lt;](https://learn.microsoft.com/en-us/dotnet/api/system.threading.tasks.task-1 'System\.Threading\.Tasks\.Task\`1')[System\.Collections\.Generic\.IList&lt;](https://learn.microsoft.com/en-us/dotnet/api/system.collections.generic.ilist-1 'System\.Collections\.Generic\.IList\`1')[TSource](ExecuteUpdateExtensions.ExecuteDeleteGetRowsAsync.NTUQ2MDCRFBCNFH2RN8XRNMZD#DotDoc.EntityFrameworkCore.Extensions.ExecuteUpdate.ExecuteUpdateExtensions.ExecuteDeleteGetRowsAsync_TSource_(thisSystem.Linq.IQueryable_TSource_,System.Threading.CancellationToken).TSource 'DotDoc\.EntityFrameworkCore\.Extensions\.ExecuteUpdate\.ExecuteUpdateExtensions\.ExecuteDeleteGetRowsAsync\<TSource\>\(this System\.Linq\.IQueryable\<TSource\>, System\.Threading\.CancellationToken\)\.TSource')[&gt;](https://learn.microsoft.com/en-us/dotnet/api/system.collections.generic.ilist-1 'System\.Collections\.Generic\.IList\`1')[&gt;](https://learn.microsoft.com/en-us/dotnet/api/system.threading.tasks.task-1 'System\.Threading\.Tasks\.Task\`1')  
An [System\.Collections\.Generic\.IList&lt;&gt;](https://learn.microsoft.com/en-us/dotnet/api/system.collections.generic.ilist-1 'System\.Collections\.Generic\.IList\`1') containing the deleted rows\.

#### Exceptions

[System\.ArgumentNullException](https://learn.microsoft.com/en-us/dotnet/api/system.argumentnullexception 'System\.ArgumentNullException')  
[source](ExecuteUpdateExtensions.ExecuteDeleteGetRowsAsync.NTUQ2MDCRFBCNFH2RN8XRNMZD#DotDoc.EntityFrameworkCore.Extensions.ExecuteUpdate.ExecuteUpdateExtensions.ExecuteDeleteGetRowsAsync_TSource_(thisSystem.Linq.IQueryable_TSource_,System.Threading.CancellationToken).source 'DotDoc\.EntityFrameworkCore\.Extensions\.ExecuteUpdate\.ExecuteUpdateExtensions\.ExecuteDeleteGetRowsAsync\<TSource\>\(this System\.Linq\.IQueryable\<TSource\>, System\.Threading\.CancellationToken\)\.source') is [null](https://docs.microsoft.com/en-us/dotnet/csharp/language-reference/keywords/null 'https://docs\.microsoft\.com/en\-us/dotnet/csharp/language\-reference/keywords/null')\.

[System\.ArgumentException](https://learn.microsoft.com/en-us/dotnet/api/system.argumentexception 'System\.ArgumentException')  
[source](ExecuteUpdateExtensions.ExecuteDeleteGetRowsAsync.NTUQ2MDCRFBCNFH2RN8XRNMZD#DotDoc.EntityFrameworkCore.Extensions.ExecuteUpdate.ExecuteUpdateExtensions.ExecuteDeleteGetRowsAsync_TSource_(thisSystem.Linq.IQueryable_TSource_,System.Threading.CancellationToken).source 'DotDoc\.EntityFrameworkCore\.Extensions\.ExecuteUpdate\.ExecuteUpdateExtensions\.ExecuteDeleteGetRowsAsync\<TSource\>\(this System\.Linq\.IQueryable\<TSource\>, System\.Threading\.CancellationToken\)\.source') is not an EF Core query\.

[System\.InvalidOperationException](https://learn.microsoft.com/en-us/dotnet/api/system.invalidoperationexception 'System\.InvalidOperationException')  
[UseExecuteUpdateExtensions\(this DbContextOptionsBuilder\)](ExecuteUpdateExtensions.UseExecuteUpdateExtensions.8EVJGRS4YF4T3EF3YO0SQPB69 'DotDoc\.EntityFrameworkCore\.Extensions\.ExecuteUpdate\.ExecuteUpdateExtensions\.UseExecuteUpdateExtensions\(this Microsoft\.EntityFrameworkCore\.DbContextOptionsBuilder\)') was not called when configuring the DbContext, another ExecuteUpdate call is in progress on the DbContext, or the query uses an operator that can't be used in a delete\.

[System\.NotSupportedException](https://learn.microsoft.com/en-us/dotnet/api/system.notsupportedexception 'System\.NotSupportedException')  
The table is configured with `UseSqlOutputClause(false)` \(SQL Server\) or `UseSqlReturningClause(false)` \(SQLite\)\.