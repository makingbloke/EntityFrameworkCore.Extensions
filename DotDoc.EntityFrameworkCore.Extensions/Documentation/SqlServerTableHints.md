#### [DotDoc\.EntityFrameworkCore\.Extensions](Home 'Home')
### [DotDoc\.EntityFrameworkCore\.Extensions\.TableHints](DotDoc.EntityFrameworkCore.Extensions.TableHints 'DotDoc\.EntityFrameworkCore\.Extensions\.TableHints')

## SqlServerTableHints Class

SQL Server Table Hints \(used to override default query optimiser behaviour\)\.

```csharp
public sealed class SqlServerTableHints : DotDoc.EntityFrameworkCore.Extensions.TableHints.ITableHint
```

Inheritance [System\.Object](https://learn.microsoft.com/en-us/dotnet/api/system.object 'System\.Object') → SqlServerTableHints

Implements [ITableHint](ITableHint 'DotDoc\.EntityFrameworkCore\.Extensions\.TableHints\.ITableHint')

| Fields | |
| :--- | :--- |
| [ForceScan](SqlServerTableHints.ForceScan 'DotDoc\.EntityFrameworkCore\.Extensions\.TableHints\.SqlServerTableHints\.ForceScan') | FORCESCAN\. |
| [HoldLock](SqlServerTableHints.HoldLock 'DotDoc\.EntityFrameworkCore\.Extensions\.TableHints\.SqlServerTableHints\.HoldLock') | HOLDLOCK\. |
| [NoExpand](SqlServerTableHints.NoExpand 'DotDoc\.EntityFrameworkCore\.Extensions\.TableHints\.SqlServerTableHints\.NoExpand') | NOEXPAND\. |
| [NoLock](SqlServerTableHints.NoLock 'DotDoc\.EntityFrameworkCore\.Extensions\.TableHints\.SqlServerTableHints\.NoLock') | NOLOCK\. |
| [NoWait](SqlServerTableHints.NoWait 'DotDoc\.EntityFrameworkCore\.Extensions\.TableHints\.SqlServerTableHints\.NoWait') | NOWAIT\. |
| [PagLock](SqlServerTableHints.PagLock 'DotDoc\.EntityFrameworkCore\.Extensions\.TableHints\.SqlServerTableHints\.PagLock') | PAGLOCK\. |
| [ReadCommitted](SqlServerTableHints.ReadCommitted 'DotDoc\.EntityFrameworkCore\.Extensions\.TableHints\.SqlServerTableHints\.ReadCommitted') | READCOMMITTED\. |
| [ReadCommittedLock](SqlServerTableHints.ReadCommittedLock 'DotDoc\.EntityFrameworkCore\.Extensions\.TableHints\.SqlServerTableHints\.ReadCommittedLock') | READCOMMITTEDLOCK\. |
| [ReadPast](SqlServerTableHints.ReadPast 'DotDoc\.EntityFrameworkCore\.Extensions\.TableHints\.SqlServerTableHints\.ReadPast') | READPAST\. |
| [ReadUncommitted](SqlServerTableHints.ReadUncommitted 'DotDoc\.EntityFrameworkCore\.Extensions\.TableHints\.SqlServerTableHints\.ReadUncommitted') | READUNCOMMITTED\. |
| [RepeatableRead](SqlServerTableHints.RepeatableRead 'DotDoc\.EntityFrameworkCore\.Extensions\.TableHints\.SqlServerTableHints\.RepeatableRead') | REPEATABLEREAD\. |
| [RowLock](SqlServerTableHints.RowLock 'DotDoc\.EntityFrameworkCore\.Extensions\.TableHints\.SqlServerTableHints\.RowLock') | ROWLOCK\. |
| [Serializable](SqlServerTableHints.Serializable 'DotDoc\.EntityFrameworkCore\.Extensions\.TableHints\.SqlServerTableHints\.Serializable') | SERIALIZABLE\. |
| [Snapshot](SqlServerTableHints.Snapshot 'DotDoc\.EntityFrameworkCore\.Extensions\.TableHints\.SqlServerTableHints\.Snapshot') | SNAPSHOT\. |
| [TabLock](SqlServerTableHints.TabLock 'DotDoc\.EntityFrameworkCore\.Extensions\.TableHints\.SqlServerTableHints\.TabLock') | TABLOCK\. |
| [TabLockX](SqlServerTableHints.TabLockX 'DotDoc\.EntityFrameworkCore\.Extensions\.TableHints\.SqlServerTableHints\.TabLockX') | TABLOCKX\. |
| [Updlock](SqlServerTableHints.Updlock 'DotDoc\.EntityFrameworkCore\.Extensions\.TableHints\.SqlServerTableHints\.Updlock') | UPDLOCK\. |
| [XLock](SqlServerTableHints.XLock 'DotDoc\.EntityFrameworkCore\.Extensions\.TableHints\.SqlServerTableHints\.XLock') | XLOCK\. |

| Methods | |
| :--- | :--- |
| [ForceSeek\(\)](SqlServerTableHints.ForceSeek#DotDoc.EntityFrameworkCore.Extensions.TableHints.SqlServerTableHints.ForceSeek() 'DotDoc\.EntityFrameworkCore\.Extensions\.TableHints\.SqlServerTableHints\.ForceSeek\(\)') | FORCESEEK\[\( \< index\_value \> \( \< index\_column\_name \> \[ , \.\.\. \]\)\)\]\. |
| [ForceSeek\(string, IEnumerable&lt;string&gt;\)](SqlServerTableHints.ForceSeek#DotDoc.EntityFrameworkCore.Extensions.TableHints.SqlServerTableHints.ForceSeek(string,System.Collections.Generic.IEnumerable_string_) 'DotDoc\.EntityFrameworkCore\.Extensions\.TableHints\.SqlServerTableHints\.ForceSeek\(string, System\.Collections\.Generic\.IEnumerable\<string\>\)') | FORCESEEK\[\( \< index\_value \> \( \< index\_column\_name \> \[ , \.\.\. \]\)\)\]\. |
| [Index\(IEnumerable&lt;string&gt;\)](SqlServerTableHints.Index.C0MPK4EUBLJE9PAEJQ30HMWBE 'DotDoc\.EntityFrameworkCore\.Extensions\.TableHints\.SqlServerTableHints\.Index\(System\.Collections\.Generic\.IEnumerable\<string\>\)') | INDEX\( \<index\_value\> \[ , \.\.\.n\] \) \| INDEX = \( \<index\_value\> \)\. |
| [SpacialWindowMaxCells\(int\)](SqlServerTableHints.SpacialWindowMaxCells.P5DNARXCPC5CBLWFPPV0HGHS8 'DotDoc\.EntityFrameworkCore\.Extensions\.TableHints\.SqlServerTableHints\.SpacialWindowMaxCells\(int\)') | SPATIAL\_WINDOW\_MAX\_CELLS = \<integer\_value\>\. |
| [ToString\(\)](SqlServerTableHints.ToString() 'DotDoc\.EntityFrameworkCore\.Extensions\.TableHints\.SqlServerTableHints\.ToString\(\)') | Convert a table hint to a string\. |
