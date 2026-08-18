# Recommended Obsidian Plugins

This vault ships pre-declaring 4 core community plugins in `community-plugins.json`. Obsidian doesn't auto-install them — you have to enable Community Plugins in Settings first, then install these individually. Instructions below.

## Core four (install these first)

| Plugin | Author | Why |
|---|---|---|
| **Git** (also called Obsidian Git) | denolehov | Automatic git commit + push + pull. Configure to pull on Obsidian startup, push every N minutes. This is what makes the vault sync across devices. |
| **Dataview** | blacksmithgu | Live queries over frontmatter. Enables the "show me all Proposed ADRs" / "show me all research from external-ai/gemini" queries. |
| **Templater** | SilentVoid13 | Templates with dynamic content (dates, filenames, prompts). Uses the `.obsidian/templates/` folder in this vault. |
| **Omnisearch** | scambier | Full-text search that's much better than the built-in one. |

**Install steps for each:**

1. Settings → Community plugins → "Turn on community plugins" (accept the warning).
2. Click "Browse" → search for the plugin name → Install → Enable.

## Optional but useful

| Plugin | Author | Why |
|---|---|---|
| **ChatGPT MD** | bramses | Chat with OpenAI/Anthropic/local LLMs inside Obsidian notes. Save AI responses directly into the vault as markdown. |
| **Smart Connections** | brianpetro | AI-suggested related notes. Uses OpenAI embeddings; costs a small amount per index. |
| **Kanban** | mgmeyers | Kanban boards inside notes. Useful for tracking which ADRs are Proposed vs Accepted at a glance. |
| **Excalidraw** | zsviczian | Draw diagrams inline. Useful for architecture sketches. |
| **Advanced Tables** | tgrosinger | Better markdown table editing. |

## Configuration notes

### Git plugin setup

Once installed:

1. Settings → Git → Set:
   - **Auto backup interval** = 10 minutes (or whatever cadence you prefer)
   - **Auto pull on startup** = true
   - **Commit message** = `docs: {{files}} updates from Obsidian`
2. If you have a GitHub remote, no further config needed — the plugin uses the vault's `.git/` folder.

### Templater setup

1. Settings → Templater → Template folder location → `.obsidian/templates/`
2. Assign a hotkey (default: none) to "Templater: Create new note from template" for quick access.
3. Optional: turn on "Trigger Templater on new file creation" so new files auto-populate frontmatter.

### Dataview setup

1. Settings → Dataview → Enable JavaScript queries (only if you plan to write complex queries; not needed for basic use).
2. Try the sample queries in `../README.md` (main repo README, "Dataview query examples").

## Plugin sprawl warning

Research consistently says >20 plugins slows Obsidian startup noticeably on modest hardware. Keep active plugins under 10 for a fast experience. You can install more but disable what you don't currently use.

## Sync options if you don't use GitHub

- **Obsidian Sync** — official service, $8/mo, end-to-end encrypted. Simpler than git for cross-device.
- **Syncthing** — free P2P sync, more DIY.
- **iCloud / OneDrive / Google Drive** — works but not recommended for git-tracked vaults (conflict resolution is messy).

For a git-tracked vault like this one, the Obsidian Git plugin is the right choice — free, cross-platform, and version-controlled.
