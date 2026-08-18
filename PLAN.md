# SystemCleaner — Canonical Plan

Single source of truth. Supersedes the individual briefs. References them for detail where anyone wants to dig deeper.

**Table of Contents**

1. [Executive summary](#1-executive-summary)
2. [What SystemCleaner is (positioning)](#2-what-systemcleaner-is-positioning)
3. [Current state (measured)](#3-current-state-measured)
4. [Known bugs — the fix backlog](#4-known-bugs--the-fix-backlog)
5. [Market position (competitor landscape)](#5-market-position-competitor-landscape)
6. [Voice of user — what shapes design](#6-voice-of-user--what-shapes-design)
7. [Design principles](#7-design-principles)
8. [What we're deliberately NOT doing](#8-what-were-deliberately-not-doing)
9. [Target stack](#9-target-stack)
10. [Target architecture](#10-target-architecture)
11. [The seven-wave roadmap](#11-the-seven-wave-roadmap)
12. [Enterprise angle (Wave 6-7 detail)](#12-enterprise-angle-wave-6-7-detail)
13. [Honest downsides (of the plan itself)](#13-honest-downsides-of-the-plan-itself)
14. [Where we are, what's next](#14-where-we-are-whats-next)
15. [Appendix — sources + reference briefs](#15-appendix--sources--reference-briefs)

---

## 1. Executive summary

SystemCleaner today is a .NET 9 WPF Windows utility bundling cleanup, uninstaller, startup manager, hardware monitor, and VirusTotal integration. It has:

- **Real safety bugs** in the residual cleanup path (C1/C2/C3 — can delete wrong registry hives, mass-match on substrings, execute uninstall strings through cmd.exe unsanitized).
- **Real runtime problems** on weak hardware (LHM opens in 2.7 s blocking startup, residual walk takes 28 s per app).
- **Thin test coverage** (1 test file, 1 test method).
- **Genuine differentiators** vs the competitive field (built-in VirusTotal, Windows-native lightweight, works on Windows 10).

The plan turns those liabilities into strengths. Seven waves, each shippable independently, landing at **v2.0.0 LTS** — an enterprise-deployable, tested, safe, runtime-light Windows-native maintenance suite. Beyond v2.0.0 is a decision point: pursue Kudu Cloud's territory with an on-prem/self-hosted server component (Wave 7), or stay pure-consumer.

**The tagline the plan is optimising for:**

> *SystemCleaner. No telemetry. No cloud. No auto-updates. No registry cleaner. No PC-health-score theatre. Just the things Windows should do itself but doesn't. Free forever. Open source top to bottom.*

The refusal list does the marketing work.

---

## 2. What SystemCleaner is (positioning)

**Windows-native maintenance suite for people who care what their tools are doing.** Clean up, uninstall, monitor, and hash-check against VirusTotal — all without leaving one lightweight window. Runs on Windows 10 and 11, tested on modest hardware. No telemetry, no auto-update, no cloud.

Three claims, each testable head-to-head against competitors:

- **"Windows-native"** — WPF, not Electron. 40–100 MB RAM at rest vs Kudu's 200–500 MB.
- **"For people who care what their tools are doing"** — every cleaner rule visible, audit log, signed rules, VT integration.
- **"Runs on modest hardware"** — the runtime brief's measurement targets are the baseline.

**Target audiences:**

- **Consumer power users** — the "runs on my 2015 laptop" crowd, the "I stopped trusting CCleaner" crowd.
- **Windows 10 users** (500M+ machines still) — nobody's optimising for this audience; FluentCleaner is Win11-only, Kudu doesn't prioritise it.
- **Trust-first / privacy-conscious** — audit-loggable, verifiable, offline-first behaviour.
- **Enterprise IT (post-v2.0.0)** — on-prem, self-hosted, MSI/MSIX-deployable, ADMX-configurable. Kudu Cloud can't reach this audience because their SaaS model excludes it structurally.

**Not our audience:**

- Users who want a maximum-feature-count product (Kudu's 15+ tools serves them; SystemCleaner does 4-5 features better instead).
- macOS / Linux users (Kudu / BleachBit already serve them; being Windows-only is our differentiator).
- Users who want a cloud dashboard for their 5-device household (Kudu Cloud's happy path).

**Positioning line for the README:**

> *SystemCleaner is a Windows-native maintenance suite for people who care about what their tools are doing. Clean up, uninstall, monitor, and hash-check against VirusTotal — all without leaving one lightweight window. Runs on Windows 10 and 11, tested on modest hardware. No telemetry, no auto-update, no cloud.*

See `system-cleaner-differentiation.md`, `system-cleaner-vs-kudu.md`, `system-cleaner-voice-of-user.md` for the reasoning behind each claim.

---

## 3. Current state (measured)

### Runtime and framework

- **.NET SDK:** 9.0.316 (project pinned to 9.0.304 rollForward latestPatch — works fine)
- **Target framework:** `net9.0-windows` (App), `net9.0` (Core, Tests)
- **UI:** WPF, `<UseWPF>true</UseWPF>`
- **Nullable:** enabled everywhere
- **Build:** clean, 0 warnings, 0 errors on this machine (Aug 8 2026)
- **Tests:** all 1 pass ✅

### Package inventory (from csproj)

| Package | Current | Notes |
|---|---|---|
| Microsoft.Extensions.DependencyInjection | **8.0.0** | Behind runtime (should be 9.x) |
| LibreHardwareMonitorLib | **0.9.4** | Behind stable (0.9.6 released Feb 2026) |
| SharpDX | **4.2.0** | 🛑 Archived since March 2019 |
| SharpDX.DXGI | **4.2.0** | 🛑 Archived since March 2019 |
| System.Management (App + Core) | 10.0.0 | Current |
| System.ServiceProcess.ServiceController | 10.0.0 | Current |
| Test tooling (Microsoft.NET.Test.Sdk 18.0.1, xunit 2.9.3, xunit.runner.visualstudio 3.1.5, coverlet.collector 6.0.4) | | Current |

### Distribution footprint

- **Self-contained single-file publish:** 132 MB (README recipe)
- **Framework-dependent publish:** 5.7 MB across 22 files
- **~2.8 MB of dead-weight transitive deps on every release** (Mono POSIX for Linux support pulled in by LHM's HidSharp — useless on Windows)

### Runtime baseline (measured on i5-7300U, 8 GB RAM, SSD, unelevated)

| Operation | Wall time | Heap Δ | Notes |
|---|---:|---:|---|
| **`LHM.Computer.Open()` — everything enabled** | **2 692 ms** | 630 KB start | Fixed cost; runs during `App.OnStartup` before window paints |
| `Open()` — CPU/GPU/Memory only | 2 154 ms | | Sensor count barely reduces Open cost |
| `Update()` per tick (216 sensors) | 2–4 ms | ~140 KB total | **Poll is cheap; Open is the elephant** |
| User temp scan (749 files, 246 dirs, 760 MB) | 216 ms | 408 KB | SSD; HDD would be 2–10× more |
| **HKLM\\SOFTWARE walk, depth ≤ 8** | **28 220 ms** | 6.2 MB | 352 205 keys visited for ONE app's residuals |
| `PerformanceCounter` fallback | ❌ FAILED | | Perf-counter registry corrupt on this machine — real thing to guard against |

**On a Celeron with 4 GB RAM, most of these numbers become 3–8× worse.** Two of them cross the "app feels dead" threshold on that hardware:

1. **LHM Open blocking startup for ~5–8 s.**
2. **Residual scan of 10 apps = ~5 minutes of one full CPU core** with no depth cap.

### CI / Build

- CI workflow: ✅ uses `actions/checkout@v5`, `actions/setup-dotnet@v5`
- Release workflow: 🛑 uses **archived** `actions/create-release@v1` + `actions/upload-release-asset@v1`
- `dotnet format --verify-no-changes` enforced
- `dotnet list --vulnerable` runs but doesn't fail on findings, no `--include-transitive`
- Dependabot: weekly, up to 5 PRs
- Code signing: none
- ReadyToRun / TieredPGO: not enabled

### Test coverage

- 1 file (`UnitTest1.cs`), 1 test method covering `DirectoryCleanupModule` happy path
- **Zero tests** for `FileSystemHelper.IsRestrictedPath`, reparse handling, `RegistryCleanupHandler.TryParseRegistryTarget`, residual token matching, StartupDiscovery approval-state binary payload, VirusTotal parser, DPAPI settings

See `system-cleaner-stack-and-roadmap.md` for full csproj inventory. See `system-cleaner-runtime-brief.md` for full runtime measurements. Raw probe output preserved in `probe-output.txt` and `perf-output.txt`.

---

## 4. Known bugs — the fix backlog

Grouped by tier. Severity assessment based on user harm.

### Critical (data loss or elevation-of-privilege risk)

| ID | Location | Problem | Fix |
|---|---|---|---|
| **C1** | UninstallerService.cs:1398 | Registry hive parser compares `"CurrentUser"` against `"HKEY_CURRENT_USER"` — never matches, defaults to LocalMachine. Every HKCU residual deletion targets HKLM instead. | Store hive as `HKEY_*` literals; reject unknown hive names in parser |
| **C2** | UninstallerService.cs:1051, :880, :1519, :1104 | Residual scanners match by 4+ char substring on names, then `DeleteSubKeyTree`/`Directory.Delete(recursive)`/`pnputil /delete-driver /uninstall /force`/`sc delete`. Uninstalling "Java" wipes anything with "java" in name across HKLM, drivers, services. | Anchored match (StartsWith/EndsWith on segment name) + confidence-rating tier (VeryGood/Good/Questionable), "Questionable" behind explicit Aggressive mode |
| **C3** | UninstallerService.cs:451 | Uninstall strings routed through `cmd.exe /c "..."` unsanitized. HKCU\Uninstall is user-writable → user-controlled process runs in elevated context when tool is run as admin (README instructs this). | Parse via `CommandLineToArgvW` P/Invoke into (fileName, argumentList); `Process.Start` with `UseShellExecute=false` |

### High

| ID | Location | Problem | Fix |
|---|---|---|---|
| **H1** | VirusTotalService.cs:75 | API key placed in URL path in `groups/{apiKey}` fallback (secret exposure via proxy logs, .NET diagnostic listeners, HAR captures) | Remove URL fallback; only header |
| **H2** | VirusTotalService.cs:55-58 | `HttpClient.DefaultRequestHeaders` mutated without sync during in-flight requests | Build `HttpRequestMessage` per call with header, or wrap `SetApiKey` in a lock |
| **H3** | UninstallerService.cs:501 | `TryCreateRestorePoint` silently fails unelevated; Windows also rate-limits to 1 per 24h; UI proceeds anyway with false "created" indication | Fail-closed when user opted in; set `SystemRestorePointCreationFrequency` override; or drop the option |
| **H4** | LargeFileCleanupModule.cs:80, DuplicateCleanupModule.cs:140 | Scanners don't skip reparse-point subdirectories → junction cycles loop indefinitely, OneDrive placeholders re-scan | Skip `FileAttributes.ReparsePoint` in both scanners |
| **H5** | UninstallerService.cs:1360 (multiple locations) | `catch { /* Try next view */ }` bare catches — cleanup failures return `false` with no reason | Surface through `messages` list |

### Hardware Monitor specifics

| ID | Location | Problem | Fix |
|---|---|---|---|
| **HM1** | DxgiAdapterReader.cs:30 | Recent DXGI PR reads `DedicatedVideoMemory` — always 0 for integrated GPUs (Intel HD/UHD/Iris). Verified on this machine: shows 0 B. LHM itself already reads D3D shared memory correctly. | Use `IDXGIAdapter3::QueryVideoMemoryInfo(DXGI_MEMORY_SEGMENT_GROUP_LOCAL)` via Vortice.DXGI |
| **HM2** | HardwareMonitorService.cs:24-33 | `IsNetworkEnabled` + `IsControllerEnabled = true` → 43 network filter adapters on this laptop, 215 sensors, useless CPU cost | Set both false; filter to top-level Wi-Fi/Ethernet if network needed |
| **HM3** | SystemUsageViewModel.cs:124-126 | "Run as admin" hint driven by `HasTelemetry` (any sensor present) — fires never when CPU load works but temps don't | Per-tile hint: if `TemperatureCelsius is null && !IsAdministrator` |
| **HM4** | HardwareMonitorService.cs:35-82 | Timer periodic; if `Update()` slows > interval, callbacks queue behind lock indefinitely | Re-arm timer at end of callback |

### Startup manager specifics

| ID | Location | Problem | Fix |
|---|---|---|---|
| **S1** | StartupDiscoveryService.cs:216-267 | HKLM entries can't be toggled unelevated; UI has no way to know before user clicks → guaranteed failure toast | Add `IsToggleable` (`IsAdmin || Location == "Current User"`), grey UI |
| **S2** | StartupDiscoveryService.cs:463-474 | Startup folder items throw "does not support toggling" even though approval binary could be written | Drop `RegistrySubKey` check; only need approval fields |
| **S3** | StartupDiscoveryService.cs:340-361 | `GetApprovalState` returns null when value absent, but scan treats absence as enabled — they disagree | Return `true` for absent, or push tri-state through UI |
| **S4** | StartupDiscoveryService.cs:34-42 | RunOnce entries toggled same as Run — but Windows deletes RunOnce values on next boot regardless | Label as "one-shot", disable toggle |
| **S5** | StartupEntry.cs | `IsEnabled` is read-only + no INPC → grid shows stale state after toggle unless whole list re-scans | Make mutable + INPC, or optimistically update + debounce re-scan |
| **S6** | Overall | No coverage of Scheduled Tasks with logon/boot triggers, no auto-start Services — real Autoruns gap | Add these two categories (Wave 7) |

### Medium & Low

Many smaller items — MainWindow.old.xaml.bak (41 KB stale backup checked in), dead `foreach (var issue in issues)` empty loop in `FindResidualItemsAsync`, empty CHANGELOG.md, SECURITY.md placeholder contact, deprecated GitHub Actions in release workflow, no signing infrastructure, WMI double-backslash string that's cosmetically odd but works, etc. All catalogued in the individual briefs.

See `system-cleaner-review.md` and `system-cleaner-audit-2.md` for full detail on every bug with file paths and line numbers.

---

## 5. Market position (competitor landscape)

### Direct competitor: Kudu

- **Stack:** Electron + TypeScript + Vite + Vitest (confirmed by tsconfig + electron.vite.config.ts in repo)
- **Distribution:** 108 MB installer, Windows/mac/Linux
- **Stars:** 2.1k GitHub (growing from 1.3k)
- **Cadence:** every 1–3 days (rapid iteration)
- **Cleaner catalog:** 445 JSON rules already (180 Windows / 135 mac / 130 Linux)
- **Business model:** MIT free desktop + **Kudu Cloud** ($5–9/device/mo SaaS-only, no self-hosted)
- **User rating:** 2.3/5 on review sites (small sample, reviewers cite "rush release" and elevation bugs)
- **Top open issues:** 3 of 7 are Windows elevation/admin bugs
- **Recurring changelog themes:** malware scanner false positives, cross-platform stability, registry WOW64 issues, startup persistence bugs

**Kudu is the benchmark, not the model.** Their scope defines what "complete" looks like. Their architecture (Electron + SaaS-only enterprise) defines what SystemCleaner can be different from.

### The rest of the landscape

| Tool | Category | License | Notes |
|---|---|---|---|
| **CCleaner** | All-in-one | Closed, freemium | Post-2017 malware + Avast acquisition → cultural distrust. Registry cleaner controversial. |
| **BleachBit** | Cleanup | GPL, Python/Qt | Reference "safe cleaner." ~90 cleaners. No registry, no automation without CLI. Basic UI. |
| **BCU (Bulk Crap Uninstaller)** | Uninstaller | Apache 2.0, C#/.NET | Reference OSS uninstaller. Confidence-rated residuals ("Very good"/"Good"/"Questionable"). |
| **Revo Uninstaller** | Uninstaller | Commercial | Trace-log install monitoring (Pro), pattern residuals. Free version limited. |
| **Sysinternals Autoruns** | Startup | MS freeware | 200+ ASEPs, SigCheck integration. Reference startup manager. |
| **HWiNFO** | HW monitor | Closed freeware; paid SDK for embedding | Best sensor coverage. Users complain "too much info." |
| **LibreHardwareMonitor** | HW monitor | MPL 2.0 | What we use. Right choice. |
| **WizTree** | Disk analyser | Freeware, closed | ~50× faster than WinDirStat via NTFS MFT scan. Admin required for MFT mode. |
| **dupeGuru** | Duplicates | GPL 3 | Size buckets → hash. Byte-exact + music + picture (perceptual) modes. |
| **FluentCleaner** | Cleanup | MIT, WinUI 3 | Windows 11 only. Uses winapp2.ini community rules (thousands). |
| **Chris Titus Winutil** | Debloat/tweaks | MIT, PowerShell + WPF | 30K+ stars. Different niche (post-install automation). |
| **Wintoys** | Debloat/tweaks | MS Store, closed | Debloat + privacy toggles + startup + repair. Win11 focused. |
| **Fleet (fleetdm.com)** | MDM | OSS | Self-hosted OSS MDM. **Adjacent category** (device config, not maintenance). Complementary to SystemCleaner Enterprise. |

**The "modern OSS all-in-one Windows maintenance suite" space in 2026 is more crowded than the initial review suggested** (I retracted the "yet another CCleaner clone" line in `system-cleaner-differentiation.md`). Kudu owns broad scope. FluentCleaner owns polished cleanup on Windows 11. Nobody owns self-hosted enterprise + weak hardware + Windows 10 first-class + VirusTotal-in-workflow. **That's the gap.**

### Eight axes where SystemCleaner can be "way different" from Kudu

1. **Native Windows runtime, not Electron** — 40–100 MB vs 200–500 MB RAM at rest
2. **Windows-only means Windows-first every UX decision** — right-click Explorer shell extension, jump lists, PowerShell module, MSIX
3. **Depth over breadth** — 3 pillars (cleanup, uninstall, HW) at 95% vs 15 at 60%
4. **VirusTotal integrated INTO the workflow** — not just at release time (Kudu VT-scans their releases; SystemCleaner scans users' files)
5. **Stability as feature** — quarterly LTS releases vs Kudu's every-1–3-days
6. **Windows 10 first-class, forever** — FluentCleaner is Win11-only, Kudu doesn't optimise
7. **Trust maximalism** — signed cleaner rules, opt-in auto-update, full audit log, offline-verifiable builds
8. **Contributor accessibility for Windows devs** — C#/.NET matches the culture; Kudu is TypeScript+Electron

**Three axes stack into a defensible position Kudu structurally can't copy:**

- **Bets 1 + 2 + 6 → "The Windows-native utility that runs on your grandma's laptop"** — Kudu's Electron will always be heavier; FluentCleaner requires Win11.
- **Bets 3 + 4 → "The safe uninstaller with built-in VirusTotal"** — the workflow moat.
- **Bets 5 + 7 → "The cleaner enterprise IT can approve"** — Kudu Cloud goes SaaS enterprise; SystemCleaner goes on-prem trust.

See `system-cleaner-comparisons.md` for feature-by-feature depth. See `system-cleaner-vs-kudu.md` for the full 8-axes analysis. See `system-cleaner-enterprise.md` for the enterprise positioning.

---

## 6. Voice of user — what shapes design

Themes from Reddit, XDA, HowToGeek, MakeUseOf, Steam Community, Microsoft's community hub, Tom's Guide.

### The core emotional shift

The "PC cleaner" category is under active criticism. XDA 2026 headline: *"I stopped using cleanup apps after discovering this built-in Windows 11 tool."* HowToGeek: *"Windows 11 already does what you thought you need Winhanced for."* Microsoft-aligned coverage: *"registry cleaners don't speed up your PC and none of them are safe."*

**Implication: cleanup can't be the headline feature.** Lead with uninstaller + VT workflow + weak-hardware.

### CCleaner distrust is still cultural memory (9 years later)

2017: APT17 compromised Piriform's build server, shipped signed malware to 2.27M users, undetected for 4 weeks. Weeks before, Avast acquired Piriform. Trust collapsed on two dimensions simultaneously. Post-Avast: forced updates without consent, unchecked Avast bundling, upgrade nag pop-ups. Users describe CCleaner as *"once-beloved."*

**Implication: every trust decision (no auto-update, no telemetry, no bundling, open source top to bottom) is signaling "we're not CCleaner." Say it explicitly in the README.**

### BleachBit is the "safe answer" — with real complaints

Reddit consensus recommendation for CCleaner alternatives. Praised for open source, no ads, no telemetry, preview-first. Criticised for basic UI, no automation without CLI. Even the recommendation is hedged: *"many suggest you still shouldn't use it."*

**Implication: match BleachBit's trust posture AND fix its known complaints — polished UI, GUI-first automation.**

### Feature demand signal strength (aggregated)

| Feature | Signal | Notes |
|---|---|---|
| Preview before delete | ★★★★★ Universal | BleachBit's Preview button = why people trust it |
| Open source, verifiable | ★★★★★ | Post-CCleaner test |
| No telemetry | ★★★★★ | Must be explicitly stated |
| No auto-update by default | ★★★★☆ | CCleaner grievance |
| Simple UI | ★★★★☆ | HWiNFO/HWMonitor complexity actively criticised |
| Right-click Explorer VT scan | ★★★★☆ | Multiple standalone tools prove demand nobody has bundled |
| Deep residual uninstall | ★★★★☆ | Universal frustration ("Windows leaves residues everywhere") |
| Lightweight tray widget | ★★★★ | Users want glanceable, not full window (Speccy, Venmon, MiniUsage cited favourably) |
| Portable option | ★★★ | Praised where available |
| CLI / PowerShell automation | ★★★ enterprise, ★★ consumer | Enterprise asks, consumers rarely notice |
| **Registry cleaner** | **★☆ actively negative** | Community view has flipped — refusing to ship one is a trust signal |
| **PC Health Score** | ✕ | CCleaner-era pseudo-metric marketing — do not ship |

### Design decisions the voice-of-user research adds to the plan

- **VT shell extension in Wave 3** (bonus deliverable, 1 day work, high visibility — users screenshot and share)
- **Tray widget for HW monitor** in Wave 4/5 — matches what users actually cite favourably
- **Explicit refusal list in README** as marketing — refusals do positioning work in this audience

### The tagline the audience would respond to

> *SystemCleaner. No telemetry. No cloud. No auto-updates. No registry cleaner. No PC-health-score theatre. Just the things Windows should do itself but doesn't. Free forever. Open source top to bottom.*

See `system-cleaner-voice-of-user.md` for quotes, sources, aggregation.

---

## 7. Design principles

Concise, load-bearing. Every design decision maps to at least one.

1. **Safety first, always.** No feature ships that can silently delete user data without confirmation. Preview-before-delete is universal. Confidence ratings on destructive actions.
2. **Windows-native.** WPF, direct P/Invoke, direct WMI, direct LHM. No cross-platform abstractions. Windows-idiomatic UX (right-click menus, jump lists, Action Center toasts, MSIX).
3. **Weak hardware first-class.** Runs acceptably on Celeron / 4 GB / HDD / integrated GPU. Performance Mode auto-detects and adapts. `RenderCapability.Tier` fallback theme.
4. **Depth over breadth.** 4–5 features at 95% quality beats 15 features at 60%. Every new feature must reinforce one of the three pillars (cleanup, uninstall+VT workflow, hardware monitor).
5. **Trust maximalism.** No telemetry. No auto-update by default. No bundled software. No cloud requirement. Signed everything (binaries + cleaner rules). Audit log everywhere. Explicit "we will never" refusal list.
6. **Reversibility.** Every destructive operation logs enough context to explain to the user what happened and why. Restore-point option before batch operations. Undo where possible.
7. **Lazy loading.** Startup cost matters more than steady-state cost on weak hardware. Nothing constructs until first use.
8. **Event-driven where feasible.** `RegNotifyChangeKeyValue` instead of polling. Windows Event Log listeners instead of scanning. Push, not pull.
9. **Same binary, policy-driven.** Consumer and enterprise are the same exe. `HKLM\Software\Policies\SystemCleaner` (populated by ADMX) toggles the enterprise-visible behaviours.
10. **Composable via feature modules.** Each pillar lives in its own assembly. New features register via `IFeatureModule.Register(IServiceCollection)`. Enables lazy loading, plugin story, and enterprise policy scoping.

---

## 8. What we're deliberately NOT doing

Stated explicitly to prevent scope creep.

- **Avalonia + NativeAOT migration.** Approach C from the lightweight brief. Only revisit after v2.0.0 has real users and there's evidence WPF's runtime cost is the ceiling. Massive effort otherwise.
- **Cross-platform.** Windows-native IS the differentiator vs Kudu. Refuse.
- **Own malware scanner.** VirusTotal integration is the moat. Don't try to build another signature engine (that's Kudu's biggest changelog burden).
- **Registry cleaner.** Genuinely dangerous, Microsoft explicitly discourages, no real user value. Kudu ships this; SystemCleaner refuses and documents why.
- **PC Health Score / performance rating.** CCleaner-era pseudo-metric marketing. No basis in technical fact.
- **Game Mode / GPU tweaker / driver installer.** Windows / GeForce Experience / AMD Adrenalin handle this.
- **Debloater as first-class feature.** Chris Titus Winutil owns that space with 30K+ stars. Small curated Appx list under Settings > Privacy is fine; not the headline.
- **Open-core paywall split.** Kills the trust story. Sustainability via sponsors + support + professional services only.
- **Auto-update by default.** Post-CCleaner audience specifically distrusts this. Opt-in only.
- **Cloud requirement.** Server component is opt-in and self-hosted. VT is the only default outbound call, and only when user initiates.
- **Bundled software.** Any installer. Ever.
- **Multi-tenant SaaS.** Enterprise angle is on-prem/self-hosted only. Optional hosted server later, but not multi-tenant.

---

## 9. Target stack

Grouped by change category.

### Package updates (immediate priority)

| Package | From | To | Why |
|---|---|---|---|
| Microsoft.Extensions.DependencyInjection | 8.0.0 | 9.0.x | Match TFM |
| LibreHardwareMonitorLib | 0.9.4 | 0.9.6 | Newer CPU support, AV false-positive reduction |
| SharpDX + SharpDX.DXGI | 4.2.0 (archived) | **Vortice.DXGI 3.x** | Archived since 2019. Vortice active + gives us DXGI 1.4+ for `QueryVideoMemoryInfo` (fixes HM1) |
| HidSharp transitive Mono deps | shipped | excluded via `<ExcludeAssets>native</ExcludeAssets>` | −1.8 MB dead weight |

### Packages to add (small, targeted)

| Package | Why | Where |
|---|---|---|
| **CommunityToolkit.Mvvm 8.x** | Source-generated `[ObservableProperty]` / `[RelayCommand]` — cuts ViewModel boilerplate ~40%, reduces reflection, AOT-friendly | App |
| **System.Diagnostics.PerformanceCounter 9.x** | Fallback CPU/RAM % when LHM fails (probe showed this can happen on corrupted perfmon registry) | App |
| **Microsoft.Extensions.Logging** + **Microsoft.Extensions.Logging.EventLog** | Structured logging replaces `DiagnosticLogger`. Event Log sink for enterprise SIEM ingest. | Both |
| **System.Text.Json** source-generator contexts | Already in BCL; add `[JsonSerializable]` for VT DTOs, AppSettings, cleaner-rules JSON. Cuts JSON reflection, AOT-ready. | App, Core |
| **Microsoft.Extensions.Configuration.Json** | Future ADMX-registry-backed policy file (`policy.json`) and JSON-defined cleaner rules | Core |
| **PInvoke.User32 / PInvoke.Advapi32** (or hand-rolled DllImport) | `RegNotifyChangeKeyValue` + `CommandLineToArgvW` + shell-extension registration | Core |

### Deliberately not adding

Serilog / NLog (M.E.L is enough), ReactiveUI (CT.Mvvm is enough), MediatR (indirection without value at our size), Prism / MVVM Light (legacy), UI-control library (too heavy, licensing story), Avalonia (different framework migration), Vortice.Direct3D12 or larger DirectX (unnecessary).

### Build & CI

| Change | From | To |
|---|---|---|
| Release workflow | `actions/create-release@v1` + `actions/upload-release-asset@v1` (archived) | `softprops/action-gh-release@v2` (single step) |
| Vulnerable-package check | prints, doesn't fail | fail on any finding + `--include-transitive` |
| Code signing | none | **SignPath.io free program for OSS** or Certum OV cert |
| Publish flags | `PublishSingleFile` only | Add `<PublishReadyToRun>true</PublishReadyToRun>` — bigger binary (we don't care), faster JIT on weak CPUs |
| Dynamic PGO | default | `<TieredPGO>true</TieredPGO>` explicitly |
| Analyzers | none | `<AnalysisLevel>latest-recommended</AnalysisLevel>` |
| Warnings as errors | off | `<TreatWarningsAsErrors>true</TreatWarningsAsErrors>` (post cleanup pass) |
| Test matrix | `windows-latest` only | `windows-2019` + `windows-latest` |

---

## 10. Target architecture

### Project layout (after Wave 5 refactor)

```
SystemCleaner.sln
├── SystemCleaner.Core.Abstractions      (interfaces, DTOs — shared)
├── SystemCleaner.Core.Common            (helpers, extensions — shared)
├── SystemCleaner.Features.Cleanup       (cleanup modules, JSON rules loader)
├── SystemCleaner.Features.Uninstall     (inventory, residual scan, confidence rating)
├── SystemCleaner.Features.Startup       (registry + folder + scheduled tasks + services)
├── SystemCleaner.Features.HardwareMonitor  (LHM + Vortice.DXGI adapter)
├── SystemCleaner.Features.VirusTotal    (moves out of App)
├── SystemCleaner.Features.ShellExtension    (right-click Explorer registration)
├── SystemCleaner.App                    (WPF UI only, references all Features via IFeatureModule)
├── SystemCleaner.Cli                    (headless mode; --config policy.json)
└── SystemCleaner.Tests                  (per-feature test projects)
```

Each `Features.*` project:

- Implements `IFeatureModule.Register(IServiceCollection services)`.
- Registers its services as `Lazy<T>` where cold-start-visible.
- Exposes a `MainTab` or `TrayCommand` metadata for the UI to discover.
- Owns its own JSON schema for policy configuration.

### Cross-cutting

- **`IPolicyProvider`** reads `HKLM\Software\Policies\SystemCleaner` — populated by ADMX. Every feature checks policy before acting.
- **`IAuditLog`** structured JSON writer with rotation. Sinks: file + Windows Event Log.
- **`IUserConfirmationService`** already exists — extended with policy-aware "confirmation required" toggle.
- **Global `INotificationService`** already exists — extended with Windows Action Center toast integration.

### Runtime patterns

- **Lazy DI service factories** for HW monitor, uninstaller, VT.
- **Event-driven refresh** via `RegNotifyChangeKeyValue` for startup + installed software.
- **Adaptive polling** for HW monitor: 2 s active, 5 s on battery or minimised, stop on hidden.
- **HDD-aware IO throttling** via WMI `Get-PhysicalDisk MediaType` once per session.
- **`RenderCapability.Tier` fallback theme** — swap in minimal theme on Tier 0/1.
- **Prefix-then-full hash** for duplicate finder.
- **Cached HKLM\SOFTWARE + HKCU\Software walk** — walk once per session, index by normalised segment name.

---

## 11. The seven-wave roadmap

Each wave ships independently. Version numbers are notional (semver flavor is your call).

### Wave 1 — Zero-downside cleanup (~1 weekend)

Small, safe, no dependencies. Ships immediately as `v1.0.1`.

1. Delete `MainWindow.old.xaml.bak`
2. Bump `Microsoft.Extensions.DependencyInjection` 8 → 9
3. Bump `LibreHardwareMonitorLib` 0.9.4 → 0.9.6, verify sensors still enumerate
4. Replace SharpDX with Vortice.DXGI (~40-line `DxgiAdapterReader.cs` rewrite)
5. Exclude Mono POSIX transitive assets from HidSharp
6. Replace deprecated release workflow actions with `softprops/action-gh-release@v2`
7. Make vulnerable-package CI check fail on findings + `--include-transitive`
8. Delete dead `foreach (var issue in issues)` empty loop in `UninstallerService`
9. Timer re-arm at end of `HardwareMonitorService` callback (HM4)
10. Suspend HM on window minimise/blur
11. Publish Winget manifest

### Wave 2 — Test scaffolding (~1 weekend)

Ships as `v1.2.0`. No binaries, but reputationally the "we have tests now" release.

- Adopt `CommunityToolkit.Mvvm` for 2-3 sample ViewModels
- Tests for `FileSystemHelper.IsRestrictedPath` (every branch)
- Tests for `FileSystemHelper.NormalizePath` (env vars, 8.3, trailing separator, invalid input)
- Tests for `RegistryCleanupHandler.TryParseRegistryTarget` (will fail immediately — confirms C1)
- Tests for `ResidualTokenBuilder.NormalizeForComparison` + `.Build`
- Tests for `StartupDiscoveryService` approval-state binary payload round-trip (against throwaway `HKCU\Software\SystemCleaner.Tests` key)
- Snapshot tests for VirusTotal JSON parsing against fixtures

Target: 30+ tests. Coverage % not the goal; the specific tests that will fail if C1/C2/C3 rewrites regress are the goal.

### Wave 3 — Safety rewrites (~2 weekends + VT shell extension bonus)

Ships as `v1.3.0`. Positioning release: "the safe uninstaller."

1. **C1** — Registry hive parser fix. Store as `HKEY_*` literals, reject unknowns.
2. **C2** — Anchored match + `ResidualItem.Confidence` tier. UI groups by tier; "Questionable" behind Aggressive mode. Depth cap on registry walk at 6.
3. **C3** — Parse uninstall strings via `CommandLineToArgvW` P/Invoke; `Process.Start` with `UseShellExecute=false` and `ArgumentList`. Fallback "Show raw command" button.
4. **H1** — Remove `groups/{apiKey}` URL fallback.
5. **H2** — Per-request `HttpRequestMessage` header (or lock around `SetApiKey`).
6. **DPAPI migration** — eagerly on legacy plaintext load.
7. **Bonus: VT shell extension** — right-click any file in Explorer → "Check with VirusTotal via SystemCleaner." 1 day work, high visibility, screenshottable moat vs Kudu.

Real release notes for this one. Position "unlike substring-based cleaners, SystemCleaner rates every residual by confidence."

### Wave 4 — Runtime lightweight (~1 weekend + measurement)

Ships as `v1.4.0`. Positioning release: "runs on modest hardware."

1. `Lazy<T>` DI factories for HW monitor + uninstaller + VT (cuts 2.7 s off cold start)
2. `IsNetworkEnabled = false`, `IsControllerEnabled = false` in LHM (216 sensors → 21)
3. Cached HKLM\SOFTWARE + HKCU\Software walk (28 s × N-apps → 28 s + O(1))
4. `RenderCapability.Tier` detection + minimal theme swap
5. HDD detection via WMI + serial IO on HDD
6. Prefix-hash for duplicate finder (~500× speedup on same-size distinct files)
7. Verify `VirtualizingStackPanel.VirtualizationMode="Recycling"` on all DataGrids

**Measure and publish before/after numbers.** Screenshot on an actual old laptop for the release note.

### Wave 5 — Feature modules + JSON cleaners (~1-2 weekends)

Ships as `v1.5.0`. Internal only, no user-facing changes.

1. Split `Core` into `Features.Cleanup / Uninstall / Startup / HardwareMonitor / VirusTotal` assemblies
2. `IFeatureModule.Register(IServiceCollection)` contract
3. `CleanupModuleCatalog.cs` → `cleaners.json` with schema. **winapp2.ini import as compat layer** → past Kudu's 445 rules
4. `RegNotifyChangeKeyValue` wrapper class in `Core.Abstractions`
5. Structured logging via `Microsoft.Extensions.Logging`, sinks = file + Windows Event Log
6. `IPolicyProvider` reads `HKLM\Software\Policies\SystemCleaner`

### Wave 6 — Enterprise-ready endpoint (~3-4 weekends)

Ships as `v2.0.0 LTS`. The enterprise-friendly release.

1. **MSI installer** (WiX Toolset 4)
2. **MSIX package** (Intune + Microsoft Store for Business)
3. **ADMX + ADML templates** (policy keys for enabled features, denied paths, VT key, tab visibility, audit-log location)
4. `SystemCleaner.exe --config policy.json --headless` mode
5. **Structured JSON audit log** — RFC 5424 severity, ISO 8601 timestamps, correlation IDs. Splunk / Sentinel / ELK ingest.
6. **Windows Event Log** under `Applications and Services Logs\SystemCleaner`
7. **Code signing** — SignPath.io for OSS or Certum OV
8. **LTS badge** — 24-month backport commitment
9. **Tested-on matrix** — Win10 21H2/22H2, Win11 22H2/23H2/24H2, Win Server 2019/2022
10. **SOC 2 / CIS-alignment docs**

Post-launch: post in r/sysadmin, r/msp, r/pcmasterrace. Watch for pilot deployments.

### Wave 7 — Defer (revisit 3-6 months after v2.0.0)

If v2.0.0 gets traction: server component + Autoruns-scale feature growth. See §12.  
If not: consumer-only reassessment.

### Release plan

| Version | Wave | Scope | User-visible? |
|---|---|---|---|
| v1.0.1 | 1 | Zero-downside cleanup, Winget | ✅ perf + housekeeping |
| v1.2.0 | 2 | Test scaffolding | ⚠️ reputational only |
| **v1.3.0** | 3 | Safety rewrites + VT shell extension | ✅ big change — safety story |
| v1.4.0 | 4 | Runtime lightweight | ✅ perf story |
| v1.5.0 | 5 | Feature modules + JSON cleaners | ⚠️ internal + JSON cleaner catalog expands |
| **v2.0.0 LTS** | 6 | Enterprise-ready endpoint | ✅ big release — enterprise story |
| v2.x | 7 | Server + feature growth | later |

Between v1.3.0 and v2.0.0, all changes are additive or internal. No user-facing breaks.

---

## 12. Enterprise angle (Wave 6-7 detail)

### The gap SystemCleaner Enterprise fills

- **Kudu Cloud** is SaaS-only ($5–9/device/mo). No self-hosted option.
- **CCleaner Cloud for Business** is SaaS-only (£2–8/PC/mo), closed source, post-scandal distrust.
- **Fleet (fleetdm.com)** is self-hosted OSS but in adjacent MDM category (device config, not maintenance) — complementary not competitive.

**No self-hosted, open-source, Windows-native maintenance suite with fleet management exists in 2026.**

Target audience Kudu Cloud excludes structurally: defence, healthcare with PHI, EU public sector with data residency, air-gapped OT, financial services, universities with FERPA, any CISO office that has vetoed SaaS.

### Three-phase build (Wave 6 = phase 0; Wave 7 = phase 1)

**Phase 0 (Wave 6) — Enterprise-ready endpoint (3-4 weekends).** Delivered above.

**Phase 1 (Wave 7, wait 3-6 months first) — Small-fleet visibility (2-4 months).**

- `SystemCleaner.Server` — ASP.NET Core 9 minimal API. Windows Service or Docker container.
- PostgreSQL (>100 devices) or SQLite (<100 devices) backend.
- Endpoint reports over mTLS WebSocket. One-time enrollment tokens.
- Blazor Server dashboard — everything Kudu Cloud shows, minus the SaaS.
- Remote commands with confirmation gates.
- **Windows Auth + Active Directory** — no separate user database.
- REST API for Grafana / PowerBI / Splunk integration.
- Air-gap first-class supported.

Deliberately: no charge for the server. Free forever. Open source.

**Phase 2 (later, if Phase 1 gets traction) — Sustainability.**

- GitHub Sponsors + Open Collective ($5-20 individual, $200-2000 corporate)
- Paid email support ($500-2000/year per organisation)
- Optional hosted server for teams who don't want to run infra ($1-2/device/mo covering hosting)
- Professional services / deployment help / custom cleaner rules ($150-250/hour)
- Government / regulated-industry consulting

**Deliberately not open-core paywall** — kills community trust.

### Real costs of the enterprise angle

- **Wave 6 is doable in 3-4 weekends** on top of the desktop app work.
- **Wave 7 is a 2-4 month project** — mini-SaaS in every respect (auth, RBAC, TLS, DB migrations, upgrade path, backup guidance, monitoring, alerting, docs). Real burnout risk for a solo dev.
- **SOC 2 Type II cert:** $50K-100K/year in audit fees. "SOC 2-aligned documentation" is free and works at small-medium enterprise level.
- **Enterprise sales cycle:** 6-18 months from first contact to purchase.
- **Legal / liability:** an enterprise tool on 5000 machines that deletes wrong = **real legal exposure.** Needs disclaimers, E&O insurance if taking money, contracts drafted by an actual lawyer. **Biggest hidden cost.**

See `system-cleaner-enterprise.md` for detailed cost + business model analysis.

---

## 13. Honest downsides (of the plan itself)

To keep the plan honest. Every recommendation has a cost.

- **C2 rewrite makes the tool feel less thorough at first.** Anchored match + confidence ratings will show fewer items than the current substring match. Users who trusted the "very thorough" behaviour will see it as regression. Mitigation: clear release notes + optional Aggressive mode opt-in.
- **Lazy `HardwareMonitorService` moves cost, doesn't eliminate it.** Cold start drops 2.7 s, but first click on Monitor tab now takes 2.7 s. Users who always check monitor first feel the regression. Speculative background `Open()` after 500 ms is possible but reintroduces the "doing work you didn't ask for" pattern.
- **Turning off `IsNetworkEnabled` loses genuine Wi-Fi/Ethernet throughput sensors.** 41 of the 43 network hardware entries are filter-layer duplicates; the top 2 are real. Correct fix is a 30-line filter, not a flag flip.
- **`RegNotifyChangeKeyValue` is P/Invoke with edge cases** — native handle management, 32/64-bit view redirection (WOW6432Node) requires two watchers per logical location, key deletion signals + rewatch cycle, thread-pool wait registration cancellation care. Budget 1-2 days including tests, not "one afternoon."
- **Cached residual walk assumes correctness of cache invalidation.** Cache invalidation is a design problem. First app still costs 28 s (if typical user cleans one app at a time, you got them nothing — real value is bulk multi-select).
- **`RenderCapability.Tier` fallback = two visual systems forever.** Every new UI feature needs two variants. Cheaper alternative: avoid `BlurEffect`, `DropShadowEffect`, gradient brushes, animations *everywhere* — slightly less flashy on high-end machines, no branching. Consider this before building the two-track system.
- **"Performance Mode" preset is a permanent maintenance branch.** Every subsequent feature must be tested in both modes. Consider single "Low-power mode" toggle in Settings with no auto-detect as simpler alternative.
- **Feature-scope expansion doubles support and test burden.** Each new cleaner category is a support commitment. Autoruns-scale adds hundreds of rows users have never seen — some will disable critical entries and blame the tool.
- **Enterprise sales is a full-time job.** Solo dev cannot do it well without dedicating significant time to non-code activities (POCs, procurement paperwork, contract negotiation, PM answering CIO questions, published SLAs, dedicated support). Realistic Wave 7 timeline could balloon accordingly.
- **Legal / liability exposure grows with each of these features.** At consumer scale, "user shrugs and uninstalls" is the failure mode. At enterprise scale on 5K machines, deleting wrong = lawyers.
- **Contributor onboarding gets harder.** Right now the codebase is a single-developer .NET WPF app. After all recommended changes it's lazy-loaded DI, event-driven registry, JSON-defined cleaners with schema validation, feature-module architecture, tier-detection, HDD/SSD adaptation. Onboarding a second contributor is much harder than today.

**Delayed execution is its own risk.** The longer we plan without touching code, the more the plan feels theoretical. Real friction only surfaces when `dotnet build` runs.

See `system-cleaner-downsides.md` for the full downsides catalog.

---

## 14. Where we are, what's next

**Research status:** effectively saturated on the "what" question. We know what to build, why, and in what order.

**Execution status:** paused by your explicit "no execution yet" instruction. The Wave 1 items are ready to execute at any time; nothing further needs to be researched to make them safe.

**Remaining research topics genuinely worth doing:**

- **UX / UI redesign brief** — wireframes for the confidence-rating residual dialog, tray widget, right-click Explorer integration, refusal-list README. **Unblocks Wave 3 UI implementation.**
- **JSON schema + winapp2.ini import spec** — before Wave 5 execution.
- **Enterprise tooling deep-dive** — WiX vs alternatives, ADMX authoring, Event Log source registration, SignPath.io setup, SOC 2 documentation templates. **Before Wave 6 execution.**

Everything else is either already researched, or would be marginal.

**Motion stability:** direction consistent since first review. No conclusions reversed; several refined by later research. No contradictions in the plan.

**Risks to stability:**

- **Scope creep** — voice-of-user added tray widget + shell extension without pruning. So far additive-only is holding. Eventually will need to say what we're *not* doing to make room; the §8 refusal list is doing that job so far.
- **Delayed execution** — 3-4 more pure-research sessions without touching code and this risk materialises.

---

## 15. Appendix — sources + reference briefs

### Individual briefs in this session

| Brief | Purpose |
|---|---|
| `system-cleaner-review.md` | Initial code review, C1/C2/C3 discovery, first fix list |
| `system-cleaner-audit-2.md` | Runtime probe results, extended bug catalog (HM1, S1-S6) |
| `system-cleaner-comparisons.md` | Feature-by-feature vs competitors (BCU, Autoruns, HWiNFO, BleachBit, WizTree, dupeGuru) |
| `system-cleaner-lightweight-brief.md` | Binary size analysis (superseded by runtime-brief) |
| `system-cleaner-runtime-brief.md` | Runtime cost measured on i5-7300U — the operative lightweight document |
| `system-cleaner-downsides.md` | Honest costs of every recommendation |
| `system-cleaner-differentiation.md` | Landscape survey; retraction of "clone" line; VirusTotal moat validation |
| `system-cleaner-vs-kudu.md` | Kudu deep-dive; 8 axes of "way different"; 3 that stack |
| `system-cleaner-enterprise.md` | Kudu Cloud pricing, on-prem gap, three-phase enterprise build |
| `system-cleaner-stack-and-roadmap.md` | Stack today → stack target → 7-wave roadmap |
| `system-cleaner-voice-of-user.md` | Reddit / XDA / HowToGeek / MakeUseOf / Steam / Microsoft community themes |
| `STATUS.md` | Motion + stability meta-check |
| `PLAN.md` | (this file) |

### Raw data

- `probe-output.txt` — headless probe run against your i5-7300U (Startup entries, Installed apps, LHM sensors, cleanup scan, registry walk timings)
- `perf-output.txt` — perf-specific probe run measuring LHM Open cost, PerfCounter fallback, temp scan, registry walk

### External sources cited (deduplicated, most-referenced)

**Kudu:**
- [Kudu — usekudu.com](https://usekudu.com/)
- [Kudu Cloud pricing](https://usekudu.com/pricing)
- [Kudu cleaner catalog (445 rules)](https://usekudu.com/cleaners)
- [AdventDevInc/kudu on GitHub](https://github.com/AdventDevInc/kudu)
- [Kudu CHANGELOG](https://github.com/AdventDevInc/kudu/blob/main/CHANGELOG.md)

**Competitor landscape:**
- [BCUninstaller](https://www.bcuninstaller.com/)
- [Revo Uninstaller Pro 5 manual](https://www.revouninstaller.com/wp-content/themes/revo/files/RevoUninstallerProUserManual.pdf)
- [Sysinternals Autoruns](https://learn.microsoft.com/en-us/sysinternals/downloads/autoruns)
- [FluentCleaner (builtbybel)](https://github.com/builtbybel/FluentCleaner/releases)
- [Fleet — open source MDM](https://fleetdm.com/lp/open-source)
- [Chris Titus Winutil](https://github.com/ChrisTitusTech/winutil)

**Voice of user:**
- [XDA — I stopped using cleanup apps](https://www.xda-developers.com/i-stopped-using-cleanup-apps-after-discovering-built-in-windows-11-tool/)
- [HowToGeek — Windows 11 already cleans up for you](https://www.howtogeek.com/stop-using-winhancedwindows-11-already-does-the-cleanup-for-you/)
- [Windows Forum — Registry cleaners myths](https://windowsforum.com/threads/do-registry-cleaners-help-windows-11-myths-risks-and-safer-fixes.418089)
- [TechCrunch — CCleaner malware 2.27M users](https://techcrunch.com/2017/09/18/avast-reckons-ccleaner-malware-infected-2-27m-users/)
- [Cisco Talos — CCleanup incident](https://blog.talosintelligence.com/avast-distributes-malware/)
- [MakeUseOf — Is CCleaner Safe?](https://www.makeuseof.com/tag/stop-using-ccleaner-windows/)
- [Genbox VirusTotalContextMenu — demand validation](https://github.com/Genbox/VirusTotalContextMenu)

**Technical references:**
- [.NET runtime issue #79166 — NativeAOT WPF](https://github.com/dotnet/runtime/issues/79166)
- [Microsoft Learn — Trim self-contained apps](https://learn.microsoft.com/en-us/dotnet/core/deploying/trimming/trim-self-contained)
- [Microsoft Learn — MSIX enterprise deployment](https://learn.microsoft.com/en-us/windows/msix/desktop/managing-your-msix-deployment-enterprise)
- [Microsoft Learn — RegNotifyChangeKeyValue](https://learn.microsoft.com/en-us/windows/win32/api/winreg/nf-winreg-regnotifychangekeyvalue)
- [Microsoft Learn — WPF Graphics Rendering Tiers](https://learn.microsoft.com/en-us/dotnet/desktop/wpf/advanced/graphics-rendering-tiers)
- [Microsoft Learn — Background garbage collection](https://learn.microsoft.com/en-us/dotnet/standard/garbage-collection/background-gc)
- [Microsoft Learn — IDXGIAdapter3::QueryVideoMemoryInfo](https://learn.microsoft.com/en-us/windows/win32/api/dxgi1_4/nf-dxgi1_4-idxgiadapter3-queryvideomemoryinfo)
- [Vortice.Windows discussions](https://github.com/amerkoleci/Vortice.Windows/discussions/106)
- [LibreHardwareMonitorLib 0.9.6 on NuGet](https://www.nuget.org/packages/LibreHardwareMonitorLib/)

Everything else cited in the individual briefs.

---

*End of canonical plan. Individual briefs remain in the scratchpad for detailed reference but this document supersedes them as the source of truth going forward.*
