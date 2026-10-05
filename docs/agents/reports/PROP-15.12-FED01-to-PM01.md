---
type: report
prop_id: PROP-15.12
prop_root: PROP-15-persona-creation-platform
from: FED-01
to: PM-01
priority: P0
status: Pass
created: 2026-10-05
updated: 2026-10-05
execution_repo: https://github.com/House-VictoriAI/SoulCore.AI_CreatorEdition
proposal: docs/agents/unexecuted_proposals/persona-creation-platform.md
ticket: docs/agents/tasks/PROP-15.12-PM01-to-FED01.md
depends_on: PROP-15.11
---

# PROP-15.12 — Persona settings model picker — Pass

## Verdict

**Pass (client).** ChatDesktop can set `InferenceModel` for the pack being created/edited inside the persona wizard only. Blank = Host default. List from `GET /api/inference/models`; persist via POST/PUT `/api/personas*`. No global Presence chrome model dropdown.

## Done

1. **Persona DTOs** — `PersonaPackInfo` / `PersonaPackWrite` carry `InferenceModel` (+ `ResolvedInferenceModel` on read). Create/Update JSON includes the field (null omitted = clear / inherit).
2. **`ListInferenceModelsAsync`** — fail-soft probe of `GET /api/inference/models` (models + `hostModel`).
3. **Wizard UI** — ComboBox on Identity step (create + edit): “Host default” first, then Host tags; hint shows resolved override vs Host default for **this** pack.
4. **Logic helpers** — `BuildModelPickerOptions`, `FindModelOption`, `NormalizeInferenceModel`, `FormatModelResolutionHint`; create/update writes accept model.
5. **No chrome picker** — `MainWindow` unchanged (read-only System Inference status only).

## Key files

| File | Role |
| --- | --- |
| `House/House.ChatDesktop/Services/SoulCorePersonaClient.cs` | models GET; inferenceModel on DTO/write |
| `House/House.ChatDesktop/Services/PersonaWizardLogic.cs` | picker options + resolution hint |
| `House/House.ChatDesktop/PersonaWizardWindow.axaml` | model ComboBox in persona settings step |
| `House/House.ChatDesktop/PersonaWizardWindow.axaml.cs` | load/bind/save pack model |
| `House/House.ChatDesktop.Tests/PersonaWizardLogicTests.cs` | blank inherit / picker / persist tests |

## Evidence

### Build (ChatDesktop)

```text
dotnet build House/House.ChatDesktop/House.ChatDesktop.csproj -c Release --no-incremental

Build succeeded.
    0 Warning(s)
    0 Error(s)
Time Elapsed 00:00:03.03
```

### Tests (persona / wizard)

```text
dotnet test House/House.ChatDesktop.Tests/House.ChatDesktop.Tests.csproj -c Release --filter "FullyQualifiedName~Persona"

Passed!  - Failed:     0, Passed:    24, Skipped:     0, Total:    24, Duration: 156 ms
```

New coverage: NormalizeInferenceModel blank→null; Host-default-first picker + orphan override; FindModelOption; resolution hint override vs Host default; create/update write InferenceModel.

### Acceptance checklist

- [x] Can change model only from persona settings (wizard Identity step) for that persona
- [x] Switching personas uses a new wizard `editTarget` — model bound from that pack only
- [x] Saving persists via PUT/POST `inferenceModel`; blank = Host default (Host 15.11 routing)
- [x] Report `docs/agents/reports/PROP-15.12-FED01-to-PM01.md`

## Notes for PM

- Model list requires Host + Ollama tags when available; fail-soft still allows saving blank Host default or a typed-in orphan override already on the pack.
- Next chat turn uses the new model once Host has PROP-15.11 (already Pass) and the pack is active / reloaded.
- Recommend QA-01 smoke: edit pack A model ≠ pack B; activate A → chat; activate B → chat; confirm no chrome model dropdown.
