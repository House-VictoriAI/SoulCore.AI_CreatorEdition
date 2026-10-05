---
type: intake
prop_id: PROP-15-persona-creation-platform
from: TT-01
to: PM-01
mode: idea
status: Ticketed — PROP-15.0–15.6
created: 2026-10-05
updated: 2026-10-05
proposal: docs/agents/unexecuted_proposals/persona-creation-platform.md
execution_repo: https://github.com/House-VictoriAI/SoulCore.AI_CreatorEdition
upstream_repo: https://github.com/Linearthrone/SoulCore.AI
pm_tickets: docs/agents/tasks/PROP-15.0-PM01-routing.md, docs/agents/tasks/PROP-15.1-PM01-to-BED01.md, docs/agents/tasks/PROP-15.2-PM01-to-BED01.md, docs/agents/tasks/PROP-15.3-PM01-to-FED01.md, docs/agents/tasks/PROP-15.4-PM01-to-FED01.md, docs/agents/tasks/PROP-15.5-PM01-to-BED01.md, docs/agents/tasks/PROP-15.6-PM01-to-QA01.md
---

# PROP-15 — CreatorEdition persona platform (quarantined brains + in-app creation)

## Proposal

`docs/agents/unexecuted_proposals/persona-creation-platform.md`

Mode: **idea**

## Execution repo (mandatory)

**All PROP-15.x tickets and PRs land on:**  
[`House-VictoriAI/SoulCore.AI_CreatorEdition`](https://github.com/House-VictoriAI/SoulCore.AI_CreatorEdition)

LinearThrone `SoulCore.AI` remains the House Victoria product line. Do not implement 15.x against LinearThrone main unless PM explicitly ports a Host fix both ways.

## Summary

Strip Victoria-as-default on CreatorEdition. Ship an **in-app persona creation / adjustment UI** (templates + trait scales + charter) with **total memory quarantine** per persona, **one active persona at a time** (APIs framed for future multi-simultaneous), and MVP runtime covering **chat + memory + charter + SoulLoop + VM + tooling**. Shared knowledge later = **MCP shared-memory server**, not a shared house table. Metahuman/VE body stays part two.

## Decisions already accepted (LinearThrone 2026-10-05)

| Lock | Decision |
| --- | --- |
| Fork | `House-VictoriAI/SoulCore.AI_CreatorEdition` |
| Concurrency v1 | Single active persona |
| Concurrency future | Frame for multi-simultaneous (no dead-end singletons) |
| Memory | Total per-persona quarantine |
| Shared memory later | MCP shared-memory server |
| Creation UI | In-app create + live adjustments |
| MVP scope | Chat + memory + charter + SoulLoop + VM/tooling |

No product clarifying questions remain for PM.

## PM accept

- Avenue **B+** accepted.
- Splits ticketed as PROP-15.0–15.6 (see `pm_tickets` frontmatter).
- First execution handoff: **PROP-15.1 → BED-01**.
