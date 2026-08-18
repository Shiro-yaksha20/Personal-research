# Measurements

Raw data from probing SystemCleaner's runtime behaviour. Every number cited in PLAN.md's §3 "Current state (measured)" traces back here.

## Files

| File | Source | Description |
|---|---|---|
| [2026-08-08-runtime-probe.txt](./2026-08-08-runtime-probe.txt) | Headless probe run against Shiro's i5-7300U | Startup entries enumerated, installed apps, LHM sensors, cleanup scan sizes, registry walk timings |
| [2026-08-08-perf-probe.txt](./2026-08-08-perf-probe.txt) | Perf-focused probe on the same machine | LHM `Computer.Open()` timing (2.7 s), Update() per tick (2-4 ms), PerformanceCounter fallback (failed on this machine), full user-temp scan (216 ms, 749 files, 760 MB), HKLM\SOFTWARE walk depth ≤ 8 (28.2 s, 352 205 keys) |
| [2026-08-08-registry-test.csx](./2026-08-08-registry-test.csx) | Small C# script | Confirmed the "double-backslash registry path" in SystemInfoViewModel is NOT a bug (Windows normalises adjacent separators) |

## Test machine specs (2026-08-08 baseline)

- CPU: Intel i5-7300U (2 cores / 4 threads, 2.6 GHz base)
- RAM: 8 GB
- Storage: SSD
- GPU: Intel HD Graphics 620 (integrated, no dedicated VRAM)
- OS: Windows 10 Pro 19045 (22H2)
- Elevation: unelevated
- .NET SDK: 9.0.316

**Important caveat from critique:** The engineer persona pointed out this is a developer machine with Visual Studio + Office installed. The 352 205 keys in HKLM\SOFTWARE include HKLM\SOFTWARE\Classes (CLSID + Interface + AppID + TypeLib) which balloons on dev machines. On a clean Win11 Home laptop the walk might be 4-8 s, not 28 s.

**Action item:** Before publishing perf claims (Wave 4 release notes), re-measure on 2-3 real user machines (clean Win10 install, clean Win11 install, older laptop with HDD). See `../decisions/0006-cut-waves-6-7-from-committed-roadmap.md` context.

## How to add new measurements

1. Run the probe (`../../../project review update/systemcleaner/probe/` in the code repo, or write a new one).
2. Save the raw output as `YYYY-MM-DD-<what-was-measured>.txt` here.
3. Update this README to describe what was measured and machine specs.
4. If findings change PLAN.md, log in CHANGELOG.md.

Never edit existing measurement files. They are historical record. If a measurement is superseded (e.g., re-run on different hardware), add a new file with a new date and note the relationship in this README.
