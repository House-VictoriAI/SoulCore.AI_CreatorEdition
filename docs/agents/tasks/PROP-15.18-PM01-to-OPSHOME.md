---
type: task
prop_id: PROP-15.18
prop_root: PROP-15-persona-creation-platform
from: PM-01
to: OPS-HOME
priority: P0
status: In Progress
created: 2026-10-05
updated: 2026-10-05
depends_on: PROP-15.13 (Fail)
qa_report: docs/agents/reports/PROP-15.13-QA01-to-PM01.md
execution_repo: https://github.com/House-VictoriAI/SoulCore.AI_CreatorEdition
title: Pin lexi during stress — find victoria activate / model mutate race
---

# PROP-15.18 — Feel-drive session pin

## Problem

During PROP-15.13 long WS run, active persona flipped to **victoria** + `gemma4:latest`, and lexi `inferenceModel` was mutated. Broke captures with `chat.model_down`.

## Action

1. Identify what else talks to `:7700` (ChatDesktop, other Host, scripts) that could `POST .../activate` or PUT lexi.
2. For feel-drive / stress: pin **lexi** + `mother:latest` — document procedure (close Presence switcher / don’t run parallel activators).
3. After BED 15.17, keep Host ready for QA re-run on lexi.
4. Optional: note pwsh 7 missing (product runner needs it).

## Report

`docs/agents/reports/PROP-15.18-OPSHOME-to-PM01.md`
