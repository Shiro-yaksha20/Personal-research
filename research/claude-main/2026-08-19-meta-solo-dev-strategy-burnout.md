---
title: "Meta: solo developer strategy and burnout risk"
source: claude-main
date: 2026-08-19
status: reviewed
topics: [meta, solo-dev, burnout, scope, roadmap]
related:
  - PLAN.md
  - PLAN-SUMMARY.md
supersedes: []
---

## 1. Scope and risk

SystemCleaner's space is inherently tempting for scope creep: uninstaller, cleaner, hardware monitor, startup manager, malware scanner, and more.

A solo developer cannot safely match the full breadth of Kudu or the long history of Revo and BCU without risking burnout or quality issues.

The plan's refusal list and focus on a narrow core are therefore not just philosophical choices; they are practical risk controls.

---

## 2. Core to protect

The core that should be protected from scope creep includes:

- High-confidence uninstall with VT-integrated reputation and clear previews.
- Conservative residual detection that avoids dangerous registry operations.
- Awareness of hardware/resource constraints, especially on weak machines.

Everything else should be treated as optional or out of scope unless there is a compelling reason.

---

## 3. Release strategy

A realistic release strategy for a solo dev should emphasise:

- Fewer, high-quality releases rather than frequent feature drops.
- Clear change logs and versioning so that users can trust updates.
- Occasional, well-planned waves of work (like this research wave) followed by consolidation and implementation.

This aligns with user expectations for trustworthy system tools more than with consumer app trends.

---

## 4. Personal sustainability

To keep the project sustainable:

- Acknowledge that not every competitor feature needs to be matched.
- Use research and ADRs to say "no" explicitly to tempting features.
- Accept that some markets (e.g., full cloud fleet management) are out of reach for now.

The outcome should be a smaller, sharper tool that can be maintained long-term, rather than a broad suite that burns out its developer.
