---
title: Future ADRs and research backlog for SystemCleaner
source: claude-main
date: 2026-08-19
status: reviewed
topics: [adrs, backlog, roadmap, research]
related:
  - PLAN.md
  - PLAN-SUMMARY.md
supersedes: []
---

## 1. ADRs to formalise

Based on the current plan and research, several decisions are already implicit and should be captured as formal ADRs:

- Positioning on weak hardware: Windows-native WPF, explicit support for low-end machines, and published performance targets.
- VT integration scope: where VT appears in the UI, how reputation is shown, and how offline/opt-out modes work.
- Refusal list: no registry cleaner, no health scores, no telemetry, no bundling, no cloud requirement.
- Enterprise/on-prem stance: target environments, documentation requirements, and supported deployment patterns.
- Release cadence and support: LTS strategy, update channels, and how breaking changes are handled.

Each ADR should record context, options considered, the chosen decision, and consequences, linking back to relevant research notes.

---

## 2. Research backlog

Future research waves could focus on:

- Detailed UI/UX flows for uninstall, including VT checks, previews, and confirmation steps.
- Comparative analysis of residual detection strategies (files, registry, services) versus BCU and Revo.
- Enterprise deployment patterns (on-prem, restricted outbound network, local VT proxies or alternatives).
- SEO and messaging: how to talk about SystemCleaner's refusal list and weak-hardware focus without sounding niche or alarmist.
- Burnout and scope management: strategies for keeping the project sustainable as a solo developer.

These topics can be converted into individual research notes and, where appropriate, ADRs.

---

## 3. Linking research to PLAN.md

To keep PLAN.md as the single source of truth, each major change or new insight should flow through a simple pipeline:

- Raw research notes are created in `research/` (by humans or AI).
- Relevant findings are distilled into ADRs.
- Accepted ADRs trigger updates to PLAN.md and PLAN-SUMMARY.md.
- CHANGELOG.md records plan-level changes.

This keeps the system coherent and prevents research from drifting away from the canonical plan.
