---
type: report
prop_id: PROP-15.15
prop_root: PROP-15-persona-creation-platform
from: BED-01
to: PM-01
status: Pass
created: 2026-10-05
updated: 2026-10-05
slop_report: docs/agents/reports/PROP-15.14-SLOP01-to-PM01.md
findings: F1, F2
execution_repo: https://github.com/House-VictoriAI/SoulCore.AI_CreatorEdition
---

# PROP-15.15 — InferenceModel JSON hygiene (F1+F2) — Pass

## Verdict

**Pass.** Host surfaces now share one pair: `hostModel` + `resolvedInferenceModel`. `GET /api/inference/models` uses a single payload builder. Resolve algorithm unchanged. No UI work (FED **PROP-15.16**).

## Canonical JSON keys

| Meaning | Key | Surfaces |
| --- | --- | --- |
| Host `Inference:Model` default | `hostModel` | `/health`, `/settings/identity`, `/api/inference/models` |
| Pack→Host resolved chat model | `resolvedInferenceModel` | `/health`, `/settings/identity`, personas DTO/activate, `/api/inference/models` |
| Pack override | `inferenceModel` / `personaInferenceModel` | unchanged |

### `/health` alias decision

Kept `inference.model` as a **documented alias** of `resolvedInferenceModel` (PROP-15.11 clients / QA probes). Also emit `inference.resolvedInferenceModel` with the same value so the canonical pair is present everywhere.

### Removed (hard unify, no dual-write)

| Old key | Was on | Now |
| --- | --- | --- |
| `hostInferenceModel` | `/settings/identity` | `hostModel` |
| `resolvedModel` | `/api/inference/models` | `resolvedInferenceModel` |

CreatorEdition loopback only — no dual-write.

## F2 — Models payload builder

`InferenceApiEndpoints.BuildModelsPayload(...)` — disabled / null-client / success branches all call one shape (`models`, `available`, `detail`, `hostModel`, `personaInferenceModel`, `resolvedInferenceModel`, `baseUrl`).

## Done

1. **F1** — unify Host JSON field names (`WebApplicationExtensions`, `InferenceApiEndpoints`).
2. **F2** — single `BuildModelsPayload` helper.
3. **Tests** — `InferenceModelsPayloadTests` asserts canonical keys and absence of old names.
4. Personas DTO already used `resolvedInferenceModel` — no change.

## Files changed

### New
- `SoulCore/SoulCore.Protocol.Tests/InferenceModelsPayloadTests.cs`

### Modified
- `SoulCore/SoulCore.Host/Inference/InferenceApiEndpoints.cs`
- `SoulCore/SoulCore.Host/Hosting/WebApplicationExtensions.cs`
- `docs/agents/tasks/PROP-15.15-PM01-to-BED01.md` (status → Pass)

## Evidence

### Build

```text
dotnet build SoulCore/SoulCore.Protocol.Tests/SoulCore.Protocol.Tests.csproj -c Release --no-incremental

Build succeeded.
    1 Warning(s)   # pre-existing CA1416 VoiceSpeakService windows-only
    0 Error(s)
```

(Stopped running SoulCore.Host briefly so Release DLLs could copy — PID locked Host output.)

### Unit tests

```text
dotnet test SoulCore/SoulCore.Protocol.Tests/SoulCore.Protocol.Tests.csproj -c Release --filter "FullyQualifiedName~InferenceModelsPayload|FullyQualifiedName~InferenceModelRouting|FullyQualifiedName~OllamaListModels" --no-build

Passed!  - Failed:     0, Passed:    14, Skipped:     0, Total:    14
```

## Unblocks / handoff

- **PROP-15.16 (FED)** — map client DTO `resolvedModel` → `resolvedInferenceModel` (and F3 normalize). Until FED lands, ChatDesktop `ListInferenceModelsAsync` may read null for resolved (JsonPropertyName still `resolvedModel`); `hostModel` already matches.
- OPS: restart Host if needed after this BED stop.

## Acceptance

- [x] Consistent JSON keys across health / identity / personas / inference/models
- [x] Models endpoint one builder
- [x] Tests updated; report this file
