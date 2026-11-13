### TODO - Code review findings (October 2026)

Items are in priority order. "Reproduced" means a temporary test showed the problem against SQLite; the others were found by reading the code.

**Before release:** none of the SQL Server tests have been run since the upsert transaction fix (44e0d49) and the SQLite NOCASE change, because no SQL Server instance was available. Run the full suite against SQL Server first.

#### 4. `UniqueConstraintInterceptor` problems (not implemented)

##### Problems

* `SaveChangesFailed` blocks on `.Result`. On SQL Server this can run a `sys.indexes` query, so it can deadlock under a synchronisation context.
* If the lookup throws (broken connection, no permission on `sys.*`, doomed transaction under `XACT_ABORT ON`), that exception replaces the original `DbUpdateException`.
* `SqliteExceptionProcessor.cs` uses `properties.First(...)`, which throws if a column doesn't map to a property.
* SQL Server only detects error 2601 (unique index). Error 2627 (primary keys, unique constraints, and EF alternate keys from `HasAlternateKey`) has a different message and comes through as a plain `DbUpdateException`.
* Both processors match English message text only.

##### Proposed implementation (November 2026)

Not implemented. A trial implementation along these lines was written and built with no warnings, and its SQLite tests passed (the SQL Server tests were not run), but it was undone pending a decision on the open questions below.

* **Detection by error code**, then parse the message for names only:
  * SQL Server: any `SqlError` in the `SqlException` with `Number` 2601 or 2627 (a `SqlException` can contain several errors, e.g. 2627 then 3621 "The statement has been terminated").
  * SQLite: `SqliteException.SqliteExtendedErrorCode` 2067 (`SQLITE_CONSTRAINT_UNIQUE`) or 1555 (`SQLITE_CONSTRAINT_PRIMARYKEY`). SQLite messages are always English, so the English-only problem is really SQL Server only.
  * The exception is found by walking the inner exceptions (`UniqueConstraintModel.FindException<T>`) rather than `GetBaseException()`.
* **SQL Server 2627:** a second regex for `Violation of (PRIMARY|UNIQUE) KEY constraint 'name'. Cannot insert duplicate key in object 'schema.table'`. No `sys.key_constraints` lookup is needed: a PRIMARY KEY or UNIQUE constraint creates an index with the same name, so the existing `sys.indexes` query works for both errors.
* **Lookup order** (the same for the sync and async methods): the exception / message, then the EF model, then the database (SQL Server only).
  * EF model: checks `GetKeys()` (primary and alternate keys, by `GetName`) as well as `GetIndexes()` (by `GetDatabaseName`). Entity matching uses `(et.GetSchema() ?? "dbo") == schema`, so it also works when the model has a default schema.
  * Database: the existing `sys.indexes` query, with a sync version (`SqlQuery(...).ToList()`) and an async version (`ToListAsync`).
  * If the index is in neither the model nor the database, the details have the table from the message and empty `FieldNames`.
  * If the message can't be parsed (e.g. SQL Server not using English), the table comes from `DbUpdateException.Entries[0].Metadata` and `FieldNames` is empty. (Guessing the fields from "the only unique index or key" was rejected - every entity has a primary key, so it would report the wrong fields.)
  * Schema: `null` for the default schema (`dbo`) whichever way the details are found, the same as in EF Core (changed in November 2026; previously the EF model path returned "dbo").
* **Synchronous path:** `IExceptionProcessor` has `GetUniqueConstraintDetails(DatabaseFacade, Exception)` alongside `GetUniqueConstraintDetailsAsync(DatabaseFacade, Exception, CancellationToken)` - same parameters. New public `GetUniqueConstraintDetails(this DatabaseFacade, Exception)` extension; `SaveChangesFailed` uses it instead of blocking on `.Result`.
* **Lookup failures** (`UniqueConstraintInterceptor`): the lookup is wrapped in try/catch and on failure `details` is set to `null`, so EF throws the original exception.
  * Async: `catch (Exception ex) when (!(ex is OperationCanceledException && cancellationToken.IsCancellationRequested))` - a cancellation of the caller's token is passed on; any other cancellation (e.g. a command timeout) is treated as a failed lookup. The token's state is checked rather than `ex.CancellationToken`, because EF Core / SqlClient can link the caller's token with their own.
  * Sync: no token, so any exception from the lookup leaves the original exception to be thrown.
* **SQLite:**
  * `First(...)` replaced with `FirstOrDefault(...)`; an unmapped column keeps its column name.
  * New regex for the expression index message form `UNIQUE constraint failed: index 'Name'`: looked up by index / key name in the EF model, otherwise the `Entries` fallback (EF can't model expression indexes, so in practice it is always the fallback).
  * The database is never queried.
* **`UniqueConstraintException`** now has a message, e.g. "Unique constraint violation on dbo.Customer (Email)." (previously the default "Exception of type ... was thrown").
* **New file** `UniqueConstraint/UniqueConstraintModel.cs`: `FindException<T>`, `FindIndexOrKeyPropertyNames`, `GetDetailsFromEntries`.
* **Docs:** XML docs on all members; generated docs updated (new page for the sync `GetUniqueConstraintDetails`; `UniqueConstraintDetails.FieldNames` documents when it is empty).
* **Tests added:**
  * `GetUniqueConstraintDetailsTests` 006-015: sync null arguments, sync with EF and SQL-only tables, primary key violation (`GuidTable`), alternate key violation (`HasAlternateKey`), SQLite expression index, non-unique violations (NOT NULL and an ordinary exception, sync and async), cancelled token on SQL Server. The SQL-only table setup was moved into a `CreateTestTable3ViolationAsync` helper (also used by 005).
  * `UniqueConstraintInterceptorTests` 004-005: sync and async with a unique index that isn't in EF (on SQL Server this uses the database lookup); checks the details, message and inner exception.
  * `UniqueConstraintExceptionTests` 003: message format.
  * Not tested: the interceptor's catch block (no reliable way to make the database lookup fail in a test).

##### Open questions

* **Sync vs async in the processors:** `SqliteExceptionProcessor.GetUniqueConstraintDetailsAsync` returns `Task.FromResult(GetUniqueConstraintDetails(...))` (the sync method wrapped in a completed task - no `Task.Run`). The SQL Server processor has separate sync and async methods that duplicate the lookup order. Decide how the sync / async versions should be structured.
* When implemented, run the SQL Server tests (2627, sync database lookup, cancelled token) before committing.

#### 6. "Get rows" fallback paths return the wrong rows

These are used when the OUTPUT / RETURNING clause is turned off (e.g. tables with triggers).

* `UpdateGetRows` runs the same `WHERE` again after the update. If the update changes a column used in the filter, rows come back missing or wrong. With joins, `SELECT *` also returns the joined tables' columns.
* `InsertGetRow` finds the new row with `SCOPE_IDENTITY()` / `LAST_INSERT_ROWID()`, so it fails for GUID or manually set keys - and only throws after the row has been inserted. On SQLite, matching on `rowid` would avoid this.

#### 9. `WithTableHints` silently does nothing on SQL Server without `UseTableHintExtensions()`

* `WithTableHints` only adds a tag to the query. Without `UseTableHintExtensions()` the custom query generator is not registered, so the tag is written out as a normal SQL comment (`-- $$TABLEHINTS$$NOLOCK`) and no `WITH (...)` hint is added. There is no error, so the caller can't tell the hints were ignored.
* Decide whether `WithTableHints` should throw if the custom query generator isn't registered (as the `ExecuteUpdate` extension methods now do when `UseExecuteUpdateExtensions()` wasn't called), or whether this is acceptable.
* On SQLite `WithTableHints` is a no-op by design (table hints are SQL Server only) - `TableHintsTests_018_Async` checks this.

### TODO - .NET 11 / EF Core 11 changes (November 2026)

These came from a review of the `Net11.Upgrade` branch, which moved the solution to .NET 11 and EF Core 11.0.0. All the tests pass on that branch.

Sources:
* EF Core source at the `v10.0.15` and `v11.0.0` tags (`SqlServerQuerySqlGenerator`, `SqliteQuerySqlGenerator`, their factories, `QuerySqlGenerator`, `UpdateExpression`, `EntityQueryProvider`, `QueryCompiler` and `QueryContextDependencies`);
* the EF Core 11 [breaking changes](https://learn.microsoft.com/en-us/ef/core/what-is-new/ef-core-11.0/breaking-changes) and [what's new](https://learn.microsoft.com/en-us/ef/core/what-is-new/ef-core-11.0/whatsnew) pages;
* the [C# 15](https://learn.microsoft.com/en-us/dotnet/csharp/whats-new/csharp-15) and [.NET 11](https://learn.microsoft.com/en-us/dotnet/core/whats-new/dotnet-11/overview) overviews.

The numbering carries on from the list above.

#### 15. Ranked free text search (new feature - parked)

Parked for now: not needed for the upgrade, and needs design decisions before any work starts.

* EF 11 adds `FreeTextTable(...)` / `ContainsTable(...)` on SQL Server, which return `FullTextSearchResult<TKey>` with `Key` and `Rank`.
* A ranked version of `FreeTextSearch` could stay database-independent: SQLite FTS5 has `rank` / `bm25()`.
* To decide:
  * the shape of the new method (e.g. returning the entity with its rank, or the key and rank like EF);
  * how stemming works on SQLite, where only the first `FreeTextSearch` call's `useStemming` value is honoured per query;
  * whether ranks need to be comparable between SQL Server and SQLite (they use different scales; FTS5 ranks are negative, lower is better).
* The existing `FreeTextSearch` translators (`FREETEXT` / `CONTAINS` on SQL Server, `MATCH` on SQLite) are unaffected by EF 11.

#### Checked for .NET 11 / EF 11 - no action needed

Recorded so these don't need checking again:
* **Reflection by name** still finds what it needs in 11.0.0:
  * `SqlServerQuerySqlGenerator._withinTable` (`bool`);
  * `EntityQueryProvider._queryCompiler`;
  * `QueryCompiler._queryContextFactory`;
  * `QueryContextDependencies.CurrentContext`.

  The source for `RelationalQueryContextFactory.Dependencies` couldn't be fetched (path 404), but the `GetDbContext` tests pass, so it still works. The `SqlServerQuerySqlGeneratorFactory` / `SqliteQuerySqlGeneratorFactory` constructors and `Create()` are unchanged.
* **`QuerySqlGenerator` changed shape.** It now derives from `ExpressionVisitor` (was `SqlExpressionVisitor`) and dispatches through a new `VisitExtension`. Many members are now `protected virtual` rather than `protected override`, and `VisitLike` / `VisitExists` / `VisitIn` are now private. The obsolete `GenerateTagsHeaderComment(SelectExpression)` overload is gone, but the `ISet<string>` overload we override is unchanged. Our overrides (`GenerateTagsHeaderComment`, `VisitUpdate` / `VisitDelete`, `VisitTable`) still compile.
* **SQL Server `GenerateTop`** has a new `WithApproximateExpression` (vector search) case. We call `this.GenerateTop(...)` rather than copying it, so we pick it up automatically.
* **Collation:** the base `VisitCollate` now delimits the collation name, so SQLite gets `COLLATE "NOCASE"` (SQL Server overrides it and doesn't quote). SQLite accepts a quoted collation name, and the NOCASE replacement tests pass.
* **Other SQL text changes in EF 11** (no-op `CAST` stripping, to-one join pruning, redundant `ORDER BY` removal, `GroupBy` changes) don't affect the library. Its tests don't assert on EF's SELECT SQL text.
* **C# 15:** nothing fits naturally.
  * No collection is built through a constructor with arguments, so `with(...)` collection-expression arguments don't apply.
  * The `SqlParser` loops have no nested-loop flags, so labelled `break` / `continue` don't apply.
  * Union types or `closed` on `ITableHint` / `SqlServerTableHints` would change the public API for little gain (`SqlServerTableHints` is already `sealed` with a private constructor).
* **.NET 11 libraries:** nothing compelling.
  * `EqualityComparer<T>.Create` isn't needed: the only comparer used is `StringComparer.OrdinalIgnoreCase`.
  * `FullJoin` and the new streams don't apply.
  * Runtime async needs no code changes, and the library no longer uses `AsyncLocal`, so there's no concern about how `ExecutionContext` flows.
