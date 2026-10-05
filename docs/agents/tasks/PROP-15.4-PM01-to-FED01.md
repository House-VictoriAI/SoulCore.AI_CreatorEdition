---
type: task
prop_id: PROP-15.4
prop_root: PROP-15-persona-creation-platform
from: PM-01
to: FED-01
priority: P1
status: Pass
created: 2026-10-05
updated: 2026-10-05
depends_on: PROP-15.1 (Pass)
report: docs/agents/reports/PROP-15.4-FED01-to-PM01.md
pm_note: Accepted Pass (client). Live /api/personas smoke deferred to Host restart + 15.6. Host tree builds green again as of accept.
proposal: docs/agents/unexecuted_proposals/persona-creation-platform.md
intake: docs/agents/tasks/PROP-15-TT01-to-PM01.md
execution_repo: https://github.com/House-VictoriAI/SoulCore.AI_CreatorEdition
title: Active-persona switcher + CreatorEdition shell de-brand
---

# PROP-15.4 — Switcher + CreatorEdition de-brand

## Problem

Presence shell still reads as House Victoria; there is no first-class active-persona switcher.

## Solution

1. Active-persona switcher control (list packs from Host; set-active)
2. Strip Victoria-required branding from CreatorEdition shell defaults (window title, empty states, hardwired “Victoria” copy). Neutral CreatorEdition brand OK.
3. Switching confirms quarantine (operator understands memories do not follow)

## Do not

- Full wizard (**15.3**)
- Change Host store paths (**15.2**)
- Use forbidden human names in UI (`Agents/AGENTS.md`)

## Acceptance

- [x] Operator can switch active persona in-app; Host session updates
- [x] Shell does not require Victoria branding to feel complete
- [x] Evidence in report

## Report

`docs/agents/reports/PROP-15.4-FED01-to-PM01.md` (Pass — live Host smoke blocked; see report Blockers)
