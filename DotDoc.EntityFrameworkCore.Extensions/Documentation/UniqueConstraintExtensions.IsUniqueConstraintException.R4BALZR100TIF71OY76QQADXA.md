#### [DotDoc\.EntityFrameworkCore\.Extensions](Home 'Home')
### [DotDoc\.EntityFrameworkCore\.Extensions\.UniqueConstraint](DotDoc.EntityFrameworkCore.Extensions.UniqueConstraint 'DotDoc\.EntityFrameworkCore\.Extensions\.UniqueConstraint').[UniqueConstraintExtensions](UniqueConstraintExtensions 'DotDoc\.EntityFrameworkCore\.Extensions\.UniqueConstraint\.UniqueConstraintExtensions')

## UniqueConstraintExtensions\.IsUniqueConstraintException\(this Exception\) Method

Checks if an exception has been raised as a result of a unique constraint violation\.

```csharp
public static bool IsUniqueConstraintException(this System.Exception ex);
```
#### Parameters

<a name='DotDoc.EntityFrameworkCore.Extensions.UniqueConstraint.UniqueConstraintExtensions.IsUniqueConstraintException(thisSystem.Exception).ex'></a>

`ex` [System\.Exception](https://learn.microsoft.com/en-us/dotnet/api/system.exception 'System\.Exception')

The exception to check\.

#### Returns
[System\.Boolean](https://learn.microsoft.com/en-us/dotnet/api/system.boolean 'System\.Boolean')  
[true](https://docs.microsoft.com/en-us/dotnet/csharp/language-reference/builtin-types/bool 'https://docs\.microsoft\.com/en\-us/dotnet/csharp/language\-reference/builtin\-types/bool') if the exception is a unique constraint violation else, [false](https://docs.microsoft.com/en-us/dotnet/csharp/language-reference/builtin-types/bool 'https://docs\.microsoft\.com/en\-us/dotnet/csharp/language\-reference/builtin\-types/bool')\.