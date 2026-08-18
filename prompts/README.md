# Prompts

Reusable prompts for AI research and critique. Copy-paste-ready — grab the file, paste into the target AI's chat.

## Existing prompts

| File | Use case |
|---|---|
| [external-ai-review.md](./external-ai-review.md) | Ask any external AI (ChatGPT, Gemini, Grok, other Claude) to critique the plan. Paste with PLAN-SUMMARY.md. |

## To add

Prompts to write when needed:

- **Persona critique templates** — the four persona prompts used for the Claude subagent critiques (senior-engineer, product-strategist, it-director, oss-maintainer). Useful for re-running critiques on updated PLAN.md versions.
- **Voice-of-user research prompt** — how to systematically search Reddit / forums for a specific feature area.
- **Competitor deep-dive prompt** — template for investigating a new competing tool (analog of what we did for Kudu).
- **ADR proposal prompt** — helper prompt to structure a raw idea into a proper ADR.
- **Release-note prompt** — helper for writing release notes that hit the positioning/values notes consistently.

## Convention

Each prompt file starts with `# Usage` explaining when to use it, followed by the copy-paste prompt in a fenced block. Include placeholders in `[SQUARE_BRACKETS]` for the user to fill in.
