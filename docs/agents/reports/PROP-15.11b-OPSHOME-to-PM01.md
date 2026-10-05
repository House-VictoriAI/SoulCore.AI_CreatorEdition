---
type: report
prop_id: PROP-15.11b
prop_root: PROP-15-persona-creation-platform
from: OPS-HOME
to: PM-01
status: Pass
created: 2026-10-05
execution_repo: https://github.com/House-VictoriAI/SoulCore.AI_CreatorEdition
---

# PROP-15.11b — Host up for model APIs — Pass

## Verdict

**Pass.** CreatorEdition **Release** Host is listening on `127.0.0.1:7700`. `/health` returns ok with resolved inference model; `GET /api/inference/models` returns **200** with a non-empty Ollama list.

## Actions

1. Probed `http://127.0.0.1:7700/health` — connection refused (Host down after BED Release rebuild).
2. Started **Release** Host from fresh DLL (`SoulCore.Host\bin\Release\net8.0\SoulCore.Host.dll`, mtime ~2026-10-05 03:15) — not Debug (`start-soulcore.ps1` defaults Debug / older DLL).
3. PID **31848** (pidfile `SoulCore/scripts/.soulcore-host.pid`); listener `127.0.0.1:7700`.

Note: `SoulCore/.env` was not present in this workspace tree at start (no secrets loaded/echoed). Host still came up healthy with appsettings defaults.

## Evidence

### GET /health → 200

| Field | Value |
| --- | --- |
| status / service | ok / SoulCore.Host |
| version | 0.1.5 |
| bind / port | 127.0.0.1 / 7700 |
| inference.model (resolved) | gemma4:latest |
| hostModel / modelSource | gemma4:latest / host |
| provider | ollama |
| memory.path | `%LOCALAPPDATA%\SoulCore\personas\blank\memory\...` (local Victoria) |

### GET /api/inference/models → 200

- `available`: true
- `resolvedModel` / `hostModel`: gemma4:latest
- `models`: non-empty list (Ollama tags reachable), including e.g. gpt-oss cloud, Qwen GGUF tags, kurtwood23/Anna, mommy, mother, nemotron-3-nano cloud
- fail-soft not needed this run

## Unblocks

- Desk / FED smoke against live Host on `:7700` (PROP-15.12 picker can call models API).

