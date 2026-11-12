### TODO - Code review findings (October 2026)

Items are in priority order. "Reproduced" means a temporary test showed the problem against SQLite; the others were found by reading the code.

**Before release:** none of the SQL Server tests have been run since the upsert transaction fix (44e0d49) and the SQLite NOCASE change, because no SQL Server instance was available. Run the full suite against SQL Server first.

#### 1. EF's SQL cache ignores which ExecuteXxx method was called (critical, reproduced)

##### What goes wrong

Several methods end up building exactly the same LINQ expression:

| Method | What it actually calls |
|---|---|
| `ExecuteUpdateAsync` / `ExecuteUpdateGetCountAsync` | `source.ExecuteUpdateAsync(setters)` |
| `ExecuteUpdateGetRowsAsync` | `source.ExecuteUpdateAsync(setters)`, with the `AsyncLocal` set to `UpdateGetRows` |
| `ExecuteInsertAsync` | `source.ExecuteUpdateAsync(setters)`, with the `AsyncLocal` set to `Insert` |
| `ExecuteInsertGetRowAsync` | `source.ExecuteUpdateAsync(setters)`, with the `AsyncLocal` set to `InsertGetRow` |
| `ExecuteDeleteAsync` / `ExecuteDeleteGetRowsAsync` | `source.ExecuteDeleteAsync()`, with or without the `AsyncLocal` set |

* Only the `ExecuteUpdateParameters` `AsyncLocal` tells `SqlServerCustomQueryGenerator` / `SqliteCustomQueryGenerator` whether to write an INSERT, an UPDATE, or a version that returns the rows.
* EF compiles each query once and caches it, including the generated SQL. The cache key is built from the expression tree, and the `AsyncLocal` isn't part of it. So the first variant to run for a query shape decides the SQL for every later call with that shape.
* In EF 10, `SetProperty(e => e.X, value)` turns the value into a parameter, so calls with different values still share a shape.
* The cache belongs to EF's internal service provider, which every `DbContext` with the same configuration shares. So the problem isn't limited to one context or one request: whichever code path runs first after the app starts decides the behaviour, which makes it hard to track down in production.

##### Reproduced on SQLite

Each test used a unique `TagWith` so only the pair being tested shared a cache entry.

1. `ExecuteUpdateGetCountAsync` then `ExecuteUpdateGetRowsAsync`: the second call reused the cached plain UPDATE. The interceptor captured and blocked it, then `SqlQueryRaw` ran it, so the update was applied. The call then threw "The required column 'Id' was not present in the results of a 'FromSql' operation".
2. `ExecuteUpdateAsync` on the whole table then `ExecuteInsertAsync`: the "insert" ran the cached UPDATE. All 5 existing rows were overwritten with the insert values and no row was inserted.
3. `ExecuteInsertAsync` then `ExecuteUpdateAsync`: the "update" ran the cached INSERT, so a new row was added and the existing rows were unchanged.

##### Other cases from the same cause (not reproduced)

* `ExecuteDeleteAsync` then `ExecuteDeleteGetRowsAsync` (same shape): same pattern as case 1 - the rows are deleted, then the call throws.
* Upsert:
  * The update step has the same collision as case 1 if a plain `ExecuteUpdateAsync` with the same shape has run first.
  * `ExecuteUpsertGetCountAsync` and `ExecuteUpsertGetRowsAsync` insert through `ExecuteInsertAsync` and `ExecuteInsertGetRowAsync` with the same shape. If the count version runs first, the rows version throws after inserting (the transaction is rolled back, so no data is changed).
* Ordinary `ExecuteUpdateAsync` calls elsewhere in an app (e.g. `context.Orders.Where(o => o.Id == id).ExecuteUpdateAsync(...)`) can collide with any of these methods that use the same shape.

##### Suggested fix: make the variant part of the expression

1. In each method, tag the source before calling EF, using the same idea as table hints, e.g. `source.TagWith($"{ExecuteUpdateTagPrefix}{QueryType.InsertGetRow}").ExecuteUpdateAsync(...)`. A tag is part of the expression tree, so each variant gets its own cache entry and its own SQL.
2. In both generators, decide between INSERT, UPDATE and the row-returning versions from `UpdateExpression.Tags` / `DeleteExpression.Tags` instead of the `AsyncLocal`.
3. Remove the tag from the header comment, the same way the table hints tag is now handled (copy the set, don't change it). SQL Server already overrides `GenerateTagsHeaderComment`; SQLite would need to as well.
4. Keep the `AsyncLocal` only to tell `ExecuteUpdateInterceptor` to capture and block the command. That part is per call rather than per query shape, so it isn't affected by caching. Keep the existing re-entrancy check and the `finally` that clears it.

Alternative: replace EF's `ICompiledQueryCacheKeyGenerator` with one that adds the `AsyncLocal` value to the cache key. It would work, but relies on another internal EF service; the tag approach is simpler and matches table hints.

##### Tests to add

* Run each pair in both orders in the same test, and check the actual data each time:
  * plain update / update get rows
  * plain update / insert
  * insert / insert get row
  * plain delete / delete get rows
* Give each test its own `TagWith`, so other tests can't pre-populate the cache.
* Run them on SQLite and SQL Server.

Do this together with item 2: even with the tag, the methods still depend on the custom generator being registered, and without it `ExecuteInsertAsync` still runs as a plain UPDATE.

#### 2. `ExecuteInsertAsync` without `UseExecuteUpdateExtensions()` updates every row (critical, reproduced)

* Without the custom query generator registered, `ExecuteInsertAsync` runs as a normal `ExecuteUpdateAsync` - in the repro all 5 rows were overwritten.
* The `GetRows` variants run the real UPDATE / DELETE, then fail because no SQL was captured.
* Suggested fix: each method checks the custom generator and `ExecuteUpdateInterceptor` are registered before running anything, and throws if not.

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
  * Existing schema behaviour kept: details found in the EF model use the schema from the message ("dbo"); details from the database or the message use `null` for dbo.
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

#### 5. Upsert isn't safe when two run at the same time on SQL Server

* Under READ COMMITTED an UPDATE that matches no rows takes no range locks, so two concurrent upserts can both update nothing and both insert (duplicate row or unique constraint error).
* Suggested fix: `UPDLOCK, HOLDLOCK` on the update, or serializable isolation for the upsert transaction. (SQLite serialises writers so is not affected.)

#### 6. "Get rows" fallback paths return the wrong rows

These are used when the OUTPUT / RETURNING clause is turned off (e.g. tables with triggers).

* `UpdateGetRows` runs the same `WHERE` again after the update. If the update changes a column used in the filter, rows come back missing or wrong. With joins, `SELECT *` also returns the joined tables' columns.
* `InsertGetRow` finds the new row with `SCOPE_IDENTITY()` / `LAST_INSERT_ROWID()`, so it fails for GUID or manually set keys - and only throws after the row has been inserted. On SQLite, matching on `rowid` would avoid this.

#### 8. `UniqueConstraintDetails.Schema` is not consistent for the SQL Server `dbo` schema

* When the table is in EF Core, `SqlServerExceptionProcessor` returns `Schema = "dbo"` (the schema parsed from the error message).
* When the table is not in EF Core and the details are read from the database, it returns `Schema = null` for `dbo` (EF Core stores the default schema as null).
* So the same table gives a different schema depending on whether it is in the model. Schemas other than `dbo` are returned as-is by both paths.
* The current tests lock in both behaviours: `GetUniqueConstraintDetailsAsyncTests_003` / `_004` (and the interceptor tests) expect `context.DefaultSchema` (`"dbo"`), and `GetUniqueConstraintDetailsAsyncTests_005` expects `null`.
* Decide which is right (`"dbo"` matches the database, `null` matches EF Core and SQLite), change the processor and update the tests to match.

#### 9. `WithTableHints` silently does nothing on SQL Server without `UseTableHintExtensions()`

* `WithTableHints` only adds a tag to the query. Without `UseTableHintExtensions()` the custom query generator is not registered, so the tag is written out as a normal SQL comment (`-- $$TABLEHINTS$$NOLOCK`) and no `WITH (...)` hint is added. There is no error, so the caller can't tell the hints were ignored.
* Decide whether `WithTableHints` should throw if the custom query generator isn't registered (as suggested for `ExecuteInsertAsync` in item 2), or whether this is acceptable.
* On SQLite `WithTableHints` is a no-op by design (table hints are SQL Server only) - `TableHintsTests_018_Async` checks this.
