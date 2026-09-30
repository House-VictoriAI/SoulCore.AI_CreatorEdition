---
type: proposal
status: draft
tt_id: TT-01
prop_id: PROP-14-presence-live-browser-observer
created: 2026-09-30
updated: 2026-09-30
title: Presence Her screen must show Victoria's live browser with a real clickable cursor
need: LinearThrone wants co-presence — continuous pointer with click-state change, and Her screen showing the live Playwright tab she drives, not a stale JPEG album or VBox still
seats: STRAT bc-a9a01353-4eb7-521d-8662-2d25e407a912, CONTRA bc-1b1e7b19-6200-527e-ad6b-60a495f6fd5b, SYS bc-d3d9eaec-17c7-539c-a10c-538ffe16c2b1, USER bc-4f6aaa0b-3229-5a6e-8f6d-81c17f959049
---

# Presence Her screen = live browser observer + click cursor

## 1. Need / Want

When Victoria drives her Host Playwright Chromium, LinearThrone watching Presence must feel **co-presence**: continuous pointer, a clear click-state change, and **live page motion** in **Her screen** — the same surface she is actuating — not a photo album of last tool screenshots and not VirtualBox guest stills.

## 2. Goal & Success Criteria

- Cursor is **continuous** (not only a stamped ring on a still).
- Click changes state (color / pulse) so the click **lands** visibly.
- Her screen shows **live** tab pixels while she works (scroll / hover / load), not action-only refresh.
- Same Playwright tab surface — not VBox/desktop stills for this path.
- Operator stays in Presence; no Alt-Tab hunting a floating Chromium window.
- JPEG hub remains as **degraded / offline** fallback only once live path ships.

## 3. Context & Constraints

Seats: STRAT, CONTRA, SYS, USER (2026-09-30). They did not edit the repo.

Verified / current stack:

- Presence **Her screen** polls `GET /browser/view` + `/image` (~200 ms). Hub is in-memory (`IVictoriaBrowserViewHub`).
- Playwright publishes frames on tool actions (`PublishFrameAsync`); optional pink burn-in via `PlaywrightClickCursor` + `AimDwellMs = 650`.
- Recent VM-primary work can also mirror `desktop_screenshot` guest frames into the same hub (`backend=vbox-guest`). This PROP is about the **Playwright Host Chromium observer**, not replacing CUA/VM desktop control (PROP-12 locked: web=Playwright / desktop=CUA+VM).
- FED-196 / BED-195 already named CDP screencast as the intended in-pane live path.

### Seat consensus (synthesized)

| Slice | STRAT | CONTRA | SYS | USER | TT pick |
| --- | --- | --- | --- | --- | --- |
| Cursor color-on-click (soft DOM + burn-in) | A1 primary | OK with stream | **Ship 1 (S)** | Required | **Ship first** |
| CDP screencast → pane (≥2–5 fps active) | B3 parked | **Preferred** | **Ship 2 (M)** | **Locked intent** | **Ship second** |
| Headed Chromium beside Presence + HUD | B2 escape hatch | Debug only | Optional ops (S) | Bridge only; does not close need | **Optional same sprint** |
| Win32 HWND `SetParent` into Avalonia | B1 primary | **Hard reject** | Park / research | Not required if live | **Park** |

CONTRA kill criteria for HWND (any one in soak → abandon): Presence dies when Chromium dies; chat loses keyboard focus; coords wrong after DPI/multi-monitor; black pane after minimize/sleep; Linux/cloud Presence path required.

## 4. Clarifying Q&A (open)

1. Is **inside-pane** live feed a hard requirement, or is headed Chromium **beside** Presence acceptable as MVP? (USER: pane must be live; headed alone does not close.)
2. Windows-only MVP for Presence observer OK? (CONTRA: HWND is Windows-only dead end; CDP/JPEG stays cross-compile friendly.)
3. Click palette — keep pink press / green flash (~220 ms), or pick new idle/click colors?
4. While Playwright live ships, should VBox guest frames **stop** publishing to Her screen (keep “What she saw” only), or dual-backend switch by `BrowserBackend`?

## 5. Recommended avenue

**Ship order:** cursor states → (optional headed+HUD) → CDP screencast into hub → park HWND.

1. **PROP-14.1 BED — Soft cursor state machine**  
   Extend `PlaywrightClickCursor` InitScript + `BurnInMarker`: idle → press on aim/click → flash → idle. Same palette on DOM overlay and JPEG burn-in. Do not steal OS pointer. Do not couple to screencast.

2. **PROP-14.2 OPS (optional) — Headed desk bridge**  
   `PlaywrightHeaded=true` on operator desk only (not CI). Presence HUD shows URL/title/backend. Does not replace in-pane live.

3. **PROP-14.3 BED+FED — CDP screencast → Her screen**  
   `Page.startScreencast` (or equivalent JPEG pump) into `IVictoriaBrowserViewHub` at ~2–5 fps while tools active; idle may drop. URL/title overlay matches live tab. Coord map stays on letterboxed frame pixels. Action JPEG remains degraded fallback.

4. **Park — HWND embed**  
   No `SetParent` Chromium into Avalonia unless 14.3 fails UX **and** operator explicitly accepts Windows-only + CONTRA kill criteria. Spike only if reopened.

## 6. Risks & kill criteria

- Stale JPEG coaching wrong UI (USER).
- Screencast CPU/bandwidth — cap fps; idle backoff.
- Wrong HWND / attaching operator Chrome (SYS stop-ship).
- Mixing VBox guest stills into Her screen while Playwright is the web actuator → wrong surface trust (USER).
- Claiming headed-beside or JPEG pump alone satisfies “actual window” / co-presence (STRAT+USER).

Kill 14.3 if: after navigate, pane shows prior URL >1s with no honesty, or active fps &lt; ~2 while she clicks.

## 7. Suggested PM handoff

| Split | Role | Hint |
| --- | --- | --- |
| PROP-14.1 | BED-01 | Cursor idle/click/flash DOM + burn-in |
| PROP-14.2 | OPS-01 | Optional headed desk config + HUD honesty |
| PROP-14.3 | BED-01 + FED-01 | CDP screencast → hub → Presence Image; coord tool preserved |
| PROP-14.4 | — | Parked HWND spike (only if 14.3 fails) |

## 8. Seat links

- STRAT: https://cursor.com/agents/bc-a9a01353-4eb7-521d-8662-2d25e407a912
- CONTRA: https://cursor.com/agents/bc-1b1e7b19-6200-527e-ad6b-60a495f6fd5b
- SYS: https://cursor.com/agents/bc-d3d9eaec-17c7-539c-a10c-538ffe16c2b1
- USER: https://cursor.com/agents/bc-4f6aaa0b-3229-5a6e-8f6d-81c17f959049
