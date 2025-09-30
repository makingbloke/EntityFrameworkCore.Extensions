#### [DotDoc\.EntityFrameworkCore\.Extensions](Home 'Home')
### [DotDoc\.EntityFrameworkCore\.Extensions\.TableHints](DotDoc.EntityFrameworkCore.Extensions.TableHints 'DotDoc\.EntityFrameworkCore\.Extensions\.TableHints')

## SqlServerTableHint Class

SQL Server Table Hints \(used to override default query optimiser behaviour\)\.

```csharp
public sealed class SqlServerTableHint : DotDoc.EntityFrameworkCore.Extensions.TableHints.ITableHint
```

Inheritance [System\.Object](https://learn.microsoft.com/en-us/dotnet/api/system.object 'System\.Object') &#129106; SqlServerTableHint

Implements [ITableHint](ITableHint 'DotDoc\.EntityFrameworkCore\.Extensions\.TableHints\.ITableHint')

| Fields | |
| :--- | :--- |
| [ForceScan](SqlServerTableHint.ForceScan 'DotDoc\.EntityFrameworkCore\.Extensions\.TableHints\.SqlServerTableHint\.ForceScan') | FORCESCAN\. |
| [HoldLock](SqlServerTableHint.HoldLock 'DotDoc\.EntityFrameworkCore\.Extensions\.TableHints\.SqlServerTableHint\.HoldLock') | HOLDLOCK\. |
| [NoExpand](SqlServerTableHint.NoExpand 'DotDoc\.EntityFrameworkCore\.Extensions\.TableHints\.SqlServerTableHint\.NoExpand') | NOEXPAND\. |
| [NoLock](SqlServerTableHint.NoLock 'DotDoc\.EntityFrameworkCore\.Extensions\.TableHints\.SqlServerTableHint\.NoLock') | NOLOCK\. |
| [NoWait](SqlServerTableHint.NoWait 'DotDoc\.EntityFrameworkCore\.Extensions\.TableHints\.SqlServerTableHint\.NoWait') | NOWAIT\. |
| [PagLock](SqlServerTableHint.PagLock 'DotDoc\.EntityFrameworkCore\.Extensions\.TableHints\.SqlServerTableHint\.PagLock') | PAGLOCK\. |
| [ReadCommitted](SqlServerTableHint.ReadCommitted 'DotDoc\.EntityFrameworkCore\.Extensions\.TableHints\.SqlServerTableHint\.ReadCommitted') | READCOMMITTED\. |
| [ReadCommittedLock](SqlServerTableHint.ReadCommittedLock 'DotDoc\.EntityFrameworkCore\.Extensions\.TableHints\.SqlServerTableHint\.ReadCommittedLock') | READCOMMITTEDLOCK\. |
| [ReadPast](SqlServerTableHint.ReadPast 'DotDoc\.EntityFrameworkCore\.Extensions\.TableHints\.SqlServerTableHint\.ReadPast') | READPAST\. |
| [ReadUncommitted](SqlServerTableHint.ReadUncommitted 'DotDoc\.EntityFrameworkCore\.Extensions\.TableHints\.SqlServerTableHint\.ReadUncommitted') | READUNCOMMITTED\. |
| [RepeatableRead](SqlServerTableHint.RepeatableRead 'DotDoc\.EntityFrameworkCore\.Extensions\.TableHints\.SqlServerTableHint\.RepeatableRead') | REPEATABLEREAD\. |
| [RowLock](SqlServerTableHint.RowLock 'DotDoc\.EntityFrameworkCore\.Extensions\.TableHints\.SqlServerTableHint\.RowLock') | ROWLOCK\. |
| [Serializable](SqlServerTableHint.Serializable 'DotDoc\.EntityFrameworkCore\.Extensions\.TableHints\.SqlServerTableHint\.Serializable') | SERIALIZABLE\. |
| [Snapshot](SqlServerTableHint.Snapshot 'DotDoc\.EntityFrameworkCore\.Extensions\.TableHints\.SqlServerTableHint\.Snapshot') | SNAPSHOT\. |
| [TabLock](SqlServerTableHint.TabLock 'DotDoc\.EntityFrameworkCore\.Extensions\.TableHints\.SqlServerTableHint\.TabLock') | TABLOCK\. |
| [TabLockX](SqlServerTableHint.TabLockX 'DotDoc\.EntityFrameworkCore\.Extensions\.TableHints\.SqlServerTableHint\.TabLockX') | TABLOCKX\. |
| [Updlock](SqlServerTableHint.Updlock 'DotDoc\.EntityFrameworkCore\.Extensions\.TableHints\.SqlServerTableHint\.Updlock') | UPDLOCK\. |
| [XLock](SqlServerTableHint.XLock 'DotDoc\.EntityFrameworkCore\.Extensions\.TableHints\.SqlServerTableHint\.XLock') | XLOCK\. |

| Methods | |
| :--- | :--- |
| [ForceSeek\(\)](SqlServerTableHint.ForceSeek#DotDoc.EntityFrameworkCore.Extensions.TableHints.SqlServerTableHint.ForceSeek() 'DotDoc\.EntityFrameworkCore\.Extensions\.TableHints\.SqlServerTableHint\.ForceSeek\(\)') | FORCESEEK\[\( \< index\_value \> \( \< index\_column\_name \> \[ , \.\.\. \]\)\)\]\. |
| [ForceSeek\(string, IEnumerable&lt;string&gt;\)](SqlServerTableHint.ForceSeek#DotDoc.EntityFrameworkCore.Extensions.TableHints.SqlServerTableHint.ForceSeek(string,System.Collections.Generic.IEnumerable_string_) 'DotDoc\.EntityFrameworkCore\.Extensions\.TableHints\.SqlServerTableHint\.ForceSeek\(string, System\.Collections\.Generic\.IEnumerable\<string\>\)') | FORCESEEK\[\( \< index\_value \> \( \< index\_column\_name \> \[ , \.\.\. \]\)\)\]\. |
| [Index\(IEnumerable&lt;string&gt;\)](SqlServerTableHint.Index.4D539YQ7CZEP0XM79UPV0Z12A 'DotDoc\.EntityFrameworkCore\.Extensions\.TableHints\.SqlServerTableHint\.Index\(System\.Collections\.Generic\.IEnumerable\<string\>\)') | INDEX\( \<index\_value\> \[ , \.\.\.n\] \) \| INDEX = \( \<index\_value\> \)\. |
| [SpacialWindowMaxCells\(int\)](SqlServerTableHint.SpacialWindowMaxCells.NM6DDTY5HCGPU9LIVBGII7ZSA 'DotDoc\.EntityFrameworkCore\.Extensions\.TableHints\.SqlServerTableHint\.SpacialWindowMaxCells\(int\)') | SPATIAL\_WINDOW\_MAX\_CELLS = \<integer\_value\>\. |
| [ToString\(\)](SqlServerTableHint.ToString() 'DotDoc\.EntityFrameworkCore\.Extensions\.TableHints\.SqlServerTableHint\.ToString\(\)') | Convert a table hint to a string\. |
