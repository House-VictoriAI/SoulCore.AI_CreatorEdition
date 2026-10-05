---
type: task
prop_id: PROP-15.16b
prop_root: PROP-15-persona-creation-platform
from: PM-01
to: FED-01
priority: P1
status: Pass
created: 2026-10-05
updated: 2026-10-05
depends_on: PROP-15.15 (Pass), PROP-15.16 (Pass)
bed_report: docs/agents/reports/PROP-15.15-BED01-to-PM01.md
execution_repo: https://github.com/House-VictoriAI/SoulCore.AI_CreatorEdition
title: Remap client models DTO — resolvedModel → resolvedInferenceModel
---

# PROP-15.16b — Align ChatDesktop to PROP-15.15 JSON keys

Host now emits `hostModel` + `resolvedInferenceModel` (removed bare `resolvedModel` / `hostInferenceModel`).

Update `SoulCorePersonaClient` (and any wizard/status readers) to deserialize `resolvedInferenceModel`. Keep working if both present during transition only if cheap — prefer hard cut to new keys.

## Acceptance

- [x] Models list + resolution hint use `resolvedInferenceModel`
- [x] Tests green; report `docs/agents/reports/PROP-15.16b-FED01-to-PM01.md`
