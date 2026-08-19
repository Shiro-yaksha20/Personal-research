---
title: ADR-0009 — VirusTotal "confidence and context" presentation model
source: perplexity
date: 2026-08-19
status: Proposed
topics: [virustotal, ux, presentation-model, trust]
related:
  - research/claude-main/2026-08-19-trust-ux-principles.md
  - research/claude-main/2026-08-19-positioning-weak-hardware-vt-refusals.md
  - research/CROSS-REVIEW.md
  - decisions/0004-virustotal-workflow-integration.md
supersedes: []
---

# ADR-0009: VirusTotal "confidence and context" presentation model

**Status:** Proposed
**Date:** 2026-08-19
**Deciders:** Shiro-yaksha20 (pending)

## Context

ADR-0004 committed to VirusTotal-in-workflow integration (uninstall-time hash check, right-click shell extension, hash-only default). It didn't specify **how VT results are presented to the user.**

Perplexity's `2026-08-19-trust-ux-principles.md` recommends a specific model:

> *"VT data should be presented as 'confidence and context' (e.g., number of engines detecting an app, first-seen date) rather than as an absolute verdict."*

The default alternative (which some tools use) is a **verdict model**: red "Malicious" / green "Clean" / yellow "Unknown." The verdict model is simpler but hides nuance and encourages users to act without thinking. The confidence-and-context model is richer but requires more UI real estate.

## Decision (Proposed)

Adopt the **"confidence and context" presentation model** for all VT surfaces in SystemCleaner (uninstall-time check, shell extension, standalone VT tab).

### Concrete UI content

When a file has VT data, display:

1. **Detection ratio** — "3 of 72 engines flag this file" (not "MALICIOUS")
2. **First-seen date** — "First seen on VirusTotal: 2024-11-14" (age = signal)
3. **Detection engine names** (top 3-5, expandable) — "Kaspersky, Microsoft, Sophos"
4. **File metadata** — signer, product name, common installation paths across VT reports
5. **Direct link to the full VT report** (permalink) — user can inspect further

When a file is **unknown** to VT:

- "This file is not yet known to VirusTotal" (not a red flag by itself)
- Offer to submit if user chooses (hash-only default, upload as explicit action)

**Never present a single-word verdict.** Never colour-code a file "green" or "red" without the context above.

### Confidence thresholds (recommended, not enforced)

For user guidance (in-app copy, not policy enforcement):

- 0 detections + old file + signed + widely-installed → high confidence safe
- 1-3 detections (with major AV) + old file → likely false positive, but check yourself
- 4+ detections + young file + unsigned → high confidence suspicious
- Unknown → check the source of the file, then submit if trusted enough

Never assert confidence programmatically; always show the underlying facts.

## Rationale

- Verdict models train users to trust or distrust based on a single signal. When that signal is wrong (VT false positive, unknown-but-safe file), users have no context to override the tool.
- Confidence-and-context matches the "good sysadmin explaining what they're about to do" tone (see ADR-0010 proposed).
- Users on the target audience (weak-hardware + trust-first) are more likely to appreciate the nuance than the general consumer audience — which is fine, we're not building for the general consumer.
- The alternative (verdict) has been done by many tools; SystemCleaner differentiating on presentation is a real UX moat.

## Alternatives Considered

- **Simple verdict (Clean/Malicious/Unknown)** — Rejected. Hides nuance, encourages blind trust.
- **Confidence-and-context by default + verdict toggle** — Considered. Adds complexity for questionable benefit; users who want a verdict can look at the detection ratio themselves.
- **Confidence-and-context for uninstall flow only; verdict for shell extension** — Considered. Inconsistent UX; if the model is right, it's right everywhere.
- **Present raw VT JSON dump** — Rejected. Overwhelming for the intended audience; the point is *presented* context, not raw data.

## Consequences

### Positive

- Better-informed users. Confidence signals persist across false-positive edge cases.
- Distinctive UX vs verdict-based cleaners and antivirus dashboards.
- Reinforces the trust-first positioning — SystemCleaner doesn't oversimplify.
- Reduces liability of a false verdict — if a user deletes a legitimate file based on a wrong "MALICIOUS" label, that's on us. Confidence-and-context puts context back on the user.

### Negative

- More UI real estate per VT result. Have to design for space constraints.
- More words on screen per interaction. Users who want fast one-glance verdicts will find it slower.
- Requires more polished UX writing (context sentences, thresholds guidance).
- Presenting nuance well is harder than presenting binaries.

### Neutral

- Enterprise deployments will want a policy-configurable version of this (some IT admins prefer verdict for their users). Handle via ADMX policy override in Wave 6.

## Open questions

- **How to visualise the detection ratio compactly?** — proposed: horizontal segmented bar (72 segments, 3 filled) with numeric overlay. Needs UX design.
- **What about VT permission-blocked files (unknown hash + user declines to upload)?** — proposed: "unknown, not submitted" state; not treated as either safe or suspicious.
- **Should we show engine-specific detection strings (e.g., "Trojan.Generic.KVE")?** — proposed: yes, but folded behind expandable "show detections" — power users benefit, casual users skip.

## References

- research/claude-main/2026-08-19-trust-ux-principles.md — §3 handling VT safely
- research/claude-main/2026-08-19-positioning-weak-hardware-vt-refusals.md — §2 VT-in-workflow analysis
- research/CROSS-REVIEW.md — §N1
- decisions/0004-virustotal-workflow-integration.md — the integration ADR this ADR refines
- [VirusTotal API v3 docs](https://docs.virustotal.com/reference/overview) — data model reference
