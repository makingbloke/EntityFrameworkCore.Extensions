### TODO - Code review findings (October 2026)

Items are in priority order. "Reproduced" means a temporary test showed the problem against SQLite; the others were found by reading the code.

**Before release:** none of the SQL Server tests have been run since the upsert transaction fix (44e0d49) and the SQLite NOCASE change, because no SQL Server instance was available. Run the full suite against SQL Server first.

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

#### Checked for .NET 11 / EF 11 - no action needed

Recorded so these don't need checking again:
* **Free text search:** the existing `FreeTextSearch` translators (`FREETEXT` / `CONTAINS` on SQL Server, `MATCH` on SQLite) are unaffected by EF 11. EF 11's new `FreeTextTable` / `ContainsTable` (ranked search, SQL Server only) are out of scope for this upgrade.
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
