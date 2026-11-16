### TODO - Bugs (November 2026)

#### 1. The custom query generator can clash with another `IQuerySqlGeneratorFactory` replacement (documentation)

* `UseCustomQueryGenerator()` (called by `UseTableHintExtensions()` and `UseExecuteUpdateExtensions()`) replaces EF's `IQuerySqlGeneratorFactory` with `ReplaceService`. Both callers install the same factory, so they don't conflict with each other, and the custom generators derive from EF's SQL Server / SQLite generators, so none of EF's SQL generation is lost.
* But only one replacement of a service can be active, so it would conflict with another library, or the application, that also replaces `IQuerySqlGeneratorFactory`: whichever is registered last wins, and table hints / the ExecuteUpdate variants (or the other library's feature) would stop working.
* There's no plugin mechanism for SQL generation, so this can't be fixed the way the method call translators were (7.0.0.0 moved them from replacing `IMethodCallTranslatorProvider` to `IMethodCallTranslatorPlugin`).
* Suggested action: add a line to the ReadMe (table hints and ExecuteUpdate sections) saying the extensions replace `IQuerySqlGeneratorFactory`, so they can't be combined with another library that replaces it.
* Checked in November 2026: everything else the library registers combines with other registrations (interceptors via `AddInterceptors`, options extensions via `AddOrUpdateExtension`, translator plugins).

### TODO - New features (November 2026)

#### 2. `Contains` ignores the NOCASE collation on SQLite

##### Where we are (16 November 2026)

Nothing is built yet. The design has moved on from the options below, so read this first.

**Done:**
* **Option 2's open question is answered: yes, a translator can see the column's collation.** This was checked against the EF 11 API, not yet tried in a real query.
  * For `string.Contains`, the string being searched is the translator's `instance` argument. For a plain column that's a `ColumnExpression`, whose `IColumnBase Column` links back to the relational model.
  * For a table column, `Column` is an `IColumn`, and `IColumn.Collation` is the collation set in the model. That's what `UseCaseInsensitiveCollation` sets (`SetCollation("NOCASE")` on every string property).
  * View columns have no `Collation`, but `IColumnBase.PropertyMappings` leads back to the EF property, where `GetCollation()` works.
  * `EF.Functions.Collate(..., "NOCASE")` arrives as a `CollateExpression`, with `Collation` on it directly.
  * Roughly:

    ```csharp
    string? collation = instance switch
    {
        CollateExpression c => c.Collation,
        ColumnExpression { Column: IColumn column } => column.Collation,
        _ => null,
    };
    ```
* **Translator order is solved.** 7.0.0.0 moved the library's translators to `IMethodCallTranslatorPlugin`, registered with `TranslatorExtensions.AddMethodCallTranslatorPlugin<TPlugin>()` (`Utilities/`). EF asks plugins before its own translators, so a plugin that handles `string.Contains` takes priority over EF's SQLite translation, and plugins combine with each other. A spike confirmed this. Never use `ReplaceService<IMethodCallTranslatorProvider>` (see `CLAUDE.md`).

**Proposed design** (refines option 2, not yet agreed in detail):
* **A new function, `nocase_instr(value, search)`**, rather than a three-argument `instr`. The built-in `instr` is never touched, and the SQL shows clearly when the NOCASE rules apply.
  * Register it in `SqliteNoCaseReplacementInterceptor.ConnectionOpened`, next to the collation, with `CreateFunction(..., isDeterministic: true)`.
  * It uses the same `CompareOptions` as `UnicodeNoCaseCompare`, kept in one shared constant, and behaves as described under "Either way" below.
* **A translator plugin** for `string.Contains(string)`:
  * if the instance's collation is `NOCASE` (as above), emit `nocase_instr(column, @p) > 0`;
  * otherwise return `null`, so EF's own translation (plain `instr`) is used, exactly as today.
* **Switched on by `UseSqliteNoCaseReplacement()`**, which registers the plugin with `AddMethodCallTranslatorPlugin<TPlugin>()`. Without it, nothing changes, which settles "decide what happens without it" below.
* **Behaviour change:** with `UseSqliteNoCaseReplacement` and `UseCaseInsensitiveCollation` together, `Contains` on SQLite becomes case insensitive, matching SQL Server. It needs a ChangeLog breaking-change line.
* **Falls back to plain `instr` (case sensitive, as today):**
  * the searched string isn't a plain column or an explicit `Collate` (e.g. concatenation, `Substring`). SQLite does the same, because only `COLLATE` carries a collation through an expression;
  * `Column` is `null`: the column comes from a subquery, a set operation (`Union` / `Concat`) or raw SQL (`FromSql` / `SqlQuery`);
  * a column searched for inside a constant (`"abc".Contains(e.Name)`). The searched string's collation is the one that counts, close to SQLite's rule that the left operand's collation wins.

**Next steps:**
1. **A short trial** (e.g. a scratch console app, as for the translator spike) to confirm:
   * `ColumnExpression.Column` is set in a real query, for a plain column, a column reached through a navigation property and a column after a join;
   * the cached query SQL is unaffected (collation is part of the model, not the query, so it should be).
2. Build `nocase_instr`, the plugin and the `UseSqliteNoCaseReplacement()` registration.
3. Check how EF translates `StartsWith` / `EndsWith` on SQLite (see "Either way").
4. Tests: the list under "Tests" below, plus, on SQLite only, `EF.Functions.Collate(..., "NOCASE")`, no change without `UseSqliteNoCaseReplacement`, and `Contains` through a navigation property and after a join.

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
* **Against:** it changes what an existing query does (arguably a fix), and it needs more checking first. Can the translator see the column's collation from the model? If it can only see `EF.Functions.Collate`, this option is much less useful than option 1. (Answered: it can. See "Where we are".)

##### Suggestion

First check whether option 2's translator can see the column's collation. If it can, do option 2. If it can't, do option 1. Option 1 could also be added alongside option 2, for columns that aren't `NOCASE`. (It can, so option 2 is the way forward, in the form under "Where we are".)

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
