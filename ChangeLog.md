### Version 7.0.0.0 - xxth November 2026

* ** Breaking Change ** Upgraded to .NET 11.
* Updated Nuget packages to latest versions.
* Fixed error in `ExecuteUpsert` methods where, an exception would be thrown if the method was used inside a transaction.
* Fixed issue where the `ExecuteUpdate` extension methods could run the wrong SQL, because EF Core cached the SQL generated for the first method called with the same query.
* The `ExecuteUpdate` extension methods now check if `UseExecuteUpdateExtensions()` has been called and will throw an `InvalidOperationException` if not.
* Fixed issue where `UseFreeTextSearchExtensions()` and `UseMatchExtensions()` stopped EF Core translating many other methods (for example `string.ToUpper`, `string.Substring` and `Math.Abs`, and on SQLite `string.Contains`), so queries using them threw an `InvalidOperationException` ("could not be translated"). Using both on SQLite also stopped either `FreeTextSearch` or `Match` working.
* Fixed issue on SQL Server where, if the connection had `SET NOCOUNT ON`, `ExecuteUpdateGetCountAsync`, `ExecuteDeleteGetCountAsync`, and EF Core's own `ExecuteUpdateAsync` / `ExecuteDeleteAsync` (when `UseExecuteUpdateExtensions()` or `UseTableHintExtensions()` had been called) returned -1 instead of the number of rows. This also stopped `ExecuteUpsertGetCountAsync` inserting a row when no rows matched; it returned -1 instead.
* ** Breaking Change ** `ExecuteDeleteGetRowsAsync`, `ExecuteInsertGetRowAsync`, `ExecuteUpdateGetRowsAsync` and `ExecuteUpsertGetRowsAsync` no longer support tables configured with `UseSqlReturningClause(false)` (SQLite) or `UseSqlOutputClause(false)` (SQL Server). They now throw a `NotSupportedException`. This was removed because the separate SELECT used to read the rows back without the clause was unreliable:
  * An update re-ran its `Where` clause to read the rows back. If the update changed a column in the `Where` clause, the rows no longer matched, so it returned the wrong rows or none. For example, `Where(e => e.Status == "Pending")` setting `Status` to `"Done"` returned nothing.
  * An update or delete with a join returned the joined tables' columns as well, giving duplicate column names.
  * An insert found the new row by the last identity value, so it failed (after the row had been inserted) if the primary key wasn't an identity column, e.g. a GUID.
  * Fixing these reliably needed more complex SQL for each database, such as temp tables on SQLite and `OUTPUT ... INTO` on SQL Server.
* Fixed issue on SQL Server where two `ExecuteUpsert` calls for the same row, running at the same time, could both insert a row (giving a duplicate row or a unique constraint error) instead of one inserting and the other updating.
* ** Breaking Change ** On SQL Server the `ExecuteUpsert` methods now throw a `NotSupportedException` if the query has table hints (`WithTableHints`).
* ** Breaking Change ** Renamed `SqlServerTableHints.Updlock` to `SqlServerTableHints.UpdLock`.
* ** Breaking Change ** Renamed `SqlServerTableHints.SpacialWindowMaxCells` to `SqlServerTableHints.SpatialWindowMaxCells`, correcting the spelling.
* ** Breaking Change ** Fixed issue on SQL Server where `WithTableHints` silently ignored the hints if `UseTableHintExtensions()` had not been called. It now throws an `InvalidOperationException`.
* ** Breaking Change ** Removed `UseUniqueConstraintInterceptor()`, `UniqueConstraintException`, `UniqueConstraintDetails` and `GetUniqueConstraintDetailsAsync`. They are replaced by a new `IsUniqueConstraintException()` extension method on `Exception` (in the `DotDoc.EntityFrameworkCore.Extensions.Utilities` namespace), which returns `true` if the exception, or one of its inner exceptions, is a unique constraint violation (such as a unique index, a unique constraint or a primary key). The old code was removed because:
  * The table and field names it returned were unreliable. It used the first entity type mapped to the table, so a unique index on a property of a derived type (table-per-hierarchy inheritance), or on a column that isn't in the EF Core model, threw an exception instead of returning the details.
  * SQLite error messages are always in English but, for SQL Server it matched English message text, so it didn't work if SQL Server was using another language.
* ** Breaking Change ** Split `UtilityExtensions` into separate classes named after what they do: `GuidExtensions` (`CreateGuid`), `DatabaseExistsExtensions` (`DoesDatabaseExistAsync`), `GetDbContextExtensions` (`GetDbContext`), `CaseInsensitiveCollationExtensions` (`UseCaseInsensitiveCollation`) and `DelimitedIdentifierExtensions` (`GetDelimitedIdentifier`). Apart from `CaseInsensitiveCollationExtensions` (below) they are still in the `DotDoc.EntityFrameworkCore.Extensions.Utilities` namespace, so code calling them as extension methods is unaffected; code calling them through the `UtilityExtensions` class name needs updating, and existing code must be recompiled.
* ** Breaking Change ** Moved `CaseInsensitiveCollationExtensions` (`UseCaseInsensitiveCollation`) to the `DotDoc.EntityFrameworkCore.Extensions.CaseInsensitivity` namespace, alongside `UseSqliteNoCaseReplacement`, as the two work together. Code calling it needs `using DotDoc.EntityFrameworkCore.Extensions.CaseInsensitivity;`.
* ** Breaking Change ** The SQLite NoCase replacement (`UseSqliteNoCaseReplacement`) is now case insensitive but accent sensitive, so it matches the SQL Server `Latin1_General_100_CI_AS_SC_UTF8` collation used by `UseCaseInsensitiveCollation`. Existing SQLite databases with indexes on NOCASE columns should run `REINDEX` once after upgrading.
* ** Breaking Change ** With `UseSqliteNoCaseReplacement()`, string methods on a SQLite `NOCASE` column (set by `UseCaseInsensitiveCollation()` or `UseCollation("NOCASE")`), or on `EF.Functions.Collate(..., "NOCASE")`, now ignore case, including the case of non-ASCII letters, as they do on SQL Server with `UseCaseInsensitiveCollation()`. Previously they used SQLite's built-in functions, which compare case sensitively, or ignore the case of ASCII letters only (`LIKE`). Columns without `NOCASE` are unchanged. `UseSqliteNoCaseReplacement()` registers the `nocase_*` functions on the connection, alongside SQLite's built-in ones, which are left alone (they can also be called from raw SQL):

  | Method | On a `NOCASE` column | Previously |
  | --- | --- | --- |
  | `Contains`, `IndexOf` (all overloads) | `nocase_instr` | `instr` (case sensitive) |
  | `StartsWith`, `EndsWith` | `nocase_startswith`, `nocase_endswith` | `LIKE` (ignores the case of ASCII letters only) |
  | `EF.Functions.Like` | `nocase_like` | `LIKE` (ignores the case of ASCII letters only) |
  | `Replace` | `nocase_replace` | `replace` (case sensitive) |
  | `Trim`, `TrimStart`, `TrimEnd` with characters | `nocase_trim`, `nocase_ltrim`, `nocase_rtrim` | `trim`, `ltrim`, `rtrim` (case sensitive) |
  | `Trim()`, `TrimStart()`, `TrimEnd()`, `Substring`, `ToLower()`, `ToUpper()` | SQLite's own function, but the result keeps the `NOCASE` collation | the result is compared as `BINARY` |

  The results of `Replace`, `Substring`, `ToLower()`, `ToUpper()` and the `Trim` methods on a `NOCASE` column, and of a string concatenation (`+` or `string.Concat`), `??` or conditional expression with a `NOCASE` operand, keep the `NOCASE` collation, as they do on SQL Server. So comparing a result (e.g. `e.Name.Trim() == "abc"` or `e.First + e.Last == "abc"`), sorting or grouping by it, or calling another of these methods on it, ignores case too.
* ** Breaking Change ** `UseSqliteNoCaseReplacement()` now also replaces SQLite's `lower` and `upper` functions with Unicode ones, so `ToLower()` and `ToUpper()` (and `lower()` and `upper()` in raw SQL) change non-ASCII letters (`é` / `É`) as they do on SQL Server, on every column. Previously only ASCII letters were changed.
* ** Breaking Change ** With `UseSqliteNoCaseReplacement()`, a value of a `NOCASE` column returned by a subquery inside a query (`First`, `FirstOrDefault`, `Single`, `SingleOrDefault`, `Last`, `LastOrDefault`, `ElementAt`, `ElementAtOrDefault`, `Min` or `Max`, e.g. `p.Children.Max(c => c.Name)`) now keeps the `NOCASE` collation, as it does on SQL Server. Comparing it, sorting or grouping by it, and calling `Contains`, `StartsWith` and the other string methods on it now ignore case. Previously they were case sensitive on SQLite.
* ** Breaking Change ** `UseSqliteNoCaseReplacement()` (both overloads) now throws an `InvalidOperationException` if .NET is running in globalization-invariant mode (`InvariantGlobalization` or `DOTNET_SYSTEM_GLOBALIZATION_INVARIANT`). Without ICU, text comparisons don't match SQL Server, and indexes on `NOCASE` columns would be stored in a different order from a database written with ICU. Its documentation now also says that every process writing to the database must use it, and that `REINDEX NOCASE` should be run after first using it on an existing database, and after an operating system, .NET or ICU upgrade or moving the database to another machine.
* ** Breaking Change ** `UseCaseInsensitiveCollation` now sets the SQL Server database collation to `Latin1_General_100_CI_AS_SC_UTF8` instead of `SQL_Latin1_General_CP1_CI_AS`. It is still case insensitive and accent sensitive, but:
  * it uses newer Unicode sorting rules, and handles more characters correctly;
  * `LEN`, `CHARINDEX`, `SUBSTRING` and the other string functions count a character outside the Basic Multilingual Plane (such as an emoji) as one character, as SQLite does, instead of two;
  * `varchar` columns are stored as UTF-8, so they can hold any Unicode character instead of only code page 1252. The length of a `varchar(n)` column is in bytes, so it may hold fewer than `n` non-ASCII characters;
  * it needs SQL Server 2019 or later.

  Changing a database's collation only changes the default for new columns. Existing columns keep `SQL_Latin1_General_CP1_CI_AS`, and comparing or joining an old column with a new one throws a collation conflict error, so existing databases should be regenerated, or have the collation of each string column changed. If the server's default collation is different, raw SQL that compares the database's columns with `#temp` table columns needs `COLLATE DATABASE_DEFAULT`.
* ** Breaking Change ** On SQLite, `UseCaseInsensitiveCollation` now only sets the `NOCASE` collation on string properties that don't already have a collation, so a property given its own collation (with `UseCollation`) before `UseCaseInsensitiveCollation` is called keeps it, as a column with its own collation does on SQL Server. Previously it was replaced with `NOCASE`. A collation set after `UseCaseInsensitiveCollation` is called still replaces `NOCASE`, as before.
* ** Breaking Change ** Fixed issue on SQLite where `UseCaseInsensitiveCollation` didn't set the `NOCASE` collation on the string properties of complex types (`ComplexProperty`), so comparing, sorting or grouping by them was case sensitive, unlike on SQL Server. It now sets it on them too, including nested complex types, but not on a complex type mapped to JSON (`ToJson`). Existing SQLite databases need a migration to change these columns' collation. With `UseSqliteNoCaseReplacement()`, string methods such as `Contains` and `StartsWith` on a `NOCASE` property of a complex type (set by `UseCaseInsensitiveCollation` or `UseCollation`) now ignore case, as they do on an entity type's own properties.
* Fixed issue where table hints may be lost if EF Core regenerates the SQL.
* Fixed issue in `SqlServerTableHints` where `SpatialWindowMaxCells` (previously `SpacialWindowMaxCells`) generated an invalid hint (with an extra closing bracket), and `Index` and `ForceSeek` did not escape a `]` in an index or column name.
* Fixed issue where `ExecuteScalarAsync` threw an `InvalidCastException` (or returned an empty string) instead of the default value if the query returned a database NULL.
* Fixed issue where `ExecuteScalarAsync`, `ExecuteInsertAsync` and the count query run by `ExecutePagedQueryAsync` were not seen by EF Core's command interceptors (`DbCommandInterceptor`), logging (`LogTo`, `ILoggerFactory`) or diagnostic events (used by tracing tools such as OpenTelemetry), and were not retried by the context's execution strategy (e.g. `EnableRetryOnFailure`) after a transient error, unlike `ExecuteNonQueryAsync` and `ExecuteQueryAsync`. As with EF Core's own `ExecuteSqlRawAsync`, if a retrying execution strategy is configured, calling them inside a transaction started with `BeginTransaction` now throws an `InvalidOperationException` unless the whole transaction is run inside the execution strategy (`CreateExecutionStrategy().ExecuteAsync(...)`).
* Fixed issue where `ExecuteScalarAsync` and `ExecuteInsertAsync` threw an `InvalidCastException` for a result of type `Guid`, `DateTimeOffset`, `TimeSpan`, `DateOnly`, `TimeOnly` or an enum (on SQLite, which returns some of these as text, and for `DateOnly` and `TimeOnly` on SQL Server). A value converter set with `DefaultTypeMapping` in `ConfigureConventions` is now also applied to the result.
* Fixed issue where the parameters of `ExecuteScalarAsync`, `ExecuteInsertAsync` and the count query run by `ExecutePagedQueryAsync` ignored model-wide configuration (`DefaultTypeMapping` in `ConfigureConventions`), which EF Core's own `ExecuteSqlRawAsync` applies. For example, with an enum configured to be stored as a string, the parameter was sent as the enum's number.
* Fixed issue where `UseSqliteNoCaseReplacement` had no effect on a connection that was already open when it was given to EF Core (e.g. `UseSqlite(connection)` with an open in-memory database, or `SetDbConnection`): `NOCASE`, `lower` and `upper` only handled ASCII letters, and `Contains`, `StartsWith` and the other string methods on a NOCASE column failed with "no such function: nocase_instr" (or another `nocase_` function). A connection that isn't a `SqliteConnection` (such as a profiling wrapper around one) now throws an `InvalidOperationException` on its first command, instead of `NOCASE` silently staying ASCII only.
* Added a `UseSqliteNoCaseReplacement()` extension method on `SqliteConnection`, which registers the same Unicode `NOCASE` collation, `lower` and `upper` functions and `nocase_*` functions on a connection used without EF Core, such as one opened directly with the same connection string as a `DbContext` that uses `UseSqliteNoCaseReplacement()` (see the connection pooling note on that method).
* Fixed issue where `IsUniqueConstraintException` returned `false` for an `AggregateException` (e.g. from `Task.Wait`, `Task.Result` or `Parallel.ForEach`) whose unique constraint violation wasn't its first inner exception. It now checks all of an `AggregateException`'s inner exceptions.
* Fixed issue where `ExecuteScalarAsync`, `ExecuteInsertAsync` and `ExecutePagedQueryAsync` checked for a second operation on the context at the same time even if thread safety checks were turned off with `EnableThreadSafetyChecks(false)`.
* Fixed issue where `ExecuteInsertAsync` returned 0 instead of the new Id if the SQL ended with a comment. SQL that doesn't contain a statement, or has an unclosed quote, comment or bracket, now throws an `ArgumentException`.
* Fixed issues where `ExecutePagedQueryAsync` could return the wrong record count or page, or fail with a database error, for queries over several lines, with a `WITH` clause, ending with a semicolon or a comment, with `ORDER BY` in a comment, string, quoted identifier, window function or subquery, or with an `OPTION` clause (SQL Server). `DISTINCT`, `GROUP BY` and `UNION` queries also gave the wrong record count. SQL that can't be paged (not a SELECT, already paged, or on SQL Server using `TOP`) now throws a `NotSupportedException` before anything is run.
* Fixed issue where `WithTableHints`, `SqlServerTableHints.Index` and `SqlServerTableHints.ForceSeek` read their hints, index names or column names more than once. A sequence that is evaluated each time it is read (such as a LINQ query) was evaluated several times, and could give the checks and the generated hint different values.
* `UseCustomQueryGenerator()`, `UseFreeTextSearchExtensions()` and `UseMatchExtensions()` now set the `ParamName` of the `UnsupportedDatabaseTypeException` they throw to `optionsBuilder`, as the other `Use` methods do.
* The `Match` function now tells EF Core that a NULL search text gives a NULL result, as `FreeTextSearch` on SQLite does.
* ** Breaking Change ** On SQL Server, `ExecuteInsertAsync` and `ExecuteInsertGetRowAsync` now throw an `InvalidOperationException` if the query has a `Take`, as they already did on SQLite. Previously SQL Server ran the insert as `INSERT TOP (n)`.
* Fixed issue on SQLite where `FreeTextSearch` with `useStemming` set to `true` threw an `InvalidOperationException` ("Stemming table not specified") if the query also used another shared-type entity type without a stemming table, such as a property bag.
* ** Breaking Change ** On SQLite, `FreeTextSearch` now decides whether to use stemming separately for each table in a query. Previously the `useStemming` value of the first `FreeTextSearch` call was used for the whole query (a documented limitation), and every table with a stemming table was swapped for it, even a table that wasn't searched. Now each use of a table (for example each side of a self join, a subquery, or each part of a union) follows its own first search, so a query that searches one table with stemming and another without may return different rows.
  * Why: SQLite can't stem words in an ordinary full text table, so a stemming search reads a second copy of the table built with the porter tokeniser instead. The library used to choose between the two tables by rewriting the LINQ query before EF Core translated it. At that point it can't tell which table a search is on (both tables use the same class), so it had to make one choice for the whole query. The choice is now made while the SQL is being written, after EF Core has translated the query, when each table and the searches on it are known.
  * The stemming table no longer needs to be in the model. A stemming table that doesn't exist now gives a `SqliteException` ("no such table") when the query runs, instead of an `InvalidOperationException` ("Stemming table ... not found").
  * Free text tables no longer need to be shared type entities: the table being searched can be an ordinary entity type. A shared type entity is only needed to have the stemming table in the model with the same class as the table being searched.
  * Later searches on the same use of a table still follow the first one. To search the same table with and without stemming, use the table twice in the query (see the ReadMe).
* Fixed issue on SQLite where `FreeTextSearch` with `useStemming` set to `true` threw an `InvalidOperationException` ("Stemming table not specified") when the column searched was declared on a derived type (table-per-hierarchy inheritance) with `SetStemmingTable` set on the base type, or was a property of an owned type (`OwnsOne`) or a complex type (`ComplexProperty`) stored in the same table. Setting different stemming tables on two types mapped to the same table now throws an `InvalidOperationException` naming the table.
* Fixed issue where `ExecuteDeleteGetRowsAsync`, `ExecuteInsertGetRowAsync`, `ExecuteUpdateGetRowsAsync` and `ExecuteUpsertGetRowsAsync` threw a `FormatException` ("Input string was not in a correct format") if the query had a `{` or `}` in a string constant (e.g. `Where(e => e.Name == "}{")`) or in a tag (`TagWith`). A constant such as `"{0}"` didn't throw but changed the query, so the wrong rows could be deleted or updated: `Where(e => e.Name.StartsWith("{0}") && e.Id > minId)` matched names starting with `@minId` instead.
* Fixed issue where `ExecuteDeleteGetRowsAsync`, `ExecuteInsertGetRowAsync`, `ExecuteUpdateGetRowsAsync` and `ExecuteUpsertGetRowsAsync` ignored the entity type's mapping when reading the rows they return. A property mapped to a column with a different name (`HasColumnName`) threw an `InvalidOperationException` ("The required column ... was not present"), a value converter (`HasConversion`) wasn't applied, and with table-per-hierarchy inheritance a row of a derived type was returned as the base type, losing the derived type's values. They also now work with shared type entities (such as property bags) and complex types (`ComplexProperty`). An entity type with an owned type (`OwnsOne` / `OwnsMany`), which EF Core can't read from the returned rows, now gives a `NotSupportedException` naming the owned type before anything is changed. So does an entity type in an inheritance hierarchy mapped with TPT or TPC (`UseTptMappingStrategy` / `UseTpcMappingStrategy`), whose rows EF Core can't read back either.
* Fixed issue where `ExecuteUpsertGetCountAsync` and `ExecuteUpsertGetRowsAsync` threw an `InvalidOperationException` ("The configured execution strategy ... does not support user-initiated transactions") if the DbContext used a retrying execution strategy, such as `EnableRetryOnFailure` on SQL Server, and had no transaction. The upsert now begins its transaction inside the context's execution strategy, so a transient error rolls the transaction back and runs the whole upsert again in a new one. If the DbContext already has a transaction, the upsert still runs in it and retrying is up to the caller.
* Fixed issue on SQL Server where `ExecuteUpsertGetCountAsync` and `ExecuteUpsertGetRowsAsync` threw an `InvalidOperationException` ("An ambient transaction has been detected..." or "The connection is currently enlisted in a transaction...") inside a `TransactionScope`, or when the connection had been enlisted in a transaction with `EnlistTransaction`, because they tried to begin a transaction of their own. They now run in that transaction, as they already did in one begun with `BeginTransaction`. (EF Core's SQLite provider doesn't support `System.Transactions` transactions.)
* Fixed issue where `ExecuteInsertAsync` and `ExecuteInsertGetRowAsync` threw an `InvalidOperationException` if the entity type had a global query filter (`HasQueryFilter`), as EF Core added the filter to the query as a `Where`, which an insert can't have. `ExecuteUpsertGetCountAsync` and `ExecuteUpsertGetRowsAsync` threw it too whenever they had to insert. The inserts now ignore global query filters. The upsert's update still uses them, so a row hidden by a filter (e.g. a soft deleted row) isn't updated, and a new row is inserted instead.
* Fixed issue where `ExecutePagedQueryAsync` ran a statement starting with a `WITH` clause and followed by an `INSERT ... SELECT` (e.g. `WITH c AS (...) INSERT INTO T (F) SELECT F FROM c`) twice, once for the count and once for the page, inserting rows each time, before throwing. It now checks the `WITH` clause's syntax and throws a `NotSupportedException` ("Only SELECT queries can be paged") before running anything if the statement after the common table expressions isn't a SELECT, or if the `WITH` clause isn't valid common table expression syntax, as it already did for an `INSERT` without a `WITH`. Paging a SELECT with a `WITH` clause still works.
* Fixed issue where `ExecutePagedQueryAsync` threw an `ArgumentException` ("The SQL must contain a single statement") if the SQL started with a semicolon, such as the common T-SQL `;WITH cte AS (...) SELECT ...`.
* Fixed issue where `ExecutePagedQueryAsync` returned a `PageCount` of 0 or -1 instead of 1 if the page size was close to `long.MaxValue` (e.g. `long.MaxValue` to return every row on one page).
* Fixed issue on SQL Server where `ExecutePagedQueryAsync` threw a `SqlException` ("ORDER BY items must appear in the select list...") for a `UNION`, `UNION ALL`, `INTERSECT`, `EXCEPT` or `SELECT DISTINCT` query without an `ORDER BY`, because the `ORDER BY (SELECT NULL)` added so that `OFFSET` / `FETCH` can be used isn't allowed on those queries. A query without an `ORDER BY` is now wrapped in a derived table (as the count query already was), and the `ORDER BY` is added to that. SQLite wasn't affected.
* ** Breaking Change ** Fixed issue on SQL Server where `WithTableHints` added its hints to every table in the query, including joined, included (`Include`) and subquery tables, so a hint for one table, such as `SqlServerTableHints.Index(...)`, `ForceSeek(index, columns)` or `NoExpand`, made any query with another table fail (e.g. "Index 'PK_Blogs' on table 'Posts' ... does not exist"). It also meant the `ExecuteUpsert` methods' own `HoldLock` / `UpdLock` hints locked the tables in a subquery in the `Where` clause until the transaction ended. `WithTableHints` now takes a builder that names the entity type each table's hints are for, and adds them only to that entity type's table (or view), e.g. `query.WithTableHints(h => h.For<Blog>(SqlServerTableHints.Index("PK_Blogs")).For<Post>(SqlServerTableHints.NoLock))`. The `ExecuteUpsert` methods' lock hints are now only added to the table being upserted. To upgrade:
  * Replace `WithTableHints(hints)` with `WithTableHints(h => h.For<TEntity>(hints))`, where `TEntity` is the query's entity type.
  * A hint that should also apply to joined, included or subquery tables (such as `NoLock`) must now be given for each of their entity types too.
  * `WithTableHints` can now be called more than once in a query (e.g. on both queries of a join), as long as each table is only given one set of hints. Different hints for the same table throw an `InvalidOperationException` when the query is run.
  * `For` throws an `ArgumentException` for a type that isn't an entity type in the model, a shared type entity type, or an entity type that isn't mapped to a table or view. The hints are added to every use of the entity type's table in the query (both sides of a self join, for example).
* Reduced the overhead `UseExecuteUpdateExtensions()` added to every command that isn't an `ExecuteUpdate` or `ExecuteDelete`, such as migrations and `ExecuteSqlRaw`.
* With `UseSqliteNoCaseReplacement()`, `EF.Functions.Like` with an escape character on a `NOCASE` column no longer allocates memory for every row (previously about 270 KB a row for a pattern such as `%a!_b%` against a 2,000 character value).
* `ExecuteInsertAsync` and `ExecuteInsertGetRowAsync` called on a query with a `Where`, `OrderBy`, `Take` or any other query operator now throw an `InvalidOperationException` saying that they must be called on the table itself. Previously the message said the query used a feature the SQL generator didn't support, and that this was a bug in the EF Core provider. Corrected their `source` and `setPropertyCalls` parameter documentation, which had been copied from `ExecuteUpdate`.
* ** Breaking Change ** `DoesDatabaseExistAsync` now throws an `ArgumentException` if the database provider isn't relational (such as the EF Core in-memory provider), as `GetDelimitedIdentifier` does. Previously it threw an `InvalidOperationException` that wasn't documented.
* Fixed issue on SQLite where `ExecuteInsertAsync` returned the Id of an earlier insert (possibly into another table, or by another context using the same pooled connection) if its SQL inserted nothing, such as an `INSERT OR IGNORE` that hit a duplicate. It now returns the default value, as it already did on SQL Server. Its documentation now also says it can only return an identity column (SQL Server) or rowid (SQLite) value, not another kind of key such as a GUID, and what to use instead, and that on SQLite the value isn't reliable for an `INSERT ... ON CONFLICT ... DO UPDATE` that updates an existing row, or for an insert into a `WITHOUT ROWID` table, which still return the earlier insert's Id (use `RETURNING` with `ExecuteScalarAsync` for these).
* Fixed issue where `ExecuteInsertAsync`, `ExecuteInsertGetRowAsync` and the `ExecuteUpsert` methods didn't work with table-per-hierarchy inheritance. The insert didn't write the discriminator, so it failed with a NOT NULL constraint error unless the caller set the discriminator's shadow property themselves, and on a derived type (e.g. `context.Set<Dog>()`) the inserts always failed, and the upserts failed whenever they had to insert. The discriminator is now written with the entity type's value, as `SaveChanges` does, unless the setters set it. Inserting a row of an abstract entity type now throws an `InvalidOperationException` before anything is run.
* Minor code tidy-ups: replaced the remaining uses of `var` with explicit types.
* Ensured all source files use UTF8 encoding with a BOM. Updated the .editorconfig file.
* Altered async tests to use a Cancellation Token from the Test Context rather than CancellationToken.None.
* Each test's SQLite database is now a file in the test's own temporary directory (`TestContext.TestTempDirectory`), which MSTest deletes after a test that passes and keeps after a failure, instead of one shared file in the test output directory. The `DatabaseUtils` test helpers now also pass the test's cancellation token to every call that takes one.
* Removed the `TestDataSourceOptions` setting from the test project reverting the test output format back to the MSTest default.
* Added tests for error handling, empty results and other cases that were not covered.
* Renamed the tests from numbers (e.g. `ExecuteDeleteGetCountTests_002_Async`) to names that say what they test, in the form `Member_Scenario_ExpectedResult` (e.g. `ExecuteDeleteGetCountAsync_Where_DeletesOnlyMatchingRowsAndReturnsCount`), so they can be read in the test runner. Each data row's display name now starts with the test's scenario.

### Version 6.0.0.16 - 08th September 2026

* Upgraded to the latest version of EF Core.

### Version 6.0.0.15 - 08th September 2026

* Corrected error the `NonRelationalDatabaseCheck` in UtilityExtensions was exposed as public when, it should have been private.

### Version 6.0.0.14 - 08th September 2026

* Updated Nuget packages to latest versions.
* Added `GetDelimitedIdentifier` methods to UtilityExtensions. These get the database specific delimited version of an object name e.g. [Table] or [dbo].[Table] for SQL server.

### Version 6.0.0.13 - 29th July 2026

* Fixed an issue where values stored in AsyncLocal were not being cleared between methods, so were being passed to the next query / execute. TableHints now passes the hints to the query generator using a custom tag. The ExecuteUpdate extensions still use AsyncLocal but clear it once the method has completed.
* Switched to using Central Package Management (CPM) for managing Nuget packages (see Directory.Packages.props) and shared project properties are now in Directory.Build.props.
* ** Breaking Change ** Renamed SqlServerTableHint to SqlServerTableHints. This makes the naming consistent with the rest of the project.

### Version 6.0.0.12 - 22nd June 2026

* Updated Nuget packages to latest versions.
* Added reference to SQLitePCLRaw.bundle_e_sqlite3 as a temporary fix for vulnerabilities in SQLitePCLRaw.core (until there is an update to Microsoft.Data.Sqlite).
* Fixed potential SQL Injection in SqlServerExceptionProcessor.
 
### Version 6.0.0.11 - 27th March 2026

* Updated Nuget packages to latest versions.
* A host of minor fixes and improvements following a code review.
* PagedQueryResult is now a `record class`.
* `ExecutePagedQueryAsync` now has a retry guard to prevent an infinite loop in rare circumstances.
* Replaced `DbContextOptionsBuilder` extension method `GetDatabaseType` with version that takes a `DbContextOptions` parameter as this can be used in more places (use `optionsBuilder.Options.GetDatabaseType()` as a replacement).

### Version 6.0.0.10 - 03rd March 2026

* Updated Nuget packages to latest versions (Did not update default documentation package as new version gave error when compiling source).

### Version 6.0.0.9 - 16th February 2026

* Upgraded Nuget packages to latest versions (.Net 10, Test, EF Core etc.).
* Updated copyright date to 2026.
 
### Version 6.0.0.8 - 05th December 2025

* Upgraded Nuget packages to release versions (.Net 10, Test, EF Core etc.).

### Version 6.0.0.7 - 09th November 2025

* Added a new test to `CreateGuidTests` which uses multiple threads to generate a large number of entries with a GUID key. It then reads them back to check they are in sequential order.
* Added new `ExecuteUpsertGetCount` and `ExecuteUpsertGetRows` methods. These work like the existing `ExecuteUpdateGetCount` and `ExecuteUpdateGetRows` methods but will insert a new record if no matching record is found.

### Version 6.0.0.6 - 30th October 2025

* Replaced FromSqlRaw with SqlQueryRaw (undoing change introduced in 6.0.0,4) as FromSqlRaw could not support shared entities without extra logic.

### Version 6.0.0.5 - 29th October 2025

* Added new utility extension method `CreateGuid`. SQL Server can store GUIDs but because of how they are handled Version 7 GUIDs are not sorted in the correct order (V7 GUIDs contain a timestamp). This method uses a third party library (UUIDNext) to create GUIDS that are compatible with the supported database types.
* Removed unused constants.
* Merged changes from 6.0.0.2 and 6.0.0.4 into master.

### Version 6.0.0.4 - 29th October 2025

* Experimental pre-release version. Tidied up code in ExecuteUpdate so methods use `FromSqlRaw`.

### Version 6.0.0.3 - 29th October 2025

* Release abandoned.

### Version 6.0.0.2 - 05th October 2025

* Experimental pre-release version. Changed the code to use AsyncLocal fields to pass information to the Custom Query Generators and Interceptors instead of using Tags (This is because I found an issue with EF Core 10 where Tags were being lost in certain conditions).
* Added new ExecuteInsert method that will insert a record with no caching of the values locally.

### Version 6.0.0.1 - 02nd October 2025

* Fixed issue with method `CustomQueryGeneratorExtensions.GetEntityType` where it was not finding the entity type if the entity was a shared type.

### Version 6.0.0.0 - 01st October 2025

* **Breaking Change** Upgraded solution to .Net 10, Entity Framework Core to 10 and MSTest Nuget packages.
* **Breaking Change** Carried on work refactoring code so it is grouped into namespaces based on functionality. Single utility methods are now in the Utilities/UtilityExtensions class.
* Added custom SQL generators. These allow for customisation of the SQL being generated such as adding table hints and `OUTPUT/RETURNING` clauses.
* **Breaking Change** Merged and rewrote the code of the `ExecuteUpdateGetRows` and `ExecuteDeleteRows` methods. These now use the Custom SQL Generators.
* Added a new `ExecuteInsertGetRow` method (as part of the `ExecuteUpdateGetRows` work) that inserts a record and returns it.
* Added `TableHint` extensions which provide support for adding SQL Server table hints to LINQ queries.
* **Breaking Change** Where a generic Type is being returned from a method, corrected the null handling so nullable and non-nullable types can be used.
* **Breaking Change** Removed `Task<long> ExecuteInsertAsync`.
* **Breaking Change** Removed `Task<PageResultTable> ExecutePagedQueryAsync` and renamed `PageResultEntity` to `PagedQueryResult`.
* **Breaking Change** Removed `Task<DataTable> ExecuteQueryAsync`.
* Refactored and simplifed the code used in the `Match` and `FreeTextSearch` methods.
* **Breaking Change** For consistency, renamed `UseFreeTextExtensions` to `UseFreeTextSearchExtensions`.
* Renamed the `<TEntity>` generic parameter to `<TSource>` for consistency with EF Core.
* Renamed the `IQueryable<TEntity> query` parameter in extension methods to `source` for consistency with EF Core.
* Refactored `UniqueConstraint` code to use an interface for exception processors rather than having a base class.
* Added new code to tests and refactored existing code to simplify it by removing the use of the `[DynamicData]` attribute.
* Added new Wiki API documentation generated from code comments.

### Version 5.0.1.0 - 13th August 2025

* Added `ExecuteDeleteGetRows` and `ExecuteDeleteGetCount` These work like their `ExecuteUpdate` counterparts but delete records instead of updating them.  
* Added missing `cancellationToken` parameter to `GetUniqueConstraintDetailsAsync`.  
* Upgraded SonarAnalyzer, EF Core and MSTest Nuget packages.  
* Fixed various style checker errors in tests.  

### Version 5.0.0.2 - 11th June 2025

* Upgraded SonarAnalyzer, EF Core and MSTest Nuget packages.

### Version 5.0.0.1 - 18th May 2025

* Upgraded solution to use new XML based .slnx format.

### Version 5.0.0.0 - 16th May 2025

* **Breaking Change** Refactored code so it is grouped into namespaces based on functionality.
* **Breaking Change** Removed Synchronous methods where an asynchronous alternative exists.
* **Breaking Change** The project now needs the full version of EF core.
* **Breaking Change** Removed ``string GetDatabaseType(string providerName)`` as this is no longer needed, use one of the other overloads. 
* `ExecuteUpdateGetRows` now alters how it obtains its output depending on the value in the `UseSqlReturningClause` / `UseSqlOutputClause` model builder options.
* Re-instated the `GetDbContext` extensions as the IQueryable variant is needed by `ExecuteUpdateGetRows`.
* When testing with SQLite changed the tests to use a physical rather than an in memory database. 
* Upgraded SonarAnalyzer, EF Core and MSTest Nuget packages.
* Refactored update setters and get DbContext to make Reflection code more efficient.
* Added new `Match` and `FreeTextSearch` DbFunction extensions to allow for free text searching in LINQ queries. 

### Version 4.0.1.1 - 03rd March 2025

* Upgraded SonarAnalyzer, EF Core and MSTest Nuget packages.

### Version 4.0.1.0 - 10 February 2025

* Added new Case Insensitivity Extensions which provide an upgrade to SQLite's NoCase collation and methods to set a case insensitive default collation sequence.
* Updated the MSTest Nuget package.
* More Test refactoring and tidying.

### Version 4.0.0.1 - 04 February 2025

* Fixed issue where `ExecuteUpdateGetRows` was not finding any rows to update due to an issue with parameter types when run against SQLite.

### Version 4.0.0.0 - 31st January 2025

* Updated copyright date to 2025.
* Upgraded SonarAnalyzer, EF Core and MSTest Nuget packages.
* **Breaking Change** Moved extensions into Extension namespace and general code into Classes.
* **Breaking Change** Renamed UniqueConstraintInterceptor to UniqueConstraintSaveChangesInterceptor.
* **Breaking Change** Change the DatabaseType to a string rather than an enum (This makes it easier to pass values about with needing a dependency on EntityFrameworkCore.Extensions).
* General source code tidying including standardising regions.
* Replaced Lambda methods with block methods for consistency and ease of adding guard clauses later. 
* **Breaking Change** Replaced `params object[]` parameter in methods with `params IEnumerable<object>`. Removed the existing query methods that took `IQueryable<object>` as the parameters collection as they are now redundant.
* Added new `ExecuteInsert` methods that take a generic parameter and return a key of this type (Kept the existing methods that return a `long` for convienience).
* **Breaking Change** Renamed `ExecuteUpdate` to `ExecuteUpdateGetCount` and added new `ExecuteUpdateGetRows` methods which return the rows updated instead of a count.
* Switched from using a .ruleset file for source analysis to .editorconfig.
* Added new `ExecuteQuery` methods that take a generic entity parameter and return a list of the entities.
* Added new `ExecutePagedQuery` methods that take a generic entity parameter and return a list of the entities in the QueryPage (now PageResult*) object.
* Made generic parameter names in source code consistent:  
 `<T>` = General type.  
 `<TEntity>` = Entity type.  
 `<TProperty>` = Property type (used in execute update code only).  
* Added guard clauses to public methods.
* **Breaking Change** Removed TableExists methods as these did not support schemas and can be easily replicated in standard EF Core.
* Enabled nullable reference types in both projects and corrected any issues associated with this.
* **Breaking Change** Removed `DbContext GetContext<TEntity>(this IQueryable<TEntity> query)` as it required a reflection hack which made it unsupportable between EF versions.
* **Breaking Change** Altered `GetUniqueConstraintDetails` and `GetUniqueConstraintDetailsAsync` to take the database facade as their first parameter.
* Refactored tests to make the results easier to read.

### Version 3.0.2.1 - 08th January 2025

* Fixed issue with GetContext() where it only worked for `DbSet<>` objects and not `IQueryable<>`.
* Upgraded SonarAnalyzer.

### Version 3.0.2.0 - 13th November 2024

* Upgraded solution to .Net 9, Entity Framework Core to 9 and MSTest Nuget packages.

### Version 3.0.1.0 - 05th October 2024

* Upgraded solution to .Net 8, Entity Framework Core to 8.0.8, SonarAnalyzer, StyleCop and MSTest Nuget packages.
* Fixed style checker errors.
* Split test methods into sync and async tests rather than using one method. 
* Made test names more logical.

### Version 3.0.0.0 - 4th June 2023

* Removed Interpolated and Raw suffixes from query method names. For example ExecuteScalarRaw and ExecuteScalarInterpolated are both now known as ExecuteScalar and the compiler chooses the correct method to use based on the parameters (method overloading).
* Upgraded SonarAnalyzer and MSTest Nuget packages.
* Tidied up projects and style checker settings.

### Version 2.0.1.0 - 27th April 2023

* Added new methods to the Execute SQL Extensions that take an `IEnumerable<object>` as a parameter instead of `params object[]`.
* Added new tests for Execute SQL Extensions that take an `IEnumerable<object>`.
* Refactored and tidied up existing test code to make it more consistent.
* Upgraded Entity Framework Core to version 7.0.5 and SonarAnalyzer packages.

### Version 2.0.0.4 - 18th March 2023

* Upgraded Entity Framework Core to version 7.0.4, SonarAnalyzer and MSTest Nuget packages.

### Version 2.0.0.4 - 17th January 2023

* Remove unnecessary generic constraint from GetContext.

### Version 2.0.0.3 - 16th January 2023

* Upgraded Entity Framework Core to version 7.0.2, SonarAnalyzer and MSTest Nuget packages.
* Added new GetContext extension to get the DbContext from an IQueryable<T> object.

### Version 2.0.0.2 - 20th November 2022

* Standardised and removed superfluous message from UniqueConstraintException.

### Version 2.0.0.1 - 12th November 2022

* Added missing cancellationToken parameter to ExecuteUpdateAsync.

### Version 2.0.0.0 - 11th November 2022

* Upgraded solution to .Net 7, Entity Framework Core to 7.0.0.
* Upgraded SonarAnalyzer.
* Added ExecuteUpdate extensions.

### Version 1.0.4.2 - 17th October 2022

* Standardised exception message in UniqueConstraintException.

### Version 1.0.4.1 - 17th October 2022

* Corrected namespace issue that broke existing clients.

### Version 1.0.4.0 - 17th October 2022

* Upgraded Entity Framework Core to version 6.0.10.
* Added interceptor and extensions to capture and process unique constraint exceptions.

### Version 1.0.3.1 - 19th September 2022

* Upgraded Entity Framework Core to version 6.0.9, SonarAnalyzer and MSTest Nuget packages.

### Version 1.0.3.0 - 29th May 2022

* Added extension methods to check the existance of a specified database or table.
* Refactored the private query methods held in QueryExtensions into a new class called QueryMethods and made them internal so they can be called from other extension classes in the project.

### Version 1.0.2.6 - 24th May 2022

* Upgraded Entity Framework Core to version 6.0.5, SonarAnalyzer and MSTest Nuget packages.
* Changed namespaces to be file scoped.

### Version 1.0.2.5 - 7th May 2022

* Upgraded SonarAnalyzer and MSTest Nuget packages.
* Minor code change in QueryExtensions LimitQuery to remove duplicated code.

### Version 1.0.2.4 - 21st April 2022

* Upgraded Entity Framework Core to version 6.0.4, SonarAnalyzer and MSTest Nuget packages.
* Changed project from using <Version> to using <VersionPrefix> for compatibility with new internal packaging script.

### Version 1.0.2.3 - 15th March 2022

* Upgraded Entity Framework Core to version 6.0.3 and SonarAnalyzer Nuget packages.

### Version 1.0.2.2 - 1st March 2022

* Upgraded SonarAnalyzer.CSharp package and tidied up properties in project files.

### Version 1.0.2.1 - 13th February 2022

* Corrected typo in package company name.

### Version 1.0.2.0 - 12th February 2022

* Upgraded Entity Framework Core to version 6.0.2 and Nuget packages used by the tests to the latest versions
* Added Style Checkers and rules
* Turned on Treat Warnings as Errors
* Fixed any errors raised by the Style Checkers

### Version 1.0.1.3 - 27th January 2022

Added GetDatabaseType extension method for MigrationBuilder objects.

### Version 1.0.1.2 - 16th January 2022

* Minor code refactoring.

### Version 1.0.1.1 - 15th January 2022

* Upgraded project so it builds a Nuget package.

### Version 1.0.1.0 - 1st January 2022

* Added support for SQL Server.

### Version 1.0.0.8 - 19th December 2021

* Upgraded Entity Framework Core to version 6.0.1
 
### Version 1.0.0.7 - 28th November 2021

* Upgraded solution to .Net 6, Entity Framework Core to 6.0.0 and Nuget packages used by the tests to the latest versions

### Version 1.0.0.6 - 12th October 2021

* Upgraded Entity Framework Core to version 5.0.11 and Nuget packages used by the tests to the latest versions

### Version 1.0.0.5 - 30th August 2021

* Upgraded Entity Framework Core to version 5.0.9 and Nuget packages used by the tests to the latest versions

### Version 1.0.0.4 - 25th July 2021

* Upgraded Entity Framework Core to version 5.0.8 and Nuget packages used by the tests to the latest versions

### Version 1.0.0.3 - 15th June 2021

* Upgraded Entity Framework Core to version 5.0.7 and Nuget packages used by the tests to the latest versions

### Version 1.0.0.2 - 12th May 2021 

* Upgraded Entity Framework Core to version 5.0.6

### Version 1.0.0.1 - 22nd April 2021 

* Upgraded Nuget packages.

### Version 1.0.0.0 - 05th April 2021 

* Initial release.
