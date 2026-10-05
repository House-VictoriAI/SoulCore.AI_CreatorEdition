---
type: report
prop_id: PROP-15.18
prop_root: PROP-15-persona-creation-platform
from: OPS-HOME
to: PM-01
priority: P0
status: Pass
created: 2026-10-05
updated: 2026-10-05
depends_on: PROP-15.13 (Fail)
qa_report: docs/agents/reports/PROP-15.13-QA01-to-PM01.md
execution_repo: https://github.com/House-VictoriAI/SoulCore.AI_CreatorEdition
title: Competing activators on :7700; pin lexi + mother:latest for feel-drive
---

# PROP-15.18 — Feel-drive session pin — Pass

## Verdict

**Pass (ops).** Mid-run flip to **victoria** + `gemma4:latest` is explained by a **competing `POST .../activate`** against the same Host, not by the stress runner mutating mid-pack. Pack **victoria** has a **blank** `inferenceModel`, so activate resolves chat model to Host `Inference:Model` = **`gemma4:latest`** (appsettings) — and that tag is **not** in local `ollama list` → `chat.model_down` / Ollama 404. **lexi** `inferenceModel` mutation is a separate **`PUT /api/personas/lexi`** path (ChatDesktop Persona Wizard / any REST PUT), not activate.

Host is **ready now** for QA re-run: active **lexi**, `resolvedInferenceModel=mother:latest`, `modelSource=persona`.

## Competing activators on `:7700`

| Source | Talks to Host? | Can activate? | Can mutate `inferenceModel`? | Notes (live home-pc) |
| --- | --- | --- | --- | --- |
| **House.ChatDesktop** (Presence) | Yes — WS + HTTP | **Yes** — title-bar persona switcher → `POST /api/personas/{id}/activate` (`MainWindow.Persona.cs` → `SoulCorePersonaClient.ActivateAsync`) | **Yes** — Persona Wizard Save → `PUT /api/personas/{id}` (can change model combo) | **Running** PID connected Established → `127.0.0.1:7700`. Primary race with long stress runs if operator (or accidental switcher change) selects **Victoria**. |
| **SoulCore.Host** (single) | Listens `:7700` | Serves activate; boot loads `active.txt` only | Upsert via API; starter merge does **not** clobber `inferenceModel` | PID **37072** Release. `hostModel=gemma4:latest` always in health when pack model blank. Victoria starter seeds **no** pack model. |
| **Stress suite** | Yes | Start-of-run only (`Invoke-ActivatePersona`) unless `-SkipActivate` | No | Product runner `#Requires -Version 7.0`; home-pc used `tmpcode/Invoke-PersonaStressSuite-PS51.ps1`. Does not flip mid-run by itself. |
| **ALLSTART.ps1** | Starts Host + launches ChatDesktop | Indirect (brings GUI online) | No | Leaves Presence GUI attached — amplifies switcher risk during QA. |
| **Archived / tmp QA probes** (`docs/archive/qa-harnesses`, `tmpcode/prop15.*.ps1`) | Optional | Some call activate | Some e2e touch packs | Do not run in parallel with feel-drive. |
| **Second Host / foreign :7700** | Would steal port | N/A | N/A | Not observed; single local Victoria Listen. |

### Why victoria → gemma4 matches QA

1. Something issued `POST /api/personas/victoria/activate` (most plausible: **ChatDesktop Presence switcher** while suite ran; Host does not auto-activate Victoria on boot — Victoria is never boot default).
2. On-disk `victoria/pack.json`: **`inferenceModel` empty**.
3. Routing: blank pack model → `hostModel` from `SoulCore.Host/appsettings.json` → **`gemma4:latest`**.
4. Local Ollama has `mother:latest` / `mommy:latest` / other GGUFs — **no `gemma4`** → inference 404 / `chat.model_down`.

### Why lexi `inferenceModel` mutated

Activate alone does **not** rewrite pack JSON. Mutation requires **`PUT /api/personas/lexi`** (Wizard save, manual REST, or another editor). Seed/merge only fills blank tool-path fields. Restore for stress: PUT `inferenceModel` back to `mother:latest` (QA already did this in 15.13).

## Pin procedure — lexi + `mother:latest` (feel-drive / stress)

Do this **before** pack A–D / soaks; keep until score sheet is closed.

1. **Stop parallel activators**
   - Quit **House.ChatDesktop**, or leave it open but **do not** touch the Presence persona switcher, Persona Wizard, or Create/Edit persona.
   - Do not run other scripts that `POST .../activate` or `PUT` packs against `:7700`.
   - Prefer one Host only (ALLSTART already validated local Victoria).

2. **Confirm Ollama has the pin model**
   - `ollama list` must show `mother:latest` (present on home-pc at report time).

3. **Pin pack model (if drifted)**
   ```powershell
   $body = @{ inferenceModel = 'mother:latest' } | ConvertTo-Json
   Invoke-RestMethod -Method PUT -Uri 'http://127.0.0.1:7700/api/personas/lexi' `
     -ContentType 'application/json' -Body $body
   ```
   (Full pack PUT from Wizard is fine if UI shows Model = `mother:latest`.)

4. **Activate lexi**
   ```powershell
   Invoke-RestMethod -Method POST -Uri 'http://127.0.0.1:7700/api/personas/lexi/activate'
   ```

5. **Gate health before capture**
   ```powershell
   (Invoke-RestMethod 'http://127.0.0.1:7700/health').inference |
     Select-Object model, resolvedInferenceModel, personaInferenceModel, hostModel, modelSource
   ```
   **Required:** active persona lexi; `resolvedInferenceModel=mother:latest`; `personaInferenceModel=mother:latest`; `modelSource=persona`.  
   **Reject and re-pin if:** persona `victoria` / blank, or resolved model `gemma4:latest`, or `modelSource=host` while testing lexi.

6. **Run stress without mid-run activate races**
   - After pin, either `-SkipActivate` or `-PersonaId lexi` once at start only.
   - Re-check `/health` between packs if anything looks like `chat.model_down`.

7. **Optional hard isolation**
   - Close ChatDesktop for the entire suite; use REST + stress script only.
   - Do not open Persona Wizard on lexi during the run.

## Host readiness (post-check)

| Check | Result |
| --- | --- |
| Host | Release `0.1.5`, Listen `127.0.0.1:7700`, PID 37072 |
| `active.txt` / API | `lexi` |
| lexi `inferenceModel` | `mother:latest` |
| `/health` resolved | `mother:latest`, `modelSource=persona` |
| `hostModel` (fallback only) | `gemma4:latest` (unused while pack model set) |
| ChatDesktop | Still running and connected — **close or freeze switcher** before QA re-run |
| BED 15.17 | Not re-validated here; Host left up for QA |

## Optional — pwsh 7

- Product runner: `SoulCore/scripts/persona-stress/Invoke-PersonaStressSuite.ps1` → `#Requires -Version 7.0`.
- home-pc: **`pwsh` not on PATH**; Windows PowerShell **5.1** only.
- QA workaround remains `tmpcode/Invoke-PersonaStressSuite-PS51.ps1` until PowerShell 7 is installed.

## Ask PM / QA

- Re-run PROP-15.13 stress with pin procedure (prefer ChatDesktop quit).
- FED/BED: optional UX guard (disable switcher / confirm) is product work — out of OPS scope; ops pin is sufficient for feel-drive isolation.
