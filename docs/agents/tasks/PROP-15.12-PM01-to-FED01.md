---
type: task
prop_id: PROP-15.12
prop_root: PROP-15-persona-creation-platform
from: PM-01
to: FED-01
priority: P0
status: Pass
created: 2026-10-05
updated: 2026-10-05
depends_on: PROP-15.11 (Pass)
proposal: docs/agents/unexecuted_proposals/persona-creation-platform.md
execution_repo: https://github.com/House-VictoriAI/SoulCore.AI_CreatorEdition
title: Persona settings — model picker (selected persona only)
report: docs/agents/reports/PROP-15.12-FED01-to-PM01.md
---

# PROP-15.12 — Model picker in persona settings only

## Problem

Operator must choose/change the LLM for the **selected persona** inside that persona’s settings — not a global Presence/Host setting that affects every brain.

## Solution (after 15.11 APIs)

1. In **edit persona** / persona settings UI (wizard edit or Settings entry for the active/selected pack — not window chrome, not a global Inference panel): control to pick model from Host list (`GET /api/inference/models` or whatever 15.11 ships).
2. Persist via `PUT /api/personas/{id}` (`InferenceModel` field). Changing the selection updates **that pack only**.
3. Show current resolved model (pack override vs “Host default”) so blank = inherit is clear.
4. Create flow: optional model on create; default blank = Host default is OK.
5. Do **not** add a global model dropdown on the main Presence chrome.

## Acceptance

- [x] Can change model only from persona settings for that persona
- [x] Switching personas does not show/edit the other pack’s model in the wrong place
- [x] Saving persists; next chat uses new model once Host has 15.11
- [x] Report `docs/agents/reports/PROP-15.12-FED01-to-PM01.md`

## Report

`docs/agents/reports/PROP-15.12-FED01-to-PM01.md` — **Pass** (FED-01)
