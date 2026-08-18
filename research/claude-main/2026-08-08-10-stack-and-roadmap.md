# SystemCleaner — Stack Today, Stack Target, and the Sequenced Fix Plan

Everything below is either measured directly from the csproj files or grounded in the prior research briefs. This is the single source of truth for "what we have, what we're changing it to, in what order."

---

## Part 1 — Current stack (measured)

### Runtime
| Layer | Version | Verified |
|---|---|---|
| .NET SDK (project pin) | 9.0.304 (rollForward=latestPatch) | `global.json` |
| Local SDK | 9.0.316 | installed after this session |
| Target framework | `net9.0-windows` (App) / `net9.0` (Core, Tests) | csproj |
| UI framework | WPF | `<UseWPF>true</UseWPF>` |
| Language | C# 13 (implicit via SDK) | csproj |
| Nullable | enabled everywhere | csproj |

### NuGet packages
| Package | Current | Status |
|---|---|---|
| Microsoft.Extensions.DependencyInjection | **8.0.0** | ⚠️ Behind runtime (should be 9.x) |
| LibreHardwareMonitorLib | **0.9.4** | ⚠️ Behind stable (0.9.6 released Feb 2026) |
| System.Management (App + Core) | 10.0.0 | Current |
| System.ServiceProcess.ServiceController (Core) | 10.0.0 | Current |
| SharpDX | **4.2.0** | 🛑 **Archived since March 2019** |
| SharpDX.DXGI | **4.2.0** | 🛑 **Archived since March 2019** |
| coverlet.collector (Tests) | 6.0.4 | Current |
| Microsoft.NET.Test.Sdk (Tests) | 18.0.1 | Current |
| xunit (Tests) | 2.9.3 | Current |
| xunit.runner.visualstudio (Tests) | 3.1.5 | Current |

### Transitive dead weight (measured in publish output)
| File | Size | Origin | Necessary? |
|---|---:|---|---|
| `libMonoPosixHelper.dll` | 1.5 MB | HidSharp (LHM dep) | ❌ Linux only |
| `Mono.Posix.NETStandard.dll` | 186 KB | HidSharp | ❌ Linux only |
| `MonoPosixHelper.dll` | 86 KB | HidSharp | ❌ Linux only |
| `System.Diagnostics.EventLog.Messages.dll` | 783 KB | transitive | ⚠️ Only if writing EventLog |
| `System.IO.Ports.dll` | 88 KB | HidSharp | ❌ Not used |
| `System.CodeDom.dll` | 181 KB | System.Management | ⚠️ Marginal |

Total dead weight: **~2.8 MB** shipped every release, zero benefit on Windows.

### Build & CI
| Component | Status |
|---|---|
| GitHub Actions CI workflow | ✅ Working, uses `actions/checkout@v5`, `actions/setup-dotnet@v5` |
| GitHub Actions **release** workflow | 🛑 Uses **archived** `actions/create-release@v1` + `actions/upload-release-asset@v1` |
| `dotnet format --verify-no-changes` | ✅ Enforced in CI |
| `dotnet list package --vulnerable` | ⚠️ Runs but doesn't fail on findings (no `--include-transitive`) |
| Dependabot | ✅ Weekly, up to 5 PRs open |
| Code signing | ❌ None |
| ReadyToRun / TieredPGO | ❌ Not enabled |
| Analyzers / warnings-as-errors | ❌ Not enabled |

### Testing
- **1 test file** (`UnitTest1.cs`), **1 test method** — covers only the happy path for `DirectoryCleanupModule`
- **0 tests** for `FileSystemHelper.IsRestrictedPath`, reparse handling, `RegistryCleanupHandler.TryParseRegistryTarget`, residual token matching, StartupDiscovery approval-state binary payload, VirusTotal parser, DPAPI settings

### App architecture
- 3 projects: `SystemCleaner.App` (WPF), `SystemCleaner.Core` (business logic), `SystemCleaner.Tests`
- DI via `Microsoft.Extensions.DependencyInjection` in `App.OnStartup`
- Manual MVVM in ViewModels (no source-generated `INotifyPropertyChanged`)
- All services registered as singletons, **constructed eagerly at startup**
- `MainWindow.old.xaml.bak` (41 KB stale backup) checked into repo

---

## Part 2 — Target stack (what we update to)

### Package updates (immediate, ratio-of-value-to-risk high)

| Package | From | To | Why |
|---|---|---|---|
| Microsoft.Extensions.DependencyInjection | 8.0.0 | **9.0.x** | Matches TFM. Trivial bump. |
| LibreHardwareMonitorLib | 0.9.4 | **0.9.6** | Newer Ryzen/Meteor Lake support, WinRing0 signing bump reduces AV false positives. |
| SharpDX + SharpDX.DXGI | 4.2.0 (archived) | **Vortice.DXGI 3.x** (active) | SharpDX dead since 2019. Vortice gives us DXGI 1.4+ → `IDXGIAdapter3::QueryVideoMemoryInfo` which fixes the integrated-GPU 0-VRAM bug. |
| HidSharp transitive Mono deps | shipped | **excluded** via `<ExcludeAssets>native</ExcludeAssets>` on HidSharp reference | -1.8 MB per release for zero downside on Windows. |

### Packages to add (small, targeted)

| Package | Why | Where |
|---|---|---|
| **CommunityToolkit.Mvvm 8.x** | Source-generated `[ObservableProperty]` and `[RelayCommand]` — cuts ViewModel boilerplate ~40%, reduces reflection cost, AOT-friendly for future | App |
| **System.Diagnostics.PerformanceCounter 9.x** | Fallback path for basic CPU/RAM % when LHM is unavailable or fails (probe showed this can fail on corrupted machines) | App |
| **Microsoft.Extensions.Logging** + **Microsoft.Extensions.Logging.EventLog** | Structured logging replaces `DiagnosticLogger`. Emits to Event Log for enterprise SIEM ingest. | Both |
| **System.Text.Json source generators** (`JsonSerializerContext`) | Already in BCL; add `[JsonSerializable]` context classes for VirusTotal DTOs, AppSettings, future cleaner-rules JSON. Cuts JSON reflection overhead + AOT-ready. | App, Core |
| **Microsoft.Extensions.Configuration.Json** | For future ADMX-registry-backed policy file (`policy.json`) and JSON-defined cleaner rules | Core |
| **Windows App SDK** or **PInvoke.User32/Advapi32** for `RegNotifyChangeKeyValue` wrapper | Registry change notifications (Startup + Uninstall list) — event-driven instead of polling | Core |

### Packages we deliberately don't add

| Not adding | Why |
|---|---|
| Serilog / NLog | `Microsoft.Extensions.Logging` is enough. Fewer deps = less audit surface. |
| ReactiveUI | Overkill. `CommunityToolkit.Mvvm` covers our patterns. |
| MediatR / Autofac | We have DI already; a mediator adds indirection without value at our size. |
| Prism / MVVM Light | Legacy; `CommunityToolkit.Mvvm` is the modern MS-endorsed replacement. |
| A UI-control library (Syncfusion, Telerik, MahApps) | Adds tens of MB + license story. Stick with vanilla WPF + our own styles. |
| Avalonia | Different framework migration — Approach C from earlier. Not this pass. |
| Vortice.Direct3D12 or larger DirectX | We only need DXGI for adapter memory query. Rest is unnecessary. |

### Build & CI updates

| Change | From | To |
|---|---|---|
| Release workflow | `actions/create-release@v1` + `actions/upload-release-asset@v1` (archived) | `softprops/action-gh-release@v2` (one step replaces both) |
| Vulnerable-package check | prints only | fail on any finding, add `--include-transitive` |
| Code signing | none | **SignPath.io free program** for OSS projects, or Certum inexpensive individual OV cert. Sign both exe and MSI. |
| Publish flags | `PublishSingleFile` only | Add `<PublishReadyToRun>true</PublishReadyToRun>` — precompiles hot paths to native. Bigger binary but faster JIT on weak CPUs. |
| Dynamic PGO | default | `<TieredPGO>true</TieredPGO>` explicitly (already default on .NET 9 but be explicit) |
| Analyzers | none | `<AnalysisLevel>latest-recommended</AnalysisLevel>` + Roslynator if desired |
| Warnings as errors | off | `<TreatWarningsAsErrors>true</TreatWarningsAsErrors>` (post cleanup) |
| Tested-on matrix | none | Run CI on `windows-2019` + `windows-latest` (Win10-era + Win11-era images) |

### Architecture updates

| Change | Rationale |
|---|---|
| Split `Core` into feature-scoped assemblies | `SystemCleaner.Features.Cleanup`, `.Uninstall`, `.Startup`, `.HardwareMon` — each contributes DI registrations. Enables lazy loading and future plugin story. |
| `Lazy<T>` service factories in DI | HardwareMonitorService, UninstallerService, VirusTotalService — cold start drops by ~2.7 s. |
| Feature-flagged policy layer | `IPolicyProvider` reads `HKLM\Software\Policies\SystemCleaner` (populated by ADMX) — same binary for consumer and enterprise. |
| Delete `MainWindow.old.xaml.bak` | Stale 41 KB backup. |
| Move cleanup module catalog to JSON | `cleaners.json` shipped alongside exe. Adopt winapp2.ini import for community rules. |

---

## Part 3 — The sequenced fix plan

Seven waves, each shippable independently. Waves 1–3 are the "safety, correctness, small wins" foundation. Waves 4–5 are the runtime-lightweight arc. Waves 6–7 are the enterprise-ready arc. Feature-scope expansion (Autoruns-parity, etc.) is deliberately not in this list — it belongs after Wave 7 when the foundation is solid.

### Wave 1 — Zero-downside cleanup (~1 weekend)

Small, safe, ships immediately, unblocks everything else. Nothing here breaks or removes existing behaviour.

1. **Delete `MainWindow.old.xaml.bak`** (5 min)
2. **Update `Microsoft.Extensions.DependencyInjection` 8.0.0 → 9.0.x** (5 min)
3. **Update `LibreHardwareMonitorLib` 0.9.4 → 0.9.6** (5 min, run app once to verify sensors still enumerate)
4. **Replace `SharpDX + SharpDX.DXGI` with `Vortice.DXGI`** — rewrite `DxgiAdapterReader.cs` (~40 lines). Same output, unarchived dep.
5. **Exclude Mono.Posix native + managed from HidSharp reference** via `<ExcludeAssets>native</ExcludeAssets>` + explicit `<PackageReference ExcludeAssets>` — 2 lines of csproj
6. **Replace deprecated release workflow actions** — `softprops/action-gh-release@v2` in a single step
7. **Make vulnerable-package CI check fail on findings** — `dotnet list ... --vulnerable --include-transitive | grep -q "no vulnerable" || exit 1`
8. **Fix the empty `foreach (var issue in issues)` loop** in `UninstallerService.FindResidualItemsAsync` — either propagate issues or delete
9. **Timer re-arm at callback end** in `HardwareMonitorService` (HM4) — 5 lines
10. **Suspend HW monitor on window minimise/blur** — wire `Window.StateChanged` + `Activated/Deactivated`
11. **Winget manifest** — one PR to `microsoft/winget-pkgs`

**Ship as v1.0.1 or v1.1.0.** Note in CHANGELOG. No user-visible regression.

### Wave 2 — Test scaffolding (~1 weekend)

Before touching destructive code, build the safety net.

1. **Adopt `CommunityToolkit.Mvvm`** — refactor 2-3 ViewModels to `[ObservableProperty]` as a smoke test. Not the whole codebase.
2. **Add xUnit tests for**:
   - `FileSystemHelper.IsRestrictedPath` — every branch of the restricted-roots logic
   - `FileSystemHelper.NormalizePath` — env var expansion, 8.3, trailing separator, invalid input
   - `RegistryCleanupHandler.TryParseRegistryTarget` (this test will fail immediately — confirms C1)
   - `ResidualTokenBuilder.NormalizeForComparison` and `.Build` — verify token semantics
   - `StartupDiscoveryService` approval-state binary payload round-trip against an in-memory registry mock (or throwaway HKCU key under `HKCU\Software\SystemCleaner.Tests`)
3. **Snapshot tests for VirusTotal JSON parsing** — feed known VT response fixtures, assert parsed model matches expectations. Uses `System.Text.Json` source-gen.

**Coverage target:** at least 30 tests in Wave 2. Doesn't need coverage %; needs the specific tests that will fail if C1/C2/C3 rewrites regress.

**Ship as v1.2.0.** Tests-only release doesn't ship binaries but does earn the "we have tests now" reputation.

### Wave 3 — Safety rewrites (~2 weekends)

The dangerous ones. Rely on Wave 2's tests.

1. **C1 — Registry hive parser fix.** Store hive as `"HKEY_LOCAL_MACHINE"` / `"HKEY_CURRENT_USER"` explicitly. Reject unknown hive names in parser. Corresponding tests turn green.
2. **C2 — Residual match rewrite.** Anchored match (StartsWith / EndsWith on segment name, not substring on full path). **Add confidence tier** (`VeryGood` / `Good` / `Questionable`) to `ResidualItem`. UI presents items grouped by tier; `Questionable` requires an explicit "Aggressive mode" opt-in. Depth cap on registry walk at 6.
3. **C3 — Uninstall string parsing.** Use `CommandLineToArgvW` P/Invoke to tokenize `UninstallString` into `(fileName, argumentList)`. Never route through `cmd.exe`. Add fallback "Show raw command" button for cases where parsing fails so user can run manually.
4. **VirusTotal H1 fix.** Remove the `groups/{_apiKey}` URL fallback. Log a clear message if quota endpoint returns 404 — don't put keys in URLs.
5. **VirusTotal H2 fix.** Build `HttpRequestMessage` per call with `x-apikey` header instead of mutating `HttpClient.DefaultRequestHeaders`. Or, wrap `SetApiKey` in a lock. Prefer the per-request approach.
6. **Migrate DPAPI key eagerly** on legacy `settings.json` load — don't leave plaintext in place until next save.

**Ship as v1.3.0** with a real release note explaining the residual-match rewrite. This is the release where you position the "safe uninstaller" story.

### Wave 4 — Runtime lightweight (~1 weekend + measurement)

Wire the lightweight pieces from the runtime brief.

1. **Lazy DI for `HardwareMonitorService`, `UninstallerService`, `VirusTotalService`** — 3 lines each. Cold start drops from ~5-8 s to ~1 s on Celeron-class hardware.
2. **`IsNetworkEnabled = false`, `IsControllerEnabled = false`** in LHM Computer config — drops from 216 sensors to 21. Still keeps CPU/GPU/Memory.
3. **Cached HKLM\SOFTWARE + HKCU\Software walk** in `UninstallerService` — walk once per session, index by normalised segment name. 28 s × N-apps → 28 s + O(1) per app.
4. **RenderCapability.Tier detection at startup.** If Tier < 2: swap in a minimal theme (no `DropShadowEffect`, no `BlurEffect`, solid brushes instead of gradients, no window animations). Fallback theme is a separate resource dictionary.
5. **HDD detection via WMI `Get-PhysicalDisk MediaType`** once per session. On HDD: gate `Parallel.ForEach` to `MaxDegreeOfParallelism = 1` for scan operations.
6. **Prefix-hash for duplicate finder.** Bucket by size → SHA-256 first 16 KB → full-file SHA-256 only for prefix matches. ~500× speedup on datasets with same-size distinct files.
7. **`RenderCapability.Tier`-aware DataGrid virtualization** — verify `VirtualizingStackPanel.VirtualizationMode="Recycling"` on all grids.

**Ship as v1.4.0.** Test on an actual old laptop (or spin up a low-spec VM). Publish measured before/after numbers in the release notes. This is where the "runs on weak hardware" positioning becomes true.

### Wave 5 — Feature module architecture (~1-2 weekends)

Prerequisite for enterprise policy + future feature growth. Refactor, not new features.

1. **Split `SystemCleaner.Core` into feature-scoped projects**:
   - `SystemCleaner.Core.Abstractions` (interfaces, models — shared)
   - `SystemCleaner.Features.Cleanup`
   - `SystemCleaner.Features.Uninstall`
   - `SystemCleaner.Features.Startup`
   - `SystemCleaner.Features.HardwareMonitor` (uses LHM + Vortice)
   - `SystemCleaner.Features.VirusTotal` (moves out of App)
2. **Introduce `IFeatureModule` contract** with `Register(IServiceCollection services)`. `App.OnStartup` iterates modules.
3. **JSON-defined cleaners** — move `CleanupModuleCatalog.cs` to `cleaners.json` under a schema. Load at startup. Support winapp2.ini import as compat layer.
4. **`RegNotifyChangeKeyValue` wrapper class** in `Core.Abstractions` with proper handle lifecycle. Use for Startup + Installed Software refresh.
5. **Structured logging via `Microsoft.Extensions.Logging`** replaces `DiagnosticLogger`. Sinks: file (existing behaviour) + Windows Event Log (new).
6. **`IPolicyProvider`** reads `HKLM\Software\Policies\SystemCleaner` via `RegistryKey`. If no policy present → default settings. If policy present → lock UI on policy-controlled settings.

**Ship as v1.5.0.** No user-facing changes; internal architecture.

### Wave 6 — Enterprise-ready endpoint (Phase 0, ~3-4 weekends)

Now the app is *deployable*.

1. **MSI installer** built with WiX Toolset 4. Silent install support.
2. **MSIX package** for Intune deployment + Microsoft Store for Business submission.
3. **ADMX + ADML templates**. Policy keys: enabled features, denied cleanup paths, VT API key (encrypted), monitor tab visibility, audit-log location.
4. **`SystemCleaner.exe --config policy.json --headless`** mode. Reads policy, runs operations, emits audit log, exits with proper code.
5. **Structured JSON audit log** — one line per event, RFC 5424 severity, ISO 8601 timestamps, correlation IDs. Splunk/Sentinel/ELK ingest natively.
6. **Windows Event Log integration** under `Applications and Services Logs\SystemCleaner`.
7. **Code signing** — SignPath.io free program setup, or Certum OV cert. Sign exe, MSI, MSIX. Update release workflow.
8. **LTS badge on the release** — commit to 24-month backport commitment.
9. **Tested-on matrix in README** — Windows 10 21H2/22H2, Windows 11 22H2/23H2/24H2, Windows Server 2019/2022. Green-badge each in CI.
10. **SOC 2 / CIS-alignment documentation** — one markdown file mapping our audit log to CIS Control 8 (audit log management) and CIS Control 6 (access control).

**Ship as v2.0.0 LTS.** This is the enterprise-friendly release. Post it on r/sysadmin, r/msp. Watch for pilot deployments.

### Wave 7 — What comes after (deliberate defer)

**Wait 3-6 months** after v2.0.0 LTS before starting more. Track Winget installs, GitHub download counts, watch for enterprise-user issues.

If Wave 6 gets traction, start on:
- **Phase 1 server component** (SystemCleaner.Server, Blazor Server dashboard, mTLS WebSocket telemetry, AD auth) — 2-4 months of focused work.
- **Autoruns-scale startup enumeration** (add Scheduled Tasks + Auto Services + shell extensions with signature verification).
- **BCU-style bulk uninstall UX** with multi-select and per-item VT check.
- **winapp2.ini community rule expansion** — target ~1000+ cleaner rules.

If Wave 6 doesn't get traction, that's data. Reassess whether enterprise is the right bet, or double down on the consumer "runs on weak hardware" positioning without the fleet story.

---

## Part 4 — Version and release plan

| Version | Wave | Scope | Approximate delta |
|---|---|---|---|
| v1.0.1 | 1 | Zero-downside cleanup, Winget | Same features, less weight, no dead deps |
| v1.2.0 | 2 | Test scaffolding | Tests-only, sets up Wave 3 |
| v1.3.0 | 3 | Safety rewrites (C1/C2/C3, VT H1/H2) | **Behavioural change** — residual scan finds fewer items (safer); VT fixes |
| v1.4.0 | 4 | Runtime lightweight | Measurable perf win, especially on weak hardware |
| v1.5.0 | 5 | Feature module refactor | Internal only, no user-facing change |
| **v2.0.0 LTS** | 6 | Enterprise-ready endpoint | MSI/MSIX/ADMX/audit-log. Major release. |
| v2.x | 7 | Server, Autoruns-scale features, feature growth | After Wave 6 gets real users |

**Between v1.3.0 and v2.0.0**, all changes are additive or internal — no breaking user-facing changes. This means CI, existing users, existing deployments all continue to work.

**v2.0.0 is the natural place** to draw the "before/after" line for the entire arc.

---

## Part 5 — What we're deliberately NOT doing (and why)

Stated explicitly to prevent scope creep:

- **Avalonia + NativeAOT migration** — Approach C from the earlier brief. Only revisit after v2.0.0 has real users and there's evidence WPF's runtime cost is the ceiling. Massive effort otherwise.
- **Cross-platform** — Windows-native IS the differentiator vs Kudu. Refuse.
- **Own malware scanner** — VirusTotal integration is the moat. Don't try to build another signature engine.
- **Registry cleaner** — genuinely dangerous, Microsoft explicitly discourages, no real user value. Kudu ships this; SystemCleaner refuses.
- **Game Mode / GPU tweaker / driver installer** — bloat. Windows / GeForce Experience / AMD Adrenalin handle this.
- **Debloater as first-class feature** — nice-to-have someday. Not required for enterprise or consumer differentiation.
- **Open-core paywall split** — kills the trust story. Sustainability via sponsors + support + professional services only.
- **CCleaner-style "junk score" / "PC health score"** — pseudo-metric marketing pattern. Don't.

---

## Part 6 — What we do first (this week / next weekend)

I'd start with **Wave 1** — the "zero-downside cleanup" list. Every item is small, none is dangerous, and it clears the deck for the harder work. Concretely, next-session-doable:

1. Delete `MainWindow.old.xaml.bak`
2. Bump `Microsoft.Extensions.DependencyInjection` 8 → 9
3. Bump `LibreHardwareMonitorLib` 0.9.4 → 0.9.6, run the app once to verify sensors still enumerate
4. Replace SharpDX with Vortice.DXGI — rewrite `DxgiAdapterReader.cs`
5. Exclude Mono POSIX transitive assets from HidSharp
6. Fix release workflow — one step with `softprops/action-gh-release@v2`
7. Fix vulnerable-package CI check to fail on findings
8. Delete the dead `foreach (var issue in issues)` loop in `UninstallerService`
9. Timer re-arm at end of `HardwareMonitorService` callback (HM4)
10. Suspend HM on window minimise/blur

**All of that is one PR, maybe two.** It ships as v1.0.1 (or v1.1.0 — your call on semver flavor), gives you a real "cleanup + modernise" release note, and unblocks Wave 2's test scaffolding.

**Say the word and I'll start on Wave 1.** If you want to spec Wave 3's safety rewrites first (the higher-value but higher-risk work), I'll switch into brainstorming mode on that specific piece and turn it into a proper spec + implementation plan through the plans skill.

Or — if you want a different sequence entirely, tell me which piece you want to tackle first and why. The plan above is optimised for "safest possible order given foundations we need"; your priorities may differ.
