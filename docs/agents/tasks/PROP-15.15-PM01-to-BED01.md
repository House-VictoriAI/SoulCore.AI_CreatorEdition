---
type: task
prop_id: PROP-15.15
prop_root: PROP-15-persona-creation-platform
from: PM-01
to: BED-01
priority: P1
status: Pass
created: 2026-10-05
updated: 2026-10-05
depends_on: none
slop_report: docs/agents/reports/PROP-15.14-SLOP01-to-PM01.md
findings: F1, F2
siblings: PROP-15.16 (FED F3, independent)
execution_repo: https://github.com/House-VictoriAI/SoulCore.AI_CreatorEdition
title: Unify inference model JSON field names + dedupe models endpoint payload
---

# PROP-15.15 — SLOP F1 + F2 (InferenceModel JSON hygiene)

## F1 — Field-name sprawl

Unify Host surfaces to one pair:
- **`hostModel`** — Host `Inference:Model` default
- **`resolvedInferenceModel`** — active pack→Host resolved chat model

Keep pack override as **`inferenceModel`** / **`personaInferenceModel`** (already consistent).

On `/health`, `inference.model` may remain as documented alias of `resolvedInferenceModel` **or** be renamed — pick one and document in report. Remove `hostInferenceModel` / bare `resolvedModel` unless you add a one-release dual-write (prefer hard unify: CreatorEdition loopback only).

## F2 — Triplicated models payload

`InferenceApiEndpoints` — single shape builder for disabled / null-client / success branches.

## Do not

- Change resolve algorithm
- UI work (FED 15.16)

## Acceptance

- [x] Consistent JSON keys across health / identity / personas / inference/models
- [x] Models endpoint one builder
- [x] Tests updated; report `docs/agents/reports/PROP-15.15-BED01-to-PM01.md`
