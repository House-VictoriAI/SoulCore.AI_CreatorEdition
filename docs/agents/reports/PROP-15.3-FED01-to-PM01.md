---
type: report
prop_id: PROP-15.3
prop_root: PROP-15-persona-creation-platform
from: FED-01
to: PM-01
priority: P1
status: Pass
created: 2026-10-05
updated: 2026-10-05
execution_repo: https://github.com/House-VictoriAI/SoulCore.AI_CreatorEdition
proposal: docs/agents/unexecuted_proposals/persona-creation-platform.md
intake: docs/agents/tasks/PROP-15.3-PM01-to-FED01.md
depends_on: PROP-15.1 (Pass), PROP-15.2 (Pass), PROP-15.4 (Pass — switcher extended)
---

# PROP-15.3 — In-app persona create/edit wizard — Pass

## Verdict

**Pass (client).** Avalonia Presence now has an in-app create/edit wizard: template gallery (Blank + starters), name / human address / trait scales / charter editor, Host `/api/personas` save, and quarantine messaging. Extends the PROP-15.4 switcher (does not rebuild it). Live Host API smoke is blocked (see Blockers).

## Done

1. **Template gallery** — Blank always first (no Victoria required); Mentor/Analyst built-ins or Host packs; optional Victoria only if present on Host.
2. **Create wizard** — multi-step: template → name/id/human address → trait sliders (band preview) → identity/charter seed + quarantine notice → `POST /api/personas`.
3. **Edit existing** — Settings → Identity → Edit selected… loads `GET /api/personas/{id}`, adjusts scales/charter/name → `PUT /api/personas/{id}`. Next-turn apply messaging matches Host 15.1 behavior.
4. **Quarantine copy** — create notice (own memory/charter/journals); edit notice (do not follow a later switch); switcher confirm from 15.4 unchanged.
5. **Out of scope held** — no Host store path / VM tooling ownership; switcher chrome not rebuilt.

## Key UI / client files

| File | Role |
| --- | --- |
| `House/House.ChatDesktop/PersonaWizardWindow.axaml` | Create/edit wizard shell |
| `House/House.ChatDesktop/PersonaWizardWindow.axaml.cs` | Steps, validation, Host save |
| `House/House.ChatDesktop/Services/PersonaWizardLogic.cs` | Templates, id slug, write payloads, notices |
| `House/House.ChatDesktop/Services/SoulCorePersonaClient.cs` | List / get / create / update / activate |
| `House/House.ChatDesktop/Services/PersonaQuarantineConfirm.cs` | Edit quarantine notice (+ switch copy) |
| `House/House.ChatDesktop/MainWindow.Persona.cs` | Open wizard; refresh switcher after save |
| `House/House.ChatDesktop/MainWindow.axaml` | Create / Edit buttons on Identity settings |
| `House/House.ChatDesktop.Tests/PersonaWizardLogicTests.cs` | Template / id / quarantine / band tests |

## Evidence

### Build (ChatDesktop)

```text
dotnet build House/House.ChatDesktop/House.ChatDesktop.csproj -c Release --no-incremental

Build succeeded.
    0 Warning(s)
    0 Error(s)
Time Elapsed 00:00:02.92
```

### Tests (persona / wizard)

```text
dotnet test House/House.ChatDesktop.Tests/House.ChatDesktop.Tests.csproj -c Release --filter "FullyQualifiedName~Persona"

Passed!  - Failed:     0, Passed:    16, Skipped:     0, Total:    16, Duration: 27 ms
```

Assertions: Blank gallery without Victoria; optional Victoria starter when Host lists it; personaId slug + Host id rules; create write from Blank; create/edit quarantine copy; next-turn save notice; Low/Mid/High band preview.

### Structured UI verify notes (no live Host)

| Path | Expected |
| --- | --- |
| Settings → Identity → Create persona… | Wizard opens on template step; Blank selectable |
| Blank → name/id/human → sliders → charter → Save | `POST /api/personas`; status notice mentions next chat turn + quarantine |
| Edit selected… on title-bar selection | Loads traits/charter; Save → `PUT`; active shell name updates if editing active pack |
| Title-bar switcher | Unchanged 15.4 flow + quarantine confirm |

### Live Host API smoke (blocked)

```text
dotnet build SoulCore/SoulCore.Host/SoulCore.Host.csproj -c Release --no-incremental

error CS1501: No overload for method 'ToDto' takes 3 arguments
  (SoulCore.Host/Persona/PersonaApiEndpoints.cs:35)
```

Incomplete Host tree (likely mid–PROP-15.5 `IPersonaToolPathsResolver` on `/api/personas/active`). Client targets the PROP-15.1 CRUD contract. Re-smoke wizard against a building 15.1+ Host after BED fixes compile.

## Acceptance checklist

| Criterion | Result |
| --- | --- |
| Operator can create a non-Victoria persona entirely in-app | Pass (UI + client); live Host create not verified — see Blockers |
| Trait scales and charter save via Host APIs; next turn reflects | Pass (client wiring + next-turn notice); Host apply is 15.1 behavior |
| Templates selectable; Blank path works with zero Victoria required | Pass (unit + UI gallery) |
| Local UI verify evidence in report | Pass (structured notes + tests) |

## Blockers

1. **Current Host tree does not build** (`PersonaApiEndpoints.ToDto` 3-arg call) — BED-01 / incomplete 15.5 wiring; not in FED-01 scope.
2. **Live desk smoke** needs a running PROP-15.1+ Host after that fix.

## Ticket

`docs/agents/tasks/PROP-15.3-PM01-to-FED01.md` → status **Pass**.
