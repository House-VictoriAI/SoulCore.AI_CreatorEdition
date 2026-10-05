---
type: routing
prop_id: PROP-15.0
prop_root: PROP-15-persona-creation-platform
from: PM-01
to: PM-01
status: Accepted
created: 2026-10-05
updated: 2026-10-05
proposal: docs/agents/unexecuted_proposals/persona-creation-platform.md
intake: docs/agents/tasks/PROP-15-TT01-to-PM01.md
execution_repo: https://github.com/House-VictoriAI/SoulCore.AI_CreatorEdition
---

# PROP-15.0 — CreatorEdition freeze + cherry-pick contract

## Decision

Accept TT Avenue **B+** on **this** repo (`House-VictoriAI/SoulCore.AI_CreatorEdition`). LinearThrone `SoulCore.AI` stays the Victoria appliance line.

## Cherry-pick policy

| Direction | Rule |
| --- | --- |
| CreatorEdition ← upstream | Host/tooling fixes that are persona-agnostic may be cherry-picked after review |
| upstream ← CreatorEdition | Only when LinearThrone explicitly wants a fix; never pull Victoria-strip / PersonaPack UI into Victoria defaults |
| PROP-14 / UE body | Stay on Victoria line or Phase 4 — out of PROP-15 v1 |

## Victoria-defaults strip checklist (execution seats own the code)

- [ ] Host boots with **no** Victoria-named pack required (Blank or starter pack OK)
- [ ] Default `DesktopTargetWindowTitle` / Playwright profile come from **active PersonaPack**, not hardwired `victoria-sandbox` alone
- [ ] Presence shell copy/brand is CreatorEdition-neutral (15.4)
- [ ] Human address / `contactId` / charter authoring keyed by `personaId`
- [ ] Dual-persona switch proves zero cross-read of episodic/charter/journal (15.2 kill gate)
- [ ] Trait bands compile to directive blocks visible in next-turn context (15.1 / 15.6)

## Out of v1

- Multi-simultaneous brains
- MCP shared-memory server
- Metahuman / VE body

## Next

Hand off **PROP-15.1** to BED-01. Remaining 15.2–15.6 queued with `depends_on`.
