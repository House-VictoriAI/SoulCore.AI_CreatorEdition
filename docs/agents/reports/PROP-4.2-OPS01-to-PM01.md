---
type: report
prop_id: PROP-4.2
prop_root: PROP-4-presence-shell-honest-hud
from: OPS-01
to: PM-01
priority: P1
status: Partial
created: 2026-09-29
branch: cursor/prop42-presence-installer-9531
environment: Linux cloud agent (pack scripts Windows-only; app builds cross-platform)
ticket: docs/agents/tasks/PROP-4.2-PM01-to-OPS01.md
---

# PROP-4.2 — Presence installer + Update button (OPS-01 → PM-01)

**Verdict: Partial** — Velopack wiring, Update button/toast, Settings → Updates, and pack/shortcut scripts are in tree. Full Windows install + live GitHub feed smoke needs LinearThrone’s PC (run `pack-presence.ps1`, install Setup.exe, publish a release).

## What shipped

| Requirement | Status | Evidence |
| --- | --- | --- |
| `.ico` on app | Done (pre-existing) | `Assets/house-victoria.ico` + `ApplicationIcon` |
| Velopack startup hooks | Done | `Program.cs` → `VelopackApp.Build().SetArgs(args).Run()` |
| Update button (title chrome) | Done | `Update` next to SETTINGS |
| Settings → Updates | Done | Check / Download & restart + version + feed |
| Toast when update available | Done | `UpdateToastBar` |
| Pack → Setup.exe | Done (script) | `House/scripts/pack-presence.ps1` |
| Start menu shortcut (dev) | Done | `House/scripts/install-presence-shortcuts.ps1` |
| Update feed | Done | GitHub Releases default; override `HOUSE_VICTORIA_UPDATE_URL` or `ui-settings.json` `updateFeedUrl` |

## Operator recipe (Windows)

```powershell
# 1) Pack installer
powershell -NoProfile -ExecutionPolicy Bypass -File House/scripts/pack-presence.ps1 -Version 0.1.0

# 2) Install
# Run House\artifacts\presence-releases\Setup.exe

# 3) Publish updates
# Upload presence-releases/* to a GitHub Release on Linearthrone/SoulCore.AI
# (or host on HTTP and set HOUSE_VICTORIA_UPDATE_URL)

# 4) In Presence: click Update (or Settings → Updates → Check for updates)
```

## Cloud verification

```
dotnet build House/House.ChatDesktop -c Release
# succeeds with Velopack reference
```

`vpk pack` not run here (Windows packaging; this agent is Linux).

## Partial — remaining

1. First live Setup.exe install + Start Menu entry on operator PC.
2. Publish at least one GitHub Release with Velopack assets so Check for updates returns a real package.
3. Optional SEC follow-up: signed feed / Authenticode (ticket notes).

## Out of scope

- PROP-4.1 visual QA
- Rewriting House drawer
