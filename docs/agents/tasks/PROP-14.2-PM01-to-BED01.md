---
type: task
prop_id: PROP-14.2
prop_root: PROP-14-victoria-browser-live-cursor-pane
from: PM-01
to: BED-01
status: In Progress
created: 2026-10-02
updated: 2026-10-02
proposal: docs/agents/unexecuted_proposals/victoria-browser-live-cursor-pane.md
---

# PROP-14.2 — HWND embed Victoria Chromium into Presence

## Goal

Windows desk: headed Playwright Chromium HWND is SetParent'd into Her screen. No floating twin. Never Kayleigh's Chrome.

## Acceptance

- `Tools:PlaywrightEmbedPane=true` forces headed launch
- `GET /browser/embed` returns `{ mode, hwnd, pid, title }` with ms-playwright path filter
- Presence `VictoriaBrowserEmbedHost` attaches HWND; JPEG is fallback when mode≠embedded
- Detach on clear does not DestroyWindow Chromium
- CI / non-Windows stays non-embed
