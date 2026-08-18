# SystemCleaner — Code Review

Repo: https://github.com/Shiro-yaksha20/system-cleaner  
Last commit: **48f509c** (2025-12-04) — this is not a legacy project; the .NET target (net9.0), packages, and CI are already current. What it has is **safety and correctness debt** in the destructive paths (uninstaller, residual cleanup, file cleanup), plus a few outdated GitHub Actions and thin test coverage. Findings are ordered by blast-radius.

---

## Critical

### C1 — Registry residual parser targets the wrong hive; HKCU deletions land on HKLM
[SystemCleaner.Core/Uninstall/UninstallerService.cs:1398](SystemCleaner.Core/Uninstall/UninstallerService.cs)

`RegistryResidualScanner` builds paths as `$"{hive}\\{rootPath}"` at line 1047, where `hive.ToString()` yields the enum name — `"CurrentUser"` or `"LocalMachine"`. `RegistryCleanupHandler.TryParseRegistryTarget` then does:

```csharp
hive = hiveName.Equals("HKEY_CURRENT_USER", StringComparison.OrdinalIgnoreCase)
    ? RegistryHive.CurrentUser
    : RegistryHive.LocalMachine;   // <-- default when it doesn't match
```

`"CurrentUser"` never equals `"HKEY_CURRENT_USER"`, so **every** residual key/value is treated as HKLM. When a user cleans residuals scanned from HKCU, the handler calls `DeleteSubKeyTree` on HKLM with the same path — for common subtrees that exist in both hives (`Software\Google`, `Software\Microsoft\Windows\...`), the wrong system-wide subtree gets deleted. Where the path doesn't exist in HKLM the delete silently fails, hiding the bug from the happy path.

**Failure scenario:** Uninstall "Google Chrome". Scanner records the residual as `CurrentUser\Software\Google\...`. Handler parses hive as HKLM, opens `HKLM\Software\Google\...` and deletes it — taking out any HKLM Google config, including entries owned by other applications.

**Fix:** Store the hive as `"HKEY_CURRENT_USER"` / `"HKEY_LOCAL_MACHINE"` (or as an enum + separator sentinel), and reject unknown hive names in the parser instead of defaulting to HKLM.

---

### C2 — Residual scanners match by substring and destroy anything that matches
[SystemCleaner.Core/Uninstall/UninstallerService.cs:1051](SystemCleaner.Core/Uninstall/UninstallerService.cs) (registry)  
[SystemCleaner.Core/Uninstall/UninstallerService.cs:880](SystemCleaner.Core/Uninstall/UninstallerService.cs) (file system)  
[SystemCleaner.Core/Uninstall/UninstallerService.cs:1519](SystemCleaner.Core/Uninstall/UninstallerService.cs) (drivers)  
[SystemCleaner.Core/Uninstall/UninstallerService.cs:1104](SystemCleaner.Core/Uninstall/UninstallerService.cs) (services)

The residual tokens (line 1591) are just the app name normalized to alphanumeric-lowercase, minimum 4 chars. Every scanner then keeps anything whose name **contains** the token as substring, and the cleanup handlers act without any additional guard:

- `RegistryCleanupHandler` → `DeleteSubKeyTree(keyPath, throwOnMissingSubKey: false)` (line 1356)
- `DirectoryCleanupHandler` → `Directory.Delete(item.Path, recursive: true)` (line 1251)
- `ServiceCleanupHandler` → `sc.exe delete "<name>"` (line 1435)
- `DriverCleanupHandler` → `pnputil.exe /delete-driver "<inf>" /uninstall /force` (line 1551)

**Failure scenarios:**

| Uninstall target | What matches (partial) | What gets destroyed |
|---|---|---|
| `Java 8 Update 351` | token `"java"` | any HKLM key with "java" in the path, e.g. JetBrains, JavaScript-related keys installed by other apps, Node.js modules |
| `Realtek Audio` | token `"realtek"` | every Realtek .inf in DriverStore — audio, LAN, USB card reader — force-uninstalled by pnputil |
| `Node.js` | token `"node"` | services and directories whose names contain "node", including many unrelated background services |
| `Steam` | token `"steam"` | files/keys mentioning "Steamworks" and unrelated products |

**Fix outline:** (1) require exact-match or start-with on registry key names, not substring; (2) whitelist safe roots (`Software\<Publisher>\<AppName>` structure); (3) never delete anything in `System32\drivers`, `WindowsApps`, `WinSxS`; (4) require user confirmation per item with the actual full path, not a per-batch confirmation; (5) drop the driver scanner altogether unless it looks up the driver's OEM-inf name via `pnputil /enum-drivers` and matches against the target app's install location.

---

### C3 — Uninstall commands piped through `cmd.exe /c` enable EoP when app is elevated
[SystemCleaner.Core/Uninstall/UninstallerService.cs:451](SystemCleaner.Core/Uninstall/UninstallerService.cs)

`BuildProcessStartInfo` takes the raw `UninstallString` from the registry and hands it to `cmd.exe /c "<expanded>"`. HKCU's uninstall subtree is **user-writable**, so any low-privileged code can register a fake "installed application" with:

```
UninstallString = notepad.exe & powershell -Command "..."
```

The README instructs users to run SystemCleaner elevated. When they do, clicking Uninstall on the malicious entry runs the attacker's payload in the elevated context — a local privilege escalation vector.

Even without elevation, `cmd.exe /c` interprets `&`, `|`, `<`, `>`, `%VAR%` in the string, so a benign uninstall string containing an `&` (rare but valid) is misinterpreted.

**Fix:** parse the uninstall string into program + args yourself (respecting quotes), then `Process.Start` with `UseShellExecute = false` and `ArgumentList` populated. Never route through `cmd.exe`. For MSI, detect and call `msiexec.exe` directly with `/x <ProductCode> /qn`.

---

## High

### H1 — VirusTotal API key placed in URL path (secret exposure)
[SystemCleaner.App/Services/VirusTotalService.cs:75](SystemCleaner.App/Services/VirusTotalService.cs)

```csharp
quota = await TryFetchQuotaAsync($"groups/{_apiKey}", token).ConfigureAwait(false);
```

The API key becomes part of the request URI. That URI ends up in TLS SNI-terminating proxies' logs, in .NET's `HttpEventSource` diagnostics, and in any HAR capture. VirusTotal's own docs specify `x-apikey` **header** only — the group endpoint expects a group ID, not the API key. Remove this fallback (or replace with a real group lookup if you actually intend to support enterprise accounts).

### H2 — HttpClient headers mutated without synchronization
[SystemCleaner.App/Services/VirusTotalService.cs:55-58](SystemCleaner.App/Services/VirusTotalService.cs)

`SetApiKey` calls `DefaultRequestHeaders.Remove/Add` on a shared `HttpClient`. If it fires while a request is in flight (Settings change triggers `SetApiKey` from the UI thread while a scan is running on a worker thread) the request can go out with a missing/stale key, or with two headers, or throw. Hold a lock, or (simpler) build a fresh `HttpRequestMessage` per call and set the header on the request instead of on `DefaultRequestHeaders`.

### H3 — Restore point creation silently fails / falsely reports success
[SystemCleaner.Core/Uninstall/UninstallerService.cs:501](SystemCleaner.Core/Uninstall/UninstallerService.cs)

`TryCreateRestorePoint` needs administrator rights (WMI SystemRestore). Under the current `asInvoker` manifest it always fails, but the UI still proceeds with the uninstall. Even elevated, Windows rate-limits restore points to one per 24 hours by default (`SystemRestorePointCreationFrequency`), so back-to-back uninstalls silently skip after the first. The returned string is "created" or "failed" but the caller in `UninstallAsync` doesn't gate uninstall on it. Either (a) fail-closed when the user opted in and the restore point genuinely didn't get created, or (b) set the `SystemRestorePointCreationFrequency` registry override before calling, or (c) remove the option and be honest that restore points aren't guaranteed.

### H4 — Cleanup / large / duplicate scanners don't skip reparse points during enumeration
[SystemCleaner.Core/Modules/LargeFileCleanupModule.cs:80](SystemCleaner.Core/Modules/LargeFileCleanupModule.cs)  
[SystemCleaner.Core/Modules/DuplicateCleanupModule.cs:140](SystemCleaner.Core/Modules/DuplicateCleanupModule.cs)

`FileSystemHelper.CleanDirectoryContents` skips reparse-point subdirectories (line 194) — good — but the Large-File and Duplicate scan modules recurse blindly. Junctions in Downloads/Documents (common with OneDrive/Dropbox placeholders or manual junctions) can loop or scan the same content repeatedly. Add the same `(directory.Attributes & FileAttributes.ReparsePoint) != 0` skip in both scanners.

### H5 — Registry cleanup and file cleanup catch and swallow the specific error
[SystemCleaner.Core/Uninstall/UninstallerService.cs:1360](SystemCleaner.Core/Uninstall/UninstallerService.cs)

`catch { /* Try next view */ }` and similar bare catches in `RegistryCleanupHandler`, `ServiceCleanupHandler`, `ScheduledTaskCleanupHandler`, `DriverCleanupHandler` return `false` with no reason. The user sees "cleanup completed" while ACL errors, active-service refusals, and locked-file failures pass silently. Surface these through the `messages` list that `CleanupResidualItemsAsync` already builds.

---

## Medium

### M1 — Deprecated GitHub Actions in the release workflow
[.github/workflows/release.yml:41,51](.github/workflows/release.yml)

`actions/create-release@v1` and `actions/upload-release-asset@v1` are archived (last updates 2021, deprecation notice on repos). Replace with `softprops/action-gh-release@v2` or `gh release create`.

### M2 — CI does not fail on vulnerable packages
[.github/workflows/ci.yml:39-40](.github/workflows/ci.yml)

`dotnet list SystemCleaner.sln package --vulnerable` prints results but its exit code is 0 even when vulns are found. Grep the output or add `--include-transitive` plus a follow-up check that fails the job on any non-empty match.

### M3 — Package/SDK drift
- [SystemCleaner.App/SystemCleaner.App.csproj:31](SystemCleaner.App/SystemCleaner.App.csproj) — `Microsoft.Extensions.DependencyInjection 8.0.0` on a `net9.0-windows` app. Move to `9.x`.
- [SystemCleaner.App/SystemCleaner.App.csproj:32-33](SystemCleaner.App/SystemCleaner.App.csproj) — SharpDX is **archived** (dead since 2019). Replace with `Vortice.DXGI` (drop-in replacement for the DXGI 1.1 adapter query you're doing).
- LibreHardwareMonitorLib 0.9.4 → current is newer; check for CI stability first.

### M4 — Empty loop in `FindResidualItemsAsync`
[SystemCleaner.Core/Uninstall/UninstallerService.cs:249-254](SystemCleaner.Core/Uninstall/UninstallerService.cs)

Dead code: iterates `issues` with only comments inside. Scanning errors are silently discarded. Either return them alongside the results (like `GetInstalledSoftwareAsync` does through the snapshot) or delete the loop.

### M5 — Random used for file shredding on SSDs
[SystemCleaner.Core/Uninstall/UninstallerService.cs:1282](SystemCleaner.Core/Uninstall/UninstallerService.cs)

`new Random()` is not cryptographically strong; more importantly, three-pass overwrite is **security theater on SSDs** because of wear-leveling — the erased data can survive in unmapped pages. If you keep the feature, use `RandomNumberGenerator.Fill` and put a "Not effective on SSDs" note in the UI. Better: drop the shred option and call `File.Delete` — anyone actually needing secure erase should use the drive's ATA Secure Erase.

### M6 — `DuplicateCleanupModule` opens files without `FileShare.Read`
[SystemCleaner.Core/Modules/DuplicateCleanupModule.cs:111](SystemCleaner.Core/Modules/DuplicateCleanupModule.cs)

`File.OpenRead` defaults to `FileShare.Read`, so this is actually fine — but the surrounding code catches all exceptions with `catch { continue; }`, hiding permission errors. Log them so users know when a duplicate group was skipped.

### M7 — `_isWaitingForQuota` read/write race
[SystemCleaner.App/Services/VirusTotalService.cs:864,924](SystemCleaner.App/Services/VirusTotalService.cs)

Non-atomic check-then-set. Low real-world impact but easy to fix — mark the field `volatile` or gate through `_rateLock`.

### M8 — Double dispose of DI singletons on shutdown
[SystemCleaner.App/App.xaml.cs:47-53](SystemCleaner.App/App.xaml.cs)

`OnExit` disposes `MainWindow.DataContext` (MainViewModel) which disposes `_hardwareMonitorService` and `_virusTotalService`; then `_serviceProvider.Dispose()` disposes the same singletons again. `HttpClient.Dispose` is idempotent so no crash, but relying on that is fragile. Let the container own singleton lifetimes — remove the manual disposes in MainViewModel or in App.OnExit, not both.

### M9 — Backwards-compat plaintext window for VirusTotal key
[SystemCleaner.App/Settings/AppSettingsService.cs:57-59](SystemCleaner.App/Settings/AppSettingsService.cs)

When a legacy `settings.json` still contains `VirusTotalApiKey`, the code deserializes it in plaintext, then migrates it to DPAPI on next save. Until the user triggers a save, the plaintext key remains in `settings.json`. Migrate immediately: after deserialize, if the plaintext field is populated, call `PersistVirusTotalKeyAsync` and re-save the settings file without the field, right there.

### M10 — Test coverage is a single happy-path scenario
[SystemCleaner.Tests/UnitTest1.cs](SystemCleaner.Tests/UnitTest1.cs)

One test, one module. Nothing exercises:
- `FileSystemHelper.IsRestrictedPath` (the load-bearing safety guard for cleanup)
- Reparse-point / symlink handling
- `RegistryCleanupHandler.TryParseRegistryTarget` (the bug in C1 would have been caught immediately)
- `ResidualTokenBuilder` matching (the bug in C2 would have surfaced)
- `StartupDiscoveryService` approval-state binary payload round-trip
- DPAPI settings persistence
- VirusTotal parser (a lot of `TryGet…` chains that will silently produce zeros)

For a tool that removes files and registry keys, this is the single highest-leverage improvement to make first — before shipping fixes for C1/C2/C3.

---

## Low

- **L1** [SystemCleaner.App/MainWindow.old.xaml.bak](SystemCleaner.App/MainWindow.old.xaml.bak) — 41 KB `.bak` file checked in. Delete.
- **L2** README claims Brave is supported for cache/extensions; the code enumerates only Chrome/Edge/Firefox paths ([UninstallerService.cs:696](SystemCleaner.Core/Uninstall/UninstallerService.cs), [CleanupModuleCatalog.cs:39](SystemCleaner.Core/Modules/CleanupModuleCatalog.cs)). Either add Brave or fix the README.
- **L3** [HardwareMonitorService.cs:35](SystemCleaner.App/Services/HardwareMonitorService.cs) — `Timer` callback re-entry: if `hardware.Update()` takes longer than `_pollInterval`, callbacks queue behind the `lock`. Set `dueTime`/`period` to `InfiniteTimeSpan` and re-arm at the end of the callback.
- **L4** [DiagnosticLogger.cs](SystemCleaner.App/Services/DiagnosticLogger.cs) — no size cap or rotation. A per-day file that grows unbounded is fine for a desktop tool, but consider a max size.
- **L5** [StartupDiscoveryService.cs:330](SystemCleaner.Core/Startup/StartupDiscoveryService.cs) — `Encoding.Default.GetString(bytes)` for arbitrary REG_BINARY values is meaningless; display as hex or skip.
- **L6** [SettingsPage.xaml.cs:14](SystemCleaner.App/Views/SettingsPage.xaml.cs) — reads `PasswordBox.Password` (string) rather than `SecurePassword`. The value is going to a header anyway, so it's a low-value hardening, but if you want to stay in `SecureString` land, keep it end-to-end.
- **L7** [CHANGELOG.md](CHANGELOG.md) — "Initial changelog scaffolding" is the only entry despite v2 having landed. Populate it, or drop the file.
- **L8** [SECURITY.md](SECURITY.md) — placeholder text ("update this section with a dedicated security contact address"). Replace with a real address before publishing more releases.
- **L9** [SystemCleaner.Core/Uninstall/UninstallerService.cs:1141](SystemCleaner.Core/Uninstall/UninstallerService.cs) — `ScheduledTaskResidualScanner` enumerates every file under `C:\Windows\System32\Tasks` recursively (can be many thousands under domain-joined machines) and does substring match on every filename. Add a token-length short-circuit and cap results.
- **L10** [SystemCleaner.Core/Uninstall/UninstallerService.cs:1418](SystemCleaner.Core/Uninstall/UninstallerService.cs) — `ServiceCleanupHandler` passes service names through `"..."` to `sc.exe`; a service name containing a `"` (rare but possible) breaks the command. Use `ProcessStartInfo.ArgumentList` instead of string-formatted arguments in all four handlers (`sc.exe`, `schtasks.exe`, `pnputil.exe`).

---

## Suggested order of work

1. **Fix C1 first** — it's a two-line change with the biggest safety win, and adding a `TryParseRegistryTarget` unit test would have caught it.
2. **Add tests around the destructive paths** (M10). You'll want them in place before touching C2/C3.
3. **Redesign residual matching** (C2) — this needs UX work, not just a code change. Consider showing every proposed deletion in a review list with the full path, greying out anything under system roots.
4. **Fix uninstall-string execution** (C3) — introduce a small `UninstallCommand` parser class that returns `(fileName, argumentList)`.
5. **Ship the small stuff** in a single housekeeping PR: L1, L7, L8, M1, M3 (DI package bump), the empty loop (M4).
6. **Migrate off SharpDX** (M3) — Vortice is API-similar and lets you drop two deprecated packages.
7. **Rewrite VirusTotal fallback** (H1) and tighten HttpClient header handling (H2).

Nothing about the overall shape of the codebase is bad — DI composition is clean, view models are properly separated, and the async/cancellation plumbing is done right. The problems are concentrated in one file (`UninstallerService.cs`, 1785 lines) that would benefit from being split by responsibility (inventory / residual scan / residual cleanup / uninstall runner) — each of those is currently a nested private class in one type.
