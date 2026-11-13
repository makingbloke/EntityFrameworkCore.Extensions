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

##### Chosen fix: tag the query with the variant, and keep the per-call state against the `DbContext`

Two separate problems are solved separately:
* **Which SQL to generate** is per query shape, so it must be in the expression (and so in the cache key). A tag with the variant name does this.
* **Which call the captured SQL belongs to** is per call, so it must not be in the expression. The `DbContext` identifies the call: a context can only run one operation at a time (EF throws on concurrent use), and both the method and the interceptor have it.

Steps:

1. In each method, tag the source with the variant name only before calling EF, using the same idea as table hints, e.g. `source.TagWith($"{ExecuteUpdateTagPrefix}{QueryType.UpdateGetRows}").ExecuteUpdateAsync(...)`.
   * A tag is part of the expression tree, so each variant gets its own cache entry and its own SQL.
   * Everything else is still part of the key: the table, the `Where` predicate and the properties being set (each `SetProperty(e => e.Field, ...)`). So two `ExecuteUpdateGetRowsAsync` calls that set different fields still get different SQL, and calls that only differ in variable values share one cache entry and run with different parameters (which is what should happen).
2. In both generators, decide between INSERT, UPDATE and the row-returning versions from `UpdateExpression.Tags` / `DeleteExpression.Tags` instead of the `AsyncLocal`.
3. Remove the tag from the header comment, the same way the table hints tag is handled (copy the set, don't change it). SQL Server already overrides `GenerateTagsHeaderComment`; SQLite would need to as well. The interceptor doesn't need the tag in the SQL, because it finds the call by the context (step 4).
4. Replace the `AsyncLocal` with a scoped service in EF's internal service provider (e.g. `ExecuteUpdateState`, holding the current `ExecuteUpdateParameters` or `null`):
   * EF creates a service scope for each `DbContext` instance, so a scoped service is one object per context - this is how EF itself keeps per-context state.
   * Register it from an `IDbContextOptionsExtension` that `UseExecuteUpdateExtensions()` adds (`ApplyServices` => `services.AddScoped<ExecuteUpdateState>()`). This needs the extension class and its `DbContextOptionsExtensionInfo`.
   * The get rows methods (`ExecuteDeleteGetRowsAsync`, `ExecuteInsertGetRowAsync`, `ExecuteUpdateGetRowsAsync`) get the state for `source.GetDbContext()` and set its parameters before calling EF. If it already has parameters, throw (this replaces the current re-entrancy check).
   * `ExecuteUpdateInterceptor` gets the state for `eventData.Context`. If it has parameters and the command source matches their query type, it stores the SQL and parameters and suppresses the command.
   * The method reads the SQL and parameters, runs them with `SqlQueryRaw`, and clears the state in a `finally`.
   * The plain `ExecuteInsertAsync` doesn't need to set the state: its SQL runs normally, it only needs the tag so the generator writes an INSERT (but it still checks the service is registered - see item 2).
   * Context pooling: a pooled context and its service scope are reused, so `ExecuteUpdateState` should implement `IResettableService` (`ResetState` / `ResetStateAsync` clear the parameters) so nothing is carried over when the context goes back to the pool.
   * Upsert still works: it runs its update and insert one after the other on the same context, so only one call uses the state at a time.
   * The same service is also the registration check for item 2.

Alternative: a `ConditionalWeakTable<DbContext, ExecuteUpdateParameters>` instead of the scoped service. Less code (no options extension) and an entry left behind is freed with the context, but it doesn't help with item 2, and a pooled context would still need its entry cleared.

Considered and rejected:
* **A unique Guid per call in the tag** (with a dictionary keyed on the Guid). It would give correct results, but `TagWith`'s argument is `[NotParameterized]`, so it's a constant in the cache key. Every call would be a cache miss: EF would compile the query (expression, translation and SQL generation) on every call, and EF's size-limited query cache would fill with entries that are never reused, pushing out the application's own cached queries. A missed cleanup would also leak a dictionary entry for the life of the process.
* **Replacing EF's `ICompiledQueryCacheKeyGenerator`** with one that adds the `AsyncLocal` value to the cache key. It would work, but relies on another internal EF service; the tag approach is simpler and matches table hints.

##### Tests to add

* Run each pair in both orders in the same test, and check the actual data each time:
  * plain update / update get rows
  * plain update / insert
  * insert / insert get row
  * plain delete / delete get rows
* Two `ExecuteUpdateGetRowsAsync` calls with the same `Where` that set different fields: each must update and return its own field.
* Two calls of the same variant with different values: both must give the right results (and share one cache entry).
* After a get rows method has finished (including when it throws), the context's `ExecuteUpdateState` has no parameters left, and a second call on the same context works.
* With context pooling (`AddDbContextPool` / `PooledDbContextFactory`), a context returned to the pool and leased again has no parameters left.
* Give each test its own `TagWith`, so other tests can't pre-populate the cache.
* Run them on SQLite and SQL Server.

Do this together with item 2: even with the tag, the methods still depend on the custom generator being registered, and without it `ExecuteInsertAsync` still runs as a plain UPDATE.

##### Progress notes (13 November 2026)

Items 1 and 2 are being done together, on `Development` (not committed). Everything builds. The full suite on SQLite and SQL Server gives **562 passed, 15 failed**. All 15 failures have the same cause: an EF Core 10 bug that drops the tag (see "Blocked" below). **A decision is needed before going further.**

**Done (library):**
* `ExecuteUpdateParameters.cs` is deleted and replaced by `ExecuteUpdate/ExecuteUpdateState.cs`:
  * `internal sealed`, implements `IResettableService`;
  * `QueryType`, `Sql?` and `Parameters?`;
  * `SetQueryType`, `SetQuery`, `ResetState` and `ResetStateAsync`;
  * the constructor calls `ResetState()`.
  * Two small fixes are still to do: `SetQueryType`'s summary was copied from `SetQuery` ("Set the Query Result"), and `ResetState` can use `null` rather than `null!`.
* `QueryType` has `Unknown` as its first value. That's the reset value, meaning "no call in progress".
* `CustomQueryGenerators/CustomTagsHandler.cs` (new):
  * holds both tag prefixes, `ExecuteUpdateTagPrefix = "$$EXECUTEUPDATE$$"` and `TableHintsTagPrefix = "$$TABLEHINTS$$"` (moved here from `ExecuteUpdateExtensions` / `TableHintExtensions`);
  * `ProcessTags(ISet<string>)` returns `QueryType`, `TableHints` and `RemainingTags`, a copy without the custom tags, so the cached set is never changed.
* **Both generators** work the same way (SQL Server also keeps `_tableHints`):
  * `GenerateTagsHeaderComment` calls `ProcessTags`, stores `_queryType`, and passes `RemainingTags` to the base, so the custom tags never reach the SQL.
  * `GenerateRootCommand` has one `DeleteExpression` case and one `UpdateExpression` case. Each writes the header comment first (which sets `_queryType`) and then picks the variant from `_queryType`. The generators never use `ExecuteUpdateState`: the factory is a singleton and the SQL is cached.
* `ExecuteUpdate/ExecuteUpdateOptionsExtension.cs` (new):
  * `ApplyServices` registers `ExecuteUpdateState` as a scoped service (one per `DbContext`).
  * It also registers it as `services.AddScoped<IResettableService>(sp => sp.GetRequiredService<ExecuteUpdateState>())`, the **same** instance, so EF resets it when a pooled context goes back to the pool. Implementing the interface isn't enough on its own. `TryAddEnumerable(ServiceDescriptor.Scoped<IResettableService, ExecuteUpdateState>())` would be wrong: it creates a second instance.
  * `Info` is created in the constructor (a nested `ExtensionInfo`). There are no settings, so `GetServiceProviderHashCode` returns 0 and `ShouldUseSameServiceProvider` checks the type.
  * `UseExecuteUpdateExtensions()` adds it with `AddOrUpdateExtension`, before the generator and the interceptor.
* `ExecuteUpdateInterceptor` gets the state with `eventData.Context?.GetInfrastructure().GetService<ExecuteUpdateState>()`.
  * It needs `using Microsoft.EntityFrameworkCore.Infrastructure;` and `using Microsoft.Extensions.DependencyInjection;`. The generic `GetService<T>()` is a DI extension method; it isn't on `IServiceProvider` itself.
  * It stores the SQL and suppresses the command for:
    * `CommandSource.ExecuteUpdate` with `InsertGetRow` / `UpdateGetRows`;
    * `CommandSource.ExecuteDelete` with `DeleteGetRows`.
* **`ExecuteUpdateExtensions`:**
  * `GetExecuteUpdateState(context, methodName)` is a private helper. It uses `GetService`, not `GetRequiredService`. If the state is missing, it throws `InvalidOperationException("{nameof(UseExecuteUpdateExtensions)} must be called when configuring the DbContext to use {methodName}.")` (item 2).
  * `ExecuteDeleteGetRowsAsync`, `ExecuteInsertGetRowAsync` and `ExecuteUpdateGetRowsAsync` each:
    1. get the state;
    2. throw "Another ExecuteUpdate call is already in progress on this DbContext." if `QueryType` isn't `Unknown`;
    3. call `SetQueryType`.

    These three steps happen **before** the `try`, so the `finally` can't wipe the state of the call that is actually running. Inside the `try`, each method:
    1. tags the source with its variant and runs EF;
    2. throws if `Sql` / `Parameters` are `null` (both are nullable, and warnings are errors);
    3. runs them with `SqlQueryRaw`.

    The `finally` calls `await state.ResetStateAsync(CancellationToken.None)`.
  * `ExecuteInsertAsync`: the registration check (`_ = GetExecuteUpdateState(...)`) and the tag only. There's no state, re-entrancy check or `finally`.
  * The `GetCount` methods are unchanged (plain EF calls, no tag, no check).
  * The upserts call `GetExecuteUpdateState` before `BeginTransactionIfRequiredAsync`.

**Done (tests):**
* The 12 `ExecuteUpdateParameters.Value` asserts now call `ExecuteUpdateTestUtils.AssertStateCleared(context)` (new file).
* The re-entrancy tests use a real context with the state already set. They check that the call throws, that nothing changes, and that the other call's query type is untouched. `ExecuteInsertTests_004` is now the item 2 test, because `ExecuteInsertAsync` no longer has a re-entrancy check.
* Item 2: a "without `UseExecuteUpdateExtensions`" test for each method, including both upserts. It checks the exception message, that the data is unchanged, and that no transaction is left open.
* After a failed call (unique index violation), `ExecuteInsertGetRowTests_006` and `ExecuteUpdateGetRowsTests_005` check that the state is cleared and that a second call on the same context works.
* `ExecuteUpdateQueryCacheTests` (new):
  * the four pairs, each in both orders;
  * the same `Where` with different setters;
  * the same variant called with different values;
  * the variant tag isn't in the SQL but the caller's tag is (`LogTo` on `CommandExecuting`).
  * Each test needs no `TagWith` of its own: `Context` turns service provider caching off, so every context has its own query cache.
* `ExecuteUpdateStateTests` (new):
  * the constructor, `ResetState` and `ResetStateAsync`;
  * one state per context, and none without the extension;
  * a pooled context (`PooledDbContextFactory`, pool size 1) leased twice: same context, same state object, and the state is cleared.
  * It uses the new `Data/PooledContext.cs`, because `Context` is configured in `OnConfiguring` and can't be pooled. It also uses the new `DatabaseUtils.GetConnectionString`.
* `TableHintsTests` now uses `CustomTagsHandler.TableHintsTagPrefix`.
* In the ExecuteUpdate test classes, the `public methods` regions are renamed to `<Member> tests`.

**Blocked: EF Core 10 drops the tags of an `UpdateExpression`**
* EF issue [#36908](https://github.com/dotnet/efcore/issues/36908), "ExecuteUpdate is losing tags depending on how null value is passed", was closed in October 2025. It's fixed in milestone **11.0.0** by "Fix UpdateExpression.VisitChildren() to retain Tags", and wasn't backported: 10.0.15 still has it. I reported it myself (as makingbloke) in October 2025, which is why 6.0.0.2 switched from tags to the `AsyncLocal` (see the ChangeLog). The repro in the issue uses a constant `null` and no `Where`, so the `==` trigger below could be added to the issue, along with a request to backport the fix to 10.x.
* Cause: any visitor that changes something inside an `UpdateExpression` rebuilds it without its tags. The issue's trigger is a constant `null` in `SetProperty`. Ours is an `==` in the `Where`.
* Checked with a plain context (no extensions), on SQLite:

  | Query | Tag in the SQL? |
  |---|---|
  | UPDATE, `Where(e => e.Id == id)`, `e.Id == 2`, or `e.TestField == s` | no |
  | UPDATE, `TagWith` before the `Where` | no |
  | UPDATE, `Where(e => e.Id <= id)` or no `Where` | yes |
  | DELETE, `Where(e => e.Id == id)` | yes |
  | SELECT, `Where(e => e.Id == id)` | yes |

* The tag still keeps the variants apart in the query cache, because the cache key is the expression before translation. But the generator never sees the tag, so it writes a plain UPDATE:
  * `ExecuteUpdateGetRowsAsync` (and so `ExecuteUpsertGetRowsAsync`) with an `==` filter applies the update, then throws "The required column 'Id' was not present in the results of a 'FromSql' operation".
  * **Regression:** `ExecuteInsertAsync` / `ExecuteInsertGetRowAsync` on a source with `Where(e => e.Id == id)` now **update the matching row** instead of throwing (`ExecuteInsertTests_005`, `ExecuteInsertGetRowTests_005`). The `AsyncLocal` version threw.
  * **Pre-existing:** table hints on an UPDATE with an `==` filter were already dropped silently. `TableHintsTests_020` (`ExecuteUpdateGetRowsAsync`) fails on a92f1bd too, with "Table hint missing from SQL". This needs its own item.
* The failing tests:
  * `ExecuteInsertTests_005`;
  * `ExecuteInsertGetRowTests_005`;
  * `ExecuteUpdateGetRowsTests_005`;
  * `ExecuteUpdateQueryCacheTests_001`, `_006` and `_007`;
  * `TableHintsTests_020`.

  All of them run on both databases except `TableHintsTests_020`, which is SQL Server only. The tests are correct, so leave them failing until this is fixed.
* Workarounds suggested online that don't help:
  * passing a lambda instead of a constant to `SetProperty`: that only avoids the issue's trigger, and we can't rewrite callers' setters;
  * putting `TagWith` last: ours already is;
  * a `DbCommandInterceptor` that adds comments: we need the tag while the SQL is generated, not afterwards.

**Options (decision needed):**
1. **Move to EF Core 11**, where it's fixed. 11.0.0-rc.3 is in the local NuGet cache. Suggested first step: run the suite against it on a scratch branch to confirm the 15 failures go away. The catch is that the library targets EF 10.
2. **Stay on EF 10 and put the tags back.** `UpdateExpression.ApplyTags(ISet<string>)` is public. But `VisitChildren` can drop the tags at more than one stage, maybe before `RelationalParameterBasedSqlProcessor.ProcessSqlNullability`, and then the original tags are already gone. So first find out where they are lost. Putting them back means replacing another internal EF service (`IRelationalParameterBasedSqlProcessorFactory`, and maybe `IQueryTranslationPostprocessorFactory`), one class per provider.
3. **Both:** EF 11 for the real fix, and make EF 10 safe in the meantime. For example, `ExecuteInsertAsync` / `ExecuteInsertGetRowAsync` could reject a source with a `Where` before running anything.
4. Mark the variant somewhere EF keeps, such as the predicate or the setters. Rejected as hacky: it would show up in the SQL and need stripping.

**Still to do after that:**
* Update `CLAUDE.md` ("Custom query generator + SQL tagging pattern", point 3) and the ChangeLog.
* Add a TODO item for table hints being dropped on an UPDATE with an `==` filter.

#### 2. `ExecuteInsertAsync` without `UseExecuteUpdateExtensions()` updates every row (critical, reproduced)

* Without the custom query generator registered, `ExecuteInsertAsync` runs as a normal `ExecuteUpdateAsync` - in the repro all 5 rows were overwritten.
* The `GetRows` variants run the real UPDATE / DELETE, then fail because no SQL was captured.
* Suggested fix: each method checks the extensions are registered before running anything, and throws if not.
  * With the scoped `ExecuteUpdateState` service from item 1, this check comes for free: the service is only registered by `UseExecuteUpdateExtensions()`, so if it can't be found the extensions aren't registered.
  * Don't rely on `context.GetService<ExecuteUpdateState>()` throwing: EF's own exception for a missing service doesn't tell the caller what to do. Look it up without throwing (e.g. through `((IInfrastructure<IServiceProvider>)context).Instance.GetService(typeof(ExecuteUpdateState))`, which returns `null`) and throw a clear exception, e.g. `InvalidOperationException("UseExecuteUpdateExtensions() must be called when configuring the DbContext to use ExecuteInsertAsync.")`.
  * Every ExecuteXxx method in `ExecuteUpdateExtensions` that depends on the custom generator should check, including the plain `ExecuteInsertAsync` and the upsert methods. `ExecuteDeleteGetCountAsync` / `ExecuteUpdateGetCountAsync` are plain EF calls and don't need it.
  * The check must happen before EF runs anything, so nothing is updated or deleted when it fails.
* Tests: each method without `UseExecuteUpdateExtensions()` throws the new exception, and the table data is unchanged afterwards.

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

### TODO - .NET 11 / EF Core 11 changes (November 2026)

These came from a review of the `Net11.Upgrade` branch, which moved the solution to .NET 11 and EF Core 11.0.0. All the tests pass on that branch.

Sources:
* EF Core source at the `v10.0.15` and `v11.0.0` tags (`SqlServerQuerySqlGenerator`, `SqliteQuerySqlGenerator`, their factories, `QuerySqlGenerator`, `UpdateExpression`, `EntityQueryProvider`, `QueryCompiler` and `QueryContextDependencies`);
* the EF Core 11 [breaking changes](https://learn.microsoft.com/en-us/ef/core/what-is-new/ef-core-11.0/breaking-changes) and [what's new](https://learn.microsoft.com/en-us/ef/core/what-is-new/ef-core-11.0/whatsnew) pages;
* the [C# 15](https://learn.microsoft.com/en-us/dotnet/csharp/whats-new/csharp-15) and [.NET 11](https://learn.microsoft.com/en-us/dotnet/core/whats-new/dotnet-11/overview) overviews.

The numbering carries on from the list above.

#### 10. SQL Server UPDATE / DELETE don't get EF 11's `SET NOCOUNT OFF` (should fix)

* In EF 11, `SqlServerQuerySqlGenerator.VisitUpdate` and `VisitDelete` start with:

  ```csharp
  Sql.Append("SET NOCOUNT OFF").AppendLine(Dependencies.SqlGenerationHelper.StatementTerminator);
  ```

  EF 10 didn't have this. Presumably it makes sure the rows-affected count comes back even when the session has `SET NOCOUNT ON` (otherwise `ExecuteUpdate` / `ExecuteDelete` would get -1). The rest of both methods is the same in 10 and 11, including the JSON `modify` setter branch.
* `SqlServerCustomQueryGenerator.GenerateRootCommand` sends **every** `UpdateExpression` and `DeleteExpression` to our own private `VisitUpdate(updateExpression, getRows)` / `VisitDelete(deleteExpression, getRows)` copies. That includes the plain variants (`QueryType.Unknown`), and so a user's own `ExecuteUpdateAsync` / `ExecuteDeleteAsync` once `UseExecuteUpdateExtensions()` **or** `UseTableHintExtensions()` has been called. The copies don't write `SET NOCOUNT OFF`, so those users don't get EF's change.
* Suggested fix: stop overriding `GenerateRootCommand`, and override `VisitUpdate` / `VisitDelete` instead. In EF 11 these are `protected virtual` on `QuerySqlGenerator`, and the SQL Server generator overrides them.
  * The base `GenerateRootCommand` already calls `GenerateTagsHeaderComment(tags)` (which sets `_queryType` through `CustomTagsHandler`) and then `VisitUpdate` / `VisitDelete`, so the query type is known by the time they run.
  * Handle only `InsertGetRow`, `UpdateGetRows`, `DeleteGetRows` and `Insert` in our code, and call `base.VisitUpdate(updateExpression)` / `base.VisitDelete(deleteExpression)` for everything else. Plain calls then always match EF (now and in future versions), and there's less copied EF code to keep up to date.
  * Table hints still work on the plain path: the base visits the tables through `Visit(...)`, which reaches our `VisitTable` override.
  * Do the same in `SqliteCustomQueryGenerator`. Its UPDATE / DELETE copies come from the relational base, which hasn't changed in 11, but delegating keeps it in line with EF.
  * Decide whether the custom variants should also write `SET NOCOUNT OFF;` (probably not needed: the get rows variants read rows rather than a count, and `ExecuteInsertAsync` doesn't return one. But it would be harmless and consistent).
* Test: open a SQL Server connection, run `SET NOCOUNT ON` on it, then check that `ExecuteUpdateGetCountAsync` / `ExecuteDeleteGetCountAsync` (and a plain `ExecuteUpdateAsync` with `UseTableHintExtensions()`) return the right count.

#### 11. Item 1 is unblocked by EF 11 - update its notes and finish it

* EF issue #36908 is fixed in 11.0.0. `UpdateExpression.VisitChildren` now ends with `new UpdateExpression(table, selectExpression, columnValueSetters ?? ColumnValueSetters, Tags)`; v10 left out `Tags`. On the `Net11.Upgrade` branch the 15 tests that failed on EF 10 now pass (including `ExecuteInsertTests_005` / `ExecuteInsertGetRowTests_005`, the insert-with-`Where` regression, and `TableHintsTests_020`, the table hints dropped on an UPDATE with `==`).
* So the tag-based design in item 1 works as planned. The options listed under "Options (decision needed)" in the item 1 notes are no longer needed: no `AsyncLocal` / option 5 and no tag-repair processor. Update the notes to say so.
* The "add a TODO item for table hints being dropped on an UPDATE with an `==` filter" step in the item 1 notes isn't needed either. It's fixed by the same EF change.
* Record that the library now **depends on EF 11** for this to be correct. If it ever targets EF 10 again (e.g. multi-targeting), the tag loss comes back. The history is in ChangeLog 6.0.0.2.
* Then finish what's left of items 1 and 2: the two small `ExecuteUpdateState` fixes, `CLAUDE.md` ("Custom query generator + SQL tagging pattern", point 3, which still describes the `AsyncLocal`), and the ChangeLog.

#### 12. Set the target framework once, in `Directory.Build.props`

* `Directory.Build.props` still has `<TargetFramework>net10.0</TargetFramework>`. The branch adds `<TargetFramework>net11.0</TargetFramework>` to both `DotDoc.EntityFrameworkCore.Extensions.csproj` and `DotDoc.EntityFrameworkCore.Extensions.Tests.csproj`, which override it.
* Change `Directory.Build.props` to `net11.0` and remove the two `.csproj` lines, so there's one place to change next time.
* Note that EF Core 11 won't run on earlier .NET versions, and Microsoft.Data.Sqlite 11 targets .NET 10 or later only, so there is no option of multi-targeting `net10.0` with EF 11.

#### 13. The direct `SQLitePCLRaw.bundle_e_sqlite3` reference is redundant

* The library references `SQLitePCLRaw.bundle_e_sqlite3` 3.0.5 (`Directory.Packages.props`). `Microsoft.EntityFrameworkCore.Sqlite` 11.0.0 already depends on `SQLitePCLRaw.bundle_e_sqlite3` 3.0.5 and `SQLitePCLRaw.core` 3.0.5.
* Either remove the reference, or keep it on purpose (e.g. to pin a newer SQLite) with a comment saying why. If it's kept, it has to be bumped in step with EF.
* Background (EF 11 breaking changes): with SQLitePCLRaw 3.x, `bundle_e_sqlcipher`, `bundle_sqlite3`, `bundle_winsqlite3`, `bundle_green` and `bundle_e_sqlite3mc` are no longer compatible. `bundle_e_sqlite3`, which we use, is fine.

#### 14. Documentation for the upgrade

* `CLAUDE.md`: the overview says "Entity Framework Core 10 (targeting net10.0)". Change it to EF Core 11 / `net11.0`.
* ChangeLog: add an entry for the upgrade (.NET 11, EF Core 11). Mark it as a **breaking change** for users, and include:
  * EF 11 now defaults to **SQL Server compatibility level 160** (SQL Server 2022). Users on SQL Server 2019 or older, or on a database with a lower compatibility level, need `UseSqlServer(..., o => o.UseCompatibilityLevel(150))` (or lower). The library's own SQL doesn't use anything version-specific, but EF's does.
  * EF 11 uses **Microsoft.Data.SqlClient 7.0**: users of `ActiveDirectory*` (Entra ID) authentication need to add `Microsoft.Data.SqlClient.Extensions.Azure`, and `ActiveDirectoryPassword` is obsolete.
  * Users need .NET 11 (EF 11 doesn't run on earlier versions).
* ChangeLog line 8 (6.0.0.17) says "In EF Core 15, MS fixed an issue in the DbCommandInterceptor...". Check whether this should say EF Core 10.0.15.
* ReadMe: consider stating the supported versions (.NET 11, EF Core 11, and the SQL Server compatibility level note), since it doesn't mention any versions at the moment.

#### 15. EF 11 full-text features (optional)

* EF 11 can configure SQL Server full-text catalogs and indexes in the model: `modelBuilder.HasFullTextCatalog("ftCatalog")` and `entity.HasFullTextIndex(b => b.FullName).UseKeyIndex("PK_...").UseCatalog("ftCatalog")`. Migrations write `CREATE FULLTEXT CATALOG` / `CREATE FULLTEXT INDEX`.
  * The tests set these up with raw SQL in `DatabaseUtils.SqlServerInitialiseFreeTextAsync`, and look up the primary key name with internal APIs to do it. Check whether `EnsureCreatedAsync` creates them from the model. If it does, configure them in `Context.OnModelCreating` and remove the raw SQL.
* EF 11 adds `FreeTextTable(...)` / `ContainsTable(...)`, which return `FullTextSearchResult<TEntity>` with `Value` and `Rank`.
  * A possible new feature: a ranked version of `FreeTextSearch` (SQLite FTS5 has `rank` / `bm25()`, so it could stay database-independent). This is a new feature, not needed for the upgrade.
* The existing `FreeTextSearch` translators (`FREETEXT` / `CONTAINS` on SQL Server, `MATCH` on SQLite) are unaffected.

#### Checked for .NET 11 / EF 11 - no action needed

Recorded so these don't need checking again:
* **Reflection by name** still finds what it needs in 11.0.0:
  * `SqlServerQuerySqlGenerator._withinTable` (`bool`);
  * `EntityQueryProvider._queryCompiler`;
  * `QueryCompiler._queryContextFactory`;
  * `QueryContextDependencies.CurrentContext`.

  The source for `RelationalQueryContextFactory.Dependencies` couldn't be fetched (path 404), but the `GetDbContext` tests pass, so it still works. The `SqlServerQuerySqlGeneratorFactory` / `SqliteQuerySqlGeneratorFactory` constructors and `Create()` are unchanged.
* **`QuerySqlGenerator` changed shape.** It now derives from `ExpressionVisitor` (was `SqlExpressionVisitor`) and dispatches through a new `VisitExtension`. Many members are now `protected virtual` rather than `protected override`, and `VisitLike` / `VisitExists` / `VisitIn` are now private. The obsolete `GenerateTagsHeaderComment(SelectExpression)` overload is gone, but the `ISet<string>` overload we override is unchanged. Our overrides (`GenerateRootCommand`, `GenerateTagsHeaderComment`, `VisitTable`) still compile.
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
