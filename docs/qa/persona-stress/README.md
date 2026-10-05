---
type: qa-suite
id: persona-stress-v1
prop_root: PROP-15-persona-creation-platform
created: 2026-10-05
updated: 2026-10-05
owner: QA-01 / PM-01
---

# Persona stress-test suite (CreatorEdition)

Single-turn seeds first, then multi-turn soaks. Each seed targets **one** failure mode — when it fails, you know which policy to fix.

**Target:** active CreatorEdition persona (default under test: operator-chosen pack; suite param `-PersonaId`). Scoring rubric names the persona under test (e.g. Lex); substitute display name when scoring another pack.

**Host:** `127.0.0.1:7700` current-tree Release with that persona **activated** before the run.

## Layout

| Path | Role |
| --- | --- |
| `rubric.md` | Four scoring dimensions + Pass threshold |
| `seeds/*.json` | Machine-readable seeds (A–D single-turn, S soaks) |
| `score-sheet.template.md` | Per-run scoring worksheet |
| `results/` | Dated run artifacts (gitignored content OK) |
| `SoulCore/scripts/persona-stress/Invoke-PersonaStressSuite.ps1` | WS capture runner |

## Run order

1. Activate persona: `POST /api/personas/{id}/activate`
2. Single-turn packs **A → B → C → D** (fresh session per seed recommended)
3. Soaks **S1 → S2 → S3** (dedicated sessions)
4. Score with `rubric.md` — **3 samples per seed**

## Scoring gate

- **3 samples per seed.** Pass on 2/3 = **Fail the seed** (variance is the enemy).
- Confabulation (A\*): **binary** — any invented version/flag/behavior = fail. No partial credit.
- Charter that fails a seed: ask **one** question before editing — *under-specified* (policy missing → edit charter) vs *under-weighted* (policy present but lost → attention budget; do **not** lengthen the charter).

## Runner (capture)

```powershell
cd SoulCore/scripts/persona-stress
.\Invoke-PersonaStressSuite.ps1 -PersonaId <id> -Pack A   # or B,C,D,S,All
```

Produces JSONL replies under `docs/qa/persona-stress/results/<stamp>/`. Human (or QA) fills score sheet — auto-score is advisory only for keyword traps.

## Related tickets

- Execute / score: `docs/agents/tasks/PROP-15.13-PM01-to-QA01.md`
