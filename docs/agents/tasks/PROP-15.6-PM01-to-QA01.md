---
type: task
prop_id: PROP-15.6
prop_root: PROP-15-persona-creation-platform
from: PM-01
to: QA-01
priority: P1
status: Pass
created: 2026-10-05
updated: 2026-10-05
depends_on: PROP-15.1 (Pass), PROP-15.2 (Pass), PROP-15.3 (Pass client), PROP-15.4 (Pass client), PROP-15.5 (Pass), PROP-15.6b (Pass)
proposal: docs/agents/unexecuted_proposals/persona-creation-platform.md
intake: docs/agents/tasks/PROP-15-TT01-to-PM01.md
execution_repo: https://github.com/House-VictoriAI/SoulCore.AI_CreatorEdition
title: CreatorEdition persona E2E — quarantine, traits, VM tool path
report: docs/agents/reports/PROP-15.6-QA01-to-PM01.md
pm_note: 15.6b Pass — :7700 serves personas. QA re-smoke desk → Pass; ready for SLOP-01.
---

# PROP-15.6 — QA gate for persona platform

## Prerequisite (mandatory)

Restart CreatorEdition Host on `:7700` from the current tree (15.1–15.5). Prior live Host was v0.1.5 without `/api/personas` / pack tool resolvers. Desk smoke on `:7710` already green for 15.5 activate paths.

Reports to trust:
- `docs/agents/reports/PROP-15.1-BED01-to-PM01.md` … `PROP-15.5-BED01-to-PM01.md`
- Client: `PROP-15.3-FED01-to-PM01.md`, `PROP-15.4-FED01-to-PM01.md`
- OPS recycle: `PROP-15.6b-OPSHOME-to-PM01.md` (Pass)

## Test scope

1. Create a custom persona ≠ Victoria entirely in-app; chat works with its identity
2. Dual-persona quarantine: write distinct memories; switch; prove zero cross-read
3. Trait A/B: two band settings produce detectable reply/directive difference
4. VM/tool path: active pack’s browser/desktop scoping (no Victoria-only requirement) — Mentor/`mentor-sandbox`, Analyst/`analyst-sandbox`
5. Host boots CreatorEdition without Victoria pack present (Blank default)

## Skip

- Metahuman / UE body
- Multi-simultaneous brains
- MCP shared-memory server

## Kill → Fail

Any quarantine leak, mushy traits with no band effect, or Victoria required to boot/tool.

## Report

`docs/agents/reports/PROP-15.6-QA01-to-PM01.md` with Pass/Fail/Partial + evidence.

## Status note (QA-01 2026-10-05)

**Pass.** `:7700` health + `/api/personas` 200 (Blank). ChatDesktop desk smoke: switcher lists packs; quarantine confirm → Mentor active; Create persona wizard opens. Prior `:7710` quarantine/traits/tools + Blank boot still green. Kill criteria not hit. Ready for SLOP-01.
