---
type: report
prop_id: PROP-15.9
prop_root: PROP-15-persona-creation-platform
from: BED-01
to: PM-01
status: Pass
created: 2026-10-05
updated: 2026-10-05
task_id: PROP-15.9
slop_finding: F1
execution_repo: https://github.com/House-VictoriAI/SoulCore.AI_CreatorEdition
---

# PROP-15.9 — SLOP F1 normalizer dedupe — Pass

## Verdict

**Pass.** Removed private `PersonaPackStore.NormalizeId`. Pack persist / activate / delete / active-marker reads now call `PersonaMemoryPaths.NormalizePersonaId` only — same API as memory + tool path resolution. PROP-15.8 starter merge helpers in the same file were left untouched.

## Done

1. Deleted `PersonaPackStore.NormalizeId` twin.
2. Replaced all call sites with `PersonaMemoryPaths.NormalizePersonaId`.
3. Added `UpsertAndGet_UsesSharedPersonaMemoryPathsNormalizer` (case-fold + reject invalid charset).

## Files changed

- `SoulCore/SoulCore.Host/Persona/PersonaPackStore.cs`
- `SoulCore/SoulCore.Protocol.Tests/PersonaPackStoreTests.cs`
- `docs/agents/tasks/PROP-15.9-PM01-to-BED01.md` (status → Pass)
- `docs/agents/PROP_NUMBERING.md`

## Coordination

PROP-15.8 (`SeedOrMergeStarterAsync` / `TryMergeMissingToolPaths` + related tests) already present in `PersonaPackStore`; this ticket only touched normalizer call sites / removal. No ChatDesktop changes (F2–F3 out of scope).

## Evidence

### Build + unit tests (Debug — Release Host bin locked by running `SoulCore.Host`)

```text
dotnet test SoulCore/SoulCore.Protocol.Tests/SoulCore.Protocol.Tests.csproj -c Debug --filter "FullyQualifiedName~PersonaPackStore"

Passed!  - Failed:     0, Passed:     7, Skipped:     0, Total:     7, Duration: 1 s
```

Note: Release rebuild of Host failed with MSB3027 while two `SoulCore.Host` processes held `bin\Release\net8.0\*.dll`. Debug build/test path verified the change. OPS may recycle `:7700` when convenient so Release output can refresh.

### Grep gate

No remaining `PersonaPackStore.NormalizeId` / private twin under `SoulCore/`. Host pack + hub + tool paths all use `PersonaMemoryPaths.NormalizePersonaId`.

## Acceptance

- [x] Single normalizer owner in Config
- [x] Pack persist + memory path use the same API
- [x] Tests green; this report
