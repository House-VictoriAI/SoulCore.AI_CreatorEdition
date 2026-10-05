---
type: report
prop_id: PROP-15.8
prop_root: PROP-15-persona-creation-platform
from: BED-01
to: PM-01
status: Pass
created: 2026-10-05
execution_repo: https://github.com/House-VictoriAI/SoulCore.AI_CreatorEdition
---

# PROP-15.8 — Starter pack vmWindowTitle on disk — Pass

## Verdict

**Pass.** `EnsureSeeded` now merges blank tool-path fields from built-in starters into existing on-disk packs. Stale mentor/analyst seeds that omitted `vmWindowTitle` pick up `mentor-sandbox` / `analyst-sandbox` on next Host boot (or any store ensure). Operator-set titles are left alone. Activate + resolver still prefer pack title over Host Tools `victoria-sandbox`.

## Root cause

`SeedIfMissingAsync` only wrote starters when `pack.json` was absent. Packs seeded before PROP-15.5 kept blank/missing `VmWindowTitle`, so `PersonaToolPaths` fell through to Tools `DesktopTargetWindowTitle` (`victoria-sandbox`) — matching QA 15.6 desk note.

## Done

1. **`PersonaPackStore.SeedOrMergeStarterAsync`** — create if missing; else fill blank `VmWindowTitle` / `PlaywrightProfileDir` from starter template and rewrite `pack.json`.
2. **`TryMergeMissingToolPaths`** — pure merge helper (blank-only; never clobbers non-whitespace operator values).
3. **Tests** — fresh seed titles; stale omit/whitespace merge; custom victoria title preserved; activate mentor/analyst resolves pack titles with Host fallback still `victoria-sandbox`.

## Files changed

### Modified
- `SoulCore/SoulCore.Host/Persona/PersonaPackStore.cs`
- `SoulCore/SoulCore.Protocol.Tests/PersonaPackStoreTests.cs`
- `docs/agents/tasks/PROP-15.8-PM01-to-BED01.md` (status → Pass)
- `docs/agents/PROP_NUMBERING.md`

### New
- `docs/agents/reports/PROP-15.8-BED01-to-PM01.md`

## Acceptance

| Criterion | Evidence |
| --- | --- |
| Fresh + already-seeded roots end with mentor/analyst `VmWindowTitle` set | `EnsureSeeded_DefaultsActive…` + `EnsureSeeded_MergesBlankVmWindowTitle…` |
| Activate mentor/analyst pack title wins over Host victoria-sandbox | `EnsureSeeded_ActivateMentorAnalyst_PackTitleWinsOverHostVictoriaFallback` |
| Quarantine / UI untouched | No changes outside store + tests |

## Evidence

### Build + unit tests

Host `bin/Release` was locked by running `SoulCore.Host` PIDs — verified via isolated output path (`artifacts/prop-15.8-verify`, gitignored):

```text
dotnet test SoulCore/SoulCore.Protocol.Tests/SoulCore.Protocol.Tests.csproj -c Release `
  --filter "FullyQualifiedName~PersonaPackStore|FullyQualifiedName~PersonaToolPaths" `
  -o artifacts/prop-15.8-verify /p:UseAppHost=false

Passed!  - Failed:     0, Passed:    15, Skipped:     0, Total:    15, Duration: 494 ms
```

(8 prior PersonaToolPaths + 7 PersonaPackStore including 3 new PROP-15.8 cases + merge unit test.)

### Key assertions

- Fresh seed: mentor=`mentor-sandbox`, analyst=`analyst-sandbox`
- Stale JSON omitting / whitespace `vmWindowTitle` → merged to starter values; identity blurb preserved
- Victoria pack with operator `custom-victoria-vm` unchanged
- Session activate mentor/analyst → resolver returns pack titles; Host Tools still `victoria-sandbox`

## Ops note

Running Host processes do **not** auto-rewrite LocalAppData packs until restarted (or any code path that calls `EnsureSeeded`). After deploy/restart, existing `%LocalAppData%/SoulCore/personas/{mentor,analyst}/pack.json` gain titles on ensure.

## Do not (honored)

- Quarantine model unchanged
- No UI work
