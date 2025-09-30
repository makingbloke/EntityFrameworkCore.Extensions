#### [DotDoc\.EntityFrameworkCore\.Extensions](Home.md 'Home')
### [DotDoc\.EntityFrameworkCore\.Extensions\.TableHints](DotDoc.EntityFrameworkCore.Extensions.TableHints.md 'DotDoc\.EntityFrameworkCore\.Extensions\.TableHints')

## SqlServerTableHint Class

SQL Server Table Hints \(used to override default query optimiser behaviour\)\.

```csharp
public sealed class SqlServerTableHint : DotDoc.EntityFrameworkCore.Extensions.TableHints.ITableHint
```

Inheritance [System\.Object](https://learn.microsoft.com/en-us/dotnet/api/system.object 'System\.Object') &#129106; SqlServerTableHint

Implements [ITableHint](ITableHint.md 'DotDoc\.EntityFrameworkCore\.Extensions\.TableHints\.ITableHint')

| Fields | |
| :--- | :--- |
| [ForceScan](SqlServerTableHint.ForceScan.md 'DotDoc\.EntityFrameworkCore\.Extensions\.TableHints\.SqlServerTableHint\.ForceScan') | FORCESCAN\. |
| [HoldLock](SqlServerTableHint.HoldLock.md 'DotDoc\.EntityFrameworkCore\.Extensions\.TableHints\.SqlServerTableHint\.HoldLock') | HOLDLOCK\. |
| [NoExpand](SqlServerTableHint.NoExpand.md 'DotDoc\.EntityFrameworkCore\.Extensions\.TableHints\.SqlServerTableHint\.NoExpand') | NOEXPAND\. |
| [NoLock](SqlServerTableHint.NoLock.md 'DotDoc\.EntityFrameworkCore\.Extensions\.TableHints\.SqlServerTableHint\.NoLock') | NOLOCK\. |
| [NoWait](SqlServerTableHint.NoWait.md 'DotDoc\.EntityFrameworkCore\.Extensions\.TableHints\.SqlServerTableHint\.NoWait') | NOWAIT\. |
| [PagLock](SqlServerTableHint.PagLock.md 'DotDoc\.EntityFrameworkCore\.Extensions\.TableHints\.SqlServerTableHint\.PagLock') | PAGLOCK\. |
| [ReadCommitted](SqlServerTableHint.ReadCommitted.md 'DotDoc\.EntityFrameworkCore\.Extensions\.TableHints\.SqlServerTableHint\.ReadCommitted') | READCOMMITTED\. |
| [ReadCommittedLock](SqlServerTableHint.ReadCommittedLock.md 'DotDoc\.EntityFrameworkCore\.Extensions\.TableHints\.SqlServerTableHint\.ReadCommittedLock') | READCOMMITTEDLOCK\. |
| [ReadPast](SqlServerTableHint.ReadPast.md 'DotDoc\.EntityFrameworkCore\.Extensions\.TableHints\.SqlServerTableHint\.ReadPast') | READPAST\. |
| [ReadUncommitted](SqlServerTableHint.ReadUncommitted.md 'DotDoc\.EntityFrameworkCore\.Extensions\.TableHints\.SqlServerTableHint\.ReadUncommitted') | READUNCOMMITTED\. |
| [RepeatableRead](SqlServerTableHint.RepeatableRead.md 'DotDoc\.EntityFrameworkCore\.Extensions\.TableHints\.SqlServerTableHint\.RepeatableRead') | REPEATABLEREAD\. |
| [RowLock](SqlServerTableHint.RowLock.md 'DotDoc\.EntityFrameworkCore\.Extensions\.TableHints\.SqlServerTableHint\.RowLock') | ROWLOCK\. |
| [Serializable](SqlServerTableHint.Serializable.md 'DotDoc\.EntityFrameworkCore\.Extensions\.TableHints\.SqlServerTableHint\.Serializable') | SERIALIZABLE\. |
| [Snapshot](SqlServerTableHint.Snapshot.md 'DotDoc\.EntityFrameworkCore\.Extensions\.TableHints\.SqlServerTableHint\.Snapshot') | SNAPSHOT\. |
| [TabLock](SqlServerTableHint.TabLock.md 'DotDoc\.EntityFrameworkCore\.Extensions\.TableHints\.SqlServerTableHint\.TabLock') | TABLOCK\. |
| [TabLockX](SqlServerTableHint.TabLockX.md 'DotDoc\.EntityFrameworkCore\.Extensions\.TableHints\.SqlServerTableHint\.TabLockX') | TABLOCKX\. |
| [Updlock](SqlServerTableHint.Updlock.md 'DotDoc\.EntityFrameworkCore\.Extensions\.TableHints\.SqlServerTableHint\.Updlock') | UPDLOCK\. |
| [XLock](SqlServerTableHint.XLock.md 'DotDoc\.EntityFrameworkCore\.Extensions\.TableHints\.SqlServerTableHint\.XLock') | XLOCK\. |

| Methods | |
| :--- | :--- |
| [ForceSeek\(\)](SqlServerTableHint.ForceSeek.md#DotDoc.EntityFrameworkCore.Extensions.TableHints.SqlServerTableHint.ForceSeek() 'DotDoc\.EntityFrameworkCore\.Extensions\.TableHints\.SqlServerTableHint\.ForceSeek\(\)') | FORCESEEK\[\( \< index\_value \> \( \< index\_column\_name \> \[ , \.\.\. \]\)\)\]\. |
| [ForceSeek\(string, IEnumerable&lt;string&gt;\)](SqlServerTableHint.ForceSeek.md#DotDoc.EntityFrameworkCore.Extensions.TableHints.SqlServerTableHint.ForceSeek(string,System.Collections.Generic.IEnumerable_string_) 'DotDoc\.EntityFrameworkCore\.Extensions\.TableHints\.SqlServerTableHint\.ForceSeek\(string, System\.Collections\.Generic\.IEnumerable\<string\>\)') | FORCESEEK\[\( \< index\_value \> \( \< index\_column\_name \> \[ , \.\.\. \]\)\)\]\. |
| [Index\(IEnumerable&lt;string&gt;\)](SqlServerTableHint.Index.4D539YQ7CZEP0XM79UPV0Z12A.md 'DotDoc\.EntityFrameworkCore\.Extensions\.TableHints\.SqlServerTableHint\.Index\(System\.Collections\.Generic\.IEnumerable\<string\>\)') | INDEX\( \<index\_value\> \[ , \.\.\.n\] \) \| INDEX = \( \<index\_value\> \)\. |
| [SpacialWindowMaxCells\(int\)](SqlServerTableHint.SpacialWindowMaxCells.NM6DDTY5HCGPU9LIVBGII7ZSA.md 'DotDoc\.EntityFrameworkCore\.Extensions\.TableHints\.SqlServerTableHint\.SpacialWindowMaxCells\(int\)') | SPATIAL\_WINDOW\_MAX\_CELLS = \<integer\_value\>\. |
| [ToString\(\)](SqlServerTableHint.ToString().md 'DotDoc\.EntityFrameworkCore\.Extensions\.TableHints\.SqlServerTableHint\.ToString\(\)') | Convert a table hint to a string\. |
