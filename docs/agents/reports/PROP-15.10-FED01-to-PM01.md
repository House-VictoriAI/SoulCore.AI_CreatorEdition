---
type: report
prop_id: PROP-15.10
prop_root: PROP-15-persona-creation-platform
from: FED-01
to: PM-01
priority: P1
status: Pass
created: 2026-10-05
updated: 2026-10-05
execution_repo: https://github.com/House-VictoriAI/SoulCore.AI_CreatorEdition
proposal: docs/agents/unexecuted_proposals/persona-creation-platform.md
intake: docs/agents/tasks/PROP-15.10-PM01-to-FED01.md
slop_report: docs/agents/reports/PROP-15.7-SLOP01-to-PM01.md
decision: host-only-gallery
---

# PROP-15.10 — Host-only persona template gallery — Pass

## Verdict

**Pass (client).** ChatDesktop gallery no longer invents `BuiltInBlank` / `BuiltInMentor` / `BuiltInAnalyst`. Templates come only from `GET /api/personas`. Empty Host list shows a blocked “Host required” state with Retry. No `SoulCore.Core` reference. Thin HTTP DTOs kept (F4). Band preview documents 0.34/0.67 as matching `PersonaTraitCompiler`; edit load prefers Host `compiledDirectives`.

## Done

1. **F2 — Host-only gallery** — `PersonaWizardLogic.BuildTemplates` returns Host packs only (order: Blank → Mentor → Analyst → Victoria → others). Null/empty Host list → empty gallery (no BuiltIn invent).
2. **Blocked UI** — create step shows Host-required copy + **Retry Host packs** (`ListAsync` refresh); Next disabled when gallery empty.
3. **F3 — Band preview** — `BandLowExclusiveMax` / `BandMidExclusiveMax` (0.34 / 0.67) documented to match `PersonaTraitCompiler.ToBand`. `PreferHostOrBandPreview` used on edit load; live sliders use local mirror.
4. **F4 — intentional boundary** — `PersonaPackInfo` / write DTOs / template id string constants remain HTTP-boundary aliases (commented). No Core converge.
5. **F5** — not done (optional; snapshot shells unchanged).

## Key files

| File | Role |
| --- | --- |
| `House/House.ChatDesktop/Services/PersonaWizardLogic.cs` | Host-only templates; band constants; PreferHostOrBandPreview |
| `House/House.ChatDesktop/PersonaWizardWindow.axaml` | Host-required + Retry chrome |
| `House/House.ChatDesktop/PersonaWizardWindow.axaml.cs` | Bind/retry gallery; edit prefers compiledDirectives |
| `House/House.ChatDesktop.Tests/PersonaWizardLogicTests.cs` | Empty gallery / Host order / band / PreferHost tests |

## Evidence

### Build (ChatDesktop)

```text
dotnet build House/House.ChatDesktop/House.ChatDesktop.csproj -c Release --no-incremental

Build succeeded.
    0 Warning(s)
    0 Error(s)
Time Elapsed 00:00:05.85
```

### Tests (persona / wizard)

```text
dotnet test House/House.ChatDesktop.Tests/House.ChatDesktop.Tests.csproj -c Release --filter "FullyQualifiedName~Persona"

Passed!  - Failed:     0, Passed:    19, Skipped:     0, Total:    19, Duration: 30 ms
```

Assertions: empty Host → empty gallery; Host blank/mentor/analyst order without invent; Victoria optional when present; create write from Host blank; PreferHostOrBandPreview; 0.34/0.67 cutovers; quarantine / next-turn notices unchanged.

### Acceptance checklist

- [x] No `BuiltIn*` pack factories left in ChatDesktop
- [x] Gallery empty/blocked state when Host packs unavailable
- [x] Create from Host blank/mentor/analyst still works with Host up (client path unchanged; templates sourced from list)
- [x] Tests updated; ChatDesktop build + persona tests green
- [x] Report `docs/agents/reports/PROP-15.10-FED01-to-PM01.md`

## Notes for PM

- Opening create still requires Host reachable (`MainWindow.Persona` early return); wizard Retry covers empty-or-stale pack list after open.
- Blank create still needs Host-seeded `blank` (or clone from another Host template id) — no offline blank.
- Recommend QA-01 smoke with `:7700` seeded packs when ready.
