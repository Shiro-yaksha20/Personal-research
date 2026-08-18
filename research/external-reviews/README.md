# External Reviews — Users, Reddit, HN, Forums

For feedback from real humans outside AI. Once SystemCleaner is shared publicly (r/sysadmin, r/pcmasterrace, r/Windows10, HN, Winget listing, etc.), capture the responses here.

## What goes here

- Reddit thread archives (link + key quotes + your reflections)
- Hacker News comment threads
- Forum posts / responses
- Direct user email feedback (anonymized)
- App store reviews when applicable
- Blog post or YouTube review references

## Structure suggestion

`YYYY-MM-DD-source-topic.md` — one file per external thread or review.

## Frontmatter suggestion

```yaml
---
title: r/sysadmin thread on SystemCleaner v1.4.0 release
source: external-review
date: YYYY-MM-DD
status: raw
topics: [tags]
platform: reddit-sysadmin        # or hn, twitter, forum-name, email
url: https://...
sentiment: positive|mixed|negative
key_asks: [tags]
---
```

## Why this matters

The persona critiques and external AI reviews are useful but abstracted. **Real user response is the highest-signal data.** Track it here so patterns are visible over time.

**Concrete moments to capture:**

- v1.0.1 release announcement (Winget, GitHub Releases page)
- v1.3.0 "safe uninstaller" release — this is the positioning test
- v1.4.0 "runs on modest hardware" release — this is the weak-hardware positioning test
- Any critique that reaches 100+ upvotes on Reddit
- Any user who takes time to write a substantive negative review

Aggregate quarterly (or after each release) into a "state of external feedback" note that informs the next planning cycle.
