# Header-mapped prices and staged reconciliation

This companion change repairs the one-shot importer used by Power Lines Web. It has no database connection and does not repair deployed rows simply by being merged. The web consumer revision/provenance changes must be rolled out first. Link to the web PR is recorded in the PR description.

`ResultReader` and `FixtureReader` now use .NET's quoted-field CSV parser and named headers; a missing `Time` never changes row indexes. UTF-8 names remain intact. Dates use explicit day/month/year formats and numbers use invariant parsing. Missing half-time observations remain nullable; missing/invalid price triplets remain missing. Diagnostics identify rejected rows. Average, maximum, closing and sampled individual bookmaker families travel separately, with file SHA-256, source season, row identity and source URL. Individual quote observation timestamps remain unknown. Source column and collection definitions: [Football-Data notes](https://football-data.co.uk/notes.txt), [collection conventions](https://football-data.co.uk/downloadm.php).

Result seasons come from the source URL rather than calendar dates, so delayed 2019/20 matches cannot collide with 2020/21 pairings. Fixture identities remain calendar-partitioned where the global fixture feed supplies no season.

The source does not provide stable match/team IDs. The derived seasonal ordered-team pairing supports ordinary fixture rescheduling, but repeated seasonal pairings are quarantined before sending. Renames and ambiguous pairings require explicit identity/alias reconciliation; Unicode removal and fuzzy automatic merging are prohibited. Stable authoritative IDs and cross-division transfer remain dependencies, not completed coverage improvements.

## Reproduce and stage without RabbitMQ

Use .NET 8:

```bash
dotnet test PowerLinesDataService.Tests --configuration Release
dotnet run --project PowerLinesDataService --configuration Release -- \
  --stage --source-file /path/to/1819-E0.csv \
  --source-url https://www.football-data.co.uk/mmz4281/1819/E0.csv \
  --output /tmp/e0-stage
```

`stage.json` contains eligible source rows, hashes, quarantined identities and diagnostics. This path exits before host/message connection setup. All 380 E0 2018/19 rows parse without diagnostics; their averages differ from the stored maxima. The full source fixture is frozen in the tests with the reconciliation URL above.

`scripts/reconcile-sources.py --snapshot <web-export.jsonl> --output <directory>` freezes source files and creates `source-reconciliation.json`, qualified historical average quotes and staged numeric corrections. It matches division/date/team names and full-time scores. It reads no live database and sends no messages. `scripts/build-repair-stage.py --stage <staged-price-corrections.json> --output <directory>` generates guarded SQL plus a numeric inverse. Execution is a separate, reviewed rollout action.

Across 176 frozen files (2017/18, 2018/19 and 2021/22–2026/27, 22 divisions), 54,323 identity/score-matched rows have qualified source averages; 15,478 numeric differences are staged. These include the proven 380/380 E0 maxima. Do not generalize the E0 mapping diagnosis to every staged difference: review each season/division and source schema. Timing is unknown, so qualification supports retrospective comparisons, not executable bookmaker returns. See `source-reconciliation-v1.json`.

The isolated PostgreSQL test in `staged-repair-test.json` repaired all 380 E0 rows, changed zero on a second apply, preserved all original ingestion times, left score revisions unchanged, and restored all numeric triplets through the inverse. It did not touch the shared database.

## Actual import path and freshness dependency

`Program` → `ImportService.RunImports` → `ImportFactory` → `FixtureImport` / `ResultImport` downloads `fixtures.csv` or `mmz4281/{season}/{division}.csv`, then publishes to `fixtures` / `results` fanout exchanges. Successful source batches also publish completion records to the separate `imports` exchange. The web application persists them per division. Average prices never become available bookmaker offers.

`scripts/run-service` is a manual container invocation. The repository's workflow builds/pushes an image on master; it does not schedule imports. There is no local user crontab and the inspected local stack contains web, RabbitMQ and PostgreSQL only. An external production scheduler was not identified in the available checkout; do not claim it has been repaired. Deployment ownership, schedule, source coverage and first successful current-season completion must be confirmed before live coverage can be claimed. HTTP/parse failures do not produce a false successful completion; result batches surface failed source lists with a nonzero process exit.

## Rollout and rollback (not executed)

1. Stop scheduled producers/consumers for the repair window. Record queue depth and source file hashes. Take a consistent PostgreSQL backup and verify restore into a disposable database before any production migration/re-import.
2. Apply the companion web migrations with workers controlled. Keep legacy timestamps/provenance unknown. Deploy compatible web consumers before publishing the new nullable half-time/source payloads or `imports` completion events.
3. Review the staged before/after report, exact identity/score guards, counts, anomalous overrounds and any unresolved aliases. Apply only approved source cohorts in one transaction; preserve `Created`, increment revisions/`Updated`, and retain missing values rather than erasing known good prices. Re-run the stage to confirm zero additional changes. Do not run unrestricted `--all` as an unreviewed repair.
4. Restore the importer schedule and inspect actual completion records/current-season per-division coverage. Verify fixture kickoff timezone conventions before enabling live workers; source date-only historical outcomes still exclude the whole forecast day. Capture actual timestamped bookmaker offers separately if actionable EV is required.
5. Roll back application images with producers paused if needed; retain additive evidence tables. The generated inverse restores numeric triplets only and guards the corrected values/source hash. Full source provenance/half-time restoration requires the verified pre-rollout backup; do not claim the numeric inverse is a complete backup. Preserve forecast archives and correction history when planning restoration.

No shared migration, re-import, deployment, merge or bet was performed by this implementation.
