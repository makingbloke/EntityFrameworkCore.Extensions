#### [DotDoc\.EntityFrameworkCore\.Extensions](Home 'Home')
### [DotDoc\.EntityFrameworkCore\.Extensions\.TableHints](DotDoc.EntityFrameworkCore.Extensions.TableHints 'DotDoc\.EntityFrameworkCore\.Extensions\.TableHints').[SqlServerTableHints](SqlServerTableHints 'DotDoc\.EntityFrameworkCore\.Extensions\.TableHints\.SqlServerTableHints')

## SqlServerTableHints\.SpatialWindowMaxCells\(int\) Method

SPATIAL\_WINDOW\_MAX\_CELLS = \<integer\_value\>\.

```csharp
public static DotDoc.EntityFrameworkCore.Extensions.TableHints.SqlServerTableHints SpatialWindowMaxCells(int value);
```
#### Parameters

<a name='DotDoc.EntityFrameworkCore.Extensions.TableHints.SqlServerTableHints.SpatialWindowMaxCells(int).value'></a>

`value` [System\.Int32](https://learn.microsoft.com/en-us/dotnet/api/system.int32 'System\.Int32')

Specifies the maximum number of cells to use for tessellating a geometry or geography object \(1\-8192\)\.

#### Returns
[SqlServerTableHints](SqlServerTableHints 'DotDoc\.EntityFrameworkCore\.Extensions\.TableHints\.SqlServerTableHints')  
[SqlServerTableHints](SqlServerTableHints 'DotDoc\.EntityFrameworkCore\.Extensions\.TableHints\.SqlServerTableHints')\.

#### Exceptions

[System\.ArgumentOutOfRangeException](https://learn.microsoft.com/en-us/dotnet/api/system.argumentoutofrangeexception 'System\.ArgumentOutOfRangeException')  
[value](SqlServerTableHints.SpatialWindowMaxCells.H6NMHWBX3FP57D3F8CHZXGB68#DotDoc.EntityFrameworkCore.Extensions.TableHints.SqlServerTableHints.SpatialWindowMaxCells(int).value 'DotDoc\.EntityFrameworkCore\.Extensions\.TableHints\.SqlServerTableHints\.SpatialWindowMaxCells\(int\)\.value') is less than 1 or greater than 8192\.