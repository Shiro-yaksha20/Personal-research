# SystemCleaner — Honest Downsides of the Recommended Work

Everything I've recommended has a cost. This document is the balancing weight — what you're actually paying to get each of those wins, and what I might have understated in the previous briefs.

Organised by risk category, most consequential first.

---

## The three biggest real costs

### 1. C2 (residual match rewrite) will make the tool feel *less* thorough at first

The current substring match catches everything with "steam" in it — including things it shouldn't. **Users who trust that behaviour see the current version as "very thorough" and don't know it's dangerous.** After the rewrite:

- **The residual dialog shows fewer items.** Anchored match + confidence rating means the "Questionable" tier hides items the current build would have deleted silently.
- **Users perceive a regression.** "The new version doesn't find as much." A percentage of your users will genuinely prefer the broken behaviour because it deletes more.
- **Real support cost.** You will get "why does it miss X" bugs from people who don't understand that X was never their app's residual — it just contained the substring.

The mitigation is clear release notes and an "Aggressive" opt-in slider that reverts to substring behaviour, so users who insist on it can flip it back and own the consequences. That opt-in slider is extra work.

This is the same trade Revo Uninstaller made when they added Safe/Moderate/Advanced modes — Advanced still finds more, but with warnings. You'd be shipping something closer to Safe/Moderate.

### 2. Approach C (Avalonia + NativeAOT rewrite) is a real rewrite, not a port

I described 2–4 weekends. That's the *code* part. It doesn't include:

- **All XAML gets touched.** Bindings port with tweaks, but styles, control templates, triggers, `DataTemplate` inheritance, WPF-specific bits like `x:Static`, `MergedDictionaries` behaviour, `Freezable` semantics — every one of these has an "Avalonia does it slightly differently" gotcha.
- **`WindowChrome` and custom titlebar behaviour rewrite** — you have a custom titlebar with `DragMove` and per-state layout. Avalonia has different primitives.
- **Ecosystem is thinner.** Third-party controls that "just work" in WPF sometimes don't have Avalonia analogues, or have less-mature ones.
- **AOT trimming warnings.** Some will block build. Some will fire at runtime and only surface as "empty combobox" or "crash on that one code path." Debugging AOT issues is significantly harder than JIT.
- **You lose `System.Management` easily** — WMI works under AOT with source-generated bindings, but the ergonomic `ManagementObjectSearcher` string API is gone. Every WMI query has to be rewritten as a source-generated binding class.
- **`JsonSerializer` reflection path breaks.** Every JSON model needs a `[JsonSerializable]` attribute and a source-generated context. Fine but pervasive.
- **Testing surface doubles temporarily** — you'll want to keep the WPF build alive during migration for regression comparison.

Realistic effort: **3–6 focused weekends** if things go well; several months if they don't and you also learn Avalonia while doing it. This is a "when the tool has product-market fit" decision, not a "let's do this while I'm here" decision.

### 3. Feature-scope expansion doubles the support and test burden

The comparison doc listed several attractive additions: BCU-style confidence rating, Autoruns-scale startup enumeration, ~90 BleachBit-style cleaners, MFT scanning, richer HW monitoring. **Each of these is not free even when the code is small.**

- **Every new cleaner category is a support commitment.** "It deleted my legit VS Code extensions" happens once and you own it.
- **Autoruns-scale startup adds hundreds of rows** users have never seen. On a normal machine that's 172 entries where before there were 10. **A percentage of users will disable critical entries and blame the tool.** Autoruns gets away with this because it's Sysinternals; a small tool with the same behaviour gets blamed when things break.
- **Scheduled Tasks toggle needs admin for HKLM tasks.** More elevation friction UX to design.
- **Service management (auto-start toggle) is genuinely dangerous.** Disabling `WinDefend`, `wuauserv`, `BITS`, `RpcSs` bricks the machine. You need a warned list and probably a hardcoded refuse-list.
- **JSON-defined cleaners = user-editable code paths.** Someone will commit a bad JSON with a typo in a path (`"%APPDATA%/Adode/..."` matches nothing) and users won't know why the module is broken. You need schema validation and version pinning.
- **Every feature is a permission escalation vector.** Right now the tool's dangerous surface is one file (`UninstallerService.cs`). Add features and the surface fragments.

The real number to internalise: **doubling features roughly triples the "supported configuration matrix"** — Windows edition × elevation × drive type × antivirus × user setup. Bug reports pile up in the intersections.

---

## Medium-cost items I've been light on

### Lazy `HardwareMonitorService` moves cost, doesn't eliminate it

I sold this as "cold start drops by 2.7 s." True. What I didn't emphasise:

- **First click on the Monitor tab now takes 2.7 s** to show data (or shows an empty tab with a spinner). Users who ALWAYS check the monitor first will feel this as regression.
- **The `Open()` cost is on a background thread** if you do it right, but it's still 2.7 s before sensors report values — the tab looks empty for that time.
- Mitigation is to speculatively `Open()` on a background thread ~500 ms after the window shows, so it's usually done by the time the user gets there. But that reintroduces the "we're doing work you didn't ask for" pattern. **You can pick which cost you'd rather pay, but you don't get to erase it.**

### Turning off `IsNetworkEnabled` loses genuine features

I called those 43 network sensor variants "junk." Two of them are the actual Wi-Fi and Ethernet throughput sensors — the other 41 are filter-layer duplicates of the same underlying counter. Turning off `IsNetworkEnabled` outright means **you lose real-time network throughput display too.**

The correct fix is more code than "flip the flag": iterate hardware, keep the top-level Wi-Fi + Ethernet entries, discard the WFP/QoS/filter variants. That's another 30-line filter method to write and maintain.

### `RegNotifyChangeKeyValue` is P/Invoke with real edge cases

Sold as "no more polling for startup entries." True, but:

- **Native handle management.** If your wait handle leaks (thread killed, cancellation not handled), the kernel keeps the notification registered. Enough leaks and you exhaust registry-notification slots system-wide.
- **Registry redirection.** HKLM\SOFTWARE has 32-bit and 64-bit views (WOW6432Node). Watching one view doesn't notify on writes to the other. You need two watchers per logical location — six wait handles for the Run keys alone.
- **Key deletion.** If the watched key is deleted, the wait handle signals and then subsequent watches on the recreated key need re-setup.
- **Thread-pool wait handle registration** in .NET requires care around cancellation — botched cleanup is a hang on shutdown.

Not a reason to skip it, but "one afternoon of work" is optimistic. Budget 1–2 days including the tests.

### Cached residual walk assumes cache correctness

Beautiful in theory: walk HKLM\SOFTWARE once, keep the index, look up per app. The unstated cost:

- **~10 MB of managed heap held for the session.** On a 4 GB machine that's noise; still, it's a permanent baseline you added.
- **Cache invalidation is a design problem.** If the user installs a new app while SystemCleaner is running, the cache is stale until they hit Refresh — or you subscribe to `RegNotifyChangeKeyValue` on both trees and rebuild in background. Now you have both — cache complexity + native handles.
- **First app still costs 28 s.** If the typical user cleans residuals of one app at a time, you got them nothing.

Real value is when a user does bulk cleanup (BCU-style multi-select). If the UI doesn't push that flow, the caching investment is wasted.

### RenderCapability.Tier fallback theme is two visual systems forever

Sold as "detect Tier 0 and simplify the visuals." What that actually means:

- **Every new UI feature needs two variants.** New tab? Two skins. New dialog? Two skins. Every animation you add? Skip it in Tier 0.
- **A resource dictionary swap is the easy part.** The hard part is discipline — someone adds a `DropShadowEffect` to a new page next month and forgets it doesn't fall back to plain.
- **Testing on Tier 0** requires a Tier 0 machine, an intel HD 3000-era laptop, or `RenderOptions.ProcessRenderMode = SoftwareOnly` for local testing. Not usually part of anyone's dev loop.

**Cheaper alternative that gets 80% of the value:** don't build a low-fi theme. Just avoid `BlurEffect`, `DropShadowEffect`, gradient brushes, and animations *everywhere*. The visual is slightly less flashy on high-end machines but works acceptably on Tier 0 with no branching. Consider this before building a two-track system.

### "Performance Mode" preset is a permanent maintenance branch

Auto-detects weak hardware and applies a bundle of settings. Real cost:

- **Every subsequent feature must be tested in both modes.**
- **User confusion.** "Hardware Monitor shows less on my old laptop than my new one." — expected, but every user hitting that will ask why.
- **Auto-detection is imperfect.** SSHDs, RAM caches, hybrid CPUs, discrete + integrated GPU laptops — edge cases where the wrong mode kicks in.
- **Undocumented settings drift.** Users find the toggle, flip half the individual settings, then can't figure out how to get back to a known state.

The lower-risk alternative is a **single "Low-power mode" toggle in Settings** with no auto-detect: users who need it turn it on. Less magic, less support surface.

---

## The C1/C3 downsides I mostly glossed over

### C1 fix (registry hive parser) is safe in isolation only

C1 by itself changes the parser to correctly identify HKCU vs HKLM. In today's code where residuals are all treated as HKLM, most deletions silently fail because the wrong-hive keys don't exist. Fix C1 alone and the deletes start actually succeeding — **on the wrong keys, if C2 substring match is still in play.** In other words: **shipping C1 without C2 makes the tool more dangerous, not less.** The tests-first sequencing from the earlier review exists specifically to prevent that.

### C3 (uninstall string parsing) has a long tail of edge cases

`cmd.exe /c "<UninstallString>"` is dangerous but it also handles a lot:
- `%SystemRoot%` and other environment variables
- Legacy InstallShield uninstallers that expect a shell context
- Old MSI packages with unusual argument quoting
- 8.3 short paths in `UninstallString`

Rolling your own parser means testing against a diverse installed-software matrix. Realistically you will miss some MSI edge cases on first release and get "the tool doesn't uninstall X" reports. **Have a fallback path where the user can hit an "Advanced" button and see the raw UninstallString, and let them run it themselves** — much less angry than "SystemCleaner failed to uninstall."

Also: existing parsers (`System.CommandLine`'s tokenizer, or Windows' own `CommandLineToArgvW` via P/Invoke) can be reused. **`CommandLineToArgvW` is the same tokenizer Windows itself uses.** Not zero cost but nothing you'd have to invent.

---

## Systemic downsides of the overall direction

### The tool becomes another CCleaner clone if scope isn't intentional

Right now SystemCleaner is a small-scoped Windows utility. Adding Autoruns-scale startup + BCU-scale uninstall + BleachBit-scale cleaners + WizTree-scale scanning turns it into a **general-purpose maintenance suite**. That has consequences:

- **No differentiator.** "Yet another Windows cleaner" — competes with 15 mature products.
- **Users expect the whole set to be as good as the best specialist.** They won't judge Uninstaller against Task Manager, they'll judge it against Revo. They won't judge Hardware Monitor against nothing, they'll judge it against HWiNFO.
- **Reputation risk is asymmetric.** Doing 8 things at 70% quality is worse than doing 3 things at 95%.

If "big plans" means "reach parity with everything," think about whether the goal is *your personal use* (fine — go broad, ship what serves you) or *external users* (better — pick one or two features to make excellent).

### Two "Approach" tracks will co-exist longer than you'd like

Approach A (stay on WPF) buys you the ability to ship fixes now. Approach C (Avalonia migration) is aspirational. If you plan to eventually migrate, **every feature you add to Approach A is a feature to port later.** Adding a `RegNotifyChangeKeyValue` wrapper and a Performance Mode preset now means porting them to Avalonia later. If Approach C is genuinely on the roadmap, being deliberate about what you add now saves that migration cost — or just commit to A permanently and stop optimising for a future rewrite.

### The safety fixes (C1/C2/C3) block you from shipping features

Until C1/C2/C3 land, adding features means layering on top of known-dangerous code. That's not a downside of the recommended work — it's the reason it exists — but **it does mean the next 3–4 weeks of "fix things" work happens before any user-visible improvements.** From the user's perspective the tool may look stalled. If you have any users right now, communicate that.

### Contributors have to learn every one of these disciplines

Right now the codebase is a single-developer .NET WPF app. After the recommended changes it's:
- Lazy-loaded DI factories
- `RegNotifyChangeKeyValue` P/Invoke wrapper
- JSON-defined cleaner modules with schema validation
- Feature-module architecture (multi-project)
- Tier-detection and adaptive behaviour
- Prefix-hash duplicate detection with SSD/HDD adaptation
- Optional AOT roadmap constraints

**Onboarding a second contributor is much harder** than in the current codebase. Each of these adds a "thing you have to know before you can safely change X." For a hobby project run by one person, that's mostly fine — you can hold it all in your head. For a project you want to grow with contributors, this is a real cost you pay in every future PR review.

---

## What I would *not* be worried about

To balance the picture, some things I recommended have essentially no real downside:

- **Kill dead Mono POSIX weight** (2 MB of unused Linux support in LHM's transitive deps). Pure win.
- **Replace SharpDX with Vortice.DXGI.** SharpDX is unmaintained. Vortice is a drop-in with a slightly cleaner API. Only cost is learning ~50 lines of Vortice's API.
- **Timer re-arm at end of callback (HM4).** Zero downside, always correct.
- **Suspend HM on window minimise.** People who need a background monitor are running a background monitor, not a cleanup app.
- **Prefix hash for duplicate finder.** Same output, faster path. Only cost: 20 lines of code.
- **Depth cap on residual walk at 6.** Two lines. Skips maybe 0.5% of legitimate residuals; saves 70% of the walk cost.
- **Anchored match in residual scanner.** Two lines change (`StartsWith` + `EndsWith` on the segment name). Combined with confidence rating the C2 rewrite is the only "hard" residual work; the rest is small.
- **Winget manifest.** One JSON file in `microsoft/winget-pkgs`. Zero code impact.
- **Removing the checked-in `MainWindow.old.xaml.bak`** and updating deprecated GitHub Actions. Housekeeping.
- **Deferred instantiation via `Lazy<T>`** in DI — small refactor, no runtime downside except the first-tab-visit cost noted above.

The above are net-positive changes with negligible tradeoffs. Roughly a weekend of work total, and they clean up the codebase enough that everything harder becomes easier.

---

## Practical recommendation

Given the honest tradeoffs above, the pragmatic ordering:

1. **Do the "no real downside" list first** (Vortice, dead weight kill, timer re-arm, minimize-suspend, prefix hash, depth cap, anchored match, winget). One weekend. Zero regret risk. Ships useful improvement immediately.
2. **C1 fix + tests + C2 rewrite as one PR** — the most valuable single change but you have to accept the "seems less thorough" perception. Two weeks with tests done properly.
3. **Lazy `HardwareMonitorService` + suspend on minimise + kill Network/Controller categories** — makes the app feel snappy. One day. Accepts the "first Monitor tab visit is 2.7 s" tradeoff.
4. **Stop.** Ship a release. Get user feedback. Actually see whether the "big plans" features are what users ask for, or whether the safety improvements alone are enough.
5. **Then** decide about JSON cleaners / feature-modules / Autoruns-scope / Performance Mode / Approach C. Every one of those is a real cost that only makes sense if you have a real user need pulling it.

The single most valuable habit for the "huge plans" arc is: **build the smallest thing that answers the question "does anyone want this?" before you commit to the full build-out.** Every downside above is amplified when you build something expensive that turns out to be low-value.
