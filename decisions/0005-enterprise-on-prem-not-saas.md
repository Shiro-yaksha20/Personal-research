---
title: ADR-0005 — Enterprise as on-prem/self-hosted, not SaaS
source: claude-main
date: 2026-08-08
status: Accepted (v2.0.0 scope) — Wave 7 server component under review per ADR-0006
topics: [enterprise, on-prem, kudu-cloud, business-model]
related:
  - research/claude-main/2026-08-08-09-enterprise-angle.md
  - research/claude-personas/CRITIQUE-SYNTHESIS.md
---

# ADR-0005: Enterprise as on-prem/self-hosted, not SaaS

**Status:** Accepted for Wave 6 (endpoint enterprise-ready). Wave 7 (self-hosted server) is under review — see ADR-0006.
**Date:** 2026-08-08
**Deciders:** Shiro-yaksha20

## Context

Kudu Cloud is SaaS-only at $5-9/device/mo. No self-hosted option. CCleaner Cloud for Business is SaaS-only, closed source. Fleet (fleetdm.com) is self-hosted OSS but in adjacent MDM category (device config), not maintenance.

**There is no self-hosted, open-source, Windows-native maintenance suite with fleet management in 2026.**

Target audience Kudu Cloud excludes structurally: defence, healthcare with PHI, EU public sector with data residency requirements, air-gapped OT, financial services, universities with FERPA, any org whose CISO has vetoed SaaS.

## Decision

If SystemCleaner enters enterprise territory, it does so as **on-prem/self-hosted, not SaaS**.

**Wave 6 (endpoint enterprise-ready):** MSI + MSIX + ADMX + audit log + code signing + LTS commitment. Every endpoint is deployable via SCCM/Intune/GPO. **This decision stands.**

**Wave 7 (self-hosted server component):** Blazor Server dashboard + mTLS WebSocket endpoint reporting + Active Directory auth + air-gap support. **This decision is under review per ADR-0006** because critique consensus argues Wave 7 is a fatal solo-dev distraction.

Deliberately NOT chosen: multi-tenant SaaS, per-device pricing, open-core paywall.

## Rationale

- On-prem/self-hosted is unmet in the space. Nobody occupies it.
- Kudu Cloud's structural constraints (SaaS-only, closed dashboard, per-device pricing, cross-platform tax, US-hosted) exclude a real audience.
- Matches SystemCleaner's overall trust posture (no cloud requirement).

## Alternatives Considered

- **SaaS-hosted** — Rejected. Structurally hostile to the trust maximalism positioning. Also, "compete with Kudu Cloud at $5" as a solo dev without a company is losing.
- **Open-core paywall split** — Rejected. Kills community trust. Every commit becomes a "should this be Community or Enterprise Edition?" fight.
- **No enterprise at all, consumer-only** — Considered. Enterprise is real work with real costs (see ADR-0006). Wave 6 endpoint-ready is small enough to keep; Wave 7 server is under review.

## Consequences

### Positive

- Fills a real gap in the market.
- Matches the trust posture — no cloud requirement, air-gap compatible.
- Free-forever self-hosted server (if built) undercuts Kudu Cloud at scale.
- Same binary, policy-driven — consumer and enterprise share the codebase.

### Negative

- **Enterprise sales requires actual company infrastructure** (LLC, E&O insurance, MSA/DPA, security questionnaire responses, SOC 2 audit if going upmarket). Solo dev cannot provide these without significant non-code work — see IT-director critique.
- **Server component is a 2-4 month project on top of the desktop app.** Real burnout risk. See OSS-maintainer critique.
- Enterprise sales cycles are 6-18 months. Long time to see revenue signal.
- Every deploy touches a live customer machine at scale — liability grows non-linearly.

### Neutral

- Enterprise adoption may drive feature priorities that don't align with consumer needs.

## Related decisions

- **ADR-0006 (Proposed)** — Cut Waves 6-7 from committed roadmap. If accepted, this ADR's scope narrows to "if we ever pursue enterprise, this is how" rather than "we are pursuing enterprise."

## References

- research/claude-main/2026-08-08-09-enterprise-angle.md
- research/claude-personas/CRITIQUE-SYNTHESIS.md (all four personas commented on this)
- [Kudu Cloud pricing](https://usekudu.com/pricing)
- [Fleet — open source MDM](https://fleetdm.com/lp/open-source) — adjacent category, self-hosted reference
