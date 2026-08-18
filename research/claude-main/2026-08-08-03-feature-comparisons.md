# SystemCleaner — Feature-by-Feature Comparison With The Field

For each feature, this document lists the incumbents (both open-source and dominant commercial), how their technique differs from SystemCleaner's, and where SystemCleaner sits on the plus/minus scale. Sources cited inline; nothing here is invented.

**Bottom-line pattern that recurs across all features:** the reference open-source implementations are more careful (confidence ratings, preview-before-act, exact-match matching, more autostart locations), and the reference commercial tools are more thorough (trace logs, MFT scanning, hundreds of autostart points). SystemCleaner sits in the middle on *scope* and near the bottom on *safety* — the fixes from the prior two reviews close most of the safety gap; the scope gap needs feature decisions.

---

## 1. Uninstaller + residual scanner

### Reference implementations

| Tool | Licence | Approach |
|---|---|---|
| **Bulk Crap Uninstaller (BCU)** | Apache 2.0, C#/.NET | Real-time install monitoring, plus post-scan for leftovers. **Assigns a confidence rating** ("Very good", "Good", "Questionable") to every leftover so users can safely bulk-remove only high-confidence matches. Uses junction-point analysis and Authenticode certificate verification to correlate files with the uninstalled program. |
| **Revo Uninstaller Pro** | Commercial | Two modes: (a) **traced install** — records every file/registry write during install so uninstall is a reliable revert; (b) **residual scan** with Safe/Moderate/Advanced modes based on name/install-ID/pattern matching. |
| **Geek Uninstaller / IObit Uninstaller** | Freemium | Registry inventory + naive name-based residual match, similar to what SystemCleaner does. |

### How SystemCleaner compares

**SystemCleaner does**
- Enumerate installed apps from the four standard `Uninstall` registry keys (matches BCU/Revo/Geek).
- Substring-match tokens ≥4 chars against paths, registry keys, services, drivers, scheduled tasks.
- Delete found "residuals" without an aggressiveness knob or confidence rating.
- Run uninstall commands via `cmd.exe /c "<UninstallString>"`.

**Plus**
- Fully integrated with the rest of the cleaner (single UI, single restore-point step).
- Covers the same set of residual categories as Revo Advanced.

**Minus**
- No confidence rating → user can't tell "this is definitely leftover" from "we found a substring match somewhere."
- No install tracing → will always be a step behind Revo on true completeness.
- No aggressiveness setting → the same aggressive match runs for every user (BCU exposes Cautious/Normal/Aggressive; SystemCleaner has one mode = aggressive).
- Substring vs exact match → BCU documents junction-point + Authenticode analysis to avoid false positives; SystemCleaner matches `"steam"` inside every path.
- Registry hive parsing bug means residual deletions target the wrong hive (C1 from the review).

### Recommendation
Steal BCU's **confidence rating pattern** — the fix for the substring-match blast radius is not "match less" but "surface the confidence and let the user act." Wire it into the residual review dialog. Trace logs are out of scope for a hobby project (they need a driver or hook), but confidence scoring costs a day.

Sources: [BCUninstaller.org](https://www.bcuninstaller.com/), [Revo Uninstaller Pro 5 manual](https://www.revouninstaller.com/wp-content/themes/revo/files/RevoUninstallerProUserManual.pdf).

---

## 2. Startup manager

### Reference implementations

| Tool | Licence | Approach |
|---|---|---|
| **Sysinternals Autoruns** | Microsoft freeware | Enumerates **all 200+ ASEPs** (Autostart Extensibility Points) that Windows exposes. Categories: Logon (Run/RunOnce/StartupFolder/Winlogon), Explorer (shell extensions, BHOs, toolbars), Scheduled Tasks with logon/boot triggers, Services with StartMode=Auto, Drivers, Codecs, Print Monitors, LSA providers, WMI. Uses SigCheck to Authenticode-verify each entry. |
| **Task Manager Startup tab** | Windows built-in | Only Run keys + Startup folder (a small subset). |
| **CCleaner Startup** | Freemium | Run keys, Startup folders, Scheduled Tasks, Services, Context menus. |

Real Autoruns findings on a normal laptop show ~172 autostart entries vs Task Manager's 11 — the delta is scheduled tasks, services, and shell extensions.

### How SystemCleaner compares

**SystemCleaner does**
- HKCU + HKLM Run + RunOnce (32/64-bit views).
- Current-User and All-Users Startup folder.
- StartupApproved binary payload toggle.

**Plus**
- Correctly handles the StartupApproved 12-byte binary payload — many hobby tools skip this and disagree with Task Manager's checkbox.
- Detects both 32-bit and 64-bit registry views.

**Minus**
- No Scheduled Tasks. Big blind spot: **every modern updater** (Chrome, Edge, Adobe, OneDrive, Discord) uses `\Microsoft\Windows\...\Update` scheduled tasks with a LogonTrigger or BootTrigger. SystemCleaner shows none of them.
- No auto-start services.
- No shell extensions, BHOs, print monitors, codecs.
- No signature verification. Autoruns colours unsigned/unknown-publisher entries yellow/pink; SystemCleaner treats a Chinese-authored `Software\Microsoft\Windows\CurrentVersion\Run` value the same as a Microsoft-signed one.
- RunOnce entries are shown and toggled the same as Run — but Windows evaporates RunOnce values on next boot regardless of the toggle (S4 from the audit).
- HKLM entries can be toggled without a pre-flight admin check → guaranteed failure toast for 4 out of 10 entries on this test machine (S1).

### Recommendation
Aim for a defined subset that closes the visible gaps: **Add Scheduled Tasks (logon/boot triggers) and auto-start Services first.** These two together bring you from Task Manager parity to CCleaner parity. Full Autoruns parity is a much bigger project (some ASEPs need shell-COM enumeration). Signature-verification via `WinVerifyTrust` is a mid-size add and worth doing for the trust story alone.

Sources: [Autoruns docs](https://learn.microsoft.com/en-us/sysinternals/downloads/autoruns), [Windows Forum: 11 startup apps hide 172 auto-start entries](https://windowsforum.com/threads/task-manager-vs-autoruns-why-11-startup-apps-can-hide-172-auto-start-entries.428783/).

---

## 3. Hardware monitoring

### Reference implementations

| Tool | Licence for embedding | Sensor coverage | Notes |
|---|---|---|---|
| **HWiNFO** | **Freeware for non-commercial only. Embedding requires paid SDK.** | Best-in-class. GPU: core temp + hotspot + GDDR memory temp + per-fan RPM + power + PCIe error counters. CPU: MSR + platform-specific paths for Ryzen SMU, Intel EMON. | Closed-source. Restrictive licence — you cannot ship its DLLs with your app without a signed agreement. |
| **LibreHardwareMonitor** | **MPL 2.0** — usable in closed and open, commercial and non-commercial. NuGet: `LibreHardwareMonitorLib`. | Solid. CPU, GPU (NVIDIA/AMD/Intel), memory, storage (SMART via WinRing0). Less coverage in memory sub-timings than HWiNFO. | Active fork of Open Hardware Monitor. Current 0.9.6 (Feb 2026). What SystemCleaner uses. |
| **Open Hardware Monitor** | MPL 2.0 | Same architecture as LHM but has been effectively dormant. | Deprecated; use LHM instead. |
| **HWMonitor (CPUID)** | Freeware, no library | GPU + CPU basics, less than HWiNFO. | Not embeddable at all. |

### The universal admin-privilege constraint

Every one of these tools reads CPU temperature and clock via **MSRs (Model-Specific Registers)**, which require ring-0 access. HWiNFO ships its own signed kernel driver; LHM/OHM use the WinRing0 driver. **All of them require Administrator to see CPU temps** — this is not an LHM shortcoming.

### How SystemCleaner compares

**SystemCleaner does**
- LHM 0.9.4 with everything enabled (CPU/GPU/Memory/Motherboard/Storage/Network/Controller).
- DXGI `DedicatedVideoMemory` for GPU VRAM (from PR #13).
- Custom mapping from LHM sensors to a `HardwareSnapshot` record consumed by the UI.

**Plus**
- Uses the best embeddable library. Choosing LHM over HWiNFO is the right call given HWiNFO's licence.
- MPL 2.0 lets you keep SystemCleaner under whatever licence you want.

**Minus**
- LHM 0.9.4 lags 0.9.6 (Feb 2026) — missing CPU support for newer Ryzen/Meteor Lake, and a WinRing0 signing bump that reduces AV false positives.
- `IsNetworkEnabled + IsControllerEnabled = true` inflates the sensor count massively (43 network hardware objects on this test laptop → ~200 sensor updates every 2 s) with no UI benefit (HM2).
- DXGI `DedicatedVideoMemory` returns 0 for integrated GPUs (HM1 — regression from PR #13). LHM itself already reads D3D shared memory correctly (~4 GB / 185 MB on this machine); the DXGI code is worse than the library it replaced.
- No differentiated user hint: `HasTelemetry` is `true` if any sensor is present, so the "run as admin" message never appears when CPU load works but temps don't (HM3).
- Timer callback re-entry — LHM's first `Update` can take 400‑800 ms; a 2 s periodic timer can queue up on slow systems (HM4).

### Recommendation
Don't switch libraries. **Upgrade LHM to 0.9.6**, replace SharpDX with `Vortice.DXGI` (SharpDX archived since 2019), and use `IDXGIAdapter3::QueryVideoMemoryInfo(DXGI_MEMORY_SEGMENT_GROUP_LOCAL)` — this returns real current usage for both dedicated and integrated GPUs and lands HM1's proper fix in one PR. Disable `IsNetworkEnabled + IsControllerEnabled`. Change the elevation hint to fire per-tile when the specific sensor is null and the process isn't elevated.

Sources: [LibreHardwareMonitorLib 0.9.6 on NuGet](https://www.nuget.org/packages/LibreHardwareMonitorLib/), [HWiNFO EULA](https://www.hwinfo.com/licenses/), [OpenHardwareMonitor.org licence page](https://openhardwaremonitor.org/license/), [Artemis RGB comparison](https://wiki.artemis-rgb.com/en/guides/user/plugins/hardware-monitoring), [SaaSHub HWiNFO vs LHM](https://www.saashub.com/compare-hwinfo-vs-libre-hardware-monitor).

---

## 4. Disk cleanup (temporary files, browser cache, etc.)

### Reference implementations

| Tool | Licence | Approach |
|---|---|---|
| **BleachBit** | GPL v3, Python + Qt | ~90 supported applications; **preview-first workflow** (Preview button lists every file and estimated bytes before Clean is enabled); granular checkboxes per cleaner; no registry mods, on-purpose. Whitelist per user; secure-delete option. |
| **Windows built-in Storage Sense** | Windows 10/11 | Modern API surface. Runs on a schedule, cleans Recycle Bin > N days, empties Downloads > N days (opt-in), makes OneDrive files online-only, purges Windows Update cache. Independent of `cleanmgr.exe`. |
| **Windows built-in Disk Cleanup (cleanmgr)** | Windows | Legacy but scriptable: `cleanmgr /sageset:N` opens the option picker and stores selection under `HKLM\...\VolumeCaches`, then `cleanmgr /sagerun:N` runs it unattended. Extensible via `VolumeCaches` handlers so third-party cleaners can register — this is how Storage Sense used to work. |
| **PrivaZer** | Freeware, closed | Privacy-focused: also wipes free space, USN journal, RTL fragments; heavy focus on making the deletion unrecoverable. |
| **CCleaner** | Freemium, closed | Historical leader. Registry cleaning (controversial, generally not recommended by MS). Custom rule files for apps. |

### How SystemCleaner compares

**SystemCleaner does**
- 5 modules: User Temp, Windows Temp, Internet Cache, Browser Cache (Edge/Chrome/Firefox), Diagnostic Data (crash dumps, WER).
- Preview-then-clean flow (you see paths and byte counts before pressing Clean).
- Restore point option before destructive operations.

**Plus**
- Preview-first is table stakes and SystemCleaner has it.
- Cross-checks against a static list of restricted roots (`C:\Windows`, `\WinSxS`, `\Installer`, `\WindowsApps`, etc.) — this is more than most CCleaner clones do.
- Skips reparse-point subdirectories in cleanup (though not in scan — X2 from the audit).

**Minus**
- ~5 supported categories vs BleachBit's ~90. No cleaners for VS Code cache, Discord cache, npm cache, pip cache, Docker layers, Windows Update download cache, Sysprep logs, DirectX shader cache — the big modern space eaters.
- Doesn't touch Chrome's `Code Cache`, `GPUCache`, `Service Worker/CacheStorage` (X1 from the audit) — modern Chrome puts most of its cache there.
- Downloads path is `%USERPROFILE%\Downloads` not the known-folder ID — misses relocated Downloads (X3 from the audit).
- No secure-delete option for cleanup targets (fine, this is intentional).
- No config-driven cleaner definitions — every module is hard-coded C#, so adding a new category requires a code change. BleachBit uses INI/XML-style cleaner files that non-devs can extend.

### Recommendation
Two directions to pick between:
- **Depth-first**: adopt a **JSON/YAML cleaner definition file** shipped alongside the exe. Each definition names paths, extensions, and safety flags. Users and contributors add cleaners without rebuilding. Copy the Chrome sub-cache list and one or two other high-impact ones as your seed set.
- **Integration-first**: instead of reimplementing what Windows already ships, expose a "**Run Storage Sense now**" button and a wrapper around `cleanmgr /sagerun:N`. Less code, always up-to-date with Windows.

The two aren't mutually exclusive; depth-first is where the differentiation is.

Sources: [BleachBit — HelloGitHub](https://hellogithub.com/en/repository/bleachbit/bleachbit), [Windows Forum: Storage Sense and Disk Cleanup](https://windowsforum.com/threads/free-disk-space-in-windows-11-with-disk-cleanup-and-storage-sense.385530), [thewindowsclub — cleanmgr Sageset](https://www.thewindowsclub.com/automate-disk-cleanup-utility-windows).

---

## 5. Large-file finder

### Reference implementations

| Tool | Licence | Speed | Technique |
|---|---|---|---|
| **WizTree** | Freeware (closed) | **~50× faster** than WinDirStat on NTFS. | **Reads the NTFS Master File Table (MFT) directly** — one raw read gives every file's size, timestamps, and cluster runs. Requires admin. On non-NTFS or network shares it falls back to `FindFirstFile` traversal. |
| **TreeSize Free/Pro** | Freeware / commercial | ~2–5× WinDirStat. | Optimised `FindFirstFile` walk, parallelised per subtree. Detailed reports, exports. |
| **WinDirStat** | GPL | Slowest. | Classic per-directory `FindFirstFile` walk. Famous treemap visualisation. |
| **SpaceSniffer / RidNacs / Scanner** | Various | Similar to WinDirStat. | Directory walk. |

### How SystemCleaner compares

**SystemCleaner does**
- Directory walk of `Downloads`, `Videos`, `Documents`, `Desktop`. Per-folder minimum size threshold (250 MB / 500 MB / 200 MB). Cap of 50/40/40/30 results.

**Plus**
- Focused on the most likely user-facing space hogs (Downloads/Videos), not a full disk survey.
- Delete integrated with the cleanup service.

**Minus**
- Directory walk is the slowest possible approach on NTFS.
- Only scans the four folders; nothing for `%ProgramData%`, Steam library, node_modules, virtualbox VMs, iTunes/Photos libraries — the real GBs.
- No treemap or visual view — you get a list.
- The minimum-size thresholds are arbitrary. A user with lots of 100 MB videos in Downloads sees nothing.

### Recommendation
If you want to actually find space, you have to scan the whole drive. **MFT scanning is a discrete, well-scoped feature** — the WinFsp/NtQuery route is doable in ~400 lines of C#. But you'd want admin. Easier win: **scan the full user profile** (not just 4 folders) with a walk, expose a size threshold slider, and add a treemap or size-bar visualisation. Full-disk MFT scan is a "later" feature.

Sources: [WizTree vs WinDirStat vs TreeSize (Zenovix)](https://zenovix.app/blog/wiztree-vs-treesize-vs-windirstat/), [Windows Forum: WizTree MFT scan](https://windowsforum.com/threads/wiztree-fast-ntfs-disk-scan-to-reveal-hidden-ssd-space-hoggers-in-minutes.386938/), [Hacker News thread — WizTree 50× faster](https://news.ycombinator.com/item?id=40451333).

---

## 6. Duplicate finder

### Reference implementations

| Tool | Licence | Algorithm |
|---|---|---|
| **dupeGuru** | GPL 3 (Python/Qt) | Group files by exact size → per-group SHA-1/MD5 → equal hashes = duplicates. Three modes: **Standard** (byte-exact), **Music** (audio metadata + fingerprint), **Picture** (perceptual hash for visually similar images regardless of resize/re-encode). |
| **AllDup** | Freeware, closed | Byte-content comparison; multi-property filters (name, ext, date, attributes). |
| **Duplicate Cleaner Pro** | Commercial | Same principle as dupeGuru + specialised music/image similarity modes. |

### How SystemCleaner compares

**SystemCleaner does**
- Bucket by exact size → per-group SHA-256 → equal hashes = duplicates.
- Scans `Downloads`, `Documents`, `Pictures`, `Videos` with per-folder minimum-size threshold (20 MB / 15 MB / 10 MB / 20 MB).

**Plus**
- Uses **SHA-256** rather than MD5 (dupeGuru default). Slightly slower per file but no realistic collision risk. Correct choice.
- Size-first bucketing avoids hashing files that are trivially different.

**Minus**
- Only exact-content match. No fuzzy/perceptual modes → user asking "find duplicate photos regardless of resize" is not served.
- **Recurses into `node_modules`, `.git`, `venv`, `bin/`, `obj/`** — dev machines with any git repo in Downloads/Documents get thousands of intended "duplicates" (X2 from the audit). dupeGuru doesn't skip these either, but SystemCleaner is silent about it.
- **Whole-file SHA-256 is over-engineered for most cases.** dupeGuru and Duplicate Cleaner Pro read the first 4 KB or 64 KB and only hash the full file when prefixes match. On a 4 GB video, this is a 100000× speedup for the miss case.
- The "originals not preserved" warning in the module info is honest but the UX gives no way to distinguish which copy is the "original" — dupeGuru shows creation date, folder depth, and lets you set a per-scan rule ("prefer files in folder X as originals").

### Recommendation
Adopt dupeGuru's **prefix-then-full hash** optimisation as the first change — it's ~50 lines and pays back on any dataset with a lot of same-size but different files. Add a well-known noise skip list (`node_modules`, `.git`, `venv`, `target`, `bin`, `obj`, `__pycache__`) exposed as an editable setting. Perceptual image hashing is a whole separate library dependency — probably not worth pulling in unless you specifically want photo-library dedupe.

Sources: [dupeGuru official site](https://dupeguru.org/), [dupeGuru FAQ](https://dupeguru.voltaicideas.net/help/en/faq.html), [dupeGuru RapidSeedBox writeup](https://www.rapidseedbox.com/blog/dupeguru-guide).

---

## 7. System information

### Reference implementations

- **Speccy** (Piriform) — comprehensive, one-page report, free.
- **CPU-Z / GPU-Z** — CPU/GPU details only.
- **HWiNFO** — mixes system info with monitoring.
- **Windows Settings → System → About + msinfo32** — built-in, no library.

### How SystemCleaner compares

**Plus**
- Combines OS build, Windows edition, uptime, BIOS version, RAM, CPU (via WMI), GPU (via WMI + DXGI), storage devices, logical drives. That's a good breadth for a cleanup app's side pane.
- Correctly resolves Windows edition/build via `HKLM\SOFTWARE\Microsoft\Windows NT\CurrentVersion` (I verified this works despite the double-backslash string looking wrong — Windows tolerates it).

**Minus**
- DXGI VRAM issue (HM1) shows through here too — the `GraphicsAdapters` collection shows `0 B` for integrated-only laptops.
- On multi-GPU machines (dGPU + iGPU laptops), name-matching between WMI's `Win32_VideoController.Name` and DXGI's `Description1.Description` uses substring match; matches fail commonly and the code falls back to `dxgiAdapters[0]` which may be the wrong one.

### Recommendation
Small clean-up rather than a redesign. Fix HM1 and the multi-GPU fallback. Consider showing a "Copy full report" button — this is a common Speccy feature and it's ~5 lines of code.

Sources: [Speccy](https://www.ccleaner.com/speccy) — commercial, no comparison specifically searched.

---

## 8. VirusTotal integration

### Reference implementations

- **VirusTotal Uploader (official, discontinued)** — used to ship as a standalone desktop app; VT retired it in favour of the web UI.
- **VTHash / vt-cli** — official CLI. Uses same v3 API SystemCleaner uses.
- **PeaZip / 7-Zip context menu integration** — right-click → "Scan with VT" opens the browser to the file's page after hashing locally.

### How SystemCleaner compares

**Plus**
- Uses the correct v3 endpoints (`/files`, `/urls`, `/analyses`, `/files/{sha256}`).
- **Hashes locally first and looks up by hash before uploading** — this is exactly what VT recommends and what all serious integrations do. Saves upload bandwidth, respects user privacy (files never leave the machine if VT already has them).
- Uses DPAPI to store the API key.
- Implements rate limiting client-side (4 requests / 60 s).

**Minus**
- Falls back to putting the API key in the URL path when the group lookup fails (H1 from the review) — this is a genuine secret-exposure defect and neither vt-cli nor any other reference implementation does this.
- Rate limit is hardcoded to VT's free-tier limits — no override for paid keys.
- Header mutation on shared HttpClient without synchronisation (H2 from the review).

### Recommendation
Fix H1 and H2, everything else is fine. This is one of the better-implemented parts of the app already.

Sources: [VirusTotal API v3 docs](https://docs.virustotal.com/reference/overview) — general knowledge, not specifically searched.

---

## Summary: where SystemCleaner has room to move

| Feature | Position today | Cheapest single move to close the gap |
|---|---|---|
| Uninstaller | Naive substring match, no confidence rating | Adopt BCU's **confidence rating pattern** in the residual review dialog. |
| Startup manager | Task Manager parity minus a few bugs | Add **Scheduled Tasks + Auto services** → CCleaner parity. |
| Hardware monitoring | LHM done right-ish + a regression in the DXGI PR | Upgrade LHM 0.9.4 → 0.9.6, replace SharpDX with **Vortice.DXGI**, use `QueryVideoMemoryInfo`. |
| Disk cleanup | 5 hard-coded modules, missing modern caches | Move cleaner definitions into a **JSON file** and seed with Chrome/VS Code/Discord/npm/pip. Optionally add a Storage Sense trigger. |
| Large-file finder | Directory walk on 4 folders | Full user-profile walk with a threshold slider; add a treemap. MFT scan is a "later" feature. |
| Duplicate finder | Whole-file SHA-256, no skip list | **Prefix hash → full hash** optimisation + skip list for `node_modules` etc. |
| System info | Broad but not deep | Fix HM1 downstream + add "Copy full report." |
| VirusTotal | Better than most hobby integrations | Just fix H1 and H2. |

---

## Where to move next

You could pick two paths:

**Path A — safety and correctness first (recommended).** Ship the fixes from the two prior reviews (C1/C2/C3 + HM1 + S1‑S5). Do not add features. After this pass SystemCleaner is a *safe* tool that covers roughly what Task Manager + basic CCleaner covers. Time estimate: two focused weekends.

**Path B — pick one feature to become best-in-class.** Choose one of: Uninstaller confidence rating (BCU pattern), Startup Manager Autoruns-level enumeration, or JSON-defined cleaner modules. Ship it. Now SystemCleaner has a story ("The cleaner with the best startup manager", or similar) instead of being a mid-tier all-in-one.

Path A is a prerequisite either way — you don't want to layer new features on top of C1/C2. When you've decided which of these you want to tackle first, I'll switch into brainstorming mode on that one feature and turn it into a spec.
