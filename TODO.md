### TODO - Solution review (24 November 2026)

These came from a review of the whole solution on the `FreeTextSearchTest` branch at c4945ba. The library was read in full, area by area, along with the build and package files, the test infrastructure, the test classes and the docs.

Each item gives its severity and how it was checked:
* **reproduced**: run against the built library on SQLite, in a scratch app outside the repo;
* **read**: found by reading the code;
* **not run**: SQL Server only. There was no SQL Server to run it against, so it comes from reading the code and SQL Server's documented rules.

Checked and found fine, so these don't need looking at again:
* **SQL injection:** the raw SQL helpers pass caller values as parameters, including the paging values. `GetDelimitedIdentifier` and the SQLite stemming table name are delimited by EF Core's `ISqlGenerationHelper`.
* **The tokenizer:** its quote, escape and comment rules match both databases. Appended SQL always goes on a new line after the last token.
* **The custom query generators:** the generated SQL depends only on the expression tree, and a new generator is created for each query.
* **`ExecuteUpdateState`:** it is set and reset in the documented order.
* **`GetDbContext`:** its reflection is cached.
* **`IsUniqueConstraintException`:** its error codes are correct.
* **Packages:** none are vulnerable or deprecated.
* **The tests:** `[assembly: DoNotParallelize]` is in place, and they never use `CancellationToken.None`.

#### Bugs

3. **An upsert with chained `Where` calls fails when the row doesn't exist** (medium, reproduced).
   * **Why:** `RemoveWhereClauseVisitor` (`ExecuteUpdateExtensions.cs:543`) returns `node.Arguments[0]` without visiting it, so only the outermost `Where` is removed, and the insert then throws.
   * **Fix:** `this.Visit(node.Arguments[0])`.
4. **A global query filter (`HasQueryFilter`) breaks `ExecuteInsertAsync`, `ExecuteInsertGetRowAsync` and an upsert's insert** (medium, reproduced).
   * **Why:** the filter becomes a predicate, and `GenerateInsert` requires none.
   * **Fix:** use `IgnoreQueryFilters()` on the insert's source, since a filter means nothing to an INSERT.
5. **Upsert fails with a retrying execution strategy** (medium, reproduced with a custom retrying strategy on SQLite; it's the same EF Core code path as `EnableRetryOnFailure` on SQL Server).
   * **Why:** `BeginTransactionIfRequiredAsync` (`ExecuteUpdateExtensions.cs:506`) starts a transaction outside the strategy, and EF Core throws "does not support user-initiated transactions".
   * **Fix:** when the library owns the transaction, run the begin / update / insert / commit inside `CreateExecutionStrategy().ExecuteAsync(...)`.
6. **Upsert inside a `TransactionScope` or an enlisted transaction** (low-medium, not run).
   * **Why:** only `CurrentTransaction` is checked, so `BeginTransactionAsync` should throw EF Core's conflicting ambient / enlisted transaction error.
   * **Fix:** treat `Transaction.Current` or `GetEnlistedTransaction()` as a transaction the caller owns.
7. **On SQLite, the raw SQL `ExecuteInsertAsync` returns an unrelated id when nothing was inserted** (medium-high, reproduced).
   * **Why:** `SELECT LAST_INSERT_ROWID()` (`ExecuteMethods.cs:226`) returns the last insert on that connection, which pooling keeps between contexts.
   * **Example:** an `INSERT OR IGNORE` that hit a duplicate returned another table's id. SQL Server's `SCOPE_IDENTITY()` gives `default(T)` instead.
   * **Fix:** `SELECT CASE WHEN changes() > 0 THEN last_insert_rowid() END` (check it runs on the same connection as the insert).
   * **Document** that only identity / rowid keys are returned and that `default(T)` means nothing was inserted. On SQL Server, an insert through `EXEC` or an `INSTEAD OF` trigger also gives `default(T)`.
8. **`ExecutePagedQueryAsync` runs a `WITH ... INSERT ... SELECT` twice** (medium, reproduced).
   * **Why:** `FindQueryStart` (`SqlDialect.cs:364-373`) takes the first SELECT outside brackets after `WITH` as the query. So `WITH c AS (SELECT 'x' AS F) INSERT INTO T (F) SELECT F FROM c` isn't rejected as "Only SELECT queries can be paged". The count query and the page query each insert a row before the result mapping throws.
   * **Fix:** require the SELECT to follow a CTE's closing bracket directly, or reject INSERT / UPDATE / DELETE / MERGE / REPLACE / INTO outside brackets before it. Only `WITH ... DELETE` is tested today.
9. **On SQL Server, paging a `UNION` / `INTERSECT` / `EXCEPT` without an `ORDER BY` fails** (medium, not run).
   * **Why:** `SqlServerDialect.cs:85-88` adds `ORDER BY (SELECT NULL)`, which SQL Server rejects on a set operation (error 104). `SELECT DISTINCT` may hit error 145 the same way (unsure).
   * **Fix:** wrap the query in `SELECT * FROM (...) AS t` before adding the dummy `ORDER BY`, or throw asking for an `ORDER BY`.
10. **Table hints are added to every table in the query, not just the root** (medium, read).
    * **Why:** `SqlServerCustomQueryGenerator.cs:98-103` writes `WITH (...)` after every `TableExpression`, so a table-specific hint breaks any query with a join, `Include` or subquery.
    * **Example:** `Include(b => b.Posts).WithTableHints(SqlServerTableHints.Index("PK_Blogs"))` puts `INDEX([PK_Blogs])` on `[Posts]`. The same applies to `FORCESEEK(...)` and `NOEXPAND`.
    * **Fix:** apply hints to the root table only, or let a hint name its entity type. At minimum, document it and add a test with a join.
11. **The stemming table is only looked up on the column's own declaring entity type** (medium-low, reproduced).
    * **Where:** `SqliteFreeTextSearchTranslator.GetStemmingTableName` (`:91-103`) doesn't walk base types, owned types or complex types.
    * **What goes wrong:** with `SetStemmingTable` on a TPH base, a derived type's column throws "Stemming table not specified", and so do an owned type and a complex property in the same table.
    * **Fix:** look through `column.Table.EntityTypeMappings` and their base types for the annotation.
12. **On SQLite, `UseCaseInsensitiveCollation` misses the string properties of complex types** (medium, reproduced).
    * **Why:** `CaseInsensitiveCollationExtensions.cs:73-78` only walks the entity types' own properties, so `ComplexProperty(e => e.Addr)` gives `"Addr_City"` no `COLLATE NOCASE`, and `==` / `Contains` are case sensitive there. On SQL Server, the database collation covers them.
    * **Fix:** walk `GetComplexProperties()` recursively.
    * **The translator has the same gap:** `GetCollation` only looks up entity type properties (in CLAUDE.md, but not in the public docs). Either support complex types there too, or document it.
13. **NOCASE is lost through a scalar subquery or `Min` / `Max`** (medium-low, reproduced).
    * **Example:** `p.Children.OrderBy(...).Select(c => c.Name).FirstOrDefault()!.Contains("école")` and `p.Children.Max(c => c.Name).Contains(...)` use the built-in `instr`, so they're case sensitive where SQL Server isn't.
    * **Fix:** in `SqliteNoCaseTranslator.GetCollation` (`:481-493`), add `ScalarSubqueryExpression` (its single projection) and one-argument `MIN` / `MAX`.
    * **Document:** `==` on a scalar subquery is case sensitive on SQLite anyway, because SQLite gives a subquery's result no collation, and the translator can't fix that.
14. **A `COLLATE NOCASE` added by the library leaks into an enclosing `CASE` or function result** (low, reproduced, only with mixed collations).
    * **Why:** SQLite gives an expression the explicit collation of any COLLATE inside it, including in a `CASE`'s `WHEN` test.
    * **Example:** `(e.Name.Trim() == "abc" ? e.Code : "zzz") == "CODE"` with a BINARY `Code` finds the row; without the library, and on SQL Server, it doesn't.
    * **Fix:** at least document it with the mixed-collation notes in the `UseCaseInsensitiveCollation` remarks.
15. **A leading `;WITH` is rejected as "The SQL must contain a single statement"** (low, reproduced).
    * **Where:** `SqlDialect.cs:164-170` skips trailing semicolons only, but `;WITH` is a common T-SQL habit.
    * **Fix:** skip leading semicolons too.
16. **`PageCount` overflows for a very large `pageSize`** (low, reproduced).
    * **Example:** `(recordCount + pageSize - 1) / pageSize` (`ExecuteMethods.cs:102`) gives -1 for `pageSize = long.MaxValue`.
    * **Fix:** `recordCount / pageSize + (recordCount % pageSize == 0 ? 0 : 1)`.
17. **On SQL Server, the count query fails when a column has no name** (low, not run).
    * **Why:** the count wraps the query in a derived table, which needs every column named (error 8155), e.g. `SELECT Id, Id + 1 FROM T`. The page query and SQLite are fine.
    * **Fix:** document it, or throw a clearer exception.

#### Security

18. **Table hint text is written into the SQL as it is** (low, read).
    * **Where:** whatever follows `$$TABLEHINTS$$` in any tag goes straight into `WITH (...)` (`SqlServerCustomQueryGenerator.cs:403-409`, `CustomTagExtensions.cs`). `ITableHint` is public, so any implementation's `ToString()` becomes SQL.
    * **The risk:** with the library installed, an ordinary `TagWith(userText)` starting with `$$TABLEHINTS$$` becomes SQL rather than a comment.
    * **Fix:** validate the hints when the tag is read (known keywords, delimited identifiers, integers), and consider stopping callers implementing `ITableHint` themselves.

#### Performance

19. **`nocase_like` with an escape character allocates for every row, and again every time `%` backtracks** (low-medium, measured).
    * **Where:** `SqliteUnicodeMethods.cs:398` and `:479-525` (`TryReadLikeLiteral`).
    * **Measured:** `Like("%a!_b%", <2,000 characters>, "!")` over 100 rows allocated 26.5 MB; the same without an escape allocated nothing.
    * **Fix:** unescape the pattern once per call, before the matching loop. The per-row `Encoding.UTF8.GetByteCount(pattern)` can be skipped when `pattern.Length * 3` is under the limit.
20. **`ExecuteUpdateInterceptor` resolves `ExecuteUpdateState` for every non-query command** (low, read).
    * **What happens:** migrations, `ExecuteSqlRaw` and plain `ExecuteUpdate` all pay for it before `CommandSource` is checked (`ExecuteUpdateInterceptor.cs:31`).
    * **Fix:** check `eventData.CommandSource` first.

#### Design and docs

21. **The NOCASE replacement's ordering can differ between processes, ICU versions and globalization modes** (medium, the invariant mode differences reproduced).
    * **Why it matters:** NOCASE indexes (including primary keys and unique indexes) are stored in the order of the .NET / ICU comparison. A different order makes lookups wrong and `integrity_check` fail.
    * **What can give a different order:**
      * another process writing the database without the replacement (the sqlite3 CLI, DB Browser, a migrations bundle);
      * an OS or .NET upgrade that changes ICU;
      * invariant globalization mode (`InvariantGlobalization`, common in chiseled containers and AOT), where `Compare("é", "f")` flips and precomposed / decomposed characters and ignorable characters no longer compare equal;
      * the same applies to `lower` / `upper` in expression indexes.
    * **Raw ADO commands:** a command run on `GetDbConnection()` before EF Core's first command uses the built-in NOCASE, as the interceptor registers in `CommandCreated`.
    * **Fix:**
      * document that every process writing the database must register the replacement;
      * document running `REINDEX NOCASE` after first enabling it and after .NET / ICU / OS upgrades (today only the ChangeLog's upgrade note mentions `REINDEX`);
      * detect invariant mode (`AppContext.TryGetSwitch("System.Globalization.Invariant", ...)`) and throw, or document it;
      * optionally register in `ConnectionOpened` as well (in addition to `CommandCreated`, which is still needed for a connection that was already open).
22. **`FreeTextSearch` without stemming treats several words differently on the two databases** (low, not run).
    * **Why:** on SQL Server, `CONTAINS(col, 'apple pie')` is a full-text syntax error, while SQLite's `MATCH` treats it as an implicit AND.
    * **Fix:** document that the text must be a valid search condition on each database, or turn plain words into `"apple" AND "pie"`.
23. **Misleading errors and copied parameter docs on the insert methods** (low, read).
    * **Errors:** the generators reuse `RelationalStrings.ExecuteOperationWithUnsupportedOperatorInSqlGeneration`, which tells a caller who put a `Where` on `ExecuteInsertAsync` that it's "a bug in your EF Core provider".
    * **Docs:** the insert methods' `source` / `setPropertyCalls` docs are copied from `ExecuteUpdate` ("to test for a condition", "properties to update").
    * **Fix:** use the library's own message, and correct the docs.
24. **The `TOP ... WITH TIES` count logic can't be reached** (low, read).
    * **Why:** `KeepOrderByInCount` (`SqlServerDialect.cs:133-138`, `SqlDialect.cs:279-287`) only applies with `TOP`, but `BuildPagedSql` throws for any `TOP` and `ExecutePagedQueryAsync` builds both queries first. Only `SqlDialectTests` reach it.
    * **Fix:** remove it, or check for `TOP` once in `Analyse`.
25. **`DoesDatabaseExistAsync` throws an undocumented `InvalidOperationException` for a non-relational provider** (low, read).
    * **Fix:** use an `IsRelational()` guard, as `GetDelimitedIdentifier` does.
26. **ReadMe** (low).
    * The FreeTextSearch section never says to call `UseFreeTextSearchExtensions()`, so following it gives "could not be translated".
    * Line 93 says "as the table hints ... do" in a SQLite note, but table hints are SQL Server only.
28. **ChangeLog 7.0.0.0** (low).
    * A few entries describe the implementation rather than the symptom: the EF Core fix and removed workaround, and "Added a new SQL Parser", where the entry before it already gives the symptom.
    * "rather, than" is a typo.
    * The release date is still "xxth November 2026".

#### Tests

29. **The SQL Server full text wait can return before the new row is indexed** (medium, likely flaky).
    * **Why:** `DatabaseUtils.cs:207-236` waits for the catalog's `PopulateStatus` to be idle, which it already is between the insert and the background crawl.
    * **Fix:** poll `OBJECTPROPERTYEX(OBJECT_ID('TestFreeText'), 'TableFulltextPendingChanges') = 0`, or poll the search itself, with a timeout.
30. **`CreateGuidTests_003` can't tell whether the GUIDs are sequential** (medium).
    * **Why:** it reads the rows back ordered by `Id` and checks that they're ascending, which is always true, so `Guid.NewGuid()` would pass.
    * **Fix:** create the GUIDs in a known order and check that the database's `ORDER BY Id` returns that order.
31. **Exception tests that can pass for the wrong reason** (low).
    * `FreeTextSearchFunctionTests_006` checks only the exception type, so a "could not be translated" error would pass. Check the message, as `_020` does.
    * `_003` uses `IsInstanceOfType`, so an `ArgumentNullException` would pass as an `ArgumentException`. Compare the exact type.
32. **Coverage gaps** (low).
    * `ExecuteDeleteGetCountTests_002` never checks that the rows were deleted.
    * `ExecuteUpdateGetCountTests_003` never checks that rows outside the filter were left alone.
    * Nothing covers `ExecuteDelete` / `ExecuteUpdate` with a SQLite stemming search changing the stemming table.
    * Each fix above should add its own test.
34. **The SQL Server connection string is hard-coded, and the SQLite tests can't run on their own** (low).
    * **Where:** `DatabaseUtils.cs:98` and `DoesDatabaseExistTests.cs:84`.
    * **Why:** there's no override for CI or a container, no `[TestCategory]`, and `SqliteSqlServerEquivalenceTests`' `[ClassInitialize]` always opens SQL Server.
    * **Fix:** read an environment variable or runsettings value with the current string as the default, and tag the SQL Server tests.
