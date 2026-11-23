### TODO - Solution review (22 November 2026)

#### Possible improvements

18. **SQLite `FreeTextSearch`: honour `useStemming` on every call, not just the first** (the limitation documented in the ReadMe). Accepted as a limitation for now (23 November 2026), as any fix needs fairly extensive changes. The idea, if it's picked up:
    * Instead of swapping the whole table for its stemming table, rewrite each `FreeTextSearch(e.Field, text, true)` call into a non-stemming search of the stemming table, matched by key: `stemmingSet.Where(s => FreeTextSearch(s.Field, text, false)).Select(s => s.Id).Contains(e.Id)`. EF Core translates it to `"t"."ROWID" IN (SELECT "t0"."ROWID" FROM "TestFreeText_Stemming" AS "t0" WHERE MATCH(...))`, and a non-stemming then a stemming call in the same query each worked (tried by hand on EF Core 11).
    * It needs the stemming table to have the same keys (`ROWID`) as the main table, row for row, and a single-column key.
    * The hard part is knowing which table each call searches, as the main and stemming tables share a CLR type. Picking the one entity type of that CLR type with a stemming table fails for two such tables of the same CLR type in one query (e.g. a stemming search on one and a non-stemming search on the other). Options: require different CLR types; pass a subquery for every candidate stemming table to an internal marker method and let the translator (which knows the column's table) pick one (needs a prototype); or follow each call's lambda parameter back to its query root (fragile through joins and `SelectMany`).
