#### [DotDoc\.EntityFrameworkCore\.Extensions](Home 'Home')
### [DotDoc\.EntityFrameworkCore\.Extensions\.Utilities](DotDoc.EntityFrameworkCore.Extensions.Utilities 'DotDoc\.EntityFrameworkCore\.Extensions\.Utilities').[DelimitedIdentifierExtensions](DelimitedIdentifierExtensions 'DotDoc\.EntityFrameworkCore\.Extensions\.Utilities\.DelimitedIdentifierExtensions')

## DelimitedIdentifierExtensions\.GetDelimitedIdentifier Method

| Overloads | |
| :--- | :--- |
| [GetDelimitedIdentifier\(this DatabaseFacade, string\)](DelimitedIdentifierExtensions.GetDelimitedIdentifier#DotDoc.EntityFrameworkCore.Extensions.Utilities.DelimitedIdentifierExtensions.GetDelimitedIdentifier(thisMicrosoft.EntityFrameworkCore.Infrastructure.DatabaseFacade,string) 'DotDoc\.EntityFrameworkCore\.Extensions\.Utilities\.DelimitedIdentifierExtensions\.GetDelimitedIdentifier\(this Microsoft\.EntityFrameworkCore\.Infrastructure\.DatabaseFacade, string\)') | Generates the delimited SQL representation of an identifier \(column name, table name, etc\.\)\. |
| [GetDelimitedIdentifier\(this DatabaseFacade, string, string\)](DelimitedIdentifierExtensions.GetDelimitedIdentifier#DotDoc.EntityFrameworkCore.Extensions.Utilities.DelimitedIdentifierExtensions.GetDelimitedIdentifier(thisMicrosoft.EntityFrameworkCore.Infrastructure.DatabaseFacade,string,string) 'DotDoc\.EntityFrameworkCore\.Extensions\.Utilities\.DelimitedIdentifierExtensions\.GetDelimitedIdentifier\(this Microsoft\.EntityFrameworkCore\.Infrastructure\.DatabaseFacade, string, string\)') | Generates the delimited SQL representation of an identifier \(column name, table name, etc\.\)\. |
| [GetDelimitedIdentifier\(this DatabaseFacade, StringBuilder, string\)](DelimitedIdentifierExtensions.GetDelimitedIdentifier#DotDoc.EntityFrameworkCore.Extensions.Utilities.DelimitedIdentifierExtensions.GetDelimitedIdentifier(thisMicrosoft.EntityFrameworkCore.Infrastructure.DatabaseFacade,System.Text.StringBuilder,string) 'DotDoc\.EntityFrameworkCore\.Extensions\.Utilities\.DelimitedIdentifierExtensions\.GetDelimitedIdentifier\(this Microsoft\.EntityFrameworkCore\.Infrastructure\.DatabaseFacade, System\.Text\.StringBuilder, string\)') | Appends the delimited SQL representation of an identifier \(column name, table name, etc\.\)\. |
| [GetDelimitedIdentifier\(this DatabaseFacade, StringBuilder, string, string\)](DelimitedIdentifierExtensions.GetDelimitedIdentifier#DotDoc.EntityFrameworkCore.Extensions.Utilities.DelimitedIdentifierExtensions.GetDelimitedIdentifier(thisMicrosoft.EntityFrameworkCore.Infrastructure.DatabaseFacade,System.Text.StringBuilder,string,string) 'DotDoc\.EntityFrameworkCore\.Extensions\.Utilities\.DelimitedIdentifierExtensions\.GetDelimitedIdentifier\(this Microsoft\.EntityFrameworkCore\.Infrastructure\.DatabaseFacade, System\.Text\.StringBuilder, string, string\)') | Appends the delimited SQL representation of an identifier \(column name, table name, etc\.\)\. |

<a name='DotDoc.EntityFrameworkCore.Extensions.Utilities.DelimitedIdentifierExtensions.GetDelimitedIdentifier(thisMicrosoft.EntityFrameworkCore.Infrastructure.DatabaseFacade,string)'></a>

## DelimitedIdentifierExtensions\.GetDelimitedIdentifier\(this DatabaseFacade, string\) Method

Generates the delimited SQL representation of an identifier \(column name, table name, etc\.\)\.

```csharp
public static string GetDelimitedIdentifier(this Microsoft.EntityFrameworkCore.Infrastructure.DatabaseFacade database, string name);
```
#### Parameters

<a name='DotDoc.EntityFrameworkCore.Extensions.Utilities.DelimitedIdentifierExtensions.GetDelimitedIdentifier(thisMicrosoft.EntityFrameworkCore.Infrastructure.DatabaseFacade,string).database'></a>

`database` [Microsoft\.EntityFrameworkCore\.Infrastructure\.DatabaseFacade](https://learn.microsoft.com/en-us/dotnet/api/microsoft.entityframeworkcore.infrastructure.databasefacade 'Microsoft\.EntityFrameworkCore\.Infrastructure\.DatabaseFacade')

The database facade\.

<a name='DotDoc.EntityFrameworkCore.Extensions.Utilities.DelimitedIdentifierExtensions.GetDelimitedIdentifier(thisMicrosoft.EntityFrameworkCore.Infrastructure.DatabaseFacade,string).name'></a>

`name` [System\.String](https://learn.microsoft.com/en-us/dotnet/api/system.string 'System\.String')

The identifier to delimit\.

#### Returns
[System\.String](https://learn.microsoft.com/en-us/dotnet/api/system.string 'System\.String')  
The generated string\.

#### Exceptions

[System\.ArgumentNullException](https://learn.microsoft.com/en-us/dotnet/api/system.argumentnullexception 'System\.ArgumentNullException')  
[database](DelimitedIdentifierExtensions#DotDoc.EntityFrameworkCore.Extensions.Utilities.DelimitedIdentifierExtensions.GetDelimitedIdentifier(thisMicrosoft.EntityFrameworkCore.Infrastructure.DatabaseFacade,string).database 'DotDoc\.EntityFrameworkCore\.Extensions\.Utilities\.DelimitedIdentifierExtensions\.GetDelimitedIdentifier\(this Microsoft\.EntityFrameworkCore\.Infrastructure\.DatabaseFacade, string\)\.database') or [name](DelimitedIdentifierExtensions#DotDoc.EntityFrameworkCore.Extensions.Utilities.DelimitedIdentifierExtensions.GetDelimitedIdentifier(thisMicrosoft.EntityFrameworkCore.Infrastructure.DatabaseFacade,string).name 'DotDoc\.EntityFrameworkCore\.Extensions\.Utilities\.DelimitedIdentifierExtensions\.GetDelimitedIdentifier\(this Microsoft\.EntityFrameworkCore\.Infrastructure\.DatabaseFacade, string\)\.name') is [null](https://docs.microsoft.com/en-us/dotnet/csharp/language-reference/keywords/null 'https://docs\.microsoft\.com/en\-us/dotnet/csharp/language\-reference/keywords/null')\.

[System\.ArgumentException](https://learn.microsoft.com/en-us/dotnet/api/system.argumentexception 'System\.ArgumentException')  
The database provider is not relational, or [name](DelimitedIdentifierExtensions#DotDoc.EntityFrameworkCore.Extensions.Utilities.DelimitedIdentifierExtensions.GetDelimitedIdentifier(thisMicrosoft.EntityFrameworkCore.Infrastructure.DatabaseFacade,string).name 'DotDoc\.EntityFrameworkCore\.Extensions\.Utilities\.DelimitedIdentifierExtensions\.GetDelimitedIdentifier\(this Microsoft\.EntityFrameworkCore\.Infrastructure\.DatabaseFacade, string\)\.name') is empty or white space\.

<a name='DotDoc.EntityFrameworkCore.Extensions.Utilities.DelimitedIdentifierExtensions.GetDelimitedIdentifier(thisMicrosoft.EntityFrameworkCore.Infrastructure.DatabaseFacade,string,string)'></a>

## DelimitedIdentifierExtensions\.GetDelimitedIdentifier\(this DatabaseFacade, string, string\) Method

Generates the delimited SQL representation of an identifier \(column name, table name, etc\.\)\.

```csharp
public static string GetDelimitedIdentifier(this Microsoft.EntityFrameworkCore.Infrastructure.DatabaseFacade database, string name, string? schema);
```
#### Parameters

<a name='DotDoc.EntityFrameworkCore.Extensions.Utilities.DelimitedIdentifierExtensions.GetDelimitedIdentifier(thisMicrosoft.EntityFrameworkCore.Infrastructure.DatabaseFacade,string,string).database'></a>

`database` [Microsoft\.EntityFrameworkCore\.Infrastructure\.DatabaseFacade](https://learn.microsoft.com/en-us/dotnet/api/microsoft.entityframeworkcore.infrastructure.databasefacade 'Microsoft\.EntityFrameworkCore\.Infrastructure\.DatabaseFacade')

The database facade\.

<a name='DotDoc.EntityFrameworkCore.Extensions.Utilities.DelimitedIdentifierExtensions.GetDelimitedIdentifier(thisMicrosoft.EntityFrameworkCore.Infrastructure.DatabaseFacade,string,string).name'></a>

`name` [System\.String](https://learn.microsoft.com/en-us/dotnet/api/system.string 'System\.String')

The identifier to delimit\.

<a name='DotDoc.EntityFrameworkCore.Extensions.Utilities.DelimitedIdentifierExtensions.GetDelimitedIdentifier(thisMicrosoft.EntityFrameworkCore.Infrastructure.DatabaseFacade,string,string).schema'></a>

`schema` [System\.String](https://learn.microsoft.com/en-us/dotnet/api/system.string 'System\.String')

The schema of the identifier\.

#### Returns
[System\.String](https://learn.microsoft.com/en-us/dotnet/api/system.string 'System\.String')  
The generated string\.

#### Exceptions

[System\.ArgumentNullException](https://learn.microsoft.com/en-us/dotnet/api/system.argumentnullexception 'System\.ArgumentNullException')  
[database](DelimitedIdentifierExtensions#DotDoc.EntityFrameworkCore.Extensions.Utilities.DelimitedIdentifierExtensions.GetDelimitedIdentifier(thisMicrosoft.EntityFrameworkCore.Infrastructure.DatabaseFacade,string,string).database 'DotDoc\.EntityFrameworkCore\.Extensions\.Utilities\.DelimitedIdentifierExtensions\.GetDelimitedIdentifier\(this Microsoft\.EntityFrameworkCore\.Infrastructure\.DatabaseFacade, string, string\)\.database') or [name](DelimitedIdentifierExtensions#DotDoc.EntityFrameworkCore.Extensions.Utilities.DelimitedIdentifierExtensions.GetDelimitedIdentifier(thisMicrosoft.EntityFrameworkCore.Infrastructure.DatabaseFacade,string,string).name 'DotDoc\.EntityFrameworkCore\.Extensions\.Utilities\.DelimitedIdentifierExtensions\.GetDelimitedIdentifier\(this Microsoft\.EntityFrameworkCore\.Infrastructure\.DatabaseFacade, string, string\)\.name') is [null](https://docs.microsoft.com/en-us/dotnet/csharp/language-reference/keywords/null 'https://docs\.microsoft\.com/en\-us/dotnet/csharp/language\-reference/keywords/null')\.

[System\.ArgumentException](https://learn.microsoft.com/en-us/dotnet/api/system.argumentexception 'System\.ArgumentException')  
The database provider is not relational, or [name](DelimitedIdentifierExtensions#DotDoc.EntityFrameworkCore.Extensions.Utilities.DelimitedIdentifierExtensions.GetDelimitedIdentifier(thisMicrosoft.EntityFrameworkCore.Infrastructure.DatabaseFacade,string,string).name 'DotDoc\.EntityFrameworkCore\.Extensions\.Utilities\.DelimitedIdentifierExtensions\.GetDelimitedIdentifier\(this Microsoft\.EntityFrameworkCore\.Infrastructure\.DatabaseFacade, string, string\)\.name') is empty or white space\.

<a name='DotDoc.EntityFrameworkCore.Extensions.Utilities.DelimitedIdentifierExtensions.GetDelimitedIdentifier(thisMicrosoft.EntityFrameworkCore.Infrastructure.DatabaseFacade,System.Text.StringBuilder,string)'></a>

## DelimitedIdentifierExtensions\.GetDelimitedIdentifier\(this DatabaseFacade, StringBuilder, string\) Method

Appends the delimited SQL representation of an identifier \(column name, table name, etc\.\)\.

```csharp
public static void GetDelimitedIdentifier(this Microsoft.EntityFrameworkCore.Infrastructure.DatabaseFacade database, System.Text.StringBuilder builder, string name);
```
#### Parameters

<a name='DotDoc.EntityFrameworkCore.Extensions.Utilities.DelimitedIdentifierExtensions.GetDelimitedIdentifier(thisMicrosoft.EntityFrameworkCore.Infrastructure.DatabaseFacade,System.Text.StringBuilder,string).database'></a>

`database` [Microsoft\.EntityFrameworkCore\.Infrastructure\.DatabaseFacade](https://learn.microsoft.com/en-us/dotnet/api/microsoft.entityframeworkcore.infrastructure.databasefacade 'Microsoft\.EntityFrameworkCore\.Infrastructure\.DatabaseFacade')

The database facade\.

<a name='DotDoc.EntityFrameworkCore.Extensions.Utilities.DelimitedIdentifierExtensions.GetDelimitedIdentifier(thisMicrosoft.EntityFrameworkCore.Infrastructure.DatabaseFacade,System.Text.StringBuilder,string).builder'></a>

`builder` [System\.Text\.StringBuilder](https://learn.microsoft.com/en-us/dotnet/api/system.text.stringbuilder 'System\.Text\.StringBuilder')

The [System\.Text\.StringBuilder](https://learn.microsoft.com/en-us/dotnet/api/system.text.stringbuilder 'System\.Text\.StringBuilder') to append the generated string to\.

<a name='DotDoc.EntityFrameworkCore.Extensions.Utilities.DelimitedIdentifierExtensions.GetDelimitedIdentifier(thisMicrosoft.EntityFrameworkCore.Infrastructure.DatabaseFacade,System.Text.StringBuilder,string).name'></a>

`name` [System\.String](https://learn.microsoft.com/en-us/dotnet/api/system.string 'System\.String')

The identifier to delimit\.

#### Exceptions

[System\.ArgumentNullException](https://learn.microsoft.com/en-us/dotnet/api/system.argumentnullexception 'System\.ArgumentNullException')  
[database](DelimitedIdentifierExtensions#DotDoc.EntityFrameworkCore.Extensions.Utilities.DelimitedIdentifierExtensions.GetDelimitedIdentifier(thisMicrosoft.EntityFrameworkCore.Infrastructure.DatabaseFacade,System.Text.StringBuilder,string).database 'DotDoc\.EntityFrameworkCore\.Extensions\.Utilities\.DelimitedIdentifierExtensions\.GetDelimitedIdentifier\(this Microsoft\.EntityFrameworkCore\.Infrastructure\.DatabaseFacade, System\.Text\.StringBuilder, string\)\.database'), [builder](DelimitedIdentifierExtensions#DotDoc.EntityFrameworkCore.Extensions.Utilities.DelimitedIdentifierExtensions.GetDelimitedIdentifier(thisMicrosoft.EntityFrameworkCore.Infrastructure.DatabaseFacade,System.Text.StringBuilder,string).builder 'DotDoc\.EntityFrameworkCore\.Extensions\.Utilities\.DelimitedIdentifierExtensions\.GetDelimitedIdentifier\(this Microsoft\.EntityFrameworkCore\.Infrastructure\.DatabaseFacade, System\.Text\.StringBuilder, string\)\.builder') or [name](DelimitedIdentifierExtensions#DotDoc.EntityFrameworkCore.Extensions.Utilities.DelimitedIdentifierExtensions.GetDelimitedIdentifier(thisMicrosoft.EntityFrameworkCore.Infrastructure.DatabaseFacade,System.Text.StringBuilder,string).name 'DotDoc\.EntityFrameworkCore\.Extensions\.Utilities\.DelimitedIdentifierExtensions\.GetDelimitedIdentifier\(this Microsoft\.EntityFrameworkCore\.Infrastructure\.DatabaseFacade, System\.Text\.StringBuilder, string\)\.name') is [null](https://docs.microsoft.com/en-us/dotnet/csharp/language-reference/keywords/null 'https://docs\.microsoft\.com/en\-us/dotnet/csharp/language\-reference/keywords/null')\.

[System\.ArgumentException](https://learn.microsoft.com/en-us/dotnet/api/system.argumentexception 'System\.ArgumentException')  
The database provider is not relational, or [name](DelimitedIdentifierExtensions#DotDoc.EntityFrameworkCore.Extensions.Utilities.DelimitedIdentifierExtensions.GetDelimitedIdentifier(thisMicrosoft.EntityFrameworkCore.Infrastructure.DatabaseFacade,System.Text.StringBuilder,string).name 'DotDoc\.EntityFrameworkCore\.Extensions\.Utilities\.DelimitedIdentifierExtensions\.GetDelimitedIdentifier\(this Microsoft\.EntityFrameworkCore\.Infrastructure\.DatabaseFacade, System\.Text\.StringBuilder, string\)\.name') is empty or white space\.

<a name='DotDoc.EntityFrameworkCore.Extensions.Utilities.DelimitedIdentifierExtensions.GetDelimitedIdentifier(thisMicrosoft.EntityFrameworkCore.Infrastructure.DatabaseFacade,System.Text.StringBuilder,string,string)'></a>

## DelimitedIdentifierExtensions\.GetDelimitedIdentifier\(this DatabaseFacade, StringBuilder, string, string\) Method

Appends the delimited SQL representation of an identifier \(column name, table name, etc\.\)\.

```csharp
public static void GetDelimitedIdentifier(this Microsoft.EntityFrameworkCore.Infrastructure.DatabaseFacade database, System.Text.StringBuilder builder, string name, string? schema);
```
#### Parameters

<a name='DotDoc.EntityFrameworkCore.Extensions.Utilities.DelimitedIdentifierExtensions.GetDelimitedIdentifier(thisMicrosoft.EntityFrameworkCore.Infrastructure.DatabaseFacade,System.Text.StringBuilder,string,string).database'></a>

`database` [Microsoft\.EntityFrameworkCore\.Infrastructure\.DatabaseFacade](https://learn.microsoft.com/en-us/dotnet/api/microsoft.entityframeworkcore.infrastructure.databasefacade 'Microsoft\.EntityFrameworkCore\.Infrastructure\.DatabaseFacade')

The database facade\.

<a name='DotDoc.EntityFrameworkCore.Extensions.Utilities.DelimitedIdentifierExtensions.GetDelimitedIdentifier(thisMicrosoft.EntityFrameworkCore.Infrastructure.DatabaseFacade,System.Text.StringBuilder,string,string).builder'></a>

`builder` [System\.Text\.StringBuilder](https://learn.microsoft.com/en-us/dotnet/api/system.text.stringbuilder 'System\.Text\.StringBuilder')

The [System\.Text\.StringBuilder](https://learn.microsoft.com/en-us/dotnet/api/system.text.stringbuilder 'System\.Text\.StringBuilder') to append the generated string to\.

<a name='DotDoc.EntityFrameworkCore.Extensions.Utilities.DelimitedIdentifierExtensions.GetDelimitedIdentifier(thisMicrosoft.EntityFrameworkCore.Infrastructure.DatabaseFacade,System.Text.StringBuilder,string,string).name'></a>

`name` [System\.String](https://learn.microsoft.com/en-us/dotnet/api/system.string 'System\.String')

The identifier to delimit\.

<a name='DotDoc.EntityFrameworkCore.Extensions.Utilities.DelimitedIdentifierExtensions.GetDelimitedIdentifier(thisMicrosoft.EntityFrameworkCore.Infrastructure.DatabaseFacade,System.Text.StringBuilder,string,string).schema'></a>

`schema` [System\.String](https://learn.microsoft.com/en-us/dotnet/api/system.string 'System\.String')

The schema of the identifier\.

#### Exceptions

[System\.ArgumentNullException](https://learn.microsoft.com/en-us/dotnet/api/system.argumentnullexception 'System\.ArgumentNullException')  
[database](DelimitedIdentifierExtensions#DotDoc.EntityFrameworkCore.Extensions.Utilities.DelimitedIdentifierExtensions.GetDelimitedIdentifier(thisMicrosoft.EntityFrameworkCore.Infrastructure.DatabaseFacade,System.Text.StringBuilder,string,string).database 'DotDoc\.EntityFrameworkCore\.Extensions\.Utilities\.DelimitedIdentifierExtensions\.GetDelimitedIdentifier\(this Microsoft\.EntityFrameworkCore\.Infrastructure\.DatabaseFacade, System\.Text\.StringBuilder, string, string\)\.database'), [builder](DelimitedIdentifierExtensions#DotDoc.EntityFrameworkCore.Extensions.Utilities.DelimitedIdentifierExtensions.GetDelimitedIdentifier(thisMicrosoft.EntityFrameworkCore.Infrastructure.DatabaseFacade,System.Text.StringBuilder,string,string).builder 'DotDoc\.EntityFrameworkCore\.Extensions\.Utilities\.DelimitedIdentifierExtensions\.GetDelimitedIdentifier\(this Microsoft\.EntityFrameworkCore\.Infrastructure\.DatabaseFacade, System\.Text\.StringBuilder, string, string\)\.builder') or [name](DelimitedIdentifierExtensions#DotDoc.EntityFrameworkCore.Extensions.Utilities.DelimitedIdentifierExtensions.GetDelimitedIdentifier(thisMicrosoft.EntityFrameworkCore.Infrastructure.DatabaseFacade,System.Text.StringBuilder,string,string).name 'DotDoc\.EntityFrameworkCore\.Extensions\.Utilities\.DelimitedIdentifierExtensions\.GetDelimitedIdentifier\(this Microsoft\.EntityFrameworkCore\.Infrastructure\.DatabaseFacade, System\.Text\.StringBuilder, string, string\)\.name') is [null](https://docs.microsoft.com/en-us/dotnet/csharp/language-reference/keywords/null 'https://docs\.microsoft\.com/en\-us/dotnet/csharp/language\-reference/keywords/null')\.

[System\.ArgumentException](https://learn.microsoft.com/en-us/dotnet/api/system.argumentexception 'System\.ArgumentException')  
The database provider is not relational, or [name](DelimitedIdentifierExtensions#DotDoc.EntityFrameworkCore.Extensions.Utilities.DelimitedIdentifierExtensions.GetDelimitedIdentifier(thisMicrosoft.EntityFrameworkCore.Infrastructure.DatabaseFacade,System.Text.StringBuilder,string,string).name 'DotDoc\.EntityFrameworkCore\.Extensions\.Utilities\.DelimitedIdentifierExtensions\.GetDelimitedIdentifier\(this Microsoft\.EntityFrameworkCore\.Infrastructure\.DatabaseFacade, System\.Text\.StringBuilder, string, string\)\.name') is empty or white space\.