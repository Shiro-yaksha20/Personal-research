---
title: ADR-0007 — Rewrite positioning as positive claim, not refusal list
source: claude-personas/product-strategist
date: 2026-08-18
status: Proposed
topics: [positioning, marketing, tagline, readme]
related:
  - research/claude-personas/CRITIQUE-SYNTHESIS.md
  - research/claude-main/2026-08-08-11-voice-of-user.md
---

# ADR-0007: Rewrite positioning as positive claim, not refusal list

**Status:** Proposed
**Date:** 2026-08-18
**Deciders:** Shiro-yaksha20 (pending)

## Context

The current positioning tagline (per PLAN.md §1 and voice-of-user brief):

> *SystemCleaner. No telemetry. No cloud. No auto-updates. No registry cleaner. No PC-health-score theatre. Just the things Windows should do itself but doesn't. Free forever. Open source top to bottom.*

Product-strategist critique response:

> "The refusal list is not positioning; it's a negative-space rant. A list of five nots tells a normal user nothing about the outcome. It reads as internal ideology leaking into copy. Test: read the tagline to a non-technical Windows user and ask what the product does — they will not be able to answer."

The refusal list has value as a values statement, but it's not a first-encounter tagline. Positioning must answer "what does this do for me?" before "what does this refuse to do?"

## Decision (Proposed)

Restructure the README and marketing surfaces as:

1. **Lead with a positive, outcome-focused tagline** — one sentence with a subject and a verb describing benefit.
2. **The refusals become an "Our values" section further down** — kept for the trust-first audience but not the first thing anyone reads.

**Candidate tagline (draft, not final):**

> *Uninstall Windows apps and see exactly what they leave behind — verified against VirusTotal, without leaving one lightweight window.*

Variants worth testing:

- *"See what's really running on your Windows PC — and safely remove what shouldn't be."*
- *"Windows maintenance for people who read the settings — cleanup, uninstall, hardware monitor, VirusTotal, in one lightweight window."*
- *"The safe uninstaller for Windows — with confidence-rated residuals and built-in VirusTotal check."*

The exact tagline needs voice-of-user validation (see open question).

## Rationale

- Positioning must communicate outcome before values. Users evaluating a utility want to know what it does, not what it doesn't.
- The refusals have real trust-signal value for a specific audience — but they belong on a "Values" or "About" page, not the first sentence.
- The plan's own audience research (voice-of-user brief) shows this audience wants both — the outcome ("safe uninstaller") AND the values ("no telemetry, no cloud"). Structure them so both are visible without competing.
- The critique's convergence is high-signal — while only product-strategist directly critiqued this, the senior-engineer and IT-director critiques implicitly agreed by noting the positioning doesn't clearly identify the audience.

## Alternatives Considered

- **Keep the refusal list as tagline** — Rejected per critique. Refuses to communicate outcome.
- **Two taglines — one outcome, one refusal** — Considered. Awkward on marketing surfaces; better to have one lead and structure the refusals as a supporting section.
- **Drop refusals entirely** — Rejected. The trust posture is a real differentiator vs Kudu and CCleaner. It just belongs in a different structural position.

## Consequences

### Positive

- Non-technical Windows user can understand what SystemCleaner does on first read.
- SEO improves — "safe uninstaller" is a search term; "no telemetry no cloud" is not.
- The trust-values section stays, giving the values-first audience something to find.
- The tagline can be A/B tested against the current one on Reddit / Winget listing / GitHub description.

### Negative

- Rewriting positioning surfaces (README, GitHub description, Winget manifest, PLAN.md) means editing 4+ places consistently.
- The current tagline's audience (users specifically searching for "no telemetry" cleaners) may not immediately recognise the rebrand.

### Neutral

- Every user-visible surface needs to be updated once. Not a code change.

## Open question

- **Which specific tagline?** The draft candidates need testing. Options for validation:
  - Post variants on r/Windows10 or r/software with a simple "which of these tells you what this app does?" poll
  - Show to 3-5 non-technical Windows users offline (family, friends, etc.) and ask what they think it does
  - Compare click-through rates on GitHub if we ship v1.0.1 with one and v1.0.2 with another
- **Blocks final adoption of ADR-0007** — accept the *principle* now, defer final tagline copy until we have validation signal.

## References

- research/claude-personas/CRITIQUE-SYNTHESIS.md (product-strategist section, specifically the "negative-space rant" critique)
- research/claude-main/2026-08-08-11-voice-of-user.md — feature-signal-strength table + audience emotional register
- PLAN.md §1, §2, §6, §7 — sections requiring update if accepted
