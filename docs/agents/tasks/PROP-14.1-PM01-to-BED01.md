---
type: task
prop_id: PROP-14.1
prop_root: PROP-14-victoria-browser-live-cursor-pane
from: PM-01
to: BED-01
status: In Progress
created: 2026-10-02
updated: 2026-10-02
proposal: docs/agents/unexecuted_proposals/victoria-browser-live-cursor-pane.md
---

# PROP-14.1 — Pink → teal soft click cursor

## Goal

DOM soft-cursor idle `#ff2d55` → click `#2ec4b6` (~220ms), matching burn-in on JPEG fallback frames.

## Acceptance

- `__scMoveCursor` paints idle pink; `__scShowClick` flashes teal then returns to pink
- Aim/click path uses `__scShowClick`
- `BurnInMarker(..., Click)` uses teal accent
- Unit tests cover palette constants + path filter companion in 14.2
