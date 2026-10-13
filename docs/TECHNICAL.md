# ScrapeX

.NET Framework 4.8 Windows Forms proxy collector and checker. Open `ScrapeX.sln` in Visual Studio; use **View Designer** on `Form1.cs` or `ExportForm.cs`. Default client size is 1440 × 1000, and the results area grows from 232 to 382 pixels tall. Smaller screens open maximized within the working area. Controls and events remain in the standard Designer files; flags and branding are local resources.

## Run

Extract `dist/ScrapeX-portable.zip` and run `ScrapeX.exe` with its DLL/config files on Windows with .NET Framework 4.8. The 168-feed catalog is embedded in the EXE; `sources.txt` is optional. **Check HTTP + TLS** automatically collects sources when there are no candidates. You can also collect or import first. The default **Working Proxy** filter shows HTTPS-verified and HTTP-only routes. Country cells show a small local flag, ISO code and name; hover for lookup provenance. Right-click a row to retry country lookup. Separate export dialogs support country/protocol selection and one file per country.

`Ping (TCP)` measures TCP setup, not ICMP; `TLS ms` measures the successful HTTPS probe through response headers. TLS verification never bypasses certificate or hostname checks. A final HTTP 403 received inside validated TLS proves HTTPS transport works but does not mean the target permits normal use. Plaintext responses to `/generate_204` must be 204.

## Bounded two-stage checker

- TCP admission and protocol testing use separate asynchronous queues. With base parallelism 200, up to 600 TCP admissions and 200 protocol jobs run; admission is capped at 1,000, protocol jobs at the chosen maximum of 500. The ready queue holds at most twice the base parallelism and applies asynchronous backpressure.
- Each admitted endpoint independently tests HTTP and HTTPS, racing HTTP/SOCKS5/SOCKS4. A successful protocol cancels and drains its siblings. Each protocol job uses at most six sockets; TCP admission uses one. There is no OS thread per proxy.
- **Timeout / stage** gives TCP admission and the subsequent probe round their full selected budgets. Time waiting in the ready queue does not consume a healthy proxy's probe budget. An unreachable endpoint stops after TCP admission; a slow admitted endpoint can use up to roughly two budgets. Optional alternate targets add one further probe round.
- Read-ahead buffers reduce per-byte network reads while preserving unread bytes between HTTP, SOCKS and TLS. Writes are unbuffered to avoid handshake deadlocks. Probe sockets use TCP_NODELAY and abortive close to avoid accumulating TIME_WAIT ports for these short-lived validation connections. A shared weighted budget bounds simultaneous checker sockets to min(1,024, parallelism × 9). TCP admission reserves one slot and a protocol job six. Local socket-capacity failures halve the budget, cool down for two seconds and retry the same endpoint with its full timeout, without recording a failed verdict. Persistent pressure remains cancellable. Budget growth is intentionally deferred until the next run.
- Input shuffling is linear-time and runs off the UI thread. Progress reports TCP/protocol stage jobs (including budget waiters) and completed proxies/sec. Cancellation drains both stages; output failures remain visible rather than being hidden by peer cancellation.
- This improves scheduling without deliberately shortening a slow proxy's timeout. It cannot guarantee discovery of every usable public proxy under every network condition. High parallelism can still overload the network; reducing it may improve accuracy on constrained connections.

## Resilient country lookup

The previous single-provider implementation and its artificial 100-lookup cutoff were removed. Two independent metadata workers try three HTTPS providers directly, then through the result's detected HTTP/SOCKS4/SOCKS5 protocol if direct requests fail:

- [Country.is](https://country.is/)
- [IPWhois](https://ipwhois.io/documentation)
- [ipapi.co](https://ipapi.co/developers/)

Every request explicitly contains the endpoint IP. A response is accepted only when its returned IP matches and its country code is valid. Country names are resolved locally from ISO codes, and flags come from existing resources; no remote flag download is required. These are **proxy-server IP countries**, not verified exit-IP countries or guaranteed physical server locations.

Positive results persist for seven days in `%LOCALAPPDATA%\ScrapeX\country-cache.json`, capped at 20,000 entries. There is no permanent negative cache. Temporary failures are automatically retried up to three rounds, then shown as **Unknown (retry available)**. Providers have separate rate gates and 429/Retry-After cooldowns shared across direct/proxy routes; switching routes does not bypass provider quotas. Failures, rate limits and invalid responses appear in Activity. Hover a country cell for its source/time. All providers can be unavailable or lack data; the app never fabricates a country.

## Storage and sources

- `%LOCALAPPDATA%\ScrapeX\sources.txt`: saved source preferences.
- `%LOCALAPPDATA%\ScrapeX\sources.catalog.txt`: previous bundled catalog, used to add new feeds on upgrades while preserving custom feeds, commented entries and subsequent user deletions. Empty/missing/unreadable preferences fall back to the embedded catalog. **Restore bundled sources** resets the editor to the shipped list.
- `%LOCALAPPDATA%\ScrapeX\Results\<session>\HTTPS\<protocol>.txt`: validated HTTPS routes.
- `HTTP_ONLY\<protocol>.txt`: HTTP worked, TLS verification failed; never mixed with HTTPS output.
- `Full Detail.txt`: one row per working endpoint, with country code/name, verified protocol, HTTPS/SSL verdict, TCP ping, HTTP and TLS latency, separate HTTP/HTTPS protocols, TLS version, UTC test time, country lookup provenance and test details. Unknown measurements are `--`; no guessed values. Updates are batched every two seconds and atomically replace the file. Stop/end flushes remaining rows; late country lookups update the same session file, even after checking ends.
- Both country export dialogs now offer **Full Detail**, including when exporting one file per country. Plain endpoint exports remain available.
- Country grouping is captured when the export dialog opens; reopen it after background lookups finish to refresh.

The first audit covered 243 URLs on 2026-10-07 and bundled 148 (all 36 requested URLs + 112 additions). A second multilingual website audit added 20 native-client-verified feeds, bringing the embedded total to 168. See `reports/website-audit.html` and `reports/website-audit/live.json`: the saved original baseline contained 327,043 unique candidates; the new live feeds added 22,228 distinct candidates (combined 349,271). These are untested candidates; the baseline was captured earlier and this is not a simultaneous two-catalog comparison. Per-feed additions depend on completion order when pools overlap. See `reports/source-audit.html` for the searchable report, `reports/Source audit.txt` for plain text, and `reports/catalog-audit/audit.json` for exact timestamps, response hashes and per-file GitHub history links. GitHub freshness uses the exact file and branch, not repository push time or README promises. The 36 requested feeds all returned parseable endpoints: 23 GitHub files were recent, 10 stale, and 3 were APIs with no independently verified publication date. Stale requested feeds remain included and annotated: r00tee (2026-08-15), Skiddle-ID (2025-11-26), MuRongPIG (2025-06-26), prxchk (2024-04-13). Stale does not prove permanent abandonment.

The new website adapters support Base64 data literals, nested IP/port spans, hidden port decoys and explicit address attributes. Nine website hosts use bounded next-page links; publicly offered bulk downloads are preferred where available. HTTP 308 redirects now work on .NET Framework with a five-hop bound. No port is guessed and no remote JavaScript is executed.

The scraper handles strict endpoint text, protocol-prefixed lists, space-separated IP/port, supported metadata suffixes, JSON arrays/objects, HTML table IP/port cells and explicit textarea/pre lists. It does not execute site scripts. Protocol hints survive parsing; country and SSL claims from feeds are not treated as verified results. Geonode, Proxiware and FreeProxy World use bounded pagination with repeated-page detection. A reachable feed does not imply fresh or working entries. Scraping retains six download slots, a 20-second per-page timeout, 16 MB page limit, 20-page limit and 500,000 candidate cap. IPv6 proxy addresses and authenticated proxies are not implemented. Original backups are under `backups`.

## Verification

Run `tools/build-and-test.ps1`. It builds Release and runs regression, performance, country, scheduler, resilience and catalog/export suites. Reports:

- `reports/tests.txt`: 60 regression assertions, including all protocol handshakes, invalid certificates, UI, country flags/codes, exports, timeout and cancellation.
- `reports/performance-tests.txt`: comparison with the old 12-sequential-attempt schedule; this is an older baseline, not the previous version's staged scheduler.
- `reports/SchedulerTests.txt`: queue backpressure, slow healthy endpoints, cancellation, output-error propagation and comparison against the immediately preceding coupled-worker version. Initial TCP-only comparison: 1.284s versus 0.428s for 24 local refused endpoints, base parallelism 4 and 200ms timeout. This controlled result is not an internet speed promise.
- `reports/ResilienceTests.txt`: 20 assertions covering website formats, same-site pagination, HTTP 308/redirect loops, non-writable streams, 50,000 simulated endpoints with TCP/protocol socket exhaustion, full timeout preservation, weighted socket bounds and cancellation. This is a deterministic fault-injection test, not 50,000 real public proxy checks.
- `reports/CountryTests.txt`: provider failure, mismatched IP, code validation, persistent cache, proxy fallback, rate limits and response parsing.
- `reports/catalog-tests.txt`: embedded defaults without a sidecar, all 36 requested URLs, upgrade/disable behavior, HTML/JSON/text parsing, country updates after checking ends, atomic concurrent output and Full Detail country export.
- `reports/catalog-live-scrape.txt`: real collection through the app's scraper using the complete embedded catalog.
- `reports/live-full-detail/Full Detail.txt`: real HTTPS sample with measured latency and resolved country. The sample is a dated test, not a guarantee of future availability.
- `reports/country-live.txt`: the two screenshot IPs resolved as NL / Netherlands and VE / Venezuela.
- `reports/country-proxy-live.txt`: all three country providers returned VE for 154.59.56.78 through a real HTTPS proxy tunnel.
- `reports/live-pipeline-check.txt`: latest fresh-source sample, 1/8 HTTPS successful. Earlier 7/8 results are historical; public proxies changed between runs.
- `reports/ui-preview*.png`: UI render checks (rows labeled Fixture are synthetic).

Both Debug and Release target .NET Framework 4.8. Newtonsoft.Json 13.0.3 is the only external runtime assembly. TLS authentication follows the platform [SslStream](https://learn.microsoft.com/en-us/dotnet/api/system.net.security.sslstream?view=netframework-4.8.1) validation behavior.
