# Decisions (ADRs)

**ADR = Architecture Decision Record.** Each file captures one load-bearing decision: the context, what we chose, why, what alternatives we rejected, and what we accept as consequence.

## Why ADRs?

- Future-you needs to know **why** a decision was made, not just **what** was decided.
- New reviewers (AI or human) can catch up quickly by reading the ADRs instead of piecing together the plan from 12 briefs.
- Reversing a decision later is easier when the original reasoning is explicit — you can see whether the premises still hold.

## ADR statuses

| Status | Meaning |
|---|---|
| **Proposed** | Under review. Not yet accepted. |
| **Accepted** | The current position. Merged into PLAN.md. |
| **Rejected** | Considered and turned down. Kept in this folder for history — do NOT move to ARCHIVE/. |
| **Deferred** | Not deciding now. Revisit later. |
| **Superseded by ADR-NNNN** | Replaced by a newer ADR. Kept for history. |

## When to write an ADR

Whenever a piece of research proposes a change to the plan that would be non-trivial to reverse. Examples:

- Framework/language choice
- Package selection when there are real alternatives
- Feature scope (in/out)
- Architecture pattern
- Business model
- Positioning claim
- Deployment/distribution approach

**Not every change needs an ADR.** Small refinements (tweak a paragraph in PLAN.md, adjust a wave estimate) go straight to PLAN.md + CHANGELOG.md.

## ADR template

Copy this for each new ADR. Also available at `.obsidian/templates/adr.md` if you use Templater.

```markdown
---
title: ADR-NNNN — [Decision title]
source: claude-main  # or wherever the decision came from
date: YYYY-MM-DD
status: Proposed  # Proposed | Accepted | Rejected | Deferred | Superseded
topics: [tag, tag]
supersedes: []
superseded_by: []
related:
  - research/<path>/<file>.md
---

# ADR-NNNN: [Decision title]

**Status:** Proposed | Accepted | Rejected | Deferred | Superseded
**Date:** YYYY-MM-DD
**Deciders:** Shiro-yaksha20 (with input from: [sources])

## Context

What problem are we solving? What constraints and forces are at play? What does the current situation look like without this decision?

## Decision

What did we decide? State it as a clear directive: "We will [X]."

## Rationale

Why is this the right choice given the context? What's the reasoning chain?

## Alternatives Considered

- **[Alternative A]** — Rejected because [reason]. Would have been the right choice if [condition].
- **[Alternative B]** — Rejected because [reason].
- **[Alternative C]** — Considered but deferred; revisit when [condition].

## Consequences

### Positive

- [Benefit 1]
- [Benefit 2]

### Negative

- [Cost 1]
- [Cost 2]

### Neutral

- [Neither good nor bad, but worth noting]

## References

- `research/claude-main/YYYY-MM-DD-XX-topic.md` — the research this decision draws from
- `research/claude-personas/persona/critique.md` — critique that influenced or challenged this decision
- External: [any web sources]
```

## Existing ADRs

See the numbered files in this folder. Also queryable via Dataview once Obsidian is set up:

````markdown
```dataview
TABLE status, date, topics FROM "decisions"
WHERE file.name != "README"
SORT file.name ASC
```
````

## Numbering

ADRs are numbered sequentially starting from 0001. Zero-padded to 4 digits. Never renumber — once assigned, the number is permanent, even if the ADR is rejected or superseded.
