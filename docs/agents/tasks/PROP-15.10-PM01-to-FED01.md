---
type: task
prop_id: PROP-15.10
prop_root: PROP-15-persona-creation-platform
from: PM-01
to: FED-01
priority: P1
status: Pass
created: 2026-10-05
updated: 2026-10-05
depends_on: none
slop_report: docs/agents/reports/PROP-15.7-SLOP01-to-PM01.md
decision: host-only-gallery
report: docs/agents/reports/PROP-15.10-FED01-to-PM01.md
pm_note: Accepted. SLOP F2–F4 closed. PROP-15 v1 spine ready for feel-drive.
proposal: docs/agents/unexecuted_proposals/persona-creation-platform.md
execution_repo: https://github.com/House-VictoriAI/SoulCore.AI_CreatorEdition
title: Host-only persona template gallery — drop BuiltIn mirrors
---

# PROP-15.10 — Host-only gallery (SLOP F2/F3/F4)

## Decision (LinearThrone 2026-10-05)

**Host-only gallery until v1 feel-drive.** No `SoulCore.Core` reference on ChatDesktop. Offline `BuiltIn*` pack mirrors are removed. Thin HTTP DTOs stay (F4 intentional boundary).

## Problem

Wizard `BuiltInBlank` / `BuiltInMentor` / `BuiltInAnalyst` duplicate Host `PersonaPack.Create*`. Trait band preview also mirrors Host cutovers locally.

## Solution

1. **F2:** `BuildTemplates` uses **only** Host packs from `GET /api/personas`. Prefer Blank → Mentor → Analyst → Victoria (if present) → others. If Host unreachable or list empty: show clear “Host required” / retry — **do not** invent BuiltIn packs. Blank create still requires Host blank (or POST clone from a Host template id).
2. **F3:** Band preview — prefer Host `compiledDirectives` on edit/refresh after PUT/GET; if create-before-save needs a label, keep a **documented** constant mirror of 0.34/0.67 with a one-line comment “must match PersonaTraitCompiler” OR skip band-name preview until first Host response. Do **not** add Core project reference.
3. **F4:** Keep `PersonaPackInfo` / write DTOs / template id string constants as HTTP-boundary aliases (no Core converge).
4. **F5 (optional):** Collapse snapshot result shells only if cheap; do not widen to other HTTP clients.

## Do not

- Reference `SoulCore.Core` from ChatDesktop
- Break create/edit against a live `:7700` Host
- Change Host seed APIs unless a tiny bug blocks Host-only gallery (then report to PM → BED)

## Acceptance

- [x] No `BuiltIn*` pack factories left in ChatDesktop
- [x] Gallery empty/blocked state when Host packs unavailable
- [x] Create from Host blank/mentor/analyst still works with Host up
- [x] Tests updated; ChatDesktop build + persona tests green
- [x] Report `docs/agents/reports/PROP-15.10-FED01-to-PM01.md`

## Report

`docs/agents/reports/PROP-15.10-FED01-to-PM01.md`
