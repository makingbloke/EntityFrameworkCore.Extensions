# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Overview

EntityFrameworkCore.Extensions is a library of extension/helper methods for Entity Framework Core 10 (targeting net10.0), supporting **SQL Server and SQLite only**. It adds capabilities EF Core doesn't provide natively: database-independent free text search, table hints, ExecuteUpdate/Delete/Insert variants that return affected rows, unique constraint violation detection, case-insensitive collation helpers, and raw-SQL execute/paging helpers.

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

1. A LINQ query is tagged (`.TagWith(...)`) with a magic string prefix (e.g. `TableHintExtensions.TableHintsTagPrefix`).
2. The provider-specific query generator (`SqlServerCustomQueryGenerator` / `SqliteCustomQueryGenerator`) recognizes the tag while generating SQL and rewrites the statement (e.g. injects `WITH (NOLOCK)` table hints).
3. For "get rows back" operations (`ExecuteDeleteGetRowsAsync`, `ExecuteInsertGetRowAsync`, `ExecuteUpdateGetRowsAsync`), an `AsyncLocal<ExecuteUpdateParameters>` (`ExecuteUpdateExtensions.ExecuteUpdateParameters`) carries per-call state across the async flow. A `DbCommandInterceptor` (`ExecuteUpdateInterceptor`) captures the generated SQL/parameters from the real EF `ExecuteUpdate`/`ExecuteDelete` pipeline *and suppresses execution* (`InterceptionResult.SuppressWithResult`), then the extension method re-runs the captured SQL itself via `SqlQueryRaw` to materialize and return the affected rows.
4. The `AsyncLocal` **must** be reset to `null!` in a `finally` block after use — a past bug class here was values leaking into unrelated concurrent/subsequent commands (see git history: "AsyncLocal leaked into the next command"). Follow this pattern exactly when touching this code: set before, always clear in `finally`, and guard against re-entrancy by throwing if a value is already set.

Note `ExecuteUpsertGetCountAsync`/`ExecuteUpsertGetRowsAsync` implement upsert as an explicit transaction (update-then-insert-if-zero-rows) rather than native `MERGE`/`ON CONFLICT`, specifically so update and insert can target different property sets — see the code comment in `ExecuteUpdateExtensions.cs` for the reasoning.

### Free text search translation

`FreeTextSearchFunction/` and `MatchFunction/` add custom `EF.Functions.*` methods translated per-provider via `IMethodCallTranslatorProvider` implementations (`SqlServerFreeTextSearchTranslatorProvider`, `SqliteFreeTextSearchTranslatorProvider`, `MatchTranslatorProvider`), all deriving from the shared `Utilities/TranslatorProviderBase`. SQLite free text search additionally needs a `SqliteFreeTextSearchInterceptor` to swap in a stemming vs. non-stemming FTS table depending on the call; because of how EF parses expressions, only the *first* `FreeTextSearch` call's `useStemming` value is honored per SQLite query (documented limitation, see `ReadMe.md`).

### Other notable pieces

- `UniqueConstraint/` — `UniqueConstraintInterceptor` (a `SaveChangesInterceptor`) turns provider-specific unique constraint violation exceptions into a structured `UniqueConstraintException` with parsed `UniqueConstraintDetails`, via provider-specific `IExceptionProcessor` implementations.
- `Execute/ExecuteMethods.cs` — raw SQL execute helpers behind the public `DatabaseFacade` extensions in `ExecuteExtensions.cs` (`ExecuteScalarAsync`, `ExecuteQueryAsync`, `ExecuteNonQueryAsync`, `ExecuteInsertAsync`, `ExecutePagedQueryAsync`). The SQL here is written by the library's caller, not generated by EF Core, so it can contain comments, strings and quoted identifiers anywhere. Don't confuse `DatabaseFacade.ExecuteInsertAsync<T>(sql, ...)` (raw SQL, here) with `IQueryable<TSource>.ExecuteInsertAsync(setters)` (EF-generated SQL, in `ExecuteUpdate/`). `ExecutePagedQueryAsync` and `GetLastInsertId` (used by `ExecuteInsertAsync`) use `SqlParser/` to rewrite the SQL rather than regex.
- `SqlParser/` — a small tokenizer (not a full SQL parser) used to build the paged and count queries and the "get last insert id" query. `SqlDialect` is the abstract base, with `SqliteDialect` / `SqlServerDialect` overriding what differs: quotes, whether block comments nest (SQL Server) and whether an unclosed `/*` is allowed to run to the end (SQLite), trailing clauses (`LIMIT`, `OPTION`, `FOR XML`...) and how a query is paged. `Tokenize` skips whitespace and comments and tracks bracket depth. `Analyse` splits a SELECT into a `SqlQueryShape` (prefix / query / outer `ORDER BY` / tail), using positions in the original string so the caller's formatting and `{0}` placeholders are kept. Anything the library adds must be appended after the end of the last token, and on a new line, so it can never end up inside a caller's trailing comment. The tokenizer tests (`Tests/SqlParser/SqlDialectTests.cs`) need no database. Tests that run the generated SQL are in `Tests/Execute/ExecutePagedQueryTests.cs` and `ExecuteInsertTests.cs`.
- `CaseInsensitivity/` — SQLite-only: replaces the default `NOCASE` collation behavior via `SqliteNoCaseReplacementInterceptor`.

## Code style

Files consistently use `#region`/`#endregion` blocks to group members (e.g. `#region public methods`, `#region private constants`), and every public member has full XML doc comments (`GenerateDocumentationFile` is enabled and used to publish API docs via `DefaultDocumentation`). Match this structure in new/edited files. `ArgumentNullException.ThrowIfNull(...)` (or `ArgumentException.ThrowIfNullOrEmpty`) is the standard guard-clause style at the top of public methods.
