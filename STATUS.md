# SystemCleaner — Status, Motion, Stability

Meta-check. Not another research brief.

---

## Where we stand

### What we've produced (all in this session directory)

| Doc | Size | Purpose |
|---|---:|---|
| **system-cleaner-review.md** | 17 KB | Initial code review — first pass on safety bugs, package drift, thin tests. Where C1/C2/C3 came from. |
| **system-cleaner-audit-2.md** | 19 KB | Extended audit with runtime probe. Confirmed C1 against your actual apps. Identified HM1 (integrated-GPU 0-VRAM), S1-S6 startup bugs, HM2-HM4 monitor perf issues. |
| **system-cleaner-comparisons.md** | 24 KB | Feature-by-feature comparison with BCU (uninstaller), Autoruns (startup), HWiNFO (monitor), BleachBit (cleanup), WizTree (large files), dupeGuru (duplicates). |
| **system-cleaner-lightweight-brief.md** | 16 KB | Binary-size interpretation of "lightweight" — 132 MB self-contained, three approaches (WPF stay / FDD / Avalonia). Superseded by runtime-brief once you clarified. |
| **system-cleaner-runtime-brief.md** | 18 KB | Runtime-cost interpretation — LHM Open 2.7 s, residual walk 28 s. Levers: lazy init, event-driven, adapt to machine. This is the operative lightweight document. |
| **system-cleaner-downsides.md** | 17 KB | Honest costs of every recommendation. Where I acknowledged the "seems less thorough" perception on C2 rewrite. |
| **system-cleaner-differentiation.md** | 12 KB | Retraction of "yet another CCleaner clone" line. Landscape: Kudu / FluentCleaner / Winutil / Wintoys / BleachBit / BCU. VirusTotal moat validated. |
| **system-cleaner-vs-kudu.md** | 15 KB | Kudu deep-dive: Electron/TypeScript, 2.1k stars, JSON cleaners already done. 8 axes of "way different." Three that stack. |
| **system-cleaner-enterprise.md** | 18 KB | Kudu Cloud is $5-9/device SaaS-only. On-prem/self-hosted is unmet. Three-phase build (endpoint → server → sustainability). Fleet (fleetdm.com) exists in adjacent MDM category. |
| **system-cleaner-stack-and-roadmap.md** | 21 KB | Stack today (measured) → stack target. 7-wave sequenced plan → v2.0.0 LTS. What we explicitly don't do. |
| **system-cleaner-voice-of-user.md** | 20 KB | Reddit / XDA / HowToGeek / MakeUseOf / Steam / DEV.to research. CCleaner scandal is still cultural memory. "Do I even need a cleaner?" is the default question. Refusal list does marketing work. |
| **STATUS.md** | this | Meta-check. |
| perf-output.txt, probe-output.txt | 60 KB | Raw output from the runtime probe I ran against your machine — the source of every measured number in the briefs. |

**Total: 12 briefs, ~220 KB of writing, plus ~60 KB of measured data.**

That's real work. Not-code work, but real work.

---

## Is our motion stable?

Honest answer: **yes.** Let me show why.

### What has stayed constant across every brief

Nothing here has been contradicted or reversed in later documents:

1. **C1 / C2 / C3 are the top safety priority.** Every subsequent brief has assumed these get fixed first. Never re-litigated.
2. **Windows-native WPF (not Avalonia, not Electron, not WinUI 3).** Confirmed after the Kudu comparison. Ruled out again in the enterprise brief.
3. **LibreHardwareMonitor + Vortice.DXGI as the hardware stack.** From the initial audit through the roadmap.
4. **Weak hardware as a target audience.** Confirmed by voice-of-user (Reddit debloat threads, "runs on my Celeron"), reinforced by Kudu comparison (Electron can't compete on this axis).
5. **Deep few features > shallow many.** Consistent from lightweight brief through Kudu comparison.
6. **Trust-first posture** (no telemetry, no auto-update, open source top to bottom, explicit refusal list). Voice-of-user validated this louder than anything else in the research.
7. **VirusTotal integration is the unique moat.** Established in differentiation brief, reinforced in Kudu comparison and voice-of-user (VT shell extensions have proven demand nobody has bundled).
8. **Feature modules architecture** as a prerequisite for scale. Consistent from lightweight through enterprise.
9. **JSON-defined cleaners + winapp2.ini import.** Consistent since the comparison brief.
10. **7-wave roadmap to v2.0.0 LTS.** Voice-of-user added tray widget + shell extension to Waves 4-5 without rewriting the wave structure.

### What has evolved — but not contradicted

Three shifts that were refinements, not reversals:

- **Positioning** — I started with "yet another CCleaner clone" (wrong; you called it out). Corrected to "VirusTotal + workflow + weak hardware" (validated by voice-of-user).
- **"Lightweight" scope** — I initially interpreted as binary size (Avalonia+NativeAOT). You clarified: runtime cost on weak hardware. Approach shifted; conclusions didn't (still Windows-native, still WPF).
- **Enterprise scope** — Consumer-only in the first briefs; you signalled interest in the Kudu Cloud territory; expanded to Wave 6 (endpoint-ready) + optional Wave 7 (server). Not a reversal; an addition with clear phasing.

### What has been added by later research

Voice-of-user added three concrete design decisions to the plan without invalidating anything earlier:

- **Ship the VT shell extension in v1.x, not v2** (Wave 3 bonus deliverable, 1-day work, high visibility)
- **Tray widget for hardware monitor** (Wave 4-5, users want glanceable-not-window)
- **Explicit refusal list in README** as marketing (no registry cleaner, no health score, no bundling — refusals do the positioning work)

None of these break the roadmap. All three slot in cleanly.

**Motion is stable.** Direction has clarified; conclusions have converged; no earlier decisions have needed reversal.

---

## What "motion" actually means from here

Three honest ways forward. They're not mutually exclusive but they're mutually informative:

### Option A — Keep researching until every question is answered

Remaining research topics from earlier:
- **UX / UI redesign** — wireframes for the workflow-first uninstall + VT + refusal-list positioning
- **Deep specs per feature** — full data model + algorithm + edge cases + tests for each Wave 3-6 feature
- **Enterprise deep-dive** — WiX Toolset choice, ADMX authoring detail, Event Log source registration, SignPath.io process, SOC 2 documentation templates

**Pros:** clearer plan, fewer surprises during execution.  
**Cons:** we've been researching for a while. There's diminishing return past a point. Additional research doesn't ship anything.

### Option B — Consolidate what we have into ONE canonical planning document

Take the 12 briefs, merge them into one document that's the single-source-of-truth for the plan. **Fewer files, easier to navigate, no cross-brief drift.**

**Pros:** discoverable. Future-you (or a contributor) reads one thing. Removes the "which brief said what?" problem.  
**Cons:** effort, doesn't add new information, doesn't ship code.

### Option C — Start executing Wave 1

The zero-downside cleanup list (10 items, one PR). We know exactly what to do. No decisions left to make. **Ships something concrete.**

**Pros:** forward motion, real progress, unblocks Waves 2-3, produces a first release note.  
**Cons:** you said "we aren't going to start anything yet." If that's still the constraint, this is off the table.

---

## My honest read

The research is close to saturated. Not fully — the UX brief would materially change some Wave 5-6 details, and the enterprise deep-dive would materially inform Wave 6 tooling choices. But most of the remaining research topics are refinements, not blockers.

**We have enough to execute Wave 1 today with zero regret.** The 10 items in Wave 1 are grounded in every subsequent brief and none of the remaining research would change any of them. If Wave 1 is off the table due to your "no execution yet" instruction, that's fine — but be aware that the reason to wait is *your call to wait*, not *a real blocker in the plan.*

**We have enough to execute Wave 2 (test scaffolding) with high confidence.** The tests we need are known from the audit; adding UX or enterprise research doesn't change which tests to write.

**Wave 3 (safety rewrites) benefits from more UX design work.** The confidence-rating UI needs wireframes. The residual-review dialog needs to be designed before it's built.

**Wave 5-6 benefit from deeper research on JSON schema design and enterprise tooling** before execution. There we're not fully ready yet.

So the real dependencies:

```
Wave 1 (execute now, zero risk)
    ↓
Wave 2 (tests) — no research dependency
    ↓
Wave 3 (safety rewrites) — needs UX brief for confidence-rating dialog
    ↓
Wave 4 (runtime) — self-contained
    ↓
Wave 5 (feature modules + JSON cleaners) — needs JSON schema research
    ↓
Wave 6 (enterprise-ready) — needs enterprise tooling deep-dive
    ↓
Wave 7 (defer, revisit later)
```

The research we still owe ourselves is:
- **Before Wave 3 execution:** UX brief (specifically the residual-review dialog design)
- **Before Wave 5 execution:** JSON schema + winapp2.ini import spec
- **Before Wave 6 execution:** Enterprise tooling deep-dive

None of these block Waves 1, 2, or 4. Those three waves alone are ~1 month of focused work and push the app from "has bugs" to "has runtime-lightweight, tested, safe" — a genuinely different tool.

---

## The stability question, answered directly

**Is our motion stable?**  
Yes. Direction has been consistent since the first review. Later research has refined and validated, not reversed.

**Are we in motion?**  
On research: yes, we've produced 12 briefs. On execution: no, by your instruction.

**Will motion remain stable if we continue researching?**  
Yes, provided we scope future research to things that materially change the plan (UX for Wave 3, JSON schema for Wave 5, enterprise tooling for Wave 6). Adding more general research (comparing more competitors, more voice-of-user threads) won't change decisions and starts to burn time.

**What's the risk to stability?**  
Two things could destabilise:
1. **Scope creep** — if we add features to the "big plans" list without pruning something else, the wave sequencing gets fragile. So far we've been additive without pruning (voice-of-user added tray widget + shell extension). At some point we'll need to say what we're *not* doing to make room. The stack-and-roadmap doc already has a "deliberately NOT doing" section; keep enforcing it.
2. **Delayed execution** — the longer we plan without touching code, the more the plan feels theoretical. Real friction only appears when you actually run `dotnet build` on a change. A pure-planning approach eventually plateaus in usefulness.

Neither risk has materialised yet. If we keep researching for another 3-4 sessions with no execution, both start to.

---

## What I'd propose (you decide)

Two paths that respect your "research first" call while making progress feel real:

**Path 1 — Finish the research trilogy, then execute.**

- Next session: **UX / UI redesign brief** (mockups + wireframes, focused on the workflow-first uninstall + VT + refusal-list positioning). Unblocks Wave 3.
- Following session: **Deep specs for Wave 3 features** (confidence-rating data model, residual-scan algorithm, uninstall-string parser, VT H1/H2 fixes). Turns brief into implementable.
- Session after: **Enterprise deep-dive** (WiX vs alternatives, ADMX authoring detail, Event Log setup, SignPath.io, SOC 2 templates). Unblocks Wave 6.
- **Then start Wave 1 execution.**

That's ~3 more research sessions, then execution. Total plan is thoroughly de-risked.

**Path 2 — Consolidate + start Wave 1.**

- **Consolidate** the 12 briefs into one canonical planning document (`system-cleaner-plan.md`).
- **Start executing Wave 1** (zero-downside, no dependencies on remaining research).
- **Interleave remaining research** with execution — UX brief when Wave 3 is next, JSON schema when Wave 5 is next, enterprise deep-dive when Wave 6 is next.

That's forward motion this session while keeping the research discipline for the harder waves.

**My honest recommendation is Path 2.** The research on Waves 1, 2, and 4 is saturated. Executing them now unlocks the "we shipped something" energy and gives us real code to base UX / enterprise decisions on. Future research can be scoped tightly to the specific wave it unblocks, which is more productive than open-ended "let's research everything."

But this is a genuine judgement call — if you specifically want the full research trilogy first, Path 1 is legitimate and safe.

**Your move.**
