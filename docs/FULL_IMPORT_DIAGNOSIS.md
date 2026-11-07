# Full results import investigation

Investigated locally on 7 November 2026 using the current repository source and the existing Docker RabbitMQ, PostgreSQL, and web app. The database already contained 241,380 results and was preserved.

## Reproduction

```sh
docker exec power-lines-data-service dotnet run --project ./PowerLinesDataService -- --results --all
```

The baseline attempted 748 league/season sources, from 1993/94 through 2026/27, and exited with code 134. Only the first file reached its completion notification. There were:

| Failure | Sources | Cause and fix |
| --- | ---: | --- |
| Duplicate messaging channel name | 577 | Recreating the `imports` sender after each file. Reuse a sender per import, with distinct channel names for fixtures and results. |
| Repeated seasonal pairings | 131 | Scottish schedules legitimately repeat a home/away pairing. Include the played date in completed result identities; retain fixture identities for rescheduling. Exact duplicate matches still fail before publishing. |
| Missing `HomeTeam` header | 5 | Greek 2000/01–2004/05 files use `HT`/`AT`. Accept those aliases while preferring the modern headers. |
| Duplicate `LB` header | 1 | German D2 2004/05 repeats an unused bookmaker label three times. Ignore that label without shifting the other columns. |
| Unavailable source | 33 | HTTP 300, 404, or other retrieval failures. Log the URL and continue without failing the job. |

The baseline also emitted 20,642 row diagnostics, mostly for comma-only padding. Empty rows are now skipped while incomplete records still produce diagnostics.

The 1999/2000 URL also used `99100`; use `9900` directly rather than depending on the source server's redirect. Source URLs remain important for season provenance.

HTTP clients are reused during the results run, responses are disposed, and the broker connection closes even if the import fails. Genuine parsing and publishing failures still produce a failing job after other sources have been attempted.

## Validation

The corrected publishing run imported 715 sources (245,067 rows), skipped 32 unavailable sources, and exposed the German duplicate-header case. After correcting it, that source was replayed successfully through the same parsing, identity, publishing, and completion logic: another 306 rows, for 245,373 published results across 716 sources.

A final pass downloaded and validated all sources through `ResultImport`, including identity checks, with result and status publishing disabled to avoid adding another full dataset to the web consumer's backlog. It completed successfully: **716 sources, 245,373 rows, 32 unavailable sources, zero failed sources**. Full logs are retained locally under `/tmp/power-lines-import-diagnosis/`.

Four source rows remain incomplete and are skipped:

| Source | Record |
| --- | --- |
| T1 2013/14 | Trailing spreadsheet padding containing `#REF!` rather than match details |
| G1 2014/15 | Niki Volos–OFI, 10 May 2015: missing full-time scores |
| G1 2018/19 | Panathinaikos–Olympiakos, 17 March 2019: missing full-time scores |
| F2 2025/26 | Bastia–Red Star, 5 December 2025: missing full-time scores |

Source availability can change between runs. Completion messages record publication, not completion of all downstream database writes; the web app processes those asynchronously.

```sh
dotnet test PowerLinesDataService.Tests --configuration Release
```

All 16 tests pass. Coverage includes complete historical E0 1993/94, SC0 1994/95, G1 2000/01, D2 2004/05, and E0 2018/19 files; repeated completed pairings; score corrections with stable identities; fixture rescheduling identities; legacy score aliases; millennium URLs; sender reuse; skipped HTTP failures; and genuine parsing, publishing, and duplicate failures.
