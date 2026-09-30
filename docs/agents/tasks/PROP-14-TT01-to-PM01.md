---
type: intake
prop_id: PROP-14-victoria-browser-live-cursor-pane
from: TT-01
to: PM-01
mode: idea
status: Sent — awaiting PM ticketing
created: 2026-09-30
updated: 2026-09-30
proposal: docs/agents/unexecuted_proposals/victoria-browser-live-cursor-pane.md
pm_tickets:
---

# PROP-14 — Victoria browser HWND embed + pink/teal click cursor

## Proposal

`docs/agents/unexecuted_proposals/victoria-browser-live-cursor-pane.md`

Mode: **idea**

## Summary

Kurt wants Victoria’s soft cursor visible with a **pink → teal** flash on click, and the Presence **Victoria browser** pane to host **her actual Playwright Chromium window** (HWND embed) by default — not action-only JPEG screenshots — so he can **intervene, assist, or verify**. One window only: the embed *is* the headed Chromium; no floating twin on the desk. CDP screencast / JPEG is **fallback only** if embed hits kill criteria.

## Decisions already accepted (Kurt 2026-09-30)

| Lock | Decision |
| --- | --- |
| Pane | HWND embed her Playwright Chromium into Presence browser surface |
| Cursor | Idle `#ff2d55` → click `#2ec4b6` (~220ms) |
| Second window | None — embed replaces floating headed Chromium |
| Screencast | Fallback only if embed killed |
| Desk OS | Windows embed MVP; CI stays headless |

No product clarifying questions remain for PM.

## Suggested splits

PM may re-split / reassign. These are hints:

- `PROP-14.1` — BED01: pink → teal click cursor state machine on DOM soft-cursor (+ burn-in only if JPEG fallback used).
- `PROP-14.2` — BED01 + FED01: HWND embed headed Playwright Chromium into Presence browser surface; profile/PID filter; resize with pane; clean teardown; **no second floating window**.
- `PROP-14.3` — FED01: pane honesty banners (`embedded` / `embed failed → fallback` / `capture off`); remove screenshot-as-default.
- `PROP-14.4` — OPS01: desk Host headed+embed config; document CI/non-Windows headless path.
- `PROP-14.5` — BED01: screencast/JPEG fallback path only if 14.2 hits kill criteria.
- QA01 — Windows Home PC smoke: see cursor colors, intervene click-in, no floating twin Chromium.

## Kill criteria (embed)

Presence exit kills her browser mid-session; permanent keyboard focus steal; DPI/reparent breaks Victoria click coords; wrong HWND (operator Chrome). Any of these → activate 14.5 fallback, do not leave a silent floating Chromium.

Activate PM-01 to ticket. TT-01 does not write the execution tickets.
