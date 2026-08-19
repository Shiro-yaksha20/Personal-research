---
title: Cross-review — Perplexity research vs 4-persona critique
source: claude-main
date: 2026-08-19
status: reviewed
topics: [meta, positioning, virustotal, refusal-list, cross-review, triangulation]
related:
  - research/claude-personas/CRITIQUE-SYNTHESIS.md
  - research/claude-main/2026-08-19-positioning-weak-hardware-vt-refusals.md
  - decisions/0007-rewrite-positioning-not-refusal-list.md
---

# Cross-Review: Perplexity vs 4-Persona Critique

Two independent external lenses on PLAN.md land in the same repo:

1. **[Perplexity research wave](./claude-main/#2026-08-19--perplexity-research-wave)** (6 briefs, committed via `0569c08`) — Shiro-driven Perplexity searches, coherent research briefs with citations.
2. **[4-Persona critique](./claude-personas/CRITIQUE-SYNTHESIS.md)** — 4 fresh Claude subagents (senior-engineer, product-strategist, IT-director, OSS-maintainer), each starting cold, reading PLAN.md.

Same model family (Claude subagents) vs different model family (Perplexity backed by different LLMs). Where they **converge**, the signal is high. Where they **diverge**, judgment call is needed.

This document is a triangulation aid — it does NOT decide anything. Decisions go in ADRs.

---

## Convergences (high-confidence findings)

Both lenses independently reached the same conclusion:

### C1 — Solo-dev sustainability is the top failure mode

- **OSS-maintainer critique:** *"Research addiction is the loudest signal in this plan. 12 briefs + 0 code changes."*
- **Perplexity `meta-solo-dev-strategy-burnout`:** *"A solo developer cannot safely match the full breadth of Kudu or the long history of Revo and BCU without risking burnout or quality issues."* Recommends "fewer, high-quality releases" and "occasional, well-planned waves of work followed by consolidation and implementation."

**Signal strength:** Both explicit and prescriptive. **Take action.**

### C2 — Refusal of registry cleaner is well-supported

- **Voice-of-user brief:** *"Registry cleaners don't speed up your PC and none of them are safe" (Microsoft/XDA consensus).*
- **Perplexity `ecosystem-kudu-pcmanager-cleaners-vt`:** *"Microsoft explicitly advises against registry cleaners due to risk of system instability."*

**Signal strength:** Both lenses cite Microsoft's own guidance. **Refusal stands.**

### C3 — Enterprise as on-prem is more achievable than SaaS for a solo dev

- **OSS-maintainer critique:** *"Delete Wave 7. If a customer someday pays for it, revisit. Do not build server infrastructure on spec."*
- **Perplexity `uninstaller-enterprise-roadmap`:** *"For a solo developer, this [on-prem] is more achievable than competing with a full cloud dashboard and multi-platform agent ecosystem."*

**Signal strength:** Same conclusion, different framing. **Confirms ADR-0005 (on-prem stance) and ADR-0006 (defer Wave 7).**

### C4 — VT integration is a differentiator but not an uncopyable moat

- **Product-strategist critique:** *"Kudu declining to build [in-workflow VT] is not a gap; it's a read of the room."*
- **Perplexity `positioning-weak-hardware-vt-refusals`:** *"VT integration itself is not rare… however, VT tightly integrated into an uninstall workflow with confidence ratings, previews, and optional actions — is unusual among all-in-one system cleaner tools. This is a differentiating feature in practice, not an uncopyable moat."*

**Signal strength:** Same nuance. Both acknowledge VT-in-workflow is differentiated but temporary — Kudu could add it. **Implication:** ship it earlier and better, tie into broader trust story.

### C5 — Reversibility as UX principle beyond preview-before-delete

- **Voice-of-user brief:** *"Preview before delete = universal ask (★★★★★)."*
- **Perplexity `uninstaller-enterprise-roadmap`:** *"UX should highlight confidence and reversibility (backups, dry runs, logs) rather than speed or aggressiveness alone."*

**Signal strength:** Perplexity extends the preview principle into a broader reversibility discipline. Both agree preview is universal; Perplexity adds backups + dry runs + logs. **Action:** expand UX principles ADR to cover the full reversibility surface.

---

## Divergences (judgment calls needed)

Where the lenses disagree, either the truth is nuanced or one lens has better ground.

### D1 — The refusal list as positioning

- **Product-strategist critique (disagrees):** *"The refusal list is not positioning; it's a negative-space rant. A list of five nots tells a normal user nothing about the outcome."*
- **Perplexity `positioning-weak-hardware-vt-refusals` (agrees):** *"Lean fully into the refusal list as a core part of the brand: 'SystemCleaner is what you get when you remove every scammy and unsafe pattern from the PC cleaner space.'"* Backed by Microsoft's own anti-registry-cleaner guidance and CCleaner's history.

**Nuance:** Both are partially right.

- **Product-strategist is right** that "no telemetry, no cloud, no auto-updates" as the FIRST sentence a user reads doesn't communicate what SystemCleaner does.
- **Perplexity is right** that the refusals are validated by real ecosystem sentiment and shouldn't be dropped.

**Synthesis for ADR-0007:** lead with an outcome-focused tagline (subject-verb-object), keep the refusals as a prominent "Our values" section immediately following. Both audiences served, no info lost.

### D2 — Weak-hardware positioning as differentiator

- **Product-strategist critique (skeptical):** *"'Runs on weak hardware' is a developer's aesthetic, not a purchase driver. No forum post you can point to says 'I picked utility X because it runs on my old laptop.'"*
- **Perplexity `positioning-weak-hardware-vt-refusals` (validates):** *"Weak-hardware-first can be a differentiator, but only if it is made concrete and visible: publish performance data, startup time, memory usage at idle, and impact of typical scans on low-end hardware, ideally head-to-head vs Kudu and one or two older cleaners."*

**Nuance:**

- Product-strategist is right that "runs on weak hardware" as a slogan is vague and unmeasured.
- Perplexity is right that Kudu's Electron overhead creates a structural gap SystemCleaner can own — **but only if measurements are published.**

**Synthesis:** the action item is the same either way — measure and publish. "Cold-start <1 s, idle RAM <40 MB on Celeron, vs Kudu's 5s + 350 MB" is a differentiator BOTH would accept. **Vague version fails; measured version wins.**

### D3 — Windows 10 first-class positioning

- **Product-strategist critique + senior-engineer critique (dismiss):** *"Win10 EoL was Oct 2025 — you're 10 months past it. Marketing 'runs on Windows 10' reads as 'runs on unsupported OS' to enterprise buyers."*
- **Perplexity (doesn't explicitly address Win10 EoL):** implicitly supports Win10 support as part of "clearly state Windows 10 support, no Win11-only limitations."

**Nuance:** The persona critique has ecosystem-timeline evidence; Perplexity may have been briefed on the plan without emphasising the Win10 EoL context. **Persona critique wins this one** — pivot from "Windows 10 first-class" to "modest Windows 11 hardware."

### D4 — Enterprise buyer trust in a solo maintainer

- **IT-director critique (blunt):** *"A tool with `pnputil /uninstall` in the code path is a non-starter for procurement without: contractual SLA, E&O insurance certificate, indemnification clause, a real corporate entity, and a DPA for the VirusTotal path."*
- **Perplexity `uninstaller-enterprise-roadmap` (softer):** *"Enterprise buyers will expect clear documentation on what SystemCleaner does and does not do… transparent handling of VT integration."* Framed as documentation requirements, not legal infrastructure.

**Nuance:** Both true at different scales.

- For **25-endpoint pilots at small/medium orgs:** Perplexity's documentation-and-transparency bar is realistic. Solo dev can meet it.
- For **500-2000 endpoints at real enterprises:** IT-director's legal infrastructure bar is real. Solo dev can't clear it without an LLC + insurance.

**Synthesis:** SystemCleaner Enterprise can serve small orgs on Perplexity's bar; enterprise-at-scale requires business infrastructure. **ADR-0006 defer decision stands.**

### D5 — Microsoft PC Manager as a competitor

- **Persona critique:** Didn't mention it (nobody flagged this gap).
- **Perplexity `ecosystem-kudu-pcmanager-cleaners-vt` (new):** *"Microsoft PC Manager is a first-party utility that bundles Health Check, deep cleanup, memory boost, and process/startup management… Its identity is 'official and safe' rather than 'deep control'; it does not aim to compete with dedicated uninstallers like Revo or BCU."*

**Not a divergence — a gap Perplexity filled.** No conflict; the plan's §5 competitive landscape needs Microsoft PC Manager added. See ADR-0008 (Proposed).

---

## New content Perplexity introduced (no persona-critique counterpart)

Items only Perplexity surfaced. Persona critique either didn't mention or doesn't apply.

### N1 — VT "confidence and context" presentation model

Perplexity `trust-ux-principles`:

> *"VT data should be presented as 'confidence and context' (e.g., number of engines detecting an app, first-seen date) rather than as an absolute verdict."*

**Actionable UX principle.** Not a raw "clean/malicious" verdict; framing as N-of-70 engines flag it + first-seen date + install metadata. Different mental model. Worth capturing in an ADR. See ADR-0009 (Proposed).

### N2 — "Good sysadmin" tone of voice

Perplexity `trust-ux-principles`:

> *"The tone of messaging is closer to a good sysadmin explaining what they are about to do than to an optimizer promising miracles."*

**Concrete UX-writing guidance.** Applies to release notes, in-app copy, docs. Distinctive vs the "your PC is broken, click to fix" tone of legacy cleaners. See ADR-0010 (Proposed).

### N3 — Local VT proxies for enterprise deployment

Perplexity `trust-ux-principles`:

> *"Users should be able to opt out of VT entirely (e.g., offline mode) while still using the uninstaller and residual detection."*

Perplexity `uninstaller-enterprise-roadmap`:

> *"Ability to run in environments where outbound HTTP may be restricted."*

**Enterprise-relevant.** Some regulated orgs run internal VT-like reputation services (Recorded Future, Anomali). Enterprise ADMX must support pointing at custom VT-compatible endpoints, not just VT public. **Extends the ADMX policy scope for Wave 6.** Add to ADR-0004 or a new sub-ADR.

### N4 — Release cadence as an explicit design decision

Perplexity `future-adrs-research-backlog`:

> *"ADRs to formalise: release cadence and support: LTS strategy, update channels, and how breaking changes are handled."*

**No existing ADR covers this** despite being discussed in the roadmap. Should be captured formally. See ADR-0011 (Proposed).

---

## What this cross-review changes in the plan

Nothing yet — this document is triangulation, not decision. **All decisions go through ADRs.**

The following ADRs are proposed as a result of this cross-review:

- **ADR-0007 amendment (existing)** — reconcile refusal-list disagreement (D1) with lead-with-outcome-tagline + values-section-below synthesis
- **ADR-0008 (new)** — Add Microsoft PC Manager as a first-party competitor to PLAN.md §5 (D5 / N/A — gap fill)
- **ADR-0009 (new)** — Adopt "confidence and context" presentation for VT (N1)
- **ADR-0010 (new)** — Tone-of-voice guidelines: "good sysadmin explaining what they are about to do" (N2)
- **ADR-0011 (new)** — Release cadence + LTS strategy (N4)

Also implicitly reinforces existing ADRs:

- **ADR-0005 (enterprise on-prem)** — reinforced by C3
- **ADR-0006 (cut Waves 6-7)** — nuanced by D4 (partial support for small-org enterprise via docs; large-enterprise requires business infra)
- **ADR-0007 (positioning rewrite)** — refined by D1 synthesis

## Reading this document in the future

If you come back later and want to know "did we resolve the refusal-list debate?" — read this cross-review first, then look at the ADR that formalized the resolution. This document preserves both lenses in case a later ADR needs to revisit.

Never delete or edit conflicting positions to reflect the "winner." Preservation of dissent is a feature: 6 months from now, the current winner might turn out to have been wrong.
