---
type: proposal
status: unexecuted
tt_id: TT-01
prop_id: PROP-14-victoria-browser-live-cursor-pane
created: 2026-09-30
updated: 2026-09-30
title: Victoria browser — click-color cursor + embedded Chromium pane
need: See Victoria’s mouse with a color change on click, and embed her actual Playwright Chromium window in the Presence browser pane so Kurt can intervene / assist / verify
related:
  - docs/archive/proposals/victoria-reliable-workspace-browser.md
  - docs/archive/tasks/TASK-20260819-196-PM01-to-FED01.md
sent_at:
pm_intake:
kurt_locks:
  pane: hwnd-embed-primary
  click_palette: pink-idle-teal-click
  second_window: none-embed-is-the-window
  locked_at: 2026-09-30
---

# Victoria browser — click-color cursor + embedded Chromium pane

## 1. Need / Want

Kurt wants:

1. A **mouse cursor he can see** in Victoria’s browser that **changes color while clicking**.
2. The Presence **Victoria browser** space to hold **her actual Chromium window** by default — **not a screenshot** album — so he can **intervene, assist, or verify** when needed.

Today the pane polls `/browser/view/image` (~5 fps capability) but frames mostly publish on tool actions; cursor is a fixed pink DOM overlay / burn-in stamp; `PlaywrightHeaded` defaults **false**.

## 2. Goal & Success Criteria

1. Continuous aim cursor visible inside **her** embedded Chromium (DOM soft-cursor).
2. Distinct **idle vs click** colors: idle pink → click teal/aqua (~220ms flash).
3. Presence browser pane **hosts her Playwright Chromium HWND** (Windows desk Host) — one window, not a JPEG gallery and not a floating second Chromium beside Presence.
4. Kurt can click into the pane and interact with the page when he needs to intervene / assist / verify (focus + input path defined and honest when blocked).
5. Pane shows **her** Playwright Chromium profile only — never Kayleigh’s daily Chrome.
6. Soft-cursor for Victoria’s automation path: do **not** drive OS mouse as the product automation path.
7. CI / headless remains headless; embed path is Windows desk Host only.
8. `AllowBrowserCapture=false` (or equivalent gate) can refuse attach / show honest “capture off” — never silently reparent the wrong process.

## 3. Context & Constraints

| Fact | Where |
| --- | --- |
| JPEG hub + poll pane | `VictoriaBrowserViewHub`, `MainWindow.BrowserView.cs` |
| Pink click overlay + burn-in | `PlaywrightClickCursor.cs` (scale pulse only; no color state) |
| Headed default off | `Tools:PlaywrightHeaded` in appsettings |
| Prior design | FED-196 / PROP-1: CDP screencast into Presence |
| PROP-12 lock | Web = Playwright; desktop = CUA+VM |
| Desk OS for embed | Windows (LinearThrone / Home PC) |

## 4. Clarifying Q&A — **LOCKED 2026-09-30**

| Q | Kurt lock |
| --- | --- |
| “Real window in the pane” | **HWND embed** of her Playwright Chromium into the Presence browser surface. Reason: intervene / assist / verify. Screencast is **fallback only** if embed fails kill criteria. |
| Click palette | Idle **pink** `#ff2d55` → click **teal/aqua** `#2ec4b6` (~220ms). (Turquoise family; PM/FED may tune hex ± a shade.) |
| Second / floating headed window | **No.** Embed **is** the headed window. Do not also leave a free-floating Chromium on the desk. |
| Desk OS | Windows-only for embed MVP. CI stays headless. |

Answers are sufficient — no further Kurt clarifying questions for MVP scope.

## 5. Avenues Explored

### Cursor

| ID | Avenue | Notes |
| --- | --- | --- |
| **A1** | Idle/click color state machine on DOM (+ burn-in only if JPEG fallback used) | **Recommended** |
| A2 | CDP-native cursor bitmap | Extra complexity; still synthetic |
| A3 | Drive OS mouse | **Rejected** — steals Kurt’s pointer for automation |

### Pane content

| ID | Avenue | Notes |
| --- | --- | --- |
| **B2** | Win32 `SetParent` (or Avalonia-native host) embed headed Chromium HWND into `VictoriaBrowserSurface` | **Locked MVP** — literal window in the space; enables intervene |
| B1 | CDP `Page.startScreencast` / continuous JPEG into pane | **Fallback** if B2 hits kill criteria; does **not** give true intervene-in-pane |
| B3 | Headed Chromium beside Presence + pane = HUD only | **Rejected for product** — Kurt does not want a second window when embed works |
| B4 | Keep action-only screenshots | **Rejected** — fails the ask |

## 6. Recommended Route

1. **PROP-14.1 BED** — Cursor A1: idle `#ff2d55` / click `#2ec4b6`; move/aim updates DOM; flash ~220ms on click/tap tools.
2. **PROP-14.2 BED + FED** — HWND embed B2: launch Playwright **headed** for desk Host, discover **her** Chromium HWND (profile / process filter), reparent into Presence browser surface; resize/move with pane; tear down cleanly on session stop / Presence close.
3. **PROP-14.3 FED** — Pane chrome: honest banners (`embedded` / `embed failed → fallback` / `capture off` / `waiting for session`); keep URL · title · last action · Waiting-on-you; **remove screenshot-as-default**.
4. **PROP-14.4 OPS** — Desk Host config: headed ON for embed path; document that embed replaces floating Chromium; CI / non-Windows stays headless + non-embed.
5. **PROP-14.5 BED (fallback)** — If embed fails kill criteria, fall back to B1 screencast (or last-good JPEG) with clear banner — **not** a silent second floating window.

**Do not** attach to Kayleigh’s Chrome. **Do not** ship a floating headed Chromium *plus* embed. **Do not** treat screencast as the happy path after this lock.

## 7. Alternatives (parked)

- Screencast-as-primary (superseded by Kurt lock)
- OS mouse drive for Victoria automation
- External headed window as sole product observer
- Dual window (embed + floating)

## 8. Risks & Kill Criteria

| Risk | Mitigation / kill |
| --- | --- |
| HWND crash / Presence dies with Chromium | Isolate process lifetime; if Presence exit kills her browser mid-session → kill embed approach for that build, activate 14.5 fallback |
| Focus steal / Kurt can’t type in chat | Document focus rules; click-to-focus pane vs chat; kill if embed permanently steals keyboard |
| DPI / resize coord break (Victoria clicks miss) | Tool coords must stay in page space; kill if DPI scale breaks automation after reparent |
| Wrong browser HWND | Strict Playwright profile path + PID filter; never operator Chrome |
| Intervene UX unclear | Define: Kurt click-in is supported when session live; if Playwright exclusive-lock blocks him, banner honesty + optional “pause Victoria tools” later (out of MVP unless PM expands) |

## 9. Open Questions for Kurt / PM

**None for MVP scope** — Kurt locks above are sufficient.

PM may still decide: intervene = click-through now vs “pause Victoria then Kurt drives” as a follow-on `.M`.

## 10. Suggested PM Handoff

- `prop_id`: `PROP-14-victoria-browser-live-cursor-pane`
- Suggested splits:
  - **14.1** BED — pink → teal click cursor
  - **14.2** BED+FED — HWND embed into Presence browser surface (no second window)
  - **14.3** FED — pane honesty / remove screenshot-default
  - **14.4** OPS — desk Host headed+embed config
  - **14.5** BED — screencast/JPEG fallback only if embed killed
  - QA — Windows Home PC smoke: see cursor colors, intervene click, no floating twin Chromium

**TT recommendation:** Ready to send-to-PM on request. Primary ship is **embedded Chromium + pink/teal soft cursor**; screencast demoted to fallback.
