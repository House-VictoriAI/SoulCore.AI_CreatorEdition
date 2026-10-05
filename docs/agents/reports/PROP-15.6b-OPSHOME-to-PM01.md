---
type: report
prop_id: PROP-15.6b
prop_root: PROP-15-persona-creation-platform
from: OPS-HOME
to: PM-01
priority: P0
status: Pass
created: 2026-10-05
updated: 2026-10-05
execution_repo: https://github.com/House-VictoriAI/SoulCore.AI_CreatorEdition
proposal: docs/agents/unexecuted_proposals/persona-creation-platform.md
ticket: docs/agents/tasks/PROP-15.6b-PM01-to-OPSHOME.md
---

# PROP-15.6b — Recycle operator Host :7700 — Pass

## Verdict: **Pass**

Operator Host on `127.0.0.1:7700` was the pre-persona **v0.1.5** Debug build (no `/api/personas`). It was stopped gracefully and replaced with current CreatorEdition **Release** Host. Health now includes `persona` / active pack; `GET /api/personas` returns **200**.

## Actions

1. **Identified listener** — `127.0.0.1:7700` Listen PID **37280** (`dotnet.exe`).
   - Command line: `...\Soulcore.AI\SoulCore.AI\SoulCore\SoulCore.Host\bin\Debug\net8.0\SoulCore.Host.dll --urls http://127.0.0.1:7700`
   - Pre-stop `/health`: `version=0.1.5`, **no** `persona` object
   - Pre-stop `GET /api/personas` → **404**
2. **Stopped** PID 37280 via `Stop-Process` (graceful process exit). Port **7700 free** afterward. Did not touch CreatorEdition Host already on `:7710` (PID 32180).
3. **Started** from workspace: `dotnet run --project SoulCore/SoulCore.Host -c Release` (loopback bind defaults). New listener PID **42824**: `...\SoulCore.AI_CreatorEdition\...\bin\Release\net8.0\SoulCore.Host.exe` on `127.0.0.1:7700`.
4. **Verified** (post-start).

## Evidence

### `GET http://127.0.0.1:7700/health` → 200

Includes:

- `"persona":{"personaId":"blank","displayName":"Blank","contactId":"blank","humanAddress":"Friend"}`
- Memory path under persona-scoped quarantine: `...\SoulCore\personas\blank\memory\soulcore_memory.db`
- Tools note references PROP-15.5 resolved pack paths

(Assembly `version` string still reports `0.1.5`; functional persona surface is present.)

### `GET http://127.0.0.1:7700/api/personas` → 200

- `activePersonaId`: `blank`
- Packs listed: `analyst`, `blank` (active), `mentor`, `victoria`

## Acceptance

- [x] `:7700` serves current-tree personas API
- [x] Report with health + `/api/personas` evidence
- [x] Notify PM so QA can promote 15.6 Partial → Pass (desk smoke)

## Notes

- No secrets echoed from `SoulCore/.env`.
- Optional ChatDesktop open against `:7700` not run this seat (API gates sufficient for recycle ticket).
- Background Host left running for desk smoke.

