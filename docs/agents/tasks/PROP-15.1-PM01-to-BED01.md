---
type: task
prop_id: PROP-15.1
prop_root: PROP-15-persona-creation-platform
from: PM-01
to: BED-01
priority: P0
status: Pass
created: 2026-10-05
updated: 2026-10-05
depends_on: none
siblings: PROP-15.2 (blocked on this), PROP-15.3, PROP-15.4, PROP-15.5, PROP-15.6
proposal: docs/agents/unexecuted_proposals/persona-creation-platform.md
intake: docs/agents/tasks/PROP-15-TT01-to-PM01.md
execution_repo: https://github.com/House-VictoriAI/SoulCore.AI_CreatorEdition
title: PersonaPack + session personaId + prompt/tool injection + trait-band compiler
---

# PROP-15.1 — PersonaPack runtime spine

## Problem

CreatorEdition still boots as a hardwired Victoria appliance: identity prompts, tool guidance, and session state assume one soul. Operators cannot load a named pack with `personaId`, and trait scales have nowhere to compile into directives.

## Root cause

No first-class `PersonaPack` / active-persona session. `ChatContextBuilder`, tool guidance, and companion `contactId` paths are Victoria-shaped singletons (`IVictoria*`, default contact, fixed identity text).

## Solution

1. Add **`PersonaPack`** (options + on-disk pack model): `personaId`, display name, human address (Kayleigh-style per AGENTS.md when speaking *to* the human), `contactId`, trait bands, charter seed path hooks, Playwright profile dir stub, VM window title stub, tool policy flags.
2. **Active persona session**: Host loads **exactly one** active pack; all public APIs / WS context carry or resolve `personaId`. Frame types so a future multi-active map is additive (no hard singleton that must be rewritten).
3. Inject pack into **`ChatContextBuilder`**, tool guidance, and episodic authoring identity — not global Victoria constants.
4. **Trait scales → discrete bands → compiled directive blocks** (identity / voice / boundaries / recall bias). Document: live adjust applies on **next turn** (no mid-turn hot-reload required in 15.1).
5. Persist/load packs under a CreatorEdition-owned directory (e.g. under LocalAppData/SoulCore/personas/). Ship **Blank** + 2–3 starter templates as pack JSON (Victoria may be an optional starter, **not** required to boot).
6. Thin companion/WS surface so FED can list/get/set-active/update pack fields in 15.3–15.4 (CRUD stubs OK if complete enough for wizard).

## Files to change (likely)

- `SoulCore/SoulCore.Config/` — PersonaPack options
- `SoulCore/SoulCore.Host/Ws/ChatContextBuilder.cs` (+ tests)
- `SoulCore/SoulCore.Host/Hosting/` — DI / session loader
- `SoulCore/SoulCore.Inference/` — tool guidance hooks that read active pack
- New pack store + trait band compiler (+ unit tests in `SoulCore.Protocol.Tests`)

## Do not

- Implement per-persona SQLite path switch (that is **15.2**)
- Build Avalonia wizard UI (**15.3/15.4**)
- Wire VM title / Playwright profile resolution end-to-end (**15.5**) — stubs on the pack model only
- Merge only to LinearThrone `SoulCore.AI` — **this repo only**
- Punch shared memory across personas
- Rename human to forbidden names (see `Agents/AGENTS.md`)

## Acceptance

- [x] Host boots with a non-Victoria Blank/starter pack as active; chat context contains that pack’s directives (not Victoria-hardwired identity)
- [x] Session / context exposes `personaId`; setting active pack switches next-turn identity text
- [x] Trait band change alters compiled directive block (unit test asserts two bands → different directive strings)
- [x] Pack CRUD or set-active API usable by FED without editing appsettings by hand
- [x] `dotnet test` on touched projects green; report includes real command output
- [x] Branch/PR targets **CreatorEdition**

## Report

`docs/agents/reports/PROP-15.1-BED01-to-PM01.md`
