# SystemCleaner R&D

Persistent knowledge base for the **SystemCleaner** Windows utility. Every planning brief, competitor analysis, critique, measurement, and decision lives here — organised so it can be curated over time until the plan converges.

---

## The project in one page

### What SystemCleaner is

A **Windows-native maintenance suite** built on .NET 9 + WPF. Combines four features that usually live in separate apps:

1. **Cleanup** — remove temp files, browser caches, Windows crash dumps, etc. (like BleachBit)
2. **Uninstaller with residual scan** — remove Windows programs AND find leftover files/registry keys (like BCU or Revo, but with VirusTotal integrated)
3. **Startup manager** — control what runs at Windows login/boot
4. **Hardware monitor** — CPU/GPU/memory/temperature via LibreHardwareMonitor
5. **VirusTotal integration** — hash-check any file against 70+ antivirus engines, from inside the uninstall flow or via right-click Explorer

Existing source repo: `../systemcleaner/` (this R&D repo is separate).

### What SystemCleaner is *not*

Explicit refusals — these are marketing message, not oversights:

- **No registry cleaner** (Microsoft warns registry cleaners can break Windows)
- **No PC health score** (a marketing pseudo-metric)
- **No telemetry** (nothing sent to us, ever)
- **No auto-update by default** (post-CCleaner-scandal trust posture)
- **No bundled software** (ever)
- **No cloud requirement** (works fully offline; VT check is opt-in)
- **No cross-platform** (Windows-native by design; Kudu covers cross-platform)
- **No own malware scanner** (VirusTotal is the reputation moat instead)

### Who it's for

- **Windows users on modest hardware** — Celeron / 4 GB RAM / HDD / integrated GPU. Kudu (Electron, ~350 MB RAM) doesn't serve them well; SystemCleaner (native WPF, ~40 MB RAM) does.
- **Trust-first users** — the kind of person who reads the telemetry policy before installing.
- **Windows 10 users** (500M+ machines) — FluentCleaner requires Win11; SystemCleaner works on both.
- **Enterprise IT (medium-term)** — on-prem, self-hosted, MSI/MSIX-deployable, ADMX-configurable. Kudu Cloud is SaaS-only ($5-9/device); SystemCleaner Enterprise will be on-prem/free.

### Where it stands (as of Aug 2026)

- **Existing code:** WPF app that builds cleanly on .NET 9, passes 1 test, has real safety bugs (C1/C2/C3 in the residual cleanup path — see PLAN.md §4).
- **Distribution baseline:** 132 MB self-contained single-file publish today.
- **Runtime baseline** (measured on i5-7300U): `LibreHardwareMonitor.Open()` blocks startup for 2.7 s, HKLM\SOFTWARE walk for residual scan takes 28 s per app.
- **Positioning:** three pillars — Windows-native lightweight, VirusTotal-in-workflow uninstaller, on-prem enterprise (future).
- **Status:** planning phase. No fixes shipped yet. First planned release: **v1.0.1** (Wave 1 — zero-downside package updates + dead-weight removal + Winget manifest).

### What this R&D repo is for

**Not the code.** SystemCleaner's source lives in `../systemcleaner/`.

**This repo is:** every piece of research, every critique, every decision, every measurement that informs the code without being the code. Multi-source (Claude, Perplexity, ChatGPT/Gemini later, human notes), curated over time until the plan converges.

The value: when Shiro comes back after a week / month / break and thinks "why did I decide X?" — the answer is in one of the ADRs, with the research that led to it linked. No context lost.

---

## When you come back after a break, read in this order

1. **[STATUS.md](./STATUS.md)** — where the plan currently stands, what's moving, what's stable
2. **[PLAN.md](./PLAN.md)** — the canonical plan (single source of truth)
3. **[PLAN-SUMMARY.md](./PLAN-SUMMARY.md)** — 10-minute version for external review
4. **[CHANGELOG.md](./CHANGELOG.md)** — what's changed in the plan over time
5. **[decisions/](./decisions/)** — Accepted Architecture Decision Records (ADRs). Check the Proposed ones — they may be waiting on your call.
6. **[research/CROSS-REVIEW.md](./research/CROSS-REVIEW.md)** — how the different research sources triangulate

## Folder map

```
systemcleaner-rnd/
├── PLAN.md, PLAN-SUMMARY.md, STATUS.md, CHANGELOG.md    ← always up-to-date
├── decisions/                    ← Architecture Decision Records (ADRs)
├── research/
│   ├── CROSS-REVIEW.md           ← Perplexity vs persona-critique triangulation
│   ├── claude-main/              ← Planning briefs (main Claude session + Perplexity wave)
│   ├── claude-personas/          ← Subagent critique outputs by persona
│   ├── external-ai/              ← ChatGPT, Gemini, Grok, other Claude sessions
│   ├── human/                    ← Your own notes and ideas
│   └── external-reviews/         ← Reddit, HN, forum feedback when it comes
├── prompts/                      ← Reusable prompts for external AI review
├── measurements/                 ← Probe outputs, benchmarks, raw data
└── ARCHIVE/                      ← Superseded or rejected ideas kept for history
```

## Current ADR status

11 ADRs on file. Read them in `decisions/` for full detail; summary:

| ID | Title | Status |
|---|---|---|
| 0001 | Stay on WPF, not Avalonia or WinUI 3 | Accepted |
| 0002 | "Lightweight" = runtime cost on weak hardware, not binary size | Accepted |
| 0003 | Depth over breadth — three pillars, not fifteen features | Accepted |
| 0004 | VirusTotal integrated into uninstall workflow | Accepted |
| 0005 | Enterprise as on-prem/self-hosted, not SaaS | Accepted (Wave 7 under review) |
| 0006 | Cut Waves 6-7 from committed roadmap | **Proposed** — awaits your call |
| 0007 | Rewrite positioning as positive claim, not refusal list | **Proposed** — awaits your call |
| 0008 | Add Microsoft PC Manager to competitive landscape | **Proposed** (from Perplexity wave) |
| 0009 | VT "confidence and context" presentation, not verdict | **Proposed** (from Perplexity wave) |
| 0010 | Tone of voice — "good sysadmin explaining what they're about to do" | **Proposed** (from Perplexity wave) |
| 0011 | Release cadence and LTS strategy | **Proposed** (from Perplexity wave) |

## Curation workflow

The purpose of this repo is to move ideas through states over time:

```
raw research → reviewed → integrated-into-plan → superseded/archived
```

**When new research arrives (any source):**

1. Drop into `research/<source>/` with the frontmatter convention below.
2. If it proposes a plan change → open an ADR in `decisions/` with status "Proposed".
3. Review + critique (either inline in the ADR or as a separate critique file).
4. Decision: ADR moves to **Accepted / Rejected / Deferred**.
5. Accepted ADRs → integrate into `PLAN.md`, log in `CHANGELOG.md`.
6. Superseded ideas → move to `ARCHIVE/` with a reason.

**When you want another AI's opinion:**

1. Grab `prompts/external-ai-review.md` — the copy-paste-ready review prompt.
2. Also copy `PLAN-SUMMARY.md` for context.
3. Paste both into ChatGPT, Gemini, Grok, Perplexity, or a fresh Claude session.
4. Save the response into `research/external-ai/<vendor>/YYYY-MM-DD-topic.md` with frontmatter.
5. If it converges with existing critique → high-confidence issue; open an ADR.

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
| `topics` | List of tag slugs — pick from existing tags or invent new ones |
| `related` | Repo-relative paths to related notes |
| `supersedes` | Repo-relative paths this note replaces (only if applicable) |

## Recommended Obsidian setup

This repo is designed to be opened as an [Obsidian](https://obsidian.md/) vault, but every file is plain markdown — you can also just use VS Code, browse it on GitHub, or open with any text editor.

**On first open in Obsidian:**

1. Install → Open folder as vault → point at this folder.
2. Enable "Community Plugins" in Settings.
3. Install the recommended plugins (details in `.obsidian/community-plugins-recommended.md`):
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

Show all research from a specific source:
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
git commit -m "docs: <what changed>"
```

**Pushing to GitHub:**

```bash
git push
```

Remote is already configured to https://github.com/Shiro-yaksha20/Personal-research.

**Pulling from another device or after external commits (e.g., Perplexity work committed separately):**

```bash
git pull
```

If using the Obsidian Git plugin, all of this happens automatically — configure it to pull on Obsidian startup and push every N minutes.

## What lives here vs what doesn't

**Here:** research, design decisions, critique, measurements, plans, positioning, marketing content, competitive intel.

**Not here:** actual SystemCleaner source code, build artifacts, user data. Those live in the code repo.

## Sources of research so far

- **Claude Code sessions with Shiro** — 12 planning briefs (2026-08-08), 1 toolchain brief (2026-08-18) → `research/claude-main/`
- **Perplexity** — 6 research-wave briefs (2026-08-19) also under `research/claude-main/` (committed via GitHub as `0569c08`)
- **Claude persona subagents** — 4 critical lenses (senior-engineer, product-strategist, IT-director, OSS-maintainer) → `research/claude-personas/`
- **Runtime probes** — actual measurements from Shiro's i5-7300U → `measurements/`

**Not yet contributed:** external AI (ChatGPT, Gemini, Grok), external users (Reddit / HN feedback), Shiro's own notes.

## Contributing

Solo project for now. If contributors ever join, this repo's structure and workflow should scale — the frontmatter-driven curation model is proven for multi-author knowledge bases (Grafana, Kubernetes, and most large open-source projects use a similar pattern via ADRs).
