---
type: report
prop_id: PROP-15.7
prop_root: PROP-15-persona-creation-platform
from: SLOP-01
to: PM-01
task_id: PROP-15.7
status: findings
created: 2026-10-05
updated: 2026-10-05
qa_report: docs/agents/reports/PROP-15.6-QA01-to-PM01.md
execution_repo: https://github.com/House-VictoriAI/SoulCore.AI_CreatorEdition
---

# SLOP-01 Report — PROP-15.7

## Scope
- Related QA task / report: `PROP-15.6` / `docs/agents/reports/PROP-15.6-QA01-to-PM01.md` (Pass)
- Files / packages scanned:
  - `SoulCore/SoulCore.Core/Persona/*`
  - `SoulCore/SoulCore.Config/PersonaOptions.cs`, `PersonaMemoryPaths.cs`, `PersonaToolPaths.cs`
  - `SoulCore/SoulCore.Host/Persona/*`
  - `SoulCore/SoulCore.Host/Ws/ChatContextBuilder.cs`
  - `SoulCore/SoulCore.Host/Hosting/ServiceCollectionExtensions/PersonaServiceCollectionExtensions.cs`
  - `SoulCore/SoulCore.Host/Hosting/ServiceCollectionExtensions/MemoryServiceCollectionExtensions.cs` (persona DI slice)
  - `House/House.ChatDesktop/` persona wizard / switcher / `SoulCorePersonaClient` / `PersonaWizard*` / `MainWindow.Persona.cs` / `PersonaQuarantineConfirm`
  - Related tests: `SoulCore.Protocol.Tests/Persona*.cs`, `House.ChatDesktop.Tests/Persona*.cs`
- Method: read + ripgrep for sibling duplicates / alias pairs; working-tree audit (persona surface largely untracked vs `main`, so diff-vs-main alone under-reports). No product code modified.

## Summary
- Findings count: **5** (2× P1, 3× P2)
- Highest severity: **P1**
- Naming (AGENTS): no forbidden human-name tokens in scoped persona paths

## Findings

### F1 — Duplicate personaId normalizers
- Category: duplicate | alias-sprawl
- Severity: P1
- Evidence:
  - `SoulCore.Config.PersonaMemoryPaths.NormalizePersonaId` (`PersonaMemoryPaths.cs` ~L38–53)
  - `PersonaPackStore.NormalizeId` private twin (`PersonaPackStore.cs` ~L261–277) — same charset + 1–64 length rules, different symbol name and check order
- Why it matters: pack persist vs memory/tool path resolution can drift if only one normalizer is updated (quarantine key mismatch risk).
- Recommended action: **dedupe**
- Suggested owner: BED-01
- Notes: Prefer `PersonaPackStore` calling `PersonaMemoryPaths.NormalizePersonaId` (single owner). Client `PersonaWizardLogic.IsValidPersonaId` is a non-throwing UI twin — acceptable if it stays rule-aligned; after BED dedupe, FED should keep wizard rules in lockstep (see F3).

### F2 — Starter pack definitions copied into ChatDesktop wizard
- Category: duplicate
- Severity: P1
- Evidence:
  - Source of truth: `PersonaPack.CreateBlank` / `CreateMentorStarter` / `CreateAnalystStarter` (`PersonaPack.cs` ~L67–125) — blurbs + trait scales + ids
  - Copy: `PersonaWizardLogic.BuiltInBlank` / `BuiltInMentor` / `BuiltInAnalyst` (`PersonaWizardLogic.cs` ~L201–252) — same ids, blurbs, and trait numbers
  - ChatDesktop `.csproj` references only `SoulCore.Protocol` (no `SoulCore.Core`), so offline BuiltIns cannot call Core factories today
- Why it matters: Host seed / template gallery can diverge (wizard offline fallback vs `PersonaPackStore.EnsureSeededAsync`). Trait/id copy-paste is the classic “two biographies” bug class.
- Recommended action: **ask-user**
- Suggested owner if cleanup: FED-01 (+ BED-01 if shared package)
- Ask-user question: May ChatDesktop take a `SoulCore.Core` (or thin shared) project reference so wizard templates call `PersonaPack.Create*` / shared ids, or should BuiltIns be removed and the gallery require Host `GET /api/personas` only?

### F3 — Trait band edges duplicated client-side
- Category: duplicate
- Severity: P1
- Evidence:
  - Host: `PersonaTraitCompiler.ToBand` Low &lt; 0.34 / Mid &lt; 0.67 (`PersonaTraitCompiler.cs` ~L22–30)
  - Desk: local `Band` inside `PersonaWizardLogic.FormatBandPreview` same cutovers (`PersonaWizardLogic.cs` ~L159–164)
  - Host API already exposes `compiledDirectives` via `PersonaApiEndpoints.ToDto` (~L211–212); wizard preview does not reuse it
- Why it matters: slider preview can disagree with next-turn Host compile if band edges ever change in one place.
- Recommended action: **dedupe** (prefer shared `ToBand` / constants; or preview via Host compile for edit path)
- Suggested owner: FED-01 (with BED-01 if extracting shared API into Core)
- Ask-user note: same project-reference decision as F2 — without Core, FED can only keep a constant mirror or call Host.

### F4 — Parallel client DTO / id aliases for pack shape
- Category: alias-sprawl
- Severity: P2
- Evidence:
  - `PersonaTraitScalesInfo` / `PersonaToolPolicyInfo` / `PersonaPackInfo` / `PersonaPackWrite` in `SoulCorePersonaClient.cs` (~L477–541) mirror `PersonaTraitScales` / `PersonaToolPolicy` / `PersonaPack` in Core
  - Id constants: `PersonaWizardLogic.BlankTemplateId` / `MentorTemplateId` / `AnalystTemplateId` vs `PersonaPack.BlankPersonaId` / `MentorPersonaId` / `AnalystPersonaId` (same string values, different names)
- Why it matters: mild maintenance tax; acceptable if ChatDesktop stays Core-free, but stacks with F2/F3.
- Recommended action: **ask-user**
- Suggested owner if cleanup: FED-01
- Ask-user question: Keep thin HTTP DTOs + template id aliases (document as intentional boundary), or converge on Core types/constants once F2 is decided?

### F5 — Near-identical persona HTTP snapshot types
- Category: duplicate | slop
- Severity: P2
- Evidence: `PersonaGetSnapshot`, `PersonaWriteSnapshot`, and `PersonaActivateSnapshot` in `SoulCorePersonaClient.cs` (~L450–475) share `Reachable` / `Ok` / `Persona` / `Detail` (Activate adds `ActivePersonaId` / `Note`); loopback + try/catch blocks repeated per verb (~L39–46, L90–97, L178–186, L232–239)
- Why it matters: noise only; no functional quarantine risk. Matches existing ChatDesktop HTTP client style (`SoulCoreDesktopViewClient`, etc.).
- Recommended action: **dedupe** (optional — collapse shared result shell / helper; do not drive-by refactor other clients unless PM wants a wider pass)
- Suggested owner: FED-01

## Clean areas
- `PersonaToolPaths` + `PersonaToolPathsResolver`: clear single resolution path (pack → Host Tools → persona-scoped default); no parallel VM/browser resolvers found.
- `PersonaScopedStoreFacades` + `IPersonaStoreHub`: repetitive but intentional DI forwarding for quarantine; not same-purpose alias sprawl.
- `FixedBlankPersonaSession` vs `ActivePersonaSession`: distinct roles (test/fallback vs live); keep.
- `ChatContextBuilder` persona injection: single read of `IPersonaSession.GetActive()` + `PersonaPromptBlocks` / `PersonaTraitCompiler` — no second identity compiler.
- `PersonaQuarantineConfirm` / switcher confirm flow: focused; no duplicate confirm builders beyond create vs edit copy (appropriate).
- Scoped paths: AGENTS naming OK (Kayleigh / LinearThrone where present; no forbidden tokens).

## Notes for PM
- Safe to archive? **no** — ticket DEV cleanup for at least **F1** (BED) and a decision on **F2/F3** before calling PROP-15 hygiene complete.
- If findings: prefer DEV cleanup before user-facing “done.”
- Suggested dispatch order:
  1. BED-01 — F1 (mechanical dedupe of `NormalizeId`)
  2. User / PM — F2 + F3 (+ F4) project-boundary decision
  3. FED-01 — apply F2/F3/F5 after decision
- QA Pass (`PROP-15.6`) is not contradicted; these are hygiene / drift risks, not reopen of kill criteria.
- Ops note from QA (mentor/analyst empty `vmWindowTitle` on operator disk) is seed-refresh / `SeedIfMissing` semantics, not slop in the Create* factories (factories already set `mentor-sandbox` / `analyst-sandbox`).
