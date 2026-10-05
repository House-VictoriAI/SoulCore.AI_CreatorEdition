---
type: task
prop_id: PROP-15.13
prop_root: PROP-15-persona-creation-platform
from: PM-01
to: QA-01
priority: P1
status: In Progress
created: 2026-10-05
updated: 2026-10-05
depends_on: PROP-15.13b (Pass)
suite: docs/qa/persona-stress/README.md
report: docs/agents/reports/PROP-15.13-QA01-to-PM01.md
pm_note: 15.13b Pass — lexi on mother:latest with real chat.done. Re-run full suite; target lexi.
execution_repo: https://github.com/House-VictoriAI/SoulCore.AI_CreatorEdition
title: Execute persona stress suite (A–D + soaks) — 3 samples/seed
---

# PROP-15.13 — Persona stress suite execution

## Suite (built)

`docs/qa/persona-stress/` — seeds A–D + S soaks, rubric, score sheet, WS capture runner:

```powershell
cd SoulCore/scripts/persona-stress
.\Invoke-PersonaStressSuite.ps1 -PersonaId <pack> -Pack All -Samples 3
```

## Task

1. Pick the persona under feel-drive (LinearThrone’s pack id). Activate on `:7700`.
2. Capture with runner (`-Samples 3`) or manual equivalent; fix runner frame parsing only via PM→BED if Host wire format breaks capture (QA does not edit product code — note harness bugs in report; PM may reticket BED for harness).
3. Score every seed with `rubric.md` + `score-sheet.template.md`. **Seed Pass = 3/3.** Confabulation binary.
4. For each Fail: mark under-specified vs under-weighted (do not charter-edit in this ticket).
5. Report Pass/Fail/Partial with filled score sheet path + results stamp.

## Skip

- Metahuman / UE
- Changing charter text (diagnosis only)

## Report

`docs/agents/reports/PROP-15.13-QA01-to-PM01.md`
