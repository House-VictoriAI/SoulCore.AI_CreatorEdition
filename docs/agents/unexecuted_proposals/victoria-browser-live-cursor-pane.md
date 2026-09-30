---
type: proposal
status: unexecuted
tt_id: TT-01
prop_id: PROP-14-victoria-browser-live-cursor-pane
created: 2026-09-30
updated: 2026-09-30
title: Victoria browser — click-color cursor + live pane (not stale screenshots)
need: See Victoria’s mouse with a color change on click, and put her real browsing surface in the Presence browser pane by default instead of action-only JPEGs
related:
  - docs/archive/proposals/victoria-reliable-workspace-browser.md
  - docs/archive/tasks/TASK-20260819-196-PM01-to-FED01.md
sent_at:
pm_intake:
---

# Victoria browser — click-color cursor + live pane

## 1. Need / Want

Kurt wants:

1. A **mouse cursor he can see** in Victoria’s browser that **changes color while clicking**.
2. The Presence **Victoria browser** space to show **the browser she uses** by default — **not a screenshot** album.

Today the pane polls `/browser/view/image` (~5 fps capability) but frames mostly publish on tool actions; cursor is a fixed pink DOM overlay / burn-in stamp; `PlaywrightHeaded` defaults **false**.

## 2. Goal & Success Criteria

1. Continuous aim cursor visible to Kurt (headed window and/or in-pane stream).
2. Distinct **idle vs click** colors (DOM overlay + any burn-in fallback match).
3. Presence browser pane is **near-live** by default while she drives (≥~2–5 fps while tools run; idle may drop) — not “last tool JPEG.”
4. Pane shows **her** Playwright Chromium profile only — never Kayleigh’s daily Chrome.
5. Soft-cursor / non-stealing rule preserved: do not drive OS mouse as the product path.
6. Loopback-only observer; `AllowBrowserCapture=false` silences the stream.

## 3. Context & Constraints

| Fact | Where |
| --- | --- |
| JPEG hub + poll pane | `VictoriaBrowserViewHub`, `MainWindow.BrowserView.cs` |
| Pink click overlay + burn-in | `PlaywrightClickCursor.cs` (scale pulse only; no color state) |
| Headed default off | `Tools:PlaywrightHeaded` in appsettings |
| Prior design | FED-196 / PROP-1: CDP screencast into Presence |
| PROP-12 lock | Web = Playwright; desktop = CUA+VM |

## 4. Clarifying Q&A

| Q | TT default for proposal | Needs Kurt lock |
| --- | --- | --- |
| Click palette | Idle `#ff2d55` → click `#00e676` ~220ms | Confirm or override |
| “Real window in the pane” | **Live CDP screencast** as default MVP (feels live; same Chromium she actuates). **HWND SetParent embed** = optional Windows spike, not MVP gate | If Kurt **requires** HWND chrome inside Avalonia, say so — CONTRA/SYS reject as primary |
| Headed Chromium on desk | ON for LinearThrone desk Host (debug + DOM cursor); CI stays headless | OK? |
| FPS floor while tools run | ≥~2–5 fps | Raise if needed |

## 5. Avenues Explored

### Cursor

| ID | Avenue | Notes |
| --- | --- | --- |
| **A1** | Idle/click color state machine on DOM + burn-in | **Recommended** — small, works with headed + stream |
| A2 | CDP-native cursor bitmap | Extra complexity; still synthetic |
| A3 | Drive OS mouse | **Rejected** — steals Kurt’s pointer |

### Pane content

| ID | Avenue | Notes |
| --- | --- | --- |
| **B1** | CDP `Page.startScreencast` (or continuous JPEG pump) into existing pane | **Recommended MVP** — prior FED-196 intent; near-live; cross-platform pane |
| B2 | Win32 `SetParent` embed headed Chromium HWND into `VictoriaBrowserSurface` | Literal “window in the space”; fragile DPI/focus/crash; Windows-only; **park behind spike kill criteria** |
| B3 | Headed Chromium beside Presence + pane = HUD only | Fast ops bridge; does not close “pane is her browser” alone |
| B4 | Keep action-only screenshots | **Rejected** — fails the ask |

## 6. Recommended Route

1. **PROP-14.1 BED** — Cursor A1: idle vs click colors; move/aim updates DOM; burn-in matches click state when JPEG fallback exists.
2. **PROP-14.2 BED** — Continuous observer B1: start/stop CDP screencast (or timer JPEG ≥2–5 fps while session active) into `VictoriaBrowserViewHub`; action frames remain optional overlays.
3. **PROP-14.3 FED** — Pane consumes live stream as default; honest “updating / stalled / capture off” banners; keep URL · title · last action · Waiting-on-you.
4. **PROP-14.4 OPS** — Desk Host: `PlaywrightHeaded=true` (optional but recommended so DOM cursor is also on the real window).
5. **PROP-14.5 (optional spike)** — HWND embed research with CONTRA kill criteria; ship only if B1 fails Kurt’s “in that space” bar **and** Windows-only is accepted.

**Do not** spend the MVP budget proving SetParent before the live stream ships. **Do not** attach to Kayleigh’s Chrome.

## 7. Alternatives (parked)

- OS mouse drive
- External headed window as sole product observer
- HWND embed as default without spike

## 8. Risks & Kill Criteria

| Risk | Mitigation |
| --- | --- |
| Screencast CPU | Cap fps/quality; pause when idle / capture off |
| Stale honesty | Banner if frame age >1s during active tools |
| HWND crash/focus/DPI | Kill embed if Presence dies with Chromium, focus steal, or DPI coord break |
| Wrong browser HWND | Profile path + process filter; never operator Chrome |

## 9. Open Questions for Kurt / PM

1. Is **live screencast inside the pane** enough for “not a screenshot,” or do you **require** Chromium’s window chrome reparented into Avalonia?
2. Confirm click flash colors (pink → green OK?).
3. Desk only Windows for any embed spike — OK?

## 10. Suggested PM Handoff

- `prop_id`: `PROP-14-victoria-browser-live-cursor-pane`
- Suggested splits: 14.1 BED cursor colors · 14.2 BED live stream · 14.3 FED pane · 14.4 OPS headed desk · 14.5 optional HWND spike · QA smoke on Home PC

**TT recommendation:** Park until Kurt locks Q1 (screencast vs HWND). Default TT stance: **screencast MVP + cursor colors**; HWND only if he insists after seeing live stream.
