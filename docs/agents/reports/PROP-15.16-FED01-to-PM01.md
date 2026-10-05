---
type: report
prop_id: PROP-15.16
prop_root: PROP-15-persona-creation-platform
from: FED-01
to: PM-01
priority: P2
status: Pass
created: 2026-10-05
updated: 2026-10-05
execution_repo: https://github.com/House-VictoriAI/SoulCore.AI_CreatorEdition
slop_report: docs/agents/reports/PROP-15.14-SLOP01-to-PM01.md
finding: F3
ticket: docs/agents/tasks/PROP-15.16-PM01-to-FED01.md
siblings: PROP-15.15 (BED; JSON key unify not yet in tree)
---

# PROP-15.16 — Dedupe client InferenceModel normalize — Pass

## Verdict

**Pass (client).** `SoulCorePersonaClient` write / `FromDto` / models-list trim paths call `PersonaWizardLogic.NormalizeInferenceModel`. Inline blank→null twins removed. PROP-15.15 Host JSON rename not present in tree yet — no DTO key change this pass.

## Done

1. **Write payload** — `inferenceModel` uses `NormalizeInferenceModel` (blank/whitespace → null, else trim).
2. **`FromDto`** — pack `InferenceModel` + `ResolvedInferenceModel` use the same helper.
3. **`ListInferenceModelsAsync`** — models list + `HostModel` + `ResolvedModel` use the same helper.
4. **DTO keys** — persona read already maps `resolvedInferenceModel`; models endpoint still Host `resolvedModel` / `hostModel` (15.15 not landed). Follow-up when BED renames models payload to `resolvedInferenceModel`.

## Key files

| File | Role |
| --- | --- |
| `House/House.ChatDesktop/Services/SoulCorePersonaClient.cs` | call shared normalize; drop inline twins |
| `House/House.ChatDesktop/Services/PersonaWizardLogic.cs` | single helper (unchanged API) |

## Evidence

### Build (ChatDesktop)

```text
dotnet build House/House.ChatDesktop/House.ChatDesktop.csproj -c Release --no-incremental

Build succeeded.
    0 Warning(s)
    0 Error(s)
Time Elapsed 00:00:04.17
```

### Tests (persona / wizard)

```text
dotnet test House/House.ChatDesktop.Tests/House.ChatDesktop.Tests.csproj -c Release --filter "FullyQualifiedName~Persona"

Passed!  - Failed:     0, Passed:    24, Skipped:     0, Total:    24, Duration: 32 ms
```

### Acceptance checklist

- [x] Single normalize helper on client paths (write / FromDto / list trim)
- [x] Tests green; report `docs/agents/reports/PROP-15.16-FED01-to-PM01.md`

## Notes for PM

- SLOP F3 closed on CreatorEdition client. No UI behavior change.
- When PROP-15.15 lands `resolvedModel` → `resolvedInferenceModel` on `/api/inference/models`, FED should flip `InferenceModelsDto` `JsonPropertyName` (and snapshot naming if desired) in a tiny follow-up — persona pack DTO already aligned.
