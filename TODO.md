### TODO - New features (November 2026)

#### 1. SQLite string functions ignore the NOCASE collation

The SQLite functions to fix are `instr`, `like`, `lower`, `upper`, `replace` and `trim` / `ltrim` / `rtrim` with a set of characters (at a minimum). This item started as `Contains` only, and options 1 and 2 below were written then.

##### Where we are (21 November 2026)

Most of it is built, on the `SqliteFunctions` branch. What's left is under "Still to do". The sections from "Problem" down are the original write-up, kept for the reasoning.

**Built:**
* **`lower` / `upper` (group 1).** `UseSqliteNoCaseReplacement()` replaces SQLite's built-in `lower` and `upper` with Unicode versions (invariant culture, deterministic), registered next to the NOCASE collation by `UseSqliteNoCaseReplacement(SqliteConnection)`, which `SqliteNoCaseReplacementInterceptor` calls in `CommandCreated`, once per connection. `ToLower()` / `ToUpper()` keep EF Core's translation and need nothing from the translator.
* **The `nocase_*` functions (group 2)**, in `SqliteUnicodeMethods`: `nocase_instr`, `nocase_like` (two and three arguments, pattern first, as SQLite's `like`), `nocase_replace`, `nocase_trim` / `nocase_ltrim` / `nocase_rtrim` (two arguments), `nocase_startswith` and `nocase_endswith`. They're registered alongside the built-ins, which are left alone, as deterministic functions sharing the NOCASE collation's `CompareOptions` and culture. Each keeps the interface of the built-in it's based on (NULL and empty arguments, LIKE escaping, positions in code points, matches of a different length).
* **`SqliteNoCaseTranslator`**, with `SqliteNoCaseTranslatorPlugin`, registered by `UseSqliteNoCaseReplacement()` with `AddMethodCallTranslatorPlugin<TPlugin>()`. On a NOCASE column, or `EF.Functions.Collate(..., "NOCASE")`:
  * `Contains` / `IndexOf` → `nocase_instr`, in the same shape as EF Core's translation (`> 0`, `- 1`);
  * `IndexOf(string, int)` / `IndexOf(char, int)` → `(nocase_instr(substr(x, start + 1), s) - 1) + start`, EF Core's own translation with `instr` renamed, so it gives the same results for every start index, including `start - 1` for no match (`SqliteNoCaseTranslatorTests_014`). A three-argument `nocase_instr` with a start position was tried and dropped: matching EF Core meant copying `substr`'s rules for the start position;
  * `StartsWith` / `EndsWith` → `nocase_startswith` / `nocase_endswith`, with no LIKE escaping (the `SqliteNoCaseTranslator` remarks say why EF Core's `LIKE` and `substr` translations can't be kept);
  * `EF.Functions.Like` → `nocase_like`, when the match expression is NOCASE;
  * `Replace` → `nocase_replace`;
  * `Trim` / `TrimStart` / `TrimEnd` with a constant `char` or a constant, non-empty `char[]` → `nocase_trim` / `nocase_ltrim` / `nocase_rtrim`.

  A NOCASE column selected from a subquery or a set operation is still translated: EF Core keeps the column it came from, and SQLite keeps its collation too. Anything else returns `null`, so EF Core's own translation is used: a column without NOCASE, an expression with no known collation (concatenation, `Substring`), a column from raw SQL, a column searched for inside a constant (`"abc".Contains(e.Name)`), non-constant trim characters, and a property of a complex type.
* **Tests:** `SqliteNoCaseFunctionsTests` (each function through SQL, including NULL and empty arguments, and each `nocase_*` function against its built-in), `SqliteSqlServerEquivalenceTests` (each function against its SQL Server equivalent, the NOCASE collation, and LINQ string methods on both databases) and `SqliteNoCaseTranslatorTests` (the generated SQL and the results, a column without NOCASE, `EF.Functions.Collate`, without `UseSqliteNoCaseReplacement`, after a join, a subquery and a set operation, through a reference and a collection navigation, `IndexOf(string, int)` against EF Core's own translation for every start index, and the fallbacks for a concatenation, raw SQL and a column searched for inside a constant). `SqliteSqlServerEquivalenceTests_009` covers `EF.Functions.Like`, `IndexOf(string, int)`, `TrimStart` / `TrimEnd`, and NULL and empty search text through LINQ.
* **Docs:** the ReadMe's "SQLite NoCase Replacement" and "Connection Pooling" sections, and the `UseSqliteNoCaseReplacement` remarks.

**Decided along the way:**
* **The collation is read from `IDesignTimeModel`**, not a custom annotation. A collation set with `UseCollation` is covered as well as `UseCaseInsensitiveCollation`, with no model convention needed.
* **`lower` / `upper` replace the built-ins, the other functions don't.** Case conversion doesn't depend on a collation, but a replacement `instr` (etc.) couldn't see its arguments' collation, so it would ignore case on every column. Replacing all of them was tried (25133b3) and dropped (101c63b).
* **Connection pooling:** once a connection string has been used with `UseSqliteNoCaseReplacement()`, every later connection with that connection string must use it too. Microsoft.Data.Sqlite removes the collation and functions when a pooled connection is closed, and SQLite can't fall back to its built-in `NOCASE`, `lower` and `upper` on that connection. Documented in the ReadMe and the `UseSqliteNoCaseReplacement` remarks.
* **On SQLite, `UseCaseInsensitiveCollation` keeps a collation already set** on a property (breaking change, in the ChangeLog).
* **SQL Server's `TRIM(chars FROM x)`, `LTRIM(x, chars)` and `RTRIM(x, chars)` follow the collation**, as `nocase_trim` / `nocase_ltrim` / `nocase_rtrim` do (`SqliteSqlServerEquivalenceTests_007`).

**Still to do:**
* a. **ChangeLog.** There's no entry yet for the behaviour change, which the design said needs a breaking-change line: with `UseSqliteNoCaseReplacement()`, these methods on a NOCASE column now ignore case as SQL Server does; and `ToLower()` / `ToUpper()` change non-ASCII letters even without `UseCaseInsensitiveCollation`. The connection pooling rule isn't in the ChangeLog either.
* b. **Doc comments.** The string methods that follow NOCASE on SQLite, and when EF Core's own translation is used instead, are now in the `UseSqliteNoCaseReplacement` remarks (TODO 8). Still to document in the doc comments (not the ReadMe, which is only an introduction, important notes and announcements): EF Core 11 doesn't translate `Trim(char)` or `Trim(char[])` on SQL Server. In a `Where` it throws ("could not be translated"), and in a final `Select` it trims on the client, case sensitively, so the two databases differ there (`TrimStart` / `TrimEnd` become `LTRIM(x, N'é')` / `RTRIM` and do match). So `SqliteSqlServerEquivalenceTests_009` has no `Trim(char)` row. Every other `char` overload the translator handles is translated on SQL Server (checked 23 November 2026). Also: `IndexOf(string, int)` with no match gives `startIndex - 1` on SQLite (as EF Core's own SQLite translation, which `nocase_instr` deliberately matches) and -1 on SQL Server, so `SqliteSqlServerEquivalenceTests_009` has no row for that either.
* d. **Decide the open question** below about function results having no collation.
* e. **Document the note** below about comparing columns with different collations, in the `UseCaseInsensitiveCollation` doc comments.
* f. **Optionally prototype** the expression tree rewrite below, on a scratch branch.
* g. **Think of a better name than `SqliteNoCase...`** (`UseSqliteNoCaseReplacement`, `SqliteNoCaseReplacementInterceptor`, `SqliteNoCaseTranslator`...).

##### Reference

* **Translator order.** 7.0.0.0 moved the library's translators to `IMethodCallTranslatorPlugin`, registered with `TranslatorExtensions.AddMethodCallTranslatorPlugin<TPlugin>()` (`Utilities/`). EF asks plugins before its own translators, so a plugin that handles `string.Contains` takes priority over EF's SQLite translation, and plugins combine with each other. Never use `ReplaceService<IMethodCallTranslatorProvider>` (see `CLAUDE.md`). What the trial found about reading a column's collation is in `CLAUDE.md` too.
* **What EF 11 generates on SQLite** (from the trial, `ToQueryString`). It's still what's used when `SqliteNoCaseTranslator` falls back:

  | C# | SQL | On a NOCASE column today |
  | --- | --- | --- |
  | `Contains(string)` / `Contains(char)` | `instr(x, @p) > 0` | case sensitive |
  | `IndexOf(string)` / `IndexOf(char)` | `instr(x, @p) - 1` | case sensitive |
  | `IndexOf(string, int)` / `IndexOf(char, int)` | `(instr(substr(x, @start + 1), @p) - 1) + @start` | case sensitive |
  | `StartsWith(string)` / `StartsWith(char)` | `x LIKE @p_startswith ESCAPE '\'` (a constant becomes `x LIKE 'abc%'`) | ignores case for ASCII letters only |
  | `EndsWith(string)` / `EndsWith(char)` | `x LIKE @p_endswith ESCAPE '\'` | ignores case for ASCII letters only |
  | `EF.Functions.Like` | `x LIKE @p` | ignores case for ASCII letters only |
  | `Replace(string, string)` / `Replace(char, char)` | `replace(x, @a, @b)` | case sensitive |
  | `ToLower()` / `ToUpper()` | `lower(x)` / `upper(x)` | change ASCII letters only |
  | `Trim` / `TrimStart` / `TrimEnd` with a `char` or `char[]` | `trim(x, 'é')` / `ltrim` / `rtrim`, with a `char[]` as one string (`'éx'`), in a `Where` as well as a projection (checked 23 November 2026) | case sensitive |

  * `LIKE` ignores ASCII case on **every** column, NOCASE or not. On a column that isn't NOCASE, `StartsWith("body")` and `StartsWith("BODY")` both match `Body ÉCOLE`, but `StartsWith("Body é")` doesn't. So `StartsWith` / `EndsWith` already behave differently from `Contains` there.
  * Already follow the column's collation, so need nothing: `==`, `!=`, `<`, `>`, `string.Compare`, `CompareTo`, `OrderBy`, `Distinct`, `GroupBy`, `Min` / `Max`, and `IN (...)` from a list's `Contains`.
  * Not translated by EF at all (they throw): `Equals` / `Contains` / `StartsWith` with a `StringComparison`.

##### Open questions and ideas

* **Open question: function results have no collation in SQLite.** `e.Name.Replace("a", "b") == "X"` compares as BINARY even on a NOCASE column, and `e.Name.Trim().Contains(...)` falls back to plain `instr`. On SQL Server both keep the column's collation. The plugin could add `COLLATE NOCASE` to the result of `nocase_replace` (and maybe of `Trim` / `Substring` on a NOCASE column). Decide whether it's worth it.
* **Note: comparing two columns with different collations.** In `a.Name == b.Name` (or a join on string keys), SQLite uses the left column's collation, and a column without one counts as `BINARY`. So when only one side is NOCASE, the result depends on which way round the comparison is written. SQL Server raises a collation conflict error instead, but only if the two columns have different collations (`UseCaseInsensitiveCollation` sets the database collation there, so normally they don't). Nothing for the translator to do, but worth documenting with `UseCaseInsensitiveCollation`, for columns given a different collation.
* **Idea to evaluate (21 November 2026): rewrite the SQL expression tree instead of translating methods.** Let EF translate as normal, then walk the SQL expression tree and rename `instr` / `replace` / `trim` / `ltrim` / `rtrim` to the `nocase_*` functions, and turn a `LikeExpression` into `nocase_like(pattern, match[, escape])`, when the relevant argument is a NOCASE column or `COLLATE NOCASE` (the same `IDesignTimeModel` check as `SqliteNoCaseTranslator`).
  * **For:** it only renames, so EF's own SQL shapes are kept (the translator rebuilds each one: `> 0`, `- 1`, type mappings); it catches SQL from any translator or plugin, and EF's `StartsWith` / `EndsWith` with a constant or parameter (`LIKE @p_startswith ESCAPE '\'`, which `nocase_like` already handles), so `nocase_startswith` / `nocase_endswith` would only be needed for a column argument (see "Against").
  * **Against:** EF has no supported plugin point for it. Replacing `IQueryTranslationPostprocessorFactory` means a provider-internal type (EF1001) and a single replacement that can clash (the trap `CLAUDE.md` warns about for `IMethodCallTranslatorProvider`). The other place is the library's own `SqliteCustomQueryGenerator` (override `VisitSqlFunction` / `VisitLike`), but that's only installed by `UseCustomQueryGenerator`, so `UseSqliteNoCaseReplacement` would have to install it too. Patterns that aren't function calls aren't fixed by a rename: `StartsWith(column)` / `EndsWith(column)` are `substr(x, 1, length(y)) = y OR y = ''`, which compares with the argument's collation rather than the column's and can cut the prefix in the wrong place (see the `SqliteNoCaseTranslator` remarks), so that shape would have to be recognised and replaced with `nocase_startswith` / `nocase_endswith`. EF's `IndexOf(s, start)` quirk (`start - 1` for no match) would come back unless special-cased. Expressions with no known collation (concatenation, raw SQL) still fall back, as now.
  * **Current view:** keep the translator (supported, done and tested). The tree rewrite only pays off through the custom SQL generator, if `StartsWith` / `EndsWith` / `LIKE` should follow EF's own SQL exactly, or if other translators' `instr` / `LIKE` need catching. A prototype in the generator, on a scratch branch, would settle it.

##### Problem

* **SQLite:** `instr` (`Contains`, `IndexOf`), `replace` (`Replace`) and `like` (`StartsWith`, `EndsWith`, `EF.Functions.Like`) ignore the column's collation (`NOCASE` set by `UseCaseInsensitiveCollation`) and `EF.Functions.Collate`. `instr` and `replace` compare the characters exactly, and `like` only ignores case for ASCII letters. `lower` / `upper` (`ToLower()` / `ToUpper()`) only change ASCII letters. `trim` / `ltrim` / `rtrim` with a set of characters remove them case sensitively. So with `UseCaseInsensitiveCollation`, `==` is case insensitive but `Contains` is case sensitive, and so on.
* **SQL Server:** `CHARINDEX`, `LIKE` and `REPLACE` all use the column's collation, and `UPPER` / `LOWER` handle all of Unicode. So with `UseCaseInsensitiveCollation` they already ignore case, and the two databases give different results for the same query.

##### Option 1: a new method, `EF.Functions.ContainsNoCase(value, search)` (name to be decided)

* **SQL Server:** `CHARINDEX(@search, value COLLATE Latin1_General_100_CI_AS_SC_UTF8) > 0`. `CHARINDEX` rather than `LIKE`, so `%`, `_` and `[` in the search text don't need escaping.
* **SQLite:** a custom function registered on the connection, implemented with `CultureInfo.InvariantCulture.CompareInfo.IndexOf(value, search, options)`.
* **For:** simple, explicit, and it works on any column, whatever its collation.
* **Against:** the caller has to remember to use it, and a plain `Contains` stays case sensitive on SQLite.

##### Option 2: a variant of `instr` that takes a collation

* **Don't replace the built-in `instr`.** A SQLite custom function can't see the collation of its arguments, so a replacement couldn't follow the column's collation. Every `Contains` would become case insensitive, including on columns that aren't `NOCASE`.
* **Instead:** register a three-argument `instr(value, search, collation)`. SQLite tells functions apart by their number of arguments, so the built-in two-argument `instr` is untouched. Then add a translator that turns `string.Contains` into the new `instr` when the expression's collation is `NOCASE`: the column's collation in the model, or `EF.Functions.Collate(..., "NOCASE")`. The translator must run before EF's own SQLite string translator.
* **For:** a plain `Contains` just works, and gives the same results as SQL Server.
* **Against:** it changes what an existing query does (arguably a fix), and it needs more checking first. Can the translator see the column's collation from the model? If it can only see `EF.Functions.Collate`, this option is much less useful than option 1. (Answered: it can. See "Where we are".)

##### Suggestion

First check whether option 2's translator can see the column's collation. If it can, do option 2. If it can't, do option 1. Option 1 could also be added alongside option 2, for columns that aren't `NOCASE`. (It can, so option 2 is the way forward, in the form under "Where we are".)

##### Either way

* **Follow the NOCASE replacement's rules exactly.** That means the invariant culture with `IgnoreCase | IgnoreKanaType | IgnoreWidth`, so it's accent sensitive like `Latin1_General_100_CI_AS_SC_UTF8`. Keep the options in one place, shared with the NOCASE collation. (Done: `SqliteUnicodeMethods` holds the options and the culture, and its `Compare` is the collation.)
* **Culture: the invariant culture for now, perhaps configurable later.** `upper` / `lower` use `ToUpperInvariant` / `ToLowerInvariant`, and the `nocase_*` functions use the invariant culture's `CompareInfo`, as the NOCASE collation already does. Keep the culture in one shared place, next to the `CompareOptions`, so a setting can be added later without breaking callers. Why invariant:
  * **It matches SQL Server.** `UPPER` / `LOWER` follow the collation's language there, and `Latin1_General_100_CI_AS_SC_UTF8` has none, so its case mapping is essentially the invariant one (a Turkish collation maps `i` to `İ`).
  * **It matches the NOCASE collation**, so `ToUpper(a) == ToUpper(b)` and `a == b` on a NOCASE column agree.
  * **It's deterministic.** The functions are registered with `isDeterministic: true` and can be used in expression indexes, so the same input must always give the same output. Never use `CurrentCulture`, which can change per thread or per user.
  * **It keeps lengths.** The invariant case mapping is one character to one character (`ß` stays `ß`), as on SQL Server, so `IndexOf` / `Substring` positions stay consistent.
  * **It rarely matters.** Most cultures case the same as the invariant culture. The real exceptions are Turkish / Azerbaijani (dotted and dotless i) and, slightly, Lithuanian.

  If a culture setting is added (e.g. `UseSqliteNoCaseReplacement(CultureInfo? culture = null)`):
  * **One setting for everything:** the NOCASE collation, `upper` / `lower` and every `nocase_*` function, so they can't disagree.
  * **SQL Server has to match.** `UseCaseInsensitiveCollation` hard-codes `Latin1_General_100_CI_AS_SC_UTF8`, so a Turkish culture on SQLite would only match SQL Server with a collation such as `Turkish_100_CI_AS_SC_UTF8`. Either let `UseCaseInsensitiveCollation` take the SQL Server collation (and perhaps the SQLite culture), or document that the two must correspond. Otherwise the setting breaks the promise that both databases give the same results.
  * **Every connection must use the same culture.** NOCASE indexes and `lower(...)` expression indexes are built with the culture in force when the rows were written, so changing it, or opening the file with a different one, can make index lookups wrong. Add this to the documented risks of the NOCASE replacement.
  * **`InvariantGlobalization=true`** has no culture data, so a specific culture wouldn't give its real rules. Throw if a culture other than the invariant one is asked for in that mode, or document it.
* **Register the functions** next to the collation, in `UseSqliteNoCaseReplacement(SqliteConnection)`, which `SqliteNoCaseReplacementInterceptor` calls in `CommandCreated` (not `ConnectionOpened`, which EF Core doesn't raise for a connection that was already open: TODO 2). So they depend on `UseSqliteNoCaseReplacement()`, as the NOCASE rules do. (Done: without it, nothing changes. See "Where we are".)
* **Match the built-ins:**
  * `NULL` if any argument is `NULL`;
  * an empty search text matches (`instr`), and `replace` with an empty search text returns the value unchanged (as SQLite's `replace` and SQL Server's `REPLACE` both do).
* **Positions are in characters (code points), not UTF-16 units.** `instr` and `substr` count characters, and EF's `IndexOf(string, int)` translation combines the two, so `nocase_instr` must convert the UTF-16 index from `CompareInfo.IndexOf` to a character position.
* **Resolved (17 November 2026): positions and lengths outside the Basic Multilingual Plane.** SQLite's `instr`, `substr` and `length` count characters (code points). SQL Server's `CHARINDEX`, `SUBSTRING` and `LEN` counted UTF-16 units with the old `SQL_Latin1_General_CP1_CI_AS` collation, which isn't `_SC` (supplementary characters), so `IndexOf`, `Substring` and `Length` gave different results on the two databases for a string containing a character outside the BMP (e.g. an emoji, which is one code point but two UTF-16 units). `UseCaseInsensitiveCollation` now uses `Latin1_General_100_CI_AS_SC_UTF8` on SQL Server, which counts characters as SQLite does (see the ChangeLog). So `nocase_instr` must count characters too, like the built-in `instr`. .NET still counts UTF-16 units, so the databases can differ from the same code run in memory, but not from each other. The other options considered and rejected:
  * making `nocase_instr` count UTF-16 units: it would only fix `IndexOf` on NOCASE columns, and would disagree with `substr` / `length`, so `Substring(IndexOf(...))` would break;
  * replacing `substr` / `length` / `instr` with UTF-16 versions on SQLite: it would change every `length` / `substr` call, including raw SQL, expression indexes, CHECK constraints and BLOBs, which is much riskier than replacing `lower` / `upper`, where the built-in result is plainly wrong;
  * documenting the difference as a known limitation.
* **A match can be a different length from the search text.** With `IgnoreCase`, `CompareInfo.IndexOf` can match text of a different length. `nocase_replace` must use the `IndexOf` overload that returns `matchLength`, not the search text's length.
* **Performance:** the functions run for every row and can't use an index, the same as `instr` today.

##### Tests

Most of these are written (see "Where we are"). The ones still missing are listed under "Still to do".

On both databases, with the same values giving the same results, for each of `Contains`, `IndexOf`, `StartsWith`, `EndsWith`, `EF.Functions.Like` and `Replace`:
* non-ASCII case (`É` / `é`) matches;
* accents (`e` / `é`) don't match;
* `NULL` and empty search text;
* `%`, `_` and `[` in the search text are treated as plain characters (except in `EF.Functions.Like`, where they're wildcards, and `[` is a wildcard on SQL Server only).

And:
* `ToLower()` / `ToUpper()` change non-ASCII letters (`é` / `É`) on both databases;
* `IndexOf` returns the same position on both databases, including after a non-ASCII character (e.g. `é`) and after a character outside the BMP (e.g. an emoji: see "Resolved" above), and for `IndexOf(string, int)` when there's a match (with no match, SQLite gives `startIndex - 1`, as EF Core's own SQLite translation does, and SQL Server -1: see 1b);
* `Replace` replaces every match whatever its case, including a match of a different length from the search text;
* the `trim` family (whichever overloads end up translated) removes the characters whatever their case (`É` for `é`), but not other accents (`e` for `é`);
* on SQLite only: a column that isn't `NOCASE` keeps EF's own translation (case sensitive, or ASCII-only for `LIKE`), `EF.Functions.Collate(..., "NOCASE")` works, nothing changes without `UseSqliteNoCaseReplacement`, and the methods work through a navigation property and after a join.

### TODO - Solution review (22 November 2026)

These came from a review of the whole solution on the `SqliteFunctions` branch at 0e56056. The library, the docs, the project files and the test infrastructure were read in full. The test classes were only searched for the `CLAUDE.md` test rules (`CancellationToken.None`, the `TestContext` property, temp paths, region names), not read line by line.

#### Likely bugs

6. **The SQL Server `GenerateInsert` doesn't check `Limit: null`, but the SQLite one does.** So `.Take(n).ExecuteInsertAsync(...)` produces `INSERT TOP (n) INTO …` on SQL Server and throws on SQLite. Harmless, but the two databases disagree.

#### Possible improvements

18. **SQLite `FreeTextSearch`: honour `useStemming` on every call, not just the first** (the limitation documented in the ReadMe). Accepted as a limitation for now (23 November 2026), as any fix needs fairly extensive changes. The idea, if it's picked up:
    * Instead of swapping the whole table for its stemming table, rewrite each `FreeTextSearch(e.Field, text, true)` call into a non-stemming search of the stemming table, matched by key: `stemmingSet.Where(s => FreeTextSearch(s.Field, text, false)).Select(s => s.Id).Contains(e.Id)`. EF Core translates it to `"t"."ROWID" IN (SELECT "t0"."ROWID" FROM "TestFreeText_Stemming" AS "t0" WHERE MATCH(...))`, and a non-stemming then a stemming call in the same query each worked (tried by hand on EF Core 11).
    * It needs the stemming table to have the same keys (`ROWID`) as the main table, row for row, and a single-column key.
    * The hard part is knowing which table each call searches, as the main and stemming tables share a CLR type. Picking the one entity type of that CLR type with a stemming table fails for two such tables of the same CLR type in one query (e.g. a stemming search on one and a non-stemming search on the other). Options: require different CLR types; pass a subquery for every candidate stemming table to an internal marker method and let the translator (which knows the column's table) pick one (needs a prototype); or follow each call's lambda parameter back to its query root (fragile through joins and `SelectMany`).

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
