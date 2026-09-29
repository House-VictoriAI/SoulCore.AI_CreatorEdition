---
type: task
prop_id: PROP-4.2
prop_root: PROP-4-presence-shell-honest-hud
from: PM-01
to: OPS-01
priority: P1
status: In progress — Partial (code + scripts; Windows pack smoke pending)
created: 2026-09-05
updated: 2026-09-29
wave: wipeout-now
title: Presence installer + Start shortcut + Velopack update toast
depends_on: PROP-4.1 preferred (icon/assets)
proposal: docs/agents/unexecuted_proposals/presence-shell-honest-hud.md
intake: docs/agents/tasks/PROP-4-TT01-to-PM01.md
report: docs/agents/reports/PROP-4.2-OPS01-to-PM01.md
---

# PROP-4.2 — Installer + updates

## Solution

1. Normal Windows install: `.ico`, Start menu shortcut, **Velopack Setup.exe** (`House/scripts/pack-presence.ps1`).
2. Update channel: **Update button** + Settings → Updates + toast; feed = GitHub Releases (or `HOUSE_VICTORIA_UPDATE_URL`). Prefer download & restart when operator clicks Update.
3. Document signed-feed expectation for SEC follow-up if missing.

## Acceptance

- [x] Update button in Presence chrome + Settings → Updates
- [x] Pack script produces Setup.exe path documented
- [ ] Install like a normal app on operator PC (Setup.exe smoke)
- [ ] Live update from published release / feed
- [x] Report: `docs/agents/reports/PROP-4.2-OPS01-to-PM01.md`
