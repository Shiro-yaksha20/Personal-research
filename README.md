# SystemCleaner R&D

Persistent knowledge base for the SystemCleaner Windows utility. Every planning brief, competitor analysis, critique, measurement, and decision lives here — organised so it can be curated over time until the plan converges.

**The code repo is separate.** SystemCleaner's actual source lives elsewhere; this repo is for research, design, decisions, and everything that informs the code without being the code.

## When you come back after a break, read in this order

1. **[STATUS.md](./STATUS.md)** — where the plan currently stands, what's moving, what's stable
2. **[PLAN.md](./PLAN.md)** — the canonical plan (single source of truth)
3. **[PLAN-SUMMARY.md](./PLAN-SUMMARY.md)** — 10-minute version for external review
4. **[CHANGELOG.md](./CHANGELOG.md)** — what's changed in the plan over time
5. **[decisions/](./decisions/)** — accepted Architecture Decision Records (ADRs). Also check for any status:proposed ADRs waiting on you.

## Folder map

```
systemcleaner-rnd/
├── PLAN.md, PLAN-SUMMARY.md, STATUS.md, CHANGELOG.md    ← always up-to-date
├── decisions/                    ← Architecture Decision Records (ADRs)
├── research/
│   ├── claude-main/              ← Planning briefs (my work in this project)
│   ├── claude-personas/          ← Subagent critique outputs by persona
│   ├── external-ai/              ← ChatGPT, Gemini, Grok, other Claude sessions
│   ├── human/                    ← Your own notes and ideas
│   └── external-reviews/         ← Reddit, HN, forum feedback when it comes
├── prompts/                      ← Reusable prompts for external AI review
├── measurements/                 ← Probe outputs, benchmarks, raw data
└── ARCHIVE/                      ← Superseded or rejected ideas kept for history
```

## Curation workflow

The purpose of this repo is to move ideas through states over time:

```
raw research → reviewed → integrated-into-plan → superseded/archived
```

**When new research arrives (any source):**

1. Drop into `research/<source>/` with the frontmatter convention below.
2. If it proposes a plan change → open an ADR in `decisions/` with status "Proposed".
3. Review + critique (either as inline comments in the ADR or as a separate critique file).
4. Decision: ADR moves to **Accepted / Rejected / Deferred**.
5. Accepted ADRs → integrate into `PLAN.md`, log in `CHANGELOG.md`.
6. Superseded ideas → move to `ARCHIVE/` with a reason.

**When you want another AI's opinion:**

1. Grab `prompts/external-ai-review.md` — the copy-paste-ready review prompt.
2. Also copy `PLAN-SUMMARY.md` for context.
3. Paste both into ChatGPT, Gemini, Grok, or a fresh Claude session.
4. Save the response into `research/external-ai/<vendor>/YYYY-MM-DD-topic.md` with frontmatter.
5. If it converges with existing critiques → high-confidence issue, open an ADR.

## Frontmatter convention

Every research file and ADR gets a YAML frontmatter header. This is what makes queries and cross-referencing work.

```yaml
---
title: Feature-by-feature comparison of Windows utilities
source: claude-main
date: 2026-08-08
status: integrated-into-plan
topics: [competitors, positioning, uninstaller, hardware-monitor]
related:
  - research/claude-main/2026-08-08-07-differentiation.md
supersedes: []
---
```

**Fields:**

| Field | Values |
|---|---|
| `title` | Human-readable one-line title |
| `source` | `claude-main` \| `claude-personas/<persona>` \| `external-ai/<vendor>` \| `human` \| `external-review` |
| `date` | ISO date `YYYY-MM-DD` |
| `status` | `raw` \| `reviewed` \| `integrated-into-plan` \| `superseded` \| `archived` |
| `topics` | List of tag slugs — pick from any existing tags in other files or invent new ones |
| `related` | Repo-relative paths to related notes |
| `supersedes` | Repo-relative paths to notes this one replaces (only for superseding notes) |

## Recommended Obsidian setup

This repo is designed to be opened as an [Obsidian](https://obsidian.md/) vault, but every file is plain markdown — you can also just use VS Code, browse it on GitHub, or open with any text editor.

**On first open in Obsidian:**

1. Install → Open folder as vault → point at this folder.
2. Enable "Community Plugins" in Settings.
3. Install the recommended plugins (also listed in `.obsidian/community-plugins-recommended.md`):
   - **Git** (or **Gitless Sync**) — GitHub sync
   - **Dataview** — live queries over frontmatter
   - **Templater** — templates for new research files
   - **Omnisearch** — better full-text search
   - Optional: **ChatGPT MD** for AI chats saved directly into the vault
4. The `.obsidian/templates/` folder has starter templates that Templater uses.

**Dataview query examples once installed** (paste inside a note):

Show all proposed ADRs:
````
```dataview
LIST FROM "decisions"
WHERE status = "Proposed"
```
````

Show all research from external AI:
````
```dataview
LIST FROM "research/external-ai"
SORT date DESC
```
````

Show research related to positioning:
````
```dataview
TABLE source, status, date FROM "research"
WHERE contains(topics, "positioning")
SORT date DESC
```
````

## Naming conventions

- **Research files:** `YYYY-MM-DD-NN-topic-slug.md` (NN = numeric ordering for same-day multiple files)
- **ADRs:** `NNNN-decision-slug.md` (4-digit zero-padded)
- **Prompts:** `prompt-<use-case>.md` or descriptive `.md`
- **Measurements:** `YYYY-MM-DD-source-slug.txt`

## Git workflow

**Committing changes:**

```bash
git add .
git commit -m "docs: add ChatGPT critique of PLAN.md"
```

**Adding a GitHub remote later:**

```bash
gh repo create systemcleaner-rnd --private --source=. --remote=origin --push
```

or manually:

```bash
git remote add origin https://github.com/<you>/systemcleaner-rnd.git
git push -u origin main
```

**Pulling from another device:**

```bash
git pull
```

If using the Obsidian Git plugin, all of this happens automatically — configure it to pull on Obsidian startup and push every N minutes.

## What lives here vs what doesn't

**Here:** research, design decisions, critique, measurements, plans, positioning, marketing content, competitive intel.

**Not here:** actual SystemCleaner source code, build artifacts, user data. Those live in the code repo.

## Contributing

Solo project for now. If contributors ever join, this repo's structure and workflow should scale — the frontmatter-driven curation model is proven for multi-author knowledge bases (Grafana, Kubernetes, and most large open-source projects use a similar pattern via ADRs).
