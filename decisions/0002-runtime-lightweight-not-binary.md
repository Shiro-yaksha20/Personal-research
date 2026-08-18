---
title: ADR-0002 — Interpret "lightweight" as runtime cost on weak hardware, not binary size
source: claude-main
date: 2026-08-08
status: Accepted
topics: [lightweight, weak-hardware, positioning, runtime-cost]
related:
  - research/claude-main/2026-08-08-04-lightweight-binary.md
  - research/claude-main/2026-08-08-05-lightweight-runtime.md
---

# ADR-0002: Interpret "lightweight" as runtime cost on weak hardware, not binary size

**Status:** Accepted
**Date:** 2026-08-08
**Deciders:** Shiro-yaksha20

## Context

Early plan work interpreted "lightweight" as binary size (132 MB self-contained). Multiple approaches were compared: staying on WPF (~132 MB), framework-dependent (5.7 MB + runtime install), or migrating to Avalonia + NativeAOT (25–40 MB).

The user explicitly reframed: **"size doesn't matter at all"** — the concern is CPU/RAM/IO on weaker/older systems (Celeron, 4 GB, HDD, integrated GPU). Distribution can be fat if runtime is thin.

Runtime probe confirmed real runtime problems: `LibreHardwareMonitor.Computer.Open()` = 2.7 s blocking startup; HKLM\SOFTWARE walk = 28 s per app for residual scan; on Celeron-class hardware these translate to 5–8 s frozen splash and multi-minute residual scans.

## Decision

We will interpret "lightweight" as **runtime cost on weak hardware**:

- Cold-start time (target: <1 s to window paint, even on a Celeron)
- RAM at rest (target: <100 MB after all lazy-init settles)
- CPU during idle monitoring (target: <2% of one core on i5-7300U)
- Responsiveness during long operations (UI never freezes)
- Adaptive behaviour based on detected hardware

Binary size is explicitly out of scope. If v2.0.0 self-contained is 150 MB, that's fine.

## Rationale

- Users don't choose or reject utilities based on binary size in 2026.
- Runtime cost directly determines usability on the target audience's hardware.
- Fixing runtime cost requires code changes we can make (lazy init, event-driven, adaptive polling); fixing binary size requires framework migration we chose not to make (ADR-0001).

## Alternatives Considered

- **Both binary AND runtime lightweight** — Rejected as overreach. Binary lightweight requires framework migration; runtime alone doesn't. Pick one.
- **Neither — just be a "normal" utility** — Rejected. "Runs on modest hardware" is a positioning bet (see ADR-0007 proposed).

## Consequences

### Positive

- The technical work concentrates on lazy init, event-driven Windows APIs, adaptive polling — all things that improve UX for everyone, not just weak-hardware users.
- Enables a "Performance Mode" auto-detected preset for weak systems.
- Aligns with the "Windows-native runtime, not Electron" differentiator vs Kudu (see ADR-0004).

### Negative

- Distribution size stays large (132 MB single-file for v2.0.0). Some users may perceive this as bloated.
- Every UI feature must be tested in Tier-0 fallback mode (WPF software rendering).
- "Performance Mode" is a permanent maintenance branch — features get tested twice.

### Neutral

- The distinction "runtime vs binary lightweight" requires explaining to users. README needs to be clear.

## References

- research/claude-main/2026-08-08-04-lightweight-binary.md
- research/claude-main/2026-08-08-05-lightweight-runtime.md
- measurements/2026-08-08-perf-probe.txt
