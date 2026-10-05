---
type: report
prop_id: PROP-15.11
prop_root: PROP-15-persona-creation-platform
from: BED-01
to: PM-01
status: Pass
created: 2026-10-05
execution_repo: https://github.com/House-VictoriAI/SoulCore.AI_CreatorEdition
---

# PROP-15.11 — Persona-scoped inference model — Pass

## Verdict

**Pass.** Optional `PersonaPack.InferenceModel` persists with the pack. Chat and tool-loop (when Host `Inference:ToolModel` unset) resolve pack → Host `Inference:Model`. Activate B → next resolve uses B. FED picker list via `GET /api/inference/models` (fail-soft). `/health` + `/settings/identity` + personas DTO expose resolved model.

## Fallback

| Knob | Order |
| --- | --- |
| Chat model | 1) `PersonaPack.InferenceModel` → 2) `Inference:Model` → 3) `gemma4:latest` |
| Tool-loop model | 1) UE-live `ToolModelUeLive` → 2) Host `ToolModel` → 3) chat resolve above |

Single-active only — framed via `IPersonaSession.TryGet(personaId)` for later multi-active.

## Done

1. **`PersonaPack.InferenceModel`** — optional string; clone + JSON persist.
2. **`InferenceModelRouting`** — pack override + session/`personaId` overloads.
3. **`OllamaInferenceClient`** — injects `IPersonaSession`; chat + tool paths use resolved model; `ListLocalModelsAsync` → Ollama `/api/tags`.
4. **`GET /api/inference/models`** — models list + `resolvedModel` / fail-soft empty.
5. **Personas API** — `inferenceModel` + `resolvedInferenceModel` on DTO; POST/PUT accept field.
6. **`/health`** — `inference.model` = resolved; also `hostModel`, `personaInferenceModel`, `modelSource`.
7. **`/settings/identity`** — `resolvedInferenceModel` + pack/host fields.
8. **Tests** — pack/host/activate-B routing; persist two packs; tags parse + fail-soft.

## Files changed

### New
- `SoulCore/SoulCore.Host/Inference/InferenceApiEndpoints.cs`
- `SoulCore/SoulCore.Protocol.Tests/OllamaListModelsTests.cs`

### Modified
- `SoulCore/SoulCore.Core/Persona/PersonaPack.cs`
- `SoulCore/SoulCore.Inference/Tooling/InferenceModelRouting.cs`
- `SoulCore/SoulCore.Inference/Clients/OllamaInferenceClient.cs`
- `SoulCore/SoulCore.Host/Persona/PersonaApiEndpoints.cs`
- `SoulCore/SoulCore.Host/Hosting/WebApplicationExtensions.cs`
- `SoulCore/SoulCore.Protocol.Tests/InferenceModelRoutingTests.cs`
- `SoulCore/SoulCore.Protocol.Tests/PersonaPackStoreTests.cs`
- `docs/agents/tasks/PROP-15.11-PM01-to-BED01.md` (status → Pass)
- `docs/agents/PROP_NUMBERING.md`

## Evidence

### Build

```text
dotnet build SoulCore/SoulCore.Host/SoulCore.Host.csproj -c Release --no-incremental

Build succeeded.
    1 Warning(s)   # pre-existing CA1416 VoiceSpeakService windows-only
    0 Error(s)
```

(Stopped running SoulCore.Host briefly so Release DLLs could copy.)

### Unit tests

```text
dotnet test SoulCore/SoulCore.Protocol.Tests/SoulCore.Protocol.Tests.csproj -c Release --filter "FullyQualifiedName~InferenceModelRouting|FullyQualifiedName~OllamaListModels|FullyQualifiedName~Upsert_TwoPersonas_PersistDistinctInferenceModels"

Passed!  - Failed:     0, Passed:    13, Skipped:     0, Total:    13
```

## Unblocks

- **PROP-15.12 (FED)** — persona settings model picker can call `GET /api/inference/models` and PUT `inferenceModel` on pack.

## Note for OPS

Host process was stopped for Release rebuild (file lock). Restart Host before desk smoke if needed.
