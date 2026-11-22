# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Overview

EntityFrameworkCore.Extensions is a library of extension/helper methods for Entity Framework Core 11 (targeting net11.0, set once in `Directory.Build.props`), supporting **SQL Server and SQLite only**. It adds capabilities EF Core doesn't provide natively: database-independent free text search, table hints, ExecuteUpdate/Delete/Insert variants that return affected rows, unique constraint violation detection, case-insensitive collation helpers, and raw-SQL execute/paging helpers.

Solution layout (`EntityFrameworkCore.Extensions.slnx`):
- `DotDoc.EntityFrameworkCore.Extensions` — the library.
- `DotDoc.EntityFrameworkCore.Extensions.Tests` — MSTest test suite (has `InternalsVisibleTo` access to the library).

## Common commands

Build and test via the solution file (`.slnx`):

```
dotnet build EntityFrameworkCore.Extensions.slnx
dotnet test EntityFrameworkCore.Extensions.slnx
```

Run a single test class or method:

```
dotnet test --filter "FullyQualifiedName~TableHintsTests"
dotnet test --filter "FullyQualifiedName~TableHintsTests.MethodName"
```

Tests run against **both SQLite and SQL Server** (most test classes parametrize over `DatabaseTypes.Sqlite` / `DatabaseTypes.SqlServer` via `DynamicDataAttribute`). SQL Server tests require a local SQL Server instance reachable via `Server=localhost;Trusted_Connection=True;TrustServerCertificate=True` (see `DotDoc.EntityFrameworkCore.Extensions.Tests/Data/DatabaseUtils.cs`); each test run drops/recreates the database via `EnsureDeletedAsync`/`EnsureCreatedAsync`. SQLite tests use a local `.db` file and need no external service.

FreeText search tests additionally require a full-text catalog (SQL Server) or FTS5 virtual tables (SQLite), both set up automatically by `DatabaseUtils.OpenDatabaseAsync`.

`TreatWarningsAsErrors` is enabled solution-wide (`Directory.Build.props`), along with StyleCop (`NewStyleCop.Analyzers`) and `SonarAnalyzer.CSharp` as global analyzers — expect a normal build to fail on style/analyzer violations, not just compile errors.

## Architecture

### Per-database-provider dispatch

Almost every feature branches on database provider via `DatabaseTypeExtensions.GetDatabaseType()` (`DatabaseType/`), which inspects the active `DbContextOptions`/`DatabaseFacade`/`MigrationBuilder` and returns `DatabaseTypes.Sqlite` or `DatabaseTypes.SqlServer`. Unsupported providers throw `UnsupportedDatabaseTypeException`. Most public `UseXxxExtensions(DbContextOptionsBuilder)` setup methods switch on this to install the correct provider-specific service (interceptor, translator provider, query generator).

### Custom query generator + SQL tagging pattern

Several features (table hints, ExecuteUpdate/Delete/Insert-with-results) work by replacing EF's `IQuerySqlGeneratorFactory` with a custom one (`CustomQueryGenerators/`, installed via `CustomQueryGeneratorExtensions.UseCustomQueryGenerator`). The generic flow:

1. A LINQ query is tagged with the `CustomTagExtensions.TagWithTableHints` / `TagWithExecuteUpdate` extension methods, which call `.TagWith(...)` with a magic string prefix. The prefixes are private to `CustomTagExtensions`, which is the only class that knows the tag format: it creates the tags, recognises them (`IsExecuteUpdateTag` / `IsTableHintsTag`) and reads them (`GetCustomTags`).
2. The provider-specific query generator (`SqlServerCustomQueryGenerator` / `SqliteCustomQueryGenerator`) recognizes the tag while generating SQL and rewrites the statement (e.g. injects `WITH (NOLOCK)` table hints). Its `GenerateTagsHeaderComment` override calls `CustomTagExtensions.GetCustomTags`, which returns a `CustomTags` holding the custom values and a copy of the set without the custom tags, so they never appear in the SQL (the cached set must not be changed).
3. The ExecuteXxx variants are told apart only by their tag (e.g. `$$EXECUTEUPDATE$$UpdateGetRows`; the suffix is a `QueryType` value). The variant **must** be in the expression: EF caches the generated SQL against the expression tree, so anything outside it (a past version used an `AsyncLocal`) lets the first variant to run decide the SQL for every later call with the same query shape. For the same reason the generators must never use per-call state.
4. For "get rows back" operations (`ExecuteDeleteGetRowsAsync`, `ExecuteInsertGetRowAsync`, `ExecuteUpdateGetRowsAsync`), the per-call state is `ExecuteUpdateState`, a scoped service in EF's internal service provider (one per `DbContext`), registered by `ExecuteUpdateOptionsExtension` from `UseExecuteUpdateExtensions()`. It is also registered as `IResettableService` (resolving to the same instance) so a pooled context is reset when it goes back to the pool. The method sets the query type, a `DbCommandInterceptor` (`ExecuteUpdateInterceptor`, which finds the state through `eventData.Context`) captures the generated SQL/parameters from the real EF `ExecuteUpdate`/`ExecuteDelete` pipeline *and suppresses execution* (`InterceptionResult.SuppressWithResult`), then the extension method re-runs the captured SQL itself via `SqlQueryRaw` to materialize and return the affected rows. Follow this pattern exactly when touching this code: check for re-entrancy (throw if the query type isn't `Unknown`) and set the query type **before** the `try`, and always reset the state in the `finally`.
5. If `ExecuteUpdateState` can't be found, `UseExecuteUpdateExtensions()` wasn't called. Every method that depends on the custom generator (all except `ExecuteDeleteGetCountAsync` / `ExecuteUpdateGetCountAsync`) throws an `InvalidOperationException` before running anything; without the generator, `ExecuteInsertAsync` would run as an UPDATE of every matching row.
6. This depends on **EF Core 11**. EF 10 drops the tags of an `UpdateExpression` when a visitor rebuilds it (e.g. an `==` in the `Where`), so the generator never sees the tag (EF issue #36908, fixed in 11.0.0). Don't target EF 10 again; see ChangeLog 6.0.0.2 for the history.

Note `ExecuteUpsertGetCountAsync`/`ExecuteUpsertGetRowsAsync` implement upsert as an explicit transaction (update-then-insert-if-zero-rows) rather than native `MERGE`/`ON CONFLICT`, specifically so update and insert can target different property sets — see the code comment in `ExecuteUpdateExtensions.cs` for the reasoning.

### Free text search translation

`FreeTextSearchFunction/` and `MatchFunction/` add custom `EF.Functions.*` methods translated per-provider by `IMethodCallTranslator` implementations (`SqlServerFreeTextSearchTranslator`, `SqliteFreeTextSearchTranslator`, `MatchTranslator`). Each is exposed through an `IMethodCallTranslatorPlugin` (`...TranslatorPlugin`) that the `Use...` method registers with `TranslatorExtensions.AddMethodCallTranslatorPlugin<TPlugin>()` (`Utilities/`), via the generic `MethodCallTranslatorPluginOptionsExtension<TPlugin>`. **Never register translators by replacing `IMethodCallTranslatorProvider`** (`ReplaceService`): a replacement deriving from `RelationalMethodCallTranslatorProvider` drops all of EF's database-specific translations (string methods, `Math`, `DateTime`...), and only one replacement can be active, so two features replacing it break each other (this was a bug fixed in 7.0.0.0). Plugins are asked before EF's own translators, add to them rather than replace them, and combine with each other. SQLite free text search additionally needs a `SqliteFreeTextSearchInterceptor` to swap in a stemming vs. non-stemming FTS table depending on the call; because of how EF parses expressions, only the *first* `FreeTextSearch` call's `useStemming` value is honored per SQLite query (documented limitation, see `ReadMe.md`).

A translator can find the collation of the column it's translating, but **not through `IColumn.Collation` or `IProperty.GetCollation()`**. Both throw an `InvalidOperationException` in a query ("The requested configuration is not stored in the read-optimized model"), because EF leaves collations out of the runtime model (only migrations need them). A trial against EF 11.0.0 on SQLite (see TODO item 2, which plans to use this to make SQLite's string functions honour NOCASE) found the following. The column is the translator's `instance` (or one of its `arguments`).
- A table column arrives as a `ColumnExpression` whose `Column` (`IColumnBase`) is set: for a plain column, through a reference or collection navigation, on either side of a join, after `SelectMany`, after a projection, and when selected from a subquery or a set operation (`SqliteNoCaseTranslatorTests_009` / `_010`; this matches SQLite, which keeps a subquery column's collation and gives a compound SELECT's column the collation of the leftmost SELECT). `Column.PropertyMappings` leads back to the runtime `IProperty`.
- Two ways of getting the collation from there work (not yet decided which to use):
  - **A custom annotation** set on the property alongside the collation. Custom annotations are kept in the runtime model, so `FindAnnotation` on the runtime `IProperty` reads it.
  - **`IDesignTimeModel`**, injected into the translator plugin: look up the same entity type and property in `designTimeModel.Model`, where `GetCollation()` works. EF builds the design-time model the first time it's used (once per model).
- `EF.Functions.Collate(..., "NOCASE")` arrives as a `CollateExpression` with `Collation` on it directly.
- Anything else has no known collation: an expression such as concatenation or `Substring`, or a column from raw SQL, where `Column` is `null`. The translator should then return `null`, so EF's own translation is used.
- EF's compiled query cache key includes the model, so a translation that depends on the model's collations is cached per model and stays correct.

With the annotation approach (`CollationAnnotation` stands for whatever annotation name is chosen):

```csharp
string? collation = instance switch
{
    CollateExpression collate => collate.Collation,
    ColumnExpression { Column: { } column } => column.PropertyMappings
        .Select(m => m.Property.FindAnnotation(CollationAnnotation)?.Value as string)
        .FirstOrDefault(),
    _ => null,
};
```

### Other notable pieces

- `Utilities/` — small extension classes, one per feature and named after what they do (as the other `*Extensions` classes are), all in the `Utilities` namespace: `GuidExtensions` (`CreateGuid`), `DatabaseExistsExtensions` (`DoesDatabaseExistAsync`), `GetDbContextExtensions` (`GetDbContext`), `DelimitedIdentifierExtensions` (`GetDelimitedIdentifier`) and `UniqueConstraintExtensions` (below). They replaced the single `UtilityExtensions` class in 7.0.0.0 (`CaseInsensitiveCollationExtensions` was split out at the same time, then moved to `CaseInsensitivity/`).
- `Utilities/UniqueConstraintExtensions.cs` — `IsUniqueConstraintException()`, an extension method on `Exception` that searches the exception and its inner exceptions and returns `true` for a unique index, unique constraint or primary key violation. It checks error codes only, never the message (which SQL Server translates), and needs no `DbContext`: SQL Server `SqlError.Number` 2601 or 2627 anywhere in `SqlException.Errors`; SQLite `SqliteExtendedErrorCode` `SQLITE_CONSTRAINT_UNIQUE` / `SQLITE_CONSTRAINT_PRIMARYKEY` (not `SqliteErrorCode`, which is 19 for every constraint violation, including NOT NULL). In 7.0.0.0 it replaced `UniqueConstraintInterceptor`, `UniqueConstraintException` and `UniqueConstraintDetails`, whose table / field name lookup was unreliable; see the ChangeLog before adding anything like it back.
- `Execute/ExecuteMethods.cs` — raw SQL execute helpers behind the public `DatabaseFacade` extensions in `ExecuteExtensions.cs` (`ExecuteScalarAsync`, `ExecuteQueryAsync`, `ExecuteNonQueryAsync`, `ExecuteInsertAsync`, `ExecutePagedQueryAsync`). The SQL here is written by the library's caller, not generated by EF Core, so it can contain comments, strings and quoted identifiers anywhere. Don't confuse `DatabaseFacade.ExecuteInsertAsync<T>(sql, ...)` (raw SQL, here) with `IQueryable<TSource>.ExecuteInsertAsync(setters)` (EF-generated SQL, in `ExecuteUpdate/`). `ExecutePagedQueryAsync` and `GetLastInsertId` (used by `ExecuteInsertAsync`) use `SqlParser/` to rewrite the SQL rather than regex.
- `SqlParser/` — a small tokenizer (not a full SQL parser) used to build the paged and count queries and the "get last insert id" query. `SqlDialect` is the abstract base, with `SqliteDialect` / `SqlServerDialect` overriding what differs: quotes, whether block comments nest (SQL Server) and whether an unclosed `/*` is allowed to run to the end (SQLite), trailing clauses (`LIMIT`, `OPTION`, `FOR XML`...) and how a query is paged. `Tokenize` skips whitespace and comments and tracks bracket depth. `Analyse` splits a SELECT into a `SqlQueryShape` (prefix / query / outer `ORDER BY` / tail), using positions in the original string so the caller's formatting and `{0}` placeholders are kept. Anything the library adds must be appended after the end of the last token, and on a new line, so it can never end up inside a caller's trailing comment. The tokenizer tests (`Tests/SqlParser/SqlDialectTests.cs`) need no database. Tests that run the generated SQL are in `Tests/Execute/ExecutePagedQueryTests.cs` and `ExecuteInsertTests.cs`.
- `CaseInsensitivity/` — case insensitivity, in the `CaseInsensitivity` namespace. `CaseInsensitiveCollationExtensions` (`UseCaseInsensitiveCollation`, both databases) sets a case insensitive collation in the model: `Latin1_General_100_CI_AS_SC_UTF8` on SQL Server (the database collation), `NOCASE` on every string property on SQLite. `SqliteNoCaseReplacementExtensions` (`UseSqliteNoCaseReplacement`, SQLite only) replaces SQLite's ASCII-only `NOCASE` collation with a Unicode one matching the SQL Server collation, via `SqliteNoCaseReplacementInterceptor`.

## Code style

Files consistently use `#region`/`#endregion` blocks to group members (e.g. `#region public methods`, `#region private constants`), and every public member has full XML doc comments (`GenerateDocumentationFile` is enabled and used to publish API docs via `DefaultDocumentation`). Match this structure in new/edited files.

**Test classes are an exception to the global region rule** (which would put the `[TestMethod]`s in `#region public methods`). Here a test class has `#region private fields` (the `TestContext`), `#region public constructors`, then one `#region <Feature> tests` per method or feature being tested (e.g. `#region WithTableHints tests`, `#region nocase_instr tests`), then `#region private methods` for helpers. Follow this, not `#region public methods`.

`ArgumentNullException.ThrowIfNull(...)` (or `ArgumentException.ThrowIfNullOrEmpty`) is the standard guard-clause style at the top of public methods.
