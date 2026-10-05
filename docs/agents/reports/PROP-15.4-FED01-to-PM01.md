---
type: report
prop_id: PROP-15.4
prop_root: PROP-15-persona-creation-platform
from: FED-01
to: PM-01
priority: P1
status: Pass
created: 2026-10-05
updated: 2026-10-05
execution_repo: https://github.com/House-VictoriAI/SoulCore.AI_CreatorEdition
proposal: docs/agents/unexecuted_proposals/persona-creation-platform.md
intake: docs/agents/tasks/PROP-15.4-PM01-to-FED01.md
depends_on: PROP-15.1 (Pass)
---

# PROP-15.4 — Active-persona switcher + CreatorEdition shell de-brand — Pass

## Verdict

**Pass (client).** Avalonia Presence now has a first-class active-persona switcher against Host `/api/personas*`, confirms memory quarantine on switch, and no longer requires Victoria branding for a complete CreatorEdition shell. Live Host session smoke is blocked on this machine (see Blockers).

## Done

1. **Active-persona switcher** — title-bar `ComboBox` lists Host packs (`GET /api/personas`), activates via `POST /api/personas/{id}/activate`, updates identity chrome + chat watermark + assistant bubble labels.
2. **Quarantine confirm** — modal before activate; copy states memories/charter/journals stay quarantined and do not follow the switch.
3. **CreatorEdition de-brand** — window titles, brand strip (`CREATOREDITION` / `CE`), default display name `Companion`, empty/browser hints, Product string, fatal dialogs, identity detail copy. Victoria remains optional pack content only, not shell-required.
4. **Out of scope held** — no create/edit wizard (15.3); no Host store path changes (15.2).

## Key UI / client files

| File | Role |
| --- | --- |
| `House/House.ChatDesktop/MainWindow.axaml` | Switcher control + de-branded shell chrome |
| `House/House.ChatDesktop/MainWindow.Persona.cs` | List / confirm / activate / apply-to-shell |
| `House/House.ChatDesktop/Services/SoulCorePersonaClient.cs` | HTTP client for personas API |
| `House/House.ChatDesktop/Services/PersonaQuarantineConfirm.cs` | Quarantine confirm copy |
| `House/House.ChatDesktop/Models/ChatMessage.cs` | `AssistantDisplayName` for bubbles |
| `House/House.ChatDesktop/Services/LocalUiSettings.cs` | Default display name → Companion |
| `House/House.ChatDesktop/House.ChatDesktop.csproj` | Product → SoulCore CreatorEdition Presence |

## Evidence

### Build (ChatDesktop)

```text
dotnet build House/House.ChatDesktop/House.ChatDesktop.csproj -c Release --no-incremental

Build succeeded.
    0 Warning(s)
    0 Error(s)
Time Elapsed 00:00:05.48
```

### Tests (touched)

```text
dotnet test House/House.ChatDesktop.Tests/House.ChatDesktop.Tests.csproj -c Release --filter "FullyQualifiedName~LocalUiSettingsLayout|FullyQualifiedName~PersonaQuarantineConfirm"

Passed!  - Failed:     0, Passed:     4, Skipped:     0, Total:     4, Duration: 140 ms
```

Assertions: quarantine message mentions persona + “do not follow”; assistant `DisplayRole` follows `AssistantDisplayName`; blank local display name normalizes to `Companion`.

### Live Host API smoke (blocked)

Loopback Host at `127.0.0.1:7700` answers `/health` as **version 0.1.5** with **no** `persona` field; `/api/personas*` returns **404** (pre–PROP-15.1 binary).

Attempting to rebuild current-tree Host for a side-port smoke failed:

```text
dotnet build SoulCore/SoulCore.Host/SoulCore.Host.csproj -c Release
error CS0103: The name 'ResolveActivePersonaId' does not exist in the current context
  (SoulCore.Host/BackfillEmbeddings.cs:43)
```

Client is wired to the PROP-15.1 contract documented in `docs/agents/reports/PROP-15.1-BED01-to-PM01.md`. Re-smoke switcher against a running 15.1+ Host after Host tree builds again.

## Acceptance checklist

| Criterion | Result |
| --- | --- |
| Operator can switch active persona in-app; Host session updates | Pass (UI + client); live Host update not verified — see Blockers |
| Shell does not require Victoria branding to feel complete | Pass |
| Evidence in report | Pass |
| No wizard / no Host store path changes | Pass |

## Blockers

1. **Stale Host process** on `:7700` (v0.1.5) — restart with PROP-15.1+ binary for GUI/API smoke.
2. **Current Host tree does not build** (`BackfillEmbeddings` / `ResolveActivePersonaId`) — looks like incomplete PROP-15.2 work; not in FED-01 scope. BED-01 should fix before QA desk smoke.

## Ticket

`docs/agents/tasks/PROP-15.4-PM01-to-FED01.md` → status **Pass**.
