---
title: ADR-0008 — Add Microsoft PC Manager to the competitive landscape
source: perplexity + claude-main
date: 2026-08-19
status: Proposed
topics: [competitors, landscape, microsoft-pc-manager, positioning]
related:
  - research/claude-main/2026-08-19-ecosystem-kudu-pcmanager-cleaners-vt.md
  - research/CROSS-REVIEW.md
  - decisions/0003-depth-over-breadth.md
supersedes: []
---

# ADR-0008: Add Microsoft PC Manager to the competitive landscape

**Status:** Proposed
**Date:** 2026-08-19
**Deciders:** Shiro-yaksha20 (pending)

## Context

PLAN.md §5 "Market position" lists Kudu, CCleaner, BleachBit, BCU, Revo, Autoruns, HWiNFO, LibreHardwareMonitor, WizTree, dupeGuru, FluentCleaner, Chris Titus Winutil, Wintoys, Fleet — but **misses Microsoft PC Manager entirely.**

Perplexity's `2026-08-19-ecosystem-kudu-pcmanager-cleaners-vt.md` documents Microsoft PC Manager as:

- A first-party Microsoft utility (available in Store or via Microsoft download)
- Bundles Health Check, deep cleanup, memory boost, process/startup management
- Wraps existing Windows capabilities (Disk Cleanup, Storage Sense, Task Manager, Defender) in a single UI
- Identity: "official and safe" rather than "deep control"
- Explicitly does NOT compete with dedicated uninstallers like Revo or BCU

This is a real competitor to SystemCleaner's "cleanup" pillar, especially for the audience that wants a first-party official option. Missing it in the landscape analysis is a factual gap.

## Decision (Proposed)

Add **Microsoft PC Manager** to PLAN.md §5 competitive landscape with:

- Category: All-in-one cleanup (Microsoft first-party)
- License: Proprietary, free
- Positioning: "Official Microsoft utility, wraps built-in Windows tools"
- Overlap with SystemCleaner: Cleanup pillar (partial overlap); Startup manager (partial overlap)
- Non-overlap: No uninstaller, no VT, no hardware monitor, no residual scan

Also update the "gap SystemCleaner fills" statement in §5 to note: *"Microsoft PC Manager occupies the 'official + basic' end; Kudu occupies the 'broad + polished' end; SystemCleaner occupies the 'trust-first + deep uninstall + weak hardware' gap between them."*

## Rationale

- Factually complete competitive analysis matters — if a reviewer or contributor asks "why not use Microsoft PC Manager?" and PLAN.md has no answer, the plan looks unaware.
- Microsoft PC Manager coexisting with SystemCleaner is actually a plausible scenario for many users: PC Manager for basic maintenance, SystemCleaner for uninstall + VT + hardware monitor.
- Positioning against PC Manager also clarifies what SystemCleaner is NOT — we're not a "trusted first-party alternative" (Microsoft owns that positioning); we're a "deep-control + trust-first + weak-hardware" alternative.

## Alternatives Considered

- **Don't add Microsoft PC Manager** — Rejected. Factual gap. Someone will point it out eventually.
- **Add it as a "not a real competitor" footnote** — Rejected. It genuinely competes for cleanup use cases. Should be treated as competitor with clear differentiation, not dismissed.
- **Also add Microsoft Defender / Windows Security as competitor** — Considered. Defender competes with the VT-check-for-malware use case. Worth mentioning but SystemCleaner's VT integration is different (multi-engine reputation vs single-engine detection). Defer to a separate follow-up ADR if needed.

## Consequences

### Positive

- Complete competitive picture for future contributors and reviewers.
- Sharpens what SystemCleaner IS by explicitly stating what it isn't (not "official cleanup," not "broad toolkit").
- Positioning against PC Manager naturally reinforces the depth-over-breadth pillar (ADR-0003).

### Negative

- Adds a section to PLAN.md — small maintenance cost.
- If Microsoft evolves PC Manager toward SystemCleaner's territory (adds uninstaller with residual scan, adds VT), the landscape shifts. But that's a general risk, not specific to this ADR.

### Neutral

- Microsoft PC Manager's audience (users who want first-party) is largely disjoint from SystemCleaner's target audience (weak hardware + trust-first + depth). Coexistence is fine.

## References

- research/claude-main/2026-08-19-ecosystem-kudu-pcmanager-cleaners-vt.md — Perplexity's ecosystem brief
- research/CROSS-REVIEW.md — §D5 gap fill
- PLAN.md §5 "Market position (competitor landscape)" — target of update
- Microsoft PC Manager: [pcmanager.microsoft.com](https://pcmanager.microsoft.com/) (verify link before merge)
