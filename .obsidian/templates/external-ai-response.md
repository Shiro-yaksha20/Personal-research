---
title: <% tp.file.title %>
source: external-ai/[vendor]
date: <% tp.date.now("YYYY-MM-DD") %>
status: raw
topics: []
model: [gpt-4o | gemini-2.5-pro | grok-4 | claude-sonnet-4-5 | etc]
context_given: [PLAN.md, PLAN-SUMMARY.md, ...]
related: []
---

# <% tp.file.title %>

## Prompt used

[Which prompt file from ../../prompts/ was used, or the actual prompt text if custom]

## Context provided

[Which files were pasted in as context]

## Response (verbatim)

[The AI's raw response, unedited]

---

## My reflection on this response

[Your notes — did it converge with other critique? Did it find something new? Do you accept the critique or push back?]

## Follow-up actions

- [ ] Cross-reference with `research/claude-personas/CRITIQUE-SYNTHESIS.md`
- [ ] If it proposes changes, open an ADR in `../../decisions/`
- [ ] Log in CHANGELOG.md
