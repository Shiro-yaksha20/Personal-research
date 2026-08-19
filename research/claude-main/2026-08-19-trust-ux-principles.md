---
title: Trust stack and UX principles for SystemCleaner
source: claude-main
date: 2026-08-19
status: reviewed
topics: [trust, ux, virustotal, previews, telemetry, refusal-list]
related:
  - PLAN.md
  - PLAN-SUMMARY.md
supersedes: []
---

## 1. Trust stack framing

SystemCleaner's strongest differentiator is its explicit trust stack: no registry cleaner, no PC health score theatre, no telemetry or bundling, and minimal irreversible actions.

This should be framed as an intentional, user-protective design rather than "missing features". The product is "what remains after we remove everything scammy or unsafe from the PC cleaner genre".

Core trust components:

- VT reputation checks integrated into uninstall and inspection workflows.
- Conservative residual detection, with no blind registry nuke operations.
- Clear previews and logs for every destructive action.
- Refusal of background telemetry and opaque cloud dependencies.

---

## 2. UX principles derived from trust

UX should make the trust stack visible and felt:

- Every destructive operation must have a preview screen that lists what will change, with clear grouping (files, folders, registry keys, scheduled tasks).
- VT data should be presented as "confidence and context" (e.g., number of engines detecting an app, first-seen date) rather than as an absolute verdict.
- Dangerous or irreversible operations should be rare, well-explained, and never enabled by default.
- Surfaces like dashboards and summaries should avoid scores; instead, they should show factual state and recommended next steps.

The tone of messaging is closer to a good sysadmin explaining what they are about to do than to an optimizer promising miracles.

---

## 3. Handling VT safely

Integrating VT into everyday workflows introduces security and UX constraints:

- Keys must be stored and used safely (no secrets in URLs, no shared mutable headers, no verbose logging of sensitive data).
- Error states (network failures, VT rate limits, unknown files) should degrade gracefully: show local heuristics and explain limitations without blocking uninstall.
- Users should be able to opt out of VT entirely (e.g., offline mode) while still using the uninstaller and residual detection.

This reinforces the idea that SystemCleaner is trustworthy even in constrained or sensitive environments.

---

## 4. Telemetry, offline mode, and documentation

The refusal of telemetry and cloud should be documented clearly:

- Explicitly state that SystemCleaner does not collect or send usage data, program lists, or file contents to external services, except when the user deliberately triggers VT checks.
- Provide toggles or configuration for VT and other outbound calls, with clear explanations.
- Document offline behaviour and limitations so that enterprise and privacy-conscious users understand what SystemCleaner will and will not do.

Documentation is part of UX: many users will never read it, but those who do should find it consistent with what the UI communicates.
