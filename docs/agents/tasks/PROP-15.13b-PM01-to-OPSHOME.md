---
type: task
prop_id: PROP-15.13b
prop_root: PROP-15-persona-creation-platform
from: PM-01
to: OPS-HOME
priority: P0
status: Pass
created: 2026-10-05
updated: 2026-10-05
depends_on: PROP-15.13 (Blocked)
qa_report: docs/agents/reports/PROP-15.13-QA01-to-PM01.md
execution_repo: https://github.com/House-VictoriAI/SoulCore.AI_CreatorEdition
title: Point lexi at an installed Ollama model (unblock stress suite)
---

# PROP-15.13b â€” Unblock chat.model_down for lexi

## Problem

QA stress suite blocked: Host resolves `gemma4:latest` but Ollama has no such model â†’ every `chat.send` â†’ `chat.model_down`.

Installed local tools-capable models include: `mommy:latest`, `mother:latest`, `hf.co/HauhauCS/Qwen3.5-9B-Uncensored-HauhauCS-Aggressive:latest`.

## Action (prefer pack override â€” PROP-15.11)

1. `PUT http://127.0.0.1:7700/api/personas/lexi` with `inferenceModel` set to **`mother:latest`** (or `mommy:latest` if mother fails load). Keep other pack fields intact (GET first, merge).
2. `POST /api/personas/lexi/activate`
3. Confirm `/api/inference/models` or `/health` shows `resolvedModel` = that model (`modelSource=persona` if exposed).
4. One `chat.send` over `/ws` that returns `chat.done` (not `chat.model_down`). Stub is **not** enough for stress suite â€” need real model.

Do **not** pull multi-GB models unless pack override fails. Do not echo `.env` secrets.

## Acceptance

- [x] lexi active; resolved model installed
- [x] One real chat.done evidence
- [x] Report `docs/agents/reports/PROP-15.13b-OPSHOME-to-PM01.md`

## After

PM re-dispatches **PROP-15.13** QA against **lexi**.
