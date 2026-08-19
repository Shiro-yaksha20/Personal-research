# Changelog

Plan-change history. Every time PLAN.md is meaningfully updated, log it here with the date, what changed, and which research/ADR triggered it.

## 2026-08-19

### Perplexity research wave integrated

Six research briefs committed via GitHub (`0569c08`) from Perplexity, then reviewed and cross-referenced against the existing 4-persona critique.

**New research files at `research/claude-main/`:**

- `2026-08-19-meta-solo-dev-strategy-burnout.md`
- `2026-08-19-trust-ux-principles.md`
- `2026-08-19-ecosystem-kudu-pcmanager-cleaners-vt.md`
- `2026-08-19-uninstaller-enterprise-roadmap.md`
- `2026-08-19-positioning-weak-hardware-vt-refusals.md`
- `2026-08-19-future-adrs-research-backlog.md`

**New cross-review document:** `research/CROSS-REVIEW.md` — triangulates Perplexity findings against the 4-persona critique. Documents 5 convergences (high-confidence findings) and 5 divergences (judgment calls needed).

**Four new ADRs proposed based on the Perplexity wave:**

- ADR-0008 — Add Microsoft PC Manager to PLAN.md §5 competitive landscape (factual gap fill)
- ADR-0009 — Adopt VirusTotal "confidence and context" presentation model (N of 72 engines + first-seen date + engine names + metadata, not a single-word verdict)
- ADR-0010 — Explicit tone-of-voice guidelines: "good sysadmin explaining what they are about to do" not "optimizer promising miracles"
- ADR-0011 — Release cadence and LTS strategy: 1 minor per quarter, semver, 24-month LTS backports

**Existing ADRs referenced but not modified yet:**

- ADR-0007 (positioning rewrite) will need an amendment reconciling the Perplexity + persona-critique disagreement on the refusal list (see CROSS-REVIEW.md §D1)

**PLAN.md updates required if these ADRs are accepted:**

- §5 Market position — add Microsoft PC Manager row
- §7 Design principles — add VT presentation model + tone-of-voice principles
- §11 Roadmap — align release cadence with ADR-0011

None of the above are integrated into PLAN.md yet — they await Shiro's decision on Accept / Reject / Defer for each ADR.

**Also updated:** `research/claude-main/INDEX.md` — now includes the 6 Perplexity briefs organized as a separate 2026-08-19 wave section.

**README expanded** to include a full "project in one page" brief at the top.

---

## 2026-08-18

### Initialized the R&D repo

Migrated 12 planning briefs, 1 critique synthesis, 3 measurement outputs, 1 review prompt into a curated structure. Added:

- Folder structure (research/, decisions/, prompts/, measurements/, ARCHIVE/)
- README.md with curation workflow + frontmatter conventions
- .gitignore for Obsidian workspace state
- Seed ADRs 0001-0007 based on PLAN.md's load-bearing decisions

**Migrated from scratchpad:**

| Original | New location |
|---|---|
| PLAN.md | ./PLAN.md |
| PLAN-SUMMARY.md | ./PLAN-SUMMARY.md |
| STATUS.md | ./STATUS.md |
| CRITIQUE.md | research/claude-personas/CRITIQUE-SYNTHESIS.md |
| system-cleaner-review.md | research/claude-main/2026-08-08-01-initial-code-review.md |
| system-cleaner-audit-2.md | research/claude-main/2026-08-08-02-extended-audit.md |
| system-cleaner-comparisons.md | research/claude-main/2026-08-08-03-feature-comparisons.md |
| system-cleaner-lightweight-brief.md | research/claude-main/2026-08-08-04-lightweight-binary.md |
| system-cleaner-runtime-brief.md | research/claude-main/2026-08-08-05-lightweight-runtime.md |
| system-cleaner-downsides.md | research/claude-main/2026-08-08-06-honest-downsides.md |
| system-cleaner-differentiation.md | research/claude-main/2026-08-08-07-differentiation.md |
| system-cleaner-vs-kudu.md | research/claude-main/2026-08-08-08-vs-kudu.md |
| system-cleaner-enterprise.md | research/claude-main/2026-08-08-09-enterprise-angle.md |
| system-cleaner-stack-and-roadmap.md | research/claude-main/2026-08-08-10-stack-and-roadmap.md |
| system-cleaner-voice-of-user.md | research/claude-main/2026-08-08-11-voice-of-user.md |
| rnd-toolchain-research.md | research/claude-main/2026-08-18-12-toolchain-research.md |
| EXTERNAL-AI-REVIEW-PROMPT.md | prompts/external-ai-review.md |
| probe-output.txt | measurements/2026-08-08-runtime-probe.txt |
| perf-output.txt | measurements/2026-08-08-perf-probe.txt |
| reg-test.csx | measurements/2026-08-08-registry-test.csx |

### Aggregated 4-persona critique landed

`research/claude-personas/CRITIQUE-SYNTHESIS.md` contains critique from 4 independent Claude subagents (senior-engineer, product-strategist, it-director, oss-maintainer).

Key convergent findings (recorded here for CHANGELOG completeness — full detail in the critique file):

1. Wave 6-7 timeline is fantasy — all four called it out with specifics.
2. Windows 10 first-class positioning is already stale (EOL Oct 2025).
3. Trust-maximalism claims (Ed25519 signed rules, offline-verifiable builds) are aspirational, not deliverable at solo-dev capacity.
4. Enterprise angle requires an actual company (LLC, E&O insurance, MSA/DPA templates).
5. **Research addiction** is the current #1 failure mode — 12+ briefs, 0 code changes.

**None of these have been merged into PLAN.md yet.** They are recorded as open items awaiting your decision on whether to accept + which changes to make. See ADR-0006 (proposed) and ADR-0007 (proposed).

---

## Format for future entries

```
## YYYY-MM-DD

### [Change title]

Short description of what changed and why.

**Related:**
- ADR-NNNN — [decision title]
- research/<path>/<file>.md — triggering research

**Impact:**
- PLAN.md sections updated: [list]
- CHANGELOG entry added
```

Keep entries append-only. If a later entry supersedes an earlier one, note it in the newer entry rather than editing the older.
