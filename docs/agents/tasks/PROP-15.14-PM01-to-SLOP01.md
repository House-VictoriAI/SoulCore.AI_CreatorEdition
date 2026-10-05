---
type: task
prop_id: PROP-15.14
prop_root: PROP-15-persona-creation-platform
from: PM-01
to: SLOP-01
priority: P1
status: Pass
created: 2026-10-05
updated: 2026-10-05
depends_on: PROP-15.12b (Pass)
qa_report: docs/agents/reports/PROP-15.12b-QA01-to-PM01.md
slop_report: docs/agents/reports/PROP-15.14-SLOP01-to-PM01.md
related:
  - docs/agents/reports/PROP-15.11-BED01-to-PM01.md
  - docs/agents/reports/PROP-15.12-FED01-to-PM01.md
execution_repo: https://github.com/House-VictoriAI/SoulCore.AI_CreatorEdition
title: Post-QA slop audit — per-persona InferenceModel (15.11–15.12)
---

# PROP-15.14 — Slop audit after model-picker Pass

## Scope paths

- `SoulCore/SoulCore.Core/Persona/` (InferenceModel on pack)
- `SoulCore/SoulCore.Inference/Tooling/InferenceModelRouting.cs` (+ call sites)
- `SoulCore/SoulCore.Host/Persona/` (API DTO, inference models endpoint)
- `House/House.ChatDesktop/` persona wizard model picker / `SoulCorePersonaClient` model list
- Related Persona* tests

## Do not

- Modify code; report only
- Expand into stress-suite harness (15.13)

## Acceptance

`docs/agents/reports/PROP-15.14-SLOP01-to-PM01.md` — clean | findings (remove|dedupe|ask-user).

## Completion

- **Status:** Pass (audit complete; **findings**)
- **Report:** `docs/agents/reports/PROP-15.14-SLOP01-to-PM01.md`
- **Verdict:** 3 findings (1× P1 alias-sprawl JSON field names; 2× P2 duplicates). Routing/picker themselves clean. Not safe to archive until PM dispatches BED/FED cleanup or accepts risk.
