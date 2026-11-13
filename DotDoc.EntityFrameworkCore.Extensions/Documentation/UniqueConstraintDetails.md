#### [DotDoc\.EntityFrameworkCore\.Extensions](Home 'Home')
### [DotDoc\.EntityFrameworkCore\.Extensions\.UniqueConstraint](DotDoc.EntityFrameworkCore.Extensions.UniqueConstraint 'DotDoc\.EntityFrameworkCore\.Extensions\.UniqueConstraint')

## UniqueConstraintDetails Class

Unique constraint details\.

```csharp
public sealed class UniqueConstraintDetails
```

Inheritance [System\.Object](https://learn.microsoft.com/en-us/dotnet/api/system.object 'System\.Object') → UniqueConstraintDetails

| Properties | |
| :--- | :--- |
| [FieldNames](UniqueConstraintDetails.FieldNames 'DotDoc\.EntityFrameworkCore\.Extensions\.UniqueConstraint\.UniqueConstraintDetails\.FieldNames') | Gets the constraint field names\. The EF Core property names if the table is in the EF Core model, otherwise the database column names\. |
| [Schema](UniqueConstraintDetails.Schema 'DotDoc\.EntityFrameworkCore\.Extensions\.UniqueConstraint\.UniqueConstraintDetails\.Schema') | Gets the constraint schema\. [null](https://docs.microsoft.com/en-us/dotnet/csharp/language-reference/keywords/null 'https://docs\.microsoft\.com/en\-us/dotnet/csharp/language\-reference/keywords/null') for the default schema \(`dbo` on SQL Server\)\. |
| [TableName](UniqueConstraintDetails.TableName 'DotDoc\.EntityFrameworkCore\.Extensions\.UniqueConstraint\.UniqueConstraintDetails\.TableName') | Gets the constraint table name\. EF Core entity name if the table is in the EF Core model, otherwise the database table name\. |
