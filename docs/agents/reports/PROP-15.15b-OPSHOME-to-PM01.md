---
type: report
prop_id: PROP-15.15b
prop_root: PROP-15-persona-creation-platform
from: OPS-HOME
to: PM-01
status: Pass
created: 2026-10-05
updated: 2026-10-05
execution_repo: https://github.com/House-VictoriAI/SoulCore.AI_CreatorEdition
title: Release Host :7700 restored; resolvedInferenceModel verified
---

# PROP-15.15b - Ensure :7700 Host after 15.15 rebuild - Pass

## Verdict

**Pass.** Release `SoulCore.Host` is listening on `127.0.0.1:7700`. `GET /api/inference/models` returns canonical `resolvedInferenceModel` (no legacy `resolvedModel`). Active persona remains **lexi** with pack `inferenceModel=mother:latest`.

## Actions

1. Probed `/health` — HTTP 200; Host PID **37072** listening on port **7700**.
2. Confirmed process path is Release output (`SoulCore.Host` under `bin\Release\...`).
3. Verified `GET /api/inference/models` JSON shape post-PROP-15.15.
4. Confirmed **lexi** still active and pack-scoped to `mother:latest` — no pack rewrite needed.

## Evidence

### Host

| Check | Result |
| --- | --- |
| `GET /health` | 200 |
| Bind | `127.0.0.1:7700` Listen (PID 37072) |
| Config | Release Host binary |

### `/api/inference/models`

| Key | Value |
| --- | --- |
| `resolvedInferenceModel` | `mother:latest` |
| `personaInferenceModel` | `mother:latest` |
| `hostModel` | `gemma4:latest` |
| `available` | true |
| `models[]` count | 8 |
| legacy `resolvedModel` | **absent** (raw JSON) |

### Persona continuity

| Check | Result |
| --- | --- |
| `activePersonaId` | `lexi` |
| lexi `isActive` | true |
| lexi `inferenceModel` | `mother:latest` |
| lexi `resolvedInferenceModel` | `mother:latest` |
| `/health` `modelSource` | `persona` |
| `/health` `resolvedInferenceModel` | `mother:latest` |

## Notes

- No restart required — Host was already up after BED’s brief stop for DLL copy.
- No secrets echoed; companion token unused for these probes.
