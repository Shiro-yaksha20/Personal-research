---
title: ADR-0001 — Stay on WPF, not Avalonia or WinUI 3
source: claude-main
date: 2026-08-08
status: Accepted
topics: [ui-framework, wpf, avalonia, native-aot, windows-10]
related:
  - research/claude-main/2026-08-08-04-lightweight-binary.md
  - research/claude-main/2026-08-08-05-lightweight-runtime.md
---

# ADR-0001: Stay on WPF, not Avalonia or WinUI 3

**Status:** Accepted
**Date:** 2026-08-08
**Deciders:** Shiro-yaksha20

## Context

SystemCleaner's current UI framework is WPF on .NET 9. Alternatives considered:

- **Avalonia + NativeAOT** — would enable 25–40 MB single-file publish, <100 ms cold start, cross-platform. But WPF XAML doesn't port 1:1; requires 2–4 weekends of UI rewrite.
- **WinUI 3** — no NativeAOT support in .NET 9. Windows 11 only (raises hardware floor). Doesn't help our target audience.
- **Web view (WebView2)** — small XAML + HTML for UI. Introduces JS toolchain. Doesn't match the "Windows-native" positioning.

The user clarified early on: "lightweight" means runtime cost on weak hardware, not binary size. That reframes the framework question entirely.

## Decision

We will stay on **.NET 9 + WPF** for v1.x and v2.0.0 LTS.

## Rationale

- Binary size is not a business constraint. Users don't uninstall CCleaner (30 MB installer) because of size. Kudu's 108 MB installer isn't why users pick or reject it.
- Runtime cost IS the constraint, and WPF's runtime cost is fine — measured 40–100 MB RAM at rest on modern hardware. The problems we found (2.7 s LHM Open, 28 s HKLM walk) are in our code, not in WPF.
- Windows 10 support matters for the immediate target audience. WinUI 3 rules it out.
- Avalonia migration is 2–4 weekends of UI rewrite for a benefit (smaller binary) users don't ask for.

## Alternatives Considered

- **Avalonia + NativeAOT** — Rejected for v1.x/v2.0.0. Would be the right choice if binary size or startup time became a hard business constraint. Revisit post-v2.0.0 if evidence emerges.
- **WinUI 3** — Rejected. Windows 11 only. Loses ~500M Windows 10 users. Also no NativeAOT.
- **Migrate to Uno Platform** — Not seriously considered. Similar tradeoffs to Avalonia but less mature.
- **Rewrite in native C++ / Win32** — Rejected. Solo dev, MVVM patterns matter, .NET productivity is worth the runtime cost.

## Consequences

### Positive

- No UI rewrite. Waves 1–5 can execute against the existing XAML.
- Windows 10 continues to work.
- WPF's mature ecosystem (WPF-UI, ModernWpf, MahApps.Metro if needed).
- We keep all existing ViewModels and code-behind.

### Negative

- Self-contained single-file publish stays at ~132 MB.
- No NativeAOT — .NET runtime overhead on startup.
- WPF's poor trimming story stays. Can't `PublishTrimmed` effectively.
- If binary size becomes important later, a full Avalonia migration is 3–6 weekends of work we've deferred, not avoided.

### Neutral

- WPF is in maintenance mode at Microsoft — not shrinking, but not getting new features. Doesn't matter for our use case.

## References

- research/claude-main/2026-08-08-04-lightweight-binary.md — the initial (superseded) size-first analysis
- research/claude-main/2026-08-08-05-lightweight-runtime.md — the runtime-first analysis that reframed the question
- [.NET runtime issue #79166 — NativeAOT WPF support](https://github.com/dotnet/runtime/issues/79166) — confirms WPF cannot AOT
- [Avalonia Native AOT docs](https://docs.avaloniaui.net/docs/deployment/native-aot) — the road not taken
