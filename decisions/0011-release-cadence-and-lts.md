---
title: ADR-0011 — Release cadence and LTS strategy
source: perplexity + claude-personas
date: 2026-08-19
status: Proposed
topics: [release-cadence, lts, roadmap, solo-dev, versioning]
related:
  - research/claude-main/2026-08-19-meta-solo-dev-strategy-burnout.md
  - research/claude-main/2026-08-19-uninstaller-enterprise-roadmap.md
  - research/claude-personas/CRITIQUE-SYNTHESIS.md
  - decisions/0006-cut-waves-6-7-from-committed-roadmap.md
supersedes: []
---

# ADR-0011: Release cadence and LTS strategy

**Status:** Proposed
**Date:** 2026-08-19
**Deciders:** Shiro-yaksha20 (pending)

## Context

PLAN.md §11 shows a 7-wave version plan (v1.0.1 → v2.0.0 LTS → v2.x). Individual ADRs (0006 in particular) have already scoped the roadmap down to Waves 1-4 committed + 5-7 deferred. But there's no explicit ADR on:

- How often releases ship
- What "LTS" actually means in this project's context
- Update channel policy (stable/beta/nightly?)
- Breaking-change policy
- Deprecation policy

Perplexity `2026-08-19-meta-solo-dev-strategy-burnout.md` and `2026-08-19-uninstaller-enterprise-roadmap.md` both explicitly recommend:

> *"Fewer, high-quality releases rather than frequent feature drops. Clear change logs and versioning so that users can trust updates."*

The persona critique reinforces:

> *"Kudu ships every 1-3 days — that's velocity but leads to elevated bug surface. Users see 40+ releases in a month. Their top issues are elevation bugs from those rapid releases."*

## Decision (Proposed)

Adopt an explicit release cadence and LTS policy:

### Release cadence

- **Feature releases:** target 1 per quarter. Never more often than 1 per month.
- **Patch releases:** as needed for critical safety fixes or regressions. Small scope, single-issue.
- **LTS releases:** designated 1 per year (usually the last stable release of the year), receive backported security fixes for 24 months from designation.
- **No nightly channel.** Beta channel optional — opt-in via a config setting, not a separate installer.

### Versioning: strict semver

- `MAJOR.MINOR.PATCH`
- **MAJOR** bump = breaking change to policy schema, JSON cleaner rule format, or any user-visible workflow that requires re-teaching. Only when unavoidable.
- **MINOR** bump = new feature, non-breaking.
- **PATCH** bump = bug fix, security fix, no new features.
- Pre-release: `-beta.N`, `-rc.N`.

### Update channel policy

Two channels only:

- **Stable** — default. Users on stable get the current stable release + LTS backports.
- **Beta** — opt-in via Settings. Users on beta get the current beta release + release candidates. Beta users file bug reports.

No auto-update on either channel by default. Users check for updates manually via a Settings action, or via Winget updates. See ADR-0007 for related trust posture.

### Breaking change policy

- Every breaking change ships in a MAJOR release with:
  - CHANGELOG entry explaining what breaks and why
  - Migration guide (docs) with concrete before/after examples
  - Configuration/policy backwards compatibility layer for at least one MAJOR (deprecated, warned, works)
  - Explicit "removed in this MAJOR" list

### Deprecation policy

- Features get deprecated in one MINOR release (with warning in UI + docs).
- Deprecated features are removed in the NEXT MAJOR (not immediately).
- LTS releases keep deprecated features working for their full 24-month window.

### LTS branding

- LTS releases are marked in CHANGELOG.md and in the download link.
- LTS receives backported patches for **critical security fixes and safety-critical bugs only.** No feature backports. No convenience backports.
- If a critical security fix requires a breaking change (rare), it goes to a new MAJOR; LTS gets a mitigation guide instead.

### First LTS

**v2.0.0 (per PLAN.md) is designated the first LTS.** Its 24-month window runs from v2.0.0 release date. This aligns with the "enterprise-friendly" positioning of v2.0.0 in ADR-0005 (assuming Wave 6 ships; if not, the next Waves 1-4 milestone that qualifies gets LTS designation).

## Rationale

- Predictability > velocity. Enterprise pilots (ADR-0005) can't evaluate a tool that ships every 3 days.
- Solo-dev sustainability requires bounded commitment (ADR-0006). "1 quarterly minor release + LTS backports" is a bounded promise.
- Semver discipline reduces user surprise. When v2.1.0 → v2.2.0, users know it's non-breaking without reading release notes.
- Two channels is enough. More = maintenance overhead for no user value.
- 24-month LTS matches Windows enterprise release cadence expectations (Win11 22H2 was supported through 30 months for Enterprise; Ubuntu LTS is 5 years but 24 months is realistic for solo-dev).

## Alternatives Considered

- **No cadence policy (ship when ready)** — Rejected. Voice-of-user research shows the target audience wants predictability. "Ship when ready" also enables research addiction / ship anxiety patterns.
- **Match Kudu's every-1-3-days cadence** — Rejected. Solo dev can't sustain and stability suffers.
- **Rolling release only (no LTS)** — Rejected. Blocks enterprise pilots per ADR-0005.
- **Long support windows (5-year LTS)** — Rejected. Solo dev can't commit to backports across 5-year windows without burnout.
- **Three-channel model (stable/beta/nightly)** — Rejected. Nightly adds maintenance for a use case (bleeding-edge devs) that doesn't align with the trust-first audience.

## Consequences

### Positive

- Users can plan around SystemCleaner updates.
- Enterprise procurement can evaluate LTS releases with a defined support window.
- Solo dev has bounded commitment — 1 minor per quarter is achievable and leaves time for research/other pursuits.
- Semver discipline is a real trust signal for the target audience (matches "no auto-updates by default" — users can trust the version number).

### Negative

- Feature velocity is lower than competitors (Kudu). Users who want the latest features fastest will pick Kudu.
- Breaking changes require migration guides — extra work per MAJOR.
- LTS backport commitment is real work per LTS window. If security issues pile up, backports become the main workload for months.
- Beta channel adds a testing dimension. If nobody opts in, it's dead weight.

### Neutral

- The cadence commitments here interact with Wave 6 LTS branding. If Wave 6 doesn't ship (per ADR-0006 discussion), LTS branding moves to whichever earlier release is stable enough.

## Open questions

- **What if a critical CVE is found in a non-LTS version?** — proposed: fix in current stable and backport to LTS; don't patch older non-LTS releases individually.
- **What if the maintainer needs to skip a quarter (life event, burnout)?** — proposed: honest CHANGELOG entry explaining the gap. No pressure to backfill.
- **What about pre-release feature flags?** — proposed: features behind flags in beta, promoted to stable in the next MINOR only after evidence. Requires infrastructure not yet built.

## References

- research/claude-main/2026-08-19-meta-solo-dev-strategy-burnout.md — sustainability framing
- research/claude-main/2026-08-19-uninstaller-enterprise-roadmap.md — competitor cadence comparison
- research/claude-personas/CRITIQUE-SYNTHESIS.md — critique of Kudu-style rapid cadence
- decisions/0005-enterprise-on-prem-not-saas.md — enterprise positioning requires predictable cadence
- decisions/0006-cut-waves-6-7-from-committed-roadmap.md — bounded roadmap scope
- decisions/0007-rewrite-positioning-not-refusal-list.md — related trust posture (auto-update opt-in)
- [SemVer 2.0.0 spec](https://semver.org/) — versioning standard
