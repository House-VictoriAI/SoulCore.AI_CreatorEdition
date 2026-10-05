---
type: task
prop_id: PROP-15.15b
prop_root: PROP-15-persona-creation-platform
from: PM-01
to: OPS-HOME
priority: P1
status: Pass
created: 2026-10-05
updated: 2026-10-05
depends_on: PROP-15.15 (Pass)
execution_repo: https://github.com/House-VictoriAI/SoulCore.AI_CreatorEdition
title: Ensure :7700 Host running after 15.15 rebuild
---

# PROP-15.15b

BED stopped Host briefly for DLL copy. Ensure Release Host on `127.0.0.1:7700`. Verify `/api/inference/models` returns `resolvedInferenceModel` (not only legacy `resolvedModel`). Keep **lexi** â†’ `mother:latest` if still set.

Report: `docs/agents/reports/PROP-15.15b-OPSHOME-to-PM01.md`

