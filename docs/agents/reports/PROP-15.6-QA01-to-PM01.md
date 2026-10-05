---
type: report
prop_id: PROP-15.6
prop_root: PROP-15-persona-creation-platform
from: QA-01
to: PM-01
priority: P1
status: Pass
created: 2026-10-05
updated: 2026-10-05
execution_repo: https://github.com/House-VictoriAI/SoulCore.AI_CreatorEdition
proposal: docs/agents/unexecuted_proposals/persona-creation-platform.md
---

# PROP-15.6 — CreatorEdition persona E2E — Pass

## Verdict: **Pass**

Operator Host `:7700` is current CreatorEdition Release with personas (15.6b recycle). Desk ChatDesktop smoke against `:7700` is green: switcher lists packs, quarantine confirm → activate non-Victoria, Create persona wizard opens. Prior `:7710` product gates (create/quarantine/traits/tools/Blank boot) remain green; no kill criteria hit.

## Environment

| Target | Result |
| --- | --- |
| `:7700` (operator Host, post-15.6b) | `/health` 200 with `persona` object; `GET /api/personas` **200**; active Blank at start/end of smoke |
| ChatDesktop GUI vs `:7700` | **Pass** — Avalonia window `SoulCore CreatorEdition - Presence 0.1.8`; UIA desk smoke |
| Prior `:7710` QA Host (Partial round) | Still trusted for deep quarantine/traits/tools evidence |

Post-recycle health (re-confirmed this seat):

```text
GET http://127.0.0.1:7700/health → 200
persona: blank / Blank / humanAddress=Friend
memory: ...\SoulCore\personas\blank\memory\soulcore_memory.db (persona-scoped)
GET /api/personas → 200; packs: analyst, blank, mentor, victoria (+ desk-smoke-aurora after create)
```

## Acceptance checklist

| # | Criterion | Result | Evidence |
| --- | --- | --- | --- |
| 1 | Create custom persona ≠ Victoria; chat with its identity | **Pass** | API `POST /api/personas` → `desk-smoke-aurora` / Aurora Desk / `humanAddress=LinearThrone`; activate → health `desk-smoke-aurora` / `aurora-sandbox`. Desk: Settings → **Create persona…** opens wizard window `Create persona` with quarantine copy. Stub/identity unit coverage from prior round still holds. |
| 2 | Dual-persona quarantine zero cross-read | **Pass** | Prior: `PersonaStoreHubQuarantine*` etc. **28** Host Persona filters + live distinct DBs on `:7710`. This seat: mentor vs `desk-smoke-aurora` memory paths distinct under `%LOCALAPPDATA%\SoulCore\personas\…`; ChatDesktop quarantine confirm copy shown on switch. Client: `House.ChatDesktop.Tests` Persona **16/16**. |
| 3 | Trait A/B detectable difference | **Pass** | Prior live A/B on `:7710` + `PersonaTraitCompilerTests`. This seat: `PUT desk-smoke-aurora` warmth=0.2 → compiledDirectives include `cool and reserved` / `direct and concise`. |
| 4 | Pack-scoped VM/browser paths (no victoria-sandbox required) | **Pass** | Activate `desk-smoke-aurora` → `resolvedDesktopTargetWindowTitle=aurora-sandbox`. Mentor/analyst browser dirs `...\mentor\browser` / `...\analyst\browser`. Operator seed packs mentor/analyst have empty `vmWindowTitle` so **title** falls back to Tools `victoria-sandbox` (documented PROP-15.5 fallback) — not Victoria-pack-required; pack override works when set (Aurora). Prior `:7710` proved mentor-sandbox/analyst-sandbox when pack titles populated. |
| 5 | Boot without Victoria pack (Blank) | **Pass** | `:7700` active Blank after recycle; restore Blank after smoke. Prior unit: `EnsureSeeded_DefaultsActiveToBlank_NotVictoria`. |
| Desk E2E on operator `:7700` | **Pass** | UIA evidence below |

## Desk smoke (`:7700`) — this seat

| Step | Result |
| --- | --- |
| ChatDesktop launch | Process alive; main window title `SoulCore CreatorEdition - Presence 0.1.8` |
| Persona switcher lists packs | ComboBox `PersonaSwitcher` items: **Analyst \| Blank \| Aurora Desk \| Mentor \| Victoria** |
| Quarantine confirm | Dialog text: `Switch active persona to Mentor (mentor)?` + `quarantined per persona` / `do not follow`; buttons Cancel / **Switch** |
| Activate non-Victoria | Click **Switch** → `/health` `mentor` / Mentor; memory `...\personas\mentor\memory\soulcore_memory.db` |
| Create/edit wizard | Settings → **Create persona…** → window `Create persona`; notice mentions quarantine / memories do not follow a switch |
| Restore | `POST /api/personas/blank/activate` → active Blank |

Artifacts (gitignored): `tmpcode/prop15.6-qa-desk-smoke-7700.json`, `tmpcode/prop15.6-qa-uia-7700.json`, `tmpcode/prop15.6-qa-uia-activate-7700.json`, `tmpcode/prop15.6-qa-uia-wizard-7700.json`.

## Commands / tests run (this seat)

```text
GET http://127.0.0.1:7700/health → 200 (persona present)
GET http://127.0.0.1:7700/api/personas → 200
# API desk-equivalent: list, activate mentor/analyst/aurora, create+edit aurora, restore blank

dotnet test House/House.ChatDesktop.Tests -c Release --filter "FullyQualifiedName~Persona"
# Passed: 16

# UIA: launch House.ChatDesktop.exe → switcher / Switch confirm → Mentor active;
# Settings → Create persona wizard window
```

Prior round (still cited): Host Persona filters **28** pass; `:7710` harness create/activate/traits/tools/Blank boot Pass — see previous Partial body / `tmpcode/prop15.6-qa-e2e*` if retained.

## Kill criteria

| Kill | Observed |
| --- | --- |
| Quarantine leak | **Not observed** (prior unit + distinct live DBs; desk confirm copy) |
| Mushy traits / no band effect | **Not observed** (prior A/B + this seat PUT directives) |
| Victoria required to boot/tool | **Not observed** (Blank default; Aurora pack title override; mentor/analyst browser scoped without Victoria pack) |

## Issues filed

None.

## Ops note (non-blocking)

Operator seed packs for mentor/analyst currently omit `vmWindowTitle`, so desktop title falls back to Tools `victoria-sandbox` while browser dirs remain persona-scoped. Optional OPS/BED seed refresh if operator wants Mentor/Analyst titles without custom packs — not a 15.6 kill.

## Ready for SLOP-01?

**Yes.** PM-01 should dispatch **SLOP-01** per QA-01 role rules (post-Pass slop/alias audit). QA does not run SLOP.

## Handoff

PM-01: accept Pass; archive/report promotion as desired; dispatch SLOP-01 on PROP-15 persona platform chain.
