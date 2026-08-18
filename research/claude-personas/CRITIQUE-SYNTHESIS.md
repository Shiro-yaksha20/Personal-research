# SystemCleaner Plan — Aggregated Critique

Four independent critique passes, each with a distinct persona lens, each starting cold with no prior context on our research sessions. **Same model family (Claude), but genuinely independent context** — they read PLAN.md and PLAN-SUMMARY.md fresh and came back with their read.

The critiques are consistent enough that the pattern is real signal, not agent hallucination. Where multiple lenses independently flagged the same thing, that's the highest-confidence weakness.

---

## Where all four converged (highest-confidence problems)

### 1. Wave 6-7 timeline is fantasy — cut it, don't sequence it

All four called this out with specifics:

- **Engineer:** WiX 4 rewrite alone is a weekend. ADMX authoring + testing in a real AD lab you don't have is another. SignPath OSS onboarding is 2-6 weeks calendar. "Tested on Server 2019/2022" needs VMs you likely don't own. **Estimated off by 2-3×.**
- **Product strategist:** "Wave 6 alone is a 6-month full-time job at a real ISV. Weekends means 18 months minimum, during which Kudu ships ~400 releases."
- **IT director:** "Wave 6 as scoped is a competent single-endpoint enterprise install story. It is not an enterprise fleet product."
- **OSS maintainer:** "When Wave 6 slips from 4 weekends to 4 months, that's the point where the plan quietly dies at v1.5." Wave 7 called "the fatal distraction, not a deferred side quest — this is a second product, not a wave."

**OSS maintainer's wave-completion probability:** Wave 1 (95%), 2 (85%), 3 (70%), 4 (60%), 5 (30%), 6 (10%), 7 (2%). **Honest cutoff: end of Wave 4.**

### 2. Windows 10 first-class positioning is already stale

- **Engineer:** "Win10 EoL was Oct 14 2025 — you're 10 months past it in Aug 2026. Marketing 'runs on Windows 10' reads as 'runs on unsupported OS' to enterprise buyers by Wave 6."
- **Product strategist:** "By v2.0.0 ship, Win10 will be 12-18 months past EOL. The Venn of 'cares about VirusTotal integration' and 'still on unsupported Win10' is roughly empty."

Both suggest: pivot to "runs on modest Windows 11 hardware." This also weakens the anti-Avalonia argument — reconsider Avalonia post-v2.0.0.

### 3. Trust-maximalism claims are aspirational, not deliverable

- **Engineer:** "Ed25519 signing = key custody, rotation, revocation, distribution — you become a mini-CA. Sigstore keyless signing depends on Rekor (online), so 'offline-verifiable' is a contradiction unless you snapshot Rekor entries with each release."
- **OSS maintainer:** "Each of these is a permanent tax on every future change. Solo maintainer + no telemetry = every bug report is a 45-minute back-and-forth. Signed rules mean you can't accept a PR without a key ceremony."

Both recommend: SHA-256 checksums + GPG-signed release tags for v2.0. Defer Ed25519 signed rules until post-v2.0 with a documented trust root ceremony. Add opt-in crash reporting (Sentry OSS) so you can debug without polling users.

### 4. Enterprise angle is unrealistic without an actual company

- **Product strategist:** "Free is a consumer marketing lever, not a B2B one. IT teams pay Kudu the $5 *precisely to avoid* self-hosting risk."
- **IT director:** "A tool with `pnputil /uninstall` in the code path is a non-starter for procurement without: contractual SLA, E&O insurance certificate, indemnification clause, a real corporate entity, and a DPA for the VirusTotal path."
- **OSS maintainer:** "Delete Wave 7. If a customer someday pays for it, revisit. Do not build server infrastructure on spec."

**Enterprise path requires business infrastructure that isn't in scope for a solo dev.**

### 5. Research addiction is the biggest failure mode right now

- **OSS maintainer** (most direct): "12 briefs, ~60 KB canonical plan, motion/stability meta-check documents, `STATUS.md` for the plan itself — and by your own admission, zero code changes. Planning gives dopamine at zero cost. `dotnet build` gives friction. I've watched more solo projects die in 'just one more brief' than in code."
- Recommendation: "Ship Wave 1 this weekend. Not 'prepare to ship' — merge and tag v1.0.1. The plan can survive that. It can't survive another three planning sessions."

---

## Where two-of-four converged (medium-confidence problems)

### 6. VirusTotal moat may be weaker than positioned

- **Product strategist:** "Users uninstall apps they already installed and used. The trust decision happened months ago. Pausing to look at a VT verdict at uninstall time is behaviourally backwards — it's like checking a smoke alarm on the way out of a burning house. VT integration at *download* or *first-run* time is the actual valuable workflow."
- **IT director:** "Corporate data going to VirusTotal Public? That's a data-exfiltration incident waiting to happen. Would you deploy this feature at all? Or is it a mandatory-off?"

The consumer strategist's insight — VT-at-download > VT-at-uninstall — is worth taking seriously. The enterprise reality — VT needs to be disable-by-GPO — is a specific policy knob we hadn't planned.

### 7. Windows Server support is a checklist item unless there's a real use case

- **IT director:** "Nobody runs cleanup utilities on Server 2019/2022 production hosts. If the use case is RDSH user-profile cleanup or Citrix golden-image prep, say that explicitly and test that specific path. Otherwise dropping 'tested on Server' reads as 'we ran the MSI and it didn't crash.'"
- **Engineer's implicit critique:** "tested-on matrix across 7 Windows SKUs implies VMs and hardware you likely don't own."

Recommendation: drop Server from tested-on unless there's a documented use case (RDSH user-profile cleanup would be one).

---

## Individual critiques worth reading carefully

### From the senior engineer (technical gaps we missed)

**C3 fix is incomplete.** Tokenizing UninstallString via `CommandLineToArgvW` defeats shell metacharacter injection but not the underlying EoP threat. HKCU\Uninstall is user-writable → a tokenized `ArgumentList` still launches whatever *filename* the low-priv user planted in `UninstallString`. Real fix: refuse to execute uninstall strings sourced from HKCU when process is elevated, or drop-token/`CreateProcessAsUser` back to the interactive user, or verify the target file is signed and located in a non-user-writable directory before launching. Plus surface the parsed command to the user before running.

**HM1 fix is likely wrong.** `QueryVideoMemoryInfo(DXGI_MEMORY_SEGMENT_GROUP_LOCAL)` still returns near-zero for Intel HD/UHD/Iris — because integrated GPUs *don't have local memory*. Local budget is the BIOS UMA reservation, typically 64-128 MB. The real number users see in Task Manager is `NonLocal` (shared system RAM). **The plan misdiagnoses HM1.** Vortice won't magically produce a "correct" VRAM number. Correct fix: report `NonLocal` for integrated adapters (detect via `DXGI_ADAPTER_FLAG_SOFTWARE`+vendor id or `AdapterLuid` matching Task Manager's Perf tab), and label the UI "Shared VRAM."

**LHM 0.9.4 → 0.9.6 does nothing for the 2.7s Open cost.** The Open cost is WMI + kernel driver probes; version bump doesn't touch that. Don't sell it as a perf win; ship as compat/AV-false-positive only. Real cold-start improvement is Lazy DI.

**Wave 5 feature-module split is premature-architecture theater.** "Assembly-per-feature adds `InternalsVisibleTo` juggling, obstructs trimming, forces DTO duplication in `Core.Abstractions`, and doubles cold-start cost (assembly-load I/O on HDDs — the very hardware you're optimising for). Keep single Core assembly. Extract only `SystemCleaner.ShellExtension` (COM host with different bitness/hosting concerns). Do the module split when a *second contributor* appears."

**HKLM 28s / 352k keys measurement is likely a dev-machine artifact.** On i5-7300U (dev workstation with Visual Studio + Office installed), most of those keys are HKLM\SOFTWARE\Classes (CLSID + Interface + AppID + TypeLib). "On a clean Win11 Home laptop that walk is 4-8 s, not 28 s." Cache-the-walk strategy may not scale the way the plan claims. **Recommendation: root allowlist + depth cap 4, not "walk everything and cache." Measure on three real user machines before publishing the perf story.**

`RegNotifyChangeKeyValue` "1-2 days" is still optimistic — "single-shot notification (must re-register after every signal), WOW6432Node redirection = two watchers per logical location, key deletion invalidates handles asymmetrically." Budget one weekend for wrapper + tests, and start with polling as fallback.

### From the product strategist (positioning gaps)

**The refusal list is not positioning; it's a negative-space rant.** "A list of five nots tells a normal user nothing about the outcome. It reads as internal ideology leaking into copy. Test: read the tagline to a non-technical Windows user and ask what the product does — they will not be able to answer." Missing: one benefit sentence with a subject and a verb.

**"Runs on weak hardware" is a developer's aesthetic, not a purchase driver.** "No forum post you can point to says 'I picked utility X because it runs on my old laptop.' They say 'it's fast' or 'it's not bloated.'" Missing: a measurable claim consumers understand ("cold-start under 1 s, idle RAM under 40 MB") not an audience segment.

**BleachBit ceiling check.** "How many stars does BleachBit have after 15 years serving this exact audience? ~2k. That's the ceiling."

**Trust-maximalism 2018-era signals.** "The post-CCleaner-supply-chain audience is now 8 years older; today's Windows utility users default-trust Winget, Store apps, and signed MSIs with auto-update. 'No auto-update' reads as 'I'll forget to patch your CVE.' Registry cleaners died in the market a decade ago — refusing to build one is table stakes nobody notices."

**Missing modern threat model.** "In 2026 the trust signals that matter are reproducible builds, signed releases, an SBOM, and a public vulnerability response — not a list of features from the CCleaner era."

**"Beat Kudu for a different audience" is rationalisation for feature deficit.** "Kudu ships weekly by a funded team. This plan ships weekends by one person. Calling that 'different audience' is what every losing OSS project says right before the star count plateaus."

**Bottom line:** "The plan is a well-engineered answer to a product question nobody asked. The technical work (C1/C2/C3, Lazy DI, HKLM cache) is genuinely valuable and should ship. The positioning, moat, enterprise story, and roadmap past Wave 3 should be deleted and re-derived from an actual user, not from a grievance list against CCleaner and Kudu."

### From the IT director (specific pilot blockers)

Non-negotiable requirements for a real 25-machine pilot:

1. Named co-maintainer or escrow, incident-response contact that isn't @gmail.com
2. C1/C2/C3 fixes verified by external code audit
3. Interim SCCM/Intune story for fleet visibility, or don't call v2.0.0 "enterprise-ready"
4. Documented audit-log JSON schema (versioned), sample events for every event type, signed Sentinel/Splunk parser, CIM/OCSF mapping, rotation/backpressure behavior when disk fills
5. Windows Event Log source registration at MSI install, not first run
6. Reproducible builds (source → signed MSI verifiable), MSI transform (MST) support for per-org config, silent-install exit codes documented, Intune Win32 app packaging validated
7. Incorporated entity, MSA/DPA templates, E&O insurance certificate
8. Vendor Security Questionnaire response (CAIQ or SIG-Lite), CIS Benchmark mapping (not "alignment docs"), SBOM, vulnerability-disclosure policy with response SLAs
9. **Missing ADMX knobs that are non-negotiable:** allowed-paths allowlist (not just deny), approved cleaner-rule signature pinning, require-restore-point-before-batch enforcement, disable-headless-CLI, require-elevation-confirmation, log-forwarding endpoint URL, proxy configuration for VT, **kill-switch registry key**
10. **VirusTotal Public is mandatory-off via GPO** — ADMX must enforce disable-VT-entirely, hash-only-never-upload, enterprise API key with private submission
11. MSI upgrade codes stable across versions, machine-readable version manifest for WSUS/Intune/Datto, signed changelog feed

**Bottom line:** "Wave 6 as scoped is a competent single-endpoint enterprise install story. It is not an enterprise fleet product. I'd pilot 25 machines under a signed contract with the entity, E&O in place, VT disabled by GPO, C1/C2/C3 verified fixed by an external code audit, and a clear roadmap to Wave 7 fleet visibility. Without any one of those five, it doesn't clear my desk."

### From the OSS maintainer (sustainability lens)

**The prediction ranking for "which refusals will crack":**

1. **Auto-update** — you already softened it to "opt-in," which is the first foot in the door. Six months in, someone will file "no auto-update = users stuck on vulnerable versions" and you'll cave.
2. Registry cleaner — safest refusal, will hold.
3. macOS — safe.
4. **Tray icon** — already caved (Wave 4/5).
5. **Telemetry** — will crack the day you can't reproduce a user's crash.

**"For every additive item, delete one item from the roadmap. Right now the ledger is unbalanced."**

**"Architect for your future self after a 3-month break, not for phantom contributors."**

**Your next five years, if executed as written:** "Every weekend through mid-2027 on the endpoint. Then 2-4 months of evenings + weekends on the server. LTS backport commitment means you're patching v2.0.0 through 2029. You've committed to being the sole maintainer of a Windows utility *and* an enterprise server *and* a fleet-management product *and* SOC 2 documentation — while running unpaid. There is no room in this plan for a partner, a health event, a job change, a different hobby, or a bad month. That's not sustainability; that's a treadmill you're pre-loading."

---

## Synthesis: what to actually change in the plan

Grouped by confidence and effort:

### Change immediately (all critics agree)

- **Cut Waves 6 and 7 from the committed roadmap.** Move to "considered future work." The honest cutoff is end of Wave 4.
- **Retire "Windows 10 first-class" as headline positioning.** Reframe as "runs on modest Windows 11 hardware." Reopens the Avalonia question post-v2.0.0.
- **Downgrade trust-maximalism claims.** SHA-256 + GPG-signed tags for v2.0.0. Ed25519 signed rules → future work with documented ceremony. "Offline-verifiable builds" → drop until you've built a verifier.
- **Ship Wave 1 THIS weekend.** Stop planning. The research addiction pattern is the biggest current risk.

### Fix in the plan document

- **Positioning tagline needs a positive claim.** "Uninstall apps and see exactly what they leave behind — verified against VirusTotal — without leaving one lightweight window" is a subject-verb-object sentence. Refusals move to About/values page.
- **HM1 diagnosis is wrong.** Report `NonLocal` for integrated adapters, label UI "Shared VRAM." Rewrite this section.
- **C3 fix needs a second layer.** Refuse-to-execute-HKCU-uninstall-strings-when-elevated OR run in the interactive user context via `CreateProcessAsUser`. Tokenising alone isn't enough.
- **HKLM walk measurement disclaimer.** "Measured on developer machine; real user machines vary; strategy depends on measuring on three clean user machines before Wave 4 sequencing."
- **VT strategy needs a rethink.** VT-at-download > VT-at-uninstall behaviourally. Add downloads-folder-watcher OR reframe the moat as "the general utility with any VT integration at all."
- **Windows Server support drops from tested-on** unless a specific RDSH/FSLogix use case is documented.
- **ADMX policy scope expands** to include the 8 additional non-negotiable knobs from the IT director list (allowlist, signature pinning, kill-switch, etc.).

### Not a plan change, but a personal/business change if enterprise stays in scope

- Incorporating an LLC before v2.0.0 ships if enterprise sales is a real goal
- E&O insurance quote
- MSA/DPA legal templates
- Vulnerability-disclosure policy published

If none of those are on the table, **drop enterprise entirely from the roadmap**. The IT director's critique is unambiguous — "SOC 2-aligned documentation" is not enough for procurement to approve at 500+ endpoints. Wave 6 is not enterprise-ready unless it has a company behind it.

---

## What survived the critique

Not everything got hit. What all four critics either endorsed or didn't challenge:

- **Safety fixes (C1/C2/C3)** are genuinely valuable and should ship. The engineer refined C3 but agreed the direction is right.
- **Runtime lightweight (Lazy DI, HW monitor suspend on minimise, HDD detection, prefix hash)** is technically sound. The measurements need re-measuring on other machines, but the *direction* holds.
- **The safety+performance Waves 1-4 as a real product v1.4.0.** All critics agreed this is a coherent 5-6 weekend shipping goal.
- **Windows-native WPF (not Avalonia yet)** was not challenged for the Wave 1-4 horizon. Only questioned post-v2.0.0 if Win10 stops being an argument.
- **VirusTotal integration existing** — questioned as a *moat*, but not as a *feature worth having*.
- **Test scaffolding as Wave 2 prerequisite** — the engineer questioned the count (30+ tests in a weekend is fantasy) but agreed the pattern is right.
- **Killing SharpDX for Vortice.DXGI** — no critic disagreed. Archived package replaced with active. Simple.

---

## My honest read (Claude, not the critique agents)

The critiques are consistent and specific enough that I trust the pattern. They agree with each other more than they disagree with the plan.

**The plan I authored was better on the "what to build" question than the "why anyone will use it" question.** The technical direction survived the critique largely intact; the positioning and enterprise story did not.

**Three specific things I got wrong:**

1. **I overweighted "Windows 10 forever" as positioning.** By late 2026 it's a shrinking market that positions us against enterprise buyers, not for them.
2. **I underestimated the operational cost of the trust-maximalism claims** (signed rules, offline-verifiable). Those are multi-month engineering projects, not README bullet points.
3. **I sequenced Wave 6-7 like it was a natural continuation** when it's structurally a different product requiring a business entity.

**The OSS maintainer's "research addiction" call is the sharpest single critique.** 12 briefs + 0 code changes is a pattern that predicts abandonment better than any technical detail. **Ship Wave 1 this weekend** is the correct concrete response.

**What I recommend:**

1. Accept the critique on Wave 6-7 — move them to "considered future work," commit only to Waves 1-4 in the roadmap.
2. Accept the positioning critique — rewrite the tagline as a positive claim.
3. Accept the HM1 correction and re-fix.
4. Accept the C3 additional layer.
5. Re-measure the HKLM walk on 2-3 real user machines before publishing perf claims.
6. Consider adding downloads-folder-watcher VT integration alongside (or instead of) VT-at-uninstall.
7. Ship Wave 1. Actually ship it. This weekend if possible.

That's the honest position after four independent critiques.
