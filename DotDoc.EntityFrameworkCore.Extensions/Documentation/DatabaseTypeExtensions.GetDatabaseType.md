#### [DotDoc\.EntityFrameworkCore\.Extensions](index.md 'index')
### [DotDoc\.EntityFrameworkCore\.Extensions\.DatabaseType](DotDoc.EntityFrameworkCore.Extensions.DatabaseType.md 'DotDoc\.EntityFrameworkCore\.Extensions\.DatabaseType').[DatabaseTypeExtensions](DatabaseTypeExtensions.md 'DotDoc\.EntityFrameworkCore\.Extensions\.DatabaseType\.DatabaseTypeExtensions')

## DatabaseTypeExtensions\.GetDatabaseType Method

| Overloads | |
| :--- | :--- |
| [GetDatabaseType\(this DbContextOptionsBuilder\)](DatabaseTypeExtensions.GetDatabaseType.md#DotDoc.EntityFrameworkCore.Extensions.DatabaseType.DatabaseTypeExtensions.GetDatabaseType(thisMicrosoft.EntityFrameworkCore.DbContextOptionsBuilder) 'DotDoc\.EntityFrameworkCore\.Extensions\.DatabaseType\.DatabaseTypeExtensions\.GetDatabaseType\(this Microsoft\.EntityFrameworkCore\.DbContextOptionsBuilder\)') | Gets the type of database in use from a [Microsoft\.EntityFrameworkCore\.DbContextOptionsBuilder](https://learn.microsoft.com/en-us/dotnet/api/microsoft.entityframeworkcore.dbcontextoptionsbuilder 'Microsoft\.EntityFrameworkCore\.DbContextOptionsBuilder') object\. |
| [GetDatabaseType\(this DatabaseFacade\)](DatabaseTypeExtensions.GetDatabaseType.md#DotDoc.EntityFrameworkCore.Extensions.DatabaseType.DatabaseTypeExtensions.GetDatabaseType(thisMicrosoft.EntityFrameworkCore.Infrastructure.DatabaseFacade) 'DotDoc\.EntityFrameworkCore\.Extensions\.DatabaseType\.DatabaseTypeExtensions\.GetDatabaseType\(this Microsoft\.EntityFrameworkCore\.Infrastructure\.DatabaseFacade\)') | Gets the type of database in use from a [Microsoft\.EntityFrameworkCore\.Infrastructure\.DatabaseFacade](https://learn.microsoft.com/en-us/dotnet/api/microsoft.entityframeworkcore.infrastructure.databasefacade 'Microsoft\.EntityFrameworkCore\.Infrastructure\.DatabaseFacade') object\. |
| [GetDatabaseType\(this MigrationBuilder\)](DatabaseTypeExtensions.GetDatabaseType.md#DotDoc.EntityFrameworkCore.Extensions.DatabaseType.DatabaseTypeExtensions.GetDatabaseType(thisMicrosoft.EntityFrameworkCore.Migrations.MigrationBuilder) 'DotDoc\.EntityFrameworkCore\.Extensions\.DatabaseType\.DatabaseTypeExtensions\.GetDatabaseType\(this Microsoft\.EntityFrameworkCore\.Migrations\.MigrationBuilder\)') | Gets the type of database in use from a [Microsoft\.EntityFrameworkCore\.Migrations\.MigrationBuilder](https://learn.microsoft.com/en-us/dotnet/api/microsoft.entityframeworkcore.migrations.migrationbuilder 'Microsoft\.EntityFrameworkCore\.Migrations\.MigrationBuilder') object\. |

<a name='DotDoc.EntityFrameworkCore.Extensions.DatabaseType.DatabaseTypeExtensions.GetDatabaseType(thisMicrosoft.EntityFrameworkCore.DbContextOptionsBuilder)'></a>

## DatabaseTypeExtensions\.GetDatabaseType\(this DbContextOptionsBuilder\) Method

Gets the type of database in use from a [Microsoft\.EntityFrameworkCore\.DbContextOptionsBuilder](https://learn.microsoft.com/en-us/dotnet/api/microsoft.entityframeworkcore.dbcontextoptionsbuilder 'Microsoft\.EntityFrameworkCore\.DbContextOptionsBuilder') object\.

```csharp
public static string? GetDatabaseType(this Microsoft.EntityFrameworkCore.DbContextOptionsBuilder optionsBuilder);
```
#### Parameters

<a name='DotDoc.EntityFrameworkCore.Extensions.DatabaseType.DatabaseTypeExtensions.GetDatabaseType(thisMicrosoft.EntityFrameworkCore.DbContextOptionsBuilder).optionsBuilder'></a>

`optionsBuilder` [Microsoft\.EntityFrameworkCore\.DbContextOptionsBuilder](https://learn.microsoft.com/en-us/dotnet/api/microsoft.entityframeworkcore.dbcontextoptionsbuilder 'Microsoft\.EntityFrameworkCore\.DbContextOptionsBuilder')

A builder used to create or modify options for this context\.

#### Returns
[System\.String](https://learn.microsoft.com/en-us/dotnet/api/system.string 'System\.String')  
[System\.String](https://learn.microsoft.com/en-us/dotnet/api/system.string 'System\.String') with the database type or [null](https://docs.microsoft.com/en-us/dotnet/csharp/language-reference/keywords/null 'https://docs\.microsoft\.com/en\-us/dotnet/csharp/language\-reference/keywords/null') if none recognised\.

<a name='DotDoc.EntityFrameworkCore.Extensions.DatabaseType.DatabaseTypeExtensions.GetDatabaseType(thisMicrosoft.EntityFrameworkCore.Infrastructure.DatabaseFacade)'></a>

## DatabaseTypeExtensions\.GetDatabaseType\(this DatabaseFacade\) Method

Gets the type of database in use from a [Microsoft\.EntityFrameworkCore\.Infrastructure\.DatabaseFacade](https://learn.microsoft.com/en-us/dotnet/api/microsoft.entityframeworkcore.infrastructure.databasefacade 'Microsoft\.EntityFrameworkCore\.Infrastructure\.DatabaseFacade') object\.

```csharp
public static string? GetDatabaseType(this Microsoft.EntityFrameworkCore.Infrastructure.DatabaseFacade database);
```
#### Parameters

<a name='DotDoc.EntityFrameworkCore.Extensions.DatabaseType.DatabaseTypeExtensions.GetDatabaseType(thisMicrosoft.EntityFrameworkCore.Infrastructure.DatabaseFacade).database'></a>

`database` [Microsoft\.EntityFrameworkCore\.Infrastructure\.DatabaseFacade](https://learn.microsoft.com/en-us/dotnet/api/microsoft.entityframeworkcore.infrastructure.databasefacade 'Microsoft\.EntityFrameworkCore\.Infrastructure\.DatabaseFacade')

The [Microsoft\.EntityFrameworkCore\.Infrastructure\.DatabaseFacade](https://learn.microsoft.com/en-us/dotnet/api/microsoft.entityframeworkcore.infrastructure.databasefacade 'Microsoft\.EntityFrameworkCore\.Infrastructure\.DatabaseFacade') for the context\.

#### Returns
[System\.String](https://learn.microsoft.com/en-us/dotnet/api/system.string 'System\.String')  
[System\.String](https://learn.microsoft.com/en-us/dotnet/api/system.string 'System\.String') with the database type or [null](https://docs.microsoft.com/en-us/dotnet/csharp/language-reference/keywords/null 'https://docs\.microsoft\.com/en\-us/dotnet/csharp/language\-reference/keywords/null') if none recognised\.

<a name='DotDoc.EntityFrameworkCore.Extensions.DatabaseType.DatabaseTypeExtensions.GetDatabaseType(thisMicrosoft.EntityFrameworkCore.Migrations.MigrationBuilder)'></a>

## DatabaseTypeExtensions\.GetDatabaseType\(this MigrationBuilder\) Method

Gets the type of database in use from a [Microsoft\.EntityFrameworkCore\.Migrations\.MigrationBuilder](https://learn.microsoft.com/en-us/dotnet/api/microsoft.entityframeworkcore.migrations.migrationbuilder 'Microsoft\.EntityFrameworkCore\.Migrations\.MigrationBuilder') object\.

```csharp
public static string? GetDatabaseType(this Microsoft.EntityFrameworkCore.Migrations.MigrationBuilder builder);
```
#### Parameters

<a name='DotDoc.EntityFrameworkCore.Extensions.DatabaseType.DatabaseTypeExtensions.GetDatabaseType(thisMicrosoft.EntityFrameworkCore.Migrations.MigrationBuilder).builder'></a>

`builder` [Microsoft\.EntityFrameworkCore\.Migrations\.MigrationBuilder](https://learn.microsoft.com/en-us/dotnet/api/microsoft.entityframeworkcore.migrations.migrationbuilder 'Microsoft\.EntityFrameworkCore\.Migrations\.MigrationBuilder')

The migration builder\.

#### Returns
[System\.String](https://learn.microsoft.com/en-us/dotnet/api/system.string 'System\.String')  
[System\.String](https://learn.microsoft.com/en-us/dotnet/api/system.string 'System\.String') with the database type or [null](https://docs.microsoft.com/en-us/dotnet/csharp/language-reference/keywords/null 'https://docs\.microsoft\.com/en\-us/dotnet/csharp/language\-reference/keywords/null') if none recognised\.