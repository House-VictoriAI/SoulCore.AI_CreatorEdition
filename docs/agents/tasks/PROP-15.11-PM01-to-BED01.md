---
type: task
prop_id: PROP-15.11
prop_root: PROP-15-persona-creation-platform
from: PM-01
to: BED-01
priority: P0
status: Pass
created: 2026-10-05
updated: 2026-10-05
depends_on: none
siblings: PROP-15.12 (FED, unblocked)
proposal: docs/agents/unexecuted_proposals/persona-creation-platform.md
execution_repo: https://github.com/House-VictoriAI/SoulCore.AI_CreatorEdition
title: Per-persona chat model on PersonaPack + resolve in inference
report: docs/agents/reports/PROP-15.11-BED01-to-PM01.md
---

# PROP-15.11 — Persona-scoped inference model

## Problem

LinearThrone needs to pick which LLM model each persona uses. Today chat/tool models resolve only from Host-global `InferenceOptions` (`InferenceModelRouting` → `Inference:Model`). Switching personas cannot change the model; there is no pack field or API.

## Root cause

`OllamaInferenceClient` / `InferenceModelRouting.ResolveChatModel` (and tool-model chain) read only `IOptions<InferenceOptions>`. `PersonaPack` has no inference model field.

## Solution

1. Add **`InferenceModel`** (string, optional) on `PersonaPack` — persisted with the pack. Empty/null → fall back to Host `Inference:Model` (same pattern as PROP-15.5 tool-path fallbacks).
2. Resolve chat (and tool-loop default when ToolModel unset) from **active** `IPersonaSession.GetActive().InferenceModel` before Host options. Frame by `personaId` (no global singleton rewrite later).
3. Expose on `/api/personas*` DTO + accept on POST/PUT (FED will edit only in persona settings).
4. Add **`GET /api/inference/models`** (or under personas) that lists models available from the configured Ollama/base URL for the picker. Fail soft with empty list + detail if Ollama unreachable — do not crash Host.
5. `/health` or `/settings/identity` should show **resolved** model for active persona (pack override vs Host fallback) for desk smoke.
6. Unit tests: pack set → Resolve uses pack; pack blank → Host; activate B with different model → next resolve uses B.

## Do not

- Global UI model picker outside persona settings (FED owns UI in 15.12)
- Per-persona embedding model in this slice (chat/tool only unless trivial)
- Multi-active concurrent models (v1 single-active)
- Punch quarantine / change memory paths

## Acceptance

- [x] Two personas can persist different `InferenceModel` values
- [x] Active persona’s model is what chat/tool loop uses; switch active → next turn uses the other
- [x] Blank pack field → Host `Inference:Model`
- [x] Model list endpoint usable by FED
- [x] Tests + report `docs/agents/reports/PROP-15.11-BED01-to-PM01.md`

## Report

`docs/agents/reports/PROP-15.11-BED01-to-PM01.md` — **Pass**
