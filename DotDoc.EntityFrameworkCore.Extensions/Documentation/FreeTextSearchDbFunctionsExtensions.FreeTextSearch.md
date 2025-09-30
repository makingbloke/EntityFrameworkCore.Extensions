#### [DotDoc\.EntityFrameworkCore\.Extensions](Home.md 'Home')
### [DotDoc\.EntityFrameworkCore\.Extensions\.FreeTextSearchFunction](DotDoc.EntityFrameworkCore.Extensions.FreeTextSearchFunction.md 'DotDoc\.EntityFrameworkCore\.Extensions\.FreeTextSearchFunction').[FreeTextSearchDbFunctionsExtensions](FreeTextSearchDbFunctionsExtensions.md 'DotDoc\.EntityFrameworkCore\.Extensions\.FreeTextSearchFunction\.FreeTextSearchDbFunctionsExtensions')

## FreeTextSearchDbFunctionsExtensions\.FreeTextSearch Method

| Overloads | |
| :--- | :--- |
| [FreeTextSearch\(this DbFunctions, object, string\)](FreeTextSearchDbFunctionsExtensions.FreeTextSearch.md#DotDoc.EntityFrameworkCore.Extensions.FreeTextSearchFunction.FreeTextSearchDbFunctionsExtensions.FreeTextSearch(thisMicrosoft.EntityFrameworkCore.DbFunctions,object,string) 'DotDoc\.EntityFrameworkCore\.Extensions\.FreeTextSearchFunction\.FreeTextSearchDbFunctionsExtensions\.FreeTextSearch\(this Microsoft\.EntityFrameworkCore\.DbFunctions, object, string\)') | Perform a free text search\. |
| [FreeTextSearch\(this DbFunctions, object, string, bool\)](FreeTextSearchDbFunctionsExtensions.FreeTextSearch.md#DotDoc.EntityFrameworkCore.Extensions.FreeTextSearchFunction.FreeTextSearchDbFunctionsExtensions.FreeTextSearch(thisMicrosoft.EntityFrameworkCore.DbFunctions,object,string,bool) 'DotDoc\.EntityFrameworkCore\.Extensions\.FreeTextSearchFunction\.FreeTextSearchDbFunctionsExtensions\.FreeTextSearch\(this Microsoft\.EntityFrameworkCore\.DbFunctions, object, string, bool\)') | Perform a free text search\. |
| [FreeTextSearch\(this DbFunctions, object, string, bool, int\)](FreeTextSearchDbFunctionsExtensions.FreeTextSearch.md#DotDoc.EntityFrameworkCore.Extensions.FreeTextSearchFunction.FreeTextSearchDbFunctionsExtensions.FreeTextSearch(thisMicrosoft.EntityFrameworkCore.DbFunctions,object,string,bool,int) 'DotDoc\.EntityFrameworkCore\.Extensions\.FreeTextSearchFunction\.FreeTextSearchDbFunctionsExtensions\.FreeTextSearch\(this Microsoft\.EntityFrameworkCore\.DbFunctions, object, string, bool, int\)') | Perform a free text search\. |
| [FreeTextSearch\(this DbFunctions, object, string, int\)](FreeTextSearchDbFunctionsExtensions.FreeTextSearch.md#DotDoc.EntityFrameworkCore.Extensions.FreeTextSearchFunction.FreeTextSearchDbFunctionsExtensions.FreeTextSearch(thisMicrosoft.EntityFrameworkCore.DbFunctions,object,string,int) 'DotDoc\.EntityFrameworkCore\.Extensions\.FreeTextSearchFunction\.FreeTextSearchDbFunctionsExtensions\.FreeTextSearch\(this Microsoft\.EntityFrameworkCore\.DbFunctions, object, string, int\)') | Perform a free text search\. |

<a name='DotDoc.EntityFrameworkCore.Extensions.FreeTextSearchFunction.FreeTextSearchDbFunctionsExtensions.FreeTextSearch(thisMicrosoft.EntityFrameworkCore.DbFunctions,object,string)'></a>

## FreeTextSearchDbFunctionsExtensions\.FreeTextSearch\(this DbFunctions, object, string\) Method

Perform a free text search\.

```csharp
public static bool FreeTextSearch(this Microsoft.EntityFrameworkCore.DbFunctions dbFunctions, object propertyReference, string freeText);
```
#### Parameters

<a name='DotDoc.EntityFrameworkCore.Extensions.FreeTextSearchFunction.FreeTextSearchDbFunctionsExtensions.FreeTextSearch(thisMicrosoft.EntityFrameworkCore.DbFunctions,object,string).dbFunctions'></a>

`dbFunctions` [Microsoft\.EntityFrameworkCore\.DbFunctions](https://learn.microsoft.com/en-us/dotnet/api/microsoft.entityframeworkcore.dbfunctions 'Microsoft\.EntityFrameworkCore\.DbFunctions')

The [Microsoft\.EntityFrameworkCore\.DbFunctions](https://learn.microsoft.com/en-us/dotnet/api/microsoft.entityframeworkcore.dbfunctions 'Microsoft\.EntityFrameworkCore\.DbFunctions') instance\.

<a name='DotDoc.EntityFrameworkCore.Extensions.FreeTextSearchFunction.FreeTextSearchDbFunctionsExtensions.FreeTextSearch(thisMicrosoft.EntityFrameworkCore.DbFunctions,object,string).propertyReference'></a>

`propertyReference` [System\.Object](https://learn.microsoft.com/en-us/dotnet/api/system.object 'System\.Object')

The property on which the search will be performed\.

<a name='DotDoc.EntityFrameworkCore.Extensions.FreeTextSearchFunction.FreeTextSearchDbFunctionsExtensions.FreeTextSearch(thisMicrosoft.EntityFrameworkCore.DbFunctions,object,string).freeText'></a>

`freeText` [System\.String](https://learn.microsoft.com/en-us/dotnet/api/system.string 'System\.String')

The text that will be searched for in the property\.

#### Returns
[System\.Boolean](https://learn.microsoft.com/en-us/dotnet/api/system.boolean 'System\.Boolean')  
A [System\.Boolean](https://learn.microsoft.com/en-us/dotnet/api/system.boolean 'System\.Boolean') value indicating whether the text exists in the property\.

<a name='DotDoc.EntityFrameworkCore.Extensions.FreeTextSearchFunction.FreeTextSearchDbFunctionsExtensions.FreeTextSearch(thisMicrosoft.EntityFrameworkCore.DbFunctions,object,string,bool)'></a>

## FreeTextSearchDbFunctionsExtensions\.FreeTextSearch\(this DbFunctions, object, string, bool\) Method

Perform a free text search\.

```csharp
public static bool FreeTextSearch(this Microsoft.EntityFrameworkCore.DbFunctions dbFunctions, object propertyReference, string freeText, bool useStemming);
```
#### Parameters

<a name='DotDoc.EntityFrameworkCore.Extensions.FreeTextSearchFunction.FreeTextSearchDbFunctionsExtensions.FreeTextSearch(thisMicrosoft.EntityFrameworkCore.DbFunctions,object,string,bool).dbFunctions'></a>

`dbFunctions` [Microsoft\.EntityFrameworkCore\.DbFunctions](https://learn.microsoft.com/en-us/dotnet/api/microsoft.entityframeworkcore.dbfunctions 'Microsoft\.EntityFrameworkCore\.DbFunctions')

The [Microsoft\.EntityFrameworkCore\.DbFunctions](https://learn.microsoft.com/en-us/dotnet/api/microsoft.entityframeworkcore.dbfunctions 'Microsoft\.EntityFrameworkCore\.DbFunctions') instance\.

<a name='DotDoc.EntityFrameworkCore.Extensions.FreeTextSearchFunction.FreeTextSearchDbFunctionsExtensions.FreeTextSearch(thisMicrosoft.EntityFrameworkCore.DbFunctions,object,string,bool).propertyReference'></a>

`propertyReference` [System\.Object](https://learn.microsoft.com/en-us/dotnet/api/system.object 'System\.Object')

The property on which the search will be performed\.

<a name='DotDoc.EntityFrameworkCore.Extensions.FreeTextSearchFunction.FreeTextSearchDbFunctionsExtensions.FreeTextSearch(thisMicrosoft.EntityFrameworkCore.DbFunctions,object,string,bool).freeText'></a>

`freeText` [System\.String](https://learn.microsoft.com/en-us/dotnet/api/system.string 'System\.String')

The text that will be searched for in the property\.

<a name='DotDoc.EntityFrameworkCore.Extensions.FreeTextSearchFunction.FreeTextSearchDbFunctionsExtensions.FreeTextSearch(thisMicrosoft.EntityFrameworkCore.DbFunctions,object,string,bool).useStemming'></a>

`useStemming` [System\.Boolean](https://learn.microsoft.com/en-us/dotnet/api/system.boolean 'System\.Boolean')

A value indicating whether stemming will be used when searching\.

#### Returns
[System\.Boolean](https://learn.microsoft.com/en-us/dotnet/api/system.boolean 'System\.Boolean')  
A [System\.Boolean](https://learn.microsoft.com/en-us/dotnet/api/system.boolean 'System\.Boolean') value indicating whether the text exists in the property\.

<a name='DotDoc.EntityFrameworkCore.Extensions.FreeTextSearchFunction.FreeTextSearchDbFunctionsExtensions.FreeTextSearch(thisMicrosoft.EntityFrameworkCore.DbFunctions,object,string,bool,int)'></a>

## FreeTextSearchDbFunctionsExtensions\.FreeTextSearch\(this DbFunctions, object, string, bool, int\) Method

Perform a free text search\.

```csharp
public static bool FreeTextSearch(this Microsoft.EntityFrameworkCore.DbFunctions dbFunctions, object propertyReference, string freeText, bool useStemming, int languageTerm);
```
#### Parameters

<a name='DotDoc.EntityFrameworkCore.Extensions.FreeTextSearchFunction.FreeTextSearchDbFunctionsExtensions.FreeTextSearch(thisMicrosoft.EntityFrameworkCore.DbFunctions,object,string,bool,int).dbFunctions'></a>

`dbFunctions` [Microsoft\.EntityFrameworkCore\.DbFunctions](https://learn.microsoft.com/en-us/dotnet/api/microsoft.entityframeworkcore.dbfunctions 'Microsoft\.EntityFrameworkCore\.DbFunctions')

The [Microsoft\.EntityFrameworkCore\.DbFunctions](https://learn.microsoft.com/en-us/dotnet/api/microsoft.entityframeworkcore.dbfunctions 'Microsoft\.EntityFrameworkCore\.DbFunctions') instance\.

<a name='DotDoc.EntityFrameworkCore.Extensions.FreeTextSearchFunction.FreeTextSearchDbFunctionsExtensions.FreeTextSearch(thisMicrosoft.EntityFrameworkCore.DbFunctions,object,string,bool,int).propertyReference'></a>

`propertyReference` [System\.Object](https://learn.microsoft.com/en-us/dotnet/api/system.object 'System\.Object')

The property on which the search will be performed\.

<a name='DotDoc.EntityFrameworkCore.Extensions.FreeTextSearchFunction.FreeTextSearchDbFunctionsExtensions.FreeTextSearch(thisMicrosoft.EntityFrameworkCore.DbFunctions,object,string,bool,int).freeText'></a>

`freeText` [System\.String](https://learn.microsoft.com/en-us/dotnet/api/system.string 'System\.String')

The text that will be searched for in the property\.

<a name='DotDoc.EntityFrameworkCore.Extensions.FreeTextSearchFunction.FreeTextSearchDbFunctionsExtensions.FreeTextSearch(thisMicrosoft.EntityFrameworkCore.DbFunctions,object,string,bool,int).useStemming'></a>

`useStemming` [System\.Boolean](https://learn.microsoft.com/en-us/dotnet/api/system.boolean 'System\.Boolean')

A value indicating whether stemming will be used when searching\.

<a name='DotDoc.EntityFrameworkCore.Extensions.FreeTextSearchFunction.FreeTextSearchDbFunctionsExtensions.FreeTextSearch(thisMicrosoft.EntityFrameworkCore.DbFunctions,object,string,bool,int).languageTerm'></a>

`languageTerm` [System\.Int32](https://learn.microsoft.com/en-us/dotnet/api/system.int32 'System\.Int32')

A Language ID from the sys\.syslanguages table \(SQL Server only\)\.

#### Returns
[System\.Boolean](https://learn.microsoft.com/en-us/dotnet/api/system.boolean 'System\.Boolean')  
A [System\.Boolean](https://learn.microsoft.com/en-us/dotnet/api/system.boolean 'System\.Boolean') value indicating whether the text exists in the property\.

<a name='DotDoc.EntityFrameworkCore.Extensions.FreeTextSearchFunction.FreeTextSearchDbFunctionsExtensions.FreeTextSearch(thisMicrosoft.EntityFrameworkCore.DbFunctions,object,string,int)'></a>

## FreeTextSearchDbFunctionsExtensions\.FreeTextSearch\(this DbFunctions, object, string, int\) Method

Perform a free text search\.

```csharp
public static bool FreeTextSearch(this Microsoft.EntityFrameworkCore.DbFunctions dbFunctions, object propertyReference, string freeText, int languageTerm);
```
#### Parameters

<a name='DotDoc.EntityFrameworkCore.Extensions.FreeTextSearchFunction.FreeTextSearchDbFunctionsExtensions.FreeTextSearch(thisMicrosoft.EntityFrameworkCore.DbFunctions,object,string,int).dbFunctions'></a>

`dbFunctions` [Microsoft\.EntityFrameworkCore\.DbFunctions](https://learn.microsoft.com/en-us/dotnet/api/microsoft.entityframeworkcore.dbfunctions 'Microsoft\.EntityFrameworkCore\.DbFunctions')

The [Microsoft\.EntityFrameworkCore\.DbFunctions](https://learn.microsoft.com/en-us/dotnet/api/microsoft.entityframeworkcore.dbfunctions 'Microsoft\.EntityFrameworkCore\.DbFunctions') instance\.

<a name='DotDoc.EntityFrameworkCore.Extensions.FreeTextSearchFunction.FreeTextSearchDbFunctionsExtensions.FreeTextSearch(thisMicrosoft.EntityFrameworkCore.DbFunctions,object,string,int).propertyReference'></a>

`propertyReference` [System\.Object](https://learn.microsoft.com/en-us/dotnet/api/system.object 'System\.Object')

The property on which the search will be performed\.

<a name='DotDoc.EntityFrameworkCore.Extensions.FreeTextSearchFunction.FreeTextSearchDbFunctionsExtensions.FreeTextSearch(thisMicrosoft.EntityFrameworkCore.DbFunctions,object,string,int).freeText'></a>

`freeText` [System\.String](https://learn.microsoft.com/en-us/dotnet/api/system.string 'System\.String')

The text that will be searched for in the property\.

<a name='DotDoc.EntityFrameworkCore.Extensions.FreeTextSearchFunction.FreeTextSearchDbFunctionsExtensions.FreeTextSearch(thisMicrosoft.EntityFrameworkCore.DbFunctions,object,string,int).languageTerm'></a>

`languageTerm` [System\.Int32](https://learn.microsoft.com/en-us/dotnet/api/system.int32 'System\.Int32')

A Language ID from the sys\.syslanguages table \(SQL Server only\)\.

#### Returns
[System\.Boolean](https://learn.microsoft.com/en-us/dotnet/api/system.boolean 'System\.Boolean')  
A [System\.Boolean](https://learn.microsoft.com/en-us/dotnet/api/system.boolean 'System\.Boolean') value indicating whether the text exists in the property\.