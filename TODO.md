### TODO - Code review findings (October 2026)

Items are in priority order. "Reproduced" means a temporary test showed the problem against SQLite; the others were found by reading the code.

#### 1. "Get rows" fallback paths return the wrong rows

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
4. **Delete (smaller):** the fallback runs the `SELECT` and then the `DELETE` as two statements with no locking between them, so another connection could change the rows in between (a narrow gap, the same kind as the SQL Server upsert race fixed in 7.0.0).

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

### TODO - New features (November 2026)

#### 2. `Contains` ignores the NOCASE collation on SQLite

##### Problem

* **SQLite:** EF translates `string.Contains` to `instr(column, @p) > 0`. `instr` compares the characters exactly and ignores the column's collation (`NOCASE` set by `UseCaseInsensitiveCollation`) and `EF.Functions.Collate`. So with `UseCaseInsensitiveCollation`, `==` is case insensitive but `Contains` is case sensitive.
* **SQL Server:** `Contains` becomes `LIKE` / `CHARINDEX`, which both use the column's collation. So with `UseCaseInsensitiveCollation` it's already case insensitive, and the two databases give different results for the same query.
* **SQLite `LIKE` doesn't help.** It also ignores collations, and only ignores case for ASCII letters.

##### Option 1: a new method, `EF.Functions.ContainsNoCase(value, search)` (name to be decided)

* **SQL Server:** `CHARINDEX(@search, value COLLATE SQL_Latin1_General_CP1_CI_AS) > 0`. `CHARINDEX` rather than `LIKE`, so `%`, `_` and `[` in the search text don't need escaping.
* **SQLite:** a custom function registered on the connection, implemented with `CultureInfo.InvariantCulture.CompareInfo.IndexOf(value, search, options)`.
* **For:** simple, explicit, and it works on any column, whatever its collation.
* **Against:** the caller has to remember to use it, and a plain `Contains` stays case sensitive on SQLite.

##### Option 2: a variant of `instr` that takes a collation

* **Don't replace the built-in `instr`.** A SQLite custom function can't see the collation of its arguments, so a replacement couldn't follow the column's collation. Every `Contains` would become case insensitive, including on columns that aren't `NOCASE`.
* **Instead:** register a three-argument `instr(value, search, collation)`. SQLite tells functions apart by their number of arguments, so the built-in two-argument `instr` is untouched. Then add a translator that turns `string.Contains` into the new `instr` when the expression's collation is `NOCASE`: the column's collation in the model, or `EF.Functions.Collate(..., "NOCASE")`. The translator must run before EF's own SQLite string translator.
* **For:** a plain `Contains` just works, and gives the same results as SQL Server.
* **Against:** it changes what an existing query does (arguably a fix), and it needs more checking first. Can the translator see the column's collation from the model? If it can only see `EF.Functions.Collate`, this option is much less useful than option 1.

##### Suggestion

First check whether option 2's translator can see the column's collation. If it can, do option 2. If it can't, do option 1. Option 1 could also be added alongside option 2, for columns that aren't `NOCASE`.

##### Either way

* **Follow the NOCASE replacement's rules exactly.** That means the invariant culture with `IgnoreCase | IgnoreKanaType | IgnoreWidth`, so it's accent sensitive like `SQL_Latin1_General_CP1_CI_AS`. Keep the options in one place, shared with `SqliteNoCaseReplacementInterceptor.UnicodeNoCaseCompare`.
* **Register the function** in `SqliteNoCaseReplacementInterceptor.ConnectionOpened`, next to the collation. So it depends on `UseSqliteNoCaseReplacement()`, as the NOCASE rules do. Decide what happens without it: throw, or fall back to the built-in behaviour.
* **Match `instr`:**
  * `NULL` if either argument is `NULL`;
  * an empty search text matches.
* **Only use the result as true / false (`> 0`).** With `IgnoreCase`, `IndexOf` can match text of a different length, so the position isn't comparable with the built-in `instr`.
* **Check `StartsWith` / `EndsWith` on SQLite at the same time.** Depending on how EF translates them, they use `LIKE` (which ignores the collation) or a `substr(...) =` comparison (which follows it).
* **Performance:** the function runs for every row and can't use an index, the same as `instr` today.

##### Tests

On both databases, with the same values giving the same results:
* non-ASCII case (`É` / `é`) matches;
* accents (`e` / `é`) don't match;
* `NULL` and empty search text;
* `%`, `_` and `[` in the search text are treated as plain characters.

Option 2 also needs a test that a plain `Contains` on a column that isn't `NOCASE` stays case sensitive.

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
