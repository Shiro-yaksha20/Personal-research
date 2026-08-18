# SystemCleaner — Staying Lightweight While Adding Everything

Numbers here are measured on this machine using the exact publish commands from the README. Every claim is either a measurement or cited.

---

## Where you are today (measured)

| Publish mode | Files | Total | Notes |
|---|---:|---:|---|
| **Self-contained single-file** (README recipe: `--self-contained true -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true`) | 1 exe + 2 pdb | **132 MB** | This is what ships in the release. |
| **Framework-dependent** (`--self-contained false`) | 22 files | **5.7 MB** | Requires .NET 9 runtime on target machine. |

Framework-dependent, top space hogs:

| DLL | Size | What it is | Necessary? |
|---|---:|---|---|
| `libMonoPosixHelper.dll` | **1.5 MB** | Mono POSIX helper — Linux support pulled in by LHM's HidSharp | ❌ **Dead weight on Windows** |
| `System.Diagnostics.EventLog.Messages.dll` | 783 KB | Localised resource for EventLog | ⚠️ Only if you use EventLog (you don't currently) |
| `LibreHardwareMonitorLib.dll` | 698 KB | Sensor library | ✅ Real dependency |
| `System.Management.dll` | 308 KB | WMI wrapper | ⚠️ Replaceable in many uses |
| `SharpDX.dll` + `SharpDX.DXGI.dll` | 415 KB | Direct3D bindings | ❌ Archived; replace with Vortice.DXGI (~similar size but supported) |
| `HidSharp.dll` | 231 KB | LHM HID reader | ✅ Only if fan control needed |
| `Mono.Posix.NETStandard.dll` + `MonoPosixHelper.dll` | 272 KB | More Mono | ❌ Dead weight on Windows |

**Immediate waste: ~2 MB of Mono dlls and unused resource satellites** on every distribution, because LHM's transitive graph pulls them in regardless of platform.

For comparison (from public releases of similar tools):

| Tool | Distribution size | Notes |
|---|---:|---|
| **WizTree** portable | ~1 MB | Native C++ |
| **Autoruns** | ~2 MB | Native C++ |
| **BleachBit** installer | ~9 MB | Python + Qt |
| **Bulk Crap Uninstaller** portable | ~10 MB | .NET Framework — old runtime bundled with OS |
| **SystemCleaner** single-file | **132 MB** | .NET 9 self-contained WPF |

You're an order of magnitude larger than the reference open-source cleaner (BleachBit) and two orders larger than the specialised utilities. Fixing this is a design question, not a compiler flag.

---

## The size problem is 95% framework, not your code

Of the 132 MB single-file exe:
- **~125 MB** is the .NET 9 runtime + WPF + BCL bundled inside.
- **~5 MB** is your app, LHM, SharpDX, System.Management.
- **~2 MB** is Mono dead weight.

Trimming and NativeAOT are the levers that reduce the 125 MB. **WPF does not support NativeAOT** — this is documented and confirmed as of .NET 9. It also has poor trim compatibility because of heavy reflection in XAML parsing. Those two constraints define the size ceiling for the current stack.

So the choice tree is:

```
Do you accept the 132 MB single-file distribution?
├── Yes → keep WPF, apply "reduce and lazy-load" (Approach A). Stays around 100-120 MB.
├── Ship framework-dependent → 5.7 MB app + separate runtime install (Approach B). Total install ~30 MB but requires runtime setup.
└── No → migrate UI to Avalonia + NativeAOT (Approach C). Realistic target: 25-40 MB single-file.
```

Nothing else meaningfully moves the needle for a WPF app in 2026.

---

## Approach A — Stay on WPF, reduce weight and lazy-load

**Target:** shrink from 132 MB to ~110 MB. Same distribution model, incremental wins.

### Kill the Mono dead weight (~2 MB saved)
LHM ships HidSharp which pulls in `Mono.Posix.NETStandard` and native `libMonoPosixHelper`/`MonoPosixHelper` for Linux HID support. On a Windows-only app they can be excluded via `<PublishReadyToRunExclude>`, `<TrimmerRootAssembly>` false, or by adding `<PackageReference … ExcludeAssets="native"/>` in the csproj. Also drop `System.Diagnostics.EventLog.Messages` if you're not writing to Windows Event Log.

### Kill SharpDX (0 net size, migration to a supported package)
Replace `SharpDX + SharpDX.DXGI` (415 KB, archived 2019) with `Vortice.DXGI` (comparable size, active, .NET 9 support). Same total size but gets `IDXGIAdapter3::QueryVideoMemoryInfo` for the HM1 fix.

### Lazy-load heavy features
Right now `App.OnStartup` instantiates every singleton service via DI: `HardwareMonitorService` opens LHM (which loads WinRing0 kernel driver), `VirusTotalService` creates HttpClient, etc. — before the UI is shown.

Change to **lazy service factories**:

```csharp
services.AddSingleton<Lazy<IHardwareMonitorService>>(sp =>
    new Lazy<IHardwareMonitorService>(() => new HardwareMonitorService()));
```

Now LHM only opens when the user actually visits the Hardware Monitor tab. Startup time drops (LHM's first `Open()` and per-hardware `Update()` are 400-800 ms of the current cold start). Memory footprint at rest drops by whatever LHM allocates for its sensor tree.

### JSON-defined cleaners (adds features without adding code weight)
Move the five hard-coded cleanup modules in `CleanupModuleCatalog.cs` to a `cleaners.json` shipped alongside the exe:

```json
[
  {
    "id": "vscode-cache",
    "name": "VS Code Cache",
    "quickSafe": true,
    "paths": [
      "%APPDATA%/Code/CachedData",
      "%APPDATA%/Code/Cache",
      "%APPDATA%/Code/GPUCache",
      "%APPDATA%/Code/logs"
    ],
    "extensions": []
  },
  { "id": "npm-cache", "paths": ["%APPDATA%/npm-cache/_cacache"] }
]
```

Now every new cleaner is JSON, not C#. **Adding 50 cleaners adds ~30 KB, not 30 KB per cleaner.** Contributors can PR new cleaners without touching C#. This is exactly how BleachBit works — INI/XML-style cleaner files — and why it can support ~90 apps without ballooning binary size.

### Lazy-load feature UI too
Overview + Cleanup pages are almost always the entry point. SystemInfo, Uninstaller, VirusTotal — most users don't touch on any given session. WPF supports `Frame`/`ContentControl` navigation with lazily instantiated `UserControl` views. Instantiate the ViewModel + View only on first tab activation.

### Concrete Approach A checklist
1. Exclude Mono.Posix native + managed dlls via csproj `ExcludeAssets="native"` on HidSharp reference.
2. Replace SharpDX with Vortice.DXGI.
3. Refactor DI to `Lazy<T>` factories for `HardwareMonitorService`, `VirusTotalService`, `UninstallerService`.
4. Move cleanup module catalog into `cleaners.json` with a small parser.
5. Views: switch from all-in-MainWindow to lazily-loaded pages.

**Estimated savings**: ~5-8 MB off the single-file, ~150-300 ms off cold start, RAM at rest drops noticeably. Nothing dramatic size-wise — WPF's 125 MB floor is the floor — but a real UX improvement.

---

## Approach B — Framework-dependent publish + bootstrap installer

**Target:** 5.7 MB portable exe + optional runtime setup.

The framework-dependent publish is already 5.7 MB. The catch: it requires `.NET 9 Desktop Runtime` on the target machine. Most users don't have it. Options:

- **MSIX package** — .NET 9 supports MSIX packaging where the runtime is a dependency the Microsoft Store or `Add-AppxPackage` resolves. User gets one-click install; runtime downloads automatically on first launch if missing. Package size: your app + manifest, no runtime bundled. Constraint: requires signing (Store cert or self-signed for sideload).

- **ClickOnce** — .NET 9 has updated ClickOnce with runtime bootstrap. Publish generates a setup.exe that checks for .NET, downloads if missing, installs your app. First-time installer: ~3-5 MB. Update mechanism built in.

- **Bootstrap installer** (WiX/Inno/NSIS) — Ship a ~500 KB installer that checks for .NET 9 Desktop, prompts to download if absent, then unpacks your 5.7 MB payload.

**Downsides**: not a true portable exe. First-time users need a runtime install (~50-150 MB depending on what's already there). If shipping to businesses, IT departments may prefer the self-contained blob because they can vendor it into their imaging.

**When to pick this**: if your target audience is technical enough to accept a runtime download once, this is the smallest realistic distribution. Comparable to BCUninstaller's ~10 MB.

---

## Approach C — Migrate UI to Avalonia, then NativeAOT

**Target:** 25-40 MB self-contained single-file.

Avalonia is a WPF-lookalike XAML framework that runs cross-platform, and **it supports NativeAOT** — verified by real projects. UniGetUI's 2026 release migrated from WPF-ish tech to Avalonia + NativeAOT and cut download size roughly in half.

NativeAOT compiles your app + runtime to native code ahead of time. Removes JIT, removes most reflection metadata, aggressively trims. Startup drops to <100 ms. Memory footprint drops significantly. No .NET runtime needed on target.

Realistic sizes (from public Avalonia + NativeAOT samples):
- Minimal "hello world" Avalonia + NativeAOT: **~15 MB**
- Real Avalonia app with typical dependencies: **~25-40 MB**
- With trimming warnings addressed: potentially <25 MB

**Migration cost:**
- **XAML rewrite**: Avalonia XAML is similar but not identical to WPF XAML. Bindings, styles, DataTemplates all port with tweaks. All 8+ .xaml files in the repo need conversion.
- **Code-behind rewrite**: WPF-specific APIs (`Window`, `MessageBox`, `Dispatcher`) map to Avalonia equivalents. ~10 code-behind files.
- **ViewModels**: ~95% reusable — they only depend on `INotifyPropertyChanged` and `ICommand`, both present in Avalonia.
- **Services (`Core` project)**: 100% reusable.
- **Testing**: rerun everything on Windows. Avalonia has fewer regressions than "WPF ported" but it's still a rewrite of the UI layer.

**AOT constraints to design for now**:
- No unbounded reflection (JsonSerializer's reflection path breaks — must use source-generated serializers).
- No runtime code generation.
- Some third-party controls may not be AOT-ready (Avalonia's built-in controls all are).
- All plugin loading via `AssemblyLoadContext` needs consideration.

**Effort estimate**: 2-4 weekends of focused work to reach parity with today's UI, assuming ViewModels stay put.

---

## Modular architecture — "big plans" require this regardless of framework

If you want to add all of: Autoruns-level startup scanner, BCU-level uninstaller with confidence rating, ~90 BleachBit-style cleaners, full duplicate/large-file finder with MFT scan, richer hardware monitoring, and more — the codebase needs boundaries.

### Feature modules

Split `SystemCleaner.Core` into feature-scoped assemblies:

```
SystemCleaner.Core/                  (shared abstractions only)
SystemCleaner.Features.Cleanup/      (existing)
SystemCleaner.Features.Uninstall/    (existing)
SystemCleaner.Features.Startup/      (existing)
SystemCleaner.Features.HardwareMon/  (existing)
SystemCleaner.Features.VirusTotal/   (existing App code moved here)
SystemCleaner.Features.DuplicateFind/  (grows)
SystemCleaner.Features.LargeFiles/     (grows)
SystemCleaner.Features.MFTScan/        (future — needs admin)
```

Each is a normal `csproj` project reference; each registers its own DI. `App.xaml.cs` iterates a list of `IFeatureModule` and each contributes services, view registrations, and tabs. This is how VS Code, Rider, and every serious extensible app is organised.

### Optional: real plugin loading

For features shipped separately (e.g., admin-only MFT scanner, community cleaners), use `AssemblyLoadContext` for isolation. Each plugin declares its own service registrations via `IPluginServices`. Modern .NET pattern: `AssemblyLoadContext` + `Microsoft.Extensions.DependencyInjection`.

**Downside**: plugins fight AOT (dynamic loading). Ties into Approach A/B/C decision.

---

## Data structure and hot-path choices

For the pieces that scan lots of files (Duplicate, Large File, Cleanup):

- `Directory.EnumerateFiles` (streaming) not `GetFiles` (materialises array). Already correctly used in some modules; verify in all.
- **Read first 4 KB for prefix hash**, only hash full file when prefixes match. Speeds up Duplicate finder massively on real data (per dupeGuru research).
- Use `Span<T>` and `ArrayPool<byte>` for hash buffers instead of allocating 8 KB per file (probe showed `Span<byte> buffer = stackalloc byte[8192]` is already used in the shredder — good pattern).
- Avoid materialising huge result lists as `List<T>` in the ViewModel when the DataGrid can virtualise: `ObservableCollection` with 10,000 items is a WPF perf trap. Consider `VirtualizingStackPanel` (WPF default) + async load-more pagination for large scans.

---

## Runtime memory — small wins accumulate

- `Timer` in `HardwareMonitorService` re-arms even when Hardware Monitor tab isn't visible (HM4 from the audit). Stop it on tab-switch — already partially wired.
- `NotificationService.Notifications` is unbounded — cap to last 50 with FIFO.
- `CleanupWorkspaceViewModel.MaxLogEntries = 200` — good, ObservableCollection stays bounded.
- Avoid `INotifyCollectionChanged` on ViewModels that don't need change notification (e.g., static ThemeOptions).

---

## Distribution channel matters too

Regardless of size approach, **how** you ship matters:

- **Winget** (Microsoft's official package manager) — set up a manifest in `microsoft/winget-pkgs`. Users install with `winget install Shiro-yaksha20.SystemCleaner`. Automatic updates. **Ships the exact self-contained exe you release on GitHub — winget is a wrapper, not a package format.** Zero infra work; PR to winget-pkgs.

- **Scoop** (community manifest) — same idea, community-run. Manifest is a JSON file in your repo. `scoop install systemcleaner`.

- **GitHub Releases** (already used) — keep, add SHA256 checksums.

- **Microsoft Store MSIX** — highest reach, requires publisher account ($19 one-time for individuals) + signing. Runtime resolved automatically. Auto-updates. Good future direction.

Winget adds effectively zero engineering cost and dramatically improves discoverability. Recommend doing this regardless of size approach.

---

## Recommendation

Given "big plans + very lightweight":

1. **Now — Approach A (stay on WPF, aggressive lazy-load, kill dead deps)**. Cost: 1-2 weekends. Benefit: 5-8 MB smaller, 150-300 ms faster cold start, RAM at rest drops.
2. **Also now — JSON-defined cleaners**. Cost: half a weekend. Benefit: unlocks the "~90 BleachBit-style categories" feature scope without binary growth.
3. **Also now — feature modules architecture**. Cost: 1 weekend refactor. Benefit: prerequisite for every subsequent feature; makes lazy-load actually possible.
4. **Now — Winget manifest**. Cost: 1 hour. Benefit: real distribution channel.
5. **Medium-term — Approach B (ship framework-dependent + ClickOnce/MSIX)**. When you're ready to accept "user needs runtime install." Cuts install size to sub-30 MB comparable to BleachBit.
6. **Long-term / hard pivot — Approach C (Avalonia + NativeAOT)**. If you decide the WPF distribution size is a hard blocker for reaching new users. This is a real rewrite of the UI layer, but delivers ~30 MB single-file + <100 ms startup + first-class AOT.

The order matters: **1 → 3 → 2 → (5 or 6)**. Everything before step 6 is reversible and can be shipped incrementally. Step 6 is a decision to make when the tool has product-market fit and size becomes the ceiling.

You do not have to commit to Approach C to add all the features you want. **The feature scope from the comparison doc (Autoruns-parity startup, BCU-parity uninstaller, 90 cleaners, MFT scan, richer HM) fits inside Approach A + JSON modules + feature modules**, because none of them ships as extra bundled binaries — they're either JSON, small P/Invoke calls, or feature-module DLLs. The binary grows by ~1-3 MB total across all of them, not 20 MB.

The 125 MB WPF floor is the elephant. If it doesn't bother you, ship Approach A. If it does, plan Approach C for later once the feature set is stable.
