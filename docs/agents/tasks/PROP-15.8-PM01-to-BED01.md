---
type: task
prop_id: PROP-15.8
prop_root: PROP-15-persona-creation-platform
from: PM-01
to: BED-01
priority: P2
status: Pass
created: 2026-10-05
updated: 2026-10-05
depends_on: none
siblings: PROP-15.7 (SLOP, independent)
proposal: docs/agents/unexecuted_proposals/persona-creation-platform.md
execution_repo: https://github.com/House-VictoriAI/SoulCore.AI_CreatorEdition
title: Refresh on-disk starter packs so mentor/analyst keep vmWindowTitle
report: docs/agents/reports/PROP-15.8-BED01-to-PM01.md
---

# PROP-15.8 — Starter pack vmWindowTitle on disk

## Problem

QA desk smoke (15.6): operator-seeded mentor/analyst packs omit `vmWindowTitle`, so desktop title falls back to Tools `victoria-sandbox`. Code starters in `PersonaPack` already set `mentor-sandbox` / `analyst-sandbox` — likely stale JSON under `%LocalAppData%/SoulCore/personas/` not re-merged on EnsureSeeded.

## Solution

On seed/ensure: merge missing tool-path fields from built-in starters into existing on-disk packs (or rewrite starters when fields blank). Prove activate mentor/analyst resolves pack titles without Tools fallback when Host fallback is still victoria-sandbox.

## Acceptance

- [x] Fresh + already-seeded roots both end with mentor/analyst `VmWindowTitle` set
- [x] Test or smoke evidence
- [x] Report `docs/agents/reports/PROP-15.8-BED01-to-PM01.md`

## Do not

- Change quarantine model
- UI work
