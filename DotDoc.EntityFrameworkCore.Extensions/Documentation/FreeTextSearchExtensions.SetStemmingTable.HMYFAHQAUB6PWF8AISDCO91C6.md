#### [DotDoc\.EntityFrameworkCore\.Extensions](index.md 'index')
### [DotDoc\.EntityFrameworkCore\.Extensions\.FreeTextSearchFunction](DotDoc.EntityFrameworkCore.Extensions.FreeTextSearchFunction.md 'DotDoc\.EntityFrameworkCore\.Extensions\.FreeTextSearchFunction').[FreeTextSearchExtensions](FreeTextSearchExtensions.md 'DotDoc\.EntityFrameworkCore\.Extensions\.FreeTextSearchFunction\.FreeTextSearchExtensions')

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

`entityBuilder` [Microsoft\.EntityFrameworkCore\.Metadata\.Builders\.EntityTypeBuilder&lt;](https://learn.microsoft.com/en-us/dotnet/api/microsoft.entityframeworkcore.metadata.builders.entitytypebuilder-1 'Microsoft\.EntityFrameworkCore\.Metadata\.Builders\.EntityTypeBuilder\`1')[TSource](FreeTextSearchExtensions.SetStemmingTable.HMYFAHQAUB6PWF8AISDCO91C6.md#DotDoc.EntityFrameworkCore.Extensions.FreeTextSearchFunction.FreeTextSearchExtensions.SetStemmingTable_TSource_(thisMicrosoft.EntityFrameworkCore.Metadata.Builders.EntityTypeBuilder_TSource_,string).TSource 'DotDoc\.EntityFrameworkCore\.Extensions\.FreeTextSearchFunction\.FreeTextSearchExtensions\.SetStemmingTable\<TSource\>\(this Microsoft\.EntityFrameworkCore\.Metadata\.Builders\.EntityTypeBuilder\<TSource\>, string\)\.TSource')[&gt;](https://learn.microsoft.com/en-us/dotnet/api/microsoft.entityframeworkcore.metadata.builders.entitytypebuilder-1 'Microsoft\.EntityFrameworkCore\.Metadata\.Builders\.EntityTypeBuilder\`1')

The builder used to construct the entity for the context\.

<a name='DotDoc.EntityFrameworkCore.Extensions.FreeTextSearchFunction.FreeTextSearchExtensions.SetStemmingTable_TSource_(thisMicrosoft.EntityFrameworkCore.Metadata.Builders.EntityTypeBuilder_TSource_,string).tableName'></a>

`tableName` [System\.String](https://learn.microsoft.com/en-us/dotnet/api/system.string 'System\.String')

The stemming table name\.

#### Returns
[Microsoft\.EntityFrameworkCore\.Metadata\.Builders\.EntityTypeBuilder&lt;](https://learn.microsoft.com/en-us/dotnet/api/microsoft.entityframeworkcore.metadata.builders.entitytypebuilder-1 'Microsoft\.EntityFrameworkCore\.Metadata\.Builders\.EntityTypeBuilder\`1')[TSource](FreeTextSearchExtensions.SetStemmingTable.HMYFAHQAUB6PWF8AISDCO91C6.md#DotDoc.EntityFrameworkCore.Extensions.FreeTextSearchFunction.FreeTextSearchExtensions.SetStemmingTable_TSource_(thisMicrosoft.EntityFrameworkCore.Metadata.Builders.EntityTypeBuilder_TSource_,string).TSource 'DotDoc\.EntityFrameworkCore\.Extensions\.FreeTextSearchFunction\.FreeTextSearchExtensions\.SetStemmingTable\<TSource\>\(this Microsoft\.EntityFrameworkCore\.Metadata\.Builders\.EntityTypeBuilder\<TSource\>, string\)\.TSource')[&gt;](https://learn.microsoft.com/en-us/dotnet/api/microsoft.entityframeworkcore.metadata.builders.entitytypebuilder-1 'Microsoft\.EntityFrameworkCore\.Metadata\.Builders\.EntityTypeBuilder\`1')  
The same builder instance so multiple calls can be chained\.