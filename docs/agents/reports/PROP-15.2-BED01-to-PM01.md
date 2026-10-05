---
type: report
prop_id: PROP-15.2
prop_root: PROP-15-persona-creation-platform
from: BED-01
to: PM-01
status: Pass
created: 2026-10-05
execution_repo: https://github.com/House-VictoriAI/SoulCore.AI_CreatorEdition
---

# PROP-15.2 — Per-persona quarantined stores — Pass

## Verdict

**Pass.** CreatorEdition Host now opens episodic / charter / journal / embedding SQLite under `personas/{personaId}/memory/soulcore_memory.db` (vector dir reserved at `personas/{personaId}/vector/`). Active-persona switch tears down the prior bundle and opens the new one. Dual-persona automated tests prove **zero cross-read**. SoulLoop / chat / charter DI facades always forward to the **active** persona only; every hub accessor is keyed by `personaId`.

## Done

1. **`PersonaMemoryPaths`** — resolve memory DB + vector directory keyed by `personaId` under the personas root.
2. **`IPersonaStoreHub` / `PersonaStoreHub`** — open / tear down per-persona `SqliteMemorySession` + repos + `CharterService`; refuse non-active `personaId` in v1 (framed for multi-active map later).
3. **DI facades** — `IMemoryStore`, `ICharter`, `IEmotionState`, journal/task/workflow/transcript, `IMemoryStats` all persona-scoped; Host no longer binds a global `Memory:DbPath` singleton for live stores.
4. **Session switch** — `ActivePersonaSession.InitializeAsync` / `SetActiveAsync` call `SwitchActiveAsync` so activate API reopens stores.
5. **SoulLoop / charter active-only** — SoulLoopScaffold documented + evidenced: it consumes the same facades (emotion / memory / journal) → active persona only. Health + `/settings/identity` read charter via hub keyed by active `personaId`.
6. **Quarantine tests** — dual-persona switch: distinct DB files; no episodic/charter/journal bleed A↔B.

## Out of scope (unchanged)

- Avalonia UI → PROP-15.3 / 15.4
- MCP shared-memory → Phase 4
- VM / Playwright E2E → PROP-15.5
- Shared house table with `persona_id` as sole isolation — not used

## Files changed

### New
- `SoulCore/SoulCore.Config/PersonaMemoryPaths.cs`
- `SoulCore/SoulCore.Host/Persona/IPersonaStoreHub.cs`
- `SoulCore/SoulCore.Host/Persona/PersonaStoreHub.cs`
- `SoulCore/SoulCore.Host/Persona/PersonaStoreBundle.cs`
- `SoulCore/SoulCore.Host/Persona/PersonaScopedStoreFacades.cs`
- `SoulCore/SoulCore.Protocol.Tests/PersonaStoreHubQuarantineTests.cs`

### Modified
- `SoulCore/SoulCore.Host/Hosting/ServiceCollectionExtensions/MemoryServiceCollectionExtensions.cs`
- `SoulCore/SoulCore.Host/Hosting/ServiceCollectionExtensions/PersonaServiceCollectionExtensions.cs`
- `SoulCore/SoulCore.Host/Persona/ActivePersonaSession.cs`
- `SoulCore/SoulCore.Host/Hosting/WebApplicationExtensions.cs` (health + `/settings/identity`)
- `SoulCore/SoulCore.Host/Program.cs` (AddPersonaRuntime before AddMemory; boot log path)
- `SoulCore/SoulCore.Host/Loop/SoulLoopScaffold.cs` (active-only note)
- `SoulCore/SoulCore.Host/BackfillEmbeddings.cs` (default → active persona DB)
- `SoulCore/SoulCore.Config/MemoryOptions.cs` (CLI vs Host note)
- `SoulCore/SoulCore.Host/appsettings.json`, `appsettings.Example.json`
- `SoulCore/SoulCore.Protocol.Tests/PersonaPackStoreTests.cs`
- `docs/agents/tasks/PROP-15.2-PM01-to-BED01.md` (status → Pass)

## Evidence

### Build

```text
dotnet build SoulCore/SoulCore.Host/SoulCore.Host.csproj -c Release --no-incremental

Build succeeded.
    1 Warning(s)   # pre-existing CA1416 VoiceSpeakService windows-only
    0 Error(s)
Time Elapsed 00:00:03.01
```

### Tests (touched / related)

```text
dotnet test SoulCore/SoulCore.Protocol.Tests/SoulCore.Protocol.Tests.csproj -c Release --filter "FullyQualifiedName~Persona"

Passed!  - Failed:     0, Passed:    15, Skipped:     0, Total:    15, Duration: 306 ms - SoulCore.Protocol.Tests.dll (net8.0)
```

```text
dotnet test SoulCore/SoulCore.Protocol.Tests/SoulCore.Protocol.Tests.csproj -c Release --filter "FullyQualifiedName~Persona|FullyQualifiedName~ChatContextBuilder|FullyQualifiedName~EpisodicMemoryPrompt|FullyQualifiedName~ChatWebSocketHandlerToolLoop" --no-build

Passed!  - Failed:     0, Passed:    47, Skipped:     0, Total:    47, Duration: 222 ms - SoulCore.Protocol.Tests.dll (net8.0)
```

Key assertions:
- Two personas → two distinct `…/personas/{id}/memory/soulcore_memory.db` files
- Write episodic + charter + journal on A; switch to B; B cannot read A; switch back; A cannot read B
- Non-active `personaId` hub access throws (`InvalidOperationException`)
- DI facades (`PersonaScopedMemoryStore` / charter / journal) follow active switch with empty cross-read
- `ActivePersonaSession.SetActive` reopens stores (blank → mentor isolation)

### Active-only SoulLoop / charter

- SoulLoop injects `IEmotionState` / `IMemoryStore` / `IVictoriaJournalStore` → persona facades → `hub.ActivePersonaId` only.
- Charter ticks / chat identity / health lock counts use `personaStores.GetCharterService(activePersonaId)`.
- No background loop opens a second persona's DB.

## Acceptance checklist

| Criterion | Result |
| --- | --- |
| Two personas → two DB files (or equivalent proven isolation) | Pass |
| Dual-persona switch: no cross-read of episodic / charter / journal | Pass |
| Active-only SoulLoop/charter tick documented + evidenced | Pass |
| Report on CreatorEdition with command output | Pass |

## Blockers

None for 15.2. Ready for FED **15.3/15.4** and BED **15.5** (VM/tool path resolution).
