---
type: intake
prop_id: PROP-13-playwright-frame-not-leaked-text
from: TT-01
to: PM-01
mode: idea
status: Ticketed — PROP-13.1–13.3 BED
created: 2026-09-29
updated: 2026-09-29
proposal: docs/agents/unexecuted_proposals/playwright-open-and-frame-not-leaked-tool-text.md
pm_tickets: docs/agents/tasks/PROP-13.1-PM01-to-BED01.md, docs/agents/tasks/PROP-13.2-PM01-to-BED01.md, docs/agents/tasks/PROP-13.3-PM01-to-BED01.md
---

# PROP-13 — Playwright open-and-frame must run, not echo tool markup

## Proposal

`docs/agents/unexecuted_proposals/playwright-open-and-frame-not-leaked-tool-text.md`

Mode: **idea**

## Summary

LinearThrone asked Victoria to open Playwright to a site and capture a frame. The chat bubble was only `<execute_tool>` markup, Her browser stayed on “No frame yet,” and no picture appeared in either pane. The host returned the model’s leaked tag as the reply, so the tool never dispatched and no frame was published.

Recommended route (Avenue A): when the backend is Playwright, treat “playwright” plus a capture/frame ask as `browser_navigate` using the `http(s)` URL from the user message, publish that JPEG in Her browser, and never show an unrecovered tool tag. A missing URL asks for one and does not open `about:blank`. This automatic open is local chat only, not SMS. Do not widen tool recovery past the tools advertised on that turn, and do not send the JPEG to the model, the desktop gallery, or SMS.

## Decisions already accepted

- Done is a picture in Her browser. A second still in What she saw is out of this slice.
- No URL → one-sentence ask, no model round, no `about:blank`.
- Auto-dispatch on the local chat tool loop only.

No product questions remain for PM.

## Suggested splits

PM may re-split. These are hints:

- `PROP-13.1` — BED01: intent, URL extract, capture-only follow-on does not defer navigate, and do not soft-dispatch `about:blank` when the user named a site.
- `PROP-13.2` — BED01: reply firewall for unrecovered tool tags; recovery stays inside the advertised allowlist. Do not log tag arguments.
- `PROP-13.3` — BED01: `AllowBrowserCapture` before the navigate frame publish on this path; reject private hosts. QA01 checks Her browser shows the JPEG and the bubble has no `<execute_tool>`.

Activate PM-01 to ticket. TT-01 does not write the execution tickets.
