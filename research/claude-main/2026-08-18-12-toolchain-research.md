# R&D Knowledge Base — Toolchain Research

Question: what's the best way to maintain SystemCleaner's R&D so you can (a) reach it anytime, (b) organise contributions from multiple AIs, (c) curate ideas over time until the plan converges?

## Options considered

| Option | What it is | Verdict |
|---|---|---|
| **Plain Git repo + markdown** | What I originally proposed. Just `.md` files in folders, edited in VS Code/Notepad, committed to git. | Fine minimum but no graph view, no backlinks, no queries. Curation is manual. |
| **Obsidian + Git sync** ⭐ | Local markdown vault (which IS the git repo). Obsidian gives graph view, backlinks, tag search, Dataview queries. Community plugin syncs to GitHub. | **Recommended.** Same underlying markdown, but with real curation tooling on top. |
| **Logseq** | Outliner — every line is a bullet-block. Journal-first workflow. | Wrong thinking model for our doc-heavy R&D. Great for daily notes; less good for long researched briefs. |
| **Notion** | Cloud SaaS. Great mobile, built-in AI, collaboration. | Proprietary format, cloud lock-in, breaks the "local markdown files in git" trust model that matches the SystemCleaner project itself. Ironic to research a trust-first tool in a not-trust-first tool. |
| **Dendron** | VS Code extension. Hierarchical file names (`decisions.001.stay-on-wpf.md`). | Structured but steep. Smaller ecosystem. Only if you live in VS Code and prefer strict hierarchy. |
| **GitHub Wiki** | Separate wiki repo attached to a GitHub project. Browser editing. | Not versioned alongside code, wiki-markdown quirks, limited linking. Fine for reference docs, poor for active R&D curation. |
| **Foam** (VS Code Roam-like) | VS Code extension. | Less active than Obsidian; smaller plugin ecosystem. |

## The recommendation: Obsidian + Git

Same underlying artifacts (markdown files in a git repo, exactly the structure I proposed earlier). Obsidian is a layer on top that adds curation superpowers, not a replacement.

### What Obsidian adds over "plain Git + Markdown"

- **Graph view.** See connections between briefs, decisions, critiques, research sources. Useful when 30+ files pile up.
- **Backlinks panel.** For any note, see every other note that links to it. Answers "which research supports this decision?"
- **Dataview queries.** Live queries like `LIST FROM #proposed AND #adr` — filter by status, source, date, topic.
- **Templater.** New research from ChatGPT/Gemini goes through a template that populates YAML frontmatter automatically.
- **Omnisearch.** Full-text search across everything, faster than `grep` and with UI.
- **AI plugins.** ChatGPT MD, Copilot, Smart Connections — chat with AI *inside* Obsidian, save results directly into vault.
- **Mobile app.** Obsidian on iOS/Android syncs via git (or paid Obsidian Sync at $8/mo) → **that's the "reach out anytime" answer.**

### What it doesn't lock you into

- Vault = plain markdown files in a folder. **Not a proprietary format.**
- If you ever hate Obsidian, just delete `.obsidian/` folder and you have a plain markdown repo. Zero lock-in.
- Every file is git-friendly. Every file is readable in VS Code/Notepad. Every file is `grep`-able.
- The `.obsidian/` folder (config + plugins) itself can be committed or gitignored — your choice.

### Cost

- **Obsidian desktop:** free forever for personal use.
- **Obsidian mobile:** free.
- **Sync options:**
  - **Free path:** Obsidian Git plugin OR Gitless Sync plugin → syncs vault to your GitHub repo. Works cross-device, needs GitHub. Small friction on first setup, then automatic.
  - **Paid path:** Obsidian Sync ($8/mo) — official end-to-end encrypted sync, no GitHub needed. Simpler but not needed if you're OK with git.
- **GitHub repo:** free for private repos.

### The 2026 signal

Obsidian's user base grew 22% year-over-year to ~1.5M users. Active plugin ecosystem. Real-time collaboration shipped in 1.8 (end-to-end encrypted). It's mature and getting more mature.

## Concrete setup for SystemCleaner R&D

**Step 1 — Create the vault (I do this)**

```
C:/Users/Shiro/Documents/project review update/systemcleaner-rnd/
```

Same structure I proposed earlier:
- `PLAN.md`, `PLAN-SUMMARY.md`, `STATUS.md`, `CHANGELOG.md`, `README.md` at root
- `decisions/` — ADRs
- `research/claude-main/` / `claude-personas/` / `external-ai/{chatgpt,gemini,grok,other-claude}/` / `human/` / `external-reviews/`
- `prompts/` — reusable AI review prompts
- `measurements/` — probe data
- `ARCHIVE/`
- `.obsidian/` — Obsidian config + plugins (I'll pre-configure)

**Step 2 — `git init` + initial commit (I do this)**

Committed with real commit messages. Ready to push to GitHub whenever you want.

**Step 3 — You install Obsidian**

Download from [obsidian.md](https://obsidian.md/) → install → "Open folder as vault" → point at the folder. That's it.

**Step 4 — Install these Obsidian plugins (I document the list)**

Core (free, install from Community Plugins):

- **Git** (by denolehov) OR **Gitless Sync** — sync vault to GitHub
- **Dataview** — queries over the vault
- **Templater** — templates with YAML frontmatter for new research
- **Omnisearch** — better search

Optional for AI workflow:

- **ChatGPT MD** — chat with ChatGPT/Claude/others inside Obsidian, save responses directly
- **Smart Connections** — AI-suggested related notes

**Step 5 — Push to GitHub (your call when you want to)**

I can `gh repo create` for you when you say the word. Private repo, one command.

**Step 6 — Mobile (whenever you want it)**

Install Obsidian mobile → configure Git plugin → your vault is accessible from your phone. Read the plan on the bus.

## Trade-offs to accept

- **Small learning curve for Obsidian.** ~30 minutes to feel comfortable, ~a week to know the plugins. Not steep, but not zero.
- **Wiki-link syntax `[[note-name]]` is slightly different from GitHub's markdown.** Doesn't render as links in GitHub UI (renders as plain text). Doesn't matter for R&D use; matters if you plan to publish parts as GitHub docs.
- **Plugin sprawl temptation.** Recommendation: cap at ~10 plugins. Research consistently says >20 slows startup.
- **`.obsidian/` folder** — includes personal editor state. Some people gitignore it; some commit the workspace config (plugins, settings) for cross-device consistency. Recommend committing plugin list + settings, gitignoring the ephemeral workspace state. I'll set up the `.gitignore` right.

## What we don't get with this setup

- **True real-time collaboration** — Obsidian 1.8 shipped this but it's paid. For a solo dev with multi-AI review, async via git PRs is the right model anyway.
- **A hosted UI for others to browse without installing Obsidian** — if you want external reviewers to browse the R&D via a web UI, options are (a) push to GitHub which renders markdown natively, or (b) later host with something like Quartz / Digital Garden generators.
- **Native mobile without paying** — free path uses git plugin on mobile; slightly clunkier than paid Obsidian Sync. Fine for reading, less smooth for editing.

## Summary of decision

**Use Obsidian on top of the git repo I was going to create anyway.** Same files, same structure, same git workflow. Obsidian adds curation superpowers (graph, backlinks, queries, templates) that will matter once you have 30-50 pieces of research from multiple AIs. Setup effort is small. Lock-in is zero (delete `.obsidian/` and you have a plain markdown repo). Reaches your phone via GitHub sync.

Sources:
- [Obsidian + GitHub free sync setup (Medium)](https://ymkfelix.medium.com/obsidian-github-the-free-sync-setup-f57a511c0c78)
- [A Private AI Knowledge Base — Obsidian, GitHub Sync, Cross-Platform AI Context](https://www.billmongan.com/posts/2026/05/obsidian-ai-vault/)
- [Best Note-Taking Apps for Developers 2026 (NexaSphere)](https://nexasphere.io/blog/best-note-taking-apps-developers-2026)
- [Obsidian vs Logseq vs Notion PKM Systems (dasroot.net 2026)](https://dasroot.net/posts/2026/03/obsidian-logseq-notion-pkm-systems-compared-2026/)
- [ChatGPT to Obsidian workflow (ChatExport AI)](https://chatexportai.com/blog/05-chatgpt-to-obsidian-workflow)
- [How to Organize AI Conversations Across ChatGPT, Claude, Gemini 2026](https://nexasphere.io/blog/organize-ai-conversations-chatgpt-claude-gemini-2026)
- [Obsidian + git repo, better than Notion (Medium)](https://jinggu-dev.medium.com/obsidian-git-repo-a-better-note-taking-alternative-than-notion-01c3481f83f5)
- [Use Obsidian in Your Git Repo (Axoga.to)](https://axoga.to/blog/use-obsidian-in-your-git-repo/)
