### TODO - Solution review (27 November 2026)

These came from a second review of the whole solution, on `Development` at 99c6170, after every item from the 24 November review had been fixed. The library, the test project, the docs and the build and package files were reviewed area by area.

Each item gives its severity and how it was checked:
* **reproduced**: run against the built library on SQLite, in a scratch app outside the repo (or by running the tests);
* **read**: found by reading the code;
* **not run**: SQL Server only. There was no SQL Server to run it against, so it comes from reading the code and SQL Server's documented rules.

Checked and found fine, so these don't need looking at again:
* **ExecuteUpdate state and interceptor:** `ExecuteUpdateState` is checked, set and reset in the documented order, the pooled context reset works, and the interceptor only captures ExecuteUpdate / ExecuteDelete commands.
* **SQL cache:** the ExecuteXxx variant and the table hints are both tags in the expression, and a new generator is made for each command.
* **Tokenizer and paging:** quotes and escapes (`''`, `N''`, `[ ]` with `]]`, `""`, SQLite backticks), nested and unclosed comments, brackets, `;WITH`, CTEs, `ORDER BY` in brackets or window functions, `OPTION` / `FOR XML`, the paging maths, parameter reuse between the count and page queries, and nulls.
* **Other utilities:** `CreateGuid`, `GetDelimitedIdentifier`, `DoesDatabaseExistAsync`, the translator plugin registration, the `Match` translator and the SQLite stemming alias lookup.
* **NOCASE replacement:** the `KeepCollation` interceptor didn't break or change any query tried; NULLs, trailing spaces, sort order, `lower` / `upper` with surrogate pairs, `nocase_instr` positions, LIKE escapes and backtracking, `Replace` with an empty search, and the once per connection registration.
* **Docs and package:** every class, method and test named in CLAUDE.md exists; every public API change since 6.0.0.16 has a ChangeLog entry; the DefaultDocumentation output is complete; the package has the right id, version, licence, ReadMe, XML docs and dependencies.
* **Tests:** every test has a three part name, a matching DisplayName and the documented region layout, and none use `async void` or static mutable state.

#### Bugs

1. **`ExecuteDeleteGetRowsAsync`, `ExecuteUpdateGetRowsAsync` and `ExecuteUpsertGetRowsAsync` fail for every TPT or TPC entity type** (medium, reproduced).
   * **Why:** `ReadRowsAsync` (`ExecuteUpdateExtensions.cs`, around line 488) reads the rows back through the root type with `FromSqlRaw`, which EF Core rejects for a hierarchy without a discriminator.
   * **Example:** `context.Set<Dog>().Where(d => d.Name == "Rex").ExecuteDeleteGetRowsAsync()` with `Animal.UseTpcMappingStrategy()` throws "Using 'FromSqlRaw' on DbSet of 'Animal' is not supported...". Nothing is changed, but the error means nothing to the caller and isn't documented.
   * **Fix:** check for it next to `ThrowIfOwnedTypes` and throw a documented `NotSupportedException` before anything runs.
2. **`ExecuteInsertAsync`, `ExecuteInsertGetRowAsync` and the upserts don't handle TPH** (medium, reproduced).
   * **Why:** the generated INSERT never sets the discriminator, and on a derived type EF Core's discriminator filter becomes a `Where`, which `ThrowIfInsertSourceNotTable` rejects.
   * **Examples:**
     * `context.Set<Animal>().ExecuteInsertAsync(...)` fails with "NOT NULL constraint failed: Animal.Discriminator" unless the caller sets the shadow property themselves.
     * `context.Set<Dog>().ExecuteInsertAsync(...)` always throws "must be called on the table itself without Where...", though the caller wrote no `Where`.
     * `context.Set<Dog>().Where(...).ExecuteUpsertGetCountAsync(...)` works while the row exists, and throws that message once it doesn't (the transaction is rolled back).
   * **Fix:** add the discriminator column and value in `GenerateInsert` when the caller didn't set it, and accept a predicate that is only the discriminator filter; or throw a documented `NotSupportedException` for types with a discriminator.
3. **On SQLite, the raw SQL `ExecuteInsertAsync` still returns an earlier insert's rowid in two cases** (medium, reproduced).
   * **Why:** the TODO 7 fix (`SELECT CASE WHEN changes() > 0 THEN last_insert_rowid() END`, `ExecuteMethods.cs:238`) assumes a changed row means the caller's SQL set `last_insert_rowid()`. An upsert that takes the `ON CONFLICT ... DO UPDATE` path changes a row without inserting one, and an insert into a `WITHOUT ROWID` table never sets `last_insert_rowid()`.
   * **Example:** after `INSERT INTO A ...` returns 4, an `INSERT INTO B ... ON CONFLICT(K) DO UPDATE ...` that updates returns 4, and so does an insert into a `WITHOUT ROWID` table. SQL Server's `SCOPE_IDENTITY()` gives NULL for the equivalent.
   * **Fix:** SQLite can't reset `last_insert_rowid()` from SQL, so document both cases in the remarks (use `RETURNING` with `ExecuteScalarAsync`), and add a test for the upsert case.
4. **With `UseSqliteNoCaseReplacement`, a NOCASE column searched for inside a constant or parameter is case sensitive** (medium, reproduced).
   * **Why:** `SqliteNoCaseTranslator` (lines 111-123) only checks the collation of the string being searched (the instance). SQL Server gives a column's collation priority over a constant's or parameter's, so `CHARINDEX(col, @p)` uses the column's case insensitive collation.
   * **Example:** with `Name = "Body ÉCOLE"` and `search = "BODY ÉCOLE!"`, `search.Contains(e.Name)` gives false, `search.IndexOf(e.Name)` gives -1 and `"a BODY ÉCOLE b".Replace(e.Name, "_")` is unchanged, while `StartsWith` / `EndsWith` already ignore case. SQL Server ignores case in all of them.
   * **Fix:** when the instance has no known collation, use the collation of the search argument (`oldValue` for `Replace`), and change `Translate_NoCaseColumnInsideConstant_KeepsEfCoreTranslation`, which asserts the current behaviour.
5. **With `UseSqliteNoCaseReplacement`, a computed NOCASE value read from a subquery, union or `GroupBy` key loses NOCASE for the string methods** (medium, reproduced).
   * **Why:** when EF Core pushes a concatenation, `??`, conditional, `Trim` / `Replace` / `ToLower` result or `EF.Functions.Collate` into a subquery, the outer column has no `Column`, so `GetCollation` (line 509) finds no collation. `==` still ignores case, as SQLite keeps the explicit `COLLATE` through the subquery.
   * **Example:** `GroupBy(e => e.Name + "!").Select(g => g.Key).Count(k => k.StartsWith("body é"))` gives 0 (SQL Server 1), and so does `Select(e => e.Name + "!").Union(...).Count(k => k.Contains("école"))`.
   * **Fix:** needs the subquery's projection, which the translator can't see. At least document it and add tests; a full fix would record the collation of projected NOCASE expressions somewhere the translator can look it up.
6. **On SQLite, `UseCaseInsensitiveCollation` skips text columns whose CLR type isn't `string`** (medium-low, reproduced).
   * **Why:** `CaseInsensitiveCollationExtensions.cs:83` checks `p.ClrType == typeof(string)`, so a property stored as text through a value converter (a strongly typed id, an enum stored as a string) or a `char` gets no NOCASE. On SQL Server the database collation applies to all of them.
   * **Example:** a `record CodeId(string Value)` with `HasConversion(...)` storing `"ABC"`: `Count(e => e.Code == new CodeId("abc"))` gives 0 (SQL Server 1); `char Grade = 'A'` with `e.Grade == 'a'` gives 0.
   * **Fix:** check `(p.GetProviderClrType() ?? p.ClrType)` for `string` (and `char`), and update the remarks ("every string property").
7. **The NOCASE replacement probably doesn't match SQL Server for ß, æ and œ** (medium; SQLite side reproduced, SQL Server not run).
   * **Why:** .NET's ICU comparison treats `'ß'` and `'ss'` (and `'æ'` / `'ae'`, `'œ'` / `'oe'`) as different, so `'straße' = 'STRASSE' COLLATE NOCASE` is false and `nocase_instr('Straße x', 'SS')` is 0. SQL Server's Latin1_General collations are understood to treat ß as ss, which also affects unique indexes; æ and œ are less certain.
   * **Fix:** add these rows to `NoCaseCollation_Compare_MatchesSqlServerCollation` and `NoCaseInstr_Search_MatchesSqlServerCharIndex` and run them on SQL Server. If they differ, document it, or expand these characters before comparing (which changes the index order, so needs `REINDEX NOCASE`).
8. **On SQLite, `ExecutePagedQueryAsync` takes a `:limit` or `$limit` parameter for a LIMIT clause** (medium-low, reproduced).
   * **Why:** `:` and `$` aren't word starts (`SqlDialect.cs:302`), so `:limit` is read as `:` followed by the word `limit`, which `SqliteDialect.GetTrailingClause` treats as paging. `@limit` works.
   * **Example:** `ExecutePagedQueryAsync<Row>("SELECT Id, Name FROM A WHERE Id < :limit", ...)` throws "The query is already paged ('limit' clause)"; the same SQL works with `ExecuteQueryAsync`.
   * **Fix:** in `SqliteDialect`, read `:name`, `$name` and `?NNN` as one token, and add `SqlDialectTests` rows.
9. **On SQL Server, `WithTableHints` silently drops the hints for an entity mapped with both `ToTable` and `ToView`** (low-medium, reproduced with `ToQueryString`).
   * **Why:** `TableHintsBuilder.For` (lines 116-119) keys the hints on `GetTableName() ?? GetViewName()`, but EF Core reads such an entity from the view.
   * **Example:** `Set<Viewed>().WithTableHints(h => h.For<Viewed>(NoLock))` gives `SELECT ... FROM [ViewedView] AS [v]` without `WITH (NOLOCK)`, and no error. ExecuteUpdate / ExecuteDelete on the same query do get the hints, as they use the table.
   * **Fix:** register the hints under both the table and the view key when both exist.
10. **The upserts' default insert values break with the usual increment setter** (low, reproduced).
    * **Why:** with no `insertSetPropertyCalls`, the update setters are reused for the INSERT, and a setter that reads a column (`SetProperty(x => x.Value, x => x.Value + 1)`) becomes `VALUES ("c"."Value" + 1)`.
    * **Example:** SQLite fails with "no such column: c.Value" when the row doesn't exist (SQL Server would give "multi-part identifier could not be bound", not run).
    * **Fix:** in `GenerateInsert`, throw an `InvalidOperationException` naming the property when a value reads a column, and say in the `insertSetPropertyCalls` docs that the values can't read the row's columns.
11. **On SQLite, the GetRows methods fail on virtual tables, including the FTS tables FreeTextSearch uses** (low, reproduced).
    * **Why:** SQLite doesn't allow `UPDATE ... RETURNING` or `DELETE ... RETURNING` on a virtual table ("DELETE RETURNING is not available on virtual tables"); `INSERT ... RETURNING` works. It always applies when a stemming search swaps in the stemming table. SQL Server's `OUTPUT` has no such limit. Nothing is changed.
    * **Fix:** document it on `ExecuteDeleteGetRowsAsync`, `ExecuteUpdateGetRowsAsync` and `ExecuteUpsertGetRowsAsync`, and perhaps throw a clear `NotSupportedException` when the generator writes a stemming table in one of them.
12. **On SQL Server, a column named `Offset` in the outer `ORDER BY` is taken for a paging clause** (low, `Analyse` reproduced; SQL Server not run).
    * **Why:** `SqlServerDialect.GetTrailingClause` (line 123) checks every top level token after `ORDER BY`. `OFFSET` isn't a reserved word in T-SQL, so `ORDER BY Offset` is valid.
    * **Example:** `SELECT Id, Offset FROM T ORDER BY Id, Offset DESC` throws "The query is already paged ('Offset' clause)".
    * **Fix:** only treat `OFFSET` as paging when a top level `ROW` / `ROWS` follows it.
13. **`IsUniqueConstraintException` misses SQLite's `SQLITE_CONSTRAINT_ROWID`** (low, reproduced).
    * **Example:** inserting a duplicate rowid into a table without an INTEGER PRIMARY KEY gives extended code 2579 ("UNIQUE constraint failed: R.rowid"), and `IsUniqueConstraintException()` returns false.
    * **Fix:** add `SQLITE_CONSTRAINT_ROWID` to the check (`UniqueConstraintExtensions.cs:46`).
14. **`GetDbContext(IQueryable)` throws an undocumented `InvalidCastException` for a non-relational provider** (low, read).
    * **Why:** `GetDbContextExtensions.cs:78-79` casts to `QueryCompiler` and `RelationalQueryContextFactory`; with the in-memory provider the factory is an `InMemoryQueryContextFactory`. This also reaches the ExecuteUpdate and TableHints methods that call it.
    * **Fix:** check with `is` and throw a documented `ArgumentException`, as `ThrowIfNonRelationalDatabase` does.
15. **A non-constant `useStemming` or `languageTerm` in `FreeTextSearch` gives an `InvalidCastException`** (low, read).
    * **Why:** `[NotParameterized]` only inlines captured values; an entity member (e.g. `FreeTextSearch(x.Text, s, x.Flag)`) arrives as a column, and `FreeTextSearchArguments.cs:54,58` casts it to `SqlConstantExpression`.
    * **Fix:** use a pattern match and throw an `InvalidOperationException` saying the argument must be a constant.
16. **On SQL Server, `ExecutePagedQueryAsync` rejects `WITH XMLNAMESPACES (...) SELECT ...`** (low, reproduced).
    * **Why:** `SkipCommonTableExpressions` (`SqlDialect.cs:380-431`) expects `name [(columns)] AS (`, and `XMLNAMESPACES` has no `AS`, so a valid SELECT throws "Only SELECT queries can be paged".
    * **Fix:** skip `XMLNAMESPACES (...)` (and a following comma) on SQL Server, or document that it isn't supported.
17. **With `UseSqliteNoCaseReplacement`, `Trim` / `TrimStart` / `TrimEnd` with a `char` variable runs in .NET, case sensitively** (low, reproduced).
    * **Why:** `SqliteNoCaseTranslator.cs:403-413` only accepts constant characters, so a variable falls back to EF Core, which trims on the client in a projection (and can't translate it in a `Where`).
    * **Example:** `e.Name.TrimStart('b')` gives "ody ÉCOLE" (in SQL, ignoring case); with `char ch = 'b'`, `e.Name.TrimStart(ch)` gives "Body ÉCOLE".
    * **Fix:** pass a single `char` parameter to `nocase_ltrim` etc. (Microsoft.Data.Sqlite binds `char` as text), or document it.
18. **On SQL Server, the INSERT column list uses alias qualified names** (low, not run).
    * **Why:** `SqlServerCustomQueryGenerator.cs:292` visits the column, giving `INSERT INTO [Counter] ([c].[Key], [c].[Value])`, where `[c]` doesn't exist in an INSERT. SQL Server appears to ignore the prefix (the SQL Server insert tests should show whether it does), but it relies on undocumented parsing. The SQLite generator writes bare names.
    * **Fix:** write `DelimitIdentifier(setter.Column.Name)`, as the SQLite generator does.

#### Documentation

19. **The ChangeLog understates the `UseExecuteUpdateExtensions()` check** (medium, read).
    * **Why:** ChangeLog.md:7 says "the `ExecuteUpdate` extension methods" check, but `ExecuteDeleteGetCountAsync` / `ExecuteUpdateGetCountAsync` don't, and it doesn't say that without the call, `ExecuteInsertAsync` used to update every row in the table instead of inserting one.
    * **Fix:** "** Breaking Change ** `ExecuteDeleteGetRowsAsync`, `ExecuteInsertAsync`, `ExecuteInsertGetRowAsync`, `ExecuteUpdateGetRowsAsync` and the `ExecuteUpsert` methods now throw an `InvalidOperationException` if `UseExecuteUpdateExtensions()` hasn't been called. Previously `ExecuteInsertAsync` updated every row in the table instead of inserting one."
20. **The ReadMe has no 7.0.0.0 upgrade notice for the biggest breaking changes** (medium, read).
    * **Why:** the only 7.0.0.0 notice is for `IsUniqueConstraintException`. Callers will also hit: the `WithTableHints` signature change (every call stops compiling); `UseCaseInsensitiveCollation` now needing SQL Server 2019 and giving collation conflicts on existing databases; `REINDEX NOCASE` on SQLite; and the `CaseInsensitivity` namespace move.
    * **Fix:** a short "Upgrading to 7.0.0.0" section, one line each, pointing to the ChangeLog.
21. **The ReadMe points to a method that no longer exists** (low-medium, read).
    * **Why:** ReadMe.md:60 says to see `CreateDatabaseAsync` in DatabaseUtils.cs. The SQL Server catalogue and index are set up in `Context.OnModelCreating` (`HasFullTextCatalog` / `HasFullTextIndex`) and the SQLite FTS tables in `DatabaseUtils.SqliteInitialiseFreeTextAsync`. The ReadMe also ships in the NuGet package, where relative references like this (and "License.txt in the solution root") don't work.
    * **Fix:** name the right places, ideally as GitHub links.
22. **ChangeLog wording** (low, read).
    * **Line 63:** names `UseCustomQueryGenerator()`, which is internal; callers saw the change through `UseExecuteUpdateExtensions()`, `UseFreeTextSearchExtensions()` and `UseMatchExtensions()`.
    * **Lines 68 and 81:** describe the implementation (line 68's "Why" bullet; line 81's "A query without an `ORDER BY` is now wrapped in a derived table...").
    * **Lines 65 and 89 overlap:** both cover `ExecuteInsertAsync` with a `Take`, and line 89's "Previously the message said..." doesn't fit SQL Server, where line 65 says it previously ran as `INSERT TOP (n)`. Merge them.
    * **Typos:** line 5 "where, an exception" (no comma); line 22 "SQLite error messages are always in English but, for SQL Server" should be "...in English, but for SQL Server".
23. **Doc comments** (low, read).
    * **The upsert remarks** (`ExecuteUpdateExtensions.cs:342-352` and 414-424) explain why MERGE / ON CONFLICT weren't used, which belongs in a `//` comment (CLAUDE.md already calls it "the code comment").
    * **The raw SQL `ExecuteInsertAsync`** overloads don't document the `InvalidCastException` / `FormatException` that `ExecuteScalarAsync` documents, though they go through the same conversion.
24. **CLAUDE.md and ReadMe tidy-ups** (low, read).
    * CLAUDE.md's `Utilities/` list leaves out the internal `RelationalDatabaseExtensions` (`ThrowIfNonRelationalDatabase`).
    * The ReadMe's FreeTextSearch, IsUniqueConstraintException and SQLite NoCase sections are `###` headings, so they sit under `## Limitations`.

#### Tests

25. **`ServiceRegistration_UseExecuteUpdateExtensions_RegistersOneStatePerContext` can't fail** (high, read).
    * **Why:** `Context.OnConfiguring` calls `EnableServiceProviderCaching(false)`, so every context gets its own service provider, and the two states would differ even if `ExecuteUpdateState` were a singleton. No test runs two contexts on one shared provider, so a state leak between contexts would go unnoticed.
    * **Fix:** build both contexts from one set of options with caching on, and add a test that interleaves two contexts' GetRows calls.
26. **A database that's turned off makes some tests fail instead of being inconclusive** (medium-high, reproduced with an empty connection string variable).
    * **Why:** when `OpenDatabaseAsync` (or `ExecuteSqlAsync`) runs inside an `Assert.Throws*` lambda, its `Assert.Inconclusive` is caught as the wrong exception.
    * **Tests:**
      * SQL Server off: `UseSqliteNoCaseReplacement_SqlServer_ThrowsUnsupportedDatabaseTypeException`, `UseMatchExtensions_SqlServer_ThrowsUnsupportedDatabaseTypeException`;
      * SQLite off: `UseTableHintExtensions_Sqlite_...`, `UseCaseInsensitiveCollation_UnsupportedDatabaseType_...`, `SetStemmingTable_NullOrEmptyTableName_...`, and `SqliteNoCaseFunctionsTests` `NoCaseLike_InvalidEscape` / `PatternTooLong` / `NoCaseTrim_OneArgument`.
    * **Fix:** get the connection string before the assert; the two SQL Server tests don't need a server, so give them a dummy connection string.
27. **`SqliteSqlServerEquivalenceTests` loses its SQLite coverage when SQL Server isn't available** (medium, read).
    * **Why:** `ClassInitializeAsync` opens SQL Server, so all 110 rows are skipped or fail without it, though some SQLite behaviour is only tested there (kana type, decomposed accents, width for trim / replace / startswith / LIKE, `[` in startswith).
    * **Fix:** move those rows into the SQLite only classes, or run the SQLite half on its own.
28. **The test `Context` always calls `UseFreeTextSearchExtensions()`** (medium, read).
    * **Why:** on SQLite this installs the custom query generator and the FreeText plugin for every test, so if `UseExecuteUpdateExtensions()` or `UseSqliteNoCaseReplacement()` stopped installing what they need, nearly all SQLite tests would still pass. It also means the "Match first" row of `Match_WithOtherTranslations_TranslatesAll` never registers Match first.
    * **Fix:** make FreeText opt in through a `Context` parameter.
29. **Tests that only generate SQL open a real database** (medium, read).
    * **Why:** `TableHintsTests` (`EmptyTableHints`, `NullTableHint`, `EachTableHint` with 28 rows, `MultipleTableHints`, `SqlGeneratedAgain`), `GetDatabaseType_DbContextOptions`, `ServiceRegistration_*` and the SQL Server rows of `GetDbContext_*` / `GetDelimitedIdentifier_*` open the database. Without SQL Server, none of the table hint SQL is tested; with it, each row drops and recreates the database and full-text catalog.
    * **Fix:** use the dummy connection string pattern already used by `WithTableHints_View_AddsHintsToView`.
30. **SQL Server test isolation** (medium-low, read).
    * **Why:** the database name is fixed (`DatabaseUtils.TestDatabaseName`), so two runs against one server (command line and Visual Studio, two branches, CI) drop each other's database. The `createNew: false` SQL Server rows (`ExecuteScalarAsync_DateTimeOffset` / `TimeSpan` / `DateOnly` / `TimeOnly` / `Enum` / `ResultWithDefaultTypeMapping` / `NullAsNullableGuid`) fail when run on their own against a server without the database.
    * **Fix:** a database name per run, and don't rely on `createNew: false` on SQL Server.
31. **Exception assertions that are too loose** (low-medium, read).
    * `Assert.ThrowsAsync<Exception>` plus `IsInstanceOfType` lets an `ArgumentException` row pass with `ArgumentNullException` or `ArgumentOutOfRangeException`: the `Execute{Insert,NonQuery,Query,Scalar,PagedQuery}Async_*StringSql*` tests, `ExecutePagedQueryAsync_InvalidOrUnpageableSql_Throws`, `SqlDialectTests.Analyse_InvalidOrUnpageableSql_Throws` and the four `GetDelimitedIdentifier_*InvalidName` tests. Compare `e.GetType()` exactly, as the FreeText and TableHints tests do.
    * `ExecutePagedQueryTests` lines 134, 152, 172 and 192: use `ThrowsExactlyAsync<ArgumentOutOfRangeException>`.
    * `IsUniqueConstraintException_SqlOtherDatabaseError_ReturnsFalse` / `EfCoreOtherDatabaseError_ReturnsFalse` accept any database error, so they'd pass if the SQL broke; assert the NOT NULL error code (SQLite 1299, SQL Server 515).
32. **The GetRows happy path tests check the returned rows but not the table** (low-medium, read).
    * `ExecuteDeleteGetRowsAsync_Where_...` doesn't check the rows are gone and the others are left; `ExecuteUpdateGetRowsAsync_Where_...` doesn't check the stored values or that other rows are untouched; `ExecuteInsertGetRowAsync_Setters_...` doesn't check the row was stored.
    * `ExecuteDeleteGetRows` has no "fails, then the state is cleared" test, unlike Update and Insert.
33. **Smaller test problems** (low, read).
    * `ExecuteInsertAsync_SqliteInsertIgnored_ReturnsDefault` relies on connection pooling to reuse the connection; with `Pooling=False` it would pass against the old bug. Open the connection first.
    * `NoCaseLike_ManyWildcardsNoMatch` / `LongestAllowedPattern` use `[Timeout(..., CooperativeCancellation = true)]`, but `SqliteUnicodeMethods.Like` never sees the token, so a slow regression would hang the run rather than time out.
    * `DoesDatabaseExistAsync_DatabaseMissing` (SQLite) uses a relative `NonExistent.db` instead of a file in `TestTempDirectory`.
    * `SqliteNoCaseFunctionsTests.cs:65`, the "nocase_like with an escape character" row, expects 0 from both functions, so it can't show a case difference.
34. **Test names and stale text after the rename** (low, read).
    * `BuildCountSqlAndBuildPagedSql_SqlServerTop_ThrowsNotSupportedException` only tests `BuildPagedSql` since TODO 24; rename it.
    * `ExecutePagedQueryAsync_PageAfterLast_ReducesPageToLast` and `..._PageAfterLast_ReturnsLastPageRows` have the same scenario and DisplayName, and the first is a subset of the second; merge them.
    * Two classes are called `ExecuteInsertTests` (`Execute\` and `ExecuteUpdate\`), both with `ExecuteInsertAsync_*` methods; rename the raw SQL one.
    * `Translate_NoCaseColumnAfterJoin_UsesNoCaseFunctions`, `Translate_CollateNoCase_UsesNoCaseFunctions` and `Translate_MethodOnMethodResult_UsesNoCaseFunctions` only assert `nocase_instr`, where their siblings end `_UsesNoCaseInstr`.
    * Stale text: `FreeTextSearchFunctionTests.cs:133` ("SetStemming is called in CreateDatabase"), `Context.cs:108` (`GuidTable` described as "Test Table 3"), `CaseInsensitivityTests.cs:120-123` (intercepting opens; the interceptor now uses `CommandCreated`), and the typo "ExecutePagedQueryAsyncwith" at `ExecutePagedQueryTests.cs:234`.
35. **Coverage gaps that matter** (low-medium, read).
    * The public `UseSqliteNoCaseReplacement(this SqliteConnection)` overload is never called directly: no null guard test, no use without EF Core, no open / close / reopen test.
    * The NOCASE subquery wrapping is only tested for `FirstOrDefault`, `Max` and `Min`, not `First`, `Single...`, `Last...` or `ElementAt...`, nor the `SelectMany` and projection cases CLAUDE.md says were checked.

#### Package

36. **Package metadata gaps** (low, read).
    * **Why:** the nuspec's `<repository>` has a commit but no URL, there's no symbols package (so the SourceLink data goes nowhere), and there are no release notes.
    * **Fix:** add `PublishRepositoryUrl`, `IncludeSymbols` with `SymbolPackageFormat` `snupkg`, and `PackageReleaseNotes` (a link to the ChangeLog) to Directory.Build.props, and build the release package with `-p:ContinuousIntegrationBuild=true`.
