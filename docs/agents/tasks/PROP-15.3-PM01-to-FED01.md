---
type: task
prop_id: PROP-15.3
prop_root: PROP-15-persona-creation-platform
from: PM-01
to: FED-01
priority: P1
status: Pass
created: 2026-10-05
updated: 2026-10-05
depends_on: PROP-15.1 (Pass), PROP-15.2 (Pass)
proposal: docs/agents/unexecuted_proposals/persona-creation-platform.md
intake: docs/agents/tasks/PROP-15-TT01-to-PM01.md
execution_repo: https://github.com/House-VictoriAI/SoulCore.AI_CreatorEdition
title: In-app persona create/edit wizard — templates, trait scales, charter
report: docs/agents/reports/PROP-15.3-FED01-to-PM01.md
pm_note: Accepted Pass (client). Host builds green again; live wizard smoke → Host restart + 15.6.
---

# PROP-15.3 — In-app creation & adjustment wizard

## Problem

Operators cannot create or tune personas in Presence; packs would only be editable via ops/files.

## Solution

Avalonia CreatorEdition UI:

1. Template gallery: Blank + starters from 15.1 pack store
2. Create wizard: name, human address, trait scales, charter editor
3. Edit existing pack: live scale/charter adjust → calls BED pack update (next-turn apply per 15.1)
4. Confirm quarantine messaging (no cross-persona memory)

## Do not

- Shell-wide de-brand / switcher chrome (**15.4**)
- Backend pack compiler (**15.1**)
- Metahuman body

## Acceptance

- [x] Operator can create a non-Victoria persona entirely in-app
- [x] Trait scales and charter save via Host APIs; next chat turn reflects changes
- [x] Templates selectable; Blank path works with zero Victoria required
- [x] Local UI verify evidence in report (screenshots or structured UI test notes)

## Report

`docs/agents/reports/PROP-15.3-FED01-to-PM01.md` — **Pass** (client). Live Host smoke blocked on Host compile (`PersonaApiEndpoints.ToDto`).
