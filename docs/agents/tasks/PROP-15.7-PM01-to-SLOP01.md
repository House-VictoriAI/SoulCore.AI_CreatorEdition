---
type: task
prop_id: PROP-15.7
prop_root: PROP-15-persona-creation-platform
from: PM-01
to: SLOP-01
priority: P1
status: Done
created: 2026-10-05
updated: 2026-10-05
depends_on: PROP-15.6 (Pass)
qa_report: docs/agents/reports/PROP-15.6-QA01-to-PM01.md
completion_report: docs/agents/reports/PROP-15.7-SLOP01-to-PM01.md
proposal: docs/agents/unexecuted_proposals/persona-creation-platform.md
execution_repo: https://github.com/House-VictoriAI/SoulCore.AI_CreatorEdition
title: Post-QA slop audit — PROP-15 persona platform
---

# PROP-15.7 — Slop audit after 15.6 Pass

## Related QA

`docs/agents/reports/PROP-15.6-QA01-to-PM01.md` — **Pass**

## Scope paths

- `SoulCore/SoulCore.Core/Persona/`
- `SoulCore/SoulCore.Config/Persona*.cs`, `PersonaMemoryPaths.cs`, `PersonaToolPaths.cs`
- `SoulCore/SoulCore.Host/Persona/`
- `SoulCore/SoulCore.Host/Ws/ChatContextBuilder.cs` (+ related chat/persona DI)
- `SoulCore/SoulCore.Host/Hosting/ServiceCollectionExtensions/PersonaServiceCollectionExtensions.cs`
- `SoulCore/SoulCore.Host/Hosting/ServiceCollectionExtensions/MemoryServiceCollectionExtensions.cs`
- `House/House.ChatDesktop/` persona wizard / switcher / `SoulCorePersonaClient*` / `PersonaWizard*` / `MainWindow.Persona.cs`
- Related tests under `SoulCore.Protocol.Tests` / `House.ChatDesktop.Tests` touching Persona*

## Do not

- Modify code (report only)
- Expand into unrelated PROP-14 / UE work

## Acceptance

Report with `clean` | findings; each finding has evidence + **remove** | **dedupe** | **ask-user**.

## Report

`docs/agents/reports/PROP-15.7-SLOP01-to-PM01.md` — **findings** (5: F1–F3 P1, F4–F5 P2). Ticket Done.
