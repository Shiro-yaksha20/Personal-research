# External AI — Cross-Model Critique

For truly cross-model opinions on the plan. Unlike `../claude-personas/` (all Claude, same model family), this folder collects responses from different AI vendors:

- [`chatgpt/`](./chatgpt/) — OpenAI GPT-4o, o1, GPT-5, etc.
- [`gemini/`](./gemini/) — Google Gemini 2.x
- [`grok/`](./grok/) — xAI Grok
- [`other-claude/`](./other-claude/) — Fresh Claude sessions on claude.ai (different from the subagents in claude-personas/)

## How to add a new response

1. Grab `../../prompts/external-ai-review.md` — the copy-paste-ready review prompt.
2. Also copy `../../PLAN-SUMMARY.md` (or the full PLAN.md for deeper detail).
3. Paste both into the target AI (ChatGPT, Gemini, etc.) in a fresh conversation.
4. Save the response into `<vendor>/YYYY-MM-DD-<topic>.md` with frontmatter:

```yaml
---
title: [What was reviewed and by whom]
source: external-ai/chatgpt   # (or gemini, grok, other-claude)
date: YYYY-MM-DD
status: raw
topics: [what was critiqued]
model: gpt-4o                # or gemini-2.5-pro, grok-4, claude-sonnet-4-5, etc.
context_given: PLAN.md, PLAN-SUMMARY.md   # (list of files pasted into context)
related: []
---
```

5. Update `../../CHANGELOG.md` with an entry noting the new external critique.
6. If it converges with existing critique (in `CRITIQUE-SYNTHESIS.md` or across other external responses), open an ADR in `../../decisions/`.

## Cross-model triangulation

**The value is convergence.** A concern flagged by:

- Only one AI = might be model-specific bias
- 2+ AIs from different vendors = real signal, worth acting on
- All 3+ AIs = high-confidence, likely change the plan

Look for:

- Weaknesses ALL models find → highest-priority fixes
- Weaknesses different models find in different areas → each model has a distinct blind-spot pattern; useful metadata
- Weaknesses NO models find → either genuinely non-issues, or all models share a blind spot (worth thinking about)

## Which models to prioritise

For the most useful spread:

1. **GPT-4o or GPT-5** (ChatGPT) — different training, strong on technical analysis
2. **Gemini 2.5 Pro** (gemini.google.com or AI Studio) — different training, notably good at seeing structural issues
3. **Grok** (x.com) — trained differently, notably direct
4. Optional: **DeepSeek R1**, **Kimi K2**, **Llama 4** via open-source frontends
5. Optional: **Fresh Claude session** on claude.ai — same family but truly cold context vs the subagents in claude-personas/

Pick 2–3 for real diversity; more than that is diminishing returns.
