---
type: task
prop_id: PROP-15.5
prop_root: PROP-15-persona-creation-platform
from: PM-01
to: BED-01
priority: P1
status: Pass
created: 2026-10-05
updated: 2026-10-05
depends_on: PROP-15.1 (Pass), PROP-15.2 (Pass)
proposal: docs/agents/unexecuted_proposals/persona-creation-platform.md
intake: docs/agents/tasks/PROP-15-TT01-to-PM01.md
execution_repo: https://github.com/House-VictoriAI/SoulCore.AI_CreatorEdition
title: VM + tooling paths per PersonaPack (profile dir, sandbox title)
---

# PROP-15.5 — Persona-scoped VM + browser tooling

## Problem

`DesktopTargetWindowTitle=victoria-sandbox` and Playwright user-data dir are global Tools options. Second persona cannot own its own VM window / Chromium profile.

## Root cause

Tool bridges read Host-global `ToolsOptions` instead of active PersonaPack resolvers.

## Solution

1. Resolve `DesktopTargetWindowTitle` and `PlaywrightUserDataDir` from **active PersonaPack** (fall back only when pack leaves blank — document fallback).
2. Tool loop / desktop_* / Playwright bridge use the resolver; no Victoria hardcode required to run tools for a custom pack.
3. Desk smoke (Windows / home-pc via OPS-HOME if needed): active pack’s title/profile used when invoking browser or desktop tool path.

## Do not

- Multi-VM simultaneous (**Phase 4**)
- UI redesign
- Punch quarantine for shared browser cookies across personas

## Acceptance

- [x] Two packs with different titles/profile dirs → tools resolve correctly per active pack
- [x] Non-Victoria pack can exercise tool path without `victoria-sandbox` name required
- [x] Unit and/or desk smoke evidence in report

## Report

`docs/agents/reports/PROP-15.5-BED01-to-PM01.md`
