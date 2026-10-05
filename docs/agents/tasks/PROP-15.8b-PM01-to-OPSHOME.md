---
type: task
prop_id: PROP-15.8b
prop_root: PROP-15-persona-creation-platform
from: PM-01
to: OPS-HOME
priority: P1
status: Pass
created: 2026-10-05
updated: 2026-10-05
depends_on: PROP-15.8 (Pass)
bed_report: docs/agents/reports/PROP-15.8-BED01-to-PM01.md
execution_repo: https://github.com/House-VictoriAI/SoulCore.AI_CreatorEdition
title: Recycle :7700 so EnsureSeeded merges mentor/analyst vmWindowTitle
---

# PROP-15.8b â€” Host recycle after starter merge

## Action

1. Restart CreatorEdition Release Host on `127.0.0.1:7700` (stop current, start current tree).
2. Verify mentor/analyst packs expose non-empty `vmWindowTitle` (`mentor-sandbox` / `analyst-sandbox`) via `GET /api/personas` (or activate + `/settings/tools` resolved title).
3. Report `docs/agents/reports/PROP-15.8b-OPSHOME-to-PM01.md`.

Do not echo `.env` secrets.


## Result

**Pass.** See `docs/agents/reports/PROP-15.8b-OPSHOME-to-PM01.md`. mentor=`mentor-sandbox`, analyst=`analyst-sandbox` on `GET /api/personas` after Release recycle on `:7700`.
