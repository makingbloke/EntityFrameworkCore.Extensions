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

#### 3. `ExecutePagedQueryAsync` SQL rewriting is fragile (reproduced)

* `SqlSplitOrderByRegex` / `SqlSplitColumnsRegex` in `Execute/ExecuteMethods.cs` don't use `RegexOptions.Singleline`, so SQL with line breaks throws "Unable to split query" (e.g. columns, `FROM` and `ORDER BY` on separate lines).
* Found testing the regexes directly:
  * An `ORDER BY` inside a subquery is treated as the outer one, producing broken SQL.
  * Anything after the `ORDER BY` columns (e.g. `OPTION (RECOMPILE)`) is silently dropped.
  * `SELECT DISTINCT` and `GROUP BY` queries get the wrong total count (`COUNT(*)` should wrap the query: `SELECT COUNT(*) FROM (<query>) AS t`).

##### Approach: wrap the SQL instead of splitting it, and use a small tokenizer

The solution must work for both SQLite and SQL Server, so Microsoft's T-SQL parser (`Microsoft.SqlServer.TransactSql.ScriptDom`) is ruled out - it only handles SQL Server.

* **Count:** wrap the query instead of replacing its column list: `SELECT COUNT(*) FROM (<sql without the outer ORDER BY>) AS t`.
  * This gives the right count for `DISTINCT`, `GROUP BY` and `UNION` queries (replacing the column list counts the rows before they are combined), and line breaks no longer matter.
  * Wrapping does not return the rows to the client - the database counts them internally and returns one value.
  * **The outer `ORDER BY` must be removed on both databases** (except when the query also has `TOP` / `LIMIT` - see edge cases).
* **Measured on SQLite 3.53.4** (2 million rows, `Notes` column of 200 characters, `upper(Notes)` in the select list, `WHERE Value < 500` matching 1 million rows):

  | Count query | Time | Plan |
  |---|---|---|
  | Column list replaced (current approach) | 13 ms | `SEARCH T USING COVERING INDEX IX_T_Value` |
  | Wrapped, no `ORDER BY` | 13 ms | Identical - SQLite merges the subquery and ignores the unused columns, so `upper(Notes)` is never evaluated |
  | Wrapped, with `ORDER BY Name` inside | 570 ms | `CO-ROUTINE t` + `USE TEMP B-TREE FOR ORDER BY` - SQLite does not drop the inner `ORDER BY`, so it sorts a million rows just to count them |
  | No `WHERE`, either form | 0 ms | Same plan |

* **SQL Server (not measured yet):** SQL Server should also expand the derived table into the outer query and remove unused columns. Check on SQL Server by comparing the actual execution plans of both forms with `SET STATISTICS IO ON` - the logical reads should match.
  * SQL Server also doesn't allow an `ORDER BY` in a derived table unless it has `TOP` or `OFFSET`, so the outer `ORDER BY` has to be removed anyway.
  * CTEs: the wrapper must go around the query after the `WITH ...` part (`WITH ... SELECT COUNT(*) FROM (<query>) AS t`), because SQL Server doesn't allow a CTE inside a derived table.
* **Paging:**
  * SQLite: trim any trailing `;`, then append `LIMIT {x} OFFSET {y}` on a new line (so a trailing `--` comment can't swallow it).
  * SQL Server: `OFFSET ... FETCH` has to go straight after the outer `ORDER BY`, before any `OPTION (...)` - or after an added `ORDER BY (SELECT NULL)` if there isn't one.
* **Tokenizer** (about 100 lines, no new dependency, works for both databases). It doesn't need to be a full parser, just find the clause keywords at the top level:
  * Skip `'strings'` (with `''` escapes), `"identifiers"`, `[identifiers]`, `` `identifiers` `` (SQLite), `-- line` comments and `/* block */` comments.
  * Track the bracket depth.
  * The outer `ORDER BY` is the last `ORDER BY` at depth 0. It ends at the next depth-0 `OPTION`, `FOR`, `OFFSET`, or the end of the text.
  * The `{0}` parameter placeholders aren't SQL, so the tokenizer just needs to treat them as ordinary text (or skip `{n}` tokens).
  * Edit the original string at the positions found, so the user's formatting and placeholders are kept.
* **Comments (`--` and `/* */`):** part of the tokenizer work - any SQL the library appends or wraps must not end up inside a user's comment, and keywords inside comments must be ignored.
  * Reproduced on SQLite: `SELECT * FROM TestTable1 ORDER BY Id -- all rows` with a page size of 3 returned all 10 rows, because the appended `LIMIT` ended up inside the comment. The SQL Server no-`ORDER BY` branch (`ORDER BY (SELECT NULL) OFFSET ...`) has the same problem.
  * Reproduced on SQLite: `ExecuteInsertAsync` with `INSERT INTO TestTable1 (TestField) VALUES ('B') -- new row` inserted the row but returned Id 0, because `;SELECT LAST_INSERT_ROWID()` (`GetLastInsertId` in `Execute/ExecuteMethods.cs`) ended up inside the comment. Same on SQL Server with `SELECT SCOPE_IDENTITY()`.
  * Quick fix until the tokenizer is done: start everything appended on a new line (`LIMIT`, `ORDER BY (SELECT NULL)`, and in `GetLastInsertId` the `;` and the `SELECT` on separate lines, since SQLite needs the `;` between statements). The count wrapper already puts its closing bracket on a new line.
  * Block comments: a closed `/* ... */` at the end is harmless when appending, but the tokenizer must skip their contents (e.g. `/* ORDER BY Name */` must not be treated as the outer `ORDER BY`). An unclosed `/*` should be rejected with a clear exception.
  * Trimming a trailing `;` must look past trailing comments too, e.g. `SELECT ...; -- note` (`TrimEnd(';')` alone doesn't handle this).
* **Edge cases to decide on:**
  * Outer query has `TOP` with an `ORDER BY`: removing the `ORDER BY` for the count is fine, because `TOP n` (and `TOP n PERCENT`) returns the same number of rows whatever the order. The exception is `TOP n WITH TIES`, which needs its `ORDER BY` (removing it is a syntax error, and the ties change the count). Keep the `ORDER BY` in that case - it's allowed inside a derived table when `TOP` is present.
  * SQL Server doesn't allow `TOP` and `OFFSET` / `FETCH` in the same query, so a `TOP` query can't be paged by appending `OFFSET`. Either wrap it for paging too, or treat it like an already-paged query and throw.
  * Query already has `OFFSET` / `FETCH` or `LIMIT`: throw a clear "already paged" exception.
  * More than one statement: throw.
* **Tests:** multi-line SQL, `ORDER BY` in a subquery, `OPTION (RECOMPILE)` after the `ORDER BY`, `DISTINCT`, `GROUP BY`, `UNION`, a CTE, `TOP` with `ORDER BY`, keywords inside strings / comments / quoted identifiers, a trailing `--` comment and a trailing `/* */` comment (paging and `ExecuteInsertAsync`), a trailing `;` followed by a comment, an unclosed `/*`, and an already-paged query - on both databases.

#### 4. `UniqueConstraintInterceptor` problems

* `SaveChangesFailed` blocks on `.Result`. On SQL Server this can run a `sys.indexes` query, so it can deadlock under a synchronisation context. Suggested fix: add a synchronous `GetUniqueConstraintDetails` to `IExceptionProcessor` (SQL Server version uses `SqlQuery(...).ToList()`) and a public sync extension method.
* If the lookup throws (broken connection, no permission on `sys.*`, doomed transaction under `XACT_ABORT ON`), that exception replaces the original `DbUpdateException`. Suggested fix: catch failures in the lookup (except `OperationCanceledException`) and fall back to the original exception, or throw `UniqueConstraintException` with the details parsed from the message and an empty field list.
* `SqliteExceptionProcessor.cs` uses `properties.First(...)`, which throws if a column doesn't map to a property. Use `FirstOrDefault` and fall back to the column name.
* SQL Server only detects error 2601 (unique index). Error 2627 (primary keys, unique constraints, and EF alternate keys from `HasAlternateKey`) has a different message and comes through as a plain `DbUpdateException`. Needs a second regex and a `sys.key_constraints` lookup for constraints not in the EF model.
* Both processors match English message text only. Detect violations by error code instead (`SqlException.Number is 2601 or 2627`, `SqliteException.SqliteExtendedErrorCode is 2067 or 1555`) and only parse the message for names.

#### 5. Upsert isn't safe when two run at the same time on SQL Server

* Under READ COMMITTED an UPDATE that matches no rows takes no range locks, so two concurrent upserts can both update nothing and both insert (duplicate row or unique constraint error).
* Suggested fix: `UPDLOCK, HOLDLOCK` on the update, or serializable isolation for the upsert transaction. (SQLite serialises writers so is not affected.)

#### 6. "Get rows" fallback paths return the wrong rows

These are used when the OUTPUT / RETURNING clause is turned off (e.g. tables with triggers).

* `UpdateGetRows` runs the same `WHERE` again after the update. If the update changes a column used in the filter, rows come back missing or wrong. With joins, `SELECT *` also returns the joined tables' columns.
* `InsertGetRow` finds the new row with `SCOPE_IDENTITY()` / `LAST_INSERT_ROWID()`, so it fails for GUID or manually set keys - and only throws after the row has been inserted. On SQLite, matching on `rowid` would avoid this.

#### 7. Table hints may be lost when EF regenerates SQL (fixed - waiting for a SQL Server test run)

* `SqlServerCustomQueryGenerator.GenerateTagsHeaderComment` called `tags.Remove(...)` on the cached query's own tag set. If EF generates the SQL again for a cached query (e.g. a parameter changes between null and non-null), the second SQL would have no `WITH (...)` hint.
* Fixed: `GenerateTagsHeaderComment` now passes the base class a copy of the tags without the table hints tag, and leaves the original set unchanged.
* Added `TableHintsTests_013_Async`, which runs the same hinted query with a null then non-null parameter and checks both SQL statements contain the hint.
* Still to do: run `TableHintsTests` against SQL Server. To confirm the original problem was real, temporarily put `tags.Remove(tableHintsTag)` back - the "non-null parameter" check should then fail. If it still passes, the fix was only defensive.
