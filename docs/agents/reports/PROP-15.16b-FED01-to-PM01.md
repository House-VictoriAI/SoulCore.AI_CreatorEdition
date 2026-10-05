---
type: report
prop_id: PROP-15.16b
prop_root: PROP-15-persona-creation-platform
from: FED-01
to: PM-01
priority: P1
status: Pass
created: 2026-10-05
updated: 2026-10-05
execution_repo: https://github.com/House-VictoriAI/SoulCore.AI_CreatorEdition
bed_report: docs/agents/reports/PROP-15.15-BED01-to-PM01.md
ticket: docs/agents/tasks/PROP-15.16b-PM01-to-FED01.md
depends_on: PROP-15.15 (Pass), PROP-15.16 (Pass)
---

# PROP-15.16b — Client models DTO remap (resolvedInferenceModel) — Pass

## Verdict

**Pass (client).** ChatDesktop hard-cuts `GET /api/inference/models` deserialization to Host’s canonical `resolvedInferenceModel` (PROP-15.15 removed bare `resolvedModel`). Persona pack DTO already used `resolvedInferenceModel` — unchanged.

## Done

1. **`InferenceModelsDto`** — `JsonPropertyName("resolvedInferenceModel")`; C# property renamed `ResolvedModel` → `ResolvedInferenceModel`.
2. **`InferenceModelsSnapshot`** — same rename; `ListInferenceModelsAsync` maps + normalizes the new field.
3. **Wizard reader** — when pack resolved is empty, hint falls back to `snap.ResolvedInferenceModel` then `snap.HostModel`.
4. **Hard cut** — no dual-read of legacy `resolvedModel`.

## Key files

| File | Role |
| --- | --- |
| `House/House.ChatDesktop/Services/SoulCorePersonaClient.cs` | DTO/snapshot/list remap |
| `House/House.ChatDesktop/PersonaWizardWindow.axaml.cs` | resolution hint fallback |
| `docs/agents/tasks/PROP-15.16b-PM01-to-FED01.md` | status → Pass |

## Evidence

### Build (ChatDesktop)

```text
dotnet build House/House.ChatDesktop/House.ChatDesktop.csproj -c Release --no-incremental

Build succeeded.
    0 Warning(s)
    0 Error(s)
Time Elapsed 00:00:03.20
```

### Tests (persona / wizard)

```text
dotnet test House/House.ChatDesktop.Tests/House.ChatDesktop.Tests.csproj -c Release --filter "FullyQualifiedName~Persona"

Passed!  - Failed:     0, Passed:    24, Skipped:     0, Total:    24, Duration: 36 ms
```

### Acceptance checklist

- [x] Models list + resolution hint use `resolvedInferenceModel`
- [x] Tests green; report `docs/agents/reports/PROP-15.16b-FED01-to-PM01.md`

## Notes for PM

- Aligns CreatorEdition ChatDesktop with PROP-15.15 Host JSON. No UI chrome change.
- Requires Host with PROP-15.15 shipped (old `resolvedModel` no longer read).
