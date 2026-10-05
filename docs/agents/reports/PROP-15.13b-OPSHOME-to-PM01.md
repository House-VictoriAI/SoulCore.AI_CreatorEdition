---
type: report
prop_id: PROP-15.13b
prop_root: PROP-15-persona-creation-platform
from: OPS-HOME
to: PM-01
status: Pass
created: 2026-10-05
updated: 2026-10-05
execution_repo: https://github.com/House-VictoriAI/SoulCore.AI_CreatorEdition
title: lexi InferenceModel -> mother:latest; real chat.done
---

# PROP-15.13b - Point lexi at installed Ollama model - Pass

## Verdict

**Pass.** Pack **lexi** now has `inferenceModel=mother:latest`. Host resolves that model with `modelSource=persona`. One `/ws` `chat.send` returned real `chat.done` (`stub=false`, `provider=ollama`); no `chat.model_down`. Did not pull new multi-GB models.

## Actions

1. Confirmed Host `:7700` healthy; active persona **lexi**; host default still `gemma4:latest` (not installed on loopback Ollama).
2. Confirmed Ollama tags include `mother:latest` and `mommy:latest` (no `gemma4`).
3. `GET /api/personas/lexi` then `PUT` with same pack fields and `inferenceModel=mother:latest`.
4. `POST /api/personas/lexi/activate`.
5. Proved one WS `chat.send` -> `chat.done`.

## Evidence

### Pack + resolution

| Check | Result |
| --- | --- |
| `PUT /api/personas/lexi` | `inferenceModel=mother:latest`, `resolvedInferenceModel=mother:latest` |
| `POST .../activate` | OK; still active |
| `GET /health` inference | `model=mother:latest`, `personaInferenceModel=mother:latest`, `modelSource=persona`, `hostModel=gemma4:latest` |
| `GET /api/inference/models` | `resolvedModel=mother:latest`, `personaInferenceModel=mother:latest`; `mother:latest` in `models[]` |

### WS chat proof

| Item | Value |
| --- | --- |
| Endpoint | `ws://127.0.0.1:7700/ws` (open gate; companion token not required this seat) |
| Frame | `v=1` `type=chat.send` `payload.text` + `sessionId` |
| Session | `prop-15.13b-20261005032127` |
| Prompt | `Reply with exactly: PONG` |
| Frames seen | `presence.status` -> `emotion.snapshot` -> `loop.want` -> `chat.delta` -> **`chat.done`** |
| `chat.done` | `stub=False`, `provider=ollama`, reply `textLen=4` |
| Failures | No `error` / `chat.model_down` |

Elapsed ~77s (cold/warm load of `mother:latest` acceptable).

## Unblocks

- Re-dispatch **PROP-15.13** QA stress suite against **lexi** (real inference path).

## Notes

- Host appsettings `hostModel` remains `gemma4:latest`; override is pack-scoped (PROP-15.11 path). Fallback `mommy:latest` not needed.
- No secrets echoed; `.env` companion token absent/unused for this probe.
