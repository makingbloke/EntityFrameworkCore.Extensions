### TODO - Code review findings (October 2026)

Items are in priority order. "Reproduced" means a temporary test showed the problem against SQLite; the others were found by reading the code.

**Before release:** none of the SQL Server tests have been run since the upsert transaction fix (44e0d49) and the SQLite NOCASE change, because no SQL Server instance was available. Run the full suite against SQL Server first.

#### 1. EF's SQL cache ignores which ExecuteXxx method was called (critical, reproduced)

* `SqlServerCustomQueryGenerator` / `SqliteCustomQueryGenerator` choose INSERT, UPDATE or "get rows" SQL from the `ExecuteUpdateParameters` `AsyncLocal`, but EF caches the generated SQL by query shape, and the `AsyncLocal` is not part of the cache key. The first variant run decides the SQL for every later call with the same shape.
* Reproduced on SQLite:
  * `ExecuteUpdateGetCountAsync` then `ExecuteUpdateGetRowsAsync` (same shape): the cached plain UPDATE runs (and is applied), then it throws "The required column 'Id' was not present in the results of a 'FromSql' operation".
  * `ExecuteUpdateAsync` on a table then `ExecuteInsertAsync` (same shape): all existing rows were overwritten and nothing was inserted.
  * `ExecuteInsertAsync` then `ExecuteUpdateAsync` (same shape): a new row was inserted instead of updating.
* The upsert methods can hit this too: `ExecuteUpsertGetCountAsync` and `ExecuteUpsertGetRowsAsync` build the same insert query shape.
* Suggested fix: add the query type to the expression with a `TagWith` marker (as table hints already do) so each variant gets its own cache key, and have the generators read the tag instead of the `AsyncLocal`.

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

#### 7. Table hints may be lost when EF regenerates SQL (needs SQL Server to confirm)

* `SqlServerCustomQueryGenerator.GenerateTagsHeaderComment` calls `tags.Remove(...)` on the cached query's own tag set. If EF generates the SQL again for a cached query (e.g. a parameter changes between null and non-null), the second SQL would have no `WITH (...)` hint.
* Suggested fix: copy the tags to a new set before removing the table hints tag, and add a test that runs the same hinted query with a null then non-null parameter.
