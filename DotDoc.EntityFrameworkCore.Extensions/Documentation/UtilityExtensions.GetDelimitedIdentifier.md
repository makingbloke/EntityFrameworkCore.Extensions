#### [DotDoc\.EntityFrameworkCore\.Extensions](Home 'Home')
### [DotDoc\.EntityFrameworkCore\.Extensions\.Utilities](DotDoc.EntityFrameworkCore.Extensions.Utilities 'DotDoc\.EntityFrameworkCore\.Extensions\.Utilities').[UtilityExtensions](UtilityExtensions 'DotDoc\.EntityFrameworkCore\.Extensions\.Utilities\.UtilityExtensions')

## UtilityExtensions\.GetDelimitedIdentifier Method

| Overloads | |
| :--- | :--- |
| [GetDelimitedIdentifier\(this DatabaseFacade, string\)](UtilityExtensions.GetDelimitedIdentifier#DotDoc.EntityFrameworkCore.Extensions.Utilities.UtilityExtensions.GetDelimitedIdentifier(thisMicrosoft.EntityFrameworkCore.Infrastructure.DatabaseFacade,string) 'DotDoc\.EntityFrameworkCore\.Extensions\.Utilities\.UtilityExtensions\.GetDelimitedIdentifier\(this Microsoft\.EntityFrameworkCore\.Infrastructure\.DatabaseFacade, string\)') | Generates the delimited SQL representation of an identifier \(column name, table name, etc\.\)\. |
| [GetDelimitedIdentifier\(this DatabaseFacade, string, string\)](UtilityExtensions.GetDelimitedIdentifier#DotDoc.EntityFrameworkCore.Extensions.Utilities.UtilityExtensions.GetDelimitedIdentifier(thisMicrosoft.EntityFrameworkCore.Infrastructure.DatabaseFacade,string,string) 'DotDoc\.EntityFrameworkCore\.Extensions\.Utilities\.UtilityExtensions\.GetDelimitedIdentifier\(this Microsoft\.EntityFrameworkCore\.Infrastructure\.DatabaseFacade, string, string\)') | Generates the delimited SQL representation of an identifier \(column name, table name, etc\.\)\. |
| [GetDelimitedIdentifier\(this DatabaseFacade, StringBuilder, string\)](UtilityExtensions.GetDelimitedIdentifier#DotDoc.EntityFrameworkCore.Extensions.Utilities.UtilityExtensions.GetDelimitedIdentifier(thisMicrosoft.EntityFrameworkCore.Infrastructure.DatabaseFacade,System.Text.StringBuilder,string) 'DotDoc\.EntityFrameworkCore\.Extensions\.Utilities\.UtilityExtensions\.GetDelimitedIdentifier\(this Microsoft\.EntityFrameworkCore\.Infrastructure\.DatabaseFacade, System\.Text\.StringBuilder, string\)') | Appends the delimited SQL representation of an identifier \(column name, table name, etc\.\)\. |
| [GetDelimitedIdentifier\(this DatabaseFacade, StringBuilder, string, string\)](UtilityExtensions.GetDelimitedIdentifier#DotDoc.EntityFrameworkCore.Extensions.Utilities.UtilityExtensions.GetDelimitedIdentifier(thisMicrosoft.EntityFrameworkCore.Infrastructure.DatabaseFacade,System.Text.StringBuilder,string,string) 'DotDoc\.EntityFrameworkCore\.Extensions\.Utilities\.UtilityExtensions\.GetDelimitedIdentifier\(this Microsoft\.EntityFrameworkCore\.Infrastructure\.DatabaseFacade, System\.Text\.StringBuilder, string, string\)') | Appends the delimited SQL representation of an identifier \(column name, table name, etc\.\)\. |

<a name='DotDoc.EntityFrameworkCore.Extensions.Utilities.UtilityExtensions.GetDelimitedIdentifier(thisMicrosoft.EntityFrameworkCore.Infrastructure.DatabaseFacade,string)'></a>

## UtilityExtensions\.GetDelimitedIdentifier\(this DatabaseFacade, string\) Method

Generates the delimited SQL representation of an identifier \(column name, table name, etc\.\)\.

```csharp
public static string GetDelimitedIdentifier(this Microsoft.EntityFrameworkCore.Infrastructure.DatabaseFacade database, string name);
```
#### Parameters

<a name='DotDoc.EntityFrameworkCore.Extensions.Utilities.UtilityExtensions.GetDelimitedIdentifier(thisMicrosoft.EntityFrameworkCore.Infrastructure.DatabaseFacade,string).database'></a>

`database` [Microsoft\.EntityFrameworkCore\.Infrastructure\.DatabaseFacade](https://learn.microsoft.com/en-us/dotnet/api/microsoft.entityframeworkcore.infrastructure.databasefacade 'Microsoft\.EntityFrameworkCore\.Infrastructure\.DatabaseFacade')

The database facade\.

<a name='DotDoc.EntityFrameworkCore.Extensions.Utilities.UtilityExtensions.GetDelimitedIdentifier(thisMicrosoft.EntityFrameworkCore.Infrastructure.DatabaseFacade,string).name'></a>

`name` [System\.String](https://learn.microsoft.com/en-us/dotnet/api/system.string 'System\.String')

The identifier to delimit\.

#### Returns
[System\.String](https://learn.microsoft.com/en-us/dotnet/api/system.string 'System\.String')  
The generated string\.

<a name='DotDoc.EntityFrameworkCore.Extensions.Utilities.UtilityExtensions.GetDelimitedIdentifier(thisMicrosoft.EntityFrameworkCore.Infrastructure.DatabaseFacade,string,string)'></a>

## UtilityExtensions\.GetDelimitedIdentifier\(this DatabaseFacade, string, string\) Method

Generates the delimited SQL representation of an identifier \(column name, table name, etc\.\)\.

```csharp
public static string GetDelimitedIdentifier(this Microsoft.EntityFrameworkCore.Infrastructure.DatabaseFacade database, string name, string? schema);
```
#### Parameters

<a name='DotDoc.EntityFrameworkCore.Extensions.Utilities.UtilityExtensions.GetDelimitedIdentifier(thisMicrosoft.EntityFrameworkCore.Infrastructure.DatabaseFacade,string,string).database'></a>

`database` [Microsoft\.EntityFrameworkCore\.Infrastructure\.DatabaseFacade](https://learn.microsoft.com/en-us/dotnet/api/microsoft.entityframeworkcore.infrastructure.databasefacade 'Microsoft\.EntityFrameworkCore\.Infrastructure\.DatabaseFacade')

The database facade\.

<a name='DotDoc.EntityFrameworkCore.Extensions.Utilities.UtilityExtensions.GetDelimitedIdentifier(thisMicrosoft.EntityFrameworkCore.Infrastructure.DatabaseFacade,string,string).name'></a>

`name` [System\.String](https://learn.microsoft.com/en-us/dotnet/api/system.string 'System\.String')

The identifier to delimit\.

<a name='DotDoc.EntityFrameworkCore.Extensions.Utilities.UtilityExtensions.GetDelimitedIdentifier(thisMicrosoft.EntityFrameworkCore.Infrastructure.DatabaseFacade,string,string).schema'></a>

`schema` [System\.String](https://learn.microsoft.com/en-us/dotnet/api/system.string 'System\.String')

The schema of the identifier\.

#### Returns
[System\.String](https://learn.microsoft.com/en-us/dotnet/api/system.string 'System\.String')  
The generated string\.

<a name='DotDoc.EntityFrameworkCore.Extensions.Utilities.UtilityExtensions.GetDelimitedIdentifier(thisMicrosoft.EntityFrameworkCore.Infrastructure.DatabaseFacade,System.Text.StringBuilder,string)'></a>

## UtilityExtensions\.GetDelimitedIdentifier\(this DatabaseFacade, StringBuilder, string\) Method

Appends the delimited SQL representation of an identifier \(column name, table name, etc\.\)\.

```csharp
public static void GetDelimitedIdentifier(this Microsoft.EntityFrameworkCore.Infrastructure.DatabaseFacade database, System.Text.StringBuilder builder, string name);
```
#### Parameters

<a name='DotDoc.EntityFrameworkCore.Extensions.Utilities.UtilityExtensions.GetDelimitedIdentifier(thisMicrosoft.EntityFrameworkCore.Infrastructure.DatabaseFacade,System.Text.StringBuilder,string).database'></a>

`database` [Microsoft\.EntityFrameworkCore\.Infrastructure\.DatabaseFacade](https://learn.microsoft.com/en-us/dotnet/api/microsoft.entityframeworkcore.infrastructure.databasefacade 'Microsoft\.EntityFrameworkCore\.Infrastructure\.DatabaseFacade')

The database facade\.

<a name='DotDoc.EntityFrameworkCore.Extensions.Utilities.UtilityExtensions.GetDelimitedIdentifier(thisMicrosoft.EntityFrameworkCore.Infrastructure.DatabaseFacade,System.Text.StringBuilder,string).builder'></a>

`builder` [System\.Text\.StringBuilder](https://learn.microsoft.com/en-us/dotnet/api/system.text.stringbuilder 'System\.Text\.StringBuilder')

The [System\.Text\.StringBuilder](https://learn.microsoft.com/en-us/dotnet/api/system.text.stringbuilder 'System\.Text\.StringBuilder') to append the generated string to\.

<a name='DotDoc.EntityFrameworkCore.Extensions.Utilities.UtilityExtensions.GetDelimitedIdentifier(thisMicrosoft.EntityFrameworkCore.Infrastructure.DatabaseFacade,System.Text.StringBuilder,string).name'></a>

`name` [System\.String](https://learn.microsoft.com/en-us/dotnet/api/system.string 'System\.String')

The identifier to delimit\.

<a name='DotDoc.EntityFrameworkCore.Extensions.Utilities.UtilityExtensions.GetDelimitedIdentifier(thisMicrosoft.EntityFrameworkCore.Infrastructure.DatabaseFacade,System.Text.StringBuilder,string,string)'></a>

## UtilityExtensions\.GetDelimitedIdentifier\(this DatabaseFacade, StringBuilder, string, string\) Method

Appends the delimited SQL representation of an identifier \(column name, table name, etc\.\)\.

```csharp
public static void GetDelimitedIdentifier(this Microsoft.EntityFrameworkCore.Infrastructure.DatabaseFacade database, System.Text.StringBuilder builder, string name, string? schema);
```
#### Parameters

<a name='DotDoc.EntityFrameworkCore.Extensions.Utilities.UtilityExtensions.GetDelimitedIdentifier(thisMicrosoft.EntityFrameworkCore.Infrastructure.DatabaseFacade,System.Text.StringBuilder,string,string).database'></a>

`database` [Microsoft\.EntityFrameworkCore\.Infrastructure\.DatabaseFacade](https://learn.microsoft.com/en-us/dotnet/api/microsoft.entityframeworkcore.infrastructure.databasefacade 'Microsoft\.EntityFrameworkCore\.Infrastructure\.DatabaseFacade')

The database facade\.

<a name='DotDoc.EntityFrameworkCore.Extensions.Utilities.UtilityExtensions.GetDelimitedIdentifier(thisMicrosoft.EntityFrameworkCore.Infrastructure.DatabaseFacade,System.Text.StringBuilder,string,string).builder'></a>

`builder` [System\.Text\.StringBuilder](https://learn.microsoft.com/en-us/dotnet/api/system.text.stringbuilder 'System\.Text\.StringBuilder')

The [System\.Text\.StringBuilder](https://learn.microsoft.com/en-us/dotnet/api/system.text.stringbuilder 'System\.Text\.StringBuilder') to append the generated string to\.

<a name='DotDoc.EntityFrameworkCore.Extensions.Utilities.UtilityExtensions.GetDelimitedIdentifier(thisMicrosoft.EntityFrameworkCore.Infrastructure.DatabaseFacade,System.Text.StringBuilder,string,string).name'></a>

`name` [System\.String](https://learn.microsoft.com/en-us/dotnet/api/system.string 'System\.String')

The identifier to delimit\.

<a name='DotDoc.EntityFrameworkCore.Extensions.Utilities.UtilityExtensions.GetDelimitedIdentifier(thisMicrosoft.EntityFrameworkCore.Infrastructure.DatabaseFacade,System.Text.StringBuilder,string,string).schema'></a>

`schema` [System\.String](https://learn.microsoft.com/en-us/dotnet/api/system.string 'System\.String')

The schema of the identifier\.