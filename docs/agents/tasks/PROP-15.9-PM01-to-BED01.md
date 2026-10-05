---
type: task
prop_id: PROP-15.9
prop_root: PROP-15-persona-creation-platform
from: PM-01
to: BED-01
priority: P1
status: Pass
created: 2026-10-05
updated: 2026-10-05
depends_on: none
slop_report: docs/agents/reports/PROP-15.7-SLOP01-to-PM01.md
finding: F1
proposal: docs/agents/unexecuted_proposals/persona-creation-platform.md
execution_repo: https://github.com/House-VictoriAI/SoulCore.AI_CreatorEdition
report: docs/agents/reports/PROP-15.9-BED01-to-PM01.md
title: Dedupe PersonaPackStore.NormalizeId → PersonaMemoryPaths.NormalizePersonaId
---

# PROP-15.9 — SLOP F1 normalizer dedupe

## Problem

`PersonaPackStore.NormalizeId` duplicates `PersonaMemoryPaths.NormalizePersonaId` (same charset/length rules, different symbol). Drift risks quarantine key mismatch.

## Solution

Delete the private twin; call `PersonaMemoryPaths.NormalizePersonaId` from pack store (and any other Host call sites that reimplemented it). Keep behavior identical; add/adjust unit coverage if needed.

## Do not

- Change ChatDesktop (FED / F2–F3 pending user boundary decision)
- Widen into unrelated refactors

## Acceptance

- [x] Single normalizer owner in Config
- [x] Pack persist + memory path use the same API
- [x] Tests green; report `docs/agents/reports/PROP-15.9-BED01-to-PM01.md`
