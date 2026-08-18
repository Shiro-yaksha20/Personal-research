# Changelog

Plan-change history. Every time PLAN.md is meaningfully updated, log it here with the date, what changed, and which research/ADR triggered it.

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
