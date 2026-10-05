---
type: task
prop_id: PROP-15.6b
prop_root: PROP-15-persona-creation-platform
from: PM-01
to: OPS-HOME
priority: P0
status: Done
created: 2026-10-05
updated: 2026-10-05
depends_on: PROP-15.6 (Partial)
proposal: docs/agents/unexecuted_proposals/persona-creation-platform.md
qa_report: docs/agents/reports/PROP-15.6-QA01-to-PM01.md
execution_repo: https://github.com/House-VictoriAI/SoulCore.AI_CreatorEdition
title: Recycle CreatorEdition Host :7700 to persona tree; unlock 15.6 desk smoke
---

# PROP-15.6b â€” Restart operator Host on :7700

## Problem

QA **15.6 Partial**: product gates green on `:7710`, but operator Host at `127.0.0.1:7700` is still **v0.1.5** without `/api/personas` â€” ChatDesktop desk E2E cannot finish.

## Action

1. Stop the old Host on `:7700` (graceful if possible).
2. Start current CreatorEdition tree:  
   `dotnet run --project SoulCore/SoulCore.Host -c Release` (loopback `127.0.0.1:7700`).
3. Verify: `GET http://127.0.0.1:7700/health` shows `persona` / active pack; `GET http://127.0.0.1:7700/api/personas` â†’ 200 (not 404).
4. Optional: quick ChatDesktop open against `:7700` â€” persona switcher lists packs.

Do **not** echo secrets from `.env`.

## Acceptance

- [x] `:7700` serves current-tree personas API
- [x] Report with health + `/api/personas` evidence
- [x] Notify PM so QA can promote 15.6 Partial â†’ Pass (desk smoke)

## Report

`docs/agents/reports/PROP-15.6b-OPSHOME-to-PM01.md`
