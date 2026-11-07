# Header-mapped imports and clean database rebuild

This companion change repairs the one-shot importer used by Power Lines Web. It has no database connection and does not repair deployed rows simply by being merged. Deploy the compatible web consumers and schema before publishing the new import payloads. Link to the web PR is recorded in the PR description.

`ResultReader` and `FixtureReader` now use .NET's quoted-field CSV parser and named headers; a missing `Time` never changes row indexes. UTF-8 names remain intact. Dates use explicit day/month/year formats and numbers use invariant parsing. Missing half-time observations remain nullable; missing/invalid price triplets remain missing. Diagnostics identify rejected rows. Average, maximum, closing and sampled individual bookmaker families travel separately, with file SHA-256, source season, row identity and source URL. Individual quote observation timestamps remain unknown. Source column and collection definitions: [Football-Data notes](https://football-data.co.uk/notes.txt), [collection conventions](https://football-data.co.uk/downloadm.php).

Result seasons come from the source URL rather than calendar dates, so delayed 2019/20 matches cannot collide with 2020/21 pairings. Fixture identities remain calendar-partitioned where the global fixture feed supplies no season.

The source does not provide stable match/team IDs. The derived seasonal ordered-team pairing supports ordinary fixture rescheduling, but repeated seasonal pairings are quarantined before sending. Renames and ambiguous pairings require explicit identity/alias reconciliation; Unicode removal and fuzzy automatic merging are prohibited. Stable authoritative IDs and cross-division transfer remain dependencies, not completed coverage improvements.

## Parser regression and source evidence

Use .NET 8 to run the parser regression suite without RabbitMQ:

```bash
dotnet test PowerLinesDataService.Tests --configuration Release
```

All 380 E0 2018/19 rows parse without diagnostics; the regression verifies their averages against the previously misread maxima. The full source fixture is frozen in the tests.

After removing the staging workflow, all five parser tests passed again in the .NET 8.0.33 container on 2026-11-07. No broker or database was used.

The companion web repository retains the [source qualification script](https://github.com/johnwatson484/power-lines-web/blob/feature/win-odds-implementation/scripts/reconcile-sources.py) and [frozen reconciliation evidence](https://github.com/johnwatson484/power-lines-web/blob/feature/win-odds-implementation/evaluation/evidence/win-odds-v1/source-reconciliation.json) used by the offline benchmark. Across 176 frozen files (2017/18, 2018/19 and 2021/22–2026/27, 22 divisions), 54,323 identity/score-matched rows have qualified source averages; 15,478 stored numeric triplets differ, including all 380 E0 rows. Do not generalize the E0 mapping diagnosis to every difference. Quote timing remains unknown, so this evidence supports retrospective comparisons, not executable bookmaker returns.

The staged repair command, SQL generation, numeric inverse and their duplicate source-audit files have been removed. The rollout rebuilds imported data in an empty database.

## Actual import path and freshness dependency

`Program` → `ImportService.RunImports` → `ImportFactory` → `FixtureImport` / `ResultImport` downloads `fixtures.csv` or `mmz4281/{season}/{division}.csv`, then publishes to `fixtures` / `results` fanout exchanges. Successful source batches also publish completion records to the separate `imports` exchange. The web application persists them per division. Average prices never become available bookmaker offers.

`scripts/run-service` is a manual container invocation. The repository's workflow builds/pushes an image on master; it does not schedule imports. There is no local user crontab and the inspected local stack contains web, RabbitMQ and PostgreSQL only. An external production scheduler was not identified in the available checkout; do not claim it has been repaired. Deployment ownership, schedule, source coverage and first successful current-season completion must be confirmed before live coverage can be claimed. HTTP/parse failures do not produce a false successful completion; result batches surface failed source lists with a nonzero process exit.

## Clean database rollout and rollback (not executed)

1. Pause scheduled import producers and retain the old database for rollback. Create a separate empty database, then apply the companion web migrations. A full import rebuilds imported results and fixtures; it cannot reconstruct historical pre-match forecasts, actual timestamped offers or their audit history.
2. Deploy compatible web consumers and the fixed importer. Isolate the new consumers from queued payloads belonging to the old deployment. Keep consumers running during import: `Workers:Enabled=false` disables message consumption as well as analysis. Configure the verified `Feed:FixtureTimeZoneId` before enabling live forecasts.
3. Run results and fixtures as separate importer invocations, using the intended broker configuration:

   ```bash
   dotnet run --project PowerLinesDataService --configuration Release -- --results --all
   # After results have been consumed and persisted:
   dotnet run --project PowerLinesDataService --configuration Release -- --fixtures
   ```

   The full results import attempts every configured season/division since 1993. Missing historical files, parse/HTTP errors and ambiguous repeated seasonal pairings must be reviewed; a full import can exit nonzero after publishing successful batches. Do not treat process exit or a source completion message as proof that all results have been persisted. Wait for result queues to drain and verify persisted per-source counts before importing fixtures.
4. Check rejected rows, source hashes, season/division coverage, newest results, fixture dates and representative average prices (including E0 2018/19). Resolve unexpected gaps before switching the web application to the rebuilt database. Confirm the actual recurring import schedule and current-season completion records. Collect actual timestamped bookmaker offers separately if actionable EV is required.
5. Cut over only after validation, then resume the import schedule. If rollback is needed, pause producers/consumers, restore the old application/database configuration and switch back to the retained database. Keep the rebuilt database and any new immutable forecast evidence for audit; do not drop the old database as part of this rollout.

No shared database reset, migration, re-import, deployment or merge was performed by this cleanup.
