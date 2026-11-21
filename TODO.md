### TODO - New features (November 2026)

#### 2. SQLite string functions ignore the NOCASE collation

The SQLite functions to fix are `instr`, `like`, `lower`, `upper`, `replace` and `trim` / `ltrim` / `rtrim` with a set of characters (at a minimum). This item started as `Contains` only, and options 1 and 2 below were written then.

##### Where we are (16 November 2026)

Nothing is built in the library yet. The design has moved on from the options below, so read this first.

**Done:**
* **A trial** (a scratch console app against EF 11.0.0 and SQLite, with a translator plugin for `Contains` that emitted `nocase_instr` for a NOCASE column) found:
  * **`ColumnExpression.Column` is set in real queries** for a table column: a plain column, through a reference navigation (a join), through a collection navigation (`Any` → `EXISTS`), on either side of an explicit `join`, after `SelectMany` and after a projection to an anonymous type. `Column.PropertyMappings` leads back to the runtime `IProperty`.
  * **`IColumn.Collation` can't be used.** It (and `IProperty.GetCollation()` on the runtime property) throws an `InvalidOperationException` in a query: "The requested configuration is not stored in the read-optimized model". EF leaves collations out of the runtime model because only migrations need them. The earlier API check missed this. Two ways round it both worked in every case above (see "Proposed design"):
    * **a custom annotation** set alongside the collation (custom annotations are kept in the runtime model), read with `FindAnnotation`;
    * **`IDesignTimeModel`** injected into the plugin, looking up the same entity type and property in `designTimeModel.Model`, where `GetCollation()` works. EF builds a second copy of the model the first time it's used (once per model).
  * **The fallbacks happen where expected:** `EF.Functions.Collate(p.Body, "NOCASE")` arrives as a `CollateExpression` with `Collation = NOCASE`, concatenation as a `SqlBinaryExpression`, and a `SqlQueryRaw` column as a `ColumnExpression` with `Column = null`. (A `Where` after `Distinct` wasn't a subquery: EF put the `WHERE` on the same `SELECT DISTINCT`.)
  * **The compiled query cache is fine.** The same query shape, with the same internal service provider, against a NOCASE and a non-NOCASE model, alternated twice: each model got its own SQL (`nocase_instr` / `instr`), the translator ran once per model, and the cached queries kept the right SQL and results. EF's cache key includes the model. With the same model and different parameter values there was one translation, and `ÉCOLE` matched `école`.
* **Translator order is solved.** 7.0.0.0 moved the library's translators to `IMethodCallTranslatorPlugin`, registered with `TranslatorExtensions.AddMethodCallTranslatorPlugin<TPlugin>()` (`Utilities/`). EF asks plugins before its own translators, so a plugin that handles `string.Contains` takes priority over EF's SQLite translation, and plugins combine with each other. A spike confirmed this. Never use `ReplaceService<IMethodCallTranslatorProvider>` (see `CLAUDE.md`).
* **What EF 11 generates on SQLite** (from the trial, `ToQueryString`):

  | C# | SQL | On a NOCASE column today |
  | --- | --- | --- |
  | `Contains(string)` / `Contains(char)` | `instr(x, @p) > 0` | case sensitive |
  | `IndexOf(string)` / `IndexOf(char)` | `instr(x, @p) - 1` | case sensitive |
  | `IndexOf(string, int)` | `(instr(substr(x, @start + 1), @p) - 1) + @start` | case sensitive |
  | `StartsWith(string)` / `StartsWith(char)` | `x LIKE @p_startswith ESCAPE '\'` (a constant becomes `x LIKE 'abc%'`) | ignores case for ASCII letters only |
  | `EndsWith(string)` | `x LIKE @p_endswith ESCAPE '\'` | ignores case for ASCII letters only |
  | `EF.Functions.Like` | `x LIKE @p` | ignores case for ASCII letters only |
  | `Replace(string, string)` | `replace(x, @a, @b)` | case sensitive |
  | `ToLower()` / `ToUpper()` | `lower(x)` / `upper(x)` | change ASCII letters only |
  | `Trim(char)` / `TrimStart(char)` | not translated (EF evaluated them on the client in a final projection) | - |

  * The `Trim` / `TrimStart` / `TrimEnd` overloads that take a `char[]` weren't checked, so it isn't known yet whether EF generates `trim(x, @chars)` / `ltrim` / `rtrim` from any of them, or whether the problem only shows up in raw SQL.

  * `LIKE` ignores ASCII case on **every** column, NOCASE or not. On a column that isn't NOCASE, `StartsWith("body")` and `StartsWith("BODY")` both match `Body ÉCOLE`, but `StartsWith("Body é")` doesn't. So `StartsWith` / `EndsWith` already behave differently from `Contains` there.
  * Already follow the column's collation, so need nothing: `==`, `!=`, `<`, `>`, `string.Compare`, `CompareTo`, `OrderBy`, `Distinct`, `GroupBy`, `Min` / `Max`, and `IN (...)` from a list's `Contains`.
  * Not translated by EF at all (they throw): `Equals` / `Contains` / `StartsWith` with a `StringComparison`.

**Proposed design** (refines option 2, not yet agreed in detail). The functions fall into two groups.

* **Group 1: `lower` / `upper` (no collation involved).** The problem is Unicode, not collation: SQLite's `upper('é')` is `é`, while SQL Server's `UPPER` / `LOWER` handle all of Unicode on `nvarchar`, whatever the collation. So no translator is needed:
  * register Unicode `lower` / `upper` (invariant culture) in `SqliteNoCaseReplacementInterceptor.ConnectionOpened`, next to the collation. An application function with the same name and number of arguments replaces SQLite's built-in one, so `ToLower()` / `ToUpper()` and raw SQL all get it;
  * register them with `isDeterministic: true`, or SQLite won't allow them in expression indexes, CHECK constraints or generated columns;
  * document the risk (as for the NOCASE replacement): an existing index on `lower(Name)` was built with the built-in function, and a connection without the replacement (e.g. the sqlite3 command-line tool) gives different results.
* **Group 2: `instr` / `like` / `replace` / `trim` (collation dependent).** On SQL Server, `CHARINDEX`, `LIKE` and `REPLACE` follow the input's collation (`REPLACE(N'ABC', N'b', N'x')` is `AxC` on a CI column), and probably `TRIM(chars FROM x)` too (to check). SQLite's `trim(X, Y)` / `ltrim` / `rtrim` remove the characters in `Y` case sensitively (`trim('Abca', 'a')` is `Abc`). A SQLite function can't see its arguments' collation, so these need a translator:
  * **New functions** `nocase_instr(value, search)`, `nocase_like(...)`, `nocase_replace(value, search, replacement)` and `nocase_trim` / `nocase_ltrim` / `nocase_rtrim(value, characters)`, rather than replacing the built-ins or adding extra arguments. The built-ins are never touched, and the SQL shows clearly when the NOCASE rules apply. Register them in `ConnectionOpened` with `isDeterministic: true`, using the same `CompareOptions` as `UnicodeNoCaseCompare`, kept in one shared constant.
  * **One translator plugin** for `Contains`, `IndexOf` (all the overloads above), `StartsWith`, `EndsWith`, `EF.Functions.Like`, `Replace` and the `Trim` / `TrimStart` / `TrimEnd` overloads that take characters (if EF translates any of them, or if the plugin should translate them itself):
    * if the instance's collation is `NOCASE`, emit the `nocase_*` function;
    * otherwise return `null`, so EF's own translation is used, exactly as today.
  * **Getting the collation:** a custom annotation or `IDesignTimeModel` (see the trial above). Leaning towards the annotation, set by a model convention that copies every property's collation into it, so collations set with `UseCollation` are covered as well as `UseCaseInsensitiveCollation`.
  * **`StartsWith` / `EndsWith`:** the translator gets the caller's raw value (EF adds the `_startswith` / `_endswith` escaped pattern in its own translator), so these could use `nocase_startswith` / `nocase_endswith` functions (`CompareInfo.IsPrefix` / `IsSuffix`) with no `LIKE` escaping at all, keeping `nocase_like` for `EF.Functions.Like`. To decide.
* **Switched on by `UseSqliteNoCaseReplacement()`**, which registers both groups (the plugin with `AddMethodCallTranslatorPlugin<TPlugin>()`). Without it, nothing changes, which settles "decide what happens without it" below.
* **Behaviour change:** with `UseSqliteNoCaseReplacement` and `UseCaseInsensitiveCollation` together, these methods on SQLite ignore case as SQL Server does, and `ToLower()` / `ToUpper()` change non-ASCII letters even without `UseCaseInsensitiveCollation`. It needs a ChangeLog breaking-change line.
* **Group 2 falls back to EF's own translation (as today):**
  * the instance isn't a plain column or an explicit `Collate` (e.g. concatenation, `Substring`). SQLite does the same, because only `COLLATE` carries a collation through an expression;
  * `Column` is `null`: the column comes from a subquery, a set operation (`Union` / `Concat`) or raw SQL (`FromSql` / `SqlQuery`);
  * a column searched for inside a constant (`"abc".Contains(e.Name)`). The searched string's collation is the one that counts, close to SQLite's rule that the left operand's collation wins.
* **Open question: function results have no collation in SQLite.** `e.Name.Replace("a", "b") == "X"` compares as BINARY even on a NOCASE column, and `e.Name.Trim().Contains(...)` falls back to plain `instr`. On SQL Server both keep the column's collation. The plugin could add `COLLATE NOCASE` to the result of `nocase_replace` (and maybe of `Trim` / `Substring` on a NOCASE column). Decide whether it's worth it.
* **Note: comparing two columns with different collations.** In `a.Name == b.Name` (or a join on string keys), SQLite uses the left column's collation, and a column without one counts as `BINARY`. So when only one side is NOCASE, the result depends on which way round the comparison is written. SQL Server raises a collation conflict error instead, but only if the two columns have different collations (`UseCaseInsensitiveCollation` sets the database collation there, so normally they don't). Nothing for the translator to do, but worth documenting with `UseCaseInsensitiveCollation`, for columns given a different collation.
* **Idea to evaluate (21 November 2026): rewrite the SQL expression tree instead of translating methods.** Let EF translate as normal, then walk the SQL expression tree and rename `instr` / `replace` / `trim` / `ltrim` / `rtrim` to the `nocase_*` functions, and turn a `LikeExpression` into `nocase_like(pattern, match[, escape])`, when the relevant argument is a NOCASE column or `COLLATE NOCASE` (the same `IDesignTimeModel` check as `SqliteNoCaseTranslator`).
  * **For:** it only renames, so EF's own SQL shapes are kept (the translator rebuilds each one: `> 0`, `- 1`, type mappings); it catches SQL from any translator or plugin, and EF's `StartsWith` / `EndsWith` (`LIKE @p_startswith ESCAPE '\'`, which `nocase_like` already handles), so `nocase_startswith` / `nocase_endswith` wouldn't be needed.
  * **Against:** EF has no supported plugin point for it. Replacing `IQueryTranslationPostprocessorFactory` means a provider-internal type (EF1001) and a single replacement that can clash (the trap `CLAUDE.md` warns about for `IMethodCallTranslatorProvider`). The other place is the library's own `SqliteCustomQueryGenerator` (override `VisitSqlFunction` / `VisitLike`), but that's only installed by `UseCustomQueryGenerator`, so `UseSqliteNoCaseReplacement` would have to install it too. Patterns that aren't function calls aren't fixed by a rename (`StartsWith(column)` is `substr(x, 1, length(y)) = y OR y = ''`, where `substr` drops the collation). EF's `IndexOf(s, start)` quirk (`start - 1` for no match) would come back unless special-cased. Expressions with no known collation (concatenation, subqueries, raw SQL) still fall back, as now.
  * **Current view:** keep the translator (supported, done and tested). The tree rewrite only pays off through the custom SQL generator, if `StartsWith` / `EndsWith` / `LIKE` should follow EF's own SQL exactly, or if other translators' `instr` / `LIKE` need catching. A prototype in the generator, on a scratch branch, would settle it.

**Next steps:**
1. Decide between the custom annotation and `IDesignTimeModel`, and how `StartsWith` / `EndsWith` are done.
2. Check the `trim` family: what EF 11 generates on SQLite and SQL Server for the `Trim` / `TrimStart` / `TrimEnd` overloads that take characters, and whether SQL Server's `TRIM(chars FROM x)` / `LTRIM` / `RTRIM` follow the collation. If EF translates none of them on SQLite, decide whether the plugin should, or whether `nocase_trim` is only for raw SQL.
3. Build group 1 (`lower` / `upper`).
4. Build the group 2 functions, the plugin and the `UseSqliteNoCaseReplacement()` registration.
5. Tests: the list under "Tests" below.

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

* **Follow the NOCASE replacement's rules exactly.** That means the invariant culture with `IgnoreCase | IgnoreKanaType | IgnoreWidth`, so it's accent sensitive like `Latin1_General_100_CI_AS_SC_UTF8`. Keep the options in one place, shared with `SqliteNoCaseReplacementInterceptor.UnicodeNoCaseCompare`.
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
* **Register the functions** in `SqliteNoCaseReplacementInterceptor.ConnectionOpened`, next to the collation. So they depend on `UseSqliteNoCaseReplacement()`, as the NOCASE rules do. (Settled: without it, nothing changes. See "Proposed design".)
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

On both databases, with the same values giving the same results, for each of `Contains`, `IndexOf`, `StartsWith`, `EndsWith`, `EF.Functions.Like` and `Replace`:
* non-ASCII case (`É` / `é`) matches;
* accents (`e` / `é`) don't match;
* `NULL` and empty search text;
* `%`, `_` and `[` in the search text are treated as plain characters (except in `EF.Functions.Like`, where they're wildcards, and `[` is a wildcard on SQL Server only).

And:
* `ToLower()` / `ToUpper()` change non-ASCII letters (`é` / `É`) on both databases;
* `IndexOf` returns the same position on both databases, including after a non-ASCII character (e.g. `é`) and after a character outside the BMP (e.g. an emoji: see "Resolved" above), and for `IndexOf(string, int)`;
* `Replace` replaces every match whatever its case, including a match of a different length from the search text;
* the `trim` family (whichever overloads end up translated) removes the characters whatever their case (`É` for `é`), but not other accents (`e` for `é`);
* on SQLite only: a column that isn't `NOCASE` keeps EF's own translation (case sensitive, or ASCII-only for `LIKE`), `EF.Functions.Collate(..., "NOCASE")` works, nothing changes without `UseSqliteNoCaseReplacement`, and the methods work through a navigation property and after a join.

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
