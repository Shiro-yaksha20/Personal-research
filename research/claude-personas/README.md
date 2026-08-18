# Claude Personas — Critique Research

Fresh Claude subagents spawned with distinct persona prompts to critique the SystemCleaner plan. Each starts with cold context (no memory of the main planning sessions) and produces independent critique from a specific lens.

**Important disclaimer:** These are all Claude (same model family). Not truly "other AI" in the strict sense — same underlying weights and training. Their independence comes from starting cold and reading only PLAN.md + PLAN-SUMMARY.md. For genuinely different model perspectives, use `../external-ai/`.

## The four personas

| Folder | Persona | Lens |
|---|---|---|
| [`senior-engineer/`](./senior-engineer/) | Skeptical Windows software engineer, 15+ years | Technical fragility, package/library choices, architecture patterns, runtime measurement accuracy |
| [`product-strategist/`](./product-strategist/) | Product strategist with dev-tools + Windows software experience | Market positioning, VirusTotal moat, "weak hardware" as differentiator, go-to-market realism |
| [`it-director/`](./it-director/) | IT director at 500-2000 endpoint org | Pilot blockers, audit-log credibility, MSI deployment, compliance frameworks, support/liability |
| [`oss-maintainer/`](./oss-maintainer/) | Veteran OSS Windows-utility maintainer | Solo-dev workload realism, scope-creep failure modes, burnout signals, refusal-list durability |

## Primary output: aggregated synthesis

**[CRITIQUE-SYNTHESIS.md](./CRITIQUE-SYNTHESIS.md)** is the single-file consolidation:

- Convergent critiques (where 2+ personas agreed) with attribution
- Individual persona critiques worth reading in full
- What survived the critique
- Concrete plan changes to make (feeding into ADR-0006 and ADR-0007)

Read the synthesis first; dip into individual persona folders only if you want the raw output verbatim (individual folders are currently placeholders for future critique runs — the 2026-08-18 critique lives in the synthesis file).

## When to spawn more persona critiques

Re-run when:

- PLAN.md changes substantially (new waves, dropped waves, positioning shift)
- New research surfaces that could change decisions
- A specific ADR needs adversarial review before acceptance
- You want a "second opinion" checkpoint before executing a wave

## Prompts used

The persona prompts are documented in `../../prompts/` (to be extracted from Aug 2026 critique session).
