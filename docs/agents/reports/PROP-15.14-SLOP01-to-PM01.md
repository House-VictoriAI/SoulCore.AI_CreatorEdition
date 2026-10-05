---
type: report
prop_id: PROP-15.14
prop_root: PROP-15-persona-creation-platform
from: SLOP-01
to: PM-01
task_id: PROP-15.14
status: findings
created: 2026-10-05
updated: 2026-10-05
qa_report: docs/agents/reports/PROP-15.12b-QA01-to-PM01.md
execution_repo: https://github.com/House-VictoriAI/SoulCore.AI_CreatorEdition
---

# SLOP-01 Report — PROP-15.14

## Scope
- Related QA task / report: `PROP-15.12b` / `docs/agents/reports/PROP-15.12b-QA01-to-PM01.md` (Pass)
- Related DEV: `PROP-15.11` (BED) + `PROP-15.12` (FED)
- Files / packages scanned:
  - `SoulCore/SoulCore.Core/Persona/PersonaPack.cs` (`InferenceModel`)
  - `SoulCore/SoulCore.Inference/Tooling/InferenceModelRouting.cs` (+ call sites in `OllamaInferenceClient`, Host health/identity/personas)
  - `SoulCore/SoulCore.Host/Inference/InferenceApiEndpoints.cs`
  - `SoulCore/SoulCore.Host/Persona/PersonaApiEndpoints.cs` (DTO + activate resolve)
  - `SoulCore/SoulCore.Host/Hosting/WebApplicationExtensions.cs` (`/health`, `/settings/identity` model fields)
  - `House/House.ChatDesktop/Services/PersonaWizardLogic.cs` (picker helpers)
  - `House/House.ChatDesktop/Services/SoulCorePersonaClient.cs` (models GET + pack `InferenceModel`)
  - `House/House.ChatDesktop/PersonaWizardWindow.axaml(.cs)` (Identity-step picker)
  - Related tests: `InferenceModelRoutingTests`, `PersonaPackStoreTests` (persist), `OllamaListModelsTests`, `PersonaWizardLogicTests` (model picker)
- Method: read + ripgrep for sibling duplicates / alias pairs across Host JSON surfaces and ChatDesktop normalize/picker helpers. No product code modified. Stress-suite (15.13) not expanded.

## Summary
- Findings count: **3** (1× P1, 2× P2)
- Highest severity: **P1**
- Naming (AGENTS): no forbidden human-name tokens in scoped paths

## Findings

### F1 — Host/resolved model JSON field-name sprawl
- Category: alias-sprawl
- Severity: P1
- Evidence:
  - Host default model string:
    - `hostModel` on `/health` (`WebApplicationExtensions.cs` ~L179) and `GET /api/inference/models` (`InferenceApiEndpoints.cs` ~L37/52/65)
    - `hostInferenceModel` on `/settings/identity` (`WebApplicationExtensions.cs` ~L427)
  - Resolved chat model string:
    - `inference.model` on `/health` (~L178)
    - `resolvedModel` on `/api/inference/models` (~L39/54/67)
    - `resolvedInferenceModel` on personas DTO / activate (`PersonaApiEndpoints.cs` ~L162/225) and `/settings/identity` (~L426)
  - Pack override: consistently `inferenceModel` / `personaInferenceModel` (health) — OK
- Why it matters: same Host values under three resolved names and two host-default names forces FED/QA to special-case each surface (QA 15.12b already probed both). Easy for a future client to read the wrong key and show Host when persona override is active.
- Recommended action: **dedupe** (prefer one pair: e.g. `hostModel` + `resolvedInferenceModel` everywhere; keep `inference.model` on `/health` only if documented as the active resolved alias)
- Suggested owner: BED-01
- Ask-user note: if any external loopback consumer already depends on `hostInferenceModel` / `resolvedModel`, freeze names and document; otherwise unify in one BED pass + FED DTO align.

### F2 — Triplicated `/api/inference/models` JSON payload
- Category: duplicate
- Severity: P2
- Evidence: `InferenceApiEndpoints.cs` ~L30–69 — disabled / client-null / success branches each return nearly identical anonymous objects (`models`, `available`, `detail`, `hostModel`, `personaInferenceModel`, `resolvedModel`, `baseUrl`); only the first three fields differ.
- Why it matters: adding a field (e.g. `modelSource`) risks updating one branch and leaving others stale — same class of drift as F1.
- Recommended action: **dedupe** (local helper / single shape builder)
- Suggested owner: BED-01

### F3 — Blank-model normalize reimplemented beside `NormalizeInferenceModel`
- Category: duplicate
- Severity: P2
- Evidence:
  - Shared helper: `PersonaWizardLogic.NormalizeInferenceModel` (`PersonaWizardLogic.cs` ~L167–172) — blank/whitespace → null, else trim
  - Inline twins in `SoulCorePersonaClient.cs`: write payload ~L308–310; `FromDto` ~L384–387 (and HostModel/ResolvedModel trim in `ListInferenceModelsAsync` ~L80–81)
- Why it matters: mild; create/update already go through wizard `NormalizeInferenceModel`, but client HTTP edge can drift if trim/null rules change in one place.
- Recommended action: **dedupe** (call `NormalizeInferenceModel` from client map/write paths)
- Suggested owner: FED-01

## Clean areas
- **`InferenceModelRouting`**: single resolve path (pack → Host → `gemma4:latest`); chat/tool session overloads share `ResolvePackInferenceModel` — no parallel resolver found.
- **Call sites**: `OllamaInferenceClient`, personas `ToDto`, activate, `/health`, `/settings/identity`, models endpoint all call routing — no second pack→Host algorithm.
- **Wizard ownership**: `InferenceModelCombo` only on Identity step; Presence `SystemInferenceBox` remains read-only status — no chrome picker alias.
- **Picker helpers**: `BuildModelPickerOptions` / `FindModelOption` / `FormatModelResolutionHint` are focused and unit-tested; Host-default-first + orphan override is one implementation.
- **`BlankTemplateId` vs `PersonaPack.BlankPersonaId`**: documented HTTP-boundary alias (prior PROP-15.7 F4) — not re-flagged as new sprawl for this surface.
- **LocalStackControl `/api/tags`**: ops readiness probe, not a second FED model-picker path — out of cleanup scope.
- Scoped paths: AGENTS naming OK (Kayleigh / LinearThrone; no forbidden tokens).

## Notes for PM
- Safe to archive? **no** — ticket BED for **F1** (and optionally **F2** in the same pass); FED **F3** is small and can ride with F1 DTO align or defer as P2.
- Functional QA Pass (15.12b) stands; these are hygiene / drift-prevention, not reopen of model routing behavior.
- Prefer DEV cleanup before user-facing “done” on the 15.11–15.12 model-picker slice; do not expand into 15.13 stress harness.
