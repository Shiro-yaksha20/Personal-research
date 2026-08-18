# Archive

Superseded or explicitly rejected ideas kept for history.

## When to archive

Move a file here when:

- **Superseded** — a newer research brief or decision replaces it. Note the replacement in the file's `superseded_by:` frontmatter before moving.
- **Rejected outright** — an idea considered and turned down. Don't just delete — archive with a note explaining why.
- **Obsolete** — the assumption underlying the file changed (e.g., a package became archived, an API changed, the target audience shifted).

## When NOT to archive

- **ADRs** — even rejected ADRs stay in `../decisions/`. Rejection is a decision worth preserving in-place.
- **Historical PLAN.md versions** — versioned via git history. No need to archive copies.
- **Files that are just old** — old isn't a reason to archive. Only supersession is.

## Structure

Mirror the folder path from the source:

```
ARCHIVE/
├── research/
│   ├── claude-main/
│   │   └── [original filename with same name].md
│   └── external-ai/
│       └── ...
└── decisions/
    └── [never — ADRs stay in decisions/ even when rejected]
```

## Frontmatter update

Before moving a file to ARCHIVE/:

```yaml
---
title: [original title]
source: [original source]
date: [original date]
status: archived
archive_reason: [supersede | reject | obsolete]
archived_date: YYYY-MM-DD
archived_by: [who/what triggered the archival]
supersedes: [original]
superseded_by: [new file path]  # if supersede
---
```

Then move the file. Don't edit content — only frontmatter changes.

## Current archive

Empty. First entries will land when Wave 1 fixes supersede pieces of the initial audit.
