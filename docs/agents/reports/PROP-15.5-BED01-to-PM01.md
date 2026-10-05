---
type: report
prop_id: PROP-15.5
prop_root: PROP-15-persona-creation-platform
from: BED-01
to: PM-01
status: Pass
created: 2026-10-05
execution_repo: https://github.com/House-VictoriAI/SoulCore.AI_CreatorEdition
---

# PROP-15.5 — Persona-scoped VM + browser tooling — Pass

## Verdict

**Pass.** CreatorEdition resolves `DesktopTargetWindowTitle` and Playwright user-data dir from the **active PersonaPack** first. Host `Tools:*` is fallback only when the pack field is blank. Desktop scope / Guest VM launcher / Playwright bridge / tool-loop guidance all consume the resolver. Mentor/Analyst starters use `mentor-sandbox` / `analyst-sandbox` — no `victoria-sandbox` name required for a non-Victoria pack.

## Fallback (documented)

| Knob | Order |
| --- | --- |
| VM window title | 1) `PersonaPack.VmWindowTitle` → 2) `Tools:DesktopTargetWindowTitle` → 3) empty (unrestricted) |
| Playwright profile | 1) `PersonaPack.PlaywrightProfileDir` → 2) `Tools:PlaywrightUserDataDir` → 3) `{personasRoot}/{personaId}/browser` |

Legacy no-persona default remains `%LOCALAPPDATA%/SoulCore/victoria-browser` when Playwright is constructed without a resolver (tests).

Single-active only — no multi-VM simultaneous (Phase 4).

## Done

1. **`PersonaToolPaths`** + **`IPersonaToolPathsResolver` / `PersonaToolPathsResolver`** — pack-first resolution keyed by active session.
2. **Desktop_* path** — `ScopedDesktopControlBackend` + `VirtualBoxGuestAppLauncher` take `Func<string>` so activate switches title without rebuilding DI.
3. **Playwright** — injects resolver; recycles persistent context when the resolved profile dir changes.
4. **Tool loop** — `ChatSendHandler` passes resolved title into `ComputerUseGuidance`.
5. **Surfaces** — `/settings/tools`, `/health`, `/api/personas/active`, activate response expose `resolvedDesktopTargetWindowTitle` / `resolvedPlaywrightUserDataDir`.
6. **Starters** — Mentor/Analyst seed distinct `VmWindowTitle` values.

## Files changed

### New
- `SoulCore/SoulCore.Config/PersonaToolPaths.cs`
- `SoulCore/SoulCore.Core/Persona/IPersonaToolPathsResolver.cs`
- `SoulCore/SoulCore.Host/Persona/PersonaToolPathsResolver.cs`
- `SoulCore/SoulCore.Protocol.Tests/PersonaToolPathsTests.cs`

### Modified
- `SoulCore/SoulCore.Inference/Tools/Desktop/ScopedDesktopControlBackend.cs`
- `SoulCore/SoulCore.Inference/Tools/Desktop/VirtualBoxGuestAppLauncher.cs` (+ Browser partial)
- `SoulCore/SoulCore.Inference/Tools/Browser/PlaywrightBrowserBridge.cs`
- `SoulCore/SoulCore.Host/Hosting/ServiceCollectionExtensions/ToolsServiceCollectionExtensions.cs`
- `SoulCore/SoulCore.Host/Hosting/ServiceCollectionExtensions/PersonaServiceCollectionExtensions.cs`
- `SoulCore/SoulCore.Host/Ws/ChatSendHandler.cs`
- `SoulCore/SoulCore.Host/Hosting/WebApplicationExtensions.cs`
- `SoulCore/SoulCore.Host/Persona/PersonaApiEndpoints.cs`
- `SoulCore/SoulCore.Host/Program.cs`
- `SoulCore/SoulCore.Core/Persona/PersonaPack.cs`, `PersonaPromptBlocks.cs`
- `SoulCore/SoulCore.Config/ToolsOptions.cs`
- `SoulCore/SoulCore.Host/appsettings.json`, `appsettings.Example.json`
- `docs/agents/tasks/PROP-15.5-PM01-to-BED01.md` (status → Pass)
- `docs/agents/PROP_NUMBERING.md`

## Evidence

### Build

```text
dotnet build SoulCore/SoulCore.Host/SoulCore.Host.csproj -c Release --no-incremental

Build succeeded.
    1 Warning(s)   # pre-existing CA1416 VoiceSpeakService windows-only
    0 Error(s)
Time Elapsed 00:00:02.87
```

### Unit tests

```text
dotnet test SoulCore/SoulCore.Protocol.Tests/SoulCore.Protocol.Tests.csproj -c Release --filter "FullyQualifiedName~PersonaToolPaths|FullyQualifiedName~Persona|FullyQualifiedName~ScopedDesktop|FullyQualifiedName~PlaywrightBrowserBridge|FullyQualifiedName~ChatContextBuilder|FullyQualifiedName~ChatWebSocketHandlerToolLoop"

Passed!  - Failed:     0, Passed:   110, Skipped:     0, Total:   110, Duration: 906 ms
```

```text
dotnet test … --filter "FullyQualifiedName~PersonaToolPaths"
Passed!  - Failed:     0, Passed:     8, Skipped:     0, Total:     8
```

Key assertions:
- Pack title/profile wins over Host Tools
- Blank pack → Host fallback; both blank title → unrestricted
- Persona-scoped Playwright default under `{root}/{id}/browser` (no `victoria-browser` required)
- Two packs (alpha/beta) resolve distinct title + profile via session switch
- `ScopedDesktopControlBackend` Func title toggles `IsActive` per call
- Mentor starter title is `mentor-sandbox` (not victoria)

### Desk smoke (home-pc, port 7710, temp personas root)

Host boot log:

```text
Tools browserBackend=native desktopTarget=victoria-sandbox playwrightProfile=…\blank\browser
```

Activate responses (`Tools:DesktopTargetWindowTitle` still `victoria-sandbox` on Host):

| Activate | resolvedDesktopTargetWindowTitle | resolvedPlaywrightUserDataDir |
| --- | --- | --- |
| analyst | `analyst-sandbox` | `…/analyst/browser` |
| victoria | `victoria-sandbox` | `…/victoria/browser` |
| mentor | `mentor-sandbox` | `…/mentor/browser` |

`GET /settings/tools` while analyst active: host `desktopTargetWindowTitle=victoria-sandbox`, resolved=`analyst-sandbox`.

No live VirtualBox click smoke in this pass (no second VM required for resolver acceptance). OPS-HOME can exercise desktop_* against a named guest when available.

## Acceptance checklist

| Criterion | Result |
| --- | --- |
| Two packs with different titles/profile dirs → tools resolve per active pack | Pass (unit + activate API) |
| Non-Victoria pack tool path without `victoria-sandbox` name required | Pass (mentor/analyst) |
| Unit and/or desk smoke evidence | Pass |
| Work on CreatorEdition | Pass |

## Blockers

None for 15.5. Production Host on `:7700` was still an older build during smoke (no `/api/personas`); restart home-pc Host on this build before QA **15.6**. Ready for **PROP-15.6** (QA E2E).
