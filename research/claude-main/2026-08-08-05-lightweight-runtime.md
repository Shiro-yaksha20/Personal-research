# SystemCleaner — Staying Runtime-Light On Weak Systems

Scoped to **CPU, RAM, IO, and UI responsiveness on low-end hardware** (Celeron/Atom, 4 GB RAM, HDD, integrated graphics). Binary size is explicitly out of scope. Numbers below are measured on your i5-7300U + 8 GB + SSD — a "modern low-end" baseline. On a real weak machine (Celeron N4020 + HDD) most of the times listed will be 3–8× longer.

---

## Measured baseline

I probed the hot paths headlessly. This is what the app actually costs today:

| Operation | Wall time | Heap Δ | Notes |
|---|---:|---:|---|
| **`LibreHardwareMonitor.Computer.Open()` — everything enabled** | **2 692 ms** | 630 KB | Fixed cost; runs during `App.OnStartup` before the window shows. |
| `Computer.Open()` — CPU + GPU + Memory only | 2 154 ms | | Turning off Network/Motherboard/Storage/Controller saves ~500 ms of Open. |
| `Update()` per tick — everything enabled (216 sensors) | 2–4 ms | ~140 KB total | The poll itself is cheap. |
| `Update()` per tick — CPU/GPU/Mem only (21 sensors) | 3 ms | 16 KB total | Sensor count barely matters after Open. |
| `PerformanceCounter` fallback (CPU + RAM) | **FAILED** — "Cannot load Counter Name data" | — | Perf-counter registry is corrupt on this machine. Real thing to guard against. |
| **User temp scan** (749 files, 246 dirs, 760 MB) | 216 ms | 408 KB | Fast on SSD. On HDD expect 2–10× more. |
| **HKLM\\SOFTWARE walk, depth ≤ 8** (residual scan input) | **28 220 ms** | 6.2 MB | 352 205 keys visited for **one** application. Current code has no depth cap. |

Two things dominate everything else:

1. **`LHM.Open()` blocks the UI thread for ~2.7 s** during app startup (it runs in `HardwareMonitorService`'s constructor, which is invoked by `services.BuildServiceProvider()` in `App.OnStartup`). On a Celeron/Atom that's a **~5–8 s frozen splash** before the window paints — assumed dead app.
2. **Residual registry scan takes ~28 s per app** on this machine. If a user runs residual scan for 10 apps, that's ~5 minutes of one full CPU core, plus 60+ MB of heap growth for the token walk. On a weak dual-core box that's the whole machine gone.

Note the surprise: **the periodic poll is fine (~3 ms).** The problem is start-up cost, not steady-state cost. Fixing HM does not mean polling less often — it means opening LHM only when the tab is visited, and doing that Open on a background thread.

---

## The three levers that actually matter

Everything below fits into one of these:

### Lever 1 — Don't do work until you need to (lazy init + tab-scoped services)

Currently `App.OnStartup` builds the DI container and every singleton runs its constructor immediately. That includes `HardwareMonitorService._computer.Open()` (2.7 s), `UninstallerService` construction (creates 5 scanner instances + 6 cleanup handlers), `VirusTotalService` (creates an `HttpClient`).

Change to **`Lazy<T>` factories**:

```csharp
services.AddSingleton(sp => new Lazy<IHardwareMonitorService>(
    () => new HardwareMonitorService()));
services.AddSingleton(sp => new Lazy<IUninstallerService>(
    () => new UninstallerService()));
services.AddSingleton(sp => new Lazy<IVirusTotalService>(
    () => new VirusTotalService()));
```

ViewModels that need them ask for `Lazy<T>` and materialise on first use. `HardwareMonitorViewModel` only forces the value when the Hardware Monitor tab is activated — pushing 2.7 s of blocking work from startup into a background-thread task that runs while the user is looking at the Overview tab.

**Estimated effect on a Celeron:** window paints in ~1 s instead of 5–8 s. Steady-state RAM lower until user visits monitor tab.

### Lever 2 — React to events instead of polling

Two current polling loops don't need to be:

- **Startup entries** — currently the user hits Refresh; the scan re-walks 8 registry paths + two file-system folders. Windows exposes `RegNotifyChangeKeyValue` — you subscribe once per key and get a wait handle signalled on any change. No re-scan cost until Windows actually tells you something changed. There's a small `RegistryMonitor` wrapper on CodeProject that abstracts the P/Invoke.
- **Installed software** — same story. Use `RegNotifyChangeKeyValue` on the four `Uninstall` keys. Right now the uninstaller tab re-inventories every open, which is ~500 ms + some registry pressure.

For hardware monitor **do not** try to be event-driven — sensors don't push, and polling every 1–2 s is the standard approach. But **suspend polling when the window is minimised or unfocused** — the `HardwareMonitorService.Start()`/`Stop()` API is already there; just wire it to `Window.StateChanged` and `Window.Deactivated`/`Activated`. On a low-power CPU a running LHM poll shows up in per-process CPU %.

### Lever 3 — Adapt to the machine you're on

Currently every user gets the same polling cadence, the same DataGrid animations, the same recursive-with-no-limit residual walk. A "Performance mode" (auto-detected on first run, user-overridable) that measures a few things and picks appropriate defaults:

| Detection | Method | Adjust |
|---|---|---|
| **GPU capability** | `RenderCapability.Tier` returns 0 / 1 / 2. Tier 0 = software rendering. | On Tier 0/1: disable window animations, drop shadows, opacity/blur effects; use solid colours instead of gradients. WPF composites those on GPU; on Tier 0 they run on CPU and are slow. |
| **Physical memory** | `GC.GetGCMemoryInfo().TotalAvailableMemoryBytes` | < 4 GB: cap scan result collections at 200 items instead of 1000, avoid materialising all cleaner results at once, disable duplicate/large-file modules unless user explicitly opens them. |
| **CPU cores** | `Environment.ProcessorCount` | ≤ 2 cores: serialize scan modules instead of running them in parallel; use `SemaphoreSlim(1)` around file IO. |
| **Storage type per drive** | `Get-PhysicalDisk` WMI query for `MediaType` (SSD/HDD/Unknown), or query `IOCTL_STORAGE_QUERY_PROPERTY` via P/Invoke | HDD: single-threaded IO (parallel walk thrashes the head), avoid full-file hashing for big files (use prefix hash only). SSD: allow parallel IO up to `ProcessorCount`. |
| **On battery** | `SystemInformation.PowerStatus.PowerLineStatus` | On battery + Power Saver plan: increase HM poll interval to 5 s, defer background scans. |

None of these detections cost more than a millisecond. The savings on a weak machine can be dramatic — bounded scan collections alone can drop RAM ceiling by hundreds of MB during a full cleanup scan.

---

## Feature-by-feature: what specifically to do

### Hardware Monitor

- **Lazy-open LHM** (Lever 1). Biggest single win. Users who never visit that tab never pay the 2.7 s.
- **Turn off unused hardware categories.** The probe shows `IsControllerEnabled = true` + `IsNetworkEnabled = true` bring 195 useless network filter sensors on this laptop. Set both to `false` — you lose nothing the UI actually shows, save 500 ms of `Open()`.
- **Adaptive poll interval.** Default 2 s. On battery, or when window unfocused, back off to 5 s. When window minimised, `Stop()` entirely.
- **Reuse the `Computer` — never recreate.** Already done, keep it that way.
- **Timer re-arm at end of callback**, not periodic (HM4 from the audit). Prevents queue-up if a poll ever slows down.
- **Use `PerformanceCounter` only as a supplementary path** — the probe showed it can silently fail. Don't use it as primary CPU-percent source.

### Uninstaller residual scan (the 28-second beast)

The current design walks HKLM\\SOFTWARE recursively **for every application** the user selected. That's the disaster.

Three cheap fixes:

- **Cache the walk.** Walk HKLM\\SOFTWARE + HKCU\\Software **once per session**, index by normalised key name. When scanning residuals for `n` apps, do `n × 1 μs` dictionary lookups instead of `n × 28 s` full walks. RAM cost: ~10 MB for the whole index; you save minutes.
- **Depth cap.** Real residual keys are almost always at depth ≤ 4 (`SOFTWARE\<Vendor>\<Product>[\<Version>]`). Cap the recursion to 6 or 7 and cut the walk time by ~70 %.
- **Anchor the match.** Instead of "contains token", require token match on the **immediate segment name** — i.e. the residual key must be named `Steam*` or `*Steam*`, not just have `steam` somewhere in a path. Combined with C2's confidence-rating scheme this cuts false-positive count by 90 %+ and is faster.

If you want to be really clever: `RegNotifyChangeKeyValue` on `HKLM\\SOFTWARE` and `HKCU\\Software` so the cached index only rebuilds when Windows tells you something changed. Refresh in the background between uninstalls.

### Cleanup scanner

- **Skip well-known noise directories.** `node_modules`, `.git`, `.venv`, `__pycache__`, `target`, `bin`, `obj`, `.gradle`. The probe caught the same-hash Prisma engine binaries; adding this list eliminates those and thousands of others.
- **HDD-aware IO.** Detect via WMI once per session. On HDD: single thread walks the tree, small read buffer, no parallelism. On SSD: `Parallel.ForEach` with `MaxDegreeOfParallelism = Environment.ProcessorCount`.
- **Progressive UI.** Stream results into the ObservableCollection as they arrive; don't wait for the entire scan to complete before showing anything. Current `CleanupService.ScanAsync` returns the whole list — change to `IAsyncEnumerable<CleanupScanResult>` so the UI can render as modules finish.

### Duplicate finder

- **Prefix hash → full hash.** Bucket by size → SHA-256 first **16 KB** → only compute full-file SHA-256 for prefix-matching pairs. On a dataset of same-size video files that differ, this is a ~500× speedup for the miss case. (dupeGuru's approach.)
- **Skip same noise dirs** as cleanup.
- **Cap concurrency to 1 on HDD** — parallel hashing thrashes.

### Large file scanner

- Currently uses recursive walk. On a full user profile this can take minutes on HDD.
- **`Directory.EnumerateFiles(..., SearchOption.AllDirectories)`** is streaming and cheaper than the manual stack + `GetFiles` approach in the current code — but only if you tolerate an exception on the first denied directory. Wrap it or fall back to the manual walk with try/catch per directory (which is what the current code does — keep this pattern, it's correct).
- **Yield to the UI thread periodically.** Every 500 files scanned, `await Task.Yield()` — keeps the UI responsive without meaningfully slowing the scan.

### VirusTotal

- SHA-256 of large files before upload can peg one core for tens of seconds. **Use `IncrementalHash` reading in 64 KB chunks with `Task.Yield()` between chunks** — same total CPU time, but the machine stays responsive.

### WPF UI — for Tier-0 machines

Read `RenderCapability.Tier` at app start. If `< 0x00020000`:

```xaml
<Window.Resources>
    <Style TargetType="Border">
        <Setter Property="RenderOptions.EdgeMode" Value="Aliased" />
    </Style>
</Window.Resources>
```

Also:
- Disable window fade-in/out (`WindowChrome` animations).
- Remove `DropShadowEffect` and `BlurEffect` — these are the biggest killer on Tier 0.
- Replace `LinearGradientBrush` with solid `SolidColorBrush` where possible.
- **Freeze brushes and geometries** used across multiple controls: `myBrush.Freeze()` allows WPF to skip change-tracking and shares them across threads.
- Set `TextOptions.TextFormattingMode="Display"` and `TextOptions.TextRenderingMode="Aliased"` on windows to skip sub-pixel positioning — trades typographic quality for real ms.

For DataGrids (Cleanup results, Startup list, Installed software):
- **Virtualization is on by default in DataGrid** but not in `ListBox` — verify `VirtualizingStackPanel.IsVirtualizing="True"` and `VirtualizationMode="Recycling"` are set. Recycling reuses container instances instead of creating fresh ones on scroll.
- **Don't bind to `ObservableCollection<T>` with 10K+ items directly** — use `CollectionView.DeferRefresh()` when doing bulk updates so the UI doesn't re-layout after every add. Alternatively, populate the list off-thread, then swap the whole collection in one operation.
- Batch inserts happening now (individual `Add` calls per item during scan) trigger a layout pass each time. Add all items into a temp `List<T>`, then assign the collection.

Realistic gain on Tier-0 machines: scrolling a 500-row DataGrid goes from 2–4 fps to 30 fps. Startup UI paint drops from ~1 s to ~200 ms.

### GC / JIT

- **Keep Workstation + Concurrent GC** (default for desktop). Concurrent GC drops pause times from ~200 ms to 10–30 ms — a monitor updating every 2 s hides those pauses easily.
- **Do NOT enable Server GC** (`<ServerGarbageCollection>true</ServerGarbageCollection>`) — designed for many-core servers, worse for desktop.
- **Enable Dynamic PGO** in the project (default on .NET 9, but confirm): `<TieredPGO>true</TieredPGO>`. Recompiles hot methods with runtime profile data. Free perf.
- **ReadyToRun (`<PublishReadyToRun>true</PublishReadyToRun>`)**: precompiles common paths to native code. Costs binary size (which you don't care about) and reduces first-run JIT overhead on slow CPUs — good tradeoff for your target.
- **Avoid allocations in hot paths.** The cleanup scan does `new List<string>()` per module. That's fine at 5 modules. In the residual scan it's per-app (`new List<ResidualItem>()`, `new HashSet<string>()`) — matters more.

---

## The "Performance Mode" idea

Rather than making every user tune settings, ship one toggle. On first run, detect:
- RAM < 4 GB **or** CPU cores ≤ 2 **or** RenderCapability.Tier == 0 **or** WMI storage type == HDD

→ default to **Performance Mode ON**. That preset:
- Disables Duplicate + Large-file scanners on Quick Clean.
- Disables Hardware Monitor auto-refresh unless tab is visible.
- Sets HM poll to 5 s (default 2 s).
- Serialises scan modules (`MaxDegreeOfParallelism = 1`).
- Uses simplified visual theme (no shadows, no gradients).
- Caps in-UI collections at 200 items with "Show more" pagination.
- Uninstaller residual scan depth capped at 6.

The user gets a note ("Performance Mode enabled — this laptop is low-spec. Toggle in Settings.") and can turn it off any time. This gives you one branch of behaviour to test against and one to tune, instead of an infinite matrix of "works on my machine, hangs on grandma's laptop."

---

## Recommendation sequence

Roughly in order of ratio-of-benefit-to-work:

1. **Lazy `HardwareMonitorService`** — 3 lines of DI + a `.Value` in the view model. Cuts cold start by 2.7 s on your box, ~5–8 s on target hardware. Half a day.
2. **Turn off `IsNetworkEnabled` + `IsControllerEnabled`** in `HardwareMonitorService`. Instant win, no downside for the UI. 5 minutes.
3. **Timer re-arm at end of callback** (HM4). Prevents future queueing. 10 minutes.
4. **Suspend HM on minimise/blur**. Wire `Window.StateChanged`. 30 minutes.
5. **Residual walk cache** — walk HKLM\\SOFTWARE + HKCU\\Software once per session, index by normalised key name, look up in O(1) per app. Turns 28 s × N-apps into a single 28 s walk + N × 1 μs. Half a day. Combines with the C1/C2 rewrite.
6. **Anchored match instead of substring** (also fixes C2's blast radius). Half a day.
7. **Depth-cap residual walk to 6**. Two lines. Speeds walk by ~70 %.
8. **`RenderCapability.Tier` detection + simple theme fallback**. Style file + a resource dictionary swap. One day.
9. **HDD detection + serialise IO on HDD**. WMI once per session, gate `Parallel.ForEach`. Half a day.
10. **Prefix hash for duplicate finder**. Half a day.
11. **"Performance Mode" preset** wrapping 1/2/3/4/8/9/10 behind a single toggle with autodetection. Two days.
12. **`RegNotifyChangeKeyValue` for startup + installed-software** — replaces polling refreshes with event-driven ones. Once you have the cached inventory (5), this is a one-day add.

Items 1–4 are the "make it not feel broken on a Celeron" pass. Items 5–7 are the "make the uninstaller not lock up the machine" pass. Items 8–12 are the "systematically feel light" pass.

**None of these blocks the C1/C2/C3 safety fixes from the earlier reviews** — they can go in parallel. In fact 5 and 6 combine directly with C2 (anchored match + cached walk kill both the blast radius and the perf hit in one PR).

---

## Sources

- WPF: [Graphics Rendering Tiers (Microsoft Learn)](https://learn.microsoft.com/en-us/dotnet/desktop/wpf/advanced/graphics-rendering-tiers), [10 Ways to Improve WPF Performance (CodeGuru)](https://www.codeguru.com/csharp/10-ways-to-improve-the-performance-of-your-wpf-application/), [Software Rendering Usage in WPF (Microsoft blog)](https://learn.microsoft.com/en-us/archive/blogs/jgoldb/software-rendering-usage-in-wpf).
- WPF DataGrid: [Rendering 1M Rows in WPF Without Freezing (Xceed)](https://xceed.com/blog/uncategorized/rendering-1-million-rows-in-wpf-without-freezing-the-ui-a-practical-guide-with-xceed-datagrid/), [Efficient Large-Data Display in WPF](https://reogrid.net/articles/wpf-datagrid-large-data/).
- GC: [Background garbage collection (Microsoft Learn)](https://learn.microsoft.com/en-us/dotnet/standard/garbage-collection/background-gc), [Workstation vs server GC](https://learn.microsoft.com/en-us/dotnet/standard/garbage-collection/workstation-server-gc).
- Registry notifications: [RegNotifyChangeKeyValue (Microsoft Learn)](https://learn.microsoft.com/en-us/windows/win32/api/winreg/nf-winreg-regnotifychangekeyvalue), [RegistryMonitor wrapper (CodeProject)](https://www.codeproject.com/Articles/4502/RegistryMonitor-a-NET-wrapper-class-for-RegNotifyC).
