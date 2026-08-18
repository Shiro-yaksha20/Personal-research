---
title: ADR-0003 — Depth over breadth — three pillars, not fifteen features
source: claude-main
date: 2026-08-08
status: Accepted
topics: [scope, feature-strategy, kudu, positioning]
related:
  - research/claude-main/2026-08-08-03-feature-comparisons.md
  - research/claude-main/2026-08-08-08-vs-kudu.md
---

# ADR-0003: Depth over breadth — three pillars, not fifteen features

**Status:** Accepted
**Date:** 2026-08-08
**Deciders:** Shiro-yaksha20

## Context

Kudu ships 15+ tools at ~60% polish each. Kudu Cloud, CCleaner Business, IObit Advanced SystemCare, Wise Care 365 — all take the same "all-in-one" approach. As a solo dev with weekend-only time, matching them feature-for-feature is impossible.

Voice-of-user research (see research brief 2026-08-08-11) confirmed users describe HWiNFO as "too much info," BleachBit as "basic UI," Kudu as "rush release" — they don't universally want maximum features.

## Decision

SystemCleaner focuses on **three pillars, done at 95% polish**:

1. **Cleanup** — the classical junk-file/browser-cache/temp cleaner, with JSON-defined rules importable from winapp2.ini's community catalog.
2. **Uninstaller + VirusTotal workflow** — deep residual scan with confidence rating (BCU-style), VT hash-check on the executable before uninstall.
3. **Hardware Monitor** — LibreHardwareMonitor-based, tray widget as primary interaction, glanceable CPU/RAM/temp.

Every proposed new feature must strengthen one of these three pillars or be rejected.

## Rationale

- Solo-dev capacity means fewer features done well beats more features done poorly.
- The three pillars are the ones the target audience (weak-hardware Windows 10/11 power user) actually uses regularly.
- Kudu already covers everything else. Chasing them across 15 features is a losing race.
- Depth on 3 features gives 3 things that can be "the best in category" for a subset of users.

## Alternatives Considered

- **All-in-one, like Kudu** — Rejected. Solo dev can't sustain 15-feature polish.
- **Single-purpose (cleanup only)** — Rejected. Loses the workflow moat that requires uninstaller + VT together.
- **Two pillars (drop hardware monitor)** — Considered. Hardware monitor is arguably lower-value than cleanup + uninstall for the target audience. But the tray widget is a durable "app is present" surface that keeps users engaged. Kept for now; reassess post-v1.4.0.

## Consequences

### Positive

- Focused development. Every wave clearly serves one of the three pillars.
- Marketing message is clearer: "clean + uninstall+VT + monitor" is three claims, not fifteen.
- Test surface bounded — fewer features to keep working across every release.
- Reduces support burden per user (three things they interact with, not fifteen).

### Negative

- Users who want a debloater or driver manager or malware scanner won't find them here. They may leave in review counts saying "I wanted feature X."
- The BCU + Autoruns + HWiNFO comparison always looks unfavourable if you compare feature counts.
- If Kudu adds a feature we lack, users may feel behind.

### Neutral

- Feature-scope refusal list (ADR-0007 proposed) makes this explicit.

## References

- research/claude-main/2026-08-08-03-feature-comparisons.md
- research/claude-main/2026-08-08-08-vs-kudu.md
- research/claude-main/2026-08-08-11-voice-of-user.md
