# SystemCleaner — Plan Summary (For External Review)

A 10-minute read of the plan. Full 60 KB canonical version at `PLAN.md`; this is the decision-focused subset for reviewers.

**What is SystemCleaner?**
A Windows-native maintenance suite: cleanup + uninstaller + startup manager + hardware monitor + VirusTotal integration. Built on .NET 9 + WPF. Currently ~5.7 MB framework-dependent binary, single solo maintainer, MIT licensed.

**What's the pitch?**
> *SystemCleaner is a Windows-native maintenance suite for people who care about what their tools are doing. Clean up, uninstall, monitor, and hash-check against VirusTotal — all without leaving one lightweight window. Runs on Windows 10 and 11, tested on modest hardware. No telemetry, no auto-update, no cloud.*

---

## The load-bearing decisions

### Decision 1: Stay on WPF instead of migrating to Avalonia or WinUI 3

- **Chose:** Keep WPF. Rejected Avalonia (would enable NativeAOT + 25-40 MB single-file but requires full XAML rewrite, 3-6 weekends) and WinUI 3 (no NativeAOT, requires Windows 11).
- **Rationale:** Windows 10 support matters (500M+ machines); the migration cost only pays off if binary size is a real business constraint (it isn't — runtime cost is what matters).
- **Downside acknowledged:** WPF's ~125 MB self-contained baseline is architectural. Can't be reduced meaningfully.

### Decision 2: Runtime lightweight, not binary lightweight

- **Chose:** Target CPU/RAM/IO on weak hardware (4 GB Celeron with HDD and integrated GPU).
- **Levers:** Lazy service initialisation (cuts 2.7 s cold-start delay from LibreHardwareMonitor's `Open()`); event-driven registry watching via `RegNotifyChangeKeyValue` P/Invoke; cached HKLM\SOFTWARE walk (turns 28 s × N-apps into 28 s + O(1)); `RenderCapability.Tier` fallback theme; HDD detection for IO serialisation.
- **Downside acknowledged:** "Performance Mode" preset becomes a permanent maintenance branch requiring dual-mode testing forever.

### Decision 3: Depth over breadth — 3 pillars, not 15 features

- **Chose:** Cleanup + Uninstaller (with VirusTotal-check-before-uninstall) + Hardware Monitor. Everything else is either supporting or refused.
- **Rejected:** Debloater as first-class feature (Chris Titus Winutil owns that space with 30K+ stars), registry cleaner (community consensus 2026: harmful), PC health score, game mode, own malware signatures (Kudu's biggest changelog burden).
- **Reasoning:** Kudu already exists with 15+ features at ~60% polish. Beating them means fewer features at 95%.

### Decision 4: VirusTotal integrated into the uninstall workflow

- **Chose:** Hash-check the exe against VirusTotal before uninstall as a first-class workflow. Right-click Explorer shell extension for on-demand file scans.
- **Rationale:** No other all-in-one Windows utility has this. Multiple standalone VT context-menu tools exist (Genbox, RightClickVirusTotal, WCMS) proving demand. Kudu VT-scans their releases but doesn't offer users VT-checking their own files.
- **Downside acknowledged:** VT API key exposure is currently a security defect (H1/H2 from the bug list) — this defect must be fixed before positioning around it.

### Decision 5: Enterprise as on-prem self-hosted (not SaaS)

- **Chose:** Wave 6 makes the endpoint enterprise-deployable (MSI + MSIX + ADMX + audit log + code signing + LTS). Wave 7 (deferred 3-6 months) potentially adds a free self-hosted server.
- **Rejected:** SaaS multi-tenant, per-device pricing, open-core paywall split.
- **Rationale:** Kudu Cloud at $5-9/device is SaaS-only, closed dashboard, cross-platform tax. Every axis reversed = the on-prem self-hosted OSS gap that exists in 2026 (Fleet is the reference in adjacent MDM category, no equivalent exists for maintenance).
- **Downside acknowledged:** Wave 7 server is a 2-4 month project on top of desktop work. Real burnout risk for solo dev. SOC 2 Type II costs $50K-100K/year (not doing formal cert, only "SOC 2-aligned documentation").

---

## The 7-wave roadmap

| Version | Wave | Scope | Effort | Ready to execute? |
|---|---|---|---|---|
| **v1.0.1** | 1 | Zero-downside cleanup (Vortice.DXGI replacing archived SharpDX, LHM 0.9.4→0.9.6, remove Mono dead weight, timer re-arm, HW monitor suspend on minimise, Winget manifest) | 1 weekend | ✅ Yes |
| v1.2.0 | 2 | Test scaffolding — 30+ tests covering the destructive paths, `TryParseRegistryTarget` (which will fail immediately, confirming C1), residual token matching, DPAPI settings | 1 weekend | ✅ Yes, no research dependency |
| **v1.3.0** | 3 | Safety rewrites — C1 registry hive parser, C2 anchored+confidence match, C3 uninstall string parsing via `CommandLineToArgvW`. VirusTotal H1/H2 fixes. **Bonus: VT shell extension.** | 2 weekends | ⚠️ Needs UX for confidence-rating dialog |
| v1.4.0 | 4 | Runtime lightweight — Lazy DI, cached residual walk, Tier-fallback theme, HDD detect + IO serialisation, prefix-hash for duplicate finder | 1 weekend | ✅ Yes, self-contained |
| v1.5.0 | 5 | Feature module architecture — split Core into per-feature assemblies, JSON-defined cleaners with winapp2.ini import, `IPolicyProvider` for enterprise policy | 1-2 weekends | ⚠️ Needs JSON schema spec |
| **v2.0.0 LTS** | 6 | Enterprise-ready endpoint — MSI (WiX) + MSIX + ADMX + JSON audit log (Splunk/Sentinel/ELK-ingestible) + Windows Event Log + code signing (SignPath.io) + LTS commitment (24-month backport) + tested-on matrix (Win10/11 + Server 2019/2022) + SOC 2/CIS-alignment docs | 3-4 weekends | ⚠️ Needs enterprise tooling deep-dive |
| v2.x | 7 | Server component + Autoruns-scale feature growth. **Deferred until 3-6 months of v2.0.0 traction data.** | 2-4 months | ⏸️ Deferred |

Waves 1 → 4 are ~1 month of focused solo work. Wave 6 is another 3-4 weekends. Wave 7 is a separate multi-month project.

---

## What we're explicitly refusing (and why)

- **No registry cleaner.** Community consensus 2026: harmful and useless. Microsoft explicitly discourages.
- **No PC Health Score.** CCleaner-era pseudo-metric marketing.
- **No auto-update by default.** Post-CCleaner audience specifically distrusts this.
- **No bundled software.** Ever.
- **No telemetry.** Ever.
- **No cloud requirement.** Wave 7 server is opt-in and self-hosted.
- **No cross-platform.** Windows-native IS the differentiator vs Kudu.
- **No own malware scanner.** VirusTotal is the moat.
- **No open-core paywall split.** Trust story broken.
- **No Avalonia migration.** Not now. Maybe post-v2.0.0 if binary size becomes a real business constraint.
- **No debloater as first-class feature.** Small curated Appx list under Settings > Privacy is fine.
- **No SaaS multi-tenant enterprise.**

The refusal list is the marketing message: *"SystemCleaner. No telemetry. No cloud. No auto-updates. No registry cleaner. No PC-health-score theatre."*

---

## The competitive landscape (why now?)

- **Kudu** (Electron + TypeScript, 2.1k stars, ~$5-9/device Kudu Cloud SaaS-only) is the direct broad-scope competitor.
- **FluentCleaner** (WinUI 3, MIT, uses winapp2.ini) — cleanup-only, Windows 11 only.
- **Chris Titus Winutil** (PowerShell + WPF, 30K+ stars) — debloat/tweaks/installs, different niche.
- **BleachBit** (Python + Qt, GPL) — reference safe cleaner, basic UI, no automation without CLI.
- **BCU** (C# + WPF, Apache 2.0) — reference OSS uninstaller with confidence-rated residuals.
- **CCleaner** — cultural distrust after 2017 supply-chain attack + Avast acquisition.

The gap SystemCleaner fills: **modern OSS Windows-native all-in-one for weak hardware with VirusTotal-in-workflow, plus enterprise-deployable via MSI/ADMX post-Wave 6.** Nobody occupies this space in 2026.

---

## The specific bugs blocking release

- **C1** — Registry hive parser bug at UninstallerService.cs:1398. Compares enum name against `HKEY_*` literals; never matches; defaults to HKLM. Every HKCU residual deletion targets HKLM instead. **Data-corruption risk.**
- **C2** — Substring matching over 4+ char tokens with `DeleteSubKeyTree` / `pnputil /uninstall /force` / `sc delete`. Uninstalling "Java" wipes anything with "java" in name. **Mass-deletion risk.**
- **C3** — Uninstall strings routed through `cmd.exe /c "..."` unsanitized. HKCU\Uninstall is user-writable → local privilege escalation when SystemCleaner is elevated (README instructs elevation). **EoP risk.**
- **H1** — VirusTotal API key placed in URL path in a fallback path. Secret exposure via proxy logs.
- **H2** — `HttpClient.DefaultRequestHeaders` mutated without sync during in-flight requests.
- **HM1** — DXGI reads `DedicatedVideoMemory` which is always 0 for integrated GPUs. All Intel HD/UHD/Iris laptops show 0 B VRAM currently.

Full backlog in PLAN.md §4.

---

## Measurements (i5-7300U + 8 GB + SSD, unelevated)

- `LHM.Computer.Open()` = **2 692 ms** — runs synchronously during startup, before window paints. On a Celeron, ~5-8 s frozen splash.
- `Update()` per tick = 2-4 ms. Polling is cheap; open is the elephant.
- Full HKLM\SOFTWARE walk = **28 220 ms** for one app's residual scan. Walking 10 apps at fleet cleanup = 5 minutes of one full CPU core with current code (no depth cap).
- User temp scan (749 files, 246 dirs, 760 MB) = 216 ms on SSD.
- Self-contained publish = 132 MB single file; framework-dependent = 5.7 MB with 22 files; ~2.8 MB of that is dead-weight Mono POSIX transitives.

---

## Open questions for reviewers

Any critique invited on the above. Specifically:

1. Is the "runs on weak hardware" positioning actually a real market differentiator, or a nice-to-have? Would users pay attention to it?
2. Is the VirusTotal-in-workflow moat as strong as claimed? Would it actually change user behaviour?
3. Is trying to also do enterprise (Wave 6-7) realistic while shipping consumer? Or does it fragment attention?
4. Which "explicit refusals" will get pressured by users later?
5. Where does the plan set up scope-creep or burnout failure modes?
6. Are the runtime measurements likely to generalise? Or are they i5-7300U-specific quirks?
7. Is Kudu really beatable, or is chasing them a losing race?
8. Is the 7-wave roadmap realistic for a solo dev, or wishful?
9. Which specific technical decisions (packages, patterns, architecture) look wrong from a 5-year horizon?
10. Are the "trust maximalism" claims (signed rules, offline-verifiable, audit log) actually deliverable at solo-dev capacity?

**Reviewers: please respond with structured critique, blunt not diplomatic. Where the plan is wrong or fragile, say so with reasoning. Where it's actually reasonable, don't waste words agreeing — focus on the weaknesses.**
