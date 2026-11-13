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

Not implemented. A trial implementation along these lines was written and built with no warnings, and its SQLite tests passed (the SQL Server tests were not run), but it was undone pending a decision on how to structure the sync and async methods. That is now decided (see "Sync / async structure" below), so this is ready to implement.

* **Detection by error code**, then parse the message for names only:
  * SQL Server: any `SqlError` in the `SqlException` with `Number` 2601 or 2627 (a `SqlException` can contain several errors, e.g. 2627 then 3621 "The statement has been terminated").
  * SQLite: `SqliteException.SqliteExtendedErrorCode` 2067 (`SQLITE_CONSTRAINT_UNIQUE`) or 1555 (`SQLITE_CONSTRAINT_PRIMARYKEY`). SQLite messages are always English, so the English-only problem is really SQL Server only.
  * The exception is found by walking the inner exceptions (`UniqueConstraintModel.FindException<T>`) rather than `GetBaseException()`.
* **SQL Server 2627:** a second regex for `Violation of (PRIMARY|UNIQUE) KEY constraint 'name'. Cannot insert duplicate key in object 'schema.table'`. No `sys.key_constraints` lookup is needed: a PRIMARY KEY or UNIQUE constraint creates an index with the same name, so the existing `sys.indexes` query works for both errors.
* **Lookup order** (the same for the sync and async methods): the exception / message, then the EF model, then the database (SQL Server only).
  * EF model: checks `GetKeys()` (primary and alternate keys, by `GetName`) as well as `GetIndexes()` (by `GetDatabaseName`). Entity matching uses `(et.GetSchema() ?? "dbo") == schema`, so it also works when the model has a default schema.
  * Database: the existing `sys.indexes` query, run with `SqlQuery(...).ToList()` or `ToListAsync` depending on the `isAsync` flag (see "Sync / async structure" below).
  * If the index is in neither the model nor the database, the details have the table from the message and empty `FieldNames`.
  * If the message can't be parsed (e.g. SQL Server not using English), the table comes from `DbUpdateException.Entries[0].Metadata` and `FieldNames` is empty. (Guessing the fields from "the only unique index or key" was rejected - every entity has a primary key, so it would report the wrong fields.)
  * Schema: `null` for the default schema (`dbo`) whichever way the details are found, the same as in EF Core (changed in November 2026; previously the EF model path returned "dbo").
* **Synchronous path:** `IExceptionProcessor` has `GetUniqueConstraintDetails(DatabaseFacade, Exception)` alongside `GetUniqueConstraintDetailsAsync(DatabaseFacade, Exception, CancellationToken)` - same parameters. New public `GetUniqueConstraintDetails(this DatabaseFacade, Exception)` extension; `SaveChangesFailed` uses it instead of blocking on `.Result`, and `SaveChangesFailedAsync` uses the async one. Both are implemented by one core method per processor (see below), so the lookup order is written once.
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
  * **Renumber when implementing:** tests have been added since the trial (`GetUniqueConstraintDetailsTests` now goes up to 011, `UniqueConstraintInterceptorTests` to 005 and `UniqueConstraintExceptionTests` to 005), so the trial's numbers above clash. Some of the trial's tests may now already exist.
  * Add a test that runs the sync path against SQL Server with the database lookup, so the "must complete synchronously" check below is exercised.

##### Sync / async structure (decided November 2026)

The trial wrapped the sync method in the async one (`Task.FromResult(GetUniqueConstraintDetails(...))` on SQLite), and the SQL Server processor had separate sync and async methods that duplicated the lookup order. Neither is wanted. Instead, each processor has one core method with an `isAsync` flag (the pattern used by the Azure SDK):

```csharp
public UniqueConstraintDetails? GetUniqueConstraintDetails(DatabaseFacade database, Exception exception)
{
    ValueTask<UniqueConstraintDetails?> task = this.GetUniqueConstraintDetailsCoreAsync(database, exception, isAsync: false, CancellationToken.None);

    // With isAsync false nothing is awaited, so the task has always completed. Check rather than assume.
    if (!task.IsCompleted)
    {
        throw new InvalidOperationException("The synchronous unique constraint lookup did not complete synchronously.");
    }

    return task.GetAwaiter().GetResult();
}

public Task<UniqueConstraintDetails?> GetUniqueConstraintDetailsAsync(DatabaseFacade database, Exception exception, CancellationToken cancellationToken)
    => this.GetUniqueConstraintDetailsCoreAsync(database, exception, isAsync: true, cancellationToken).AsTask();

private async ValueTask<UniqueConstraintDetails?> GetUniqueConstraintDetailsCoreAsync(DatabaseFacade database, Exception exception, bool isAsync, CancellationToken cancellationToken)
{
    // Parse the message, then try the EF model: the same for both paths.
    ...

    // Only the database query differs.
    List<string> fieldNames = isAsync
        ? await query.ToListAsync(cancellationToken).ConfigureAwait(false)
        : query.ToList();
    ...
}
```

* **Every I/O call in the core method must branch on `isAsync`.** If anything awaits real async work regardless of the flag, the sync path becomes sync-over-async again. The `IsCompleted` check makes that fail with an exception instead of blocking, and the sync SQL Server test catches it.
* **Analyzer:** SonarAnalyzer S4462 ("calls to async methods should not be blocking") flags `GetAwaiter().GetResult()`, and warnings are errors. Suppress it on the sync method only, with `[SuppressMessage]` and a justification such as "Completes synchronously when isAsync is false; checked with IsCompleted."
* **`ValueTask`** for the core method: on the sync path (and always on SQLite) it completes synchronously, so no `Task` is allocated.
* **SQLite:** the core method does no I/O, so it ignores `isAsync`. Both processors have the same public sync / async pair, and `IExceptionProcessor` keeps both methods with the same parameters.
* **Interceptor:** `SaveChangesFailed` calls the sync method and `SaveChangesFailedAsync` calls the async one. Nothing blocks on `.Result`.
* **Rejected:**
  * wrapping the sync method in the async one: blocks a thread when there is I/O (SQL Server), and isn't wanted even without it;
  * separate sync and async implementations: duplicates the lookup order.

##### Before committing

* Run the SQL Server tests (2627, the sync database lookup, cancelled token).

#### 6. "Get rows" fallback paths return the wrong rows

##### When the fallback is used

`ExecuteUpdateGetRowsAsync`, `ExecuteInsertGetRowAsync` and `ExecuteDeleteGetRowsAsync` normally read the rows back with `OUTPUT INSERTED.*` / `OUTPUT DELETED.*` (SQL Server) or `RETURNING *` (SQLite). The fallback is only used when the table is configured with `UseSqlOutputClause(false)` / `UseSqlReturningClause(false)`. The usual reason is a trigger on a SQL Server table: SQL Server rejects a plain `OUTPUT` (without `INTO`) on a table with triggers. So only tables set up this way are affected, which makes this lower priority than the other items.

##### Problems

1. **Update: the rows are found with the same `WHERE`, after the update** (`GenerateSelectModifiedRows`, called from `GenerateUpdateGetRows` in both generators):

   ```sql
   UPDATE ... SET [Status] = 'Done' WHERE [Status] = 'Pending';
   SELECT * FROM ... WHERE [Status] = 'Pending';
   ```

   If the update changes a column the `WHERE` uses, the select finds different rows (here none, because every matching row is now `'Done'`), or picks up rows that only match after the update.
2. **Update and delete with joins: `SELECT *` returns every table's columns.** `GenerateSelectModifiedRows` projects `*`, so with a join the result includes the joined tables' columns. `SqlQueryRaw<TSource>` then gets duplicate names (e.g. two `Id` columns), and fails or maps the wrong values.
3. **Insert: the new row is found by the last identity value** (`GenerateInsert`):

   ```sql
   SELECT * FROM [T] WHERE [Id] = SCOPE_IDENTITY()          -- SQL Server
   SELECT * FROM "T" WHERE "Id" = LAST_INSERT_ROWID()       -- SQLite
   ```

   This only works when the primary key is an identity / auto-increment column. With a GUID key or a key set by the caller, it finds nothing, and the method throws "Unexpected entry count from insert: 0" **after** the row has been inserted. Unless the caller is in a transaction, the row stays in the table.
4. **Delete (smaller):** the fallback runs the `SELECT` and then the `DELETE` as two statements with no locking between them, so another connection could change the rows in between (a narrow gap, the same kind as the upsert race in item 5).

##### Possible fixes

* **SQL Server - capture the keys with `OUTPUT ... INTO` a table variable.** `OUTPUT INTO` is allowed on tables with triggers (EF uses the same technique for `SaveChanges` on such tables):

  ```sql
  DECLARE @keys TABLE ([Id] bigint);
  UPDATE [t] SET ... OUTPUT INSERTED.[Id] INTO @keys FROM ... WHERE ...;
  SELECT [t].* FROM [T] AS [t] WHERE [t].[Id] IN (SELECT [Id] FROM @keys);
  ```

  * Fixes problems 1, 2 and 3 at once: the rows are found by key, only the target table's columns are returned, and any key type works (including composite keys).
  * Returns the values after any triggers have run, which is usually what the caller wants.
  * The table variable needs the key columns' store types, which the generator can get from the EF model.
  * Delete: `OUTPUT DELETED.<keys> INTO` doesn't help (the rows are gone), so either keep select-then-delete (with `[t].*` and a lock hint), or use `OUTPUT DELETED.* INTO` a table variable declaring every column.
* **SQLite:**
  * Insert: match on `rowid = last_insert_rowid()` instead of the primary key column. Works for any key type on a normal table (not a `WITHOUT ROWID` table). A one-line change.
  * Update: harder, as SQLite has no `OUTPUT INTO`; it would need the rowids saved before the update (e.g. in a temp table). RETURNING works with triggers on SQLite, so turning it off there is rare; low priority.
  * Joins: select `[t].*` (the target table's alias) instead of `*`, as for SQL Server.

##### Suggested order

1. SQL Server `OUTPUT INTO @keys` for update and insert (where triggers make the fallback a real need).
2. The SQLite `rowid` change for insert.
3. `[t].*` instead of `*` in `GenerateSelectModifiedRows`.

Leave the SQLite update fallback and the delete gap as notes unless they're needed.

##### Tests

* With the clause turned off: an update that changes a column used in its `Where` returns the updated rows; an insert with a GUID key (`GuidTable`) returns the new row; an update / delete with a join returns only the target table's rows.
* Run on SQL Server with a real trigger on the table, to check the `OUTPUT INTO` SQL is accepted.

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
