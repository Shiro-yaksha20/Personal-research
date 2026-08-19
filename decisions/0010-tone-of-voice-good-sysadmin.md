---
title: ADR-0010 — Tone of voice — "good sysadmin explaining what they are about to do"
source: perplexity
date: 2026-08-19
status: Proposed
topics: [tone-of-voice, ux, writing, marketing, brand]
related:
  - research/claude-main/2026-08-19-trust-ux-principles.md
  - research/CROSS-REVIEW.md
  - decisions/0007-rewrite-positioning-not-refusal-list.md
supersedes: []
---

# ADR-0010: Tone of voice — "good sysadmin explaining what they are about to do"

**Status:** Proposed
**Date:** 2026-08-19
**Deciders:** Shiro-yaksha20 (pending)

## Context

SystemCleaner's writing shows up in many places: README, release notes, in-app copy, error messages, confirmation dialogs, docs, marketing pages. Without an explicit tone-of-voice guideline, these surfaces drift — sometimes clinical, sometimes marketing-y, sometimes overly technical.

Perplexity's `2026-08-19-trust-ux-principles.md` proposes a specific tone anchor:

> *"The tone of messaging is closer to a good sysadmin explaining what they are about to do than to an optimizer promising miracles."*

This maps to:

- **NOT** the "your PC is broken, click FIX NOW" tone of legacy cleaners (CCleaner-era pattern)
- **NOT** the marketing-optimizer tone of freemium utilities ("Boost your PC by 200%!")
- **NOT** the clinical-technical tone of Sysinternals utilities (assumes you already know what an ASEP is)
- **YES** the tone of a competent friend or colleague explaining exactly what a proposed action does and letting you decide

## Decision (Proposed)

Adopt an explicit tone-of-voice guideline: **"good sysadmin explaining what they are about to do."** All written surfaces align.

### Practical rules

1. **Explain the action, then offer it.** Not "Click Clean to free up 2.3 GB." Instead: "This removes 2.3 GB of temporary files from your browser caches, downloaded installers you've already run, and Windows update artifacts. Restore points won't be affected. [Preview] [Clean]"

2. **Show your work.** For destructive operations, list what will change. Not "Cleanup complete." Instead: "Deleted 47 files (2.3 GB freed). Skipped 3 files that were in use. Preserved everything under System Restore."

3. **Never oversell.** "Boost", "Optimize", "Turbocharge", "Speed up your PC" — these words are banned in first-party copy. "Frees disk space" is honest; "makes your PC faster" is usually not measurable, so don't claim it.

4. **Never fear-monger.** "Your PC has 4,782 problems! Fix now!" — banned. If SystemCleaner finds nothing to clean, say so calmly.

5. **When declining, explain why.** Not "This action is disabled." Instead: "This action needs administrator privileges. Restart SystemCleaner as administrator to enable it, or configure Group Policy to allow it for standard users."

6. **When VT flags something, explain what VT is.** Not "Malicious." Instead: "3 of 72 antivirus engines on VirusTotal flag this file. First seen: 2024-11-14. [See full report] [Continue anyway] [Cancel uninstall]"

7. **Sound like a person, not a template.** Contractions are fine ("won't", "you're"). "Something went wrong" is fine when actually true. Formal precision matters where safety matters; casual is fine where it doesn't.

8. **In release notes: what changed and why. In marketing: what it does for you. In errors: what happened and what to try.** Don't mix them.

### Reference tones we're NOT emulating

- **CCleaner marketing:** "Your PC has [dramatic number] issues!"
- **IObit Advanced SystemCare:** "One-click boost!"
- **Registry cleaner ads:** "Fix your slow PC in seconds!"
- **Enterprise antivirus dashboards:** clinical detection strings without context

### Reference tones we're closer to

- **Sysinternals Autoruns:** clear, precise, assumes competence
- **Tailscale docs:** clear, precise, doesn't assume competence
- **Postgres release notes:** what changed, why, and any migration required
- **1Password prompts:** explains the action before executing

## Rationale

- The tone is a durable competitive differentiator. Every legacy Windows utility shouts. Almost none explains.
- It reinforces the trust-first positioning — you cannot make trust-first claims in an alarmist voice without contradiction.
- It reduces user support burden: users who understand what's happening file better bug reports and make better use of features.
- It's actually a Perplexity-highlighted UX principle, backed by community sentiment that treats "PC optimizer" as suspect.

## Alternatives Considered

- **Style-guide-free (let each release find its voice)** — Rejected. Predictable drift. Every solo project needs at least one style guide.
- **Cheerful marketing tone** — Rejected. Contradicts the trust-first pillar.
- **Fully technical/clinical tone** — Rejected. Alienates the "weak hardware" target audience, who are not universally sysadmins themselves.
- **Formal tone (like Microsoft support docs)** — Rejected. Reads as corporate, not indie.

## Consequences

### Positive

- Coherent brand voice across surfaces.
- Every future PR review has a tone reference to test against.
- Distinctive vs every competitor's marketing tone.
- Users trust the tool more because the writing consistently levels with them.

### Negative

- Writing takes longer than "just ship the copy." Every dialog, every error, every button label needs to pass the tone test.
- Contributors need to internalize the tone or PR review will bounce copy changes.
- The tone works best in English; localization to other languages needs care to preserve the register.

### Neutral

- The tone guide will need updates as edge cases surface. That's fine — first version is a start.

## Examples of tone applied

### Before (current or plausible-CCleaner-style)

> "🚀 Boost your PC! Click Clean to fix 4,782 issues and reclaim disk space instantly!"

### After (good-sysadmin tone)

> "Ready to clean: 2.3 GB across 47 files, all in known-safe temp folders. [Preview details] [Clean now]"

### Before

> "ERROR: Access denied."

### After

> "Can't delete C:\Program Files\SomeApp — this folder is protected by Windows and requires administrator privileges. Right-click SystemCleaner and choose 'Run as administrator' to try again."

### Before

> "⚠️ Malicious file detected!"

### After

> "3 of 72 antivirus engines on VirusTotal flag this executable. First seen November 2024. This might be a false positive on legitimate software, or a real threat — the [full VirusTotal report](link) shows which engines and what they detected. [Cancel uninstall] [Uninstall anyway]"

## References

- research/claude-main/2026-08-19-trust-ux-principles.md — original source
- research/CROSS-REVIEW.md — §N2
- decisions/0007-rewrite-positioning-not-refusal-list.md — related positioning ADR
- decisions/0009-virustotal-confidence-and-context-presentation.md — sibling ADR on VT presentation
