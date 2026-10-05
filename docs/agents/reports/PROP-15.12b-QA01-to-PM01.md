---
type: report
prop_id: PROP-15.12b
prop_root: PROP-15-persona-creation-platform
from: QA-01
to: PM-01
priority: P1
status: Pass
created: 2026-10-05
updated: 2026-10-05
execution_repo: https://github.com/House-VictoriAI/SoulCore.AI_CreatorEdition
proposal: docs/agents/unexecuted_proposals/persona-creation-platform.md
ticket: docs/agents/tasks/PROP-15.12b-PM01-to-QA01.md
fed_report: docs/agents/reports/PROP-15.12-FED01-to-PM01.md
depends_on: PROP-15.12, PROP-15.11, PROP-15.11b
---

# PROP-15.12b — Per-persona model picker smoke — Pass

## Verdict: **Pass**

Operator Host `:7700` exposes `GET /api/inference/models` and persona-scoped resolve. Pack A (`mentor` → `mommy:latest`) vs Pack B (`analyst` → `mother:latest`) activate with matching `/health` + `/settings/identity` resolved models; blank pack inherits Host default (`gemma4:latest`, `modelSource=host`). Presence chrome has **no** model ComboBox (UIA: only `PersonaSwitcher`). Wizard Identity step owns `InferenceModelCombo` (source + FED 15.12). Client unit tests **24/24**. Ready for **SLOP-01**.

## Environment

| Target | Result |
| --- | --- |
| Host `127.0.0.1:7700` | `/health` 200; active `lexi` / Alexia at start/end; `inference.hostModel=gemma4:latest` |
| `GET /api/inference/models` | **200** — `available=true`, `hostModel=gemma4:latest`; tags include `mommy:latest`, `mother:latest`, cloud/local GGUFs |
| ChatDesktop Release `0.1.8` | UIA main title `SoulCore CreatorEdition - Presence 0.1.8`; chrome combos = `PersonaSwitcher` only |

## Acceptance checklist

| # | Criterion | Result | Evidence |
| --- | --- | --- | --- |
| 1 | Edit persona → Identity step model ComboBox; **no** Presence chrome model dropdown | **Pass** | Wizard AXAML: `InferenceModelCombo` under `IdentityStep` (“Chat model (this persona only)”). MainWindow: `SystemInferenceBox` **IsReadOnly**; no `InferenceModelCombo` / chrome picker. UIA: `mainCombos=\|PersonaSwitcher`, `chromeModelCombo=false`. Settings→Edit path not fully UIA-exposed on Avalonia (candidates sparse) — source + FED 15.12 bind/save cover Identity picker. |
| 2 | Pack A model X ≠ Pack B model Y; activate each; health/identity resolved matches pack | **Pass** | See live API smoke below. Blank inherit: activate `blank` → `model=gemma4:latest`, `personaInferenceModel=null`, `modelSource=host`. |
| 3 | Optional chat turn per pack | **Skipped (noted)** | Not required for Pass. Host configured chat default `gemma4:latest` was not present in local Ollama tags at seat start (ALLSTART warning). Routing evidence is sufficient via `/health` resolve. |

## Live API smoke (`:7700`)

Precondition: `GET /api/personas` → 6 packs (analyst, blank, desk-smoke-aurora, lexi, mentor, victoria); all `inferenceModel` blank → resolved Host `gemma4:latest`.

| Step | Result |
| --- | --- |
| `PUT /api/personas/mentor` `inferenceModel=mommy:latest` | 200; DTO `resolvedInferenceModel=mommy:latest` |
| `PUT /api/personas/analyst` `inferenceModel=mother:latest` | 200; DTO `resolvedInferenceModel=mother:latest` |
| `POST …/mentor/activate` → `/health` | `persona=mentor`, `inference.model=mommy:latest`, `personaInferenceModel=mommy:latest`, `hostModel=gemma4:latest`, `modelSource=persona` |
| Identity / models (mentor active) | `/settings/identity` `resolvedInferenceModel=mommy:latest`; `/api/inference/models` `resolvedModel=mommy:latest` |
| `POST …/analyst/activate` → `/health` | `persona=analyst`, `inference.model=mother:latest`, `personaInferenceModel=mother:latest`, `modelSource=persona` |
| Identity / models (analyst active) | `resolvedInferenceModel=mother:latest` / `resolvedModel=mother:latest` |
| Activate blank (cleared override) | `model=gemma4:latest`, `modelSource=host` |
| Restore | `POST …/lexi/activate`; mentor/analyst display names + blank `inferenceModel` restored; `vmWindowTitle` mentor-sandbox / analyst-sandbox intact |

## Client tests

```text
dotnet test House/House.ChatDesktop.Tests -c Release --filter "FullyQualifiedName~Persona"
Passed!  - Failed: 0, Passed: 24, Skipped: 0, Total: 24
```

Covers Host-default-first picker, blank→null normalize, orphan override, resolution hint, create/update `InferenceModel` persist helpers (FED 15.12).

## Kill criteria

| Kill | Observed |
| --- | --- |
| Chrome global model dropdown | **None** |
| Pack A/B resolve cross-talk / stuck on Host | **None** — activate flips `modelSource` persona↔host correctly |
| Models endpoint down | **None** — 200 / available |

## Artifacts (gitignored)

- `tmpcode/prop15.12b-qa-uia.ps1`, `tmpcode/prop15.12b-qa-uia2.ps1`
- `tmpcode/prop15.12b-qa-uia-out.txt`, `tmpcode/prop15.12b-qa-uia2-out.txt`

## Notes for PM

- Smoke mutated mentor/analyst `inferenceModel` then **cleared** and restored display names; operator Host left on **lexi**.
- Desk Settings → Edit selected… wizard open needs manual visual glance if desired; not a product fail (Avalonia UIA sparse under Settings).
- Next: **SLOP-01** on 15.12 client surface; full stress remains **15.13**.
