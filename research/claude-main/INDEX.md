# Claude Main — Research Index

Planning briefs produced during the SystemCleaner R&D sessions. Read in numeric order for the historical thinking arc; jump to specific ones by topic.

## Chronological reading order

### 2026-08-08 — Initial planning wave (Claude Code sessions with Shiro)

| # | Date | File | Topic |
|---|---|---|---|
| 01 | 2026-08-08 | [initial-code-review](./2026-08-08-01-initial-code-review.md) | First-pass audit — C1/C2/C3 safety bugs, package drift, thin tests |
| 02 | 2026-08-08 | [extended-audit](./2026-08-08-02-extended-audit.md) | Runtime probe results — S1-S6 startup bugs, HM1-HM4 monitor issues |
| 03 | 2026-08-08 | [feature-comparisons](./2026-08-08-03-feature-comparisons.md) | Feature-by-feature vs BCU, Autoruns, HWiNFO, BleachBit, WizTree, dupeGuru |
| 04 | 2026-08-08 | [lightweight-binary](./2026-08-08-04-lightweight-binary.md) | Binary-size analysis (superseded by runtime brief) |
| 05 | 2026-08-08 | [lightweight-runtime](./2026-08-08-05-lightweight-runtime.md) | Runtime cost on weak hardware — the operative lightweight brief |
| 06 | 2026-08-08 | [honest-downsides](./2026-08-08-06-honest-downsides.md) | Real costs of every recommendation in the plan |
| 07 | 2026-08-08 | [differentiation](./2026-08-08-07-differentiation.md) | Landscape survey; retraction of "yet another clone"; VirusTotal moat validation |
| 08 | 2026-08-08 | [vs-kudu](./2026-08-08-08-vs-kudu.md) | Kudu deep-dive; 8 axes of "way different"; 3 that stack |
| 09 | 2026-08-08 | [enterprise-angle](./2026-08-08-09-enterprise-angle.md) | Kudu Cloud pricing, on-prem gap, three-phase enterprise build |
| 10 | 2026-08-08 | [stack-and-roadmap](./2026-08-08-10-stack-and-roadmap.md) | Stack today → stack target → 7-wave roadmap (superseded by PLAN.md) |
| 11 | 2026-08-08 | [voice-of-user](./2026-08-08-11-voice-of-user.md) | Reddit / XDA / HowToGeek / MakeUseOf themes; feature-signal-strength table |

### 2026-08-18 — Meta / infrastructure

| # | Date | File | Topic |
|---|---|---|---|
| 12 | 2026-08-18 | [toolchain-research](./2026-08-18-12-toolchain-research.md) | Obsidian + Git decision for this R&D repo |

### 2026-08-19 — Perplexity research wave

Committed separately from Perplexity (via GitHub commit `0569c08`, not through the Claude Code session). Cross-referenced in [`../claude-personas/CRITIQUE-SYNTHESIS.md`](../claude-personas/CRITIQUE-SYNTHESIS.md) and [`CROSS-REVIEW.md`](../CROSS-REVIEW.md).

| # | Date | File | Topic |
|---|---|---|---|
| P1 | 2026-08-19 | [meta-solo-dev-strategy-burnout](./2026-08-19-meta-solo-dev-strategy-burnout.md) | Solo-dev sustainability, scope refusal as risk control |
| P2 | 2026-08-19 | [trust-ux-principles](./2026-08-19-trust-ux-principles.md) | Trust stack framing; UX principles; "good sysadmin" tone |
| P3 | 2026-08-19 | [ecosystem-kudu-pcmanager-cleaners-vt](./2026-08-19-ecosystem-kudu-pcmanager-cleaners-vt.md) | **Adds Microsoft PC Manager as first-party competitor** we missed |
| P4 | 2026-08-19 | [uninstaller-enterprise-roadmap](./2026-08-19-uninstaller-enterprise-roadmap.md) | BCU/Revo/PC Manager UX; on-prem vs Kudu Cloud; cadence realism |
| P5 | 2026-08-19 | [positioning-weak-hardware-vt-refusals](./2026-08-19-positioning-weak-hardware-vt-refusals.md) | Stress-tests the three pillars; strongly validates the refusal list |
| P6 | 2026-08-19 | [future-adrs-research-backlog](./2026-08-19-future-adrs-research-backlog.md) | Proposed ADRs to formalise; research backlog |

## By topic

### Safety / bugs
- [initial-code-review](./2026-08-08-01-initial-code-review.md)
- [extended-audit](./2026-08-08-02-extended-audit.md)

### Competitive landscape
- [feature-comparisons](./2026-08-08-03-feature-comparisons.md)
- [differentiation](./2026-08-08-07-differentiation.md)
- [vs-kudu](./2026-08-08-08-vs-kudu.md)
- [ecosystem-kudu-pcmanager-cleaners-vt](./2026-08-19-ecosystem-kudu-pcmanager-cleaners-vt.md) ⭐ *adds Microsoft PC Manager*
- [uninstaller-enterprise-roadmap](./2026-08-19-uninstaller-enterprise-roadmap.md)

### Performance / weak hardware
- [lightweight-binary](./2026-08-08-04-lightweight-binary.md) (superseded)
- [lightweight-runtime](./2026-08-08-05-lightweight-runtime.md)
- [positioning-weak-hardware-vt-refusals](./2026-08-19-positioning-weak-hardware-vt-refusals.md) — §1 weak-hardware validation

### Positioning / voice of user
- [differentiation](./2026-08-08-07-differentiation.md)
- [voice-of-user](./2026-08-08-11-voice-of-user.md)
- [positioning-weak-hardware-vt-refusals](./2026-08-19-positioning-weak-hardware-vt-refusals.md) ⭐ *main positioning brief*

### VirusTotal
- [positioning-weak-hardware-vt-refusals](./2026-08-19-positioning-weak-hardware-vt-refusals.md) — §2 VT in workflow
- [trust-ux-principles](./2026-08-19-trust-ux-principles.md) — §3 handling VT safely; confidence-and-context

### Trust / refusal list
- [voice-of-user](./2026-08-08-11-voice-of-user.md) — refusal-list-as-marketing thesis
- [trust-ux-principles](./2026-08-19-trust-ux-principles.md) ⭐ *trust stack UX principles*
- [positioning-weak-hardware-vt-refusals](./2026-08-19-positioning-weak-hardware-vt-refusals.md) — §3 refusal list ecosystem validation

### Enterprise / business
- [enterprise-angle](./2026-08-08-09-enterprise-angle.md)
- [uninstaller-enterprise-roadmap](./2026-08-19-uninstaller-enterprise-roadmap.md) — §2 on-prem vs Kudu Cloud

### Plan / roadmap
- [stack-and-roadmap](./2026-08-08-10-stack-and-roadmap.md) — superseded by [../../PLAN.md](../../PLAN.md)
- [future-adrs-research-backlog](./2026-08-19-future-adrs-research-backlog.md) — proposed ADRs to formalise

### Meta / sustainability
- [honest-downsides](./2026-08-08-06-honest-downsides.md)
- [meta-solo-dev-strategy-burnout](./2026-08-19-meta-solo-dev-strategy-burnout.md) ⭐
- [toolchain-research](./2026-08-18-12-toolchain-research.md)

## Sources of research

- **2026-08-08 briefs** — Claude Code session with Shiro (main session, this Claude assistant)
- **2026-08-18 brief** — Same session, after PKM tooling decision
- **2026-08-19 briefs (P1-P6)** — Perplexity, run by Shiro externally and committed via `0569c08`

## Cross-references

- **4-persona critique of PLAN.md** → [`../claude-personas/CRITIQUE-SYNTHESIS.md`](../claude-personas/CRITIQUE-SYNTHESIS.md)
- **Perplexity + persona critique comparison** → [`../CROSS-REVIEW.md`](../CROSS-REVIEW.md)
