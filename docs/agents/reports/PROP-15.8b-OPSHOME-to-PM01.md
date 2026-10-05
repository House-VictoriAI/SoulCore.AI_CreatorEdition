---
type: report
prop_id: PROP-15.8b
prop_root: PROP-15-persona-creation-platform
from: OPS-HOME
to: PM-01
priority: P1
status: Pass
created: 2026-10-05
updated: 2026-10-05
execution_repo: https://github.com/House-VictoriAI/SoulCore.AI_CreatorEdition
ticket: docs/agents/tasks/PROP-15.8b-PM01-to-OPSHOME.md
depends_on: PROP-15.8 (Pass)
---

# PROP-15.8b - Host recycle after starter merge - Pass

## Verdict: **Pass**

CreatorEdition **Release** Host on `127.0.0.1:7700` was stopped, rebuilt from current tree (PROP-15.8 merge), and restarted. `GET /api/personas` shows mentor/analyst with non-empty `vmWindowTitle` = `mentor-sandbox` / `analyst-sandbox`. Packs were rewritten at ensure (`updatedAtUtc` ~ `2026-10-05T04:13:50Z`).

## Actions

1. **Pre-state** - Listener `127.0.0.1:7700` PID **42824** (`SoulCore.Host.exe` Release). Also PID **32180** on `:7710` locking the same Release output dir.
2. **Stopped** PID 42824 (operator `:7700`). Port free.
3. **Build blocked** by PID 32180 (`:7710` QA/smoke Host holding Release DLLs). Stopped 32180 so `dotnet build SoulCore/SoulCore.Host -c Release` could succeed (**0** warnings / **0** errors).
4. **Started** `dotnet run --project SoulCore/SoulCore.Host -c Release --no-build`. New listener PID **13388** on `127.0.0.1:7700`.
5. **Verified** health + personas API (below).

## Evidence

### `GET http://127.0.0.1:7700/health` → 200

Listening log: persona-scoped blank memory under `%LocalAppData%\SoulCore\personas\blank\...`; Host Tools fallback `desktopTarget=victoria-sandbox`.

### `GET http://127.0.0.1:7700/api/personas` → 200

- `activePersonaId`: `blank`
- **mentor** `vmWindowTitle`: `mentor-sandbox` (non-empty)
- **analyst** `vmWindowTitle`: `analyst-sandbox` (non-empty)
- Also listed: `blank`, `desk-smoke-aurora`, `victoria`

## Acceptance

- [x] `:7700` restarted from current-tree Release build
- [x] mentor/analyst expose `mentor-sandbox` / `analyst-sandbox` via `/api/personas`
- [x] Report written; ticket updated
- [x] No `.env` secrets echoed

## Notes

- Brief stop of `:7710` Host was required to unlock Release bin copy; that smoke Host was **not** restarted (ticket scope is `:7700` only).
- Background operator Host left running on `:7700` for desk use.
