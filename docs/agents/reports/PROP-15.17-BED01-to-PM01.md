---
type: report
prop_id: PROP-15.17
prop_root: PROP-15-persona-creation-platform
from: BED-01
to: PM-01
priority: P0
status: Pass
created: 2026-10-05
updated: 2026-10-05
depends_on: PROP-15.13 (Fail)
qa_report: docs/agents/reports/PROP-15.13-QA01-to-PM01.md
execution_repo: https://github.com/House-VictoriAI/SoulCore.AI_CreatorEdition
title: IdentityBlurb LIMITS win over soft trait boundaries
---

# PROP-15.17 — Prompt merge / attention — **Pass**

## Verdict

**Pass.** Soft Low-band “Flexible boundaries…” no longer undercuts IdentityBlurb LIMITS/META. No charter lengthening. Unit tests prove order + gating. Victoria flip is **not** a Host chat-path bug — hand to OPS-HOME (PROP-15.18).

## Fix

### Order (`BuildIdentityBlock`)

Was: name+blurb → human address → `Compile` (incl. Flexible boundaries after LIMITS).

Now:

1. Name line only  
2. Human address  
3. `PersonaTraitCompiler.Compile` (voice / recall; boundaries gated)  
4. **IdentityBlurb last** (LIMITS / META / UNCERTAINTY win attention)  
5. Charter identity anchors  

### Gate

`PersonaPromptBlocks.BlurbHasHardLimits` — blurb contains `LIMITS` (case-insensitive).

When true and `BoundaryStrictness` snaps to **Low**, `Compile(..., hardLimitsFromBlurb: true)` **omits** the Boundaries line entirely (no soft “Flexible…”). Mid/High boundary lines still emit.

### Debug log (once per turn)

`ChatContextBuilder.BuildAsync` debug line now includes `blurbHasLimits` + `boundaryBand` alongside `persona`.

## Victoria activate race (quick investigate)

| Question | Finding |
| --- | --- |
| Can Host chat WS switch active persona without activate API? | **No.** `ChatSendHandler` / `ChatContextBuilder` only `GetActive()` — never `SetActiveAsync`. |
| What switches active? | Only `POST /api/personas/{id}/activate` → `ActivePersonaSession.SetActiveAsync` (also ChatDesktop switcher). |
| Pack `inferenceModel` mutation? | `PUT` upsert + `ReloadActiveAsync` if editing the active pack — not chat.send. |
| Boot default victoria? | Store explicitly refuses Victoria as default active. |

**Conclusion:** Mid-run flip to victoria/`gemma4` and lexi `inferenceModel` rewrite are almost certainly a **second client** (ChatDesktop / other activate/PUT) or Host restart reloading persisted active — **not** Host chat auto-switch. **OPS-HOME PROP-15.18** to pin session / keep ChatDesktop off lexi soaks.

## Files changed

### Modified
- `SoulCore/SoulCore.Core/Persona/PersonaPromptBlocks.cs` — `BlurbHasHardLimits`, `BuildIdentityNameLine`
- `SoulCore/SoulCore.Core/Persona/PersonaTraitCompiler.cs` — `hardLimitsFromBlurb` gate
- `SoulCore/SoulCore.Host/Ws/ChatContextBuilder.cs` — reorder + debug fields
- `SoulCore/SoulCore.Protocol.Tests/ChatContextBuilderTests.cs` — LIMITS-after-traits + no Flexible
- `SoulCore/SoulCore.Protocol.Tests/PersonaTraitCompilerTests.cs` — gate unit tests
- `docs/agents/tasks/PROP-15.17-PM01-to-BED01.md` — status → Pass

## Evidence

```text
dotnet test SoulCore/SoulCore.Protocol.Tests/SoulCore.Protocol.Tests.csproj -c Release --filter "FullyQualifiedName~ChatContextBuilder|FullyQualifiedName~PersonaTraitCompiler"

Passed!  - Failed: 0, Passed: 18, Skipped: 0, Total: 18
```

(Stopped SoulCore.Host briefly so Release DLLs could copy — PID locked Host output.)

### Order proof (test)

`BuildIdentityBlock_WhenBlurbHasLimits_FlexibleBoundariesAbsentAndLimitsAfterTraits`: Low band + LIMITS blurb → no “Flexible boundaries”; `## LIMITS` / `## META` indices **after** `[Persona directives]`.

## Ask PM

1. Re-dispatch QA-01 stress suite after Host reload with this build.  
2. Keep PROP-15.18 OPS pin for victoria flip / inferenceModel races.  
3. Restart Host / ALLSTART if still down after BED DLL unlock.
