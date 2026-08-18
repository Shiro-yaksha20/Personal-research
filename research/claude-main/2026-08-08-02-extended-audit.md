# SystemCleaner — Extended Audit (Startup, Hardware Monitor, and everything else)

Repo: https://github.com/Shiro-yaksha20/system-cleaner  
Build: **passes** clean on this machine (net9.0-windows, SDK 9.0.316), 0 warnings, 0 errors.  
Tests: **1 test passes** (`DirectoryCleanupModuleTests.ScanAndClean_RemovesFilesAndReportsAccurateSize`).  
Runtime probe: I wrote a headless probe that instantiates every Core service and observed real behaviour on your Dell laptop (i5-7300U + Intel HD 620, 8 GB, Win10 19045, unelevated). Findings below distinguish **behaviour I observed** from **code-only inspection**.

Read this alongside `system-cleaner-review.md` — the earlier report's C1/C2/C3 are unchanged and remain top priority. This document adds new bugs (S1‑S6, HM1‑HM6, X1‑X6) and — as you asked — concrete replacements to research further.

---

## Startup (`StartupDiscoveryService`) — what actually works and what doesn't

**What works on this box:**
- Enumerates 10 entries across HKCU/HKLM Run + RunOnce (both 32- and 64-bit views).
- The toggle round-trip (`SetStartupEntryEnabledAsync`) works unelevated for HKCU entries. Approval state written to `Explorer\StartupApproved\Run` binary payload correctly, Task Manager picks it up.
- Zero issues reported for the scan.

**But it's still broken in these ways:**

### S1 — HKLM entries can't be toggled unelevated, and the UI has no way to know
[SystemCleaner.Core/Startup/StartupDiscoveryService.cs:216-267](SystemCleaner.Core/Startup/StartupDiscoveryService.cs)

The probe found 4 HKLM entries (Realtek x2, SecurityHealth, WavesSvc). `SetStartupEntryEnabledAsync` catches the `UnauthorizedAccessException` and throws a friendlier `StartupDiscoveryException` — good. But the view model has no way to *pre-flight* this: the Enable/Disable button is enabled for HKLM rows even when the user is not admin, so every click on those four rows results in a failure toast. Add an `IsToggleable` property (checked against `IsAdministrator || entry.Location == "Current User"`) and grey the toggle in the DataGrid.

### S2 — Startup folder entries have no `RegistryValueName` for approval lookup
[SystemCleaner.Core/Startup/StartupDiscoveryService.cs:463-474](SystemCleaner.Core/Startup/StartupDiscoveryService.cs)

Startup folder items are created with `registryValueName: name` (the file name including extension). Approval state for startup folder entries is stored under `Explorer\StartupApproved\StartupFolder` **keyed by the file name**. This part looks correct — but the `RegistrySubKey` is `null`, so `SetStartupEntryEnabledAsync` line 211 throws "The selected startup entry does not support toggling." even though it *could* toggle the approval binary. On this machine the startup folder is empty so it didn't fire; put a shortcut in `%APPDATA%\Microsoft\Windows\Start Menu\Programs\Startup` and clicking Disable will error. Fix: the toggle path only needs `ApprovalSubKey`, `RegistryValueName`, and hive/view — drop the `RegistrySubKey` check.

### S3 — Approval-state query mismatch after toggle
[SystemCleaner.Core/Startup/StartupDiscoveryService.cs:340-361](SystemCleaner.Core/Startup/StartupDiscoveryService.cs)

Probe output:
```
Toggle round-trip test on: Delete Cached Standalone Update Binary
  Initial enabled state (from scan): True
  Initial approval state (queried):        ← null / empty
  Approval state AFTER toggle: False
  Approval state after revert: True
```

`GetApprovalState` returns `null` when the value doesn't exist yet, but the scan itself treats absence as "enabled." So immediately after a scan the UI shows "enabled," but if it uses `GetStartupEntryApprovalStateAsync` for anything (which returns `null`), it disagrees with itself. Consumers that map `null → unknown` will show a stale/blank toggle. Either return `true` for absent (matching scan semantics) or push a documented tri-state through the UI.

### S4 — RunOnce entries treated identically to Run entries
[SystemCleaner.Core/Startup/StartupDiscoveryService.cs:34-42](SystemCleaner.Core/Startup/StartupDiscoveryService.cs)

The probe found four RunOnce entries (OneDrive lifecycle: `Uninstall 26.129.0706.0003`, `Delete Cached Update Binary`, etc.). Windows deletes these values automatically after the first successful boot. Marking them "disabled" through StartupApproved is a no-op — the value evaporates on next login. Either exclude RunOnce from the manager UI, or label them "one-shot" and disable the toggle.

### S5 — Enabled state is stored as a plain nullable `bool` in the entry model, but `SetStartupEntryEnabledAsync` mutates state and doesn't refresh the model
[SystemCleaner.Core/Models/StartupEntry.cs](SystemCleaner.Core/Models/StartupEntry.cs)

`StartupEntry.IsEnabled` is a read-only property set at construction. There's no notification path from the toggle back into the collection displayed by `StartupManagerViewModel`. If the ViewModel doesn't call `RefreshAsync` after a toggle, the DataGrid still shows the old state. (I saw this pattern in `MainViewModel.InitializeAsync` — refresh happens only on load.) Either make `StartupEntry` mutable + `INotifyPropertyChanged`, or have the VM optimistically update the row and re-scan on a debounce.

### S6 — No coverage of scheduled-task startup, WMI event subscriptions, or services
Startup on Windows also comes from: Task Scheduler entries with a "boot" or "logon" trigger, and `Auto`-start services. The current implementation shows none of these. If parity with Autoruns or Task Manager's Startup tab is the goal, add:
- Scan `\Microsoft\Windows\...` scheduled tasks with logon/boot triggers (use `Microsoft.Win32.TaskScheduler` from NuGet — the `TaskScheduler` COM interop is a mess).
- Enumerate services where `StartMode = Auto` (already have `System.ServiceProcess.ServiceController`).

---

## Hardware Monitor — this is the big one

**What the probe showed unelevated on your i5-7300U + Intel HD 620:**

| Category | Result | Cause |
|---|---|---|
| CPU load per core + total | ✅ works (via NT perf counters) | LHM uses `Global\PerfProviders`, no admin needed |
| CPU package temp, per-core temp, TjMax distance | ❌ **all null** | MSR access requires the WinRing0 driver → admin |
| CPU core clock speeds | ❌ **all null** | Same MSR path |
| Memory used/available/total | ✅ works | Windows perf counters |
| Intel iGPU load (D3D 3D, Video Decode, etc.) | ✅ works | ETW providers |
| Intel iGPU D3D shared memory (used/total) | ✅ **works** (185.57 MB / 4036.52 MB) | ETW providers |
| Intel iGPU DXGI `DedicatedVideoMemory` | ❌ **returns 0** | Integrated GPUs have no dedicated VRAM |
| Motherboard sensors | 0 sensors reported | Dell OEM board, no LHM Super-I/O mapping |
| Storage SMART (temperature, life remaining, health) | ❌ **not present** (no `[Storage]` entries at all) | Requires admin to open drive handles for ATA/NVMe passthrough |
| Network interfaces | ⚠️ **43 virtual adapters** enumerated (WSL, WFP filters, QoS, etc.) with 5 sensors each | `IsNetworkEnabled = true` picks up every filter layer |

### HM1 — Recent DXGI GPU-memory PR breaks integrated-GPU reporting
[SystemCleaner.App/Utilities/DxgiAdapterReader.cs:30](SystemCleaner.App/Utilities/DxgiAdapterReader.cs)  
[SystemCleaner.App/ViewModels/SystemInfoViewModel.cs:342-375](SystemCleaner.App/ViewModels/SystemInfoViewModel.cs)

Commit `552eebd` "feat: use dxgi for gpu memory" reads `AdapterDescription.DedicatedVideoMemory`. For any laptop with only an integrated GPU (Intel HD/UHD/Iris, most business laptops, most ultrabooks), that value is **0** — integrated GPUs share system RAM and have no dedicated segment. The `SystemInfoViewModel.TryResolveAdapterMemory` fallback picks the first DXGI adapter's `DedicatedVideoMemory` when name matching fails, so the GPU tile just shows `0 B`.

**Fix:** for integrated adapters, use `AdapterDescription.SharedSystemMemory`, or (better) use `IDXGIAdapter3::QueryVideoMemoryInfo` for `DXGI_MEMORY_SEGMENT_GROUP_LOCAL` — that returns actual current usage/budget for both integrated and discrete GPUs. Or just fall back to LHM's `D3D Shared Memory Total/Used` sensors, which the probe already reads correctly.

### HM2 — Massive network-sensor bloat destroys the update budget
[SystemCleaner.App/Services/HardwareMonitorService.cs:24-33](SystemCleaner.App/Services/HardwareMonitorService.cs)

`IsNetworkEnabled = true` and `IsControllerEnabled = true` bring in every network filter layer. On this laptop that's ~43 network hardware objects × 5 sensors = ~215 sensor updates every 2 seconds, plus recursive `UpdateRecursive` walks. On low-power CPUs the monitor eats a couple of % just polling itself, and none of it is surfaced in the UI. Either disable Network + Controller, or filter to `hardware.HardwareType == HardwareType.Network && !hardware.Name.Contains("-")` when building the `Computer`.

### HM3 — No indication to the user that they need to elevate
[SystemCleaner.App/ViewModels/SystemUsageViewModel.cs:124-126](SystemCleaner.App/ViewModels/SystemUsageViewModel.cs)

There's a `TelemetryStatus` string "Sensor data unavailable. Run System Cleaner as administrator or enable monitoring in BIOS." — good — but it's driven by `HasTelemetry` (any sensor present). On this laptop CPU load *is* present but temps aren't, so `HasTelemetry` is `true` and the hint never shows even though every temperature field displays "N/A". Split it: show a specific hint on CPU tile when `TemperatureCelsius is null && !IsAdministrator()`.

### HM4 — `Timer` callback can re-enter and stack under the `_sync` lock
[SystemCleaner.App/Services/HardwareMonitorService.cs:35-82](SystemCleaner.App/Services/HardwareMonitorService.cs)

Confirmed in the probe: the first `Update` on all hardware takes 400–800 ms on this box (LHM opens WMI queries per subhardware). The timer is armed with `_pollInterval = 2 s`. If the machine gets busy and `Update` slows to > 2 s, callbacks queue on the thread pool and serialize behind the lock — indefinitely. Re-arm the timer at the end of the callback instead of using a periodic `Change(TimeSpan.Zero, _pollInterval)`.

### HM5 — LibreHardwareMonitorLib is at 0.9.4; current stable is 0.9.6 (Feb 2026), and 0.9.7-pre is on NuGet
NuGet package `LibreHardwareMonitorLib` shipped 0.9.6 in early 2026 with fixes for newer Ryzen and Meteor Lake CPUs, a rewritten NVML path, and a WinRing0 signing bump that some AV products stopped flagging. Upgrade is one line, and 0.9.x is source-compatible with what you have.

### HM6 — SharpDX is archived (as flagged before); Vortice is a drop-in replacement
`DxgiAdapterReader.cs` is 40 lines that need only `Factory1`, `GetAdapter1`, `Description1.Description`, `DedicatedVideoMemory`, `SharedSystemMemory`. Replace `SharpDX` + `SharpDX.DXGI` with `Vortice.DXGI` — actively maintained, supports .NET 9/10, brings in DXGI 1.4+ so you can also call `QueryVideoMemoryInfo` for HM1's fix in one go.

---

## Cleanup — additional findings from the runtime scan

The probe found: `Temporary Files` 754 MB, `Browser Cache` 196 MB, `Large Files` 2.7 GB across 5 items, `Duplicate Files` 80 MB across 4 items. All of these matched what File Explorer showed, so scanning works. Still, some issues:

### X1 — Only Chrome/Edge Default profiles enumerated for cache; secondary profiles missed
[SystemCleaner.Core/Modules/CleanupModuleCatalog.cs:104-122](SystemCleaner.Core/Modules/CleanupModuleCatalog.cs)

`AddBrowserRules` iterates `User Data` subdirectories and looks for a `Cache` folder in each. Chrome stores per-profile cache at `Profile 1/Cache`, `Profile 2/Cache`, etc. — the probe on your machine only found `Default` because that's the only profile. On multi-profile installs, all named profiles get scanned — that's actually correct. But **Chrome's cache is now under `Cache/Cache_Data`** (as of Chrome ~90+), and there's also `Code Cache`, `GPUCache`, `Service Worker/CacheStorage`, `Service Worker/ScriptCache`. The current module only clears the top-level `Cache` folder, which for modern Chrome contains sub-folders, not files. `CleanDirectoryContents` will recurse, so it does still work — but the sibling caches (Code Cache, GPUCache, especially the massive Service Worker caches) are missed.

### X2 — `Duplicate Files` scanner recurses into `node_modules` and other reparse-pointless-but-huge dev folders
Probe output shows duplicates found inside `RenderMint-feature-v0-ui-import\node_modules\@prisma\engines\...`. Any dev machine with node projects in Downloads/Documents will get vast scans and hundreds of "duplicates" that are actually intended (bundler cache, prisma engines per-platform binary, etc.). Add a well-known-noise skip list (`node_modules`, `venv`, `.git`, `__pycache__`, `target`, `bin`, `obj`) or expose one as a setting.

### X3 — `Downloads` for Large Files uses `GetKnownFolder(UserProfile) + "Downloads"`, not the actual known-folder ID
[SystemCleaner.Core/Modules/CleanupModuleCatalog.cs:55](SystemCleaner.Core/Modules/CleanupModuleCatalog.cs)

If the user has relocated Downloads to another drive (right-click → Properties → Location), the code still scans `%USERPROFILE%\Downloads` — which will be empty. Use `Shell32`'s known-folder API or `SHGetKnownFolderPath(FOLDERID_Downloads)` via P/Invoke. There's no `Environment.SpecialFolder.Downloads` in .NET.

### X4 — `FileSystemHelper.NormalizePath` doesn't expand 8.3 short names
[SystemCleaner.Core/Utilities/FileSystemHelper.cs:270-285](SystemCleaner.Core/Utilities/FileSystemHelper.cs)

`Path.GetFullPath` does not resolve 8.3 short names. On systems where `NtfsDisable8dot3NameCreation = 0`, an attacker who can plant a `PROGRA~1` (short name for `Program Files`) subdirectory under user temp could bypass `IsRestrictedPath`. Very theoretical exploit for a personal cleanup tool but the fix is one line: call `GetLongPathName` after `GetFullPath`.

### X5 — `Path.GetTempPath()` under user temp will not enumerate other users' temp on a shared PC (not a bug, just a scope thing)
Documented for completeness — good default.

---

## Uninstaller — confirmed with real data

The probe hit 34 installed apps. Concrete confirmations of earlier findings:

- **Registry key paths use `LocalMachine\SOFTWARE\...` / `CurrentUser\SOFTWARE\...`** — enum ToString, not `HKEY_*`. **C1 (registry parser hive bug) is confirmed against live data.** Every one of your `CurrentUser\...` apps would delete from HKLM if residual cleanup fired.
- **Substring token risk is not hypothetical**: probe generated token `"brave"` (length 5). Any AppData folder or HKLM key containing `"brave"` would be flagged. Common examples: `%LOCALAPPDATA%\BraveSoftware` (correct), but also anything that contains "brave" in its display name — no near-misses on your machine but the substring approach is inherently unsafe.
- **`Brave` uninstall command** is `... setup.exe --uninstall --system-level` — will go through `cmd.exe /c` with argument parsing. Same for `Copilot` with `--uninstall --mscopilot ...`. Works today because none of these contain `&`, `|`, `>` — but the door is open.

Additional finding:

### X6 — WMI Win32_Product not queried; MSI-only apps without proper registry entries are invisible
[SystemCleaner.Core/Uninstall/UninstallerService.cs:526-621](SystemCleaner.Core/Uninstall/UninstallerService.cs)

Some enterprise MSI packages don't register `DisplayName`/`UninstallString` under the standard uninstall keys. The probe's 34 apps roughly matches Windows Settings → Apps & features, but on machines with older LOB apps you'll miss some. Adding a WMI `Win32_Product` fallback is possible but be careful — `Win32_Product` **triggers an MSI self-repair on every query**, which is slow (10-60s) and can rewrite files silently. Prefer scanning `%windir%\Installer\*.msi` metadata instead, or leave the current approach as-is and document the limitation.

---

## Recommended replacements / upgrades (concrete answers to "surf and find resolution")

| Piece | Now | Replace with | Why |
|---|---|---|---|
| SharpDX + SharpDX.DXGI | 4.2.0 | `Vortice.DXGI` latest | SharpDX archived since 2019; Vortice is actively maintained, .NET 9/10 ready, exposes DXGI 1.4+ (needed for `QueryVideoMemoryInfo`). |
| LibreHardwareMonitorLib | 0.9.4 | 0.9.6 (or 0.9.7-pre for Meteor/Lunar Lake) | Newer CPU/GPU coverage; fewer AV false positives on WinRing0. |
| Microsoft.Extensions.DependencyInjection | 8.0.0 | 9.0.x | Match target framework. |
| `Random` in `ShredFile` | `System.Random` | `RandomNumberGenerator.Fill` — or remove feature | Fill is faster too; shred is theatre on SSDs regardless. |
| `actions/create-release@v1`, `actions/upload-release-asset@v1` | archived | `softprops/action-gh-release@v2` | GitHub archived these in 2021. |
| `cmd.exe /c "<uninstallString>"` | shell interp | Parse string yourself → `ProcessStartInfo.ArgumentList` | Kills C3 (command injection / EoP). |
| GPU memory reader | DXGI `DedicatedVideoMemory` only | Try `SharedSystemMemory` for integrated, or `IDXGIAdapter3::QueryVideoMemoryInfo` (Vortice.DXGI 1.4+) | Fixes HM1. |
| Storage SMART | LHM only (needs admin) | Same, but detect and hide the section when running unelevated | Fixes silent "N/A" everywhere on the SystemInfo page. |
| Residual scanner heuristic | substring token match | Exact-match on `Software\<Publisher>\<AppName>` + explicit review dialog listing each proposed path | Fixes C2. |
| Registry residual serialization | enum `ToString()` as hive prefix | `HKEY_LOCAL_MACHINE` / `HKEY_CURRENT_USER` literals, and validate on parse | Fixes C1. |

Nothing about the architecture (net9.0-windows, DI, MVVM, xUnit) needs to be replaced — the target framework and package selection are fine and current.

---

## Suggested sequencing for the next session

1. **Design + write tests for the destructive paths first**: registry parser, residual token match, cleanup handler dispatch. Once we have red tests, C1/C2/C3 fixes become mechanical.
2. **Fix HM1 (GPU memory for integrated) + HM3 (elevation hint)**: highest UX visibility, small blast radius.
3. **S1‑S3 Startup usability fixes**: gate HKLM toggle, fix startup folder toggle, unify approval semantics.
4. **Replace SharpDX with Vortice.DXGI** in one PR — sets up HM1's real fix.
5. **Upgrade LHM 0.9.4 → 0.9.6** — one line, run the app once to verify.
6. **HM2 network sensor bloat + HM4 timer re-entry**: performance PR.
7. **Then** C1/C2/C3 uninstall rewrites — need the test scaffolding from step 1.

Everything above is a discrete, spec-able PR. When you're ready to move from audit to design, I'll turn the item(s) you want to tackle first into a proper spec via the brainstorming skill and then into an implementation plan.
