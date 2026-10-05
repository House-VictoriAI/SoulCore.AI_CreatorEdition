---
type: task
prop_id: PROP-15.12b
prop_root: PROP-15-persona-creation-platform
from: PM-01
to: QA-01
priority: P1
status: Pass
created: 2026-10-05
updated: 2026-10-05
qa_report: docs/agents/reports/PROP-15.12b-QA01-to-PM01.md
depends_on: PROP-15.12 (Pass), PROP-15.11 (Pass), PROP-15.11b (Pass)
fed_report: docs/agents/reports/PROP-15.12-FED01-to-PM01.md
execution_repo: https://github.com/House-VictoriAI/SoulCore.AI_CreatorEdition
title: Smoke — per-persona model picker (settings only)
---

# PROP-15.12b — Model picker QA smoke

## Scope

1. ChatDesktop: Edit persona → Identity step shows model ComboBox; **no** global Presence chrome model dropdown.
2. Pack A set model X, Pack B set model Y (or blank = Host default); activate each; `/health` or identity shows resolved model matching pack.
3. Optional: one stub/real chat turn per pack if model_down blocks, note Partial with API evidence.

## Skip

- Full stress suite (15.13)
- Charter edits

## Report

`docs/agents/reports/PROP-15.12b-QA01-to-PM01.md`
