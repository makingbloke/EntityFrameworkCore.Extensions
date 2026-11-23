#### [DotDoc\.EntityFrameworkCore\.Extensions](Home 'Home')
### [DotDoc\.EntityFrameworkCore\.Extensions\.FreeTextSearchFunction](DotDoc.EntityFrameworkCore.Extensions.FreeTextSearchFunction 'DotDoc\.EntityFrameworkCore\.Extensions\.FreeTextSearchFunction').[FreeTextSearchExtensions](FreeTextSearchExtensions 'DotDoc\.EntityFrameworkCore\.Extensions\.FreeTextSearchFunction\.FreeTextSearchExtensions')

## FreeTextSearchExtensions\.SetStemmingTable\<TSource\>\(this EntityTypeBuilder\<TSource\>, string\) Method

Set the stemming table name associated with this entity\.

```csharp
public static Microsoft.EntityFrameworkCore.Metadata.Builders.EntityTypeBuilder<TSource> SetStemmingTable<TSource>(this Microsoft.EntityFrameworkCore.Metadata.Builders.EntityTypeBuilder<TSource> entityBuilder, string tableName)
    where TSource : class;
```
#### Type parameters

<a name='DotDoc.EntityFrameworkCore.Extensions.FreeTextSearchFunction.FreeTextSearchExtensions.SetStemmingTable_TSource_(thisMicrosoft.EntityFrameworkCore.Metadata.Builders.EntityTypeBuilder_TSource_,string).TSource'></a>

`TSource`

The type of the elements of the source\.
#### Parameters

<a name='DotDoc.EntityFrameworkCore.Extensions.FreeTextSearchFunction.FreeTextSearchExtensions.SetStemmingTable_TSource_(thisMicrosoft.EntityFrameworkCore.Metadata.Builders.EntityTypeBuilder_TSource_,string).entityBuilder'></a>

`entityBuilder` [Microsoft\.EntityFrameworkCore\.Metadata\.Builders\.EntityTypeBuilder&lt;](https://learn.microsoft.com/en-us/dotnet/api/microsoft.entityframeworkcore.metadata.builders.entitytypebuilder-1 'Microsoft\.EntityFrameworkCore\.Metadata\.Builders\.EntityTypeBuilder\`1')[TSource](FreeTextSearchExtensions.SetStemmingTable.HMYFAHQAUB6PWF8AISDCO91C6#DotDoc.EntityFrameworkCore.Extensions.FreeTextSearchFunction.FreeTextSearchExtensions.SetStemmingTable_TSource_(thisMicrosoft.EntityFrameworkCore.Metadata.Builders.EntityTypeBuilder_TSource_,string).TSource 'DotDoc\.EntityFrameworkCore\.Extensions\.FreeTextSearchFunction\.FreeTextSearchExtensions\.SetStemmingTable\<TSource\>\(this Microsoft\.EntityFrameworkCore\.Metadata\.Builders\.EntityTypeBuilder\<TSource\>, string\)\.TSource')[&gt;](https://learn.microsoft.com/en-us/dotnet/api/microsoft.entityframeworkcore.metadata.builders.entitytypebuilder-1 'Microsoft\.EntityFrameworkCore\.Metadata\.Builders\.EntityTypeBuilder\`1')

The builder used to construct the entity for the context\.

<a name='DotDoc.EntityFrameworkCore.Extensions.FreeTextSearchFunction.FreeTextSearchExtensions.SetStemmingTable_TSource_(thisMicrosoft.EntityFrameworkCore.Metadata.Builders.EntityTypeBuilder_TSource_,string).tableName'></a>

`tableName` [System\.String](https://learn.microsoft.com/en-us/dotnet/api/system.string 'System\.String')

The stemming table name\.

#### Returns
[Microsoft\.EntityFrameworkCore\.Metadata\.Builders\.EntityTypeBuilder&lt;](https://learn.microsoft.com/en-us/dotnet/api/microsoft.entityframeworkcore.metadata.builders.entitytypebuilder-1 'Microsoft\.EntityFrameworkCore\.Metadata\.Builders\.EntityTypeBuilder\`1')[TSource](FreeTextSearchExtensions.SetStemmingTable.HMYFAHQAUB6PWF8AISDCO91C6#DotDoc.EntityFrameworkCore.Extensions.FreeTextSearchFunction.FreeTextSearchExtensions.SetStemmingTable_TSource_(thisMicrosoft.EntityFrameworkCore.Metadata.Builders.EntityTypeBuilder_TSource_,string).TSource 'DotDoc\.EntityFrameworkCore\.Extensions\.FreeTextSearchFunction\.FreeTextSearchExtensions\.SetStemmingTable\<TSource\>\(this Microsoft\.EntityFrameworkCore\.Metadata\.Builders\.EntityTypeBuilder\<TSource\>, string\)\.TSource')[&gt;](https://learn.microsoft.com/en-us/dotnet/api/microsoft.entityframeworkcore.metadata.builders.entitytypebuilder-1 'Microsoft\.EntityFrameworkCore\.Metadata\.Builders\.EntityTypeBuilder\`1')  
The same builder instance so multiple calls can be chained\.

#### Exceptions

[System\.ArgumentNullException](https://learn.microsoft.com/en-us/dotnet/api/system.argumentnullexception 'System\.ArgumentNullException')  
[entityBuilder](FreeTextSearchExtensions.SetStemmingTable.HMYFAHQAUB6PWF8AISDCO91C6#DotDoc.EntityFrameworkCore.Extensions.FreeTextSearchFunction.FreeTextSearchExtensions.SetStemmingTable_TSource_(thisMicrosoft.EntityFrameworkCore.Metadata.Builders.EntityTypeBuilder_TSource_,string).entityBuilder 'DotDoc\.EntityFrameworkCore\.Extensions\.FreeTextSearchFunction\.FreeTextSearchExtensions\.SetStemmingTable\<TSource\>\(this Microsoft\.EntityFrameworkCore\.Metadata\.Builders\.EntityTypeBuilder\<TSource\>, string\)\.entityBuilder') or [tableName](FreeTextSearchExtensions.SetStemmingTable.HMYFAHQAUB6PWF8AISDCO91C6#DotDoc.EntityFrameworkCore.Extensions.FreeTextSearchFunction.FreeTextSearchExtensions.SetStemmingTable_TSource_(thisMicrosoft.EntityFrameworkCore.Metadata.Builders.EntityTypeBuilder_TSource_,string).tableName 'DotDoc\.EntityFrameworkCore\.Extensions\.FreeTextSearchFunction\.FreeTextSearchExtensions\.SetStemmingTable\<TSource\>\(this Microsoft\.EntityFrameworkCore\.Metadata\.Builders\.EntityTypeBuilder\<TSource\>, string\)\.tableName') is [null](https://docs.microsoft.com/en-us/dotnet/csharp/language-reference/keywords/null 'https://docs\.microsoft\.com/en\-us/dotnet/csharp/language\-reference/keywords/null')\.

[System\.ArgumentException](https://learn.microsoft.com/en-us/dotnet/api/system.argumentexception 'System\.ArgumentException')  
[tableName](FreeTextSearchExtensions.SetStemmingTable.HMYFAHQAUB6PWF8AISDCO91C6#DotDoc.EntityFrameworkCore.Extensions.FreeTextSearchFunction.FreeTextSearchExtensions.SetStemmingTable_TSource_(thisMicrosoft.EntityFrameworkCore.Metadata.Builders.EntityTypeBuilder_TSource_,string).tableName 'DotDoc\.EntityFrameworkCore\.Extensions\.FreeTextSearchFunction\.FreeTextSearchExtensions\.SetStemmingTable\<TSource\>\(this Microsoft\.EntityFrameworkCore\.Metadata\.Builders\.EntityTypeBuilder\<TSource\>, string\)\.tableName') is empty\.

### Remarks
SQLite only\. A FreeTextSearch with stemming on this entity's table searches the stemming table \(an FTS table using the porter tokeniser,
configured as a shared type entity of the same type\) and finds the matching rows in this table by their ROWID\. So both entity types need
a property mapped to their ROWID column, and each row in the stemming table must have the same ROWID as the matching row in this table
\(an external content FTS table does this automatically\)\.