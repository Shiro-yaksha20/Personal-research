---
title: ADR-0006 — Cut Waves 6-7 from committed roadmap
source: claude-personas/CRITIQUE-SYNTHESIS
date: 2026-08-18
status: Proposed
topics: [roadmap, enterprise, scope, solo-dev-sustainability]
related:
  - decisions/0005-enterprise-on-prem-not-saas.md
  - research/claude-personas/CRITIQUE-SYNTHESIS.md
  - research/claude-main/2026-08-08-10-stack-and-roadmap.md
---

# ADR-0006: Cut Waves 6-7 from committed roadmap

**Status:** Proposed
**Date:** 2026-08-18
**Deciders:** Shiro-yaksha20 (pending)

## Context

Four independent critique passes (senior-engineer, product-strategist, IT-director, OSS-maintainer) converged on the same finding: **Wave 6 timeline is fantasy, Wave 7 is a fatal distraction.**

Specific numbers from the critique:

- Senior engineer: Wave 6 estimated "off by 2-3×". WiX 4 alone is a weekend. ADMX authoring needs a real AD lab. SignPath OSS onboarding is 2-6 weeks calendar. "Tested on Server 2019/2022" implies VMs we don't own.
- Product strategist: "Wave 6 alone is a 6-month full-time job at a real ISV. Weekends means 18 months minimum, during which Kudu ships ~400 releases."
- IT director: "Wave 6 as scoped is a competent single-endpoint enterprise install story. It is not an enterprise fleet product."
- OSS maintainer: completion probability = Wave 5 (30%), Wave 6 (10%), Wave 7 (2%). Honest cutoff: end of Wave 4.

Additionally, enterprise sales require infrastructure (LLC, E&O insurance, MSA/DPA, SOC 2 documentation) that is out of scope for a solo dev without significant non-code investment.

## Decision (Proposed)

Cut **Waves 6 and 7 from the committed roadmap.** Move them to "considered future work."

**Committed scope becomes v1.0.1 → v1.4.0** (Waves 1–4):

1. Wave 1 — Zero-downside cleanup (v1.0.1)
2. Wave 2 — Test scaffolding (v1.2.0)
3. Wave 3 — Safety rewrites + VT shell extension (v1.3.0)
4. Wave 4 — Runtime lightweight (v1.4.0)

**Deferred:**

5. Wave 5 — Feature module architecture — questioned by senior engineer as premature; reassess after v1.4.0 ships
6. Wave 6 — Enterprise-ready endpoint — moved to future work; return only if concrete demand emerges (a real IT admin asks for MSI + ADMX)
7. Wave 7 — Server component — deleted from roadmap unless a customer pays for it

**PLAN.md updates required if accepted:**

- Rewrite §11 "The seven-wave roadmap" to show 4 committed + 3 deferred
- Update §14 "Where we are, what's next" to reflect end-of-Wave-4 as the honest cutoff
- Note in §12 (enterprise angle) that Wave 6 is deferred and Wave 7 requires customer demand
- CHANGELOG entry logging the change

## Rationale

- Cross-critique convergence is high-signal. When four independent lenses agree, it's almost certainly right.
- Wave 6 requires business infrastructure (company, insurance, contracts, legal) not currently present.
- Wave 7 is a second product, not a sequel to the first — it deserves its own decision when the demand exists, not a pre-committed timeline.
- Cutting waves increases the probability of shipping what's actually committed. Overcommitment is a leading indicator of solo-dev abandonment.
- The v1.0.1 → v1.4.0 arc is still a genuinely different tool from what exists today. Users get real value.

## Alternatives Considered

- **Keep the full 7-wave roadmap** — Rejected per critique. Timeline is unrealistic; commit-and-slip is worse than commit-less-and-ship.
- **Cut only Wave 7, keep Wave 6** — Considered. Wave 6 is a substantial 6-month project without business infrastructure to back it. The half-measure risks producing "enterprise-ready" software with no company behind it — worse than not shipping enterprise at all.
- **Cut everything past Wave 3** — Considered. Wave 4 (runtime lightweight) is self-contained, high-value, and delivers on the "runs on modest hardware" positioning. Keeping it in scope is justified.

## Consequences

### Positive

- Realistic commitment. Waves 1-4 are ~5-6 focused weekends. Achievable.
- v1.4.0 becomes a natural pause point for real user feedback before deciding on 5, 6, 7.
- Reduces liability exposure (no MSI-deployed-at-fleet-scale until we're ready).
- Reduces scope pressure — every additive feature must displace something else.
- The plan can survive a slipped weekend; the current 7-wave plan cannot.

### Negative

- The "enterprise on-prem gap" (ADR-0005) is a real market opportunity that competitors could fill in the interim. If someone ships this before we do, we lose the moat.
- Users who want enterprise features have to wait for concrete signal-and-response.
- "LTS" branding becomes weaker without Wave 6 backport commitments.
- The three critique bets that stack ("Windows-native + weak hardware + Windows 10", "safe uninstaller with VT", "cleaner enterprise IT can approve") lose the third bet.

### Neutral

- The critique also identified positioning issues (see ADR-0007) that don't depend on this decision.

## What "reactivation" of Waves 5-7 looks like

If any of these emerges, the deferred waves come back on the table:

- **Wave 5:** contributor arrives who wants the module structure, OR internal code becomes unmanageable in a single Core assembly
- **Wave 6:** real IT admin (r/sysadmin, direct email) asks for MSI + ADMX for actual pilot deployment
- **Wave 7:** paying customer signs a contract for the server component, funding a real time investment

Without those signals, keep the deferred waves in "considered" state — not on the roadmap.

## References

- research/claude-personas/CRITIQUE-SYNTHESIS.md — the aggregated critique that triggered this ADR
- research/claude-personas/oss-maintainer/ — completion probability data
- research/claude-personas/it-director/ — pilot-blocker requirements
- decisions/0005-enterprise-on-prem-not-saas.md — the earlier decision this partially rescopes
- research/claude-main/2026-08-08-10-stack-and-roadmap.md — the original 7-wave plan
