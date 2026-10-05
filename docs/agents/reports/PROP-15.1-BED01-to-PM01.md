---
type: report
prop_id: PROP-15.1
prop_root: PROP-15-persona-creation-platform
from: BED-01
to: PM-01
status: Pass
created: 2026-10-05
execution_repo: https://github.com/House-VictoriAI/SoulCore.AI_CreatorEdition
---

# PROP-15.1 — PersonaPack runtime spine — Pass

## Verdict

**Pass.** CreatorEdition Host now boots on Blank (Victoria optional starter only), keeps a single-active `IPersonaSession` framed for multi later, injects pack identity + trait-band directives into chat context / tool guidance / episodic authoring, and exposes thin `/api/personas` CRUD + activate for FED.

Live trait/pack edits apply on the **next chat turn** (session re-read at `ChatContextBuilder.BuildAsync`).

## Done

1. **PersonaPack model** (`personaId`, display name, human address, `contactId`, trait scales, charter seed path hook, Playwright profile stub, VM window title stub, tool policy).
2. **Active persona session** — exactly one active pack; `TryGet` / loaded snapshot framed for future multi-active map.
3. **Injection** into `ChatContextBuilder` (`ChatContext.PersonaId`), `PersonaPromptBlocks` tool guidance, episodic `BuildSystemInstruction(displayName)`.
4. **Trait band compiler** — Low/Mid/High bands → Voice / Boundaries / Recall directive blocks.
5. **Pack persist/load** under `%LocalAppData%/SoulCore/personas/` with Blank + Mentor + Analyst + optional Victoria starters; default active = `blank`.
6. **FED API** — `GET/POST/PUT/DELETE /api/personas`, `GET /api/personas/active`, `POST /api/personas/{id}/activate`. Health + `/settings/identity` + companion contacts expose active pack.

## Out of scope (stubs only / deferred)

- Per-persona SQLite quarantine → **PROP-15.2**
- Avalonia wizard / switcher UI → **PROP-15.3 / 15.4**
- End-to-end VM title / Playwright profile resolution → **PROP-15.5** (fields stubbed on pack)

## Files changed

### New
- `SoulCore/SoulCore.Config/PersonaOptions.cs`
- `SoulCore/SoulCore.Core/Persona/PersonaPack.cs`
- `SoulCore/SoulCore.Core/Persona/PersonaTraitCompiler.cs`
- `SoulCore/SoulCore.Core/Persona/PersonaPromptBlocks.cs`
- `SoulCore/SoulCore.Core/Persona/IPersonaPackStore.cs`
- `SoulCore/SoulCore.Core/Persona/IPersonaSession.cs`
- `SoulCore/SoulCore.Host/Persona/PersonaPackStore.cs`
- `SoulCore/SoulCore.Host/Persona/ActivePersonaSession.cs`
- `SoulCore/SoulCore.Host/Persona/FixedBlankPersonaSession.cs`
- `SoulCore/SoulCore.Host/Persona/PersonaApiEndpoints.cs`
- `SoulCore/SoulCore.Host/Persona/PersonaSessionHostedService.cs`
- `SoulCore/SoulCore.Host/Hosting/ServiceCollectionExtensions/PersonaServiceCollectionExtensions.cs`
- `SoulCore/SoulCore.Protocol.Tests/PersonaTraitCompilerTests.cs`
- `SoulCore/SoulCore.Protocol.Tests/PersonaPackStoreTests.cs`

### Modified
- `SoulCore/SoulCore.Host/Ws/ChatContextBuilder.cs`, `ChatContext.cs`, `ChatSendHandler.cs`
- `SoulCore/SoulCore.Core/EpisodicMemoryPrompt.cs`
- `SoulCore/SoulCore.Host/Program.cs`, `Hosting/WebApplicationExtensions.cs`
- `SoulCore/SoulCore.Host/Companion/CompanionApiEndpoints.cs`
- `SoulCore/SoulCore.Host/appsettings.json`, `appsettings.Example.json`
- `SoulCore/SoulCore.Protocol.Tests/ChatContextBuilderTests.cs`
- `SoulCore/SoulCore.Protocol.Tests/ChatWebSocketHandlerToolLoopTests.cs` (persona DI + native BrowserBackend harness align)
- `SoulCore/SoulCore.Protocol.Tests/EpisodicMemoryPromptTests.cs`
- `docs/agents/tasks/PROP-15.1-PM01-to-BED01.md` (status → Pass)

## Evidence

### Build

```text
dotnet build SoulCore/SoulCore.Host/SoulCore.Host.csproj -c Release --no-incremental

Build succeeded.
    1 Warning(s)   # pre-existing CA1416 VoiceSpeakService windows-only
    0 Error(s)
Time Elapsed 00:00:03.11
```

### Tests (touched / related)

```text
dotnet test SoulCore/SoulCore.Protocol.Tests/SoulCore.Protocol.Tests.csproj -c Release --filter "FullyQualifiedName~Persona|FullyQualifiedName~ChatContextBuilder|FullyQualifiedName~EpisodicMemoryPrompt|FullyQualifiedName~ChatWebSocketHandlerToolLoop"

Passed!  - Failed:     0, Passed:    43, Skipped:     0, Total:    43, Duration: 222 ms - SoulCore.Protocol.Tests.dll (net8.0)
```

Key assertions:
- Blank active by default; Victoria not required
- Two trait bands → different compiled directive strings
- Session set-active / `FixedBlankPersonaSession.ReplaceActive` switches next-turn identity text
- Tool loop preamble includes `[Persona tools]` + `personaId`

## Acceptance checklist

| Criterion | Result |
| --- | --- |
| Host boots Blank/starter; chat context uses pack directives (not Victoria-hardwired) | Pass |
| Session/context exposes `personaId`; set-active switches next-turn identity | Pass |
| Trait band change alters compiled directives (unit test) | Pass |
| Pack CRUD / set-active API for FED | Pass (`/api/personas*`) |
| `dotnet test` on touched projects green + real output | Pass (43) |
| Work on CreatorEdition repo | Pass |

## Blockers

None for 15.1. Ready for **PROP-15.2** (per-persona SQLite quarantine) and FED **15.3/15.4**.
