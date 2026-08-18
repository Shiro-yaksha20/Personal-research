# Human — Your Notes

Your own research, ideas, questions, and observations. This is where you put things that don't come from AI.

Examples:

- Ideas that come to you while riding the bus
- Questions to research later
- Observations from actually using SystemCleaner or competitors
- Notes from conversations
- Screenshots or design sketches
- Half-baked ideas worth revisiting

## Frontmatter suggestion

```yaml
---
title: [Descriptive title]
source: human
date: YYYY-MM-DD
status: raw
topics: [tags]
related: []
---
```

## Convention

Keep entries append-only for durability. If an idea evolves, write a new file linking back to the earlier one via `supersedes:` frontmatter rather than editing the older file.

If an idea moves into the plan, note the ADR that formalizes it and change `status: integrated-into-plan`.
