---
type: task
prop_id: PROP-15.2
prop_root: PROP-15-persona-creation-platform
from: PM-01
to: BED-01
priority: P0
status: Pass
created: 2026-10-05
updated: 2026-10-05
depends_on: PROP-15.1 (Pass)
proposal: docs/agents/unexecuted_proposals/persona-creation-platform.md
intake: docs/agents/tasks/PROP-15-TT01-to-PM01.md
execution_repo: https://github.com/House-VictoriAI/SoulCore.AI_CreatorEdition
title: Per-persona quarantined SQLite/vector stores; dual-persona zero cross-read
---

# PROP-15.2 — Total memory quarantine

## Problem

Memory/charter/journal share one global DB path (`MemoryOptions.ResolveDbPath` → single `soulcore_memory.db`). Switching personas would bleed episodics and charter across brains.

## Root cause

Stores are Host singletons bound to one path. No persona-scoped path resolution for Memory / Charter / vector data.

## Solution

1. Resolve **per-persona** SQLite (and any vector) paths from active `personaId` (separate files under personas/{id}/… — Avenue B+).
2. On active-persona switch: tear down / reopen stores against the new paths (or equivalent isolation). SoulLoop + charter tick only against active persona stores.
3. Automated proof: create persona A and B, write distinct episodic/charter rows, switch A↔B, assert **zero** cross-read.
4. Keep `personaId` on every store access path so multi-active later does not require a rewrite.

## Do not

- Shared house table with `persona_id` column as the only isolation
- MCP shared-memory server (Phase 4)
- UI work (FED)

## Acceptance

- [x] Two personas → two DB files (or equivalent proven isolation)
- [x] Dual-persona switch test: no cross-read of episodic / charter / journal
- [x] Active-only SoulLoop/charter tick documented + evidenced
- [x] Report on CreatorEdition with command output

Report: `docs/agents/reports/PROP-15.2-BED01-to-PM01.md`

## Report

`docs/agents/reports/PROP-15.2-BED01-to-PM01.md`
