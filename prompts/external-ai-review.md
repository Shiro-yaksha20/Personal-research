# External AI Review — Copy/Paste Prompt

For getting **truly cross-model opinions** on the plan, paste this prompt into ChatGPT, Gemini, Grok, another Claude session, or any AI you want. The critique agents in the last message were all Claude (same model family, fresh context); this prompt lets you get non-Claude opinions.

**How to use:**
1. Open ChatGPT / Gemini / another AI in a new conversation.
2. Copy the block below.
3. Paste PLAN-SUMMARY.md contents where indicated.
4. Optionally also paste PLAN.md (60 KB) for deeper detail.

---

## The prompt to paste

> **Please review this technical plan for a Windows software utility. Be blunt, not diplomatic. Focus on identifying weaknesses, unrealistic assumptions, positioning problems, and technical fragility.**
>
> **Context: this is a solo-developer .NET 9 WPF Windows utility (cleanup + uninstaller + startup manager + hardware monitor + VirusTotal integration). The maintainer is looking for critical review before committing to executing the plan.**
>
> **I want your genuine assessment, not validation. Where the plan is fragile or wrong, say so with specific reasoning. Where it's actually reasonable, don't waste words agreeing — focus on the weaknesses.**
>
> **Specifically evaluate:**
>
> 1. Positioning coherence — is "no telemetry / no cloud / no auto-updates / no registry cleaner / no PC-health-score theatre" a real positioning statement, or a negative-space rant?
> 2. Is VirusTotal-check-before-uninstall a real user behaviour change, or an engineer's fantasy?
> 3. Is "runs on weak hardware" a real market differentiator or developer aesthetics?
> 4. Is the free-forever self-hosted enterprise pitch realistic vs. Kudu Cloud at $5/device?
> 5. Is the 7-wave roadmap realistic for a solo developer working weekends?
> 6. Which "explicit refusals" will get pressured by users later and crack first?
> 7. What's the biggest technical risk in the plan?
> 8. What's the biggest business/product risk?
> 9. What's missing that a competent competitor would already have?
> 10. If you had 30 seconds to give the maintainer one piece of advice, what would it be?
>
> **Structure your response as 5-10 numbered critique points with specific reasoning. Be direct.**
>
> **---**
>
> **[PASTE PLAN-SUMMARY.md CONTENT HERE]**

---

## Models worth asking

For genuine cross-model diversity, try 2-3 of these:

- **GPT-4 / GPT-4o / o1** (via ChatGPT) — different training, different opinion base than Claude
- **Gemini 2.0 / 2.5 Pro** (via gemini.google.com or AI Studio) — Google's model, different training
- **Grok 4** (via x.com) — xAI's model, notably direct
- **Llama 4 / DeepSeek R1 / Kimi K2** (via various frontends) — open-source model perspective
- **Another Claude session** (via claude.ai) — same model family but fresh context and no memory of our sessions

Compare the responses. **The convergent critiques across models are the highest-signal issues.** If ChatGPT, Gemini, and Claude all flag the same problem, that's real. If only one model flags it, it might be model-specific bias.

---

## What to do with the responses

1. **Aggregate.** List each critique point from each model.
2. **Cross-reference.** Which points did 2+ models converge on?
3. **Feed back to me.** Paste the aggregated external critique into our session and I'll compare it to the internal critique in CRITIQUE.md. The intersection is the highest-confidence set of things to actually change in the plan.
4. **Compare with CRITIQUE.md** — if external models converge on things our internal critique also found, that's very high confidence. If external models find NEW issues our internal critique missed, that's model-family-specific blind spots worth taking seriously.

The point isn't to seek consensus — it's to triangulate. A plan that survives critique from 3-4 independent perspectives (different personas + different model families) is a plan you can commit to.
