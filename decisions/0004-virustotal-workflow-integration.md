---
title: ADR-0004 — VirusTotal integrated into workflow, not just at release time
source: claude-main
date: 2026-08-08
status: Accepted
topics: [virustotal, workflow, moat, positioning]
related:
  - research/claude-main/2026-08-08-07-differentiation.md
  - research/claude-main/2026-08-08-08-vs-kudu.md
  - research/claude-personas/CRITIQUE-SYNTHESIS.md
---

# ADR-0004: VirusTotal integrated into workflow, not just at release time

**Status:** Accepted
**Date:** 2026-08-08
**Deciders:** Shiro-yaksha20

## Context

Kudu, CCleaner, BleachBit, BCU — none of them integrate VirusTotal in-app for users to check files against. Kudu VT-scans their release binaries (a trust signal about *their* tool), but users can't scan their *own* files without leaving the app.

Standalone VirusTotal context-menu extensions exist (Genbox VirusTotalContextMenu, RightClickVirusTotal, WCMS) — demonstrating validated demand nobody has bundled into a general utility.

SystemCleaner already has a VirusTotal service (with H1/H2 security bugs to fix per PLAN.md §4).

## Decision

VirusTotal is a **workflow integration**, not a bolted-on feature:

1. **Uninstaller integration** — before executing an uninstall, hash-check the target executable against VT. Show verdict inline.
2. **Right-click Explorer shell extension** — "Check with VirusTotal via SystemCleaner" on any file. Shell extension registered by MSI.
3. **Standalone VirusTotal tab** — the current implementation, kept but repositioned as a supporting surface.
4. **Hash-only by default** — files never leave the machine unless the user explicitly opts to upload.

## Rationale

- Validated demand (multiple standalone VT context-menu tools have users). SystemCleaner absorbs that demand into a general utility.
- Nobody else does it in an all-in-one — this is a genuine differentiator vs Kudu.
- The uninstaller integration creates a workflow ("I don't trust this app → VT-check → uninstall → clean residuals") no other tool covers end-to-end.
- Hash-only default preserves the trust posture (files stay on the machine unless the user chooses).

## Alternatives Considered

- **VT integration only in a settings-hidden VirusTotal tab** — Rejected. That's what we currently have, and it doesn't create a workflow moat.
- **VT-check at download time** (via downloads folder watcher) — **Reconsidered per critique.** Product-strategist critique argues VT-at-download > VT-at-uninstall behaviourally, because the trust decision happened at install/download time. This is worth exploring — see open question below.
- **No VT integration, drop the feature** — Rejected. It's the unique differentiator vs every other all-in-one utility.

## Consequences

### Positive

- Unique workflow moat. No other all-in-one utility offers "safe uninstall with VT integration."
- Right-click shell extension is a high-visibility feature users screenshot and share.
- Preserves user privacy by default (hash-only, no upload unless opted in).

### Negative

- VT rate limits are strict on free tier (4 req/min, 32 MB uploads). Enterprise/power users may find this limiting.
- Corporate deployment requires GPO-configurable disable — see IT-director critique. **Enterprise ADMX must include VT-disable-entirely + hash-only-never-upload + enterprise-API-key knobs.**
- The uninstaller integration adds a synchronous VT lookup step to a flow users expect to be immediate. Must be async + skippable with clear "don't wait for VT" option.

### Neutral

- Behavioural question (VT-at-uninstall vs VT-at-download) remains open — see critique.

## Open questions

- **Is VT-at-uninstall the right integration point, or should VT-at-download / VT-at-first-run be primary?** Product-strategist critique in `research/claude-personas/CRITIQUE-SYNTHESIS.md` argues the trust decision happened before uninstall time.
  - **Suggested experiment:** ship BOTH — VT check when a new .exe appears in Downloads (via watcher) AND VT check before uninstall. Users pick their workflow.
  - **Blocks resolution of this ADR from Accepted → confirmed final until we measure.**

## References

- research/claude-main/2026-08-08-07-differentiation.md
- research/claude-main/2026-08-08-08-vs-kudu.md
- research/claude-main/2026-08-08-11-voice-of-user.md (§4 — validated demand for standalone VT tools)
- research/claude-personas/CRITIQUE-SYNTHESIS.md (product-strategist section)
- [Genbox VirusTotalContextMenu (GitHub)](https://github.com/Genbox/VirusTotalContextMenu) — reference implementation
