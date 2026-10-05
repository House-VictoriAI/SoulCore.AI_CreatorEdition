---
type: task
prop_id: PROP-15.16
prop_root: PROP-15-persona-creation-platform
from: PM-01
to: FED-01
priority: P2
status: Pass
created: 2026-10-05
updated: 2026-10-05
depends_on: none
slop_report: docs/agents/reports/PROP-15.14-SLOP01-to-PM01.md
finding: F3
siblings: PROP-15.15 (BED; align DTO property names if BED renames JSON keys)
execution_repo: https://github.com/House-VictoriAI/SoulCore.AI_CreatorEdition
title: Dedupe client InferenceModel normalize via PersonaWizardLogic
---

# PROP-15.16 — SLOP F3

Call `PersonaWizardLogic.NormalizeInferenceModel` from `SoulCorePersonaClient` write/`FromDto` (and list trim paths if applicable). Drop inline blank→null twins.

If PROP-15.15 renames JSON keys, update client DTO mapping to `hostModel` / `resolvedInferenceModel` in the same pass when those land (or follow immediately after BED).

## Acceptance

- [x] Single normalize helper on client paths
- [x] Tests green; report `docs/agents/reports/PROP-15.16-FED01-to-PM01.md`
